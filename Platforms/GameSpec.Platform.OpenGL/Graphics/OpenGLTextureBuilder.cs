using OpenStack.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Transactions;

namespace GameSpec.Graphics
{
    //was:Types/Renderer/MaterialLoader
    public unsafe class OpenGLTextureBuilder : AbstractTextureBuilder<int>
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

        public override int BuildTexture(ITexture info, Range? range = null)
        {
            var id = GL.GenTexture();
            var numMipMaps = Math.Max(1, info.NumMipMaps);
            var start = range?.Start.Value ?? 0;
            var end = numMipMaps - 1;

            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, end - start);
            info.MoveToData(out var forward);

            static bool CompressedTexImage2D(ITexture info, int i, InternalFormat internalFormat)
            {
                var width = info.Width >> i;
                var height = info.Height >> i;
                var pixels = info[i];
                if (pixels == null) return false;
                fixed (byte* data = pixels) GL.CompressedTexImage2D(TextureTarget.Texture2D, i, internalFormat, width, height, 0, pixels.Length, (IntPtr)data);
                return true;
            }

            static bool TexImage2D(ITexture info, int i, PixelInternalFormat internalFormat, PixelFormat format, PixelType type)
            {
                var width = info.Width >> i;
                var height = info.Height >> i;
                var pixels = info[i];
                if (pixels == null) return false;
                fixed (byte* data = pixels) GL.TexImage2D(TextureTarget.Texture2D, i, internalFormat, width, height, 0, format, type, (IntPtr)data);
                return true;
            }

            for (var i = 0; i < start; i++) { var _ = info[i]; }
            if (info.GLFormat is TextureGLFormat glFormat)
            {
                var internalFormat = (InternalFormat)glFormat;
                if (internalFormat == 0) { Console.Error.WriteLine("Unsupported texture, using default"); return DefaultTexture; }
                if (forward) for (var i = start; i <= end; i++) { if (!CompressedTexImage2D(info, i, internalFormat)) return DefaultTexture; }
                else for (var i = end; i >= start; i--) { if (!CompressedTexImage2D(info, i, internalFormat)) return DefaultTexture; }
            }
            else if (info.GLFormat is ValueTuple<TextureGLFormat, TextureGLPixelFormat, TextureGLPixelType> glPixelFormat)
            {
                var internalFormat = (PixelInternalFormat)glPixelFormat.Item1;
                if (internalFormat == 0) { Console.Error.WriteLine("Unsupported texture, using default"); return DefaultTexture; }
                var format = (PixelFormat)glPixelFormat.Item2;
                var type = (PixelType)glPixelFormat.Item3;
                if (forward) for (var i = start; i < numMipMaps; i++) { if (!TexImage2D(info, i, internalFormat, format, type)) return DefaultTexture; }
                else for (var i = end; i >= start; i--) { if (!TexImage2D(info, i, internalFormat, format, type)) return DefaultTexture; }
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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)(info.Flags.HasFlag(TextureFlags.SUGGEST_CLAMPS) ? TextureWrapMode.Clamp : TextureWrapMode.Repeat));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)(info.Flags.HasFlag(TextureFlags.SUGGEST_CLAMPT) ? TextureWrapMode.Clamp : TextureWrapMode.Repeat));
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return id;
        }

        public override int BuildSolidTexture(int width, int height, float[] pixels)
        {
            var id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, PixelFormat.Rgba, PixelType.Float, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return id;
        }

        public override int BuildNormalMap(int source, float strength) => throw new NotImplementedException();

        public override void DeleteTexture(int id) => GL.DeleteTexture(id);
    }
}