using GameSpec.Algorithms;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GameSpec.Valve.Formats
{
    public class PakBinaryValve : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryValve();
        PakBinaryValve() { }

        #region Headers

        public const int MAGIC = 0x55AA1234;

        /// <summary>
        /// Header
        /// </summary>
        public struct Header
        {
            public uint Version;
            public uint TreeSize;
            public uint FileDataSectionSize;
            public uint ArchiveMd5SectionSize;
            public uint OtherMd5SectionSize;
            public uint SignatureSectionSize;
            public uint HeaderSize;
        }

        /// <summary>
        /// ArchiveMd5Entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        public struct ArchiveMd5Entry
        {
            /// <summary>
            /// Gets or sets the CRC32 checksum of this entry.
            /// </summary>
            public uint ArchiveIndex { get; set; }
            /// <summary>
            /// Gets or sets the offset in the package.
            /// </summary>
            public uint Offset { get; set; }
            /// <summary>
            /// Gets or sets the length in bytes.
            /// </summary>
            public uint Length { get; set; }
            /// <summary>
            /// Gets or sets the expected Checksum checksum.
            /// </summary>
            public byte[] Checksum { get; set; }
        }

        #endregion

        #region Verification

        /// <summary>
        /// Verification
        /// </summary>
        public class Verification
        {
            /// <summary>
            /// Gets the archive MD5 checksum section entries. Also known as cache line hashes.
            /// </summary>
            public List<ArchiveMd5Entry> ArchiveMd5Entries;
            /// <summary>
            /// Gets the MD5 checksum of the file tree.
            /// </summary>
            public byte[] TreeChecksum;
            /// <summary>
            /// Gets the MD5 checksum of the archive MD5 checksum section entries.
            /// </summary>
            public byte[] ArchiveMd5EntriesChecksum;
            /// <summary>
            /// Gets the MD5 checksum of the complete package until the signature structure.
            /// </summary>
            public byte[] WholeFileChecksum;
            /// <summary>
            /// Gets the public key.
            /// </summary>
            public byte[] PublicKey;
            /// <summary>
            /// Gets the signature.
            /// </summary>
            public byte[] Signature;

            /// <summary>
            /// Verify checksums and signatures provided in the VPK
            /// </summary>
            public void VerifyHashes(BinaryReader r, Header header)
            {
                if (header.Version != 2) throw new InvalidDataException("Only version 2 is supported.");
                using (var md5 = MD5.Create())
                {
                    r.Position(0);
                    var hash = md5.ComputeHash(r.ReadBytes((int)(header.HeaderSize + header.TreeSize + header.FileDataSectionSize + header.ArchiveMd5SectionSize + 32)));
                    if (!hash.SequenceEqual(WholeFileChecksum)) throw new InvalidDataException($"Package checksum mismatch ({BitConverter.ToString(hash)} != expected {BitConverter.ToString(WholeFileChecksum)})");

                    r.Position(header.HeaderSize);
                    hash = md5.ComputeHash(r.ReadBytes((int)header.TreeSize));
                    if (!hash.SequenceEqual(TreeChecksum)) throw new InvalidDataException($"File tree checksum mismatch ({BitConverter.ToString(hash)} != expected {BitConverter.ToString(TreeChecksum)})");

                    r.Position(header.HeaderSize + header.TreeSize + header.FileDataSectionSize);
                    hash = md5.ComputeHash(r.ReadBytes((int)header.ArchiveMd5SectionSize));
                    if (!hash.SequenceEqual(ArchiveMd5EntriesChecksum)) throw new InvalidDataException($"Archive MD5 entries checksum mismatch ({BitConverter.ToString(hash)} != expected {BitConverter.ToString(ArchiveMd5EntriesChecksum)})");
                }
                if (PublicKey == null || Signature == null) return;
                if (!IsSignatureValid(r, header)) throw new InvalidDataException("VPK signature is not valid.");
            }

            /// <summary>
            /// Verifies the RSA signature.
            /// </summary>
            /// <returns>True if signature is valid, false otherwise.</returns>
            public bool IsSignatureValid(BinaryReader r, Header header)
            {
                r.Position(0);
                var keyParser = new AsnKeyParser(PublicKey);
                var rsa = RSA.Create();
                rsa.ImportParameters(keyParser.ParseRSAPublicKey());
                var data = r.ReadBytes((int)(header.HeaderSize + header.TreeSize + header.FileDataSectionSize + header.ArchiveMd5SectionSize + header.OtherMd5SectionSize));
                return rsa.VerifyData(data, Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }

        #endregion

        public unsafe override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            // header
            Header header;
            if (r.ReadUInt32() != MAGIC) throw new InvalidDataException("Given file is not a VPK.");
            var version = r.ReadUInt32();
            if (version == 1)
                header = new Header
                {
                    Version = version,
                    TreeSize = r.ReadUInt32(),
                    HeaderSize = (uint)r.Position(),
                };
            else if (version == 2)
                header = new Header
                {
                    Version = version,
                    TreeSize = r.ReadUInt32(),
                    FileDataSectionSize = r.ReadUInt32(),
                    ArchiveMd5SectionSize = r.ReadUInt32(),
                    OtherMd5SectionSize = r.ReadUInt32(),
                    SignatureSectionSize = r.ReadUInt32(),
                    HeaderSize = (uint)r.Position(),
                };
            else throw new InvalidDataException($"Bad VPK version. ({version})");

            // sourceFilePath
            var files = multiSource.Files = new List<FileMetadata>();
            var sourceFilePath = source.FilePath;
            var sourceFileDirVpk = sourceFilePath.EndsWith("_dir.vpk", StringComparison.OrdinalIgnoreCase);
            if (sourceFileDirVpk) sourceFilePath = sourceFilePath.Substring(0, sourceFilePath.Length - 8);

            source.FileMask = path =>
            {
                var extension = Path.GetExtension(path);
                if (extension.EndsWith("_c", StringComparison.Ordinal)) extension = extension.Substring(0, extension.Length - 2);
                if (extension.StartsWith(".v")) extension = extension.Remove(1, 1);
                return $"{Path.GetFileNameWithoutExtension(path)}{extension}";
            };

            // types
            while (true)
            {
                var typeName = r.ReadZUTF8();
                if (typeName?.Length == 0) break;
                // directories
                while (true)
                {
                    var directoryName = r.ReadZUTF8();
                    if (directoryName?.Length == 0) break;
                    // files
                    while (true)
                    {
                        var fileName = r.ReadZUTF8();
                        if (fileName?.Length == 0) break;

                        var metadata = new FileMetadata
                        {
                            Path = $"{(directoryName[0] != ' ' ? $"{directoryName}/" : null)}{fileName}.{typeName}",
                            Digest = r.ReadUInt32(),
                            Extra = new byte[r.ReadUInt16()],
                            Id = r.ReadUInt16(),
                            Position = r.ReadUInt32(),
                            FileSize = r.ReadUInt32(),
                        };
                        if (metadata.Id != 0x7FFF)
                        {
                            if (!sourceFileDirVpk) throw new InvalidOperationException("Given VPK is not a _dir, but entry is referencing an external archive.");
                            metadata.Tag = $"{sourceFilePath}_{metadata.Id:D3}.vpk";
                        }
                        else metadata.Tag = (long)(header.HeaderSize + header.TreeSize);
                        files.Add(metadata);
                        if (r.ReadUInt16() != 0xFFFF) throw new FormatException("Invalid terminator.");
                        if (metadata.Extra.Length > 0) r.Read(metadata.Extra, 0, metadata.Extra.Length);
                    }
                }
            }

            // verification
            if (version == 2)
            {
                // Skip over file data, if any
                r.Skip(header.FileDataSectionSize);
                var verification = new Verification { };
                // archive md5
                if (header.ArchiveMd5SectionSize != 0)
                {
                    verification.ArchiveMd5Entries = new List<ArchiveMd5Entry>();
                    var entries = header.ArchiveMd5SectionSize / 28; // 28 is sizeof(VPK_MD5SectionEntry), which is int + int + int + 16 chars
                    for (var i = 0; i < entries; i++)
                        verification.ArchiveMd5Entries.Add(new ArchiveMd5Entry
                        {
                            ArchiveIndex = r.ReadUInt32(),
                            Offset = r.ReadUInt32(),
                            Length = r.ReadUInt32(),
                            Checksum = r.ReadBytes(16)
                        });
                }
                // other md5
                if (header.OtherMd5SectionSize != 48) throw new InvalidDataException($"Encountered OtherMD5Section with size of {header.OtherMd5SectionSize} (should be 48)");
                verification.TreeChecksum = r.ReadBytes(16);
                verification.ArchiveMd5EntriesChecksum = r.ReadBytes(16);
                verification.WholeFileChecksum = r.ReadBytes(16);
                // signature
                if (header.SignatureSectionSize != 0) { verification.PublicKey = r.ReadBytes(r.ReadInt32()); verification.Signature = r.ReadBytes(r.ReadInt32()); }
                source.Tag = verification;
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            var data = new byte[file.Extra.Length + file.FileSize];
            if (file.Extra.Length > 0) file.Extra.CopyTo(data, 0);
            if (file.FileSize > 0)
            {
                if (file.Tag is string path)
                    source.GetBinaryReader(path).Action(r2 =>
                    {
                        r2.Position(file.Position);
                        r2.Read(data, file.Extra.Length, (int)file.FileSize);
                    });
                else
                {
                    r.Position(file.Position + (long)file.Tag);
                    r.Read(data, file.Extra.Length, (int)file.FileSize);
                }
            }
            if (file.Digest != Crc32Digest.Compute(data)) throw new InvalidDataException("CRC32 mismatch for read data.");
            return Task.FromResult((Stream)new MemoryStream(data));
        }
    }
}