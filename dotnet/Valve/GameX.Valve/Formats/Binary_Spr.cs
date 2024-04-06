using GameX.Meta;
using GameX.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Formats
{
    // https://github.com/yuraj11/HL-Texture-Tools
    public unsafe class Binary_Spr : ITexture, IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Spr(r, f));

        // Headers
        #region SPR

        const uint SPR_MAGIC = 0x50534449; //: IDSP

        /// <summary>
        /// Type of sprite.
        /// </summary>
        public enum SprType : int
        {
            VP_PARALLEL_UPRIGHT,
            FACING_UPRIGHT,
            VP_PARALLEL,
            ORIENTED,
            VP_PARALLEL_ORIENTED
        }

        /// <summary>
        /// Texture format of sprite.
        /// </summary>
        public enum SprTextFormat : int
        {
            SPR_NORMAL,
            SPR_ADDITIVE,
            SPR_INDEXALPHA,
            SPR_ALPHTEST
        }

        /// <summary>
        /// Synch. type of sprite.
        /// </summary>
        public enum SprSynchType : int
        {
            Synchronized,
            Random
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SPR_Header
        {
            public static (string, int) Struct = ("<I3if3ifi", sizeof(SPR_Header));
            public uint Signature;
            public int Version;
            public SprType Type;
            public SprTextFormat TextFormat;
            public float BoundingRadius;
            public int MaxWidth;
            public int MaxHeight;
            public int NumFrames;
            public float BeamLen;
            public SprSynchType SynchType;
        }

        //[StructLayout(LayoutKind.Sequential)]
        //struct WAD_Lump
        //{
        //    public const int SizeOf = 32;
        //    public uint Offset;
        //    public uint DiskSize;
        //    public uint Size;
        //    public byte Type;
        //    public byte Compression;
        //    public ushort Padding;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string Name;
        //}

        [StructLayout(LayoutKind.Sequential)]
        struct SPR_Frame
        {
            public static (string, int) Struct = ("<5i", sizeof(SPR_Frame));
            public int Group;
            public int OriginX;
            public int OriginY;
            public int Width;
            public int Height;
        }

        #endregion

        public Binary_Spr(BinaryReader r, FileSource f)
        {
            Format = ((TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGBA32, TextureUnityFormat.RGBA32);

            // read file
            var header = r.ReadS<SPR_Header>();
            if (header.Signature != SPR_MAGIC) throw new FormatException("BAD MAGIC");

            // load palette
            palette = r.ReadBytes(r.ReadUInt16() * 3);

            // load frames
            frames = new SPR_Frame[header.NumFrames];
            pixels = new byte[header.NumFrames][];
            for (var i = 0; i < header.NumFrames; i++)
            {
                frames[i] = r.ReadS<SPR_Frame>();
                ref SPR_Frame frame = ref frames[i];
                var pixelSize = frame.Width * frame.Height;
                pixels[i] = r.ReadBytes(pixelSize);
            }
            width = frames[0].Width;
            height = frames[0].Height;
        }

        int width;
        int height;
        SPR_Frame[] frames;
        byte[][] pixels;
        byte[] palette;

        (object gl, object vulken, object unity, object unreal) Format;

        public IDictionary<string, object> Data => null;
        public int Width => width;
        public int Height => height;
        public int Depth => 0;
        public int MipMaps => pixels.Length;
        public TextureFlags Flags => 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            static void FlattenPalette(Span<byte> data, byte[] source, byte[] palette)
            {
                fixed (byte* _ = data)
                    for (int i = 0, pi = 0; i < source.Length; i++, pi += 4)
                    {
                        var pa = source[i] * 3;
                        //if (pa + 3 > palette.Length) continue;
                        _[pi + 0] = palette[pa + 0];
                        _[pi + 1] = palette[pa + 1];
                        _[pi + 2] = palette[pa + 2];
                        _[pi + 3] = 0xFF;
                    }
            }

            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.StereoKit => throw new NotImplementedException("StereoKit"),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            var bytes = new byte[pixels.Sum(x => x.Length) * 4];
            ranges = new Range[pixels.Length];
            byte[] p;
            for (int index = 0, offset = 0; index < pixels.Length; index++, offset += p.Length * 4)
            {
                p = pixels[index];
                var range = ranges[index] = new Range(offset, offset + p.Length * 4);
                FlattenPalette(bytes.AsSpan(range), p, palette);
            }
            return bytes;
        }
        public void End() { }

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetaInfo("Texture", items: new List<MetaInfo> {
                new MetaInfo($"Width: {Width}"),
                new MetaInfo($"Height: {Height}"),
                new MetaInfo($"Mipmaps: {MipMaps}"),
            }),
        };
    }
}
