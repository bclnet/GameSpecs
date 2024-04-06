//#define BUILD_FREETYPE
using System.Runtime.InteropServices;
using System.Text;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        const int FILESIZE_fontInfo_t = 20548;

#if BUILD_FREETYPE
        static int _FLOOR(int x) => x & -64;
        static int _CEIL(int x) => (x + 63) & -64;
        static int _TRUNC(int x) => x >> 6;
        static FT_Library ftLibrary;
#endif


#if BUILD_FREETYPE

        void R_GetGlyphInfo(FT_GlyphSlot glyph, out int left, out int right, out int width, out int top, out int bottom, out int height, out int pitch)
        {
            left = _FLOOR(glyph.metrics.horiBearingX);
            right = _CEIL(glyph.metrics.horiBearingX + glyph.metrics.width);
            width = _TRUNC(right - left);

            top = _CEIL(glyph.metrics.horiBearingY);
            bottom = _FLOOR(glyph.metrics.horiBearingY - glyph.metrics.height);
            height = _TRUNC(top - bottom);
            pitch = qtrue ? (width + 3) & -4 : (width + 7) >> 3;
        }

        FT_Bitmap R_RenderGlyph(FT_GlyphSlot glyph, GlyphInfo glyphOut)
        {
            FT_Bitmap bit2;

            R_GetGlyphInfo(glyph, out var left, out var right, out var width, out var top, out var bottom, out var height, out var pitch);

            if (glyph.format == ft_glyph_format_outline)
            {
                size = pitch * height;

                bit2 = Mem_Alloc(sizeof(FT_Bitmap));

                bit2.width = width;
                bit2.rows = height;
                bit2.pitch = pitch;
                bit2.pixel_mode = ft_pixel_mode_grays;
                //bit2.pixel_mode = ft_pixel_mode_mono;
                bit2.buffer = Mem_Alloc(pitch * height);
                bit2.num_grays = 256;

                memset(bit2.buffer, 0, size);

                FT_Outline_Translate(&glyph.outline, -left, -bottom);

                FT_Outline_Get_Bitmap(ftLibrary, &glyph.outline, bit2);

                glyphOut.height = height;
                glyphOut.pitch = pitch;
                glyphOut.top = (glyph.metrics.horiBearingY >> 6) + 1;
                glyphOut.bottom = bottom;

                return bit2;
            }
            else common.Printf("Non-outline fonts are not supported\n");
            return null;
        }

        GlyphInfo RE_ConstructGlyphInfo(byte[] imageOut, out int xOut, out int yOut, out int maxHeight, FT_Face face, char c, bool calcHeight)
        {
            int i;
            static glyphInfo_t glyph;
            unsigned char* src, *dst;
            float scaled_width, scaled_height;
            FT_Bitmap* bitmap = NULL;

            memset(&glyph, 0, sizeof(glyphInfo_t));
            // make sure everything is here
            if (face != NULL)
            {
                FT_Load_Glyph(face, FT_Get_Char_Index(face, c), FT_LOAD_DEFAULT);
                bitmap = R_RenderGlyph(face.glyph, &glyph);
                if (bitmap)
                    glyph.xSkip = (face.glyph.metrics.horiAdvance >> 6) + 1;
                else
                    return &glyph;

                if (glyph.height > *maxHeight)
                    maxHeight = glyph.height;

                if (calcHeight)
                {
                    Mem_Free(bitmap.buffer);
                    Mem_Free(bitmap);
                    return &glyph;
                }

                /*
                        // need to convert to power of 2 sizes so we do not get
                        // any scaling from the gl upload
                        for (scaled_width = 1 ; scaled_width < glyph.pitch ; scaled_width<<=1)
                            ;
                        for (scaled_height = 1 ; scaled_height < glyph.height ; scaled_height<<=1)
                            ;
                */

                scaled_width = glyph.pitch;
                scaled_height = glyph.height;

                // we need to make sure we fit
                if (*xOut + scaled_width + 1 >= 255)
                {
                    if (*yOut + *maxHeight + 1 >= 255)
                    {
                        *yOut = -1;
                        *xOut = -1;
                        Mem_Free(bitmap.buffer);
                        Mem_Free(bitmap);
                        return &glyph;
                    }
                    else
                    {
                        *xOut = 0;
                        *yOut += *maxHeight + 1;
                    }
                }
                else if (*yOut + *maxHeight + 1 >= 255)
                {
                    *yOut = -1;
                    *xOut = -1;
                    Mem_Free(bitmap.buffer);
                    Mem_Free(bitmap);
                    return &glyph;
                }

                src = bitmap.buffer;
                dst = imageOut + (*yOut * 256) + *xOut;

                if (bitmap.pixel_mode == ft_pixel_mode_mono)
                {
                    for (i = 0; i < glyph.height; i++)
                    {
                        int j;
                        unsigned char* _src = src;
                        unsigned char* _dst = dst;
                        unsigned char mask = 0x80;
                        unsigned char val = *_src;
                        for (j = 0; j < glyph.pitch; j++)
                        {
                            if (mask == 0x80)
                            {
                                val = *_src++;
                            }
                            if (val & mask)
                            {
                                *_dst = 0xff;
                            }
                            mask >>= 1;

                            if (mask == 0)
                            {
                                mask = 0x80;
                            }
                            _dst++;
                        }

                        src += glyph.pitch;
                        dst += 256;

                    }
                }
                else
                {
                    for (i = 0; i < glyph.height; i++)
                    {
                        memcpy(dst, src, glyph.pitch);
                        src += glyph.pitch;
                        dst += 256;
                    }
                }

                // we now have an 8 bit per pixel grey scale bitmap
                // that is width wide and pf.ftSize.metrics.y_ppem tall

                glyph.imageHeight = scaled_height;
                glyph.imageWidth = scaled_width;
                glyph.s = (float)*xOut / 256;
                glyph.t = (float)*yOut / 256;
                glyph.s2 = glyph.s + (float)scaled_width / 256;
                glyph.t2 = glyph.t + (float)scaled_height / 256;

                *xOut += scaled_width + 1;
            }

            Mem_Free(bitmap.buffer);
            Mem_Free(bitmap);

            return &glyph;
        }

