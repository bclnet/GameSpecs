using Microsoft.Win32;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;

namespace GameSpec.Base.FileManagers
{
    /// <summary>
    /// SteamStoreManager
    /// </summary>
    internal static class SteamStoreManager
    {
        static Dictionary<string, string> AppPaths = new();

        static SteamStoreManager()
        {
            var root = GetPath();
            if (root == null) return;
            var libraryFolders = AcfStruct.Read(Path.Join(root, "steamapps", "libraryfolders.vdf"));
            foreach (var folder in libraryFolders.Get["libraryfolders"].Get.Values)
            {
                var path = folder.Value["path"];
                if (!Directory.Exists(path)) { continue; }
                foreach (var appId in folder.Get["apps"].Value.Keys)
                {
                    var appManifest = AcfStruct.Read(Path.Join(path, "steamapps", $"appmanifest_{appId}.acf"));
                    if (appManifest == null) { continue; }
                    var appPath = Path.Join(path, "steamapps", "common", appManifest.Get["AppState"].Value["installdir"]);
                    if (Directory.Exists(appPath)) AppPaths.Add(appId, appPath);
                }
            }
        }

        public static bool TryGetPathByKey(string key, JsonProperty prop, JsonElement? keyElem, out string path)
            => AppPaths.TryGetValue(key, out path);

        static string GetPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam") ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (key != null && key.GetValue("SteamPath") is string path) return path;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var paths = new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" };
                return paths
                    .Select(path => Path.Join(home, path))
                    .FirstOrDefault(path => Directory.Exists(Path.Join(path, "appcache")));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var paths = new[] { "Library/Application Support/Steam" };
                return paths
                    .Select(path => Path.Join(home, path))
                    .FirstOrDefault(path => Directory.Exists(Path.Join(path, "appcache")));
            }
            throw new PlatformNotSupportedException();
        }

        #region steamapp

        public class AcfStruct
        {
            public static AcfStruct Read(string path) => File.Exists(path) ? new(File.ReadAllText(path)) : null;
            public Dictionary<string, AcfStruct> Get = new();
            public Dictionary<string, string> Value = new();

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

