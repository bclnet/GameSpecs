using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class Image
    {
        static int MakePowerOfTwo(int num)
        {
            int pot;
            for (pot = 1; pot < num; pot <<= 1) { }
            return pot;
        }

        // Used for determining memory utilization
        static int BitsForInternalFormat(InternalFormat internalFormat)
        {
            switch ((int)internalFormat)
            {
                case 1:
                case 2:
                case 3:
                case 4: return 32;
                case (int)InternalFormat.Rgba4: return 16;
                case (int)InternalFormat.Rgb5A1: return 16;
                default: common.Error($"R_BitsForInternalFormat: BAD FORMAT:{internalFormat}"); return 0;
            }
        }

        // Create a 256 color palette to be used by compressed normal maps
        void UploadCompressedNormalMap(int width, int height, byte* rgba, int mipLevel)
        {
            int i, j, x, y, z, row; byte* in1, out_;

            // OpenGL's pixel packing rule
            row = width < 4 ? 4 : width;

            var normals = stackalloc byte[row * height];
            if (normals == null) common.Error("R_UploadCompressedNormalMap: _alloca failed");

            in1 = rgba;
            out_ = normals;
            for (i = 0; i < height; i++, out_ += row, in1 += width * 4)
                for (j = 0; j < width; j++)
                {
                    x = in1[j * 4 + 0];
                    y = in1[j * 4 + 1];
                    z = in1[j * 4 + 2];

                    int c;
                    if (x == 128 && y == 128 && z == 128) c = 255; // the "nullnormal" color
                    else
                    {
                        c = (globalImages.originalToCompressed[x] << 4) | globalImages.originalToCompressed[y];
                        if (c == 255) c = 254;    // don't use the nullnormal color
                    }
                    out_[j] = (byte)c;
                }

            // Optionally write out_ the paletized normal map to a .tga
            if (mipLevel == 0 && ImageManager.image_writeNormalTGAPalletized.Bool)
            {
                ImageProgramStringToCompressedFileName(imgName, out var filename);
                var ext = filename.LastIndexOf('.');
                if (ext != -1)
                {
                    filename = $"{filename.Remove(ext)}_pal.tga";
                    R_WritePalTGA(filename, normals, globalImages.compressedPalette, width, height);
                }
            }
        }


        //=======================================================================

        static byte[,] mipBlendColors = {
            {0,0,0,0},
            {255,0,0,128},
            {0,255,0,128},
            {0,0,255,128},
            {255,0,0,128},
            {0,255,0,128},
            {0,0,255,128},
            {255,0,0,128},
            {0,255,0,128},
            {0,0,255,128},
            {255,0,0,128},
            {0,255,0,128},
            {0,0,255,128},
            {255,0,0,128},
            {0,255,0,128},
            {0,0,255,128},
        };

        public void SetImageFilterAndRepeat()
        {
            // set the minimize / maximize filtering
            switch (filter)
            {
                case TF.DEFAULT:
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMinFilter, globalImages.textureMinFilter);
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMagFilter, globalImages.textureMaxFilter);
                    break;
                case TF.LINEAR:
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMinFilter, (float)BlitFramebufferFilter.Linear);
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMagFilter, (float)BlitFramebufferFilter.Linear);
                    break;
                case TF.NEAREST:
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMinFilter, (float)BlitFramebufferFilter.Nearest);
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMagFilter, (float)BlitFramebufferFilter.Nearest);
                    break;
                default: common.FatalError("R_CreateImage: bad texture filter"); break;
            }

            // only do aniso filtering on mip mapped images
            if (glConfig.anisotropicAvailable) qglTexParameterf(TextureTarget.Texture2d, (GetTextureParameter)GL_TEXTURE_MAX_ANISOTROPY_EXT, filter == TF.DEFAULT ? globalImages.textureAnisotropy : 1);

            // set the wrap/clamp modes
            switch (repeat)
            {
                case TR.REPEAT:
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapS, (float)TextureWrapMode.Repeat);
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapT, (float)TextureWrapMode.Repeat);
                    break;
                case TR.CLAMP_TO_BORDER:
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
                    // Disabled for OES2
                    //qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapS, (float)TextureWrapMode.ClampToBorder);
                    //qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapT, (float)TextureWrapMode.ClampToBorder);
                    break;
                case TR.CLAMP_TO_ZERO:
                case TR.CLAMP_TO_ZERO_ALPHA:
                case TR.CLAMP:
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
                    qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
                    break;
                default: common.FatalError("R_CreateImage: bad texture repeat"); break;
            }
        }

        // helper function that takes the current width/height and might make them smaller
        void GetDownsize(ref int scaled_width, ref int scaled_height)
        {
            var size = 0;

            // perform optional picmip operation to save texture memory
            //GB Forced specular downsizing in1 code
            if (depth == TD.SPECULAR && (ImageManager.image_downSizeSpecular.Integer != 0 || true)) { size = ImageManager.image_downSizeSpecularLimit.Integer; if (size == 0) size = 64; }
            else if (depth == TD.BUMP && ImageManager.image_downSizeBump.Integer != 0) { size = ImageManager.image_downSizeBumpLimit.Integer; if (size == 0) size = 64; }
            else if ((allowDownSize || ImageManager.image_forceDownSize.Bool) && ImageManager.image_downSize.Integer != 0) { size = ImageManager.image_downSizeLimit.Integer; if (size == 0) size = 256; }

            if (size > 0)
                while (scaled_width > size || scaled_height > size)
                {
                    if (scaled_width > 1) scaled_width >>= 1;
                    if (scaled_height > 1) scaled_height >>= 1;
                }

            // clamp to minimum size
            if (scaled_width < 1) scaled_width = 1;
            if (scaled_height < 1) scaled_height = 1;

            // clamp size to the hardware specific upper limit scale both axis down equally so we don't have to deal with a half mip resampling
            // This causes a 512*256 texture to sample down to 256*128 on a voodoo3, even though it could be 256*256
            while (scaled_width > glConfig.maxTextureSize || scaled_height > glConfig.maxTextureSize)
            {
                scaled_width >>= 1;
                scaled_height >>= 1;
            }
        }

        #region Extra Code

        static bool isopaque(int width, int height, byte* pixels)
        {
            for (var i = 0; i < width * height; i++) if (pixels[i * 4 + 3] != 0xff) return false;
            return true;
        }

        static void rgba4444_convert_tex_image(string path, TextureTarget target, int level, InternalFormat internalformat, int width, int height, int border, PixelFormat format, VertexAttribPointerType type, byte* pixels)
        {
            var data = new byte[sizeof(ushort) * width * height + 1];
            fixed (byte* dataB = data)
            {
                dataB[0] = 1; //: compress flag (uncompressed)
                var rgba4444S = (ushort*)(dataB + 1);
                for (var i = 0; i < width * height; i++)
                {
                    byte r = (byte)(pixels[4 * i] >> 4),
                        g = (byte)(pixels[4 * i + 1] >> 4),
                        b = (byte)(pixels[4 * i + 2] >> 4),
                        a = (byte)(pixels[4 * i + 3] >> 4);
                    rgba4444S[i] = (ushort)(r << 12 | g << 8 | b << 4 | a);
                }
                qglTexImage2D(target, level, (InternalFormat)format, width, height, border, format, (VertexAttribPointerType)PixelType.UnsignedShort4444, dataB);
                if (path != null) fileSystem.WriteFile(path, data, width * height * 2 + 1);
            }
        }

        static int etc1_data_size(int width, int height)
            => (((width + 3) & ~3) * ((height + 3) & ~3)) >> 1;

        static void etc1_compress_tex_image(string path, TextureTarget target, int level, InternalFormat internalformat, int width, int height, int border, PixelFormat format, VertexAttribPointerType type, byte* pixels)
        {
            var size = etc1_data_size(width, height);
            var data = new byte[size + 1];
            fixed (byte* dataB = data)
            {
                dataB[0] = 0; //: compress flag (compressed)
#if USE_RG_ETC1
	            Etc1Default.etc1_encode_image(pixels, width, height, 4, width*4, dataB + 1);
#else
                Etc1Android.etc1_encode_image(pixels, (uint)width, (uint)height, 4, (uint)width * 4, dataB + 1);
#endif
                qglCompressedTexImage2D(target, level, (InternalFormat)Etc1Android.GL_ETC1_RGB8_OES, width, height, 0, size, dataB + 1);
                if (path != null) fileSystem.WriteFile(path, data, size + 1);
            }
        }

        static bool etcavail(string path)
            => r_useETC1Cache.Bool && r_useETC1.Bool && path != null && fileSystem.ReadFile(path, out _) != -1;

        static bool uploadetc(string path, TextureTarget target, int level, InternalFormat internalformat, int width, int height, int border, PixelFormat format, VertexAttribPointerType type)
        {
            var failed = false;

            if (!etcavail(path)) return true;

            var sz = fileSystem.ReadFile(path, out var data, out _);
            if (sz == -1) return true;

            fixed (byte* dataB = data)
                if (dataB[0] == 0)
                {
                    if (sz == etc1_data_size(width, height) + 1) qglCompressedTexImage2D(target, level, (InternalFormat)Etc1Android.GL_ETC1_RGB8_OES, width, height, 0, etc1_data_size(width, height), dataB + 1);
                    else failed = false;
                }
                else
                {
                    if (sz == width * height * 2 + 1) qglTexImage2D(target, level, (InternalFormat)format, width, height, border, format, (VertexAttribPointerType)PixelType.UnsignedShort4444, dataB + 1);
                    else failed = false;
                }

            fileSystem.FreeFile(data);
            return failed;
        }

        static bool myglTexImage2D_opaque = false;
        static void myglTexImage2D(string cachefname, TextureTarget target, int level, InternalFormat internalformat, int width, int height, int border, PixelFormat format, VertexAttribPointerType type, byte* pixels)
        {
            //Console.WriteLine($"myglTexImage2D, name = {cachefname}");
            if (r_useETC1.Bool && format == PixelFormat.Rgba && type == VertexAttribPointerType.UnsignedByte)
            {
                if (level == 0) myglTexImage2D_opaque = isopaque(width, height, pixels);
                if (!r_useETC1Cache.Bool) cachefname = null;
                if (uploadetc(cachefname, target, level, internalformat, width, height, border, format, type))
                {
                    if (myglTexImage2D_opaque) etc1_compress_tex_image(cachefname, target, level, (InternalFormat)format, width, height, border, format, type, pixels);
                    else rgba4444_convert_tex_image(cachefname, target, level, (InternalFormat)format, width, height, border, format, type, pixels);
                }
                else Console.WriteLine($"Loaded cached image from {cachefname}");
            }
            else qglTexImage2D(target, level, internalformat, width, height, border, format, type, pixels);
        }

        #endregion

        //The alpha channel bytes should be 255 if you don't want the channel.
        //We need a material characteristic to ask for specific texture modes.
        //Designed limitations of flexibility:
        //No support for texture borders.
        //No support for texture border color.
        //No support for texture environment colors or GL_BLEND or GL_DECAL texture environments, because the automatic optimization to single or dual component textures makes those modes potentially undefined.
        //No non-power-of-two images.
        //No palettized textures.
        //There is no way to specify separate wrap/clamp values for S and T
        //There is no way to specify explicit mip map levels
        public void GenerateImage(byte* pic, int width, int height, TF filterParm, bool allowDownSizeParm, TR repeatParm, TD depthParm)
        {
            int scaled_width, scaled_height, ext; bool preserveBorder; byte* scaledBuffer, shrunk;

            PurgeImage();

            filter = filterParm;
            allowDownSize = allowDownSizeParm;
            repeat = repeatParm;
            depth = depthParm;

            // if we don't have a rendering context, just return after we have filled in1 the parms.  We must have the values set, or
            // an image match from a shader before OpenGL starts would miss the generated texture
            if (!glConfig.isInitialized) return;

            // don't let mip mapping smear the texture into the clamped border
            preserveBorder = repeat == TR.CLAMP_TO_ZERO;

            // make sure it is a power of 2
            scaled_width = MakePowerOfTwo(width);
            scaled_height = MakePowerOfTwo(height);
            if (scaled_width != width || scaled_height != height) common.Error("R_CreateImage: not a power of 2 image");

            // Optionally modify our width/height based on options/hardware
            GetDownsize(ref scaled_width, ref scaled_height);

            scaledBuffer = null;

            // generate the texture number
            fixed (uint* texnumU = &texnum) qglGenTextures(1, texnumU);

            // select proper internal format before we resample
            internalFormat = InternalFormat.Rgba;

            // copy or resample data as appropriate for first MIP level
            if (scaled_width == width && scaled_height == height)
            {
                // we must copy even if unchanged, because the border zeroing would otherwise modify  data
                scaledBuffer = (byte*)R_StaticAlloc(sizeof(uint) * scaled_width * scaled_height);
                Unsafe.CopyBlock(scaledBuffer, pic, (uint)(width * height * 4));
            }
            else
            {
                // resample down as needed (FIXME: this doesn't seem like it resamples anymore!)
                //scaledBuffer = R_ResampleTexture(pic, width, height, width >>= 1, height >>= 1);
                scaledBuffer = R_MipMap(pic, width, height, preserveBorder);
                width >>= 1;
                height >>= 1;
                if (width < 1) width = 1;
                if (height < 1) height = 1;

                while (width > scaled_width || height > scaled_height)
                {
                    shrunk = R_MipMap(scaledBuffer, width, height, preserveBorder);
                    R_StaticFree(scaledBuffer);
                    scaledBuffer = shrunk;

                    width >>= 1;
                    height >>= 1;
                    if (width < 1) width = 1;
                    if (height < 1) height = 1;
                }

                // one might have shrunk down below the target size
                scaled_width = width;
                scaled_height = height;
            }

            uploadHeight = scaled_height;
            uploadWidth = scaled_width;
            type = TT._2D;

            // zero the border if desired, allowing clamped projection textures even after picmip resampling or careless artists.
            if (repeat == TR.CLAMP_TO_ZERO) { byte* rgba = stackalloc byte[4] { 0, 0, 0, 255 }; R_SetBorderTexels(scaledBuffer, width, height, rgba); }
            if (repeat == TR.CLAMP_TO_ZERO_ALPHA) { byte* rgba = stackalloc byte[4] { 255, 255, 255, 0 }; R_SetBorderTexels(scaledBuffer, width, height, rgba); }

            if (generatorFunction == null && ((depth == TD.BUMP && ImageManager.image_writeNormalTGA.Bool) || (depth != TD.BUMP && ImageManager.image_writeTGA.Bool)))
            {
                // Optionally write out_ the texture to a .tga
                ImageProgramStringToCompressedFileName(imgName, out var filename2);
                ext = filename2.LastIndexOf('.');
                if (ext != -1)
                {
                    filename2 = $"{filename2.Remove(ext)}.tga";
                    // swap the red/alpha for the write
                    //if (depth == TD.BUMP) for (var i = 0; i < scaled_width * scaled_height * 4; i += 4) { scaledBuffer[i] = scaledBuffer[i + 3]; scaledBuffer[i + 3] = 0; }
                    R_WriteTGA(filename2, scaledBuffer, scaled_width, scaled_height, false);
                    // put it back
                    //if (depth == TD.BUMP) for (var i = 0; i < scaled_width * scaled_height * 4; i += 4) { scaledBuffer[i + 3] = scaledBuffer[i]; scaledBuffer[i] = 0; }
                }
            }

            // swap the red and alpha for rxgb support do this even on tga normal maps so we only have to use one fragment program
            if (depth == TD.BUMP)
                for (var i = 0; i < scaled_width * scaled_height * 4; i += 4)
                {
                    scaledBuffer[i + 3] = scaledBuffer[i];
                    scaledBuffer[i] = 0;
                }

            // upload the main image level
            Bind();
            //Console.WriteLine($"LOADING IMAGE {texnum} ({imgName})");

            //qglTexImage2D(TextureTarget.Texture2d, 0, internalFormat, scaled_width, scaled_height, 0, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, scaledBuffer);

            ImageProgramStringToCompressedFileName(imgName, out var filename);
            ext = filename.LastIndexOf('.');
            if (ext != -1) filename = $"{filename.Remove(ext)}.etc";
            else filename = null;
            myglTexImage2D(filename, TextureTarget.Texture2d, 0, internalFormat, scaled_width, scaled_height, 0, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, scaledBuffer);

            // create and upload the mip map levels, which we do in1 all cases, even if we don't think they are needed
            var miplevel = 0;
            while (scaled_width > 1 || scaled_height > 1)
            {
                // preserve the border after mip map unless repeating
                shrunk = R_MipMap(scaledBuffer, scaled_width, scaled_height, preserveBorder);
                R_StaticFree(scaledBuffer);
                scaledBuffer = shrunk;

                scaled_width >>= 1;
                scaled_height >>= 1;
                if (scaled_width < 1) scaled_width = 1;
                if (scaled_height < 1) scaled_height = 1;
                miplevel++;

                // this is a visualization tool that shades each mip map level with a different color so you can see the rasterizer's texture level selection algorithm
                // Changing the color doesn't help with lumminance/alpha/intensity formats...
                if (depth == TD.DIFFUSE && ImageManager.image_colorMipLevels.Bool) fixed (byte* blendB = &mipBlendColors[miplevel, 0]) R_BlendOverTexture(scaledBuffer, scaled_width * scaled_height, blendB);

                // upload the mip map
                //qglTexImage2D(TextureTarget.Texture2d, miplevel, internalFormat, scaled_width, scaled_height, 0, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, scaledBuffer);

                ImageProgramStringToCompressedFileName(imgName, out filename);
                ext = filename.LastIndexOf('.');
                if (ext != -1) filename = $"{filename.Remove(ext)}.e{miplevel / 10}{miplevel % 10}";
                else filename = null;
                myglTexImage2D(filename, TextureTarget.Texture2d, miplevel, internalFormat, scaled_width, scaled_height, 0, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, scaledBuffer);
            }

            if (scaledBuffer != null) R_StaticFree(scaledBuffer);

            SetImageFilterAndRepeat();

            // see if we messed anything up
            GL_CheckErrors();
        }

        // Non-square cube sides are not allowed
        public void GenerateCubeImage(byte** pic, int size, TF filterParm, bool allowDownSizeParm, TD depthParm)
        {
            int scaled_width, scaled_height, width, height, i;

            PurgeImage();

            filter = filterParm;
            allowDownSize = allowDownSizeParm;
            depth = depthParm;

            type = TT.CUBIC;

            // if we don't have a rendering context, just return after we have filled in1 the parms.  We must have the values set, or
            // an image match from a shader before OpenGL starts would miss the generated texture
            if (!glConfig.isInitialized) return;

            width = height = size;

            // generate the texture number
            fixed (uint* texnumU = &texnum) qglGenTextures(1, texnumU);

            // select proper internal format before we resample
            internalFormat = InternalFormat.Rgba;

            // don't bother with downsample for now
            scaled_width = width;
            scaled_height = height;

            uploadHeight = scaled_height;
            uploadWidth = scaled_width;

            Bind();

            // no other clamp mode makes sense
            qglTexParameteri(TextureTarget.TextureCubeMap, GetTextureParameter.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            qglTexParameteri(TextureTarget.TextureCubeMap, GetTextureParameter.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // set the minimize / maximize filtering
            switch (filter)
            {
                case TF.DEFAULT:
                    qglTexParameterf(TextureTarget.TextureCubeMap, GetTextureParameter.TextureMinFilter, globalImages.textureMinFilter);
                    qglTexParameterf(TextureTarget.TextureCubeMap, GetTextureParameter.TextureMagFilter, globalImages.textureMaxFilter);
                    break;
                case TF.LINEAR:
                    qglTexParameterf(TextureTarget.TextureCubeMap, GetTextureParameter.TextureMinFilter, (float)BlitFramebufferFilter.Linear);
                    qglTexParameterf(TextureTarget.TextureCubeMap, GetTextureParameter.TextureMagFilter, (float)BlitFramebufferFilter.Linear);
                    break;
                case TF.NEAREST:
                    qglTexParameterf(TextureTarget.TextureCubeMap, GetTextureParameter.TextureMinFilter, (float)BlitFramebufferFilter.Nearest);
                    qglTexParameterf(TextureTarget.TextureCubeMap, GetTextureParameter.TextureMagFilter, (float)BlitFramebufferFilter.Nearest);
                    break;
                default: common.FatalError("R_CreateImage: bad texture filter"); break;
            }

            // upload the base level
            // FIXME: support GL_COLOR_INDEX8_EXT?
            for (i = 0; i < 6; i++) qglTexImage2D((TextureTarget)((int)TextureTarget.TextureCubeMapPositiveX + i), 0, internalFormat, scaled_width, scaled_height, 0, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, pic[i]);

            // create and upload the mip map levels
            var shrunk = stackalloc byte*[6];
            for (i = 0; i < 6; i++) shrunk[i] = R_MipMap(pic[i], scaled_width, scaled_height, false);

            var miplevel = 1;
            while (scaled_width > 1)
            {
                for (i = 0; i < 6; i++)
                {
                    qglTexImage2D((TextureTarget)((int)TextureTarget.TextureCubeMapPositiveX + i), miplevel, internalFormat, scaled_width / 2, scaled_height / 2, 0, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, shrunk[i]);
                    var shrunken = scaled_width > 2
                        ? R_MipMap(shrunk[i], scaled_width / 2, scaled_height / 2, false)
                        : null;
                    R_StaticFree(shrunk[i]);
                    shrunk[i] = shrunken;
                }

                scaled_width >>= 1;
                scaled_height >>= 1;
                miplevel++;
            }

            // see if we messed anything up
            GL_CheckErrors();
        }

        void ImageProgramStringToCompressedFileName(string imageProg, out string fileName)
        {
            var imageProg2 = imageProg.ToCharArray();
            var fileName2 = stackalloc char[imageProg2.Length + 1];
            fixed (char* imageProg2B = imageProg2)
            {
                var imageProg2BEnd = imageProg2B + imageProg.Length;
                fixed (char* dds = "dds/") Unsafe.CopyBlock(fileName2, dds, 4);
                var f = imageProg2B + 4;
                // convert all illegal characters to underscores. this could conceivably produce a duplicated mapping, but we aren't going to worry about it
                var depth = 0;
                for (var s = imageProg2B; s != imageProg2BEnd; s++)
                    if (*s == '/' || *s == '\\' || *s == '(')
                    {
                        if (depth < 4) { *f = '/'; depth++; }
                        else *f = ' ';
                        f++;
                    }
                    else if (*s == '<' || *s == '>' || *s == ':' || *s == '|' || *s == '"' || *s == '.') { *f = '_'; f++; }
                    else if (*s == ' ' && *(f - 1) == '/') { } // ignore a space right after a slash
                    else if (*s == ')' || *s == ',') { } // always ignore these
                    else { *f = *s; f++; }
                *f = (char)0;
            }
            fileName = new string(fileName2) + ".dds";
        }

        public int NumLevelsForImageSize(int width, int height)
        {
            var numLevels = 1;
            while (width > 1 || height > 1) { numLevels++; width >>= 1; height >>= 1; }
            return numLevels;
        }

        // Absolutely every image goes through this path
        // On exit, the Image will have a valid OpenGL texture number that can be bound
        public void ActuallyLoadImage(bool fromBind)
        {
            int width, height; byte* pic = null;

            if (fromBind) { Console.WriteLine("ERROR!! CAN NOT LOAD IMAGE FROM BIND"); globalImages.AddAllocList(this); return; }

            if (cinematic != null)
            {
                var cin = cinematic.ImageForTime(cinmaticNextTime);
                if (texnum == TEXTURE_NOT_LOADED) fixed (uint* texnumU = &texnum) qglGenTextures(1, texnumU);
                if (cin.image != null) UploadScratch(cin.image, cin.imageWidth, cin.imageHeight);
                //else globalImages.blackImage.Bind();
                return;
            }

            // this is the ONLY place generatorFunction will ever be called
            if (generatorFunction != null) { generatorFunction(this); return; }

            // load the image from disk
            if (cubeFiles != CF._2D)
            {
                byte** pics = stackalloc byte*[6];
                // we don't check for pre-compressed cube images currently
                R_LoadCubeImages(imgName, cubeFiles, pics, out width, out timestamp);
                if (pics[0] == null) { common.Warning($"Couldn't load cube image: {imgName}"); MakeDefault(); return; }
                GenerateCubeImage(pics, width, filter, allowDownSize, depth);
                for (var i = 0; i < 6; i++) if (pics[i] != null) R_StaticFree(pics[i]);
            }
            else
            {
                // see if we have a pre-generated image file that is already image processed and compressed
                R_LoadImageProgram(imgName, ref pic, out width, out height, out timestamp, ref depth);
                if (pic == null) { common.Warning($"Couldn't load image: {imgName}"); MakeDefault(); return; }

                // build a hash for checking duplicate image files. NOTE: takes about 10% of image load times (SD).
                // may not be strictly necessary, but some code uses it, so let's leave it in1
                //imageHash = MD4_BlockChecksum( pic, width * height * 4 );

                GenerateImage(pic, width, height, filter, allowDownSize, repeat, depth);
                //timestamp = timestamp;

                R_StaticFree(pic);
            }
        }

        //=========================================================================================================

        // deletes the texture object, but leaves the structure so it can be reloaded
        public void PurgeImage()
        {
            if (texnum != TEXTURE_NOT_LOADED)
            {
                //Console.WriteLine($"DELETING IMAGE {texnum}");
                fixed (uint* texnumU = &texnum) qglDeleteTextures(1, texnumU);  // this should be the ONLY place it is ever called!
                texnum = TEXTURE_NOT_LOADED;
            }
        }

        // Makes this image active on the current GL texture unit. automatically enables or disables cube mapping
        // May perform file loading if the image was not preloaded. May start a background image read.
        // Automatically enables 2D mapping, cube mapping, or 3D texturing if needed
        public bool Bind()
        {
            // load the image if necessary (FIXME: not SMP safe!)
            if (texnum == TEXTURE_NOT_LOADED)
            {
                // load the image on demand here, which isn't our normal game operating mode
                ActuallyLoadImage(true);
                // Load a black image to reduce flicker
                globalImages.blackImage.Bind();
                return false;
            }

            // bump our statistic counters
            frameUsed = backEnd.frameCount;
            bindCount++;

            // bind the texture
            if (type == TT._2D) qglBindTexture(TextureTarget.Texture2d, texnum);
            else if (type == TT.CUBIC) qglBindTexture(TextureTarget.TextureCubeMap, texnum);
            return true;
        }

        // for use with fragment programs, doesn't change any enable2D/3D/cube states
        // Fragment programs explicitly say which type of map they want, so we don't need to do any enable / disable changes
        public bool BindFragment()
        {
            // load the image if necessary (FIXME: not SMP safe!)
            if (texnum == TEXTURE_NOT_LOADED)
            {
                // load the image on demand here, which isn't our normal game operating mode
                ActuallyLoadImage(true);
                return false;
            }

            // bump our statistic counters
            frameUsed = backEnd.frameCount;
            bindCount++;

            // bind the texture
            if (type == TT._2D) qglBindTexture(TextureTarget.Texture2d, texnum);
            else if (type == TT.CUBIC) qglBindTexture(TextureTarget.TextureCubeMap, texnum);
            return true;
        }

        public void CopyFramebuffer(int x, int y, int imageWidth, int imageHeight, bool useOversizedBuffer)
        {
            Bind();

            if (cvarSystem.GetCVarBool("g_lowresFullscreenFX")) { imageWidth = 512; imageHeight = 512; }

            // if the size isn't a power of 2, the image must be increased in1 size
            int potWidth, potHeight;

            potWidth = MakePowerOfTwo(imageWidth);
            potHeight = MakePowerOfTwo(imageHeight);

            // Don't do this, otherwise the Grabber gun graphics from ROE do not work properly
            //GetDownsize(imageWidth, imageHeight);
            //GetDownsize(potWidth, potHeight);

            //Disabled for OES2
            //qglReadBuffer(GL_BACK);

            // only resize if the current dimensions can't hold it at all, otherwise subview renderings could thrash this
            if ((useOversizedBuffer && (uploadWidth < potWidth || uploadHeight < potHeight)) || (!useOversizedBuffer && (uploadWidth != potWidth || uploadHeight != potHeight)))
            {
                uploadWidth = potWidth;
                uploadHeight = potHeight;
                if (potWidth == imageWidth && potHeight == imageHeight) qglCopyTexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgb, x, y, imageWidth, imageHeight, 0);
                else
                {
                    // we need to create a dummy image with power of two dimensions, then do a qglCopyTexSubImage2D of the data we want this might be a 16+ meg allocation, which could fail on _alloca
                    var junk = (byte*)Marshal.AllocHGlobal(potWidth * potHeight * 3);
                    Unsafe.InitBlock(junk, 0, (uint)(potWidth * potHeight * 3));
                    qglTexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgb, potWidth, potHeight, 0, PixelFormat.Rgb, VertexAttribPointerType.UnsignedByte, junk);
                    Marshal.FreeHGlobal((IntPtr)junk);

                    qglCopyTexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, x, y, imageWidth, imageHeight);
                }
            }
            // otherwise, just subimage upload it so that drivers can tell we are going to be changing it and don't try and do a texture compression or some other silliness
            else qglCopyTexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, x, y, imageWidth, imageHeight);

            // if the image isn't a full power of two, duplicate an extra row and/or column to fix bilerps
            if (imageWidth != potWidth) qglCopyTexSubImage2D(TextureTarget.Texture2d, 0, imageWidth, 0, x + imageWidth - 1, y, 1, imageHeight);
            if (imageHeight != potHeight) qglCopyTexSubImage2D(TextureTarget.Texture2d, 0, 0, imageHeight, x, y + imageHeight - 1, imageWidth, 1);

            qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMinFilter, (float)BlitFramebufferFilter.Linear);
            qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMagFilter, (float)BlitFramebufferFilter.Linear);

            qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapT, (float)TextureWrapMode.ClampToEdge);

            backEnd.c_copyFrameBuffer++;
        }

        // This should just be part of copyFramebuffer once we have a proper image type field
        public void CopyDepthbuffer(int x, int y, int imageWidth, int imageHeight)
        {
            Bind();

            // if the size isn't a power of 2, the image must be increased in1 size
            int potWidth, potHeight;

            potWidth = MakePowerOfTwo(imageWidth);
            potHeight = MakePowerOfTwo(imageHeight);

            if (uploadWidth != potWidth || uploadHeight != potHeight)
            {
                uploadWidth = potWidth;
                uploadHeight = potHeight;
                if (potWidth == imageWidth && potHeight == imageHeight) qglCopyTexImage2D(TextureTarget.Texture2d, 0, InternalFormat.DepthComponent, x, y, imageWidth, imageHeight, 0);
                else
                {
                    // we need to create a dummy image with power of two dimensions, then do a qglCopyTexSubImage2D of the data we want
                    qglTexImage2D(TextureTarget.Texture2d, 0, InternalFormat.DepthComponent, potWidth, potHeight, 0, (PixelFormat)InternalFormat.DepthComponent, VertexAttribPointerType.UnsignedByte, null);
                    qglCopyTexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, x, y, imageWidth, imageHeight);
                }
            }
            // otherwise, just subimage upload it so that drivers can tell we are going to be changing it and don't try and do a texture compression or some other silliness
            else qglCopyTexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, x, y, imageWidth, imageHeight);

            //qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMinFilter, (float)BlitFramebufferFilter.Linear);
            //qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMagFilter, (float)BlitFramebufferFilter.Linear);

            qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
        }

        // if rows = cols * 6, assume it is a cube map animation
        public void UploadScratch(byte* data, int cols, int rows)
        {
            int i;

            // if rows = cols * 6, assume it is a cube map animation
            if (rows == cols * 6)
            {
                if (type != TT.CUBIC) { type = TT.CUBIC; uploadWidth = -1; } // -1 for a non-sub upload

                Bind();

                rows /= 6;
                // if the scratchImage isn't in1 the format we want, specify it as a new texture
                if (cols != uploadWidth || rows != uploadHeight)
                {
                    uploadWidth = cols;
                    uploadHeight = rows;
                    // upload the base level
                    for (i = 0; i < 6; i++) qglTexImage2D((TextureTarget)((int)TextureTarget.TextureCubeMapPositiveX + i), 0, InternalFormat.Rgba, cols, rows, 0, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, data + cols * rows * 4 * i);
                }
                // otherwise, just subimage upload it so that drivers can tell we are going to be changing it and don't try and do a texture compression
                else for (i = 0; i < 6; i++) qglTexSubImage2D((TextureTarget)((int)TextureTarget.TextureCubeMapPositiveX + i), 0, 0, 0, cols, rows, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, data + cols * rows * 4 * i);
                qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMinFilter, (int)BlitFramebufferFilter.Linear);
                qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMagFilter, (int)BlitFramebufferFilter.Linear);

                // no other clamp mode makes sense
                qglTexParameteri(TextureTarget.TextureCubeMap, GetTextureParameter.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                qglTexParameteri(TextureTarget.TextureCubeMap, GetTextureParameter.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
            else
            {
                // otherwise, it is a 2D image
                if (type != TT._2D) { type = TT._2D; uploadWidth = -1; } // -1 for a non-sub upload

                Bind();

                // if the scratchImage isn't in1 the format we want, specify it as a new texture
                if (cols != uploadWidth || rows != uploadHeight)
                {
                    uploadWidth = cols;
                    uploadHeight = rows;
                    qglTexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, cols, rows, 0, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, data);
                }
                // otherwise, just subimage upload it so that drivers can tell we are going to be changing it and don't try and do a texture compression
                else qglTexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, cols, rows, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, data);
                qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMinFilter, (float)BlitFramebufferFilter.Linear);
                qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureMagFilter, (float)BlitFramebufferFilter.Linear);

