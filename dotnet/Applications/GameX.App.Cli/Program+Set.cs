using CommandLine;
using System;
using System.Threading.Tasks;

namespace GameX.App.Cli
{
    partial class Program
    {
        [Verb("import", HelpText = "Insert files contents to pak.")]
        class ImportOptions
        {
            [Option('f', "family", Required = true, HelpText = "Family")]
            public string Family { get; set; }

            [Option('u', "uri", Required = true, HelpText = "Pak file to be created")]
            public Uri Uri { get; set; }

            [Option("path", Default = @".\out", HelpText = "Insert folder")]
            public string Path { get; set; }

            [Option("option", Default = 0, HelpText = "Data option")]
            public FileOption Option { get; set; }
        }

        static async Task<int> RunImportAsync(ImportOptions args)
        {
            var from = ProgramState.Load(data => Convert.ToInt32(data), 0);

            // get family
            var family = FamilyManager.GetFamily(args.Family);
            if (family == null) { Console.WriteLine($"No family found named \"{args.Family}\"."); return 0; }

            // import
            await ImportManager.ImportAsync(family, family.ParseResource(args.Uri), GetPlatformPath(args.Path), from, args.Option);
            
            ProgramState.Clear();
            return 0;
        }
    }
}