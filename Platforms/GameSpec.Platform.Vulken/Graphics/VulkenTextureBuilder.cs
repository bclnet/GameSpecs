using OpenStack.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace GameSpec.Graphics
{
    public unsafe class VulkenTextureBuilder : AbstractTextureBuilder<int>
    {
        public void Release()
        {
            if (_defaultTexture != 0)
            {
                GL.DeleteTexture(_defaultTexture);
                _defaultTexture = 0;
            }
        }

        int _defaultTexture;
        public override int DefaultTexture => _defaultTexture != 0 ? _defaultTexture : _defaultTexture = BuildAutoTexture();

        int BuildAutoTexture() => BuildSolidTexture(4, 4, new[]
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

        public override int BuildTexture(ITexture info)
        {
            var id = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, info.NumMipMaps - 1);
            info.MoveToData();

            if (info.GLFormat is TextureGLFormat glFormat)
            {
                var internalFormat = (InternalFormat)info.GLFormat;
                if (internalFormat == 0) { Console.Error.WriteLine("Unsupported texture, using default"); return DefaultTexture; }
                for (var i = info.NumMipMaps - 1; i >= 0; i--)
                {
                    var width = info.Width >> i;
                    var height = info.Height >> i;
                    var bytes = info[i];
                    if (bytes == null) return DefaultTexture;

                    fixed (byte* data = bytes) GL.CompressedTexImage2D(TextureTarget.Texture2D, i, internalFormat, width, height, 0, bytes.Length, (IntPtr)data);
                }
            }
            else if (info.GLFormat is ValueTuple<TextureGLFormat, TextureGLPixelFormat, TextureGLPixelType> glPixelFormat)
            {
                var internalFormat = (PixelInternalFormat)glPixelFormat.Item1;
                if (internalFormat == 0) { Console.Error.WriteLine("Unsupported texture, using default"); return DefaultTexture; }
                var format = (PixelFormat)glPixelFormat.Item2;
                var type = (PixelType)glPixelFormat.Item3;

                for (var i = info.NumMipMaps - 1; i >= 0; i--)
                {
                    var width = info.Width >> i;
                    var height = info.Height >> i;
                    var bytes = info[i];
                    if (bytes == null) return DefaultTexture;

                    fixed (byte* data = bytes) GL.TexImage2D(TextureTarget.Texture2D, i, internalFormat, width, height, 0, format, type, (IntPtr)data);
                }
            }
            else throw new NotImplementedException();

            if (info is IDisposable disposable) disposable.Dispose();

            if (MaxTextureMaxAnisotropy >= 4)
            {
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, MaxTextureMaxAnisotropy);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }

            var clampModeS = info.Flags.HasFlag(TextureFlags.SUGGEST_CLAMPS)
                ? TextureWrapMode.Clamp
                : TextureWrapMode.Repeat;
            var clampModeT = info.Flags.HasFlag(TextureFlags.SUGGEST_CLAMPT)
                ? TextureWrapMode.Clamp
                : TextureWrapMode.Repeat;

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)clampModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)clampModeT);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return id;
        }

        public override int BuildSolidTexture(int width, int height, float[] rgba)
        {
            var texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, PixelFormat.Rgba, PixelType.Float, rgba);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }

        public override int BuildNormalMap(int source, float strength) => throw new NotImplementedException();
    }
}