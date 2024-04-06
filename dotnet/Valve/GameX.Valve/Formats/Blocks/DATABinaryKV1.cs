using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/BinaryKV1
    public class DATABinaryKV1: DATA
    {
        public const int MAGIC = 0x564B4256; // VBKV

        public IDictionary<string, object> KeyValues { get; private set; }

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.BaseStream.Position = Offset;
            //KeyValues = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary).Deserialize(r.BaseStream);
        }

        public override string ToString()
        {
            using var ms = new MemoryStream();
            using var r = new StreamReader(ms);
            //KVSerializer.Create(KVSerializationFormat.KeyValues1Text).Serialize(ms, KeyValues);
            ms.Seek(0, SeekOrigin.Begin);
            return r.ReadToEnd();
        }
    }
}
