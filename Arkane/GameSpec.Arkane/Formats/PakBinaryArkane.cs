using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSpec.Arkane.Formats
{
    public unsafe class PakBinaryArkane : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryArkane();
        const uint RES_MAGIC = 0x04534552;

        class SubPakFile : BinaryPakManyFile
        {
            public SubPakFile(Family estate, string game, string filePath, object tag = null) : base(estate, game, filePath, Instance, tag) => Open();
        }

        enum Magic
        {
            PAK,
            INDEX,
        }

        PakBinaryArkane() { }

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            var (gameId, game) = source.Family.GetGame(source.Game);
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());
            var extention = Path.GetExtension(source.FilePath);
            switch (extention)
            {
                case ".pak":
                    {
                        source.Magic = (int)Magic.PAK;
                        var files = multiSource.Files = new List<FileMetadata>();
                        var key = game.Key is Family.ByteKey z ? z.Key : null;
                        int keyLength = key.Length, keyIndex = 0;

                        int readFatInteger(ref byte* b)
                        {
                            var p = b;
                            *(p + 0) = (byte)(*(p + 0) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                            *(p + 1) = (byte)(*(p + 1) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                            *(p + 2) = (byte)(*(p + 2) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                            *(p + 3) = (byte)(*(p + 3) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                            var r = *(int*)p;
                            b += 4;
                            return r;
                        }

                        string readFatString(ref byte* b)
                        {
                            var p = b;
                            while (true)
                            {
                                *p = (byte)(*p ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                                if (*p == 0) break;
                                p++;
                            }
                            var length = (int)(p - b);
                            var r = Encoding.ASCII.GetString(new ReadOnlySpan<byte>(b, length));
                            b = p + 1;
                            return r;
                        }

                        // move to fat table
                        r.Seek(r.ReadUInt32());
                        var fatSize = (int)r.ReadUInt32();
                        var fatBytes = r.ReadBytes(fatSize);

                        fixed (byte* _ = fatBytes)
                        {
                            byte* c = _, end = _ + fatSize;
                            while (c < end)
                            {
                                var dirPath = readFatString(ref c);
                                var numFiles = readFatInteger(ref c);
                                while (numFiles-- != 0)
                                    files.Add(new FileMetadata
                                    {
                                        Path = dirPath + readFatString(ref c),
                                        Position = readFatInteger(ref c),
                                        Compressed = readFatInteger(ref c),
                                        FileSize = readFatInteger(ref c),
                                        PackedSize = readFatInteger(ref c),
                                    });
                            }
                        }
                        return Task.CompletedTask;
                    }
                // index games
                case ".index":
                    {
                        source.Magic = (int)Magic.INDEX;
                        //if (Path.GetExtension(source.FilePath) != ".index") throw new ArgumentOutOfRangeException("must be index");
                        if (Path.GetFileName(source.FilePath) == "master.index")
                        {
                            const uint SubMarker = 0x18000000;
                            const uint EndMarker = 0x01000000;

                            var magic = MathX.Reverse(r.ReadUInt32());
                            if (magic != RES_MAGIC) throw new FormatException("BAD MAGIC");
                            r.Skip(4);
                            var files2 = multiSource.Files = new List<FileMetadata>();
                            var state = 0;
                            do
                            {
                                var nameSize = r.ReadUInt32();
                                if (nameSize == SubMarker) { state++; nameSize = r.ReadUInt32(); }
                                else if (nameSize == EndMarker) break;
                                var path = r.ReadFString((int)nameSize).Replace('\\', '/');
                                var packId = state > 0 ? r.ReadUInt16() : 0;
                                files2.Add(new FileMetadata
                                {
                                    Path = path,
                                    Pak = new SubPakFile(source.Family, source.Game, path),
                                });
                            }
                            while (true);
                            return Task.CompletedTask;
                        }

                        var pathFile = Path.GetFileName(source.FilePath);
                        var pathDir = Path.GetDirectoryName(source.FilePath);
                        var resourcePath = Path.Combine(pathDir, $"{pathFile[0..^6]}.resources");
                        if (!File.Exists(resourcePath))
                            throw new ArgumentOutOfRangeException("Unable to find resources extension");
                        var sharedResourcePath = new[] {
                "shared_2_3.sharedrsc",
                "shared_2_3_4.sharedrsc",
                "shared_1_2_3.sharedrsc",
                "shared_1_2_3_4.sharedrsc" }
                            .Select(x => Path.Combine(pathDir, x)).FirstOrDefault(File.Exists);
                        if (sharedResourcePath == null)
                            throw new ArgumentOutOfRangeException("Unable to find Sharedrsc");

                        r.Position(4);
                        var mainFileSize = MathX.Reverse(r.ReadUInt32()); // mainFileSize
                        r.Skip(24);
                        var numFiles = MathX.Reverse(r.ReadUInt32());
                        var files = multiSource.Files = new FileMetadata[numFiles];
                        for (var i = 0; i < numFiles; i++)
                        {
                            var id = MathX.Reverse(r.ReadUInt32());
                            var tag1 = r.ReadL32Encoding();
                            var tag2 = r.ReadL32Encoding();
                            var path = r.ReadL32Encoding()?.Replace('\\', '/');
                            var position = MathX.Reverse(r.ReadUInt64());
                            var fileSize = MathX.Reverse(r.ReadUInt32());
                            var packedSize = MathX.Reverse(r.ReadUInt32());
                            r.Skip(4);
                            var flags = MathX.Reverse(r.ReadUInt32());
                            var flags2 = MathX.Reverse(r.ReadUInt16());
                            var useSharedResources = (flags & 32) != 0 && flags2 == 0x8000;
                            var newPath = !useSharedResources ? resourcePath : sharedResourcePath;
                            files[i] = new FileMetadata
                            {
                                Id = (int)id,
                                Path = path,
                                Compressed = fileSize != packedSize ? 1 : 0,
                                FileSize = fileSize,
                                PackedSize = packedSize,
                                Position = (long)position,
                                Tag = (newPath, tag1, tag2),
                            };
                        }
                    }
                    return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        // Bad Positions - Dishonored2
        static HashSet<long> _badPositions = new HashSet<long> {
            293, //: generated/decls/renderparm/atm/worldfog/artistscatteringcolor.decl
            917004, //: generated/decls/renderparm/ocean/patchtransform.decl
            9923823, //: generated/decls/soundevent/sound_events/bsp/bsp_physmat/bsp_foosteps/bsp_fs_player/emily/fs_e_metal_chandelier/fs_e_metal_chandelier_w.decl
            9924002, //: generated/decls/fx/contactsystem/w.emily_env.metal.chandelier.fx.decl
            32872162, //: generated/image/models/effects/textures/gameplay/blood/blood_leads_05_fl.bimage7
            32966564, //: generated/decls/material/models/effects/materials/gameplay/blood/blood_leads_05_bf.material.decl
            45704814, //: generated/decls/fx/contactsystem/pr.ar.venom_env.tile.fx.decl
        };

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            switch ((Magic)source.Magic)
            {
                case Magic.PAK:
                    r.Position(file.Position);
                    return Task.FromResult((Stream)new MemoryStream((file.Compressed & 1) != 0
                        ? r.DecompressPkzip((int)file.PackedSize, (int)file.FileSize)
                        : r.ReadBytes((int)file.PackedSize)));
                case Magic.INDEX:
                    if (file.FileSize == 0 || _badPositions.Contains(file.Position)) return Task.FromResult(System.IO.Stream.Null);
                    var (path, tag1, tag2) = ((string, string, string))file.Tag;
                    return Task.FromResult((Stream)new MemoryStream(source.GetBinaryReader(path).Func(r2 =>
                    {
                        r2.Position(file.Position);
                        return file.Compressed != 0
                            ? r2.DecompressZlib((int)file.PackedSize, (int)file.FileSize)
                            : r2.ReadBytes((int)file.PackedSize);
                    })));
                default: throw new NotImplementedException();
            }
        }
    }
}