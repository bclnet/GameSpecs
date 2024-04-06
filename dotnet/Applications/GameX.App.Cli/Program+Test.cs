using CommandLine;
using System;
using System.Threading.Tasks;

namespace GameX.App.Cli
{
    [Verb("dev", HelpText = "Test.")]
    class TestOptions
    {
        [Option('f', "family", HelpText = "Family")]
        public string Family { get; set; }

        [Option('u', "uri", HelpText = "Pak file to be extracted")]
        public Uri Uri { get; set; }
    }

    partial class Program
    {
        static Task<int> RunTestAsync(TestOptions args)
        {
            //await new FamilyTest().TestAsync();
            return Task.FromResult(0);
        }
    }
}