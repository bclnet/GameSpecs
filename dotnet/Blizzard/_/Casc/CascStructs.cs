/// <summary>
/// On-disk structures for CASC storages
/// </summary>
using static GameX.Blizzard.Formats.Casc.CascStructs;
using static GameX.Blizzard.Formats.Casc.CascLib;

namespace GameX.Blizzard.Formats.Casc
{
	public static partial class CascStructs
	{
		// Common definitions
		public const int CASC_INDEX_COUNT = 0x10;               // Number of index files
		public const int CASC_CKEY_SIZE = 0x10;                 // Size of the content key
		public const int CASC_EKEY_SIZE = 0x09;                 // Size of the encoded key
		public const int CASC_MAX_DATA_FILES = 0x100;           // Maximum number of data files

		// The index files structures
		public const int FILE_INDEX_PAGE_SIZE = 0x200;          // Number of bytes in one page of EKey items
	}

	// Structure describing the 32-bit block size and 32-bit Jenkins hash of the block
	public struct BLOCK_SIZE_AND_HASH
	{
		public uint cbBlockSize;
		public uint dwBlockHash;
	}

	// Checksum describing a block in the index file (v2)
	public struct FILE_INDEX_GUARDED_BLOCK
	{
		public uint BlockSize;
		public uint BlockHash;
	}

	// Structure of the header of the index files version 1
	public struct FILE_INDEX_HEADER_V1
	{
		public ushort IndexVersion;                             // Must be 0x05
		public byte BucketIndex;                                // The bucket index of this file; should be the same as the first byte of the hex filename. 
		public byte align_3;
		public uint field_4;
		public ulong field_8;
		public ulong SegmentSize;                               // Size of one data segment (aka data.### file)
		public byte EncodedSizeLength;                          // Length, in bytes, of the EncodedSize in the EKey entry
		public byte StorageOffsetLength;                        // Length, in bytes, of the StorageOffset field in the EKey entry
		public byte EKeyLength;                                 // Length of the encoded key (bytes)
		public byte FileOffsetBits;                             // Number of bits of the archive file offset in StorageOffset field. Rest is data segment index
		public uint EKeyCount1;
		public uint EKeyCount2;
		public uint KeysHash1;
		public uint KeysHash2;
		public uint HeaderHash;
	}

	public struct FILE_INDEX_HEADER_V2
	{
		public ushort IndexVersion;                             // Must be 0x07
		public byte BucketIndex;                                // The bucket index of this file; should be the same as the first byte of the hex filename. 
		public byte ExtraBytes;                                 // Unknown; must be 0
		public byte EncodedSizeLength;                          // Length, in bytes, of the EncodedSize in the EKey entry
		public byte StorageOffsetLength;                        // Length, in bytes, of the StorageOffset field in the EKey entry
		public byte EKeyLength;                                 // Length of the encoded key (bytes)
		public byte FileOffsetBits;                             // Number of bits of the archive file offset in StorageOffset field. Rest is data segment index
		public ulong SegmentSize;                               // Size of one data segment (aka data.### file)
	}

	// The EKey entry from the ".idx" files. Note that the lengths are optional and controlled by the FILE_INDEX_HEADER_Vx
	public unsafe struct FILE_EKEY_ENTRY
	{
		public fixed byte EKey[CASC_EKEY_SIZE];                 // The first 9 bytes of the encoded key
		public fixed byte FileOffsetBE[5];                            // Index of data file and offset within (big endian).
		public fixed byte EncodedSize[4];                             // Encoded size (big endian). This is the size of encoded header, all file frame headers and all file frames
	}

	//-----------------------------------------------------------------------------
	// The archive index (md5.index) files structures
	// https://wowdev.wiki/TACT#CDN_File_Organization

