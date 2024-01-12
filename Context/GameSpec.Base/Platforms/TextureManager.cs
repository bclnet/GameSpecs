using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Platforms
{
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

        public Texture LoadTexture(object key, out IDictionary<string, object> data, Range? range = null)
        {
            if (CachedTextures.TryGetValue(key, out var cache)) { data = cache.data; return cache.texture; }
            // Load & cache the texture.
            var info = key is ITexture z ? z : LoadTexture(key);
            var texture = info != null ? Builder.BuildTexture(info, range) : Builder.DefaultTexture;
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
}