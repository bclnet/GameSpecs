/// <summary>
/// On-disk structures for CASC storages
/// </summary>
using System;
using static GameX.Blizzard.Formats.Casc.CascLib;

namespace GameX.Blizzard.Formats.Casc
{
    public static partial class CascLib
    {
        public const int CASCLIB_VERSION = 0x0210;  // CascLib version - integral (2.1)
        public const string CASCLIB_VERSION_STRING = "2.1";  // CascLib version - string

        // Values for CascOpenFile
        public const uint CASC_OPEN_BY_NAME = 0x00000000;  // Open the file by name. This is the default value
        public const uint CASC_OPEN_BY_CKEY = 0x00000001;  // The name is just the content key; skip ROOT file processing
        public const uint CASC_OPEN_BY_EKEY = 0x00000002;  // The name is just the encoded key; skip ROOT file processing
        public const uint CASC_OPEN_BY_FILEID = 0x00000003;  // The name is CASC_FILE_DATA_ID(FileDataId)
        public const uint CASC_OPEN_TYPE_MASK = 0x0000000F;  // The mask which gets open type from the dwFlags
        public const uint CASC_OPEN_FLAGS_MASK = 0xFFFFFFF0;  // The mask which gets open type from the dwFlags
        public const uint CASC_STRICT_DATA_CHECK = 0x00000010;  // Verify all data read from a file
        public const uint CASC_OVERCOME_ENCRYPTED = 0x00000020;  // When CascReadFile encounters a block encrypted with a key that is missing, the block is filled with zeros and returned as success

        public const uint CASC_LOCALE_ALL = 0xFFFFFFFF;
        public const uint CASC_LOCALE_ALL_WOW = 0x0001F3F6;  // All except enCN and enTW
        public const uint CASC_LOCALE_NONE = 0x00000000;
        public const uint CASC_LOCALE_UNKNOWN1 = 0x00000001;
        public const uint CASC_LOCALE_ENUS = 0x00000002;
        public const uint CASC_LOCALE_KOKR = 0x00000004;
        public const uint CASC_LOCALE_RESERVED = 0x00000008;
        public const uint CASC_LOCALE_FRFR = 0x00000010;
        public const uint CASC_LOCALE_DEDE = 0x00000020;
        public const uint CASC_LOCALE_ZHCN = 0x00000040;
        public const uint CASC_LOCALE_ESES = 0x00000080;
        public const uint CASC_LOCALE_ZHTW = 0x00000100;
        public const uint CASC_LOCALE_ENGB = 0x00000200;
        public const uint CASC_LOCALE_ENCN = 0x00000400;
        public const uint CASC_LOCALE_ENTW = 0x00000800;
        public const uint CASC_LOCALE_ESMX = 0x00001000;
        public const uint CASC_LOCALE_RURU = 0x00002000;
        public const uint CASC_LOCALE_PTBR = 0x00004000;
        public const uint CASC_LOCALE_ITIT = 0x00008000;
        public const uint CASC_LOCALE_PTPT = 0x00010000;

        // Content flags on WoW
        public const uint CASC_CFLAG_LOAD_ON_WINDOWS = 0x08;
        public const uint CASC_CFLAG_LOAD_ON_MAC = 0x10;
        public const uint CASC_CFLAG_LOW_VIOLENCE = 0x80;
        public const uint CASC_CFLAG_DONT_LOAD = 0x100;
        public const uint CASC_CFLAG_NO_NAME_HASH = 0x10000000;
        public const uint CASC_CFLAG_BUNDLE = 0x40000000;
        public const uint CASC_CFLAG_NO_COMPRESSION = 0x80000000;

        public const int MD5_HASH_SIZE = 0x10;
        public const int MD5_STRING_SIZE = 0x20;

        // Return value for CascGetFileSize and CascSetFilePointer
        public const uint CASC_INVALID_INDEX = 0xFFFFFFFF;
        public const uint CASC_INVALID_SIZE = 0xFFFFFFFF;
        public const uint CASC_INVALID_POS = 0xFFFFFFFF;
        public const uint CASC_INVALID_ID = 0xFFFFFFFF;
        public const ulong CASC_INVALID_OFFS64 = 0xFFFFFFFFFFFFFFFF;
        public const ulong CASC_INVALID_SIZE64 = 0xFFFFFFFFFFFFFFFF;

