using GameX.Formats;
using GameX.Meta;
using GameX.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZstdNet;

// https://wiki.ultimacodex.com/wiki/Ultima_IX_internal_formats#FLX_Format
namespace GameX.Origin.Formats.U9
{
    #region Binary_Palette

    public unsafe class Binary_Palette : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Palette(r));
        public static Binary_Palette Instance;

        #region Records

        public byte[][] Records;

        #endregion

        // file: static/ankh.pal
        public Binary_Palette(BinaryReader r)
        {
            Records = r.ReadTArray<uint>(sizeof(uint), 256).Select(s => BitConverter.GetBytes(s)).ToArray();
            Instance = this;
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Pallet File" }),
                new MetaInfo("Pallet", items: new List<MetaInfo> {
                    new MetaInfo($"Records: {Records.Length}"),
                })
            };
    }

    #endregion

    #region Binary_Music

    public unsafe class Binary_Music : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Music(r));

        #region Records

        #endregion

        // file: sound/music.flx:file0000.sfm
        public Binary_Music(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Music File" }),
                new MetaInfo("Music", items: new List<MetaInfo> {
                    //new MetaInfo($"Records: {Records.Length}"),
                })
            };
    }

    #endregion

    #region Binary_Sfx

    public unsafe class Binary_Sfx : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Sfx(r));

        #region Records

        #endregion

        // file: sound/sfx.flx:file0000.sfx
        public Binary_Sfx(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Sfx File" }),
                new MetaInfo("Sfx", items: new List<MetaInfo> {
                    //new MetaInfo($"Records: {Records.Length}"),
                })
            };
    }

    #endregion

    #region Binary_Speech

    public unsafe class Binary_Speech : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Speech(r));

        #region Records

        #endregion

        // file: sound/Speech.flx:file0000.spk
        public Binary_Speech(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Speech File" }),
                new MetaInfo("Speech", items: new List<MetaInfo> {
                    //new MetaInfo($"Records: {Records.Length}"),
                })
            };
    }

    #endregion

    #region Binary_Anim

    public unsafe class Binary_Anim : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Anim(r));

        #region Records

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct AnimHeader
        {
            public static (string, int) Struct = ("<IQIQ", sizeof(AnimHeader));
            public uint Index;              // Same as the record index.
            public ulong Unknown1;          //
            public uint FrameCount;         // The maximum number of frames.
            public ulong Unknown2;           //
        }

        public struct AnimPart
        {
            public uint Id;
            public string Name;
            public AnimFrame[] Frames;
            public override string ToString() => $"{Id:000}: {Name}";
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        public struct AnimFrame
        {
            public static (string, int) Struct = ("<5HI", sizeof(AnimFrame));
            public uint MS;             // The number of milliseconds from the start of the animation to display this frame for this part.
            public float RotationW;     // Rotation quaternion
            public float RotationX;     // Rotation quaternion
            public float RotationY;     // Rotation quaternion
            public float RotationZ;     // Rotation quaternion
            public Vector3 Position;    // The relative transform for positioning the part.
            public Vector3 Scale;       // The scaling factor.
        }

        public string Filename;
        public uint[] Elements;
        public AnimPart[] Parts;
        public (uint, uint, uint)[] Suffixs;

        #endregion

        // file: static/anim.flx:file00ac.anim
        public Binary_Anim(BinaryReader r)
        {
            var header = r.ReadS<AnimHeader>();
            Filename = r.ReadL32AString();
            Elements = r.ReadL32TArray<uint>(sizeof(uint));
            Parts = r.ReadFArray(s => new AnimPart
            {
                Id = r.ReadUInt32(),
                Name = r.ReadL32AString(),
                Frames = r.ReadL32SArray<AnimFrame>(),
            }, r.ReadInt32());
            Suffixs = r.ReadL32TArray<(uint, uint, uint)>(sizeof((uint, uint, uint)));
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = $"Anim: {Filename}" }),
                new MetaInfo("Anim", items: new List<MetaInfo> {
                    new MetaInfo($"Elements: {Elements.Length}"),
                    new MetaInfo($"Parts: {Parts.Length}"),
                    new MetaInfo($"Suffixs: {Suffixs.Length}"),
                })
            };
    }

    #endregion

    #region Binary_Bitmap

    public unsafe class Binary_Bitmap : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Bitmap(r, s));

        #region Records

        class Record
        {
            public int Width;
            public int Height;
            public byte[] Pixels;
            public Record(int width, int height, byte[] pixels)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct BmpHeader
        {
            public static (string, int) Struct = ("<5HI", sizeof(BmpHeader));
            public ushort Width;        // Maximum width in pixels of all frames.
            public ushort Format;       // Possibly format-related.
            public ushort Height;       // Maximum height in pixels of all frames.
            public ushort Compression;  // 0 = uncompressed, 1 = 8-bit compression of some sort.
            public uint FrameCount;   // Number of frames.
            public uint Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct BmpFrameOffset
        {
            public static (string, int) Struct = ("<2I", sizeof(BmpFrameOffset));
            public uint Offset;     // Offset of the frame from the beginning of the file.
            public uint Size;       // Length of the frame in bytes.
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct BmpFrame
        {
            public static (string, int) Struct = ("<2H4I", sizeof(BmpFrame));
            public ushort Unknown1;     // Unknown
            public ushort Unknown2;     // Unknown, usually 0x6000.
            public uint Width;          // Width in pixels of the frame.
            public uint Height;         // Height in pixels of the frame.
            public uint Unknown3;       // Unknown, almost always 0.
            public uint Unknown4;       // Unknown, almost always 0.
        }

        int Index = 0;
        Record Current => Records[Index];
        Record[] Records;
        int BytesPerPixel = 1;
        (object gl, object vulken, object unity, object unreal) Format => Formats[BytesPerPixel == 1 ? 0 : 1];
        static (object gl, object vulken, object unity, object unreal)[] Formats = new (object gl, object vulken, object unity, object unreal)[] {
            // format-0
            ((TextureGLFormat.Rgba, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedInt8888Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedInt8888Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown),
            // format-1
            ((TextureGLFormat.Rgb, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedShort565),
            (TextureGLFormat.Rgb, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedShort565),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown)};

        #endregion

        // file: static/bitmap16.flx:file0001.bmp
        public Binary_Bitmap(BinaryReader r, PakFile s)
        {
            switch ((char)s.Tag)
            {
                case '6': BytesPerPixel = 2; break;
                case 'c': BytesPerPixel = 2; break;
            }

            // get palette
            byte[][] palette = null;
            if (BytesPerPixel == 1)
            {
                s.Game.Ensure();
                palette = Binary_Palette.Instance?.Records ?? throw new NotImplementedException();
            }

            // read header
            var header = r.ReadS<BmpHeader>();
            var compression = header.Compression;

            // read records
            Records = r.ReadSArray<BmpFrameOffset>((int)header.FrameCount).Select(s =>
            {
                r.Seek(s.Offset);
                var frame = r.ReadS<BmpFrame>();
                int width = (int)frame.Width, height = (int)frame.Height;
                var offsets = r.ReadTArray<uint>(sizeof(uint), height); // Offset to the data for each row relative to the start of the resource.
                if (offsets[0] == 0xcdcdcdcd) // unknownFrame
                    return new Record(width, height, null);
                r.Seek(s.Offset + offsets[0]);
                var length = width * height * BytesPerPixel;
                var data = compression == 1 ? r.DecompressLz4((int)s.Size, length) : r.ReadBytes(length);
                //if (length != data.Length) throw new Exception("Length");
                return new Record(width, height, BytesPerPixel == 1 ? data.SelectMany(x => palette[x]).ToArray() : data);
            }).ToArray();
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width => Current.Width;
        public int Height => Current.Height;
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Current?.Pixels;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("Bitmap", items: new List<MetaInfo> {
                    new MetaInfo($"BytesPerPixel: {BytesPerPixel}"),
                    new MetaInfo($"Index: {Index}"),
                    new MetaInfo($"Width: {Current.Width}"),
                    new MetaInfo($"Height: {Current.Height}"),
                })
            };
    }

    #endregion

    #region Binary_Book

    public unsafe class Binary_Book : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Book(r));

        #region Records

        public string Title;
        public string Body;

        #endregion

        // file: static/BOOKS-EN.FLX:file0000.book
        public Binary_Book(BinaryReader r)
        {
            Title = r.ReadL32AString();
            Body = r.ReadL32AString();
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = Body }),
                new MetaInfo("Book", items: new List<MetaInfo> {
                    new MetaInfo($"Title: {Title}"),
                })
            };
    }

    #endregion

    #region Binary_Text

    public unsafe class Binary_Text : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Text(r, (int)f.FileSize));

        #region Records

        public string Text;

        #endregion

        // file: static/misctext.flx:file0000.str
        // file: static/text.flx:file0000.str
        public Binary_Text(BinaryReader r, int length)
        {
            Text = Encoding.Unicode.GetString(r.ReadBytes(length));
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = Text }),
                new MetaInfo("Text", items: new List<MetaInfo> {
                })
            };
    }

    #endregion

    #region Binary_Mesh

    public unsafe class Binary_Mesh : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Mesh(r));

        #region Records

        #endregion

        // file: static/sappear.flx:file0000.mesh
        public Binary_Mesh(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Mesh File" }),
                new MetaInfo("Mesh", items: new List<MetaInfo> {
                    //new MetaInfo($"Records: {Records.Length}"),
                })
            };
    }

    #endregion

    #region Binary_Texture

    public unsafe class Binary_Texture : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Texture(r, s));

        #region Records

        int BytesPerPixel = 1;
        byte[] Pixels;
        (object gl, object vulken, object unity, object unreal) Format => Formats[BytesPerPixel == 1 ? 0 : 1];
        static (object gl, object vulken, object unity, object unreal)[] Formats = new (object gl, object vulken, object unity, object unreal)[] {
            // format-0
            ((TextureGLFormat.Rgba, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedInt8888Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedInt8888Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown),
            // format-1
            ((TextureGLFormat.Rgb, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedShort565),
            (TextureGLFormat.Rgb, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedShort565),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown)};

        #endregion

        // file: static/Texture8.9:file043f.tex
        // file: static/texture16.9:file043f.tex
        public Binary_Texture(BinaryReader r, PakFile s)
        {
            switch ((char)s.Tag)
            {
                case '6': BytesPerPixel = 2; break;
            }

            // get palette
            byte[][] palette = null;
            if (BytesPerPixel == 1)
            {
                s.Game.Ensure();
                palette = Binary_Palette.Instance?.Records ?? throw new NotImplementedException();
            }

            // read header
            var width = Width = r.ReadInt32();
            var height = Height = r.ReadInt32();
            r.Skip(8);

            // read record
            var length = width * height * BytesPerPixel;
            var data = r.ReadBytes(width * height * BytesPerPixel);
            Pixels = BytesPerPixel == 1 ? data.SelectMany(x => palette[x]).ToArray() : data;
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Pixels;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("Texture", items: new List<MetaInfo> {
                    new MetaInfo($"BytesPerPixel: {BytesPerPixel}"),
                    new MetaInfo($"Width: {Width}"),
                    new MetaInfo($"Height: {Height}"),
                })
            };
    }

    #endregion

    #region Binary_Typename

    public unsafe class Binary_Typename : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Typename(r));

        #region Records

        public uint ScriptId;       // Either 0, -1, or rarely a value above 255. Possibly script-related.
        public ushort IconId;         // Inventory Icon ID for an image in "static/bitmaps16.flx" or "static/bitmaps8.flx".
        public string ToolTip;      // Tooltip for the type. Null-terminated text.

        #endregion

        // file: static/TYPENAME.FLX:file0001.type
        public Binary_Typename(BinaryReader r)
        {
            ScriptId = r.ReadUInt32();
            IconId = r.ReadUInt16();
            ToolTip = r.ReadCString();
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = ToolTip }),
                new MetaInfo("Typename", items: new List<MetaInfo> {
                    new MetaInfo($"ScriptId: {ScriptId}"),
                    new MetaInfo($"IconId: {IconId}"),
                })
            };
    }

    #endregion
}
