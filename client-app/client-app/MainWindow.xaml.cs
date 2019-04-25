using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace client_app
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MqttClient MqttClient;
        string clientId;
        string macPi = "b8-27-eb-df-ac-b7";
        Timer connectionTimer;
        Timer timeoutConnection;


        public MainWindow()
        {
            InitializeComponent();

            connectionTimer = new Timer(1000);
            connectionTimer.Elapsed += connectionTimer_Elapsed;
            connectionTimer.AutoReset = true;
            connectionTimer.Enabled = true;

            timeoutConnection = new Timer(10000);
            timeoutConnection.Elapsed += timeoutConnection_Elapsed;
            timeoutConnection.AutoReset = false;
            timeoutConnection.Enabled = false;
        }

        protected void connectionTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            try
            {
                string BrokerAddress = IPMACMapper.FindIPFromMacAddress(macPi);

                MqttClient = new MqttClient(BrokerAddress, 8883, false, MqttSslProtocols.None, null, null);

                MqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;

                clientId = Guid.NewGuid().ToString();

                MqttClient.Connect(clientId, clientId, clientId);

                connectionTimer.Stop();

                Handshake();
            }
            catch(Exception)
            {

            }            
        }

        protected void timeoutConnection_Elapsed(object source, ElapsedEventArgs e)
        {
            // warn the user the user that he should restart the pi

            // reconnect
            try
            {
                MqttClient.Disconnect();
            }
            catch(Exception) { }

            connectionTimer.Start();
        }

        protected void Handshake()
        {
            MqttClient.Subscribe(new string[] { "image", "client-app" }, new byte[] { 0, 0 });
            //start three-way handshake phase
            MqttClient.Publish("rpi", Encoding.UTF8.GetBytes("handshake1"));

            timeoutConnection.Start();
        }

        
        protected override void OnClosed(EventArgs e)
        {
            MqttClient.Disconnect();

            //start reversed one-way handshake phase
            MqttClient.Publish("rpi", Encoding.UTF8.GetBytes("rhandshake"));

            base.OnClosed(e);
            App.Current.Shutdown();
        }

        
        void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            timeoutConnection.Stop();
            string Topic = e.Topic;

            string ReceivedMessage = Encoding.UTF8.GetString(e.Message);
            switch (Topic)
            {
                case "client-app":
                    if (ReceivedMessage == "handshake2")
                    {
                        MqttClient.Publish("rpi", Encoding.UTF8.GetBytes("handshake3"));
                    }
                    break;

                case "image":
                    PiImage piImage = JsonConvert.DeserializeObject<PiImage>(ReceivedMessage);
                    using (var ms = new MemoryStream(piImage.image))
                    {
                        JpegBitmapDecoder decoder = new JpegBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        BitmapSource bitmapSource = decoder.Frames[0];

                        Dispatcher.Invoke(delegate { 
                        
                            // Set Image.Source  
                            PiImage.Source = bitmapSource;
                        });
                    }
                    break;
            }

            timeoutConnection.Start();
        }
    }
}
