using System.Text;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkHeader : Chunk
    {
        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append($"*** CHUNK HEADER ***");
            b.Append($"    ChunkType: {ChunkType}");
            b.Append($"    ChunkVersion: {Version:X}");
            b.Append($"    Offset: {Offset:X}");
            b.Append($"    ID: {ID:X}");
            b.Append($"    Size: {Size:X}");
            b.Append($"*** END CHUNK HEADER ***");
            return b.ToString();
        }
    }
}