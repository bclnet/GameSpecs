using GameX.Formats;
using GameX.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GameX.Valve.Formats.Blocks.DATATexture.VTexFormat;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/Texture
    public class DATATexture : DATA, ITexture
    {
        public enum VTexExtraData //was:Resource/Enums/VTexExtraData
        {
            UNKNOWN = 0,
            FALLBACK_BITS = 1,
            SHEET = 2,
            FILL_TO_POWER_OF_TWO = 3,
            COMPRESSED_MIP_SIZE = 4,
            CUBEMAP_RADIANCE_SH = 5,
        }

        [Flags]
        public enum VTexFlags //was:Resource/Enums/VTexFlags
        {
            SUGGEST_CLAMPS = 0x00000001,
            SUGGEST_CLAMPT = 0x00000002,
            SUGGEST_CLAMPU = 0x00000004,
            NO_LOD = 0x00000008,
            CUBE_TEXTURE = 0x00000010,
            VOLUME_TEXTURE = 0x00000020,
            TEXTURE_ARRAY = 0x00000040,
        }

        public enum VTexFormat : byte //was:Resource/Enums/VTexFlags
        {
            UNKNOWN = 0,
            DXT1 = 1,
            DXT5 = 2,
            I8 = 3,
            RGBA8888 = 4,
            R16 = 5,
            RG1616 = 6,
            RGBA16161616 = 7,
            R16F = 8,
            RG1616F = 9,
            RGBA16161616F = 10,
            R32F = 11,
            RG3232F = 12,
            RGB323232F = 13,
            RGBA32323232F = 14,
            JPEG_RGBA8888 = 15,
            PNG_RGBA8888 = 16,
            JPEG_DXT5 = 17,
            PNG_DXT5 = 18,
            BC6H = 19,
            BC7 = 20,
            ATI2N = 21,
            IA88 = 22,
            ETC2 = 23,
            ETC2_EAC = 24,
            R11_EAC = 25,
            RG11_EAC = 26,
            ATI1N = 27,
            BGRA8888 = 28,
        }

        public BinaryReader Reader { get; private set; }
        long DataOffset;
        public ushort Version { get; private set; }
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public ushort Depth { get; private set; }
        public float[] Reflectivity { get; private set; }
        public VTexFlags Flags { get; private set; }
        public VTexFormat Format { get; private set; }
        public byte NumMipMaps { get; private set; }
        public uint Picmip0Res { get; private set; }
        public Dictionary<VTexExtraData, byte[]> ExtraData { get; private set; } = new Dictionary<VTexExtraData, byte[]>();
        public ushort NonPow2Width { get; private set; }
        public ushort NonPow2Height { get; private set; }

        int[] CompressedMips;
        bool IsActuallyCompressedMips;
        float[] RadianceCoefficients;

        public ushort ActualWidth => NonPow2Width > 0 ? NonPow2Width : Width;
        public ushort ActualHeight => NonPow2Height > 0 ? NonPow2Height : Height;

        #region ITextureInfo

        (VTexFormat type, object gl, object vulken, object unity, object unreal) TexFormat;
        byte[] Bytes;
        Range[] Mips;

        IDictionary<string, object> ITexture.Data => null;
        int ITexture.Width => Width;
        int ITexture.Height => Height;
        int ITexture.Depth => Depth;
        int ITexture.MipMaps => NumMipMaps;
        TextureFlags ITexture.Flags => (TextureFlags)Flags;

        void ITexture.Select(int id) { }
        byte[] ITexture.Begin(int platform, out object format, out Range[] mips)
        {
            Reader.BaseStream.Position = Offset + Size;

            using (var b = new MemoryStream())
            {
                Mips = new Range[NumMipMaps];
                var lastLength = 0;
                for (var i = NumMipMaps - 1; i >= 0; i--)
                {
                    b.Write(ReadOne(i));
                    Mips[i] = new Range(lastLength, (int)b.Length);
                    lastLength = (int)b.Length;
                }
                Bytes = b.ToArray();
            }

            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => TexFormat.gl,
                Platform.Type.Unity => TexFormat.unity,
                Platform.Type.Unreal => TexFormat.unreal,
                Platform.Type.Vulken => TexFormat.vulken,
                Platform.Type.StereoKit => throw new NotImplementedException("StereoKit"),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            mips = Mips;
            return Bytes;
        }
        void ITexture.End() { }

        #endregion

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            Reader = r;
            Version = r.ReadUInt16();
            if (Version != 1) throw new FormatException($"Unknown vtex version. ({Version} != expected 1)");
            Flags = (VTexFlags)r.ReadUInt16();
            Reflectivity = new[] { r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle() };
            Width = r.ReadUInt16();
            Height = r.ReadUInt16();
            Depth = r.ReadUInt16();
            NonPow2Width = 0;
            NonPow2Height = 0;
            Format = (VTexFormat)r.ReadByte();
            NumMipMaps = r.ReadByte();
            Picmip0Res = r.ReadUInt32();
            var extraDataOffset = r.ReadUInt32();
            var extraDataCount = r.ReadUInt32();
            if (extraDataCount > 0)
            {
                r.Skip(extraDataOffset - 8); // 8 is 2 uint32s we just read
                for (var i = 0; i < extraDataCount; i++)
                {
                    var type = (VTexExtraData)r.ReadUInt32();
                    var offset = r.ReadUInt32() - 8;
                    var size = r.ReadUInt32();
                    r.Peek(z =>
                    {
                        z.Skip(offset);
                        ExtraData.Add(type, r.ReadBytes((int)size));
                        z.Skip(-size);
                        if (type == VTexExtraData.FILL_TO_POWER_OF_TWO)
                        {
                            z.ReadUInt16();
                            var nw = z.ReadUInt16();
                            var nh = z.ReadUInt16();
                            if (nw > 0 && nh > 0 && Width >= nw && Height >= nh)
                            {
                                NonPow2Width = nw;
                                NonPow2Height = nh;
                            }
                        }
                        else if (type == VTexExtraData.COMPRESSED_MIP_SIZE)
                        {
                            var int1 = z.ReadUInt32(); // 1?
                            var mipsOffset = z.ReadUInt32();
                            var mips = z.ReadUInt32();
                            if (int1 != 1 && int1 != 0) throw new FormatException($"int1 got: {int1}");
                            IsActuallyCompressedMips = int1 == 1; // TODO: Verify whether this int is the one that actually controls compression
                            r.Skip(mipsOffset - 8);
                            CompressedMips = z.ReadTArray<int>(sizeof(int), (int)mips);
                        }
                        else if (type == VTexExtraData.CUBEMAP_RADIANCE_SH)
                        {
                            var coeffsOffset = r.ReadUInt32();
                            var coeffs = r.ReadUInt32();
                            r.Skip(coeffsOffset - 8);
                            RadianceCoefficients = z.ReadTArray<float>(sizeof(float), (int)coeffs); // Spherical Harmonics
                        }
                    });
                }
            }
            DataOffset = Offset + Size;

            TexFormat = Format switch
            {
                DXT1 => (DXT1, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureUnityFormat.DXT1, TextureUnrealFormat.DXT1),
                //DXT3 => (DXT3, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureUnityFormat.DXT3_POLYFILL, TextureUnrealFormat.DXT3),
                DXT5 => (DXT5, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureUnityFormat.DXT5, TextureUnrealFormat.DXT5),
                ETC2 => (ETC2, TextureGLFormat.CompressedRgb8Etc2, TextureGLFormat.CompressedRgb8Etc2, TextureUnityFormat.ETC2_RGBA8Crunched, TextureUnrealFormat.ETC2RGB),
                ETC2_EAC => (ETC2_EAC, TextureGLFormat.CompressedRgba8Etc2Eac, TextureGLFormat.CompressedRgba8Etc2Eac, TextureUnityFormat.Unknown, TextureUnrealFormat.Unknown),
                ATI1N => (ATI1N, TextureGLFormat.CompressedRedRgtc1, TextureGLFormat.CompressedRedRgtc1, TextureUnityFormat.BC4, TextureUnrealFormat.BC4),
                ATI2N => (ATI2N, TextureGLFormat.CompressedRgRgtc2, TextureGLFormat.CompressedRgRgtc2, TextureUnityFormat.BC5, TextureUnrealFormat.BC5),
                BC6H => (BC6H, TextureGLFormat.CompressedRgbBptcUnsignedFloat, TextureGLFormat.CompressedRgbBptcUnsignedFloat, TextureUnityFormat.BC6H, TextureUnrealFormat.BC6H),
                BC7 => (BC7, TextureGLFormat.CompressedRgbaBptcUnorm, TextureGLFormat.CompressedRgbaBptcUnorm, TextureUnityFormat.BC7, TextureUnrealFormat.BC7),
                RGBA8888 => (RGBA8888, (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGBA32, TextureUnrealFormat.R8G8B8A8),
                RGBA16161616F => (RGBA16161616F, (TextureGLFormat.Rgba16f, TextureGLPixelFormat.Rgba, TextureGLPixelType.Float), (TextureGLFormat.Rgba16f, TextureGLPixelFormat.Rgba, TextureGLPixelType.Float), TextureUnityFormat.RGBAFloat, TextureUnrealFormat.FloatRGBA),
                I8 => (I8, TextureGLFormat.Intensity8, TextureGLFormat.Intensity8, TextureUnityFormat.Unknown, TextureUnityFormat.Unknown), //(TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte)
                R16 => (R16, (TextureGLFormat.R16, TextureGLPixelFormat.Red, TextureGLPixelType.UnsignedShort), (TextureGLFormat.R16, TextureGLPixelFormat.Red, TextureGLPixelType.UnsignedShort), TextureUnityFormat.R16, TextureUnrealFormat.R16UInt),
                R16F => (R16F, (TextureGLFormat.R16f, TextureGLPixelFormat.Red, TextureGLPixelType.Float), (TextureGLFormat.R16f, TextureGLPixelFormat.Red, TextureGLPixelType.Float), TextureUnityFormat.RFloat, TextureUnrealFormat.R16F),
                RG1616 => (RG1616, (TextureGLFormat.Rg16, TextureGLPixelFormat.Rg, TextureGLPixelType.UnsignedShort), (TextureGLFormat.Rg16, TextureGLPixelFormat.Rg, TextureGLPixelType.UnsignedShort), TextureUnityFormat.RG16, TextureUnrealFormat.R16G16UInt),
                RG1616F => (RG1616F, (TextureGLFormat.Rg16f, TextureGLPixelFormat.Rg, TextureGLPixelType.Float), (TextureGLFormat.Rg16f, TextureGLPixelFormat.Rg, TextureGLPixelType.Float), TextureUnityFormat.RGFloat, TextureUnrealFormat.R16G16UInt),
                _ => (Format, null, null, null, null),
            };
        }

        public byte[] ReadOne(int index)
        {
            var uncompressedSize = TextureHelper.GetMipmapTrueDataSize(TexFormat.gl, Width, Height, Depth, index);
            if (!IsActuallyCompressedMips) return Reader.ReadBytes(uncompressedSize);
            var compressedSize = CompressedMips[index];
            if (compressedSize >= uncompressedSize) return Reader.ReadBytes(uncompressedSize);
            return Reader.DecompressLz4(compressedSize, uncompressedSize);
        }

        public TextureSequences GetSpriteSheetData()
        {
            if (!ExtraData.TryGetValue(VTexExtraData.SHEET, out var bytes)) return null;
            var sequences = new TextureSequences();
            using var r = new BinaryReader(new MemoryStream(bytes));
            var version = r.ReadUInt32();
            if (version != 8) throw new ArgumentOutOfRangeException(nameof(version), $"Unknown version {version}");

            var numSequences = r.ReadUInt32();
            for (var i = 0; i < numSequences; i++)
            {
                var sequence = new TextureSequences.Sequence();
                var id = r.ReadUInt32();
                sequence.Clamp = r.ReadBoolean();
                sequence.AlphaCrop = r.ReadBoolean();
                sequence.NoColor = r.ReadBoolean();
                sequence.NoAlpha = r.ReadBoolean();
                var framesOffset = r.BaseStream.Position + r.ReadUInt32();
                var numFrames = r.ReadUInt32();
                sequence.FramesPerSecond = r.ReadSingle(); // Not too sure about this one
                var nameOffset = r.BaseStream.Position + r.ReadUInt32();
                var floatParamsOffset = r.BaseStream.Position + r.ReadUInt32();
                var floatParamsCount = r.ReadUInt32();
                r.Peek(z =>
                {
                    z.Seek(nameOffset);
                    sequence.Name = z.ReadZUTF8();

                    if (floatParamsCount > 0)
                    {
                        r.Seek(floatParamsOffset);
                        for (var p = 0; p < floatParamsCount; p++)
                        {
                            var floatParamNameOffset = r.BaseStream.Position + r.ReadUInt32();
                            var floatValue = r.ReadSingle();
                            var offsetNextParam = r.BaseStream.Position;
                            r.Seek(floatParamNameOffset);
                            var floatName = r.ReadZUTF8();
                            r.Seek(offsetNextParam);
                            sequence.FloatParams.Add(floatName, floatValue);
                        }
                    }

                    z.Seek(framesOffset);
                    sequence.Frames = new TextureSequences.Frame[numFrames];
                    for (var f = 0; f < numFrames; f++)
                    {
                        var displayTime = r.ReadSingle();
                        var imageOffset = r.BaseStream.Position + r.ReadUInt32();
                        var imageCount = r.ReadUInt32();
                        var originalOffset = r.BaseStream.Position;
                        var images = new TextureSequences.Image[imageCount];
                        sequence.Frames[f] = new TextureSequences.Frame
                        {
                            DisplayTime = displayTime,
                            Images = images,
                        };

                        r.Seek(imageOffset);
                        for (var i = 0; i < images.Length; i++)
                            images[i] = new TextureSequences.Image
                            {
                                CroppedMin = r.ReadVector2(),
                                CroppedMax = r.ReadVector2(),
                                UncroppedMin = r.ReadVector2(),
                                UncroppedMax = r.ReadVector2(),
                            };
                        r.Skip(originalOffset);
                    }
                });
                sequences.Add(sequence);
            }
            return sequences;
        }

        public override string ToString()
        {
            using var w = new IndentedTextWriter();
            w.WriteLine($"{"VTEX Version",-12} = {Version}");
            w.WriteLine($"{"Width",-12} = {Width}");
            w.WriteLine($"{"Height",-12} = {Height}");
            w.WriteLine($"{"Depth",-12} = {Depth}");
            w.WriteLine($"{"NonPow2W",-12} = {NonPow2Width}");
            w.WriteLine($"{"NonPow2H",-12} = {NonPow2Height}");
            w.WriteLine($"{"Reflectivity",-12} = ( {Reflectivity[0]:F6}, {Reflectivity[1]:F6}, {Reflectivity[2]:F6}, {Reflectivity[3]:F6} )");
            w.WriteLine($"{"NumMipMaps",-12} = {NumMipMaps}");
            w.WriteLine($"{"Picmip0Res",-12} = {Picmip0Res}");
            w.WriteLine($"{"Format",-12} = {(int)Format} (VTEX_FORMAT_{Format})");
            w.WriteLine($"{"Flags",-12} = 0x{(int)Flags:X8}");
            foreach (Enum value in Enum.GetValues(Flags.GetType())) if (Flags.HasFlag(value)) w.WriteLine($"{"",-12} | 0x{(Convert.ToInt32(value)):X8} = VTEX_FLAG_{value}");
            w.WriteLine($"{"Extra Data",-12} = {ExtraData.Count} entries:");
            var entry = 0;
            foreach (var b in ExtraData)
            {
                w.WriteLine($"{"",-12}   [ Entry {entry++}: VTEX_EXTRA_DATA_{b.Key} - {b.Value.Length} bytes ]");
                if (b.Key == VTexExtraData.COMPRESSED_MIP_SIZE && CompressedMips != null) w.WriteLine($"{"",-16}   [ {CompressedMips.Length} mips, sized: {string.Join(", ", CompressedMips)} ]");
                else if (b.Key == VTexExtraData.CUBEMAP_RADIANCE_SH && RadianceCoefficients != null) w.WriteLine($"{"",-16}   [ {RadianceCoefficients.Length} coefficients, sized: {string.Join(", ", RadianceCoefficients)} ]");
                else if (b.Key == VTexExtraData.SHEET && CompressedMips != null) w.WriteLine($"{"",-16}   [ {CompressedMips.Length} mips, sized: {string.Join(", ", CompressedMips)} ]");
            }
            //if (Format is not JPEG_DXT5 and not JPEG_RGBA8888 and not PNG_DXT5 and not PNG_RGBA8888)
            if (!(Format is JPEG_DXT5 || Format is JPEG_RGBA8888 || Format is PNG_DXT5 || Format is PNG_RGBA8888))
                for (var j = 0; j < NumMipMaps; j++) w.WriteLine($"Mip level {j} - buffer size: {TextureHelper.GetMipmapTrueDataSize(TexFormat.gl, Width, Height, Depth, j)}");
            return w.ToString();
        }

        public int CalculateTextureDataSize()
        {
            if (Format == PNG_DXT5 || Format == PNG_RGBA8888) return TextureHelper.CalculatePngSize(Reader, DataOffset);
            var bytes = 0;
            if (CompressedMips != null) bytes = CompressedMips.Sum();
            else for (var j = 0; j < NumMipMaps; j++) bytes += CalculateBufferSizeForMipLevel(j);
            return bytes;
        }

        int CalculateBufferSizeForMipLevel(int mipLevel)
        {
            var (bytesPerPixel, _) = TextureHelper.GetBlockSize(TexFormat.gl);
            var width = TextureHelper.MipLevelSize(Width, mipLevel);
            var height = TextureHelper.MipLevelSize(Height, mipLevel);
            var depth = TextureHelper.MipLevelSize(Depth, mipLevel);
            if ((Flags & VTexFlags.CUBE_TEXTURE) != 0) bytesPerPixel *= 6;
            if (Format == DXT1 || Format == DXT5 || Format == BC6H || Format == BC7 ||
                Format == ETC2 || Format == ETC2_EAC || Format == ATI1N)
            {
                var misalign = width % 4;
                if (misalign > 0) width += 4 - misalign;
                misalign = height % 4;
                if (misalign > 0) height += 4 - misalign;
                if (width < 4 && width > 0) width = 4;
                if (height < 4 && height > 0) height = 4;
                if (depth < 4 && depth > 1) depth = 4;
                var numBlocks = (width * height) >> 4;
                numBlocks *= depth;
                return numBlocks * bytesPerPixel;
            }
            return width * height * depth * bytesPerPixel;
        }
    }
}
