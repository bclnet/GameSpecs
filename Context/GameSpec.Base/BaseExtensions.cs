using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GameSpec
{
    public static class BaseExtensions
    {
        public static string[] GetStringOrArray(this JsonElement source)
            => source.ValueKind switch
            {
                JsonValueKind.Number => new[] { source.GetInt32().ToString() },
                JsonValueKind.String => new[] { source.GetString() },
                JsonValueKind.Array => source.EnumerateArray().Select(y => y.GetString()).ToArray(),
                _ => throw new ArgumentOutOfRangeException($"{source}"),
            };

        public static T[] GetStringOrArray<T>(this JsonElement source, Func<string, T> func)
            => source.ValueKind switch
            {
                JsonValueKind.Number => new[] { func(source.GetInt32().ToString()) },
                JsonValueKind.String => new[] { func(source.GetString()) },
                JsonValueKind.Array => source.EnumerateArray().Select(y => func(y.GetString())).ToArray(),
                _ => throw new ArgumentOutOfRangeException($"{source}"),
            };

        internal static IEnumerable<string> FindPaths(this IFileSystem fileSystem, string path, string searchPattern)
        {
            // expand
            int expandStartIdx, expandMidIdx, expandEndIdx;
            if ((expandStartIdx = searchPattern.IndexOf('(')) != -1 &&
                (expandMidIdx = searchPattern.IndexOf(':', expandStartIdx)) != -1 &&
                (expandEndIdx = searchPattern.IndexOf(')', expandMidIdx)) != -1 &&
                expandStartIdx < expandEndIdx)
            {
                foreach (var expand in searchPattern.Substring(expandStartIdx + 1, expandEndIdx - expandStartIdx - 1).Split(':'))
                    foreach (var found in FindPaths(fileSystem, path, searchPattern.Remove(expandStartIdx, expandEndIdx - expandStartIdx + 1).Insert(expandStartIdx, expand)))
                        yield return found;
                yield break;
            }
            foreach (var file in fileSystem.Glob(path, searchPattern)) yield return file;
            //// folder
            //var directoryPattern = Path.GetDirectoryName(searchPattern);
            //if (directoryPattern.Contains('*'))
            //{
            //    foreach (var directory in fileSystem.GetDirectories(path, directoryPattern, directoryPattern.Contains("**")))
            //        foreach (var found in fileSystem.FindPaths(directory, Path.GetFileName(directoryPattern)))
            //            yield return found;
            //    searchPattern = Path.GetFileName(searchPattern);
            //}
            //// file
            //if (!searchPattern.Contains('*')) yield return fileSystem.GetFile(Path.Combine(path, searchPattern));
            //else foreach (var file in fileSystem.GetFiles(path, searchPattern)) yield return file;
        }
    }
}
