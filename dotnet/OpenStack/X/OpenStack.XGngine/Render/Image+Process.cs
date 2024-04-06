using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class Image
    {
        // Used to resample images in a more general than quartering fashion.
        // This will only have filter coverage if the resampled size is greater than half the original size.
        // If a larger shrinking is needed, use the mipmap function after resampling to the next lower power of two.
        const int MAX_DIMENSION = 4096;
        static byte* R_ResampleTexture(byte* in1, int inwidth, int inheight, int outwidth, int outheight)
        {
            int i, j; byte* inrow, inrow2; uint frac, fracstep; byte* pix1, pix2, pix3, pix4, out_, out_p;
            uint* p1 = stackalloc uint[MAX_DIMENSION], p2 = stackalloc uint[MAX_DIMENSION];

            if (outwidth > MAX_DIMENSION) outwidth = MAX_DIMENSION;
            if (outheight > MAX_DIMENSION) outheight = MAX_DIMENSION;

            out_ = (byte*)R_StaticAlloc(outwidth * outheight * 4);
            out_p = out_;

            fracstep = (uint)(inwidth * 0x10000 / outwidth);

            frac = fracstep >> 2;
            for (i = 0; i < outwidth; i++) { p1[i] = 4 * (frac >> 16); frac += fracstep; }
            frac = 3 * (fracstep >> 2);
            for (i = 0; i < outwidth; i++) { p2[i] = 4 * (frac >> 16); frac += fracstep; }

            for (i = 0; i < outheight; i++, out_p += outwidth * 4)
            {
                inrow = in1 + 4 * inwidth * (int)((i + 0.25f) * inheight / outheight);
                inrow2 = in1 + 4 * inwidth * (int)((i + 0.75f) * inheight / outheight);
                frac = fracstep >> 1;
                for (j = 0; j < outwidth; j++)
                {
                    pix1 = inrow + p1[j];
                    pix2 = inrow + p2[j];
                    pix3 = inrow2 + p1[j];
                    pix4 = inrow2 + p2[j];
                    out_p[j * 4 + 0] = (byte)((pix1[0] + pix2[0] + pix3[0] + pix4[0]) >> 2);
                    out_p[j * 4 + 1] = (byte)((pix1[1] + pix2[1] + pix3[1] + pix4[1]) >> 2);
                    out_p[j * 4 + 2] = (byte)((pix1[2] + pix2[2] + pix3[2] + pix4[2]) >> 2);
                    out_p[j * 4 + 3] = (byte)((pix1[3] + pix2[3] + pix3[3] + pix4[3]) >> 2);
                }
            }

            return out_;
        }

        // Used to resample images in a more general than quartering fashion. Normal maps and such should not be bilerped.
        static byte* R_Dropsample(byte* in1, int inwidth, int inheight, int outwidth, int outheight)
        {
            int i, j, k; byte* inrow, pix1, out_, out_p;

            out_ = (byte*)R_StaticAlloc(outwidth * outheight * 4);
            out_p = out_;

            for (i = 0; i < outheight; i++, out_p += outwidth * 4)
            {
                inrow = in1 + 4 * inwidth * (int)((i + 0.25) * inheight / outheight);
                for (j = 0; j < outwidth; j++)
                {
                    k = j * inwidth / outwidth;
                    pix1 = inrow + k * 4;
                    out_p[j * 4 + 0] = pix1[0];
                    out_p[j * 4 + 1] = pix1[1];
                    out_p[j * 4 + 2] = pix1[2];
                    out_p[j * 4 + 3] = pix1[3];
                }
            }

            return out_;
        }

        internal static void R_SetBorderTexels(byte* inBase, int width, int height, byte* border)
        {
            int i; byte* out_;

            out_ = inBase;
            for (i = 0; i < height; i++, out_ += width * 4)
            {
                out_[0] = border[0];
                out_[1] = border[1];
                out_[2] = border[2];
                out_[3] = border[3];
            }
            out_ = inBase + (width - 1) * 4;
            for (i = 0; i < height; i++, out_ += width * 4)
            {
                out_[0] = border[0];
                out_[1] = border[1];
                out_[2] = border[2];
                out_[3] = border[3];
            }
            out_ = inBase;
            for (i = 0; i < width; i++, out_ += 4)
            {
                out_[0] = border[0];
                out_[1] = border[1];
                out_[2] = border[2];
                out_[3] = border[3];
            }
            out_ = inBase + width * 4 * (height - 1);
            for (i = 0; i < width; i++, out_ += 4)
            {
                out_[0] = border[0];
                out_[1] = border[1];
                out_[2] = border[2];
                out_[3] = border[3];
            }
        }

        static void R_SetBorderTexels3D(byte* inBase, int width, int height, int depth, byte* border)
        {
            int i, j; byte* out_; int row, plane;

            row = width * 4;
            plane = row * depth;

            for (j = 1; j < depth - 1; j++)
            {
                out_ = inBase + j * plane;
                for (i = 0; i < height; i++, out_ += row)
                {
                    out_[0] = border[0];
                    out_[1] = border[1];
                    out_[2] = border[2];
                    out_[3] = border[3];
                }
                out_ = inBase + (width - 1) * 4 + j * plane;
                for (i = 0; i < height; i++, out_ += row)
                {
                    out_[0] = border[0];
                    out_[1] = border[1];
                    out_[2] = border[2];
                    out_[3] = border[3];
                }
                out_ = inBase + j * plane;
                for (i = 0; i < width; i++, out_ += 4)
                {
                    out_[0] = border[0];
                    out_[1] = border[1];
                    out_[2] = border[2];
                    out_[3] = border[3];
                }
                out_ = inBase + width * 4 * (height - 1) + j * plane;
                for (i = 0; i < width; i++, out_ += 4)
                {
                    out_[0] = border[0];
                    out_[1] = border[1];
                    out_[2] = border[2];
                    out_[3] = border[3];
                }
            }

            out_ = inBase;
            for (i = 0; i < plane; i += 4, out_ += 4)
            {
                out_[0] = border[0];
                out_[1] = border[1];
                out_[2] = border[2];
                out_[3] = border[3];
            }
            out_ = inBase + (depth - 1) * plane;
            for (i = 0; i < plane; i += 4, out_ += 4)
            {
                out_[0] = border[0];
                out_[1] = border[1];
                out_[2] = border[2];
                out_[3] = border[3];
            }
        }

        // If any of the angles inside the cone would directly reflect to the light, there will be a specular highlight.  The intensity of the highlight is inversely proportional to the
        // area of the spread.
        // Light source area is important for the base size.
        // area subtended in light is the divergence times the distance
        // Shininess value is subtracted from the divergence
        // Sets the alpha channel to the greatest divergence dot product of the surrounding texels. 1.0 = flat, 0.0 = turns a 90 degree angle
        // Lower values give less shiny specular. With mip maps, the lowest samnpled value will be retained
        // Should we rewrite the normal as the centered average?
        static void R_SetAlphaNormalDivergence(byte* in1, int width, int height)
        {
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    // the divergence is the smallest dot product of any of the eight surrounding texels
                    byte* pic_p = in1 + (y * width + x) * 4;
                    Vector3 center;
                    center.x = (pic_p[0] - 128) / 127;
                    center.y = (pic_p[1] - 128) / 127;
                    center.z = (pic_p[2] - 128) / 127;
                    center.Normalize();

                    var maxDiverge = 1f;

                    // FIXME: this assumes wrap mode, but should handle clamp modes and border colors
                    for (var yy = -1; yy <= 1; yy++)
                        for (var xx = -1; xx <= 1; xx++)
                        {
                            if (yy == 0 && xx == 0) continue;
                            var corner_p = in1 + (((y + yy) & (height - 1)) * width + ((x + xx) & (width - 1))) * 4;
                            Vector3 corner;
                            corner.x = (corner_p[0] - 128) / 127;
                            corner.y = (corner_p[1] - 128) / 127;
                            corner.z = (corner_p[2] - 128) / 127;
                            corner.Normalize();

                            var diverge = corner * center;
                            if (diverge < maxDiverge) maxDiverge = diverge;
                        }

                    // we can get a diverge < 0 in some extreme cases
                    if (maxDiverge < 0) maxDiverge = 0;
                    pic_p[3] = (byte)(maxDiverge * 255);
                }
        }

        // Returns a new copy of the texture, quartered in size and filtered. The alpha channel is taken to be the minimum of the dots of all surrounding normals.
        static int MIP_MIN(int a, int b) => a < b ? a : b;

        static byte* R_MipMapWithAlphaSpecularity(byte* in1, int width, int height)
        {
            int i, j, c, x, y, sx, sy; byte* in_p; byte* out_, out_p; int newWidth, newHeight; float* fbuf_p;

            if (width < 1 || height < 1 || (width + height == 2)) common.FatalError($"R_MipMapWithAlphaMin called with size {width},{height}");

            // convert the incoming texture to centered floating point
            c = width * height;
            float* fbuf = stackalloc float[c * 4];
            in_p = in1;
            fbuf_p = fbuf;
            for (i = 0; i < c; i++, in_p += 4, fbuf_p += 4)
            {
                fbuf_p[0] = in_p[0] / 255f * 2f - 1f;  // convert to a normal
                fbuf_p[1] = in_p[1] / 255f * 2f - 1f;
                fbuf_p[2] = in_p[2] / 255f * 2f - 1f;
                fbuf_p[3] = in_p[3] / 255f;              // filtered divegence / specularity
            }

            newWidth = width >> 1;
            newHeight = height >> 1;
            if (newWidth == 0) newWidth = 1;
            if (newHeight == 0) newHeight = 1;
            out_ = (byte*)R_StaticAlloc(newWidth * newHeight * 4);
            out_p = out_;

            in_p = in1;

            for (i = 0; i < newHeight; i++)
                for (j = 0; j < newWidth; j++, out_p += 4)
                {
                    Vector3 total = default; float totalSpec;
                    total.Zero();
                    totalSpec = 0;
                    // find the average normal
                    for (x = -1; x <= 1; x++)
                    {
                        sx = (j * 2 + x) & (width - 1);
                        for (y = -1; y <= 1; y++)
                        {
                            sy = (i * 2 + y) & (height - 1);
                            fbuf_p = fbuf + (sy * width + sx) * 4;

                            total.x += fbuf_p[0];
                            total.y += fbuf_p[1];
                            total.z += fbuf_p[2];
                            totalSpec += fbuf_p[3];
                        }
                    }
                    total.Normalize();
                    totalSpec /= 9f;

                    // find the maximum divergence
                    //for (x = -1; x <= 1; x++) for (y = -1; y <= 1; y++) { }
                    // store the average normal and divergence
                }

            return out_;
        }

        //Returns a new copy of the texture, quartered in size and filtered.
        //If a texture is intended to be used in GL_CLAMP or GL_CLAMP_TO_EDGE mode with a completely transparent border, we must prevent any blurring into the outer
        //ring of texels by filling it with the border from the previous level.  This will result in a slight shrinking of the texture as it mips, but better than
        //smeared clamps...
        internal static byte* R_MipMap(byte* in1, int width, int height, bool preserveBorder)
        {
            int i, j; byte* in_p, out_, out_p; int row; int newWidth, newHeight;
            byte* border = stackalloc byte[4];

            if (width < 1 || height < 1 || (width + height == 2)) common.FatalError($"R_MipMap called with size {width},{height}");

            border[0] = in1[0];
            border[1] = in1[1];
            border[2] = in1[2];
            border[3] = in1[3];

            row = width * 4;

            newWidth = width >> 1;
            newHeight = height >> 1;
            if (newWidth == 0) newWidth = 1;
            if (newHeight == 0) newHeight = 1;
            out_ = (byte*)R_StaticAlloc(newWidth * newHeight * 4);
            out_p = out_;

            in_p = in1;

            width >>= 1;
            height >>= 1;

            if (width == 0 || height == 0)
            {
                width += height;    // get largest
                if (preserveBorder)
                    for (i = 0; i < width; i++, out_p += 4)
                    {
                        out_p[0] = border[0];
                        out_p[1] = border[1];
                        out_p[2] = border[2];
                        out_p[3] = border[3];
                    }
                else
                    for (i = 0; i < width; i++, out_p += 4, in_p += 8)
                    {
                        out_p[0] = (byte)((in_p[0] + in_p[4]) >> 1);
                        out_p[1] = (byte)((in_p[1] + in_p[5]) >> 1);
                        out_p[2] = (byte)((in_p[2] + in_p[6]) >> 1);
                        out_p[3] = (byte)((in_p[3] + in_p[7]) >> 1);
                    }
                return out_;
            }

            for (i = 0; i < height; i++, in_p += row)
                for (j = 0; j < width; j++, out_p += 4, in_p += 8)
                {
                    out_p[0] = (byte)((in_p[0] + in_p[4] + in_p[row + 0] + in_p[row + 4]) >> 2);
                    out_p[1] = (byte)((in_p[1] + in_p[5] + in_p[row + 1] + in_p[row + 5]) >> 2);
                    out_p[2] = (byte)((in_p[2] + in_p[6] + in_p[row + 2] + in_p[row + 6]) >> 2);
                    out_p[3] = (byte)((in_p[3] + in_p[7] + in_p[row + 3] + in_p[row + 7]) >> 2);
                }

            // copy the old border texel back around if desired
            if (preserveBorder) R_SetBorderTexels(out_, width, height, border);

            return out_;
        }

        //Returns a new copy of the texture, eigthed in size and filtered.
        //If a texture is intended to be used in GL_CLAMP or GL_CLAMP_TO_EDGE mode with a completely transparent border, we must prevent any blurring into the outer
        //ring of texels by filling it with the border from the previous level.  This will result in a slight shrinking of the texture as it mips, but better than
        //smeared clamps...
        static byte* R_MipMap3D(byte* in1, int width, int height, int depth, bool preserveBorder)
        {
            int i, j, k; byte* in_p, out_, out_p; int row, plane, newWidth, newHeight, newDepth;
            byte* border = stackalloc byte[4];

            if (depth == 1) return R_MipMap(in1, width, height, preserveBorder);

            // assume symetric for now
            if (width < 2 || height < 2 || depth < 2) common.FatalError($"R_MipMap3D called with size {width},{height},{depth}");

            border[0] = in1[0];
            border[1] = in1[1];
            border[2] = in1[2];
            border[3] = in1[3];

            row = width * 4;
            plane = row * height;

            newWidth = width >> 1;
            newHeight = height >> 1;
            newDepth = depth >> 1;

            out_ = (byte*)R_StaticAlloc(newWidth * newHeight * newDepth * 4);
            out_p = out_;

            in_p = in1;

            width >>= 1;
            height >>= 1;
            depth >>= 1;

            for (k = 0; k < depth; k++, in_p += plane)
                for (i = 0; i < height; i++, in_p += row)
                    for (j = 0; j < width; j++, out_p += 4, in_p += 8)
                    {
                        out_p[0] = (byte)((in_p[0] + in_p[4] + in_p[row + 0] + in_p[row + 4] + in_p[plane + 0] + in_p[plane + 4] + in_p[plane + row + 0] + in_p[plane + row + 4]) >> 3);
                        out_p[1] = (byte)((in_p[1] + in_p[5] + in_p[row + 1] + in_p[row + 5] + in_p[plane + 1] + in_p[plane + 5] + in_p[plane + row + 1] + in_p[plane + row + 5]) >> 3);
                        out_p[2] = (byte)((in_p[2] + in_p[6] + in_p[row + 2] + in_p[row + 6] + in_p[plane + 2] + in_p[plane + 6] + in_p[plane + row + 2] + in_p[plane + row + 6]) >> 3);
                        out_p[3] = (byte)((in_p[3] + in_p[7] + in_p[row + 3] + in_p[row + 7] + in_p[plane + 3] + in_p[plane + 6] + in_p[plane + row + 3] + in_p[plane + row + 6]) >> 3);
                    }

            // copy the old border texel back around if desired
            if (preserveBorder) R_SetBorderTexels3D(out_, width, height, depth, border);

            return out_;
        }

        // Apply a color blend over a set of pixels
        internal static void R_BlendOverTexture(byte* data, int pixelCount, byte* blend)
        {
            int i, inverseAlpha;
            int* premult = stackalloc int[3];

            inverseAlpha = 255 - blend[3];
            premult[0] = blend[0] * blend[3];
            premult[1] = blend[1] * blend[3];
            premult[2] = blend[2] * blend[3];

            for (i = 0; i < pixelCount; i++, data += 4)
            {
                data[0] = (byte)((data[0] * inverseAlpha + premult[0]) >> 9);
                data[1] = (byte)((data[1] * inverseAlpha + premult[1]) >> 9);
                data[2] = (byte)((data[2] * inverseAlpha + premult[2]) >> 9);
            }
        }

        // Flip the image in place
        internal static void R_HorizontalFlip(byte* data, int width, int height)
        {
            int i, j, temp;

            for (i = 0; i < height; i++)
                for (j = 0; j < width / 2; j++)
                {
                    temp = *((int*)data + i * width + j);
                    *((int*)data + i * width + j) = *((int*)data + i * width + width - 1 - j);
                    *((int*)data + i * width + width - 1 - j) = temp;
                }
        }

        internal static void R_VerticalFlip(byte* data, int width, int height)
        {
            int i, j, temp;

            for (i = 0; i < width; i++)
                for (j = 0; j < height / 2; j++)
                {
                    temp = *((int*)data + j * width + i);
                    *((int*)data + j * width + i) = *((int*)data + (height - 1 - j) * width + i);
                    *((int*)data + (height - 1 - j) * width + i) = temp;
                }
        }

        internal static void R_RotatePic(byte* data, int width)
        {
            int i, j;

            int* temp = (int*)R_StaticAlloc(width * width * 4);
            for (i = 0; i < width; i++) for (j = 0; j < width; j++) *(temp + i * width + j) = *((int*)data + j * width + i);
            Unsafe.CopyBlock(data, temp, (uint)(width * width * 4));
            R_StaticFree((byte*)temp);
        }
    }
}