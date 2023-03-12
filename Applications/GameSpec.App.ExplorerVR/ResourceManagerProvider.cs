using GameSpec.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameSpec.App.Explorer
{
    public class ResourceManagerProvider : MetadataManager
    {
        readonly Dictionary<string, object> Icons = new();
        readonly ConcurrentDictionary<string, object> ImageCache = new();
        readonly object _defaultIcon;
        readonly object _folderIcon;
        readonly object _packageIcon;

        public ResourceManagerProvider()
        {
            LoadIcons();
            _defaultIcon = GetIcon("_default");
            _folderIcon = GetIcon("_folder");
            _packageIcon = GetIcon("_package");
        }

        void LoadIcons()
        {
            var assembly = typeof(ResourceManagerProvider).Assembly;
            var names = assembly.GetManifestResourceNames().Where(n => n.StartsWith("GameSpec.App.ExplorerVR.Resources.Icons.", StringComparison.Ordinal));
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
            var assembly = typeof(ResourceManagerProvider).Assembly;
            using var stream = assembly.GetManifestResourceStream(x);
            //var image = PlatformImage.FromStream(stream);
            //return image;
            return null;
        });
    }
}
