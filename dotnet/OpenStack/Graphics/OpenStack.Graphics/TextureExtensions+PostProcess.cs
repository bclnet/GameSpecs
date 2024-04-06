using System;
using System.Diagnostics;

namespace OpenStack.Graphics
{
    /// <summary>
    /// TextureExtensions
    /// </summary>
    static partial class TextureExtensions
    {
        /// <summary>
        /// Generates missing mipmap levels for a DDS texture and optionally flips it.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="flipVertically">if set to <c>true</c> [flip vertically].</param>
        public static void PostProcess(this EmptyTexture source, bool flipVertically)
        {
            Debug.Assert(source.Width > 0 && source.Height > 0 && source.BytesPerPixel > 0 && source.Mipmaps > 0 && source.Data != null);
            // Flip mip-maps if necessary and generate missing mip-map levels.
            var mipMapLevelWidth = source.Width;
            var mipMapLevelHeight = source.Height;
            var mipMapLevelIndex = 0;
            var mipMapLevelDataOffset = 0;
            var bytesPerPixel = source.BytesPerPixel;
            var mipmaps = source.Mipmaps;
            var data = source.Data;
            var hasMipmaps = source.HasMipmaps;
            // While we haven't processed all of the mipmap levels we should process.
            while (mipMapLevelWidth > 1 || mipMapLevelHeight > 1)
            {
                var mipMapDataSize = mipMapLevelWidth * mipMapLevelHeight * bytesPerPixel;
                // If the DDS file contains the current mipmap level, flip it vertically if necessary.
                if (flipVertically && mipMapLevelIndex < mipmaps) MathX.Flip2DSubArrayVertically(data, mipMapLevelDataOffset, mipMapLevelHeight, mipMapLevelWidth * bytesPerPixel);
                // Break after optionally flipping the first mipmap level if the DDS texture doesn't have mipmaps.
                if (!hasMipmaps) break;
                // Generate the next mipmap level's data if the DDS file doesn't contain it.
                if (mipMapLevelIndex + 1 >= mipmaps) TextureHelper.Downscale4Component32BitPixelsX2(data, mipMapLevelDataOffset, mipMapLevelHeight, mipMapLevelWidth, data, mipMapLevelDataOffset + mipMapDataSize);
                // Switch to the next mipmap level.
                mipMapLevelIndex++;
                mipMapLevelWidth = mipMapLevelWidth > 1 ? (mipMapLevelWidth / 2) : mipMapLevelWidth;
                mipMapLevelHeight = mipMapLevelHeight > 1 ? (mipMapLevelHeight / 2) : mipMapLevelHeight;
                mipMapLevelDataOffset += mipMapDataSize;
            }
        }
    }
}