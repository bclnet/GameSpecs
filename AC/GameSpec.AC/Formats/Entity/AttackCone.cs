using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class AttackCone : IGetExplorerInfo
    {
        public readonly uint PartIndex;
        // these Left and Right are technically Vec2D types
        public readonly float LeftX; public readonly float LeftY;
        public readonly float RightX; public readonly float RightY;
        public readonly float Radius; public readonly float Height;

        public AttackCone(BinaryReader r)
        {
            PartIndex = r.ReadUInt32();
            LeftX = r.ReadSingle(); LeftY = r.ReadSingle();
            RightX = r.ReadSingle(); RightY = r.ReadSingle();
            Radius = r.ReadSingle(); Height = r.ReadSingle();
        }

        //: Entity.AttackCone
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"PartIndex: {PartIndex}"),
                new ExplorerInfoNode($"LeftX: {LeftX}"),
                new ExplorerInfoNode($"LeftY: {LeftY}"),
                new ExplorerInfoNode($"RightX: {RightX}"),
                new ExplorerInfoNode($"RightY: {RightY}"),
                new ExplorerInfoNode($"Radius: {Radius}"),
                new ExplorerInfoNode($"Height: {Height}"),
            };
            return nodes;
        }
    }
}
