using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SOD_CS_Library;


/* 
 * ====================
 * TABLETOP DISPLAY 
 * ====================
 * 
 */


namespace PPStudyClient_TableTop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var path = System.IO.Path.GetFullPath("image.png");
            var uri = new Uri(path);
            var bitmap = new BitmapImage(uri);
            image.Source = bitmap;

            image.Visibility = System.Windows.Visibility.Hidden;

            DealWithSoD();
        }


        #region SoD Config

        private void DealWithSoD()
        {
            if (SoD == null)
            {
                configureSoD();
                configureDevice();
                registerSoDEvents();
                connectSoD();
                //SoD.ConnectToProjector();

            }
        }



        #region SOD parameters
        static SOD_CS_Library.SOD SoD;
        // Device parameters. Set 
        // TODO: FILL THE FOLLOWING VARIABLES AND WITH POSSIBLE VALUES
        static int _deviceID = 21;                   // OPTIONAL. If it's not unique, it will be "randomly" assigned by locator.
        static string _deviceName = "TableTop";   // You can name your device
        static string _deviceType = "CS Client";   // Cusomize device
        static bool _deviceIsStationary = true;     // If mobile device, assign false.
        static double _deviceWidthInM = 1          // Device width in metres
                        , _deviceHeightInM = 1.5   // Device height in metres
                        , _deviceLocationX = 1.5     // Distance in metres from the sensor which was first connected to the server
                        , _deviceLocationY = 1      // Distance in metres from the sensor which was first connected to the server
                        , _deviceLocationZ = 2.5      // Distance in metres from the sensor which was first connected to the server
                        , _deviceOrientation = 0    // Device orientation in Degrees, if mobile device, 0.
                        , _deviceFOV = 70;           // Device Field of View in degrees


        // observers can let device know who enters/leaves the observe area.
        static string _observerType = "rectangular";
        static double _observeHeight = 2;
        static double _observeWidth = 2;
        static double _observerDistance = 2;
        static double _observeRange = 2;
        /*
         * You can also do Radial type observer. Simply change _observerType to "radial": 
         *      static string _observerType = "radial";
         * Then observeRange will be taken as the radius of the observeRange.
        */

        // SOD connection parameters
        static string _SODAddress = "beastwin.marinhomoreira.com"; // LOCATOR URL or IP
        //static string _SODAddress = "192.168.0.144"; // Sydney's computer
        static int _SODPort = 3000; // Port of LOCATOR
        #endregion

        public static void configureSoD()
        {
            // Configure and instantiate SOD object
            string address = _SODAddress;
            int port = _SODPort;
            SoD = new SOD_CS_Library.SOD(address, port);

            // configure and connect
            configureDevice();
        }

        private static void configureDevice()
        {
            // This method takes all the parameters you specified above and set the properties accordingly in the SOD object.
            // Configure device with its dimensions (mm), location in physical space (X, Y, Z in meters, from sensor), orientation (degrees), Field Of View (FOV. degrees) and name
            SoD.ownDevice.SetDeviceInformation(_deviceWidthInM, _deviceHeightInM, _deviceLocationX, _deviceLocationY, _deviceLocationZ, _deviceType, _deviceIsStationary);
            //SoD.ownDevice.orientation = _deviceOrientation;
            SoD.ownDevice.FOV = _deviceFOV;
            if (_observerType == "rectangular")
            {
                SoD.ownDevice.observer = new SOD_CS_Library.observer(_observeWidth, _observeHeight, _observerDistance);
            }
            else if (_observerType == "radial")
            {
                SoD.ownDevice.observer = new SOD_CS_Library.observer(_observeRange);
            }

            // Name and ID of device - displayed in Locator
            SoD.ownDevice.ID = _deviceID;
            SoD.ownDevice.name = _deviceName;
        }

        /// <summary>
        /// Connect SOD to Server
        /// </summary>
        public static void connectSoD()
        {
            SoD.SocketConnect();
        }


        /// <summary>
        /// Disconnect SOD from locator.
        /// </summary>
        public static void disconnectSoD()
        {
            SoD.Close();
        }

        /// <summary>
        /// Reconnect SOD to the locator.
        /// </summary>
        public static void reconnectSoD()
        {
            SoD.ReconnectToServer();
        }


        #endregion

        #region SoD Events

        private void registerSoDEvents()
        {
            #region SOD Default Events
            SoD.On("connect", (data) =>
            {
                Console.WriteLine("\r\nConnected...");
                Console.WriteLine("Registering with server...\r\n");
                SoD.RegisterDevice();  //register the device with server everytime it connects or re-connects

            });

            // Sample event handler for when any device connects to server
            SoD.On("someDeviceConnected", (msgReceived) =>
            {
                Console.WriteLine("Some device connected to server: " + msgReceived.data);
            });

            // listener for event a person walks into a device
            SoD.On("enterObserveRange", (msgReceived) =>
            {
                // Parse the message 
                Dictionary<String, String> payload = new Dictionary<string, string>();
                SoD.SendToDevices.All("EnterView", payload);
            });

            // listener for event a person grab in the observeRange of another instance.
            SoD.On("grabInObserveRange", (msgReceived) =>
            {
                Console.WriteLine(" person " + msgReceived.data["payload"]["invader"] + " perform Grab gesture in a " + msgReceived.data["payload"]["observer"]["type"] + ": " + msgReceived.data["payload"]["observer"]["ID"]);
            });

            // listener for event a person leaves a device.
            SoD.On("leaveObserveRange", (msgReceived) =>
            {
                Dictionary<String, String> payload = new Dictionary<string, string>();
                SoD.SendToDevices.All("LeaveView", payload);
            });

            // Sample event handler for when any device disconnects from server
            SoD.On("someDeviceDisconnected", (msgReceived) =>
            {
                Console.WriteLine("Some device disconnected from server : " + msgReceived.data["name"]);
            });
            #endregion

            // 
            SoD.On("ShowInformation", (msgReceived) =>
            {
                ShowInformationOnDisplay();
            });

            SoD.On("RemoveInformation", (msgReceived) =>
            {
                RemoveInformationOnDisplay();
            });
        }

        #endregion

        private void RemoveInformationOnDisplay()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                image.Visibility = System.Windows.Visibility.Hidden;
            }));
        }
        private void ShowInformationOnDisplay()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                image.Visibility = System.Windows.Visibility.Visible;
            }));
            
        }

        

        private void Window_Closed(object sender, EventArgs e)
        {
            disconnectSoD();
        }

    }
}
