using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public class PakBinary<Self> : PakBinary where Self : PakBinary, new()
    {
        public static readonly PakBinary Instance = new Self();

        protected class SubPakFile : BinaryPakFile
        {
            FileSource File;
            BinaryPakFile Source;

            public SubPakFile(FileSource file, BinaryPakFile source, FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, Instance, tag)
            {
                File = file;
                Source = source;
                ObjectFactoryFactoryMethod = source.ObjectFactoryFactoryMethod;
                UseReader = file == null;
                //Open();
            }

            public async override Task Read(BinaryReader r, object tag = null)
            {
                if (UseReader) { await base.Read(r, tag); return; }
                using var r2 = await Source.GetReader().Func(async r => new BinaryReader(await ReadData(r, File)));
                await PakBinary.Read(this, r2, tag);
            }
        }
    }
}