using System.Runtime.Serialization;

namespace CrazyflieAPI.JSONContainer
{
    /// <summary>
    /// Container for LED ring interface packages
    /// </summary>
    [DataContract]
    public class LEDRingJSONContainer : JSONContainerBase
    {
        /// <summary>
        /// Colors of the LED ring per LED
        /// </summary>
        public int[,] rgbring = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        /// <summary>
        /// Member needed for serialization
        /// </summary>
        [DataMember]
        private int[][] rgbleds;

        /// <summary>
        /// Serializes the data of a container class into JSON data
        /// Note: This method abstracts the publicly accessable member before calling
        /// the serialize method of the base class because the JSON serialzier can't 
        /// handle multidimensional arrays.
        /// </summary>
        /// <returns>Serialized JSON data</returns>
        public new string Serialize()
        {
            var retVal = "";

            rgbleds = new int[12][];

            for (var i = 0; i < 12; i++)
            {
                rgbleds[i] = new int[3];

                for (var j = 0; j < 3; j++)
                {
                    rgbleds[i][j] = rgbring[i, j];
                }
            }

            retVal = base.Serialize();

            rgbleds = null;

            return retVal;
        }
    }
}
