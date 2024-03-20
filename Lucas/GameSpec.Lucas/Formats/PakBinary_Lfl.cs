using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static GameSpec.Lucas.Formats.PakBinary_Lfl;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

namespace GameSpec.Lucas.Formats
{
    public unsafe class PakBinary_Lfl : PakBinary<PakBinary_Lfl>
    {

        [Flags]
        public enum GameFeatures
        {
            None,
            SixteenColors = 0x01,
            Old256 = 0x02,
            FewLocals = 0x04,
            Demo = 0x08,
            Is16BitColor = 0x10,
            AudioTracks = 0x20,
        }

        public byte[] ObjectOwnerTable;
        public byte[] ObjectStateTable;
        public uint[] ClassData;
        byte[] roomDisks = new byte[59];
        byte[] roomTracks = new byte[59];
        byte[] roomSectors = new byte[59];
        struct Res { public byte RoomNum; public long Offset; }
        Res[] CostumeResources;
        Res[] ScriptResources;
        Res[] SoundResources;

        void ParseIndex1(FamilyGame game, GameFeatures gameFeatures, BinaryReader r)
        {
            const ushort MAGIC = 0x0A31;

            int numGlobalObjects;
            int numRooms;
            int numCostumes;
            int numScripts;
            int numSounds;

            if (game.Id == "MM") // Maniac Mansion
            {
                numGlobalObjects = 256;
                numRooms = 55;
                numCostumes = 25;

                if (gameFeatures.HasFlag(GameFeatures.Demo))
                {
                    numScripts = 55;
                    numSounds = 40;
                }
                else
                {
                    numScripts = 160;
                    numSounds = 70;
                }
            }
            else
            {
                numGlobalObjects = 775;
                numRooms = 59;
                numCostumes = 38;
                numScripts = 155;
                numSounds = 127;
            }

            var magic = r.ReadUInt16();
            if (magic != MAGIC) throw new FormatException("BAD MAGIC");

            // object flags
            ObjectOwnerTable = new byte[numGlobalObjects];
            ObjectStateTable = new byte[numGlobalObjects];
            ClassData = new uint[numGlobalObjects];
            for (var i = 0; i < numGlobalObjects; i++)
            {
                var tmp = r.ReadByte();
                ObjectOwnerTable[i] = (byte)(tmp & 0x0F);
                ObjectStateTable[i] = (byte)(tmp >> 4);
            }

            // room offsets
            for (var i = 0; i < numRooms; i++)
            {
                roomDisks[i] = (byte)(r.ReadByte() - '0');
            }
            for (var i = 0; i < numRooms; i++)
            {
                roomSectors[i] = r.ReadByte();
                roomTracks[i] = r.ReadByte();
            }
            CostumeResources = ReadResTypeList(r, numCostumes);
            ScriptResources = ReadResTypeList(r, numScripts);
            SoundResources = ReadResTypeList(r, numSounds);
        }

        Res[] ReadResTypeList(BinaryReader r, int? numEntries = null)
        {
            var num = numEntries.HasValue ? numEntries.Value : r.ReadByte();
            var res = new Res[num];
            var rooms = r.ReadBytes(num);
            for (var i = 0; i < num; i++)
            {
                var offset = ToOffset(r.ReadUInt16());
                res[i] = new Res { RoomNum = rooms[i], Offset = offset };
            }
            return res;
        }

        static uint ToOffset(ushort offset) => offset == 0xFFFF ? 0xFFFFFFFF : offset;

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var game = source.Game;
            var detect = source.Game.Detect<Dictionary<string, object>>(string.Empty, r);
            if (detect == null) throw new FormatException("No Detect");
            var featureStr = (string)detect["features"];
            var gameFeatures = featureStr.Split(' ').Aggregate(GameFeatures.None, (a, f) => a |= (GameFeatures)Enum.Parse(typeof(GameFeatures), f, true));

            ParseIndex1(game, gameFeatures, r);


            var files = source.Files = new List<FileSource>();

            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            throw new NotImplementedException();
        }
    }
}