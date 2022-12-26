using Newtonsoft.Json;
using PinMAME.NvMaps.Model;
using System.Text;
using System.Threading.Tasks;
using WPFPrismTemplate.Base;

namespace PinMAME.NvMaps
{
    public partial class ParseNVRAM
    {
        public Endian byteOrder;
        public byte[] nvRamBytes;
        public ParseNVRAM(string nvJsonMapFile, string nvRamFile)
        {
            NvRamMap = DeserializeNvJsonMappingFromFile(nvJsonMapFile);
            nvRamBytes = NvRamFileToBytes(nvRamFile);
            SetByteOrder();
        }

        public ParseNVRAM(NvRamMap nvMapping, byte[] nvRamBytes)
        {
            this.NvRamMap = nvMapping;
            this.nvRamBytes = nvRamBytes;
            SetByteOrder();
        }

        public NvRamMap NvRamMap { get; private set; }

        /// <summary>
        /// Creates a <see cref="NvRamMap"/> class from a NvMappingJson file <para/>
        /// https://github.com/tomlogic/pinmame-nvram-maps
        /// </summary>
        /// <param name="nvJsonMapFile"></param>
        public static NvRamMap DeserializeNvJsonMappingFromFile(string nvJsonMapFile)
        {
            var json = File.ReadAllText(nvJsonMapFile);
            return DeserializeNvJsonMapping(json);
        }

        /// <summary>
        /// Converts json mapping to <see cref="NvRamMap"/>
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static NvRamMap DeserializeNvJsonMapping(string json) => JsonConvert.DeserializeObject<NvRamMap>(json);

        /// <summary>
        /// Converts the high_scores section to scores
        /// </summary>
        /// <param name="includeInitials"></param>
        /// <returns></returns>
        public IEnumerable<ScoreResult> GetHighScores(bool includeInitials = true)
        {
            if (NvRamMap?.high_scores == null) return Enumerable.Empty<ScoreResult>();
            var scores = new List<ScoreResult>();
            foreach (var item in NvRamMap?.high_scores)
            {
                var score = new ScoreResult();

                if(item.score.offsets?.Length > 0)
                {
                    byte[] bytes = new byte[item.score.offsets.Length];
                    int i = 0;
                    foreach (var offsetItem in item.score.offsets)
                    {
                        int offset = GetMemoryOffset(offsetItem.ToString());
                        bytes[i] = nvRamBytes[offset];
                        i++;
                    }

                    score.Score = GetValue(bytes, item.score.encoding, item.score.packed, endian: byteOrder);
                }
                else
                {
                    //remove the 0x and use NumberStyles.HexNumber
                    int offset = GetMemoryOffset(item.score.start.ToString());
                    score.Score = GetValue(nvRamBytes.Skip(offset).Take(item.score.length).ToArray(),
                        item.score.encoding, item.score.packed, endian: byteOrder);
                }

                //set labels
                score.Label = item.label;
                score.LabelShort = item.short_label;

                //Get players name
                if (includeInitials && item.Initials?.start !=null)
                {
                    int offset = GetMemoryOffset(item.Initials.start.ToString());
                    
                    if(NvRamMap._char_map?.Length > 0)
                        score.Name = GetStringValue(nvRamBytes, offset, item.Initials.length, charMap: NvRamMap._char_map.ToCharArray());
                    else if(item.Initials.mask.HasValue)
                        score.Name = GetStringValue(nvRamBytes, offset, item.Initials.length, mask:item.Initials.mask);
                    else
                        score.Name = GetStringValue(nvRamBytes, offset, item.Initials.length);
                }

                //add score to list
                scores.Add(score);
            }

            return scores;
        }

        /// <summary>
        /// Converts the last_game section
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScoreResult> GetLastScores()
        {
            if (NvRamMap?.last_game == null) return Enumerable.Empty<ScoreResult>();
            var scores = new List<ScoreResult>();
            foreach (var item in NvRamMap.last_game)
            {
                var score = new ScoreResult();
                //remove the 0x and use NumberStyles.HexNumber
                int offset = GetMemoryOffset(item.start.ToString());
                //set score value and labels
                score.Score = GetValue(nvRamBytes.Skip(offset).Take(item.length).ToArray(), item.encoding, item.packed, endian: byteOrder);
                //add score to list
                scores.Add(score);
            }

            return scores;
        }

