using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/KeyValuesOrNTRO
    public class DATABinaryKV3OrNTRO : DATA
    {
        readonly string IntrospectionStructName;
        protected Binary_Pak Parent { get; private set; }
        public IDictionary<string, object> Data { get; private set; }
        DATA BackingData;

        public DATABinaryKV3OrNTRO() { }
        public DATABinaryKV3OrNTRO(string introspectionStructName) => IntrospectionStructName = introspectionStructName;

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            Parent = parent;
            if (!parent.ContainsBlockType<NTRO>())
            {
                var kv3 = new DATABinaryKV3 { Offset = Offset, Size = Size };
                kv3.Read(parent, r);
                Data = kv3.Data;
                BackingData = kv3;
            }
            else
            {
                var ntro = new DATABinaryNTRO { StructName = IntrospectionStructName, Offset = Offset, Size = Size };
                ntro.Read(parent, r);
                Data = ntro.Data;
                BackingData = ntro;
            }
        }

        public override string ToString() => BackingData is DATABinaryKV3 kv3 ? kv3.ToString() : BackingData.ToString();
    }
}
