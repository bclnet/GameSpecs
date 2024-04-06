using CommandLine;
using System;
using System.Threading.Tasks;

namespace GameX.App.Cli
{
    partial class Program
    {
        [Verb("get", HelpText = "Get files contents to folder.")]
        class GetOptions
        {
            [Option('f', "family", Required = true, HelpText = "Family")]
            public string Family { get; set; }

            [Option('u', "uri", Required = true, HelpText = "Pak file to be extracted")]
            public Uri Uri { get; set; }

            [Option("path", Default = @".\out", HelpText = "Output folder")]
            public string Path { get; set; }

            [Option("option", Default = FileOption.Stream | FileOption.Model, HelpText = "Data option")]
            public FileOption Option { get; set; }
        }

        static async Task<int> RunGetAsync(GetOptions args)
        {
            var from = ProgramState.Load(data => Convert.ToInt32(data), 0);

            // get family
            var family = FamilyManager.GetFamily(args.Family);
            if (family == null) { Console.WriteLine($"No family found named \"{args.Family}\"."); return 0; }

            // export
            await ExportManager.ExportAsync(family, family.ParseResource(args.Uri), GetPlatformPath(args.Path), from, args.Option);

            ProgramState.Clear();
            return 0;
        }
    }
}