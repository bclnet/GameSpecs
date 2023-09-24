using System.Diagnostics;
using System.IO;
using static GameSpec.Unreal.Formats.Core.Game;

namespace GameSpec.Unreal.Formats.Core
{
    public partial class UPackage
    {
        const int MAX_FNAME_LEN = 1024;
        public static Game GForceGame = UNKNOWN;
        public static int GForcePackageVersion = 0;
        public static Platform GForcePlatform = Platform.UNKNOWN;

        public Game Game;

        Platform Platform;

        public int ArVer;
        public int ArLicenseeVer;
        //public bool IsLoading;
        public bool ReverseBytes;

        // Package structures
        FPackageFileSummary Summary;
        string[] Names;
        public FObjectImport[] Imports;
        public FObjectExport[] Exports;
        //FPackageObjectIndex ExportIndices_IOS;

        public UPackage(BinaryReader r, string path)
        {
            var filename = Path.GetFileName(path);
            //Loader = CreateLoader(r);
            Summary = new FPackageFileSummary(r, this);
            var packageLine = $"Loading package: {filename} Ver: {ArVer}/{ArLicenseeVer} ";
            if (Game >= UE3)
            {
                packageLine += $"Engine: {Summary.EngineVersion} ";
                //FUE3ArchiveReader* UE3Loader = Loader->CastTo<FUE3ArchiveReader>();
                //if (UE3Loader && UE3Loader->IsFullyCompressed)
                //    ADD_LOG("[FullComp] ");
            }
            if (Game >= UE4_BASE && Summary.IsUnversioned) packageLine += "[Unversioned] ";
            Debug.WriteLine(packageLine);
            Debug.WriteLine($"Names: {Summary.NameCount} Exports: {Summary.ExportCount} Imports: {Summary.ImportCount} Game: {Game}");
            Debug.WriteLine($"Flags: {Summary.PackageFlags}, Name offset: {Summary.NameOffset}, Export offset: {Summary.ExportOffset}, Import offset: {Summary.ImportOffset}");
            for (var i = 0; i < Summary.CompressedChunks.Length; i++) Debug.WriteLine($"chunk[{i}]: {Summary.CompressedChunks[i]}");

            ReplaceLoader(r);
            
            LoadNames(r);
            LoadImports(r);
            LoadExports(r);

            ProcessEventDrivenFile(r);
        }

        bool VerifyName(ref string nameStr, int nameIndex)
        {
            // Verify name, some Korean games (B&S) has garbage in FName (unicode?)
            var goodName = true;
            var numBadChars = 0;
            foreach (var c in nameStr)
                if (c == 0) break; // end of line is included into FString unreadable character
                else if (c < ' ' || c > 0x7F) { goodName = false; break; }
                else if (c == '$') numBadChars++; // unicode characters replaced with '$' in FString serializer
            if (goodName && numBadChars != 0)
            {
                var nameLen = nameStr.Length;
                if (nameLen >= 64 || (numBadChars >= nameLen / 2 && nameLen > 16)) goodName = false;
            }
            if (!goodName)
            {
                // replace name
                Debug.WriteLine($"WARNING: XX: fixing name {nameIndex} ({nameStr})");
                nameStr = $"__name_{nameIndex}__";
            }
            return goodName;
        }

        void ReplaceLoader(BinaryReader r)
        {
            if (Game >= UE3 && Summary.CompressionFlags != 0 && Summary.CompressedChunks.Length != 0)
            {
                //FUE3ArchiveReader* UE3Loader = Loader->CastTo<FUE3ArchiveReader>();
                //if (UE3Loader && UE3Loader->IsFullyCompressed)
                //    appError("Fully compressed package %s has additional compression table", filename);
                //// replace Loader with special reader for compressed UE3 archives
                //Loader = new FUE3ArchiveReader(Loader, Summary.CompressionFlags, Summary.CompressedChunks);
            }
        }

        void LoadNames(BinaryReader r)
        {
            if (Summary.NameCount == 0) return;
            r.Seek(Summary.NameOffset);
            Names = new string[Summary.NameCount];
            if (Game >= UE4_BASE) LoadNames4(r, this);
            else if (Game >= UE3) LoadNames3(r, this);
            else LoadNames2(r, this);
        }

        void LoadImports(BinaryReader r)
        {
            if (Summary.ImportCount == 0) return;
            r.Seek(Summary.ImportOffset);
            Imports = r.ReadTArray(r => new FObjectImport(r, this), (int)Summary.ImportCount);
            for (var i = 0; i < Summary.ImportCount; i++) Debug.WriteLine($"Import[{i}]: {Imports[i]}");
        }

        void PatchBnSExports(FObjectExport[] exp, FPackageFileSummary summary) { }
        void PatchDunDefExports(FObjectExport[] exp, FPackageFileSummary summary) { }

        void LoadExports(BinaryReader r)
        {
            if (Summary.ExportCount == 0) return;
            r.Seek(Summary.ExportOffset);
            Exports = r.ReadTArray(r => new FObjectExport(r, this), (int)Summary.ExportCount);
            if (Game == BladeNSoul && (Summary.PackageFlags & 0x08000000) != 0) PatchBnSExports(Exports, Summary);
            if (Game == DunDef) PatchDunDefExports(Exports, Summary);
            for (var i = 0; i < Summary.ExportCount; i++) Debug.WriteLine($"Export[{i}]: {GetClassNameFor(Exports[i])} {Exports[i]}");
        }

        // Process Event Driven Loader packages: such packages are split into 2 pieces: .uasset with headers
        // and .uexp with object's data. At this moment we already have FPackageFileSummary fully loaded,
        // so we can replace loader with .uexp file - with providing correct position offset.
        void ProcessEventDrivenFile(BinaryReader r)
        {
            //if (Game >= UE4_BASE && Summary.HeadersSize == Loader->GetFileSize())
            //{
            //    var name = $"{Path.GetFileNameWithoutExtension(filename)}.uexp";
            //    // When finding, explicitly tell to use the same folder where .uasset file exists
            //    const CGameFileInfo* expInfo = CGameFileInfo::Find(buf, FileInfo ? FileInfo->FolderIndex : -1);
            //    if (expInfo)
            //    {
            //        // Open .exp file
            //        FArchive* expLoader = expInfo->CreateReader();
            //        // Replace loader with this file, but add offset so it will work like it is part of original uasset
            //        Loader = new FReaderWrapper(expLoader, -Summary.HeadersSize);
            //    }
            //    else appPrintf("WARNING: it seems package %s has missing .uexp file\n", filename);
            //}
        }
    }
}