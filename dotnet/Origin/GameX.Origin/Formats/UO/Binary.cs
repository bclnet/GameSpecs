using GameX.Meta;
using GameX.Origin.Structs.UO;
using GameX.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static GameX.Origin.Formats.PakBinary_UO;

namespace GameX.Origin.Formats.UO
{
    #region Binary_Anim - TODO

    public unsafe class Binary_Anim : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Anim(r));

        // file: artLegacyMUL.mul:static/file04000.art
        public Binary_Anim(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Anim File" }),
                new MetaInfo("Anim", items: new List<MetaInfo> {
                    //new MetaInfo($"Default: {Default.GumpID}"),
                })
            };
    }

    #endregion

    #region Binary_Animdata

    public unsafe class Binary_Animdata : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Animdata(r));

        #region Records

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AnimRecord
        {
            public static (string, int) Struct = ("<64b4B", sizeof(AnimRecord));
            public fixed sbyte Frames[64];
            public byte Unknown;
            public byte FrameCount;
            public byte FrameInterval;
            public byte StartInterval;
        }

        public class Record
        {
            public sbyte[] Frames = new sbyte[64];
            public byte FrameCount;
            public byte FrameInterval;
            public byte StartInterval;

            public Record(ref AnimRecord record)
            {
                fixed (sbyte* frames_ = record.Frames) Frames = UnsafeX.FixedTArray(frames_, 64);
                FrameCount = record.FrameCount;
                FrameInterval = record.FrameInterval;
                StartInterval = record.StartInterval;
            }
        }

        readonly Dictionary<int, Record> Records = new Dictionary<int, Record>();

        #endregion

        // file: animdata.mul
        public Binary_Animdata(BinaryReader r)
        {
            var id = 0;
            var length = r.BaseStream.Length / (4 + (8 * (64 + 4)));
            for (var i = 0; i < length; i++)
            {
                r.Skip(4);
                var records = r.ReadSArray<AnimRecord>(8);
                for (var j = 0; j < 8; j++, id++)
                {
                    ref AnimRecord record = ref records[j];
                    if (record.FrameCount > 0)
                        Records[id] = new Record(ref record);
                }
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Animdata File" }),
                new MetaInfo("Animdata", items: new List<MetaInfo> {
                    new MetaInfo($"Records: {Records.Count}"),
                })
            };
    }

    #endregion

    #region Binary_AsciiFont

    public unsafe class Binary_AsciiFont : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_AsciiFont(r));

        #region Records

        public class AsciiFont
        {
            public byte[][] Characters = new byte[224][];
            public int Height;

            public AsciiFont(BinaryReader r)
            {
                r.ReadByte();
                for (var i = 0; i < 224; ++i)
                {
                    var width = r.ReadByte();
                    var height = r.ReadByte();
                    r.ReadByte();
                    if (width <= 0 || height <= 0) continue;

                    if (height > Height && i < 96) Height = height;

                    //var bd = MemoryMarshal.Cast<byte, ushort>(data.AsSpan(0, length << 1));
                    //for (var i = 0; i < length; i++) if (bd[i] != 0) bd[i] ^= 0x8000;
                    //Pixels = MemoryMarshal.Cast<ushort, byte>(bd.ToArray()).ToArray();

                    var bd = new byte[width * height << 1];
                    var bd_Stride = width << 1;
                    fixed (byte* bd_Scan0 = bd)
                    {
                        var line = (ushort*)bd_Scan0;
                        var delta = bd_Stride >> 1;

                        for (var y = 0; y < height; ++y, line += delta)
                        {
                            ushort* cur = line;
                            for (var x = 0; x < width; ++x)
                            {
                                var pixel = (ushort)(r.ReadByte() | (r.ReadByte() << 8));
                                cur[x] = pixel == 0 ? pixel : (ushort)(pixel ^ 0x8000);
                            }
                        }
                    }
                    Characters[i] = bd;
                }
            }
        }

        readonly AsciiFont[] Fonts = new AsciiFont[10];

        #endregion

        // file: fonts.mul
        public Binary_AsciiFont(BinaryReader r)
        {
            for (var i = 0; i < Fonts.Length; i++)
                Fonts[i] = new AsciiFont(r);
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "AsciiFont File" }),
                new MetaInfo("AsciiFont", items: new List<MetaInfo> {
                    new MetaInfo($"Fonts: {Fonts.Length}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_BodyConverter

    public unsafe class Binary_BodyConverter : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_BodyConverter(r.ToStream()));

        #region Records

        readonly int[] Table1;
        readonly int[] Table2;
        readonly int[] Table3;
        readonly int[] Table4;

        public bool Contains(int body)
            => Table1 != null && body >= 0 && body < Table1.Length && Table1[body] != -1 ? true
            : Table2 != null && body >= 0 && body < Table2.Length && Table2[body] != -1 ? true
            : Table3 != null && body >= 0 && body < Table3.Length && Table3[body] != -1 ? true
            : Table4 != null && body >= 0 && body < Table4.Length && Table4[body] != -1 ? true
            : false;

        public int Convert(ref int body)
        {
            // Converts MountItemID to BodyID
            //if (body > 0x3E93)
            //    for (var i = 0; i < MountIDConv.Length; ++i)
            //    {
            //        var conv = MountIDConv[i];
            //        if (conv[0] == body) { body = conv[1]; break; }
            //    }
            if (Table1 != null && body >= 0 && body < Table1.Length)
            {
                var val = Table1[body];
                if (val != -1) { body = val; return 2; }
            }
            if (Table2 != null && body >= 0 && body < Table2.Length)
            {
                var val = Table2[body];
                if (val != -1) { body = val; return 3; }
            }
            if (Table3 != null && body >= 0 && body < Table3.Length)
            {
                var val = Table3[body];
                if (val != -1) { body = val; return 4; }
            }
            if (Table4 != null && body >= 0 && body < Table4.Length)
            {
                var val = Table4[body];
                if (val != -1) { body = val; return 5; }
            }
            return 1;
        }

        public int GetTrueBody(int fileType, int index)
        {
            switch (fileType)
            {
                case 1:
                default: { return index; }
                case 2:
                    {
                        if (Table1 != null && index >= 0)
                            for (var i = 0; i < Table1.Length; ++i) if (Table1[i] == index) return i;
                        break;
                    }
                case 3:
                    {
                        if (Table2 != null && index >= 0)
                            for (var i = 0; i < Table2.Length; ++i) if (Table2[i] == index) return i;
                        break;
                    }
                case 4:
                    {
                        if (Table3 != null && index >= 0)
                            for (var i = 0; i < Table3.Length; ++i) if (Table3[i] == index) return i;
                        break;
                    }
                case 5:
                    {
                        if (Table4 != null && index >= 0)
                            for (var i = 0; i < Table4.Length; ++i) if (Table4[i] == index) return i;
                        break;
                    }
            }
            return -1;
        }

        #endregion

        #region Records2

        // Mounts: ItemID, BodyID
        //static readonly int[][] MountIDConv = {
        //    new [] { 0x3E94, 0xF3 }, // Hiryu
        //    new [] { 0x3E97, 0xC3 }, // Beetle
        //    new [] { 0x3E98, 0xC2 }, // Swamp Dragon
        //    new [] { 0x3E9A, 0xC1 }, // Ridgeback
        //    new [] { 0x3E9B, 0xC0 }, // Unicorn
        //    new [] { 0x3E9C, 0xBF }, // Ki-Rin
        //    new [] { 0x3E9E, 0xBE }, // Fire Steed
        //    new [] { 0x3E9F, 0xC8 }, // Horse
        //    new [] { 0x3EA0, 0xE2 }, // Grey Horse
        //    new [] { 0x3EA1, 0xE4 }, // Horse
        //    new [] { 0x3EA2, 0xCC }, // Brown Horse
        //    new [] { 0x3EA3, 0xD2 }, // Zostrich
        //    new [] { 0x3EA4, 0xDA }, // Zostrich
        //    new [] { 0x3EA5, 0xDB }, // Zostrich
        //    new [] { 0x3EA6, 0xDC }, // Llama
        //    new [] { 0x3EA7, 0x74 }, // Nightmare
        //    new [] { 0x3EA8, 0x75 }, // Silver Steed
        //    new [] { 0x3EA9, 0x72 }, // Nightmare
        //    new [] { 0x3EAA, 0x73 }, // Ethereal Horse
        //    new [] { 0x3EAB, 0xAA }, // Ethereal Llama
        //    new [] { 0x3EAC, 0xAB }, // Ethereal Zostrich
        //    new [] { 0x3EAD, 0x84 }, // Ki-Rin
        //    new [] { 0x3EAF, 0x78 }, // Minax Warhorse
        //    new [] { 0x3EB0, 0x79 }, // ShadowLords Warhorse
        //    new [] { 0x3EB1, 0x77 }, // COM Warhorse
        //    new [] { 0x3EB2, 0x76 }, // TrueBritannian Warhorse
        //    new [] { 0x3EB3, 0x90 }, // Seahorse
        //    new [] { 0x3EB4, 0x7A }, // Unicorn
        //    new [] { 0x3EB5, 0xB1 }, // Nightmare
        //    new [] { 0x3EB6, 0xB2 }, // Nightmare
        //    new [] { 0x3EB7, 0xB3 }, // Dark Nightmare
        //    new [] { 0x3EB8, 0xBC }, // Ridgeback
        //    new [] { 0x3EBA, 0xBB }, // Ridgeback
        //    new [] { 0x3EBB, 0x319 }, // Undead Horse
        //    new [] { 0x3EBC, 0x317 }, // Beetle
        //    new [] { 0x3EBD, 0x31A }, // Swamp Dragon
        //    new [] { 0x3EBE, 0x31F }, // Armored Swamp Dragon
        //    new [] { 0x3F6F, 0x9 }    // Daemon
        //};

        //public static int DeathAnimationIndex(Body body)
        //    => (object)body.Type switch
        //    {
        //        BodyType.Human => 21,
        //        BodyType.Monster => 2,
        //        BodyType.Animal => 8,
        //        _ => 2,
        //    };

        //public static int DeathAnimationFrameCount(Body body)
        //    => (object)body.Type switch
        //    {
        //        BodyType.Human => 6,
        //        BodyType.Monster => 4,
        //        BodyType.Animal => 4,
        //        _ => 4,
        //    };

        //public static bool CheckIfItemIsMount(ref int itemID)
        //{
        //    if (itemID > 0x3E93)
        //        for (var i = 0; i < MountIDConv.Length; ++i)
        //        {
        //            var conv = MountIDConv[i];
        //            if (conv[0] == itemID) { itemID = conv[1]; return true; }
        //        }
        //    return false;
        //}

        #endregion

        // file: Bodyconv.def
        public Binary_BodyConverter(StreamReader r)
        {
            List<int> list1 = new List<int>(), list2 = new List<int>(), list3 = new List<int>(), list4 = new List<int>();
            int max1 = 0, max2 = 0, max3 = 0, max4 = 0;

            while (r.ReadLine() is { } line)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("\"#")) continue;

                try
                {
                    var split = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    var hasOriginalBodyId = int.TryParse(split[0], out int original);
                    if (!hasOriginalBodyId) continue;

                    if (!int.TryParse(split[1], out var anim2)) anim2 = -1;
                    if (!int.TryParse(split[2], out var anim3)) anim3 = -1;
                    if (!int.TryParse(split[3], out var anim4)) anim4 = -1;
                    if (!int.TryParse(split[4], out var anim5)) anim5 = -1;

                    if (anim2 != -1)
                    {
                        if (anim2 == 68) anim2 = 122;
                        if (original > max1) max1 = original;
                        list1.Add(original);
                        list1.Add(anim2);
                    }
                    if (anim3 != -1)
                    {
                        if (original > max2) max2 = original;
                        list2.Add(original);
                        list2.Add(anim3);
                    }
                    if (anim4 != -1)
                    {
                        if (original > max3) max3 = original;
                        list3.Add(original);
                        list3.Add(anim4);
                    }
                    if (anim5 != -1)
                    {
                        if (original > max4) max4 = original;
                        list4.Add(original);
                        list4.Add(anim5);
                    }
                }
                catch { }

                Table1 = new int[max1 + 1];
                for (var i = 0; i < Table1.Length; ++i) Table1[i] = -1;
                for (var i = 0; i < list1.Count; i += 2) Table1[list1[i]] = list1[i + 1];

                Table2 = new int[max2 + 1];
                for (var i = 0; i < Table2.Length; ++i) Table2[i] = -1;
                for (var i = 0; i < list2.Count; i += 2) Table2[list2[i]] = list2[i + 1];

                Table3 = new int[max3 + 1];
                for (var i = 0; i < Table3.Length; ++i) Table3[i] = -1;
                for (var i = 0; i < list3.Count; i += 2) Table3[list3[i]] = list3[i + 1];

                Table4 = new int[max4 + 1];
                for (var i = 0; i < Table4.Length; ++i) Table4[i] = -1;
                for (var i = 0; i < list4.Count; i += 2) Table4[list4[i]] = list4[i + 1];
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "BodyConverter Config" }),
                new MetaInfo("BodyConverter", items: new List<MetaInfo> {
                    new MetaInfo($"Table1: {Table1.Length}"),
                    new MetaInfo($"Table2: {Table2.Length}"),
                    new MetaInfo($"Table3: {Table3.Length}"),
                    new MetaInfo($"Table4: {Table4.Length}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_BodyTable

    public unsafe class Binary_BodyTable : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_BodyTable(r.ToStream()));

        #region Records

        public class Record
        {
            public readonly int OldId;
            public readonly int NewId;
            public readonly int NewHue;

            public Record(int oldId, int newId, int newHue)
            {
                OldId = oldId;
                NewId = newId;
                NewHue = newHue;
            }
        }

        readonly Dictionary<int, Record> Records = new Dictionary<int, Record>();

        //public static void TranslateBodyAndHue(ref int id, ref int hue)
        //{
        //    if (Records.TryGetValue(id, out var bte))
        //    {
        //        id = bte.NewId;
        //        if (hue == 0) hue = bte.NewHue;
        //    }
        //}

        #endregion

        // file: Body.def
        public Binary_BodyTable(StreamReader r)
        {
            while (r.ReadLine() is { } line)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                try
                {
                    var index1 = line.IndexOf("{", StringComparison.Ordinal);
                    var index2 = line.IndexOf("}", StringComparison.Ordinal);

                    var param1 = line[..index1];
                    var param2 = line[(index1 + 1)..index2];
                    var param3 = line[(index2 + 1)..];

                    var indexOf = param2.IndexOf(',');
                    if (indexOf > -1) param2 = param2[..indexOf].Trim();

                    var oldId = Convert.ToInt32(param1);
                    var newId = Convert.ToInt32(param2);
                    var newHue = Convert.ToInt32(param3);
                    Records[oldId] = new Record(oldId, newId, newHue);
                }
                catch { }
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "BodyTable config" }),
                new MetaInfo("BodyTable", items: new List<MetaInfo> {
                    new MetaInfo($"Records: {Records.Count}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_CalibrationInfo

    public unsafe class Binary_CalibrationInfo : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_CalibrationInfo(r.ToStream()));

        #region Records

        public class Record
        {
            public readonly byte[] Mask;
            public readonly byte[] Vals;
            public readonly byte[] DetX;
            public readonly byte[] DetY;
            public readonly byte[] DetZ;
            public readonly byte[] DetF;

            public Record(byte[] mask, byte[] vals, byte[] detx, byte[] dety, byte[] detz, byte[] detf)
            {
                Mask = mask;
                Vals = vals;
                DetX = detx;
                DetY = dety;
                DetZ = detz;
                DetF = detf;
            }
        }

        readonly List<Record> Records = new List<Record>();

        static Record[] DefaultRecords = {
            new Record(
                // Post 7.0.4.0 (Andreew)
                new byte[]
                {
                    0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                    0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF
                },
                new byte[]
                {
                    0xFF, 0xD0, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x11, 0x8B,
                    0x82, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xD0, 0x5B, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00, 0xEC
                },
                new byte[]{ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x0C }, // x
                new byte[]{ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x08 }, // y
                new byte[]{ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x04 }, // z
                new byte[]{ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x10 }),// f
            new Record(
                // (arul) 6.0.9.x+ : Calibrates both 
                new byte[]
                {
                    0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                    0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF
                },
                new byte[]
                {
                    0xFF, 0xD0, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x11, 0x8B,
                    0x82, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xD0, 0x5E, 0xE9, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D
                },
                new byte[]{ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x0C },
                new byte[]{ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x08 },
                new byte[]{ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x04 },
                new byte[]{ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x10 }),
            new Record(
                // Facet
                new byte[]
                {
                    0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                },
                new byte[]
                {
                    0xA0, 0x00, 0x00, 0x00, 0x00, 0x84, 0xC0, 0x0F, 0x85, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D
                },
                Array.Empty<byte>(),
                Array.Empty<byte>(),
                Array.Empty<byte>(),
                new byte[]{ 0x01, 0x04, 0xFF, 0xFF, 0xFF, 0x01 }),
            new Record(
                // Location
                new byte[]
                {
                    0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0x00, 0x00,
                    0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00
                },
                new byte[]
                {
                    0x8B, 0x15, 0x00, 0x00, 0x00, 0x00, 0x83, 0xC4, 0x10, 0x66, 0x89, 0x5A, 0x00, 0xA1, 0x00, 0x00,
                    0x00, 0x00, 0x66, 0x89, 0x78, 0x00, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x66, 0x89, 0x71, 0x00
                },
                new byte[]{ 0x02, 0x04, 0x04, 0x0C, 0x01, 0x02 },
                new byte[]{ 0x0E, 0x04, 0x04, 0x15, 0x01, 0x02 },
                new byte[]{ 0x18, 0x04, 0x04, 0x1F, 0x01, 0x02 },
                Array.Empty<byte>()),
            new Record(
                // UO3D Only, calibrates both
                new byte[]
                {
                    0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF,
                    0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00
                },
                new byte[]
                {
                    0xA1, 0x00, 0x00, 0x00, 0x00, 0x68, 0x40, 0x2E, 0x04, 0x01, 0x0F, 0xBF, 0x50, 0x00, 0x0F, 0xBF,
                    0x48, 0x00, 0x52, 0x51, 0x0F, 0xBF, 0x50, 0x00, 0x52, 0x8D, 0x85, 0xE4, 0xFD, 0xFF, 0xFF, 0x68,
                    0x00, 0x00, 0x00, 0x00, 0x50, 0xE8, 0x07, 0x44, 0x10, 0x00, 0x8A, 0x0D, 0x00, 0x00, 0x00, 0x00
                },
                new byte[] { 0x01, 0x04, 0x04, 0x17, 0x01, 0x02 },
                new byte[] { 0x01, 0x04, 0x04, 0x11, 0x01, 0x02 },
                new byte[] { 0x01, 0x04, 0x04, 0x0D, 0x01, 0x02 },
                new byte[] { 0x2C, 0x04, 0xFF, 0xFF, 0xFF, 0x01 })
        };

        #endregion

        // file: calibration.cfg
        public Binary_CalibrationInfo(StreamReader r)
        {
            while (r.ReadLine() is { } line)
            {
                line = line.Trim();
                if (!line.Equals("Begin", StringComparison.OrdinalIgnoreCase)) continue;

                byte[] mask, vals, detx, dety, detz, detf;
                if ((mask = ReadBytes(r)) == null) continue;
                if ((vals = ReadBytes(r)) == null) continue;
                if ((detx = ReadBytes(r)) == null) continue;
                if ((dety = ReadBytes(r)) == null) continue;
                if ((detz = ReadBytes(r)) == null) continue;
                if ((detf = ReadBytes(r)) == null) continue;
                Records.Add(new Record(mask, vals, detx, dety, detz, detf));
            }
            Records.AddRange(DefaultRecords);
        }

        static byte[] ReadBytes(TextReader r)
        {
            var line = r.ReadLine();
            if (line == null) return null;

            var b = new byte[(line.Length + 2) / 3];
            var index = 0;
            for (var i = 0; (i + 1) < line.Length; i += 3)
            {
                var ch = line[i + 0];
                var cl = line[i + 1];

                if (ch >= '0' && ch <= '9') ch -= '0';
                else if (ch >= 'a' && ch <= 'f') ch -= (char)('a' - 10);
                else if (ch >= 'A' && ch <= 'F') ch -= (char)('A' - 10);
                else return null;

                if (cl >= '0' && cl <= '9') cl -= '0';
                else if (cl >= 'a' && cl <= 'f') cl -= (char)('a' - 10);
                else if (cl >= 'A' && cl <= 'F') cl -= (char)('A' - 10);
                else return null;

                b[index++] = (byte)((ch << 4) | cl);
            }
            return b;
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "CalibrationInfo File" }),
                new MetaInfo("CalibrationInfo", items: new List<MetaInfo> {
                    new MetaInfo($"Records: {Records.Count}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_Gump

    public unsafe class Binary_Gump : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Gump(r, (int)f.FileSize, f.Compressed));

        #region Records

        static byte[] _pixels;
        static byte[] _colors;

        byte[] Pixels;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: gumpartLegacyMUL.uop:file00000.tex
        public Binary_Gump(BinaryReader r, int length, int extra)
        {
            int width = Width = (extra >> 16) & 0xFFFF;
            int height = Height = extra & 0xFFFF;
            if (width <= 0 || height <= 0) return;
            Load(r.ReadBytes(length), width, height);
        }

        void Load(byte[] data, int width, int height)
        {
            fixed (byte* _ = data)
            {
                var bd = Pixels = new byte[width * height << 1];
                var delta = width;
                fixed (byte* bd_Scan0 = bd)
                {
                    var lookup = (int*)_;
                    var dat = (ushort*)_;
                    var line = (ushort*)bd_Scan0;

                    for (var y = 0; y < height; ++y, line += delta)
                    {
                        var count = *lookup++ << 1;
                        ushort* cur = line, end = line + width;
                        while (cur < end)
                        {
                            var color = dat[count++];
                            var next = cur + dat[count++];

                            if (color == 0) cur = next;
                            else
                            {
                                color ^= 0x8000;
                                while (cur < next) *cur++ = color;
                            }
                        }
                    }
                }
            }
        }

        void LoadWithHue(byte[] data, Binary_Hues.Record hue, bool onlyHueGrayPixels)
        {
            int width = Width, height = Height;
            fixed (byte* _ = data)
            {
                if (width <= 0 || height <= 0) return;

                int bytesPerLine = width << 1, bytesPerStride = (bytesPerLine + 3) & ~3, bytesForImage = height * bytesPerStride;
                int pixelsPerStride = (width + 1) & ~1, pixelsPerStrideDelta = pixelsPerStride - width;

                if (_pixels == null || _pixels.Length < bytesForImage) _pixels = new byte[(bytesForImage + 2047) & ~2047];
                _colors ??= new byte[128];

                fixed (ushort* hueColors_ = hue.Colors)
                fixed (byte* pixels_ = _pixels)
                fixed (byte* colors_ = _colors)
                {
                    var hueColors = hueColors_;
                    var hueColorsEnd = hueColors + 32;
                    var colors = (ushort*)colors_;
                    var colorsOpaque = colors;

                    while (hueColors < hueColorsEnd) *colorsOpaque++ = *hueColors++;

                    var pixelsStart = (ushort*)pixels_;

                    var lookup = (int*)_;
                    int* lookupEnd = lookup + height, pixelRleStart = lookup, pixelRle;
                    ushort* pixel = pixelsStart, rleEnd, pixelEnd = pixel + width;

                    ushort color, count;
                    if (onlyHueGrayPixels)
                        while (lookup < lookupEnd)
                        {
                            pixelRle = pixelRleStart + *lookup++;
                            rleEnd = pixel;

                            while (pixel < pixelEnd)
                            {
                                color = *(ushort*)pixelRle;
                                count = *(1 + (ushort*)pixelRle);
                                ++pixelRle;

                                rleEnd += count;

                                if (color != 0 && (color & 0x1F) == ((color >> 5) & 0x1F) && (color & 0x1F) == ((color >> 10) & 0x1F)) color = colors[color >> 10];
                                else if (color != 0) color ^= 0x8000;

                                while (pixel < rleEnd) *pixel++ = color;
                            }

                            pixel += pixelsPerStrideDelta;
                            pixelEnd += pixelsPerStride;
                        }
                    else
                        while (lookup < lookupEnd)
                        {
                            pixelRle = pixelRleStart + *lookup++;
                            rleEnd = pixel;

                            while (pixel < pixelEnd)
                            {
                                color = *(ushort*)pixelRle;
                                count = *(1 + (ushort*)pixelRle);
                                ++pixelRle;

                                rleEnd += count;

                                if (color != 0) color = colors[color >> 10];

                                while (pixel < rleEnd) *pixel++ = color;
                            }

                            pixel += pixelsPerStrideDelta;
                            pixelEnd += pixelsPerStride;
                        }
                    //Image = new Bitmap(width, height, bytesPerStride, PixelFormat.Format16bppArgb1555, (IntPtr)pixelsStart);
                }
            }
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Pixels;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("Gump", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Width}"),
                    new MetaInfo($"Height: {Height}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_GumpDef

    public unsafe class Binary_GumpDef : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_GumpDef(r.ToStream()));

        #region Records

        public bool ItemHasGumpTranslation(int gumpIndex, out int gumpIndexTranslated, out int defaultHue)
        {
            if (Records.TryGetValue(gumpIndex, out var translation))
            {
                gumpIndexTranslated = translation.Item1;
                defaultHue = translation.Item2;
                return true;
            }
            gumpIndexTranslated = 0;
            defaultHue = 0;
            return false;
        }

        readonly Dictionary<int, (int, int)> Records = new Dictionary<int, (int, int)>();

        #endregion

        // file: gump.def
        public Binary_GumpDef(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var defs = line.Replace('\t', ' ').Split(' ');
                if (defs.Length != 3) continue;
                var inGump = int.Parse(defs[0]);
                var outGump = int.Parse(defs[1].Replace("{", string.Empty).Replace("}", string.Empty));
                var outHue = int.Parse(defs[2]);
                Records[inGump] = (outGump, outHue);
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Gump Language File" }),
                new MetaInfo("GumpDef", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_Hues

    public unsafe class Binary_Hues : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Hues(r));

        #region Records

        public class Record
        {
            public int Id;
            public ushort[] Colors = new ushort[32];
            public ushort TableStart;
            public ushort TableEnd;
            public string Name;

            public Record(int id)
            {
                Id = id;
                Colors = new ushort[32];
                Name = "Null";
            }
            public Record(int id, ref HueRecord record)
            {
                Id = id;
                fixed (ushort* colors_ = record.Colors) Colors = UnsafeX.FixedTArray(colors_, 32);
                TableStart = record.TableStart;
                TableEnd = record.TableEnd;
                fixed (byte* name_ = record.Name) Name = UnsafeX.FixedAString(name_, 20);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HueRecord
        {
            public static (string, int) Struct = ("<?", sizeof(HueRecord));
            public fixed ushort Colors[32];
            public ushort TableStart;
            public ushort TableEnd;
            public fixed byte Name[20];
        }

        readonly Record[] Records = new Record[3000];

        #endregion

        // file: hues.mul
        public Binary_Hues(BinaryReader r)
        {
            var blockCount = (int)r.BaseStream.Length / 708;
            if (blockCount > 375) blockCount = 375;

            var id = 0;
            for (var i = 0; i < blockCount; ++i)
            {
                r.Skip(4);
                var records = r.ReadSArray<HueRecord>(8);
                for (var j = 0; j < 8; j++, id++)
                    Records[id] = new Record(id, ref records[j]);
            }
            for (; id < Records.Length; id++)
                Records[id] = new Record(id);
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Hues File" }),
                new MetaInfo("Hues", items: new List<MetaInfo> {
                    new MetaInfo($"Records: {Records.Length}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_Land

    public unsafe class Binary_Land : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Land(r, (int)f.FileSize));

        #region Records

        byte[] Pixels;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: artLegacyMUL.uop:land/file00000.land
        public Binary_Land(BinaryReader r, int length)
        {
            fixed (byte* _ = r.ReadBytes(length))
            {
                var bd = Pixels = new byte[44 * 44 << 1];
                var bd_Stride = 44 << 1;
                fixed (byte* bd_Scan0 = bd)
                {
                    var bdata = (ushort*)_;
                    int xOffset = 21, xRun = 2;
                    var line = (ushort*)bd_Scan0;
                    var delta = bd_Stride >> 1;

                    for (var y = 0; y < 22; ++y, --xOffset, xRun += 2, line += delta)
                    {
                        ushort* cur = line + xOffset, end = cur + xRun;
                        while (cur < end) *cur++ = (ushort)(*bdata++ | 0x8000);
                    }

                    xOffset = 0; xRun = 44;
                    for (var y = 0; y < 22; ++y, ++xOffset, xRun -= 2, line += delta)
                    {
                        ushort* cur = line + xOffset, end = cur + xRun;
                        while (cur < end) *cur++ = (ushort)(*bdata++ | 0x8000);
                    }
                }
            }
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; } = 44;
        public int Height { get; } = 44;
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Pixels;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("Land", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Width}"),
                    new MetaInfo($"Height: {Height}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_Light

    public unsafe class Binary_Light : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Light(r, (int)f.FileSize, f.Compressed));

        #region Records

        byte[] Pixels;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: lightidx.mul:file00000.light
        public Binary_Light(BinaryReader r, int length, int extra)
        {
            fixed (byte* _ = r.ReadBytes(length))
            {
                var bdata = (sbyte*)_;
                var width = Width = extra & 0xFFFF;
                var height = Height = (extra >> 16) & 0xFFFF;
                if (width <= 0 || height <= 0) return;

                var bd = Pixels = new byte[width * height << 1];
                var delta = width;
                fixed (byte* bd_Scan0 = bd)
                {
                    var line = (ushort*)bd_Scan0;
                    for (var y = 0; y < height; ++y, line += delta)
                    {
                        ushort* cur = line, end = cur + width;
                        while (cur < end)
                        {
                            var value = *bdata++;
                            *cur++ = (ushort)(((0x1f + value) << 10) + ((0x1F + value) << 5) + (0x1F + value));
                        }
                    }
                }
            }
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Pixels;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("Light", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Width}"),
                    new MetaInfo($"Height: {Height}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_MobType

    public unsafe class Binary_MobType : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_MobType(r.ToStream()));

        #region Records

        public enum MobType
        {
            Null = -1,
            Monster = 0,
            Animal = 1,
            Humanoid = 2
        }

        struct Record
        {
            public string Flags;
            public MobType AnimationType;

            public Record(string type, string flags)
            {
                Flags = flags;
                AnimationType = type switch
                {
                    "MONSTER" => MobType.Monster,
                    "ANIMAL" => MobType.Animal,
                    "SEA_MONSTER" => MobType.Monster,
                    "HUMAN" => MobType.Humanoid,
                    "EQUIPMENT" => MobType.Humanoid,
                    _ => MobType.Null,
                };
            }
        }

        public MobType AnimationTypeXXX(int bodyID) => Records[bodyID].AnimationType;

        readonly Dictionary<int, Record> Records = new Dictionary<int, Record>();

        #endregion

        // file: mobtypes.txt
        public Binary_MobType(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var data = line.Replace("   ", "\t").Split('\t');
                var bodyID = int.Parse(data[0]);
                Records[bodyID] = new Record(data[1].Trim(), data[2].Trim());
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "MobType File" }),
                new MetaInfo("MobType", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_MultiMap

    public unsafe class Binary_MultiMap : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_MultiMap(r, f));

        #region Records

        byte[] Pixels;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: Multimap.rle
        public Binary_MultiMap(BinaryReader r, FileSource f)
        {
            if (f.Path.StartsWith("facet"))
            {
                Width = r.ReadInt16();
                Height = r.ReadInt16();

                var bd = Pixels = new byte[Width * Height << 1];
                var bd_Stride = Width << 1;
                fixed (byte* bd_Scan0 = bd)
                {
                    var line = (ushort*)bd_Scan0;
                    int delta = bd_Stride >> 1;

                    for (var y = 0; y < Height; y++, line += delta)
                    {
                        var colorsCount = r.ReadInt32() / 3;
                        ushort* cur = line, endline = line + delta;
                        for (var c = 0; c < colorsCount; c++)
                        {
                            var count = r.ReadByte();
                            var color = r.ReadInt16();
                            var end = cur + count;
                            while (cur < end)
                            {
                                if (cur > endline) break;
                                *cur++ = (ushort)(color ^ 0x8000);
                            }
                        }
                    }
                }
            }
            else
            {
                Width = r.ReadInt32();
                Height = r.ReadInt32();

                var bd = Pixels = new byte[Width * Height << 1];
                var bd_Stride = Width << 1;
                fixed (byte* bd_Scan0 = bd)
                {
                    var line = (ushort*)bd_Scan0;
                    var delta = bd_Stride >> 1;

                    var cur = line;
                    var len = (int)(r.BaseStream.Length - r.BaseStream.Position);

                    var b = new byte[len];
                    r.Read(b, 0, len);

                    int j = 0, x = 0;
                    while (j != len)
                    {
                        var pixel = b[j++];
                        var count = pixel & 0x7f;

                        // black or white color
                        var c = (pixel & 0x80) != 0 ? (ushort)0x8000 : (ushort)0xffff;

                        for (var i = 0; i < count; ++i)
                        {
                            cur[x++] = c;
                            if (x < Width) continue;
                            cur += delta;
                            x = 0;
                        }
                    }
                }
            }
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Pixels;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("MultiMap", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Width}"),
                    new MetaInfo($"Height: {Height}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_MusicDef

    public unsafe class Binary_MusicDef : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_MusicDef(r.ToStream()));

        #region Records

        public static bool TryGetMusicData(int index, out string name, out bool doesLoop)
        {
            if (Records.ContainsKey(index))
            {
                name = Records[index].Item1;
                doesLoop = Records[index].Item2;
                return true;
            }
            name = null;
            doesLoop = false;
            return false;
        }

        static readonly Dictionary<int, (string, bool)> Records = new Dictionary<int, (string, bool)>();

        #endregion

        // file: Music/Digital/Config.txt
        public Binary_MusicDef(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                var splits = line.Split(new[] { ' ', ',', '\t' });
                if (splits.Length < 2 || splits.Length > 3) continue;
                var index = int.Parse(splits[0]);
                var name = splits[1].Trim();
                var doesLoop = splits.Length == 3 && splits[2] == "loop";
                Records.Add(index, (name, doesLoop));
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Music config" }),
                new MetaInfo("MusicDef", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_Multi

    public unsafe class Binary_Multi : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Multi(r, (int)f.FileSize, Art_IsUOAHS));

        #region Records

        public class Record
        {
            public ushort ItemId;
            public short OffsetX;
            public short OffsetY;
            public short OffsetZ;
            public int Flags;
            public int Unknown;

            public Record(ref MultiRecordV1 record)
            {
                ItemId = Art_ClampItemId(record.ItemId);
                OffsetX = record.OffsetX;
                OffsetY = record.OffsetY;
                OffsetZ = record.OffsetZ;
                Flags = record.Flags;
            }
            public Record(ref MultiRecordV2 record)
            {
                ItemId = Art_ClampItemId(record.ItemId);
                OffsetX = record.OffsetX;
                OffsetY = record.OffsetY;
                OffsetZ = record.OffsetZ;
                Flags = record.Flags;
                Unknown = record.Unknown;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MultiRecordV1
        {
            public static (string, int) Struct = ("<?", sizeof(MultiRecordV1));
            public ushort ItemId;
            public short OffsetX;
            public short OffsetY;
            public short OffsetZ;
            public int Flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MultiRecordV2
        {
            public static (string, int) Struct = ("<?", sizeof(MultiRecordV2));
            public ushort ItemId;
            public short OffsetX;
            public short OffsetY;
            public short OffsetZ;
            public int Flags;
            public int Unknown;
        }

        public Point Min;
        public Point Max;
        public Point Center;
        public int Width;
        public int Height;
        public int MaxHeight;
        public Record[] SortedTiles;
        public MTile[][][] Tiles;
        public int Surfaces;

        #endregion

        public class MTileList : List<MTile>
        {
        }

        public class MTile
        {
            public int Solver { get; internal set; }
        }

        // file: multi.idx:file00000.multi
        public Binary_Multi(BinaryReader r, int length, bool newFormat)
        {
            length &= 0x7FFFFFFF;
            SortedTiles = newFormat
                ? r.ReadSArray<MultiRecordV1>(length / 16).Select(x => new Record(ref x)).ToArray()
                : r.ReadSArray<MultiRecordV2>(length / 12).Select(x => new Record(ref x)).ToArray();
            foreach (var e in SortedTiles)
            {
                if (e.OffsetX < Min.X) Min.X = e.OffsetX;
                if (e.OffsetY < Min.Y) Min.Y = e.OffsetY;
                if (e.OffsetX > Max.X) Max.X = e.OffsetX;
                if (e.OffsetY > Max.Y) Max.Y = e.OffsetY;
                if (e.OffsetZ > MaxHeight) MaxHeight = e.OffsetZ;
            }
            ConvertList();
        }

        void ConvertList()
        {
            Center = new Point(-Min.X, -Min.Y);
            Width = (Max.X - Min.X) + 1;
            Height = (Max.Y - Min.Y) + 1;

            // build tiles
            var tiles = new MTileList[Width][];
            Tiles = new MTile[Width][][];
            for (var x = 0; x < Width; ++x)
            {
                tiles[x] = new MTileList[Height];
                Tiles[x] = new MTile[Height][];
                for (var y = 0; y < Height; ++y)
                    tiles[x][y] = new MTileList();
            }
            for (var i = 0; i < SortedTiles.Length; ++i)
            {
                var xOffset = SortedTiles[i].OffsetX + Center.X;
                var yOffset = SortedTiles[i].OffsetY + Center.Y;
                //tiles[xOffset][yOffset].Add(SortedTiles[i].ItemId, (sbyte)SortedTiles[i].OffsetZ,
                //    (sbyte)SortedTiles[i].Flags, SortedTiles[i].Unknown);
            }

            // count surfaces
            Surfaces = 0;
            for (var x = 0; x < Width; ++x)
                for (var y = 0; y < Height; ++y)
                {
                    Tiles[x][y] = tiles[x][y].ToArray();
                    for (var i = 0; i < Tiles[x][y].Length; ++i) Tiles[x][y][i].Solver = i;
                    if (Tiles[x][y].Length > 1) Array.Sort(Tiles[x][y]);
                    if (Tiles[x][y].Length > 0) Surfaces++;
                }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Multi File" }),
                new MetaInfo("Multi", items: new List<MetaInfo> {
                    new MetaInfo($"Min: {Min}"),
                    new MetaInfo($"Max: {Max}"),
                    new MetaInfo($"Center: {Center}"),
                    new MetaInfo($"Width: {Width}"),
                    new MetaInfo($"Height: {Height}"),
                    new MetaInfo($"MaxHeight: {MaxHeight}"),
                    new MetaInfo($"SortedTiles: {SortedTiles.Length}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_RadarColor

    public unsafe class Binary_RadarColor : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_RadarColor(r));

        #region Records

        public uint[] Colors = new uint[0x20000];

        #endregion

        // file: radarcol.mul
        public Binary_RadarColor(BinaryReader r)
        {
            const int multiplier = 0xFF / 0x1F;
            // Prior to 7.0.7.1, all clients have 0x10000 colors. Newer clients have fewer colors.
            var colorCount = (int)r.BaseStream.Length >> 1;
            for (var i = 0; i < colorCount; i++)
            {
                var c = (uint)r.ReadUInt16();
                Colors[i] = 0xFF000000 |
                        ((((c >> 10) & 0x1F) * multiplier)) |
                        ((((c >> 5) & 0x1F) * multiplier) << 8) |
                        (((c & 0x1F) * multiplier) << 16);
            }
            // fill the remainder of the color table with non-transparent magenta.
            for (var i = colorCount; i < Colors.Length; i++) Colors[i] = 0xFFFF00FF;
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Radar Color File" }),
                new MetaInfo("RadarColor", items: new List<MetaInfo> {
                    new MetaInfo($"Colors: {Colors.Length}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_SkillGroups

    public unsafe class Binary_SkillGroups : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SkillGroups(r));

        #region Records


        #endregion

        // file: skillgrp.mul
        public Binary_SkillGroups(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "SkillGroups File" }),
                new MetaInfo("SkillGroups", items: new List<MetaInfo> {
                    //new MetaInfo($"Colors: {Colors.Length}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_Skills

    #endregion

    #region Binary_Sound

    #endregion

    #region Binary_SpeechList

    public unsafe class Binary_SpeechList : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_SpeechList(r));

        #region Records

        public class Record
        {
            public int Index;
            public List<string> Strings = new List<string>();
            //public List<Regex> Regex = new List<Regex>();
            public Record(int index) => Index = index;
        }

        //public void GetSpeechTriggers(string text, string lang, out int count, out int[] triggers)
        //{
        //    var t = new List<int>();
        //    var speechTable = 0;
        //    foreach (var e in Records[speechTable])
        //        for (var i = 0; i < e.Value.Regex.Count; i++)
        //            if (e.Value.Regex[i].IsMatch(text) && !t.Contains(e.Key))
        //                t.Add(e.Key);
        //    count = t.Count;
        //    triggers = t.ToArray();
        //}

        readonly List<Dictionary<int, Record>> Records = new List<Dictionary<int, Record>>();

        #endregion

        // file: speech.mul
        public Binary_SpeechList(BinaryReader r)
        {
            var lastId = -1;
            Dictionary<int, Record> records = null;
            //while (r.PeekChar() >= 0)
            while (r.BaseStream.Length != r.BaseStream.Position)
            {
                var id = r.ReadInt16E(); // (r.ReadByte() << 8) | r.ReadByte();
                var length = r.ReadInt16E(); //(r.ReadByte() << 8) | r.ReadByte();
                if (length > 128) length = 128;

                var text = Encoding.UTF8.GetString(r.ReadBytes(length)).Trim();
                if (text.Length == 0) continue;

                if (records == null || lastId > id)
                {
                    if (id == 0 && text == "*withdraw*") Records.Insert(0, records = new Dictionary<int, Record>());
                    else Records.Add(records = new Dictionary<int, Record>());
                }
                lastId = id;
                records.TryGetValue(id, out var record);
                if (record == null) records[id] = record = new Record(id);
                record.Strings.Add(text);
                //record.Regex.Add(new Regex(text.Replace("*", @".*"), RegexOptions.IgnoreCase));
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "SpeechList File" }),
                new MetaInfo("SpeechList", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_Static

    public unsafe class Binary_Static : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Static(r, (int)f.FileSize));

        #region Records

        byte[] Pixels;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: artLegacyMUL.mul:static/file04000.art
        public Binary_Static(BinaryReader r, int length)
        {
            fixed (byte* _ = r.ReadBytes(length))
            {
                var bdata = (ushort*)_;
                var count = 2;
                var width = Width = bdata[count++];
                var height = Height = bdata[count++];
                if (width <= 0 || height <= 0) return;

                var lookups = new int[height];
                var start = height + 4;
                for (var i = 0; i < height; ++i)
                    lookups[i] = start + bdata[count++];

                var bd = Pixels = new byte[width * height << 1];
                var bd_Stride = width << 1;
                fixed (byte* bd_Scan0 = bd)
                {
                    var line = (ushort*)bd_Scan0;
                    var delta = bd_Stride >> 1;

                    for (var y = 0; y < height; ++y, line += delta)
                    {
                        count = lookups[y];
                        var cur = line;
                        int xOffset, xRun;
                        while ((xOffset = bdata[count++]) + (xRun = bdata[count++]) != 0)
                        {
                            if (xOffset > delta) break;

                            cur += xOffset;
                            if (xOffset + xRun > delta) break;

                            var end = cur + xRun;
                            while (cur < end)
                                *cur++ = (ushort)(bdata[count++] ^ 0x8000);
                        }
                    }
                }
            }
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Pixels;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("Static", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Width}"),
                    new MetaInfo($"Height: {Height}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_StringTable

    public unsafe class Binary_StringTable : IHaveMetaInfo
    {
        public static Dictionary<string, Binary_StringTable> Instances = new Dictionary<string, Binary_StringTable>();
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_StringTable(r, f));

        #region Records

        [Flags]
        public enum RecordFlag : byte
        {
            Original = 0x0,
            Custom = 0x1,
            Modified = 0x2
        }

        public class Record
        {
            public string Text;
            public RecordFlag Flag;
            public Record(string text, RecordFlag flag)
            {
                Text = text;
                Flag = flag;
            }
        }

        public Dictionary<int, string> Strings = new Dictionary<int, string>();
        public Dictionary<int, Record> Records = new Dictionary<int, Record>();

        public static string GetString(int id) => Instances.TryGetValue(".enu", out var y) && y.Strings.TryGetValue(id, out var z) ? z : string.Empty;

        #endregion

        // file: Cliloc.enu
        public Binary_StringTable(BinaryReader r, FileSource f)
        {
            r.Skip(6);
            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                var id = r.ReadInt32();
                var flag = (RecordFlag)r.ReadByte();
                var text = r.ReadL16AString();
                Records[id] = new Record(text, flag);
                Strings[id] = text;
            }
            Instances[Path.GetExtension(f.Path)] = this;
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "StringTable File" }),
                new MetaInfo("StringTable", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_TileData - VERIFY

    public unsafe class Binary_TileData : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_TileData(r));

        #region Records

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct LandV1
        {
            public readonly uint flags;
            public readonly ushort texID;
            public fixed byte name[20];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct LandV2
        {
            public readonly ulong flags;
            public readonly ushort texID;
            public fixed byte name[20];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct ItemV1
        {
            public readonly uint flags;
            public readonly byte weight;
            public readonly byte quality;
            public readonly short miscData;
            public readonly byte unk2;
            public readonly byte quantity;
            public readonly short anim;
            public readonly byte unk3;
            public readonly byte hue;
            public readonly byte stackingOffset;
            public readonly byte value;
            public readonly byte height;
            public fixed byte name[20];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct ItemV2
        {
            public readonly ulong flags;
            public readonly byte weight;
            public readonly byte quality;
            public readonly short miscData;
            public readonly byte unk2;
            public readonly byte quantity;
            public readonly short anim;
            public readonly byte unk3;
            public readonly byte hue;
            public readonly byte stackingOffset;
            public readonly byte value;
            public readonly byte height;
            public fixed byte name[20];
        }

        public struct ItemData
        {
            public int ItemId;
            public int Weight;
            public TileFlag Flags;
            public int Height;
            public int Quality;
            public int Quantity;
            public int AnimID;
            public int Value;
            public string Name;
            public bool IsStairs;
            public byte Unknown1, Unknown2, Unknown3, Unknown4;
            public readonly bool Ignored => ItemId >= 0x0C45 && ItemId <= 0x0DAF;
            public readonly bool IsBackground => (Flags & TileFlag.Background) != 0;
            public readonly bool IsBridge => (Flags & TileFlag.Bridge) != 0;
            public readonly int CalcHeight => Height;
            //public readonly int CalcHeight => ((Flags & TileFlag.Bridge) != 0 ? Height / 2 : Height;
            public readonly bool IsAnimation => (Flags & TileFlag.Animation) != 0;
            public readonly bool IsContainer => (Flags & TileFlag.Container) != 0;
            public readonly bool IsFoliage => (Flags & TileFlag.Foliage) != 0;
            public readonly bool IsGeneric => (Flags & TileFlag.Generic) != 0;
            public readonly bool IsImpassable => (Flags & TileFlag.Impassable) != 0;
            public readonly bool IsLightSource => (Flags & TileFlag.LightSource) != 0;
            public readonly bool IsPartialHue => (Flags & TileFlag.PartialHue) != 0;
            public readonly bool IsRoof => (Flags & TileFlag.Roof) != 0;
            public readonly bool IsDoor => (Flags & TileFlag.Door) != 0;
            public readonly bool IsSurface => (Flags & TileFlag.Surface) != 0;
            public readonly bool IsWall => (Flags & TileFlag.Wall) != 0;
            public readonly bool IsWearable => (Flags & TileFlag.Wearable) != 0;
            public readonly bool IsWet => (Flags & TileFlag.Wet) != 0;
        }

        public struct LandData
        {
            public TileFlag Flags;
            public short TextureID;
            public string Name;
            public readonly bool IsWet => (Flags & TileFlag.Wet) != 0;
            public readonly bool IsImpassible => (Flags & TileFlag.Impassable) != 0;
        }

        // Stairs IDs, taken from RunUO Data folder (stairs.txt)
        static readonly int[] StairsID = {
            1006, 1007, 1008, 1009, 1010, 1012, 1014, 1016, 1017,
            1801, 1802, 1803, 1804, 1805, 1807, 1809, 1811, 1812,
            1822, 1823, 1825, 1826, 1827, 1828, 1829, 1831, 1833,
            1835, 1836, 1846, 1847, 1848, 1849, 1850, 1851, 1852,
            1854, 1856, 1861, 1862, 1865, 1867, 1869, 1872, 1873,
            1874, 1875, 1876, 1878, 1880, 1882, 1883, 1900, 1901,
            1902, 1903, 1904, 1906, 1908, 1910, 1911, 1928, 1929,
            1930, 1931, 1932, 1934, 1936, 1938, 1939, 1955, 1956,
            1957, 1958, 1959, 1961, 1963, 1978, 1979, 1980, 1991,
            7600, 7601, 7602, 7603, 7604, 7605, 7606, 7607, 7608,
            7609, 7610, 7611, 7612, 7613, 7614, 7615, 7616, 7617,
            7618, 7619, 7620, 7621, 7622, 7623, 7624, 7625, 7626,
            7627, 7628, 7629, 7630, 7631, 7632, 7633, 7634, 7635,
            7636, 7639
        };

        public ItemData ItemDataByAnimID(int animID)
        {
            for (var i = 0; i < ItemDatas.Length; i++)
                if (ItemDatas[i].AnimID == animID)
                    return ItemDatas[i];
            return new ItemData();
        }

        public LandData[] LandDatas;
        public ItemData[] ItemDatas;

        #endregion

        // file: tiledata.mul
        public Binary_TileData(BinaryReader r)
        {
            //var useNeW = r.BaseStream.Length == 0x30A800;
            LandData landData;
            ItemData itemData;
            var length = r.BaseStream.Length;
            if (length == 0x30A800) // 7.0.9.0
            {
                LandDatas = new LandData[0x4000];
                for (var i = 0; i < 0x4000; ++i)
                {
                    landData = new LandData();
                    if (i == 1 || (i > 0 && (i & 0x1F) == 0)) r.ReadInt32();
                    var flags = (TileFlag)r.ReadInt64();
                    var textureID = r.ReadInt16();
                    landData.Name = Encoding.ASCII.GetString(r.ReadBytes(20)).Trim('\0');
                    landData.Flags = flags;
                    landData.TextureID = textureID;
                    LandDatas[i] = landData;
                }
                ItemDatas = new ItemData[0x10000];
                for (var i = 0; i < 0x10000; ++i)
                {
                    itemData = new ItemData();
                    itemData.ItemId = i;
                    if ((i & 0x1F) == 0) r.ReadInt32();
                    itemData.Flags = (TileFlag)r.ReadInt64();
                    itemData.Weight = r.ReadByte();
                    itemData.Quality = r.ReadByte();
                    itemData.Unknown1 = r.ReadByte();
                    itemData.Unknown2 = r.ReadByte();
                    itemData.Unknown3 = r.ReadByte();
                    itemData.Quantity = r.ReadByte();
                    itemData.AnimID = r.ReadInt16();
                    r.Skip(2); // hue?
                    itemData.Unknown4 = r.ReadByte();
                    itemData.Value = r.ReadByte();
                    itemData.Height = r.ReadByte();
                    itemData.Name = Encoding.ASCII.GetString(r.ReadBytes(20));
                    itemData.Name = itemData.Name.Trim('\0');
                    if (i > 1005 && i < 7640) itemData.IsStairs = !(Array.BinarySearch(StairsID, i) < 0);
                    ItemDatas[i] = itemData;
                }
            }
            else
            {
                LandDatas = new LandData[0x4000];
                for (var i = 0; i < 0x4000; ++i)
                {
                    landData = new LandData();
                    if ((i & 0x1F) == 0) r.ReadInt32();
                    var flags = (TileFlag)r.ReadInt32();
                    var textureID = r.ReadInt16();
                    landData.Name = Encoding.ASCII.GetString(r.ReadBytes(20)).Trim('\0');
                    //r.BaseStream.Seek(20, SeekOrigin.Current);
                    landData.Flags = flags;
                    landData.TextureID = textureID;
                    LandDatas[i] = landData;
                }
                if (length == 0x191800) // 7.0.0.0
                {
                    ItemDatas = new ItemData[0x8000];
                    for (var i = 0; i < 0x8000; ++i)
                    {
                        itemData = new ItemData();
                        itemData.ItemId = i;
                        if ((i & 0x1F) == 0) r.ReadInt32();
                        itemData.Flags = (TileFlag)r.ReadInt32();
                        itemData.Weight = r.ReadByte();
                        itemData.Quality = r.ReadByte();
                        itemData.Unknown1 = r.ReadByte();
                        itemData.Unknown2 = r.ReadByte();
                        itemData.Unknown3 = r.ReadByte();
                        itemData.Quantity = r.ReadByte();
                        itemData.AnimID = r.ReadInt16();
                        r.Skip(2); // hue?
                        itemData.Unknown4 = r.ReadByte();
                        itemData.Value = r.ReadByte();
                        itemData.Height = r.ReadByte();
                        itemData.Name = Encoding.ASCII.GetString(r.ReadBytes(20)).Trim('\0');
                        if (i > 1005 && i < 7640) itemData.IsStairs = !(Array.BinarySearch(StairsID, i) < 0);
                        ItemDatas[i] = itemData;
                    }
                }
                else
                {
                    ItemDatas = new ItemData[0x4000];
                    for (var i = 0; i < 0x4000; ++i)
                    {
                        itemData = new ItemData();
                        itemData.ItemId = i;
                        if ((i & 0x1F) == 0)
                            r.ReadInt32();
                        itemData.Flags = (TileFlag)r.ReadInt32();
                        itemData.Weight = r.ReadByte();
                        itemData.Quality = r.ReadByte();
                        itemData.Unknown1 = r.ReadByte();
                        itemData.Unknown2 = r.ReadByte();
                        itemData.Unknown3 = r.ReadByte();
                        itemData.Quantity = r.ReadByte();
                        itemData.AnimID = r.ReadInt16();
                        r.Skip(2); // hue?
                        itemData.Unknown4 = r.ReadByte();
                        itemData.Value = r.ReadByte();
                        itemData.Height = r.ReadByte();
                        itemData.Name = Encoding.ASCII.GetString(r.ReadBytes(20)).Trim('\0');
                        if (i > 1005 && i < 7640) itemData.IsStairs = !(Array.BinarySearch(StairsID, i) < 0);
                        ItemDatas[i] = itemData;
                    }
                }
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Tile Data File" }),
                new MetaInfo("TileData", items: new List<MetaInfo> {
                    new MetaInfo($"LandDatas: {LandDatas.Length}"),
                    new MetaInfo($"ItemDatas: {ItemDatas.Length}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_UnicodeFont - TODO

    public unsafe class Binary_UnicodeFont : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_UnicodeFont(r));

        #region Records

        public class UnicodeFont
        {
            public byte[][] Characters = new byte[224][];
            public int Height;

            public UnicodeFont(BinaryReader r)
            {
                r.ReadByte();
                for (var i = 0; i < 224; ++i)
                {
                    var width = r.ReadByte();
                    var height = r.ReadByte();
                    r.ReadByte();
                    if (width <= 0 || height <= 0) continue;

                    if (height > Height && i < 96) Height = height;

                    var bd = new byte[width * height << 1];
                    var bd_Stride = width << 1;
                    fixed (byte* bd_Scan0 = bd)
                    {
                        var line = (ushort*)bd_Scan0;
                        var delta = bd_Stride >> 1;

                        for (var y = 0; y < height; ++y, line += delta)
                        {
                            ushort* cur = line;
                            for (var x = 0; x < width; ++x)
                            {
                                var pixel = (ushort)(r.ReadByte() | (r.ReadByte() << 8));
                                cur[x] = pixel == 0 ? pixel : (ushort)(pixel ^ 0x8000);
                            }
                        }
                    }
                    Characters[i] = bd;
                }
            }
        }

        readonly UnicodeFont Font;

        #endregion

        // file: unifont.mul - unifont12.mul
        public Binary_UnicodeFont(BinaryReader r)
        {
            Font = new UnicodeFont(r);
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "UnicodeFont File" }),
                new MetaInfo("UnicodeFont", items: new List<MetaInfo> {
                    //new MetaInfo($"Fonts: {Fonts.Length}"),
                })
            };
            return nodes;
        }
    }

    #endregion

    #region Binary_Verdata

    public unsafe class Binary_Verdata : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Verdata(r, (BinaryPakFile)s));
        public static Binary_Verdata Instance;

        #region Records

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Patch
        {
            public static (string, int) Struct = ("<5i", sizeof(Patch));
            public int File;
            public int Index;
            public int Offset;
            public int FileSize;
            public int Extra;
        }

        public Stream ReadData(long offset, int fileSize)
            => PakFile.GetReader().Func(r => new MemoryStream(r.Seek(offset).ReadBytes(fileSize)));

        public BinaryPakFile PakFile;
        public IDictionary<int, Patch[]> Patches = new Dictionary<int, Patch[]>();

        #endregion

        // file: verdata.mul
        public Binary_Verdata(BinaryReader r, BinaryPakFile s)
        {
            PakFile = s;
            Patches = r.ReadL32SArray<Patch>().GroupBy(x => x.File).ToDictionary(x => x.Key, x => x.ToArray());
            Instance = this;
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Version Data" }),
                new MetaInfo("Verdata", items: new List<MetaInfo> {
                    new MetaInfo($"Patches: {Patches.Count}"),
                })
            };
            return nodes;
        }
    }

    #endregion
}
