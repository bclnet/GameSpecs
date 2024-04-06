using GameX.Formats;

namespace GameX.Bethesda
{
    public class Database
    {
        public readonly BinaryPakFile Source;

        public Database(PakFile source) => Source = source as BinaryPakFile;

        public override string ToString() => Source.Name;

        //public ConcurrentDictionary<uint, FileType> FileCache { get; } = new ConcurrentDictionary<uint, FileType>();
    }
}