	public unsafe struct FILE_INDEX_FOOTER_X32
	{
		public fixed byte TocHash[MD5_HASH_SIZE];               // Client tries to read with 0x10, then backs off in size when smaller
		public byte Version;                                    // Version of the index header
		public fixed byte Reserved[2];                          // Length, in bytes, of the file offset field
		public byte PageSizeKB;                                 // Length, in kilobytes, of the index page
		public byte OffsetBytes;                                // Normally 4 for archive indices, 6 for group indices, and 0 for loose file indices
		public byte SizeBytes;                                  // Normally 4
		public byte EKeyLength;                                 // Normally 16
		public byte FooterHashBytes;                            // Normally 8, <= 0x10
		public fixed byte ElementCount[4];                      // BigEndian in _old_ versions (e.g. 18179)
		public fixed byte FooterHash[32]; //: X32 - CHKSUM_LENGTH
    }

	//-----------------------------------------------------------------------------
	// The ENCODING manifest structures
	//
	// The ENCODING file is in the form of:
	// * File header. Fixed length.
	// * Encoding Specification (ESpec) in the string form. Length is stored in FILE_ENCODING_HEADER::ESpecBlockSize
	// https://wowdev.wiki/CASC#Encoding
	// https://wowdev.wiki/TACT#Encoding_table
	partial class CascStructs
	{
		public const int FILE_MAGIC_ENCODING = 0x4E45;          // 'EN'
	}

	// File header of the ENCODING manifest
	public unsafe struct FILE_ENCODING_HEADER
	{
		public ushort Magic;                                    // FILE_MAGIC_ENCODING ('EN')
		public byte Version;                                    // Expected to be 1 by CascLib
		public byte CKeyLength;                                 // The content key length in ENCODING file. Usually 0x10
		public byte EKeyLength;                                 // The encoded key length in ENCODING file. Usually 0x10
		public fixed byte CKeyPageSize[2];                      // Size of the CKey page, in KB (big-endian)
		public fixed byte EKeyPageSize[2];                      // Size of the EKey page, in KB (big-endian)
		public fixed byte CKeyPageCount[4];                     // Number of CKey pages in the table (big endian)
		public fixed byte EKeyPageCount[4];                     // Number of EKey pages in the table (big endian)
		public byte field_11;                                   // Asserted to be zero by the agent
		public fixed byte ESpecBlockSize[4];                    // Size of the ESpec string block
	}

	// Page header of the ENCODING manifest
	public unsafe struct FILE_CKEY_PAGE
	{
		public fixed byte FirstKey[MD5_HASH_SIZE];              // The first CKey/EKey in the segment
		public fixed byte SegmentHash[MD5_HASH_SIZE];           // MD5 hash of the entire segment
	}

	// Single entry in the page
	public unsafe struct FILE_CKEY_ENTRY
	{
		public ushort EKeyCount;                                // Number of EKeys
		public fixed byte ContentSize[4];                       // Content file size (big endian)
		public fixed byte CKey[CASC_CKEY_SIZE];                 // Content key. This is MD5 of the file content
		public fixed byte EKey[CASC_CKEY_SIZE];                 // Encoded key. This is (trimmed) MD5 hash of the file header, containing MD5 hashes of all the logical blocks of the file
	}

	public unsafe struct FILE_ESPEC_ENTRY
	{
		public fixed byte ESpecKey[MD5_HASH_SIZE];              // The ESpec key of the file
		public fixed byte ESpecIndexBE[4];                      // Index of ESPEC entry, assuming zero-terminated strings (big endian)
		public fixed byte FileSizeBE[5];                        // Size of the encoded version of the file (big endian)
	}

	//-----------------------------------------------------------------------------
	// The DOWNLOAD manifest structures
	//
	// See https://wowdev.wiki/TACT#Download_manifest
	//
	partial class CascStructs
	{
		public const int FILE_MAGIC_DOWNLOAD = 0x4C44;          // 'DL'
	}

