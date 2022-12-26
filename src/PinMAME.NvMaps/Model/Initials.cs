using Newtonsoft.Json;

namespace PinMAME.NvMaps.Model
{
    public class Initials : OffSetOption
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? mask;

        public string encoding { get; set; } = "ch";
    }
}
