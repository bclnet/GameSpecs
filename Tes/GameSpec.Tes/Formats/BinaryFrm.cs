using GameSpec.Formats;
using GameSpec.Metadata;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Tes.Formats
{
    public class BinaryFrm : IGetMetadataInfo, ITexture, ITextureMultiple
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryFrm(r, f, s));

        // Header
        #region Header
        // https://falloutmods.fandom.com/wiki/FRM_File_Format

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        public unsafe struct FrmHeader
        {
            internal static string Endian = "B4B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B4B4B4B4B4B4B4";
            public uint Version;                            // Version
            public ushort Fps;                              // FPS
            public ushort ActionFrame;                      // Action frame
            public ushort FramesPerDirection;               // Number of frames per direction
            public fixed short PixelShiftX[6];              // Pixel shift in the x direction, of frames with orientation N
            public fixed short PixelShiftY[6];              // Pixel shift in the y direction, of frames with orientation N
            public fixed uint FrameOffset[6];               // Offset of first frame in orientation N from beginning of frame area
            public uint SizeOfFrame;                        // Size of frame data
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        public struct FrmFrame
        {
            internal static string Endian = "B2B2B4B2B2";
            public ushort Width;                            // FRAME-0-WIDTH: Width of frame 0 
            public ushort Height;                           // FRAME-0-HEIGHT: Height of frame 0
            public uint Size;                               // FRAME-0-SIZE: Number of pixels for frame 0
            public short PixelShiftX;                       // Pixel shift in x direction of frame 0
            public short PixelShiftY;                       // Pixel shift in y direction of frame 0
        }

        #endregion

        static BinaryPal DefaultPallet;

        async Task<BinaryPal> GetPalletObjAsync(string path, BinaryPakManyFile s)
        {
            var palletPath = $"{path[..^4]}.PAL";
            if (s.Contains(palletPath))
                return await s.LoadFileObjectAsync<BinaryPal>(palletPath);
            if (DefaultPallet == null && s.Contains("COLOR.PAL"))
            {
                DefaultPallet ??= await s.LoadFileObjectAsync<BinaryPal>("COLOR.PAL");
                DefaultPallet.SetColors();
            }
            return DefaultPallet;
        }

        public unsafe BinaryFrm(BinaryReader r, FileMetadata f, PakFile s)
        {
            if (!(s is BinaryPakManyFile ms)) throw new NotSupportedException();
            var pallet = GetPalletObjAsync(f.Path, ms).Result ?? throw new Exception("No pallet found");
            var rgba32 = pallet.Rgba32;

            // parse header
            var header = r.ReadTE<FrmHeader>(sizeof(FrmHeader), FrmHeader.Endian);
            var frames = new List<(FrmFrame f, byte[] b)>();
            var stream = r.BaseStream;
            for (var i = 0; i < 6 * header.FramesPerDirection && stream.Position < stream.Length; i++)
            {
                var frameOffset = Header.FrameOffset[i];
                var frame = r.ReadTE<FrmFrame>(sizeof(FrmFrame), FrmFrame.Endian);
                var data = r.ReadBytes((int)frame.Size);
                var image = new byte[frame.Width * frame.Height * 4];
                fixed (byte* image_ = image)
                {
                    var _ = image_;
                    for (var j = 0; j < data.Length; j++, _ += 4) *(uint*)_ = rgba32[data[j]];
                }
                frames.Add((f: frame, b: image));
            }
            Header = header;
            Frames = frames.ToArray();
            Format = ((TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGBA32, TextureUnrealFormat.R8G8B8A8);

            // select a frame
            FrameSelect(0);
        }

        public FrmHeader Header;
        public (FrmFrame f, byte[] b)[] Frames;
        byte[] Bytes;
        (object gl, object vulken, object unity, object unreal) Format;
        public IDictionary<string, object> Data => null;
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int Depth => 0;
        public int MipMaps => 1;
        public TextureFlags Flags => 0;

        public unsafe byte[] Begin(int platform, out object format, out Range[] ranges)
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
            ranges = null;
            return Bytes;
        }
        public void End() { }

        public int Fps => Header.Fps;
        public int FrameMaxIndex => Frames.Length == 1 ? 1 : Header.FramesPerDirection;
        public void FrameSelect(int index)
        {
            Bytes = Frames[index].b;
            Width = Frames[index].f.Width;
            Height = Frames[index].f.Height;
        }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetadataInfo($"{nameof(BinaryFrm)}", items: new List<MetadataInfo> {
                new MetadataInfo($"Frames: {Frames.Length}"),
                new MetadataInfo($"Width: {Width}"),
                new MetadataInfo($"Height: {Height}"),
            })
        };
    }
}
