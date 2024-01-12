using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Platforms
{
    /// <summary>
    /// Manages loading and instantiation of materials.
    /// </summary>
    public class MaterialManager<Material, Texture> : IMaterialManager<Material, Texture>
    {
        readonly PakFile PakFile;
        readonly MaterialBuilderBase<Material, Texture> Builder;
        readonly Dictionary<object, (Material material, IDictionary<string, object> data)> CachedMaterials = new Dictionary<object, (Material material, IDictionary<string, object> data)>();
        readonly Dictionary<object, Task<IMaterial>> PreloadTasks = new Dictionary<object, Task<IMaterial>>();

        public ITextureManager<Texture> TextureManager { get; }

        public MaterialManager(PakFile pakFile, ITextureManager<Texture> textureManager, MaterialBuilderBase<Material, Texture> builder)
        {
            PakFile = pakFile;
            TextureManager = textureManager;
            Builder = builder;
        }

        public Material LoadMaterial(object key, out IDictionary<string, object> data)
        {
            if (CachedMaterials.TryGetValue(key, out var cache)) { data = cache.data; return cache.material; }
            // Load & cache the material.
            var info = key is IMaterial z ? z : LoadMaterialInfo(key);
            var material = info != null ? Builder.BuildMaterial(info) : Builder.DefaultMaterial;
            data = info?.Data;
            CachedMaterials[key] = (material, data);
            return material;
        }

        public void PreloadMaterial(string path)
        {
            if (CachedMaterials.ContainsKey(path)) return;
            // Start loading the material file asynchronously if we haven't already started.
            if (!PreloadTasks.ContainsKey(path)) PreloadTasks[path] = PakFile.LoadFileObject<IMaterial>(path);
        }

        IMaterial LoadMaterialInfo(object key)
        {
            Assert(!CachedMaterials.ContainsKey(key));
            switch (key)
            {
                case string path:
                    PreloadMaterial(path);
                    var info = PreloadTasks[key].Result;
                    PreloadTasks.Remove(key);
                    return info;
                default: throw new ArgumentOutOfRangeException(nameof(key), $"{key}");
            }
        }
    }
}