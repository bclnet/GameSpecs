using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using static OpenStack.Graphics.GXColor;

namespace OpenStack.Graphics
{
    /// <summary>
    /// Texture helper
    /// </summary>
    public static class TextureHelper
    {
        public static (int, bool) GetBlockSize(object format)
            => format is TextureGLFormat glFormat ? glFormat switch
            {
                TextureGLFormat.CompressedRgbaS3tcDxt1Ext => (8, true),
                TextureGLFormat.CompressedRgbaS3tcDxt5Ext => (6, true),
                TextureGLFormat.Rgba8 => (4, false),
                TextureGLFormat.R16 => (2, false),
                //TextureGLFormat.RG1616 => (4, false),
                TextureGLFormat.Rgba16f => (8, false),
                TextureGLFormat.R16f => (2, false),
                //TextureGLFormat.RG1616F => (4, false),
                //TextureGLFormat.RGBA16161616F => (8, false),
                //TextureGLFormat.R32F => (4, false),
                //TextureGLFormat.RG3232F => (8, false),
                //TextureGLFormat.RGB323232F => (12, false),
                //TextureGLFormat.RGBA32323232F => (16, false),
                TextureGLFormat.CompressedRgbBptcUnsignedFloat => (16, true),
                TextureGLFormat.CompressedRgbaBptcUnorm => (16, true),
                TextureGLFormat.Intensity8 => (2, false),
                TextureGLFormat.CompressedRgb8Etc2 => (8, true),
                TextureGLFormat.CompressedRgba8Etc2Eac => (16, true),
                //TextureGLFormat.BGRA8888 => (4, false),
                TextureGLFormat.CompressedRedRgtc1 => (8, true),
                _ => (1, false),
            }
            : format is ValueTuple<TextureGLFormat, TextureGLPixelFormat, TextureGLPixelType> glPixelFormat ? GetBlockSize(glPixelFormat.Item1)
            : throw new ArgumentOutOfRangeException(nameof(format), $"{format}");

        public static int GetMipmapCount(int width, int height)
        {
            Debug.Assert(width > 0 && height > 0);
            var longerLength = Math.Max(width, height);
            var mipMapCount = 0;
            var currentLongerLength = longerLength;
            while (currentLongerLength > 0) { mipMapCount++; currentLongerLength /= 2; }
            return mipMapCount;
        }

        public static int GetMipmapDataSize(int width, int height, int bytesPerPixel)
        {
            Debug.Assert(width > 0 && height > 0 && bytesPerPixel > 0);
            var dataSize = 0;
            var currentWidth = width;
            var currentHeight = height;
            while (true)
            {
                dataSize += currentWidth * currentHeight * bytesPerPixel;
                if (currentWidth == 1 && currentHeight == 1) break;
                currentWidth = currentWidth > 1 ? (currentWidth / 2) : currentWidth;
                currentHeight = currentHeight > 1 ? (currentHeight / 2) : currentHeight;
            }
            return dataSize;
        }

        public static int GetMipmapTrueDataSize(object format, int width, int height, int depth, int mipLevel)
        {
            var (bytesPerPixel, compressed) = GetBlockSize(format);
            var currentWidth = width >> mipLevel;
            var currentHeight = height >> mipLevel;
            var currentDepth = depth >> mipLevel;
            if (currentDepth < 1) currentDepth = 1;
            if (compressed)
            {
                var misalign = currentWidth % 4;
                if (misalign > 0) currentWidth += 4 - misalign;
                misalign = currentHeight % 4;
                if (misalign > 0) currentHeight += 4 - misalign;
                if (currentWidth < 4 && currentWidth > 0) currentWidth = 4;
                if (currentHeight < 4 && currentHeight > 0) currentHeight = 4;
                if (currentDepth < 4 && currentDepth > 1) currentDepth = 4;
                var numBlocks = (currentWidth * currentHeight) >> 4;
                numBlocks *= currentDepth;
                return numBlocks * bytesPerPixel;
            }
            return currentWidth * currentHeight * currentDepth * bytesPerPixel;
        }

