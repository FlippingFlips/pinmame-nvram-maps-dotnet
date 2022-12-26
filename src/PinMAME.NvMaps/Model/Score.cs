using Newtonsoft.Json;

namespace PinMAME.NvMaps.Model
{
    public class Score : PackedOffSetOption
    {
        public string encoding { get; set; } = "bcd";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Can be string or integer
        /// </summary>
        public object[] offsets { get; set; }
    }
}
