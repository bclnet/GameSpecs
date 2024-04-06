using OpenStack;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static OpenStack.Debug;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace GameX.Platforms
{
    #region TextureBuilderBase

    public abstract class TextureBuilderBase<Texture>
    {
        public static int MaxTextureMaxAnisotropy
        {
            get => PlatformStats.MaxTextureMaxAnisotropy;
            set => PlatformStats.MaxTextureMaxAnisotropy = value;
        }

        public abstract Texture DefaultTexture { get; }
        public abstract Texture BuildTexture(ITexture info, Range? rng = null);
        public abstract Texture BuildSolidTexture(int width, int height, float[] rgba);
        public abstract Texture BuildNormalMap(Texture source, float strength);
        public abstract void DeleteTexture(Texture texture);
    }

    #endregion

    #region TextureManager

    public class TextureManager<Texture> : ITextureManager<Texture>
    {
        readonly PakFile PakFile;
        readonly TextureBuilderBase<Texture> Builder;
        readonly Dictionary<object, (Texture texture, IDictionary<string, object> data)> CachedTextures = new Dictionary<object, (Texture texture, IDictionary<string, object> data)>();
        readonly Dictionary<object, Task<ITexture>> PreloadTasks = new Dictionary<object, Task<ITexture>>();

        public TextureManager(PakFile pakFile, TextureBuilderBase<Texture> builder)
        {
            PakFile = pakFile;
            Builder = builder;
        }

        public Texture BuildSolidTexture(int width, int height, params float[] rgba) => Builder.BuildSolidTexture(width, height, rgba);

        public Texture BuildNormalMap(Texture source, float strength) => Builder.BuildNormalMap(source, strength);

        public Texture DefaultTexture => Builder.DefaultTexture;

        public Texture LoadTexture(object key, out IDictionary<string, object> data, Range? rng = null)
        {
            if (CachedTextures.TryGetValue(key, out var cache)) { data = cache.data; return cache.texture; }
            // Load & cache the texture.
            var info = key is ITexture z ? z : LoadTexture(key);
            var texture = info != null ? Builder.BuildTexture(info, rng) : Builder.DefaultTexture;
            data = info?.Data;
            CachedTextures[key] = (texture, data);
            return texture;
        }

        public void PreloadTexture(string path)
        {
            if (CachedTextures.ContainsKey(path)) return;
            // Start loading the texture file asynchronously if we haven't already started.
            if (!PreloadTasks.ContainsKey(path)) PreloadTasks[path] = PakFile.LoadFileObject<ITexture>(path);
        }

        public void DeleteTexture(object key)
        {
            if (!CachedTextures.TryGetValue(key, out var cache)) return;
            Builder.DeleteTexture(cache.texture);
            CachedTextures.Remove(key);
        }

        ITexture LoadTexture(object key)
        {
            Assert(!CachedTextures.ContainsKey(key));
            switch (key)
            {
                case string path:
                    PreloadTexture(path);
                    var info = PreloadTasks[key].Result;
                    PreloadTasks.Remove(key);
                    return info;
                default: throw new ArgumentOutOfRangeException(nameof(key), $"{key}");
            }
        }
    }

    #endregion

    #region ShaderBuilderBase

    public abstract class ShaderBuilderBase<Shader>
    {
        public abstract Shader BuildShader(string path, IDictionary<string, bool> args);
        public abstract Shader BuildPlaneShader(string path, IDictionary<string, bool> args);
    }

    #endregion

    #region ShaderManager

    public class ShaderManager<Shader> : IShaderManager<Shader>
    {
        static readonly Dictionary<string, bool> EmptyArgs = new Dictionary<string, bool>();
        readonly PakFile _pakFile;
        readonly ShaderBuilderBase<Shader> _builder;

        public ShaderManager(PakFile pakFile, ShaderBuilderBase<Shader> builder)
        {
            _pakFile = pakFile;
            _builder = builder;
        }

        public Shader LoadShader(string path, IDictionary<string, bool> args = null)
            => _builder.BuildShader(path, args ?? EmptyArgs);

        public Shader LoadPlaneShader(string path, IDictionary<string, bool> args = null)
            => _builder.BuildPlaneShader(path, args ?? EmptyArgs);
    }

    #endregion

    #region ObjectBuilderBase

    public abstract class ObjectBuilderBase<Object, Material, Texture>
    {
        public abstract Object CreateObject(Object prefab);
        public abstract void EnsurePrefabContainerExists();
        public abstract Object BuildObject(object source, IMaterialManager<Material, Texture> materialManager);
    }

    #endregion

    #region ObjectManager

    public class ObjectManager<Object, Material, Texture> : IObjectManager<Object, Material, Texture>
    {
        readonly PakFile _pakFile;
        readonly IMaterialManager<Material, Texture> _materialManager;
        readonly ObjectBuilderBase<Object, Material, Texture> _builder;
        readonly Dictionary<string, Object> _cachedPrefabs = new Dictionary<string, Object>();
        readonly Dictionary<string, Task<object>> _preloadTasks = new Dictionary<string, Task<object>>();

        public ObjectManager(PakFile pakFile, IMaterialManager<Material, Texture> materialManager, ObjectBuilderBase<Object, Material, Texture> builder)
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
            if (!_preloadTasks.ContainsKey(path)) _preloadTasks[path] = _pakFile.LoadFileObject<object>(path);
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

    #endregion

    #region MaterialBuilderBase

    public abstract class MaterialBuilderBase<Material, Texture>
    {
        protected ITextureManager<Texture> TextureManager;

        public MaterialBuilderBase(ITextureManager<Texture> textureManager) => TextureManager = textureManager;

        public float? NormalGeneratorIntensity = 0.75f;
        public abstract Material DefaultMaterial { get; }
        public abstract Material BuildMaterial(object key);
    }

    #endregion

    #region MaterialManager

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

    #endregion

    #region Platform

    /// <summary>
    /// Platform
    /// </summary>
    public static class Platform
    {
        /// <summary>
        /// The platform stats.
        /// </summary>
        public class Stats
        {
            static readonly bool _HighRes = Stopwatch.IsHighResolution;
            static readonly double _HighFrequency = 1000.0 / Stopwatch.Frequency;
            static readonly double _LowFrequency = 1000.0 / TimeSpan.TicksPerSecond;
            static bool _UseHRT = false;

            public static bool UsingHighResolutionTiming => _UseHRT && _HighRes && !Unix;
            public static long TickCount => (long)Ticks;
            public static double Ticks => _UseHRT && _HighRes && !Unix ? Stopwatch.GetTimestamp() * _HighFrequency : DateTime.UtcNow.Ticks * _LowFrequency;

            public static readonly bool Is64Bit = Environment.Is64BitProcess;
            public static bool MultiProcessor { get; private set; }
            public static int ProcessorCount { get; private set; }
            public static bool Unix { get; private set; }
            public static bool VR { get; private set; }
        }

        /// <summary>
        /// The platform type.
        /// </summary>
        public enum Type { Unknown, OpenGL, Unity, Unreal, Vulken, StereoKit, Test, Other }

        /// <summary>
        /// The platform OS.
        /// </summary>
        public enum OS { Windows, OSX, Linux, Android }

        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        public static Type PlatformType;

        /// <summary>
        /// Gets or sets the platform tag.
        /// </summary>
        public static string PlatformTag;

        /// <summary>
        /// Gets the platform os.
        /// </summary>
        public static readonly OS PlatformOS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OS.Windows
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OS.OSX
            : RuntimeInformation.OSDescription.StartsWith("android-") ? OS.Android
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OS.Linux
            : throw new ArgumentOutOfRangeException(nameof(RuntimeInformation.IsOSPlatform), RuntimeInformation.OSDescription);

        /// <summary>
        /// Gets or sets the platform graphics factory.
        /// </summary>
        public static Func<PakFile, IOpenGraphic> GraphicFactory;

        /// <summary>
        /// Gets the platform startups.
        /// </summary>
        public static readonly List<Func<bool>> Startups = new List<Func<bool>>();

        /// <summary>
        /// Determines if in a test host.
        /// </summary>
        public static bool InTestHost => AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName.StartsWith("testhost,"));
    }

    #endregion

    #region TestGraphic

    public interface ITestGraphic : IOpenGraphic { }

    public class TestGraphic : ITestGraphic
    {
        readonly PakFile _source;

        public TestGraphic(PakFile source) => _source = source;
        public object Source => _source;
        public Task<T> LoadFileObject<T>(string path) => throw new NotSupportedException();
        public void PreloadTexture(string texturePath) => throw new NotSupportedException();
        public void PreloadObject(string filePath) => throw new NotSupportedException();
    }

    #endregion

    #region TestPlatform

    public static class TestPlatform
    {
        public static bool Startup()
        {
            try
            {
                Platform.PlatformType = Platform.Type.Test;
                Platform.GraphicFactory = source => new TestGraphic(source);
                Debug.AssertFunc = x => System.Diagnostics.Debug.Assert(x);
                Debug.LogFunc = a => System.Diagnostics.Debug.Print(a);
                Debug.LogFormatFunc = (a, b) => System.Diagnostics.Debug.Print(a, b);
                return true;
            }
            catch { return false; }
        }
    }

    #endregion
}