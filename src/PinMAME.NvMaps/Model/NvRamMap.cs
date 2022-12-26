using Newtonsoft.Json;

namespace PinMAME.NvMaps.Model
{
    public class NvRamMap
    {
        /// <summary>
        /// Can be string or string[]
        /// </summary>
        public object _notes { get; set; }
        public string _copyright { get; set; }
        public string _license { get; set; } = "GNU Lesser General Public License v3.0";
        public string _endian { get; set; } = "big";
        public long _ramsize { get; set; }
        public List<string> _roms { get; set; }
        public float _fileformat { get; set; } = 0.1f;
        public float _version { get; set; } = 0.1f;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string _char_map { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<LastGame> last_game { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<HighScore> high_scores { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<HighScore> mode_champions { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Dictionary<string,NvMapObject>> adjustments { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Dictionary<string, NvMapObject>> audits { get; set; }
    }
}
