using GameSpec.Graphics;
using OpenStack.Graphics.OpenGL;
using System.Collections.Generic;
using OpenStack;
using OpenStack.Graphics;
using System.Threading.Tasks;
using OpenStack.Graphics.Renderer;

namespace GameSpec
{
    //public interface IOpenGLGraphic : IEstateGraphic<object, Material, int, Shader> { }

    public class OpenGLGraphic : IOpenGLGraphic
    {
        readonly PakFile _source;
        readonly TextureManager<int> _textureManager;
        readonly MaterialManager<Material, int> _materialManager;
        readonly ObjectManager<object, Material, int> _objectManager;
        readonly ShaderManager<Shader> _shaderManager;

        public OpenGLGraphic(PakFile source)
        {
            _source = source;
            _textureManager = new TextureManager<int>(source, new OpenGLTextureBuilder());
            _materialManager = new MaterialManager<Material, int>(source, _textureManager, new OpenGLMaterialBuilder(_textureManager));
            _objectManager = new ObjectManager<object, Material, int>(source, _materialManager, new OpenGLObjectBuilder());
            _shaderManager = new ShaderManager<Shader>(source, new OpenGLShaderBuilder());
            MeshBufferCache = new GpuMeshBufferCache();
        }

        public PakFile Source => _source;
        public ITextureManager<int> TextureManager => _textureManager;
        public IMaterialManager<Material, int> MaterialManager => _materialManager;
        public IShaderManager<Shader> ShaderManager => _shaderManager;
        public int LoadTexture(string path, out IDictionary<string, object> data) => _textureManager.LoadTexture(path, out data);
        public void PreloadTexture(string path) => _textureManager.PreloadTexture(path);
        public object CreateObject(string path, out IDictionary<string, object> data) => _objectManager.CreateObject(path, out data);
        public void PreloadObject(string path) => _objectManager.PreloadObject(path);
        public Shader LoadShader(string path, IDictionary<string, bool> args = null) => _shaderManager.LoadShader(path, args);

        public Task<T> LoadFileObjectAsync<T>(string path) => _source.LoadFileObjectAsync<T>(path);

        // cache
        QuadIndexBuffer _quadIndices;
        public QuadIndexBuffer QuadIndices => _quadIndices != null ? _quadIndices : _quadIndices = new QuadIndexBuffer(65532);
        public GpuMeshBufferCache MeshBufferCache { get; }
    }
}