using GameX.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameX.Platforms
{
    public interface IUnityGraphic : IOpenGraphic<GameObject, Material, Texture2D, Shader> { }

    public class UnityGraphic : IUnityGraphic
    {
        readonly PakFile _source;
        readonly ITextureManager<Texture2D> _textureManager;
        readonly IMaterialManager<Material, Texture2D> _materialManager;
        readonly IObjectManager<GameObject, Material, Texture2D> _objectManager;
        readonly IShaderManager<Shader> _shaderManager;

        public UnityGraphic(PakFile source)
        {
            _source = source;
            _textureManager = new TextureManager<Texture2D>(source, new UnityTextureBuilder());
            //switch (MaterialType.Default)
            //{
            //    case MaterialType.None: _material = null; break;
            //    case MaterialType.Default: _material = new DefaultMaterial(_textureManager); break;
            //    case MaterialType.Standard: _material = new StandardMaterial(_textureManager); break;
            //    case MaterialType.Unlit: _material = new UnliteMaterial(_textureManager); break;
            //    default: _material = new BumpedDiffuseMaterial(_textureManager); break;
            //}
            _materialManager = new MaterialManager<Material, Texture2D>(source, _textureManager, new BumpedDiffuseMaterialBuilder(_textureManager));
            //_objectManager = new ObjectManager<GameObject, Material, Texture2D>(source, _materialManager, new UnityObjectBuilder(0));
            _shaderManager = new ShaderManager<Shader>(source, new UnityShaderBuilder());
        }

        public PakFile Source => _source;
        public ITextureManager<Texture2D> TextureManager => _textureManager;
        public IMaterialManager<Material, Texture2D> MaterialManager => _materialManager;
        public IObjectManager<GameObject, Material, Texture2D> ObjectManager => _objectManager;
        public IShaderManager<Shader> ShaderManager => _shaderManager;
        public Texture2D LoadTexture(string path, out IDictionary<string, object> data, Range? range = null) => _textureManager.LoadTexture(path, out data, range);
        public void PreloadTexture(string path) => _textureManager.PreloadTexture(path);
        public GameObject CreateObject(string path, out IDictionary<string, object> data) => _objectManager.CreateObject(path, out data);
        public void PreloadObject(string path) => _objectManager.PreloadObject(path);
        public Shader LoadShader(string path, IDictionary<string, bool> args = null) => _shaderManager.LoadShader(path, args);

        public Task<T> LoadFileObject<T>(string path) => _source.LoadFileObject<T>(path);
    }
}