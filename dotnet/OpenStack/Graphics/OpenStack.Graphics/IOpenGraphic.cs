using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[assembly: InternalsVisibleTo("OpenStack.Graphics.OpenGL")]

namespace OpenStack.Graphics
{
    /// <summary>
    /// IOpenGraphic
    /// </summary>
    public interface IOpenGraphic
    {
        //object Source { get; }
        Task<T> LoadFileObject<T>(string path);
        void PreloadTexture(string texturePath);
        void PreloadObject(string filePath);
    }

    /// <summary>
    /// IOpenGraphic
    /// </summary>
    public interface IOpenGraphic<Object, Material, Texture, Shader> : IOpenGraphic
    {
        ITextureManager<Texture> TextureManager { get; }
        IMaterialManager<Material, Texture> MaterialManager { get; }
        IObjectManager<Object, Material, Texture> ObjectManager { get; }
        IShaderManager<Shader> ShaderManager { get; }
        Texture LoadTexture(string path, out IDictionary<string, object> data, Range? rng = null);
        Object CreateObject(string path, out IDictionary<string, object> data);
        Shader LoadShader(string path, IDictionary<string, bool> args = null);
    }
}