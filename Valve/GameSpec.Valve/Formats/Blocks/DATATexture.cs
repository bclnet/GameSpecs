using GameSpec.Formats;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static OpenStack.Debug;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATATexture : DATA, ITextureInfo
    {
        [Flags]
        public enum VTexFlags
        {
            SUGGEST_CLAMPS = 0x00000001,
            SUGGEST_CLAMPT = 0x00000002,
            SUGGEST_CLAMPU = 0x00000004,
            NO_LOD = 0x00000008,
            CUBE_TEXTURE = 0x00000010,
            VOLUME_TEXTURE = 0x00000020,
            TEXTURE_ARRAY = 0x00000040,
        }

        public enum VTexExtraData
        {
            UNKNOWN = 0,
            FALLBACK_BITS = 1,
            SHEET = 2,
            FILL_TO_POWER_OF_TWO = 3,
            COMPRESSED_MIP_SIZE = 4,
        }

        public BinaryReader Reader { get; private set; }
        public ushort Version { get; private set; }
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public ushort Depth { get; private set; }
        public float[] Reflectivity { get; private set; }
        public VTexFlags Flags { get; private set; }
        public TextureGLFormat Format { get; private set; }
        public byte NumMipMaps { get; private set; }
        public uint Picmip0Res { get; private set; }
        public Dictionary<VTexExtraData, byte[]> ExtraData { get; private set; } = new Dictionary<VTexExtraData, byte[]>();
        public ushort NonPow2Width { get; private set; }
        public ushort NonPow2Height { get; private set; }

        int[] CompressedMips;
        bool IsActuallyCompressedMips;
        long DataOffset;

        #region ITextureInfo

        IDictionary<string, object> ITextureInfo.Data => null;
        int ITextureInfo.Width => Width;
        int ITextureInfo.Height => Height;
        int ITextureInfo.Depth => Depth;
        TextureFlags ITextureInfo.Flags => (TextureFlags)Flags;
        object ITextureInfo.UnityFormat => throw new NotImplementedException();
        object ITextureInfo.GLFormat => Format;
        int ITextureInfo.NumMipMaps => NumMipMaps;

        void ITextureInfo.MoveToData() => Reader.BaseStream.Position = Offset + Size;

        byte[] ITextureInfo.this[int index]
        {
            get
            {
                var uncompressedSize = Format.GetMipMapTrueDataSize(this, index);
                if (!IsActuallyCompressedMips) return Reader.ReadBytes(uncompressedSize);
                var compressedSize = CompressedMips[index];
                if (compressedSize >= uncompressedSize) return Reader.ReadBytes(uncompressedSize);
                return Reader.DecompressLz4(compressedSize, uncompressedSize);
            }
            set => throw new NotImplementedException();
        }

        #endregion

        public enum ValveTextureFormat : byte
        {
#pragma warning disable 1591
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
#pragma warning restore 1591
        }

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            static TextureGLFormat MapFormat(byte format)
                => (ValveTextureFormat)format switch
                {
                    ValveTextureFormat.DXT1 => TextureGLFormat.CompressedRgbaS3tcDxt1Ext,
                    //TextureFormat.DXT3 => TextureGLFormat.CompressedRgbaS3tcDxt3Ext,
                    ValveTextureFormat.DXT5 => TextureGLFormat.CompressedRgbaS3tcDxt5Ext,
                    ValveTextureFormat.ETC2 => TextureGLFormat.CompressedRgb8Etc2,
                    ValveTextureFormat.ETC2_EAC => TextureGLFormat.CompressedRgba8Etc2Eac,
                    ValveTextureFormat.ATI1N => TextureGLFormat.CompressedRedRgtc1,
                    ValveTextureFormat.ATI2N => TextureGLFormat.CompressedRgRgtc2,
                    ValveTextureFormat.BC6H => TextureGLFormat.CompressedRgbBptcUnsignedFloat,
                    ValveTextureFormat.BC7 => TextureGLFormat.CompressedRgbaBptcUnorm,
                    ValveTextureFormat.RGBA8888 => TextureGLFormat.Rgba8,
                    ValveTextureFormat.RGBA16161616F => TextureGLFormat.Rgba16f,
                    ValveTextureFormat.I8 => TextureGLFormat.Intensity8,
                    _ => 0,
                };

            r.Position(Offset);
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
            var format = r.ReadByte(); Format = MapFormat(format); if (Format == 0) Log($"Unsupported texture format: {format}");
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
                        if (type == VTexExtraData.FILL_TO_POWER_OF_TWO)
                        {
                            z.ReadUInt16();
                            var nw = z.ReadUInt16();
                            var nh = z.ReadUInt16();
                            if (nw > 0 && nh > 0 && Width >= nw && Height >= nh) { NonPow2Width = nw; NonPow2Height = nh; }
                            z.Skip(-6);
                        }
                        ExtraData.Add(type, r.ReadBytes((int)size));
                        if (type == VTexExtraData.COMPRESSED_MIP_SIZE)
                        {
                            z.Skip(-size);
                            var int1 = z.ReadUInt32(); // 1?
                            var int2 = z.ReadUInt32(); // 8?
                            var mips = z.ReadUInt32();
                            if (int1 != 1 && int1 != 0) throw new FormatException($"int1 got: {int1}");
                            if (int2 != 8) throw new FormatException($"int2 expected 8 but got: {int2}");
                            IsActuallyCompressedMips = int1 == 1; // TODO: Verify whether this int is the one that actually controls compression
                            CompressedMips = z.ReadTArray<int>(sizeof(int), (int)mips);
                        }
                    });
                }
            }
            DataOffset = Offset + Size;
        }

        public TextureSequences GetSpriteSheetData()
        {
            if (!ExtraData.TryGetValue(VTexExtraData.SHEET, out var bytes)) return null;
            var sequences = new TextureSequences();
            using var s = new MemoryStream(bytes);
            using var r = new BinaryReader(s);
            var version = r.ReadUInt32();
            var numSequences = r.ReadUInt32();
            for (var i = 0; i < numSequences; i++)
            {
                var sequenceNumber = r.ReadUInt32();
                var unknown1 = r.ReadUInt32(); // 1?
                var unknown2 = r.ReadUInt32();
                var numFrames = r.ReadUInt32();
                var framesPerSecond = r.ReadSingle(); // Not too sure about this one
                var dataOffset = r.BaseStream.Position + r.ReadUInt32();
                var unknown4 = r.ReadUInt32(); // 0?
                var unknown5 = r.ReadUInt32(); // 0?
                var frames = r.Peek(z =>
                {
                    z.Position(dataOffset);
                    var sequenceName = z.ReadZUTF8();
                    var frameUnknown = z.ReadUInt16();
                    for (var j = 0; j < numFrames; j++)
                    {
                        var frameUnknown1 = z.ReadSingle();
                        var frameUnknown2 = z.ReadUInt32();
                        var frameUnknown3 = z.ReadSingle();
                    }
                    var fs = new TextureSequences.Sequence.Frame[numFrames];
                    for (var j = 0; j < numFrames; j++)
                        fs[j] = new TextureSequences.Sequence.Frame
                        {
                            StartMins = z.ReadVector2(),
                            StartMaxs = z.ReadVector2(),
                            EndMins = z.ReadVector2(),
                            EndMaxs = z.ReadVector2()
                        };
                    return fs;
                });
                sequences.Add(new TextureSequences.Sequence { Frames = frames, FramesPerSecond = framesPerSecond });
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
            w.WriteLine($"{"NumMips",-12} = {NumMipMaps}");
            w.WriteLine($"{"Picmip0Res",-12} = {Picmip0Res}");
            w.WriteLine($"{"Format",-12} = {(int)Format} (VTEX_FORMAT_{Format})");
            w.WriteLine($"{"Flags",-12} = 0x{((int)Flags):X8}");
            foreach (Enum value in Enum.GetValues(Flags.GetType())) if (Flags.HasFlag(value)) w.WriteLine($"{"",-12} | 0x{(Convert.ToInt32(value)):X8} = VTEX_FLAG_{value}");
            w.WriteLine($"{"Extra Data",-12} = {ExtraData.Count} entries:");
            var entry = 0;
            foreach (var b in ExtraData)
            {
                w.WriteLine($"{0,-12}   [ Entry {entry++}: VTEX_EXTRA_DATA_{b.Key} - {b.Value.Length} bytes ]");
                if (b.Key == VTexExtraData.COMPRESSED_MIP_SIZE) w.WriteLine($"{"",-16}   [ {CompressedMips.Length} mips, sized: {string.Join(", ", CompressedMips)} ]");
            }
            for (var j = 0; j < NumMipMaps; j++) w.WriteLine($"Mip level {j} - buffer size: {TextureInfo.GetMipmapTrueDataSize(Format, Width, Height, Depth, j)}");
            return w.ToString();
        }
    }
}
