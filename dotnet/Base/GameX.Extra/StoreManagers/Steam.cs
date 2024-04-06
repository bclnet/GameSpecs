using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ValveKeyValue;

namespace GameX.StoreManagers
{
    /// <summary>
    /// Steam
    /// NuGet: ValveKeyValue
    /// </summary>
    internal class Steam
    {
        #region appcache:app

        public enum EUniverse
        {
            Invalid = 0,
            Public = 1,
            Beta = 2,
            Internal = 3,
            Dev = 4,
            Max = 5,
        }

        public class App
        {
            public uint AppId;
            public uint InfoState;
            public DateTime LastUpdated;
            public ulong Token;
            public byte[] Hash;
            public byte[] BinaryDataHash;
            public uint ChangeNumber;
            public KVObject Data;
        }

        public class AppInfo
        {
            const uint Magic28 = 0x07564428;
            const uint Magic27 = 0x07564427;
            public EUniverse Universe;
            public List<App> Apps = new List<App>();

            /// <summary>
            /// Opens and reads the given filename.
            /// </summary>
            /// <param name="filename">The file to open and read.</param>
            public AppInfo(string filename)
            {
                using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                Read(fs);
            }

            /// <summary>
            /// Reads the given <see cref="Stream"/>.
            /// </summary>
            /// <param name="input">The input <see cref="Stream"/> to read from.</param>
            public void Read(Stream input)
            {
                using var r = new BinaryReader(input);
                var magic = r.ReadUInt32();
                if (magic != Magic27 && magic != Magic28) throw new InvalidDataException($"Unknown magic header: {magic:X}");
                Universe = (EUniverse)r.ReadUInt32();
                var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);
                do
                {
                    var appId = r.ReadUInt32();
                    if (appId == 0) break;
                    r.ReadUInt32(); // size until end of Data
                    var app = new App
                    {
                        AppId = appId,
                        InfoState = r.ReadUInt32(),
                        LastUpdated = DateTimeFromUnixTime(r.ReadUInt32()),
                        Token = r.ReadUInt64(),
                        Hash = r.ReadBytes(20),
                        ChangeNumber = r.ReadUInt32(),
                    };
                    if (magic == Magic28) app.BinaryDataHash = r.ReadBytes(20);
                    app.Data = deserializer.Deserialize(input);
                    Apps.Add(app);
                } while (true);
            }

            public static DateTime DateTimeFromUnixTime(uint unixTime)
                => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }

        #endregion

        #region appcache:package

        public class Package
        {
            public uint SubId;
            public byte[] Hash;
            public uint ChangeNumber;
            public ulong Token;
            public KVObject Data;
        }

        public class PackageInfo
        {
            const uint Magic28 = 0x06565528;
            const uint Magic27 = 0x06565527;
            public EUniverse Universe;
            public List<Package> Packages = new List<Package>();

            /// <summary>
            /// Opens and reads the given filename.
            /// </summary>
            /// <param name="filename">The file to open and read.</param>
            public PackageInfo(string filename)
            {
                using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                Read(fs);
            }

            /// <summary>
            /// Reads the given <see cref="Stream"/>.
            /// </summary>
            /// <param name="input">The input <see cref="Stream"/> to read from.</param>
            public void Read(Stream input)
            {
                using var r = new BinaryReader(input);
                var magic = r.ReadUInt32();
                if (magic != Magic28 && magic != Magic27) throw new InvalidDataException($"Unknown magic header: {magic:X}");
                Universe = (EUniverse)r.ReadUInt32();
                var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);
                do
                {
                    var subid = r.ReadUInt32();
                    if (subid == 0xFFFFFFFF) break;
                    var package = new Package
                    {
                        SubId = subid,
                        Hash = r.ReadBytes(20),
                        ChangeNumber = r.ReadUInt32(),
                    };
                    if (magic != Magic27) package.Token = r.ReadUInt64();
                    package.Data = deserializer.Deserialize(input);
                    Packages.Add(package);
                } while (true);
            }
        }

        #endregion

        #region steamapp

        public class AcfStruct
        {
            public static AcfStruct Read(string path) => File.Exists(path) ? new AcfStruct(File.ReadAllText(path)) : null;
            public Dictionary<string, AcfStruct> Get = new Dictionary<string, AcfStruct>();
            public Dictionary<string, string> Value = new Dictionary<string, string>();

            public AcfStruct(string region)
            {
                int lengthOfRegion = region.Length, index = 0;
                while (lengthOfRegion > index)
                {
                    var firstItemStart = region.IndexOf('"', index);
                    if (firstItemStart == -1) break;
                    var firstItemEnd = region.IndexOf('"', firstItemStart + 1);
                    index = firstItemEnd + 1;
                    var firstItem = region.Substring(firstItemStart + 1, firstItemEnd - firstItemStart - 1);
                    int secondItemStartQuote = region.IndexOf('"', index), secondItemStartBraceleft = region.IndexOf('{', index);
                    if (secondItemStartBraceleft == -1 || secondItemStartQuote < secondItemStartBraceleft)
                    {
                        var secondItemEndQuote = region.IndexOf('"', secondItemStartQuote + 1);
                        var secondItem = region.Substring(secondItemStartQuote + 1, secondItemEndQuote - secondItemStartQuote - 1);
                        index = secondItemEndQuote + 1;
                        Value.Add(firstItem, secondItem.Replace(@"\\", @"\"));
                    }
                    else
                    {
                        var secondItemEndBraceright = NextEndOf(region, '{', '}', secondItemStartBraceleft + 1);
                        var acfs = new AcfStruct(region.Substring(secondItemStartBraceleft + 1, secondItemEndBraceright - secondItemStartBraceleft - 1));
                        index = secondItemEndBraceright + 1;
                        Get.Add(firstItem, acfs);
                    }
                }
            }

            static int NextEndOf(string str, char open, char close, int startIndex)
            {
                if (open == close) throw new Exception("\"Open\" and \"Close\" char are equivalent!");
                int openItem = 0, closeItem = 0;
                for (var i = startIndex; i < str.Length; i++)
                {
                    if (str[i] == open) openItem++;
                    if (str[i] == close) { closeItem++; if (closeItem > openItem) return i; }
                }
                throw new Exception("Not enough closing characters!");
            }

            public override string ToString() => ToString(0);
            public string ToString(int depth)
            {
                var b = new StringBuilder();
                foreach (var item in Value)
                {
                    b.Append('\t', depth);
                    b.AppendFormat("\"{0}\"\t\t\"{1}\"\r\n", item.Key, item.Value);
                }
                foreach (var item in Get)
                {
                    b.Append('\t', depth);
                    b.AppendFormat("\"{0}\"\n", item.Key);
                    b.Append('\t', depth);
                    b.AppendLine("{");
                    b.Append(item.Value.ToString(depth + 1));
                    b.Append('\t', depth);
                    b.AppendLine("}");
                }
                return b.ToString();
            }
        }

        #endregion
    }
}

//using static GameX.StoreManagers.Steam;
//static SteamStoreManager()
//{
//    var steamLocation = GetSteamPath();
//    if (steamLocation == null) return;
//    //var appInfo = new AppInfo(Path.Join(steamLocation, "appcache", "appinfo.vdf"));
//    //var packageInfo = new PackageInfo(Path.Join(steamLocation, "appcache", "packageinfo.vdf"));
//    var libraryFolders = AcfStruct.Read(Path.Join(steamLocation, "steamapps", "libraryfolders.vdf"));
//}