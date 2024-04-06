namespace System.NumericsX.OpenStack.Gngine.Render
{
    public unsafe class Etc1Android
    {
        const int ETC1_ENCODED_BLOCK_SIZE = 8;
        const int ETC1_DECODED_BLOCK_SIZE = 48;

        public const uint GL_ETC1_RGB8_OES = 0x8D64;

        // Encode a block of pixels.
        //
        // pIn is a pointer to a ETC_DECODED_BLOCK_SIZE array of bytes that represent a 4 x 4 square of 3-byte pixels in form R, G, B. Byte (3 * (x + 4 * y) is the R
        // value of pixel (x, y).
        //
        // validPixelMask is a 16-bit mask where bit (1 << (x + y * 4)) indicates whether the corresponding (x,y) pixel is valid. Invalid pixel color values are ignored when compressing.
        //
        // pOut is an ETC1 compressed version of the data.
        public static void etc1_encode_block(byte[] pIn, uint validPixelMask, byte[] pOut) => throw new NotImplementedException();

        // Decode a block of pixels.
        //
        // pIn is an ETC1 compressed version of the data.
        //
        // pOut is a pointer to a ETC_DECODED_BLOCK_SIZE array of bytes that represent a 4 x 4 square of 3-byte pixels in form R, G, B. Byte (3 * (x + 4 * y) is the R
        // value of pixel (x, y).
        public static void etc1_decode_block(byte[] pIn, byte[] pOut) => throw new NotImplementedException();

        // Return the size of the encoded image data (does not include size of PKM header).
        public static uint etc1_get_encoded_data_size(uint width, uint height) => throw new NotImplementedException();

        // Encode an entire image.
        // pIn - pointer to the image data. Formatted such that pixel (x,y) is at pIn + pixelSize * x + stride * y;
        // pOut - pointer to encoded data. Must be large enough to store entire encoded image.
        // pixelSize can be 2 or 3. 2 is an GL_UNSIGNED_SHORT_5_6_5 image, 3 is a GL_BYTE RGB image.
        // returns non-zero if there is an error.
        public static int etc1_encode_image(byte* pIn, uint width, uint height, uint pixelSize, uint stride, byte* pOut) => throw new NotImplementedException();

        // Decode an entire image.
        // pIn - pointer to encoded data.
        // pOut - pointer to the image data. Will be written such that pixel (x,y) is at pIn + pixelSize * x + stride * y. Must be
        //        large enough to store entire image.
        // pixelSize can be 2 or 3. 2 is an GL_UNSIGNED_SHORT_5_6_5 image, 3 is a GL_BYTE RGB image.
        // returns non-zero if there is an error.
        public static int etc1_decode_image(byte* pIn, byte* pOut, uint width, uint height, uint pixelSize, uint stride) => throw new NotImplementedException();

        // Size of a PKM header, in bytes.

        const int ETC_PKM_HEADER_SIZE = 16;

        // Format a PKM header

        void etc1_pkm_format_header(byte[] pHeader, uint width, uint height) => throw new NotImplementedException();

        // Check if a PKM header is correctly formatted.

        bool etc1_pkm_is_valid(byte[] pHeader) => throw new NotImplementedException();

        // Read the image width from a PKM header

        uint etc1_pkm_get_width(byte[] pHeader) => throw new NotImplementedException();

        // Read the image height from a PKM header

        uint etc1_pkm_get_height(byte[] pHeader) => throw new NotImplementedException();
    }
}