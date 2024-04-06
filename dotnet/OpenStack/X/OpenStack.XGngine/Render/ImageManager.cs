using System.Collections.Generic;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public partial class ImageManager
    {
        static string[] ImageFilter = new[] {
            "GL_LINEAR_MIPMAP_NEAREST",
            "GL_LINEAR_MIPMAP_LINEAR",
            "GL_NEAREST",
            "GL_LINEAR",
            "GL_NEAREST_MIPMAP_NEAREST",
            "GL_NEAREST_MIPMAP_LINEAR",
            null
        };

        //void Init() => throw new NotImplementedException();
        //void Shutdown() => throw new NotImplementedException();

        // If the exact combination of parameters has been asked for already, an existing image will be returned, otherwise a new image will be created.
        // Be careful not to use the same image file with different filter / repeat / etc parameters if possible, because it will cause a second copy to be loaded.
        // If the load fails for any reason, the image will be filled in with the default grid pattern.
        // Will automatically resample non-power-of-two images and execute image programs if needed.
        //public Image ImageFromFile(string name, Image.TF filter, bool allowDownSize, Image.TR repeat, Image.TD depth, Image.CF cubeMap = Image.CF._2D) => throw new NotImplementedException();

        // look for a loaded image, whatever the parameters
        //public Image GetImage(string name) => throw new NotImplementedException();

        // The callback will be issued immediately, and later if images are reloaded or vid_restart
        // The callback function should call one of the idImage::Generate* functions to fill in the data
        //public Image ImageFromFunction(string name, Action<Image> generatorFunction) => throw new NotImplementedException();

        // returns the number of bytes of image data bound in the previous frame
        //public int SumOfUsedImages() => throw new NotImplementedException();

        // called each frame to allow some cvars to automatically force changes
        //public void CheckCvars() => throw new NotImplementedException();

        // purges all the images before a vid_restart
        //public void PurgeAllImages() => throw new NotImplementedException();

        // reloads all apropriate images after a vid_restart
        //public void ReloadAllImages() => throw new NotImplementedException();

        // disable the active texture unit
        //public void BindNull() => throw new NotImplementedException();

        // Mark all file based images as currently unused, but don't free anything.  Calls to ImageFromFile() will
        // either mark the image as used, or create a new image without loading the actual data.
        // Called only by renderSystem::BeginLevelLoad
        //public void BeginLevelLoad() => throw new NotImplementedException();

        // Free all images marked as unused, and load all images that are necessary. This architecture prevents us from having the union of two level's
        // worth of data present at one time. Called only by renderSystem::EndLevelLoad
        //public void EndLevelLoad() => throw new NotImplementedException();

        //public void AddAllocList(Image image) => throw new NotImplementedException();
        //public void AddPurgeList(Image image) => throw new NotImplementedException();

        //public Image GetNextAllocImage() => throw new NotImplementedException();
        //public Image GetNextPurgeImage() => throw new NotImplementedException();

        // used to clear and then write the dds conversion batch file
        //public void StartBuild() => throw new NotImplementedException();
        //public void FinishBuild(bool removeDups = false) => throw new NotImplementedException();
        //public void AddDDSCommand(string cmd) => throw new NotImplementedException();

        //public void PrintMemInfo(MemInfo mi) => throw new NotImplementedException();

        // cvars
        public static CVar image_roundDown = new("image_roundDown", "1", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.BOOL, "round bad sizes down to nearest power of two");          // round bad sizes down to nearest power of two
        public static CVar image_colorMipLevels = new("image_colorMipLevels", "0", CVAR.RENDERER | CVAR.BOOL, "development aid to see texture mip usage");     // development aid to see texture mip usage
        public static CVar image_downSize = new("image_downSize", "0", CVAR.RENDERER | CVAR.ARCHIVE, "controls texture downsampling");               // controls texture downsampling
        public static CVar image_filter = new("image_filter", ImageFilter[1], CVAR.RENDERER | CVAR.ARCHIVE, "changes texture filtering on mipmapped images", ImageFilter, CmdArgs.ArgCompletion_String(ImageFilter));             // changes texture filtering on mipmapped images
        public static CVar image_anisotropy = new("image_anisotropy", "1", CVAR.RENDERER | CVAR.ARCHIVE, "set the maximum texture anisotropy if available");         // set the maximum texture anisotropy if available
        public static CVar image_writeNormalTGA = new("image_writeNormalTGA", "0", CVAR.RENDERER | CVAR.BOOL, "write .tgas of the final normal maps for debugging");     // debug tool to write out .tgas of the final normal maps
        public static CVar image_writeNormalTGAPalletized = new("image_writeNormalTGAPalletized", "0", CVAR.RENDERER | CVAR.BOOL, "write .tgas of the final palletized normal maps for debugging");       // debug tool to write out palletized versions of the final normal maps
        public static CVar image_writeTGA = new("image_writeTGA", "0", CVAR.RENDERER | CVAR.BOOL, "write .tgas of the non normal maps for debugging");               // debug tool to write out .tgas of the non normal maps
        public static CVar image_preload = new("image_preload", "1", CVAR.RENDERER | CVAR.BOOL | CVAR.ARCHIVE, "if 0, dynamically load all images");                // if 0, dynamically load all images
        public static CVar image_showBackgroundLoads = new("image_showBackgroundLoads", "0", CVAR.RENDERER | CVAR.BOOL, "1 = print number of outstanding background loads");    // 1 = print number of outstanding background loads
        public static CVar image_forceDownSize = new("image_forceDownSize", "0", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.BOOL, "");      // allows the ability to force a downsize
        public static CVar image_downSizeSpecular = new("image_downSizeSpecular", "1", CVAR.RENDERER | CVAR.ARCHIVE, "controls specular downsampling");       // downsize specular
        public static CVar image_downSizeSpecularLimit = new("image_downSizeSpecularLimit", "64", CVAR.RENDERER | CVAR.ARCHIVE, "controls specular downsampled limit"); // downsize specular limit
        public static CVar image_downSizeBump = new("image_downSizeBump", "1", CVAR.RENDERER | CVAR.ARCHIVE, "controls normal map downsampling");           // downsize bump maps
        public static CVar image_downSizeBumpLimit = new("image_downSizeBumpLimit", "256", CVAR.RENDERER | CVAR.ARCHIVE, "controls normal map downsample limit");  // downsize bump limit
        public static CVar image_downSizeLimit = new("image_downSizeLimit", "256", CVAR.RENDERER | CVAR.ARCHIVE, "controls diffuse map downsample limit");      // downsize diffuse limit

        // built-in images
        public Image defaultImage;
        public Image flatNormalMap;             // 128 128 255 in all pixels
        public Image ambientNormalMap;          // tr.ambientLightVector encoded in all pixels
        public Image rampImage;                 // 0-255 in RGBA in S
        public Image alphaRampImage;                // 0-255 in alpha, 255 in RGB
        public Image alphaNotchImage;           // 2x1 texture with just 1110 and 1111 with point sampling
        public Image whiteImage;                    // full of 0xff
        public Image blackImage;                    // full of 0x00
        public Image normalCubeMapImage;            // cube map to normalize STR into RGB
        public Image noFalloffImage;                // all 255, but zero clamped
        public Image quadraticImage;                //
        public Image fogImage;                  // increasing alpha is denser fog
        public Image fogEnterImage;             // adjust fogImage alpha based on terminator plane
        public Image cinematicImage;
        public Image scratchImage;
        public Image scratchImage2;
        public Image accumImage;
        public Image currentRenderImage;            // for SS_POST_PROCESS shaders
        public Image scratchCubeMapImage;
        public Image specularTableImage;            // 1D intensity texture with our specular function
        public Image specular2DTableImage;      // 2D intensity texture with our specular function with variable specularity
        public Image borderClampImage;          // white inside, black outside

        public Image hudImage;
        public Image pdaImage;

        //--------------------------------------------------------

        //public Image AllocImage(string name) => throw new NotImplementedException();
        //public void SetNormalPalette() => throw new NotImplementedException();
        //public void ChangeTextureFilter() => throw new NotImplementedException();

        public List<Image> images = new();
        public Dictionary<string, List<Image>> imagesByName = new(StringComparer.OrdinalIgnoreCase);
        public List<string> ddsList = new();
        public Dictionary<string, List<Image>> ddsHash = new(StringComparer.OrdinalIgnoreCase);

        public List<Image> imagesAlloc = new(); //List for the backend thread
        public List<Image> imagesPurge = new(); //List for the backend thread

        public bool insideLevelLoad;           // don't actually load images now

        public byte[] originalToCompressed = new byte[256]; // maps normal maps to 8 bit textures
        public byte[] compressedPalette = new byte[768];        // the palette that normal maps use

        // default filter modes for images
        public int textureMinFilter;
        public int textureMaxFilter;
        public float textureAnisotropy;

        public Image backgroundImageLoads;      // chain of images that have background file loads active
        public Image cacheLRU;                   // head/tail of doubly linked list
        public int totalCachedImageSize;       // for determining when something should be purged

        public int numActiveBackgroundImageLoads;
        public const int MAX_BACKGROUND_IMAGE_LOADS = 8;
    }
}