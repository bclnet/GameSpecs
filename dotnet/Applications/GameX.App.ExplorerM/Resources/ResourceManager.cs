using GameX.Meta;
//using Microsoft.Maui.Graphics.Platform;
using System.Collections.Concurrent;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace GameX.App.Explorer
{
    public class ResourceManager : MetaManager
    {
        readonly Dictionary<string, IImage> Icons = new();
        readonly ConcurrentDictionary<string, IImage> ImageCache = new();
        readonly object _defaultIcon;
        readonly object _folderIcon;
        readonly object _packageIcon;

        public ResourceManager()
        {
            LoadIcons();
            _defaultIcon = GetIcon("_default");
            _folderIcon = GetIcon("_folder");
            _packageIcon = GetIcon("_package");
        }

        void LoadIcons()
        {
            var assembly = typeof(ResourceManager).Assembly;
            var names = assembly.GetManifestResourceNames().Where(n => n.StartsWith("GameX.App.Explorer2.Resources.Icons.", StringComparison.Ordinal));
            foreach (var name in names)
            {
                var res = name.Split('.');
                using var stream = assembly.GetManifestResourceStream(name);
                //var image = PlatformImage.FromStream(stream);
                //Icons.Add(res[5], image);
            }
        }

        public override object FolderIcon => _folderIcon;

        public override object PackageIcon => _packageIcon;

        public override object GetIcon(string name) => Icons.TryGetValue(name, out var z) ? z : _defaultIcon;

        public override object GetImage(string name) => ImageCache.GetOrAdd(name, x =>
        {
            var assembly = typeof(ResourceManager).Assembly;
            using var stream = assembly.GetManifestResourceStream(x);
            //var image = PlatformImage.FromStream(stream);
            //return image;
            return null;
        });
    }
}
