using CommandLine;
using GameSpec.Formats.Collada;
using GameSpec.Formats.Unknown;
using GameSpec.Formats.Wavefront;
using System;
using System.IO;

namespace GameSpec.App.Cli
{
    // test
    // list -e "Tes"
    // import -e "Tes" -u "game:/Oblivion*.bsa#Oblivion" --path "D:\T_\Test2"
    // export -e "Tes" -u "game:/Oblivion*.bsa#Oblivion" --path "D:\T_\Test2"
    // xsport -e "Tes" -u "game:/Oblivion*.bsa#Oblivion" --path "D:\T_\Test2"
    partial class Program
    {
        #region ProgramState

        static class ProgramState
        {
            public static T Load<T>(Func<byte[], T> action, T defaultValue)
            {
                try
                {
                    if (File.Exists(@".\lastChunk.txt"))
                    {
                        using var s = File.Open(@".\lastChunk.txt", FileMode.Open);
                        var data = new byte[s.Length];
                        s.Read(data, 0, (int)s.Length);
                        return action(data);
                    }
                }
                catch { }
                return defaultValue;
            }

            public static void Store(Func<byte[]> action)
            {
                try
                {
                    var data = action();
                    using var s = new FileStream(@".\lastChunk.txt", FileMode.Create, FileAccess.Write);
                    s.Write(data, 0, data.Length);
                }
                catch { Clear(); }
            }

            public static void Clear()
            {
                try
                {
                    if (File.Exists(@".\lastChunk.txt")) File.Delete(@".\lastChunk.txt");
                }
                catch { }
            }
        }

        #endregion

        static string[] test00 = new[] { "test" };

        static string[] args00 = new[] { "list" };
        static string[] args01 = new[] { "list", "-f", "Red" };
        static string[] args02 = new[] { "list", "-f", "Tes", "-u", "game:/Oblivion*.bsa#Oblivion" };
        static string[] args03 = new[] { "list", "-f", "Tes", "-u", "file:///D:/T_/Oblivion/Oblivion*.bsa#Oblivion" };

        static string[] dev00 = new[] { "list", "-f", "Unity", "-u", "game:/resources.assets#Cities" };

        static string[] dev01a = new[] { "list", "-f", "Unity", "-u", @"game:/StreamingAssets\aa\Steam\StandaloneWindows\initialmaps_assets_all.bundle#AmongUs" };
        static string[] dev01b = new[] { "list", "-f", "Unity", "-u", @"game:/resources.assets#AmongUs" };
        static string[] dev01c = new[] { "list", "-f", "Unity", "-u", @"game:/globalgamemanagers.assets#AmongUs" };

        static string[] dev02a = new[] { "list", "-f", "Unity", "-u", @"game:/resources.assets#Cities" };
        static string[] dev02b = new[] { "list", "-f", "Unity", "-u", @"game:/resources.assets#Cities" };
        static string[] dev02c = new[] { "list", "-f", "Unity", "-u", @"game:/globalgamemanagers.assets#Cities" };


        static string[] argsRsi1 = new[] { "export", "-f", "Rsi", "-u", "game:/Data.p4k#StarCitizen", "--path", @"D:\T_\StarCitizen" };

        static string[] argsTes1 = new[] { "export", "-f", "Tes", "-u", "game:/Oblivion*.bsa#Oblivion", "--path", @"D:\T_\Oblivion" };
        static string[] argsTes2 = new[] { "import", "-f", "Tes", "-u", "game:/Oblivion*.bsa#Oblivion", "--path", @"D:\T_\Oblivion" };
        //
        static string[] argsRed1 = new[] { "export", "-f", "Red", "-u", "game:/main.key#Witcher", "--path", @"D:\T_\Witcher" };
        static string[] argsRed2 = new[] { "export", "-f", "Red", "-u", "game:/krbr.dzip#Witcher2", "--path", @"D:\T_\Witcher2" };

        static void Register()
        {
            UnknownFileWriter.Factories["Collada"] = file => new ColladaFileWriter(file);
            UnknownFileWriter.Factories["Wavefront"] = file => new WavefrontFileWriter(file);
            UnknownFileWriter.Factories["default"] = UnknownFileWriter.Factories["Wavefront"];
        }

        static void Main(string[] args)
        {
            Register();
            Parser.Default.ParseArguments<TestOptions, ListOptions, ExportOptions, ImportOptions>(dev01c)
            .MapResult(
                (TestOptions opts) => RunTestAsync(opts).GetAwaiter().GetResult(),
                (ListOptions opts) => RunListAsync(opts).GetAwaiter().GetResult(),
                (ExportOptions opts) => RunExportAsync(opts).GetAwaiter().GetResult(),
                (ImportOptions opts) => RunImportAsync(opts).GetAwaiter().GetResult(),
                errs => 1);
        }
    }
}