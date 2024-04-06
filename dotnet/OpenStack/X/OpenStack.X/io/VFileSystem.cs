using System.Collections.Generic;

namespace System.NumericsX.OpenStack
{
    // modes for OpenFileByMode. used as bit mask internally
    public enum FS
    {
        READ = 0,
        WRITE = 1,
        APPEND = 2
    }

    public enum PURE
    {
        OK,     // we are good to connect as-is
        RESTART,    // restart required
        MISSING,    // pak files missing on the client
    }

    public enum DLTYPE
    {
        URL,
        FILE
    }

    public enum DL
    {
        WAIT,        // waiting in the list for beginning of the download
        INPROGRESS,  // in progress
        DONE,        // download completed, success
        ABORTING,    // this one can be set during a download, it will force the next progress callback to abort - then will go to DL_FAILED
        FAILED
    }

    public enum DL_FILE : byte
    {
        EXEC,
        OPEN
    }

    public enum FIND
    {
        NO,
        YES,
        ADDON
    }

    public struct UrlDownload
    {
        public string url;
        public string dlerror;
        public int dltotal;
        public int dlnow;
        public int dlstatus;
        public DL status;
    }

    public struct FileDownload
    {
        public int position;
        public int length;
        public byte[] buffer;
    }

    public class BackgroundDownload
    {
        public BackgroundDownload next; // set by the fileSystem
        public DLTYPE opcode;
        public VFile f;
        public FileDownload file;
        public UrlDownload url;
        public volatile bool completed;
    }

    // file list for directory listings
    public class FileList
    {
        public string BasePath => basePath;
        public int NumFiles => list.Count;
        public string GetFile(int index) => list[index];
        public List<string> List => list;

        string basePath;
        readonly List<string> list = new();
    }

    // mod list
    public class ModList
    {
        public int NumMods => mods.Count;
        public string GetMod(int index) => mods[index];
        public string GetDescription(int index) => descriptions[index];

        readonly List<string> mods = new();
        readonly List<string> descriptions = new();
    }

    public interface IVFileSystem
    {
        // FIXME: DG: this assumes 32bit time_t, but it's 64bit now, at least on some platforms incl. Win32 in modern VS
        //            => change it (to -1?) or does that break anything?
        //public static const ID_TIME_T FILE_NOT_FOUND_TIMESTAMP = 0xFFFFFFFF;
        public const int MAX_PURE_PAKS = 128;
        //public const int MAX_OSPATH = FILENAME_MAX;

