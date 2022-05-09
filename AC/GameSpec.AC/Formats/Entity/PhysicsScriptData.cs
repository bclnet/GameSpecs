using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class PhysicsScriptData : IGetMetadataInfo
    {
        public readonly double StartTime;
        public readonly AnimationHook Hook;

        public PhysicsScriptData(BinaryReader r)
        {
            StartTime = r.ReadDouble();
            Hook = AnimationHook.Factory(r);
        }

        //: Entity.PhysicsScriptData
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"StartTime: {StartTime}"),
                new MetadataInfo($"Hook:", items: (Hook as IGetMetadataInfo).GetInfoNodes(tag:tag)),
            };
            return nodes;
        }
    }
}
