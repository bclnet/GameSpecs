using System.IO;

namespace GameSpec.Crytek.Formats.Core
{
    public interface IBinaryChunk
    {
        void Read(BinaryReader r);
        void Write(BinaryWriter w);
    }
}
