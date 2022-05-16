using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class TimeOfDay : IGetMetadataInfo
    {
        public readonly float Start;
        public readonly bool IsNight;
        public readonly string Name;

        public TimeOfDay(BinaryReader r)
        {
            Start = r.ReadSingle();
            IsNight = r.ReadUInt32() == 1;
            Name = r.ReadL16String(Encoding.Default); r.Align();
        }

        //: Entity.TimeOfDay
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Start: {Start}"),
                new MetadataInfo($"IsNight: {IsNight}"),
                new MetadataInfo($"Name: {Name}"),
            };
            return nodes;
        }
    }
}