#if true
                // these probably should be clamp, but we have a lot of issues with editor geometry coming out_ with texcoords slightly off one side, resulting in1 a smear across the entire polygon
                qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapS, (float)TextureWrapMode.Repeat);
                qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapT, (float)TextureWrapMode.Repeat);
#else
                // these probably should be clamp, but we have a lot of issues with editor geometry coming out_ with texcoords slightly off one side, resulting in1 a smear across the entire polygon
                qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
                qglTexParameterf(TextureTarget.Texture2d, GetTextureParameter.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
#endif
            }
        }

        // just for resource tracking
        public void SetClassification(int tag)
            => classification = tag;

        public bool IsLoaded
            => !purgePending && (texnum != TEXTURE_NOT_LOADED);

        // estimates size of the GL image based on dimensions and storage type
        public int StorageSize
        {
            get
            {
                if (texnum == TEXTURE_NOT_LOADED) return 0;
                var baseSize = type switch
                {
                    TT.CUBIC => 6 * uploadWidth * uploadHeight,
                    _ => uploadWidth * uploadHeight,
                };
                baseSize *= BitsForInternalFormat(internalFormat);
                baseSize /= 8;

                // account for mip mapping
                return baseSize * 4 / 3;
            }
        }

        // print a one line summary of the image
        public void Print()
        {
            common.Printf(generatorFunction != null ? "F" : " ");
            switch (type)
            {
                case TT._2D: common.Printf(" "); break;
                case TT.CUBIC: common.Printf("C"); break;
                case TT.RECT: common.Printf("R"); break;
                default: common.Printf($"<BAD TYPE:{type}>"); break;
            }
            common.Printf($"{uploadWidth,4} {uploadHeight,4} ");
            switch (filter)
            {
                case TF.DEFAULT: common.Printf("dflt "); break;
                case TF.LINEAR: common.Printf("linr "); break;
                case TF.NEAREST: common.Printf("nrst "); break;
                default: common.Printf($"<BAD FILTER:{filter}>"); break;
            }
            switch ((int)internalFormat)
            {
                case 1:
                case 2:
                case 3:
                case 4: common.Printf("RGBA  "); break;
                case (int)InternalFormat.Rgba4: common.Printf("RGBA4 "); break;
                case (int)InternalFormat.Rgb5A1: common.Printf("RGB5_A1  "); break;
                case 0: common.Printf("      "); break;
                default: common.Printf($"<BAD FORMAT:{internalFormat}>"); break;
            }
            switch (repeat)
            {
                case TR.REPEAT: common.Printf("rept "); break;
                case TR.CLAMP_TO_ZERO: common.Printf("zero "); break;
                case TR.CLAMP_TO_ZERO_ALPHA: common.Printf("azro "); break;
                case TR.CLAMP: common.Printf("clmp "); break;
                default: common.Printf($"<BAD REPEAT:{repeat}>"); break;
            }
            common.Printf($"{StorageSize / 1024,4}k ");
            common.Printf($" {imgName}\n");
        }
    }
}