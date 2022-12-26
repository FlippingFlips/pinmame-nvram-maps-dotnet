using PinMAME.NvMaps;
using System.Reflection.PortableExecutable;
using System.Text;

namespace PinMAMENvMapsTests
{
    public class NvScoreParsingTests
    {
        [Theory]
        [InlineData("f14_l1", "f14_l1", 4000000, 3800000, 3600000, 3400000, null, null)]
        [InlineData("afm_113", "afm_113b", 7904247710, 7500000000, 7000000000, 6500000000, 6000000000, null)]
        [InlineData("tomy_400", "tomy_400", 1000000000, 900000000, 800000000, 700000000, 600000000, 500000000)]
        [InlineData("robo_a34", "robo_a34", 4000000, 3500000, 3000000, 2500000, null, null)]
        [InlineData("sorcr_l1", "sorcr_l1", 2500000, 2000000, 1500000, 1100000, null, null)]
        [InlineData("hd_l3", "che_cho", 15000000, 12000000, 11000000, 10000000, 9000000, null)]
        [InlineData("tf_180", "tf_180", 75000000, 55000000, 40000000, 30000000, 25000000, null)]
        public void HighScoreParse_Tests(string romName, string nvName, long s1, long? s2, long? s3, long? s4, long? s5, long? s6)
        {
            var nvParser = CreateNvRamParser(romName, nvName);
            Assert.NotNull(nvParser?.NvRamMap.high_scores);

            //parse scores and test expected values
            var scores = nvParser?.GetHighScores(true);
            for (int i = 0; i < scores?.Count(); i++)
            {
                var score = scores.ElementAt(i).Score;
                switch (i)
                {
                    case 0:
                        Assert.True(score == s1);
                        break;
                    case 1:
                        if (!s2.HasValue) break;
                        Assert.True(score == s2);
                        break;
                    case 2:
                        if (!s3.HasValue) break;
                        Assert.True(score == s3);
                        break;
                    case 3:
                        if (!s4.HasValue) break;
                        Assert.True(score == s4);
                        break;
                    case 4:
                        if (!s5.HasValue) break;
                        Assert.True(score == s5);
                        break;
                    case 5:
                        if (!s6.HasValue) break;
                        Assert.True(score == s6);
                        break;
                    default:
                        break;
                }
            }
        }

        [Theory]
        [InlineData("f14_l1", "f14_l1", 21950, 270170, 109250, 6830)]
        [InlineData("afm_113", "afm_113b", 14280010, 0, 0, 0)]
        [InlineData("tomy_400", "tomy_400", 51563560, 3671330, 90510050, 46782010)]
        [InlineData("robo_a34", "robo_a34", 36300)]
        [InlineData("hd_l3", "che_cho", 2645520, 2208440, 2136180, 481220)]
        [InlineData("tf_180", "tf_180", 2376870, 317490, 2680740, 311200)]
        public void LastGameScoreParse_Tests(string romName, string nvName, long s1, long? s2 = 0, long? s3 = 0, long? s4 = 0)
        {
            var nvParser = CreateNvRamParser(romName, nvName);
            Assert.NotNull(nvParser?.NvRamMap.last_game);

            //parse scores and test expected values
            var scores = nvParser?.GetLastScores();
            for (int i = 0; i < scores?.Count(); i++)
            {
                var score = scores.ElementAt(i).Score;
                switch (i)
                {
                    case 0:
                        Assert.True(score == s1);
                        break;
                    case 1:
                        if (!s2.HasValue) break;
                        Assert.True(score == s2);
                        break;
                    case 2:
                        if (!s3.HasValue) break;
                        Assert.True(score == s3);
                        break;
                    case 3:
                        if (!s4.HasValue) break;
                        Assert.True(score == s4);
                        break;
                    default:
                        break;
                }
            }
        }

        [Theory]
        [InlineData("robo_a34", "robo_a34")]
        [InlineData("tf_180", "tf_180")]
        public void ConvertToBase64Tests(string romName, string nvName)
        {
            var nvParser = CreateNvRamParser(romName, nvName);
            var base64 = Convert.ToBase64String(nvParser.nvRamBytes);

            var length = base64.Length;
            decimal kbLen = (decimal)length / 1024;
        }

        [Theory]
        [InlineData("hd_l3", "che_cho", "2645520", 5953)]
        [InlineData("tf_180", "tf_180", "75000000", 11744)]
        //[InlineData("grand_l4", "grand_l4", "", 0)]
        [InlineData("st_162h", "st_162h", "30000000", 11744, "int")]
        public void SearchForScoreTests(string romName, string nvName, string searchVal, int expectedOffset, string encode = "bcd")
        {
            var nvfile = $"data/{romName}/{nvName}.nv";
            //get hex string to search on
            var hex = ParseNVRAM.NvRamFileToHexString(nvfile);
            var results = ParseNVRAM.SearchNvRamHexString(hex, searchVal, encode:encode);
            bool containsExpected = results.Any(x => x.Offset == expectedOffset);
            Assert.True(containsExpected);
        } 

