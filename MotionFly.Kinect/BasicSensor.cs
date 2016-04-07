using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace MotionFly.Kinect
{
    /// <summary>
    /// Copied from ZundCutterInterface
    /// </summary>
    public class BasicSensor
    {
        #region Singleton
        /// <summary>
        /// Is Instance already made
        /// </summary>
        private static object instanceLock = new object();

        private static BasicSensor instance;

        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static BasicSensor Instance
        {
            get
            {
                lock (BasicSensor.instanceLock)
                {
                    if (BasicSensor.instance == null)
                    {
                        BasicSensor.instance = new BasicSensor();
                    }
                }
                return BasicSensor.instance;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private BasicSensor()
        {
            this.Sensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            this.depthDescription = this.Sensor.DepthFrameSource.FrameDescription;
            this.colorDescription = this.Sensor.ColorFrameSource.FrameDescription;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicSensor()
        {
            this.CloseSensor();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Default Kinect sensor
        /// </summary>
        public KinectSensor Sensor
        {
            get { return KinectSensor.GetDefault(); }
        }

        private bool isSensorOpen = false;

        /// <summary>
        /// Determines if the sensor is open
        /// </summary>
        public bool IsSensorOpen
        {
            get { return this.isSensorOpen; }
        }

        private DepthFrameReader depthFrameReader = null;

        /// <summary>
        /// Frame reader of the depth frame
        /// Sensor has to be opened, otherwise this property retruns a null reference
        /// </summary>
        public DepthFrameReader DepthFrameReader
        {
            get { return this.depthFrameReader; }
        }

        private FrameDescription depthDescription = null;

        /// <summary>
        /// Description of the depth frame
        /// </summary>
        public FrameDescription DepthDescription
        {
            get { return this.depthDescription; }
        }

        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Frame reader of the body frame
        /// Sensor has to be opened, otherwise this property retruns a null reference
        /// </summary>
        public BodyFrameReader BodyFrameReader
        {
            get { return this.bodyFrameReader; }
        }

        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Frame reader of the color frame
        /// Sensor has to be opened, otherwise this property retruns a null reference
        /// </summary>
        public ColorFrameReader ColorFrameReader
        {
            get { return this.colorFrameReader; }
        }

        private FrameDescription colorDescription = null;

        /// <summary>
        /// Description of the color frame
        /// </summary>
        public FrameDescription ColorDescription
        {
            get { return this.colorDescription; }
        }
        #endregion

        #region Events
        public event EventHandler<IsAvailableChangedEventArgs> IsAvailableChanged;
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (this.IsAvailableChanged != null)
            {
                this.IsAvailableChanged(sender, e);
            }
        }

        public event EventHandler SensorOpened;
        private void OnSensorOpened()
        {
            if (this.SensorOpened != null)
            {
                this.SensorOpened(this, new EventArgs());
            }
        }

        public event EventHandler SensorClosed;
        private void OnSensorClosed()
        {
            if (this.SensorClosed != null)
            {
                this.SensorClosed(this, new EventArgs());
            }
        }

        public event EventHandler<BodyFrameArrivedEventArgs> BodyFrameArrived;
        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            if (this.BodyFrameArrived != null)
            {
                this.BodyFrameArrived(sender, e);
            }
        }

        public event EventHandler<DepthFrameArrivedEventArgs> DepthFrameArrived;
        private void DepthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            if (this.DepthFrameArrived != null)
            {
                this.DepthFrameArrived(sender, e);
            }
        }

        public event EventHandler<ColorFrameArrivedEventArgs> ColorFrameArrived;
        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            if (this.ColorFrameArrived != null)
            {
                this.ColorFrameArrived(sender, e);
            }
        }
        #endregion

        /// <summary>
        /// Connects to the camera and assigns the sensors
        /// </summary>
        public void OpenSensor()
        {
            if (!this.IsSensorOpen)
            {
                this.Sensor.Open();
                this.depthFrameReader = this.Sensor.DepthFrameSource.OpenReader();
                this.depthFrameReader.FrameArrived += this.DepthFrameReader_FrameArrived;
                this.colorFrameReader = this.Sensor.ColorFrameSource.OpenReader();
                this.colorFrameReader.FrameArrived += this.ColorFrameReader_FrameArrived;
                this.bodyFrameReader = this.Sensor.BodyFrameSource.OpenReader();
                this.bodyFrameReader.FrameArrived += this.BodyFrameReader_FrameArrived;

                this.isSensorOpen = true;
                OnSensorOpened();
            }
        }

        /// <summary>
        /// Closes the connection to camera
        /// </summary>
        public void CloseSensor()
        {
            if (this.IsSensorOpen)
            {
                this.isSensorOpen = false;
                OnSensorClosed();

                if (this.depthFrameReader != null)
                {
                    this.depthFrameReader.FrameArrived -= this.DepthFrameReader_FrameArrived;
                    this.depthFrameReader.Dispose();
                    this.depthFrameReader = null;
                }
                if (this.colorFrameReader != null)
                {
                    this.colorFrameReader.FrameArrived -= this.ColorFrameReader_FrameArrived;
                    this.colorFrameReader.Dispose();
                    this.colorFrameReader = null;
                }
                if (this.bodyFrameReader != null)
                {
                    this.bodyFrameReader.FrameArrived -= this.BodyFrameReader_FrameArrived;
                    this.bodyFrameReader.Dispose();
                    this.bodyFrameReader = null;
                }
            }
        }
    }
}
