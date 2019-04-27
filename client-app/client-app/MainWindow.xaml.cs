using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Timers;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace client_app
{
    public partial class MainWindow : Window
    {
        MqttClient MqttClient; // client interface to handle MQTT communication
        string clientId;
        public static string macPi = "b8-27-eb-df-ac-b7"; // MAC address of raspberry PI
        System.Timers.Timer connectionToBrokerTimer;
        System.Timers.Timer tryHandshakeTimer;
        System.Timers.Timer timeoutConnection; 
        SemaphoreSlim timeoutConnectionSemaphore = new SemaphoreSlim(1, 1); // semaphore to handle the access to the timeoutConnection Timer
        int numDots = 0;

        public MainWindow()
        {
            InitializeComponent();
            
            // connectionTimer initializiation 
            connectionToBrokerTimer = new System.Timers.Timer(1000);
            connectionToBrokerTimer.Elapsed += connectionToBrokerTimer_Elapsed;
            connectionToBrokerTimer.AutoReset = true;
            connectionToBrokerTimer.Enabled = false;

            // tryHandshakeTimer initializiation 
            tryHandshakeTimer = new System.Timers.Timer(1000);
            tryHandshakeTimer.Elapsed += tryHandshakeTimer_Elapsed;
            tryHandshakeTimer.AutoReset = true;
            tryHandshakeTimer.Enabled = false;

            // timeoutConnection initializiation
            timeoutConnection = new System.Timers.Timer(10000);
            timeoutConnection.Elapsed += timeoutConnection_Elapsed;
            timeoutConnection.AutoReset = false;
            timeoutConnection.Enabled = false;
        }


        // try to connect to Raspberry PI every 1 sec 
        protected void connectionToBrokerTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                numDots++;
                if(numDots == 4)
                {
                    numDots = 0;
                    LoadingLabel.Content = "Connessione in corso";
                }
                else
                {
                    LoadingLabel.Content += ".";
                }
            });
            try
            {
                //get raspberry PI IP from MAC
                string BrokerAddress = IPMACMapper.FindIPFromMacAddress(macPi);

                // connect to the MQTT broker
                // MqttClient = new MqttClient(BrokerAddress, 8883, false, MqttSslProtocols.None, null, null);
                MqttClient = new MqttClient(BrokerAddress, 8883, false, MqttSslProtocols.None, null, null);
                MqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
                clientId = Guid.NewGuid().ToString();
                MqttClient.Connect(clientId, clientId, clientId);

                // if everything went fine start handshake phase
                connectionToBrokerTimer.Stop();
                tryHandshakeTimer.Start();
            }
            catch (Exception)
            {

            }
        }


        // try handshake phase every 1 sec
        protected void tryHandshakeTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                numDots++;
                if (numDots == 4)
                {
                    numDots = 0;
                    LoadingLabel.Content = "Connessione in corso";
                }
                else
                {
                    LoadingLabel.Content += ".";
                }
            });
            try
            {
                MqttClient.Subscribe(new string[] { "image", "client-app" }, new byte[] { 0, 0 });

                //start three-way handshake phase
                MqttClient.Publish("rpi", Encoding.UTF8.GetBytes("handshake1"));
            }
            catch (Exception)
            {
                tryHandshakeTimer.Stop();
                DisconnectFromPi();
                ConnectToPi();
            }
        }
        
        protected void ConnectToPi()
        {
            Dispatcher.Invoke(delegate
            {
                LoadingLabel.Content = "Connessione in corso";
            });
            connectionToBrokerTimer.Start();
        }
        protected void DisconnectFromPi()
        {
            Dispatcher.Invoke(delegate
            {
                DateTimeLabel.Content = "";
                PiImage.Source = null;
            });

            try
            {
                //start reversed one-way handshake phase
                MqttClient.Publish("rpi", Encoding.UTF8.GetBytes("rhandshake"));
                MqttClient.Disconnect();
            }
            catch (Exception) { }
        }

        // check if no messages are sent by the PI for 10 sec 
        protected void timeoutConnection_Elapsed(object source, ElapsedEventArgs e)
        {
            DisconnectFromPi();
            // warn the user that he should restart the pi
            Dispatcher.Invoke(delegate
            {
                PiImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/ErroreDiConnessione.png"));
            });

            // reconnect
            ConnectToPi();
        }


       


        protected override void OnClosed(EventArgs e)
        {
            DisconnectFromPi();

            // finally close the application
            base.OnClosed(e);
            App.Current.Shutdown();
            Environment.Exit(0);
        }

        // message arrived
        async void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            await timeoutConnectionSemaphore.WaitAsync();
            timeoutConnection.Stop();

            string Topic = e.Topic;

            string ReceivedMessage = Encoding.UTF8.GetString(e.Message);
            switch (Topic)
            {
                case "client-app":
                    if (ReceivedMessage == "handshake2")
                    {
                        tryHandshakeTimer.Stop();
                        timeoutConnection.Start();
                        // third-phase of handshake, connection is open
                        MqttClient.Publish("rpi", Encoding.UTF8.GetBytes("handshake3"));
                        Dispatcher.Invoke(delegate
                        {
                            LoadingLabel.Content = "";
                        });
                    }
                    break;

                case "image":
                    // get image and time from raspberry and refresh UI accordingly
                    PiImage piImage = JsonConvert.DeserializeObject<PiImage>(ReceivedMessage);
                    using (var ms = new MemoryStream(piImage.image.data))
                    {
                        var decoder = BitmapDecoder.Create(ms,
                            BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        BitmapSource bitmapSource = decoder.Frames[0];

                        Dispatcher.Invoke(delegate {
                            PiImage.Source = bitmapSource;
                            DateTimeLabel.Content = "Ricevuto il " + RefreshDateTime(piImage.time).ToString();
                        });
                    }
                    break;
            }

            timeoutConnection.Start();
            timeoutConnectionSemaphore.Release();
        }

        private DateTime RefreshDateTime(long Milliseconds)
        {
            DateTime StartingDateTime = new DateTime(1970, 1, 1, 0, 0,0, DateTimeKind.Utc);
            return StartingDateTime.AddMilliseconds(Milliseconds);
        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectToPi();
        }



        public string preMac; // previous MAC
        void MacButton_Click(object sender, RoutedEventArgs e)
        {
            preMac = macPi;
            WindowMAC windowMAC = new WindowMAC();
            windowMAC.Closed += WindowMAC_Closed;
            windowMAC.Show();
        }

        private async void WindowMAC_Closed(object sender, EventArgs e)
        {
            await timeoutConnectionSemaphore.WaitAsync();
            // if a connection was previously opened and the MAC is changed, reconnect
            if (timeoutConnection.Enabled && preMac != macPi)
            {
                DisconnectFromPi();

                timeoutConnection.Stop();
                ConnectToPi();
            }
            timeoutConnectionSemaphore.Release();
        }
    }
}