        // Flags for CASC_STORAGE_FEATURES::dwFeatures
        public const uint CASC_FEATURE_FILE_NAMES = 0x00000001;  // File names are supported by the storage
        public const uint CASC_FEATURE_ROOT_CKEY = 0x00000002;  // Present if the storage's ROOT returns CKey
        public const uint CASC_FEATURE_TAGS = 0x00000004;  // Tags are supported by the storage
        public const uint CASC_FEATURE_FNAME_HASHES = 0x00000008;  // The storage contains file name hashes on ALL files
        public const uint CASC_FEATURE_FNAME_HASHES_OPTIONAL = 0x00000010; // The storage contains file name hashes for SOME files
        public const uint CASC_FEATURE_FILE_DATA_IDS = 0x00000020;  // The storage indexes files by FileDataId
        public const uint CASC_FEATURE_LOCALE_FLAGS = 0x00000040;  // Locale flags are supported
        public const uint CASC_FEATURE_CONTENT_FLAGS = 0x00000080;  // Content flags are supported
        public const uint CASC_FEATURE_ONLINE = 0x00000100;  // The storage is an online storage

        // Macro to convert FileDataId to the argument of CascOpenFile
        public static IntPtr CASC_FILE_DATA_ID(int FileDataId) => (IntPtr)FileDataId;
        public static uint CASC_FILE_DATA_ID_FROM_STRING(int szFileName) => (uint)szFileName;

        // Maximum length of encryption key
        public const int CASC_KEY_LENGTH = 0x10;
    }

    public enum CASC_STORAGE_INFO_CLASS
    {
        // Returns the number of local files in the storage. Note that files
        // can exist under different names, so the total number of files in the archive
        // can be higher than the value returned by this info class
        CascStorageLocalFileCount,

        // Returns the total file count, including the offline files
        CascStorageTotalFileCount,

        CascStorageFeatures,                        // Returns the features flag
        CascStorageInstalledLocales,                // Not supported
        CascStorageProduct,                         // Gives CASC_STORAGE_PRODUCT
        CascStorageTags,                            // Gives CASC_STORAGE_TAGS structure
        CascStoragePathProduct,                     // Gives Path:Product into a LPTSTR buffer
        CascStorageInfoClassMax
    }

    public enum CASC_FILE_INFO_CLASS
    {
        CascFileContentKey,
        CascFileEncodedKey,
        CascFileFullInfo,                           // Gives CASC_FILE_FULL_INFO structure
        CascFileSpanInfo,                           // Gives CASC_FILE_SPAN_INFO structure for each file span
        CascFileInfoClassMax
    }

    // CascLib may provide a fake name, constructed from file data id, CKey or EKey.
    // This enum helps to see what name was actually returned
    // Note that any of these names can be passed to CascOpenFile with no extra flags
    public enum CASC_NAME_TYPE
    {
        CascNameFull,                               // Fully qualified file name
        CascNameDataId,                             // Name created from file data id (FILE%08X.dat)
        CascNameCKey,                               // Name created as string representation of CKey
        CascNameEKey                                // Name created as string representation of EKey
    }

    // Structure for SFileFindFirstFile and SFileFindNextFile
    public unsafe struct CASC_FIND_DATA
    {
        // Full name of the found file. In case when this is CKey/EKey,
        // this will be just string representation of the key stored in 'FileKey'
        public string szFileName;

        // Content key. This is present if the CASC_FEATURE_ROOT_CKEY is present
        public fixed byte CKey[MD5_HASH_SIZE];

        // Encoded key. This is always present.
        public fixed byte EKey[MD5_HASH_SIZE];

        // Tag mask. Only valid if the storage supports tags, otherwise 0
        public ulong TagBitMask;

        // Size of the file, as retrieved from CKey entry
        public ulong FileSize;

        // Plain name of the found file. Pointing inside the 'szFileName' array
        public string szPlainName;

        // File data ID. Only valid if the storage supports file data IDs, otherwise CASC_INVALID_ID
        public uint dwFileDataId;

        // Locale flags. Only valid if the storage supports locale flags, otherwise CASC_INVALID_ID
        public uint dwLocaleFlags;

        // Content flags. Only valid if the storage supports content flags, otherwise CASC_INVALID_ID
        public uint dwContentFlags;

        // Span count
        public uint dwSpanCount;

        // If true the file is available locally
        public bool bFileAvailable;

        // Name type in 'szFileName'. In case the file name is not known,
        // CascLib can put FileDataId-like name or a string representation of CKey/EKey
        public CASC_NAME_TYPE NameType;
    }

    public struct CASC_STORAGE_TAG
    {
        public string szTagName;                                // Tag name (zero terminated, ANSI)
        public uint TagNameLength;                              // Length of the tag name
        public uint TagValue;                                   // Tag value
    }

    public struct CASC_STORAGE_TAGS
    {
        public IntPtr TagCount;                                 // Number of items in the Tags array
        public IntPtr Reserved;                                 // Reserved for future use
        public CASC_STORAGE_TAG[] Tags;                         // Array of CASC tags
    }

    public unsafe struct CASC_STORAGE_PRODUCT
    {
        public fixed char szCodeName[0x1C];                     // Code name of the product ("wowt" = "World of Warcraft PTR")
        public uint BuildNumber;                                // Build number. If zero, then CascLib didn't recognize build number
    }

