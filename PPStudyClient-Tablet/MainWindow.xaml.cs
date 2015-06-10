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
 * PARTICIPANT TABLET 
 * ====================
 * 
 */


namespace PPStudyClient_Tablet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
        static int _deviceID = 37;                   // OPTIONAL. If it's not unique, it will be "randomly" assigned by locator.
        static string _deviceName = "Participant";   // You can name your device
        static string _deviceType = "CS Client";   // Cusomize device
        static bool _deviceIsStationary = false;     // If mobile device, assign false.
        static double _deviceWidthInM = 1          // Device width in metres
                        , _deviceHeightInM = 1.5   // Device height in metres
                        , _deviceLocationX = 1     // Distance in metres from the sensor which was first connected to the server
                        , _deviceLocationY = 1      // Distance in metres from the sensor which was first connected to the server
                        , _deviceLocationZ = 1      // Distance in metres from the sensor which was first connected to the server
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
            SoD.On("loadTask1", (msgReceived) =>
            {
                LoadTaskOne();
            });

            SoD.On("loadTask2", (msgReceived) =>
            {
                LoadTaskTwo();
            });

            SoD.On("Task2Ready", (msgReceived) =>
            {
                this.Dispatcher.Invoke((Action)(() => 
                {
                    start_find_button.IsEnabled = true;
                }));
            });

            SoD.On("FoundDataPoint", (msgReceived) =>
            {
                FoundDataPoint();
            });

            SoD.On("loadTask3", (msgReceived) =>
            {
                LoadTaskThree();
            });

            SoD.On("ShowInformation", (msgReceived) =>
            {
                ShowInformation();
            });
        }

        #endregion


        #region Parameters 

        int controllerID = 57;

        List<int> rounds = new List<int>();
        int curr_round = 0;

        int CurrentTask = 0;

        Button send_button;
        Button wall1_button;
        Button wall2_button;
        Button table_button;
        Button unknown_button;

        Button start_find_button;

        Button start_receive_button;

        #endregion


        private void GenerateRandomOrder()
        {
            // clear previous order
            rounds.Clear();
            curr_round = 0;

            Random rnd = new Random();
            // get new order 
            while (rounds.Count() < 3)
            {
                int num = rnd.Next(0, 3);
                Console.WriteLine(num);
                if (!rounds.Contains(num))
                    rounds.Add(num);
            }

        }

        private void CreateResponseButtons()
        {
            // TABLETOP
            table_button = new Button();
            table_button = AddFeaturesToButton(table_button);

            // custom features 
            table_button.Content = "TableTop";
            Grid.SetRow(table_button, 2);
            Grid.SetColumn(table_button, 0);

            if (CurrentTask == 1)
                table_button.Click += Response_Clicked;
            else if (CurrentTask == 3)
                table_button.Click += Task3_Response_Clicked;

            task_grid.Children.Add(table_button);

            // WALLDISPLAY 1
            wall1_button = new Button();
            wall1_button = AddFeaturesToButton(wall1_button);

            // custom features 
            wall1_button.Content = "WallDisplay";
            Grid.SetRow(wall1_button, 2);
            Grid.SetColumn(wall1_button, 1);

            if (CurrentTask == 1)
                wall1_button.Click += Response_Clicked;
            else if (CurrentTask == 3)
                wall1_button.Click += Task3_Response_Clicked;

            task_grid.Children.Add(wall1_button);

            // WALLDISPLAY 2
            wall2_button = new Button();
            wall2_button = AddFeaturesToButton(wall2_button);

            // custom features 
            wall2_button.Content = "Display 2";
            Grid.SetRow(wall2_button, 2);
            Grid.SetColumn(wall2_button, 2);

            if (CurrentTask == 1)
                wall2_button.Click += Response_Clicked;
            else if (CurrentTask == 3)
                wall2_button.Click += Task3_Response_Clicked;

            task_grid.Children.Add(wall2_button);

            // UNKNOWN
            unknown_button = new Button();
            unknown_button = AddFeaturesToButton(unknown_button);

            // custom features 
            unknown_button.Content = "Unknown";
            Grid.SetRow(unknown_button, 2);
            Grid.SetColumn(unknown_button, 3);

            if (CurrentTask == 1)
                unknown_button.Click += Response_Clicked;
            else if (CurrentTask == 3)
                unknown_button.Click += Task3_Response_Clicked;

            task_grid.Children.Add(unknown_button);
        }



        private Button AddFeaturesToButton(Button button)
        {
            button.Height = 64;
            button.Width = 169;
            var converter = new BrushConverter();
            button.Background = (Brush)converter.ConvertFromString("#FFFBFBFB");
            button.BorderThickness = new Thickness(2);

            // text
            button.FontWeight = FontWeights.Bold;
            button.FontSize = 24;

            // alignment
            button.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            return button;
        }


        #region Task One

        private void LoadTaskOne()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (CurrentTask == 1)
                {
                    task_grid.Children.Remove(table_button);
                    task_grid.Children.Remove(wall1_button);
                    task_grid.Children.Remove(wall2_button);
                    task_grid.Children.Remove(unknown_button);

                    send_button.IsEnabled = false;
                    send_button.Click -= send_button_Click;

                    task_grid.Children.Remove(send_button);
                }

                GenerateRandomOrder();
                header_label.Content = "Task 1 - Round " + (curr_round+1);
                CurrentTask = 1;

                CreateSendButton();
                CreateResponseButtons();
            }));
        }


        private void CreateSendButton()
        {
            send_button = new Button();

            // set standard features
            send_button = AddFeaturesToButton(send_button);

            //custom features
            send_button.Content = "Send";
            Grid.SetRow(send_button, 1);
            Grid.SetColumn(send_button, 1);
            Grid.SetColumnSpan(send_button, 2);
            send_button.Click += send_button_Click;

            task_grid.Children.Add(send_button);
        }

        void send_button_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> payload = new Dictionary<string, string>();
            payload.Add("round", rounds[curr_round].ToString());
            SoD.SendToDevices.WithID(controllerID, "SendInformationToDisplay", payload);

            // disable send until round answered 
            send_button.IsEnabled = false;
            table_button.IsEnabled = true;
            wall1_button.IsEnabled = true;
            wall2_button.IsEnabled = true;
            unknown_button.IsEnabled = true;
            curr_round++;
        }

        private void Response_Clicked(object sender, RoutedEventArgs e)
        {
            Button response = (Button)sender;

            Dictionary<String, String> payload = new Dictionary<string, string>();
            payload.Add("answer", response.Content.ToString());
            SoD.SendToDevices.WithID(controllerID, "UserResponse", payload);


            send_button.IsEnabled = true;
            table_button.IsEnabled = false;
            wall1_button.IsEnabled = false;
            wall2_button.IsEnabled = false;
            unknown_button.IsEnabled = false;

            header_label.Content = "Task 1 - Round " + (curr_round + 1);

            if (curr_round == 3)
            {
                //table_button.Visibility = System.Windows.Visibility.Hidden;
                //wall1_button.Visibility = System.Windows.Visibility.Hidden;
                //wall2_button.Visibility = System.Windows.Visibility.Hidden;
                task_grid.Children.Remove(table_button);
                task_grid.Children.Remove(wall1_button);
                task_grid.Children.Remove(wall2_button);
                task_grid.Children.Remove(unknown_button);

                send_button.IsEnabled = false;
                send_button.Click -= send_button_Click;

                task_grid.Children.Remove(send_button);

                header_label.Content = "";

                SoD.SendToDevices.WithID(controllerID, "NextTask", null);
            }
        }


        #endregion

        #region Task Two 

        private void LoadTaskTwo()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                GenerateRandomOrder();
                header_label.Content = "Task 2 - Round " + (curr_round + 1);

                CreateStartFindButton();
                CreateUnknownFindButton();
                CurrentTask = 2;
            }));
        }

        private void CreateStartFindButton()
        {
            start_find_button = new Button();

            // set standard features
            start_find_button = AddFeaturesToButton(start_find_button);

            //custom features
            start_find_button.Content = "Start";
            Grid.SetRow(start_find_button, 1);
            Grid.SetColumn(start_find_button, 1);
            start_find_button.Click += Start_Find_Click;
            start_find_button.IsEnabled = true;

            task_grid.Children.Add(start_find_button);
        }

        private void CreateUnknownFindButton()
        {
            unknown_button = new Button();

            // set standard features
            unknown_button = AddFeaturesToButton(unknown_button);

            //custom features
            unknown_button.Content = "Can't Find";
            Grid.SetRow(unknown_button, 1);
            Grid.SetColumn(unknown_button, 2);
            unknown_button.Click += Unknown_Find_Click;
            unknown_button.IsEnabled = false;

            task_grid.Children.Add(unknown_button);
        }

        private void Unknown_Find_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> payload = new Dictionary<string, string>();
            payload.Add("answer", "Could not locate information");
            SoD.SendToDevices.WithID(controllerID, "CantFindDataPoint", payload);

            this.Dispatcher.Invoke((Action)(() =>
            {
                start_find_button.IsEnabled = false;
                unknown_button.IsEnabled = false;
                finding_label.Content = "Couldn't locate information.";

                header_label.Content = "Task 2 - Round " + (curr_round + 1);

                if (curr_round == 3)
                {
                    start_find_button.Click -= Start_Find_Click;
                    start_find_button.IsEnabled = false;
                    task_grid.Children.Remove(start_find_button);
                    task_grid.Children.Remove(unknown_button);
                    task_grid.Children.Remove(finding_label);

                    header_label.Content = "";

                    SoD.SendToDevices.WithID(controllerID, "NextTask", null);
                }
            }));
        }

        void Start_Find_Click(object sender, RoutedEventArgs e)
        {
            start_find_button.IsEnabled = false;
            unknown_button.IsEnabled = true;
            finding_label.Content = "Walk around to find information.";

            Dictionary<String, String> payload = new Dictionary<string, string>();
            payload.Add("round", rounds[curr_round].ToString());
            SoD.SendToDevices.WithID(controllerID, "StartFindRound", payload);
            curr_round++;
        }



        private void FoundDataPoint()
        {
            
            //Dictionary<String, String> payload = new Dictionary<string, string>();
            
            //SoD.SendToDevices.WithID(controllerID, "FoundDataPoint", payload);

            this.Dispatcher.Invoke((Action)(() =>
            {
                start_find_button.IsEnabled = false;
                unknown_button.IsEnabled = false;
                finding_label.Content = "Found Information!";

                header_label.Content = "Task 2 - Round " + (curr_round + 1);

                if (curr_round == 3)
                {
                    start_find_button.Click -= Start_Find_Click;
                    start_find_button.IsEnabled = false;
                    task_grid.Children.Remove(start_find_button);
                    task_grid.Children.Remove(unknown_button);
                    task_grid.Children.Remove(finding_label);

                    header_label.Content = "";

                    SoD.SendToDevices.WithID(controllerID, "NextTask", null);
                }
            }));
            
        }

        #endregion


        #region Task 3

        private void LoadTaskThree()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                GenerateRandomOrder();
                header_label.Content = "Task 3 - Round " + (curr_round + 1);

                CurrentTask = 3;

                CreateStartReceiveButton();
                CreateResponseButtons();
                CreateImage();
            }));
        }

        private void ShowInformation()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                image.Visibility = System.Windows.Visibility.Visible;
            }));
        }

        private void CreateImage()
        {
            var path = System.IO.Path.GetFullPath("image.png");
            var uri = new Uri(path);
            var bitmap = new BitmapImage(uri);
            image.Source = bitmap;

            image.Visibility = System.Windows.Visibility.Hidden;
        }

        private void CreateStartReceiveButton()
        {
            start_receive_button = new Button();

            // set standard features
            start_receive_button = AddFeaturesToButton(start_receive_button);

            //custom features
            start_receive_button.Content = "Start";
            Grid.SetRow(start_receive_button, 1);
            Grid.SetColumn(start_receive_button, 1);
            Grid.SetColumnSpan(start_receive_button, 2);
            start_receive_button.Click += Start_Receive_Click;
            start_receive_button.IsEnabled = true;

            task_grid.Children.Add(start_receive_button);
        }

        void Start_Receive_Click(object sender, RoutedEventArgs e)
        {
            start_receive_button.IsEnabled = false;
            table_button.IsEnabled = true;
            wall1_button.IsEnabled = true;
            wall2_button.IsEnabled = true;
            unknown_button.IsEnabled = true;

            Dictionary<String, String> payload = new Dictionary<string, string>();
            payload.Add("round", rounds[curr_round].ToString());
            SoD.SendToDevices.WithID(controllerID, "StartReceiveRound", payload);
            curr_round++;
        }


        private void Task3_Response_Clicked(object sender, RoutedEventArgs e)
        {
            Button response = (Button)sender;

            Dictionary<String, String> payload = new Dictionary<string, string>();
            payload.Add("answer", response.Content.ToString());
            SoD.SendToDevices.WithID(controllerID, "Task3Response", payload);


            start_receive_button.IsEnabled = true;
            table_button.IsEnabled = false;
            wall1_button.IsEnabled = false;
            wall2_button.IsEnabled = false;
            unknown_button.IsEnabled = false;
            image.Visibility = System.Windows.Visibility.Hidden;

            header_label.Content = "Task 3 - Round " + (curr_round + 1);

            if (curr_round == 3)
            {
                task_grid.Children.Remove(table_button);
                task_grid.Children.Remove(wall1_button);
                task_grid.Children.Remove(wall2_button);
                task_grid.Children.Remove(unknown_button);

                start_receive_button.IsEnabled = false;
                start_receive_button.Click -= send_button_Click;

                task_grid.Children.Remove(start_receive_button);

                header_label.Content = "";

                SoD.SendToDevices.WithID(controllerID, "NextTask", null);
            }
        }

        #endregion


        private void Window_Closed(object sender, EventArgs e)
        {
            disconnectSoD();
        }
    }
}
