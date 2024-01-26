using GameSpec.Formats;
using GameSpec.Meta;
using GameSpec.Origin.Games.UO;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_BodyConv : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_BodyConv(r.ToStream()));

        #region Records

        // Mounts: ItemID , BodyID
        static readonly int[][] MountIDConv = {
            new [] { 0x3E94, 0xF3 }, // Hiryu
            new [] { 0x3E97, 0xC3 }, // Beetle
            new [] { 0x3E98, 0xC2 }, // Swamp Dragon
            new [] { 0x3E9A, 0xC1 }, // Ridgeback
            new [] { 0x3E9B, 0xC0 }, // Unicorn
            new [] { 0x3E9C, 0xBF }, // Ki-Rin
            new [] { 0x3E9E, 0xBE }, // Fire Steed
            new [] { 0x3E9F, 0xC8 }, // Horse
            new [] { 0x3EA0, 0xE2 }, // Grey Horse
            new [] { 0x3EA1, 0xE4 }, // Horse
            new [] { 0x3EA2, 0xCC }, // Brown Horse
            new [] { 0x3EA3, 0xD2 }, // Zostrich
            new [] { 0x3EA4, 0xDA }, // Zostrich
            new [] { 0x3EA5, 0xDB }, // Zostrich
            new [] { 0x3EA6, 0xDC }, // Llama
            new [] { 0x3EA7, 0x74 }, // Nightmare
            new [] { 0x3EA8, 0x75 }, // Silver Steed
            new [] { 0x3EA9, 0x72 }, // Nightmare
            new [] { 0x3EAA, 0x73 }, // Ethereal Horse
            new [] { 0x3EAB, 0xAA }, // Ethereal Llama
            new [] { 0x3EAC, 0xAB }, // Ethereal Zostrich
            new [] { 0x3EAD, 0x84 }, // Ki-Rin
            new [] { 0x3EAF, 0x78 }, // Minax Warhorse
            new [] { 0x3EB0, 0x79 }, // ShadowLords Warhorse
            new [] { 0x3EB1, 0x77 }, // COM Warhorse
            new [] { 0x3EB2, 0x76 }, // TrueBritannian Warhorse
            new [] { 0x3EB3, 0x90 }, // Seahorse
            new [] { 0x3EB4, 0x7A }, // Unicorn
            new [] { 0x3EB5, 0xB1 }, // Nightmare
            new [] { 0x3EB6, 0xB2 }, // Nightmare
            new [] { 0x3EB7, 0xB3 }, // Dark Nightmare
            new [] { 0x3EB8, 0xBC }, // Ridgeback
            new [] { 0x3EBA, 0xBB }, // Ridgeback
            new [] { 0x3EBB, 0x319 }, // Undead Horse
            new [] { 0x3EBC, 0x317 }, // Beetle
            new [] { 0x3EBD, 0x31A }, // Swamp Dragon
            new [] { 0x3EBE, 0x31F }, // Armored Swamp Dragon
            new [] { 0x3F6F, 0x9 }    // Daemon
        };

        static int[] Table1;
        static int[] Table2;
        static int[] Table3;
        static int[] Table4;

        public static int DeathAnimationIndex(Body body)
            => (object)body.Type switch
            {
                BodyType.Human => 21,
                BodyType.Monster => 2,
                BodyType.Animal => 8,
                _ => 2,
            };

        public static int DeathAnimationFrameCount(Body body)
            => (object)body.Type switch
            {
                BodyType.Human => 6,
                BodyType.Monster => 4,
                BodyType.Animal => 4,
                _ => 4,
            };

        /// <summary>
        /// Checks to see if <paramref name="body" /> is contained within the mapping table.
        /// </summary>
        /// <returns>True if it is, false if not.</returns>
        public static bool Contains(int body)
            => Table1 != null && body >= 0 && body < Table1.Length && Table1[body] != -1 ? true
            : Table2 != null && body >= 0 && body < Table2.Length && Table2[body] != -1 ? true
            : Table3 != null && body >= 0 && body < Table3.Length && Table3[body] != -1 ? true
            : Table4 != null && body >= 0 && body < Table4.Length && Table4[body] != -1 ? true
            : false;

        /// <summary>
        /// Attempts to convert <paramref name="body" /> to a body index relative to a file subset, specified by the return value.
        /// </summary>
        /// <returns>A value indicating a file subset:
        /// <list type="table">
        /// <listheader>
        /// <term>Return Value</term>
        /// <description>File Subset</description>
        /// </listheader>
        /// <item>
        /// <term>1</term>
        /// <description>Anim.mul, Anim.idx (Standard)</description>
        /// </item>
        /// <item>
        /// <term>2</term>
        /// <description>Anim2.mul, Anim2.idx (LBR)</description>
        /// </item>
        /// <item>
        /// <term>3</term>
        /// <description>Anim3.mul, Anim3.idx (AOS)</description>
        /// </item>
        /// </list>
        /// </returns>
        public static int Convert(ref int body)
        {
            // Converts MountItemID to BodyID
            if (body > 0x3E93)
                for (var i = 0; i < MountIDConv.Length; ++i)
                {
                    var conv = MountIDConv[i];
                    if (conv[0] == body) { body = conv[1]; break; }
                }
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

        /// <summary>
        /// Returns true if the parameter itemID corresponds to a mountable entity.
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static bool CheckIfItemIsMount(ref int itemID)
        {
            if (itemID > 0x3E93)
                for (var i = 0; i < MountIDConv.Length; ++i)
                {
                    var conv = MountIDConv[i];
                    if (conv[0] == itemID) { itemID = conv[1]; return true; }
                }
            return false;
        }

        #endregion

        // file: Bodyconv.def
        public Binary_BodyConv(StreamReader r)
        {
            ArrayList list1 = new ArrayList(), list2 = new ArrayList(), list3 = new ArrayList(), list4 = new ArrayList();
            int max1 = 0, max2 = 0, max3 = 0, max4 = 0;
            string line;
            while ((line = r.ReadLine()) != null)
            {
                if ((line = line.Trim()).Length == 0 || line.StartsWith("#") || line.StartsWith("\"#")) continue;
                var split = Regex.Split(line, @"\t|\s+", RegexOptions.IgnoreCase);
                int original = System.Convert.ToInt32(split[0]), anim2 = System.Convert.ToInt32(split[1]);
                if (split.Length < 3 || !int.TryParse(split[2], out var anim3)) anim3 = -1;
                if (split.Length < 4 || !int.TryParse(split[3], out var anim4)) anim4 = -1;
                if (split.Length < 5 || !int.TryParse(split[4], out var anim5)) anim5 = -1;
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
            Table1 = new int[max1 + 1];
            Table2 = new int[max2 + 1];
            Table3 = new int[max3 + 1];
            Table4 = new int[max4 + 1];
            for (var i = 0; i < Table1.Length; ++i) Table1[i] = -1;
            for (var i = 0; i < list1.Count; i += 2) Table1[(int)list1[i]] = (int)list1[i + 1];
            for (var i = 0; i < Table2.Length; ++i) Table2[i] = -1;
            for (var i = 0; i < list2.Count; i += 2) Table2[(int)list2[i]] = (int)list2[i + 1];
            for (var i = 0; i < Table3.Length; ++i) Table3[i] = -1;
            for (var i = 0; i < list3.Count; i += 2) Table3[(int)list3[i]] = (int)list3[i + 1];
            for (var i = 0; i < Table4.Length; ++i) Table4[i] = -1;
            for (var i = 0; i < list4.Count; i += 2) Table4[(int)list4[i]] = (int)list4[i + 1];
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Bodyconv config" }),
                new MetaInfo("Bodyconv", items: new List<MetaInfo> {
                    new MetaInfo($"Table1: {Table1.Length}"),
                    new MetaInfo($"Table2: {Table2.Length}"),
                    new MetaInfo($"Table3: {Table3.Length}"),
                    new MetaInfo($"Table4: {Table4.Length}"),
                })
            };
            return nodes;
        }
    }
}
