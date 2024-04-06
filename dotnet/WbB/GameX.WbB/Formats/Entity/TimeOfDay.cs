using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.WbB.Formats.Entity
{
    public class TimeOfDay : IHaveMetaInfo
    {
        public readonly float Start;
        public readonly bool IsNight;
        public readonly string Name;

        public TimeOfDay(BinaryReader r)
        {
            Start = r.ReadSingle();
            IsNight = r.ReadUInt32() == 1;
            Name = r.ReadL16Encoding(Encoding.Default); r.Align();
        }

        //: Entity.TimeOfDay
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Start: {Start}"),
                new MetaInfo($"IsNight: {IsNight}"),
                new MetaInfo($"Name: {Name}"),
            };
            return nodes;
        }
    }
}
