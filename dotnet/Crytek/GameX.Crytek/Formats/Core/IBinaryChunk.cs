using System.IO;

namespace GameX.Crytek.Formats.Core
{
    public interface IBinaryChunk
    {
        void Read(BinaryReader r);
        void Write(BinaryWriter w);
    }
}
