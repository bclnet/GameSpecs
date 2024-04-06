using System;
using System.IO;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkSourceInfo_0 : ChunkSourceInfo
    {
        public override void Read(BinaryReader r)
        {
            ChunkType = _header.ChunkType;
            Version = _header.Version;
            Offset = _header.Offset;
            ID = _header.ID;
            Size = _header.Size;

            r.BaseStream.Seek(_header.Offset, 0);
            var peek = r.ReadUInt32();
            // Try and detect SourceInfo type - if it's there, we need to skip ahead a few bytes
            if ((peek == (uint)ChunkType.SourceInfo) || (peek + 0xCCCBF000 == (uint)ChunkType.SourceInfo)) SkipBytes(r, 12);
            else r.BaseStream.Seek(_header.Offset, 0);

            if (Offset != _header.Offset || Size != _header.Size)
            {
                Log($"Conflict in chunk definition:  SourceInfo chunk");
                Log($"{_header.Offset:X}+{_header.Size:X}");
                Log($"{Offset:X}+{Size:X}");
                LogChunk();
            }

            ChunkType = ChunkType.SourceInfo; // this chunk doesn't actually have the chunktype header.
            SourceFile = r.ReadCString();
            Date = r.ReadCString().TrimEnd(); // Strip off last 2 Characters, because it contains a return
            // It is possible that Date has a newline in it instead of a null.  If so, split it based on newline.  Otherwise read Author.
            if (Date.Contains('\n')) { Author = Date.Split('\n')[1]; Date = Date.Split('\n')[0]; }
            else Author = r.ReadCString();
        }
    }
}