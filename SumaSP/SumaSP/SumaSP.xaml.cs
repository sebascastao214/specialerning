//-------------------------------------------------------------------------------------
// <copyright file="squat(ROM)window.xaml.cs" company="Special Learning">
//     Copyright (c) Special Learning.  All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------

#region libraries references

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Globalization;


// Coding4Fun Kinect Libraries (http://c4fkinect.codeplex.com/), 
//Libraries for Hoverbuttons, ScalePosition
using Coding4Fun.Kinect.Wpf;
using Coding4Fun.Kinect.Wpf.Controls;
using System.Media;
//using Microsoft.Samples.Kinect.WpfViewers;
#endregion

#region Special Learning Ejercicio Suma

namespace SumaSP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        // SpecialLearning Timer
        private readonly System.Windows.Threading.DispatcherTimer _timer;

        // (Int) TimerCounter, contains the increment of the timer tick
        public int TimerCounter;

        // (Int) TimeSelection, contains user time selection
        public int TimeSelection = 4;

        // (Int) Second, Timer seconds
        public int Second;

        // (Int) Minute, Timer minutes
        public int Minute;

        // (bool)BoolStartTimer, Enable/Disable Timer
        public bool BoolStartTimer;

        // (bool)BoolStartSquat, Enable/Disable squat detection
        public bool BoolStartSquat;

        // (bool)Repetition, Enable/Disable squat detection when user Performs a good squat
        public bool Repetitions;

        // (double) FinalAngle, Squat angle value
        public double FinalAngle;

        // 16 degrees as a Final Angle
        const double RangeOfFinalAnglePositive = 16;

        // 3 degrees as a Final Angle
        const double RangeOfFinalAngleNegative = 3;

        // (int)RepetitionCounter, number of repetitions chosen, default value (10)
        public int RepetitionCounter = 10; 

        public int RepetitionCounterCopy;

        

        public int RepsPerformed;

        public bool HideCursor = false;

        // max value returned
        const float MaxDepthDistance = 4095;

        // min value returned
        const float MinDepthDistance = 850;

        //Depth Value parameters
        const float MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

        //(int) HigherAngle, squat maximum Angle, (-10) reference value 
        public int HigherAngle = -10;

        //(int) SmallerAngle, squat minimum Angle, (1000) reference value 
        public int SmallerAngle = 1000; 

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor _kinect;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        //private WriteableBitmap _colorImageBitmap;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        //private Int32Rect _colorImageBitmapRect;
        //private int _colorImageStride;
        private Skeleton[] _frameSkeletons;

        /// <summary>
        /// List of buttons for each interface
        /// </summary>
        List<Button> _buttons;
        private List<Button> _btnSetConfigurations;
        private List<Button> _btndashboard;
        private List<Button> _btnKeyPad;

        static Button _selected;

        float _handX;
        float _handY;     
        
        //Booleans are Change to TRUE if user is Synchronized and ready
        public bool AngleIsCaptured;
        public bool AngleIsSet;                               
        
         /// <summary>
        /// Active Kinect Sensor
        /// </summary>
        private KinectSensor _sensor;

        /// <summary>
        /// Buttons Sound
        /// </summary>
        SoundPlayer _buttonSound;
        SoundPlayer _VozSuma;

        //My new Random Number
        Random randomNumber = new Random();


        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            InitializeButtons();
            kinectCursorMainWindow.Click += KinectCursorMainWindowClick;
            kinectCursorSettings.Click += KinectCursorSettingsClick;
            kinectCursorDashbd.Click += KinectCursorDashbdClick;


            this.Loaded += (s, e) => DiscoverKinectSensor();
            this.Unloaded += (s, e) => { this.Kinect = null; };

            _timer = new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1) };
            _timer.Tick += TimerTick;
            

        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    
                    break;
                }
                else
                {
                    MessageBox.Show("POR FAVOR CONECTE UN DISPOSITIVO KINECT");
                    
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.VideoStream.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
            var imageClockPath = new Image
            {
                Source =
                    new BitmapImage(
                        new Uri(
                            Path.GetDirectoryName(
                                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) +
                            "/Images/imageClock.png"))
            };
            imageClock.Source = imageClockPath.Source;            
            

            //sign up for the event
            //kinectSensorChooserSP.KinectSensorChanged += KinectSensorChooserPpKinectSensorChanged;
            //canvasPersonalPreferences.Margin = new Thickness(244, 109, 0, 150);

            var soundPath = new Uri(Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) +
                                    "/Sounds/snd_buttonselect.wav");

            var soundPath2 = new Uri(Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) +
                                    "/Sounds/Vozsumasp.wav");

            _buttonSound = new SoundPlayer(soundPath.ToString());

            
            _VozSuma = new SoundPlayer(soundPath2.ToString());

        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }
        
        public void TimerTick(object sender, EventArgs e)
        {
            TimerCounter += 1;
            Minute = TimerCounter / 60;
            Second = TimerCounter % 60;
            txtBlockTimer.Text = Minute.ToString("00") + ":" + Second.ToString("00");
        }

        
        //initialize buttons to be checked
        private void InitializeButtons()
        {
            _btndashboard = new List<Button> { BtnBack };
            _buttons = new List<Button> { btnPlay, btnStop, btnPause, btnQuit, btnDashboard, btnReset,btnNumber0, btnNumber1, btnNumber2, btnNumber3, btnNumber4, btnNumber5,
                                          btnNumber6, btnNumber7, btnNumber8, btnNumber9, btnNumber0,
                                          btnSuma, btnAceptar_correcto };
            _btnSetConfigurations = new List<Button> { btnAddAngle, btnMinusAngle, btnAddReps, btnMinusReps, 
                                                       BtnAddTime, BtnReduceTime, btnContinue };
            _btnKeyPad = new List<Button>{btnNumber0, btnNumber1, btnNumber2, btnNumber3, btnNumber4, btnNumber5,
                                          btnNumber6, btnNumber7, btnNumber8, btnNumber9, btnNumber0,
                                          btnSuma};
        }

        //raise event for Kinect sensor status changed
        private void DiscoverKinectSensor()
        {
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }
                

        public KinectSensor Kinect
        {
            get { return this._kinect; }
            set
            {
                if (this._kinect != value)
                {
                    if (this._kinect != null)
                    {
                        UninitializeKinectSensor(this._kinect);
                        this._kinect = null;
                    }
                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this._kinect = value;
                        InitializeKinectSensor(this._kinect);
                    }
                }
            }
        }


        /// <summary>
        /// Execute shutdown tasks and UninitializeKinectSensor
        /// </summary>
        /// <param name="kinectSensor">object sending the event</param>

        private void UninitializeKinectSensor(KinectSensor kinectSensor)
        {
            if (kinectSensor != null)
            {
                kinectSensor.Stop();
                kinectSensor.SkeletonFrameReady -= KinectSkeletonFrameReady;
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="kinectSensor">object sending the event</param>

        private void InitializeKinectSensor(KinectSensor kinectSensor)
        {
            if (kinectSensor != null)
            {
                // Turn on the color stream to receive color frames
                ColorImageStream colorStream = kinectSensor.ColorStream;
                colorStream.Enable();                                          

                

                
                //reduces noise on skeletal tracking methods
                //var parameters = new TransformSmoothParameters
                //{
                //    Smoothing = 0.3f,
                //    Correction = 0.0f,
                //    Prediction = 0.0f,
                //    JitterRadius = 1.0f,
                //    MaxDeviationRadius = 0.5f
                //};

                //Pass TransformSmoothParameters to SkeletonStream
                //kinectSensor.SkeletonStream.Enable(parameters);                               

                // Add an event handler to be called whenever there is new skeleton data
                kinectSensor.SkeletonFrameReady += KinectSkeletonFrameReady;

                // Add an event handler to be called whenever there is new color frame data
                //kinectSensor.ColorFrameReady += KinectColorFrameReady;
                kinectSensor.Start();
                this._frameSkeletons = new Skeleton[this.Kinect.SkeletonStream.FrameSkeletonArrayLength];

            }
        }


        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        //private void KinectColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        //{
        //    using (ColorImageFrame frame = e.OpenColorImageFrame())
        //    {
        //        if (frame != null)
        //        {
        //            var pixelData = new byte[frame.PixelDataLength];
        //            frame.CopyPixelDataTo(pixelData);
        //            this._colorImageBitmap.WritePixels(this._colorImageBitmapRect, pixelData,
        //                this._colorImageStride, 0);
        //        }
        //    }
        //}

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void KinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            lblUserRepsDashbd.Content = (RepetitionCounterCopy - RepetitionCounter);

            if (HigherAngle == -10 && SmallerAngle == 1000)
            {
                lblMaximumAngleDashbd.Content = "0";
                lblMinimumAngleDashbd.Content = "0";
            }
            else
            {
                lblMaximumAngleDashbd.Content = HigherAngle;
                lblMinimumAngleDashbd.Content = SmallerAngle;
            }

            try
            {
                using (SkeletonFrame frame = e.OpenSkeletonFrame())
                {
                    if (frame != null)
                    {
                        frame.CopySkeletonDataTo(this._frameSkeletons);
                        Skeleton skeleton = GetPrimarySkeleton(this._frameSkeletons);

                        //Joint variables
                        if (skeleton == null)
                        {
                            return;
                        }

                        #region Joints
                            // Joints to be tracked
                            var hipCenter = skeleton.Joints[JointType.HipCenter];
                            var hipleft = skeleton.Joints[JointType.HipLeft];
                            var hipRight = skeleton.Joints[JointType.HipRight];

                            var leftKnee = skeleton.Joints[JointType.KneeLeft];
                            var rightKnee = skeleton.Joints[JointType.KneeRight];

                            var leftAnkle = skeleton.Joints[JointType.AnkleLeft];
                            var rightAnkle = skeleton.Joints[JointType.AnkleRight];

                            double hipCenterZPosition = hipCenter.Position.Z;
                        #endregion

                                                
                        

                       
                        #endregion

                        #region Time/Reps finished
                        
                        if (TimeSelection == Minute)
                        {
                            _timer.Stop();
                            LblUserMessages.Content = "PRESS DASHBOARD TO SAVE THIS INFORMATION AND SEE RESULTS";
                            btnDashboard.Foreground = Brushes.Red;
                            HideCursor = false;
                            kinectCursorMainWindow.Visibility = Visibility.Visible;
                            btnReset.Visibility = Visibility.Visible;

                        }

                        if (RepetitionCounter == 0)
                        {
                            _timer.Stop();
                            LblUserMessages.Content = "PRESS DASHBOARD TO SAVE THIS INFORMATION AND SEE RESULTS";
                            btnDashboard.Foreground = Brushes.Red;
                            HideCursor = false;
                            kinectCursorMainWindow.Visibility = Visibility.Visible;
                            btnReset.Visibility = Visibility.Visible;
                        }

                        #endregion

                        #region Syncing User

                        if (hipCenter.TrackingState == JointTrackingState.NotTracked)
                        {
                            if (TimeSelection == Minute)
                            {
                                _timer.Stop();
                                

                            }
                            if (RepetitionCounter == 0)
                            {
                                _timer.Stop();
                                
                            }
                            LblUserMessages.Content = ".......null...........";
                        }

                        //hipCenterZPosition value is expressed in meters (2.0) is equal to Two meters
                        if (hipCenterZPosition <= 1.90)
                        {
                            UserPositioninglabel.Content = "  MUEVETE HACIA ATRAS";
                            UserPositioninglabel.Foreground = Brushes.LimeGreen;
                            
                            
                            if (TimeSelection == Minute)
                            {
                                _timer.Stop();                                

                            }
                            if (RepetitionCounter == 0)
                            {
                                _timer.Stop();                                

                            }

                        }

                        if (hipCenterZPosition > 2.30)
                        {
                            UserPositioninglabel.Content = "MUEVETE HACIA ADELANTE";
                            UserPositioninglabel.Foreground = Brushes.LimeGreen;
                            
                            
                            if (TimeSelection == Minute)
                            {
                                _timer.Stop();
                                

                            }
                            if (RepetitionCounter == 0)
                            {
                                _timer.Stop();                               

                            }

                        }


                        if (hipCenterZPosition > 1.90 && hipCenterZPosition < 2.30)
                        {
                            UserPositioninglabel.Content = "  EXCELENTE POSICIÓN";
                            UserPositioninglabel.Foreground = Brushes.White;
                            

                            if (TimeSelection == Minute)
                            {
                                _timer.Stop();                               

                            }

                            if (RepetitionCounter == 0)
                            {
                                _timer.Stop();                               

                            }

                            AngleIsCaptured = true;

                        }
                        #endregion                       


                        if (skeleton == null)
                        {
                            kinectCursorMainWindow.Visibility = Visibility.Collapsed;
                            kinectCursorSettings.Visibility = Visibility.Collapsed;
                            kinectCursorDashbd.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            Joint primaryHand = GetPrimaryHand(skeleton);
                            TrackHand(primaryHand);

                        }
                    }
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        // Scale Kinect cursor on window(Resolution), Coding4Fun function
        private void ScalePosition(FrameworkElement element, Joint joints)
        {
            //convert the value to X/Y
            //convert & scale (.3 = means 1/3 of joint distance)
            Joint scaledJoint = joints.ScaleTo(1400, 2000, .5f, .5f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);

        }

        //track and display hand
        private void TrackHand(Joint hand)
        {
            if (hand.TrackingState == JointTrackingState.NotTracked)
            {
                kinectCursorMainWindow.Visibility = Visibility.Collapsed;
                kinectCursorSettings.Visibility = Visibility.Collapsed;
                kinectCursorDashbd.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (HideCursor==false)
                {
                    kinectCursorMainWindow.Visibility = Visibility.Visible;
                    kinectCursorSettings.Visibility = Visibility.Visible;
                    kinectCursorDashbd.Visibility = Visibility.Visible;

                    //Map a joint location to a point on the depth map
                    //hand
                    DepthImagePoint point = this.Kinect.MapSkeletonPointToDepth(hand.Position, DepthImageFormat.Resolution640x480Fps30);
                    _handX = (int)((point.X * GridLayoutRoot.ActualWidth / this.Kinect.DepthStream.FrameWidth) -
                        (kinectCursorMainWindow.ActualWidth / 2.0));
                    _handY = (int)((point.Y * GridLayoutRoot.ActualHeight / this.Kinect.DepthStream.FrameHeight) -
                        (kinectCursorMainWindow.ActualHeight / 2.0));
                    Canvas.SetLeft(kinectCursorMainWindow, _handX);
                    Canvas.SetTop(kinectCursorMainWindow, _handY);
                    ScalePosition(kinectCursorMainWindow, hand);

                    //_handX = (int)((point.X * GridLayoutRoot.ActualWidth / this.Kinect.DepthStream.FrameWidth) -
                    //    (kinectCursorSettings.ActualWidth / 2.0));
                    //_handY = (int)((point.Y * GridLayoutRoot.ActualHeight / this.Kinect.DepthStream.FrameHeight) -
                    //    (kinectCursorSettings.ActualHeight / 2.0));
                    //Canvas.SetLeft(kinectCursorSettings, _handX);
                    //Canvas.SetTop(kinectCursorSettings, _handY);
                    //ScalePosition(kinectCursorSettings, hand);


                    //_handX = (int)((point.X * GridLayoutRoot.ActualWidth / this.Kinect.DepthStream.FrameWidth) -
                    //    (kinectCursorDashbd.ActualWidth / 2.0));
                    //_handY = (int)((point.Y * GridLayoutRoot.ActualHeight / this.Kinect.DepthStream.FrameHeight) -
                    //    (kinectCursorDashbd.ActualHeight / 2.0));
                    //Canvas.SetLeft(kinectCursorDashbd, _handX);
                    //Canvas.SetTop(kinectCursorDashbd, _handY);
                    //ScalePosition(kinectCursorDashbd, hand);



                    if (isHandOver(kinectCursorMainWindow, _buttons)) kinectCursorMainWindow.Hovering();
                    else kinectCursorMainWindow.Release();

                    if (isHandOver(kinectCursorSettings, _btnSetConfigurations)) kinectCursorSettings.Hovering();
                    else kinectCursorSettings.Release();

                    if (isHandOver(kinectCursorDashbd, _btndashboard)) kinectCursorDashbd.Hovering();
                    else kinectCursorDashbd.Release();

                    if (isHandOver(kinectCursorDashbd, _btnKeyPad)) kinectCursorDashbd.Hovering();
                    else kinectCursorDashbd.Release();

                    if (hand.JointType == JointType.HandRight)
                    {

                        var imageRightHand = new Image
                        {
                            Source =
                                new BitmapImage(
                                    new Uri(
                                        Path.GetDirectoryName(
                                            System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) +
                                        "/Images/hand_r.png"))
                        };                      


                        kinectCursorMainWindow.ImageSource = imageRightHand.Source.ToString();
                        kinectCursorMainWindow.ActiveImageSource = imageRightHand.Source.ToString();

                        kinectCursorSettings.ImageSource = imageRightHand.Source.ToString();
                        kinectCursorSettings.ActiveImageSource = imageRightHand.Source.ToString();

                        kinectCursorDashbd.ImageSource = imageRightHand.Source.ToString();
                        kinectCursorDashbd.ActiveImageSource = imageRightHand.Source.ToString();

                    }
                    else
                    {

                        var imageLeftHand = new Image
                        {
                            Source =
                                new BitmapImage(
                                    new Uri(
                                        Path.GetDirectoryName(
                                            System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) +
                                        "/Images/hand_l.png"))
                        };

                        kinectCursorMainWindow.ImageSource = imageLeftHand.Source.ToString();
                        kinectCursorMainWindow.ActiveImageSource = imageLeftHand.Source.ToString();

                        kinectCursorSettings.ImageSource = imageLeftHand.Source.ToString();
                        kinectCursorSettings.ActiveImageSource = imageLeftHand.Source.ToString();

                        kinectCursorDashbd.ImageSource = imageLeftHand.Source.ToString();
                        kinectCursorDashbd.ActiveImageSource = imageLeftHand.Source.ToString();
                    } 
                }
            }
        }

        //detect if hand is overlapping over any button
        private bool isHandOver(FrameworkElement hand, IEnumerable<Button> buttonslist)
        {
            var handTopLeft = new Point(Canvas.GetLeft(hand), Canvas.GetTop(hand));
            var handX = handTopLeft.X + hand.ActualWidth / 2;
            var handY = handTopLeft.Y + hand.ActualHeight / 2;

            foreach (Button target in buttonslist)
            {
                var targetTopLeft = new Point(Canvas.GetLeft(target), Canvas.GetTop(target));
                if (handX > targetTopLeft.X && handX < targetTopLeft.X + target.Width && handY > targetTopLeft.Y && handY < targetTopLeft.Y + target.Height)
                {
                    _selected = target;
                    return true;

                }
            }
            return false;
        }

        //get the hand closest to the Kinect sensor
        private static Joint GetPrimaryHand(Skeleton skeleton)
        {
            var primaryHand = new Joint();
            if (skeleton != null)
            {
                primaryHand = skeleton.Joints[JointType.HandLeft];
                Joint rightHand = skeleton.Joints[JointType.HandRight];
                if (rightHand.TrackingState != JointTrackingState.NotTracked)
                {
                    if (primaryHand.TrackingState == JointTrackingState.NotTracked)
                    {
                        primaryHand = rightHand;
                    }
                    else
                    {
                        if (primaryHand.Position.Z > rightHand.Position.Z)
                        {
                            primaryHand = rightHand;
                        }
                    }
                }
            }
            return primaryHand;
        }


        //get the skeleton closest to the Kinect sensor
        private static Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;
            if (skeletons != null)
            {
                for (int i = 0; i < skeletons.Length; i++)
                {
                    if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (skeleton == null)
                        {
                            skeleton = skeletons[i];
                        }
                        else
                        {
                            if (skeleton.Position.Z > skeletons[i].Position.Z)
                            {
                                skeleton = skeletons[i];
                            }
                        }
                    }
                }
            }
            return skeleton;
        }    



        void KinectSensorChooserPpKinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            var oldSensor = (KinectSensor)e.OldValue;

            //stop the old sensor
            if (oldSensor != null)
            {
                oldSensor.Stop();
            }

            //get the new sensor
            var newSensor = (KinectSensor)e.NewValue;
            if (newSensor == null)
            {
                return;
            }

            //register for event and enable Kinect sensor features you want
            // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.VideoStream.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                
            //newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            newSensor.SkeletonStream.Enable();
            //newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            

            //sign up for events if you want to get at API directly
            newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(NewSensorAllFramesReady);


            try
            {
                newSensor.Start();
            }
            catch (System.IO.IOException)
            {
                //this happens if another app is using the Kinect
                //kinectSensorChooserSP.AppConflictOccurred();
            }
        }

        //this event fires when Color/Depth/Skeleton are synchronized
        void NewSensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }

                byte[] pixels = GenerateColoredBytes(depthFrame);

                //number of bytes per row width * 4 (B,G,R,Empty)
                int stride = depthFrame.Width * 4;

                ////create image
                //imageDepthSync.Source =
                //    BitmapSource.Create(depthFrame.Width, depthFrame.Height,
                //    96, 96, PixelFormats.Bgr32, null, pixels, stride);

            }
        }

        private byte[]GenerateColoredBytes(DepthImageFrame depthFrame)
        {

            //get the raw data from kinect with the depth for every pixel
            var rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            //use depthFrame to create the image to display on-screen
            //depthFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            var pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            //Bgr32  - Blue, Green, Red, empty byte
            //Bgra32 - Blue, Green, Red, transparency 
            //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparent

            //hardcoded locations to Blue, Green, Red (BGR) index positions       
            const int blueIndex = 0;
            const int greenIndex = 1;
            const int redIndex = 2;


            //loop through all distances
            //pick a RGB color based on distance
            for (int depthIndex = 0, colorIndex = 0;
                depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                depthIndex++, colorIndex += 4)
            {
                //get the player (requires skeleton tracking enabled for values)
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                //gets the depth value
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (player > 0)
                {
                    if (depth < 2000)
                    {
                        pixels[colorIndex + blueIndex] = Colors.Red.B;
                        pixels[colorIndex + greenIndex] = Colors.Red.G;
                        pixels[colorIndex + redIndex] = Colors.Red.R;
                    }
                    else
                        if (depth > 2000 & depth <= 2900)
                        {
                            pixels[colorIndex + blueIndex] = Colors.Green.B;
                            pixels[colorIndex + greenIndex] = Colors.Green.G;
                            pixels[colorIndex + redIndex] = Colors.Green.R;

                        }

                        else
                            if (depth > 2900)
                            {
                                pixels[colorIndex + blueIndex] = Colors.OrangeRed.B;
                                pixels[colorIndex + greenIndex] = Colors.OrangeRed.G;
                                pixels[colorIndex + redIndex] = Colors.OrangeRed.R;
                            }
                }

            }


            return pixels;
        }


        public static byte CalculateIntensityFromDepth(int distance)
        {
            //formula for calculating monochrome intensity for histogram
            return (byte)((Math.Max(distance - MinDepthDistance, 0)
                / (MaxDepthDistanceOffset)));
        }

        #region Kinect Cursor Events

        void KinectCursorMainWindowClick(object sender, RoutedEventArgs e)
        {
            _selected.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, _selected));
            _buttonSound.Play();

        }

        void KinectCursorSettingsClick(object sender, RoutedEventArgs e)
        {
            _selected.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, _selected));
            _buttonSound.Play();
        }

        void KinectCursorDashbdClick(object sender, RoutedEventArgs e)
        {
            _selected.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, _selected));
            _buttonSound.Play();
        }

        #endregion

        #region Interface buttons events
        float frandomNumber;
        Boolean hayPause = false;

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            btnPause.Visibility = Visibility.Visible;
            btnPlay.Visibility = Visibility.Hidden;

            if (hayPause != true)
            {
                _VozSuma.Play();

                frandomNumber = randomNumber.Next(1, 10);
                lblRandomNumber.Content = frandomNumber.ToString();


                btnReset.Visibility = Visibility.Collapsed;

                _timer.Start();

                BoolStartTimer = true;
                BoolStartSquat = true;
            }
            else
            {
                _timer.Start();

                BoolStartTimer = true;
                BoolStartSquat = true;
            }
        }


        private void StopButtonClick(object sender, RoutedEventArgs e)
        {                       
            _buttonSound.Play();
            _timer.Stop();
            TimerCounter = 0;
            BoolStartSquat = false;
            btnReset.Visibility = Visibility.Visible;
        }

        

        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            btnPause.Visibility = Visibility.Hidden;
            btnPlay.Visibility = Visibility.Visible;

            hayPause = true;
            _buttonSound.Play();            
            BoolStartTimer = false;
            BoolStartSquat = false;
            _timer.Stop();                        
            btnReset.Visibility = Visibility.Visible;
        }

        private void QuitButtonClick(object sender, RoutedEventArgs e)
        {
            //_kinect.Stop();
            this.Close();
            //Application.Current.Shutdown();
        }

        private void BtnAddRepsClick(object sender, RoutedEventArgs e)
        {
            _buttonSound.Play();
            if (RepetitionCounter >= 0 && RepetitionCounter <= 48)
            {
                RepetitionCounter += 2;
                RepetitionCounterCopy = RepetitionCounter;
                
            }

            if (RepetitionCounter > 0 && RepetitionCounter < 10)
            {
                lblReps.Content = " " + RepetitionCounter.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                lblReps.Content = "" + RepetitionCounter.ToString(CultureInfo.CurrentCulture);
            }
        }

        private void BtnMinusRepsClick(object sender, RoutedEventArgs e)
        {
            _buttonSound.Play();
            if (RepetitionCounter >= 5)
            {
                RepetitionCounter -= 2;
                RepetitionCounterCopy = RepetitionCounter;
                
            }
            if (RepetitionCounter > 0 && RepetitionCounter < 10)
            {
                lblReps.Content = " " + RepetitionCounter.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                lblReps.Content = "" + RepetitionCounter.ToString(CultureInfo.CurrentCulture);
            }
            if (RepetitionCounter <= 0)
            {
                RepetitionCounter = 0;
                lblReps.Content = " " + RepetitionCounter.ToString(CultureInfo.CurrentCulture);
                RepetitionCounter = 0;

            }
        }        

        private void BtnAddTimeClick(object sender, RoutedEventArgs e)
        {

            _buttonSound.Play();
            TimeSelection += 2;
            lblTimer.Content = TimeSelection.ToString(CultureInfo.CurrentCulture) + "'";

            if (TimeSelection >= 4 && TimeSelection < 10)
            {
                lblTimer.Content = " " + TimeSelection.ToString(CultureInfo.CurrentCulture) + "'";

            }
            if (TimeSelection > 10)
            {
                TimeSelection = 10;
                lblTimer.Content = "" + TimeSelection.ToString(CultureInfo.CurrentCulture) + "'";
            }
        }

        private void BtnReduceTimeClick(object sender, RoutedEventArgs e)
        {
            _buttonSound.Play();
            if (TimeSelection >= 4 && TimeSelection <= 10)
            {
                TimeSelection -= 2;

                lblTimer.Content = " " + TimeSelection.ToString(CultureInfo.CurrentCulture) + "'";

            }
            if (TimeSelection <= 4)
            {
                TimeSelection = 4;
                lblTimer.Content = " " + TimeSelection.ToString(CultureInfo.CurrentCulture) + "'";
            }
        }

        void BtnResetClick(object sender, RoutedEventArgs e)
        {
            _buttonSound.Play();
            btnDashboard.Foreground = Brushes.White;
            TimerCounter = 0;
            
            _timer.Stop();
            BoolStartTimer = false;
            
            RepetitionCounter = RepetitionCounterCopy;
            lblReps.Content = RepetitionCounterCopy.ToString(CultureInfo.InvariantCulture);
            
            LblUserMessages.Content = "PRESIONE PLAY PARA INICIAR DE NUEVO";
        }


        private void BtnContinueClick(object sender, RoutedEventArgs e)
        {
            _buttonSound.Play();
                        
            canvasPersonalPreferences.Visibility = Visibility.Collapsed;
            //canvasSpecialLearningInterface.Visibility = Visibility.Visible;
            
            if (TimeSelection < 10)
            {
                lblClock.Content = " " + TimeSelection.ToString(CultureInfo.InvariantCulture) + "'";
            }
            else
            {
                lblClock.Content = TimeSelection.ToString(CultureInfo.InvariantCulture) + "'";
            }
        }


        private void BtnDashboardClick(object sender, RoutedEventArgs e)
        {
            _buttonSound.Play();
            canvasActivityDashboard.Visibility = Visibility.Visible;
            HideCursor = false;
            kinectCursorMainWindow.Visibility = Visibility.Visible;            
            _timer.Stop();
            
            canvasMainDashboard.Visibility = Visibility.Visible;
            BtnBack.Visibility = Visibility.Visible;

            
            if (RepetitionCounterCopy == 0)
            {
                RepetitionCounterCopy = 1;
            }
            const int maximumPercentaje = 100;
            RepsPerformed = RepetitionCounterCopy - RepetitionCounter;
            int ruleThreeResult = ((RepsPerformed * maximumPercentaje) / RepetitionCounterCopy);

            progressBarReps.Minimum = 0;
            progressBarReps.Maximum = 100;
            progressBarReps.Value = ruleThreeResult;
            progressBarReps.Foreground = Brushes.DarkOrange;
            lblRepsPercentage.Content = ruleThreeResult + "% of goal " + RepetitionCounterCopy;

            /////////////////////////////////////////////
            lblShowTimerDashBd.Content = Minute.ToString("00") + ":" + Second.ToString("00");


            int rulethreeResultTime = ((TimerCounter * maximumPercentaje) / (60 * TimeSelection));
            progressBarTime.Minimum = 0;
            progressBarTime.Maximum = 100;
            //progressBarTime.Value = RulethreeResultTime;
            progressBarTime.Foreground = Brushes.DarkOrange;
            progressBarTime.Value = rulethreeResultTime;
            lblTimePercentage.Content = rulethreeResultTime + "% of goal " + TimeSelection + ":00 Minutes";

            ///////////////////////////////////////////////////////////////////////////////////////

            if (HigherAngle == -10 && SmallerAngle == 1000)
            {
                HigherAngle = 0;
                SmallerAngle = 0;
            }
        }

        private void BtnBackClick(object sender, RoutedEventArgs e)
        {
            _buttonSound.Play();
            
            GridLayoutRoot.Visibility = Visibility.Visible;
            canvasMainDashboard.Visibility = Visibility.Collapsed;
            BtnBack.Visibility = Visibility.Collapsed;
        }

        #endregion

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this._kinect)
            {
                this._kinect.Stop();
                this._sensor = null;
            }
        }

        //int numero1 = 0, numero2 = 0, numero3 = 0, numero4 = 0, numero5 = 0;
        //int numero6 = 0, numero7 = 0, numero8 = 0, numero9 = 0, numero0 = 0;
        //int suma;
       // int[] sumatoria;

        List<int> listNumeros = new List<int>();
        int sumatoria;      

        public void sumarNum()
        {          
            for (int i = 0; i < listNumeros.Count; i++)
            {
                sumatoria += listNumeros[i];
            }
        }

        public void mostrarSuma()
        {
            txtResultado.Text = "";

            for (int i = 0; i < listNumeros.Count; i++)
            {
                txtResultado.Text += listNumeros[i];
                if(i!=listNumeros.Count-1){
                    txtResultado.Text += " + ";
                }
            }
        }
        
        #region Eventos Botones        
        
        private void btnNumber1_Click(object sender, RoutedEventArgs e)
        {

            listNumeros.Add(1);
            mostrarSuma();
            
        }

        private void btnNumber2_Click(object sender, RoutedEventArgs e)
        {

            listNumeros.Add(2);
            mostrarSuma();
        }

        private void btnNumber3_Click(object sender, RoutedEventArgs e)
        {

            listNumeros.Add(3);
            mostrarSuma();
        }        
        

        private void btnNumber4_Click(object sender, RoutedEventArgs e)
        {
            listNumeros.Add(4);
            mostrarSuma();
        }

        private void btnNumber5_Click(object sender, RoutedEventArgs e)
        {
            listNumeros.Add(5);
            mostrarSuma();

        } 

        private void btnNumber6_Click(object sender, RoutedEventArgs e)
        {
            listNumeros.Add(6);
            mostrarSuma();
        }

        private void btnNumber7_Click(object sender, RoutedEventArgs e)
        {
            listNumeros.Add(7);
            mostrarSuma();
        }

        private void btnNumber8_Click(object sender, RoutedEventArgs e)
        {
            listNumeros.Add(8);
            mostrarSuma();
        }

        private void btnNumber9_Click(object sender, RoutedEventArgs e)
        {
            listNumeros.Add(9);
            mostrarSuma();
        }

        private void btnNumber0_Click(object sender, RoutedEventArgs e)
        {
            listNumeros.Add(0);
            mostrarSuma();
        }
        #endregion

        ButtonClass OperacionesAritmeticas = new ButtonClass();

        private void btnResultado_Click_1(object sender, RoutedEventArgs e)
        {
            sumarNum();

            txtResultado.Text += " = "+sumatoria;

            if (frandomNumber == sumatoria)
            {
                btnAceptar_correcto.Visibility = Visibility.Visible;
              //LblUserMessages.Content = "CORRECTO";
               // MessageBox.Show("Muy bien ¡¡¡\n\n La respuesta es CORRECTA", "SPECIAL LEARNING", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                btnAceptar_incorrecto.Visibility = Visibility.Visible;
                //LblUserMessages.Content = "INCORRECTO";
              //  MessageBox.Show("Lo siento !!!\n\n La respuesta es INCORRECTA", "SPECIAL LEARNING", MessageBoxButton.OK,MessageBoxImage.Error);   
                sumatoria = 0;
                txtResultado.Text = "";
                listNumeros.Clear();
            }          
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            btnAceptar_correcto.Visibility = Visibility.Hidden;
            btnAceptar_incorrecto.Visibility = Visibility.Hidden;
            frandomNumber = randomNumber.Next(1, 10);
            lblRandomNumber.Content = frandomNumber.ToString();
            sumatoria = 0;
            LblUserMessages.Content = "";
            txtResultado.Text = "";
            listNumeros.Clear();
        }

        private void btnLimpiarSuma_Click_1(object sender, RoutedEventArgs e)
        {
            sumatoria = 0;
            LblUserMessages.Content = "";
            txtResultado.Text = "";
            listNumeros.Clear();
        }

        private void kinectCursorMainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

    }
}
