using System;
using System.Collections.Generic;
using System.IO;

namespace OpenStack.Graphics
{
    /// <summary>
    /// Stores information about a texture.
    /// </summary>
    public class EmptyTexture : Dictionary<string, object> //, IGetMetaInfo
    {
        public int Width, Height, Depth;
        public object UnityFormat;
        public object GLFormat;
        public TextureFlags Flags;
        public bool HasMipmaps;
        public ushort Mipmaps;
        public byte BytesPerPixel;
        public byte[] Data;
        public Action Decompress;
        public int[] CompressedSizeForMipLevel;

        public BinaryReader GetDecompressedBuffer(int offset)
           => throw new NotImplementedException();

        //BinaryReader GetDecompressedBuffer()
        //{
        //    if (!IsActuallyCompressedMips) return Reader;
        //    var outStream = new MemoryStream(GetDecompressedTextureAtMipLevel(MipmapLevelToExtract), false);
        //    return new BinaryReader(outStream); // TODO: dispose
        //}

        //public byte[] GetDecompressedTextureAtMipLevel(int mipLevel)
        //{
        //    var uncompressedSize = CalculateBufferSizeForMipLevel(mipLevel);
        //    if (!IsActuallyCompressedMips) return Reader.ReadBytes(uncompressedSize);
        //    var compressedSize = CompressedMips[mipLevel];
        //    if (compressedSize >= uncompressedSize) return Reader.ReadBytes(uncompressedSize);
        //    var input = Reader.ReadBytes(compressedSize);
        //    var output = new Span<byte>(new byte[uncompressedSize]);
        //    LZ4Codec.Decode(input, output);
        //    return output.ToArray();
        //}

        #region MipMap

        int GetDataOffsetForMip(int mipLevel)
        {
            if (Mipmaps < 2) return 0;

            var offset = 0;
            for (var j = Mipmaps - 1; j > mipLevel; j--)
                offset += CompressedSizeForMipLevel != null
                    ? CompressedSizeForMipLevel[j]
                    : TextureHelper.GetMipmapTrueDataSize(GLFormat, Width, Height, Depth, j) * (Flags.HasFlag(TextureFlags.CUBE_TEXTURE) ? 6 : 1);
            return offset;
        }

        object GetDataSpanForMip(int mipLevel)
        {
            return null;
            //var offset = GetDataOffsetForMip(mipLevel);
            //var dataSize = GetMipmapDataSize(Width, Height, Depth, GLFormat, mipLevel);
            //if (CompressedSizeForMipLevel == null) return new Span<byte>(Data, 10, 10);
            //var compressedSize = CompressedSizeForMipLevel[mipLevel];
            //if (compressedSize >= dataSize) return Reader.ReadBytes(dataSize);
            //var input = Reader.ReadBytes(compressedSize);
            //var output = new Span<byte>(new byte[dataSize]);
            //LZ4Codec.Decode(input, output);
            //return output.ToArray();
        }

        public byte[] GetTexture(int offset)
            => throw new NotImplementedException();

        public byte[] GetDecompressedTextureAtMipLevel(int offset, int v)
            => throw new NotImplementedException();

        internal static int GetMipmapDataSize(int dwWidth, int dwHeight, int v, object bytesPerPixel)
            => throw new NotImplementedException();

        #endregion
    }
}