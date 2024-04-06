#define USE_TRI_DATA_ALLOCATOR
using System.Runtime.InteropServices;
// using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public class RenderLightLocal : IRenderLight
    {
        public RenderLightLocal() => throw new NotImplementedException();

        public override void FreeRenderLight() => throw new NotImplementedException();
        public override void UpdateRenderLight(RenderLight re, bool forceUpdate = false) => throw new NotImplementedException();
        public override void GetRenderLight(RenderLight re) => throw new NotImplementedException();
        public override void ForceUpdate() => throw new NotImplementedException();
        public override int Index => 0;
    }

    public class RenderEntityLocal : IRenderEntity
    {
        public RenderEntityLocal() => throw new NotImplementedException();

        public override void FreeRenderEntity() => throw new NotImplementedException();
        public override void UpdateRenderEntity(RenderEntity re, bool forceUpdate = false) => throw new NotImplementedException();
        public override void GetRenderEntity(RenderEntity re) => throw new NotImplementedException();
        public override void ForceUpdate() => throw new NotImplementedException();
        public override int Index => 0;

        // overlays are extra polygons that deform with animating models for blood and damage marks
        public override void ProjectOverlay(Plane[] localTextureAxis, Material material) => throw new NotImplementedException();
        public override void RemoveDecals() => throw new NotImplementedException();

        public RenderModelDecal decals;                 // chain of decals that have been projected on this model
        public RenderModelOverlay overlay;              // blood overlays on animated models
    }


    //unsafe partial class TR
    //{
    //    public static void R_LockSurfaceScene(ViewDef parms);
    //    public static void R_ClearCommandChain();
    //    public static void R_AddDrawViewCmd(ViewDef parms);

    //    //public static void R_ReloadGuis_f(CmdArgs args);
    //    //public static void R_ListGuis_f(CmdArgs args);

    //    public static void* R_GetCommandBuffer(int bytes);

    //    // this allows a global override of all materials
    //    public static bool R_GlobalShaderOverride(Material shader);

    //    // this does various checks before calling the idDeclSkin
    //    public static Material R_RemapShaderBySkin(Material shader, DeclSkin customSkin, Material customShader);
    //}


    //        #region MAIN

    //        #region PROJECTION_

    //        // This uses the "infinite far z" trick
    //        void R_SetupProjection_()
    //        {
    //            float xmin, xmax, ymin, ymax;
    //            float width, height;
    //            float zNear;
    //            float zFar;
    //            float jitterx, jittery;
    //            static RandomX random = new();

    //            // random jittering is usefull when multiple frames are going to be blended together for motion blurred anti-aliasing
    //            if (r_jitter.Bool) { jitterx = random.RandomFloat(); jittery = random.RandomFloat(); }
    //            else jitterx = jittery = 0;

    //            // set up projection matrix
    //#if Z_HACK
    //	zNear = 8;
    //#else
    //            zNear = r_znear.Float;
    //#endif

    //            if (tr.viewDef.renderView.cramZNear) zNear *= 0.25f;

    //            zFar = 4000;

    //            ymax = (float)(zNear * Math.Tan(tr.viewDef.renderView.fov_y * MathX.PI / 360f));
    //            ymin = -ymax;

    //            xmax = (float)(zNear * Math.Tan(tr.viewDef.renderView.fov_x * MathX.PI / 360f));
    //            xmin = -xmax;

    //            width = xmax - xmin;
    //            height = ymax - ymin;

    //            jitterx = jitterx * width / (tr.viewDef.viewport.x2 - tr.viewDef.viewport.x1 + 1);
    //            xmin += jitterx;
    //            xmax += jitterx;
    //            jittery = jittery * height / (tr.viewDef.viewport.y2 - tr.viewDef.viewport.y1 + 1);
    //            ymin += jittery;
    //            ymax += jittery;

    //            tr.viewDef.projectionMatrix[0] = 2 * zNear / width;
    //            tr.viewDef.projectionMatrix[4] = 0;
    //            tr.viewDef.projectionMatrix[8] = (xmax + xmin) / width;    // normally 0
    //            tr.viewDef.projectionMatrix[12] = 0;

    //            tr.viewDef.projectionMatrix[1] = 0;
    //            tr.viewDef.projectionMatrix[5] = 2 * zNear / height;
    //            tr.viewDef.projectionMatrix[9] = (ymax + ymin) / height;   // normally 0
    //            tr.viewDef.projectionMatrix[13] = 0;

    //            // this is the far-plane-at-infinity formulation, and crunches the Z range slightly so w=0 vertexes do not rasterize right at the wraparound point
    //            tr.viewDef.projectionMatrix[2] = 0;
    //            tr.viewDef.projectionMatrix[6] = 0;
    //#if Z_HACK
    //	tr.viewDef.projectionMatrix[10] = (-zFar-zNear)/(zFar-zNear);//-0.999f;
    //	tr.viewDef.projectionMatrix[14] = -2f*zFar*zNear/(zFar-zNear);
    //#else
    //            tr.viewDef.projectionMatrix[10] = -0.999f;
    //            tr.viewDef.projectionMatrix[14] = -2f * zNear;
    //#endif

    //            tr.viewDef.projectionMatrix[3] = 0;
    //            tr.viewDef.projectionMatrix[7] = 0;
    //            tr.viewDef.projectionMatrix[11] = -1;
    //            tr.viewDef.projectionMatrix[15] = 0;
    //        }

    //        // Setup that culling frustum planes for the current view
    //        // FIXME: derive from modelview matrix times projection matrix
    //        static void R_SetupViewFrustum_()
    //        {
    //            var ang = MathX.DEG2RAD(tr.viewDef.renderView.fov_x) * 0.5f;
    //            MathX.SinCos(ang, out var xs, out var xc);

    //            tr.viewDef.frustum[0] = xs * tr.viewDef.renderView.viewaxis[0] + xc * tr.viewDef.renderView.viewaxis[1];
    //            tr.viewDef.frustum[1] = xs * tr.viewDef.renderView.viewaxis[0] - xc * tr.viewDef.renderView.viewaxis[1];

    //            ang = MathX.DEG2RAD(tr.viewDef.renderView.fov_y) * 0.5f;
    //            MathX.SinCos(ang, out xs, out xc);

    //            tr.viewDef.frustum[2] = xs * tr.viewDef.renderView.viewaxis[0] + xc * tr.viewDef.renderView.viewaxis[2];
    //            tr.viewDef.frustum[3] = xs * tr.viewDef.renderView.viewaxis[0] - xc * tr.viewDef.renderView.viewaxis[2];

    //            // plane four is the front clipping plane
    //            tr.viewDef.frustum[4] = /* vec3_origin - */ tr.viewDef.renderView.viewaxis[0];

    //            for (var i = 0; i < 5; i++)
    //            {
    //                // flip direction so positive side faces out (FIXME: globally unify this)
    //                tr.viewDef.frustum[i] = -tr.viewDef.frustum[i].Normal;
    //                tr.viewDef.frustum[i].d = -(tr.viewDef.renderView.vieworg * tr.viewDef.frustum[i].Normal);
    //            }

    //            // eventually, plane five will be the rear clipping plane for fog
    //            float dNear, dFar, dLeft, dUp;

    //            dNear = r_znear.Float;
    //            if (tr.viewDef.renderView.cramZNear) dNear *= 0.25f;

    //            dFar = MAX_WORLD_SIZE;
    //            dLeft = (float)(dFar * Math.Tan(MathX.DEG2RAD(tr.viewDef.renderView.fov_x * 0.5f)));
    //            dUp = (float)(dFar * Math.Tan(MathX.DEG2RAD(tr.viewDef.renderView.fov_y * 0.5f)));
    //            tr.viewDef.viewFrustum.SetOrigin(tr.viewDef.renderView.vieworg);
    //            tr.viewDef.viewFrustum.SetAxis(tr.viewDef.renderView.viewaxis);
    //            tr.viewDef.viewFrustum.SetSize(dNear, dFar, dLeft, dUp);
    //        }

    //        static void R_ConstrainViewFrustum_()
    //        {
    //            Bounds bounds = default;

    //            // constrain the view frustum to the total bounds of all visible lights and visible entities
    //            bounds.Clear();
    //            for (ViewLight vLight = tr.viewDef.viewLights; vLight != null; vLight = vLight.next) bounds.AddBounds(vLight.lightDef.frustumTris.bounds);
    //            for (ViewEntity vEntity = tr.viewDef.viewEntitys; vEntity != null; vEntity = vEntity.next) bounds.AddBounds(vEntity.entityDef.referenceBounds);
    //            tr.viewDef.viewFrustum.ConstrainToBounds(bounds);

    //            if (r_useFrustumFarDistance.Float > 0f) tr.viewDef.viewFrustum.MoveFarDistance(r_useFrustumFarDistance.Float);
    //        }

    //        #endregion

    //        #region DRAWSURF SORTING

    //        static int R_QsortSurfaces_(DrawSurf a, DrawSurf b)
    //            => a.sort < b.sort ? -1 : a.sort > b.sort ? 1 : 0;

    //        static void R_SortDrawSurfs_()
    //        {
    //            // sort the drawsurfs by sort type, then orientation, then shader
    //            qsort(tr.viewDef.drawSurfs, tr.viewDef.numDrawSurfs, sizeof(tr.viewDef.drawSurfs[0]), R_QsortSurfaces_);
    //        }

    //        #endregion

    //        // A view may be either the actual camera view, a mirror / remote location, or a 3D view on a gui surface.
    //        // Parms will typically be allocated with R_FrameAlloc
    //        public static void R_RenderView(ViewDef parms)
    //        {
    //            ViewDef oldView;

    //            if (parms.renderView.width <= 0 || parms.renderView.height <= 0) return;

    //            tr.viewCount++;

    //            // save view in case we are a subview
    //            oldView = tr.viewDef;

    //            tr.viewDef = parms;

    //            tr.sortOffset = 0;

    //            // set the matrix for world space to eye space
    //            R_SetViewMatrix(tr.viewDef);

    //            // the four sides of the view frustum are needed for culling and portal visibility
    //            R_SetupViewFrustum_();

    //            // we need to set the projection matrix before doing portal-to-screen scissor box calculations
    //            R_SetupProjection_();

    //            // identify all the visible portalAreas, and the entityDefs and lightDefs that are in them and pass culling.
    //            ((RenderWorldLocal)parms.renderWorld).FindViewLightsAndEntities();

    //            // constrain the view frustum to the view lights and entities
    //            R_ConstrainViewFrustum_();

    //            // make sure that interactions exist for all light / entity combinations that are visible add any pre-generated light shadows, and calculate the light shader values
    //            R_AddLightSurfaces();

    //            // adds ambient surfaces and create any necessary interaction surfaces to add to the light lists
    //            R_AddModelSurfaces();

    //            // any viewLight that didn't have visible surfaces can have it's shadows removed
    //            R_RemoveUnecessaryViewLights();

    //            // sort all the ambient surfaces for translucency ordering
    //            R_SortDrawSurfs_();

    //            // generate any subviews (mirrors, cameras, etc) before adding this view
    //            if (R_GenerateSubViews())
    //                // if we are debugging subviews, allow the skipping of the main view draw
    //                if (R.r_subviewOnly.Bool) return;

    //            // write everything needed to the demo file
    //            if (session.writeDemo != null) ((RenderWorldLocal)parms.renderWorld).WriteVisibleDefs(tr.viewDef);

    //            // add the rendering commands for this viewDef
    //            R_AddDrawViewCmd(parms);

    //            // restore view in case we are a subview
    //            tr.viewDef = oldView;
    //        }




    //        #endregion

    //        #region LIGHT (TR_Light.cs)

    //        public static void R_ListRenderLightDefs_f(CmdArgs args);
    //        public static void R_ListRenderEntityDefs_f(CmdArgs args);

    //        //public static bool R_IssueEntityDefCallback(RenderEntityLocal def);
    //        //public static IRenderModel R_EntityDefDynamicModel(RenderEntityLocal def);

    //        //public static ViewEntity R_SetEntityDefViewEntity(RenderEntityLocal def);
    //        //public static ViewLight R_SetLightDefViewLight(RenderLightLocal def);

    //        //public static void R_AddDrawSurf(SrfTriangles tri, ViewEntity space, RenderEntity renderEntity, Material shader, ScreenRect scissor);

    //        //public static void R_LinkLightSurf(ref DrawSurf link, SrfTriangles tri, ViewEntity space, RenderLightLocal light, Material shader, ScreenRect scissor, bool viewInsideShadow);

    //        //public static bool R_CreateAmbientCache(SrfTriangles tri, bool needsLighting);
    //        //public static bool R_CreateIndexCache(SrfTriangles tri);
    //        //public static bool R_CreatePrivateShadowCache(SrfTriangles tri);
    //        //public static bool R_CreateVertexProgramShadowCache(SrfTriangles tri);

    //        #endregion

    //        #region LIGHTRUN (TR_LightRun.cs)

    //        //public static void R_RegenerateWorld_f(CmdArgs args);

    //        //public static void R_ModulateLights_f(CmdArgs args);

    //        //public static void R_SetLightProject(Plane[] lightProject, in Vector3 origin, in Vector3 targetPoint, in Vector3 rightVector, in Vector3 upVector, in Vector3 start, in Vector3 stop);

    //        //public static void R_AddLightSurfaces();
    //        //public static void R_AddModelSurfaces();
    //        //public static void R_RemoveUnecessaryViewLights();

    //        //public static void R_FreeDerivedData();
    //        //public static void R_ReCreateWorldReferences();

    //        //public static void R_CreateEntityRefs(RenderEntityLocal def);
    //        //public static void R_CreateLightRefs(RenderLightLocal light);

    //        //public static void R_DeriveLightData(RenderLightLocal light);
    //        //public static void R_FreeLightDefDerivedData(RenderLightLocal light);
    //        //public static void R_CheckForEntityDefsUsingModel(IRenderModel model);

    //        //public static void R_ClearEntityDefDynamicModel(RenderEntityLocal def);
    //        //public static void R_FreeEntityDefDerivedData(RenderEntityLocal def, bool keepDecals, bool keepCachedDynamicModel);
    //        public static void R_FreeEntityDefCachedDynamicModel(RenderEntityLocal def) => throw new NotImplementedException();
    //        //public static void R_FreeEntityDefDecals(RenderEntityLocal def);
    //        //public static void R_FreeEntityDefOverlay(RenderEntityLocal def);
    //        //public static void R_FreeEntityDefFadedDecals(RenderEntityLocal def, int time);

    //        //public static void R_CreateLightDefFogPortals(RenderLightLocal ldef);

    //        // Framebuffer stuff
    //        public static void R_InitFrameBuffer();
    //        public static void R_FrameBufferStart();
    //        public static void R_FrameBufferEnd();

    //        #endregion

    //        #region POLYTOPE (TR_Polytop.cs)

    //        //public static SrfTriangles R_PolytopeSurface(int numPlanes, Plane[] planes, Winding[] windings);

    //        #endregion

    //        #region RENDER BACKEND (TR_Render.cs)
    //        // NB: Not touching to GLSL shader stuff. This is using classic OGL calls only.

    //        //public static void RB_DrawView(DrawSurfsCommand data);
    //        public static void RB_RenderView();

    //        //public static void RB_DrawElementsWithCounters(DrawSurf surf);
    //        //public static void RB_DrawShadowElementsWithCounters(DrawSurf surf, int numIndexes);
    //        //public static void RB_SubmitInteraction(DrawInteraction din, Action<DrawInteraction> drawInteraction);
    //        //public static void RB_SetDrawInteraction(ShaderStage surfaceStage, float* surfaceRegs, ref Image image, Vector4[] matrix, float[] color);
    //        //public static void RB_BindVariableStageImage(TextureStage texture, float* shaderRegisters);
    //        //public static void RB_BeginDrawingView();
    //        //public static void RB_GetShaderTextureMatrix(float* shaderRegisters, TextureStage texture, float[] matrix);
    //        public static void RB_BakeTextureMatrixIntoTexgen(Matrix4x4 lightProject, float[] textureMatrix);

    //        #endregion

    //        void R_ReloadGLSLPrograms_f(CmdArgs args);

    //        void RB_GLSL_PrepareShaders();
    //        void RB_GLSL_FillDepthBuffer(DrawSurf[] drawSurfs, int numDrawSurfs);
    //        void RB_GLSL_DrawInteractions();
    //        int RB_GLSL_DrawShaderPasses(DrawSurf[] drawSurfs, int numDrawSurfs);
    //        void RB_GLSL_FogAllLights();

    //        // TR_STENCILSHADOWS
    //        // "facing" should have one more element than tri->numIndexes / 3, which should be set to 1
    //        void R_MakeShadowFrustums(RenderLightLocal def);

    //        public enum ShadowGen
    //        {
    //            SG_DYNAMIC,     // use infinite projections
    //            SG_STATIC,      // clip to bounds
    //        }
    //        SrfTriangles R_CreateShadowVolume(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, ShadowGen optimize, SrfCullInfo cullInfo);

    //        // TR_TURBOSHADOW
    //        // Fast, non-clipped overshoot shadow volumes
    //        // "facing" should have one more element than tri->numIndexes / 3, which should be set to 1 calling this function may modify "facing" based on culling
    //        //public static SrfTriangles R_CreateVertexProgramTurboShadowVolume(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, SrfCullInfo cullInfo);
    //        ////public static SrfTriangles R_CreateTurboShadowVolume(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, SrfCullInfo cullInfo);

    //        #region TRISURF (TR_TriSurf.cs)

    //        //public static void R_InitTriSurfData();
    //        //public static void R_ShutdownTriSurfData();
    //        //public static void R_PurgeTriSurfData(FrameData frame);
    //        //public static void R_ShowTriSurfMemory_f(CmdArgs args);

    //        //public static SrfTriangles R_AllocStaticTriSurf();
    //        //public static SrfTriangles R_CopyStaticTriSurf(SrfTriangles tri);
    //        //public static void R_AllocStaticTriSurfVerts(SrfTriangles tri, int numVerts);
    //        //public static void R_AllocStaticTriSurfIndexes(SrfTriangles tri, int numIndexes);
    //        //public static void R_AllocStaticTriSurfShadowVerts(SrfTriangles tri, int numVerts);
    //        //public static void R_AllocStaticTriSurfPlanes(SrfTriangles tri, int numIndexes);
    //        //public static void R_ResizeStaticTriSurfVerts(SrfTriangles tri, int numVerts);
    //        //public static void R_ResizeStaticTriSurfIndexes(SrfTriangles tri, int numIndexes);
    //        //public static void R_ResizeStaticTriSurfShadowVerts(SrfTriangles tri, int numVerts);
    //        //public static void R_ReferenceStaticTriSurfVerts(SrfTriangles tri, SrfTriangles reference);
    //        //public static void R_ReferenceStaticTriSurfIndexes(SrfTriangles tri, SrfTriangles reference);
    //        //public static void R_FreeStaticTriSurfSilIndexes(SrfTriangles tri);
    //        //public static void R_FreeStaticTriSurf(SrfTriangles tri);
    //        //public static void R_FreeStaticTriSurfVertexCaches(SrfTriangles tri);
    //        //public static void R_ReallyFreeStaticTriSurf(SrfTriangles tri);
    //        //public static void R_FreeDeferredTriSurfs(FrameData frame);
    //        //public static int R_TriSurfMemory(SrfTriangles tri);

    //        //public static void R_BoundTriSurf(SrfTriangles tri);
    //        //public static void R_RemoveDuplicatedTriangles(SrfTriangles tri);
    //        //public static void R_CreateSilIndexes(SrfTriangles tri);
    //        //public static void R_RemoveDegenerateTriangles(SrfTriangles tri);
    //        //public static void R_RemoveUnusedVerts(SrfTriangles tri);
    //        //public static void R_RangeCheckIndexes(SrfTriangles tri);
    //        //public static void R_CreateVertexNormals(SrfTriangles tri);    // also called by dmap
    //        //public static void R_DeriveFacePlanes(SrfTriangles tri);       // also called by renderbump
    //        //public static void R_CleanupTriangles(SrfTriangles tri, bool createNormals, bool identifySilEdges, bool useUnsmoothedTangents);
    //        //public static void R_ReverseTriangles(SrfTriangles tri);

    //        // Only deals with vertexes and indexes, not silhouettes, planes, etc. Does NOT perform a cleanup triangles, so there may be duplicated verts in the result.
    //        //public static SrfTriangles R_MergeSurfaceList(SrfTriangles[] surfaces, int numSurfaces);
    //        //public static SrfTriangles R_MergeTriangles(SrfTriangles tri1, SrfTriangles tri2);

    //        // if the deformed verts have significant enough texture coordinate changes to reverse the texture polarity of a triangle, the tangents will be incorrect
    //        //public static void R_DeriveTangents(SrfTriangles tri, bool allocFacePlanes = true);

    //        // deformable meshes precalculate as much as possible from a base frame, then generate complete srfTriangles_t from just a new set of vertexes
    //        public class DeformInfo
    //        {
    //            public int numSourceVerts;

    //            // numOutputVerts may be smaller if the input had duplicated or degenerate triangles it will often be larger if the input had mirrored texture seams that needed to be busted for proper tangent spaces
    //            public int numOutputVerts;
    //            public DrawVert[] verts;

    //            public int numMirroredVerts;
    //            public int[] mirroredVerts;

    //            public int numIndexes;
    //            public GlIndex[] indexes;

    //            public GlIndex[] silIndexes;

    //            public int numDupVerts;
    //            public int[] dupVerts;

    //            public int numSilEdges;
    //            public SilEdge[] silEdges;

    //            public DominantTri[] dominantTris;
    //        }

    //        //public static DeformInfo R_BuildDeformInfo(int numVerts, DrawVert[] verts, int numIndexes, int[] indexes, bool useUnsmoothedTangents);
    //        //public static void R_FreeDeformInfo(DeformInfo deformInfo);
    //        //public static int R_DeformInfoMemoryUsed(DeformInfo deformInfo);

    //        #endregion

    //        #region SUBVIEW (TR_SubView.cs)

    //        //public static bool R_PreciseCullSurface(DrawSurf drawSurf, Bounds ndcBounds);
    //        //public static bool R_GenerateSubViews();

    //        #endregion

    //     
    //        public static void R_DirectFrameBufferStart();

    //        public static void R_DirectFrameBufferEnd();


    //        #region TR_BACKEND (TR_Backend.cs)

    //        //public static void RB_SetDefaultGLState();
    //        //public static void RB_ExecuteBackEndCommands(EmptyCommand cmds);

    //        #endregion

    //        #region TR_GUISURF (TR_GuiSurf.cs)

    //        //public static void R_SurfaceToTextureAxis(SrfTriangles tri, Vector3 origin, Vector3[] axis);
    //        //public static void R_RenderGuiSurf(IUserInterface gui, DrawSurf drawSurf);

    //        #endregion

    //        #region TR_ORDERINDEXES (TR_OrderIndexes.cs)

    //        //public static void R_OrderIndexes(int numIndexes, GlIndex[] indexes);

    //        #endregion

    //        #region TR_DEFORM (TR_Deform.cs)

    //        //public static void R_DeformDrawSurf(DrawSurf drawSurf);

    //        #endregion

    //        #region TR_TRACE (TR_Trace.cs)

    //        //public struct LocalTrace
    //        //{
    //        //    public float fraction;
    //        //    // only valid if fraction < 1.0
    //        //    public Vector3 point;
    //        //    public Vector3 normal;
    //        //    public int[] indexes;
    //        //}

    //        //public static LocalTrace R_LocalTrace(Vector3 start, Vector3 end, float radius, SrfTriangles tri);

    //        #endregion

    //        #region TR_SHADOWBOUNDS (TR_ShadowBounds.cs)

    //        //public static ScreenRect R_CalcIntersectionScissor(RenderLightLocal lightDef, RenderEntityLocal entityDef, ViewDef viewDef);

    //        #endregion
}
