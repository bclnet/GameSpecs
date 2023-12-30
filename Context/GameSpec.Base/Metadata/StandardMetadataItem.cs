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
            if (pakFile.Files == null || pakFile.Files.Count == 0) return root;
            string currentPath = null; List<MetadataItem> currentFolder = null;

            // parse paths
            foreach (var file in pakFile.Files.OrderBy(x => x.Path))
            {
                // next path, skip empty
                var path = file.Path[pakFile.PathSkip..];
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
                            else
                            {
                                found = new MetadataItem(file, folder, manager.FolderIcon);
                                currentFolder.Add(found);
                                currentFolder = found.Items;
                            }
                        }
                }
                // pakfile
                if (file.Pak != null)
                {
                    var items = await GetPakFilesAsync(manager, file.Pak);
                    currentFolder.Add(new MetadataItem(file, Path.GetFileName(file.Path), manager.PackageIcon, pakFile: pakFile, items: items));
                    continue;
                }
                // file
                var fileName = Path.GetFileName(path);
                var fileNameForIcon = pakFile.FileMask?.Invoke(fileName) ?? fileName;
                var extentionForIcon = Path.GetExtension(fileNameForIcon);
                if (extentionForIcon.Length > 0) extentionForIcon = extentionForIcon[1..];
                currentFolder.Add(new MetadataItem(file, fileName, manager.GetIcon(extentionForIcon), pakFile: pakFile));
            }
            return root;
        }
    }
}