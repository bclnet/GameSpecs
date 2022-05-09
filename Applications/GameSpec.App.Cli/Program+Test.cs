using CommandLine;
using System;
using System.Threading.Tasks;

namespace GameSpec.App.Cli
{
    [Verb("dev", HelpText = "Extract files contents to folder.")]
    class TestOptions
    {
        [Option('f', "family", HelpText = "Family")]
        public string Family { get; set; }

        [Option('u', "uri", HelpText = "Pak file to be extracted")]
        public Uri Uri { get; set; }
    }

    partial class Program
    {
        static async Task<int> RunTestAsync(TestOptions opts)
        {
            await new FamilyTest().TestAsync();
            return 0;
        }
    }
}