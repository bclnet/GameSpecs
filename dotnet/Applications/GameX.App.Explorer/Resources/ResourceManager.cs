using GameX.Meta;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace GameX.App.Explorer
{
    public class ResourceManager : MetaManager
    {
        readonly Dictionary<string, BitmapImage> Icons = new();
        readonly ConcurrentDictionary<string, BitmapImage> ImageCache = new();
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
            var names = assembly.GetManifestResourceNames().Where(n => n.StartsWith("GameX.App.Explorer.Resources.Icons.", StringComparison.Ordinal));
            foreach (var name in names)
            {
                var res = name.Split('.');
                using var stream = assembly.GetManifestResourceStream(name);
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                Icons.Add(res[5], image);
            }
        }

        public override object FolderIcon => _folderIcon;

        public override object PackageIcon => _packageIcon;

        public override object GetIcon(string name) => Icons.TryGetValue(name?.ToLowerInvariant(), out var z) ? z : _defaultIcon;

        public override object GetImage(string name) => ImageCache.GetOrAdd(name, x =>
        {
            var image = new BitmapImage(new Uri("pack://application:,,,//{x}"));
            image.Freeze(); // to prevent error: "Must create DependencySource on same Thread as the DependencyObject"
            return image;
        });
    }
}
