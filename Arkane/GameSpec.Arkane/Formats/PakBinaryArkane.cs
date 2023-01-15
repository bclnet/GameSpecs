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

        PakBinaryArkane() { }

        unsafe class PakFat
        {
            byte[] cKey;
            byte[] fat;
            int pcFAT;
            public int iTailleFAT;
            int iPassKey;

            public PakFat(string key, byte[] fat, int iTailleFAT)
            {
                this.cKey = Encoding.ASCII.GetBytes(key);
                this.fat = fat;
                this.iTailleFAT = iTailleFAT;
            }

            public void CryptChar(byte* _pChar)
            {
                var iTailleKey = cKey.Length;
                var iDecalage = 0;
                *_pChar = (byte)((*_pChar ^ cKey[iPassKey]) >> iDecalage);
                iPassKey++;
                if (iPassKey >= cKey.Length) iPassKey = 0;
            }

            public void UnCryptChar(byte* _pChar)
            {
                var iTailleKey = cKey.Length;
                var iDecalage = 0;
                *_pChar = (byte)((*_pChar ^ cKey[iPassKey]) << iDecalage);
                iPassKey++;
                if (iPassKey >= cKey.Length) iPassKey = 0;
            }

            public void CryptString(byte* _pTxt, int strLength)
            {
                var pTxtCopy = _pTxt;
                var iTaille = strLength + 1;
                while (iTaille-- != 0)
                {
                    CryptChar(pTxtCopy);
                    pTxtCopy++;
                }
            }

            public int UnCryptString(byte* _pTxt)
            {
                var pTxtCopy = _pTxt;
                var iNbChar = 0;
                while (true)
                {
                    UnCryptChar(pTxtCopy);
                    if (*pTxtCopy == 0) break;
                    pTxtCopy++;
                    iNbChar++;
                }
                return iNbChar;
            }

            public void CryptShort(ushort* _pShort)
            {
                var cA = (byte)((*_pShort) & 0xFF);
                var cB = (byte)(((*_pShort) >> 8) & 0xFF);

                CryptChar(&cA);
                CryptChar(&cB);
                *_pShort = (ushort)(cA | (cB << 8));
            }

            public void UnCryptShort(ushort* _pShort)
            {
                var cA = (byte)((*_pShort) & 0xFF);
                var cB = (byte)(((*_pShort) >> 8) & 0xFF);

                UnCryptChar(&cA);
                UnCryptChar(&cB);
                *_pShort = (ushort)(cA | (cB << 8));
            }

            public void CryptInt(uint* _iInt)
            {
                var sA = (ushort)((*_iInt) & 0xFFFF);
                var sB = (ushort)(((*_iInt) >> 16) & 0xFFFF);

                CryptShort(&sA);
                CryptShort(&sB);
                *_iInt = (uint)(sA | (sB << 16));
            }

            public void UnCryptInt(uint* _iInt)
            {
                var sA = (ushort)((*_iInt) & 0xFFFF);
                var sB = (ushort)(((*_iInt) >> 16) & 0xFFFF);

                UnCryptShort(&sA);
                UnCryptShort(&sB);
                *_iInt = (uint)(sA | (sB << 16));
            }

            public int ReadFAT_int()
            {
                var iTailleKey = cKey.Length;
                var iDecalage = 0;
                *_pChar = (byte)((*_pChar ^ cKey[iPassKey]) << iDecalage);
                iPassKey++;
                if (iPassKey >= cKey.Length) iPassKey = 0;

                return 0;
                //int i = *((int*)pcFAT);
                //pcFAT += 4;
                //iTailleFAT -= 4;

                //UnCryptInt((uint*)&i);

                //return i;
            }

            public string ReadFAT_string()
            {
                return null;
                //char* t = pcFAT;
                //int i = UnCryptString((byte*)t) + 1;
                //pcFAT += i;
                //iTailleFAT -= i;

                //return t;
            }
        }

        static byte readByte(byte* b)
        {
            return 0;
        }

        static string readFatString(byte* b)
        {
            var length = 0;
            while (true)
            {
                readByte(b);
                if (*b == 0) break;
                b++;
                length++;
            }
            return new string((char*)b, length);
        }

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());
            var extention = Path.GetExtension(source.FilePath);
            switch (extention)
            {
                case ".pak":
                    {
                        var files = multiSource.Files = new List<FileMetadata>();
                        var key = Encoding.ASCII.GetBytes("AVQF3FCKE50GRIAYXJP2AMEYO5QGA0JGIIH2NHBTVOA1VOGGU5H3GSSIARKPRQPQKKYEOIAQG1XRX0J4F5OEAEFI4DD3LL45VJTVOA1VOGGUKE50GRIAYX");

                        // move to fat table
                        r.Seek(r.ReadUInt32());
                        var fatSize = (int)r.ReadUInt32();
                        var fatBytes = r.ReadBytes(fatSize);
                        fixed (byte* _ = fatBytes)
                        {

                        }

                        //void foo()
                        //{
                        //    var pTxtCopy = _pTxt;
                        //    var iNbChar = 0;
                        //    while (true)
                        //    {
                        //        UnCryptChar(pTxtCopy);
                        //        if (*pTxtCopy == 0) break;
                        //        pTxtCopy++;
                        //        iNbChar++;
                        //    }
                        //}

                        var fat = new PakFat("AVQF3FCKE50GRIAYXJP2AMEYO5QGA0JGIIH2NHBTVOA1VOGGU5H3GSSIARKPRQPQKKYEOIAQG1XRX0J4F5OEAEFI4DD3LL45VJTVOA1VOGGUKE50GRIAYX", fatBytes, fatSize);
                        while (fat.iTailleFAT != 0)
                        {
                            var dirName = fat.ReadFAT_string();
                            var numFiles = fat.ReadFAT_int();
                            while (numFiles-- != 0)
                                files.Add(new FileMetadata
                                {
                                    Path = fat.ReadFAT_string(),
                                    Position = fat.ReadFAT_int(),
                                    Compressed = fat.ReadFAT_int(),
                                    PackedSize = fat.ReadFAT_int(),
                                    FileSize = fat.ReadFAT_int(),
                                });
                        }
                        return Task.CompletedTask;
                    }
                // index games
                case ".index":
                    {
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
            if (file.FileSize == 0 || _badPositions.Contains(file.Position)) return Task.FromResult(System.IO.Stream.Null);
            var (path, tag1, tag2) = ((string, string, string))file.Tag;
            return Task.FromResult((Stream)new MemoryStream(source.GetBinaryReader(path).Func(r2 =>
            {
                r2.Position(file.Position);
                return file.Compressed != 0
                    ? r2.DecompressZlib((int)file.PackedSize, (int)file.FileSize)
                    : r2.ReadBytes((int)file.PackedSize);
            })));
        }
    }
}