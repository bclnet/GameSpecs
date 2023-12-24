using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpec.Metadata
{
    public static class StandardMetadataItem
    {
        /// <summary>
        /// Gets the pak files asynchronous.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="pakFile">The pak file.</param>
        /// <returns></returns>
        public static async Task<List<MetadataItem>> GetPakFilesAsync(MetadataManager manager, BinaryPakFile pakFile)
        {
            var root = new List<MetadataItem>();
            if (pakFile.Files == null) return root;
            string currentPath = null;
            List<MetadataItem> currentFolder = null;
            foreach (var file in pakFile.Files.OrderBy(x => x.Path))
            {
                var path = file.Path[pakFile.VisualPathSkip..];

                // skip empty
                if (string.IsNullOrEmpty(path)) continue;

                // folder
                var fileFolder = Path.GetDirectoryName(path);
                if (currentPath != fileFolder)
                {
                    currentPath = fileFolder;
                    currentFolder = root;
                    if (!string.IsNullOrEmpty(fileFolder))
                        foreach (var folder in fileFolder.Split('\\'))
                        {
                            var found = currentFolder.Find(x => x.Name == folder && x.PakFile == null);
                            if (found != null) currentFolder = found.Items;
                            else { found = new MetadataItem(file, folder, manager.FolderIcon); currentFolder.Add(found); currentFolder = found.Items; }
                        }
                }

                // extract pak
                if (file.Pak != null)
                {
                    var children = await GetPakFilesAsync(manager, file.Pak);
                    currentFolder.Add(new MetadataItem(file, Path.GetFileName(file.Path), manager.PackageIcon, items: children) { PakFile = pakFile });
                    continue;
                }

                // file
                var fileName = Path.GetFileName(path);
                var fileNameForIcon = pakFile.FileMask?.Invoke(fileName) ?? fileName;
                var extentionForIcon = Path.GetExtension(fileNameForIcon);
                if (extentionForIcon.Length > 0) extentionForIcon = extentionForIcon.Substring(1);
                currentFolder.Add(new MetadataItem(file, fileName, manager.GetIcon(extentionForIcon)) { PakFile = pakFile });
            }
            return root;
        }
    }
}