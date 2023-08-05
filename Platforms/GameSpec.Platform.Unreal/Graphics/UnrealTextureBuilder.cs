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
            throw new NotImplementedException();
            //Texture2D tex;
            //var bytes = info.Begin((int)FamilyPlatform.Type.Unity, out var format, out _, out _);
            //if (format is TextureUnityFormat unityFormat)
            //{
            //    var textureFormat = (TextureFormat)unityFormat;
            //    tex = new Texture2D(info.Width, info.Height, textureFormat, info.NumMipMaps, false);
            //    tex.LoadRawTextureData(bytes);
            //    tex.Apply();
            //    tex.Compress(true);
            //}
            //else if (format is ValueTuple<TextureUnityFormat> unityPixelFormat)
            //{
            //    var textureFormat = (TextureFormat)unityPixelFormat.Item1;
            //    tex = new Texture2D(info.Width, info.Height, textureFormat, info.NumMipMaps, false);
            //}
            //else throw new NotImplementedException();

            //return tex;
        }

        public override Texture2D BuildSolidTexture(int width, int height, float[] pixels)
        {
            throw new NotImplementedException();
        }

        public override Texture2D BuildNormalMap(Texture2D source, float strength)
        {
            throw new NotImplementedException();
        }

        public override void DeleteTexture(Texture2D id) { }
    }
}