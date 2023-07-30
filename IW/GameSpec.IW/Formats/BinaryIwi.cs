using GameSpec.Formats;
using GameSpec.Metadata;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.IW.Formats
{
    // https://github.com/XLabsProject/img-format-helper - IWI
    // https://github.com/DentonW/DevIL/blob/master/DevIL/src-IL/src/il_iwi.cpp - IWI
    // https://github.com/XLabsProject/img-format-helper - IWI
    public class BinaryIwi : ITexture, IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryIwi(r));

        BinaryReader _r;

        public enum VERSION : byte
        {
            /// <summary>
            /// COD2
            /// </summary>
            COD2 = 0x05,
            /// <summary>
            /// COD4
            /// </summary>
            COD4 = 0x06,
            /// <summary>
            /// COD5
            /// </summary>
            COD5 = 0x06,
            /// <summary>
            /// CODMW2
            /// </summary>
            CODMW2 = 0x08,
            /// <summary>
            /// CODMW3
            /// </summary>
            CODMW3 = 0x08,
            /// <summary>
            /// CODBO1
            /// </summary>
            CODBO1 = 0x0D,
            /// <summary>
            /// CODBO2
            /// </summary>
            CODBO2 = 0x1B,
        }

        public enum FORMAT : byte
        {
            /// <summary>
            /// ARGB32 - DDS_Standard_A8R8G8B8
            /// </summary>
            ARGB32 = 0x01,
            /// <summary>
            /// RGB24 - DDS_Standard_R8G8B8
            /// </summary>
            RGB24 = 0x02,
            /// <summary>
            /// GA16 - DDS_Standard_D16_UNORM
            /// </summary>
            GA16 = 0x03,
            /// <summary>
            /// A8 - DDS_Standard_A8_UNORM
            /// </summary>
            A8 = 0x04,
            /// <summary>
            /// A8b - DDS_Standard_A8_UNORM
            /// </summary>
            A8b = 0x05,
            /// <summary>
            /// JPG
            /// </summary>
            JPG = 0x07,
            /// <summary>
            /// DXT1 - DDS_BC1_UNORM;
            /// </summary>
            DXT1 = 0x0B,
            /// <summary>
            /// DXT3 - DDS_BC2_UNORM
            /// </summary>
            DXT2 = 0x0C,
            /// <summary>
            /// DXT5 - DDS_BC3_UNORM
            /// </summary>
            DXT3 = 0x0D,
            /// <summary>
            /// DXT5 - DDS_BC5_UNORM
            /// </summary>
            DXT5 = 0x0E,
        }

        [Flags]
        public enum FLAGS : byte
        {
            NOPICMIP = 1 << 0,
            /// <summary>
            /// NOMIPMAPS
            /// </summary>
            NOMIPMAPS = 1 << 1,
            /// <summary>
            /// CUBEMAP
            /// </summary>
            CUBEMAP = 1 << 2,
            /// <summary>
            /// VOLMAP
            /// </summary>
            VOLMAP = 1 << 3,
            /// <summary>
            /// STREAMING
            /// </summary>
            STREAMING = 1 << 4,
            /// <summary>
            /// LEGACY_NORMALS
            /// </summary>
            LEGACY_NORMALS = 1 << 5,
            /// <summary>
            /// CLAMP_U
            /// </summary>
            CLAMP_U = 1 << 6,
            /// <summary>
            /// CLAMP_V
            /// </summary>
            CLAMP_V = 1 << 7,
        }

        [Flags]
        public enum FLAGS_EXT : int
        {
            /// <summary>
            /// DYNAMIC
            /// </summary>
            DYNAMIC = 1 << 16,
            /// <summary>
            /// RENDER_TARGET
            /// </summary>
            RENDER_TARGET = 1 << 17,
            /// <summary>
            /// SYSTEMMEM
            /// </summary>
            SYSTEMMEM = 1 << 18
        }

        /// <summary>
        /// Describes a IWI file header.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        public unsafe struct HEADER
        {
            public const int SizeOf = 8;
            /// <summary>
            /// MAGIC (IWi)
            /// </summary>
            public const uint MAGIC = 0x69574900;
            /// <summary>
            /// Format
            /// </summary>
            [MarshalAs(UnmanagedType.U1)] public FORMAT Format;
            /// <summary>
            /// Usage
            /// </summary>
            [MarshalAs(UnmanagedType.U1)] public FLAGS Flags;
            /// <summary>
            /// Width
            /// </summary>
            public ushort Width;
            /// <summary>
            /// Height
            /// </summary>
            public ushort Height;
            /// <summary>
            /// Depth
            /// </summary>
            public ushort Depth;

            /// <summary>
            /// Verifies this instance.
            /// </summary>
            public void Verify()
            {
                if (Width == 0 || Height == 0)
                    throw new FormatException($"Invalid DDS file header");
                if (Format >= FORMAT.DXT1 && Format <= FORMAT.DXT5 && Width != MathX.NextPower(Width) && Height != MathX.NextPower(Height))
                    throw new FormatException($"DXT images must have power-of-2 dimensions..");
                if (Format > FORMAT.DXT5)
                    throw new FormatException($"Unknown Format: {Format}");
            }
        }

        //public BinaryIwi() { }
        public BinaryIwi(BinaryReader r)
        {
            _r = r;
            var magic = r.ReadUInt32();
            Version = (VERSION)(magic >> 24);
            magic <<= 8;
            if (magic != HEADER.MAGIC) throw new FormatException($"Invalid IWI file magic: {magic}.");
            if (Version == VERSION.CODMW2) r.Seek(8);
            Header = r.ReadT<HEADER>(HEADER.SizeOf);
            Header.Verify();

            // read mips offsets
            r.Seek(Version switch
            {
                VERSION.COD2 => 0xC,
                VERSION.COD4 => 0xC,
                VERSION.CODMW2 => 0x10,
                VERSION.CODBO1 => 0x10,
                VERSION.CODBO2 => 0x20,
                _ => throw new FormatException($"Invalid IWI Version: {Version}."),
            });

            var mips = r.ReadTArray<int>(sizeof(int), Version < VERSION.CODBO1 ? 4 : 8);
            var mipsLength = mips[0] == mips[1] || mips[0] == mips[^1] ? 1 : mips.Length - 1;
            var mipsBase = mipsLength == 1 ? (int)r.Position() : mips[^1];
            var size = (int)(r.BaseStream.Length - mipsBase);
            Mips = mipsLength > 1
                ? Enumerable.Range(0, mipsLength).Select(i => new Range(mips[i + 1] - mipsBase, mips[i] - mipsBase)).ToArray()
                : new[] { new Range(0, size) };
            r.Seek(mipsBase);
            Body = r.ReadBytes(size);
            Format = Header.Format switch
            {
                FORMAT.ARGB32 => ((TextureGLFormat.Rgba, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedInt8888Reversed), TextureUnityFormat.RGBA32),
                FORMAT.RGB24 => (TextureGLFormat.Rgb, TextureUnityFormat.Unknown),
                FORMAT.DXT1 => (TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureUnityFormat.DXT1),
                FORMAT.DXT2 => (TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureUnityFormat.Unknown),
                FORMAT.DXT3 => (TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureUnityFormat.Unknown),
                FORMAT.DXT5 => (TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureUnityFormat.DXT5),
                _ => throw new ArgumentOutOfRangeException(nameof(Header.Format), $"{Header.Format}"),
            };
        }

        HEADER Header;
        VERSION Version;
        Range[] Mips;
        byte[] Body;
        (object gl, object unity) Format;

        public byte[] RawBytes => null;
        public IDictionary<string, object> Data => null;
        public int Width => (int)Header.Width;
        public int Height => (int)Header.Height;
        public int Depth => 0;
        public TextureFlags Flags => (Header.Flags & FLAGS.CUBEMAP) != 0 ? TextureFlags.CUBE_TEXTURE : 0;
        public object UnityFormat => Format.unity;
        public object GLFormat => Format.gl;
        public int NumMipMaps => Mips.Length;
        public Span<byte> this[int index]
        {
            get => Mips.Length > 1
                ? Body.AsSpan(Mips[index]).ToArray()
                : Body;
            set => throw new NotImplementedException();
        }
        public void MoveToData(out bool forward) => forward = true;

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetadataInfo("DDS Texture", items: new List<MetadataInfo> {
                new MetadataInfo($"Width: {Header.Width}"),
                new MetadataInfo($"Height: {Header.Height}"),
                new MetadataInfo($"Mipmaps: {Mips.Length}"),
            }),
        };
    }
}
