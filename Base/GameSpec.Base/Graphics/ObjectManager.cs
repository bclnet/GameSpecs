using OpenStack.Graphics;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Graphics
{
    public class ObjectManager<Object, Material, Texture> : IObjectManager<Object, Material, Texture>
    {
        readonly PakFile _pakFile;
        readonly IMaterialManager<Material, Texture> _materialManager;
        readonly AbstractObjectBuilder<Object, Material, Texture> _builder;
        readonly Dictionary<string, Object> _cachedPrefabs = new Dictionary<string, Object>();
        readonly Dictionary<string, Task<object>> _preloadTasks = new Dictionary<string, Task<object>>();

        public ObjectManager(PakFile pakFile, IMaterialManager<Material, Texture> materialManager, AbstractObjectBuilder<Object, Material, Texture> builder)
        {
            _pakFile = pakFile;
            _materialManager = materialManager;
            _builder = builder;
        }

        public Object CreateObject(string path, out IDictionary<string, object> data)
        {
            data = null;
            _builder.EnsurePrefabContainerExists();
            // Load & cache the NIF prefab.
            if (!_cachedPrefabs.TryGetValue(path, out var prefab)) prefab = _cachedPrefabs[path] = LoadPrefabDontAddToPrefabCache(path);
            // Instantiate the prefab.
            return _builder.CreateObject(prefab);
        }

        public void PreloadObject(string path)
        {
            if (_cachedPrefabs.ContainsKey(path)) return;
            // Start loading the object asynchronously if we haven't already started.
            if (!_preloadTasks.ContainsKey(path)) _preloadTasks[path] = _pakFile.LoadFileObjectAsync<object>(path);
        }

        Object LoadPrefabDontAddToPrefabCache(string path)
        {
            Assert(!_cachedPrefabs.ContainsKey(path));
            PreloadObject(path);
            var source = _preloadTasks[path].Result;
            _preloadTasks.Remove(path);
            return _builder.BuildObject(source, _materialManager);
        }
    }
}