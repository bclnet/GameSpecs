using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using static GameX.Arkane.Formats.Danae.Binary_Ftl;

namespace GameX.Arkane.Formats.Danae
{
    //struct
    //{

    //    Vector3 v[3];
    //} EERIE_TRI; // Aligned 1 2 4

    //struct
    //{

    //    EERIE_2D min;
    //EERIE_2D max;
    //} EERIE_2D_BBOX; // Aligned 1 2 4 8

    //struct
    //{

    //    Vector3 min;
    //Vector3 max;
    //} EERIE_3D_BBOX; // Aligned 1 2 4

    //struct
    //{

    //    char exist;
    //char type;
    //char treat;
    //char selected;
    //short extras;
    //short status; // on/off 1/0
    //Vector3 pos;
    //float fallstart;
    //float fallend;
    //float falldiff;
    //float falldiffmul;
    //float precalc;
    //EERIE_RGB rgb255;
    //float intensity;
    //EERIE_RGB rgb;
    //float i;
    //Vector3 mins;
    //Vector3 maxs;
    //float temp;
    //long ltemp;
    //EERIE_RGB ex_flicker;
    //float ex_radius;
    //float ex_frequency;
    //float ex_size;
    //float ex_speed;
    //float ex_flaresize;
    //long tl;
    //unsigned long time_creation;
    //long duration; // will start to fade before the end of duration...
    //long sample;
    //} EERIE_LIGHT; // Aligned 1 2 4

    //enum EERIE_TYPES_EXTRAS_MODE
    //{
    //    EXTRAS_SEMIDYNAMIC = 0x00000001,
    //    EXTRAS_EXTINGUISHABLE = 0x00000002,
    //    EXTRAS_STARTEXTINGUISHED = 0x00000004,
    //    EXTRAS_SPAWNFIRE = 0x00000008,
    //    EXTRAS_SPAWNSMOKE = 0x00000010,
    //    EXTRAS_OFF = 0x00000020,
    //    EXTRAS_COLORLEGACY = 0x00000040,
    //    EXTRAS_NOCASTED = 0x00000080,
    //    EXTRAS_FIXFLARESIZE = 0x00000100,
    //    EXTRAS_FIREPLACE = 0x00000200,
    //    EXTRAS_NO_IGNIT = 0x00000400,
    //    EXTRAS_FLARE = 0x00000800
    //};

    //#define TYP_SPECIAL1 1


    ////*************************************************************************************
    //// EERIE Types
    ////*************************************************************************************

    //struct E_MATRIX
    //{
    //    D3DVALUE _11, _12, _13, _14;
    //    D3DVALUE _21, _22, _23, _24;
    //    D3DVALUE _31, _32, _33, _34;
    //    D3DVALUE _41, _42, _43, _44;
    //}

    public unsafe struct E_CYLINDER
    {
        public static (string, int) Struct = ("<5f", sizeof(E_CYLINDER));
        public Vector3 origin;
        public float radius;
        public float height;
    }

    public unsafe struct E_SPHERE
    {
        public static (string, int) Struct = ("<4f", sizeof(E_SPHERE));
        public Vector3 Origin;
        public float Radius;
    }

    public unsafe struct E_POLY
    {
        public POLY Type;  // at least 16 bits
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 Norm;
        public Vector3 Norm2;
        public TLVERTEX[] V; // new TLVERTEX[4];
        public TLVERTEX[] Tv; // new TLVERTEX[4];
        public Vector3[] Nrml; // new Vector3[4];
        public E_TEXTURE Tex;
        public Vector3 Center;
        public float TransVal;
        public float Area;
        public short Room;
        public short Misc;
        //public float DistBump;
        //public ushort[] UslInd;// new ushort[4];
        internal void memset()
        {
            Misc = 0;
        }
    }

    public unsafe struct E_VERTEX
    {
        public TLVERTEX Vert;
        public Vector3 V;
        public Vector3 Norm;
        public Vector3 VWorld;
    }

    public enum MATERIAL
    {
        NONE = 0,
        WEAPON = 1,
        FLESH = 2,
        METAL = 3,
        GLASS = 4,
        CLOTH = 5,
        WOOD = 6,
        EARTH = 7,
        WATER = 8,
        ICE = 9,
        GRAVEL = 10,
        STONE = 11,
        FOOT_LARGE = 12,
        FOOT_BARE = 13,
        FOOT_SHOE = 14,
        FOOT_METAL = 15,
        FOOT_STEALTH = 16,
    }

