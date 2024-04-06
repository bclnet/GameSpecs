using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static GameX.IW.Zone.Asset;

namespace GameX.IW.Zone
{
    public unsafe class ZoneInfo
    {
        const int MAX_ASSET_COUNT = 4096;
        const int MAX_SCRIPT_STRINGS = 2048;

        public string name;
        public int scriptStringCount;
        public string[] scriptStrings = new string[MAX_SCRIPT_STRINGS];
        public int assetCount;
        public Asset[] assets = new Asset[MAX_ASSET_COUNT];
        public int index_start;

        public static ZoneInfo getZoneInfo(string zoneName)
            => new ZoneInfo { name = zoneName };

        //public void freeZoneInfo()
        //{
        //    assets = null;
        //    scriptStrings = null;
        //}

        static uint R_HashString(string str)
        {
            var hash = 0U;
            foreach (var c in str)
                hash = (uint)((byte)c | 0x20) ^ (0x21 * hash);
            return hash;
        }

        public int containsAsset(UnkAssetType type, string name)
        {
            var str = R_HashString(name);
            for (var i = 0; i < assetCount; i++)
            {
                if (assets[i].type != type || assets[i].name != str) continue;
                return i;
            }
            return -1;
        }

        public int containsScriptString(string str)
        {
            for (var i = 0; i < scriptStringCount; i++)
                if (scriptStrings[i] == str) return i;
            return -1;
        }

        public int addAsset(UnkAssetType type, string name, object data)
        {
            if (assetCount >= MAX_ASSET_COUNT) throw new Exception("Increase MAX_ASSET_COUNT!");
            var a = containsAsset(type, name);
            if (a >= 0) return a;

            // force data to have correct name
            var assetName = Asset.getAssetName(type, data);
            if (assetName != name)
                Asset.setAssetName(type, data, name);

            assets[assetCount].name = R_HashString(name);
            assets[assetCount].type = type;
            assets[assetCount].data = data;
#if DEBUG
            assets[assetCount].debugName = name;
            assets[assetCount].verified = false;
#endif
            return assetCount++;
        }

        public int addScriptString(string str)
        {
            if (scriptStringCount >= MAX_SCRIPT_STRINGS) throw new Exception("Increase MAX_SCRIPT_STRINGS!");
            var a = containsScriptString(str);
            if (a >= 0) return a;

            scriptStrings[scriptStringCount] = str;
            return scriptStringCount++;
        }
        //public int addScriptString(string str)
        //    => addScriptString(str ?? "DEFAULT_STRING");

        public void doLastAsset(string name)
        {
            throw new NotImplementedException();
            //var data = new Rawfile
            //{
            //    name = name,
            //    compressedData = Encoding.ASCII.GetBytes("Made With ZoneBuilder"),
            //    sizeUnCompressed = 0
            //};
            //data.sizeCompressed = data.compressedData.Length + 1;

            //addAsset(ASSET_TYPE.RAWFILE, name, data);
        }

        public object getAsset(UnkAssetType type, string name)
        {
            for (var i = 0; i < assetCount; i++)
                if (assets[i].type == type && assets[i].name == R_HashString(name))
                    return assets[i].data;
            return null;
        }

        public object findAssetEverywhere(UnkAssetType type, string name)
        {
            var ret = getAsset(type, name);
            if (ret != null) return ret;
            throw new NotImplementedException();
            //return DB_FindXAssetHeader(type, name);
        }

#if DEBUG
        public void verifyAsset(UnkAssetType type, string name)
        {
            for (var i = 0; i < assetCount; i++)
                if (assets[i].type == type && assets[i].name == R_HashString(name))
                    assets[i].verified = true;
        }

        public Asset nextUnverifiedAsset()
        {
            for (var i = 0; i < assetCount; i++)
                if (!assets[i].verified)
                    return assets[i];
            return default;
        }
#endif

        // TOOLMAIN
        // 276 is original
        // 277 is filepointers
        public const int desiredFFVersion = 276;

