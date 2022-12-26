using Newtonsoft.Json;

namespace WPFPrismTemplate.Base
{
    /// <summary>
    /// Brian Rogers <para/>
    /// https://stackoverflow.com/questions/65557528/how-do-you-limit-indentation-depth-when-serializing-with-newtonsoft-json
    /// </summary>
    public class CustomIndentingJsonTextWriter : JsonTextWriter
    {
        public int? MaxIndentDepth { get; set; }

        public CustomIndentingJsonTextWriter(TextWriter writer) : base(writer)
        {
            Formatting = Formatting.Indented;
        }

        public override void WriteStartArray()
        {
            base.WriteStartArray();
            if (MaxIndentDepth.HasValue && Top > MaxIndentDepth.Value)
                Formatting = Formatting.None;
        }

        public override void WriteStartObject()
        {
            base.WriteStartObject();
            if (MaxIndentDepth.HasValue && Top > MaxIndentDepth.Value)
                Formatting = Formatting.None;
        }

        public override void WriteEndArray()
        {
            base.WriteEndArray();
            if (MaxIndentDepth.HasValue && Top <= MaxIndentDepth.Value)
                Formatting = Formatting.Indented;
        }

        public override void WriteEndObject()
        {
            base.WriteEndObject();
            if (MaxIndentDepth.HasValue && Top <= MaxIndentDepth.Value)
                Formatting = Formatting.Indented;
        }
    }
}