        // Initializes the file system.
        void Init();
        // Restarts the file system.
        void Restart();
        // Shutdown the file system.
        void Shutdown(bool reloading);
        // Returns true if the file system is initialized.
        bool IsInitialized();
        // Returns true if we are doing an fs_copyfiles.
        bool PerformingCopyFiles();
        // Returns a list of mods found along with descriptions
        // 'mods' contains the directory names to be passed to fs_game
        // 'descriptions' contains a free form string to be used in the UI
        ModList ListMods();
        // Frees the given mod list
        void FreeModList(ModList modList);
        // Lists files with the given extension in the given directory.
        // Directory should not have either a leading or trailing '/'
        // The returned files will not include any directories or '/' unless fullRelativePath is set.
        // The extension must include a leading dot and may not contain wildcards.
        // If extension is "/", only subdirectories will be returned.
        FileList ListFiles(string relativePath, string extension, bool sort = false, bool fullRelativePath = false, string gamedir = null);
        // Lists files in the given directory and all subdirectories with the given extension.
        // Directory should not have either a leading or trailing '/'
        // The returned files include a full relative path.
        // The extension must include a leading dot and may not contain wildcards.
        FileList ListFilesTree(string relativePath, string extension, bool sort = false, string gamedir = null);
        // Frees the given file list.
        void FreeFileList(FileList fileList);
        // Converts a relative path to a full OS path.
        string OSPathToRelativePath(string OSPath);
        // Converts a full OS path to a relative path.
        string RelativePathToOSPath(string relativePath, string basePath = "fs_devpath");
        // Builds a full OS path from the given components.
        string BuildOSPath(string base_, string game, string relativePath);
        // Creates the given OS path for as far as it doesn't exist already.
        void CreateOSPath(string OSPath);
        // Returns true if a file is in a pak file.
        bool FileIsInPAK(string relativePath);
        // Returns a space separated string containing the checksums of all referenced pak files.
        // will call SetPureServerChecksums internally to restrict itself
        void UpdatePureServerChecksums();
        // 0-terminated list of pak checksums
        // if pureChecksums[ 0 ] == 0, all data sources will be allowed
        // otherwise, only pak files that match one of the checksums will be checked for files
        // with the sole exception of .cfg files.
        // the function tries to configure pure mode from the paks already referenced and this new list
        // it returns wether the switch was successfull, and sets the missing checksums
        // the process is verbosive when fs_debug 1
        PURE SetPureServerChecksums(int[] pureChecksums, int[] missingChecksums); //:[MAX_PURE_PAKS]
        // fills a 0-terminated list of pak checksums for a client
        // if OS is -1, give the current game pak checksum. if >= 0, lookup the game pak table (server only)
        void GetPureServerChecksums(int[] checksums); //:[MAX_PURE_PAKS]
        // before doing a restart, force the pure list and the search order
        // if the given checksum list can't be completely processed and set, will error out
        void SetRestartChecksums(int[] pureChecksums); //:[MAX_PURE_PAKS]
        // equivalent to calling SetPureServerChecksums with an empty list
        void ClearPureChecksums();
        // Reads a complete file.
        // Returns the length of the file, or -1 on failure.
        // A null buffer will just return the file length without loading.
        // A null timestamp will be ignored.
        // As a quick check for existance. -1 length == not present.
        // A 0 byte will always be appended at the end, so string ops are safe.
        // The buffer should be considered read-only, because it may be cached for other uses.
        int ReadFile(string relativePath, out byte[] buffer, out DateTime timestamp);
        int ReadFile(string relativePath, out DateTime timestamp);
        int ReadFile(string relativePath);
        // Frees the memory allocated by ReadFile.
        void FreeFile(byte[] buffer);
        // Writes a complete file, will create any needed subdirectories.
        // Returns the length of the file, or -1 on failure.
        int WriteFile(string relativePath, byte[] buffer, int size, string basePath = "fs_savepath");
        // Removes the given file.
        void RemoveFile(string relativePath);
        // Opens a file for reading.
        VFile OpenFileRead(string relativePath, bool allowCopyFiles = true, string gamedir = null);
        // Opens a file for writing, will create any needed subdirectories.
        VFile OpenFileWrite(string relativePath, string basePath = "fs_savepath");
        // Opens a file for writing at the end.
        VFile OpenFileAppend(string filename, bool sync = false, string basePath = "fs_basepath");
        // Opens a file for reading, writing, or appending depending on the value of mode.
        VFile OpenFileByMode(string relativePath, FS mode);
        // Opens a file for reading from a full OS path.
        VFile OpenExplicitFileRead(string OSPath);
        // Opens a file for writing to a full OS path.
        VFile OpenExplicitFileWrite(string OSPath);
        // Closes a file.
        void CloseFile(VFile f);
        // Returns immediately, performing the read from a background thread.
        void BackgroundDownload(BackgroundDownload bgl);
        // resets the bytes read counter
        void ResetReadCount();
        // retrieves the current read count
        int GetReadCount();
        // adds to the read count
        void AddToReadCount(int c);
        // look for a dynamic module
        void FindDLL(string basename, string dllPath);
        // case sensitive filesystems use an internal directory cache
        // the cache is cleared when calling OpenFileWrite and RemoveFile
        // in some cases you may need to use this directly
        void ClearDirCache();

        // is D3XP installed? even if not running it atm
        bool HasD3XP { get; }
        // are we using D3XP content ( through a real d3xp run or through a double mod )
        bool RunningD3XP { get; }

        // don't use for large copies - allocates a single memory block for the copy
        void CopyFile(string fromOSPath, string toOSPath);

        // lookup a relative path, return the size or 0 if not found
        int ValidateDownloadPakForChecksum(int checksum, string path);

        VFile MakeTemporaryFile();

        // make downloaded pak files known so pure negociation works next time
        int AddZipFile(string path);

        // look for a file in the loaded paks or the addon paks
        // if the file is found in addons, FS's internal structures are ready for a reloadEngine
        FIND FindFile(string path, bool scheduleAddons = false);

        // get map/addon decls and take into account addon paks that are not on the search list
        // the decl 'name' is in the "path" entry of the dict
        int NumMaps { get; }
        Dictionary<string, string> GetMapDecl(int i);
        void FindMapScreenshot(string path, out string s);
        //void FindMapScreenshot(string path, byte[] buf, int len);

        // ignore case and seperator char distinctions
        bool FilenameCompare(string s1, string s2);
    }
}
