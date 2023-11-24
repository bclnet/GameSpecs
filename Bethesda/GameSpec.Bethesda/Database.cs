using GameSpec.Formats;

namespace GameSpec.Bethesda
{
    public class Database
    {
        public readonly BinaryPakManyFile Source;

        public Database(PakFile source) => Source = source as BinaryPakManyFile;

        public override string ToString() => Source.Name;

        //public ConcurrentDictionary<uint, FileType> FileCache { get; } = new ConcurrentDictionary<uint, FileType>();
    }
}
