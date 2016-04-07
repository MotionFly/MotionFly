using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CrazyflieAPI;
using Microsoft.Kinect;
using MotionFly.Kinect;
using MotionFly.MovementDetection;
using TrackingState = MotionFly.MovementDetection.MotionDetection.TrackingState;

namespace MotionFly.ViewModels
{
    /// <summary>
    /// View model of the main window
    /// </summary>
    public class MainWindowViewModel : ViewModel
    {
        #region Constants
        /// <summary>
        /// Brush for connection label
        /// </summary>
        private readonly Brush ConnectedBrush = Brushes.GreenYellow;
        /// <summary>
        /// Brush for connection label
        /// </summary>
        private readonly Brush DisconnectedBrush = Brushes.OrangeRed;
        /// <summary>
        /// String for connnection button
        /// </summary>
        private const string ConnectString = "Connect";
        /// <summary>
        /// String for connnection button
        /// </summary>
        private const string DisconnectString = "Disconnect";
        /// <summary>
        /// Kinect connection string
        /// </summary>
        private const string KinectAvailableString = "Kinect is available";
        /// <summary>
        /// Kinect connection string
        /// </summary>
        private const string KinectUnavailableString = "Kinect is unavailable";
        #endregion

        #region Properties
        /// <summary>
        /// Determines if the transfered data is logged
        /// </summary>
        public bool IsCrazyflieDebuggingActive
        {
            get { return crazyflieApi.DebugMode; }
            set { crazyflieApi.DebugMode = value; }
        }
        private WriteableBitmap colorBitmap = null;
        /// <summary>
        /// Image of the colour sensor, with overlay
        /// </summary>
        public ImageSource ImageSource
        {
            get { return colorBitmap; }
        }
        /// <summary>
        /// Brush for kinect connection label
        /// </summary>
        public Brush KinectConnectionLabelColor
        {
            get
            {
                if (basicSensor.IsSensorOpen)
                {
                    return ConnectedBrush;
                }
                else
                {
                    return DisconnectedBrush;
                }
            }
        }
        /// <summary>
        /// Text for kinect button
        /// </summary>
        public string KinectConnectionButtonText
        {
            get
            {
                if (basicSensor.IsSensorOpen)
                {
                    return DisconnectString;
                }
                else
                {
                    return ConnectString;
                }
            }
        }
        /// <summary>
        /// Text for kinect connection label
        /// </summary>
        public string KinectIsAvailableString
        {
            get
            {
                if (basicSensor.IsSensorOpen &&
                    basicSensor.Sensor.IsAvailable)
                {
                    return KinectAvailableString;
                }
                else
                {
                    return KinectUnavailableString;
                }
            }
        }
        /// <summary>
        /// Brush for crazyflie connection label
        /// </summary>
        public Brush CrazyflieConnectionLabelColor
        {
            get
            {
                if (connectionHelper.Running)
                {
                    return ConnectedBrush;
                }
                else
                {
                    return DisconnectedBrush;
                }
            }
        }
        /// <summary>
        /// Text for crazyflie button
        /// </summary>
        public string CrazyflieConnectionButtonText
        {
            get
            {
                if (connectionHelper.Running)
                {
                    return DisconnectString;
                }
                else
                {
                    return ConnectString;
                }
            }
        }
        /// <summary>
        /// Button of the tracking button
        /// </summary>
        public string TrackingButtonText
        {
            get
            {
                switch (motionDetection.TrackingStatus)
                {
                    case TrackingState.Idle:
                        return "Search for Operator";
                    case TrackingState.Searching:
                        return "Searching for Operator";
                    case TrackingState.LearnLeftHandMiddle:
                        return "Learn Left Hand Middle";
                    case TrackingState.LearnLeftHandTop:
                        return "Learn Left Hand Top";
                    case TrackingState.LearnLeftHandBottom:
                        return "Learn Left Hand Bottom";
                    case TrackingState.LearnLeftHandLeft:
                        return "Learn Left Hand Left";
                    case TrackingState.LearnLeftHandRight:
                        return "Learn Left Hand Right";
                    case TrackingState.LearnRightHandMiddle:
                        return "Learn Right Hand Middle";
                    case TrackingState.LearnRightHandTop:
                        return "Learn Right Hand Top";
                    case TrackingState.LearnRightHandBottom:
                        return "Learn Right Hand Bottom";
                    case TrackingState.LearnRightHandLeft:
                        return "Learn Right Hand Left";
                    case TrackingState.LearnRightHandRight:
                        return "Learn Right Hand Right";
                    case TrackingState.Tracking:
                        return "Tracking...";
                    default:
                        return "Unknown Tracking State";
                }
            }
        }
        /// <summary>
        /// Determines if the cancel button is enabled
        /// </summary>
        public bool IsCancelTrackingButtonEnabled
        {
            get { return motionDetection.TrackingStatus != TrackingState.Idle; }
        }
        /// <summary>
        /// Targeted thrust value
        /// </summary>
        public string ThrustValue
        {
            get { return motionDetection.ThrustValue.ToString(); }
        }
        /// <summary>
        /// Targeted yaw value
        /// </summary>
        public string YawValue
        {
            get { return motionDetection.YawValue.ToString(); }
        }
        /// <summary>
        /// Targeted pitch value
        /// </summary>
        public string PitchValue
        {
            get { return motionDetection.PitchValue.ToString(); }
        }
        /// <summary>
        /// Targeted roll value
        /// </summary>
        public string RollValue
        {
            get { return motionDetection.RollValue.ToString(); }
        }
        #endregion

