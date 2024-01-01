﻿using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSpec.Metadata
{
    public abstract class MetaManager
    {
        public abstract object FolderIcon { get; }
        public abstract object PackageIcon { get; }
        public abstract object GetIcon(string name);
        public abstract object GetImage(string name);

        /// <summary>
        /// Gets the string or bytes.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="dispose">The dispose.</param>
        /// <returns></returns>
        public static object GuessStringOrBytes(Stream stream, bool dispose = true)
        {
            using var ms = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(ms);
            var bytes = ms.ToArray();
            if (dispose) stream.Dispose();
            return !bytes.Contains<byte>(0x00)
                ? (object)Encoding.UTF8.GetString(bytes)
                : bytes;
        }

        /// <summary>
        /// Gets the explorer information nodes.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="pakFile">The pak file.</param>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task<List<MetaInfo>> GetMetaInfosAsync(MetaManager manager, BinaryPakFile pakFile, FileSource file)
        {
            List<MetaInfo> nodes = null;
            var obj = await pakFile.LoadFileObjectAsync<object>(file);
            if (obj == null) return null;
            else if (obj is IHaveMetaInfo info) nodes = info.GetInfoNodes(manager, file);
            else if (obj is Stream stream)
            {
                var value = GuessStringOrBytes(stream);
                nodes = value is string text ? new List<MetaInfo> {
                        new MetaInfo(null, new MetaContent { Type = "Text", Name = "Text", Value = text }),
                        new MetaInfo("Text", items: new List<MetaInfo> {
                            new MetaInfo($"Length: {text.Length}"),
                        }) }
                    : value is byte[] bytes ? new List<MetaInfo> {
                        new MetaInfo(null, new MetaContent { Type = "Hex", Name = "Hex", Value = new MemoryStream(bytes) }),
                        new MetaInfo("Bytes", items: new List<MetaInfo> {
                            new MetaInfo($"Length: {bytes.Length}"),
                        }) }
                    : throw new ArgumentOutOfRangeException(nameof(value), value.GetType().Name);
            }
            else if (obj is IDisposable disposable) disposable.Dispose();
            if (nodes == null) return null;
            nodes.Add(new MetaInfo("File", items: new List<MetaInfo> {
                new MetaInfo($"Path: {file.Path}"),
                new MetaInfo($"FileSize: {file.FileSize}"),
                file.Parts != null
                    ? new MetaInfo("Parts", items: file.Parts.Select(part => new MetaInfo($"{part.FileSize}@{part.Path}")))
                    : null
            }));
            //nodes.Add(new MetaInfo(null, new MetaContent { Type = "Hex", Name = "TEST", Value = new MemoryStream() }));
            //nodes.Add(new MetaInfo(null, new MetaContent { Type = "Image", Name = "TEST", MaxWidth = 500, MaxHeight = 500, Value = null }));
            return nodes;
        }

        /// <summary>
        /// Gets the meta items asynchronous.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="pakFile">The pak file.</param>
        /// <returns></returns>
        public static async Task<List<MetaItem>> GetMetaItemsAsync(MetaManager manager, BinaryPakFile pakFile)
        {
            var root = new List<MetaItem>();
            if (pakFile.Files == null || pakFile.Files.Count == 0) return root;
            string currentPath = null; List<MetaItem> currentFolder = null;

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
                                found = new MetaItem(file, folder, manager.FolderIcon);
                                currentFolder.Add(found);
                                currentFolder = found.Items;
                            }
                        }
                }
                // pakfile
                if (file.Pak != null)
                {
                    var items = await GetMetaItemsAsync(manager, file.Pak);
                    currentFolder.Add(new MetaItem(file, Path.GetFileName(file.Path), manager.PackageIcon, pakFile: pakFile, items: items));
                    continue;
                }
                // file
                var fileName = Path.GetFileName(path);
                var fileNameForIcon = pakFile.FileMask?.Invoke(fileName) ?? fileName;
                var extentionForIcon = Path.GetExtension(fileNameForIcon);
                if (extentionForIcon.Length > 0) extentionForIcon = extentionForIcon[1..];
                currentFolder.Add(new MetaItem(file, fileName, manager.GetIcon(extentionForIcon), pakFile: pakFile));
            }
            return root;
        }
    }
}
