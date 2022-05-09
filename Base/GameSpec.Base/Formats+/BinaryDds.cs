using GameSpec.Metadata;
using OpenStack.Graphics;
using OpenStack.Graphics.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public class BinaryDds : ITextureInfo, IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryDds(r));

        BinaryReader _r;

        public BinaryDds() { }
        public BinaryDds(BinaryReader r)
        {
            _r = r;
            var magic = r.ReadUInt32();
            if (magic != DDS_HEADER.DDS_) throw new FormatException($"Invalid DDS file magic: \"{magic}\".");
            Header = r.ReadT<DDS_HEADER>(DDS_HEADER.SizeOf);
            HeaderDXT10 = Header.ddspf.dwFourCC == DDS_HEADER.DX10
                ? (DDS_HEADER_DXT10?)r.ReadT<DDS_HEADER_DXT10>(DDS_HEADER_DXT10.SizeOf)
                : null;
            Header.Verify();
        }

        public DDS_HEADER Header;
        public DDS_HEADER_DXT10? HeaderDXT10;

        public IDictionary<string, object> Data => null;
        public int Width => (int)Header.dwWidth;
        public int Height => (int)Header.dwHeight;
        public int Depth => 0;
        public TextureFlags Flags => 0;
        public TextureUnityFormat UnityFormat => Header.ddspf.dwFourCC switch
        {
            DDS_HEADER.DXT1 => TextureUnityFormat.DXT1,
            DDS_HEADER.DXT5 => TextureUnityFormat.DXT5,
            DDS_HEADER.DX10 => HeaderDXT10?.dxgiFormat switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(HeaderDXT10.Value.dxgiFormat), $"{HeaderDXT10?.dxgiFormat}"),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(Header.ddspf.dwFourCC), $"{Header.ddspf.dwFourCC}"),
        };
        // https://www.g-truc.net/post-0335.html
        public TextureGLFormat GLFormat => Header.ddspf.dwFourCC switch
        {
            DDS_HEADER.DXT1 => TextureGLFormat.CompressedRgbaS3tcDxt1Ext,
            DDS_HEADER.DXT5 => TextureGLFormat.CompressedRgbaS3tcDxt5Ext,
            DDS_HEADER.DX10 => HeaderDXT10?.dxgiFormat switch
            {
                DXGI_FORMAT.BC5_UNORM => TextureGLFormat.CompressedRgRgtc2,
                DXGI_FORMAT.BC5_SNORM => TextureGLFormat.CompressedSignedRgRgtc2,
                _ => throw new ArgumentOutOfRangeException(nameof(HeaderDXT10.Value.dxgiFormat), $"{HeaderDXT10?.dxgiFormat}"),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(Header.ddspf.dwFourCC), $"{Header.ddspf.dwFourCC}"),
        };
        public int NumMipMaps => (int)Header.dwMipMapCount;
        public byte[] this[int index]
        {
            get
            {
                //var uncompressedSize = this.GetMipMapTrueDataSize(index);
                //return _r.ReadBytes(uncompressedSize);
                return null;
            }
            set => throw new NotImplementedException();
        }
        public void MoveToData() { }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetadataInfo("DDS Texture", items: new List<MetadataInfo> {
                new MetadataInfo($"Width: {Header.dwWidth}"),
                new MetadataInfo($"Height: {Header.dwHeight}"),
                new MetadataInfo($"Mipmaps: {Header.dwMipMapCount}"),
            }),
        };
    }
}