    public unsafe struct CASC_FILE_FULL_INFO
    {
        public fixed byte CKey[MD5_HASH_SIZE];                  // CKey
        public fixed byte EKey[MD5_HASH_SIZE];                  // EKey
        public fixed char DataFileName[0x10];                   // Plain name of the data file where the file is stored
        public ulong StorageOffset;                             // Offset of the file over the entire storage
        public ulong SegmentOffset;                             // Offset of the file in the segment file ("data.###")
        public ulong TagBitMask;                                // Bitmask of tags. Zero if not supported
        public ulong FileNameHash;                              // Hash of the file name. Zero if not supported
        public ulong ContentSize;                               // Content size of all spans
        public ulong EncodedSize;                               // Encoded size of all spans
        public uint SegmentIndex;                               // Index of the segment file (aka 0 = "data.000")
        public uint SpanCount;                                  // Number of spans forming the file
        public uint FileDataId;                                 // File data ID. CASC_INVALID_ID if not supported.
        public uint LocaleFlags;                                // Locale flags. CASC_INVALID_ID if not supported.
        public uint ContentFlags;                               // Locale flags. CASC_INVALID_ID if not supported
    }

    public unsafe struct CASC_FILE_SPAN_INFO
    {
        public fixed byte CKey[MD5_HASH_SIZE];                  // Content key of the file span
        public fixed byte EKey[MD5_HASH_SIZE];                  // Encoded key of the file span
        public ulong StartOffset;                               // Starting offset of the file span
        public ulong EndOffset;                                 // Ending offset of the file span
        public uint ArchiveIndex;                               // Index of the archive
        public uint ArchiveOffs;                                // Offset in the archive
        public uint HeaderSize;                                 // Size of encoded frame headers
        public uint FrameCount;                                 // Number of frames in this span
    }

    //-----------------------------------------------------------------------------
    // Extended version of CascOpenStorage

    // Some operations (e.g. opening an online storage) may take long time.
    // This callback allows an application to be notified about loading progress
    // and even cancel the storage loading process
    //typedef bool (WINAPI* PFNPROGRESSCALLBACK) (    // Return 'true' to cancel the loading process
    //    void* PtrUserParam,                        // User-specific parameter passed to the callback
    //    LPCSTR szWork,                              // Text for the current activity (example: "Loading "ENCODING" file")
    //    LPCSTR szObject,                            // (optional) name of the object tied to the activity (example: index file name)
    //    uint CurrentValue,                         // (optional) current object being processed
    //    uint TotalValue                            // (optional) If non-zero, this is the total number of objects to process
    //    );

    // Some storages support multi-product installation (e.g. World of Warcraft).
    // With this callback, the calling application can specify which storage to open
    //typedef bool (WINAPI* PFNPRODUCTCALLBACK) (     // Return 'true' to cancel the loading process
    //    void* PtrUserParam,                        // User-specific parameter passed to the callback
    //    LPCSTR* ProductList,                       // Array of product codenames found in the storage
    //    size_t ProductCount,                        // Number of products in the ProductList array
    //    size_t* PtrSelectedProduct                 // [out] This is the selected product to open. On input, set to 0 (aka the first product)
    //    );

    //public struct CASC_OPEN_STORAGE_ARGS
    //{
    //    size_t Size;                                // Length of this structure. Initialize to sizeof(CASC_OPEN_STORAGE_ARGS)

    //    LPCTSTR szLocalPath;                        // Local:  Path to the storage directory (where ".build.info: is) or any of the sub-path
    //                                                // Online: Path to the local storage cache

    //    LPCTSTR szCodeName;                         // If non-null, this will specify a product in a multi-product local storage
    //                                                // Has higher priority than PfnProductCallback (if both specified)
    //    LPCTSTR szRegion;                           // If non-null, this will specify a product region.

    //    PFNPROGRESSCALLBACK PfnProgressCallback;    // Progress callback. If non-NULL, this can inform the caller about state of the opening storage
    //    void* PtrProgressParam;                    // Pointer-sized parameter that will be passed to PfnProgressCallback
    //    PFNPRODUCTCALLBACK PfnProductCallback;      // Progress callback. If non-NULL, will be called on multi-product storage to select one of the products
    //    void* PtrProductParam;                     // Pointer-sized parameter that will be passed to PfnProgressCallback

    //    uint dwLocaleMask;                         // Locale mask to open
    //    uint dwFlags;                              // Reserved. Set to zero.

    //    //
    //    // Any additional member from here on must be checked for availability using the ExtractVersionedArgument function.
    //    // Example:
    //    //
    //    // LPCTSTR szBuildKey = NULL;
    //    // ExtractVersionedArgument(pArgs, offsetof(CASC_OPEN_STORAGE_ARGS, szBuildId), &szBuildKey);
    //    //

    //    LPCTSTR szBuildKey;                         // If non-null, this will specify a build key (aka MD5 of build config that is different that current online version)
    //}
}
