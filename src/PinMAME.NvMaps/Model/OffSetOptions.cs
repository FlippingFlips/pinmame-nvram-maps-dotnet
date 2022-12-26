using Newtonsoft.Json;

namespace PinMAME.NvMaps.Model
{
    public abstract class OffSetOption
    {        
        public int length { get; set; } = 4;

        /// <summary>
        /// Can be string or integer
        /// </summary>
        public object start { get; set; }        
    }

    public abstract class PackedOffSetOption : OffSetOption
    {
        [JsonProperty("packed")]
        public bool packed { get; set; } = true;

        /// <summary>
        /// Conditional Property Serialization. https://www.newtonsoft.com/json/help/html/conditionalproperties.htm
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializepacked()
        {
            //defaults to true, only serialize packed = false
            return (packed == false);
        }            
    }
}
