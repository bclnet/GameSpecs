using System;
using System.NumericsX;
using System.NumericsX.OpenStack;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace Gengine.Render
{
    #region IDs

    static partial class ModelXLwo
    {
        // chunk and subchunk IDs
        public const int ID_FORM = 'F' << 24 | 'O' << 16 | 'R' << 8 | 'M';
        public const int ID_LWO2 = 'L' << 24 | 'W' << 16 | 'O' << 8 | '2';
        public const int ID_LWOB = 'L' << 24 | 'W' << 16 | 'O' << 8 | 'B';

        // top-level chunks				
        public const int ID_LAYR = 'L' << 24 | 'A' << 16 | 'Y' << 8 | 'R';
        public const int ID_TAGS = 'T' << 24 | 'A' << 16 | 'G' << 8 | 'S';
        public const int ID_PNTS = 'P' << 24 | 'N' << 16 | 'T' << 8 | 'S';
        public const int ID_BBOX = 'B' << 24 | 'B' << 16 | 'O' << 8 | 'X';
        //public const int ID_VMAP = 'V' << 24 | 'M' << 16 | 'A' << 8 | 'P';
        public const int ID_VMAD = 'V' << 24 | 'M' << 16 | 'A' << 8 | 'D';
        public const int ID_POLS = 'P' << 24 | 'O' << 16 | 'L' << 8 | 'S';
        public const int ID_PTAG = 'P' << 24 | 'T' << 16 | 'A' << 8 | 'G';
        public const int ID_ENVL = 'E' << 24 | 'N' << 16 | 'V' << 8 | 'L';
        public const int ID_CLIP = 'C' << 24 | 'L' << 16 | 'I' << 8 | 'P';
        //public const int ID_SURF = 'S' << 24 | 'U' << 16 | 'R' << 8 | 'F';
        public const int ID_DESC = 'D' << 24 | 'E' << 16 | 'S' << 8 | 'C';
        public const int ID_TEXT = 'T' << 24 | 'E' << 16 | 'X' << 8 | 'T';
        public const int ID_ICON = 'I' << 24 | 'C' << 16 | 'O' << 8 | 'N';

        // polygon types		          
        public const int ID_FACE = 'F' << 24 | 'A' << 16 | 'C' << 8 | 'E';
        public const int ID_CURV = 'C' << 24 | 'U' << 16 | 'R' << 8 | 'V';
        public const int ID_PTCH = 'P' << 24 | 'T' << 16 | 'C' << 8 | 'H';
        public const int ID_MBAL = 'M' << 24 | 'B' << 16 | 'A' << 8 | 'L';
        public const int ID_BONE = 'B' << 24 | 'O' << 16 | 'N' << 8 | 'E';

        // polygon tags			          
        public const int ID_SURF = 'S' << 24 | 'U' << 16 | 'R' << 8 | 'F';
        public const int ID_PART = 'P' << 24 | 'A' << 16 | 'R' << 8 | 'T';
        public const int ID_SMGP = 'S' << 24 | 'M' << 16 | 'G' << 8 | 'P';

        // envelopes			          
        public const int ID_PRE = 'P' << 24 | 'R' << 16 | 'E' << 8 | ' ';
        public const int ID_POST = 'P' << 24 | 'O' << 16 | 'S' << 8 | 'T';
        public const int ID_KEY = 'K' << 24 | 'E' << 16 | 'Y' << 8 | ' ';
        public const int ID_SPAN = 'S' << 24 | 'P' << 16 | 'A' << 8 | 'N';
        public const int ID_TCB = 'T' << 24 | 'C' << 16 | 'B' << 8 | ' ';
        public const int ID_HERM = 'H' << 24 | 'E' << 16 | 'R' << 8 | 'M';
        public const int ID_BEZI = 'B' << 24 | 'E' << 16 | 'Z' << 8 | 'I';
        public const int ID_BEZ2 = 'B' << 24 | 'E' << 16 | 'Z' << 8 | '2';
        public const int ID_LINE = 'L' << 24 | 'I' << 16 | 'N' << 8 | 'E';
        public const int ID_STEP = 'S' << 24 | 'T' << 16 | 'E' << 8 | 'P';

        // clips				          
        public const int ID_STIL = 'S' << 24 | 'T' << 16 | 'I' << 8 | 'L';
        public const int ID_ISEQ = 'I' << 24 | 'S' << 16 | 'E' << 8 | 'Q';
        public const int ID_ANIM = 'A' << 24 | 'N' << 16 | 'I' << 8 | 'M';
        public const int ID_XREF = 'X' << 24 | 'R' << 16 | 'E' << 8 | 'F';
        public const int ID_STCC = 'S' << 24 | 'T' << 16 | 'C' << 8 | 'C';
        public const int ID_TIME = 'T' << 24 | 'I' << 16 | 'M' << 8 | 'E';
        public const int ID_CONT = 'C' << 24 | 'O' << 16 | 'N' << 8 | 'T';
        public const int ID_BRIT = 'B' << 24 | 'R' << 16 | 'I' << 8 | 'T';
        public const int ID_SATR = 'S' << 24 | 'A' << 16 | 'T' << 8 | 'R';
        public const int ID_HUE = 'H' << 24 | 'U' << 16 | 'E' << 8 | ' ';
        public const int ID_GAMM = 'G' << 24 | 'A' << 16 | 'M' << 8 | 'M';
        public const int ID_NEGA = 'N' << 24 | 'E' << 16 | 'G' << 8 | 'A';
        public const int ID_IFLT = 'I' << 24 | 'F' << 16 | 'L' << 8 | 'T';
        public const int ID_PFLT = 'P' << 24 | 'F' << 16 | 'L' << 8 | 'T';

        // surfaces				          
        //public const int ID_COLR = 'C' << 24 | 'O' << 16 | 'L' << 8 | 'R';
        public const int ID_LUMI = 'L' << 24 | 'U' << 16 | 'M' << 8 | 'I';
        public const int ID_DIFF = 'D' << 24 | 'I' << 16 | 'F' << 8 | 'F';
        public const int ID_SPEC = 'S' << 24 | 'P' << 16 | 'E' << 8 | 'C';
        public const int ID_GLOS = 'G' << 24 | 'L' << 16 | 'O' << 8 | 'S';
        public const int ID_REFL = 'R' << 24 | 'E' << 16 | 'F' << 8 | 'L';
        public const int ID_RFOP = 'R' << 24 | 'F' << 16 | 'O' << 8 | 'P';
        public const int ID_RIMG = 'R' << 24 | 'I' << 16 | 'M' << 8 | 'G';
        public const int ID_RSAN = 'R' << 24 | 'S' << 16 | 'A' << 8 | 'N';
        public const int ID_TRAN = 'T' << 24 | 'R' << 16 | 'A' << 8 | 'N';
        public const int ID_TROP = 'T' << 24 | 'R' << 16 | 'O' << 8 | 'P';
        public const int ID_TIMG = 'T' << 24 | 'I' << 16 | 'M' << 8 | 'G';
        public const int ID_RIND = 'R' << 24 | 'I' << 16 | 'N' << 8 | 'D';
        public const int ID_TRNL = 'T' << 24 | 'R' << 16 | 'N' << 8 | 'L';
        public const int ID_BUMP = 'B' << 24 | 'U' << 16 | 'M' << 8 | 'P';
        public const int ID_SMAN = 'S' << 24 | 'M' << 16 | 'A' << 8 | 'N';
        public const int ID_SIDE = 'S' << 24 | 'I' << 16 | 'D' << 8 | 'E';
        public const int ID_CLRH = 'C' << 24 | 'L' << 16 | 'R' << 8 | 'H';
        public const int ID_CLRF = 'C' << 24 | 'L' << 16 | 'R' << 8 | 'F';
        public const int ID_ADTR = 'A' << 24 | 'D' << 16 | 'T' << 8 | 'R';
        public const int ID_SHRP = 'S' << 24 | 'H' << 16 | 'R' << 8 | 'P';
        //public const int ID_LINE = 'L' << 24 | 'I' << 16 | 'N' << 8 | 'E';
        public const int ID_LSIZ = 'L' << 24 | 'S' << 16 | 'I' << 8 | 'Z';
        public const int ID_ALPH = 'A' << 24 | 'L' << 16 | 'P' << 8 | 'H';
        public const int ID_AVAL = 'A' << 24 | 'V' << 16 | 'A' << 8 | 'L';
        public const int ID_GVAL = 'G' << 24 | 'V' << 16 | 'A' << 8 | 'L';
        public const int ID_BLOK = 'B' << 24 | 'L' << 16 | 'O' << 8 | 'K';

        // texture layer		          
        public const int ID_TYPE = 'T' << 24 | 'Y' << 16 | 'P' << 8 | 'E';
        public const int ID_CHAN = 'C' << 24 | 'H' << 16 | 'A' << 8 | 'N';
        public const int ID_NAME = 'N' << 24 | 'A' << 16 | 'M' << 8 | 'E';
        public const int ID_ENAB = 'E' << 24 | 'N' << 16 | 'A' << 8 | 'B';
        public const int ID_OPAC = 'O' << 24 | 'P' << 16 | 'A' << 8 | 'C';
        public const int ID_FLAG = 'F' << 24 | 'L' << 16 | 'A' << 8 | 'G';
        public const int ID_PROJ = 'P' << 24 | 'R' << 16 | 'O' << 8 | 'J';
        public const int ID_STCK = 'S' << 24 | 'T' << 16 | 'C' << 8 | 'K';
        public const int ID_TAMP = 'T' << 24 | 'A' << 16 | 'M' << 8 | 'P';

        // texture coordinates	          
        public const int ID_TMAP = 'T' << 24 | 'M' << 16 | 'A' << 8 | 'P';
        public const int ID_AXIS = 'A' << 24 | 'X' << 16 | 'I' << 8 | 'S';
        public const int ID_CNTR = 'C' << 24 | 'N' << 16 | 'T' << 8 | 'R';
        public const int ID_SIZE = 'S' << 24 | 'I' << 16 | 'Z' << 8 | 'E';
        public const int ID_ROTA = 'R' << 24 | 'O' << 16 | 'T' << 8 | 'A';
        public const int ID_OREF = 'O' << 24 | 'R' << 16 | 'E' << 8 | 'F';
        public const int ID_FALL = 'F' << 24 | 'A' << 16 | 'L' << 8 | 'L';
        public const int ID_CSYS = 'C' << 24 | 'S' << 16 | 'Y' << 8 | 'S';

        // image map				      
        public const int ID_IMAP = 'I' << 24 | 'M' << 16 | 'A' << 8 | 'P';
        public const int ID_IMAG = 'I' << 24 | 'M' << 16 | 'A' << 8 | 'G';
        public const int ID_WRAP = 'W' << 24 | 'R' << 16 | 'A' << 8 | 'P';
        public const int ID_WRPW = 'W' << 24 | 'R' << 16 | 'P' << 8 | 'W';
        public const int ID_WRPH = 'W' << 24 | 'R' << 16 | 'P' << 8 | 'H';
        public const int ID_VMAP = 'V' << 24 | 'M' << 16 | 'A' << 8 | 'P';
        public const int ID_AAST = 'A' << 24 | 'A' << 16 | 'S' << 8 | 'T';
        public const int ID_PIXB = 'P' << 24 | 'I' << 16 | 'X' << 8 | 'B';

        // procedural			         
        public const int ID_PROC = 'P' << 24 | 'R' << 16 | 'O' << 8 | 'C';
        public const int ID_COLR = 'C' << 24 | 'O' << 16 | 'L' << 8 | 'R';
        public const int ID_VALU = 'V' << 24 | 'A' << 16 | 'L' << 8 | 'U';
        public const int ID_FUNC = 'F' << 24 | 'U' << 16 | 'N' << 8 | 'C';
        public const int ID_FTPS = 'F' << 24 | 'T' << 16 | 'P' << 8 | 'S';
        public const int ID_ITPS = 'I' << 24 | 'T' << 16 | 'P' << 8 | 'S';
        public const int ID_ETPS = 'E' << 24 | 'T' << 16 | 'P' << 8 | 'S';

        // gradient				          
        public const int ID_GRAD = 'G' << 24 | 'R' << 16 | 'A' << 8 | 'D';
        public const int ID_GRST = 'G' << 24 | 'R' << 16 | 'S' << 8 | 'T';
        public const int ID_GREN = 'G' << 24 | 'R' << 16 | 'E' << 8 | 'N';
        public const int ID_PNAM = 'P' << 24 | 'N' << 16 | 'A' << 8 | 'M';
        public const int ID_INAM = 'I' << 24 | 'N' << 16 | 'A' << 8 | 'M';
        public const int ID_GRPT = 'G' << 24 | 'R' << 16 | 'P' << 8 | 'T';
        public const int ID_FKEY = 'F' << 24 | 'K' << 16 | 'E' << 8 | 'Y';
        public const int ID_IKEY = 'I' << 24 | 'K' << 16 | 'E' << 8 | 'Y';

        // shader				          
        public const int ID_SHDR = 'S' << 24 | 'H' << 16 | 'D' << 8 | 'R';
        public const int ID_DATA = 'D' << 24 | 'A' << 16 | 'T' << 8 | 'A';
    }

    #endregion

    #region Records

    // generic linked list
    public class lwNode
    {
        public lwNode next, prev;
    }

    // plug-in reference
    public class lwPlugin : lwNode
    {
        public string ord;
        public string name;
        public int flags;
        public object data;
    }

    // envelopes
    public class lwKey : lwNode
    {
        public float value;
        public float time;
        public uint shape;              // ID_TCB, ID_BEZ2, etc.
        public float tension;
        public float continuity;
        public float bias;
        public float[] param = new float[4];
    }

    public class lwEnvelope : lwNode
    {
        public int index;
        public int type;
        public string name;
        public lwKey key;               // linked list of keys
        public int nkeys;
        public int[] behavior = new int[2]; // pre and post (extrapolation)
        public lwPlugin cfilter;        // linked list of channel filters
        public int ncfilters;
    }

    static partial class ModelXLwo
    {
        public const int BEH_RESET = 0;
        public const int BEH_CONSTANT = 1;
        public const int BEH_REPEAT = 2;
        public const int BEH_OSCILLATE = 3;
        public const int BEH_OFFSET = 4;
        public const int BEH_LINEAR = 5;
    }

    // values that can be enveloped
    public struct lwEParam
    {
        public float val;
        public int eindex;
    }

    public unsafe struct lwVParam
    {
        public fixed float val[3];
        public int eindex;
    }

    // clips
    public struct lwClipStill
    {
        public string name;
    }

    public struct lwClipSeq
    {
        public string prefix;           // filename before sequence digits
        public string suffix;           // after digits, e.g. extensions
        public int digits;
        public int flags;
        public int offset;
        public int start;
        public int end;
    }

    public struct lwClipAnim
    {
        public string name;
        public string server;           // anim loader plug-in
        public object data;
    }

    public struct lwClipXRef
    {
        public string s;
        public int index;
        public lwClip clip;
    }

    public struct lwClipCycle
    {
        public string name;
        public int lo;
        public int hi;
    }

    [StructLayout(LayoutKind.Explicit)] //: UNION
    public struct lwClip_source
    {
        [FieldOffset(0)] public lwClipStill still;
        [FieldOffset(0)] public lwClipSeq seq;
        [FieldOffset(0)] public lwClipAnim anim;
        [FieldOffset(0)] public lwClipXRef xref;
        [FieldOffset(0)] public lwClipCycle cycle;
    }
    public class lwClip : lwNode
    {
        public int index;
        public uint type;               // ID_STIL, ID_ISEQ, etc.
        public lwClip_source source;
        public float start_time;
        public float duration;
        public float frame_rate;
        public lwEParam contrast;
        public lwEParam brightness;
        public lwEParam saturation;
        public lwEParam hue;
        public lwEParam gamma;
        public int negative;
        public lwPlugin ifilter;        // linked list of image filters
        public int nifilters;
        public lwPlugin pfilter;        // linked list of pixel filters
        public int npfilters;
    }

    // textures
    public struct lwTMap
    {
        public lwVParam size;
        public lwVParam center;
        public lwVParam rotate;
        public lwVParam falloff;
        public int fall_type;
        public string ref_object;
        public int coord_sys;
    }

    public struct lwImageMap
    {
        public int cindex;
        public int projection;
        public string vmap_name;
        public int axis;
        public int wrapw_type;
        public int wraph_type;
        public lwEParam wrapw;
        public lwEParam wraph;
        public float aa_strength;
        public int aas_flags;
        public int pblend;
        public lwEParam stck;
        public lwEParam amplitude;
    }

    static partial class ModelXLwo
    {
        public const int PROJ_PLANAR = 0;
        public const int PROJ_CYLINDRICAL = 1;
        public const int PROJ_SPHERICAL = 2;
        public const int PROJ_CUBIC = 3;
        public const int PROJ_FRONT = 4;

        public const int WRAP_NONE = 0;
        public const int WRAP_EDGE = 1;
        public const int WRAP_REPEAT = 2;
        public const int WRAP_MIRROR = 3;
    }

    public unsafe struct lwProcedural
    {
        public int axis;
        public fixed float value[3];
        public string name;
        public object data;
    }

    public class lwGradKey : lwNode
    {
        public float value;
        public float[] rgba = new float[4];
    }

    public struct lwGradient
    {
        public string paramname;
        public string itemname;
        public float start;
        public float end;
        public int repeat;
        public lwGradKey[] key;         // array of gradient keys
        public short[] ikey;            // array of interpolation codes
    }

    [StructLayout(LayoutKind.Explicit)] //: UNION
    public struct lwTexture_param
    {
        [FieldOffset(0)] public lwImageMap imap;
        [FieldOffset(0)] public lwProcedural proc;
        [FieldOffset(0)] public lwGradient grad;
    }
    public class lwTexture : lwNode
    {
        public string ord;
        public uint type;
        public uint chan;
        public lwEParam opacity;
        public short opac_type;
        public short enabled;
        public short negative;
        public short axis;
        public lwTexture_param param;
        public lwTMap tmap;
    }

    // values that can be textured
    public struct lwTParam
    {
        public float val;
        public int eindex;
        public lwTexture tex;           // linked list of texture layers
    }

    public unsafe struct lwCParam
    {
        public fixed float rgb[3];
        public int eindex;
        public lwTexture tex;           // linked list of texture layers
    }

    // surfaces
    public struct lwGlow
    {
        public short enabled;
        public short type;
        public lwEParam intensity;
        public lwEParam size;
    }

    public struct lwRMap
    {
        public lwTParam val;
        public int options;
        public int cindex;
        public float seam_angle;
    }

    public struct lwLine
    {
        public short enabled;
        public ushort flags;
        public lwEParam size;
    }

    public class lwSurface : lwNode
    {
        public string name;
        public string srcname;
        public lwCParam color;
        public lwTParam luminosity;
        public lwTParam diffuse;
        public lwTParam specularity;
        public lwTParam glossiness;
        public lwRMap reflection;
        public lwRMap transparency;
        public lwTParam eta;
        public lwTParam translucency;
        public lwTParam bump;
        public float smooth;
        public int sideflags;
        public float alpha;
        public int alpha_mode;
        public lwEParam color_hilite;
        public lwEParam color_filter;
        public lwEParam add_trans;
        public lwEParam dif_sharp;
        public lwEParam glow;
        public lwLine line;
        public lwPlugin shader;         // linked list of shaders
        public int nshaders;
    }

    // vertex maps
    public class lwVMap : lwNode
    {
        public string name;
        public uint type;
        public int dim;
        public int nverts;
        public bool perpoly;
        public int[] vindex;            // array of point indexes 
        public int[] pindex;            // array of polygon indexes
        public float[] val;

        // added by duffy
        public int offset;
    }

    public struct lwVMapPt
    {
        public lwVMap vmap;
        public int index;               // vindex or pindex element
    }

    // points and polygons
    public unsafe struct lwPoint
    {
        public fixed float pos[3];
        public int npols;               // number of polygons sharing the point
        public int[] pol;               // array of polygon indexes
        public int nvmaps;
        public lwVMapPt[] vm;           // array of vmap references
    }

    public unsafe struct lwPolVert
    {
        public int index;               // index into the point array
        public fixed float norm[3];
        public int nvmaps;
        public lwVMapPt[] vm;           // array of vmap references
    }

    public unsafe struct lwPolygon
    {
        public lwSurface surf;
        public int part;                // part index
        public int smoothgrp;           // smoothing group
        public int flags;
        public uint type;
        public fixed float norm[3];
        public int nverts;
        public lwPolVert[] v_;          // array of vertex records
        public Memory<lwPolVert> v;     // Memory<array> of vertex records
    }

    public class lwPointList
    {
        public int count;
        public int offset;              // only used during reading
        public lwPoint[] pt;            // array of points
    }

    public class lwPolygonList
    {
        public int count;
        public int offset;              // only used during reading
        public int vcount;              // total number of vertices
        public int voffset;             // only used during reading
        public lwPolygon[] pol;         // array of polygons
    }

    // geometry layers
    public class lwLayer : lwNode
    {
        public string name;
        public int index;
        public int parent;
        public int flags;
        public float[] pivot = new float[3];
        public float[] bbox = new float[6];
        public lwPointList point;
        public lwPolygonList polygon;
        public int nvmaps;
        public lwVMap vmap;             // linked list of vmaps
    }

    // tag strings
    public struct lwTagList
    {
        public int count;
        public int offset;              // only used during reading
        public string[] tag;            // array of strings
    }

    // an object
    public class lwObject
    {
        public DateTime timeStamp;
        public lwLayer layer;           // linked list of layers
        public lwEnvelope env;          // linked list of envelopes
        public lwClip clip;             // linked list of clips
        public lwSurface surf;          // linked list of surfaces
        public lwTagList taglist;
        public int nlayers;
        public int nenvs;
        public int nclips;
        public int nsurfs;
    }

    #endregion

    static unsafe partial class ModelXLwo
    {
        #region lwo2.c

        // Returns the contents of a LightWave object, given its filename, or null if the file couldn't be loaded.  On failure, failID and failpos
        // can be used to diagnose the cause.
        // 
        // 1.  If the file isn't an LWO2 or an LWOB, failpos will contain 12 and failID will be unchanged.
        // 
        // 2.  If an error occurs while reading, failID will contain the most recently read IFF chunk ID, and failpos will contain the value
        //     returned by fp.Tell() at the time of the failure.
        // 
        // 3.  If the file couldn't be opened, or an error occurs while reading the first 12 bytes, both failID and failpos will be unchanged.
        // 
        // If you don't need this information, failID and failpos can be null.
        public static lwObject lwGetObject(string filename, ref uint failID, ref int failpos)
        {
            lwObject obj; lwLayer layer; lwNode node;
            //int id, formsize, type, cksize;

            var fp = fileSystem.OpenFileRead(filename); if (fp == null) return null;

            // read the first 12 bytes
            set_flen(0);
            var id = getU4(fp);
            var formsize = getU4(fp);
            var type = getU4(fp);
            if (get_flen() != 12) { fileSystem.CloseFile(fp); return null; }

            // is this a LW object?
            if (id != ID_FORM) { fileSystem.CloseFile(fp); failpos = 12; return null; }

            if (type != ID_LWO2)
            {
                fileSystem.CloseFile(fp);
                if (type == ID_LWOB) return lwGetObject5(filename, ref failID, ref failpos);
                else { failpos = 12; return null; }
            }

            // allocate an object and a default layer
            obj = new lwObject();
            layer = new lwLayer();
            obj.layer = layer;
            obj.timeStamp = fp.Timestamp;

            // get the first chunk header
            id = getU4(fp);
            var cksize = (int)getU4(fp);
            if (get_flen() < 0) goto Fail;

            // process chunks as they're encountered
            int i, rlen;
            while (true)
            {
                cksize += cksize & 1;

                switch (id)
                {
                    case ID_LAYR:
                        if (obj.nlayers > 0) { layer = new lwLayer(); lwListAdd(ref obj.layer, layer); }
                        obj.nlayers++;

                        set_flen(0);
                        layer.index = getU2(fp);
                        layer.flags = getU2(fp);
                        layer.pivot[0] = getF4(fp);
                        layer.pivot[1] = getF4(fp);
                        layer.pivot[2] = getF4(fp);
                        layer.name = getS0(fp);

                        rlen = get_flen();
                        if (rlen < 0 || rlen > cksize) goto Fail;
                        if (rlen <= cksize - 2) layer.parent = getU2(fp);
                        rlen = get_flen();
                        if (rlen < cksize) fp.Seek(cksize - rlen, FS_SEEK.CUR);
                        break;

                    case ID_PNTS:
                        if (!lwGetPoints(fp, cksize, ref layer.point)) goto Fail;
                        break;

                    case ID_POLS:
                        if (!lwGetPolygons(fp, cksize, ref layer.polygon, layer.point.offset)) goto Fail;
                        break;

                    case ID_VMAP:
                    case ID_VMAD:
                        node = lwGetVMap(fp, cksize, layer.point.offset, layer.polygon.offset, id == ID_VMAD); if (node == null) goto Fail;
                        lwListAdd(ref layer.vmap, (lwVMap)node);
                        layer.nvmaps++;
                        break;

                    case ID_PTAG:
                        if (!lwGetPolygonTags(fp, cksize, ref obj.taglist, ref layer.polygon)) goto Fail;
                        break;

                    case ID_BBOX:
                        set_flen(0);
                        for (i = 0; i < 6; i++) layer.bbox[i] = getF4(fp);
                        rlen = get_flen();
                        if (rlen < 0 || rlen > cksize) goto Fail;
                        if (rlen < cksize) fp.Seek(cksize - rlen, FS_SEEK.CUR);
                        break;

                    case ID_TAGS:
                        if (!lwGetTags(fp, cksize, ref obj.taglist)) goto Fail;
                        break;

                    case ID_ENVL:
                        node = lwGetEnvelope(fp, cksize); if (node == null) goto Fail;
                        lwListAdd(ref obj.env, (lwEnvelope)node);
                        obj.nenvs++;
                        break;

                    case ID_CLIP:
                        node = lwGetClip(fp, cksize); if (node == null) goto Fail;
                        lwListAdd(ref obj.clip, (lwClip)node);
                        obj.nclips++;
                        break;

                    case ID_SURF:
                        node = lwGetSurface(fp, cksize); if (node == null) goto Fail;
                        lwListAdd(ref obj.surf, (lwSurface)node);
                        obj.nsurfs++;
                        break;

                    case ID_DESC:
                    case ID_TEXT:
                    case ID_ICON:
                    default:
                        fp.Seek(cksize, FS_SEEK.CUR);
                        break;
                }

                // end of the file?
                if (formsize <= fp.Tell - 8) break;

                // get the next chunk header
                set_flen(0);
                id = getU4(fp);
                cksize = (int)getU4(fp);
                if (get_flen() != 8) goto Fail;
            }

            fileSystem.CloseFile(fp); fp = null;

            if (obj.nlayers == 0) obj.nlayers = 1;

            layer = obj.layer;
            while (layer != null)
            {
                lwGetBoundingBox(layer.point, layer.bbox);
                lwGetPolyNormals(layer.point, layer.polygon);
                if (!lwGetPointPolygons(layer.point, layer.polygon)) goto Fail;
                if (!lwResolvePolySurfaces(layer.polygon, obj.taglist, obj.surf, obj.nsurfs)) goto Fail;
                lwGetVertNormals(layer.point, layer.polygon);
                if (!lwGetPointVMaps(layer.point, layer.vmap)) goto Fail;
                if (!lwGetPolyVMaps(layer.polygon, layer.vmap)) goto Fail;
                layer = (lwLayer)layer.next;
            }

            return obj;

        Fail:
            failID = id;
            if (fp != null) { failpos = fp.Tell; fileSystem.CloseFile(fp); }
            lwFreeObject(obj);
            return null;
        }

        // Free memory used by an lwObject.
        public static void lwFreeObject(lwObject o)
        {
            if (o != null)
            {
                lwListFree<lwLayer>(o.layer, lwFreeLayer);
                lwListFree<lwEnvelope>(o.env, lwFreeEnvelope);
                lwListFree<lwClip>(o.clip, lwFreeClip);
                lwListFree<lwSurface>(o.surf, lwFreeSurface);
                lwFreeTags(o.taglist);
                o = null;
            }
        }

        // Free memory used by an lwLayer.
        public static void lwFreeLayer(lwLayer layer)
        {
            if (layer != null)
            {
                if (layer.name != null) layer.name = null;
                lwFreePoints(layer.point);
                lwFreePolygons(layer.polygon);
                lwListFree<lwVMap>(layer.vmap, lwFreeVMap);
                layer = null;
            }
        }

        #endregion

        #region pntspols.c

        // Free the memory used by an lwPointList.
        public static void lwFreePoints(lwPointList point)
        {
            int i;

            if (point != null)
            {
                if (point.pt != null)
                {
                    for (i = 0; i < point.count; i++)
                    {
                        if (point.pt[i].pol != null) point.pt[i].pol = null;
                        if (point.pt[i].vm != null) point.pt[i].vm = null;
                    }
                    point.pt = null;
                }
                point.count = 0; point.offset = 0;
            }
        }

        // Free the memory used by an lwPolygonList.
        public static void lwFreePolygons(lwPolygonList plist)
        {
            int i, j;

            if (plist != null)
            {
                if (plist.pol != null)
                {
                    for (i = 0; i < plist.count; i++) if (plist.pol[i].v != null)
                            for (j = 0; j < plist.pol[i].nverts; j++) if (plist.pol[i].v[j].vm != null) plist.pol[i].v[j].vm = null;
                    if (plist.pol[0].v != null) plist.pol[0].v = null;
                    plist.pol = null;
                }
                plist.count = 0; plist.offset = 0; plist.vcount = 0; plist.voffset = 0;
            }
        }

        // Read point records from a PNTS chunk in an LWO2 file.  The points are added to the array in the lwPointList.
        public static bool lwGetPoints(VFile fp, int cksize, ref lwPointList point)
        {
            int i, j;

            if (cksize == 1) return true;

            // extend the point array to hold the new points
            var np = cksize / 12;
            point.offset = point.count;
            point.count += np;
            var oldpt = point.pt;
            point.pt = new lwPoint[point.count];
            if (oldpt != null) { Array.Copy(oldpt, point.pt, point.offset); oldpt = null; }

            // read the whole chunk
            var buf = getbytes(fp, cksize); if (buf == null) return false;
            fixed (byte* bufB = buf)
            {
                var f = (float*)bufB;
                BigRevBytes(f, 4, np * 3);

                // assign position values
                for (i = 0, j = 0; i < np; i++, j += 3)
                {
                    point.pt[i].pos[0] = f[j];
                    point.pt[i].pos[1] = f[j + 1];
                    point.pt[i].pos[2] = f[j + 2];
                }
            }

            return true;
        }

        // Calculate the bounding box for a point list, but only if the bounding box hasn't already been initialized.
        public static void lwGetBoundingBox(lwPointList point, float[] bbox)
        {
            int i, j;

            if (point.count == 0) return;

            for (i = 0; i < 6; i++) if (bbox[i] != 0f) return;

            bbox[0] = bbox[1] = bbox[2] = 1e20f;
            bbox[3] = bbox[4] = bbox[5] = -1e20f;
            for (i = 0; i < point.count; i++)
                for (j = 0; j < 3; j++)
                {
                    if (bbox[j] > point.pt[i].pos[j]) bbox[j] = point.pt[i].pos[j];
                    if (bbox[j + 3] < point.pt[i].pos[j]) bbox[j + 3] = point.pt[i].pos[j];
                }
        }

        // Allocate or extend the polygon arrays to hold new records.
        public static bool lwAllocPolygons(lwPolygonList plist, int npols, int nverts)
        {
            int i;

            plist.offset = plist.count;
            plist.count += npols;
            Array.Resize(ref plist.pol, plist.count);
            //var oldpol = plist.pol;
            //plist.pol = new lwPolygon[plist.count];
            //if (oldpol != null) { Array.Copy(oldpol, plist.pol, plist.offset); oldpol = null; }

            plist.voffset = plist.vcount;
            plist.vcount += nverts;
            Array.Resize(ref plist.pol[0].v_, plist.vcount);
            //var oldpolv = plist.pol[0].v_;
            //plist.pol[0].v_ = new lwPolVert[plist.vcount];
            //if (oldpolv != null) { Array.Copy(oldpolv, plist.pol[0].v_, plist.voffset); oldpolv = null; }

            // fix up the old vertex pointers
            plist.pol[0].v = plist.pol[0].v_.AsMemory();
            for (i = 1; i < plist.offset; i++) plist.pol[i].v = plist.pol[i - 1].v + plist.pol[i - 1].nverts;

            return true;
        }

        // Read polygon records from a POLS chunk in an LWO2 file.  The polygons are added to the array in the lwPolygonList.
        public static bool lwGetPolygons(VFile fp, int cksize, ref lwPolygonList plist, int ptoffset)
        {
            lwPolygon pp; lwPolVert pv; int i, j, flags, nv;

            if (cksize == 0) return true;

            // read the whole chunk

            set_flen(0);
            var type = getU4(fp);
            var buf = getbytes(fp, cksize - 4);
            if (get_flen() != cksize) goto Fail;

            fixed (byte* bufB = buf)
            {
                // count the polygons and vertices
                var nverts = 0;
                var npols = 0;
                var bp = bufB;

                while (bp < bufB + cksize - 4)
                {
                    nv = sgetU2(ref bp);
                    nv &= 0x03FF;
                    nverts += nv;
                    npols++;
                    for (i = 0; i < nv; i++) j = sgetVX(ref bp);
                }

                if (!lwAllocPolygons(plist, npols, nverts)) goto Fail;

                // fill in the new polygons
                lwPolygon polP = plist.pol;
                {
                    bp = bufB;
                    pp = polP + plist.offset;
                    pv = plist.pol[0].v + plist.voffset;

                    for (i = 0; i < npols; i++)
                    {
                        nv = sgetU2(ref bp);
                        flags = nv & 0xFC00;
                        nv &= 0x03FF;

                        pp->nverts = nv;
                        pp->flags = flags;
                        pp->type = type;
                        if (pp->v != null) pp->v = pv;
                        for (j = 0; j < nv; j++) pp->v[j].index = sgetVX(ref bp) + ptoffset;

                        pp++;
                        pv += nv;
                    }
                }
            }
            return true;

        Fail:
            lwFreePolygons(plist);
            return false;
        }

        // Calculate the polygon normals.  By convention, LW's polygon normals are found as the cross product of the first and last edges.It's
        // undefined for one- and two-point polygons.
        public static void lwGetPolyNormals(lwPointList point, lwPolygonList polygon)
        {
            int i, j;
            float p1[3], p2[3], pn[3], v1[3], v2[3];

            for (i = 0; i < polygon.count; i++)
            {
                if (polygon.pol[i].nverts < 3) continue;
                for (j = 0; j < 3; j++)
                {
                    // FIXME: track down why indexes are way out of range
                    p1[j] = point.pt[polygon.pol[i].v[0].index].pos[j];
                    p2[j] = point.pt[polygon.pol[i].v[1].index].pos[j];
                    pn[j] = point.pt[polygon.pol[i].v[polygon.pol[i].nverts - 1].index].pos[j];
                }

                for (j = 0; j < 3; j++)
                {
                    v1[j] = p2[j] - p1[j];
                    v2[j] = pn[j] - p1[j];
                }

                cross(v1, v2, polygon.pol[i].norm);
                normalize(polygon.pol[i].norm);
            }
        }

        // For each point, fill in the indexes of the polygons that share the point.Returns 0 if any of the memory allocations fail, otherwise returns 1.
        public static bool lwGetPointPolygons(lwPointList point, lwPolygonList polygon)
        {
            int i, j, k;

            // count the number of polygons per point
            for (i = 0; i < polygon.count; i++)
                for (j = 0; j < polygon.pol[i].nverts; j++)
                    ++point.pt[polygon.pol[i].v[j].index].npols;

            // alloc per-point polygon arrays
            for (i = 0; i < point.count; i++)
            {
                if (point.pt[i].npols == 0) continue;
                point.pt[i].pol = (int*)Mem_ClearedAlloc(point.pt[i].npols * sizeof(int));
                if (!point.pt[i].pol) return 0;
                point.pt[i].npols = 0;
            }

            // fill in polygon array for each point
            for (i = 0; i < polygon.count; i++)
                for (j = 0; j < polygon.pol[i].nverts; j++)
                {
                    k = polygon.pol[i].v[j].index;
                    point.pt[k].pol[point.pt[k].npols] = i;
                    ++point.pt[k].npols;
                }

            return 1;
        }

        // Convert tag indexes into actual lwSurface pointers.  If any polygons point to tags for which no corresponding surface can be found, a default surface is created.
        public static bool lwResolvePolySurfaces(lwPolygonList polygon, lwTagList tlist, lwSurface[] surf, int[] nsurfs)
        {
            lwSurface** s, *st;
            int i;
            ptrdiff_t index;

            if (tlist.count == 0) return 1;

            s = (lwSurface**)Mem_ClearedAlloc(tlist.count * sizeof(lwSurface*));
            if (!s) return 0;

            for (i = 0; i < tlist.count; i++)
            {
                st = *surf;
                while (st)
                {
                    if (!strcmp(st.name, tlist.tag[i]))
                    {
                        s[i] = st;
                        break;
                    }
                    st = st.next;
                }
            }

            for (i = 0; i < polygon.count; i++)
            {
                index = (ptrdiff_t)polygon.pol[i].surf;
                if (index < 0 || index > tlist.count) return 0;
                if (!s[index])
                {
                    s[index] = lwDefaultSurface();
                    if (!s[index]) return 0;
                    s[index].name = (char*)Mem_ClearedAlloc(strlen(tlist.tag[index]) + 1);
                    if (!s[index].name) return 0;
                    strcpy(s[index].name, tlist.tag[index]);
                    lwListAdd((void**)surf, s[index]);
                    *nsurfs = *nsurfs + 1;
                }
                polygon.pol[i].surf = s[index];
            }

            Mem_Free(s);
            return 1;
        }

        // Calculate the vertex normals.  For each polygon vertex, sum the normals of the polygons that share the point.If the normals of the
        // current and adjacent polygons form an angle greater than the max smoothing angle for the current polygon's surface, the normal of the
        // adjacent polygon is excluded from the sum.  It's also excluded if the polygons aren't in the same smoothing group.
        //
        // Assumes that lwGetPointPolygons(), lwGetPolyNormals() and lwResolvePolySurfaces() have already been called.
        public static void lwGetVertNormals(lwPointList point, lwPolygonList polygon)
        {
            int j, k, n, g, h, p;
            float a;

            for (j = 0; j < polygon.count; j++)
            {
                for (n = 0; n < polygon.pol[j].nverts; n++)
                {
                    for (k = 0; k < 3; k++)
                        polygon.pol[j].v[n].norm[k] = polygon.pol[j].norm[k];

                    if (polygon.pol[j].surf.smooth <= 0) continue;

                    p = polygon.pol[j].v[n].index;

                    for (g = 0; g < point.pt[p].npols; g++)
                    {
                        h = point.pt[p].pol[g];
                        if (h == j) continue;

                        if (polygon.pol[j].smoothgrp != polygon.pol[h].smoothgrp)
                            continue;
                        a = vecangle(polygon.pol[j].norm, polygon.pol[h].norm);
                        if (a > polygon.pol[j].surf.smooth) continue;

                        for (k = 0; k < 3; k++)
                            polygon.pol[j].v[n].norm[k] += polygon.pol[h].norm[k];
                    }

                    normalize(polygon.pol[j].v[n].norm);
                }
            }
        }

        // Free memory used by an lwTagList.
        public static void lwFreeTags(lwTagList tlist)
        {
            int i;

            if (tlist)
            {
                if (tlist.tag)
                {
                    for (i = 0; i < tlist.count; i++)
                        if (tlist.tag[i])
                        {
                            Mem_Free(tlist.tag[i]);
                        }
                    Mem_Free(tlist.tag);
                }
                memset(tlist, 0, sizeof(lwTagList));
            }
        }

        // Read tag strings from a TAGS chunk in an LWO2 file.  The tags are added to the lwTagList array.
        public static bool lwGetTags(VFile fp, int cksize, lwTagList tlist)
        {
            char* buf, bp;
            int i, len, ntags;

            if (cksize == 0) return 1;

            // read the whole chunk
            set_flen(0);
            buf = (char*)getbytes(fp, cksize);
            if (!buf) return 0;

            // count the strings
            ntags = 0;
            bp = buf;
            while (bp < buf + cksize)
            {
                len = strlen(bp) + 1;
                len += len & 1;
                bp += len;
                ++ntags;
            }

            // expand the string array to hold the new tags
            tlist.offset = tlist.count;
            tlist.count += ntags;
            char** oldtag = tlist.tag;
            tlist.tag = (char**)Mem_Alloc(tlist.count * sizeof(char*));
            if (!tlist.tag) goto Fail;
            if (oldtag)
            {
                memcpy(tlist.tag, oldtag, tlist.offset * sizeof(char*));
                Mem_Free(oldtag);
            }
            memset(&tlist.tag[tlist.offset], 0, ntags * sizeof(char*));

            // copy the new tags to the tag array
            bp = buf;
            for (i = 0; i < ntags; i++)
                tlist.tag[i + tlist.offset] = sgetS0((unsigned char * *) & bp );

            Mem_Free(buf);
            return 1;

        Fail:
            if (buf) Mem_Free(buf);
            return 0;
        }

        // Read polygon tags from a PTAG chunk in an LWO2 file.
        public static bool lwGetPolygonTags(VFile fp, int cksize, ref lwTagList tlist, ref lwPolygonList plist)
        {
            uint type;
            int rlen = 0, i;
            ptrdiff_t j;

            set_flen(0);
            type = getU4(fp);
            rlen = get_flen();
            if (rlen < 0) return 0;

            if (type != ID_SURF && type != ID_PART && type != ID_SMGP) { fp.Seek(cksize - 4, FS_SEEK.CUR); return 1; }

            while (rlen < cksize)
            {
                i = getVX(fp) + plist.offset;
                j = getVX(fp) + tlist.offset;
                rlen = get_flen();
                if (rlen < 0 || rlen > cksize) return 0;

                switch (type)
                {
                    case ID_SURF: plist.pol[i].surf = (lwSurface)j; break;
                    case ID_PART: plist.pol[i].part = j; break;
                    case ID_SMGP: plist.pol[i].smoothgrp = j; break;
                }
            }

            return 1;
        }

        #endregion

        #region vmap.c

        // Free memory used by an lwVMap.
        public static void lwFreeVMap(lwVMap vmap)
        {
            //if (vmap != null)
            //{
            //    if (vmap.name != null) vmap.name = null;
            //    if (vmap.vindex != null) vmap.vindex = null;
            //    if (vmap.pindex != null) vmap.pindex = null;
            //    if (vmap.val != null)
            //    {
            //        //if (vmap.val[0] != 0f) vmap.val[0] = null;
            //        vmap.val = null;
            //    }
            //    vmap = null;
            //}
        }

        // Read an lwVMap from a VMAP or VMAD chunk in an LWO2.
        public static lwVMap lwGetVMap(VFile fp, int cksize, int ptoffset, int poloffset, bool perpoly)
        {
            byte[] buf, bp;
            lwVMap vmap;
            float* f;
            int i, j, npts, rlen;


            // read the whole chunk
            set_flen(0);
            buf = getbytes(fp, cksize); if (buf == null) return null;

            vmap = new lwVMap();

            // initialize the vmap
            vmap.perpoly = perpoly;

            bp = buf;
            set_flen(0);
            vmap.type = sgetU4(&bp);
            vmap.dim = sgetU2(&bp);
            vmap.name = sgetS0(&bp);
            rlen = get_flen();

            /* count the vmap records */

            npts = 0;
            while (bp < buf + cksize)
            {
                i = sgetVX(&bp);
                if (perpoly)
                    i = sgetVX(&bp);
                bp += vmap.dim * sizeof(float);
                ++npts;
            }

            /* allocate the vmap */

            vmap.nverts = npts;
            vmap.vindex = (int*)Mem_ClearedAlloc(npts * sizeof(int));
            if (!vmap.vindex) goto Fail;
            if (perpoly)
            {
                vmap.pindex = (int*)Mem_ClearedAlloc(npts * sizeof(int));
                if (!vmap.pindex) goto Fail;
            }

            if (vmap.dim > 0)
            {
                vmap.val = (float**)Mem_ClearedAlloc(npts * sizeof(float*));
                if (!vmap.val) goto Fail;
                f = (float*)Mem_ClearedAlloc(npts * vmap.dim * sizeof(float));
                if (!f) goto Fail;
                for (i = 0; i < npts; i++)
                    vmap.val[i] = f + i * vmap.dim;
            }

            /* fill in the vmap values */

            bp = buf + rlen;
            for (i = 0; i < npts; i++)
            {
                vmap.vindex[i] = sgetVX(&bp);
                if (perpoly)
                    vmap.pindex[i] = sgetVX(&bp);
                for (j = 0; j < vmap.dim; j++)
                    vmap.val[i][j] = sgetF4(&bp);
            }

            Mem_Free(buf);
            return vmap;

        Fail:
            if (buf) Mem_Free(buf);
            lwFreeVMap(vmap);
            return null;
        }

        // Fill in the lwVMapPt structure for each point.
        public static bool lwGetPointVMaps(lwPointList point, lwVMap vmap)
        {
            lwVMap* vm;
            int i, j, n;

            /* count the number of vmap values for each point */

            vm = vmap;
            while (vm)
            {
                if (!vm.perpoly)
                    for (i = 0; i < vm.nverts; i++)
                        ++point.pt[vm.vindex[i]].nvmaps;
                vm = vm.next;
            }

            /* allocate vmap references for each mapped point */

            for (i = 0; i < point.count; i++)
            {
                if (point.pt[i].nvmaps)
                {
                    point.pt[i].vm = (lwVMapPt*)Mem_ClearedAlloc(point.pt[i].nvmaps * sizeof(lwVMapPt));
                    if (!point.pt[i].vm) return 0;
                    point.pt[i].nvmaps = 0;
                }
            }

            /* fill in vmap references for each mapped point */

            vm = vmap;
            while (vm)
            {
                if (!vm.perpoly)
                {
                    for (i = 0; i < vm.nverts; i++)
                    {
                        j = vm.vindex[i];
                        n = point.pt[j].nvmaps;
                        point.pt[j].vm[n].vmap = vm;
                        point.pt[j].vm[n].index = i;
                        ++point.pt[j].nvmaps;
                    }
                }
                vm = vm.next;
            }

            return 1;
        }

        // Fill in the lwVMapPt structure for each polygon vertex.
        public static bool lwGetPolyVMaps(lwPolygonList polygon, lwVMap vmap)
        {
            lwVMap* vm;
            lwPolVert* pv;
            int i, j;

            /* count the number of vmap values for each polygon vertex */

            vm = vmap;
            while (vm)
            {
                if (vm.perpoly)
                {
                    for (i = 0; i < vm.nverts; i++)
                    {
                        for (j = 0; j < polygon.pol[vm.pindex[i]].nverts; j++)
                        {
                            pv = &polygon.pol[vm.pindex[i]].v[j];
                            if (vm.vindex[i] == pv.index)
                            {
                                ++pv.nvmaps;
                                break;
                            }
                        }
                    }
                }
                vm = vm.next;
            }

            /* allocate vmap references for each mapped vertex */

            for (i = 0; i < polygon.count; i++)
            {
                for (j = 0; j < polygon.pol[i].nverts; j++)
                {
                    pv = &polygon.pol[i].v[j];
                    if (pv.nvmaps)
                    {
                        pv.vm = (lwVMapPt*)Mem_ClearedAlloc(pv.nvmaps * sizeof(lwVMapPt));
                        if (!pv.vm) return 0;
                        pv.nvmaps = 0;
                    }
                }
            }

            /* fill in vmap references for each mapped point */

            vm = vmap;
            while (vm)
            {
                if (vm.perpoly)
                {
                    for (i = 0; i < vm.nverts; i++)
                    {
                        for (j = 0; j < polygon.pol[vm.pindex[i]].nverts; j++)
                        {
                            pv = &polygon.pol[vm.pindex[i]].v[j];
                            if (vm.vindex[i] == pv.index)
                            {
                                pv.vm[pv.nvmaps].vmap = vm;
                                pv.vm[pv.nvmaps].index = i;
                                ++pv.nvmaps;
                                break;
                            }
                        }
                    }
                }
                vm = vm.next;
            }

            return 1;
        }

        #endregion

        #region clip.c

        // Free memory used by an lwClip.
        public static void lwFreeClip(lwClip clip)
        {
            if (clip != null)
            {
                lwListFree(clip.ifilter, lwFreePlugin);
                lwListFree(clip.pfilter, lwFreePlugin);
                switch (clip.type)
                {
                    case ID_STIL:
                        if (clip.source.still.name != null) clip.source.still.name = null;
                        break;
                    case ID_ISEQ:
                        if (clip.source.seq.suffix != null) clip.source.seq.suffix = null;
                        if (clip.source.seq.prefix != null) clip.source.seq.prefix = null;
                        break;
                    case ID_ANIM:
                        if (clip.source.anim.server != null) clip.source.anim.server = null;
                        if (clip.source.anim.name != null) clip.source.anim.name = null;
                        break;
                    case ID_XREF:
                        if (clip.source.xref.s != null) clip.source.xref.s = null;
                        break;
                    case ID_STCC:
                        if (clip.source.cycle.name != null) clip.source.cycle.name = null;
                        break;
                }
                clip = null;
            }
        }

        // Read image references from a CLIP chunk in an LWO2 file.
        public static lwClip lwGetClip(VFile fp, int cksize)
        {
            uint id; ushort sz; int pos, rlen;

            // allocate the Clip structure
            var clip = new lwClip(); if (clip == null) goto Fail;
            clip.contrast.val = 1f;
            clip.brightness.val = 1f;
            clip.saturation.val = 1f;
            clip.gamma.val = 1f;

            // remember where we started
            set_flen(0);
            pos = fp.Tell;

            // index
            clip.index = getI4(fp);

            // first subchunk header
            clip.type = getU4(fp);
            sz = getU2(fp);
            if (get_flen() < 0) goto Fail;

            sz += (ushort)(sz & 1);
            set_flen(0);

            switch (clip.type)
            {
                case ID_STIL:
                    clip.source.still.name = getS0(fp);
                    break;

                case ID_ISEQ:
                    clip.source.seq.digits = getU1(fp);
                    clip.source.seq.flags = getU1(fp);
                    clip.source.seq.offset = getI2(fp);
                    clip.source.seq.start = getI2(fp);
                    clip.source.seq.end = getI2(fp);
                    clip.source.seq.prefix = getS0(fp);
                    clip.source.seq.suffix = getS0(fp);
                    break;

                case ID_ANIM:
                    clip.source.anim.name = getS0(fp);
                    clip.source.anim.server = getS0(fp);
                    rlen = get_flen();
                    clip.source.anim.data = getbytes(fp, sz - rlen);
                    break;

                case ID_XREF:
                    clip.source.xref.index = getI4(fp);
                    clip.source.xref.s = getS0(fp);
                    break;

                case ID_STCC:
                    clip.source.cycle.lo = getI2(fp);
                    clip.source.cycle.hi = getI2(fp);
                    clip.source.cycle.name = getS0(fp);
                    break;

                default:
                    break;
            }

            // error while reading current subchunk?

            rlen = get_flen();
            if (rlen < 0 || rlen > sz) goto Fail;

            // skip unread parts of the current subchunk

            if (rlen < sz) fp.Seek(sz - rlen, FS_SEEK.CUR);

            // end of the CLIP chunk?

            rlen = fp.Tell - pos;
            if (cksize < rlen) goto Fail;
            if (cksize == rlen) return clip;

            // process subchunks as they're encountered

            id = getU4(fp);
            sz = getU2(fp);
            if (get_flen() < 0) goto Fail;

            while (true)
            {
                sz += (ushort)(sz & 1);
                set_flen(0);

                switch (id)
                {
                    case ID_TIME:
                        clip.start_time = getF4(fp);
                        clip.duration = getF4(fp);
                        clip.frame_rate = getF4(fp);
                        break;

                    case ID_CONT:
                        clip.contrast.val = getF4(fp);
                        clip.contrast.eindex = getVX(fp);
                        break;

                    case ID_BRIT:
                        clip.brightness.val = getF4(fp);
                        clip.brightness.eindex = getVX(fp);
                        break;

                    case ID_SATR:
                        clip.saturation.val = getF4(fp);
                        clip.saturation.eindex = getVX(fp);
                        break;

                    case ID_HUE:
                        clip.hue.val = getF4(fp);
                        clip.hue.eindex = getVX(fp);
                        break;

                    case ID_GAMM:
                        clip.gamma.val = getF4(fp);
                        clip.gamma.eindex = getVX(fp);
                        break;

                    case ID_NEGA:
                        clip.negative = getU2(fp);
                        break;

                    case ID_IFLT:
                    case ID_PFLT:
                        var filt = new lwPlugin(); if (filt == null) goto Fail;

                        filt.name = getS0(fp);
                        filt.flags = getU2(fp);
                        rlen = get_flen();
                        filt.data = getbytes(fp, sz - rlen);

                        if (id == ID_IFLT) { lwListAdd(clip.ifilter, filt); clip.nifilters++; }
                        else { lwListAdd(clip.pfilter, filt); clip.npfilters++; }
                        break;

                    default:
                        break;
                }

                // error while reading current subchunk?

                rlen = get_flen();
                if (rlen < 0 || rlen > sz) goto Fail;

                // skip unread parts of the current subchunk

                if (rlen < sz) fp.Seek(sz - rlen, FS_SEEK.CUR);

                // end of the CLIP chunk?

                rlen = fp.Tell - pos;
                if (cksize < rlen) goto Fail;
                if (cksize == rlen) break;

                // get the next chunk header //

                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (get_flen() != 6) goto Fail;
            }

            return clip;

        Fail:
            lwFreeClip(clip);
            return null;
        }

        // Returns an lwClip pointer, given a clip index.
        public static lwClip lwFindClip(lwClip list, int index)
        {
            var clip = list;
            while (clip != null)
            {
                if (clip.index == index) break;
                clip = clip.next;
            }
            return clip;
        }

        #endregion

        #region envelope.c

        static void lwFree(ref object ptr)
        {
            ptr = null;
        }

        public static void lwFreeEnvelope(lwEnvelope env)
        {
            if (env != null)
            {
                if (env.name != null) env.name = null;
                lwListFree(env.key, lwFree);
                lwListFree(env.cfilter, lwFreePlugin);
                env = null;
            }
        }

        static int compare_keys(lwKey k1, lwKey k2)
            => k1.time > k2.time ? 1 : k1.time < k2.time ? -1 : 0;

        // Read an ENVL chunk from an LWO2 file.
        public static lwEnvelope lwGetEnvelope(VFile fp, int cksize)
        {
            lwKey key = null;
            lwPlugin plug;
            uint id;
            ushort sz;
            var f = stackalloc float[4];
            int i, nparams, pos, rlen;

            // allocate the Envelope structure
            var env = new lwEnvelope(); if (env == null) goto Fail;

            // remember where we started
            set_flen(0);
            pos = fp.Tell;

            // index
            env.index = getVX(fp);

            // first subchunk header
            id = getU4(fp);
            sz = getU2(fp);
            if (get_flen() < 0) goto Fail;

            // process subchunks as they're encountered
            while (true)
            {
                sz += sz & 1;
                set_flen(0);

                switch (id)
                {
                    case ID_TYPE:
                        env.type = getU2(fp);
                        break;

                    case ID_NAME:
                        env.name = getS0(fp);
                        break;

                    case ID_PRE:
                        env.behavior[0] = getU2(fp);
                        break;

                    case ID_POST:
                        env.behavior[1] = getU2(fp);
                        break;

                    case ID_KEY:
                        key = new lwKey(); if (key == null) goto Fail;
                        key.time = getF4(fp);
                        key.value = getF4(fp);
                        lwListInsert(env.key, key, compare_keys);
                        env.nkeys++;
                        break;

                    case ID_SPAN:
                        if (key == null) goto Fail;
                        key.shape = getU4(fp);

                        nparams = (sz - 4) / 4;
                        if (nparams > 4) nparams = 4;
                        for (i = 0; i < nparams; i++)
                            f[i] = getF4(fp);

                        switch (key.shape)
                        {
                            case ID_TCB:
                                key.tension = f[0];
                                key.continuity = f[1];
                                key.bias = f[2];
                                break;

                            case ID_BEZI:
                            case ID_HERM:
                            case ID_BEZ2:
                                for (i = 0; i < nparams; i++)
                                    key.param[i] = f[i];
                                break;
                        }
                        break;

                    case ID_CHAN:
                        plug = new lwPlugin(); if (plug == null) goto Fail;

                        plug.name = getS0(fp);
                        plug.flags = getU2(fp);
                        plug.data = getbytes(fp, sz - get_flen());

                        lwListAdd(env.cfilter, plug);
                        env.ncfilters++;
                        break;

                    default:
                        break;
                }

                // error while reading current subchunk?

                rlen = get_flen();
                if (rlen < 0 || rlen > sz) goto Fail;

                // skip unread parts of the current subchunk
                if (rlen < sz) fp.Seek(sz - rlen, FS_SEEK.CUR);

                // end of the ENVL chunk?

                rlen = fp.Tell - pos;
                if (cksize < rlen) goto Fail;
                if (cksize == rlen) break;

                // get the next subchunk header
                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (get_flen() != 6) goto Fail;
            }

            return env;

        Fail:
            lwFreeEnvelope(env);
            return null;
        }

        // Returns an lwEnvelope pointer, given an envelope index.
        public static lwEnvelope lwFindEnvelope(lwEnvelope list, int index)
        {
            var env = list;
            while (env != null)
            {
                if (env.index == index) break;
                env = env.next;
            }
            return env;
        }

        // Given the value v of a periodic function, returns the equivalent value v2 in the principal interval [lo, hi].  If i isn't null, it receives
        // the number of wavelengths between v and v2.
        // 
        //    v2 = v - i * (hi - lo)
        // 
        // For example, range( 3 pi, 0, 2 pi, i ) returns pi, with i = 1.
        static float range(float v, float lo, float hi, ref int i)
        {
            float v2, r = hi - lo;

            if (r == 0f)
            {
                i = 0;
                return lo;
            }

            v2 = lo + v - r * (float)Math.Floor((double)v / r);
            i = -(int)((v2 - v) / r + (v2 > v ? 0.5 : -0.5));

            return v2;
        }

        // Calculate the Hermite coefficients.
        static void hermite(float t, out float h1, out float h2, out float h3, out float h4)
        {
            float t2, t3;

            t2 = t * t;
            t3 = t * t2;

            h2 = 3f * t2 - t3 - t3;
            h1 = 1f - h2;
            h4 = t3 - t2;
            h3 = h4 - t2 + t;
        }

        // Interpolate the value of a 1D Bezier curve.
        static float bezier(float x0, float x1, float x2, float x3, float t)
        {
            float a, b, c, t2, t3;

            t2 = t * t;
            t3 = t2 * t;

            c = 3f * (x1 - x0);
            b = 3f * (x2 - x1) - c;
            a = x3 - x0 - c - b;

            return a * t3 + b * t2 + c * t + x0;
        }


        // Find the t for which bezier() returns the input time.  The handle endpoints of a BEZ2 curve represent the control points, and these have
        // (time, value) coordinates, so time is used as both a coordinate and a parameter for this curve type.
        static float bez2_time(float x0, float x1, float x2, float x3, float time, ref float t0, ref float t1)
        {
            float v, t;

            t = t0 + (t1 - t0) * 0.5f;
            v = bezier(x0, x1, x2, x3, t);
            if (MathX.Fabs(time - v) > .0001f)
            {
                if (v > time) t1 = t;
                else t0 = t;
                return bez2_time(x0, x1, x2, x3, time, ref t0, ref t1);
            }
            else return t;
        }

        // Interpolate the value of a BEZ2 curve.
        static float bez2(lwKey key0, lwKey key1, float time)
        {
            float x, y, t, t0 = 0f, t1 = 1f;

            x = key0.shape == ID_BEZ2
                ? key0.time + key0.param[2]
                : key0.time + (key1.time - key0.time) / 3f;

            t = bez2_time(key0.time, x, key1.time + key1.param[0], key1.time, time, ref t0, ref t1);

            y = key0.shape == ID_BEZ2
                 ? key0.value + key0.param[3]
                : key0.value + key0.param[1] / 3f;

            return bezier(key0.value, y, key1.param[1] + key1.value, key1.value, t);
        }


        // Return the outgoing tangent to the curve at key0.  The value returned for the BEZ2 case is used when extrapolating a linear pre behavior and
        // when interpolating a non-BEZ2 span.
        static float outgoing(lwKey key0, lwKey key1)
        {
            float a, b, d, t, out_;

            switch (key0.shape)
            {
                case ID_TCB:
                    a = (1f - key0.tension) * (1f + key0.continuity) * (1f + key0.bias);
                    b = (1f - key0.tension) * (1f - key0.continuity) * (1f - key0.bias);
                    d = key1.value - key0.value;
                    if (key0.prev != null) { t = (key1.time - key0.time) / (key1.time - key0.prev.time); out_ = t * (a * (key0.value - key0.prev.value) + b * d); }
                    else out_ = b * d;
                    break;

                case ID_LINE:
                    d = key1.value - key0.value;
                    if (key0.prev != null) { t = (key1.time - key0.time) / (key1.time - key0.prev.time); out_ = t * (key0.value - key0.prev.value + d); }
                    else out_ = d;
                    break;

                case ID_BEZI:
                case ID_HERM:
                    out_ = key0.param[1];
                    if (key0.prev != null) out_ *= (key1.time - key0.time) / (key1.time - key0.prev.time);
                    break;

                case ID_BEZ2:
                    out_ = key0.param[3] * (key1.time - key0.time);
                    if (MathX.Fabs(key0.param[2]) > 1e-5f) out_ /= key0.param[2];
                    else out_ *= 1e5f;
                    break;

                case ID_STEP:
                default: out_ = 0f; break;
            }

            return out_;
        }

        // Return the incoming tangent to the curve at key1.  The value returned
        // for the BEZ2 case is used when extrapolating a linear post behavior.
        static float incoming(lwKey key0, lwKey key1)
        {
            float a, b, d, t, in_;

            switch (key1.shape)
            {
                case ID_LINE:
                    d = key1.value - key0.value;
                    if (key1.next != null) { t = (key1.time - key0.time) / (key1.next.time - key0.time); in_ = t * (key1.next.value - key1.value + d); }
                    else in_ = d;
                    break;

                case ID_TCB:
                    a = (1f - key1.tension) * (1f - key1.continuity) * (1f + key1.bias);
                    b = (1f - key1.tension) * (1f + key1.continuity) * (1f - key1.bias);
                    d = key1.value - key0.value;

                    if (key1.next != null) { t = (key1.time - key0.time) / (key1.next.time - key0.time); in_ = t * (b * (key1.next.value - key1.value) + a * d); }
                    else in_ = a * d;
                    break;

                case ID_BEZI:
                case ID_HERM:
                    in_ = key1.param[0];
                    if (key1.next != null) in_ *= (key1.time - key0.time) / (key1.next.time - key0.time);
                    break;

                case ID_BEZ2:
                    in_ = key1.param[1] * (key1.time - key0.time);
                    if (MathX.Fabs(key1.param[0]) > 1e-5f) in_ /= key1.param[0];

                    else in_ *= 1e5f;
                    break;

                case ID_STEP:
                default:
                    in_ = 0f;
                    break;
            }

            return in_;
        }

        // Given a list of keys and a time, returns the interpolated value of the envelope at that time.
        public static float lwEvalEnvelope(lwEnvelope env, float time)
        {
            lwKey key0, key1, skey, ekey;
            float t, h1, h2, h3, h4, in_, out_, offset = 0f;
            int noff;

            // if there's no key, the value is 0
            if (env.nkeys == 0) return 0f;

            // if there's only one key, the value is constant
            if (env.nkeys == 1) return env.key.value;

            // find the first and last keys
            skey = ekey = env.key;
            while (ekey.next != null) ekey = ekey.next;

            // use pre-behavior if time is before first key time
            if (time < skey.time)
            {
                switch (env.behavior[0])
                {
                    case BEH_RESET: return 0f;
                    case BEH_CONSTANT: return skey.value;
                    case BEH_REPEAT: time = range(time, skey.time, ekey.time, null); break;
                    case BEH_OSCILLATE: time = range(time, skey.time, ekey.time, ref noff); if ((noff % 2) != 0) time = ekey.time - skey.time - time; break;
                    case BEH_OFFSET: time = range(time, skey.time, ekey.time, ref noff); offset = noff * (ekey.value - skey.value); break;
                    case BEH_LINEAR: out_ = outgoing(skey, skey.next) / (skey.next.time - skey.time); return out_ * (time - skey.time) + skey.value;
                }
            }

            // use post-behavior if time is after last key time
            else if (time > ekey.time)
            {
                switch (env.behavior[1])
                {
                    case BEH_RESET: return 0f;
                    case BEH_CONSTANT: return ekey.value;
                    case BEH_REPEAT: time = range(time, skey.time, ekey.time, _); break;
                    case BEH_OSCILLATE: time = range(time, skey.time, ekey.time, ref noff); if ((noff % 2) != 0) time = ekey.time - skey.time - time; break;
                    case BEH_OFFSET: time = range(time, skey.time, ekey.time, ref noff); offset = noff * (ekey.value - skey.value); break;
                    case BEH_LINEAR: in_ = incoming(ekey.prev, ekey) / (ekey.time - ekey.prev.time); return in_ * (time - ekey.time) + ekey.value;
                }
            }

            // get the endpoints of the interval being evaluated
            key0 = env.key;
            while (time > key0.next.time) key0 = key0.next;
            key1 = key0.next;

            // check for singularities first
            if (time == key0.time) return key0.value + offset;
            else if (time == key1.time) return key1.value + offset;

            // get interval length, time in [0, 1]
            t = (time - key0.time) / (key1.time - key0.time);

            // interpolate
            switch (key1.shape)
            {
                case ID_TCB:
                case ID_BEZI:
                case ID_HERM:
                    out_ = outgoing(key0, key1);
                    in_ = incoming(key0, key1);
                    hermite(t, out h1, out h2, out h3, out h4);
                    return h1 * key0.value + h2 * key1.value + h3 * out_ + h4 * in_ + offset;
                case ID_BEZ2: return bez2(key0, key1, time) + offset;
                case ID_LINE: return key0.value + t * (key1.value - key0.value) + offset;
                case ID_STEP: return key0.value + offset;
                default: return offset;
            }
        }

        #endregion

        #region surface.c

        // Free the memory used by an lwPlugin.
        public static void lwFreePlugin(lwPlugin p)
        {
            if (p)
            {
                if (p.ord) Mem_Free(p.ord);
                if (p.name) Mem_Free(p.name);
                if (p.data) Mem_Free(p.data);
                Mem_Free(p);
            }
        }

        // Free the memory used by an lwTexture.
        public static void lwFreeTexture(lwTexture t)
        {
            if (t)
            {
                if (t.ord) Mem_Free(t.ord);
                switch (t.type)
                {
                    case ID_IMAP:
                        if (t.param.imap.vmap_name) Mem_Free(t.param.imap.vmap_name);
                        break;
                    case ID_PROC:
                        if (t.param.proc.name) Mem_Free(t.param.proc.name);
                        if (t.param.proc.data) Mem_Free(t.param.proc.data);
                        break;
                    case ID_GRAD:
                        if (t.param.grad.key) Mem_Free(t.param.grad.key);
                        if (t.param.grad.ikey) Mem_Free(t.param.grad.ikey);
                        break;
                }
                if (t.tmap.ref_object) Mem_Free(t.tmap.ref_object);
                Mem_Free(t);
            }
        }

        // Free the memory used by an lwSurface.
        public static void lwFreeSurface(lwSurface surf)
        {
            if (surf)
            {
                if (surf.name) Mem_Free(surf.name);
                if (surf.srcname) Mem_Free(surf.srcname);

                lwListFree(surf.shader, (void(__cdecl *)(void*))lwFreePlugin );

                lwListFree(surf.color.tex, (void(__cdecl *)(void*))lwFreeTexture );
                lwListFree(surf.luminosity.tex, (void(__cdecl *)(void*))lwFreeTexture );
                lwListFree(surf.diffuse.tex, (void(__cdecl *)(void*))lwFreeTexture );
                lwListFree(surf.specularity.tex, (void(__cdecl *)(void*))lwFreeTexture );
                lwListFree(surf.glossiness.tex, (void(__cdecl *)(void*))lwFreeTexture );
                lwListFree(surf.reflection.val.tex, (void(__cdecl *)(void*))lwFreeTexture );
                lwListFree(surf.transparency.val.tex, (void(__cdecl *)(void*))lwFreeTexture );
                lwListFree(surf.eta.tex, (void(__cdecl *)(void*))lwFreeTexture );
                lwListFree(surf.translucency.tex, (void(__cdecl *)(void*))lwFreeTexture );
                lwListFree(surf.bump.tex, (void(__cdecl *)(void*))lwFreeTexture );

                Mem_Free(surf);
            }
        }

        // Read a texture map header from a SURF.BLOK in an LWO2 file.  This is the first subchunk in a BLOK, and its contents are common to all three texture types.
        public static int lwGetTHeader(VFile fp, int hsz, lwTexture tex)
        {
            unsigned int id;
            unsigned short sz;
            int pos, rlen;


            /* remember where we started */

            set_flen(0);
            pos = fp.Tell();

            /* ordinal string */

            tex.ord = getS0(fp);

            /* first subchunk header */

            id = getU4(fp);
            sz = getU2(fp);
            if (0 > get_flen()) return 0;

            /* process subchunks as they're encountered */

            while (1)
            {
                sz += sz & 1;
                set_flen(0);

                switch (id)
                {
                    case ID_CHAN:
                        tex.chan = getU4(fp);
                        break;

                    case ID_OPAC:
                        tex.opac_type = getU2(fp);
                        tex.opacity.val = getF4(fp);
                        tex.opacity.eindex = getVX(fp);
                        break;

                    case ID_ENAB:
                        tex.enabled = getU2(fp);
                        break;

                    case ID_NEGA:
                        tex.negative = getU2(fp);
                        break;

                    case ID_AXIS:
                        tex.axis = getU2(fp);
                        break;

                    default:
                        break;
                }

                /* error while reading current subchunk? */

                rlen = get_flen();
                if (rlen < 0 || rlen > sz) return 0;

                /* skip unread parts of the current subchunk */

                if (rlen < sz)
                    fp.Seek(sz - rlen, FS_SEEK_CUR);

                /* end of the texture header subchunk? */

                if (hsz <= fp.Tell() - pos)
                    break;

                /* get the next subchunk header */

                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (6 != get_flen()) return 0;
            }

            set_flen(fp.Tell() - pos);
            return 1;
        }

        // Read a texture map from a SURF.BLOK in an LWO2 file.  The TMAP defines the mapping from texture to world or object coordinates.
        public static int lwGetTMap(VFile fp, int tmapsz, lwTMap tmap)
        {
            unsigned int id;
            unsigned short sz;
            int rlen, pos, i;

            pos = fp.Tell();
            id = getU4(fp);
            sz = getU2(fp);
            if (0 > get_flen()) return 0;

            while (1)
            {
                sz += sz & 1;
                set_flen(0);

                switch (id)
                {
                    case ID_SIZE:
                        for (i = 0; i < 3; i++)
                            tmap.size.val[i] = getF4(fp);
                        tmap.size.eindex = getVX(fp);
                        break;

                    case ID_CNTR:
                        for (i = 0; i < 3; i++)
                            tmap.center.val[i] = getF4(fp);
                        tmap.center.eindex = getVX(fp);
                        break;

                    case ID_ROTA:
                        for (i = 0; i < 3; i++)
                            tmap.rotate.val[i] = getF4(fp);
                        tmap.rotate.eindex = getVX(fp);
                        break;

                    case ID_FALL:
                        tmap.fall_type = getU2(fp);
                        for (i = 0; i < 3; i++)
                            tmap.falloff.val[i] = getF4(fp);
                        tmap.falloff.eindex = getVX(fp);
                        break;

                    case ID_OREF:
                        tmap.ref_object = getS0(fp);
                        break;

                    case ID_CSYS:
                        tmap.coord_sys = getU2(fp);
                        break;

                    default:
                        break;
                }

                /* error while reading the current subchunk? */

                rlen = get_flen();
                if (rlen < 0 || rlen > sz) return 0;

                /* skip unread parts of the current subchunk */

                if (rlen < sz)
                    fp.Seek(sz - rlen, FS_SEEK_CUR);

                /* end of the TMAP subchunk? */

                if (tmapsz <= fp.Tell() - pos)
                    break;

                /* get the next subchunk header */

                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (6 != get_flen()) return 0;
            }

            set_flen(fp.Tell() - pos);
            return 1;
        }

        // Read an lwImageMap from a SURF.BLOK in an LWO2 file.
        public static int lwGetImageMap(VFile fp, int rsz, lwTexture tex)
        {
            unsigned int id;
            unsigned short sz;
            int rlen, pos;

            pos = fp.Tell();
            id = getU4(fp);
            sz = getU2(fp);
            if (0 > get_flen()) return 0;

            while (1)
            {
                sz += sz & 1;
                set_flen(0);

                switch (id)
                {
                    case ID_TMAP:
                        if (!lwGetTMap(fp, sz, &tex.tmap)) return 0;
                        break;

                    case ID_PROJ:
                        tex.param.imap.projection = getU2(fp);
                        break;

                    case ID_VMAP:
                        tex.param.imap.vmap_name = getS0(fp);
                        break;

                    case ID_AXIS:
                        tex.param.imap.axis = getU2(fp);
                        break;

                    case ID_IMAG:
                        tex.param.imap.cindex = getVX(fp);
                        break;

                    case ID_WRAP:
                        tex.param.imap.wrapw_type = getU2(fp);
                        tex.param.imap.wraph_type = getU2(fp);
                        break;

                    case ID_WRPW:
                        tex.param.imap.wrapw.val = getF4(fp);
                        tex.param.imap.wrapw.eindex = getVX(fp);
                        break;

                    case ID_WRPH:
                        tex.param.imap.wraph.val = getF4(fp);
                        tex.param.imap.wraph.eindex = getVX(fp);
                        break;

                    case ID_AAST:
                        tex.param.imap.aas_flags = getU2(fp);
                        tex.param.imap.aa_strength = getF4(fp);
                        break;

                    case ID_PIXB:
                        tex.param.imap.pblend = getU2(fp);
                        break;

                    case ID_STCK:
                        tex.param.imap.stck.val = getF4(fp);
                        tex.param.imap.stck.eindex = getVX(fp);
                        break;

                    case ID_TAMP:
                        tex.param.imap.amplitude.val = getF4(fp);
                        tex.param.imap.amplitude.eindex = getVX(fp);
                        break;

                    default:
                        break;
                }

                /* error while reading the current subchunk? */

                rlen = get_flen();
                if (rlen < 0 || rlen > sz) return 0;

                /* skip unread parts of the current subchunk */

                if (rlen < sz)
                    fp.Seek(sz - rlen, FS_SEEK_CUR);

                /* end of the image map? */

                if (rsz <= fp.Tell() - pos)
                    break;

                /* get the next subchunk header */

                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (6 != get_flen()) return 0;
            }

            set_flen(fp.Tell() - pos);
            return 1;
        }

        // Read an lwProcedural from a SURF.BLOK in an LWO2 file.
        public static int lwGetProcedural(VFile fp, int rsz, lwTexture tex)
        {
            unsigned int id;
            unsigned short sz;
            int rlen, pos;

            pos = fp.Tell();
            id = getU4(fp);
            sz = getU2(fp);
            if (0 > get_flen()) return 0;

            while (1)
            {
                sz += sz & 1;
                set_flen(0);

                switch (id)
                {
                    case ID_TMAP:
                        if (!lwGetTMap(fp, sz, &tex.tmap)) return 0;
                        break;

                    case ID_AXIS:
                        tex.param.proc.axis = getU2(fp);
                        break;

                    case ID_VALU:
                        tex.param.proc.value[0] = getF4(fp);
                        if (sz >= 8) tex.param.proc.value[1] = getF4(fp);
                        if (sz >= 12) tex.param.proc.value[2] = getF4(fp);
                        break;

                    case ID_FUNC:
                        tex.param.proc.name = getS0(fp);
                        rlen = get_flen();
                        tex.param.proc.data = getbytes(fp, sz - rlen);
                        break;

                    default:
                        break;
                }

                /* error while reading the current subchunk? */

                rlen = get_flen();
                if (rlen < 0 || rlen > sz) return 0;

                /* skip unread parts of the current subchunk */

                if (rlen < sz)
                    fp.Seek(sz - rlen, FS_SEEK_CUR);

                /* end of the procedural block? */

                if (rsz <= fp.Tell() - pos)
                    break;

                /* get the next subchunk header */

                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (6 != get_flen()) return 0;
            }

            set_flen(fp.Tell() - pos);
            return 1;
        }

        // Read an lwGradient from a SURF.BLOK in an LWO2 file.
        public static int lwGetGradient(VFile fp, int rsz, lwTexture tex)
        {
            unsigned int id;
            unsigned short sz;
            int rlen, pos, i, j, nkeys;

            pos = fp.Tell();
            id = getU4(fp);
            sz = getU2(fp);
            if (0 > get_flen()) return 0;

            while (1)
            {
                sz += sz & 1;
                set_flen(0);

                switch (id)
                {
                    case ID_TMAP:
                        if (!lwGetTMap(fp, sz, &tex.tmap)) return 0;
                        break;

                    case ID_PNAM:
                        tex.param.grad.paramname = getS0(fp);
                        break;

                    case ID_INAM:
                        tex.param.grad.itemname = getS0(fp);
                        break;

                    case ID_GRST:
                        tex.param.grad.start = getF4(fp);
                        break;

                    case ID_GREN:
                        tex.param.grad.end = getF4(fp);
                        break;

                    case ID_GRPT:
                        tex.param.grad.repeat = getU2(fp);
                        break;

                    case ID_FKEY:
                        nkeys = sz / sizeof(lwGradKey);
                        tex.param.grad.key = (lwGradKey*)Mem_ClearedAlloc(nkeys * sizeof(lwGradKey));
                        if (!tex.param.grad.key) return 0;
                        for (i = 0; i < nkeys; i++)
                        {
                            tex.param.grad.key[i].value = getF4(fp);
                            for (j = 0; j < 4; j++)
                                tex.param.grad.key[i].rgba[j] = getF4(fp);
                        }
                        break;

                    case ID_IKEY:
                        nkeys = sz / 2;
                        tex.param.grad.ikey = (short*)Mem_ClearedAlloc(nkeys * sizeof(short));
                        if (!tex.param.grad.ikey) return 0;
                        for (i = 0; i < nkeys; i++)
                            tex.param.grad.ikey[i] = getU2(fp);
                        break;

                    default:
                        break;
                }

                /* error while reading the current subchunk? */

                rlen = get_flen();
                if (rlen < 0 || rlen > sz) return 0;

                /* skip unread parts of the current subchunk */

                if (rlen < sz)
                    fp.Seek(sz - rlen, FS_SEEK_CUR);

                /* end of the gradient? */

                if (rsz <= fp.Tell() - pos)
                    break;

                /* get the next subchunk header */

                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (6 != get_flen()) return 0;
            }

            set_flen(fp.Tell() - pos);
            return 1;
        }

        // Read an lwTexture from a SURF.BLOK in an LWO2 file.
        public static lwTexture lwGetTexture(VFile fp, int bloksz, uint type)
        {
            lwTexture* tex;
            unsigned short sz;
            int ok;

            tex = (lwTexture*)Mem_ClearedAlloc(sizeof(lwTexture));
            if (!tex) return null;

            tex.type = type;
            tex.tmap.size.val[0] =
                tex.tmap.size.val[1] =
                    tex.tmap.size.val[2] = 1f;
            tex.opacity.val = 1f;
            tex.enabled = 1;

            sz = getU2(fp);
            if (!lwGetTHeader(fp, sz, tex))
            {
                Mem_Free(tex);
                return null;
            }

            sz = bloksz - sz - 6;
            switch (type)
            {
                case ID_IMAP:
                    ok = lwGetImageMap(fp, sz, tex);
                    break;
                case ID_PROC:
                    ok = lwGetProcedural(fp, sz, tex);
                    break;
                case ID_GRAD:
                    ok = lwGetGradient(fp, sz, tex);
                    break;
                default:
                    ok = !fp.Seek(sz, FS_SEEK_CUR);
            }

            if (!ok)
            {
                lwFreeTexture(tex);
                return null;
            }

            set_flen(bloksz);
            return tex;
        }

        // Read a shader record from a SURF.BLOK in an LWO2 file.
        public static lwPlugin lwGetShader(VFile fp, int bloksz)
        {
            lwPlugin* shdr;
            unsigned int id;
            unsigned short sz;
            int hsz, rlen, pos;

            shdr = (lwPlugin*)Mem_ClearedAlloc(sizeof(lwPlugin));
            if (!shdr) return null;

            pos = fp.Tell();
            set_flen(0);
            hsz = getU2(fp);
            shdr.ord = getS0(fp);
            id = getU4(fp);
            sz = getU2(fp);
            if (0 > get_flen()) goto Fail;

            while (hsz > 0)
            {
                sz += sz & 1;
                hsz -= sz;
                if (id == ID_ENAB)
                {
                    shdr.flags = getU2(fp);
                    break;
                }
                else
                {
                    fp.Seek(sz, FS_SEEK_CUR);
                    id = getU4(fp);
                    sz = getU2(fp);
                }
            }

            id = getU4(fp);
            sz = getU2(fp);
            if (0 > get_flen()) goto Fail;

            while (1)
            {
                sz += sz & 1;
                set_flen(0);

                switch (id)
                {
                    case ID_FUNC:
                        shdr.name = getS0(fp);
                        rlen = get_flen();
                        shdr.data = getbytes(fp, sz - rlen);
                        break;

                    default:
                        break;
                }

                /* error while reading the current subchunk? */

                rlen = get_flen();
                if (rlen < 0 || rlen > sz) goto Fail;

                /* skip unread parts of the current subchunk */

                if (rlen < sz)
                    fp.Seek(sz - rlen, FS_SEEK_CUR);

                /* end of the shader block? */

                if (bloksz <= fp.Tell() - pos)
                    break;

                /* get the next subchunk header */

                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (6 != get_flen()) goto Fail;
            }

            set_flen(fp.Tell() - pos);
            return shdr;

        Fail:
            lwFreePlugin(shdr);
            return null;
        }

        // Callbacks for the lwListInsert() function, which is called to add textures to surface channels and shaders to surfaces.

        static int compare_textures(lwTexture a, lwTexture b)
            => strcmp(a.ord, b.ord);

        static int compare_shaders(lwPlugin a, lwPlugin b)
            => strcmp(a.ord, b.ord);

        // Finds the surface channel(lwTParam or lwCParam) to which a texture is applied, then calls lwListInsert().

        static int add_texture(lwSurface* surf, lwTexture* tex)
        {
            lwTexture** list;

            switch (tex.chan)
            {
                case ID_COLR:
                    list = &surf.color.tex;
                    break;
                case ID_LUMI:
                    list = &surf.luminosity.tex;
                    break;
                case ID_DIFF:
                    list = &surf.diffuse.tex;
                    break;
                case ID_SPEC:
                    list = &surf.specularity.tex;
                    break;
                case ID_GLOS:
                    list = &surf.glossiness.tex;
                    break;
                case ID_REFL:
                    list = &surf.reflection.val.tex;
                    break;
                case ID_TRAN:
                    list = &surf.transparency.val.tex;
                    break;
                case ID_RIND:
                    list = &surf.eta.tex;
                    break;
                case ID_TRNL:
                    list = &surf.translucency.tex;
                    break;
                case ID_BUMP:
                    list = &surf.bump.tex;
                    break;
                default:
                    return 0;
            }

            lwListInsert((void**)list, tex, (int(__cdecl *)(void*,void*))compare_textures );
            return 1;
        }

        // Read an lwSurface from an LWO2 file.
        public static lwSurface lwGetSurface(VFile fp, int cksize)
        {
            lwSurface* surf;
            lwTexture* tex;
            lwPlugin* shdr;
            unsigned int id, type;
            unsigned short sz;
            int pos, rlen;


            /* allocate the Surface structure */

            surf = (lwSurface*)Mem_ClearedAlloc(sizeof(lwSurface));
            if (!surf) goto Fail;

            /* non-zero defaults */

            surf.color.rgb[0] = 0.78431f;
            surf.color.rgb[1] = 0.78431f;
            surf.color.rgb[2] = 0.78431f;
            surf.diffuse.val = 1f;
            surf.glossiness.val = 0.4f;
            surf.bump.val = 1f;
            surf.eta.val = 1f;
            surf.sideflags = 1;

            /* remember where we started */

            set_flen(0);
            pos = fp.Tell();

            /* names */

            surf.name = getS0(fp);
            surf.srcname = getS0(fp);

            /* first subchunk header */

            id = getU4(fp);
            sz = getU2(fp);
            if (0 > get_flen()) goto Fail;

            /* process subchunks as they're encountered */

            while (1)
            {
                sz += sz & 1;
                set_flen(0);

                switch (id)
                {
                    case ID_COLR:
                        surf.color.rgb[0] = getF4(fp);
                        surf.color.rgb[1] = getF4(fp);
                        surf.color.rgb[2] = getF4(fp);
                        surf.color.eindex = getVX(fp);
                        break;

                    case ID_LUMI:
                        surf.luminosity.val = getF4(fp);
                        surf.luminosity.eindex = getVX(fp);
                        break;

                    case ID_DIFF:
                        surf.diffuse.val = getF4(fp);
                        surf.diffuse.eindex = getVX(fp);
                        break;

                    case ID_SPEC:
                        surf.specularity.val = getF4(fp);
                        surf.specularity.eindex = getVX(fp);
                        break;

                    case ID_GLOS:
                        surf.glossiness.val = getF4(fp);
                        surf.glossiness.eindex = getVX(fp);
                        break;

                    case ID_REFL:
                        surf.reflection.val.val = getF4(fp);
                        surf.reflection.val.eindex = getVX(fp);
                        break;

                    case ID_RFOP:
                        surf.reflection.options = getU2(fp);
                        break;

                    case ID_RIMG:
                        surf.reflection.cindex = getVX(fp);
                        break;

                    case ID_RSAN:
                        surf.reflection.seam_angle = getF4(fp);
                        break;

                    case ID_TRAN:
                        surf.transparency.val.val = getF4(fp);
                        surf.transparency.val.eindex = getVX(fp);
                        break;

                    case ID_TROP:
                        surf.transparency.options = getU2(fp);
                        break;

                    case ID_TIMG:
                        surf.transparency.cindex = getVX(fp);
                        break;

                    case ID_RIND:
                        surf.eta.val = getF4(fp);
                        surf.eta.eindex = getVX(fp);
                        break;

                    case ID_TRNL:
                        surf.translucency.val = getF4(fp);
                        surf.translucency.eindex = getVX(fp);
                        break;

                    case ID_BUMP:
                        surf.bump.val = getF4(fp);
                        surf.bump.eindex = getVX(fp);
                        break;

                    case ID_SMAN:
                        surf.smooth = getF4(fp);
                        break;

                    case ID_SIDE:
                        surf.sideflags = getU2(fp);
                        break;

                    case ID_CLRH:
                        surf.color_hilite.val = getF4(fp);
                        surf.color_hilite.eindex = getVX(fp);
                        break;

                    case ID_CLRF:
                        surf.color_filter.val = getF4(fp);
                        surf.color_filter.eindex = getVX(fp);
                        break;

                    case ID_ADTR:
                        surf.add_trans.val = getF4(fp);
                        surf.add_trans.eindex = getVX(fp);
                        break;

                    case ID_SHRP:
                        surf.dif_sharp.val = getF4(fp);
                        surf.dif_sharp.eindex = getVX(fp);
                        break;

                    case ID_GVAL:
                        surf.glow.val = getF4(fp);
                        surf.glow.eindex = getVX(fp);
                        break;

                    case ID_LINE:
                        surf.line.enabled = 1;
                        if (sz >= 2) surf.line.flags = getU2(fp);
                        if (sz >= 6) surf.line.size.val = getF4(fp);
                        if (sz >= 8) surf.line.size.eindex = getVX(fp);
                        break;

                    case ID_ALPH:
                        surf.alpha_mode = getU2(fp);
                        surf.alpha = getF4(fp);
                        break;

                    case ID_AVAL:
                        surf.alpha = getF4(fp);
                        break;

                    case ID_BLOK:
                        type = getU4(fp);

                        switch (type)
                        {
                            case ID_IMAP:
                            case ID_PROC:
                            case ID_GRAD:
                                tex = lwGetTexture(fp, sz - 4, type);
                                if (!tex) goto Fail;
                                if (!add_texture(surf, tex))
                                    lwFreeTexture(tex);
                                set_flen(4 + get_flen());
                                break;
                            case ID_SHDR:
                                shdr = lwGetShader(fp, sz - 4);
                                if (!shdr) goto Fail;
                                lwListInsert((void**)&surf.shader, shdr, (int(__cdecl *)(void*,void*))compare_shaders );
                                ++surf.nshaders;
                                set_flen(4 + get_flen());
                                break;
                        }
                        break;

                    default:
                        break;
                }

                /* error while reading current subchunk? */

                rlen = get_flen();
                if (rlen < 0 || rlen > sz) goto Fail;

                /* skip unread parts of the current subchunk */

                if (rlen < sz)
                    fp.Seek(sz - rlen, FS_SEEK_CUR);

                /* end of the SURF chunk? */

                if (cksize <= fp.Tell() - pos)
                    break;

                /* get the next subchunk header */

                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (6 != get_flen()) goto Fail;
            }

            return surf;

        Fail:
            if (surf) lwFreeSurface(surf);
            return null;
        }

        // Allocate and initialize a surface.
        public static lwSurface lwDefaultSurface()
        {
            lwSurface* surf;

            surf = (lwSurface*)Mem_ClearedAlloc(sizeof(lwSurface));
            if (!surf) return null;

            surf.color.rgb[0] = 0.78431f;
            surf.color.rgb[1] = 0.78431f;
            surf.color.rgb[2] = 0.78431f;
            surf.diffuse.val = 1f;
            surf.glossiness.val = 0.4f;
            surf.bump.val = 1f;
            surf.eta.val = 1f;
            surf.sideflags = 1;

            return surf;
        }

        #endregion

        #region lwob.c

        // IDs specific to LWOB
        const int ID_SRFS = 'S' << 24 | 'R' << 16 | 'F' << 8 | 'S';
        //const int ID_FLAG = 'F' << 24 | 'L' << 16 | 'A' << 8 | 'G';
        const int ID_VLUM = 'V' << 24 | 'L' << 16 | 'U' << 8 | 'M';
        const int ID_VDIF = 'V' << 24 | 'D' << 16 | 'I' << 8 | 'F';
        const int ID_VSPC = 'V' << 24 | 'S' << 16 | 'P' << 8 | 'C';
        const int ID_RFLT = 'R' << 24 | 'F' << 16 | 'L' << 8 | 'T';
        const int ID_BTEX = 'B' << 24 | 'T' << 16 | 'E' << 8 | 'X';
        const int ID_CTEX = 'C' << 24 | 'T' << 16 | 'E' << 8 | 'X';
        const int ID_DTEX = 'D' << 24 | 'T' << 16 | 'E' << 8 | 'X';
        const int ID_LTEX = 'L' << 24 | 'T' << 16 | 'E' << 8 | 'X';
        const int ID_RTEX = 'R' << 24 | 'T' << 16 | 'E' << 8 | 'X';
        const int ID_STEX = 'S' << 24 | 'T' << 16 | 'E' << 8 | 'X';
        const int ID_TTEX = 'T' << 24 | 'T' << 16 | 'E' << 8 | 'X';
        const int ID_TFLG = 'T' << 24 | 'F' << 16 | 'L' << 8 | 'G';
        const int ID_TSIZ = 'T' << 24 | 'S' << 16 | 'I' << 8 | 'Z';
        const int ID_TCTR = 'T' << 24 | 'C' << 16 | 'T' << 8 | 'R';
        const int ID_TFAL = 'T' << 24 | 'F' << 16 | 'A' << 8 | 'L';
        const int ID_TVEL = 'T' << 24 | 'V' << 16 | 'E' << 8 | 'L';
        const int ID_TCLR = 'T' << 24 | 'C' << 16 | 'L' << 8 | 'R';
        const int ID_TVAL = 'T' << 24 | 'V' << 16 | 'A' << 8 | 'L';
        //const int ID_TAMP = 'T' << 24 | 'A' << 16 | 'M' << 8 | 'P';
        //const int ID_TIMG = 'T' << 24 | 'I' << 16 | 'M' << 8 | 'G';
        const int ID_TAAS = 'T' << 24 | 'A' << 16 | 'A' << 8 | 'S';
        const int ID_TREF = 'T' << 24 | 'R' << 16 | 'E' << 8 | 'F';
        const int ID_TOPC = 'T' << 24 | 'O' << 16 | 'P' << 8 | 'C';
        const int ID_SDAT = 'S' << 24 | 'D' << 16 | 'A' << 8 | 'T';
        const int ID_TFP0 = 'T' << 24 | 'F' << 16 | 'P' << 8 | '0';
        const int ID_TFP1 = 'T' << 24 | 'F' << 16 | 'P' << 8 | '1';

        // Add a clip to the clip list.  Used to store the contents of an RIMG or TIMG surface subchunk.
        static int add_clip(char* s, lwClip** clist, int* nclips)
        {
            lwClip* clip;
            char* p;

            clip = (lwClip*)Mem_ClearedAlloc(sizeof(lwClip));
            if (!clip) return 0;

            clip.contrast.val = 1f;
            clip.brightness.val = 1f;
            clip.saturation.val = 1f;
            clip.gamma.val = 1f;

            if ((p = strstr(s, "(sequence)")))
            {
                p[-1] = 0;
                clip.type = ID_ISEQ;
                clip.source.seq.prefix = s;
                clip.source.seq.digits = 3;
            }
            else
            {
                clip.type = ID_STIL;
                clip.source.still.name = s;
            }

            *nclips += 1;
            clip.index = *nclips;

            lwListAdd((void**)clist, clip);

            return clip.index;
        }

        // Add a triple of envelopes to simulate the old texture velocity parameters.
        static int add_tvel(float pos[], float vel[], lwEnvelope** elist, int* nenvs)
        {
            lwEnvelope* env;
            lwKey* key0, *key1;
            int i;

            for (i = 0; i < 3; i++)
            {
                env = (lwEnvelope*)Mem_ClearedAlloc(sizeof(lwEnvelope));
                key0 = (lwKey*)Mem_ClearedAlloc(sizeof(lwKey));
                key1 = (lwKey*)Mem_ClearedAlloc(sizeof(lwKey));
                if (!env || !key0 || !key1) return 0;

                key0.next = key1;
                key0.value = pos[i];
                key0.time = 0f;
                key1.prev = key0;
                key1.value = pos[i] + vel[i] * 30f;
                key1.time = 1f;
                key0.shape = key1.shape = ID_LINE;

                env.index = *nenvs + i + 1;
                env.type = 0x0301 + i;
                env.name = (char*)Mem_ClearedAlloc(11);
                if (env.name)
                {
                    strcpy(env.name, "Position.X");
                    env.name[9] += i;
                }
                env.key = key0;
                env.nkeys = 2;
                env.behavior[0] = BEH_LINEAR;
                env.behavior[1] = BEH_LINEAR;

                lwListAdd((void**)elist, env);
            }

            *nenvs += 3;
            return env.index - 2;
        }

        // Create a new texture for BTEX, CTEX, etc. subchunks.
        static lwTexture get_texture(string s)
        {
            var tex = new lwTexture(); if (tex == null) return null;
            tex.tmap.size.val[0] = tex.tmap.size.val[1] = tex.tmap.size.val[2] = 1f;
            tex.opacity.val = 1f;
            tex.enabled = 1;

            if (s.Contains("Image Map"))
            {
                tex.type = ID_IMAP;
                if (s.Contains("Planar")) tex.param.imap.projection = 0;
                else if (s.Contains("Cylindrical")) tex.param.imap.projection = 1;
                else if (s.Contains("Spherical")) tex.param.imap.projection = 2;
                else if (s.Contains("Cubic")) tex.param.imap.projection = 3;
                else if (s.Contains("Front")) tex.param.imap.projection = 4;
                tex.param.imap.aa_strength = 1f;
                tex.param.imap.amplitude.val = 1f;
            }
            else { tex.type = ID_PROC; tex.param.proc.name = s; }

            return tex;
        }

        public static lwSurface lwGetSurface5(VFile fp, int cksize, lwObject obj)
        {
            lwSurface surf;
            lwTexture tex = null;
            lwPlugin shdr = null;
            string s;
            float v[3];
            uint id, flags;
            ushort sz;
            int pos, rlen, i;

            // allocate the Surface structure
            surf = new lwSurface(); if (surf == null) goto Fail;

            // non-zero defaults
            surf.color.rgb[0] = 0.78431f;
            surf.color.rgb[1] = 0.78431f;
            surf.color.rgb[2] = 0.78431f;
            surf.diffuse.val = 1f;
            surf.glossiness.val = 0.4f;
            surf.bump.val = 1f;
            surf.eta.val = 1f;
            surf.sideflags = 1;

            // remember where we started
            set_flen(0);
            pos = fp.Tell;

            // name
            surf.name = getS0(fp);

            // first subchunk header
            id = getU4(fp);
            sz = getU2(fp);
            if (get_flen() < 0) goto Fail;

            // process subchunks as they're encountered
            while (true)
            {
                sz += (ushort)(sz & 1);
                set_flen(0);

                switch (id)
                {
                    case ID_COLR:
                        surf.color.rgb[0] = getU1(fp) / 255f;
                        surf.color.rgb[1] = getU1(fp) / 255f;
                        surf.color.rgb[2] = getU1(fp) / 255f;
                        break;
                    case ID_FLAG:
                        flags = getU2(fp);
                        if ((flags & 4) != 0) surf.smooth = 1.56207f;
                        if ((flags & 8) != 0) surf.color_hilite.val = 1f;
                        if ((flags & 16) != 0) surf.color_filter.val = 1f;
                        if ((flags & 128) != 0) surf.dif_sharp.val = 0.5f;
                        if ((flags & 256) != 0) surf.sideflags = 3;
                        if ((flags & 512) != 0) surf.add_trans.val = 1f;
                        break;
                    case ID_LUMI: surf.luminosity.val = getI2(fp) / 256f; break;
                    case ID_VLUM: surf.luminosity.val = getF4(fp); break;
                    case ID_DIFF: surf.diffuse.val = getI2(fp) / 256f; break;
                    case ID_VDIF: surf.diffuse.val = getF4(fp); break;
                    case ID_SPEC: surf.specularity.val = getI2(fp) / 256f; break;
                    case ID_VSPC: surf.specularity.val = getF4(fp); break;
                    case ID_GLOS: surf.glossiness.val = (float)Math.Log((float)getU2(fp)) / 20.7944f; break;
                    case ID_SMAN: surf.smooth = getF4(fp); break;
                    case ID_REFL: surf.reflection.val.val = getI2(fp) / 256f; break;
                    case ID_RFLT: surf.reflection.options = getU2(fp); break;
                    case ID_RIMG:
                        s = getS0(fp);
                        surf.reflection.cindex = add_clip(s, &obj.clip, &obj.nclips);
                        surf.reflection.options = 3;
                        break;

                    case ID_RSAN: surf.reflection.seam_angle = getF4(fp); break;
                    case ID_TRAN: surf.transparency.val.val = getI2(fp) / 256f; break;
                    case ID_RIND: surf.eta.val = getF4(fp); break;
                    case ID_BTEX:
                        s = getbytes(fp, sz);
                        tex = get_texture(s);
                        lwListAdd(surf.bump.tex, tex);
                        break;

                    case ID_CTEX:
                        s = getbytes(fp, sz);
                        tex = get_texture(s);
                        lwListAdd(surf.color.tex, tex);
                        break;

                    case ID_DTEX:
                        s = getbytes(fp, sz);
                        tex = get_texture(s);
                        lwListAdd(surf.diffuse.tex, tex);
                        break;

                    case ID_LTEX:
                        s = getbytes(fp, sz);
                        tex = get_texture(s);
                        lwListAdd(surf.luminosity.tex, tex);
                        break;

                    case ID_RTEX:
                        s = getbytes(fp, sz);
                        tex = get_texture(s);
                        lwListAdd(surf.reflection.val.tex, tex);
                        break;

                    case ID_STEX:
                        s = getbytes(fp, sz);
                        tex = get_texture(s);
                        lwListAdd(surf.specularity.tex, tex);
                        break;

                    case ID_TTEX:
                        s = getbytes(fp, sz);
                        tex = get_texture(s);
                        lwListAdd(surf.transparency.val.tex, tex);
                        break;

                    case ID_TFLG:
                        flags = getU2(fp);

                        i = 0;
                        if ((flags & 2) != 0) i = 1;
                        if ((flags & 4) != 0) i = 2;
                        tex.axis = i;
                        if (tex.type == ID_IMAP) tex.param.imap.axis = i;
                        else tex.param.proc.axis = i;

                        if ((flags & 8) != 0) tex.tmap.coord_sys = 1;
                        if ((flags & 16) != 0) tex.negative = 1;
                        if ((flags & 32) != 0) tex.param.imap.pblend = 1;
                        if ((flags & 64) != 0) { tex.param.imap.aa_strength = 1f; tex.param.imap.aas_flags = 1; }
                        break;
                    case ID_TSIZ: for (i = 0; i < 3; i++) tex.tmap.size.val[i] = getF4(fp); break;
                    case ID_TCTR: for (i = 0; i < 3; i++) tex.tmap.center.val[i] = getF4(fp); break;
                    case ID_TFAL: for (i = 0; i < 3; i++) tex.tmap.falloff.val[i] = getF4(fp); break;
                    case ID_TVEL:
                        for (i = 0; i < 3; i++) v[i] = getF4(fp);
                        tex.tmap.center.eindex = add_tvel(tex.tmap.center.val, v, &obj.env, &obj.nenvs);
                        break;
                    case ID_TCLR: if (tex.type == ID_PROC) for (i = 0; i < 3; i++) tex.param.proc.value[i] = getU1(fp) / 255f; break;
                    case ID_TVAL: tex.param.proc.value[0] = getI2(fp) / 256f; break;
                    case ID_TAMP: if (tex.type == ID_IMAP) tex.param.imap.amplitude.val = getF4(fp); break;
                    case ID_TIMG: s = getS0(fp); tex.param.imap.cindex = add_clip(s, &obj.clip, &obj.nclips); break;
                    case ID_TAAS:
                        tex.param.imap.aa_strength = getF4(fp);
                        tex.param.imap.aas_flags = 1;
                        break;
                    case ID_TREF: tex.tmap.ref_object = getbytes(fp, sz); break;
                    case ID_TOPC: tex.opacity.val = getF4(fp); break;
                    case ID_TFP0: if (tex.type == ID_IMAP) tex.param.imap.wrapw.val = getF4(fp); break;
                    case ID_TFP1: if (tex.type == ID_IMAP) tex.param.imap.wraph.val = getF4(fp); break;
                    case ID_SHDR:
                        shdr = new lwPlugin(); if (shdr == null) goto Fail;
                        shdr.name = getbytes(fp, sz);
                        lwListAdd(surf.shader, shdr);
                        surf.nshaders++;
                        break;
                    case ID_SDAT:
                        if (shdr == null) goto Fail;
                        shdr.data = getbytes(fp, sz);
                        break;
                    default: break;
                }

                // error while reading current subchunk?
                rlen = get_flen();
                if (rlen < 0 || rlen > sz) goto Fail;

                // skip unread parts of the current subchunk
                if (rlen < sz) fp.Seek(sz - rlen, FS_SEEK.CUR);

                // end of the SURF chunk?
                if (cksize <= fp.Tell - pos) break;

                // get the next subchunk header
                set_flen(0);
                id = getU4(fp);
                sz = getU2(fp);
                if (get_flen() != 6) goto Fail;
            }

            return surf;

        Fail:
            if (surf != null) lwFreeSurface(surf);
            return null;
        }

        // Read polygon records from a POLS chunk in an LWOB file.  The polygons are added to the array in the lwPolygonList.
        public static bool lwGetPolygons5(VFile fp, int cksize, lwPolygonList plist, int ptoffset)
        {
            lwPolygon pp;
            lwPolVert pv;
            int i, nv;
            int j;

            if (cksize == 0) return true;

            // read the whole chunk

            set_flen(0);
            var buf = getbytes(fp, cksize); if (buf == null) goto Fail;
            fixed (byte* bufB = buf)
            {
                // count the polygons and vertices
                var nverts = 0;
                var npols = 0;
                var bp = bufB;

                while (bp < bufB + cksize)
                {
                    nv = sgetU2(ref bp);
                    nverts += nv;
                    npols++;
                    bp += 2 * nv;
                    i = sgetI2(ref bp);
                    if (i < 0) bp += 2; // detail polygons
                }

                if (lwAllocPolygons(plist, npols, nverts) == 0) goto Fail;

                // fill in the new polygons
                bp = bufB;
                pp = plist.pol + plist.offset;
                pv = plist.pol[0].v + plist.voffset;

                for (i = 0; i < npols; i++)
                {
                    nv = sgetU2(ref bp);

                    pp.nverts = nv;
                    pp.type = ID_FACE;
                    if (!pp.v) pp.v = pv;
                    for (j = 0; j < nv; j++) pv[j].index = sgetU2(ref bp) + ptoffset;
                    j = sgetI2(ref bp);
                    if (j < 0) { j = -j; bp += 2; }
                    j -= 1;
                    pp.surf = (lwSurface*)j;

                    pp++;
                    pv += nv;
                }
            }
            return true;

        Fail:
            lwFreePolygons(plist);
            return false;
        }

        // Returns the contents of an LWOB, given its filename, or null if the file couldn't be loaded.  On failure, failID and failpos can be used
        // to diagnose the cause.
        // 
        // 1.  If the file isn't an LWOB, failpos will contain 12 and failID will be unchanged.
        // 
        // 2.  If an error occurs while reading an LWOB, failID will contain the most recently read IFF chunk ID, and failpos will contain the
        //     value returned by fp.Tell() at the time of the failure.
        // 
        // 3.  If the file couldn't be opened, or an error occurs while reading the first 12 bytes, both failID and failpos will be unchanged.
        // 
        // If you don't need this information, failID and failpos can be null.
        public static lwObject lwGetObject5(string filename, ref uint failID, ref int failpos)
        {
            lwObject obj;
            lwLayer layer;
            lwNode node;
            uint id, formsize, type;

            // open the file. read the first 12 bytes
            var fp = fileSystem.OpenFileRead(filename); if (fp == null) return null;

            set_flen(0);
            id = getU4(fp);
            formsize = getU4(fp);
            type = getU4(fp);
            if (get_flen() != 12) { fileSystem.CloseFile(fp); return null; }

            // LWOB?

            if (id != ID_FORM || type != ID_LWOB) { fileSystem.CloseFile(fp); failpos = 12; return null; }

            // allocate an object and a default layer
            obj = new lwObject();
            layer = new lwLayer();
            obj.layer = layer;
            obj.nlayers = 1;

            // get the first chunk header

            id = getU4(fp);
            var cksize = (int)getU4(fp);
            if (get_flen() < 0) goto Fail2;

            // process chunks as they're encountered
            while (true)
            {
                cksize += cksize & 1;

                switch (id)
                {
                    case ID_PNTS:
                        if (lwGetPoints(fp, cksize, ref layer.point) == 0) goto Fail2;
                        break;

                    case ID_POLS:
                        if (lwGetPolygons5(fp, cksize, ref layer.polygon, layer.point.offset) == 0) goto Fail2;
                        break;

                    case ID_SRFS:
                        if (lwGetTags(fp, cksize, ref obj.taglist) == 0) goto Fail2;
                        break;

                    case ID_SURF:
                        node = lwGetSurface5(fp, cksize, obj); if (node == null) goto Fail2;
                        lwListAdd(ref obj.surf, (lwSurface)node);
                        obj.nsurfs++;
                        break;

                    default:
                        fp.Seek(cksize, FS_SEEK.CUR);
                        break;
                }

                // end of the file?

                if (formsize <= fp.Tell - 8) break;

                // get the next chunk header
                set_flen(0);
                id = getU4(fp);
                cksize = (int)getU4(fp);
                if (get_flen() != 8) goto Fail2;
            }

            fileSystem.CloseFile(fp);

            lwGetBoundingBox(layer.point, layer.bbox);
            lwGetPolyNormals(layer.point, layer.polygon);
            if (lwGetPointPolygons(layer.point, layer.polygon) == 0) goto Fail2;
            if (lwResolvePolySurfaces(layer.polygon, obj.taglist, obj.surf, obj.nsurfs) == 0) goto Fail2;
            lwGetVertNormals(layer.point, layer.polygon);

            return obj;

        Fail2:
            failID = id;
            if (fp != null) { failpos = fp.Tell; fileSystem.CloseFile(fp); }
            lwFreeObject(obj);
            return null;
        }

        #endregion

        #region list.c

        // Free the items in a list.
        static void lwListFree<T>(lwNode list, Action<T> freeNode) where T : lwNode
        {
            lwNode node, next;

            node = list;
            while (node != null) { next = node.next; freeNode((T)node); node = next; }
        }

        // Append a node to a list.
        static void lwListAdd<T>(ref T list, T node) where T : lwNode
        {
            lwNode head, tail = null;

            head = list;
            if (head == null) { list = node; return; }
            while (head != null) { tail = head; head = head.next; }
            tail.next = node;
            node.prev = tail;
        }

        // Insert a node into a list in sorted order.
        static void lwListInsert<T>(ref T vlist, T vitem, Func<T, T, int> compare) where T : lwNode
        {
            lwNode list, item, node, prev;

            if (vlist == null) { vlist = vitem; return; }

            list = vlist; item = vitem; node = list; prev = null;
            while (node != null)
            {
                if (compare((T)node, (T)item) > 0) break;
                prev = node;
                node = node.next;
            }

            if (prev != null) { list = item; node.prev = item; item.next = node; }
            else if (node != null) { prev.next = item; item.prev = prev; }
            else { item.next = node; item.prev = prev; prev.next = item; node.prev = item; }
        }

        #endregion

        #region vecmath.c

        static float dot(float[] a, float[] b)
            => a[0] * b[0] + a[1] * b[1] + a[2] * b[2];

        static void cross(float[] a, float[] b, float[] c)
        {
            c[0] = a[1] * b[2] - a[2] * b[1];
            c[1] = a[2] * b[0] - a[0] * b[2];
            c[2] = a[0] * b[1] - a[1] * b[0];
        }

        static void normalize(float[] v)
        {
            var r = (float)MathX.Sqrt(dot(v, v));
            if (r > 0) { v[0] /= r; v[1] /= r; v[2] /= r; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float vecangle(float[] a, float[] b)
            => (float)MathX.ACos(dot(a, b));

        #endregion

        #region lwio.c

        // flen
        //
        // This accumulates a count of the number of bytes read.Callers can set it at the beginning of a sequence of reads and then retrieve it to get
        // the number of bytes actually read.If one of the I/O functions fails, flen is set to an error code, after which the I/O functions ignore
        // read requests until flen is reset.

        const int FLEN_ERROR = -9999;

        static int flen;

        public static void set_flen(int i)
            => flen = i;

        public static int get_flen()
            => flen;

        public static byte[] getbytes(VFile fp, int size)
        {
            if (flen == FLEN_ERROR) return null;
            if (size < 0) { flen = FLEN_ERROR; return null; }
            var data = new byte[size]; if (data == null) { flen = FLEN_ERROR; return null; }
            if (size != fp.Read(data, size)) { flen = FLEN_ERROR; return null; }

            flen += size;
            return data;
        }

        public static void skipbytes(VFile fp, int n)
        {
            if (flen == FLEN_ERROR) return;
            if (fp.Seek(n, FS_SEEK.CUR) != 0) flen = FLEN_ERROR;
            else flen += n;
        }

        public static int getI1(VFile fp)
        {
            int i, c;

            if (flen == FLEN_ERROR) return 0;
            c = 0;
            i = fp.Read((byte*)&c, 1);
            if (i < 0) { flen = FLEN_ERROR; return 0; }
            if (c > 127) c -= 256;
            flen += 1;
            return c;
        }

        public static short getI2(VFile fp)
        {
            short i;

            if (flen == FLEN_ERROR) return 0;
            if (fp.Read((byte*)&i, 2) != 2) { flen = FLEN_ERROR; return 0; }
            BigRevBytes(&i, 2, 1);
            flen += 2;
            return i;
        }

        public static int getI4(VFile fp)
        {
            int i;

            if (flen == FLEN_ERROR) return 0;
            if (fp.Read((byte*)&i, 4) != 4) { flen = FLEN_ERROR; return 0; }
            BigRevBytes(&i, 4, 1);
            flen += 4;
            return i;
        }

        public static byte getU1(VFile fp)
        {
            int c;

            if (flen == FLEN_ERROR) return 0;
            c = 0;
            if (fp.Read((byte*)&c, 1) < 0) { flen = FLEN_ERROR; return 0; }
            flen += 1;
            return (byte)c;
        }

        public static ushort getU2(VFile fp)
        {
            ushort i;

            if (flen == FLEN_ERROR) return 0;
            if (fp.Read((byte*)&i, 2) != 2) { flen = FLEN_ERROR; return 0; }
            BigRevBytes(&i, 2, 1);
            flen += 2;
            return i;
        }

        public static uint getU4(VFile fp)
        {
            uint i;

            if (flen == FLEN_ERROR) return 0;
            if (fp.Read((byte*)&i, 4) != 4) { flen = FLEN_ERROR; return 0; }
            BigRevBytes(&i, 4, 1);
            flen += 4;
            return i;
        }

        public static int getVX(VFile fp)
        {
            byte c;
            int i;

            if (flen == FLEN_ERROR) return 0;

            c = 0;
            if (fp.Read(&c, 1) == -1) return 0;

            if (c != 0xFF)
            {
                i = c << 8;
                c = 0;
                if (fp.Read(&c, 1) == -1) return 0;
                i |= c;
                flen += 2;
            }
            else
            {
                c = 0;
                if (fp.Read(&c, 1) == -1) return 0;
                i = c << 16;
                c = 0;
                if (fp.Read(&c, 1) == -1) return 0;
                i |= c << 8;
                c = 0;
                if (fp.Read(&c, 1) == -1) return 0;
                i |= c;
                flen += 4;
            }

            return i;

        }
        public static float getF4(VFile fp)
        {
            float f;

            if (flen == FLEN_ERROR) return 0f;
            if (fp.Read((byte*)&f, 4) != 4) { flen = FLEN_ERROR; return 0f; }
            BigRevBytes(&f, 4, 1);
            flen += 4;

            if (MathX.FLOAT_IS_DENORMAL(f)) f = 0f;
            return f;

        }
        public static string getS0(VFile fp)
        {
            byte[] s; int i, c, len, pos;

            if (flen == FLEN_ERROR) return null;

            pos = fp.Tell;
            for (i = 1; ; i++)
            {
                c = 0;
                if (fp.Read((byte*)&c, 1) == -1) { flen = FLEN_ERROR; return null; }
                if (c == 0) break;
            }

            if (i == 1)
            {
                if (fp.Seek(pos + 2, FS_SEEK.SET) != 0) flen = FLEN_ERROR;
                else flen += 2;
                return null;
            }

            len = i + (i & 1);
            s = new byte[len]; if (s == null) { flen = FLEN_ERROR; return null; }

            if (fp.Seek(pos, FS_SEEK.SET) != 0) { flen = FLEN_ERROR; return null; }
            if (len != fp.Read(s, len)) { flen = FLEN_ERROR; return null; }

            flen += len;
            return Encoding.ASCII.GetString(s, 0, len);
        }

        public static int sgetI1(ref byte* bp)
        {
            int i;

            if (flen == FLEN_ERROR) return 0;
            i = *bp;
            if (i > 127) i -= 256;
            flen += 1;
            bp += 1;
            return i;
        }

        public static short sgetI2(ref byte* bp)
        {
            short i;

            if (flen == FLEN_ERROR) return 0;
            i = *bp;
            BigRevBytes(&i, 2, 1);
            flen += 2;
            bp += 2;
            return i;
        }

        public static int sgetI4(ref byte* bp)
        {
            int i;

            if (flen == FLEN_ERROR) return 0;
            i = *bp;
            BigRevBytes(&i, 4, 1);
            flen += 4;
            bp += 4;
            return i;

        }
        public static byte sgetU1(ref byte* bp)
        {
            byte c;

            if (flen == FLEN_ERROR) return 0;
            c = *bp;
            flen += 1;
            bp += 1;
            return c;

        }
        public static ushort sgetU2(ref byte* bp)
        {
            byte* buf = bp;
            ushort i;

            if (flen == FLEN_ERROR) return 0;
            i = (ushort)((buf[0] << 8) | buf[1]);
            flen += 2;
            *bp += 2;
            return i;
        }

        public static uint sgetU4(ref byte* bp)
        {
            uint i;

            if (flen == FLEN_ERROR) return 0;
            i = *bp;
            BigRevBytes(&i, 4, 1);
            flen += 4;
            bp += 4;
            return i;
        }

        public static int sgetVX(ref byte* bp)
        {
            byte* buf = bp;
            int i;

            if (flen == FLEN_ERROR) return 0;
            if (buf[0] != 0xFF)
            {
                i = buf[0] << 8 | buf[1];
                flen += 2;
                bp += 2;
            }
            else
            {
                i = (buf[1] << 16) | (buf[2] << 8) | buf[3];
                flen += 4;
                bp += 4;
            }
            return i;
        }

        public static float sgetF4(ref byte* bp)
        {
            float f;

            if (flen == FLEN_ERROR) return 0f;
            f = *bp;
            BigRevBytes(&f, 4, 1);
            flen += 4;
            bp += 4;

            if (MathX.FLOAT_IS_DENORMAL(f)) f = 0f;
            return f;
        }

        public static string sgetS0(ref byte* bp)
        {
            byte[] s;
            byte* buf = bp;
            int len;

            if (flen == FLEN_ERROR) return null;

            len = stringX.strlen(buf) + 1;
            if (len == 1) { flen += 2; bp += 2; return null; }
            len += len & 1;
            s = new byte[len]; if (s == null) { flen = FLEN_ERROR; return null; }

            fixed (byte* sB = s) Unsafe.CopyBlock(sB, buf, (uint)len);
            flen += len;
            bp += len;
            return Encoding.ASCII.GetString(s);
        }

        #endregion
    }
}

