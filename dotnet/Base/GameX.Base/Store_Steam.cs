using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GameX.Store
{
    /// <summary>
    /// Store_Steam
    /// </summary>
    internal static class Store_Steam
    {
        internal static Dictionary<string, string> SteamPaths = new Dictionary<string, string>();

        static Store_Steam()
        {
            // get dbPath
            var root = GetPath();
            if (root == null) return;

            // query games
            var libraryFolders = AcfStruct.Read(Path.Join(root, "steamapps", "libraryfolders.vdf"));
            foreach (var folder in libraryFolders.Get["libraryfolders"].Get.Values)
            {
                var path = folder.Value["path"];
                if (!Directory.Exists(path)) continue;
                foreach (var appId in folder.Get["apps"].Value.Keys)
                {
                    var appManifest = AcfStruct.Read(Path.Join(path, "steamapps", $"appmanifest_{appId}.acf"));
                    if (appManifest == null) continue;
                    // add appPath if exists
                    var appPath = Path.Join(path, "steamapps", Path.Join("common", appManifest.Get["AppState"].Value["installdir"]));
                    if (Directory.Exists(appPath)) SteamPaths.Add(appId, appPath);
                }
            }
        }

        static string GetPath()
        {
            IEnumerable<string> paths;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // windows paths
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam")
                    ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (key == null) return null;
                return (string)key.GetValue("SteamPath");
            }
            else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // linux paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var search = new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" };
                paths = search.Select(path => Path.Join(home, path, "appcache"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // mac paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var search = new[] { "Library/Application Support/Steam" };
                paths = search.Select(path => Path.Join(home, path, "appcache"));
            }
            else throw new PlatformNotSupportedException();
            return paths.FirstOrDefault(Directory.Exists);
        }

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
                    var firstStart = region.IndexOf('"', index);
                    if (firstStart == -1) break;
                    var firstEnd = region.IndexOf('"', firstStart + 1);
                    index = firstEnd + 1;
                    var first = region.Substring(firstStart + 1, firstEnd - firstStart - 1);
                    int secondStart = region.IndexOf('"', index), secondOpen = region.IndexOf('{', index);
                    if (secondStart == -1)
                        Get.Add(first, null);
                    else if (secondOpen == -1 || secondStart < secondOpen)
                    {
                        var secondEnd = region.IndexOf('"', secondStart + 1);
                        index = secondEnd + 1;
                        var second = region.Substring(secondStart + 1, secondEnd - secondStart - 1);
                        Value.Add(first, second.Replace(@"\\", @"\"));
                    }
                    else
                    {
                        var secondClose = NextEndOf(region, '{', '}', secondOpen + 1);
                        var acfs = new AcfStruct(region.Substring(secondOpen + 1, secondClose - secondOpen - 1));
                        index = secondClose + 1;
                        Get.Add(first, acfs);
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

