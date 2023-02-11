using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec
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
    'name': 'Some Estate',
    'pakFileType': 'GameEstate.Some+PakFile, GameEstateTests',
    'games': {
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
        public static readonly Family Family = FamilyManager.ParseFamily(FamilyJson.Replace("'", "\""));

        public class SomePakFile : PakFile
        {
            public SomePakFile(Family family, FamilyGame game, string filePath, object tag = null) : base(family, game, "Some Name") { }
            public override void Dispose() { }
            public override int Count => 0;
            public override void Close() { }
            public override bool Contains(string path) => false;
            public override bool Contains(int fileId) => false;
            public override Task<Stream> LoadFileDataAsync(string path, DataOption option = 0, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
            public override Task<Stream> LoadFileDataAsync(int fileId, DataOption option = 0, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
            public override Task<Stream> LoadFileDataAsync(FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
            public override Task<T> LoadFileObjectAsync<T>(string path, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
            public override Task<T> LoadFileObjectAsync<T>(int fileId, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
            public override Task<T> LoadFileObjectAsync<T>(FileMetadata file, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
        }
    }
}
