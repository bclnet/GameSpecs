using OpenStack.Graphics;
using System;
using UnityEngine;

namespace GameSpec.Graphics
{
    public class UnityTextureBuilder : AbstractTextureBuilder<Texture2D>
    {
        Texture2D _defaultTexture;
        public override Texture2D DefaultTexture => _defaultTexture != null ? _defaultTexture : _defaultTexture = BuildAutoTexture();

        Texture2D BuildAutoTexture() => new Texture2D(4, 4);

        public override Texture2D BuildTexture(ITexture info)
        {
            var tex = new Texture2D(info.Width, info.Height, (TextureFormat)info.UnityFormat, info.NumMipMaps, false);
            //if (info.Bytes != null)
            //{
            //    tex.LoadRawTextureData(info.Bytes);
            //    tex.Apply();
            //    tex.Compress(true);
            //}
            return tex;
        }

        public override Texture2D BuildSolidTexture(int width, int height, float[] rgba) => throw new NotImplementedException();

        public override Texture2D BuildNormalMap(Texture2D source, float strength)
        {
            strength = Mathf.Clamp(strength, 0.0F, 1.0F);
            float xLeft, xRight, yUp, yDown, yDelta, xDelta;
            var normalTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);
            for (var y = 0; y < normalTexture.height; y++)
                for (var x = 0; x < normalTexture.width; x++)
                {
                    xLeft = source.GetPixel(x - 1, y).grayscale * strength;
                    xRight = source.GetPixel(x + 1, y).grayscale * strength;
                    yUp = source.GetPixel(x, y - 1).grayscale * strength;
                    yDown = source.GetPixel(x, y + 1).grayscale * strength;
                    xDelta = (xLeft - xRight + 1) * 0.5f;
                    yDelta = (yUp - yDown + 1) * 0.5f;
                    normalTexture.SetPixel(x, y, new UnityEngine.Color(xDelta, yDelta, 1.0f, yDelta));
                }
            normalTexture.Apply();
            return normalTexture;
        }
    }
}