        // TODO: Improve algorithm for images with odd dimensions.
        public static void Downscale4Component32BitPixelsX2(byte[] srcBytes, int srcStartIndex, int srcRowCount, int srcColumnCount, byte[] dstBytes, int dstStartIndex)
        {
            var bytesPerPixel = 4;
            var componentCount = 4;
            Debug.Assert(srcStartIndex >= 0 && srcRowCount >= 0 && srcColumnCount >= 0 && (srcStartIndex + (bytesPerPixel * srcRowCount * srcColumnCount)) <= srcBytes.Length);
            var dstRowCount = srcRowCount / 2;
            var dstColumnCount = srcColumnCount / 2;
            Debug.Assert(dstStartIndex >= 0 && (dstStartIndex + (bytesPerPixel * dstRowCount * dstColumnCount)) <= dstBytes.Length);
            for (var dstRowIndex = 0; dstRowIndex < dstRowCount; dstRowIndex++)
                for (var dstColumnIndex = 0; dstColumnIndex < dstColumnCount; dstColumnIndex++)
                {
                    var srcRowIndex0 = 2 * dstRowIndex;
                    var srcColumnIndex0 = 2 * dstColumnIndex;
                    var srcPixel0Index = (srcColumnCount * srcRowIndex0) + srcColumnIndex0;

                    var srcPixelStartIndices = new int[4];
                    srcPixelStartIndices[0] = srcStartIndex + (bytesPerPixel * srcPixel0Index); // top-left
                    srcPixelStartIndices[1] = srcPixelStartIndices[0] + bytesPerPixel; // top-right
                    srcPixelStartIndices[2] = srcPixelStartIndices[0] + (bytesPerPixel * srcColumnCount); // bottom-left
                    srcPixelStartIndices[3] = srcPixelStartIndices[2] + bytesPerPixel; // bottom-right

                    var dstPixelIndex = (dstColumnCount * dstRowIndex) + dstColumnIndex;
                    var dstPixelStartIndex = dstStartIndex + (bytesPerPixel * dstPixelIndex);
                    for (var componentIndex = 0; componentIndex < componentCount; componentIndex++)
                    {
                        var averageComponent = 0F;
                        for (var srcPixelIndex = 0; srcPixelIndex < srcPixelStartIndices.Length; srcPixelIndex++) averageComponent += srcBytes[srcPixelStartIndices[srcPixelIndex] + componentIndex];
                        averageComponent /= srcPixelStartIndices.Length;
                        dstBytes[dstPixelStartIndex + componentIndex] = (byte)Math.Round(averageComponent);
                    }
                }
        }

        //from:Resource/ResourceTypes/Texture
        public static int MipLevelSize(int size, int level) => Math.Max(size >>= level, 1);

        //from:Resource/ResourceTypes/Texture
        public static int CalculatePngSize(BinaryReader r, long dataOffset)
        {
            var size = 8; // PNG header
            var originalPosition = r.BaseStream.Position;
            r.BaseStream.Position = dataOffset;
            try
            {
                var pngHeaderA = r.ReadInt32(); if (pngHeaderA != 0x474E5089) throw new FormatException($"This is not PNG {pngHeaderA}");
                var pngHeaderB = r.ReadInt32(); if (pngHeaderB != 0x0A1A0A0D) throw new FormatException($"This is not PNG {pngHeaderB}");
                var chunk = 0;
                // Scan all the chunks until IEND
                do
                {
                    // Integers in png are big endian
                    var number = r.ReadBytes(sizeof(uint));
                    Array.Reverse(number);
                    size += BitConverter.ToInt32(number);
                    size += 12; // length + chunk type + crc
                    chunk = r.ReadInt32();
                    r.BaseStream.Position = dataOffset + size;
                }
                while (chunk != 0x444E4549);
            }
            finally { r.BaseStream.Position = originalPosition; }
            return size;
        }
    }
}