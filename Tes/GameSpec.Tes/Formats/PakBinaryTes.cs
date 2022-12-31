using GameSpec.Formats;
using OpenStack.Graphics.DirectX;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Tes.Formats
{
    public unsafe class PakBinaryTes : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryTes();
        PakBinaryTes() { }

        // Header : DAT
        #region Header : DAT
        // https://falloutmods.fandom.com/wiki/DAT_file_format

        const uint F2_BSAHEADER_FILEID = 0x000000011;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F2_Header
        {
            public uint TreeSize;               // Size of DirTree in bytes
            public uint DataSize;               // Full size of the archive in bytes
        }

        #endregion

        // Header : TES3
        #region Header : TES3
        // http://en.uesp.net/wiki/Tes3Mod:BSA_File_Format

        // Default header data
        const uint MW_BSAHEADER_FILEID = 0x00000100; // Magic for Morrowind BSA

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct MW_Header
        {
            public uint HashOffset;         // Offset of hash table minus header size (12)
            public uint FileCount;          // Number of files in the archive
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct MW_HeaderFile
        {
            public uint FileSize;           // File size
            public uint FileOffset;         // File offset relative to data position
            public uint Size => FileSize > 0 ? FileSize & 0x3FFFFFFF : 0; // The size of the file inside the BSA
        }

        #endregion

        // Header : TES4
        #region Header : TES4
        // http://en.uesp.net/wiki/Tes4Mod:BSA_File_Format

        // Default header data
        const uint OB_BSAHEADER_FILEID = 0x00415342;    // Magic for Oblivion BSA, the literal string "BSA\0".
        const uint OB_BSAHEADER_VERSION = 0x67;         // Version number of an Oblivion BSA
        const uint F3_BSAHEADER_VERSION = 0x68;         // Version number of a Fallout 3 BSA
        const uint SSE_BSAHEADER_VERSION = 0x69;        // Version number of a Skyrim SE BSA

        // Archive flags
        const ushort OB_BSAARCHIVE_PATHNAMES = 0x0001;  // Whether the BSA has names for paths
        const ushort OB_BSAARCHIVE_FILENAMES = 0x0002;  // Whether the BSA has names for files
        const ushort OB_BSAARCHIVE_COMPRESSFILES = 0x0004; // Whether the files are compressed
        const ushort F3_BSAARCHIVE_PREFIXFULLFILENAMES = 0x0100; // Whether the name is prefixed to the data?

        // File flags
        //const ushort OB_BSAFILE_NIF = 0x0001; // Set when the BSA contains NIF files (Meshes)
        //const ushort OB_BSAFILE_DDS = 0x0002; // Set when the BSA contains DDS files (Textures)
        //const ushort OB_BSAFILE_XML = 0x0004; // Set when the BSA contains XML files (Menus)
        //const ushort OB_BSAFILE_WAV = 0x0008; // Set when the BSA contains WAV files (Sounds)
        //const ushort OB_BSAFILE_MP3 = 0x0010; // Set when the BSA contains MP3 files (Voices)
        //const ushort OB_BSAFILE_TXT = 0x0020; // Set when the BSA contains TXT files (Shaders)
        //const ushort OB_BSAFILE_HTML = 0x0020; // Set when the BSA contains HTML files
        //const ushort OB_BSAFILE_BAT = 0x0020; // Set when the BSA contains BAT files
        //const ushort OB_BSAFILE_SCC = 0x0020; // Set when the BSA contains SCC files
        //const ushort OB_BSAFILE_SPT = 0x0040; // Set when the BSA contains SPT files (Trees)
        //const ushort OB_BSAFILE_TEX = 0x0080; // Set when the BSA contains TEX files
        //const ushort OB_BSAFILE_FNT = 0x0080; // Set when the BSA contains FNT files (Fonts)
        //const ushort OB_BSAFILE_CTL = 0x0100; // Set when the BSA contains CTL files (Miscellaneous)

        // Bitmasks for the size field in the header
        const uint OB_BSAFILE_SIZEMASK = 0x3fffffff; // Bit mask with OB_HeaderFile:SizeFlags to get the compression status
        const uint OB_BSAFILE_SIZECOMPRESS = 0xC0000000; // Bit mask with OB_HeaderFile:SizeFlags to get the compression status

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct OB_Header
        {
            public uint Version;            // 04
            public uint FolderRecordOffset; // Offset of beginning of folder records
            public uint ArchiveFlags;       // Archive flags
            public uint FolderCount;        // Total number of folder records (OBBSAFolderInfo)
            public uint FileCount;          // Total number of file records (OBBSAFileInfo)
            public uint FolderNameLength;   // Total length of folder names
            public uint FileNameLength;     // Total length of file names
            public uint FileFlags;          // File flags
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct OB_HeaderFolder
        {
            public ulong Hash;              // Hash of the folder name
            public uint FileCount;          // Number of files in folder
            public uint Offset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct OB_HeaderFolderSSE
        {
            public ulong Hash;              // Hash of the folder name
            public uint FileCount;          // Number of files in folder
            public uint Unk;
            public ulong Offset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct OB_HeaderFile
        {
            public ulong Hash;              // Hash of the filename
            public uint Size;               // Size of the data, possibly with OB_BSAFILE_SIZECOMPRESS set
            public uint Offset;             // Offset to raw file data
        }

        #endregion

        // Header : TES5
        #region Header : TES5
        // http://en.uesp.net/wiki/Tes5Mod:Archive_File_Format

        // Default header data
        const uint F4_BSAHEADER_FILEID = 0x58445442; // Magic for Fallout 4 BA2, the literal string "BTDX".
        const uint F4_BSAHEADER_VERSION = 0x01; // Version number of a Fallout 4 BA2
        const uint F4_HEADERTYPE_GNRL = 0x4c524e47;
        const uint F4_HEADERTYPE_GNMF = 0x464d4e47;
        const uint F4_HEADERTYPE_DX10 = 0x30315844;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_Header
        {
            public uint Version;            // 04
            public uint Type;               // 08 GNRL=General, DX10=Textures, GNMF=?, ___=?
            public uint NumFiles;           // 0C
            public ulong NameTableOffset;   // 10 - relative to start of file
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_HeaderFile
        {
            public uint NameHash;           // 00
            public fixed byte Ext[4];       // 04 - extension
            public uint DirHash;            // 08
            public uint Flags;              // 0C - flags? 00100100
            public ulong Offset;            // 10 - relative to start of file
            public uint PackedSize;         // 18 - packed length (zlib)
            public uint FileSize;           // 1C - unpacked length
            public uint Align;              // 20 - BAADF00D
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_HeaderTexture
        {
            public uint NameHash;           // 00
            public fixed byte Ext[4];       // 04 - extension
            public uint DirHash;            // 08
            public byte Unk0C;              // 0C
            public byte NumChunks;          // 0D
            public ushort ChunkHeaderSize;  // 0E - size of one chunk header
            public ushort Height;           // 10
            public ushort Width;            // 12
            public byte NumMips;            // 14
            public byte Format; // 15 - DXGI_FORMAT
            public ushort Unk16;            // 16 - 0800
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_HeaderGNMF
        {
            public uint NameHash;           // 00
            public fixed byte Ext[4];       // 04 - extension
            public uint DirHash;            // 08
            public byte Unk0C;              // 0C
            public byte NumChunks;          // 0D
            public ushort Unk0E;            // 0E
            public fixed byte Header[32];   // 10
            public ulong Offset;            // 30
            public uint PackedSize;         // 38
            public uint FileSize;           // 3C
            public uint Unk40;              // 40
            public uint Align;              // 44
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F4_HeaderTextureChunk
        {
            public ulong Offset;            // 00
            public uint PackedSize;         // 08
            public uint FileSize;           // 0C
            public ushort StartMip;         // 10
            public ushort EndMip;           // 12
            public uint Align;              // 14 - BAADFOOD
        }

        #endregion

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());
            FileMetadata[] files;

            // Fallout 4
            var magic = source.Magic = r.ReadUInt32();
            if (magic == F4_BSAHEADER_FILEID)
            {
                var header = r.ReadT<F4_Header>(sizeof(F4_Header));
                if (header.Version != F4_BSAHEADER_VERSION) throw new FormatException("BAD MAGIC");
                source.Version = header.Version;
                multiSource.Files = files = new FileMetadata[header.NumFiles];
                // General BA2 Format
                if (header.Type == F4_HEADERTYPE_GNRL)
                {
                    var headerFiles = r.ReadTArray<F4_HeaderFile>(sizeof(F4_HeaderFile), (int)header.NumFiles);
                    for (var i = 0; i < headerFiles.Length; i++)
                    {
                        var headerFile = headerFiles[i];
                        files[i] = new FileMetadata
                        {
                            Compressed = headerFile.PackedSize != 0 ? 1 : 0,
                            PackedSize = headerFile.PackedSize,
                            FileSize = headerFile.FileSize,
                            Position = (long)headerFile.Offset,
                        };
                    }
                }
                // Texture BA2 Format
                else if (header.Type == F4_HEADERTYPE_DX10)
                    for (var i = 0; i < header.NumFiles; i++)
                    {
                        var headerTexture = r.ReadT<F4_HeaderTexture>(sizeof(F4_HeaderTexture));
                        var headerTextureChunks = r.ReadTArray<F4_HeaderTextureChunk>(sizeof(F4_HeaderTextureChunk), headerTexture.NumChunks);
                        var firstChunk = headerTextureChunks[0];
                        files[i] = new FileMetadata
                        {
                            FileInfo = headerTexture,
                            PackedSize = firstChunk.PackedSize,
                            FileSize = firstChunk.FileSize,
                            Position = (long)firstChunk.Offset,
                            Tag = headerTextureChunks,
                        };
                    }
                // GNMF BA2 Format
                else if (header.Type == F4_HEADERTYPE_GNMF)
                    for (var i = 0; i < header.NumFiles; i++)
                    {
                        var headerGNMF = r.ReadT<F4_HeaderGNMF>(sizeof(F4_HeaderGNMF));
                        var headerTextureChunks = r.ReadTArray<F4_HeaderTextureChunk>(sizeof(F4_HeaderTextureChunk), headerGNMF.NumChunks);
                        files[i] = new FileMetadata
                        {
                            FileInfo = headerGNMF,
                            PackedSize = headerGNMF.PackedSize,
                            FileSize = headerGNMF.FileSize,
                            Position = (long)headerGNMF.Offset,
                            Tag = headerTextureChunks,
                        };

                    }
                else throw new ArgumentOutOfRangeException(nameof(header.Type), header.Type.ToString());

                // Assign full names to each file
                if (header.NameTableOffset > 0)
                {
                    r.Position((long)header.NameTableOffset);
                    var path = r.ReadL16Encoding().Replace('\\', '/');
                    foreach (var file in files) file.Path = path;
                }
            }

            // Oblivion - Skyrim
            else if (magic == OB_BSAHEADER_FILEID)
            {
                var header = r.ReadT<OB_Header>(sizeof(OB_Header));
                if (header.Version != OB_BSAHEADER_VERSION && header.Version != F3_BSAHEADER_VERSION && header.Version != SSE_BSAHEADER_VERSION) throw new FormatException("BAD MAGIC");
                if ((header.ArchiveFlags & OB_BSAARCHIVE_PATHNAMES) == 0 || (header.ArchiveFlags & OB_BSAARCHIVE_FILENAMES) == 0) throw new FormatException("HEADER FLAGS");
                source.Version = header.Version;

                // calculate some useful values
                var compressedToggle = (header.ArchiveFlags & OB_BSAARCHIVE_COMPRESSFILES) > 0;
                if (header.Version == F3_BSAHEADER_VERSION || header.Version == SSE_BSAHEADER_VERSION) source.Params["namePrefix"] = (header.ArchiveFlags & F3_BSAARCHIVE_PREFIXFULLFILENAMES) > 0 ? "Y" : "N";

                // read-all folders
                var foldersFiles = header.Version == SSE_BSAHEADER_VERSION
                    ? r.ReadTArray<OB_HeaderFolderSSE>(sizeof(OB_HeaderFolderSSE), (int)header.FolderCount).Select(x => x.FileCount).ToArray()
                    : r.ReadTArray<OB_HeaderFolder>(sizeof(OB_HeaderFolder), (int)header.FolderCount).Select(x => x.FileCount).ToArray();

                // read-all folder files
                var fileIdx = 0U;
                multiSource.Files = files = new FileMetadata[header.FileCount];
                for (var i = 0; i < header.FolderCount; i++)
                {
                    var folder_name = r.ReadFString(r.ReadByte() - 1).Replace('\\', '/'); r.Skip(1);
                    var headerFiles = r.ReadTArray<OB_HeaderFile>(sizeof(OB_HeaderFile), (int)foldersFiles[i]);
                    foreach (var headerFile in headerFiles)
                    {
                        var compressed = (headerFile.Size & OB_BSAFILE_SIZECOMPRESS) != 0;
                        files[fileIdx++] = new FileMetadata
                        {
                            Path = folder_name,
                            Position = headerFile.Offset,
                            Compressed = compressed ^ compressedToggle ? 1 : 0,
                            PackedSize = compressed ? headerFile.Size ^ OB_BSAFILE_SIZECOMPRESS : headerFile.Size,
                        };
                    };
                }

                // read-all names
                foreach (var file in files) file.Path = $"{file.Path}/{r.ReadZString()}";
            }

            // Morrowind
            else if (magic == MW_BSAHEADER_FILEID)
            {
                var header = r.ReadT<MW_Header>(sizeof(MW_Header));
                var dataOffset = 12 + header.HashOffset + (8 * header.FileCount);

                // Create file metadatas
                multiSource.Files = files = new FileMetadata[header.FileCount];
                var headerFiles = r.ReadTArray<MW_HeaderFile>(sizeof(MW_HeaderFile), (int)header.FileCount);
                for (var i = 0; i < headerFiles.Length; i++)
                {
                    var headerFile = headerFiles[i];
                    files[i] = new FileMetadata
                    {
                        PackedSize = headerFile.Size,
                        Position = dataOffset + headerFile.FileOffset,
                    };
                }

                // Read filename offsets
                var filenameOffsets = r.ReadTArray<uint>(sizeof(uint), (int)header.FileCount); // relative offset in filenames section

                // Read filenames
                var filenamesPosition = r.Position();
                for (var i = 0; i < files.Length; i++)
                {
                    r.Position(filenamesPosition + filenameOffsets[i]);
                    files[i].Path = r.ReadZASCII(1000).Replace('\\', '/');
                }
            }

            // Fallout 2
            else if (string.Equals(Path.GetExtension(source.FilePath), ".dat", StringComparison.OrdinalIgnoreCase))
            {
                source.Magic = F2_BSAHEADER_FILEID;
                r.Position(r.BaseStream.Length - 8);
                var header = r.ReadT<F2_Header>(sizeof(F2_Header));
                if (header.DataSize != r.BaseStream.Length) throw new InvalidOperationException("File is not a valid bsa archive.");
                r.Position(header.DataSize - header.TreeSize - 8);

                // Create file metadatas
                multiSource.Files = files = new FileMetadata[r.ReadInt32()];
                for (var i = 0; i < files.Length; i++)
                    files[i] = new FileMetadata
                    {
                        Path = r.ReadL32Encoding().TrimStart('\\'),
                        Compressed = r.ReadByte(),
                        FileSize = r.ReadUInt32(),
                        PackedSize = r.ReadUInt32(),
                        Position = r.ReadUInt32(),
                    };
            }
            else throw new InvalidOperationException("BAD MAGIC");
            return Task.CompletedTask;
        }

        public override Task WriteAsync(BinaryPakFile source, BinaryWriter w, WriteStage stage)
            => throw new NotImplementedException();

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            const int GNF_HEADER_MAGIC = 0x20464E47;
            const int GNF_HEADER_CONTENT_SIZE = 248;
            Stream fileData = null;
            var magic = source.Magic;
            // BSA2
            if (magic == F4_BSAHEADER_FILEID)
            {
                // position
                r.Position(file.Position);

                // General BA2 Format
                if (file.FileInfo == null)
                    fileData = file.Compressed != 0
                        ? new MemoryStream(r.DecompressZlib_2((int)file.PackedSize, (int)file.FileSize))
                        : new MemoryStream(r.ReadBytes((int)file.FileSize));
                // Texture BA2 Format
                else if (file.FileInfo is F4_HeaderTexture tex)
                {
                    var s = new MemoryStream();
                    {
                        // write header
                        var w = new BinaryWriter(s);
                        var ddsHeader = new DDS_HEADER
                        {
                            dwSize = DDS_HEADER.SizeOf,
                            dwFlags = DDSD.HEADER_FLAGS_TEXTURE | DDSD.HEADER_FLAGS_LINEARSIZE | DDSD.HEADER_FLAGS_MIPMAP,
                            dwHeight = tex.Height,
                            dwWidth = tex.Width,
                            dwMipMapCount = tex.NumMips,
                            dwCaps = DDSCAPS.SURFACE_FLAGS_TEXTURE | DDSCAPS.SURFACE_FLAGS_MIPMAP,
                            dwCaps2 = tex.Unk16 == 2049 ? DDSCAPS2.CUBEMAP_ALLFACES : 0,
                        };
                        ddsHeader.ddspf.dwSize = DDS_PIXELFORMAT.SizeOf;
                        switch ((DXGI_FORMAT)tex.Format)
                        {
                            case DXGI_FORMAT.BC1_UNORM:
                                ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                                ddsHeader.ddspf.dwFourCC = DDS_HEADER.DXT1;
                                ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height / 2U); // 4bpp
                                break;
                            case DXGI_FORMAT.BC2_UNORM:
                                ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                                ddsHeader.ddspf.dwFourCC = DDS_HEADER.DXT3;
                                ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                                break;
                            case DXGI_FORMAT.BC3_UNORM:
                                ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                                ddsHeader.ddspf.dwFourCC = DDS_HEADER.DXT5;
                                ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                                break;
                            case DXGI_FORMAT.BC5_UNORM:
                                ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                                ddsHeader.ddspf.dwFourCC = DDS_HEADER.ATI2;
                                ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                                break;
                            case DXGI_FORMAT.BC1_UNORM_SRGB:
                                ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                                ddsHeader.ddspf.dwFourCC = DDS_HEADER.DX10;
                                ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height / 2); // 4bpp
                                break;
                            case DXGI_FORMAT.BC3_UNORM_SRGB:
                            case DXGI_FORMAT.BC4_UNORM:
                            case DXGI_FORMAT.BC5_SNORM:
                            case DXGI_FORMAT.BC7_UNORM:
                            case DXGI_FORMAT.BC7_UNORM_SRGB:
                                ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                                ddsHeader.ddspf.dwFourCC = DDS_HEADER.DX10;
                                ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                                break;
                            case DXGI_FORMAT.R8G8B8A8_UNORM:
                            case DXGI_FORMAT.R8G8B8A8_UNORM_SRGB:
                                ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHA;
                                ddsHeader.ddspf.dwRGBBitCount = 32;
                                ddsHeader.ddspf.dwRBitMask = 0x000000FF;
                                ddsHeader.ddspf.dwGBitMask = 0x0000FF00;
                                ddsHeader.ddspf.dwBBitMask = 0x00FF0000;
                                ddsHeader.ddspf.dwABitMask = 0xFF000000;
                                ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height * 4); // 32bpp
                                break;
                            case DXGI_FORMAT.B8G8R8A8_UNORM:
                            case DXGI_FORMAT.B8G8R8X8_UNORM:
                                ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHA;
                                ddsHeader.ddspf.dwRGBBitCount = 32;
                                ddsHeader.ddspf.dwRBitMask = 0x00FF0000;
                                ddsHeader.ddspf.dwGBitMask = 0x0000FF00;
                                ddsHeader.ddspf.dwBBitMask = 0x000000FF;
                                ddsHeader.ddspf.dwABitMask = 0xFF000000;
                                ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height * 4); // 32bpp
                                break;
                            case DXGI_FORMAT.R8_UNORM:
                                ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHA;
                                ddsHeader.ddspf.dwRGBBitCount = 8;
                                ddsHeader.ddspf.dwRBitMask = 0xFF;
                                ddsHeader.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height); // 8bpp
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(tex.Format), $"Unsupported DDS header format. File: {file.Path}");
                        }
                        w.Write(DDS_HEADER.DDS_);
                        w.WriteT(ddsHeader, sizeof(DDS_HEADER));
                        switch ((DXGI_FORMAT)tex.Format)
                        {
                            case DXGI_FORMAT.BC1_UNORM_SRGB:
                            case DXGI_FORMAT.BC3_UNORM_SRGB:
                            case DXGI_FORMAT.BC4_UNORM:
                            case DXGI_FORMAT.BC5_SNORM:
                            case DXGI_FORMAT.BC7_UNORM:
                            case DXGI_FORMAT.BC7_UNORM_SRGB:
                                var dxt10 = new DDS_HEADER_DXT10
                                {
                                    dxgiFormat = (DXGI_FORMAT)tex.Format,
                                    resourceDimension = D3D10_RESOURCE_DIMENSION.TEXTURE2D,
                                    miscFlag = 0,
                                    arraySize = 1,
                                    miscFlags2 = (uint)DDS_ALPHA_MODE.ALPHA_MODE_UNKNOWN,
                                };
                                w.WriteT(dxt10, sizeof(DDS_HEADER_DXT10));
                                break;
                        }

                        // write chunks
                        var chunks = (F4_HeaderTextureChunk[])file.Tag;
                        for (var i = 0; i < tex.NumChunks; i++)
                        {
                            var chunk = chunks[i];
                            r.Position((long)chunk.Offset);
                            if (chunk.PackedSize != 0) s.WriteBytes(r.DecompressZlib((int)file.PackedSize, (int)file.FileSize));
                            else s.WriteBytes(r, (int)file.FileSize);
                        }
                        s.Position = 0;
                        fileData = s;
                    }
                }
                // GNMF BA2 Format
                else if (file.FileInfo is F4_HeaderGNMF gnmf)
                {
                    var s = new MemoryStream();
                    {
                        // write header
                        var w = new BinaryWriter(s);
                        w.Write(GNF_HEADER_MAGIC); // 'GNF ' magic
                        w.Write(GNF_HEADER_CONTENT_SIZE); // Content-size. Seems to be either 4 or 8 bytes
                        w.Write((byte)0x2); // Version
                        w.Write((byte)0x1); // Texture Count
                        w.Write((byte)0x8); // Alignment
                        w.Write((byte)0x0); // Unused
                        w.Write(BitConverter.GetBytes(gnmf.FileSize + 256).Reverse().ToArray()); // File size + header size
                        w.Write(UnsafeX.ReadBytes(gnmf.Header, 32));
                        for (var i = 0; i < 208; i++) w.Write((byte)0x0); // Padding

                        // write chunks
                        var chunks = (F4_HeaderTextureChunk[])file.Tag;
                        for (var i = 0; i < gnmf.NumChunks; i++)
                        {
                            var chunk = chunks[i];
                            r.Position((long)chunk.Offset);
                            if (chunk.PackedSize != 0) s.WriteBytes(r.DecompressZlib_2((int)file.PackedSize, (int)file.FileSize));
                            else s.WriteBytes(r, (int)file.FileSize);
                        }
                        s.Position = 0;
                        fileData = s;
                    }
                }
                else throw new ArgumentOutOfRangeException(nameof(file.FileInfo), file.FileInfo.ToString());
            }
            // BSA
            else if (magic == OB_BSAHEADER_FILEID || magic == MW_BSAHEADER_FILEID || magic == F2_BSAHEADER_FILEID)
            {
                // position
                var fileSize = (int)(source.Version == SSE_BSAHEADER_VERSION
                    ? file.PackedSize & OB_BSAFILE_SIZEMASK
                    : file.PackedSize);
                r.Position(file.Position);
                if (source.Params.TryGetValue("namePrefix", out var z2) && z2 == "Y")
                {
                    var prefixLength = r.ReadByte() + 1;
                    if (source.Version == SSE_BSAHEADER_VERSION)
                        fileSize -= prefixLength;
                    r.Position(file.Position + prefixLength);
                }

                // fallout2
                if (source.Magic == F2_BSAHEADER_FILEID)
                    fileData = r.Peek(z => z.ReadUInt16()) == 0xda78
                        ? new MemoryStream(r.DecompressZlib(fileSize, -1))
                        : new MemoryStream(r.ReadBytes(fileSize));
                // not compressed
                else if (fileSize <= 0 || file.Compressed == 0)
                    fileData = new MemoryStream(r.ReadBytes(fileSize));
                // compressed
                else
                {
                    var newFileSize = (int)r.ReadUInt32(); fileSize -= 4;
                    fileData = source.Version == SSE_BSAHEADER_VERSION
                        ? new MemoryStream(r.DecompressLz4(fileSize, newFileSize))
                        : new MemoryStream(r.DecompressZlib_2(fileSize, newFileSize));
                }
            }
            else throw new InvalidOperationException("BAD MAGIC");
            return Task.FromResult(fileData);
        }
    }
}