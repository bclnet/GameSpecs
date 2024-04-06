using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameX.Formats
{
    // Library
    // embeded: Compression.Doboz:DobozDecoder - PakBinaryRed
    // embeded: Compression:Lzf - PakBinaryRed
    // embeded: SevenZip.Compression.LZMA:Decoder - CompiledShader
    // K4os.Compression.LZ4:LZ4Codec - PakBinaryRed, DATABinaryKV3, DATATexture
    // ICSharpCode.SharpZipLib:Zip:ZipFile - PakBinaryZip (not a compression algorithm)
    // ICSharpCode.SharpZipLib:Zip.Compression.Streams:InflaterInputStream - PakBinaryRed, PakBinaryTes
    // ICSharpCode.SharpZipLib:Lzw:Lzw​Input​Stream - PakBinaryTes
    // Zstd.Net - not used

    [TestClass]
    public class CompressTests
    {
        [DataTestMethod]
        [DataRow("Tes:Morrowind", "meshes/lavasteam.nif", 17725)]
        [DataRow("Red:Witcher", "main.key:2da00.bif", 887368)]
        [DataRow("Red:Witcher3", "content0/bundles/xml.bundle:engine/physics/apexclothmaterialpresets.xml", 2512)] // 0 - None
        public async Task None(string pak, string sampleFile, int sampleFileSize) => await LoadDataAync(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        [DataTestMethod] // embeded: Doboz:DobozDecoder
        [DataRow("Red:Witcher3", "content0/bundles/xml.bundle:gameplay/items/def_item_misc.xml", 29362)] // 3
        public async Task DecompressDoboz(string pak, string sampleFile, int sampleFileSize) => await LoadDataAync(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        [DataTestMethod] // K4os.Compression.LZ4:LZ4Codec
        [DataRow("Red:Witcher3", "content0/bundles/xml.bundle:gameplay/abilities/monster_base_abl.xml", 591546)] // 4,5
        [DataRow("Tes:SkyrimSE", "Skyrim - Meshes0.bsa:meshes/scalegizmo.nif", 8137)]
        [DataRow("Tes:SkyrimSE", "Skyrim - Textures0.bsa:textures/actors/dog/dog.dds", 1398240)]
        [DataRow("Valve:Dota2", "dota/pak01_dir.vpk:gameplay/abilities/monster_base_abl.xml", 0)]
        public async Task DecompressLz4(string pak, string sampleFile, int sampleFileSize) => await LoadDataAync(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        [DataTestMethod] // embeded: Compression:Lzf
        [DataRow("Tes:Witcher2", "base_scripts.dzip:core/2darray.ws", 968)]
        public async Task DecompressLzf(string pak, string sampleFile, int sampleFileSize) => await LoadDataAync(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        [DataTestMethod]
        [DataRow("Red:CP77", "", 0)] // 2
        public async Task DecompressOodleLZ(string pak, string sampleFile, int sampleFileSize) => await LoadDataAync(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        [DataTestMethod]
        [DataRow("Red:Witcher3", "", 0)] // 2
        public async Task DecompressSnappy(string pak, string sampleFile, int sampleFileSize) => await LoadDataAync(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        [DataTestMethod] // ICSharpCode.SharpZipLib:Zip.Compression.Streams:InflaterInputStream
        [DataRow("Arkane:D2:1", "strings/english_m.lang", 968)]
        [DataRow("Red:Witcher3", "content0/bundles/xml.bundle:engine/io/priority_table.xml", 8596)] // 1
        [DataRow("Tes:Oblivion", "Oblivion - Meshes.bsa:trees/treecottonwoodsu.spt", 62296)]
        [DataRow("Tes:Skyrim", "Skyrim - Textures.bsa:textures/actors/dog/dog.dds", 1398256)]
        [DataRow("Tes:Fallout4", "Fallout4 - Meshes.ba2:Meshes/Marker_Error.NIF", 2334)]
        public async Task DecompressZlib(string pak, string sampleFile, int sampleFileSize) => await LoadDataAync(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        [DataTestMethod]
        public async Task DecompressZlib_2(string pak, string sampleFile, int sampleFileSize) => await LoadDataAync(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        //[DataTestMethod] // ICSharpCode.SharpZipLib:Lzw:Lzw​Input​Stream
        //[DataRow("Witcher2", "base_scripts.dzip:gameplay/abilities/monster_base_abl.xml", 593998)]
        //public async Task DecompressLzw​(string pak, string sampleFile, int sampleFileSize) => await LoadData(Helper.Paks[pak].Value, sampleFile, sampleFileSize);

        //[DataTestMethod] // embeded: SevenZip.Compression.LZMA:Decoder
        //[DataRow("Valve:Dota2", "dota/pak01_dir.vpk:gameplay/abilities/monster_base_abl.xml", 593998)]
        //public async Task DecompressLzma​(string pak, string sampleFile, int sampleFileSize) => await LoadData(Helper.Paks[pak].Value, sampleFile, sampleFileSize);

        [DataTestMethod]
        [DataRow("Tes:Fallout76", "SeventySix - 00UpdateMain.ba2:meshes/actors/moleminer/treasurehunter/treasurehunter.ztl", 17725)]
        public async Task Errors(string pak, string sampleFile, int sampleFileSize) => await LoadDataAync(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        static async Task LoadDataAync(PakFile source, string sampleFile, int sampleFileSize)
        {
            Assert.IsTrue(source.Contains(sampleFile));
            Assert.AreEqual(sampleFileSize, (await source.LoadFileData(sampleFile)).Length);
        }
    }
}