        public static byte[] Header = { (byte)'I', (byte)'W', (byte)'f', (byte)'f', (byte)'u', (byte)'1', (byte)'0', (byte)'0',
            (desiredFFVersion & 0xFF), desiredFFVersion >> 8, desiredFFVersion >> 16, desiredFFVersion >> 24,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        [StructLayout(LayoutKind.Sequential)] public struct FILETIME { public uint dwLowDateTime; public uint dwHighDateTime; }
        [DllImport("kernel32")] public static extern void GetSystemTimeAsFileTime(ref FILETIME lpSystemTimeAsFileTime);

        public void loadAsset(UnkAssetType type, string filename, string name)
        {
            if (containsAsset(type, name) > 0) return;

            Console.Write($"Loading Asset {name} of type {getAssetStringForType(type)}");
            if (filename != null) Console.Write($" ({filename})");
            Console.WriteLine();

            char* data = null;
            int size = -1;
            //if (filename == null) // stock asset
            //{
            //    data = (char*)DB_FindXAssetHeader(type, name);
            //    if (DB_IsAssetDefault(type, name))
            //        Console.WriteLine("Got Default asset! Make sure this is correct\n");
            //    size = -1;
            //}
            //else
            //{
            //    size = FS_ReadFile(filename, (void**)&data);
            //    if (size < 0) // renamed stock asset
            //    {
            //        data = (char*)DB_FindXAssetHeader(type, filename);
            //        if (DB_IsAssetDefault(type, filename))
            //            Console.WriteLine("Got Default asset! Make sure this is correct\n");
            //        size = -1;
            //    }
            //}

            object asset = null;
            switch (type)
            {
                case UnkAssetType.PHYSPRESET: asset = PhysPreset.addPhysPreset(this, name, data, size); break;
                case UnkAssetType.PHYS_COLLMAP: asset = PhysGeomList.addPhysCollmap(this, name, data, size); break;
                case UnkAssetType.XANIM: asset = XAnim.addXAnim(this, name, data, size); break;
                    //case ASSET_TYPE.RAWFILE: asset = Rawfile.addRawfile(this, name, data, size); break;
                    //case ASSET_TYPE.MATERIAL: asset = addMaterial(this, name, data, size); break;
                    //case ASSET_TYPE.PIXELSHADER: asset = addPixelShader(this, name, data, size); break;
                    //case ASSET_TYPE.VERTEXSHADER: asset = addVertexShader(this, name, data, size); break;
                    //case ASSET_TYPE.VERTEXDECL: asset = addVertexDecl(this, name, data, size); break;
                    //case ASSET_TYPE.IMAGE: asset = addGfxImage(this, name, data, size); break;
                    //case ASSET_TYPE.TECHSET: asset = addTechset(this, name, data, size); break;
                    //case ASSET_TYPE.XMODEL: asset = addXModel(this, name, data, size); break;
                    //case ASSET_TYPE.XMODELSURFS: throw new Exception("Can't create XModelSurfs directly. Use XModel.");
                    //case ASSET_TYPE.COL_MAP_MP: asset = addColMap(this, name, data, size); break;
                    //case ASSET_TYPE.MAP_ENTS: asset = addMapEnts(this, name, data, size); break;
                    //case ASSET_TYPE.COM_MAP: asset = addComWorld(this, name, data, size); break;
                    //case ASSET_TYPE.GAME_MAP_MP: asset = addGameMap_MP(this, name, data, size); break;
                    //case ASSET_TYPE.GAME_MAP_SP: asset = addGameMap_SP(this, name, data, size); type = ASSET_TYPE.GAME_MAP_MP; break;
                    //case ASSET_TYPE.STRINGTABLE: asset = addStringTable(this, name, data, size); break;
                    //case ASSET_TYPE.SOUND: asset = addSoundAlias(this, name, data, size); break;
                    //case ASSET_TYPE.SNDCURVE: asset = addSndCurve(this, name, data, size); break;
                    //case ASSET_TYPE.LOADED_SOUND: asset = addLoadedSound(this, name, data, size); break;
                    //case ASSET_TYPE.LIGHTDEF: asset = addGfxLightDef(this, name, data, size); break;
                    //case ASSET_TYPE.FONT: asset = addFont(this, name, data, size); break;
                    //case ASSET_TYPE.FX: asset = addFxEffectDef(this, name, data, size); break;
                    //case ASSET_TYPE.IMPACTFX: asset = addFxImpactTable(this, name, data, size); break;
                    //// this one is weird b/c there's more than 1 asset in each file
                    //case ASSET_TYPE.LOCALIZE: addLocalize(this, name, data, size); break;
                    //// its either already loaded or we need to load it here
                    //case ASSET_TYPE.WEAPON: asset = size > 0 ? addWeaponVariantDef(this, name, (char*)filename, strlen(filename)) : addWeaponVariantDef(this, name, data, -1);
                    //case ASSET_TYPE.LEADERBOARDDEF: asset = addLeaderboardDef(this, name, data, size); break;
                    //case ASSET_TYPE.STRUCTUREDDATADEF: asset = addStructuredDataDefSet(this, name, data, size); break;
                    //case ASSET_TYPE.TRACER: asset = addTracer(this, name, data, size); break;
                    //case ASSET_TYPE.VEHICLE: asset = addVehicleDef(this, name, data, size); break;
                    //case ASSET_TYPE.MPTYPE:
                    //case ASSET_TYPE.AITYPE:
                    //case ASSET_TYPE.CHARACTER:
                    //case ASSET_TYPE.XMODELALIAS:
                    //case ASSET_TYPE.SNDDRIVERGLOBALS:
                    //case ASSET_TYPE.ADDON_MAP_ENTS:
                    //    throw new Exception($"Cannot define a new asset of type {getAssetStringForType(type)}! Ignoring asset and continuing...");
            }

            if (type != UnkAssetType.LOCALIZE)
            {
                if (asset == null) throw new Exception($"Failed to add asset {name}!");
                else addAsset(type, name, asset);
            }

            //if (size > 0 && type != ASSET_TYPE.WEAPON) // weapon loading destroys data for some reason
            //    FS_FreeFile(data);
        }

        void loadAssetsFromFile(string file)
        {
            if (!File.Exists(file)) throw new Exception($"Input file {file} does not exist!");

            using (var fp = File.OpenRead(file))
            {
                var csv = new CsvFile(fp.ReadAllBytes());
                var row = 0;
                var typeStr = csv.GetData(row, 0);
                while (typeStr != null)
                {
                    var type = getAssetTypeForString(typeStr);
                    var realname = csv.GetData(row, 1);
                    var filename = csv.GetData(row, 2);
                    loadAsset(type, filename, realname);
                    row++;
                    typeStr = csv.GetData(row, 0);
                }
            }
        }

        static void debugChecks()
        {
            Debug.Assert(sizeof(PhysGeomList) == 0x48);
            Debug.Assert(sizeof(PhysGeomInfo) == 0x44);
            Debug.Assert(sizeof(BrushWrapper) == 0x44);
            Debug.Assert(sizeof(cPlane) == 0x14);
            Debug.Assert(sizeof(cBrushSide) == 8);
            //Debug.Assert(sizeof(XAnim) == DB_GetXAssetTypeSize(ASSET_TYPE.XANIM));
            //Debug.Assert(sizeof(XModelSurfaces) == DB_GetXAssetTypeSize(ASSET_TYPE.XMODELSURFS));
            //Debug.Assert(sizeof(XModel) == DB_GetXAssetTypeSize(ASSET_TYPE.XMODEL));
            Debug.Assert(sizeof(Material) == 0x60);
            Debug.Assert(sizeof(GfxImage) == 0x20);
            //Debug.Assert(sizeof(SoundAliasList) == DB_GetXAssetTypeSize(ASSET_TYPE.SOUND));
            Debug.Assert(sizeof(SoundAlias) == 100);
            Debug.Assert(sizeof(SpeakerMap) == 408);
            Debug.Assert(sizeof(SoundFile) == 12);
            Debug.Assert(sizeof(WeaponVariantDef) == 0x74);
            Debug.Assert(sizeof(WeaponDef) == 0x684);
            Debug.Assert(Marshal.SizeOf<ClipMap>() == 256);
        }

        static ZoneInfo currentInfo;
        public static void ZoneBuild(string building)
        {
            debugChecks();
            var toBuild = building;
#if DEBUG
            Console.WriteLine("----ZoneBuilder Startup----");
#endif
            var info = currentInfo = getZoneInfo(toBuild);
            Console.WriteLine($"Building Zone : {toBuild}");
            Console.WriteLine("Loading Assets...");
            var sourceFile = $"zone_source/{toBuild}.csv";
            info.loadAssetsFromFile(sourceFile);
            info.doLastAsset(toBuild);
            Console.WriteLine("Writing Zone...");
            byte[] compressed;
            using (var buf = ZoneWriter.writeZone(info))
            {
#if DEBUG
                using (var fp = File.OpenWrite("uncompressed_zone"))
                    buf.writetofile(fp);
#endif
                Console.WriteLine("Compressing Zone...");
                compressed = buf.compressZlib();
            }

            Console.WriteLine("Writing to Disk...");
            Directory.CreateDirectory("zone");
            //var outputdir = ((char*(*)())0x45CBA0)();
            var outputdir = "alter";
            using (var o = File.OpenWrite($"zone\\{outputdir}\\{toBuild}.ff"))
            {
                var time = new FILETIME();
                GetSystemTimeAsFileTime(ref time);
                var header = (byte[])Header.Clone();
                fixed (byte* _ = header)
                {
                    *(uint*)&_[13] = time.dwHighDateTime;
                    *(uint*)&_[17] = time.dwLowDateTime;
                }
                o.Write(header, 0, 21);
                o.Write(compressed, 0, compressed.Length);
            }
            Console.WriteLine("Done!\n");
        }

#if __DEBUG
        static void buildVerify(ASSET_TYPE type, string name, void* asset)
        {
            // images & surfs dont get added
            if (type == ASSET_TYPE.IMAGE || type == ASSET_TYPE.XMODELSURFS) return;

            if (containsAsset(currentInfo, type, name) < 0)
                throw new Exception($"Invalid zone created!\nSee asset {name} of type 0x{type:x}");
            verifyAsset(currentInfo, type, name);
        }

        static void checkVerified()
        {
            asset_t* a = nextUnverifiedAsset(currentInfo);
            while (a != NULL)
            {
                Com_Printf("Asset '%s' unverified\n", a->debugName.c_str());
                a = nextUnverifiedAsset(currentInfo);
            }
        }
#endif
    }
}