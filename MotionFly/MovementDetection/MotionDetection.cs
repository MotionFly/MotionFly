using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MotionFly.Kinect;
using Microsoft.Kinect;
using ConnectionHelper = CrazyflieAPI.ConnectionHelper;

namespace MotionFly.MovementDetection
{
    /// <summary>
    /// Class for motion detection
    /// </summary>
    public sealed class MotionDetection
    {
        /// <summary>
        /// Enumerator for tracking finite state machine
        /// </summary>
        public enum TrackingState
        {
            Idle,
            Searching,
            LearnLeftHandMiddle,
            LearnLeftHandTop,
            LearnLeftHandBottom,
            LearnLeftHandLeft,
            LearnLeftHandRight,
            LearnRightHandMiddle,
            LearnRightHandTop,
            LearnRightHandBottom,
            LearnRightHandLeft,
            LearnRightHandRight,
            Tracking
        }

        #region Singleton Implementation
        /// <summary>
        /// Instance variable for singleton
        /// </summary>
        private static MotionDetection instance;
        /// <summary>
        /// Lock variable for singleton
        /// </summary>
        private static object instanceLock = new object();
        /// <summary>
        /// Gets the Crazyflie API instance
        /// </summary>
        public static MotionDetection Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new MotionDetection();
                    }
                }

                return instance;
            }
        }
        #endregion

        /// <summary>
        /// Current state of tracking finite state machine
        /// </summary>
        public TrackingState TrackingStatus { get; private set; }
        /// <summary>
        /// Targeted thrust value
        /// </summary>
        public int ThrustValue { get; private set; } = 0;
        /// <summary>
        /// Targeted pitch value
        /// </summary>
        public int PitchValue { get; private set; } = 0;
        /// <summary>
        /// Targeted roll value
        /// </summary>
        public int RollValue { get; private set; } = 0;
        /// <summary>
        /// Targeted yaw value
        /// </summary>
        public int YawValue { get; private set; } = 0;

        private BasicSensor basicSensor = BasicSensor.Instance;
        private Body[] bodies;
        private bool allowNextStep = true;
        private ulong trackingId = 0;
        private CoordinateMapper coordinateMapper;
        private int colorFrameHeight = 0;
        private int colorFrameWidth = 0;
        private ColorSpacePoint leftHandMiddlePoint;
        private ColorSpacePoint leftHandTopPoint;
        private ColorSpacePoint leftHandBottomPoint;
        private ColorSpacePoint leftHandLeftPoint;
        private ColorSpacePoint leftHandRightPoint;
        private ColorSpacePoint rightHandMiddlePoint;
        private ColorSpacePoint rightHandTopPoint;
        private ColorSpacePoint rightHandBottomPoint;
        private ColorSpacePoint rightHandLeftPoint;
        private ColorSpacePoint rightHandRightPoint;
        private ConnectionHelper connectionHelper = ConnectionHelper.Instance;

        /// <summary>
        /// Gets raised when the targeted flight parameters have changed
        /// </summary>
        public event EventHandler<EventArgs> InputChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        private MotionDetection()
        {
            coordinateMapper = basicSensor.Sensor.CoordinateMapper;

            colorFrameHeight = basicSensor.ColorDescription.Height;
            colorFrameWidth = basicSensor.ColorDescription.Width;

            basicSensor.BodyFrameArrived += BasicSensor_BodyFrameArrived;
            basicSensor.IsAvailableChanged += BasicSensor_IsAvailableChanged;
        }

        /// <summary>
        /// Handler for Kinect.IsAvailableChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasicSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (!e.IsAvailable)
            {
                CancelTracking();
            }
        }

        /// <summary>
        /// Moves to the next step
        /// </summary>
        public void NextStep()
        {
            if (allowNextStep)
            {
                switch (TrackingStatus)
                {
                    case TrackingState.Idle:
                        TrackingStatus = TrackingState.Searching;
                        break;
                    case TrackingState.Searching:
                        TrackingStatus = TrackingState.LearnLeftHandMiddle;
                        break;
                    case TrackingState.LearnLeftHandMiddle:
                        TrackingStatus = TrackingState.LearnLeftHandTop;
                        break;
                    case TrackingState.LearnLeftHandTop:
                        TrackingStatus = TrackingState.LearnLeftHandBottom;
                        break;
                    case TrackingState.LearnLeftHandBottom:
                        TrackingStatus = TrackingState.LearnLeftHandLeft;
                        break;
                    case TrackingState.LearnLeftHandLeft:
                        TrackingStatus = TrackingState.LearnLeftHandRight;
                        break;
                    case TrackingState.LearnLeftHandRight:
                        TrackingStatus = TrackingState.LearnRightHandMiddle;
                        break;
                    case TrackingState.LearnRightHandMiddle:
                        TrackingStatus = TrackingState.LearnRightHandTop;
                        break;
                    case TrackingState.LearnRightHandTop:
                        TrackingStatus = TrackingState.LearnRightHandBottom;
                        break;
                    case TrackingState.LearnRightHandBottom:
                        TrackingStatus = TrackingState.LearnRightHandLeft;
                        break;
                    case TrackingState.LearnRightHandLeft:
                        TrackingStatus = TrackingState.LearnRightHandRight;
                        break;
                    case TrackingState.LearnRightHandRight:
                        TrackingStatus = TrackingState.Tracking;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Cancels the current tracking process
        /// </summary>
        public void CancelTracking()
        {
            TrackingStatus = TrackingState.Idle;
            allowNextStep = true;
            trackingId = 0;
        }

        /// <summary>
        /// Handler for BodyFrameReader.FrameArrived
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasicSensor_BodyFrameArrived(object sender, Microsoft.Kinect.BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (bodies == null)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                switch (TrackingStatus)
                {
                    case TrackingState.Idle:
                        break;
                    case TrackingState.Searching:
                        SearchOperator();
                        break;
                    case TrackingState.LearnLeftHandMiddle:
                    case TrackingState.LearnLeftHandTop:
                    case TrackingState.LearnLeftHandBottom:
                    case TrackingState.LearnLeftHandLeft:
                    case TrackingState.LearnLeftHandRight:
                    case TrackingState.LearnRightHandMiddle:
                    case TrackingState.LearnRightHandTop:
                    case TrackingState.LearnRightHandBottom:
                    case TrackingState.LearnRightHandLeft:
                    case TrackingState.LearnRightHandRight:
                        LearnPosition();
                        break;
                    case TrackingState.Tracking:
                        TrackOperator();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Searches for an operator
        /// </summary>
        private void SearchOperator()
        {
            var trackedBodyCount = 0;
            Body trackedBody = null;

            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    trackedBodyCount++;
                    trackedBody = body;
                }
            }

            if (trackedBodyCount == 1)
            {
                allowNextStep = true;

                trackingId = trackedBody.TrackingId;
            }
            else
            {
                allowNextStep = false;
            }
        }

        /// <summary>
        /// Learn the default positions of the operator
        /// </summary>
        private void LearnPosition()
        {
            var body = GetTrackedBody();

            switch (TrackingStatus)
            {
                case TrackingState.LearnLeftHandMiddle:
                    leftHandMiddlePoint = GetColorSpacePoint(body.Joints[JointType.HandLeft].Position);
                    break;
                case TrackingState.LearnLeftHandTop:
                    leftHandTopPoint = GetColorSpacePoint(body.Joints[JointType.HandLeft].Position);
                    break;
                case TrackingState.LearnLeftHandBottom:
                    leftHandBottomPoint = GetColorSpacePoint(body.Joints[JointType.HandLeft].Position);
                    break;
                case TrackingState.LearnLeftHandLeft:
                    leftHandLeftPoint = GetColorSpacePoint(body.Joints[JointType.HandLeft].Position);
                    break;
                case TrackingState.LearnLeftHandRight:
                    leftHandRightPoint = GetColorSpacePoint(body.Joints[JointType.HandLeft].Position);
                    break;
                case TrackingState.LearnRightHandMiddle:
                    rightHandMiddlePoint = GetColorSpacePoint(body.Joints[JointType.HandRight].Position);
                    break;
                case TrackingState.LearnRightHandTop:
                    rightHandTopPoint = GetColorSpacePoint(body.Joints[JointType.HandRight].Position);
                    break;
                case TrackingState.LearnRightHandBottom:
                    rightHandBottomPoint = GetColorSpacePoint(body.Joints[JointType.HandRight].Position);
                    break;
                case TrackingState.LearnRightHandLeft:
                    rightHandLeftPoint = GetColorSpacePoint(body.Joints[JointType.HandRight].Position);
                    break;
                case TrackingState.LearnRightHandRight:
                    rightHandRightPoint = GetColorSpacePoint(body.Joints[JointType.HandRight].Position);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Tracks the operator
        /// </summary>
        private void TrackOperator()
        {
            var joints = GetTrackedBody().Joints;
            var leftHandPoint = GetColorSpacePoint(joints[JointType.HandLeft].Position);
            var rightHandPoint = GetColorSpacePoint(joints[JointType.HandRight].Position);
            var value = 0;

            allowNextStep = false;

            // Thrust
            value =
                Convert.ToInt32(100.0 / (leftHandBottomPoint.Y - leftHandTopPoint.Y) *
                                (leftHandPoint.Y - leftHandTopPoint.Y));
            ThrustValue = value;
            connectionHelper.InputData.ctrl.thrust = value;

            // Yaw
            if (Math.Abs(leftHandMiddlePoint.X - leftHandPoint.X) < 0.5)
            {
                value = 0;
            }
            else if (leftHandMiddlePoint.X < leftHandPoint.X)
            {
                if (leftHandPoint.X > leftHandLeftPoint.X)
                {
                    value =
                        Convert.ToInt32(-200.0 / (leftHandLeftPoint.X - leftHandMiddlePoint.X) *
                                        (leftHandPoint.X - leftHandMiddlePoint.X));
                }
                else
                {
                    value = -200;
                }
            }
            else // leftHandMiddlePoint.X > leftHandPoint.X
            {
                if (leftHandPoint.X > leftHandRightPoint.X)
                {
                    value =
                        Convert.ToInt32(200.0 / (leftHandMiddlePoint.X - leftHandRightPoint.X) *
                                        (leftHandPoint.X - leftHandRightPoint.X));
                }
                else
                {
                    value = 200;
                }
            }
            YawValue = value;
            connectionHelper.InputData.ctrl.yaw = value;

            // Pitch
            if (Math.Abs(rightHandMiddlePoint.Y - rightHandPoint.Y) < 0.5)
            {
                value = 0;
            }
            else if (rightHandMiddlePoint.Y < rightHandPoint.Y)
            {
                if (rightHandPoint.Y < rightHandTopPoint.Y)
                {
                    value =
                        Convert.ToInt32(30.0 / (rightHandMiddlePoint.Y - rightHandTopPoint.Y) *
                                        (rightHandPoint.Y - rightHandTopPoint.Y));
                }
                else
                {
                    value = 30;
                }
            }
            else // rightHandMiddlePoint.Y > rightHandPoint.Y
            {
                if (rightHandPoint.Y < rightHandBottomPoint.Y)
                {
                    value =
                        Convert.ToInt32(-30.0 / (rightHandBottomPoint.Y - leftHandMiddlePoint.Y) *
                                        (leftHandPoint.Y - leftHandMiddlePoint.Y));
                }
                else
                {
                    value = -30;
                }
            }
            PitchValue = value;
            connectionHelper.InputData.ctrl.pitch = value;

            // Roll
            if (Math.Abs(rightHandMiddlePoint.X - rightHandPoint.X) < 0.5)
            {
                value = 0;
            }
            else if (rightHandMiddlePoint.X < rightHandPoint.X)
            {
                if (rightHandPoint.X > rightHandLeftPoint.X)
                {
                    value =
                        Convert.ToInt32(-30.0 / (rightHandLeftPoint.X - rightHandMiddlePoint.X) *
                                        (rightHandPoint.X - rightHandMiddlePoint.X));
                }
                else
                {
                    value = -30;
                }
            }
            else // rightHandMiddlePoint.X > rightHandPoint.X
            {
                if (rightHandPoint.X > rightHandRightPoint.X)
                {
                    value =
                        Convert.ToInt32(30.0 / (rightHandMiddlePoint.X - rightHandRightPoint.X) *
                                        (rightHandPoint.X - rightHandRightPoint.X));
                }
                else
                {
                    value = 30;
                }
            }

            OnInputChanged();
        }

        /// <summary>
        /// Gets the currently tracked body
        /// </summary>
        /// <returns>Tracked body</returns>
        private Body GetTrackedBody()
        {
            Body body = null;

            foreach (var b in bodies)
            {
                if (b.IsTracked && b.TrackingId == trackingId)
                {
                    body = b;
                    break;
                }
            }

            return body;
        }

        /// <summary>
        /// Converts the camera space point to a colour space point
        /// </summary>
        /// <param name="cameraSpacePoint">Camera space point</param>
        /// <returns>Colour space point</returns>
        private ColorSpacePoint GetColorSpacePoint(CameraSpacePoint cameraSpacePoint)
        {
            return coordinateMapper.MapCameraPointToColorSpace(cameraSpacePoint);
        }

        /// <summary>
        /// Raises the InputChanged event
        /// </summary>
        private void OnInputChanged()
        {
            InputChanged?.Invoke(this, new EventArgs());
        }
    }
}
