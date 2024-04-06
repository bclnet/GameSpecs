using OpenStack.Graphics;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameX.Platforms
{
    public interface IStereoKitGraphic : IOpenGraphic<object, Material, Tex, Shader> { }

    public class StereoKitGraphic : IStereoKitGraphic
    {
        readonly PakFile _source;
        readonly TextureManager<Tex> _textureManager;
        readonly MaterialManager<Material, Tex> _materialManager;
        readonly ObjectManager<object, Material, Tex> _objectManager;
        readonly ShaderManager<Shader> _shaderManager;

        public StereoKitGraphic(PakFile source)
        {
            _source = source;
            _textureManager = new TextureManager<Tex>(source, new StereoKitTextureBuilder());
            _materialManager = new MaterialManager<Material, Tex>(source, _textureManager, new StereoKitMaterialBuilder(_textureManager));
            _objectManager = new ObjectManager<object, Material, Tex>(source, _materialManager, new StereoKitObjectBuilder());
            _shaderManager = new ShaderManager<Shader>(source, new StereoKitShaderBuilder());
        }

        public PakFile Source => _source;
        public ITextureManager<Tex> TextureManager => _textureManager;
        public IMaterialManager<Material, Tex> MaterialManager => _materialManager;
        public IObjectManager<object, Material, Tex> ObjectManager => _objectManager;
        public IShaderManager<Shader> ShaderManager => _shaderManager;
        public Tex LoadTexture(string path, out IDictionary<string, object> data, Range? range = null) => _textureManager.LoadTexture(path, out data, range);
        public void PreloadTexture(string path) => _textureManager.PreloadTexture(path);
        public object CreateObject(string path, out IDictionary<string, object> data) => _objectManager.CreateObject(path, out data);
        public void PreloadObject(string path) => _objectManager.PreloadObject(path);
        public Shader LoadShader(string path, IDictionary<string, bool> args = null) => _shaderManager.LoadShader(path, args);

        public Task<T> LoadFileObject<T>(string path) => _source.LoadFileObject<T>(path);
    }
}