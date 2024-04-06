using CommandLine;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.App.Cli
{
    partial class Program
    {
        [Verb("list", HelpText = "List files contents.")]
        class ListOptions
        {
            [Option('f', "family", HelpText = "Family")]
            public string Family { get; set; }

            [Option('u', "uri", HelpText = "Pak file to be list")]
            public Uri Uri { get; set; }
        }

        static Task<int> RunListAsync(ListOptions args)
        {
            // list families
            if (string.IsNullOrEmpty(args.Family))
            {
                Console.WriteLine("Families installed:\n");
                foreach (var _ in FamilyManager.Families) Console.WriteLine($"{_.Key} - {_.Value.Name}");
                return Task.FromResult(0);
            }

            // get family
            var family = FamilyManager.GetFamily(args.Family, false);
            if (family == null) { Console.WriteLine($"No family found named \"{args.Family}\"."); return Task.FromResult(0); }

            // list found paths in family
            if (args.Uri == null)
            {
                Console.WriteLine($"{family.Name}\nDescription: {family.Description}\nStudio: {family.Studio}");
                Console.WriteLine($"\nGames:");
                foreach (var game in family.Games.Values)
                    Console.WriteLine($"{game.Name}{(game.Found ? $" -> {string.Join(',', (IEnumerable<Uri>)game.Paks)}" : null)}");
                var paths = family.FileManager.Paths;
                Console.WriteLine("\nPaths:");
                if (paths.Count == 0) { Console.WriteLine($"No paths found for family \"{args.Family}\"."); return Task.FromResult(0); }
                foreach (var path in paths) Console.WriteLine($"{family.GetGame(path.Key, out var _)} - {string.Join(", ", path.Value)}");
                return Task.FromResult(0);
            }

            // list files in pack for family
            else
            {
                Console.WriteLine($"{family.Name} - {args.Uri}\n");
                //if (estate.OpenPakFile(estate.ParseResource(opts.Uri)) is not MultiPakFile multiPak) throw new InvalidOperationException("multiPak not a MultiPakFile");
                using var multiPak = family.OpenPakFile(args.Uri) as MultiPakFile ?? throw new InvalidOperationException("multiPak not a MultiPakFile");
                if (multiPak.PakFiles.Count == 0) { Console.WriteLine("No paks found."); return Task.FromResult(0); }
                Console.WriteLine("Paks found:");
                foreach (var p in multiPak.PakFiles)
                {
                    if (p is not BinaryPakFile pak) throw new InvalidOperationException("multiPak not a BinaryPakFile");
                    Console.WriteLine($"\n{pak.Name}");
                    foreach (var exts in pak.Files.Select(x => Path.GetExtension(x.Path)).GroupBy(x => x)) Console.WriteLine($"  files{exts.Key}: {exts.Count()}");
                }
            }
            return Task.FromResult(0);
        }
    }
}