        [Theory]
        [InlineData("ripleys", "PML", 3)]
        [InlineData("ripleys", "LNK", 1)]
        [InlineData("ripleys", "JRK", 1)]
        [InlineData("ripleys", "J Y", 1)]
        [InlineData("ripleys", "C G", 1)]        
        [InlineData("tf_180", "PWL", 1)]
        //[InlineData("st_162h", "T", 1)] //fail
        public void SearchForInitialsTests(string rom, string searchVal, int expectedResults)
        {
            var nvfile = $"data/{rom}/{rom}.nv";
            var bytes = ParseNVRAM.NvRamFileToBytes(nvfile);
            var results = ParseNVRAM.SearchForStringValue(bytes, searchVal);
            Assert.True(results.Length == expectedResults);

            var init = ParseNVRAM.GetStringValue(bytes, results.ElementAt(0).Offset, results.ElementAt(0).Length);
            Assert.True(searchVal == init);
        }

        [Theory]
        [InlineData("grand_l4", "BSO", 1)]
        public void SearchForMaskedInitialsTests(string rom, string searchVal, int expectedResults)
        {
            var nvfile = $"data/{rom}/{rom}.nv";
            var bytes = ParseNVRAM.NvRamFileToBytes(nvfile);
            var results = ParseNVRAM.SearchForStringValueMasked(bytes, searchVal, 0x7F);
            Assert.True(results.Length == expectedResults);

            var init = ParseNVRAM.GetStringValue(bytes, results.ElementAt(0).Offset, results.ElementAt(0).Length, 127);
            Assert.True(searchVal == init);
        }

        [Theory]
        [InlineData("whirl_l3", "HHR", 1)]
        [InlineData("whirl_l3", "JCY", 1)]
        [InlineData("whirl_l3", "JRK", 1)]
        [InlineData("whirl_l3", "CPG", 1)]
        [InlineData("whirl_l3", "PFZ", 1)]
        public void SearchForCharMappedInitialsTests(string rom, string searchVal, int expectedResults)
        {
            char[] charMap = "???????????ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var nvfile = $"data/{rom}/{rom}.nv";
            var bytes = ParseNVRAM.NvRamFileToBytes(nvfile);
            var results = ParseNVRAM.SearchForStringValueCharMap(bytes, searchVal, charMap);
            Assert.True(results.Length == expectedResults);

            var init = ParseNVRAM.GetStringValue(bytes, results.ElementAt(0).Offset, results.ElementAt(0).Length, null, charMap);
            Assert.True(searchVal == init);
        }

        [Theory]
        [InlineData("jm_12r", "jm_12r")]
        public void ParseAdjustments(string romName, string nvName)
        {
            var nvParser = CreateNvRamParser(romName, nvName);
            Assert.NotNull(nvParser?.NvRamMap.adjustments);

            var results = nvParser?.ExportAdjustments();
            Assert.NotNull(results);

            var results2 = nvParser?.ExportAudits();
            Assert.NotNull(results2);
            //21 Play Time : 0:05:10
            //22 Machine On: 0:18:00
        }

        [Theory]
        [InlineData("drac_l1", "drac_l1")]
        public void ParseAdjustmentsWithDefaults(string romName, string nvName)
        {
            var nvParser = CreateNvRamParser(romName, nvName);
            Assert.NotNull(nvParser?.NvRamMap.adjustments);

            var results = nvParser?.ExportAdjustmentsAndDefaults();
        }

        [Theory]
        [InlineData("drac_l1", "drac_l1")]
        [InlineData("grand_l4", "grand_l4")]
        public void GetAdjustmentValue(string romName, string nvName)
        {
            var nvParser = CreateNvRamParser(romName, nvName);
            Assert.NotNull(nvParser?.NvRamMap.adjustments);

            var gameAdjustments = nvParser?.GetStandardAdjustments();
            Assert.NotNull(gameAdjustments);

            Assert.True(gameAdjustments?.BallsPerGame == 3);
        }

        private ParseNVRAM CreateNvRamParser(string romName, string nvName)
        {
            var mapfile = $"data/{romName}/{romName}";
            var nvfile = $"data/{romName}/{nvName}";

            return new ParseNVRAM($"{mapfile}.nv.json", $"{nvfile}.nv");
        }
    }
}