    [Flags]
    public enum POLY
    {
        NO_SHADOW = 1,
        DOUBLESIDED = 1 << 1,
        TRANS = 1 << 2,
        WATER = 1 << 3,
        GLOW = 1 << 4,
        //
        IGNORE = 1 << 5,
        QUAD = 1 << 6,
        TILED = 1 << 7,
        METAL = 1 << 8,
        HIDE = 1 << 9,
        //
        STONE = 1 << 10,
        WOOD = 1 << 11,
        GRAVEL = 1 << 12,
        EARTH = 1 << 13,
        NOCOL = 1 << 14,
        LAVA = 1 << 15,
        CLIMB = 1 << 16,
        FALL = 1 << 17,
        NOPATH = 1 << 18,
        NODRAW = 1 << 19,
        PRECISE_PATH = 1 << 20,
        NO_CLIMB = 1 << 21,
        ANGULAR = 1 << 22,
        ANGULAR_IDX0 = 1 << 23,
        ANGULAR_IDX1 = 1 << 24,
        ANGULAR_IDX2 = 1 << 25,
        ANGULAR_IDX3 = 1 << 26,
        LATE_MIP = 1 << 27,
    }

    [DebuggerDisplay("FACE: {FaceType}")]
    public struct E_FACE
    {
        public int FaceType;  // 0 = flat, 1 = text, 2 = Double-Side
        public short TexId;
        public Vector3<ushort> Vid;
        public Vector3 U;
        public Vector3 V;

        public float TransVal;
        public Vector3 Norm;
        public Vector3[] Nrmls;
        public float Temp;

        public Vector3<short> Ou;
        public Vector3<short> Ov;
        public Vector2[] Color;
    }

    //#define MAX_PFACE 16
    //struct E_PFACE
    //{
    //    //short faceidx[MAX_PFACE];
    //    //int facetype;
    //    //short texid;  //long
    //    //short nbvert;
    //    //float transval;
    //    //ushort vid[MAX_PFACE];
    //    //float u[MAX_PFACE];
    //    //float v[MAX_PFACE];
    //    //D3DCOLOR color[MAX_PFACE];
    //}

    [StructLayout(LayoutKind.Sequential)]
    public struct TLVERTEX
    {
        public Vector3 S;           // Screen coordinates
        public float Rhw;           // Reciprocal of homogeneous w
        public uint Color;          // Vertex color
        public uint Specular;       // Specular component of vertex
        public Vector2 T;           // Texture coordinates
    }

    ////***********************************************************************
    ////*		BEGIN EERIE OBJECT STRUCTURES									*
    ////***********************************************************************
    //struct
    //{

    //    short nb_Nvertex;
    //short nb_Nfaces;
    //short* Nvertex;
    //short* Nfaces;
    //} NEIGHBOURS_DATA; // Aligned 1 2 4

    public struct PROGRESSIVE_DATA // Aligned 1 2 4
    {
        // ingame data
        public short ActualCollapse; // -1 = no collapse
        public short NeedComputing;
        public float CollapseRatio;
        // static data
        public float CollapseCost;
        public short CollapseCandidate;
        public short Padd;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct E_SPRINGS
    {
        public short startidx;
        public short endidx;
        public float restlength;
        public float constant; // spring constant
        public float damping;  // spring damping
        public long type;
    }

    //#define CLOTHES_FLAG_NORMAL	0
    //#define CLOTHES_FLAG_FIX	1
    //#define CLOTHES_FLAG_NOCOL	2

    [StructLayout(LayoutKind.Sequential)]
    public struct CLOTHESVERTEX
    {
        public short idx;
        public byte flags;
        public byte coll;
        public Vector3 pos;
        public Vector3 velocity;
        public Vector3 force;
        public float mass; // 1.f/mass
        //
        public Vector3 t_pos;
        public Vector3 t_velocity;
        public Vector3 t_force;
        //
        public Vector3 lastpos;
    }

    public struct CLOTHES_DATA
    {
        public CLOTHESVERTEX[] Cvert;
        //public CLOTHESVERTEX[] backup;
        public short NumCvert;
        public short NumSprings;
        public E_SPRINGS[] Springs;
    }

    public struct COLLISION_SPHERE
    {
        public short idx;
        public short flags;
        public float radius;
    }

    public struct COLLISION_SPHERES_DATA
    {
        public int NumSpheres;
        public COLLISION_SPHERE[] Spheres;
    }

    //struct
    //{

