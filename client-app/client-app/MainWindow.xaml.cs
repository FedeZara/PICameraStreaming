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
/*!
\file MainWindow.xaml.cs
\brief Implementazione della classe parziale MainWindow
\version 1.0
*/
namespace client_app

/*!
\class MainWindow
\brief Classe della finestra principale
*/
{
    public partial class MainWindow : Window
    {
        //*! \var MqttClient
        //*! \brief Interfaccia client per gestire la comunicazione con il broker
        MqttClient MqttClient;
        //*! \var clientId
        string clientId;
        //*! \var macPi
        //*! \brief MAC della Raspberry (è settato di default alla rasberry che abbiamo solitamente usato per i test)
        public static string macPi = "b8-27-eb-df-ac-b7";
        //*! \var connectionToBrokerTimer
        //*! \brief Timer per la connessione al broker
        System.Timers.Timer connectionToBrokerTimer;
        //*! \var tryHandshakeTimer
        //*! \brief Timeout di risposta per la fase handshake
        System.Timers.Timer tryHandshakeTimer;
        //*! \var timeoutConnection
        //*! \brief Timer per la mancanza di connessione
        System.Timers.Timer timeoutConnection;
        //*! \var timeoutConnectionSemaphore
        //*! \brief Gestisce l'accesso al timer timeoutConnectionSemaphore
        SemaphoreSlim timeoutConnectionSemaphore = new SemaphoreSlim(1, 1); // semaphore to handle the access to the timeoutConnection Timer
        //*! \var numDots
        //*! \brief Numero di punti dopo scritta "Connessione in corso" 
        int numDots = 0;
        //*! \fn MainWindow
        //*! \brief Costruttore con l'inizzalizzazione dei vari timer
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



        //*! \fn connectionToBrokerTimer_Elapsed
        //*! \brief Tentativo di connessione alla Raspberry ogni 1 secondo
        protected void connectionToBrokerTimer_Elapsed(object source, ElapsedEventArgs e)
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
        //*! \fn tryHandshakeTimer_Elapsed
        //*! \brief Tentativo di inizio di una fase di handshake ogni 1 secondo
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
        //*! \fn ConnectToPi
        //*! \brief Tentativo di connessione alla raspberry
        protected void ConnectToPi()
        {
            Dispatcher.Invoke(delegate
            {
                LoadingLabel.Content = "Connessione in corso";
            });
            connectionToBrokerTimer.Start();
        }
        //*! \fn DisconnectFromPi
        //*! \brief Disconnessione dalla raspberry
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


        //*! \fn timeoutConnection_Elapsed
        //*! \brief Controlle se non sono stati inviati messaggi dalla raspberry per 10
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




        //*! \fn OnClosed
        //*! \brief Alla chiusura dell'app chiude la connessione alla raspberry
        protected override void OnClosed(EventArgs e)
        {
            DisconnectFromPi();

            // finally close the application
            base.OnClosed(e);
            App.Current.Shutdown();
            Environment.Exit(0);
        }


        //*! \fn MqttClient_MqttMsgPublishReceived
        //*! \brief Handler dell'arrivo di un messaggio
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

                        Dispatcher.Invoke(delegate
                        {
                            PiImage.Source = bitmapSource;
                            DateTimeLabel.Content = "Ricevuto il " + RefreshDateTime(piImage.time).ToString();
                        });
                    }
                    break;
            }

            timeoutConnection.Start();
            timeoutConnectionSemaphore.Release();
        }
        /*!
         \fn RefreshDateTime
         \brief Converte una data in UnixTime(millisecondi dalla mezzanotte del 1-1-1970) in una data formato stringa
         \param[in] Milliseconds = Data in UnixTime
         \param[out] Data formato convenzionale
         */

        private DateTime RefreshDateTime(long Milliseconds)
        {
            DateTime StartingDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return StartingDateTime.AddMilliseconds(Milliseconds);
        }
        //*! \fn ConnectionButton_Click
        //*! \brief Gestisce la pressione del bottone di connessione
        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectToPi();
        }


        //*! \var preMac
        //*! \brief MAC precedente
        public string preMac;
        //*! \fn MacButton_Click
        //*! \brief Handler per la pressione sul bottone "Imposta MAC"
        void MacButton_Click(object sender, RoutedEventArgs e)
        {
            preMac = macPi;
            WindowMAC windowMAC = new WindowMAC();
            windowMAC.Closed += WindowMAC_Closed;
            windowMAC.Show();
        }
        //*! \fn WindowMAC_Closed
        //*! \brief Handler per la chiusura della finestra per il MAC
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


