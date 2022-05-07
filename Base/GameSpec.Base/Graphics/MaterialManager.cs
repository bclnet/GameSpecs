using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Graphics
{
    /// <summary>
    /// Manages loading and instantiation of materials.
    /// </summary>
    public class MaterialManager<Material, Texture> : IMaterialManager<Material, Texture>
    {
        readonly PakFile _pakFile;
        readonly AbstractMaterialBuilder<Material, Texture> _builder;
        readonly Dictionary<object, (Material material, IDictionary<string, object> data)> _cachedMaterials = new Dictionary<object, (Material material, IDictionary<string, object> data)>();
        readonly Dictionary<object, Task<IMaterialInfo>> _preloadTasks = new Dictionary<object, Task<IMaterialInfo>>();

        public ITextureManager<Texture> TextureManager { get; }

        public MaterialManager(PakFile pakFile, ITextureManager<Texture> textureManager, AbstractMaterialBuilder<Material, Texture> builder)
        {
            _pakFile = pakFile;
            TextureManager = textureManager;
            _builder = builder;
        }

        public Material LoadMaterial(object key, out IDictionary<string, object> data)
        {
            if (_cachedMaterials.TryGetValue(key, out var cache)) { data = cache.data; return cache.material; }
            // Load & cache the material.
            var info = key is IMaterialInfo z ? z : LoadMaterialInfo(key);
            var material = info != null ? _builder.BuildMaterial(info) : _builder.DefaultMaterial;
            data = info?.Data;
            _cachedMaterials[key] = (material, data);
            return material;
        }

        public void PreloadMaterial(string path)
        {
            if (_cachedMaterials.ContainsKey(path)) return;
            // Start loading the material file asynchronously if we haven't already started.
            if (!_preloadTasks.ContainsKey(path)) _preloadTasks[path] = _pakFile.LoadFileObjectAsync<IMaterialInfo>(path);
        }

        IMaterialInfo LoadMaterialInfo(object key)
        {
            Assert(!_cachedMaterials.ContainsKey(key));
            switch (key)
            {
                case string path:
                    PreloadMaterial(path);
                    var info = _preloadTasks[key].Result;
                    _preloadTasks.Remove(key);
                    return info;
                default: throw new ArgumentOutOfRangeException(nameof(key), $"{key}");
            }
        }
    }
}