	// File header of the DOWNLOAD manifest
	public unsafe struct FILE_DOWNLOAD_HEADER
	{
		public ushort Magic;                                    // FILE_MAGIC_DOWNLOAD ('DL')
		public byte Version;                                    // Expected to be 1 by CascLib
		public byte EKeyLength;                                 // The content key length in DOWNLOAD file. Expected to be 0x10
		public byte EntryHasChecksum;                           // If nonzero, then the entry has checksum.
		public fixed byte EntryCount[4];                        // Number of entries (big-endian)
		public fixed byte TagCount[2];                          // Number of tag entries (big endian)
																// Version 2 or newer
		public byte FlagByteSize;                               // Number of flag bytes
																// Verion 3 or newer
		public byte BasePriority;
		public fixed byte Unknown2[3];
	}

	public unsafe struct FILE_DOWNLOAD_ENTRY
	{
		public fixed byte EKey[MD5_HASH_SIZE];                  // Encoding key (variable length)
		public fixed byte FileSize[5];                          // File size
		public byte Priority;
		//uint Checksum[optional]
		//byte Flags;
	}

	//-----------------------------------------------------------------------------
	// The INSTALL manifest structures
	//
	// See https://wowdev.wiki/TACT#Install_manifest
	//
	partial class CascStructs
	{
		public const int FILE_MAGIC_INSTALL = 0x4E49;           // 'IN'
	}

	// File header of the INSTALL manifest
	public unsafe struct FILE_INSTALL_HEADER
	{
		public ushort Magic;                                    // FILE_MAGIC_INSTALL ('DL')
		public byte Version;                                    // Expected to be 1 by CascLib
		public byte EKeyLength;                                 // The content key length in INSTALL file. Expected to be 0x10
		public fixed byte TagCount[2];                          // Number of tag entries (big endian)
		public fixed byte EntryCount[4];                        // Number of entries (big-endian)
	}

	//-----------------------------------------------------------------------------
	// Data file structures
	partial class CascStructs
	{
		public const int BLTE_HEADER_SIGNATURE = 0x45544C42;    // 'BLTE' header in the data files
		public const int BLTE_HEADER_DELTA = 0x1E;              // Distance of BLTE header from begin of the header area
		public const int MAX_ENCODED_HEADER = 0x1000;           // Starting size for the frame headers
	}

	public unsafe struct BLTE_HEADER
	{
		public fixed byte Signature[4];                         // Must be "BLTE"
		public fixed byte HeaderSize[4];                        // Header size in bytes (big endian)
		public byte MustBe0F;                                   // Must be 0x0F. Optional, only if HeaderSize != 0
		public fixed byte FrameCount[3];                        // Frame count (big endian). Optional, only if HeaderSize != 0
	}

	public unsafe struct BLTE_ENCODED_HEADER
	{
		// Header span.
		public fixed byte EKey[MD5_HASH_SIZE];                  // Encoded key of the data beginning with "BLTE" (byte-reversed)
		public uint EncodedSize;                                // Encoded size of the data data beginning with "BLTE" (little endian)
		public byte field_14;                                   // Seems to be 1 if the header span has no data
		public byte field_15;                                   // Hardcoded to zero (Agent.exe 2.15.0.6296: 01370000->0148E2AA)
		public fixed byte JenkinsHash[4];                       // Jenkins hash (hashlittle2) of the preceding fields (EKey + EncodedSize + field_14 + field_15) (little endian)
		public fixed byte Checksum[4];                          // Checksum of the previous part. See "VerifyHeaderSpan()" for more information.

		// BLTE header. Always present.
		public fixed byte Signature[4];                         // Must be "BLTE"
		public fixed byte HeaderSize[4];                        // Header size in bytes (big endian)
		public byte MustBe0F;                                   // Must be 0x0F. Optional, only if HeaderSize != 0
		public fixed byte FrameCount[3];                        // Frame count (big endian). Optional, only if HeaderSize != 0
	}

	public unsafe struct BLTE_FRAME
	{
		public fixed byte EncodedSize[4];                       // Encoded frame size (big endian)
		public fixed byte ContentSize[4];                       // Content frame size (big endian)
		public fixed byte FrameHash[MD5_HASH_SIZE];             // Hash of the encoded frame
	}
}