        /// <summary>
        /// Builds a model for BPG, TW, MEB, BS BallsPerGame, TiltWarns, Max Extra Balls, Ball Save
        /// </summary>
        /// <returns></returns>
        public GameAdjustment GetStandardAdjustments()
        {
            var ga = new GameAdjustment();

            if(NvRamMap.adjustments?.Count > 0)
            {
                //first menu, should always have what we need
                var menu = NvRamMap.adjustments.FirstOrDefault();
                var items = menu.Value.Values;

                var ballsPerGame = items.FirstOrDefault(x => x.short_label == "BPG");
                if (ballsPerGame != null)
                    ga.BallsPerGame = (int)GetValue(nvRamBytes, ballsPerGame, byteOrder);

                var tiltwarns = items.FirstOrDefault(x => x.short_label == "TW");
                if (tiltwarns != null)
                    ga.TiltWarnings = (int)GetValue(nvRamBytes, ballsPerGame, byteOrder);

                var maxExBalls = items.FirstOrDefault(x => x.short_label == "MEB");
                if (maxExBalls != null)
                    ga.MaxExtraBall = (int)GetValue(nvRamBytes, ballsPerGame, byteOrder);

                var ballSaveTime = items.FirstOrDefault(x => x.short_label == "BS");
                if (maxExBalls != null)
                    ga.BallSaveTime = (int)GetValue(nvRamBytes, ballsPerGame, byteOrder);
            }

            return ga;
        }

        public static long GetValue(byte[] bytes, NvMapObject mapItem, Endian endian = Endian.BIG)
        {
            var start = GetMemoryOffset(mapItem.start?.ToString());
            var end = start + mapItem.length;

            //get bytes from start end, length can also be a single byte
            var bytesToSearch = start != end ? bytes[start..end] : new byte[1] { bytes[start] };

            return GetValue(bytesToSearch, mapItem.encoding, packed:mapItem.packed, scale: mapItem.scale, 
                offset: mapItem.offset, endian: endian);
        }

        /// <summary>
        /// Converts a string into start / offset position integer. Checks if the incoming string is appended with 0x like many starting positions are.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int GetMemoryOffset(string start)
        {
            int offset = 0;
            if (start == null) return offset;
            if (start.StartsWith("0x"))
                int.TryParse(start.Remove(0, 2), NumberStyles.HexNumber, null, out offset);
            else
                int.TryParse(start, out offset);
            return offset;
        }

        public static string GetStringValue(byte[] bytes, int offset, int length, int? mask = null, char[] charMap = null)
        {
            string result = string.Empty;
            for (int i = 0; i < length; i++)
            {
                char c = '\0';
                if(mask ==null && charMap == null) //standard char
                    c = (char)bytes[offset + i];
                else if(mask !=null)
                    c = (char)(bytes[offset + i] & mask.Value);
                else if(charMap !=null)
                    c = charMap[bytes[offset+i]];                

                if (c>0 && c<255)
                    result += c;
            }            
            return result;
        }

        /// <summary>
        /// Search bytes for a string value for finding players initials
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public static SearchedNvResult[] SearchForStringValue(byte[] bytes, string expected)
        {
            if (string.IsNullOrWhiteSpace(expected)) return null;
            var foundIndexes = new List<SearchedNvResult>();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (expected[0] == (char)bytes[i])
                {
                    //when finding a single char
                    if (i == expected.Length - 1)
                    {
                        //add found result
                        foundIndexes.Add(new SearchedNvResult(i, expected.Length, expected));
                        i += expected.Length;
                        continue;
                    }

                    //found first char now look into the next chars, break back out into the main loop if not found
                    for (int ii = 1; ii < expected.Length; ii++)
                    {
                        //when finding a two chars
                        if (ii == expected.Length - 1)
                        {
                            //add found result
                            foundIndexes.Add(new SearchedNvResult(i, expected.Length, expected));
                            i += expected.Length;
                            continue;
                        }

                        //next char is a match ?
                        if (expected[ii] == (char)bytes[i + ii])
                        {
                            if (ii == expected.Length - 1)
                            {
                                //add found result
                                foundIndexes.Add(new SearchedNvResult(i, expected.Length, expected));
                                //move index forward
                                i += expected.Length;
                            }
                            continue;
                        }
                        else
                        {
                            break; //exit this loop
                        }
                    }
                }
            }
            return foundIndexes.ToArray();
        }

