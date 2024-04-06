using System.Diagnostics;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.Gngine.UI;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public unsafe struct DecalInfo
    {
        public int stayTime;        // msec for no change
        public int fadeTime;        // msec to fade vertex colors over
        public fixed float start[4];      // vertex color at spawn (possibly out of 0.0 - 1.0 range, will clamp after calc)
        public fixed float end[4];            // vertex color at fade-out (possibly out of 0.0 - 1.0 range, will clamp after calc)
    }

    public enum DFRM
    {
        NONE,
        SPRITE,
        TUBE,
        FLARE,
        EXPAND,
        MOVE,
        EYEBALL,
        PARTICLE,
        PARTICLE2,
        TURB
    }

    public enum DI
    {
        STATIC,
        SCRATCH,     // video, screen wipe, etc
        CUBE_RENDER,
        MIRROR_RENDER,
        XRAY_RENDER,
        REMOTE_RENDER
    }

    // note: keep opNames[] in sync with changes
    public enum OP_TYPE
    {
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
        MOD,
        TABLE,
        GT,
        GE,
        LT,
        LE,
        EQ,
        NE,
        AND,
        OR,
        SOUND
    }

    public enum EXP_REG
    {
        TIME,

        PARM0,
        PARM1,
        PARM2,
        PARM3,
        PARM4,
        PARM5,
        PARM6,
        PARM7,
        PARM8,
        PARM9,
        PARM10,
        PARM11,

        GLOBAL0,
        GLOBAL1,
        GLOBAL2,
        GLOBAL3,
        GLOBAL4,
        GLOBAL5,
        GLOBAL6,
        GLOBAL7,

        NUM_PREDEFINED
    }

    public struct ExpOp
    {
        public OP_TYPE opType;
        public int a, b, c;
    }

    public unsafe struct ColorStage
    {
        public fixed int registers[4];
    }

    public enum TG
    {
        EXPLICIT,
        DIFFUSE_CUBE,
        REFLECT_CUBE,
        SKYBOX_CUBE,
        WOBBLESKY_CUBE,
        SCREEN,          // screen aligned, for mirrorRenders and screen space temporaries
        SCREEN2,
        GLASSWARP
    }

    public struct TextureStage
    {
        public Cinematic cinematic;
        public Image image;
        public TG texgen;
        public bool hasMatrix;
        public int[][] matrix; //[2][3];   // we only allow a subset of the full projection matrix

        // dynamic image variables
        public DI dynamic;
        public int width, height;
        public int dynamicFrameCount;
    }

    // the order BUMP / DIFFUSE / SPECULAR is necessary for interactions to draw correctly on low end cards
    public enum SL
    {
        AMBIENT,                     // execute after lighting
        BUMP,
        DIFFUSE,
        SPECULAR
    }

    // cross-blended terrain textures need to modulate the color by the vertex color to smoothly blend between two textures
    public enum SVC
    {
        IGNORE,
        MODULATE,
        INVERSE_MODULATE
    }

    public class NewShaderStage
    {
        public int vertexProgram;
        public int numVertexParms;
        public int[,] vertexParms = new int[ShaderStage.MAX_VERTEX_PARMS, 4];   // evaluated register indexes

        public int fragmentProgram;
        public int numFragmentProgramImages;
        public Image[] fragmentProgramImages = new Image[ShaderStage.MAX_FRAGMENT_IMAGES];

        public MegaTexture megaTexture;        // handles all the binding and parameter setting
    }

    public class ShaderStage
    {
        public const int MAX_FRAGMENT_IMAGES = 8;
        public const int MAX_VERTEX_PARMS = 4;

        public int conditionRegister;  // if registers[conditionRegister] == 0, skip stage
        public SL lighting;           // determines which passes interact with lights
        public int drawStateBits;
        public ColorStage color;
        public bool hasAlphaTest;
        public int alphaTestRegister;
        public TextureStage texture;
        public SVC vertexColor;
        public bool ignoreAlphaTest;   // this stage should act as translucent, even if the surface is alpha tested
        public float privatePolygonOffset; // a per-stage polygon offset

        public NewShaderStage newStage;         // vertex / fragment program based stage
    }

    public enum MC
    {
        BAD,
        OPAQUE,          // completely fills the triangle, will have black drawn on fillDepthBuffer
        PERFORATED,      // may have alpha tested holes
        TRANSLUCENT      // blended with background
    }

    public enum SS
    {
        SUBVIEW = -3,    // mirrors, viewscreens, etc
        GUI = -2,        // guis
        BAD = -1,
        OPAQUE,          // opaque

        PORTAL_SKY,

        DECAL,           // scorch marks, etc.

        FAR,
        MEDIUM,          // normal translucent
        CLOSE,

        ALMOST_NEAREST,  // gun smoke puffs

        NEAREST,         // screen blood blobs

        POST_PROCESS = 100   // after a screen copy to texture
    }

    public enum CT : int
    {
        FRONT_SIDED,
        BACK_SIDED,
        TWO_SIDED
    }

    // material flags
    [Flags]
    public enum MF
    {
        DEFAULTED = 1 << 0,
        POLYGONOFFSET = 1 << 1,
        NOSHADOWS = 1 << 2,
        FORCESHADOWS = 1 << 3,
        NOSELFSHADOW = 1 << 4,
        NOPORTALFOG = 1 << 5,    // this fog volume won't ever consider a portal fogged out
        EDITOR_VISIBLE = 1 << 6  // in use (visible) per editor
    }

    // contents flags, NOTE: make sure to keep the defines in doom_defs.script up to date with these!
    [Flags]
    public enum CONTENTS
    {
        SOLID = 1 << 0,    // an eye is never valid in a solid
        OPAQUE = 1 << 1,   // blocks visibility (for ai)
        WATER = 1 << 2,    // used for water
        PLAYERCLIP = 1 << 3,   // solid to players
        MONSTERCLIP = 1 << 4,  // solid to monsters
        MOVEABLECLIP = 1 << 5, // solid to moveable entities
        IKCLIP = 1 << 6,   // solid to IK
        BLOOD = 1 << 7,    // used to detect blood decals
        BODY = 1 << 8, // used for actors
        PROJECTILE = 1 << 9,   // used for projectiles
        CORPSE = 1 << 10,  // used for dead bodies
        RENDERMODEL = 1 << 11, // used for render models for collision detection
        TRIGGER = 1 << 12, // used for triggers
        AAS_SOLID = 1 << 13,   // solid for AAS
        AAS_OBSTACLE = 1 << 14,    // used to compile an obstacle into AAS that can be enabled/disabled
        FLASHLIGHT_TRIGGER = 1 << 15,  // used for triggers that are activated by the flashlight

        // contents used by utils
        AREAPORTAL = 1 << 20,  // portal separating renderer areas
        NOCSG = 1 << 21,   // don't cut this brush with CSG operations in the editor

        REMOVE_UTIL = ~(AREAPORTAL | NOCSG)
    }

    public enum SURFTYPE
    {
        NONE,                  // default type
        METAL,
        STONE,
        FLESH,
        WOOD,
        CARDBOARD,
        LIQUID,
        GLASS,
        PLASTIC,
        RICOCHET,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15
    }

    // surface flags
    [Flags]
    public enum SURF
    {
        TYPE_BIT0 = 1 << 0,    // encodes the material type (metal, flesh, concrete, etc.)
        TYPE_BIT1 = 1 << 1,    // "
        TYPE_BIT2 = 1 << 2,    // "
        TYPE_BIT3 = 1 << 3,    // "
        TYPE_MASK = (1 << Material.NUM_SURFACE_BITS) - 1,

        NODAMAGE = 1 << 4, // never give falling damage
        SLICK = 1 << 5,    // effects game physics
        COLLISION = 1 << 6,    // collision surface
        LADDER = 1 << 7,   // player can climb up this surface
        NOIMPACT = 1 << 8, // don't make missile explosions
        NOSTEPS = 1 << 9,  // no footstep sounds
        DISCRETE = 1 << 10,    // not clipped or merged by utilities
        NOFRAGMENT = 1 << 11,  // dmap won't cut surface at each bsp boundary
        NULLNORMAL = 1 << 12   // renderbump will draw this surface as 0x80 0x80 0x80, which won't collect light from any angle
    }

    public class Material : Decl
    {
        // surface types
        public const int NUM_SURFACE_BITS = 4;
        public const int MAX_SURFACE_TYPES = 1 << NUM_SURFACE_BITS;

        // these don't effect per-material storage, so they can be very large
        public const int MAX_SHADER_STAGES = 256;

        public const int MAX_TEXGEN_REGISTERS = 4;

        public const int MAX_ENTITY_SHADER_PARMS = 12;

        //public Material();

        public override int Size => throw new NotImplementedException();
        public override bool SetDefaultText() => throw new NotImplementedException();
        public override string DefaultDefinition => throw new NotImplementedException();
        public override bool Parse(string text) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();
        public override void Print() => throw new NotImplementedException();

        //BSM Nerve: Added for material editor
        public bool Save(string fileName = null) => throw new NotImplementedException();

        // returns the internal image name for stage 0, which can be used for the renderer CaptureRenderToImage() call
        // I'm not really sure why this needs to be virtual...
        public virtual string ImageName() => throw new NotImplementedException();

        public void ReloadImages(bool force) => throw new NotImplementedException();

        // returns number of stages this material contains
        public int NumStages => numStages;

        // get a specific stage
        public ShaderStage GetStage(int index)
        {
            Debug.Assert(index >= 0 && index < numStages);
            return stages[index];
        }

        // get the first bump map stage, or NULL if not present. used for bumpy-specular
        public ShaderStage GetBumpStage() => throw new NotImplementedException();

        // returns true if the material will draw anything at all.  Triggers, portals, etc, will not have anything to draw.  A not drawn surface can still castShadow,
        // which can be used to make a simplified shadow hull for a complex object set as noShadow
        public bool IsDrawn => numStages > 0 || entityGui != 0 || gui != null;

        // returns true if the material will draw any non light interaction stages
        public bool HasAmbient => numAmbientStages > 0;

        // returns true if material has a gui
        public bool HasGui => entityGui != 0 || gui != null;

        // returns true if the material will generate another view, either as a mirror or dynamic rendered image
        public bool HasSubview => hasSubview;

        // returns true if the material will generate shadows, not making a distinction between global and no-self shadows
        public bool SurfaceCastsShadow => TestMaterialFlag(MF.FORCESHADOWS) || !TestMaterialFlag(MF.NOSHADOWS);

        // returns true if the material will generate interactions with fog/blend lights All non-translucent surfaces receive fog unless they are explicitly noFog
        public bool ReceivesFog => IsDrawn && !noFog && coverage != MC.TRANSLUCENT;

        // returns true if the material will generate interactions with normal lights Many special effect surfaces don't have any bump/diffuse/specular
        // stages, and don't interact with lights at all
        public bool ReceivesLighting => numAmbientStages != numStages;

        // returns true if the material should generate interactions on sides facing away from light centers, as with noshadow and noselfshadow options
        public bool ReceivesLightingOnBackSides => (materialFlags & (MF.NOSELFSHADOW | MF.NOSHADOWS)) != 0;

        // Standard two-sided triangle rendering won't work with bump map lighting, because the normal and tangent vectors won't be correct for the back sides.  When two
        // sided lighting is desired. typically for alpha tested surfaces, this is addressed by having CleanupModelSurfaces() create duplicates of all the triangles
        // with apropriate order reversal.
        public bool ShouldCreateBackSides => shouldCreateBackSides;

        // characters and models that are created by a complete renderbump can use a faster method of tangent and normal vector generation than surfaces which have a flat
        // renderbump wrapped over them.
        public bool UseUnsmoothedTangents => unsmoothedTangents;

        // by default, monsters can have blood overlays placed on them, but this can be overrided on a per-material basis with the "noOverlays" material command.
        // This will always return false for translucent surfaces
        public bool AllowOverlays => allowOverlays;

        // MC_OPAQUE, MC_PERFORATED, or MC_TRANSLUCENT, for interaction list linking and dmap flood filling
        // The depth buffer will not be filled for MC_TRANSLUCENT surfaces
        // FIXME: what do nodraw surfaces return?
        public MC Coverage => coverage;

        // returns true if this material takes precedence over other in coplanar cases
        public bool HasHigherDmapPriority(Material other) => (IsDrawn && !other.IsDrawn) || (Coverage < other.Coverage);

        // returns a idUserInterface if it has a global gui, or NULL if no gui
        public IUserInterface GlobalGui => gui;

        // a discrete surface will never be merged with other surfaces by dmap, which is necessary to prevent mutliple gui surfaces, mirrors, autosprites, and some other
        // special effects from being combined into a single surface guis, merging sprites or other effects, mirrors and remote views are always discrete
        public bool IsDiscrete => entityGui != 0 || gui != null || deform != DFRM.NONE || sort == (float)SS.SUBVIEW || (surfaceFlags & SURF.DISCRETE) != 0;

        // Normally, dmap chops each surface by every BSP boundary, then reoptimizes. For gigantic polygons like sky boxes, this can cause a huge number of planar
        // triangles that make the optimizer take forever to turn back into a single triangle.  The "noFragment" option causes dmap to only break the polygons at
        // area boundaries, instead of every BSP boundary.  This has the negative effect of not automatically fixing up interpenetrations, so when this is used, you
        // should manually make the edges of your sky box exactly meet, instead of poking into each other.
        public bool NoFragment => (surfaceFlags & SURF.NOFRAGMENT) != 0;

        //------------------------------------------------------------------
        // light shader specific functions, only called for light entities. lightshader option to fill with fog from viewer instead of light from center
        public bool IsFogLight => fogLight;

        // perform simple blending of the projection, instead of interacting with bumps and textures
        public bool IsBlendLight => blendLight;

        // an ambient light has non-directional bump mapping and no specular
        public bool IsAmbientLight => ambientLight;

        // implicitly no-shadows lights (ambients, fogs, etc) will never cast shadows but individual light entities can also override this value
        public bool LightCastsShadows => TestMaterialFlag(MF.FORCESHADOWS) || (!fogLight && !ambientLight && !blendLight && !TestMaterialFlag(MF.NOSHADOWS));

        // fog lights, blend lights, ambient lights, etc will all have to have interaction triangles generated for sides facing away from the light as well as those
        // facing towards the light.  It is debatable if noshadow lights should effect back sides, making everything "noSelfShadow", but that would make noshadow lights
        // potentially slower than normal lights, which detracts from their optimization ability, so they currently do not.
        public bool LightEffectsBackSides => fogLight || ambientLight || blendLight;

        // NULL unless an image is explicitly specified in the shader with "lightFalloffShader <image>"
        public Image LightFalloffImage => lightFalloffImage;

        //------------------------------------------------------------------

        // returns the renderbump command line for this shader, or an empty string if not present
        public string RenderBump => renderBump;

        // set specific material flag(s)
        public void SetMaterialFlag(MF flag) => materialFlags |= flag;

        // clear specific material flag(s)
        public void ClearMaterialFlag(MF flag) => materialFlags &= ~flag;

        // test for existance of specific material flag(s)
        public bool TestMaterialFlag(MF flag) => (materialFlags & flag) != 0;

        // get content flags
        public CONTENTS ContentFlags => contentFlags;

        // get surface flags
        public SURF SurfaceFlags => surfaceFlags;

        // gets name for surface type (stone, metal, flesh, etc.)
        public SURFTYPE SurfaceType => (SURFTYPE)(surfaceFlags & SURF.TYPE_MASK);

        // get material description
        public string Description => desc;

        // get sort order
        public float Sort
        {
            get => sort;
            // this is only used by the gui system to force sorting order on images referenced from tga's instead of materials.
            // this is done this way as there are 2000 tgas the guis use
            set => sort = value;
        }

        // DFRM_NONE, DFRM_SPRITE, etc
        public DFRM Deform => deform;

        // flare size, expansion size, etc
        public int GetDeformRegister(int index) => deformRegisters[index];

        // particle system to emit from surface and table for turbulent
        public Decl DeformDecl => deformDecl;

        // currently a surface can only have one unique texgen for all the stages
        public TG Texgen => throw new NotImplementedException();

        // wobble sky parms
        public int[] TexGenRegisters => texGenRegisters;

        // get cull type
        public CT CullType => cullType;

        public float EditorAlpha => editorAlpha;

        public int EntityGui => entityGui;

        public DecalInfo DecalInfo => this.decalInfo;

        // spectrums are used for "invisible writing" that can only be illuminated by a light of matching spectrum
        public int Spectrum => spectrum;

        public float PolygonOffset => polygonOffset;

        public float SurfaceArea => surfaceArea;

        public void AddToSurfaceArea(float area) => surfaceArea += area;

        //------------------------------------------------------------------

        // returns the length, in milliseconds, of the videoMap on this material, or zero if it doesn't have one
        public int CinematicLength => throw new NotImplementedException();

        public void CloseCinematic() => throw new NotImplementedException();

        public void ResetCinematicTime(int time) => throw new NotImplementedException();

        public void UpdateCinematic(int time) => throw new NotImplementedException();

        //------------------------------------------------------------------

        // gets an image for the editor to use
        public Image EditorImage => throw new NotImplementedException();
        public int ImageWidth => throw new NotImplementedException();
        public int ImageHeight => throw new NotImplementedException();

        public void SetGui(string _gui) => throw new NotImplementedException();

        // just for resource tracking
        public void SetImageClassifications(int tag) => throw new NotImplementedException();

        //------------------------------------------------------------------

        // returns number of registers this material contains
        public int NumRegisters => numRegisters;

        // regs should point to a float array large enough to hold GetNumRegisters() floats
        public void EvaluateRegisters(float[] regs, float[] entityParms, ViewDef view, ISoundEmitter soundEmitter = null) => throw new NotImplementedException();

        // if a material only uses constants (no entityParm or globalparm references), this will return a pointer to an internal table, and EvaluateRegisters will not need
        // to be called.  If NULL is returned, EvaluateRegisters must be used.
        public float[] ConstantRegisters() => throw new NotImplementedException();

        public bool SuppressInSubview() => suppressInSubview;
        public bool IsPortalSky => portalSky;
        public void AddReference() => throw new NotImplementedException();

        // parse the entire material
        void CommonInit() => throw new NotImplementedException();
        void ParseMaterial(Lexer src) => throw new NotImplementedException();
        bool MatchToken(Lexer src, string match) => throw new NotImplementedException();
        void ParseSort(Lexer src) => throw new NotImplementedException();
        void ParseBlend(Lexer src, ShaderStage stage) => throw new NotImplementedException();
        void ParseVertexParm(Lexer src, NewShaderStage newStage) => throw new NotImplementedException();
        void ParseFragmentMap(Lexer src, NewShaderStage newStage) => throw new NotImplementedException();
        void ParseStage(Lexer src, Image.TR trpDefault = Image.TR.REPEAT) => throw new NotImplementedException();
        void ParseDeform(Lexer src) => throw new NotImplementedException();
        void ParseDecalInfo(Lexer src) => throw new NotImplementedException();
        bool CheckSurfaceParm(Token token) => throw new NotImplementedException();
        int GetExpressionConstant(float f) => throw new NotImplementedException();
        int GetExpressionTemporary() => throw new NotImplementedException();
        ExpOp GetExpressionOp() => throw new NotImplementedException();
        int EmitOp(int a, int b, OP_TYPE opType) => throw new NotImplementedException();
        int ParseEmitOp(Lexer src, int a, OP_TYPE opType, int priority) => throw new NotImplementedException();
        int ParseTerm(Lexer src) => throw new NotImplementedException();
        int ParseExpressionPriority(Lexer src, int priority) => throw new NotImplementedException();
        int ParseExpression(Lexer src) => throw new NotImplementedException();
        void ClearStage(ShaderStage ss) => throw new NotImplementedException();
        int NameToSrcBlendMode(string name) => throw new NotImplementedException();
        int NameToDstBlendMode(string name) => throw new NotImplementedException();
        void MultiplyTextureMatrix(TextureStage ts, int[,] registers) => throw new NotImplementedException();   // FIXME: for some reason the const is bad for gcc and Mac
        void SortInteractionStages() => throw new NotImplementedException();
        void AddImplicitStages(Image.TR trpDefault = Image.TR.REPEAT) => throw new NotImplementedException();
        void CheckForConstantRegisters() => throw new NotImplementedException();

        string desc;             // description
        string renderBump;           // renderbump command options, without the "renderbump" at the start

        Image lightFalloffImage;

        int entityGui;          // draw a gui with the idUserInterface from the renderEntity_t
                                // non zero will draw gui, gui2, or gui3 from renderEnitty_t
        IUserInterface gui;           // non-custom guis are shared by all users of a material

        bool noFog;             // surface does not create fog interactions

        int spectrum;           // for invisible writing, used for both lights and surfaces

        float polygonOffset;

        CONTENTS contentFlags;       // content flags
        SURF surfaceFlags;       // surface flags
        MF materialFlags;      // material flags

        DecalInfo decalInfo;

        float sort;             // lower numbered shaders draw before higher numbered
        DFRM deform;
        int[] deformRegisters = new int[4];     // numeric parameter for deforms
        Decl deformDecl;           // for surface emitted particle deforms and tables

        int[] texGenRegisters = new int[MAX_TEXGEN_REGISTERS];  // for wobbleSky

        MC coverage;
        CT cullType;            // CT_FRONT_SIDED, CT_BACK_SIDED, or CT_TWO_SIDED
        bool shouldCreateBackSides;

        bool fogLight;
        bool blendLight;
        bool ambientLight;
        bool unsmoothedTangents;
        bool hasSubview;            // mirror, remote render, etc
        bool allowOverlays;

        int numOps;
        ExpOp ops;               // evaluate to make expressionRegisters

        int numRegisters;                                                                           //
        float[] expressionRegisters;

        float[] constantRegisters;   // NULL if ops ever reference globalParms or entityParms

        int numStages;
        int numAmbientStages;

        ShaderStage[] stages;

        object pd;            // only used during parsing

        float surfaceArea;      // only for listSurfaceAreas

        // we defer loading of the editor image until it is asked for, so the game doesn't load up
        // all the invisible and uncompressed images.
        // If editorImage is NULL, it will atempt to load editorImageName, and set editorImage to that or defaultImage
        string editorImageName;
        Image editorImage;        // image used for non-shaded preview
        float editorAlpha;

        bool suppressInSubview;
        bool portalSky;
        int refCount;
    }
}