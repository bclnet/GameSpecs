using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Lucas.Formats
{
    public unsafe class PakBinary_Scumm : PakBinary<PakBinary_Scumm>
    {
        [Flags]
        public enum Features
        {
            None,
            SixteenColors = 0x01,
            Old256 = 0x02,
            FewLocals = 0x04,
            Demo = 0x08,
            Is16BitColor = 0x10,
            AudioTracks = 0x20,
        }

        public enum Platform
        {
            None,
            Apple2GS,
            C64,
            Amiga,
            AtariST,
            SegaCD,
            Macintosh,
            FMTowns,
        }

        public class ArrayDefinition
        {
            public uint Index;
            public int Type;
            public int Dim1;
            public int Dim2;
        }

        public class ResourceIndex
        {
            public const ushort CLASSIC_MAGIC = 0x0A31;
            public const ushort ENHANCE_MAGIC = 0x0100;

            // objects
            public Dictionary<string, int> ObjectIDMap; // Version8
            public byte[] ObjectOwnerTable;
            public byte[] ObjectStateTable;
            public uint[] ClassData;

            // resources
            public Dictionary<byte, string> RoomNames;
            public (byte, long)[] RoomResources;
            public (byte, long)[] ScriptResources;
            public (byte, long)[] SoundResources;
            public (byte, long)[] CostumeResources;
            public (byte, long)[] CharsetResources;
            public (byte, long)[] RoomScriptResources; // Version8

            // max sizes
            public List<ArrayDefinition> ArrayDefinitions = new List<ArrayDefinition>();
            public int NumVerbs = 100;
            public int NumInventory = 80;
            public int NumVariables = 800;
            public int NumBitVariables = 4096;
            public int NumLocalObjects = 200;
            public int NumArray = 50;
            public int NumGlobalScripts = 200;
            public byte[] ObjectRoomTable;
            public string[] AudioNames = new string[0];

            public ResourceIndex(BinaryReader r, FamilyGame game, Dictionary<string, object> detect)
            {
                var features = ((Features)detect["features"]);
                var variants = detect["variant"];
                var version = (int)((FamilyGame.Edition)detect["edition"]).Data["version"];
                var oldBundle = version <= 3 && features.HasFlag(Features.SixteenColors);
                switch (version)
                {
                    case 0: Load0(game, r, detect); break;
                    case 1: if ((Platform)detect["platform"] == Platform.C64) Load0(game, r, detect); else Load2(game, r, detect); break;
                    case 2: Load2(game, r, detect); break;
                    case 3: if (oldBundle) Load3_16(game, r, detect); else Load3(game, r, detect, features.HasFlag(Features.Old256) ? (byte)0 : (byte)0xff); break;
                    case 4: Load4(game, r, detect); break;
                    case 5: Load5(game, r, detect); break;
                    case 6: Load6(game, r, detect); break;
                    case 7: Load7(game, r, detect); break;
                    case 8: Load8(game, r, detect); break;
                    default: throw new NotSupportedException($"Version {0} is not supported.");
                };
            }

            #region Reads

            [Flags]
            enum ObjectFlags
            {
                CountX = 0b00,
                CountS = 0b01,
                CountI = 0b10,
                CountMask = 0b11,
                Loop1 = 0b00100,
                Loop2 = 0b01000,
                Loop3 = 0b01100,
                Loop4 = 0b10000,
                Loop5 = 0b10100,
                LoopMask = 0b11100,
                // combined
                CXL1 = CountX | Loop1,
                CSL1 = CountS | Loop1,
                CSL2 = CountS | Loop2,
                CSL3 = CountS | Loop3,
                CSL4 = CountS | Loop4,
                CIL5 = CountS | Loop5,
            }

            [Flags]
            enum ResourceFlags
            {
                CountX = 0b00,
                CountB = 0b01,
                CountS = 0b10,
                CountMask = 0b11,
                Loop1 = 0b00100,
                Loop2 = 0b01000,
                Loop3 = 0b01100,
                Loop4 = 0b10000,
                LoopMask = 0b11100,
                // combined
                CXL1 = CountX | Loop1,
                CXL2 = CountX | Loop2,
                CBL1 = CountB | Loop1,
                CBL2 = CountB | Loop2,
                CSL3 = CountB | Loop3,
                CSL4 = CountB | Loop4,
            }

            static uint ToOffset(ushort offset) => offset == 0xFFFF ? 0xFFFFFFFF : offset;

            void ReadObjects(BinaryReader r, ObjectFlags flags, int count = 0)
            {
                count = (flags & ObjectFlags.CountMask) switch
                {
                    ObjectFlags.CountX => count,
                    ObjectFlags.CountS => r.ReadUInt16(),
                    ObjectFlags.CountI => r.ReadInt32(),
                    _ => throw new NotSupportedException(),
                };
                switch (flags & ObjectFlags.LoopMask)
                {
                    case ObjectFlags.Loop1:
                        ObjectOwnerTable = new byte[count];
                        ObjectStateTable = new byte[count];
                        ClassData = new uint[count];
                        for (var i = 0; i < count; i++)
                        {
                            var tmp = r.ReadByte();
                            ObjectStateTable[i] = (byte)(tmp >> 4);
                            ObjectOwnerTable[i] = (byte)(tmp & 0x0F);
                        }
                        break;
                    case ObjectFlags.Loop2:
                        ObjectOwnerTable = new byte[count];
                        ObjectStateTable = new byte[count];
                        ClassData = new uint[count];
                        for (var i = 0; i < count; i++)
                        {
                            ClassData[i] = r.ReadByte() | (uint)(r.ReadByte() << 8) | (uint)(r.ReadByte() << 16);
                            var tmp = r.ReadByte();
                            ObjectStateTable[i] = (byte)(tmp >> 4);
                            ObjectOwnerTable[i] = (byte)(tmp & 0x0F);
                        }
                        break;
                    case ObjectFlags.Loop3:
                        ObjectOwnerTable = new byte[count];
                        ObjectStateTable = new byte[count];
                        for (var i = 0; i < count; i++)
                        {
                            var tmp = r.ReadByte();
                            ObjectStateTable[i] = (byte)(tmp >> 4);
                            ObjectOwnerTable[i] = (byte)(tmp & 0x0F);
                        }
                        ClassData = r.ReadTArray<uint>(sizeof(uint), count);
                        break;
                    case ObjectFlags.Loop4:
                        ObjectStateTable = r.ReadBytes(count);
                        ObjectRoomTable = r.ReadBytes(count);
                        ObjectOwnerTable = new byte[count];
                        for (var i = 0; i < count; i++)
                            ObjectOwnerTable[i] = 0xFF;
                        ClassData = r.ReadTArray<uint>(sizeof(uint), count);
                        break;
                    case ObjectFlags.Loop5:
                        ObjectIDMap = new Dictionary<string, int>();
                        ObjectStateTable = new byte[count];
                        ObjectRoomTable = new byte[count];
                        ObjectOwnerTable = new byte[count];
                        ClassData = new uint[count];
                        for (var i = 0; i < count; i++)
                        {
                            var name = r.ReadCString(40, Encoding.UTF8);
                            ObjectIDMap[name] = i;
                            ObjectStateTable[i] = r.ReadByte();
                            ObjectRoomTable[i] = r.ReadByte();
                            ClassData[i] = r.ReadUInt32();
                            ObjectOwnerTable[i] = 0xFF;
                        }
                        break;
                }
            }

            static (byte, long)[] ReadResources(BinaryReader r, ResourceFlags flags, int count = 0)
            {
                count = (flags & ResourceFlags.CountMask) switch
                {
                    ResourceFlags.CountX => count,
                    ResourceFlags.CountB => r.ReadByte(),
                    ResourceFlags.CountS => r.ReadUInt16(),
                    _ => throw new NotSupportedException(),
                };
                var res = new (byte, long)[count];
                var rooms = r.ReadBytes(count);
                switch (flags & ResourceFlags.LoopMask)
                {
                    case ResourceFlags.Loop1: for (var i = 0; i < count; i++) res[i] = ((byte)i, ToOffset(r.ReadUInt16())); break;
                    case ResourceFlags.Loop2: for (var i = 0; i < count; i++) res[i] = (rooms[i], ToOffset(r.ReadUInt16())); break;
                    case ResourceFlags.Loop3: for (var i = 0; i < count; i++) res[i] = (r.ReadByte(), r.ReadUInt32()); break;
                    case ResourceFlags.Loop4: for (var i = 0; i < count; i++) res[i] = (rooms[i], r.ReadUInt32()); break;
                }
                return res;
            }

            static string[] ReadNames(BinaryReader r)
            {
                var values = new string[r.ReadUInt16()];
                for (var i = 0; i < values.Length; i++)
                    values[i] = r.ReadCString(9, Encoding.UTF8);
                return values;
            }

            static Dictionary<byte, string> ReadRoomNames(BinaryReader r)
            {
                var values = new Dictionary<byte, string>();
                for (byte room; (room = r.ReadByte()) != 0;)
                {
                    var name = r.ReadBytes(9);
                    var b = new StringBuilder();
                    for (var i = 0; i < 9; i++)
                    {
                        var c = name[i] ^ 0xFF;
                        if (c == 0) continue;
                        b.Append((char)c);
                    }
                    values[room] = b.ToString();
                }
                return values;
            }

            void ReadMaxSizes(BinaryReader r, FamilyGame game, Features features, int ver)
            {
                switch (ver)
                {
                    case 5:
                        {
                            NumVariables = r.ReadUInt16();      // 800
                            r.ReadUInt16();                     // 16
                            NumBitVariables = r.ReadUInt16();   // 2048
                            NumLocalObjects = r.ReadUInt16();   // 200
                            r.ReadUInt16();                     // 50
                            var numCharsets = r.ReadUInt16();   // 9
                            r.ReadUInt16();                     // 100
                            r.ReadUInt16();                     // 50
                            NumInventory = r.ReadUInt16();      // 80
                            break;
                        }
                    case 6:
                        {
                            NumVariables = r.ReadUInt16();      // 800
                            r.ReadUInt16();                     // 16
                            NumBitVariables = r.ReadUInt16();   // 2048
                            NumLocalObjects = r.ReadUInt16();   // 200
                            NumArray = r.ReadUInt16();          // 50
                            r.ReadUInt16();
                            NumVerbs = r.ReadUInt16();          // 100
                            var numFlObject = r.ReadUInt16();   // 50
                            NumInventory = r.ReadUInt16();      // 80
                            var numRooms = r.ReadUInt16();
                            var numScripts = r.ReadUInt16();
                            var numSounds = r.ReadUInt16();
                            var numCharsets = r.ReadUInt16();
                            var numCostumes = r.ReadUInt16();
                            var numGlobalObjects = r.ReadUInt16();
                            break;
                        }
                    case 7:
                        {
                            r.Skip(50); // Skip over SCUMM engine version
                            r.Skip(50); // Skip over data file version
                            NumVariables = r.ReadUInt16();
                            NumBitVariables = r.ReadUInt16();
                            r.ReadUInt16();
                            var numGlobalObjects = r.ReadUInt16();
                            NumLocalObjects = r.ReadUInt16();
                            var numNewNames = r.ReadUInt16();
                            NumVerbs = r.ReadUInt16();
                            var numFlObject = r.ReadUInt16();
                            NumInventory = r.ReadUInt16();
                            NumArray = r.ReadUInt16();
                            var numRooms = r.ReadUInt16();
                            var numScripts = r.ReadUInt16();
                            var numSounds = r.ReadUInt16();
                            var numCharsets = r.ReadUInt16();
                            var numCostumes = r.ReadUInt16();
                            NumGlobalScripts = game.Id == "FT" && features.HasFlag(Features.Demo) ? 300 : 2000;
                            break;
                        }
                    case 8:
                        {
                            r.Skip(50); // Skip over SCUMM engine version
                            r.Skip(50); // Skip over data file version
                            NumVariables = r.ReadInt32();
                            NumBitVariables = r.ReadInt32();
                            r.ReadInt32();
                            var numScripts = r.ReadInt32();
                            var numSounds = r.ReadInt32();
                            var numCharsets = r.ReadInt32();
                            var numCostumes = r.ReadInt32();
                            var numRooms = r.ReadInt32();
                            r.ReadInt32();
                            var numGlobalObjects = r.ReadInt32();
                            r.ReadInt32();
                            NumLocalObjects = r.ReadInt32();
                            var numNewNames = r.ReadInt32();
                            var numFlObject = r.ReadInt32();
                            NumInventory = r.ReadInt32();
                            NumArray = r.ReadInt32();
                            NumVerbs = r.ReadInt32();
                            NumGlobalScripts = 2000;
                            break;
                        }
                }
            }

            static List<ArrayDefinition> ReadIndexFile(BinaryReader r)
            {
                var values = new List<ArrayDefinition>();
                uint num;
                while ((num = r.ReadUInt16()) != 0)
                {
                    var a = r.ReadUInt16();
                    var b = r.ReadUInt16();
                    var c = r.ReadUInt16();
                    values.Add(new ArrayDefinition { Index = num, Type = c, Dim2 = a, Dim1 = b });
                }
                return values;
            }

            //$"DIRN: 0x{BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DIRN")):X}".Dump();

            #endregion

            #region Loads

            byte[] V0_roomDisks;
            byte[] V0_roomTracks;
            byte[] V0_roomSectors;

            void Load0(FamilyGame game, BinaryReader r, Dictionary<string, object> detect)
            {
                V0_roomDisks = new byte[59];
                V0_roomTracks = new byte[59];
                V0_roomSectors = new byte[59];
                // determine counts
                int numGlobalObjects, numRooms, numCostumes, numScripts, numSounds;
                if (game.Id == "MM") // Maniac Mansion
                {
                    numGlobalObjects = 256; numRooms = 55; numCostumes = 25;
                    if (((Features)detect["features"]).HasFlag(Features.Demo)) { numScripts = 55; numSounds = 40; }
                    else { numScripts = 160; numSounds = 70; }
                }
                else { numGlobalObjects = 775; numRooms = 59; numCostumes = 38; numScripts = 155; numSounds = 127; }

                // skip
                if ((Platform)detect["platform"] == Platform.Apple2GS) r.Seek(142080);

                // read magic
                var magic = r.ReadUInt16();
                if (magic != CLASSIC_MAGIC) throw new FormatException("BAD MAGIC");

                // object flags
                ReadObjects(r, ObjectFlags.CXL1, numGlobalObjects);

                // room offsets
                for (var i = 0; i < numRooms; i++) V0_roomDisks[i] = (byte)(r.ReadByte() - '0');
                for (var i = 0; i < numRooms; i++)
                {
                    V0_roomSectors[i] = r.ReadByte();
                    V0_roomTracks[i] = r.ReadByte();
                }
                CostumeResources = ReadResources(r, ResourceFlags.CXL2, numCostumes);
                ScriptResources = ReadResources(r, ResourceFlags.CXL2, numScripts);
                SoundResources = ReadResources(r, ResourceFlags.CXL2, numSounds);
            }

            void Load2(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect)
            {
                var r = new BinaryReader(new ByteXorStream(r_.BaseStream, 0xff));
                var magic = r.ReadUInt16();
                switch (magic)
                {
                    case CLASSIC_MAGIC:
                        {
                            int numGlobalObjects, numRooms, numCostumes, numScripts, numSounds;
                            if (game.Id == "MM") { numGlobalObjects = 800; numRooms = 55; numCostumes = 35; numScripts = 200; numSounds = 100; }
                            else if (game.Id == "ZMatAM") { numGlobalObjects = 775; numRooms = 61; numCostumes = 37; numScripts = 155; numSounds = 120; }
                            else throw new NotSupportedException($"Version2 for {game.Id} is not supported.");
                            ReadObjects(r, ObjectFlags.CXL1, numGlobalObjects);
                            RoomResources = ReadResources(r, ResourceFlags.CXL1, numRooms);
                            CostumeResources = ReadResources(r, ResourceFlags.CXL2, numCostumes);
                            ScriptResources = ReadResources(r, ResourceFlags.CXL2, numScripts);
                            SoundResources = ReadResources(r, ResourceFlags.CXL2, numSounds);
                            return;
                        }
                    case ENHANCE_MAGIC:
                        {
                            ReadObjects(r, ObjectFlags.CSL1);
                            RoomResources = ReadResources(r, ResourceFlags.CBL1);
                            CostumeResources = ReadResources(r, ResourceFlags.CBL2);
                            ScriptResources = ReadResources(r, ResourceFlags.CBL2);
                            SoundResources = ReadResources(r, ResourceFlags.CBL2);
                            return;
                        }
                    default: throw new FormatException("BAD MAGIC");
                }
            }

            void Load3_16(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect)
            {
                var r = new BinaryReader(new ByteXorStream(r_.BaseStream, 0xff));
                var magic = r.ReadUInt16();
                if (magic != ENHANCE_MAGIC) throw new FormatException("BAD MAGIC");
                ReadObjects(r, ObjectFlags.CSL2);
                RoomResources = ReadResources(r, ResourceFlags.CBL1);
                CostumeResources = ReadResources(r, ResourceFlags.CBL2);
                ScriptResources = ReadResources(r, ResourceFlags.CBL2);
                SoundResources = ReadResources(r, ResourceFlags.CBL2);
            }

            void Load3(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect, byte xorByte)
            {
                var indy3FmTowns = game.Id == "IJatLC" && (Platform)detect["platform"] == Platform.FMTowns;
                var r = xorByte != 0 ? new BinaryReader(new ByteXorStream(r_.BaseStream, xorByte)) : r_;
                while (r.BaseStream.Position < r.BaseStream.Length)
                {
                    r.ReadUInt32();
                    var block = r.ReadInt16();
                    switch (block)
                    {
                        case 0x4E52: RoomNames = ReadRoomNames(r); break;
                        case 0x5230: RoomResources = ReadResources(r, ResourceFlags.CSL3); break; // 'R0'
                        case 0x5330: ScriptResources = ReadResources(r, ResourceFlags.CSL3); break; // 'S0'
                        case 0x4E30: SoundResources = ReadResources(r, ResourceFlags.CSL3); break; // 'N0'
                        case 0x4330: CostumeResources = ReadResources(r, ResourceFlags.CSL3); break; // 'C0'
                        case 0x4F30: ReadObjects(r, ObjectFlags.CSL2); if (indy3FmTowns) r.Skip(32); break;// 'O0' - Indy3 FM-TOWNS has 32 extra bytes
                        default: Log($"Unknown block {block:X2}"); break;
                    }
                }
            }

            void Load4(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect) => Load3(game, r_, detect, 0);

            void Load5(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect)
            {
                var features = (Features)detect["features"];
                var r = new BinaryReader(new ByteXorStream(r_.BaseStream, 0x69));
                while (r.BaseStream.Position < r.BaseStream.Length)
                {
                    var block = r.ReadUInt32();
                    r.ReadUInt32E(); // size
                    switch (block)
                    {
                        case 0x4D414E52: RoomNames = ReadRoomNames(r); break; // 'RNAM'
                        case 0x5358414D: ReadMaxSizes(r, game, features, 5); break; // 'MAXS'
                        case 0x4F4F5244: RoomResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DROO'
                        case 0x52435344: ScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSCR'
                        case 0x554F5344: SoundResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSOU'
                        case 0x534F4344: CostumeResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCOS'
                        case 0x52484344: CharsetResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCHR'
                        case 0x4A424F44: ReadObjects(r, ObjectFlags.CSL3); break; // 'DOBJ'
                        default: Log($"Unknown block {block:X2}"); break;
                    }
                }
            }

            void Load6(FamilyGame game, BinaryReader r_, Dictionary<string, object> detect)
            {
                var features = (Features)detect["features"];
                var r = new BinaryReader(new ByteXorStream(r_.BaseStream, 0x69));
                while (r.BaseStream.Position < r.BaseStream.Length)
                {
                    var block = r.ReadUInt32();
                    r.ReadUInt32E(); // size
                    switch (block)
                    {
                        case 0x52484344: case 0x46524944: CharsetResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCHR'/'DIRF'
                        case 0x4A424F44: ReadObjects(r, ObjectFlags.CSL3); break; // 'DOBJ'
                        case 0x4D414E52: RoomNames = ReadRoomNames(r); break; // 'RNAM'
                        case 0x4F4F5244: case 0x52524944: RoomResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DROO'/'DIRR'
                        case 0x52435344: case 0x53524944: ScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSCR'/'DIRS'
                        case 0x534F4344: case 0x43524944: CostumeResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCOS'/'DIRC'
                        case 0x5358414D: ReadMaxSizes(r, game, features, 6); break; // 'MAXS'
                        case 0x554F5344: case 0x4E524944: SoundResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSOU'/'DIRN'
                        case 0x59524141: ArrayDefinitions = ReadIndexFile(r); break; // 'AARY'
                        default: Log($"Unknown block {block:X2}"); break;
                    }
                }
            }

            void Load7(FamilyGame game, BinaryReader r, Dictionary<string, object> detect)
            {
                var features = (Features)detect["features"];
                while (r.BaseStream.Position < r.BaseStream.Length)
                {
                    var block = r.ReadUInt32();
                    r.ReadUInt32E(); // size
                    switch (block)
                    {
                        case 0x52484344: case 0x46524944: CharsetResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCHR'/'DIRF'
                        case 0x4A424F44: ReadObjects(r, ObjectFlags.CSL4); break; // 'DOBJ'
                        case 0x4D414E52: RoomNames = ReadRoomNames(r); break; // 'RNAM'
                        case 0x4F4F5244: case 0x52524944: RoomResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DROO'/'DIRR'
                        case 0x52435344: case 0x53524944: ScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSCR'/'DIRS'
                        case 0x534F4344: case 0x43524944: CostumeResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCOS'/'DIRC'
                        case 0x5358414D: ReadMaxSizes(r, game, features, 7); break; // 'MAXS'
                        case 0x554F5344: case 0x4E524944: SoundResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSOU'/'DIRN'
                        case 0x59524141: ArrayDefinitions = ReadIndexFile(r); break; // 'AARY'
                        case 0x4D414E41: AudioNames = ReadNames(r); break; // 'ANAM' - Used by: The Dig, FT
                        default: Log($"Unknown block {block:X2}"); break;
                    }
                }
            }

            void Load8(FamilyGame game, BinaryReader r, Dictionary<string, object> detect)
            {
                var features = (Features)detect["features"];
                while (r.BaseStream.Position < r.BaseStream.Length)
                {
                    var block = r.ReadUInt32();
                    r.ReadUInt32E(); // size
                    switch (block)
                    {
                        case 0x52484344: case 0x46524944: CharsetResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCHR'/'DIRF'
                        case 0x4A424F44: ReadObjects(r, ObjectFlags.CIL5); break; // 'DOBJ'
                        case 0x4D414E52: RoomNames = ReadRoomNames(r); break; // 'RNAM'
                        case 0x4F4F5244: case 0x52524944: RoomResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DROO'/'DIRR'
                        case 0x52435344: case 0x53524944: ScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSCR'/'DIRS'
                        case 0x43535244: RoomScriptResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DRSC''
                        case 0x534F4344: case 0x43524944: CostumeResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DCOS'/'DIRC'
                        case 0x5358414D: ReadMaxSizes(r, game, features, 8); break; // 'MAXS'
                        case 0x554F5344: case 0x4E524944: SoundResources = ReadResources(r, ResourceFlags.CSL4); break; // 'DSOU'/'DIRN'
                        case 0x59524141: ArrayDefinitions = ReadIndexFile(r); break; // 'AARY'
                        case 0x4D414E41: AudioNames = ReadNames(r); break; // 'ANAM' - Used by: The Dig, FT
                        default: Log($"Unknown block {block:X2}"); break;
                    }
                }
            }

            #endregion
        }

        public class ResourceFile
        {
        }

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var game = source.Game;
            var detect = source.Game.Detect<Dictionary<string, object>>("scumm", source.PakPath, r, s =>
            {
                s["features"] = ((string)s["features"]).Split(' ').Aggregate(Features.None, (a, f) => a |= (Features)Enum.Parse(typeof(Features), f, true));
                s["platform"] = (Platform)Enum.Parse(typeof(Platform), s.TryGetValue("platform", out var z) ? (string)z : "None", true);
                return s;
            }) ?? throw new FormatException("No Detect");
            // get index
            ResourceIndex index = new ResourceIndex(r, game, detect);

            // add files
            var files = new List<FileSource>(); source.Files = files;

            if (index.RoomResources != null) files.AddRange(index.RoomResources
                .Select((s, i) => new FileSource { Path = $"rooms/room{i:00}.dat", FileSize = s.Item1, Offset = s.Item2 }));
            if (index.ScriptResources != null) files.AddRange(index.ScriptResources
                .Select((s, i) => new FileSource { Path = $"scripts/script{i:000}.dat", FileSize = s.Item1, Offset = s.Item2 }));
            if (index.SoundResources != null) files.AddRange(index.SoundResources
                .Select((s, i) => new FileSource { Path = $"sounds/sound{i:000}.dat", FileSize = s.Item1, Offset = s.Item2 }));
            if (index.CostumeResources != null) files.AddRange(index.CostumeResources
                .Select((s, i) => new FileSource { Path = $"costumes/costume{i:00}.dat", FileSize = s.Item1, Offset = s.Item2 }));
            if (index.CharsetResources != null) files.AddRange(index.CharsetResources
                .Select((s, i) => new FileSource { Path = $"charsets/charset{i:002}.dat", FileSize = s.Item1, Offset = s.Item2 }));
            if (index.RoomScriptResources != null) files.AddRange(index.RoomScriptResources
                .Select((s, i) => new FileSource { Path = $"scripts/roomScript{i:00}.dat", FileSize = s.Item1, Offset = s.Item2 }));
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            throw new NotImplementedException();
        }
    }
}