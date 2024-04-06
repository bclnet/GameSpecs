using GameX.Algorithms;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GameX.Valve.Formats
{
    // https://developer.valvesoftware.com/wiki/VPK_File_Format
    public unsafe class PakBinary_Vpk : PakBinary<PakBinary_Vpk>
    {
        #region Headers

        public const int MAGIC = 0x55aa1234;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HeaderV1
        {
            public static (string, int) Struct = ("<I", sizeof(HeaderV1));
            public uint TreeSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HeaderV2
        {
            public static (string, int) Struct = ("<5I", sizeof(HeaderV2));
            public uint TreeSize;
            public uint FileDataSectionSize;
            public uint ArchiveMd5SectionSize;
            public uint OtherMd5SectionSize;
            public uint SignatureSectionSize;
            public int ArchiveMd5Entries => (int)ArchiveMd5SectionSize / 28; // 28 is sizeof(VPK_MD5SectionEntry), which is int + int + int + 16 chars
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ArchiveMd5Entry
        {
            public uint ArchiveIndex; // Gets or sets the CRC32 checksum of this entry.
            public uint Offset; // Gets or sets the offset in the package.
            public uint Length; // Gets or sets the length in bytes.
            public fixed byte Checksum[16];// Gets or sets the expected Checksum checksum.
        }

        #endregion

        #region Verification

        /// <summary>
        /// Verification
        /// </summary>
        class Verification
        {
            public ArchiveMd5Entry[] ArchiveMd5Entries; // Gets the archive MD5 checksum section entries. Also known as cache line hashes.
            public byte[] TreeChecksum; // Gets the MD5 checksum of the file tree.
            public byte[] ArchiveMd5EntriesChecksum; // Gets the MD5 checksum of the archive MD5 checksum section entries.
            public byte[] WholeFileChecksum; // Gets the MD5 checksum of the complete package until the signature structure.
            public byte[] PublicKey; // Gets the public key.
            public byte[] Signature; // Gets the signature.

#if true
            /// <summary>
            /// Verify checksums and signatures provided in the VPK
            /// </summary>
            void VerifyHashesV2(BinaryReader r, int version, ref HeaderV2 header, long headerPosition)
            {
                if (version != 2) throw new InvalidDataException("Only version 2 is supported.");
                using (var md5 = MD5.Create())
                {
                    r.Seek(0);
                    var hash = md5.ComputeHash(r.ReadBytes((int)(headerPosition + header.TreeSize + header.FileDataSectionSize + header.ArchiveMd5SectionSize + 32)));
                    if (!hash.SequenceEqual(WholeFileChecksum)) throw new InvalidDataException($"Package checksum mismatch ({BitConverter.ToString(hash)} != expected {BitConverter.ToString(WholeFileChecksum)})");

                    r.Seek(headerPosition);
                    hash = md5.ComputeHash(r.ReadBytes((int)header.TreeSize));
                    if (!hash.SequenceEqual(TreeChecksum)) throw new InvalidDataException($"File tree checksum mismatch ({BitConverter.ToString(hash)} != expected {BitConverter.ToString(TreeChecksum)})");

                    r.Seek(headerPosition + header.TreeSize + header.FileDataSectionSize);
                    hash = md5.ComputeHash(r.ReadBytes((int)header.ArchiveMd5SectionSize));
                    if (!hash.SequenceEqual(ArchiveMd5EntriesChecksum)) throw new InvalidDataException($"Archive MD5 entries checksum mismatch ({BitConverter.ToString(hash)} != expected {BitConverter.ToString(ArchiveMd5EntriesChecksum)})");
                }
                if (PublicKey == null || Signature == null) return;
                if (!IsSignatureValidV2(r, ref header, headerPosition)) throw new InvalidDataException("VPK signature is not valid.");
            }

            /// <summary>
            /// Verifies the RSA signature.
            /// </summary>
            /// <returns>True if signature is valid, false otherwise.</returns>
            bool IsSignatureValidV2(BinaryReader r, ref HeaderV2 header, long headerPosition)
            {
                r.Seek(0);
                var keyParser = new AsnKeyParser(PublicKey);
                var rsa = RSA.Create();
                rsa.ImportParameters(keyParser.ParseRSAPublicKey());
                var data = r.ReadBytes((int)(headerPosition + header.TreeSize + header.FileDataSectionSize + header.ArchiveMd5SectionSize + header.OtherMd5SectionSize));
                return rsa.VerifyData(data, Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
#endif
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();

            // header
            if (r.ReadUInt32() != MAGIC) throw new FormatException("BAD MAGIC");
            var version = r.ReadUInt32();
            if (version > 2) throw new FormatException($"Bad VPK version. ({version})");
            var headerV1 = version == 1 ? r.ReadS<HeaderV1>() : default;
            var headerV2 = version != 1 ? r.ReadS<HeaderV2>() : default;
            var headerPosition = (uint)r.Tell();
            var headerTreeSize = version == 1 ? headerV1.TreeSize : headerV2.TreeSize;

            // sourceFilePath
            var sourceFilePath = source.PakPath;
            var sourceFileDirVpk = sourceFilePath.EndsWith("_dir.vpk", StringComparison.OrdinalIgnoreCase);
            if (sourceFileDirVpk) sourceFilePath = sourceFilePath[..^8];
            source.FileMask = path =>
            {
                var extension = Path.GetExtension(path);
                if (extension.EndsWith("_c", StringComparison.Ordinal)) extension = extension[..^2];
                if (extension.StartsWith(".v")) extension = extension.Remove(1, 1);
                return $"{Path.GetFileNameWithoutExtension(path)}{extension}";
            };

            // types
            while (true)
            {
                var typeName = r.ReadZUTF8(); if (typeName?.Length == 0) break;
                // directories
                while (true)
                {
                    var directoryName = r.ReadZUTF8(); if (directoryName?.Length == 0) break;
                    // files
                    while (true)
                    {
                        var fileName = r.ReadZUTF8(); if (fileName?.Length == 0) break;

                        var metadata = new FileSource
                        {
                            Path = $"{(directoryName[0] != ' ' ? $"{directoryName}/" : null)}{fileName}.{typeName}",
                            Hash = r.ReadUInt32(),
                            Extra = new byte[r.ReadUInt16()],
                            Id = r.ReadUInt16(),
                            Offset = r.ReadUInt32(),
                            FileSize = r.ReadUInt32(),
                        };
                        if (metadata.Id != 0x7FFF)
                        {
                            if (!sourceFileDirVpk) throw new FormatException("Given VPK is not a _dir, but entry is referencing an external archive.");
                            metadata.Tag = $"{sourceFilePath}_{metadata.Id:D3}.vpk";
                        }
                        else metadata.Tag = (long)(headerPosition + headerTreeSize);
                        files.Add(metadata);
                        if (r.ReadUInt16() != 0xFFFF) throw new FormatException("Invalid terminator.");
                        if (metadata.Extra.Length > 0) r.Read(metadata.Extra, 0, metadata.Extra.Length);
                    }
                }
            }

            // verification
            if (version == 2)
            {
                if (headerV2.OtherMd5SectionSize != 48) throw new FormatException($"Encountered OtherMD5Section with size of {headerV2.OtherMd5SectionSize} (should be 48)");
                // Skip over file data, if any
                r.Skip(headerV2.FileDataSectionSize);
                source.Tag = new Verification
                {
                    // archive md5
                    ArchiveMd5Entries = headerV2.ArchiveMd5SectionSize != 0 ? r.ReadTArray<ArchiveMd5Entry>(sizeof(ArchiveMd5Entry), (int)headerV2.ArchiveMd5Entries) : null,
                    // other md5
                    TreeChecksum = r.ReadBytes(16),
                    ArchiveMd5EntriesChecksum = r.ReadBytes(16),
                    WholeFileChecksum = r.ReadBytes(16),
                    // signature
                    PublicKey = headerV2.SignatureSectionSize != 0 ? r.ReadBytes(r.ReadInt32()) : null,
                    Signature = headerV2.SignatureSectionSize != 0 ? r.ReadBytes(r.ReadInt32()) : null
                };
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            var data = new byte[file.Extra.Length + file.FileSize];
            if (file.Extra.Length > 0) file.Extra.CopyTo(data, 0);
            if (file.FileSize > 0)
            {
                if (file.Tag is string path)
                    source.GetReader(path).Action(r2 =>
                    {
                        r2.Seek(file.Offset);
                        r2.Read(data, file.Extra.Length, (int)file.FileSize);
                    });
                else
                {
                    r.Seek(file.Offset + (long)file.Tag);
                    r.Read(data, file.Extra.Length, (int)file.FileSize);
                }
            }
            if (file.Hash != Crc32Digest.Compute(data)) throw new InvalidDataException("CRC32 mismatch for read data.");
            return Task.FromResult((Stream)new MemoryStream(data));
        }
    }
}