using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Arkane.Formats
{
    public unsafe class PakBinary_Void : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinary_Void();
        const uint RES_MAGIC = 0x04534552;

        class SubPakFile : BinaryPakFile
        {
            public SubPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, Instance, tag) => Open();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct V_File
        {
            public static string Map = "B8B4B4B4B4B2";
            public ulong Position;
            public uint FileSize;
            public uint PackedSize;
            public uint Unknown1;
            public uint Flags;
            public ushort Flags2;
        }

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            // must be .index file
            if (!source.FilePath.EndsWith(".index")) throw new FormatException("must be a .index file");

            // master.index file
            if (source.FilePath == "master.index")
            {
                const uint SubMarker = 0x18000000;
                const uint EndMarker = 0x01000000;

                var files2 = source.Files = new List<FileSource>();
                var magic = r.ReadUInt32E();
                if (magic != RES_MAGIC) throw new FormatException("BAD MAGIC");
                r.Skip(4);
                var first = true;
                while (true)
                {
                    var pathSize = r.ReadUInt32();
                    if (pathSize == SubMarker) { first = false; pathSize = r.ReadUInt32(); }
                    else if (pathSize == EndMarker) break;
                    var path = r.ReadFString((int)pathSize).Replace('\\', '/');
                    var packId = first ? 0 : r.ReadUInt16();
                    if (!path.EndsWith(".index")) continue;
                    files2.Add(new FileSource
                    {
                        Path = path,
                        Pak = new SubPakFile(source.Game, source.FileSystem, path),
                    });
                }
                return Task.CompletedTask;
            }

            // find files
            var fileSystem = source.FileSystem;
            var resourcePath = $"{source.FilePath[0..^6]}.resources";
            if (!fileSystem.FileExists(resourcePath)) throw new FormatException("Unable to find resources extension");
            var sharedResourcePath = new[] {
                "shared_2_3.sharedrsc",
                "shared_2_3_4.sharedrsc",
                "shared_1_2_3.sharedrsc",
                "shared_1_2_3_4.sharedrsc" }
                .FirstOrDefault(fileSystem.FileExists);

            r.Seek(4);
            var mainFileSize = r.ReadUInt32E();
            r.Skip(24);
            var numFiles = r.ReadUInt32E();
            var files = source.Files = new FileSource[numFiles];
            for (var i = 0; i < numFiles; i++)
            {
                var id = r.ReadUInt32E();
                var tag1 = r.ReadL32Encoding();
                var tag2 = r.ReadL32Encoding();
                var path = (r.ReadL32Encoding() ?? "").Replace('\\', '/');
                var file = r.ReadTE<V_File>(sizeof(V_File), V_File.Map);
                //var position = r.ReadUInt64E();
                //var fileSize = r.ReadUInt32E();
                //var packedSize = r.ReadUInt32E();
                //r.Skip(4);
                //var flags = r.ReadUInt32E();
                //var flags2 = r.ReadUInt16E();
                var useSharedResources = (file.Flags & 0x20) != 0 && file.Flags2 == 0x8000;
                if (useSharedResources && sharedResourcePath == null) throw new FormatException("sharedResourcePath not available");
                var newPath = useSharedResources ? sharedResourcePath : resourcePath;
                files[i] = new FileSource
                {
                    Id = (int)id,
                    Path = path,
                    Compressed = file.FileSize != file.PackedSize ? 1 : 0,
                    FileSize = file.FileSize,
                    PackedSize = file.PackedSize,
                    Position = (long)file.Position,
                    Tag = (newPath, tag1, tag2),
                };
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
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