    //    Vector3 initpos;
    //Vector3 temp;
    //Vector3 pos;
    //Vector3 velocity;
    //Vector3 force;
    //Vector3 inertia;
    //float mass;
    //} PHYSVERT; // Aligned 1 2 4

    //struct
    //{

    //    PHYSVERT* vert;
    //long nb_physvert;
    //short active;
    //short stopcount;
    //float radius; //radius around vert[0].pos for spherical collision
    //float storedtiming;
    //} PHYSICS_BOX_DATA; // Aligned 1 2 4


    //struct
    //{

    //    long sx;
    //long sy;
    //unsigned long bpp;
    //unsigned char* bmpdata;
    //} EERIE_MAP; // Aligned 1 2 4


    [DebuggerDisplay("Group: {Name}")]
    public struct E_GROUPLIST
    {
        public string Name;
        public int Origin;
        public int NumIndex;
        public int[] Indexes;
        public float Size;
    }

    [DebuggerDisplay("Action: {Name}")]
    public struct E_ACTIONLIST
    {
        public string Name;
        public int Idx; //index vertex;
        public int Act; //action
        public int Sfx; //sfx
    }

    //struct
    //{

    //    float xmin;
    //float xmax;
    //float ymin;
    //float ymax;
    //float zmin;
    //float zmax;
    //} CUB3D; // Aligned 1 2 4

    //struct
    //{

    //    long link_origin;
    //Vector3 link_position;
    //Vector3 scale;
    //Vector3 rot;
    //unsigned long flags;
    //} EERIE_MOD_INFO; // Aligned 1 2 4

    //struct
    //{

    //    long lgroup; //linked to group n� if lgroup=-1 NOLINK
    //long lidx;
    //long lidx2;
    //void* obj;
    //EERIE_MOD_INFO modinfo;
    //void* io;
    //} EERIE_LINKED; // Aligned 1 2 4


    [DebuggerDisplay("Selection: {Name}")]
    public unsafe struct E_SELECTIONS
    {
        public string Name;
        public int NumSelected;
        public int[] Selected;
    }

    //#define DRAWFLAG_HIGHLIGHT	1

    //struct
    //{

    //    short view_attach;
    //short primary_attach;

    //short left_attach;
    //short weapon_attach;

    //short secondary_attach;
    //short mouth_group;

    //short jaw_group;
    //short head_group_origin;

    //short head_group;
    //short mouth_group_origin;

    //short V_right;
    //short U_right;

    //short fire;
    //short sel_head;

    //short sel_chest;
    //short sel_leggings;

    //short carry_attach;
    //short __padd;
    //} EERIE_FASTACCESS;

    ///////////////////////////////////////////////////////////////////////////////////
    //struct
    //{

    //    long nb_idxvertices;
    //long* idxvertices;
    //EERIE_GROUPLIST* original_group;
    //long father;
    //EERIE_QUAT quatanim;
    //Vector3 transanim;
    //Vector3 scaleanim;
    //EERIE_QUAT quatlast;
    //Vector3 translast;
    //Vector3 scalelast;
    //EERIE_QUAT quatinit;
    //Vector3 transinit;
    //Vector3 scaleinit;
    //Vector3 transinit_global;
    //} EERIE_BONE;

    //struct
    //{

    //    EERIE_BONE* bones;
    //long nb_bones;
    //} EERIE_C_DATA;
    ////////////////////////////////////////////////////////////////////////////////////
    //struct
    //{

    //    float x;
    //float y;
    //float z;
    //float w;
    //} EERIE_3DPAD;

    [DebuggerDisplay("Texture: {Path}")]
    public class E_TEXTURE
    {
        public int Id;
        public string Path;
        public POLY Poly;
    }

    [DebuggerDisplay("Obj: {File}")]
    public class E_3DOBJ
    {
        //public string Name;
        public string File;
        //public Vector3 Pos;
        public Vector3 Point0;
        //public Vector3 Angle;
        public int Origin;
        //public int Ident;
        public int NumVertex;
        //public int TrueNumVertex;
        public int NumFaces;
        public int NumPfaces;
        public int NumMaps;
        public int NumGroups;
        public int NumAction;
        public int NumSelections;
        //public uint DrawFlags;
        //public EERIE_3DPAD* VertexLocal;
        public E_VERTEX[] VertexList;
        //public E_VERTEX[] VertexList3;

