using GameX.Formats;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.App
{
    public static class ExportManager
    {
        public static async Task ExportAsync(Family family, Resource resource, string filePath, int from, FileOption option)
        {
            using var pak = family.OpenPakFile(resource);

            // export pak
            if (!(pak is MultiPakFile multiPak))
            {
                await ExportPakAsync(filePath, from, option, pak);
                return;
            }

            // write paks
            if ((option & FileOption.Marker) != 0)
            {
                if (!string.IsNullOrEmpty(filePath) && !Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                var setPath = Path.Combine(filePath, ".set");
                using var w = new BinaryWriter(new FileStream(setPath, FileMode.Create, FileAccess.Write));
                await PakBinary.Stream.Write(new StreamPakFile(HttpHost.Factory, new PakState(null, null, null, "Root"))
                {
                    Files = multiPak.PakFiles.Select(x => new FileSource { Path = x.Name }).ToList()
                }, w, "Set");
            }
            foreach (var _ in multiPak.PakFiles) await ExportPakAsync(filePath, from, option, _);
        }

        static async Task ExportPakAsync(string filePath, int from, FileOption option, PakFile _)
        {
            if (!(_ is BinaryPakFile pak)) throw new InvalidOperationException("pak not a BinaryPakFile");
            var newPath = filePath != null ? Path.Combine(filePath, Path.GetFileName(pak.PakPath)) : null;

            // write pak
            await pak.ExportAsync(newPath, from, option, (file, index) =>
            {
                if ((index % 50) == 0) Console.WriteLine($"{index,6}> {file.Path}");
            }, (file, message) =>
            {
                Console.WriteLine($"ERROR: {message} - {file?.Path}");
            });
        }
    }
}