#endif

        static int fdOffset;
        static byte[] fdFile;

        static int readInt()
        {
            var i = fdFile[fdOffset] + (fdFile[fdOffset + 1] << 8) + (fdFile[fdOffset + 2] << 16) + (fdFile[fdOffset + 3] << 24);
            fdOffset += 4;
            return i;
        }

        [StructLayout(LayoutKind.Explicit)]
        unsafe struct poor
        {
            [FieldOffset(0)] public float f;
            [FieldOffset(0)] public fixed byte b[4];
        }

        static float readFloat()
        {
            poor me = default;
            me.b[0] = fdFile[fdOffset + 0];
            me.b[1] = fdFile[fdOffset + 1];
            me.b[2] = fdFile[fdOffset + 2];
            me.b[3] = fdFile[fdOffset + 3];
            fdOffset += 4;
            return me.f;
        }

        // Loads 3 point sizes, 12, 24, and 48
        static bool RegisterFont(string fontName, out FontInfoEx font)
        {
#if BUILD_FREETYPE
            FT_Face face;
            int j, k, xOut, yOut, lastStart, imageNumber;
            int scaledSize, newSize, maxHeight, left, satLevels;
            byte[] o, imageBuff;
            GlyphInfo glyph;
            Image image;
            Material h;
            float max;
#endif
            int i, len, fontCount;
            string name;

            var pointSize = 12;

            //if (registeredFontCount >= MAX_FONTS) { common.Warning("RegisterFont: Too many fonts registered already."); return false; }
            //name = $"{fontName}/fontImage_{pointSize}.dat";
            //for (i = 0; i < registeredFontCount; i++)
            //    if (string.Equals(name, registeredFont[i].fontInfoSmall.name, StringComparison.OrdinalIgnoreCase))
            //    {
            //        memcpy(&font, &registeredFont[i], sizeof(FontInfoEx));
            //        return true;
            //    }

            font = new FontInfoEx();
            for (fontCount = 0; fontCount < 3; fontCount++)
            {
                pointSize = fontCount == 0 ? 12
                    : fontCount == 1 ? 24
                    : 48;

                // we also need to adjust the scale based on point size relative to 48 points as the ui scaling is based on a 48 point font
                var glyphScale = 1f; // change the scale to be relative to 1 based on 72 dpi ( so dpi of 144 means a scale of .5 )
                glyphScale *= 48f / pointSize;

                name = $"{fontName}/fontImage_{pointSize}.dat";

                var outFont = fontCount == 0 ? font.fontInfoSmall
                    : fontCount == 1 ? font.fontInfoMedium
                    : font.fontInfoLarge;
                outFont.name = name;

                len = fileSystem.ReadFile(name, out var ftime);
                if (len != FILESIZE_fontInfo_t) { common.Warning($"RegisterFont: couldn't find font: '{name}'"); return false; }

                fileSystem.ReadFile(name, out var faceData, out ftime);
                fdOffset = 0;
                fdFile = faceData;
                for (i = 0; i < R.GLYPHS_PER_FONT; i++)
                {
                    outFont.glyphs[i].height = readInt();
                    outFont.glyphs[i].top = readInt();
                    outFont.glyphs[i].bottom = readInt();
                    outFont.glyphs[i].pitch = readInt();
                    outFont.glyphs[i].xSkip = readInt();
                    outFont.glyphs[i].imageWidth = readInt();
                    outFont.glyphs[i].imageHeight = readInt();
                    outFont.glyphs[i].s = readFloat();
                    outFont.glyphs[i].t = readFloat();
                    outFont.glyphs[i].s2 = readFloat();
                    outFont.glyphs[i].t2 = readFloat();
                    readInt(); // font.glyphs[i].glyph = readInt();
                    outFont.glyphs[i].shaderName = Encoding.ASCII.GetString(fdFile, fdOffset + 6, 32 - 6); //FIXME: the +6, -6 skips the embedded fonts/
                    fdOffset += 32;
                }
                outFont.glyphScale = readFloat();

                int mw = 0, mh = 0;
                for (i = R.GLYPH_START; i < R.GLYPH_END; i++)
                {
                    name = $"{fontName}/{outFont.glyphs[i].shaderName}";
                    outFont.glyphs[i].glyph = declManager.FindMaterial(name);
                    outFont.glyphs[i].glyph.Sort = (float)SS.GUI;
                    if (mh < outFont.glyphs[i].height) mh = outFont.glyphs[i].height;
                    if (mw < outFont.glyphs[i].xSkip) mw = outFont.glyphs[i].xSkip;
                }
                if (fontCount == 0) { font.maxWidthSmall = mw; font.maxHeightSmall = mh; }
                else if (fontCount == 1) { font.maxWidthMedium = mw; font.maxHeightMedium = mh; }
                else { font.maxWidthLarge = mw; font.maxHeightLarge = mh; }
                fileSystem.FreeFile(faceData);
            }
            return true;

#if !BUILD_FREETYPE
            //common.Warning($"RegisterFont: couldn't load FreeType code {name}");
#else
            if (ftLibrary == null) { common.Warning("RegisterFont: FreeType not initialized."); return false; }

            len = fileSystem.ReadFile(fontName, &faceData, &ftime);
            if (len <= 0) { common.Warning("RegisterFont: Unable to read font file"); return false; }

            // allocate on the stack first in case we fail
            if (FT_New_Memory_Face(ftLibrary, faceData, len, 0, &face)) { common.Warning("RegisterFont: FreeType2, unable to allocate new face."); return false; }


            if (FT_Set_Char_Size(face, pointSize << 6, pointSize << 6, dpi, dpi)) { common.Warning("RegisterFont: FreeType2, Unable to set face char size."); return false; }

            // font = registeredFonts[registeredFontCount++];

            // make a 256x256 image buffer, once it is full, register it, clean it and keep going
            // until all glyphs are rendered

            o = Mem_Alloc(1024 * 1024);
            if (o == null) { common.Warning("RegisterFont: Mem_Alloc failure during output image creation."); return false; }
            memset(o, 0, 1024 * 1024);

            maxHeight = 0;

            for (i = R.GLYPH_START; i < R.GLYPH_END; i++)
                glyph = RE_ConstructGlyphInfo(o, &xOut, &yOut, &maxHeight, face, (byte)i, qtrue);

            xOut = 0;
            yOut = 0;
            i = R.GLYPH_START;
            lastStart = i;
            imageNumber = 0;

            while (i <= R.GLYPH_END)
            {
                glyph = RE_ConstructGlyphInfo(o, &xOut, &yOut, &maxHeight, face, (byte)i, qfalse);

                if (xOut == -1 || yOut == -1 || i == R.GLYPH_END)
                {
                    // ran out of room. we need to create an image from the bitmap, set all the handles in the glyphs to this point
                    scaledSize = 256 * 256;
                    newSize = scaledSize * 4;
                    imageBuff = Mem_Alloc(newSize);
                    left = 0;
                    max = 0;
                    satLevels = 255;
                    for (k = 0; k < (scaledSize); k++) if (max < o[k]) max = o[k];
                    if (max > 0) max = 255 / max;
                    for (k = 0; k < (scaledSize); k++)
                    {
                        imageBuff[left++] = 255;
                        imageBuff[left++] = 255;
                        imageBuff[left++] = 255;
                        imageBuff[left++] = ((float)o[k] * max);
                    }

                    name = $"fonts/fontImage_{imageNumber++}_{pointSize}.tga";
                    if (r_saveFontData.integer) R_WriteTGA(name, imageBuff, 256, 256);

                    image = R_CreateImage(name, imageBuff, 256, 256, qfalse, qfalse, GL_CLAMP);
                    h = RE_RegisterShaderFromImage(name, LIGHTMAP_2D, image, qfalse);
                    for (j = lastStart; j < i; j++)
                    {
                        font.glyphs[j].glyph = h;
                        font.glyphs[j].shaderName = name;
                    }
                    lastStart = i;
                    memset(o, 0, 1024 * 1024);
                    xOut = 0;
                    yOut = 0;
                    Mem_Free(imageBuff);
                    i++;
                }
                else
                {
                    memcpy(&font.glyphs[i], glyph, sizeof(glyphInfo_t));
                    i++;
                }
            }

            registeredFont[registeredFontCount].glyphScale = glyphScale;
            font.glyphScale = glyphScale;
            memcpy(&registeredFont[registeredFontCount++], &font, sizeof(fontInfo_t));

            if (r_saveFontData.integer) { common.Warning("FIXME: font saving doesnt respect alignment!"); fileSystem.WriteFile($"fonts/fontImage_{pointSize}.dat", &font, sizeof(fontInfo_t)); }

            Mem_Free(o);

            fileSystem.FreeFile(faceData);
            return true;
#endif
        }

        static void R_InitFreeType()
        {
#if BUILD_FREETYPE
            if (FT_Init_FreeType(&ftLibrary)) common.Printf("R_InitFreeType: Unable to initialize FreeType.\n");
#endif
        }

        public static void R_DoneFreeType()
        {
#if BUILD_FREETYPE
            if (ftLibrary != null) { FT_Done_FreeType(ftLibrary); ftLibrary = null; }
#endif
        }
    }
}
