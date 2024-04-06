using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    //was:Serialization/KV3File
    public class DATABinaryKV3File
    {
        // <!-- kv3 encoding:text:version{e21c7f3c-8a33-41c5-9977-a76d3a32aa0d} format:generic:version{7412167c-06e9-4698-aff2-e63eb59037e7} -->
        public IDictionary<string, object> Root { get; }
        public string Encoding { get; }
        public string Format { get; }

        public DATABinaryKV3File(IDictionary<string, object> root,
            string encoding = "text:version{e21c7f3c-8a33-41c5-9977-a76d3a32aa0d}",
            string format = "generic:version{7412167c-06e9-4698-aff2-e63eb59037e7}")
        {
            Root = root;
            Encoding = encoding;
            Format = format;
        }

        public override string ToString()
        {
            using var w = new IndentedTextWriter();
            w.WriteLine(string.Format("<!-- kv3 encoding:{0} format:{1} -->", Encoding, Format));
            //Root.Serialize(w);
            return w.ToString();
        }
    }
}
