using GameSpec.Metadata;
using OpenStack.Graphics;
using OpenStack.Graphics.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public class BinaryDds : ITexture, IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryDds(r));

        BinaryReader _r;

        //public BinaryDds() { }
        public BinaryDds(BinaryReader r, bool readMagic = true)
        {
            _r = r;
            Buffer = DDS_HEADER.Read(r, readMagic, out Header, out HeaderDXT10, out Format);
        }

        DDS_HEADER Header;
        DDS_HEADER_DXT10? HeaderDXT10;
        (object type, int blockSize, object gl, object unity) Format;
        byte[] Buffer;
        int Offset;

        public IDictionary<string, object> Data => null;
        public int Width => (int)Header.dwWidth;
        public int Height => (int)Header.dwHeight;
        public int Depth => 0;
        public TextureFlags Flags => 0;
        public object UnityFormat => Format.unity;
        public object GLFormat => Format.gl;
        public int NumMipMaps => (int)Header.dwMipMapCount;
        public Span<byte> this[int index]
        {
            get
            {
                int w = (int)Header.dwWidth >> index, h = (int)Header.dwHeight >> index;
                if (w == 0 || h == 0) return null;
                var size = ((w + 3) / 4) * ((h + 3) / 4) * Format.blockSize;
                var remaining = Buffer.Length - Offset;
                var r = remaining > 0 ? Buffer.AsSpan(Offset, Math.Min(size, remaining)) : null;
                Offset += size;
                return r;
            }
            set => throw new NotImplementedException();
        }
        public void MoveToData(out bool forward) { forward = true; Offset = 0; }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new() {
            new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetadataInfo("Texture", items: new List<MetadataInfo> {
                new MetadataInfo($"Format: {Format.type}"),
                new MetadataInfo($"Width: {Width}"),
                new MetadataInfo($"Height: {Height}"),
                new MetadataInfo($"Mipmaps: {NumMipMaps}"),
            }),
        };
    }
}
