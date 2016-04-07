using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace CrazyflieAPI.JSONContainer
{
    /// <summary>
    /// Abstract base class for JSON container classes
    /// Implements the serialize method
    /// </summary>
    [DataContract]
    public abstract class JSONContainerBase
    {
        /// <summary>
        /// Describes the API version
        /// Default (defined in API specification): 1
        /// </summary>
        [DataMember]
        public int version = 1;

        /// <summary>
        /// Serializes the data of a container class into JSON data
        /// </summary>
        /// <returns>Serialized JSON data</returns>
        public string Serialize()
        {
            var retVal = "";

            using (var stream = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(this.GetType());
                var sr = new StreamReader(stream);

                ser.WriteObject(stream, this);

                stream.Position = 0;

                retVal = sr.ReadToEnd();
            }

            return retVal;
        }
    }
}
