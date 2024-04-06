using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkCompiledMorphTargets_800 : ChunkCompiledMorphTargets
    {
        // TODO: Implement this.
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            NumberOfMorphTargets = (int)r.ReadUInt32();
            MorphTargetVertices = r.ReadTArray<MeshMorphTargetVertex>(MeshMorphTargetVertex.SizeOf, NumberOfMorphTargets);

            // Add to SkinningInfo
            var skin = GetSkinningInfo();
            //skin.MorphTargets = MorphTargetVertices.ToList();
        }
    }
}