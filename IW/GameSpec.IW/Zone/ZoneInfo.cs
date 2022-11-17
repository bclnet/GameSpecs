using System;
using System.Text;
using static GameSpec.IW.Zone.Asset;

namespace GameSpec.IW.Zone
{
    public class ZoneInfo
    {
        const int MAX_ASSET_COUNT = 4096;
        const int MAX_SCRIPT_STRINGS = 2048;

        public string name;
        public int scriptStringCount;
        public string[] scriptStrings = new string[MAX_SCRIPT_STRINGS];
        public int assetCount;
        public Asset[] assets = new Asset[MAX_ASSET_COUNT];
        public int index_start;

        public ZoneInfo getZoneInfo(string zoneName)
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

        public int containsAsset(ASSET_TYPE type, string name)
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

        public int addAsset(ASSET_TYPE type, string name, object data)
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

        public object getAsset(ASSET_TYPE type, string name)
        {
            for (var i = 0; i < assetCount; i++)
                if (assets[i].type == type && assets[i].name == R_HashString(name))
                    return assets[i].data;
            return null;
        }

        public object findAssetEverywhere(ASSET_TYPE type, string name)
        {
            var ret = getAsset(type, name);
            if (ret != null) return ret;
            throw new NotImplementedException();
            //return DB_FindXAssetHeader(type, name);
        }

#if DEBUG
        public void verifyAsset(ASSET_TYPE type, string name)
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

        //public void loadAsset(ASSET_TYPE type, string filename, string name)
        //{
        //    if (containsAsset(type, name) > 0) return;

        //    Console.Write($"Loading Asset {name} of type {getAssetStringForType(type)}");
        //    if (filename != null) Console.Write($" ({filename})");
        //    Console.WriteLine();

        //    char* data;
        //    int size;

        //    if (filename == null) // stock asset
        //    {
        //        data = (char*)DB_FindXAssetHeader(type, name);
        //        if (DB_IsAssetDefault(type, name))
        //            Console.WriteLine("Got Default asset! Make sure this is correct\n");
        //        size = -1;
        //    }
        //    else
        //    {
        //        size = FS_ReadFile(filename, (void**)&data);
        //        if (size < 0) // renamed stock asset
        //        {
        //            data = (char*)DB_FindXAssetHeader(type, filename);
        //            if (DB_IsAssetDefault(type, filename))
        //                Console.WriteLine("Got Default asset! Make sure this is correct\n");
        //            size = -1;
        //        }
        //    }

        //    object asset = null;
        //    switch (type)
        //    {
        //        case ASSET_TYPE.PHYSPRESET:
        //            asset = addPhysPreset(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.PHYS_COLLMAP:
        //            asset = addPhysCollmap(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.XANIM:
        //            asset = addXAnim(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.RAWFILE:
        //            asset = addRawfile(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.MATERIAL:
        //            asset = addMaterial(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.PIXELSHADER:
        //            asset = addPixelShader(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.VERTEXSHADER:
        //            asset = addVertexShader(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.VERTEXDECL:
        //            asset = addVertexDecl(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.IMAGE:
        //            asset = addGfxImage(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.TECHSET:
        //            asset = addTechset(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.XMODEL:
        //            asset = addXModel(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.XMODELSURFS:
        //            throw new Exception("Can't create XModelSurfs directly. Use XModel.");
        //        case ASSET_TYPE.COL_MAP_MP:
        //            asset = addColMap(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.MAP_ENTS:
        //            asset = addMapEnts(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.COM_MAP:
        //            asset = addComWorld(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.GAME_MAP_MP:
        //            asset = addGameMap_MP(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.GAME_MAP_SP:
        //            asset = addGameMap_SP(info, name, data, size);
        //            type = ASSET_TYPE_GAME_MAP_MP;
        //            break;
        //        case ASSET_TYPE.STRINGTABLE:
        //            asset = addStringTable(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.SOUND:
        //            asset = addSoundAlias(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.SNDCURVE:
        //            asset = addSndCurve(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.LOADED_SOUND:
        //            asset = addLoadedSound(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.LIGHTDEF:
        //            asset = addGfxLightDef(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.FONT:
        //            asset = addFont(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.FX:
        //            asset = addFxEffectDef(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.IMPACTFX:
        //            asset = addFxImpactTable(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.LOCALIZE:
        //            addLocalize(info, name, data, size); // this one is weird b/c there's more than 1 asset in each file
        //            break;
        //        case ASSET_TYPE.WEAPON:
        //            {
        //                // its either already loaded or we need to load it here
        //                if (size > 0)
        //                    asset = addWeaponVariantDef(info, name, (char*)filename, strlen(filename));
        //                else
        //                    asset = addWeaponVariantDef(info, name, data, -1);
        //                break;
        //            }
        //        case ASSET_TYPE.LEADERBOARDDEF:
        //            asset = addLeaderboardDef(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.STRUCTUREDDATADEF:
        //            asset = addStructuredDataDefSet(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.TRACER:
        //            asset = addTracer(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.VEHICLE:
        //            asset = addVehicleDef(info, name, data, size);
        //            break;
        //        case ASSET_TYPE.MPTYPE:
        //        case ASSET_TYPE.AITYPE:
        //        case ASSET_TYPE.CHARACTER:
        //        case ASSET_TYPE.XMODELALIAS:
        //        case ASSET_TYPE.SNDDRIVERGLOBALS:
        //        case ASSET_TYPE.ADDON_MAP_ENTS:
        //            throw new Exception($"Cannot define a new asset of type {getAssetStringForType(type)}! Ignoring asset and continuing...");
        //    }

        //    if (type != ASSET_TYPE.LOCALIZE)
        //    {
        //        if (asset == null) throw new Exception($"Failed to add asset {name}!");
        //        else addAsset(type, name, asset);
        //    }

        //    if (size > 0 && type != ASSET_TYPE.WEAPON) // weapon loading destroys data for some reason
        //        FS_FreeFile(data);
        //}

        //void loadAssetsFromFile(string file)
        //{
        //    if (GetFileAttributesA(file) == INVALID_FILE_ATTRIBUTES) { throw new Exception($"Input file {file} does not exist!"); return; }

        //    FILE* fp = fopen(file, "rb");
        //    int csvLength = flength(fp);
        //    char* buf = new char[csvLength];
        //    fread(buf, 1, csvLength, fp);

        //    CSVFile csv(buf, csvLength);
        //    int row = 0;

        //    char* typeStr = csv.getData(row, 0);

        //    while (typeStr != NULL)
        //    {
        //        int type = getAssetTypeForString(typeStr);
        //        char* realname = csv.getData(row, 1);
        //        char* filename = csv.getData(row, 2);
        //        loadAsset(info, type, filename, realname);

        //        row++;
        //        typeStr = csv.getData(row, 0);
        //    }

        //    delete[] buf;

        //    fclose(fp);
        //}
    }
}