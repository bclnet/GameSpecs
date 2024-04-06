using System.NumericsX.OpenStack.Gngine.Framework;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public enum ImageState
    {
        IS_UNLOADED,   // no gl texture number
        IS_PARTIAL,        // has a texture number and the low mip levels loaded
        IS_LOADED      // has a texture number and the full mip hierarchy
    }

    public static partial class ImageX
    {
        const int MAX_TEXTURE_LEVELS = 14;

        // surface description flags
        const uint DDSF_CAPS = 0x00000001;
        const uint DDSF_HEIGHT = 0x00000002;
        const uint DDSF_WIDTH = 0x00000004;
        const uint DDSF_PITCH = 0x00000008;
        const uint DDSF_PIXELFORMAT = 0x00001000;
        const uint DDSF_MIPMAPCOUNT = 0x00020000;
        const uint DDSF_LINEARSIZE = 0x00080000;
        const uint DDSF_DEPTH = 0x00800000;

        // pixel format flags
        const uint DDSF_ALPHAPIXELS = 0x00000001;
        const uint DDSF_FOURCC = 0x00000004;
        const uint DDSF_RGB = 0x00000040;
        const uint DDSF_RGBA = 0x00000041;

        // our extended flags
        const uint DDSF_ID_INDEXCOLOR = 0x10000000;
        const uint DDSF_ID_MONOCHROME = 0x20000000;

        // dwCaps1 flags
        const uint DDSF_COMPLEX = 0x00000008;
        const uint DDSF_TEXTURE = 0x00001000;
        const uint DDSF_MIPMAP = 0x00400000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static uint DDS_MAKEFOURCC(uint a, uint b, uint c, uint d) => a | (b << 8) | (c << 16) | (d << 24);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DdsFilePixelFormat
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwFourCC;
        public uint dwRGBBitCount;
        public uint dwRBitMask;
        public uint dwGBitMask;
        public uint dwBBitMask;
        public uint dwABitMask;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct DdsFileHeader
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwHeight;
        public uint dwWidth;
        public uint dwPitchOrLinearSize;
        public uint dwDepth;
        public uint dwMipMapCount;
        public fixed uint dwReserved1[11];
        public DdsFilePixelFormat ddspf;
        public uint dwCaps1;
        public uint dwCaps2;
        public fixed uint dwReserved2[3];
    }

    public unsafe partial class Image
    {
        public enum TF
        {
            LINEAR,
            NEAREST,
            DEFAULT             // use the user-specified r_textureFilter
        }

        public enum TR
        {
            REPEAT,
            CLAMP,
            CLAMP_TO_BORDER,        // this should replace TR_CLAMP_TO_ZERO and TR_CLAMP_TO_ZERO_ALPHA, but I don't want to risk changing it right now
            CLAMP_TO_ZERO,      // guarantee 0,0,0,255 edge for projected textures, set AFTER image format selection
            CLAMP_TO_ZERO_ALPHA // guarantee 0 alpha edge for projected textures, set AFTER image format selection
        }

        // increasing numeric values imply more information is stored
        public enum TD
        {
            SPECULAR,            // may be compressed, and always zeros the alpha channel
            DIFFUSE,             // may be compressed
            DEFAULT,             // will use compressed formats when possible
            BUMP,                // may be compressed with 8 bit lookup
            HIGH_QUALITY         // either 32 bit or a component format, no loss at all
        }

        public enum TT
        {
            DISABLED,
            _2D,
            CUBIC,
            RECT
        }

        public enum CF
        {
            _2D,          // not a cube map
            NATIVE,      // _px, _nx, _py, etc, directly sent to GL
            CAMERA       // _forward, _back, etc, rotated and flipped as needed before sending to GL
        }

        internal const int MAX_IMAGE_NAME = 256;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image()
        {
            texnum = TEXTURE_NOT_LOADED;
            purgePending = false;
            type = TT.DISABLED;
            frameUsed = 0;
            classification = 0;
            backgroundLoadInProgress = false;
            bgl.opcode = DLTYPE.FILE;
            bgl.f = null;
            bglNext = null;
            imgName = string.Empty;
            generatorFunction = null;
            allowDownSize = false;
            filter = TF.DEFAULT;
            repeat = TR.REPEAT;
            depth = TD.DEFAULT;
            cubeFiles = CF._2D;
            referencedOutsideLevelLoad = false;
            levelLoadReferenced = false;
            defaulted = false;
            timestamp = DateTime.MinValue;
            bindCount = 0;
            uploadWidth = uploadHeight = uploadDepth = 0;
            internalFormat = 0;
            cacheUsagePrev = cacheUsageNext = null;
            hashNext = null;
            refCount = 0;
            cinematic = null;
            cinmaticNextTime = 0;
        }

        // Makes this image active on the current GL texture unit. automatically enables or disables cube mapping
        // May perform file loading if the image was not preloaded. May start a background image read.
        //public bool Bind() => throw new NotImplementedException();

        // for use with fragment programs, doesn't change any enable2D/3D/cube states
        //public void BindFragment() => throw new NotImplementedException();

        // deletes the texture object, but leaves the structure so it can be reloaded
        //public void PurgeImage() => throw new NotImplementedException();

        // used by callback functions to specify the actual data data goes from the bottom to the top line of the image, as OpenGL expects it
        // These perform an implicit Bind() on the current texture unit
        // FIXME: should we implement cinematics this way, instead of with explicit calls?
        //public void GenerateImage(byte* pic, int width, int height, TF filter, bool allowDownSize, TR repeat, TD depth) => throw new NotImplementedException();
        //public void GenerateCubeImage(byte[][] pic, int size, TF filter, bool allowDownSize, TD depth) => throw new NotImplementedException();

        //public void CopyFramebuffer(int x, int y, int width, int height, bool useOversizedBuffer) => throw new NotImplementedException();

        //public void CopyDepthbuffer(int x, int y, int width, int height) => throw new NotImplementedException();

        public void UploadScratch(void* pic, int width, int height) => throw new NotImplementedException();

        // just for resource tracking
        //public void SetClassification(int tag) => throw new NotImplementedException();

        // estimates size of the GL image based on dimensions and storage type
        //public int StorageSize => throw new NotImplementedException();

        // print a one line summary of the image
        //public void Print() => throw new NotImplementedException();

        // check for changed timestamp on disk and reload if necessary
        public void Reload(bool force)
        {
            // always regenerate functional images
            if (generatorFunction != null) { common.DPrintf($"regenerating {imgName}.\n"); generatorFunction(this); return; }

            // check file times
            if (!force)
            {
                TD depth = 0;
                DateTime current;
                var pic = byteX.empty;
                if (cubeFiles != CF._2D) R_LoadCubeImages(imgName, cubeFiles, null, out _, out current);
                // get the current values
                else R_LoadImageProgram(imgName, ref pic, out _, out _, out current, ref depth);
                if (current <= timestamp) return;
            }

            common.DPrintf($"reloading {imgName}.\n");

            //PurgeImage();
            globalImages.AddPurgeList(this);

            //ActuallyLoadImage( false );
            globalImages.AddAllocList(this);
        }

        public void AddReference() => refCount++;

        //public bool IsLoaded => throw new NotImplementedException();

        //==========================================================

        //public void GetDownsize(out int scaled_width, out int scaled_height) => throw new NotImplementedException();

        // fill with a grid pattern
        internal const int DEFAULT_SIZE = 16; // the default image will be grey with a white box outline to allow you to see the mapping coordinates on a surface
        public void MakeDefault()
        {
            int x, y;
            var data = new byte[DEFAULT_SIZE, DEFAULT_SIZE, 4];

            if (C.com_developer.Bool)
            {
                // grey center
                for (y = 0; y < DEFAULT_SIZE; y++)
                    for (x = 0; x < DEFAULT_SIZE; x++)
                    {
                        data[y, x, 0] = 32;
                        data[y, x, 1] = 32;
                        data[y, x, 2] = 32;
                        data[y, x, 3] = 255;
                    }

                // white border
                for (x = 0; x < DEFAULT_SIZE; x++)
                {
                    data[0, x, 0] = data[0, x, 1] = data[0, x, 2] = data[0, x, 3] = 255;
                    data[x, 0, 0] = data[x, 0, 1] = data[x, 0, 2] = data[x, 0, 3] = 255;
                    data[DEFAULT_SIZE - 1, x, 0] = data[DEFAULT_SIZE - 1, x, 1] = data[DEFAULT_SIZE - 1, x, 2] = data[DEFAULT_SIZE - 1, x, 3] = 255;
                    data[x, DEFAULT_SIZE - 1, 0] = data[x, DEFAULT_SIZE - 1, 1] = data[x, DEFAULT_SIZE - 1, 2] = data[x, DEFAULT_SIZE - 1, 3] = 255;
                }
            }
            else
                for (y = 0; y < DEFAULT_SIZE; y++)
                    for (x = 0; x < DEFAULT_SIZE; x++)
                    {
                        data[y, x, 0] = 0;
                        data[y, x, 1] = 0;
                        data[y, x, 2] = 0;
                        data[y, x, 3] = 0;
                    }

            fixed (byte* dataB = data) GenerateImage(dataB, DEFAULT_SIZE, DEFAULT_SIZE, TF.DEFAULT, true, TR.REPEAT, TD.DEFAULT);

            defaulted = true;
        }

        //public void SetImageFilterAndRepeat() => throw new NotImplementedException();
        //public void ActuallyLoadImage(bool fromBind) => throw new NotImplementedException();
        //public int BitsForInternalFormat(int internalFormat) => throw new NotImplementedException();
        public void UploadCompressedNormalMap(int width, int height, byte[] rgba, int mipLevel) => throw new NotImplementedException();
        //public void ImageProgramStringToCompressedFileName(string imageProg, out string fileName) => throw new NotImplementedException();
        //public int NumLevelsForImageSize(int width, int height) => throw new NotImplementedException();

        // data commonly accessed is grouped here
        public const uint TEXTURE_NOT_LOADED = uint.MaxValue;
        public uint texnum;                  // gl texture binding, will be TEXTURE_NOT_LOADED if not loaded
        public TT type;
        public int frameUsed;              // for texture usage in frame statistics
        public int bindCount;              // incremented each bind

        // background loading information
        public bool backgroundLoadInProgress;  // true if another thread is reading the complete d3t file
        public BackgroundDownload bgl;
        public Image bglNext;               // linked from tr.backgroundImageLoads

        // parameters that define this image
        public string imgName;              // game path, including extension (except for cube maps), may be an image program
        public Action<Image> generatorFunction; // NULL for files
        public bool allowDownSize;         // this also doubles as a don't-partially-load flag
        public TF filter;
        public TR repeat;
        public TD depth;
        public CF cubeFiles;              // determines the naming and flipping conventions for the six images

        public bool referencedOutsideLevelLoad;
        public bool levelLoadReferenced;   // for determining if it needs to be purged
        public bool defaulted;             // true if the default image was generated because a file couldn't be loaded
        public DateTime timestamp;                // the most recent of all images used in creation, for reloadImages command

        public int imageHash;              // for identical-image checking

        public int classification;         // just for resource profiling

        // data for listImages
        public int uploadWidth, uploadHeight, uploadDepth; // after power of two, downsample, and MAX_TEXTURE_SIZE
        public InternalFormat internalFormat;

        public Image cacheUsagePrev, cacheUsageNext;    // for dynamic cache purging of old images

        public Image hashNext;              // for hash chains to speed lookup

        public int refCount;               // overall ref count

        // If bound to a cinematic
        public Cinematic cinematic;
        public int cinmaticNextTime;

        public bool purgePending = false;
    }

    partial class R
    {
        // data is RGBA
        public static void WriteTGA(string filename, byte[] data, int width, int height, bool flipVertical = false) => throw new NotImplementedException();

        // data is an 8 bit index into palette, which is RGB (no A)
        public static void WritePalTGA(string filename, byte[] data, byte[] palette, int width, int height, bool flipVertical = false) => throw new NotImplementedException();

        // data is in top-to-bottom raster order unless flipVertical is set
    }

    partial class R
    {
        public static int _MakePowerOfTwo(int num) => throw new NotImplementedException();

        #region IMAGEPROCESS
        // FIXME: make an "imageBlock" type to hold byte*,width,height?

        public static byte[] Dropsample(byte[] i, int inwidth, int inheight, int outwidth, int outheight) => throw new NotImplementedException();
        public static byte[] ResampleTexture(byte[] i, int inwidth, int inheight, int outwidth, int outheight) => throw new NotImplementedException();
        public static byte[] MipMapWithAlphaSpecularity(byte[] i, int width, int height) => throw new NotImplementedException();
        public static byte[] MipMap(byte[] i, int width, int height, bool preserveBorder) => throw new NotImplementedException();
        public static byte[] MipMap3D(byte[] i, int width, int height, int depth, bool preserveBorder) => throw new NotImplementedException();

        // these operate in-place on the provided pixels
        public static void SetBorderTexels(byte[] inBase, int width, int height, byte[] border) => throw new NotImplementedException();
        public static void SetBorderTexels3D(byte[] inBase, int width, int height, int depth, byte[] border) => throw new NotImplementedException();
        public static void BlendOverTexture(byte[] data, int pixelCount, byte[] blend) => throw new NotImplementedException();
        public static void HorizontalFlip(byte[] data, int width, int height) => throw new NotImplementedException();
        public static void VerticalFlip(byte[] data, int width, int height) => throw new NotImplementedException();
        public static void RotatePic(byte[] data, int width) => throw new NotImplementedException();

        #endregion

        #region IMAGEFILES

        public static void LoadImage(string name, out byte[] pic, out int width, out int height, out DateTime timestamp, bool makePowerOf2) => throw new NotImplementedException();
        // pic is in top to bottom raster format
        public static bool LoadCubeImages(string cname, Image.CF extensions, out byte[] pic, out int size, out DateTime timestamp) => throw new NotImplementedException();

        #endregion

        #region IMAGEPROGRAM

        public static void LoadImageProgram(string name, out byte[] pic, out int width, out int height, out DateTime timestamp, out Image.TD depth) => throw new NotImplementedException();
        public static string ParsePastImageProgram(Lexer src) => throw new NotImplementedException();

        #endregion
    }
}