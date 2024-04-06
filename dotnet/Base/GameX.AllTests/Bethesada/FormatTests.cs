//using GameX.AC.Formats;
//using GameX.AC.Formats.FileTypes;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.IO;
//using System.Threading.Tasks;

//namespace GameX.AC
//{
//    [TestClass]
//    public class FormatsTests
//    {
//        const string cellDatLocation = "game:/client_cell_1.dat#AC"; const int ExpectedCellDatFileCount = 805003;
//        const string portalDatLocation = "game:/client_portal.dat#AC"; const int ExpectedPortalDatFileCount = 79694;
//        const string localEnglishDatLocation = "game:/client_local_English.dat#AC"; const int ExpectedLocalEnglishDatFileCount = 118;

//        readonly Family family = FamilyManager.GetFamily("Bethesda");

//        [TestMethod]
//        public void LoadCellDat_NoExceptions()
//        {
//            var dat = new Database(family.OpenPakFile(new Uri(cellDatLocation)));
//            var count = dat.Source.Count;
//            Assert.IsTrue(ExpectedCellDatFileCount <= count, $"Insufficient files parsed from .dat. Expected: >= {ExpectedCellDatFileCount}, Actual: {count}");
//        }

//        [TestMethod]
//        public void LoadPortalDat_NoExceptions()
//        {
//            var dat = new Database(family.OpenPakFile(new Uri(portalDatLocation)));
//            var count = dat.Source.Count;
//            Assert.IsTrue(ExpectedPortalDatFileCount <= count, $"Insufficient files parsed from .dat. Expected: >= {ExpectedPortalDatFileCount}, Actual: {count}");
//        }

//        [TestMethod]
//        public void LoadLocalEnglishDat_NoExceptions()
//        {
//            var dat = new Database(family.OpenPakFile(new Uri(localEnglishDatLocation)));
//            var count = dat.Source.Count;
//            Assert.IsTrue(ExpectedLocalEnglishDatFileCount <= count, $"Insufficient files parsed from .dat. Expected: >= {ExpectedLocalEnglishDatFileCount}, Actual: {count}");
//        }

//        [TestMethod]
//        public async Task UnpackCellDatFiles_NoExceptions()
//        {
//            var dat = new Database(family.OpenPakFile(new Uri(cellDatLocation)));
//            foreach (var (key, value) in dat.Source.FilesById)
//            {
//                if ((uint)key == Iteration.FILE_ID) continue;
//                if (value.FileSize == 0) continue; // DatFileType.LandBlock files can be empty

//                var fileType = value.GetFileType(PakType.Cell).fileType;
//                Assert.IsNotNull(fileType, $"Key: 0x{key:X8}, ObjectID: 0x{value.Id:X8}, FileSize: {value.FileSize}");

//                var factory = value.ObjectFactory;
//                if (factory == null) throw new Exception($"Class for fileType: {fileType} does not implement an ObjectFactory.");

//                using var r = new BinaryReader(await dat.Source.LoadFileDataAsync(value));
//                await factory(r, value);
//                if (r.Position() != value.FileSize) throw new Exception($"Failed to parse all bytes for fileType: {fileType}, ObjectId: 0x{value.Id:X8}. Bytes parsed: {r.Position()} of {value.FileSize}");
//            }
//        }

//        [TestMethod]
//        public async Task UnpackPortalDatFiles_NoExceptions()
//        {
//            var dat = new Database(family.OpenPakFile(new Uri(portalDatLocation)));
//            foreach (var (key, value) in dat.Source.FilesById)
//            {
//                if ((uint)key == Iteration.FILE_ID) continue;

//                var fileType = value.GetFileType(PakType.Portal).fileType;
//                Assert.IsNotNull(fileType, $"Key: 0x{key:X8}, ObjectID: 0x{value.Id:X8}, FileSize: {value.FileSize}");

//                // These file types aren't converted yet
//                if (fileType == PakFileType.KeyMap) continue;
//                if (fileType == PakFileType.RenderMaterial) continue;
//                if (fileType == PakFileType.MaterialModifier) continue;
//                if (fileType == PakFileType.MaterialInstance) continue;
//                if (fileType == PakFileType.ActionMap) continue;
//                if (fileType == PakFileType.MasterProperty) continue;
//                if (fileType == PakFileType.DbProperties) continue;

//                var factory = value.ObjectFactory;
//                if (factory == null) throw new Exception($"Class for fileType: {fileType} does not implement an ObjectFactory.");

//                using var r = new BinaryReader(await dat.Source.LoadFileDataAsync(value));
//                await factory(r, value);
//                if (r.Position() != value.FileSize) throw new Exception($"Failed to parse all bytes for fileType: {fileType}, ObjectId: 0x{value.Id:X8}. Bytes parsed: {r.Position()} of {value.FileSize}");
//            }
//        }

//        [TestMethod]
//        public async Task UnpackLocalEnglishDatFiles_NoExceptions()
//        {
//            var dat = new Database(family.OpenPakFile(new Uri(localEnglishDatLocation)));
//            foreach (var (key, value) in dat.Source.FilesById)
//            {
//                if ((uint)key == Iteration.FILE_ID) continue;

//                var fileType = value.GetFileType(PakType.Language).fileType;

//                Assert.IsNotNull(fileType, $"Key: 0x{key:X8}, ObjectID: 0x{value.Id:X8}, FileSize: {value.FileSize}");

//                // These file types aren't converted yet
//                if (fileType == PakFileType.UILayout) continue;

//                var factory = value.ObjectFactory;
//                if (factory == null) throw new Exception($"Class for fileType: {fileType} does not implement an ObjectFactory.");

//                using var r = new BinaryReader(await dat.Source.LoadFileDataAsync(value));
//                await factory(r, value);
//                if (r.Position() != value.FileSize) throw new Exception($"Failed to parse all bytes for fileType: {fileType}, ObjectId: 0x{value.Id:X8}. Bytes parsed: {r.Position()} of {value.FileSize}");
//            }
//        }

//        // uncomment if you want to run this
//        // [TestMethod]
//        public void ExtractCellDatByLandblock()
//        {
//            var output = @"C:\T_\cell_dat_export_by_landblock";
//            var dat = new DatabaseCell(family.OpenPakFile(new Uri(cellDatLocation)));
//            //dat.ExtractLandblockContents(output);
//        }

//        // uncomment if you want to run this
//        // [TestMethod]
//        public void ExportPortalDatsWithTypeInfo()
//        {
//            var output = @"C:\T_\typed_portal_dat_export";
//            var dat = new DatabasePortal(family.OpenPakFile(new Uri(portalDatLocation)));
//            //dat.ExtractCategorizedPortalContents(output);
//        }
//    }
//}
