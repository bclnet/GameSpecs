using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class PhysicsScriptData : IHaveMetaInfo
    {
        public readonly double StartTime;
        public readonly AnimationHook Hook;

        public PhysicsScriptData(BinaryReader r)
        {
            StartTime = r.ReadDouble();
            Hook = AnimationHook.Factory(r);
        }

        //: Entity.PhysicsScriptData
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"StartTime: {StartTime}"),
                new MetaInfo($"Hook:", items: (Hook as IHaveMetaInfo).GetInfoNodes(tag:tag)),
            };
            return nodes;
        }
    }
}
