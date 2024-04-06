using GameX.Formats;
using GameX.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Origin.Formats.UO
{
    public unsafe class Binary_Font_DEL : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Font_DEL(r, s));

        #region Records

        public abstract class Character
        {
            public bool HuePassedColor;
            public uint[] Pixels;
            public int Width;
            public int Height;
            public int ExtraWidth;
            public int XOffset;
            public int YOffset;
        }

        public abstract class Font
        {
            public int Baseline;
            public int Height;
            public abstract Character GetCharacter(char ch);
            public bool HasBuiltInOutline;
            public int GetWidth(char ch) => GetCharacter(ch).Width;
            public int GetWidth(string text)
            {
                if (text == null || text.Length == 0) return 0;
                var width = 0;
                for (var i = 0; i < text.Length; ++i)
                    width += GetCharacter(text[i]).Width;
                return width;
            }
        }

        readonly static AsciiFont[] AsciiFonts = new AsciiFont[10];
        readonly static UnicodeFont[] UnicodeFonts = new UnicodeFont[3];

        public Font GetUniFont(int index) => UnicodeFonts[index < 0 || index >= UnicodeFonts.Length ? 0 : index];
        public Font GetAsciiFont(int index) => AsciiFonts[index < 0 || index >= AsciiFonts.Length ? 9 : index];

        #endregion

        #region Characters

        class AsciiCharacter : Character
        {
            public AsciiCharacter(BinaryReader r)
            {
                if (r == null) return;
                Width = r.ReadByte();
                Height = r.ReadByte();
                HuePassedColor = true;
                r.ReadByte();
                var startY = Height;
                var endY = -1;
                uint[] pixels = null;
                if (Width > 0 && Height > 0)
                {
                    pixels = new uint[Width * Height];
                    var i = 0;
                    for (var y = 0; y < Height; y++)
                    {
                        var rowHasData = false;
                        for (var x = 0; x < Width; x++)
                        {
                            var pixel = (ushort)(r.ReadByte() | (r.ReadByte() << 8));
                            if (pixel != 0)
                            {
                                pixels[i] = (uint)(0xFF000000 + (
                                    ((((pixel >> 10) & 0x1F) * 0xFF / 0x1F)) |
                                    ((((pixel >> 5) & 0x1F) * 0xFF / 0x1F) << 8) |
                                    (((pixel & 0x1F) * 0xFF / 0x1F) << 16)
                                    ));
                                rowHasData = true;
                            }
                            i++;
                        }
                        if (rowHasData)
                        {
                            if (startY > y) startY = y;
                            endY = y;
                        }
                    }
                }

                endY += 1;
                if (endY == 0) Pixels = null;
                else if (endY == Height) Pixels = pixels;
                else
                {
                    Pixels = new uint[Width * endY];
                    var i = 0;
                    for (var y = 0; y < endY; y++)
                        for (var x = 0; x < Width; x++)
                            Pixels[i++] = pixels[y * Width + x];
                    YOffset = Height - endY;
                    Height = endY;
                }
            }
        }

        class UnicodeCharacter : Character
        {
            public UnicodeCharacter(BinaryReader r)
            {
                if (r == null) return;
                XOffset = r.ReadSByte();
                YOffset = r.ReadSByte();
                Width = r.ReadByte();
                Height = r.ReadByte();
                ExtraWidth = 1;
                if (Width > 0 && Height > 0)
                {
                    Pixels = new uint[Width * Height];
                    for (var y = 0; y < Height; ++y)
                    {
                        var scanline = r.ReadBytes(((Width - 1) / 8) + 1);
                        int bitX = 7, byteX = 0;
                        for (var x = 0; x < Width; ++x)
                        {
                            var color = 0x00000000U;
                            if ((scanline[byteX] & (byte)Math.Pow(2, bitX)) != 0) color = 0xFFFFFFFF;
                            Pixels[y * Width + x] = color;
                            bitX--;
                            if (bitX < 0) { bitX = 7; byteX++; }
                        }
                    }
                }
            }
        }

        #endregion

        #region Fonts

        public class AsciiFont : Font
        {
            static readonly AsciiCharacter NullCharacter = new AsciiCharacter(null);
            readonly AsciiCharacter[] Characters = new AsciiCharacter[224];

            public AsciiFont(BinaryReader r)
            {
                HasBuiltInOutline = true;
                r.ReadByte();
                // space characters have no data in AFont files.
                Characters[0] = NullCharacter;
                // We load all 224 characters; this seeds the font with correct height values.
                for (var i = 0; i < 224; i++)
                {
                    var ch = new AsciiCharacter(r);
                    var height = ch.Height;
                    if (i > 32 && i < 90 && height > Height) Height = height;
                    Characters[i] = ch;
                }
                for (var i = 0; i < 224; i++)
                    Characters[i].YOffset = Height - (Characters[i].Height + Characters[i].YOffset);
                Height -= 2; // ascii fonts are so tall! why?
                // Determine the width of the space character - arbitrarily .333 the width of capital M (.333 em?).
                GetCharacter(' ').Width = GetCharacter('M').Width / 3;
            }

            public override Character GetCharacter(char character)
            {
                var index = (character & 0xFFFFF) - 0x20;
                if (index < 0) return NullCharacter;
                if (index >= Characters.Length) return NullCharacter;
                return Characters[index];
            }
        }

        public class UnicodeFont : Font
        {
            static readonly UnicodeCharacter NullCharacter = new UnicodeCharacter(null);
            UnicodeCharacter[] Characters = new UnicodeCharacter[224];
            BinaryReader _r;

            public UnicodeFont(BinaryReader r)
            {
                _r = r;
                // space characters have no data in UniFont files.
                Characters[0] = NullCharacter;
                // We load the first 96 characters to 'seed' the font with correct height values.
                for (var i = 33; i < 128; i++)
                    GetCharacter((char)i);
                // Determine the width of the space character - arbitrarily .333 the width of capital M (.333 em?).
                GetCharacter(' ').Width = GetCharacter('M').Width / 3;
            }

            public override Character GetCharacter(char character)
            {
                var index = (character & 0xFFFFF) - 0x20;
                if (index < 0) return NullCharacter;
                if (Characters[index] == null)
                {
                    var ch = NewCharacter(index + 0x20);
                    var height = ch.Height + ch.YOffset;
                    if (index < 128 && height > Height) Height = height;
                    Characters[index] = ch;
                }
                return Characters[index];
            }

            UnicodeCharacter NewCharacter(int index)
            {
                // get the lookup table - 0x10000 ints.
                _r.BaseStream.Position = index * 4;
                var lookup = _r.ReadInt32();
                if (lookup == 0) return NullCharacter; // no character - so we just return null
                _r.BaseStream.Position = lookup;
                return new UnicodeCharacter(_r);
            }
        }

        #endregion

        // file: fonts.mul, unifont?.mul
        public Binary_Font_DEL(BinaryReader r, PakFile s)
        {
            for (var i = 0; i < AsciiFonts.Length; i++)
                AsciiFonts[i] = new AsciiFont(r);
            // load Unicode fonts
            var maxHeight = 0; // because all unifonts are designed to be used together, they must all share a single maxheight value.
            for (var i = 0; i < UnicodeFonts.Length; i++)
            {
                var stream = s.LoadFileData($"unifont{(i == 0 ? string.Empty : i.ToString())}.mul").Result;
                if (stream != null)
                {
                    UnicodeFonts[i] = new UnicodeFont(new BinaryReader(stream));
                    if (UnicodeFonts[i].Height > maxHeight) maxHeight = UnicodeFonts[i].Height;
                }
            }
            for (var i = 0; i < UnicodeFonts.Length; i++)
            {
                if (UnicodeFonts[i] == null) continue;
                UnicodeFonts[i].Height = maxHeight;
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Font File(s)" }),
                new MetaInfo("Font", items: new List<MetaInfo> {
                    new MetaInfo($"AsciiFonts: {AsciiFonts.Length}"),
                    new MetaInfo($"UnicodeFonts: {UnicodeFonts.Length}"),
                })
            };
            return nodes;
        }
    }
}
