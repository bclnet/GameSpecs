using GameX.Formats;
using OpenStack.Graphics.DirectX;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Bethesda.Formats
{
    public unsafe class PakBinary_Ba2 : PakBinary<PakBinary_Ba2>
    {
        // Header : TES5
        #region Header : TES5
        // http://en.uesp.net/wiki/Bethesda5Mod:Archive_File_Format

        // Default header data
        const uint F4_BSAHEADER_FILEID = 0x58445442; // Magic for Fallout 4 BA2, the literal string "BTDX".
        const uint F4_BSAHEADER_VERSION1 = 0x01; // Version number of a Fallout 4 BA2
        const uint F4_BSAHEADER_VERSION2 = 0x02; // Version number of a Starfield BA2

        public enum F4_HeaderType : uint
        {
            GNRL = 0x4c524e47,
            DX10 = 0x30315844,
            GNMF = 0x464d4e47,
            Unknown,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_Header
        {
            public static (string, int) Struct = ("<IIIQ", sizeof(F4_Header));
            public uint Version;            // 04
            public F4_HeaderType Type;      // 08 GNRL=General, DX10=Textures, GNMF=?, ___=?
            public uint NumFiles;           // 0C
            public ulong NameTableOffset;   // 10 - relative to start of file
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_File
        {
            public static (string, int) Struct = ("<I4sIIQIII", sizeof(F4_File));
            public uint NameHash;           // 00
            public fixed byte Ext[4];       // 04 - extension
            public uint DirHash;            // 08
            public uint Flags;              // 0C - flags? 00100100
            public ulong Offset;            // 10 - relative to start of file
            public uint PackedSize;         // 18 - packed length (zlib)
            public uint FileSize;           // 1C - unpacked length
            public uint Align;              // 20 - BAADF00D
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_Texture
        {
            public static (string, int) Struct = ("<I4sIBBHHHBBBB", sizeof(F4_Texture));
            public uint NameHash;           // 00
            public fixed byte Ext[4];       // 04 - extension
            public uint DirHash;            // 08
            public byte Unk0C;              // 0C
            public byte NumChunks;          // 0D
            public ushort ChunkHeaderSize;  // 0E - size of one chunk header
            public ushort Height;           // 10
            public ushort Width;            // 12
            public byte NumMips;            // 14
            public byte Format;             // 15 - DXGI_FORMAT
            public byte IsCubemap;          // 16
            public byte TileMode;           // 17
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_GNMF
        {
            public static (string, int) Struct = ("<I4sIBBH32sQIIII", sizeof(F4_GNMF));
            public uint NameHash;           // 00
            public fixed byte Ext[4];       // 04 - extension
            public uint DirHash;            // 08
            public byte Unk0C;              // 0C
            public byte NumChunks;          // 0D
            public ushort Unk0E;            // 0E
            public fixed byte Header[32];   // 10
            public ulong Offset;            // 30
            public uint PackedSize;         // 38
            public uint FileSize;           // 3C
            public uint Unk40;              // 40
            public uint Align;              // 44
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_TextureChunk
        {
            public static (string, int) Struct = ("<QIIHHI", sizeof(F4_TextureChunk));
            public ulong Offset;            // 00
            public uint PackedSize;         // 08
            public uint FileSize;           // 0C
            public ushort StartMip;         // 10
            public ushort EndMip;           // 12
            public uint Align;              // 14 - BAADFOOD
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            FileSource[] files;

            // Fallout 4 - Starfield
            var magic = source.Magic = r.ReadUInt32();
            if (magic == F4_BSAHEADER_FILEID)
            {
                var header = r.ReadS<F4_Header>();
                if (header.Version > F4_BSAHEADER_VERSION2) throw new FormatException("BAD MAGIC");
                source.Version = header.Version;
                source.Files = files = new FileSource[header.NumFiles];
                // version2
                //if (header.Version == F4_BSAHEADER_VERSION2) r.Skip(8);

                switch (header.Type)
                {
                    // General BA2 Format
                    case F4_HeaderType.GNRL:
                        var headerFiles = r.ReadTArray<F4_File>(sizeof(F4_File), (int)header.NumFiles);
                        for (var i = 0; i < headerFiles.Length; i++)
                        {
                            ref F4_File headerFile = ref headerFiles[i];
                            files[i] = new FileSource
                            {
                                Compressed = headerFile.PackedSize != 0 ? 1 : 0,
                                PackedSize = headerFile.PackedSize,
                                FileSize = headerFile.FileSize,
                                Offset = (long)headerFile.Offset,
                            };
                        }
                        break;
                    // Texture BA2 Format
                    case F4_HeaderType.DX10:
                        for (var i = 0; i < header.NumFiles; i++)
                        {
                            var headerTexture = r.ReadS<F4_Texture>();
                            var headerTextureChunks = r.ReadTArray<F4_TextureChunk>(sizeof(F4_TextureChunk), headerTexture.NumChunks);
                            ref F4_TextureChunk firstChunk = ref headerTextureChunks[0];
                            files[i] = new FileSource
                            {
                                FileInfo = headerTexture,
                                PackedSize = firstChunk.PackedSize,
                                FileSize = firstChunk.FileSize,
                                Offset = (long)firstChunk.Offset,
                                Tag = headerTextureChunks,
                            };
                        }
                        break;
                    // GNMF BA2 Format
                    case F4_HeaderType.GNMF:
                        for (var i = 0; i < header.NumFiles; i++)
                        {
                            var headerGNMF = r.ReadS<F4_GNMF>();
                            var headerTextureChunks = r.ReadTArray<F4_TextureChunk>(sizeof(F4_TextureChunk), headerGNMF.NumChunks);
                            files[i] = new FileSource
                            {
                                FileInfo = headerGNMF,
                                PackedSize = headerGNMF.PackedSize,
                                FileSize = headerGNMF.FileSize,
                                Offset = (long)headerGNMF.Offset,
                                Tag = headerTextureChunks,
                            };
                        }
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(header.Type), header.Type.ToString());
                }

                // assign full names to each file
                if (header.NameTableOffset > 0)
                {
                    r.Seek((long)header.NameTableOffset);
                    var path = r.ReadL16Encoding().Replace('\\', '/');
                    foreach (var file in files) file.Path = path;
                }
            }
            else throw new InvalidOperationException("BAD MAGIC");
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            const int GNF_HEADER_MAGIC = 0x20464E47;
            const int GNF_HEADER_CONTENT_SIZE = 248;

            // position
            r.Seek(file.Offset);

            // General BA2 Format
            if (file.FileInfo == null)
                return Task.FromResult<Stream>(file.Compressed != 0
                    ? new MemoryStream(r.DecompressZlib2((int)file.PackedSize, (int)file.FileSize))
                    : new MemoryStream(r.ReadBytes((int)file.FileSize)));

            // Texture BA2 Format
            else if (file.FileInfo is F4_Texture tex)
            {
                var s = new MemoryStream();
                {
                    // write header
                    var w = new BinaryWriter(s);
                    var ddsHeader = new DDS_HEADER
                    {
                        dwSize = DDS_HEADER.SizeOf,
                        dwFlags = DDSD.HEADER_FLAGS_TEXTURE | DDSD.HEADER_FLAGS_LINEARSIZE | DDSD.HEADER_FLAGS_MIPMAP,
                        dwHeight = tex.Height,
                        dwWidth = tex.Width,
                        dwMipMapCount = tex.NumMips,
                        dwCaps = DDSCAPS.SURFACE_FLAGS_TEXTURE | DDSCAPS.SURFACE_FLAGS_MIPMAP,
                        dwCaps2 = tex.IsCubemap == 1 ? DDSCAPS2.CUBEMAP_ALLFACES : 0,
                    };
                    ddsHeader.ddspf.dwSize = DDS_PIXELFORMAT.SizeOf;
                    switch ((DXGI_FORMAT)tex.Format)
                    {
                        case DXGI_FORMAT.BC1_UNORM:
                            ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                            ddsHeader.ddspf.dwFourCC = FourCC.DXT1;
                            ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height / 2U); // 4bpp
                            break;
                        case DXGI_FORMAT.BC2_UNORM:
                            ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                            ddsHeader.ddspf.dwFourCC = FourCC.DXT3;
                            ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                            break;
                        case DXGI_FORMAT.BC3_UNORM:
                            ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                            ddsHeader.ddspf.dwFourCC = FourCC.DXT5;
                            ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                            break;
                        case DXGI_FORMAT.BC5_UNORM:
                            ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                            ddsHeader.ddspf.dwFourCC = FourCC.ATI2;
                            ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                            break;
                        case DXGI_FORMAT.BC1_UNORM_SRGB:
                            ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                            ddsHeader.ddspf.dwFourCC = FourCC.DX10;
                            ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height / 2); // 4bpp
                            break;
                        case DXGI_FORMAT.BC3_UNORM_SRGB:
                        case DXGI_FORMAT.BC4_UNORM:
                        case DXGI_FORMAT.BC5_SNORM:
                        case DXGI_FORMAT.BC7_UNORM:
                        case DXGI_FORMAT.BC7_UNORM_SRGB:
                            ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                            ddsHeader.ddspf.dwFourCC = FourCC.DX10;
                            ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                            break;
                        case DXGI_FORMAT.R8G8B8A8_UNORM:
                        case DXGI_FORMAT.R8G8B8A8_UNORM_SRGB:
                            ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHA;
                            ddsHeader.ddspf.dwRGBBitCount = 32;
                            ddsHeader.ddspf.dwRBitMask = 0x000000FF;
                            ddsHeader.ddspf.dwGBitMask = 0x0000FF00;
                            ddsHeader.ddspf.dwBBitMask = 0x00FF0000;
                            ddsHeader.ddspf.dwABitMask = 0xFF000000;
                            ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height * 4); // 32bpp
                            break;
                        case DXGI_FORMAT.B8G8R8A8_UNORM:
                        case DXGI_FORMAT.B8G8R8X8_UNORM:
                            ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHA;
                            ddsHeader.ddspf.dwRGBBitCount = 32;
                            ddsHeader.ddspf.dwRBitMask = 0x00FF0000;
                            ddsHeader.ddspf.dwGBitMask = 0x0000FF00;
                            ddsHeader.ddspf.dwBBitMask = 0x000000FF;
                            ddsHeader.ddspf.dwABitMask = 0xFF000000;
                            ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height * 4); // 32bpp
                            break;
                        case DXGI_FORMAT.R8_UNORM:
                            ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHA;
                            ddsHeader.ddspf.dwRGBBitCount = 8;
                            ddsHeader.ddspf.dwRBitMask = 0xFF;
                            ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                            break;
                        default: throw new ArgumentOutOfRangeException(nameof(tex.Format), $"Unsupported DDS header format. File: {file.Path}");
                    }
                    w.Write(DDS_HEADER.MAGIC);
                    w.WriteT(ddsHeader, sizeof(DDS_HEADER));
                    switch ((DXGI_FORMAT)tex.Format)
                    {
                        case DXGI_FORMAT.BC1_UNORM_SRGB:
                        case DXGI_FORMAT.BC3_UNORM_SRGB:
                        case DXGI_FORMAT.BC4_UNORM:
                        case DXGI_FORMAT.BC5_SNORM:
                        case DXGI_FORMAT.BC7_UNORM:
                        case DXGI_FORMAT.BC7_UNORM_SRGB:
                            var dxt10 = new DDS_HEADER_DXT10
                            {
                                dxgiFormat = (DXGI_FORMAT)tex.Format,
                                resourceDimension = D3D10_RESOURCE_DIMENSION.TEXTURE2D,
                                miscFlag = 0,
                                arraySize = 1,
                                miscFlags2 = (uint)DDS_ALPHA_MODE.ALPHA_MODE_UNKNOWN,
                            };
                            w.WriteT(dxt10, sizeof(DDS_HEADER_DXT10));
                            break;
                    }

                    // write chunks
                    var chunks = (F4_TextureChunk[])file.Tag;
                    for (var i = 0; i < tex.NumChunks; i++)
                    {
                        var chunk = chunks[i];
                        r.Seek((long)chunk.Offset);
                        if (chunk.PackedSize != 0) s.WriteBytes(r.DecompressZlib((int)file.PackedSize, (int)file.FileSize));
                        else s.WriteBytes(r, (int)file.FileSize);
                    }
                    s.Position = 0;
                    return Task.FromResult<Stream>(s);
                }
            }
            // GNMF BA2 Format
            else if (file.FileInfo is F4_GNMF gnmf)
            {
                var s = new MemoryStream();
                {
                    // write header
                    var w = new BinaryWriter(s);
                    w.Write(GNF_HEADER_MAGIC); // 'GNF ' magic
                    w.Write(GNF_HEADER_CONTENT_SIZE); // Content-size. Seems to be either 4 or 8 bytes
                    w.Write((byte)0x2); // Version
                    w.Write((byte)0x1); // Texture Count
                    w.Write((byte)0x8); // Alignment
                    w.Write((byte)0x0); // Unused
                    w.Write(BitConverter.GetBytes(gnmf.FileSize + 256).Reverse().ToArray()); // File size + header size
                    w.Write(UnsafeX.FixedTArray(gnmf.Header, 32));
                    for (var i = 0; i < 208; i++) w.Write((byte)0x0); // Padding

                    // write chunks
                    var chunks = (F4_TextureChunk[])file.Tag;
                    for (var i = 0; i < gnmf.NumChunks; i++)
                    {
                        var chunk = chunks[i];
                        r.Seek((long)chunk.Offset);
                        if (chunk.PackedSize != 0) s.WriteBytes(r.DecompressZlib2((int)file.PackedSize, (int)file.FileSize));
                        else s.WriteBytes(r, (int)file.FileSize);
                    }
                    s.Position = 0;
                    return Task.FromResult<Stream>(s);
                }
            }
            else throw new ArgumentOutOfRangeException(nameof(file.FileInfo), file.FileInfo.ToString());
        }
    }
}