using GameSpec.Metadata;
using OpenStack.Graphics;
using OpenStack.Graphics.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// https://github.com/paroj/nv_dds/blob/master/nv_dds.cpp
namespace GameSpec.Formats
{
    public class BinaryDds : ITexture, IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new BinaryDds(r));

        public BinaryDds(BinaryReader r, bool readMagic = true)
        {
            Bytes = DDS_HEADER.Read(r, readMagic, out Header, out HeaderDXT10, out Format);
            var numMipMaps = Math.Max(1, Header.dwMipMapCount);
            var offset = 0;
            Mips = new Range[numMipMaps];
            for (var i = 0; i < numMipMaps; i++)
            {
                int w = (int)Header.dwWidth >> i, h = (int)Header.dwHeight >> i;
                if (w == 0 || h == 0) { Mips[i] = -1..; continue; }
                var size = ((w + 3) / 4) * ((h + 3) / 4) * Format.blockSize;
                var remains = Math.Min(size, Bytes.Length - offset);
                Mips[i] = remains > 0 ? offset..(offset + remains) : -1..;
                offset += remains;
            }
        }

        DDS_HEADER Header;
        DDS_HEADER_DXT10? HeaderDXT10;
        (object type, int blockSize, object gl, object vulken, object unity, object unreal) Format;
        byte[] Bytes;
        Range[] Mips;

        public IDictionary<string, object> Data => null;
        public int Width => (int)Header.dwWidth;
        public int Height => (int)Header.dwHeight;
        public int Depth => 0;
        public int MipMaps => (int)Header.dwMipMapCount;
        public TextureFlags Flags => 0;

        public byte[] Begin(int platform, out object format, out Range[] mips)
        {
            format = (FamilyPlatform.Type)platform switch
            {
                FamilyPlatform.Type.OpenGL => Format.gl,
                FamilyPlatform.Type.Unity => Format.unity,
                FamilyPlatform.Type.Unreal => Format.unreal,
                FamilyPlatform.Type.Vulken => Format.vulken,
                FamilyPlatform.Type.StereoKit => throw new NotImplementedException("StereoKit"),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            mips = Mips;
            return Bytes;
        }
        public void End() { }

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetaInfo("Texture", items: new List<MetaInfo> {
                new MetaInfo($"Format: {Format.type}"),
                new MetaInfo($"Width: {Width}"),
                new MetaInfo($"Height: {Height}"),
                new MetaInfo($"Mipmaps: {MipMaps}"),
            }),
        };
    }
}
