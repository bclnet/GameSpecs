using CASCLib;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Blizzard.Formats.Casc
{
    public unsafe class CascContext
    {
        readonly BackgroundWorkerEx worker = new BackgroundWorkerEx();
        CASCConfig config;
        CASCHandler handle;
        CASCFolder root;

        public void Read(string filePath, string product, IList<FileSource> files)
        {
            var localeFlags = LocaleFlags.enUS;
            CASCConfig.LoadFlags |= LoadFlags.Install;
            config = false
                ? CASCConfig.LoadOnlineStorageConfig(product, "us")
                : CASCConfig.LoadLocalStorageConfig(filePath, product);
            handle = CASCHandler.OpenStorage(config, worker);
            handle.Root.SetFlags(localeFlags, false, false);
            CascLoadFileDataComplete(handle);
            handle.Root.LoadListFile("listfile.csv", worker);
            root = handle.Root.SetFlags(localeFlags, false);
            handle.Root.MergeInstall(handle.Install);
            GC.Collect();
            CascLoadFiles(handle, root, files);
        }

        static void CascLoadFiles(CASCHandler handle, CASCFolder folder, IList<FileSource> files)
        {
            foreach (var f in folder.Files.Values)
                files.Add(new FileSource
                {
                    Path = f.FullName,
                    Hash = f.Hash,
                    //FileSize = f.GetFileSize(f.Hash),
                });
            foreach (var f in folder.Folders.Values)
                CascLoadFiles(handle, f, files);
        }

        static void CascLoadFileDataComplete(CASCHandler casc)
        {
            if (!casc.FileExists("DBFilesClient\\FileDataComplete.db2"))
                return;
            Logger.WriteLine("WowRootHandler: loading file names from FileDataComplete.db2...");
            using (var s = casc.OpenFile("DBFilesClient\\FileDataComplete.db2"))
            {
                var fd = new WDC1Reader(s);
                var hasher = new Jenkins96();
                foreach (var row in fd)
                {
                    var path = row.Value.GetField<string>(0);
                    var name = row.Value.GetField<string>(1);
                    var fullname = path + name;
                    var fileHash = hasher.ComputeHash(fullname);
                    // skip invalid names
                    if (!casc.FileExists(fileHash))
                    {
                        //Logger.WriteLine("Invalid file name: {0}", fullname);
                        continue;
                    }
                    CASCFile.Files[fileHash] = new CASCFile(fileHash, fullname);
                }
            }
        }

        public Stream ReadData(FileSource file)
            => handle.OpenFile(file.Hash);
    }
}