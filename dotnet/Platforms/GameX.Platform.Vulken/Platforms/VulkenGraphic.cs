using OpenStack.Graphics;
using OpenStack.Graphics.OpenGL.Renderer1;
using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameX.Platforms
{
    public interface IVulkenGraphic : IOpenGraphic<object, GLRenderMaterial, int, Shader> { }

    public class VulkenGraphic : IVulkenGraphic
    {
        readonly PakFile _source;
        readonly TextureManager<int> _textureManager;
        readonly MaterialManager<GLRenderMaterial, int> _materialManager;
        readonly ObjectManager<object, GLRenderMaterial, int> _objectManager;
        readonly ShaderManager<Shader> _shaderManager;

        public VulkenGraphic(PakFile source)
        {
            _source = source;
            _textureManager = new TextureManager<int>(source, new VulkenTextureBuilder());
            _materialManager = new MaterialManager<GLRenderMaterial, int>(source, _textureManager, new VulkenMaterialBuilder(_textureManager));
            _objectManager = new ObjectManager<object, GLRenderMaterial, int>(source, _materialManager, new VulkenObjectBuilder());
            _shaderManager = new ShaderManager<Shader>(source, new VulkenShaderBuilder());
            MeshBufferCache = new GLMeshBufferCache();
        }

        public PakFile Source => _source;
        public ITextureManager<int> TextureManager => _textureManager;
        public IMaterialManager<GLRenderMaterial, int> MaterialManager => _materialManager;
        public IObjectManager<object, GLRenderMaterial, int> ObjectManager => _objectManager;
        public IShaderManager<Shader> ShaderManager => _shaderManager;
        public int LoadTexture(string path, out IDictionary<string, object> data, Range? range = null) => _textureManager.LoadTexture(path, out data, range);
        public void PreloadTexture(string path) => _textureManager.PreloadTexture(path);
        public object CreateObject(string path, out IDictionary<string, object> data) => _objectManager.CreateObject(path, out data);
        public void PreloadObject(string path) => _objectManager.PreloadObject(path);
        public Shader LoadShader(string path, IDictionary<string, bool> args = null) => _shaderManager.LoadShader(path, args);

        public Task<T> LoadFileObject<T>(string path) => _source.LoadFileObject<T>(path);

        // cache
        QuadIndexBuffer _quadIndices;
        public QuadIndexBuffer QuadIndices => _quadIndices != null ? _quadIndices : _quadIndices = new QuadIndexBuffer(65532);
        public GLMeshBufferCache MeshBufferCache { get; }
    }
}