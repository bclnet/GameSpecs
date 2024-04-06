using System;

namespace GameX.IW.Zone
{
    //public enum CodAssetType : int
    //{
    //    xmodelpieces = 0,
    //    physpreset = 1,
    //    xanim = 2,
    //    xmodel = 3,
    //    material = 4,
    //    pixelshader = 5,
    //    techset = 6,
    //    image = 7,
    //    sndcurve = 8,
    //    loaded_sound = 9,
    //    col_map_sp = 0x0a,
    //    col_map_mp = 0x0b,
    //    com_map = 0x0c,
    //    game_map_sp = 0x0d,
    //    game_map_mp = 0x0e,
    //    map_ents = 0x0f,
    //    gfx_map = 0x10,
    //    lightdef = 0x11,
    //    ui_map = 0x12,
    //    font = 0x13,
    //    menufile = 0x14,
    //    menu = 0x15,
    //    localize = 0x16,
    //    weapon = 0x17,
    //    snddriverglobals = 0x18,
    //    impactfx = 0x19,
    //    aitype = 0x1a,
    //    mptype = 0x1b,
    //    character = 0x1c,
    //    xmodelalias = 0x1d,
    //    rawfile = 0x1f,
    //    stringtable = 0x20,
    //}
    public enum UnkAssetType : int
    {
        PHYSPRESET = 0,
        PHYS_COLLMAP = 1,
        XANIM = 2,
        XMODELSURFS = 3,
        XMODEL = 4,
        MATERIAL = 5,
        PIXELSHADER = 6,
        VERTEXSHADER = 7,
        VERTEXDECL = 8,
        TECHSET = 9,
        IMAGE = 10,
        SOUND = 11,
        SNDCURVE = 12,
        LOADED_SOUND = 13,
        COL_MAP_SP = 14,
        COL_MAP_MP = 15,
        COM_MAP = 16,
        GAME_MAP_SP = 17,
        GAME_MAP_MP = 18,
        MAP_ENTS = 19,
        FX_MAP = 20,
        GFX_MAP = 21,
        LIGHTDEF = 22,
        UI_MAP = 23,
        FONT = 24,
        MENUFILE = 25,
        MENU = 26,
        LOCALIZE = 27,
        WEAPON = 28,
        SNDDRIVERGLOBALS = 29,
        FX = 30,
        IMPACTFX = 31,
        AITYPE = 32,
        MPTYPE = 33,
        CHARACTER = 34,
        XMODELALIAS = 35,
        RAWFILE = 36,
        STRINGTABLE = 37,
        LEADERBOARDDEF = 38,
        STRUCTUREDDATADEF = 39,
        TRACER = 40,
        VEHICLE = 41,
        ADDON_MAP_ENTS = 42,
        MAX = 43
    }

    public unsafe partial class Asset
    {
        static string[] assetTypeStrings = {
            "physpreset",
            "phys_collmap",
            "xanim",
            "xmodelsurfs",
            "xmodel",
            "material",
            "pixelshader",
            "vertexshader",
            "vertexdecl",
            "techset",
            "image",
            "sound",
            "sndcurve",
            "loaded_sound",
            "col_map_sp",
            "col_map_mp",
            "com_map",
            "game_map_sp",
            "game_map_mp",
            "map_ents",
            "fx_map",
            "gfx_map",
            "lightdef",
            "ui_map",
            "font",
            "menufile",
            "menu",
            "localize",
            "weapon",
            "snddriverglobals",
            "fx",
            "impactfx",
            "aitype",
            "mptype",
            "character",
            "xmodelalias",
            "rawfile",
            "stringtable",
            "leaderboarddef",
            "structureddatadef",
            "tracer",
            "vehicle",
            "addon_map_ents",
        };

        public static UnkAssetType getAssetTypeForString(string str)
        {
            var i = 42;
            while (i > -1)
            {
                if (assetTypeStrings[i] == str) return (UnkAssetType)i;
                i--;
            }
            return (UnkAssetType)(-1);
        }

        public static string getAssetStringForType(UnkAssetType type)
            => assetTypeStrings[(int)type];

        public static string getAssetName(UnkAssetType type, object data)
        {
            throw new NotImplementedException();
            //if (type == ASSET_TYPE.LOCALIZE) return ((Localize)data).name;
            //if (type == ASSET_TYPE.IMAGE) return ((GfxImage)data).name;
            //return ((Rawfile)data).name;
        }

        public static void setAssetName(UnkAssetType type, object data, string name)
        {
            throw new NotImplementedException();
            //if (type == ASSET_TYPE.LOCALIZE) ((Localize)data).name = name;
            //else if (type == ASSET_TYPE.IMAGE) ((GfxImage)data).name = name;
            //else ((Rawfile)data).name = name;
        }

        //public static void WRITE_ASSET<T>(object asset)
        //{
        //    T* dest = (T*)buf.at();
        //    buf.write(asset, sizeof(T), 1);
        //}

        //public static void WRITE_ASSET_NUM<T>(object asset, int num)
        //{
        //    T* dest = (T*)buf.at();
        //    buf.write(asset, sizeof(T), num);
        //}

        //public static void WRITE_NAME(object asset)
        //{
        //    buf.write(asset.name, strlen(asset.name) + 1, 1);
        //    dest.name = (char*)-1;
        //}

        //public static void WRITE_FIELD<T>(object asset, object field, int count)
        //{
        //    if (asset.field)
        //    {
        //        buf.write(asset.field, sizeof(T), asset.count);
        //        dest.field = (T*)-1;
        //    }
        //}

        //public static void WRITE_FIELD_ALIGNED<T>(object asset, object field, int count, int alignment)
        //{
        //    if (asset.field)
        //    {
        //        buf.align(alignment);
        //        buf.write(asset.field, sizeof(T), asset.count);
        //        dest.field = (T*)-1;
        //    }
        //}

        //public static void WRITE_FIELD_ON_SIZE<T>(object asset, object field, int count)
        //{
        //    if (asset.field)
        //    {
        //        buf.write(asset.field, asset.count > 255 ? asset.count * 2 : asset.count, 1);
        //        dest.field = (T*)-1;
        //    }
        //}

        //public static void WRITE_STRING(object asset, string str)
        //{
        //    buf.write(asset.str, strlen(asset.str) + 1, 1);
        //    dest.str = (char*)-1;
        //}

        //public static void HAS_FIELD(object asset, field)
        //{
        //    asset.field != 0;
        //}


    }
}