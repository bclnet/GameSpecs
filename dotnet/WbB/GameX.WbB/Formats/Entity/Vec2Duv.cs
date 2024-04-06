using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    /// <summary>
    /// Info on texture UV mapping
    /// </summary>
    public class Vec2Duv : IHaveMetaInfo
    {
        public readonly float U;
        public readonly float V;

        public Vec2Duv(BinaryReader r)
        {
            U = r.ReadSingle();
            V = r.ReadSingle();
        }

        //: Entity.UV
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"U: {U} V: {V}"),
            };
            return nodes;
        }
    }
}
