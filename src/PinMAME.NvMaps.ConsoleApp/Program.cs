namespace PinMAME.NvMaps.ConsoleApp
{
    internal class Program
    {
        const string USAGE = "Usage: <json_file> <nvram_file>";
        static void Main(string[] args)
        {
            if (args.Length < 2) { Console.WriteLine(USAGE); return; }

            var mapFile = args[0];
            var nvFile = args[1];
            if (!File.Exists(mapFile))
                throw new FileNotFoundException($"Mapping json not found at: {mapFile}");
            if (!File.Exists(nvFile))
                throw new FileNotFoundException($"NVRam file not found at: {nvFile}");

            var parser = new ParseNVRAM(mapFile, nvFile);

            Console.WriteLine(parser.ExportScoreResults());
            Console.WriteLine(parser.ExportAdjustments());
            Console.WriteLine(parser.ExportAudits());
        }
    }
}