        public E_FACE[] FaceList;
        //public EERIE_PFACE* PfaceList;
        //public EERIE_MAP* MapList;
        public E_GROUPLIST[] GroupList;
        public E_ACTIONLIST[] ActionList;
        public E_SELECTIONS[] Selections;
        public E_TEXTURE[] Textures;

        //public char* OriginalTextures;
        //public CUB3D Cub;
        //public EERIE_QUAT Quat;
        //public EERIE_LINKED* Linked;
        //public int NumLinked;

        //public PHYSICS_BOX_DATA Pbox;
        //public PROGRESSIVE_DATA Pdata;
        //public NEIGHBOURS_DATA Ndata;
        public CLOTHES_DATA Cdata;
        public COLLISION_SPHERES_DATA Sdata;
        //public EERIE_FASTACCESS FastAccess;
        //public EERIE_C_DATA* C_data;
    }


    //struct
    //{

    //    long nbobj;
    //EERIE_3DOBJ** objs;
    //Vector3 pos;
    //Vector3 point0;
    //long nbtex;
    //TextureContainer** texturecontainer;
    //long nblight;
    //EERIE_LIGHT** light;
    //float ambient_r;
    //float ambient_g;
    //float ambient_b;
    //CUB3D cub;
    //} EERIE_3DSCENE; // Aligned 1 2 4


    //#define MAX_SCENES 64
    //struct
    //{

    //    long nb_scenes;
    //EERIE_3DSCENE* scenes[MAX_SCENES];
    //CUB3D cub;
    //Vector3 pos;
    //Vector3 point0;
    //} EERIE_MULTI3DSCENE; // Aligned 1 2 4


    //struct
    //{

    //    long num_frame;
    //long flag;
    //int master_key_frame;
    //short f_translate; //int
    //short f_rotate; //int
    //float time;
    //Vector3 translate;
    //EERIE_QUAT quat;
    //long sample;
    //} EERIE_FRAME; // Aligned 1 2 4



    //struct
    //{

    //    int key;
    //Vector3 translate;
    //EERIE_QUAT quat;
    //Vector3 zoom;
    //} EERIE_GROUP; // Aligned 1 2 4

    //// Animation playing flags
    //#define EA_LOOP			1	// Must be looped at end (indefinitely...)
    //#define EA_REVERSE		2	// Is played reversed (from end to start)
    //#define EA_PAUSED		4	// Is paused
    //#define EA_ANIMEND		8	// Has just finished
    //#define	EA_STATICANIM	16	// Is a static Anim (no movement offset returned).
    //#define	EA_STOPEND		32	// Must Be Stopped at end.
    //#define EA_FORCEPLAY	64	// User controlled... MUST be played...
    //#define EA_EXCONTROL	128	// ctime externally set, no update.
    //struct
    //{

    //    float anim_time;
    //unsigned long flag;
    //long nb_groups;
    //long nb_key_frames;
    //EERIE_FRAME* frames;
    //EERIE_GROUP* groups;
    //unsigned char* voidgroups;
    //} EERIE_ANIM; // Aligned 1 2 4

