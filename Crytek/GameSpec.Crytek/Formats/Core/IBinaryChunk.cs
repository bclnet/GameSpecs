using System.IO;

namespace GameSpec.Cry.Formats.Core
{
    public interface IBinaryChunk
    {
        void Read(BinaryReader r);
        void Write(BinaryWriter w);
    }
}
