using Newtonsoft.Json;

namespace PinMAME.NvMaps.Model
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class HighScore
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string label { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string short_label { get; set; }

        [JsonProperty("initials")]
        public Initials Initials { get; set; } = new Initials();
        public Score score { get; set; } = new Score();

        public bool ShouldSerializeInitials()
        {
            int.TryParse(Initials.start.ToString(), out var s);
            return (s > 0); 
        }
    }
}