    ////-------------------------------------------------------------------------
    //Portal Data;

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SAVE_EERIEPOLY
    {
        public static (string, int) Struct = ("<?", sizeof(SAVE_EERIEPOLY));
        public POLY Type;  // at least 16 bits
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 Norm;
        public Vector3 Norm2;
        public TLVERTEX V0; public TLVERTEX V1; public TLVERTEX V2; public TLVERTEX V3;
        public TLVERTEX Tv0; public TLVERTEX Tv1; public TLVERTEX Tv2; public TLVERTEX Tv3;
        public Vector3 Nrml0; public Vector3 Nrml1; public Vector3 Nrml2; public Vector3 Nrml3;
        public int TexPtr;
        public Vector3 Center;
        public float TransVal;
        public float Area;
        public short Room;
        public short Misc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct E_SAVE_PORTALS
    {
        public static (string, int) Struct = ("<?", sizeof(E_SAVE_PORTALS));
        public SAVE_EERIEPOLY Poly;
        public int Room1; // facing normal
        public int Room2;
        public short UsePortal;
        public short Paddy;
    }

    public unsafe struct E_PORTALS
    {
        public static (string, int) Struct = ("<?", sizeof(E_PORTALS));
        public E_POLY Poly;
        public int Room1; // facing normal
        public int Room2;
        public short UsePortal;
        public short Paddy;

        internal void memset()
        {
        }
    }

    public struct EP_DATA
    {
        public short Px;
        public short Py;
        public short Idx;
        public short Padd;
    }

    public class E_ROOM_DATA
    {
        public int NumPortals;
        public int[] Portals;
        public int NumPolys;
        public EP_DATA[] EpData;
        public Vector3 Center;
        public float Radius;
        public ushort[] PussIndice;
        //public LPDIRECT3DVERTEXBUFFER7 VertexBuffer;
        public uint NumTextures;
        public E_TEXTURE TextureContainer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct E_SAVE_ROOM_DATA
    {
        public static (string, int) Struct = ("<2i6i", sizeof(E_SAVE_ROOM_DATA));
        public int NumPortals;
        public int NumPolys;
        public fixed int Padd[6];
    }

    public class E_PORTAL_DATA
    {
        public int NumRooms;
        public E_ROOM_DATA[] Room;
        public int NumTotal;  // of portals
        public E_PORTALS[] Portals;
    }

    //#define ARX_D3DVERTEX D3DTLVERTEX


    //struct
    //{

    //    float x, y, z;
    //int color;
    //float tu, tv;
    //} SMY_D3DVERTEX;

    //struct
    //{

    //    float x, y, z;
    //int color;
    //float tu, tv;
    //float tu2, tv2;
    //float tu3, tv3;
    //} SMY_D3DVERTEX3;

    //struct
    //{

    //    float x, y, z;
    //float rhw;
    //int color;
    //float tu, tv;
    //float tu2, tv2;
    //float tu3, tv3;
    //} SMY_D3DVERTEX3_T;

    //struct
    //{

    //    D3DTLVERTEX pD3DVertex[3];
    //float uv[6];
    //float color[3];
    //} SMY_ZMAPPINFO;

    //struct
    //{

    //    unsigned long uslStartVertex;
    //unsigned long uslNbVertex;

    //unsigned long uslStartCull;
    //unsigned long uslNbIndiceCull;
    //unsigned long uslStartNoCull;
    //unsigned long uslNbIndiceNoCull;

    //unsigned long uslStartCull_TNormalTrans;
    //unsigned long uslNbIndiceCull_TNormalTrans;
    //unsigned long uslStartNoCull_TNormalTrans;
    //unsigned long uslNbIndiceNoCull_TNormalTrans;

    //unsigned long uslStartCull_TMultiplicative;
    //unsigned long uslNbIndiceCull_TMultiplicative;
    //unsigned long uslStartNoCull_TMultiplicative;
    //unsigned long uslNbIndiceNoCull_TMultiplicative;

    //unsigned long uslStartCull_TAdditive;
    //unsigned long uslNbIndiceCull_TAdditive;
    //unsigned long uslStartNoCull_TAdditive;
    //unsigned long uslNbIndiceNoCull_TAdditive;

    //unsigned long uslStartCull_TSubstractive;
    //unsigned long uslNbIndiceCull_TSubstractive;
    //unsigned long uslStartNoCull_TSubstractive;
    //unsigned long uslNbIndiceNoCull_TSubstractive;
    //} SMY_ARXMAT;

    //class CMY_DYNAMIC_VERTEXBUFFER
    //{
    //    public:
    //		unsigned long uslFormat;
    //    unsigned short ussMaxVertex;
    //    unsigned short ussNbVertex;
    //    unsigned short ussNbIndice;
    //    LPDIRECT3DVERTEXBUFFER7 pVertexBuffer;
    //    unsigned short* pussIndice;
    //    public:
    //		CMY_DYNAMIC_VERTEXBUFFER(unsigned short, unsigned long);
    //    ~CMY_DYNAMIC_VERTEXBUFFER();

    //    void* Lock(unsigned int);
    //    bool UnLock();
    //};

    //#define FVF_D3DVERTEX	(D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX1|D3DFVF_TEXTUREFORMAT2)
    //#define FVF_D3DVERTEX2	(D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX2|D3DFVF_TEXTUREFORMAT2)
    //#define FVF_D3DVERTEX3	(D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX3|D3DFVF_TEXTUREFORMAT2)

    //#define FVF_D3DVERTEX_T		(D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX1|D3DFVF_TEXTUREFORMAT2)
    //#define FVF_D3DVERTEX2_T	(D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX2|D3DFVF_TEXTUREFORMAT2)
    //#define FVF_D3DVERTEX3_T	(D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX3|D3DFVF_TEXTUREFORMAT2)

    //extern long USE_PORTALS;
    //extern EERIE_PORTAL_DATA* portals;
}