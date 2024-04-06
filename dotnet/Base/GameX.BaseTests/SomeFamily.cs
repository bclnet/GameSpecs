using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX
{
    internal class SomePlatform
    {
        public static bool Startup() => true;
    }

    internal static class Some
    {
        public const string FamilyJson =
@"{
    'id': 'Some',
    'name': 'Some Family',
    'games': {
        '*': {
            'pakFileType': 'GameX.Some+SomePakFile, GameX.BaseTests'
        },
        'Found': {
            'name': 'Found',
            'pak': 'game:/path#Found'
        },
        'Missing': {
            'name': 'Missing',
            'pak': 'game:/path#Missing'
        }
    },
    'fileManager': {
    }
}";
        public static readonly Family Family = FamilyManager.CreateFamily(FamilyJson.Replace("'", "\""));

        public class SomePakFile : PakFile
        {
            public SomePakFile(PakState state) : base(state) { Name = "Some Name"; }
            public override int Count => 0;
            public override void Closing() { }
            public override void Opening() { }
            public override bool Contains(object path) => false;
            public override (PakFile, FileSource) GetFileSource(object path, bool throwOnError = true) => throw new NotImplementedException();
            public override Task<Stream> LoadFileData(object path, FileOption option = default, bool throwOnError = true) => throw new NotImplementedException();
            public override Task<T> LoadFileObject<T>(object path, FileOption option = default, bool throwOnError = true) => throw new NotImplementedException();
        }

        public const string FileManagerJson =
@"{
}";
    }
}