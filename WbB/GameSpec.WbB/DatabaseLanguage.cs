using GameSpec.WbB.Formats.FileTypes;

namespace GameSpec.WbB
{
    public class DatabaseLanguage : Database
    {
        public DatabaseLanguage(PakFile pakFile) : base(pakFile)
            => CharacterTitles = GetFile<StringTable>(StringTable.CharacterTitle_FileID);

        public StringTable CharacterTitles { get; }
    }
}
