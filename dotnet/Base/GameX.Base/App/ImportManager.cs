using GameX.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.App
{
    public static class ImportManager
    {
        public static async Task ImportAsync(Family family, Resource resource, string filePath, int from, FileOption option)
        {
            //foreach (var path in resource.Paths)
            //{
            //    using var pak = family.OpenPakFile(resource.Game, new[] { path }) as BinaryPakFile;
            //    if (pak == null) throw new InvalidOperationException("Pak not a BinaryPakFile");

            //    // import pak
            //    var w = await ImportPakAsync(filePath, from, path, option, pak);
            //}
        }

        static async Task<BinaryWriter> ImportPakAsync(string filePath, int from, string path, FileOption option, BinaryPakFile pak)
        {
            // import pak
            var w = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
            await pak.ImportAsync(w, filePath, from, option, (file, index) =>
            {
                //if ((index % 50) == 0)
                //    Console.WriteLine($"{file.Path}");
            }, (file, message) =>
            {
                Console.WriteLine($"{message}: {file?.Path}");
            });
            return w;
        }
    }
}