        public static SearchedNvResult[] SearchForStringValueMasked(byte[] bytes, string expected, int mask = 127)
        {
            if (string.IsNullOrWhiteSpace(expected)) return null;
            var foundIndexes = new List<SearchedNvResult>();

            for (int i = 0; i < bytes.Length; i++)
            {
                var c = bytes[i] & mask;
                if (expected[0] == (char)c)
                {
                    //single length search?
                    if(i == expected.Length - 1)
                    {
                        foundIndexes.Add(new SearchedNvResult(i, expected.Length, expected));
                        i += expected.Length;
                        continue; //move on, search again
                    }

                    //matched the first try the next
                    //found first char now look into the next chars, break back out into the main loop if not found
                    for (int ii = 1; ii < expected.Length; ii++)
                    {
                        //when finding a two chars
                        if (ii == expected.Length - 1)
                        {
                            //add found result
                            foundIndexes.Add(new SearchedNvResult(i, expected.Length, expected));
                            i += expected.Length;
                            continue;
                        }

                        //next char is a match ?
                        if (expected[ii] == (bytes[i + ii] & mask))
                        {
                            if (ii == expected.Length - 1)
                            {
                                //add found result
                                foundIndexes.Add(new SearchedNvResult(i, expected.Length, expected));
                                //move index forward
                                i += expected.Length;
                            }
                            continue;
                        }
                        else
                        {
                            break; //exit this loop
                        }
                    }
                }
            }

            return foundIndexes.ToArray();
        }

        public static SearchedNvResult[] SearchForStringValueCharMap(byte[] bytes, string expected, char[] charMap)
        {
            if (string.IsNullOrWhiteSpace(expected)) return null;
            var foundIndexes = new List<SearchedNvResult>();

            for (int i = 0; i < bytes.Length; i++)
            {
                var c = bytes[i];
                if(c < charMap.Length)
                {
                    if (expected[0] == charMap[c])
                    {
                        //single length search?
                        if (i == expected.Length - 1)
                        {
                            foundIndexes.Add(new SearchedNvResult(i, expected.Length, expected));
                            i += expected.Length;
                            continue; //move on, search again
                        }

                        //matched the first try the next
                        //found first char now look into the next chars, break back out into the main loop if not found
                        for (int ii = 1; ii < expected.Length; ii++)
                        {
                            //when finding a two chars
                            if (ii == expected.Length - 1)
                            {
                                //add found result
                                foundIndexes.Add(new SearchedNvResult(i, expected.Length, expected));
                                i += expected.Length;
                                continue;
                            }

                            //next char is a match ?
                            if(bytes[i + ii] < charMap.Length)
                            {
                                if (expected[ii] == charMap[bytes[i + ii]])
                                {
                                    if (ii == expected.Length - 1)
                                    {
                                        //add found result
                                        foundIndexes.Add(new SearchedNvResult(i, expected.Length, expected));
                                        //move index forward
                                        i += expected.Length;
                                    }
                                    continue;
                                }
                                else
                                {
                                    break; //exit this loop
                                }
                            }
                            else { break; }

                        }
                    }
                }                
            }

            return foundIndexes.ToArray();
        }

        /// <summary>
        /// https://github.com/tomlogic/py-pinmame-nvmaps/blob/master/nvram_parser.py
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static long GetValue(byte[] bytes, string encoding, bool packed = true, int? scale = null, int? offset = null, Endian endian = Endian.BIG)
        {
            long value = -1;
            if (endian == Endian.LITTLE)
                bytes = bytes.Reverse().ToArray();
            switch (encoding)
            {
                case "bcd":
                    value = 0;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        var b = (int)bytes[i];
                        if (packed)
                            value = value * 100 + (b >> 4) * 10 + (b & 0x0F);
                        else
                            value = value * 10 + (b & 0X0F);
                    }
                    break;
                case "int":
                case "bits":
                    value = 0;
                    foreach (var b in bytes) { value = (value << 8) + (b & 0xFF); }
                    break;
                case "enum":
                    value = (int)bytes[0];
                    break;
                default:
                    break;
            }

            if(value > 0 && scale.HasValue)
            {
                if(scale > 1)
                    value *= scale.Value;
            }

            if (value > 0 && offset.HasValue)
            {                
                value += offset.Value;
            }

