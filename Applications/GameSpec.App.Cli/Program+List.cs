using CommandLine;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpec.App.Cli
{
    partial class Program
    {
        [Verb("list", HelpText = "Extract files contents to folder.")]
        class ListOptions
        {
            [Option('f', "family", HelpText = "Family")]
            public string Family { get; set; }

            [Option('u', "uri", HelpText = "Pak file to be extracted")]
            public Uri Uri { get; set; }
        }

        static Task<int> RunListAsync(ListOptions opts)
        {
            // list families
            if (string.IsNullOrEmpty(opts.Family))
            {
                Console.WriteLine("Families installed:\n");
                foreach (var _ in FamilyManager.Families) Console.WriteLine($"{_.Key} - {_.Value.Name}");
                return Task.FromResult(0);
            }

            // get estate
            var family = FamilyManager.GetFamily(opts.Family, false);
            if (family == null) { Console.WriteLine($"No Estate found named {opts.Family}."); return Task.FromResult(0); }

            // list found locations in estate
            if (opts.Uri == null)
            {
                Console.WriteLine($"{family.Name}\nDescription: {family.Description}\nStudio: {family.Studio}");
                Console.WriteLine($"\nGames:");
                foreach (var game in family.Games.Values) Console.WriteLine($"{game.Name}{(game.Found ? $" -> {string.Join(',', (IEnumerable<Uri>)game.Paks)}" : null)}");
                Console.WriteLine("\nLocations:");
                var paths = family.FileManager.Paths;
                if (paths.Count == 0) { Console.WriteLine($"No locations found for estate {opts.Family}."); return Task.FromResult(0); }
                foreach (var path in paths) Console.WriteLine($"{family.GetGame(path.Key)} - {string.Join(", ", path.Value)}");
                return Task.FromResult(0);
            }

            // list files in pack for estate
            else
            {
                Console.WriteLine($"{family.Name} - {opts.Uri}\n");
                //if (estate.OpenPakFile(estate.ParseResource(opts.Uri)) is not MultiPakFile multiPak) throw new InvalidOperationException("multiPak not a MultiPakFile");
                using var multiPak = family.OpenPakFile(family.ParseResource(opts.Uri)) as MultiPakFile ?? throw new InvalidOperationException("multiPak not a MultiPakFile");
                if (multiPak.PakFiles.Count == 0) { Console.WriteLine("No paks found."); return Task.FromResult(0); }
                Console.WriteLine("Paks found:");
                foreach (var p in multiPak.PakFiles)
                {
                    if (p is not BinaryPakManyFile pak) throw new InvalidOperationException("multiPak not a BinaryPakFile");
                    Console.WriteLine($"\n{pak.Name}");
                    foreach (var exts in pak.Files.Select(x => Path.GetExtension(x.Path)).GroupBy(x => x)) Console.WriteLine($"  files{exts.Key}: {exts.Count()}");
                }
            }
            return Task.FromResult(0);
        }
    }
}