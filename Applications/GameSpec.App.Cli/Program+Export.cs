using CommandLine;
using System;
using System.Threading.Tasks;

namespace GameSpec.App.Cli
{
    partial class Program
    {
        [Verb("export", HelpText = "Extract files contents to folder.")]
        class ExportOptions
        {
            [Option('f', "family", Required = true, HelpText = "Family")]
            public string Family { get; set; }

            [Option('u', "uri", Required = true, HelpText = "Pak file to be extracted")]
            public Uri Uri { get; set; }

            [Option("path", Default = @".\out", HelpText = "Output folder")]
            public string Path { get; set; }

            [Option("option", Default = DataOption.Stream | DataOption.Model, HelpText = "Data option")]
            public DataOption Option { get; set; }
        }

        static async Task<int> RunExportAsync(ExportOptions opts)
        {
            var from = ProgramState.Load(data => Convert.ToInt32(data), 0);
            var family = FamilyManager.GetFamily(opts.Family);
            await ExportManager.ExportAsync(family, family.ParseResource(opts.Uri), opts.Path, from, opts.Option);
            ProgramState.Clear();
            return 0;
        }
    }
}