            return value;
        }

        public static string SerializeWithCustomIndenting(object obj, int? maxIdentDepth = null)
        {
            using (StringWriter sw = new StringWriter())
            using (CustomIndentingJsonTextWriter jw = new CustomIndentingJsonTextWriter(sw))
            {
                jw.MaxIndentDepth = maxIdentDepth;
                Newtonsoft.Json.JsonSerializer ser = new();
                ser.Serialize(jw, obj);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Converts .Nv to bytes
        /// </summary>
        /// <param name="nvRamFile"></param>
        /// <returns></returns>
        public static byte[] NvRamFileToBytes(string nvRamFile) => File.ReadAllBytes(nvRamFile);

        /// <summary>
        /// Converts NvRam to hex string
        /// </summary>
        /// <param name="nvRamFile"></param>
        /// <returns></returns>
        public static string NvRamFileToHexString(string nvRamFile)
        {
            using var fs = new FileStream(nvRamFile, FileMode.Open);
            int hexInt;
            var sb = new StringBuilder();
            for (int i = 0; (hexInt = fs.ReadByte()) != -1; i++)
            {
                sb.Append(string.Format("{0:X2}", hexInt));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Searches for the given value. Tries simple string search first the int search
        /// </summary>
        /// <param name="nvHex"></param>
        /// <param name="searchVal"></param>
        /// <returns></returns>
        public static SearchedNvResult[] SearchNvRamHexString(string nvHex, string searchVal, int length = 4, string encode = "bcd")
        {
            var indexes = new List<SearchedNvResult>();
            var index = 0;

            //standard string search
            if (encode == "bcd")
            {
                while (true)
                {
                    index = nvHex.IndexOf(searchVal, index + length);
                    if (index < 0)
                        break;
                    else
                    {
                        indexes.Add(
                            new SearchedNvResult(index / 2, length, searchVal));
                    }
                }
            }

            //none found try doing int search
            if (indexes?.Count <= 0)
            {
                int.TryParse(searchVal, out var intVal);
                if (intVal > 0)
                {
                    var bytes = BitConverter.GetBytes(intVal);
                    string strVal = string.Empty;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        strVal += string.Format("{0:X2}", bytes[i]);
                    }
                    
                    while (true)
                    {
                        index = nvHex.IndexOf(strVal, index + length);
                        if (index < 0)
                            break;
                        else
                        {
                            indexes.Add(
                                new SearchedNvResult(index / 2, length, strVal));
                        }
                    }
                }
            }
            return indexes.ToArray();
        }

        /// <summary>
        /// Sets the byte order and reverses the array if needed
        /// </summary>
        private void SetByteOrder()
        {
            byteOrder = NvRamMap._endian == "little" ? Endian.LITTLE : Endian.BIG;
        }

        public string ExportAdjustments()
        {
            string results = string.Empty;
            if (NvRamMap.adjustments == null) return string.Empty;
            try
            {
                foreach (var item in NvRamMap.adjustments)
                {
                    results += item.Key + "\n";

                    foreach (var item2 in item.Value)
                    {
                        var key = item2.Key;
                        var value = item2.Value;
                        var printVal = FormatValue(item2.Value, nvRamBytes, scale: item2.Value.scale);
                        results += $"{key} {value.label}: {printVal}\n";
                    }

                    results += "\n";
                }
            }
            catch (Exception ex)
            {
                results += $"\n Error occurred: {ex.Message}";
                return results;
            }

            return results;
        }

        /// <summary>
        /// Exports menu items to list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RomMenuItem> ExportAdjustmentsAndDefaults(string menuType = "adjustments")
        {
            var adjItems = new List<RomMenuItem>();

            if(menuType == "adjustments")
            {
                foreach (var item in NvRamMap.adjustments)
                {
                    foreach (var item2 in item.Value)
                    {
                        var printVal = FormatValue(item2.Value, nvRamBytes, scale: item2.Value.scale);
                        var adjItem = new RomMenuItem(item.Key, item2.Key, item2.Value.label, printVal, item2.Value.@default?.ToString());
                        adjItems.Add(adjItem);
                    }
                }
            }
            else
            {
                foreach (var item in NvRamMap.audits)
                {
                    foreach (var item2 in item.Value)
                    {
                        var printVal = FormatValue(item2.Value, nvRamBytes, scale: item2.Value.scale);
                        var adjItem = new RomMenuItem(item.Key, item2.Key, item2.Value.label, printVal, item2.Value.@default?.ToString());
                        adjItems.Add(adjItem);
                    }
                }
            }

            return adjItems;
        }

        public string ExportAudits()
        {
            string results = string.Empty;
            try
            {
                foreach (var item in NvRamMap.audits)
                {
                    results += item.Key + "\n";

                    foreach (var item2 in item.Value)
                    {
                        var key = item2.Key;
                        var value = item2.Value;
                        var printVal = FormatValue(item2.Value, nvRamBytes, scale: item2.Value.scale);
                        results += $"{key} {value.label}: {printVal}\n";
                    }

                    results += "\n";
                }
            }
            catch (Exception ex)
            {
                results += $"\n Error occurred: {ex.Message}";
                return results;
            }

            return results;
        }

        public string ExportScoreResults()
        {
            string results = string.Empty;

            try
            {
                var lastscores = GetLastScores();
                results += "Last Scores\n";
                foreach (var score in lastscores)
                {
                    results += $"{score.Score.ToString("N0")}\n";
                }
                results += "\n";
                results += "High Scores\n";
                var hiScores = GetHighScores();
                foreach (var score in hiScores)
                {
                    var label = score.Label == null ? "" : score.Label;
                    var name = score.Name == null ? "" : score.Name;

                    if (label != null)
                        results += $"{label}";
                    if (name != null)
                        results += $" {name}";

                    results += $" {score.Score.ToString("N0")}";

                    results += "\n";
                }
            }
            catch (Exception ex)
            {
                results += $"\n Error occurred: {ex.Message}";
                return results;
            }

            return results;
        }

        #region Public Static Methods
        public static string FormatValue(NvMapObject nvMapObject, byte[] nvBytes, bool packed = true, int? scale = null, Endian endian = Endian.BIG)
        {
            string printVal = string.Empty;
            long val = 0;
            if (nvMapObject.length > 0)
            {
                if (nvMapObject.encoding == "wpc_rtc")
                {
                    var start = GetMemoryOffset(nvMapObject.start.ToString());
                    var b = start + nvMapObject.length;
                    var bytes = nvBytes[start..b];
                    var year = bytes[0] * 256 + bytes[1];
                    var m = bytes[2];
                    var d = bytes[3];
                    var time = bytes[5].ToString("D2") + ":" + bytes[6].ToString("D2");
                    printVal = $"{year}-{m}-{d} {time}";
                }
                else if (nvMapObject.encoding == "ch")
                {
                    var start = GetMemoryOffset(nvMapObject.start.ToString());
                    printVal = GetStringValue(nvBytes, start, nvMapObject.length);
                }
                else if (nvMapObject.units != null)
                {
                    val = GetValue(nvBytes, nvMapObject, endian);
                    switch (nvMapObject.units)
                    {
                        case "seconds":
                            var m = Math.DivRem(val, 60, out var sec);
                            var t = Math.DivRem(m, 60, out var h);
                            printVal = $"{t}:{m.ToString("D2")}:{sec.ToString("D2")}";
                            break;
                        case "minutes":
                            Math.DivRem(val, 60, out var mins);
                            var ts = new TimeSpan(0, (int)mins, 0);
                            printVal = $"{ts.ToString("hh\\:mm\\:ss")}";
                            break;
                        default:
                            break;
                    }
                }
                else if (nvMapObject.encoding == "enum" && nvMapObject.values?.Length > 0)
                {
                    if (nvMapObject.values.Length >= val)
                    {
                        var enumVal = nvMapObject.values[val];
                        printVal = enumVal;
                    }
                }
                else
                {
                    val = GetValue(nvBytes, nvMapObject, endian);
                    printVal = val.ToString();
                }
            }
            else
            {                
                val = GetValue(nvBytes, nvMapObject, endian);
                if (nvMapObject.encoding == "int")
                {
                    printVal = val.ToString();
                }
                else if (nvMapObject.encoding == "enum" && nvMapObject.values?.Length > 0)
                {
                    if (nvMapObject.values.Length >= val)
                    {
                        var enumVal = nvMapObject.values[val];
                        printVal = enumVal;
                    }
                }
                else
                {

                }
            }

            if (nvMapObject.special_values?.ContainsKey((int)val) ?? false)
            {
                var specialVal = nvMapObject.special_values[(int)val];
                printVal = specialVal;
            }

            return printVal += !string.IsNullOrWhiteSpace(nvMapObject.suffix) ? nvMapObject.suffix : string.Empty;
        }
        #endregion
    }
}