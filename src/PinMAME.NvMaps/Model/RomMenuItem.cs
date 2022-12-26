namespace PinMAME.NvMaps.Model
{
    public class RomMenuItem
    {
        public string Menu { get; }
        public string Key { get; }
        public string Name { get; }
        public string Value { get; }
        public string DefaultValue { get; }

        public RomMenuItem(string menu, string key, string name, string value, string defaultValue)
        {
            Menu = menu;
            Key = key;
            Name = name;
            Value = value;
            DefaultValue = defaultValue;
        }
    }
}