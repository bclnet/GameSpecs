#define USE_TRI_DATA_ALLOCATOR
using System.Runtime.InteropServices;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    // ScreenRect gets carried around with each drawSurf, so it makes sense to keep it compact, instead of just using the idBounds class
    public class ScreenRect
    {
        public short x1, y1, x2, y2;                            // inclusive pixel bounds inside viewport
        public float zmin, zmax;                                // for depth bounds test

        // clear to backwards values
        public void Clear()
        {
            x1 = y1 = 32000;
            x2 = y2 = -32000;
            zmin = 0f;
            zmax = 1f;
        }

        // adds a point
        public void AddPoint(float x, float y)
        {
            var ix = (short)MathX.FtoiFast(x);
            var iy = (short)MathX.FtoiFast(y);

            if (ix < x1) x1 = ix;
            if (ix > x2) x2 = ix;
            if (iy < y1) y1 = iy;
            if (iy > y2) y2 = iy;
        }

        // expand by one pixel each way to fix roundoffs
        public void Expand()
        {
            x1--;
            y1--;
            x2++;
            y2++;
        }

        public void Intersect(ScreenRect rect)
        {
            if (rect.x1 > x1) x1 = rect.x1;
            if (rect.x2 < x2) x2 = rect.x2;
            if (rect.y1 > y1) y1 = rect.y1;
            if (rect.y2 < y2) y2 = rect.y2;
        }

        public void Union(ScreenRect rect)
        {
            if (rect.x1 < x1) x1 = rect.x1;
            if (rect.x2 > x2) x2 = rect.x2;
            if (rect.y1 < y1) y1 = rect.y1;
            if (rect.y2 > y2) y2 = rect.y2;
        }

        public bool Equals(ScreenRect rect)
            => x1 == rect.x1 && x2 == rect.x2 && y1 == rect.y1 && y2 == rect.y2;

        public bool IsEmpty
            => x1 > x2 || y1 > y2;
    }

    public enum DC
    {
        BAD,
        RENDERVIEW,
        UPDATE_ENTITYDEF,
        DELETE_ENTITYDEF,
        UPDATE_LIGHTDEF,
        DELETE_LIGHTDEF,
        LOADMAP,
        CROP_RENDER,
        UNCROP_RENDER,
        CAPTURE_RENDER,
        END_FRAME,
        DEFINE_MODEL,
        SET_PORTAL_STATE,
        UPDATE_SOUNDOCCLUSION,
        GUI_MODEL
    }

    #region SURFACES

    // drawSurf_t structures command the back end to render surfaces a given srfTriangles_t may be used with multiple viewEntity_t,
    // as when viewed in a subview or multiple viewport render, or with multiple shaders when skinned, or, possibly with multiple
    // lights, although currently each lighting interaction creates unique srfTriangles_t

    // drawSurf_t are always allocated and freed every frame, they are never cached
    public class DrawSurf
    {
        public const int DSF_VIEW_INSIDE_SHADOW = 1;
        public const int SHADOW_CAP_INFINITE = 64;

        public SrfTriangles geoFrontEnd;
        public ViewEntity space;
        public Material material; // may be NULL for shadow volumes
        public float sort;     // material->sort, modified by gui / entity sort offsets
        public float[] shaderRegisters;   // evaluated and adjusted for referenceShaders
        public DrawSurf nextOnLight;    // viewLight chains
        public ScreenRect scissorRect;   // for scissor clipping, local inside renderView viewport
        public int dsFlags;            // DSF_VIEW_INSIDE_SHADOW, etc
        public float[] wobbleTransform = new float[16];
        public int numIndexes;
        // data in vertex object space, not directly readable by the CPU
        public VertCache indexCache;                // int
        public VertCache ambientCache;          // idDrawVert
        public VertCache shadowCache;           // shadowCache_t

        public int numShadowIndexesNoFrontCaps;    // shadow volumes with front caps omitted
        public int numShadowIndexesNoCaps;         // shadow volumes with the front and rear caps omitted
        public int shadowCapPlaneBits;     // bits 0-5 are set when that plane of the interacting light has triangles
    }

    public class ShadowFrustum
    {
        public int numPlanes;      // this is always 6 for now
        public Plane[] planes = new Plane[6];
        // positive sides facing inward plane 5 is always the plane the projection is going to, the other planes are just clip planes all planes are in global coordinates
        public bool makeClippedPlanes;
        // a projected light with a single frustum needs to make sil planes from triangles that clip against side planes, but a point light that has adjacent frustums doesn't need to
    }

    // areas have references to hold all the lights and entities in them
    public class AreaReference : BlockAllocElement<AreaReference>
    {
        public AreaReference areaNext;              // chain in the area
        public AreaReference areaPrev;
        public AreaReference ownerNext;             // chain on either the entityDef or lightDef
        public IRenderEntity entity;                    // only one of entity / light will be non-NULL
        public IRenderLight light;                  // only one of entity / light will be non-NULL
        public PortalArea area;                 // so owners can find all the areas they are in
    }

    // IRenderLight should become the new public interface replacing the qhandle_t to light defs in the idRenderWorld interface
    public abstract class IRenderLight
    {
        public abstract void FreeRenderLight();
        public abstract void UpdateRenderLight(RenderLight re, bool forceUpdate = false);
        public abstract void GetRenderLight(RenderLight re);
        public abstract void ForceUpdate();
        public abstract int Index { get; }

        public RenderLight parms;                    // specification
        public bool lightHasMoved;         // the light has changed its position since it was first added, so the prelight model is not valid
        public float[] modelMatrix = new float[16];      // this is just a rearrangement of parms.axis and parms.origin
        public IRenderWorld world;
        public int index;                  // in world lightdefs
        public int areaNum;                // if not -1, we may be able to cull all the light's interactions if !viewDef->connectedAreas[areaNum]
        public int lastModifiedFrameNum;   // to determine if it is constantly changing, and should go in the dynamic frame memory, or kept in the cached memory
        public bool archived;              // for demo writing

        // derived information
        public Plane[] lightProject = new Plane[4];

        public Material lightShader;          // guaranteed to be valid, even if parms.shader isn't
        public Image falloffImage;

        public Vector3 globalLightOrigin;       // accounting for lightCenter and parallel

        public Plane[] frustum = new Plane[6];             // in global space, positive side facing out, last two are front/back
        public Winding[] frustumWindings = new Winding[6];      // used for culling
        public SrfTriangles frustumTris;            // triangulated frustumWindings[]

        public int numShadowFrustums;      // one for projected lights, usually six for point lights
        public ShadowFrustum[] shadowFrustums = new ShadowFrustum[6];

        public int viewCount;              // if == tr.viewCount, the light is on the viewDef->viewLights list
        public ViewLight viewLight;

        public AreaReference references;                // each area the light is present in will have a lightRef
        public IInteraction firstInteraction;        // doubly linked list
        public IInteraction lastInteraction;

        public DoublePortal foggedPortals;
    }

    // IRenderEntity should become the new public interface replacing the qhandle_t to entity defs in the idRenderWorld interface
    public abstract class IRenderEntity
    {
        public abstract void FreeRenderEntity();
        public abstract void UpdateRenderEntity(RenderEntity re, bool forceUpdate = false);
        public abstract void GetRenderEntity(RenderEntity re);
        public abstract void ForceUpdate();
        public abstract int Index { get; }
        // overlays are extra polygons that deform with animating models for blood and damage marks
        public abstract void ProjectOverlay(Plane[] localTextureAxis, Material material);
        public abstract void RemoveDecals();

        public RenderEntity parms;

        public float[] modelMatrix = new float[16];      // this is just a rearrangement of parms.axis and parms.origin

        public IRenderWorld world;
        public int index;                  // in world entityDefs

        public int lastModifiedFrameNum;   // to determine if it is constantly changing, and should go in the dynamic frame memory, or kept in the cached memory
        public bool archived;              // for demo writing

        public IRenderModel dynamicModel;            // if parms.model->IsDynamicModel(), this is the generated data
        public int dynamicModelFrameCount; // continuously animating dynamic models will recreate dynamicModel if this doesn't == tr.viewCount
        public IRenderModel cachedDynamicModel;

        public Bounds referenceBounds;       // the local bounds used to place entityRefs, either from parms or a model

        // a viewEntity_t is created whenever a idRenderEntityLocal is considered for inclusion in a given view, even if it turns out to not be visible
        public int viewCount;              // if tr.viewCount == viewCount, viewEntity is valid, but the entity may still be off screen
        public ViewEntity viewEntity;                // in frame temporary memory

        public int visibleCount;
        // if tr.viewCount == visibleCount, at least one ambient surface has actually been added by R_AddAmbientDrawsurfs
        // note that an entity could still be in the view frustum and not be visible due to portal passing

        public AreaReference entityRefs;                // chain of all references
        public IInteraction firstInteraction;        // doubly linked list
        public IInteraction lastInteraction;

        public bool needsPortalSky;
    }

    // viewLights are allocated on the frame temporary stack memory a viewLight contains everything that the back end needs out of an idRenderLightLocal,
    // which the front end may be modifying simultaniously if running in SMP mode. a viewLight may exist even without any surfaces, and may be relevent for fogging,
    // but should never exist if its volume does not intersect the view frustum
    public class ViewLight
    {
        public ViewLight next;

        // back end should NOT reference the lightDef, because it can change when running SMP
        public IRenderLight lightDef;

        // for scissor clipping, local inside renderView viewport scissorRect.Empty() is true if the viewEntity_t was never actually seen through any portals
        public ScreenRect scissorRect;

        // if the view isn't inside the light, we can use the non-reversed shadow drawing, avoiding the draws of the front and rear caps
        public bool viewInsideLight;

        // true if globalLightOrigin is inside the view frustum, even if it may be obscured by geometry.  This allows us to skip shadows from non-visible objects
        public bool viewSeesGlobalLightOrigin;

        // if !viewInsideLight, the corresponding bit for each of the shadowFrustum projection planes that the view is on the negative side of will be set,
        // allowing us to skip drawing the projected caps of shadows if we can't see the face
        public int viewSeesShadowPlaneBits;

        public Vector3 globalLightOrigin;           // global light origin used by backend
        public Plane[] lightProject = new Plane[4];            // light project used by backend
        public Plane fogPlane;                   // fog plane for backend fog volume rendering
        public SrfTriangles frustumTris;              // light frustum for backend fog volume rendering
        public Material lightShader;              // light shader used by backend
        public float[] shaderRegisters;           // shader registers used by backend
        public Image falloffImage;              // falloff image used by backend

        public DrawSurf globalShadows;             // shadow everything
        public DrawSurf localInteractions;         // don't get local shadows
        public DrawSurf localShadows;              // don't shadow local Surfaces
        public DrawSurf globalInteractions;        // get shadows from everything
        public DrawSurf translucentInteractions;   // get shadows from everything
    }

    public unsafe delegate void ForEyeDelegate(float* matrix);
    // a viewEntity is created whenever a idRenderEntityLocal is considered for inclusion in the current view, but it may still turn out to be culled.
    // viewEntity are allocated on the frame temporary stack memory a viewEntity contains everything that the back end needs out of a idRenderEntityLocal,
    // which the front end may be modifying simultaniously if running in SMP mode. A single entityDef can generate multiple viewEntity_t in a single frame, as when seen in a mirror
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ViewEntity_Union
    {
        public void eyeViewGet(int eye, ForEyeDelegate action)
        {
            switch (eye)
            {
                case 0: fixed (float* matrixF = eyeViewMatrix0) action(matrixF); return;
                case 1: fixed (float* matrixF = eyeViewMatrix1) action(matrixF); return;
                case 2: fixed (float* matrixF = eyeViewMatrix2) action(matrixF); return;
                default: throw new ArgumentOutOfRangeException(nameof(eye));
            }
        }
        // local coords to left/right/center eye coords
        [FieldOffset(0)] public fixed float eyeViewMatrix0[16];
        [FieldOffset(0)] public fixed float eyeViewMatrix1[16];
        [FieldOffset(0)] public fixed float eyeViewMatrix2[16];
        // Can also be treated as a float[48]
        [FieldOffset(0)] public fixed float viewMatrix[48];
    }

    public class ViewEntity
    {
        public ViewEntity next;

        // back end should NOT reference the entityDef, because it can change when running SMP
        public IRenderEntity entityDef;

        // for scissor clipping, local inside renderView viewport scissorRect.Empty() is true if the viewEntity_t was never actually
        // seen through any portals, but was created for shadow casting. a viewEntity can have a non-empty scissorRect, meaning that an area
        // that it is in is visible, and still not be visible.
        public ScreenRect scissorRect;

        public bool weaponDepthHack;
        public float modelDepthHack;

        public float[] modelMatrix = new float[16];      // local coords to global coords

        public ViewEntity_Union u;

        public void memset()
        {
            throw new NotImplementedException();
        }
    }

    // viewDefs are allocated on the frame temporary stack memory
    public class ViewDef
    {
        const int MAX_CLIP_PLANES = 1;              // we may expand this to six for some subview issues

        public ViewDef() { }
        public ViewDef(ViewDef s) { }

        // specified in the call to DrawScene()
        public RenderView renderView;

        public float[] projectionMatrix = new float[16];
        public ViewEntity worldSpace; // left, right and untransformed, Eye World space view entities

        public IRenderWorld renderWorld;

        public float floatTime;

        public Vector3 initialViewAreaOrigin;

        // Used to find the portalArea that view flooding will take place from. for a normal view, the initialViewOrigin will be renderView.viewOrg,
        // but a mirror may put the projection origin outside of any valid area, or in an unconnected area of the map, so the view
        // area must be based on a point just off the surface of the mirror / subview. It may be possible to get a failed portal pass if the plane of the
        // mirror intersects a portal, and the initialViewAreaOrigin is on a different side than the renderView.viewOrg is.
        public bool isSubview;             // true if this view is not the main view
        public bool isMirror;              // the portal is a mirror, invert the face culling
        public bool isXraySubview;

        public bool isEditor;

        public int numClipPlanes;          // mirrors will often use a single clip plane
        public Plane[] clipPlanes = new Plane[MAX_CLIP_PLANES];        // in world space, the positive side of the plane is the visible side
        public ScreenRect viewport;              // in real pixels and proper Y flip

        // for scissor clipping, local inside renderView viewport subviews may only be rendering part of the main view
        // these are real physical pixel values, possibly scaled and offset from the renderView x/y/width/height
        public ScreenRect scissor;

        public ViewDef superView;              // never go into an infinite subview loop
        public DrawSurf subviewSurface;

        // drawSurfs are the visible surfaces of the viewEntities, sorted by the material sort parameter
        public DrawSurf[] drawSurfs;            // we don't use an List for this, because it is allocated in frame temporary memory and may be resized
        public int numDrawSurfs;
        public int maxDrawSurfs;

        public ViewLight viewLights;           // chain of all viewLights effecting view
        public ViewEntity viewEntitys;         // chain of all viewEntities effecting view, including off screen ones casting shadows

        // we use viewEntities as a check to see if a given view consists solely of 2D rendering, which we can optimize in certain ways.  A 2D view will not have any viewEntities
        public Plane[] frustum = new Plane[5];             // positive sides face outward, [4] is the front clip plane
        public Frustum viewFrustum;

        public int areaNum;                // -1 = not in a valid area

        // An array in frame temporary memory that lists if an area can be reached without crossing a closed door.  This is used to avoid drawing interactions when the light is behind a closed door.
        public bool[] connectedAreas;
    }

    // complex light / surface interactions are broken up into multiple passes of a simple interaction shader
    public class DrawInteraction
    {
        public DrawSurf surf;

        public Image lightImage;
        public Image lightFalloffImage;
        public Image bumpImage;
        public Image diffuseImage;
        public Image specularImage;

        public Vector4 diffuseColor;    // may have a light color baked into it, will be < tr.backEndRendererMaxLight
        public Vector4 specularColor;   // may have a light color baked into it, will be < tr.backEndRendererMaxLight
        public SVC vertexColor; // applies to both diffuse and specular

        public int ambientLight;   // use tr.ambientNormalMap instead of normalization cube map (not a bool just to avoid an uninitialized memory check of the pad region by valgrind)

        // these are loaded into the vertex program
        public Vector4 localLightOrigin;
        public Vector4 localViewOrigin;
        public Matrix4x4 lightProjection; // S,T,R=Falloff,Q   // in local coordinates, possibly with a texture matrix baked in
        public Vector4[] bumpMatrix = new Vector4[2];
        public Vector4[] diffuseMatrix = new Vector4[2];
        public Vector4[] specularMatrix = new Vector4[2];
    }

    #endregion

    #region RENDERER BACK END COMMAND QUEUE

    // TR_CMDS

    public enum RC
    {
        NOP,
        DRAW_VIEW,
        SET_BUFFER,
        COPY_RENDER,
        SWAP_BUFFERS,        // can't just assume swap at end of list because
        DIRECT_BUFFER_START,
        DIRECT_BUFFER_END
        // of forced list submission before syncs
    }

    public class EmptyCommand
    {
        public RC commandId;
        public EmptyCommand next;
    }

    public class SetBufferCommand : EmptyCommand
    {
        public int buffer;
        public int frameCount;
    }

    public class DrawSurfsCommand : EmptyCommand
    {
        public ViewDef viewDef;

        public void Set(DrawSurfsCommand lockSurfacesCmd)
        {
            throw new NotImplementedException();
        }

        public void memset()
        {
            throw new NotImplementedException();
        }
    }

    public class CopyRenderCommand : EmptyCommand
    {
        public int x, y, imageWidth, imageHeight;
        public Image image;
        public int cubeFace;                   // when copying to a cubeMap
    }

    #endregion

    unsafe partial class R
    {
        // this is the inital allocation for max number of drawsurfs in a given view, but it will automatically grow if needed
        public const int INITIAL_DRAWSURFS = 0x4000;
    }

    // a request for frame memory will never fail (until malloc fails), but it may force the allocation of a new memory block that will be discontinuous with the existing memory
    public unsafe struct FrameMemoryBlock
    {
        public FrameMemoryBlock* next;
        public int size;
        public int used;
        public int poop;           // so that base is 16 byte aligned dynamically allocated as [size]
        public byte base_;
        public byte base1;
        public byte base2;
        public byte base3;
    }

    // all of the information needed by the back end must be contained in a frameData_t.  This entire structure is
    // duplicated so the front and back end can run in parallel on an SMP machine (OBSOLETE: this capability has been removed)
    public unsafe class FrameData
    {
        // one or more blocks of memory for all frame temporary allocations
        public FrameMemoryBlock* memory;

        // alloc will point somewhere into the memory chain
        public FrameMemoryBlock* alloc;

        public SrfTriangles firstDeferredFreeTriSurf;
        public SrfTriangles lastDeferredFreeTriSurf;

        public int memoryHighwater;    // max used on any frame

        // the currently building command list commands can be inserted at the front if needed, as for required dynamically generated textures
        public EmptyCommand cmdHead, cmdTail;     // may be of other command type based on commandId
    }

    public class PerformanceCounters
    {
        public int c_sphere_cull_in, c_sphere_cull_clip, c_sphere_cull_out;
        public int c_box_cull_in, c_box_cull_out;
        public int c_createInteractions;   // number of calls to idInteraction::CreateInteraction
        public int c_createLightTris;
        public int c_createShadowVolumes;
        public int c_generateMd5;
        public int c_entityDefCallbacks;
        public int c_alloc, c_free;    // counts for R_StaticAllc/R_StaticFree
        public int c_visibleViewEntities;
        public int c_shadowViewEntities;
        public int c_viewLights;
        public int c_numViews;         // number of total views rendered
        public int c_deformedSurfaces; // idMD5Mesh::GenerateSurface
        public int c_deformedVerts;    // idMD5Mesh::GenerateSurface
        public int c_deformedIndexes;  // idMD5Mesh::GenerateSurface
        public int c_tangentIndexes;   // R_DeriveTangents()
        public int c_entityUpdates, c_lightUpdates, c_entityReferences, c_lightReferences;
        public int c_guiSurfs;
        public int frontEndMsec;       // sum of time in all RE_RenderScene's in a frame

        public void memset()
        {
            throw new NotImplementedException();
        }
    }

    unsafe partial class R
    {
        public const int MAX_MULTITEXTURE_UNITS = 8;
    }

    public struct GLstate
    {
        public CT faceCulling;
        public int glStateBits;
        public bool forceGlState;      // the next GL_State will ignore glStateBits and set everything
        public int currentTexture;

        public ShaderProgram currentProgram;
    }

    public class BackEndCounters
    {
        public int c_surfaces;
        public int c_shaders;
        public int c_vertexes;
        public int c_indexes;      // one set per pass
        public int c_totalIndexes; // counting all passes

        public int c_drawElements;
        public int c_drawIndexes;
        public int c_drawVertexes;
        public int c_drawRefIndexes;
        public int c_drawRefVertexes;

        public int c_shadowElements;
        public int c_shadowIndexes;
        public int c_shadowVertexes;

        public int c_vboIndexes;

        public int msec;           // total msec for backend run

        internal void memset()
        {
            throw new NotImplementedException();
        }
    }

    // all state modified by the back end is separated from the front end state
    public class BackEndState
    {
        public int frameCount;     // used to track all images used in a frame
        public ViewDef viewDef;
        public BackEndCounters pc;

        // Current states, for optimizations
        public ViewEntity currentSpace;       // for detecting when a matrix must change
        public ScreenRect currentScissor; // for scissor clipping, local inside renderView viewport
        public bool currentRenderCopied;   // true if any material has already referenced _currentRender

        // our OpenGL state deltas
        public GLstate glState;

        public int c_copyFrameBuffer;

        public void memset()
        {
            throw new NotImplementedException();
        }
    }

    unsafe partial class R
    {
        const int MAX_GUI_SURFACES = 1024;      // default size of the drawSurfs list for guis, will be automatically expanded as needed
    }

    public class RenderCrop
    {
        public const int MAX_RENDER_CROPS = 8;

        public int x, y, width, height; // these are in physical, OpenGL Y-at-bottom pixels
    }

    public class ShaderProgram
    {
        public uint program;

        public uint vertexShader;
        public uint fragmentShader;

        public int glColor;
        public int alphaTest;
        public int specularExponent;

        public int modelMatrix;

        // New for multiview - The view and projection matrix uniforms
        public uint projectionMatrixBinding;
        public uint viewMatricesBinding;

        public int modelViewMatrix;
        public int textureMatrix;
        public int localLightOrigin;
        public int localViewOrigin;

        public int lightProjection;

        public int bumpMatrixS;
        public int bumpMatrixT;
        public int diffuseMatrixS;
        public int diffuseMatrixT;
        public int specularMatrixS;
        public int specularMatrixT;

        public int colorModulate;
        public int colorAdd;
        public int diffuseColor;
        public int specularColor;
        public int fogColor;

        public int fogMatrix;

        public int clipPlane;

        /* gl_... */
        public int attr_TexCoord;
        public int attr_Tangent;
        public int attr_Bitangent;
        public int attr_Normal;
        public int attr_Vertex;
        public int attr_Color;

        public int[] u_fragmentMap = new int[ShaderStage.MAX_FRAGMENT_IMAGES];
        public int[] u_fragmentCubeMap = new int[ShaderStage.MAX_FRAGMENT_IMAGES];

        public object memset()
        {
            throw new NotImplementedException();
        }
    }
}
