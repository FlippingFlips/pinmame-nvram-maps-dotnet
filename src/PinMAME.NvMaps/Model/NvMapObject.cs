using Newtonsoft.Json;

namespace PinMAME.NvMaps.Model
{
    /// <summary>
    /// NvMapObject for deserializing dictionaries ReadMe: https://github.com/tomlogic/pinmame-nvram-maps
    /// </summary>
    public class NvMapObject
    {
        public string label { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string short_label { get; set; }
        /// <summary>
        /// start: Offset into the .nv file of the first byte to interpret. <para/> 
        /// Default behavior is to use that single byte unless the end or length keys are present. Either start or offsets are required.
        /// </summary>
        public object start { get; set; }
        public int length { get; set; }
        public string encoding { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// min : Used for adjustments to specify the valid range of values.
        /// </summary>
        public int? min { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// max : Used for adjustments to specify the valid range of values.
        /// </summary>
        public int? max { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? @default { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? multiple_of { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string suffix { get; set; }

        public bool packed { get; set; } = true;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// A mask to apply to each byte before processing. <para/> 
        /// For example, a mask of "0x5F" converts lowercase initials to uppercase and a mask of "0x0F" clears the upper four bits.
        /// </summary>
        public string mask { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] values { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// A numeric multiplier for a decoded int, bcd, or bits.
        /// </summary>
        public int? scale { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? offset { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Used to indicate that a field contains a time value as either a number of "seconds" or "minutes", and should be displayed as HH:MM:SS.
        /// </summary>
        public string units { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<int, string> special_values { get; set; }

        #region JSON EXPORT OPTIONS
        public bool ShouldSerializelength() => length > 1;
        public bool ShouldSerializepacked() => !packed;
        public bool ShouldSerializesuffix() => suffix?.Length > 0;
        public bool ShouldSerializeoffset() => offset.HasValue && (offset.Value > 0 || offset.Value < 0);
        public bool ShouldSerializevalues() => values?.Length > 0;
        #endregion
    }
}
