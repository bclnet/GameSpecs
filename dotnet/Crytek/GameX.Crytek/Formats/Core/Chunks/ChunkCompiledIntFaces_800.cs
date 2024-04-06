using System.IO;
using System.Linq;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkCompiledIntFaces_800 : ChunkCompiledIntFaces
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            NumIntFaces = (int)(DataSize / 6); // This is an array of TFaces, which are 3 uint16.
            Faces = r.ReadTArray<TFace>(TFace.SizeOf, NumIntFaces);

            // Add to SkinningInfo
            var skin = GetSkinningInfo();
            skin.IntFaces = Faces.ToList();
        }
    }
}