        private BasicSensor basicSensor = BasicSensor.Instance;
        private ConnectionHelper connectionHelper = ConnectionHelper.Instance;
        private CrazyflieAPI.CrazyflieAPI crazyflieApi = CrazyflieAPI.CrazyflieAPI.Instance;
        private MotionDetection motionDetection = MotionDetection.Instance;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            colorBitmap = new WriteableBitmap(basicSensor.ColorDescription.Width, basicSensor.ColorDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            basicSensor.IsAvailableChanged += BasicSensor_IsAvailableChanged;
            basicSensor.ColorFrameArrived += BasicSensor_ColorFrameArrived;

            motionDetection.InputChanged += MotionDetection_InputChanged;
        }

        /// <summary>
        /// Connects/deconnects the kinect
        /// </summary>
        public void ToggleKinectConnection()
        {
            if (basicSensor.IsSensorOpen)
            {
                basicSensor.CloseSensor();
            }
            else
            {
                basicSensor.OpenSensor();
            }

            OnPropertyChanged(nameof(KinectConnectionLabelColor));
            OnPropertyChanged(nameof(KinectConnectionButtonText));
            OnPropertyChanged(nameof(KinectIsAvailableString));
        }

        /// <summary>
        /// Connects/deconnects the crazyflie client
        /// </summary>
        public void ToggleCrazyflieConnection()
        {
            if (connectionHelper.Running)
            {
                connectionHelper.StopThread();
                crazyflieApi.Disconnect();
            }
            else
            {
                crazyflieApi.Connect();
                connectionHelper.StartThread();

            }

            OnPropertyChanged(nameof(CrazyflieConnectionLabelColor));
            OnPropertyChanged(nameof(CrazyflieConnectionButtonText));
        }

        /// <summary>
        /// Handler for Kinect.IsAvailableChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasicSensor_IsAvailableChanged(object sender, Microsoft.Kinect.IsAvailableChangedEventArgs e)
        {
            OnPropertyChanged(nameof(KinectIsAvailableString));
        }

        /// <summary>
        /// Handler for ColorFrameReader.FrameArrived event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasicSensor_ColorFrameArrived(object sender, Microsoft.Kinect.ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        /// <summary>
        /// Goes to the next tracking step
        /// </summary>
        public void NextTrackingStep()
        {
            motionDetection.NextStep();
            OnPropertyChanged(nameof(TrackingButtonText));
            OnPropertyChanged(nameof(IsCancelTrackingButtonEnabled));
        }

        /// <summary>
        /// Cancels tracking
        /// </summary>
        public void CancelTracking()
        {
            motionDetection.CancelTracking();
            OnPropertyChanged(nameof(TrackingButtonText));
            OnPropertyChanged(nameof(IsCancelTrackingButtonEnabled));
        }

        /// <summary>
        /// Handler for MotionDetection.InputChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MotionDetection_InputChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ThrustValue));
            OnPropertyChanged(nameof(YawValue));
            OnPropertyChanged(nameof(PitchValue));
            OnPropertyChanged(nameof(RollValue));
        }
    }
}
