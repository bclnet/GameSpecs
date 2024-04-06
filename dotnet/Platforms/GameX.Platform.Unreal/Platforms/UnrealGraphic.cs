using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnrealEngine.Framework;

namespace GameX.Platforms
{
    public interface IUnrealGraphic : IOpenGraphic<object, object, Texture2D, object> { }

    public class UnrealGraphic : IUnrealGraphic
    {
        readonly PakFile _source;
        readonly ITextureManager<Texture2D> _textureManager;

        public UnrealGraphic(PakFile source)
        {
            _source = source;
            _textureManager = new TextureManager<Texture2D>(source, new UnrealTextureBuilder());
        }

        public PakFile Source => _source;
        public ITextureManager<Texture2D> TextureManager => _textureManager;
        public IMaterialManager<object, Texture2D> MaterialManager => throw new NotImplementedException();
        public IObjectManager<object, object, Texture2D> ObjectManager => throw new NotImplementedException();
        public IShaderManager<object> ShaderManager => throw new NotImplementedException();
        public Texture2D LoadTexture(string path, out IDictionary<string, object> data, Range? range = null) => _textureManager.LoadTexture(path, out data, range);
        public void PreloadTexture(string path) => throw new NotImplementedException();
        public object CreateObject(string path, out IDictionary<string, object> data) => throw new NotImplementedException();
        public void PreloadObject(string path) => throw new NotImplementedException();
        public object LoadShader(string path, IDictionary<string, bool> args = null) => throw new NotImplementedException();

        public Task<T> LoadFileObject<T>(string path) => _source.LoadFileObject<T>(path);
    }
}