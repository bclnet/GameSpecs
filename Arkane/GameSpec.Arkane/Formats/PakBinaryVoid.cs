using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpec.Arkane.Formats
{
    public unsafe class PakBinaryVoid : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryVoid();
        const uint RES_MAGIC = 0x04534552;

        class SubPakFile : BinaryPakManyFile
        {
            public SubPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, Instance, tag) => Open();
        }

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (Path.GetExtension(source.FilePath) != ".index") throw new FormatException("must be a .index file");
            var files2 = multiSource.Files = new List<FileMetadata>();

            // index games
            if (Path.GetFileName(source.FilePath) == "master.index")
            {
                const uint SubMarker = 0x18000000;
                const uint EndMarker = 0x01000000;

                var magic = MathX.Reverse(r.ReadUInt32());
                if (magic != RES_MAGIC) throw new FormatException("BAD MAGIC");
                r.Skip(4);
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
                        Pak = new SubPakFile(source.Game, source.FileSystem, path),
                    });
                }
                while (true);
                return Task.CompletedTask;
            }

            var pathFile = Path.GetFileName(source.FilePath);
            var pathDir = Path.GetDirectoryName(source.FilePath);
            var resourcePath = Path.Combine(pathDir, $"{pathFile[0..^6]}.resources");
            if (!File.Exists(resourcePath)) throw new FormatException("Unable to find resources extension");
            var sharedResourcePath = new[] {
                "shared_2_3.sharedrsc",
                "shared_2_3_4.sharedrsc",
                "shared_1_2_3.sharedrsc",
                "shared_1_2_3_4.sharedrsc" }
                .Select(x => Path.Combine(pathDir, x)).FirstOrDefault(File.Exists);

            r.Seek(4);
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
                if (useSharedResources && sharedResourcePath == null) throw new FormatException("sharedResourcePath not available");
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
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            if (file.FileSize == 0 || _badPositions.Contains(file.Position)) return Task.FromResult(System.IO.Stream.Null);
            var (path, tag1, tag2) = ((string, string, string))file.Tag;
            return Task.FromResult((Stream)new MemoryStream(source.GetBinaryReader(path).Func(r2 =>
            {
                r2.Seek(file.Position);
                return file.Compressed != 0
                    ? r2.DecompressZlib((int)file.PackedSize, (int)file.FileSize)
                    : r2.ReadBytes((int)file.PackedSize);
            })));
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
    }
}