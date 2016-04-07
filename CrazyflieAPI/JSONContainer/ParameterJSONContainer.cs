using System.Runtime.Serialization;

namespace CrazyflieAPI.JSONContainer
{
    /// <summary>
    /// Container for parameter interface packages
    /// </summary>
    [DataContract]
    public class ParameterJSONContainer : JSONContainerBase
    {
        /// <summary>
        /// Parameter command
        /// Default (defined in API specification): 1
        /// Currently no other command is supported
        /// </summary>
        [DataMember]
        public string cmd = "set";
        /// <summary>
        /// Name of the parameter
        /// </summary>
        [DataMember]
        public string name = "";
        /// <summary>
        /// New value for the parameter
        /// </summary>
        [DataMember]
        public string value = "";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ParameterJSONContainer()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">New value for the parameter</param>
        public ParameterJSONContainer(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
