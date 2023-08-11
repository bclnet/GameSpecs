using OpenStack.Graphics;
using System;
using UnrealEngine.Framework;

namespace GameSpec.Graphics
{
    public class UnrealTextureBuilder : AbstractTextureBuilder<Texture2D>
    {
        Texture2D _defaultTexture;
        public override Texture2D DefaultTexture => _defaultTexture ??= BuildAutoTexture();

        Texture2D BuildAutoTexture() => BuildSolidTexture(4, 4, new[]
        {
            0.9f, 0.2f, 0.8f, 1f,
            0f, 0.9f, 0f, 1f,
            0.9f, 0.2f, 0.8f, 1f,
            0f, 0.9f, 0f, 1f,

            0f, 0.9f, 0f, 1f,
            0.9f, 0.2f, 0.8f, 1f,
            0f, 0.9f, 0f, 1f,
            0.9f, 0.2f, 0.8f, 1f,

            0.9f, 0.2f, 0.8f, 1f,
            0f, 0.9f, 0f, 1f,
            0.9f, 0.2f, 0.8f, 1f,
            0f, 0.9f, 0f, 1f,

            0f, 0.9f, 0f, 1f,
            0.9f, 0.2f, 0.8f, 1f,
            0f, 0.9f, 0f, 1f,
            0.9f, 0.2f, 0.8f, 1f,
        });

        public override Texture2D BuildTexture(ITexture info, Range? range = null)
        {
            var bytes = info.Begin((int)FamilyPlatform.Type.Unreal, out var format, out _);
            if (format is TextureUnrealFormat unrealFormat)
            {
                var pixelFormat = (PixelFormat)unrealFormat;
                //var tex = new Texture2D(info.Width, info.Height, pixelFormat, "Name");
                return null;
                //return tex;
            }
            else throw new ArgumentOutOfRangeException(nameof(format), $"{format}");
        }

        public override Texture2D BuildSolidTexture(int width, int height, float[] pixels)
        {
            return null;
        }

        public override Texture2D BuildNormalMap(Texture2D source, float strength)
        {
            throw new NotImplementedException();
        }

        public override void DeleteTexture(Texture2D id) { }
    }
}