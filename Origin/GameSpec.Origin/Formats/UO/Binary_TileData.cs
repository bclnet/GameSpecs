using GameSpec.Formats;
using GameSpec.Meta;
using GameSpec.Origin.Games.UO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_TileData : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_TileData(r));

        #region Records

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

        public static ItemData ItemDataByAnimID(int animID)
        {
            for (var i = 0; i < ItemDatas.Length; i++)
                if (ItemDatas[i].AnimID == animID)
                    return ItemDatas[i];
            return new ItemData();
        }

        public static LandData[] LandDatas;
        public static ItemData[] ItemDatas;

        #endregion

        // file: tiledata.mul
        public Binary_TileData(BinaryReader r)
        {
            LandData landData;
            ItemData itemData;
            var length = r.BaseStream.Length;
            if (length == 3188736) // 7.0.9.0
            {
                LandDatas = new LandData[0x4000];
                for (var i = 0; i < 0x4000; ++i)
                {
                    landData = new LandData();
                    if (i == 1 || (i > 0 && (i & 0x1F) == 0)) r.ReadInt32();
                    var flags = (TileFlag)r.ReadInt64();
                    var textureID = r.ReadInt16();
                    landData.Name = Encoding.ASCII.GetString(r.ReadBytes(20));
                    landData.Name = landData.Name.Trim('\0');
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
                    landData.Name = Encoding.ASCII.GetString(r.ReadBytes(20));
                    landData.Name = landData.Name.Trim('\0');
                    //r.BaseStream.Seek(20, SeekOrigin.Current);
                    landData.Flags = flags;
                    landData.TextureID = textureID;
                    LandDatas[i] = landData;
                }
                if (length == 1644544) // 7.0.0.0
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
                        itemData.Name = Encoding.ASCII.GetString(r.ReadBytes(20));
                        itemData.Name = itemData.Name.Trim('\0');
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
                        itemData.Name = Encoding.ASCII.GetString(r.ReadBytes(20));
                        itemData.Name = itemData.Name.Trim('\0');
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
}
