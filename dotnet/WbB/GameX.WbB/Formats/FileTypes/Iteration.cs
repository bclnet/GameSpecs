using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are stored in the client_cell.dat, client_portal.dat, and client_local_English.dat files with the index 0xFFFF0001
    ///
    /// This is essentially the dat "versioning" system.
    /// This is used when first connecting to the server to compare the client dat files with the server dat files and any subsequent patching that may need to be done.
    /// 
    /// Special thanks to the GDLE team for pointing me the right direction on how/where to find this info in the dat files- OptimShi
    /// </summary>
    public class Iteration : FileType, IHaveMetaInfo
    {
        public const uint FILE_ID = 0xFFFF0001;

        public readonly int[] Ints;
        public readonly bool Sorted;

        public Iteration(BinaryReader r)
        {
            Ints = new[] { r.ReadInt32(), r.ReadInt32() };
            Sorted = r.ReadBoolean(); r.Align();
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            for (var i = 0; i < Ints.Length; i++) b.Append($"{Ints[i]},");
            b.Append(Sorted ? "1" : "0");
            return b.ToString();
        }

        //: New
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(Iteration)}: {Id:X8}", items: new List<MetaInfo> {
                })
            };
            return nodes;
        }
    }
}
