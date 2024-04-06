using System.Collections.Generic;
using System.Linq;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.Image;
using static System.NumericsX.OpenStack.Gngine.Render.IRenderSystem;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class ImageManager
    {
        public enum IC
        {
            NPC,
            WEAPON,
            MONSTER,
            MODELGEOMETRY,
            ITEMS,
            MODELSOTHER,
            GUIS,
            WORLDGEOMETRY,
            OTHER,
            COUNT
        }

        public struct ImageClassificate
        {
            public ImageClassificate(string rootPath, string desc, IC type, int maxWidth, int maxHeight)
            {
                this.rootPath = rootPath;
                this.desc = desc;
                this.type = type;
                this.maxWidth = maxWidth;
                this.maxHeight = maxHeight;
            }
            public string rootPath;
            public string desc;
            public IC type;
            public int maxWidth;
            public int maxHeight;
        }

        internal static ImageClassificate[] IC_Info = new[] {
            new ImageClassificate("models/characters", "Characters", IC.NPC, 512, 512),
            new ImageClassificate("models/weapons", "Weapons", IC.WEAPON, 512, 512),
            new ImageClassificate("models/monsters", "Monsters", IC.MONSTER, 512, 512),
            new ImageClassificate("models/mapobjects", "Model Geometry", IC.MODELGEOMETRY, 512, 512),
            new ImageClassificate("models/items", "Items", IC.ITEMS, 512, 512),
            new ImageClassificate("models", "Other model textures", IC.MODELSOTHER, 512, 512),
            new ImageClassificate("guis/assets", "Guis", IC.GUIS, 256, 256),
            new ImageClassificate("textures", "World Geometry", IC.WORLDGEOMETRY, 256, 256),
            new ImageClassificate("", "Other", IC.OTHER, 256, 256)
        };

        internal static IC ClassifyImage(string name)
        {
            for (var i = 0; i < (int)IC.COUNT; i++) if (name.StartsWith(IC_Info[i].rootPath)) return IC_Info[i].type;
            return IC.OTHER;
        }

        // Creates a 0-255 ramp image
        static void R_RampImage(Image image)
        {
            var data = new byte[256, 4];
            for (var x = 0; x < 256; x++) data[x, 0] = data[x, 1] = data[x, 2] = data[x, 3] = (byte)x;

            fixed (byte* dataB = data) image.GenerateImage(dataB, 256, 1, TF.NEAREST, false, TR.CLAMP, TD.HIGH_QUALITY);
        }

        // Creates a ramp that matches our fudged specular calculation
        static void R_SpecularTableImage(Image image)
        {
            var data = new byte[256, 4];
            for (var x = 0; x < 256; x++)
            {
                var f = x / 255f;
#if false
                f = pow(f, 16);
#else
                // this is the behavior of the hacked up fragment programs that can't really do a power function
                f = (f - 0.75f) * 4;
                if (f < 0f) f = 0f;
                f *= f;
#endif
                var b = (byte)(f * 255);
                data[x, 0] = data[x, 1] = data[x, 2] = data[x, 3] = b;
            }

            fixed (byte* dataB = data) image.GenerateImage(dataB, 256, 1, TF.LINEAR, false, TR.CLAMP, TD.HIGH_QUALITY);
        }

        // Create a 2D table that calculates ( reflection dot , specularity )
        static void R_Specular2DTableImage(Image image)
        {
            var data = new byte[256, 256, 4];
            for (var x = 0; x < 256; x++)
            {
                var f = x / 255f;
                for (var y = 0; y < 256; y++)
                {
                    var b = (byte)(Math.Pow(f, y) * 255f);
                    // as soon as b equals zero all remaining values in this column are going to be zero we early out to avoid pow() underflows
                    if (b == 0) break;

                    data[y, x, 0] = data[y, x, 1] = data[y, x, 2] = data[y, x, 3] = b;
                }
            }

            fixed (byte* dataB = data) image.GenerateImage(dataB, 256, 256, TF.LINEAR, false, TR.CLAMP, TD.HIGH_QUALITY);
        }

        static void R_DefaultImage(Image image)
        {
            image.MakeDefault();
        }

        static void R_WhiteImage(Image image)
        {
            var data = new byte[DEFAULT_SIZE, DEFAULT_SIZE, 4];

            // solid white texture
            fixed (byte* dataB = data) { Unsafe.InitBlock(dataB, 255, (uint)data.Length); image.GenerateImage(dataB, DEFAULT_SIZE, DEFAULT_SIZE, TF.DEFAULT, false, TR.REPEAT, TD.DEFAULT); }
        }

        static void R_BlackImage(Image image)
        {
            var data = new byte[DEFAULT_SIZE, DEFAULT_SIZE, 4];

            // solid black texture
            fixed (byte* dataB = data) image.GenerateImage(dataB, DEFAULT_SIZE, DEFAULT_SIZE, TF.DEFAULT, false, TR.REPEAT, TD.DEFAULT);
        }

        // the size determines how far away from the edge the blocks start fading
        const int BORDER_CLAMP_SIZE = 32;
        static void R_BorderClampImage(Image image)
        {
            var data = new byte[BORDER_CLAMP_SIZE, BORDER_CLAMP_SIZE, 4];

            // solid white texture with a single pixel black border
            fixed (byte* dataB = data)
            {
                Unsafe.InitBlock(dataB, 255, (uint)data.Length);
                for (var i = 0; i < BORDER_CLAMP_SIZE; i++)
                    data[i, 0, 0] = data[i, 0, 1] = data[i, 0, 2] = data[i, 0, 3] =
                        data[i, BORDER_CLAMP_SIZE - 1, 0] = data[i, BORDER_CLAMP_SIZE - 1, 1] = data[i, BORDER_CLAMP_SIZE - 1, 2] = data[i, BORDER_CLAMP_SIZE - 1, 3] =
                        data[0, i, 0] = data[0, i, 1] = data[0, i, 2] = data[0, i, 3] =
                        data[BORDER_CLAMP_SIZE - 1, i, 0] = data[BORDER_CLAMP_SIZE - 1, i, 1] = data[BORDER_CLAMP_SIZE - 1, i, 2] = data[BORDER_CLAMP_SIZE - 1, i, 3] = 0;

                image.GenerateImage(dataB, BORDER_CLAMP_SIZE, BORDER_CLAMP_SIZE, TF.LINEAR, false, TR.CLAMP_TO_BORDER, TD.DEFAULT);
            }

            // can't call qglTexParameterfv yet
            if (!glConfig.isInitialized) return;

            // explicit zero border - Disabled for OES2
            //var color = stackalloc float[4];
            //color[0] = color[1] = color[2] = color[3] = 0;
            //qglTexParameterfv(GL_TEXTURE_2D, GL_TEXTURE_BORDER_COLOR, color);
        }

        static void R_RGBA8Image(Image image)
        {
            var data = new byte[DEFAULT_SIZE, DEFAULT_SIZE, 4];
            data[0, 0, 0] = 16;
            data[0, 0, 1] = 32;
            data[0, 0, 2] = 48;
            data[0, 0, 3] = 96; // 255

            fixed (byte* dataB = data) image.GenerateImage(dataB, DEFAULT_SIZE, DEFAULT_SIZE, TF.DEFAULT, false, TR.REPEAT, TD.HIGH_QUALITY);
        }

        // Koz begin
        // used for Hud and PDA surfaces in VR
        static void R_VRSurfaceImage(Image image)
        {
            var data = new byte[1024, 1024, 4];

            fixed (byte* dataB = data) image.GenerateImage(dataB, 1024, 1024, TF.DEFAULT, false, TR.CLAMP, TD.HIGH_QUALITY);
        }

        static void R_AlphaNotchImage(Image image)
        {
            var data = new byte[2, 4];

            // this is used for alpha test clip planes
            data[0, 0] = data[0, 1] = data[0, 2] = 255;
            data[0, 3] = 0;
            data[1, 0] = data[1, 1] = data[1, 2] = 255;
            data[1, 3] = 255;

            fixed (byte* dataB = data) image.GenerateImage(dataB, 2, 1, TF.NEAREST, false, TR.CLAMP, TD.HIGH_QUALITY);
        }

        static void R_FlatNormalImage(Image image)
        {
            var data = new byte[DEFAULT_SIZE, DEFAULT_SIZE, 4];

            const int red = 3;
            var alpha = red == 0 ? 3 : 0;
            // flat normal map for default bunp mapping
            for (var i = 0; i < 4; i++)
            {
                data[0, i, red] = 128;
                data[0, i, 1] = 128;
                data[0, i, 2] = 255;
                data[0, i, alpha] = 255;
            }
            fixed (byte* dataB = data) image.GenerateImage(dataB, 2, 2, TF.DEFAULT, true, TR.REPEAT, TD.HIGH_QUALITY);
        }

        static void R_AmbientNormalImage(Image image)
        {
            int i;
            var data = new byte[DEFAULT_SIZE, DEFAULT_SIZE, 4];

            const int red = 3;
            var alpha = red == 0 ? 3 : 0;
            // flat normal map for default bunp mapping
            for (i = 0; i < 4; i++)
            {
                data[0, i, red] = (byte)(255 * tr.ambientLightVector.x);
                data[0, i, 1] = (byte)(255 * tr.ambientLightVector.y);
                data[0, i, 2] = (byte)(255 * tr.ambientLightVector.z);
                data[0, i, alpha] = 255;
            }
            var pics = stackalloc byte*[6];
            fixed (byte* dataB = &data[0, 0, 0])
            {
                for (i = 0; i < 6; i++) pics[i] = dataB;
                // this must be a cube map for fragment programs to simply substitute for the normalization cube map
                image.GenerateCubeImage(pics, 2, TF.DEFAULT, true, TD.HIGH_QUALITY);
            }
        }

#if false
        static void CreateSquareLight()
        {
            int x, y, dx, dy, width, height; byte d; byte* buf;

            width = height = 128;

            buf = (byte*)R_StaticAlloc(128 * 128 * 4);

            for (x = 0; x < 128; x++)
            {
                if (x < 32) dx = 32 - x;
                else if (x > 96) dx = x - 96;
                else dx = 0;
                for (y = 0; y < 128; y++)
                {
                    if (y < 32) dy = 32 - y;
                    else if (y > 96) dy = y - 96;
                    else dy = 0;
                    d = (byte)MathX.Sqrt(dx * dx + dy * dy);
                    if (d > 32) d = 32;
                    d = (byte)(255 - d * 8);
                    if (d < 0) d = 0;
                    buf[(y * 128 + x) * 4 + 0] = buf[(y * 128 + x) * 4 + 1] = buf[(y * 128 + x) * 4 + 2] = d;
                    buf[(y * 128 + x) * 4 + 3] = 255;
                }
            }

            R_WriteTGA("lights/squarelight.tga", buf, width, height);

            R_StaticFree(buf);
        }

        static void CreateFlashOff()
        {
            int x, y, width, height; byte d; byte* buf;

            width = 256;
            height = 4;

            buf = (byte*)R_StaticAlloc(width * height * 4);

            for (x = 0; x < width; x++)
            {
                for (y = 0; y < height; y++)
                {
                    d = 255 - (x * 256 / width);
                    buf[(y * width + x) * 4 + 0] =
                        buf[(y * width + x) * 4 + 1] =
                            buf[(y * width + x) * 4 + 2] = d;
                    buf[(y * width + x) * 4 + 3] = 255;
                }
            }

            R_WriteTGA("lights/flashoff.tga", buf, width, height);

            R_StaticFree(buf);
        }
#endif

        static void CreatePitFogImage()
        {
            int i, j;
            var data = new byte[16, 16, 4];

            for (i = 0; i < 16; i++)
            {
                byte a;
#if false
                if (i > 14) a = 0;
                else
#endif
                {
                    a = (byte)(i * 255 / 15);
                    if (a > 255) a = 255;
                }

                for (j = 0; j < 16; j++)
                {
                    data[j, i, 0] = data[j, i, 1] = data[j, i, 2] = 255;
                    data[j, i, 3] = a;
                }
            }

            fixed (byte* dataB = &data[0, 0, 0]) R_WriteTGA("shapes/pitFalloff.tga", dataB, 16, 16);
        }

        static void CreatealphaSquareImage()
        {
            int i, j;
            var data = new byte[16, 16, 4];

            for (i = 0; i < 16; i++)
                for (j = 0; j < 16; j++)
                {
                    data[j, i, 0] = data[j, i, 1] = data[j, i, 2] = 255;
                    data[j, i, 3] = i == 0 || i == 15 || j == 0 || j == 15 ? (byte)0 : (byte)255;
                }

            fixed (byte* dataB = &data[0, 0, 0]) R_WriteTGA("shapes/alphaSquare.tga", dataB, 16, 16);
        }

        const int NORMAL_MAP_SIZE = 32;

        // Given a cube map face index, cube map size, and integer 2D face position, return the cooresponding normalized vector.
        static void getCubeVector(int i, int cubesize, int x, int y, float* vector)
        {
            float s, t, sc, tc, mag;

            s = ((float)x + 0.5f) / (float)cubesize;
            t = ((float)y + 0.5f) / (float)cubesize;
            sc = s * 2f - 1f;
            tc = t * 2f - 1f;

            switch (i)
            {
                case 0: vector[0] = 1f; vector[1] = -tc; vector[2] = -sc; break;
                case 1: vector[0] = -1f; vector[1] = -tc; vector[2] = sc; break;
                case 2: vector[0] = sc; vector[1] = 1f; vector[2] = tc; break;
                case 3: vector[0] = sc; vector[1] = -1f; vector[2] = -tc; break;
                case 4: vector[0] = sc; vector[1] = -tc; vector[2] = 1f; break;
                case 5: vector[0] = -sc; vector[1] = -tc; vector[2] = -1f; break;
                default: common.Error("getCubeVector: invalid cube map face index"); return;
            }

            mag = MathX.InvSqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
            vector[0] *= mag;
            vector[1] *= mag;
            vector[2] *= mag;
        }

        // Initialize a cube map texture object that generates RGB values that when expanded to a [-1,1] range in the register combiners
        // form a normalized vector matching the per-pixel vector used to access the cube map.
        static void makeNormalizeVectorCubeMap(Image image)
        {
            int i, x, y, size;
            var vector = stackalloc float[3];
            var pixels = stackalloc byte*[6];

            size = NORMAL_MAP_SIZE;
            var data = new byte[size * size * 4 * 6];
            fixed (byte* dataB = data)
            {
                pixels[0] = dataB;
                for (i = 0; i < 6; i++)
                {
                    pixels[i] = pixels[0] + i * size * size * 4;
                    for (y = 0; y < size; y++)
                        for (x = 0; x < size; x++)
                        {
                            getCubeVector(i, size, x, y, vector);
                            pixels[i][4 * (y * size + x) + 0] = (byte)(128 + 127 * vector[0]);
                            pixels[i][4 * (y * size + x) + 1] = (byte)(128 + 127 * vector[1]);
                            pixels[i][4 * (y * size + x) + 2] = (byte)(128 + 127 * vector[2]);
                            pixels[i][4 * (y * size + x) + 3] = 255;
                        }
                }

                image.GenerateCubeImage(pixels, size, TF.LINEAR, false, TD.HIGH_QUALITY);
            }
        }

        // This is a solid white texture that is zero clamped.
        static void R_CreateNoFalloffImage(Image image)
        {
            int x, y;
            var data = new byte[16, FALLOFF_TEXTURE_SIZE, 4];

            for (x = 1; x < FALLOFF_TEXTURE_SIZE - 1; x++)
                for (y = 1; y < 15; y++)
                {
                    data[y, x, 0] = 255;
                    data[y, x, 1] = 255;
                    data[y, x, 2] = 255;
                    data[y, x, 3] = 255;
                }
            fixed (byte* dataB = data) image.GenerateImage(dataB, FALLOFF_TEXTURE_SIZE, 16, TF.DEFAULT, false, TR.CLAMP_TO_ZERO, TD.HIGH_QUALITY);
        }

        // We calculate distance correctly in two planes, but the third will still be projection based
        const int FOG_SIZE = 128;
        void R_FogImage(Image image)
        {
            int i, x, y, b;
            var data = new byte[FOG_SIZE, FOG_SIZE, 4];
            var step = stackalloc float[256];

            var remaining = 1f;
            for (i = 0; i < 256; i++)
            {
                step[i] = remaining;
                remaining *= 0.982f;
            }

            for (x = 0; x < FOG_SIZE; x++)
                for (y = 0; y < FOG_SIZE; y++)
                {
                    var d = MathX.Sqrt((x - FOG_SIZE / 2) * (x - FOG_SIZE / 2) + (y - FOG_SIZE / 2) * (y - FOG_SIZE / 2));
                    d /= FOG_SIZE / 2 - 1;

                    b = (byte)(d * 255);
                    if (b <= 0) b = 0;
                    else if (b > 255) b = 255;
                    b = (byte)(255 * (1f - step[b]));
                    if (x == 0 || x == FOG_SIZE - 1 || y == 0 || y == FOG_SIZE - 1) b = 255; // avoid clamping issues
                    data[y, x, 0] = data[y, x, 1] = data[y, x, 2] = 255;
                    data[y, x, 3] = (byte)b;
                }

            fixed (byte* dataB = data) image.GenerateImage(dataB, FOG_SIZE, FOG_SIZE, TF.LINEAR, false, TR.CLAMP, TD.HIGH_QUALITY);
        }

        // Height values below zero are inside the fog volume
        const float RAMP_RANGE = 8f;
        const float DEEP_RANGE = -30f;
        static float FogFraction(float viewHeight, float targetHeight)
        {
            var total = MathX.Fabs(targetHeight - viewHeight);
            //return targetHeight >= 0 ? 0f : 1f;

            // only ranges that cross the ramp range are special
            if (targetHeight > 0 && viewHeight > 0) return 0f;
            if (targetHeight < -RAMP_RANGE && viewHeight < -RAMP_RANGE) return 1f;

            float above;
            if (targetHeight > 0) above = targetHeight;
            else if (viewHeight > 0) above = viewHeight;
            else above = 0;

            float rampTop, rampBottom;
            if (viewHeight > targetHeight) { rampTop = viewHeight; rampBottom = targetHeight; }
            else { rampTop = targetHeight; rampBottom = viewHeight; }
            if (rampTop > 0) rampTop = 0;
            if (rampBottom < -RAMP_RANGE) rampBottom = -RAMP_RANGE;

            var rampSlope = 1f / RAMP_RANGE;
            if (total == 0) return -viewHeight * rampSlope;

            var ramp = (1f - (rampTop * rampSlope + rampBottom * rampSlope) * -0.5f) * (rampTop - rampBottom);
            var frac = (total - above - ramp) / total;

            // after it gets moderately deep, always use full value
            var deepest = viewHeight < targetHeight ? viewHeight : targetHeight;

            var deepFrac = deepest / DEEP_RANGE;
            if (deepFrac >= 1f) return 1f;

            frac = frac * (1f - deepFrac) + deepFrac;

            return frac;
        }

        // Modulate the fog alpha density based on the distance of the start and end points to the terminator plane
        void R_FogEnterImage(Image image)
        {
            int x, y, b;
            var data = new byte[FOG_ENTER_SIZE, FOG_ENTER_SIZE, 4];

            for (x = 0; x < FOG_ENTER_SIZE; x++)
                for (y = 0; y < FOG_ENTER_SIZE; y++)
                {
                    var d = FogFraction(x - (FOG_ENTER_SIZE / 2), y - (FOG_ENTER_SIZE / 2));

                    b = (byte)(d * 255);
                    if (b <= 0) b = 0;
                    else if (b > 255) b = 255;
                    data[y, x, 0] = data[y, x, 1] = data[y, x, 2] = 255;
                    data[y, x, 3] = (byte)b;
                }

            // if mipmapped, acutely viewed surfaces fade wrong
            fixed (byte* dataB = data) image.GenerateImage(dataB, FOG_ENTER_SIZE, FOG_ENTER_SIZE, TF.LINEAR, false, TR.CLAMP, TD.HIGH_QUALITY);
        }

        const int QUADRATIC_WIDTH = 32;
        const int QUADRATIC_HEIGHT = 4;

        void R_QuadraticImage(Image image)
        {
            int x, y, b;
            var data = new byte[QUADRATIC_HEIGHT, QUADRATIC_WIDTH, 4];

            for (x = 0; x < QUADRATIC_WIDTH; x++)
                for (y = 0; y < QUADRATIC_HEIGHT; y++)
                {
                    var d = x - (QUADRATIC_WIDTH / 2 - 0.5f);
                    d = MathX.Fabs(d);
                    d -= 0.5f;
                    d /= QUADRATIC_WIDTH / 2;

                    d = 1f - d;
                    d *= d;

                    b = (byte)(d * 255);
                    if (b <= 0) b = 0;
                    else if (b > 255) b = 255;
                    data[y, x, 0] = data[y, x, 1] = data[y, x, 2] = (byte)b;
                    data[y, x, 3] = 255;
                }

            fixed (byte* dataB = data) image.GenerateImage(dataB, QUADRATIC_WIDTH, QUADRATIC_HEIGHT, TF.DEFAULT, false, TR.CLAMP, TD.HIGH_QUALITY);
        }

        //=====================================================================

        struct FilterName
        {
            public FilterName(string name, int minimize, int maximize)
            {
                this.name = name;
                this.minimize = minimize;
                this.maximize = maximize;
            }
            public string name;
            public int minimize, maximize;
        }

        static FilterName[] TextureFilters = new[] {
            new FilterName("GL_LINEAR_MIPMAP_NEAREST", (int)TextureMinFilter.LinearMipmapNearest, (int)BlitFramebufferFilter.Linear),
            new FilterName("GL_LINEAR_MIPMAP_LINEAR", (int)TextureMinFilter.LinearMipmapLinear, (int)BlitFramebufferFilter.Linear),
            new FilterName("GL_NEAREST", (int)BlitFramebufferFilter.Nearest, (int)BlitFramebufferFilter.Nearest),
            new FilterName("GL_LINEAR", (int)BlitFramebufferFilter.Linear, (int)BlitFramebufferFilter.Linear),
            new FilterName("GL_NEAREST_MIPMAP_NEAREST", (int)TextureMinFilter.NearestMipmapNearest, (int)BlitFramebufferFilter.Nearest),
            new FilterName("GL_NEAREST_MIPMAP_LINEAR", (int)TextureMinFilter.NearestMipmapLinear, (int)BlitFramebufferFilter.Nearest)
        };

        // This resets filtering on all loaded images
        // New images will automatically pick up the current values.
        public void ChangeTextureFilter()
        {
            int i; string s;

            // if these are changed dynamically, it will force another ChangeTextureFilter
            image_filter.ClearModified();
            image_anisotropy.ClearModified();

            s = image_filter.String;
            for (i = 0; i < 6; i++) if (string.Equals(TextureFilters[i].name, s, StringComparison.OrdinalIgnoreCase)) break;
            // default to LINEAR_MIPMAP_NEAREST
            if (i == 6) { common.Warning($"bad r_textureFilter: '{s}'"); i = 0; }

            // set the values for future images
            textureMinFilter = TextureFilters[i].minimize;
            textureMaxFilter = TextureFilters[i].maximize;
            textureAnisotropy = image_anisotropy.Float;
            if (textureAnisotropy < 1f) textureAnisotropy = 1f;
            else if (textureAnisotropy > glConfig.maxTextureAnisotropy) textureAnisotropy = glConfig.maxTextureAnisotropy;

            // change all the existing mipmap texture objects with default filtering
            foreach (var image in images)
            {
                var texEnum = TextureTarget.Texture2d;
                switch (image.type)
                {
                    case Image.TT._2D: texEnum = TextureTarget.Texture2d; break;
                    case Image.TT.CUBIC: texEnum = TextureTarget.TextureCubeMap; break;
                }

                // make sure we don't start a background load
                if (image.texnum == TEXTURE_NOT_LOADED) continue;
                image.Bind();
                if (image.filter == TF.DEFAULT)
                {
                    qglTexParameterf(texEnum, GetTextureParameter.TextureMinFilter, globalImages.textureMinFilter);
                    qglTexParameterf(texEnum, GetTextureParameter.TextureMagFilter, globalImages.textureMaxFilter);
                }
                if (glConfig.anisotropicAvailable) qglTexParameterf(texEnum, (GetTextureParameter)GL_TEXTURE_MAX_ANISOTROPY_EXT, globalImages.textureAnisotropy);
            }
        }

        // Regenerate all images that came directly from files that have changed, so any saved changes will show up in place.
        // New r_texturesize/r_texturedepth variables will take effect on reload
        // reloadImages <all>
        static void R_ReloadImages_f(CmdArgs args)
        {
            // DG: notify the game DLL about the reloadImages command
            gameCallbacks.reloadImagesCB?.Invoke(gameCallbacks.reloadImagesUserArg, args);

            // this probably isn't necessary...
            globalImages.ChangeTextureFilter();

            var all = false;
            if (args.Count == 2)
            {
                if (string.Equals(args[1], "all", StringComparison.OrdinalIgnoreCase)) all = true;
                else if (string.Equals(args[1], "reload", StringComparison.OrdinalIgnoreCase)) all = true;
                else { common.Printf("USAGE: reloadImages <all>\n"); return; }
            }

            foreach (var image in globalImages.images) image.Reload(all);
        }

        struct SortedImage
        {
            public Image image;
            public int size;
        }

        static void R_ListImages_f(CmdArgs args)
        {
            int matchTag = 0;
            bool unloaded = false,
                cached = false,
                uncached = false,
                failed = false,
                touched = false,
                sorted = false,
                duplicated = false,
                byClassification = false,
                overSized = false;

            if (args.Count == 1) { }
            else if (args.Count == 2)
            {
                if (string.Equals(args[1], "sorted", StringComparison.OrdinalIgnoreCase)) sorted = true;
                else if (string.Equals(args[1], "unloaded", StringComparison.OrdinalIgnoreCase)) unloaded = true;
                else if (string.Equals(args[1], "cached", StringComparison.OrdinalIgnoreCase)) cached = true;
                else if (string.Equals(args[1], "uncached", StringComparison.OrdinalIgnoreCase)) uncached = true;
                else if (string.Equals(args[1], "tagged", StringComparison.OrdinalIgnoreCase)) matchTag = 1;
                else if (string.Equals(args[1], "duplicated", StringComparison.OrdinalIgnoreCase)) duplicated = true;
                else if (string.Equals(args[1], "touched", StringComparison.OrdinalIgnoreCase)) touched = true;
                else if (string.Equals(args[1], "classify", StringComparison.OrdinalIgnoreCase)) { byClassification = true; sorted = true; }
                else if (string.Equals(args[1], "oversized", StringComparison.OrdinalIgnoreCase)) { byClassification = true; sorted = true; overSized = true; }
                else failed = true;
            }
            else failed = true;

            if (failed)
            {
                common.Printf("usage: listImages [ sorted | unloaded | cached | uncached | tagged | duplicated | touched | classify | showOverSized ]\n");
                return;
            }

            int i, j, partialSize;
            int count = 0;
            var sortedArray = new SortedImage[globalImages.images.Count];

            const string header = "       -w-- -h-- filt -fmt-- wrap  size --name-------\n";
            common.Printf($"\n{header}");

            var totalSize = 0;
            for (i = 0; i < globalImages.images.Count; i++)
            {
                var image = globalImages.images[i];

                if (matchTag != 0 && image.classification != matchTag) continue;
                if (unloaded && image.texnum != TEXTURE_NOT_LOADED) continue;
                if (cached && (image.texnum == TEXTURE_NOT_LOADED)) continue;
                if (uncached && (image.texnum != TEXTURE_NOT_LOADED)) continue;

                // only print duplicates (from mismatched wrap / clamp, etc)
                if (duplicated)
                {
                    for (j = i + 1; j < globalImages.images.Count; j++) if (string.Equals(image.imgName, globalImages.images[j].imgName, StringComparison.OrdinalIgnoreCase)) break;
                    if (j == globalImages.images.Count) continue;
                }

                // "listimages touched" will list only images bound since the last "listimages touched" call
                if (touched)
                {
                    if (image.bindCount == 0) continue;
                    image.bindCount = 0;
                }

                if (sorted) { sortedArray[count].image = image; sortedArray[count].size = image.StorageSize; }
                else { common.Printf($"{i:4}"); image.Print(); }
                totalSize += image.StorageSize;
                count++;
            }

            if (sorted)
            {
                Array.Sort(sortedArray, (a, b) =>
                {
                    if (a.size > b.size) return -1;
                    if (a.size < b.size) return 1;
                    return string.Compare(a.image.imgName, b.image.imgName);
                });
                partialSize = 0;
                for (i = 0; i < count; i++)
                {
                    common.Printf($"{i:4}");
                    sortedArray[i].image.Print();
                    partialSize += sortedArray[i].image.StorageSize;
                    if (((i + 1) % 10) == 0) common.Printf($"-------- {1024 * 1024f:5.1} of {totalSize / (1024 * 1024f):5.1} megs --------\n");
                }
            }

            common.Printf(header);
            common.Printf($" {count} images ({globalImages.images.Count} total)\n");
            common.Printf($" {totalSize / (1024 * 1024f):5.1} total megabytes of images\n\n\n");

            if (byClassification)
            {
                var classifications = new List<List<int>>((int)IC.COUNT);
                for (i = 0; i < count; i++)
                {
                    var cl = (int)ClassifyImage(sortedArray[i].image.imgName);
                    classifications[cl].Add(i);
                }

                for (i = 0; i < (int)IC.COUNT; i++)
                {
                    partialSize = 0;
                    var overSizedList = new List<int>();
                    for (j = 0; j < classifications[i].Count; j++)
                    {
                        partialSize += sortedArray[classifications[i][j]].image.StorageSize;
                        if (overSized && sortedArray[classifications[i][j]].image.uploadWidth > IC_Info[i].maxWidth && sortedArray[classifications[i][j]].image.uploadHeight > IC_Info[i].maxHeight)
                            overSizedList.Add(classifications[i][j]);
                    }
                    common.Printf($" Classification {IC_Info[i].desc} contains {classifications[i].Count} images using {partialSize / (1024 * 1024f):5.1} megabytes\n");
                    if (overSized && overSizedList.Count != 0)
                    {
                        common.Printf("  The following images may be oversized\n");
                        for (j = 0; j < overSizedList.Count; j++)
                        {
                            common.Printf("    ");
                            sortedArray[overSizedList[j]].image.Print();
                            common.Printf("\n");
                        }
                    }
                }
            }
        }

        // Create a 256 color palette to be used by compressed normal maps
        public void SetNormalPalette()
        {
            int i, j; Vector3 v; float t;
            var temptable = compressedPalette;
            var compressedToOriginal = stackalloc int[16];

            // make an ad-hoc separable compression mapping scheme
            for (i = 0; i < 8; i++)
            {
                float f, y;

                f = (i + 1) / 8.5f;
                y = MathX.Sqrt(1f - f * f);
                y = 1f - y;

                compressedToOriginal[7 - i] = 127 - (int)(y * 127 + 0.5f);
                compressedToOriginal[8 + i] = 128 + (int)(y * 127 + 0.5f);
            }

            for (i = 0; i < 256; i++)
            {
                if (i <= compressedToOriginal[0]) originalToCompressed[i] = 0;
                else if (i >= compressedToOriginal[15]) originalToCompressed[i] = 15;
                else
                {
                    for (j = 0; j < 14; j++) if (i <= compressedToOriginal[j + 1]) break;
                    originalToCompressed[i] = (byte)(i - compressedToOriginal[j] < compressedToOriginal[j + 1] - i ? j : j + 1);
                }
            }

#if false
            for (i = 0; i < 16; i++)
                for (j = 0; j < 16; j++)
                {

                    v[0] = (i - 7.5) / 8;
                    v[1] = (j - 7.5) / 8;

                    t = 1.0 - (v[0] * v[0] + v[1] * v[1]);
                    if (t < 0)
                    {
                        t = 0;
                    }
                    v[2] = idMath::Sqrt(t);

                    temptable[(i * 16 + j) * 3 + 0] = 128 + floor(127 * v[0] + 0.5);
                    temptable[(i * 16 + j) * 3 + 1] = 128 + floor(127 * v[1]);
                    temptable[(i * 16 + j) * 3 + 2] = 128 + floor(127 * v[2]);
                }
#else
            for (i = 0; i < 16; i++)
                for (j = 0; j < 16; j++)
                {
                    v.x = (compressedToOriginal[i] - 127.5f) / 128;
                    v.y = (compressedToOriginal[j] - 127.5f) / 128;

                    t = 1f - (v.x * v.x + v.y * v.y);
                    if (t < 0) t = 0;
                    v.z = MathX.Sqrt(t);

                    temptable[(i * 16 + j) * 3 + 0] = (byte)(128 + Math.Floor(127 * v.x + 0.5));
                    temptable[(i * 16 + j) * 3 + 1] = (byte)(128 + Math.Floor(127 * v.y));
                    temptable[(i * 16 + j) * 3 + 2] = (byte)(128 + Math.Floor(127 * v.z));
                }
#endif

            // color 255 will be the "nullnormal" color for no reflection
            temptable[255 * 3 + 0] = temptable[255 * 3 + 1] = temptable[255 * 3 + 2] = 128;

            return;
        }

        // Allocates an idImage, adds it to the list, copies the name, and adds it to the hash chain.
        public Image AllocImage(string name)
        {
            if (name.Length >= MAX_IMAGE_NAME) common.Error($"ImageManager::AllocImage: \"{name}\" is too long\n");
            var image = new Image();
            images.Add(image);
            imagesByName.Add(name, new List<Image> { image });
            return image;
        }

        // Images that are procedurally generated are allways specified with a callback which must work at any time, allowing the OpenGL system to be completely regenerated if needed.
        public Image ImageFromFunction(string name, Action<Image> generatorFunction)
        {
            if (name == null) common.FatalError("ImageManager::ImageFromFunction: NULL name");

            // strip any .tga file extensions from anywhere in the _name
            name = PathX.BackSlashesToSlashes(name.Replace(".tga", ""));

            // see if the image already exists
            if (imagesByName.TryGetValue(name, out var images2))
                foreach (var image2 in images2)
                {
                    if (image2.generatorFunction != generatorFunction) common.DPrintf($"WARNING: reused image {name} with mixed generators\n");
                    return image2;
                }

            // create the image and issue the callback
            var image = AllocImage(name);
            image.generatorFunction = generatorFunction;

            if (image_preload.Bool)
            {
                image.referencedOutsideLevelLoad = true;
                //image.ActuallyLoadImage(false);
                globalImages.AddAllocList(image);
            }

            return image;
        }

        // Finds or loads the given image, always returning a valid image pointer.
        // Loading of the image may be deferred for dynamic loading.
        public Image ImageFromFile(string name, TF filter, bool allowDownSize, TR repeat, TD depth, CF cubeMap = 0)
        {
            if (string.IsNullOrEmpty(name) || string.Equals(name, "default", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "_default", StringComparison.OrdinalIgnoreCase)) { declManager.MediaPrint("DEFAULTED\n"); return globalImages.defaultImage; }

            // strip any .tga file extensions from anywhere in the _name, including image program parameters
            name = PathX.BackSlashesToSlashes(name.Replace(".tga", ""));

            // see if the image is already loaded, unless we are in a reloadImages call
            if (imagesByName.TryGetValue(name, out var images2))
                foreach (var image2 in images2)
                {
                    // the built in's, like _white and _flat always match the other options
                    if (name[0] == '_') return image2;
                    if (image2.cubeFiles != cubeMap) common.Error($"Image '{name}' has been referenced with conflicting cube map states");

                    // we might want to have the system reset these parameters on every bind and share the image data
                    if (image2.filter != filter || image2.repeat != repeat) continue;

                    // note that it is used this level load
                    if (image2.allowDownSize == allowDownSize && image2.depth == depth) { image2.levelLoadReferenced = true; return image2; }

                    // the same image is being requested, but with a different allowDownSize or depth
                    // so pick the highest of the two and reload the old image with those parameters
                    if (!image2.allowDownSize) allowDownSize = false;
                    if (image2.depth > depth) depth = image2.depth;

                    // the already created one is already the highest quality
                    if (image2.allowDownSize == allowDownSize && image2.depth == depth) { image2.levelLoadReferenced = true; return image2; }

                    image2.allowDownSize = allowDownSize;
                    image2.depth = depth;
                    image2.levelLoadReferenced = true;

                    if (image_preload.Bool && !insideLevelLoad)
                    {
                        image2.referencedOutsideLevelLoad = true;
                        //image.ActuallyLoadImage(false);
                        globalImages.AddAllocList(image2);
                        declManager.MediaPrint($"{image2.uploadWidth}x{image2.uploadHeight} {image2.imgName} (reload for mixed references)\n");
                    }
                    return image2;
                }

            // create a new image
            var image = AllocImage(name);

            // HACK: to allow keep fonts from being mip'd, as new ones will be introduced with localization this keeps us from having to make a material for each font tga
            if (name.Contains("fontImage_")) allowDownSize = false;

            image.allowDownSize = allowDownSize;
            image.repeat = repeat;
            image.depth = depth;
            image.type = Image.TT._2D;
            image.cubeFiles = cubeMap;
            image.filter = filter;

            image.levelLoadReferenced = true;

            // load it if we aren't in a level preload
            if (image_preload.Bool && !insideLevelLoad)
            {
                image.referencedOutsideLevelLoad = true;
                //image.ActuallyLoadImage(false);
                globalImages.AddAllocList(image);
                declManager.MediaPrint($"{image.uploadWidth}x{image.uploadHeight} {image.imgName}\n");
            }
            else declManager.MediaPrint($"{image.imgName}\n");

            return image;
        }

        public Image GetImage(string name)
        {
            if (string.IsNullOrEmpty(name) || string.Equals(name, "default", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "_default", StringComparison.OrdinalIgnoreCase)) { declManager.MediaPrint("DEFAULTED\n"); return globalImages.defaultImage; }

            // strip any .tga file extensions from anywhere in the _name, including image program parameters
            name = PathX.BackSlashesToSlashes(name.Replace(".tga", ""));

            // look in loaded images
            return imagesByName.TryGetValue(name, out var images2) ? images2.First() : null;
        }

        public void PurgeAllImages()
        {
            foreach (var image in images)
            {
                //image.PurgeImage();
                globalImages.AddPurgeList(image);
            }
        }

        public void ReloadAllImages()
        {
            CmdArgs args = new();

            // build the compressed normal map palette
            SetNormalPalette();

            args.TokenizeString("reloadImages reload", false);
            R_ReloadImages_f(args);
        }

        // Used to combine animations of six separate tga files into a serials of 6x taller tga files, for preparation to roq compress
        static void R_CombineCubeImages_f(CmdArgs args)
        {
            if (args.Count != 2)
            {
                common.Printf("usage: combineCubeImages <baseName>\n");
                common.Printf(" combines basename[1-6][0001-9999].tga to basenameCM[0001-9999].tga\n");
                common.Printf(" 1: forward 2:right 3:back 4:left 5:up 6:down\n");
                return;
            }

            var baseName = args[1];
            common.SetRefreshOnPrint(true);

            var pics = stackalloc byte*[6];
            var orderRemap = stackalloc int[6] { 1, 3, 4, 2, 5, 6 };

            for (var frameNum = 1; frameNum < 10000; frameNum++)
            {
                int width = 0, height = 0, side; string filename;

                for (side = 0; side < 6; side++)
                {
                    filename = $"{baseName}{orderRemap[side]}{frameNum:04}.tga";

                    common.Printf($"reading {filename}\n");
                    R_LoadImage(filename, ref pics[side], out width, out height, out _, true);

                    if (pics[side] == null) { common.Printf("not found.\n"); break; }

                    // convert from "camera" images to native cube map images
                    switch (side)
                    {
                        case 0: R_RotatePic(pics[side], width); break; // forward
                        case 1: R_RotatePic(pics[side], width); R_HorizontalFlip(pics[side], width, height); R_VerticalFlip(pics[side], width, height); break; // back
                        case 2: R_VerticalFlip(pics[side], width, height); break; // left 
                        case 3: R_HorizontalFlip(pics[side], width, height); break; // right
                        case 4: R_RotatePic(pics[side], width); break; // up
                        case 5: R_RotatePic(pics[side], width); break; // down
                    }
                }

                if (side != 6)
                {
                    for (var i = 0; i < side; side++) Marshal.FreeHGlobal((IntPtr)pics[side]);
                    break;
                }

                var combined = new byte[width * height * 6 * 4];
                fixed (byte* combinedB = combined)
                {
                    for (side = 0; side < 6; side++)
                    {
                        Unsafe.CopyBlock(combinedB + width * height * 4 * side, pics[side], (uint)(width * height * 4));
                        Marshal.FreeHGlobal((IntPtr)pics[side]);
                    }
                    filename = $"{baseName}CM{frameNum:04}.tga";

                    common.Printf($"writing {filename}\n");
                    R_WriteTGA(filename, combinedB, width, height * 6);
                }
            }
            common.SetRefreshOnPrint(false);
        }

        public void CheckCvars()
        {
            // textureFilter stuff
            if (image_filter.IsModified || image_anisotropy.IsModified)
            {
                ChangeTextureFilter();
                image_filter.ClearModified();
                image_anisotropy.ClearModified();
            }
        }

        public int SumOfUsedImages()
        {
            var total = 0;
            foreach (var image in images) if (image.frameUsed == backEnd.frameCount) total += image.StorageSize;
            return total;
        }

        public void BindNull()
        {
            qglBindTexture(TextureTarget.Texture2d, 0);
        }

        public void Init()
        {
            images.Resize(1024, 1024);

            imagesAlloc.Resize(1024, 1024);
            imagesPurge.Resize(1024, 1024);

            // clear the cached LRU
            cacheLRU.cacheUsageNext = cacheLRU;
            cacheLRU.cacheUsagePrev = cacheLRU;

            // set default texture filter modes
            ChangeTextureFilter();

            // create built in images
            defaultImage = ImageFromFunction("_default", R_DefaultImage);
            whiteImage = ImageFromFunction("_white", R_WhiteImage);
            blackImage = ImageFromFunction("_black", R_BlackImage);
            borderClampImage = ImageFromFunction("_borderClamp", R_BorderClampImage);
            flatNormalMap = ImageFromFunction("_flat", R_FlatNormalImage);
            ambientNormalMap = ImageFromFunction("_ambient", R_AmbientNormalImage);
            specularTableImage = ImageFromFunction("_specularTable", R_SpecularTableImage);
            specular2DTableImage = ImageFromFunction("_specular2DTable", R_Specular2DTableImage);
            rampImage = ImageFromFunction("_ramp", R_RampImage);
            alphaRampImage = ImageFromFunction("_alphaRamp", R_RampImage);
            alphaNotchImage = ImageFromFunction("_alphaNotch", R_AlphaNotchImage);
            fogImage = ImageFromFunction("_fog", R_FogImage);
            fogEnterImage = ImageFromFunction("_fogEnter", R_FogEnterImage);
            normalCubeMapImage = ImageFromFunction("_normalCubeMap", makeNormalizeVectorCubeMap);
            noFalloffImage = ImageFromFunction("_noFalloff", R_CreateNoFalloffImage);
            quadraticImage = ImageFromFunction("_quadratic", R_QuadraticImage);

            // cinematicImage is used for cinematic drawing
            // scratchImage is used for screen wipes/doublevision etc..
            cinematicImage = ImageFromFunction("_cinematic", R_RGBA8Image);
            scratchImage = ImageFromFunction("_scratch", R_RGBA8Image);
            scratchImage2 = ImageFromFunction("_scratch2", R_RGBA8Image);
            accumImage = ImageFromFunction("_accum", R_RGBA8Image);
            scratchCubeMapImage = ImageFromFunction("_scratchCubeMap", makeNormalizeVectorCubeMap);
            currentRenderImage = ImageFromFunction("_currentRender", R_RGBA8Image);

            hudImage = ImageFromFunction("_hudImage", R_VRSurfaceImage); // R_RGBA8Image );
            pdaImage = ImageFromFunction("_pdaImage", R_VRSurfaceImage); // R_RGBA8Image );


            cmdSystem.AddCommand("reloadImages", R_ReloadImages_f, CMD_FL.RENDERER, "reloads images");
            cmdSystem.AddCommand("listImages", R_ListImages_f, CMD_FL.RENDERER, "lists images");
            cmdSystem.AddCommand("combineCubeImages", R_CombineCubeImages_f, CMD_FL.RENDERER, "combines six images for roq compression");

            // should forceLoadImages be here?
        }

        public void Shutdown()
        {
            images.Clear();
            while (imagesAlloc.Count > 0) imagesAlloc.RemoveAt(0);
            while (imagesPurge.Count > 0) imagesPurge.RemoveAt(0);
        }


        // Mark all file based images as currently unused, but don't free anything.  Calls to ImageFromFile() will
        // either mark the image as used, or create a new image without loading the actual data.
        public void BeginLevelLoad()
        {
            insideLevelLoad = true;

            foreach (var image in images)
            {
                // generator function images are always kept around
                if (image.generatorFunction != null) continue;

                if (C.com_purgeAll.Bool)
                {
                    //image.PurgeImage();
                    globalImages.AddPurgeList(image);

                    // Need to do this so it doesn't get missed to be added to alloc list
                    //image.texnum = TEXTURE_NOT_LOADED;
                }

                image.levelLoadReferenced = false;
            }
        }

        // Free all images marked as unused, and load all images that are necessary.
        // This architecture prevents us from having the union of two level's worth of data present at one time.
        // preload everything, never free
        // preload everything, free unused after level load
        // blocking load on demand
        // preload low mip levels, background load remainder on demand
        public void EndLevelLoad()
        {
            var start = SysW.Milliseconds;

            insideLevelLoad = false;
            //if (AsyncNetwork.serverDedicated.Integer != 0) return;

            common.Printf("----- idImageManager::EndLevelLoad -----\n");

            int purgeCount = 0, keepCount = 0, loadCount = 0;

            // purge the ones we don't need
            foreach (var image in images)
            {
                if (image.generatorFunction != null) continue;

                if (!image.levelLoadReferenced && !image.referencedOutsideLevelLoad)
                {
                    //common.Printf($"Purging {image.imgName}\n");
                    purgeCount++;
                    //image.PurgeImage();
                    globalImages.AddPurgeList(image);

                    // Need to do this so it doesn't get missed to be added to alloc list
                    //image.texnum = TEXTURE_NOT_LOADED;
                }
                else if (image.texnum != TEXTURE_NOT_LOADED)
                {
                    //common.Printf($"Keeping {image.imgName}\n");
                    keepCount++;
                }
            }

            // load the ones we do need, if we are preloading
            foreach (var image in images)
            {
                if (image.generatorFunction != null) continue;

                if (image.levelLoadReferenced && !image.IsLoaded)
                {
                    //common.Printf($"Loading {image.imgName}\n");
                    loadCount++;
                    //image.ActuallyLoadImage( false );
                    globalImages.AddAllocList(image);

                    if ((loadCount & 15) == 0) session.PacifierUpdate();
                }
            }

            var end = SysW.Milliseconds;
            common.Printf($"{purgeCount:5} purged from previous\n");
            common.Printf($"{keepCount:5} kept from previous\n");
            common.Printf($"{loadCount:5} new loaded\n");
            common.Printf($"all images loaded in {(end - start) * 0.001f:5.1} seconds\n");
        }

        public void AddAllocList(Image image)
        {
            // Not the bind from the backend can add an image to the list
            ISystem.EnterCriticalSection(CRITICAL_SECTION.SECTION_TWO);

            if (image != null)
            {
                //Console.WriteLine("AddAllocList");
                imagesAlloc.Add(image);
            }

            ISystem.LeaveCriticalSection(CRITICAL_SECTION.SECTION_TWO);
        }

        internal void AddPurgeList(Image image)
        {
            if (image != null)
            {
                //Console.WriteLine("AddPurgeList");
                imagesPurge.Add(image);
                image.purgePending = true;
            }
        }

        public Image GetNextAllocImage()
        {
            Image img = null;
            if (imagesAlloc.Count > 0) { img = imagesAlloc[0]; imagesAlloc.Remove(img); }
            return img;
        }

        public Image GetNextPurgeImage()
        {
            Image img = null;
            if (imagesPurge.Count > 0) { img = imagesPurge[0]; imagesPurge.Remove(img); img.purgePending = false; }
            return img;
        }

        public void StartBuild()
        {
            ddsList.Clear();
            ddsHash.Clear();
        }

        public void FinishBuild(bool removeDups = false)
        {
            VFile batchFile;
            if (removeDups)
            {
                ddsList.Clear();
                fileSystem.ReadFile("makedds.bat", out var buffer, out _);
                if (buffer == null) return;
                var str = Encoding.ASCII.GetString(buffer);
                while (str.Length != 0)
                {
                    var n = str.IndexOf('\n');
                    if (n <= 0) break;
                    var line = str[..(n + 1)];
                    str = str[(str.Length - n - 1)..];
                    ddsList.AddUnique(line);
                }
            }
            batchFile = fileSystem.OpenFileWrite(removeDups ? "makedds2.bat" : "makedds.bat");
            if (batchFile != null)
            {
                var ddsNum = ddsList.Count;
                for (var i = 0; i < ddsNum; i++)
                {
                    batchFile.WriteFloatString(ddsList[i]);
                    batchFile.Printf($"@echo Finished compressing {i + 1} of {ddsNum}.  {((float)(i + 1) / (float)ddsNum) * 100f:.1}f percent done.\n");
                }
                fileSystem.CloseFile(batchFile);
            }
            ddsList.Clear();
            ddsHash.Clear();
        }

        public void AddDDSCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd) || ddsHash.ContainsKey(cmd)) return;
            ddsList.Add(cmd);
        }

        public void PrintMemInfo(MemInfo mi)
        {
            int i, j, total = 0;

            var f = fileSystem.OpenFileWrite($"{mi.filebase}_images.txt");
            if (f == null) return;

            // sort first
            var sortIndex = new int[images.Count];
            for (i = 0; i < images.Count; i++) sortIndex[i] = i;
            for (i = 0; i < images.Count - 1; i++)
                for (j = i + 1; j < images.Count; j++)
                    if (images[sortIndex[i]].StorageSize < images[sortIndex[j]].StorageSize) Platform.Swap(ref sortIndex[i], ref sortIndex[j]);

            // print next
            for (i = 0; i < images.Count; i++)
            {
                var image = images[sortIndex[i]];
                var size = image.StorageSize;
                total += size;

                f.Printf($"{size} {image.refCount:3} {image.imgName}\n");
            }

            mi.imageAssetsTotal = total;

            f.Printf($"\nTotal image bytes allocated: {total}\n");
            fileSystem.CloseFile(f);
        }
    }
}