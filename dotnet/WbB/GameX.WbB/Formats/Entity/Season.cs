using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.WbB.Formats.Entity
{
    public class Season : IHaveMetaInfo
    {
        public readonly uint StartDate;
        public readonly string Name;

        public Season(BinaryReader r)
        {
            StartDate = r.ReadUInt32();
            Name = r.ReadL16Encoding(Encoding.Default); r.Align();
        }

        //: Entity.Season
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"StartDate: {StartDate}"),
                new MetaInfo($"Name: {Name}"),
            };
            return nodes;
        }
    }
}
