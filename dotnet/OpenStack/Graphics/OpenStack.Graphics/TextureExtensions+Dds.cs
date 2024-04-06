using OpenStack.Graphics.DirectX;
using System;
using System.IO;

namespace OpenStack.Graphics
{
    /// <summary>
    /// TextureExtensions
    /// </summary>
    static partial class TextureExtensions
    {
#if false
        /// <summary>
        /// Reads the quick DDS.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="r">The r.</param>
        /// <param name="length">The length.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// format - Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.
        /// or
        /// data - Invalid DDS DXTn texture. Unable to read
        /// </exception>
        public static EmptyTexture ReadQuickDds(this EmptyTexture source, BinaryReader r, int length, TextureUnityFormat format)
        {
            if (format != TextureUnityFormat.DXT1 && format != TextureUnityFormat.DXT5) throw new ArgumentOutOfRangeException(nameof(format), "Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");
            var data = r.ReadBytes(length);
            source.UnityFormat = format;
            var ddsSize = data[4];
            if (ddsSize != DDS_HEADER.SizeOf) throw new ArgumentOutOfRangeException(nameof(data), "Invalid DDS DXTn texture. Unable to read");
            source.Height = (data[13] << 8) | data[12];
            source.Width = (data[17] << 8) | data[16];
            var fileData = new byte[data.Length - DDS_HEADER.SizeOf];
            Buffer.BlockCopy(fileData, DDS_HEADER.SizeOf, fileData, 0, data.Length - DDS_HEADER.SizeOf);
            source.Data = fileData;
            return source;
        }
#endif

#if false
        /// <summary>
        /// Loads a DDS texture from an input stream.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="r">The r.</param>
        /// <param name="flipVertically">if set to <c>true</c> [flip vertically].</param>
        /// <returns></returns>
        /// <exception cref="FileFormatException">Invalid DDS file magic: \"{magic}\".</exception>
        public unsafe static EmptyTexture ReadDds(this EmptyTexture source, BinaryReader r, bool flipVertically = false)
        {
            // Check the magic string.
            var magic = r.ReadUInt32();
            if (magic != DDS_HEADER.DDS_) throw new FormatException($"Invalid DDS file magic: \"{magic}\".");
            var header = r.ReadT<DDS_HEADER>(DDS_HEADER.SizeOf);
            if (header.ddspf.dwFourCC == DDS_HEADER.DX10) r.ReadT<DDS_HEADER_DXT10>(DDS_HEADER_DXT10.SizeOf);
            header.Verify();
            header.ReadAndDecode(source, r);
            source.PostProcess(flipVertically);
            return source;
        }
#endif
    }
}