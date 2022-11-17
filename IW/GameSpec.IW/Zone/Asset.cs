using GameSpec.IW.Zone;
using System;
using System.Runtime.InteropServices;

namespace GameSpec.IW.Zone
{
    public enum ASSET_TYPE : int
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

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct vec3
    {
        public float x, y, z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct vec4
    {
        public float x, y, z, w;
    }

    public unsafe class Asset
    {
        // 276 is original
        // 277 is filepointers
        public const int desiredFFVersion = 276;

        public static byte[] header = { (byte)'I', (byte)'W', (byte)'f', (byte)'f', (byte)'u', (byte)'1', (byte)'0', (byte)'0',
            (desiredFFVersion & 0xFF), desiredFFVersion >> 8, desiredFFVersion >> 16, desiredFFVersion >> 24,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public uint name;
        public ASSET_TYPE type;
        public object data;
        public int offset;
        public bool written;
#if DEBUG
        public string debugName;
        public bool verified;
#endif

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

        public static ASSET_TYPE getAssetTypeForString(string str)
        {
            var i = 42;
            while (i > -1)
            {
                if (assetTypeStrings[i] == str) return (ASSET_TYPE)i;
                i--;
            }
            return (ASSET_TYPE)(-1);
        }

        public static string getAssetStringForType(ASSET_TYPE type)
            => assetTypeStrings[(int)type];

        public static string getAssetName(ASSET_TYPE type, object data)
        {
            throw new NotImplementedException();
            //if (type == ASSET_TYPE.LOCALIZE) return ((Localize)data).name;
            //if (type == ASSET_TYPE.IMAGE) return ((GfxImage)data).name;
            //return ((Rawfile)data).name;
        }

        public static void setAssetName(ASSET_TYPE type, object data, string name)
        {
            throw new NotImplementedException();
            //if (type == ASSET_TYPE.LOCALIZE) ((Localize)data).name = name;
            //else if (type == ASSET_TYPE.IMAGE) ((GfxImage)data).name = name;
            //else ((Rawfile)data).name = name;
        }

        public static int strlen(char* value)
        {
            return 0;
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