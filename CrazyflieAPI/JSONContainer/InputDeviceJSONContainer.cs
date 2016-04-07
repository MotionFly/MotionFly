using System.Runtime.Serialization;

namespace CrazyflieAPI.JSONContainer
{
    /// <summary>
    /// Container for input device interface packages
    /// </summary>
    [DataContract]
    public class InputDeviceJSONContainer : JSONContainerBase
    {
        /// <summary>
        /// Container for flight control inputs
        /// </summary>
        [DataContract]
        public class ControlJSONContainer
        {
            /// <summary>
            /// Value for targeted roll in degrees
            /// </summary>
            [DataMember]
            public double roll = 0.0;
            /// <summary>
            /// Value for targeted pitch in degrees
            /// </summary>
            [DataMember]
            public double pitch = 0.0;
            /// <summary>
            /// Value for targeted yaw in degrees
            /// </summary>
            [DataMember]
            public double yaw = 0.0;
            /// <summary>
            /// Value for targeted thrust in percent
            /// Range: 0.0-100.0
            /// </summary>
            [DataMember]
            public double thrust = 0.0;
            /// <summary>
            /// If true, stops the drone and disables the control
            /// </summary>
            [DataMember]
            public bool estop = false;
            /// <summary>
            /// Internal function
            /// Switch LED ring effect
            /// </summary>
            [DataMember]
            public bool alt1 = false;
            /// <summary>
            /// Internal function
            /// Switch LED ring headlight on/off
            /// </summary>
            [DataMember]
            public bool alt2 = false;
        }

        /// <summary>
        /// Name of the client
        /// Not yet used
        /// </summary>
        [DataMember]
        public string client_name = "ZMQ Client";
        /// <summary>
        /// Flight control inputs
        /// </summary>
        [DataMember]
        public ControlJSONContainer ctrl = new ControlJSONContainer();

        /// <summary>
        /// Serializes the data of a container class into JSON data
        /// Note: This method verifies the data in the ctrl member.
        /// </summary>
        /// <returns>Serialized JSON data</returns>
        public new string Serialize()
        {
            if (ctrl.thrust < 0.0)
            {
                ctrl.thrust = 0.0;
            }
            else if (ctrl.thrust > 100.0)
            {
                ctrl.thrust = 100.0;
            }

            return base.Serialize();
        }
    }
}
