using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    /// <summary>
    /// Info on texture UV mapping
    /// </summary>
    public class Vec2Duv : IGetExplorerInfo
    {
        public readonly float U;
        public readonly float V;

        public Vec2Duv(BinaryReader r)
        {
            U = r.ReadSingle();
            V = r.ReadSingle();
        }

        //: Entity.UV
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"U: {U} V: {V}"),
            };
            return nodes;
        }
    }
}
