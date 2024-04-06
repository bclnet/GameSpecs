using System.Diagnostics;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.Gngine.Render.TR;
using static System.NumericsX.OpenStack.OpenStack;
using Qhandle = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        static void R_ListRenderLightDefs_f(CmdArgs args)
        {
            int i; IRenderLight ldef;

            if (tr.primaryWorld == null) return;

            int active = 0, totalRef = 0, totalIntr = 0;
            for (i = 0; i < tr.primaryWorld.lightDefs.Count; i++)
            {
                ldef = tr.primaryWorld.lightDefs[i];
                if (ldef == null) { common.Printf($"{i,4}: FREED\n"); continue; }

                // count up the interactions
                var iCount = 0;
                for (var inter = ldef.firstInteraction; inter != null; inter = inter.lightNext) iCount++;
                totalIntr += iCount;

                // count up the references
                var rCount = 0;
                for (var ref_ = ldef.references; ref_ != null; ref_ = ref_.ownerNext) rCount++;
                totalRef += rCount;

                common.Printf($"{i,4}: {iCount,3} intr {rCount,2} refs {ldef.lightShader.Name}\n");
                active++;
            }

            common.Printf($"{active} lightDefs, {totalIntr} interactions, {totalRef} areaRefs\n");
        }

        static void R_ListRenderEntityDefs_f(CmdArgs args)
        {
            int i; IRenderEntity mdef;

            if (tr.primaryWorld == null) return;

            int active = 0, totalRef = 0, totalIntr = 0;

            for (i = 0; i < tr.primaryWorld.entityDefs.Count; i++)
            {
                mdef = tr.primaryWorld.entityDefs[i];
                if (mdef == null) { common.Printf($"{i,4}: FREED\n"); continue; }

                // count up the interactions
                var iCount = 0;
                for (var inter = mdef.firstInteraction; inter != null; inter = inter.entityNext) iCount++;
                totalIntr += iCount;

                // count up the references
                var rCount = 0;
                for (var ref_ = mdef.entityRefs; ref_ != null; ref_ = ref_.ownerNext) rCount++;
                totalRef += rCount;

                common.Printf($"{i,4}: {iCount,3} intr {rCount,2} refs {mdef.parms.hModel.Name}\n");
                active++;
            }

            common.Printf("total active: %i\n", active);
        }
    }
    public unsafe partial class RenderWorldLocal : IRenderWorld
    {
        public RenderWorldLocal()
        {
            mapName = string.Empty;
            mapTimeStamp = default;
            generateAllInteractionsCalled = false;
            areaNodes = null;
            numAreaNodes = 0;
            portalAreas = null;
            numPortalAreas = 0;
            doublePortals = null;
            numInterAreaPortals = 0;
            interactionTable = null;
            interactionTableWidth = 0;
            interactionTableHeight = 0;
        }

        public void Dispose()
        {
            // free all the entityDefs, lightDefs, portals, etc
            FreeWorld();

            // free up the debug lines, polys, and text
            //RB_ClearDebugPolygons(0);
            //RB_ClearDebugLines(0);
            //RB_ClearDebugText(0);
        }

        public void ResizeInteractionTable()
        {
            // we overflowed the interaction table, so dump it. we may want to resize this in the future if it turns out to be common
            common.Printf("RenderWorldLocal::ResizeInteractionTable: overflowed interactionTableWidth, dumping\n");
            interactionTable = null;
        }

        public override Qhandle AddEntityDef(RenderEntity re)
        {
            // try and reuse a free spotd
            var entityHandle = entityDefs.FindIndex(x => x == null);
            if (entityHandle == -1)
            {
                entityHandle = entityDefs.Add_(null);
                if (interactionTable != null && entityDefs.Count > interactionTableWidth) ResizeInteractionTable();
            }

            UpdateEntityDef(entityHandle, re);

            return entityHandle;
        }

        // Does not write to the demo file, which will only be updated for visible entities
        int c_callbackUpdate;

        public override void UpdateEntityDef(Qhandle entityHandle, RenderEntity re)
        {
            if (r_skipUpdates.Bool) return;

            tr.pc.c_entityUpdates++;

            if (re.hModel == null && re.callback == null) common.Error("RenderWorld::UpdateEntityDef: NULL hModel");

            // create new slots if needed
            if (entityHandle < 0 || entityHandle > LUDICROUS_INDEX) common.Error($"RenderWorld::UpdateEntityDef: index = {entityHandle}");
            while (entityHandle >= entityDefs.Count) entityDefs.Add(null);

            var def = entityDefs[entityHandle];
            if (def != null)
            {
                if (re.forceUpdate == 0)
                {
                    // check for exact match (OPTIMIZE: check through pointers more)
                    if (re.joints == null && re.callbackData == null && def.dynamicModel == null && re.memcmp(def.parms) == 0) return;

                    // if the only thing that changed was shaderparms, we can just leave things as they are after updating parms

                    // if we have a callback function and the bounds, origin, axis and model match, then we can leave the references as they are
                    if (re.callback != null)
                    {
                        var axisMatch = re.axis == def.parms.axis;
                        var originMatch = re.origin == def.parms.origin;
                        var boundsMatch = re.bounds == def.referenceBounds;
                        var modelMatch = re.hModel == def.parms.hModel;

                        if (boundsMatch && originMatch && axisMatch && modelMatch)
                        {
                            // only clear the dynamic model and interaction surfaces if they exist
                            c_callbackUpdate++;
                            R_ClearEntityDefDynamicModel(def);
                            def.parms = new RenderEntity(re);
                            return;
                        }
                    }
                }

                // save any decals if the model is the same, allowing marks to move with entities
                if (def.parms.hModel == re.hModel) R_FreeEntityDefDerivedData(def, true, true);
                else R_FreeEntityDefDerivedData(def, false, false);
            }
            else
            {
                // creating a new one
                def = new RenderEntityLocal();
                entityDefs[entityHandle] = def;
                def.world = this;
                def.index = entityHandle;
            }

            def.parms = new RenderEntity(re);

            R_AxisToModelMatrix(def.parms.axis, def.parms.origin, def.modelMatrix);

            def.lastModifiedFrameNum = tr.frameCount;
            if (session.writeDemo != null && def.archived) { WriteFreeEntity(entityHandle); def.archived = false; }

            // optionally immediately issue any callbacks
            if (!r_useEntityCallbacks.Bool && def.parms.callback != null) R_IssueEntityDefCallback(def);

            // based on the model bounds, add references in each area that may contain the updated surface
            R_CreateEntityRefs(def);
        }

        // Frees all references and lit surfaces from the model, and NULL's out it's entry in the world list
        public override void FreeEntityDef(Qhandle entityHandle)
        {
            IRenderEntity def;
            if (entityHandle < 0 || entityHandle >= entityDefs.Count) { common.Printf("RenderWorld::FreeEntityDef: handle {entityHandle} > {entityDefs.Count}\n"); return; }

            def = entityDefs[entityHandle];
            if (def == null) { common.Printf($"RenderWorld::FreeEntityDef: handle {entityHandle} is NULL\n"); return; }

            R_FreeEntityDefDerivedData(def, false, false);

            if (session.writeDemo != null && def.archived) WriteFreeEntity(entityHandle);

            // if we are playing a demo, these will have been freed in R_FreeEntityDefDerivedData(), otherwise the gui object still exists in the game
            def.parms.gui[0] = null;
            def.parms.gui[1] = null;
            def.parms.gui[2] = null;

            entityDefs[entityHandle] = null;
        }

        public override RenderEntity GetRenderEntity(Qhandle entityHandle)
        {
            IRenderEntity def;

            if (entityHandle < 0 || entityHandle >= entityDefs.Count) { common.Printf($"RenderWorld::GetRenderEntity: invalid handle {entityHandle} [0, {entityDefs.Count}]\n"); return null; }

            def = entityDefs[entityHandle];
            if (def == null) { common.Printf($"RenderWorld::GetRenderEntity: handle {entityHandle} is NULL\n"); return null; }

            return def.parms;
        }

        public override Qhandle AddLightDef(RenderLight rlight)
        {
            // try and reuse a free spot
            var lightHandle = lightDefs.FindIndex(x => x == null);

            if (lightHandle == -1)
            {
                lightHandle = lightDefs.Add_(null);
                if (interactionTable != null && lightDefs.Count > interactionTableHeight) ResizeInteractionTable();
            }
            UpdateLightDef(lightHandle, rlight);

            return lightHandle;
        }

        // The generation of all the derived interaction data will usually be deferred until it is visible in a scene
        // Does not write to the demo file, which will only be done for visible lights
        public override void UpdateLightDef(Qhandle lightHandle, RenderLight rlight)
        {
            if (r_skipUpdates.Bool) return;

            tr.pc.c_lightUpdates++;

            // create new slots if needed
            if (lightHandle < 0 || lightHandle > LUDICROUS_INDEX) common.Error($"RenderWorld::UpdateLightDef: index = {lightHandle}");
            while (lightHandle >= lightDefs.Count) lightDefs.Add(null);

            var justUpdate = false;
            var light = lightDefs[lightHandle];
            if (light != null)
            {
                // if the shape of the light stays the same, we don't need to dump any of our derived data, because shader parms are calculated every frame
                if (rlight.axis == light.parms.axis && rlight.end == light.parms.end &&
                        rlight.lightCenter == light.parms.lightCenter && rlight.lightRadius == light.parms.lightRadius &&
                        rlight.noShadows == light.parms.noShadows && rlight.origin == light.parms.origin &&
                        rlight.parallel == light.parms.parallel && rlight.pointLight == light.parms.pointLight &&
                        rlight.right == light.parms.right && rlight.start == light.parms.start &&
                        rlight.target == light.parms.target && rlight.up == light.parms.up &&
                        rlight.shader == light.lightShader && rlight.prelightModel == light.parms.prelightModel)
                {
                    justUpdate = true;
                }
                else
                {
                    // if we are updating shadows, the prelight model is no longer valid
                    light.lightHasMoved = true;
                    R_FreeLightDefDerivedData(light);
                }
            }
            else
            {
                // create a new one
                light = new RenderLightLocal();
                lightDefs[lightHandle] = light;

                light.world = this;
                light.index = lightHandle;
            }

            light.parms = new RenderLight(rlight);
            light.lastModifiedFrameNum = tr.frameCount;
            if (session.writeDemo != null && light.archived) { WriteFreeLight(lightHandle); light.archived = false; }

            if (light.lightHasMoved) light.parms.prelightModel = null;

            if (!justUpdate)
            {
                R_DeriveLightData(light);
                R_CreateLightRefs(light);
                R_CreateLightDefFogPortals(light);
            }
        }

        // Frees all references and lit surfaces from the light, and NULL's out it's entry in the world list
        public override void FreeLightDef(Qhandle lightHandle)
        {
            IRenderLight light;

            if (lightHandle < 0 || lightHandle >= lightDefs.Count) { common.Printf($"RenderWorld::FreeLightDef: invalid handle {lightHandle} [0, {lightDefs.Count}]\n"); return; }

            light = lightDefs[lightHandle];
            if (light == null) { common.Printf($"RenderWorld::FreeLightDef: handle {lightHandle} is NULL\n"); return; }

            R_FreeLightDefDerivedData(light);

            if (session.writeDemo != null && light.archived) WriteFreeLight(lightHandle);

            lightDefs[lightHandle] = null;
        }

        public override RenderLight GetRenderLight(Qhandle lightHandle)
        {
            IRenderLight def;

            if (lightHandle < 0 || lightHandle >= lightDefs.Count) { common.Printf($"RenderWorld::GetRenderLight: handle {lightHandle} > {lightDefs.Count}\n"); return null; }

            def = lightDefs[lightHandle];
            if (def == null) { common.Printf($"RenderWorld::GetRenderLight: handle {lightHandle} is NULL\n"); return null; }

            return def.parms;
        }

        public override void ProjectDecalOntoWorld(FixedWinding winding, in Vector3 projectionOrigin, bool parallel, float fadeDepth, Material material, int startTime)
        {
            int i, numAreas;
            AreaReference ref_;
            PortalArea area;
            IRenderModel model;
            RenderEntityLocal def;
            DecalProjectionInfo info, localInfo;
            var areas = new int[10];

            if (!RenderModelDecal.CreateProjectionInfo(out info, winding, projectionOrigin, parallel, fadeDepth, material, startTime)) return;

            // get the world areas touched by the projection volume
            numAreas = BoundsInAreas(info.projectionBounds, areas, 10);

            // check all areas for models
            for (i = 0; i < numAreas; i++)
            {
                area = portalAreas[areas[i]];

                // check all models in this area
                for (ref_ = area.entityRefs.areaNext; ref_ != area.entityRefs; ref_ = ref_.areaNext)
                {
                    def = (RenderEntityLocal)ref_.entity;

                    // completely ignore any dynamic or callback models
                    model = def.parms.hModel;
                    if (model == null || model.IsDynamicModel != DynamicModel.DM_STATIC || def.parms.callback != null) continue;

                    if (def.parms.customShader != null && !def.parms.customShader.AllowOverlays) continue;

                    Bounds bounds = default;
                    bounds.FromTransformedBounds(model.Bounds(def.parms), def.parms.origin, def.parms.axis);

                    // if the model bounds do not overlap with the projection bounds
                    if (!info.projectionBounds.IntersectsBounds(bounds)) continue;

                    // transform the bounding planes, fade planes and texture axis into local space
                    RenderModelDecal.GlobalProjectionInfoToLocal(out localInfo, info, def.parms.origin, def.parms.axis);
                    localInfo.force = def.parms.customShader != null;

                    if (def.decals == null) def.decals = RenderModelDecal.Alloc();
                    def.decals.CreateDecal(model, localInfo);
                }
            }
        }

        public override void ProjectDecal(Qhandle entityHandle, FixedWinding winding, in Vector3 projectionOrigin, bool parallel, float fadeDepth, Material material, int startTime)
        {
            DecalProjectionInfo info, localInfo;

            if (entityHandle < 0 || entityHandle >= entityDefs.Count) { common.Error($"RenderWorld::ProjectOverlay: index = {entityHandle}"); return; }

            var def = (RenderEntityLocal)entityDefs[entityHandle];
            if (def == null) return;

            var model = def.parms.hModel;
            if (model == null || model.IsDynamicModel != DynamicModel.DM_STATIC || def.parms.callback != null) return;

            if (!RenderModelDecal.CreateProjectionInfo(out info, winding, projectionOrigin, parallel, fadeDepth, material, startTime)) return;

            Bounds bounds = default;
            bounds.FromTransformedBounds(model.Bounds(def.parms), def.parms.origin, def.parms.axis);

            // if the model bounds do not overlap with the projection bounds
            if (!info.projectionBounds.IntersectsBounds(bounds)) return;

            // transform the bounding planes, fade planes and texture axis into local space
            RenderModelDecal.GlobalProjectionInfoToLocal(out localInfo, info, def.parms.origin, def.parms.axis);
            localInfo.force = (def.parms.customShader != null);

            if (def.decals == null) def.decals = RenderModelDecal.Alloc();
            def.decals.CreateDecal(model, localInfo);
        }

        public override void ProjectOverlay(Qhandle entityHandle, Plane[] localTextureAxis, Material material)
        {
            if (entityHandle < 0 || entityHandle >= entityDefs.Count) { common.Error($"RenderWorld::ProjectOverlay: index = {entityHandle}"); return; }

            var def = (RenderEntityLocal)entityDefs[entityHandle];
            if (def == null) return;

            var refEnt = def.parms;

            var model = refEnt.hModel;
            if (model.IsDynamicModel != DynamicModel.DM_CACHED) return; // FIXME: probably should be MD5 only
            model = R_EntityDefDynamicModel(def);

            if (def.overlay == null) def.overlay = RenderModelOverlay.Alloc();
            def.overlay.CreateOverlay(model, localTextureAxis, material);
        }

        public override void RemoveDecals(Qhandle entityHandle)
        {
            if (entityHandle < 0 || entityHandle >= entityDefs.Count) { common.Error($"RenderWorld::ProjectOverlay: index = {entityHandle}"); return; }

            var def = entityDefs[entityHandle];
            if (def == null) return;

            R_FreeEntityDefDecals(def);
            R_FreeEntityDefOverlay(def);
        }

        // Sets the current view so any calls to the render world will use the correct parms.
        public override void SetRenderView(RenderView renderView)
            => tr.primaryRenderView = renderView;

        // Draw a 3D view into a part of the window, then return to 2D drawing.
        // Rendering a scene may require multiple views to be rendered to handle mirrors,
        public override void RenderScene(RenderView renderView)
        {
            var tr_ = (RenderSystemLocal)tr;
            if (!glConfig.isInitialized) return;

            // skip front end rendering work, which will result in only gui drawing
            if (r_skipFrontEnd.Bool) return;

            if (renderView.fov_x <= 0 || renderView.fov_y <= 0) common.Error($"RenderWorld::RenderScene: bad FOVs: {renderView.fov_x}, {renderView.fov_y}");

            // close any gui drawing
            tr_.guiModel.EmitFullScreen();
            tr_.guiModel.Clear();

            var startTime = SysW.Milliseconds;

            // setup view parms for the initial view
            var parms = new ViewDef();
            parms.renderView = new RenderView(renderView);

            if (tr.takingScreenshot) parms.renderView.forceUpdate = true;

            // set up viewport, adjusted for resolution and OpenGL style 0 at the bottom
            tr.RenderViewToViewport(parms.renderView, out parms.viewport);

            // the scissor bounds may be shrunk in subviews even if
            // the viewport stays the same
            // this scissor range is local inside the viewport
            parms.scissor.x1 = 0;
            parms.scissor.y1 = 0;
            parms.scissor.x2 = (short)(parms.viewport.x2 - parms.viewport.x1);
            parms.scissor.y2 = (short)(parms.viewport.y2 - parms.viewport.y1);

            parms.isSubview = false;
            parms.initialViewAreaOrigin = renderView.vieworg;
            parms.floatTime = parms.renderView.time * 0.001f;
            parms.renderWorld = this;

            // use this time for any subsequent 2D rendering, so damage blobs/etc can use level time
            tr.frameShaderTime = parms.floatTime;

            // see if the view needs to reverse the culling sense in mirrors or environment cube sides
            var cross = parms.renderView.viewaxis[1].Cross(parms.renderView.viewaxis[2]);
            parms.isMirror = cross * parms.renderView.viewaxis[0] <= 0f;

            if (r_lockSurfaces.Bool) { R_LockSurfaceScene(parms); return; }

            // save this world for use by some console commands
            tr.primaryWorld = this;
            tr.primaryRenderView = new RenderView(renderView);
            tr.primaryView = parms;

            // rendering this view may cause other views to be rendered for mirrors / portals / shadows / environment maps
            // this will also cause any necessary entities and lights to be updated to the demo file
            R_RenderView(parms);

            // now write delete commands for any modified-but-not-visible entities, and add the renderView command to the demo
            if (session.writeDemo != null) WriteRenderView(renderView);

#if false
            for (var i = 0; i < entityDefs.Count; i++)
            {
                var def = entityDefs[i];
                if (def == null || def.parms.callback != null) continue;
                if (def.parms.hModel.IsDynamicModel == DynamicModel.DM_CONTINUOUS) { }
            }
#endif

            var endTime = SysW.Milliseconds;

            tr.pc.frontEndMsec += endTime - startTime;

            // prepare for any 2D drawing after this
            tr_.guiModel.Clear();
        }

        public override int NumAreas
            => numPortalAreas;

        public override int NumPortalsInArea(int areaNum)
        {
            if (areaNum >= numPortalAreas || areaNum < 0) common.Error("RenderWorld::NumPortalsInArea: bad areanum {areaNum}");
            var area = portalAreas[areaNum];

            var count = 0;
            for (var portal = area.portals; portal != null; portal = portal.next) count++;
            return count;
        }

        public override ExitPortal GetPortal(int areaNum, int portalNum)
        {
            if (areaNum > numPortalAreas) common.Error("RenderWorld::GetPortal: areaNum > numAreas");
            var area = portalAreas[areaNum];

            ExitPortal ret = default;
            var count = 0;
            for (var portal = area.portals; portal != null; portal = portal.next)
            {
                if (count == portalNum)
                {
                    ret.areas[0] = areaNum;
                    ret.areas[1] = portal.intoArea;
                    ret.w = portal.w;
                    ret.blockingBits = portal.doublePortal.blockingBits;
                    ret.portalHandle = (Qhandle)(portal.doublePortal - doublePortals + 1);
                    return ret;
                }
                count++;
            }

            common.Error("RenderWorld::GetPortal: portalNum > numPortals");
            return ret;
        }

        // Will return -1 if the point is not in an area, otherwise it will return 0 <= value < tr.world.numPortalAreas
        public override int PointInArea(in Vector3 point)
        {
            int nodeNum; float d;

            var node = areaNodes[0];
            if (node == null) return -1;
            while (true)
            {
                d = point * node.plane.Normal + node.plane.d;
                nodeNum = d > 0 ? node.children0 : node.children1;
                if (nodeNum == 0) return -1; // in solid
                if (nodeNum < 0)
                {
                    nodeNum = -1 - nodeNum;
                    if (nodeNum >= numPortalAreas) common.Error("RenderWorld::PointInArea: area out of range");
                    return nodeNum;
                }
                node = areaNodes[nodeNum];
            }
        }

        public void BoundsInAreas_r(int nodeNum, in Bounds bounds, int[] areas, ref int numAreas, int maxAreas)
        {
            int i; PLANESIDE side;

            do
            {
                if (nodeNum < 0)
                {
                    nodeNum = -1 - nodeNum;
                    for (i = 0; i < numAreas; i++) if (areas[i] == nodeNum) break;
                    if (i >= numAreas && numAreas < maxAreas) areas[numAreas++] = nodeNum;
                    return;
                }

                var node = areaNodes[nodeNum];
                side = bounds.PlaneSide(node.plane);
                if (side == PLANESIDE.FRONT) nodeNum = node.children0;
                else if (side == PLANESIDE.BACK) nodeNum = node.children1;
                else
                {
                    if (node.children1 != 0)
                    {
                        BoundsInAreas_r(node.children1, bounds, areas, ref numAreas, maxAreas);
                        if (numAreas >= maxAreas) return;
                    }
                    nodeNum = node.children0;
                }
            } while (nodeNum != 0);

            return;
        }

        // fills the *areas array with the number of the areas the bounds are in returns the total number of areas the bounds are in
        public override int BoundsInAreas(in Bounds bounds, int[] areas, int maxAreas)
        {
            Debug.Assert(bounds.b0.x <= bounds.b1.x && bounds.b0.y <= bounds.b1.y && bounds.b0.z <= bounds.b1.z);
            Debug.Assert(bounds.b1.x - bounds.b0.x < 1e4f && bounds.b1.y - bounds.b0.y < 1e4f && bounds.b1.z - bounds.b0.z < 1e4f);

            var numAreas = 0;
            if (areaNodes == null) return numAreas;
            BoundsInAreas_r(0, bounds, areas, ref numAreas, maxAreas);
            return numAreas;
        }


        // checks a ray trace against any gui surfaces in an entity, returning the fraction location of the trace on the gui surface, or -1,-1 if no hit.
        // this doesn't do any occlusion testing, simply ignoring non-gui surfaces. start / end are in global world coordinates.
        public override GuiPoint GuiTrace(Qhandle entityHandle, object animator, in Vector3 start, in Vector3 end)
        {
            LocalTrace local; Vector3 localStart, localEnd;
            int j; IRenderModel model; SrfTriangles tri; Material shader;

            GuiPoint pt;
            pt.fraction = 1f; // Koz
            pt.x = pt.y = -1;
            pt.guiId = 0;

            var isPDA = false;
            if (entityHandle < 0 || entityHandle >= entityDefs.Count) { common.Printf($"RenderWorld::GuiTrace: invalid handle {entityHandle}\n"); return pt; }

            var def = entityDefs[entityHandle];
            if (def == null) { common.Printf($"RenderWorld::GuiTrace: handle {entityHandle} is NULL\n"); return pt; }

            model = def.parms.hModel;
            //if (def.parms.callback || !def.parms.hModel || def.parms.hModel.IsDynamicModel() != DM_STATIC) {
            if (def.parms.hModel == null) return pt;

            var guiJoints = stackalloc JointHandle[4];
            if (game.IsPDAOpen)
            {
                isPDA = string.Equals("models/md5/items/pda_view/pda_vr_idle.md5mesh", model.Name, StringComparison.OrdinalIgnoreCase);
                if (isPDA)
                {
                    guiJoints[3] = model.GetJointHandle("BLgui");
                    guiJoints[0] = model.GetJointHandle("BRgui");
                    guiJoints[1] = model.GetJointHandle("TRgui");
                    guiJoints[2] = model.GetJointHandle("TLgui");
                    for (var checkJoint = 0; checkJoint < 4; checkJoint++) if (guiJoints[checkJoint] == JointHandle.INVALID_JOINT) isPDA = false;
                }
            }

            if ((model.IsDynamicModel != DynamicModel.DM_STATIC || def.parms.callback != null) && !isPDA) return pt;

            // transform the points into local space
            R_GlobalPointToLocal(def.modelMatrix, start, out localStart);
            R_GlobalPointToLocal(def.modelMatrix, end, out localEnd);

            for (j = 0; j < model.NumSurfaces; j++)
            {
                var surf = model.Surface(j);
                tri = surf.geometry;
                if (tri == null) continue;

                shader = R_RemapShaderBySkin(surf.shader, def.parms.customSkin, def.parms.customShader);
                if (shader == null) continue;
                // only trace against gui surfaces
                if (!shader.HasGui && !isPDA) continue;
                if (isPDA)
                {
                    var discardAxis = Matrix3x3.identity;
                    var modelOrigin = def.parms.origin;
                    var modelAxis = def.parms.axis;
                    for (var jj = 0; jj < 4; jj++)
                    {
                        // overwrite surface coords for testing
                        game.AnimatorGetJointTransform(animator, guiJoints[jj], SysW.Milliseconds, tri.verts[jj].xyz, discardAxis);
                        //animator.GetJointTransform( guiJoints[jj], Sys_Milliseconds(), tri.verts[jj].xyz, discardAxis );

                        // draw debug lines from view start to gui corners
                        //gameRenderWorld.DebugLine(colorYellow, start, modelOrigin + tri.verts[jj].xyz * modelAxis, 20);
                    }
                }

                local = R_LocalTrace(localStart, localEnd, 0f, tri);
                if (local.fraction < 1f)
                {
                    Vector3 origin = default, cursor;
                    var axis = stackalloc Vector3[3];
                    var axisLen = stackalloc float[2];

                    R_SurfaceToTextureAxis(tri, ref origin, axis);
                    cursor = local.point - origin;

                    axisLen[0] = axis[0].Length;
                    axisLen[1] = axis[1].Length;

                    pt.x = cursor * axis[0] / (axisLen[0] * axisLen[0]);
                    pt.y = cursor * axis[1] / (axisLen[1] * axisLen[1]);
                    pt.guiId = shader.EntityGui;
                    pt.fraction = local.fraction;
                    return pt;
                }
            }

            return pt;
        }

        public override bool ModelTrace(out ModelTrace trace, Qhandle entityHandle, in Vector3 start, in Vector3 end, float radius)
        {
            int i; bool collisionSurface;
            ModelSurface surf; LocalTrace localTrace; IRenderModel model; Vector3 localStart, localEnd; Material shader;
            var modelMatrix = new float[16];

            trace = default;
            trace.fraction = 1f;

            if (entityHandle < 0 || entityHandle >= entityDefs.Count) { /*common.Error($"RenderWorld::ModelTrace: index = {entityHandle}");*/ return false; }

            var def = entityDefs[entityHandle];
            if (def == null) return false;

            var refEnt = def.parms;
            model = R_EntityDefDynamicModel(def);
            if (model == null) return false;

            // transform the points into local space
            R_AxisToModelMatrix(refEnt.axis, refEnt.origin, modelMatrix);
            R_GlobalPointToLocal(modelMatrix, start, out localStart);
            R_GlobalPointToLocal(modelMatrix, end, out localEnd);

            // if we have explicit collision surfaces, only collide against them (FIXME, should probably have a parm to control this)
            collisionSurface = false;
            for (i = 0; i < model.NumBaseSurfaces; i++)
            {
                surf = model.Surface(i);

                shader = R_RemapShaderBySkin(surf.shader, def.parms.customSkin, def.parms.customShader);

                if ((shader.SurfaceFlags & SURF.COLLISION) != 0) { collisionSurface = true; break; }
            }

            // only use baseSurfaces, not any overlays
            for (i = 0; i < model.NumBaseSurfaces; i++)
            {
                surf = model.Surface(i);

                shader = R_RemapShaderBySkin(surf.shader, def.parms.customSkin, def.parms.customShader);

                if (surf.geometry == null || shader == null) continue;

                if (collisionSurface)
                {
                    // only trace vs collision surfaces
                    if ((shader.SurfaceFlags & SURF.COLLISION) != 0) continue;
                }
                else
                {
                    // skip if not drawn or translucent
                    if (!shader.IsDrawn || (shader.Coverage != MC.OPAQUE && shader.Coverage != MC.PERFORATED)) continue;
                }

                localTrace = R_LocalTrace(localStart, localEnd, radius, surf.geometry);

                if (localTrace.fraction < trace.fraction)
                {
                    trace.fraction = localTrace.fraction;
                    R_LocalPointToGlobal(modelMatrix, localTrace.point, out trace.point);
                    trace.normal = localTrace.normal * refEnt.axis;
                    trace.material = shader;
                    trace.entity = def.parms;
                    trace.jointNumber = refEnt.hModel.NearestJoint(i, localTrace.indexes[0], localTrace.indexes[1], localTrace.indexes[2]);
                }
            }

            return trace.fraction < 1f;
        }

        // FIXME: _D3XP added those.
        static string[] playerModelExcludeList = {
            "models/md5/characters/player/d3xp_spplayer.md5mesh",
            "models/md5/characters/player/head/d3xp_head.md5mesh",
            "models/md5/weapons/pistol_world/worldpistol.md5mesh",
            null
        };

        static string[] playerMaterialExcludeList = {
            "muzzlesmokepuff",
            null
        };

        public override bool Trace(out ModelTrace trace, in Vector3 start, in Vector3 end, float radius, bool skipDynamic = true, bool skipPlayer = false)
        {
            AreaReference ref_;
            IRenderEntity def;
            PortalArea area;
            IRenderModel model;
            SrfTriangles tri;
            LocalTrace localTrace;
            int numAreas, i, j, numSurfaces;
            Bounds traceBounds = default, bounds = default;
            Vector3 localStart, localEnd;
            Material shader;
            var areas = new int[128];
            var modelMatrix = new float[16];

            trace = default;
            trace.fraction = 1f;
            trace.point = end;

            // bounds for the whole trace
            traceBounds.Clear();
            traceBounds.AddPoint(start);
            traceBounds.AddPoint(end);

            // get the world areas the trace is in
            numAreas = BoundsInAreas(traceBounds, areas, 128);

            numSurfaces = 0;

            // check all areas for models
            for (i = 0; i < numAreas; i++)
            {
                area = portalAreas[areas[i]];

                // check all models in this area
                for (ref_ = area.entityRefs.areaNext; ref_ != area.entityRefs; ref_ = ref_.areaNext)
                {
                    def = ref_.entity;

                    model = def.parms.hModel;
                    if (model == null) continue;

                    if (model.IsDynamicModel != DynamicModel.DM_STATIC)
                    {
                        if (skipDynamic) continue;

#if true	            // _D3XP addition. could use a cleaner approach
                        if (skipPlayer)
                        {
                            var name = model.Name;
                            int k;
                            for (k = 0; playerModelExcludeList[k] != null; k++) { var exclude = playerModelExcludeList[k]; if (name == exclude) break; }
                            if (playerModelExcludeList[k] != null) continue;
                        }
#endif

                        model = R_EntityDefDynamicModel(def);
                        if (model == null) continue;   // can happen with particle systems, which don't instantiate without a valid view
                    }

                    bounds.FromTransformedBounds(model.Bounds(def.parms), def.parms.origin, def.parms.axis);

                    // if the model bounds do not overlap with the trace bounds
                    if (!traceBounds.IntersectsBounds(bounds) || !bounds.LineIntersection(start, trace.point)) continue;

                    // check all model surfaces
                    for (j = 0; j < model.NumSurfaces; j++)
                    {
                        var surf = model.Surface(j);
                        shader = R_RemapShaderBySkin(surf.shader, def.parms.customSkin, def.parms.customShader);

                        // if no geometry or no shader
                        if (surf.geometry == null || shader == null) continue;

#if true                // _D3XP addition. could use a cleaner approach
                        if (skipPlayer)
                        {
                            var name = shader.Name;
                            int k;
                            for (k = 0; playerMaterialExcludeList[k] != null; k++) { var exclude = playerMaterialExcludeList[k]; if (name == exclude) break; }
                            if (playerMaterialExcludeList[k] != null) continue;
                        }
#endif

                        tri = surf.geometry;

                        bounds.FromTransformedBounds(tri.bounds, def.parms.origin, def.parms.axis);

                        // if triangle bounds do not overlap with the trace bounds
                        if (!traceBounds.IntersectsBounds(bounds) || !bounds.LineIntersection(start, trace.point)) continue;

                        numSurfaces++;

                        // transform the points into local space
                        R_AxisToModelMatrix(def.parms.axis, def.parms.origin, modelMatrix);
                        R_GlobalPointToLocal(modelMatrix, start, out localStart);
                        R_GlobalPointToLocal(modelMatrix, end, out localEnd);

                        localTrace = R_LocalTrace(localStart, localEnd, radius, surf.geometry);

                        if (localTrace.fraction < trace.fraction)
                        {
                            trace.fraction = localTrace.fraction;
                            R_LocalPointToGlobal(modelMatrix, localTrace.point, out trace.point);
                            trace.normal = localTrace.normal * def.parms.axis;
                            trace.material = shader;
                            trace.entity = def.parms;
                            trace.jointNumber = model.NearestJoint(j, localTrace.indexes[0], localTrace.indexes[1], localTrace.indexes[2]);

                            traceBounds.Clear();
                            traceBounds.AddPoint(start);
                            traceBounds.AddPoint(start + trace.fraction * (end - start));
                        }
                    }
                }
            }
            return trace.fraction < 1f;
        }

        void RecurseProcBSP_r(ref ModelTrace results, int parentNodeNum, int nodeNum, float p1f, float p2f, in Vector3 p1, in Vector3 p2)
        {
            float t1, t2, frac, midf; Vector3 mid;
            AreaNode node;

            if (results.fraction <= p1f) return;     // already hit something nearer
            // empty leaf
            if (nodeNum < 0) return;
            // if solid leaf node
            if (nodeNum == 0 && parentNodeNum != -1)
            {
                results.fraction = p1f;
                results.point = p1;
                node = areaNodes[parentNodeNum];
                results.normal = node.plane.Normal;
                return;
            }
            node = areaNodes[nodeNum];

            // distance from plane for trace start and end
            t1 = node.plane.Normal * p1 + node.plane.d;
            t2 = node.plane.Normal * p2 + node.plane.d;

            if (t1 >= 0f && t2 >= 0f) { RecurseProcBSP_r(ref results, nodeNum, node.children0, p1f, p2f, p1, p2); return; }
            if (t1 < 0f && t2 < 0f) { RecurseProcBSP_r(ref results, nodeNum, node.children1, p1f, p2f, p1, p2); return; }
            frac = t1 / (t1 - t2);
            midf = p1f + frac * (p2f - p1f);
            mid.x = p1.x + frac * (p2.x - p1.x);
            mid.y = p1.y + frac * (p2.y - p1.y);
            mid.z = p1.z + frac * (p2.z - p1.z);
            RecurseProcBSP_r(ref results, nodeNum, t1 < t2 ? node.children1 : node.children0, p1f, midf, p1, mid);
            RecurseProcBSP_r(ref results, nodeNum, t1 < t2 ? node.children0 : node.children1, midf, p2f, mid, p2);
        }

        public override bool FastWorldTrace(out ModelTrace results, in Vector3 start, in Vector3 end)
        {
            results = default;
            results.fraction = 1f;
            if (areaNodes != null) { RecurseProcBSP_r(ref results, -1, 0, 0f, 1f, start, end); return results.fraction < 1f; }
            return false;
        }

        /*
        =================================================================================

        CREATE MODEL REFS

        =================================================================================
        */

        // This is called by R_PushVolumeIntoTree and also directly for the world model references that are precalculated.
        public void AddEntityRefToArea(IRenderEntity def, PortalArea area)
        {
            if (def == null) common.Error("RenderWorldLocal::AddEntityRefToArea: NULL def");
            var ref_ = areaReferenceAllocator.Alloc();

            tr.pc.c_entityReferences++;

            ref_.entity = def;

            // link to entityDef
            ref_.ownerNext = def.entityRefs;
            def.entityRefs = ref_;

            // link to end of area list
            ref_.area = area;
            ref_.areaNext = area.entityRefs;
            ref_.areaPrev = area.entityRefs.areaPrev;
            ref_.areaNext.areaPrev = ref_;
            ref_.areaPrev.areaNext = ref_;
        }

        void AddLightRefToArea(IRenderLight light, PortalArea area)
        {
            // add a lightref to this area
            var lref = areaReferenceAllocator.Alloc();
            lref.light = light;
            lref.area = area;
            lref.ownerNext = light.references;
            light.references = lref;
            tr.pc.c_lightReferences++;

            // doubly linked list so we can free them easily later
            area.lightRefs.areaNext.areaPrev = lref;
            lref.areaNext = area.lightRefs.areaNext;
            lref.areaPrev = area.lightRefs;
            area.lightRefs.areaNext = lref;
        }

        // Force the generation of all light / surface interactions at the start of a level
        // If this isn't called, they will all be dynamically generated
        // This really isn't all that helpful anymore, because the calculation of shadows and light interactions is deferred from idRenderWorldLocal::CreateLightDefInteractions(), but we
        // use it as an oportunity to size the interactionTable
        public override void GenerateAllInteractions()
        {
            if (!glConfig.isInitialized) return;

            var start = SysW.Milliseconds;

            generateAllInteractionsCalled = false;

            // watch how much memory we allocate
            tr.staticAllocCount = 0;

            // let idRenderWorldLocal::CreateLightDefInteractions() know that it shouldn't try and do any view specific optimizations
            tr.viewDef = null;

            for (var i = 0; i < lightDefs.Count; i++)
            {
                var ldef = lightDefs[i];
                if (ldef == null) continue;
                CreateLightDefInteractions(ldef);
            }

            var end = SysW.Milliseconds;
            var msec = end - start;
            common.Printf($"RenderWorld::GenerateAllInteractions, msec = {msec}, staticAllocCount = {tr.staticAllocCount}.\n");

            // build the interaction table
            if (r_useInteractionTable.Bool)
            {
                interactionTableWidth = entityDefs.Count + 100;
                interactionTableHeight = lightDefs.Count + 100;
                var size = interactionTableWidth * interactionTableHeight;
                interactionTable = new IInteraction[size];

                var count = 0;
                for (var i = 0; i < lightDefs.Count; i++)
                {
                    var ldef = lightDefs[i];
                    if (ldef == null) continue;
                    for (var inter = ldef.firstInteraction; inter != null; inter = inter.lightNext)
                    {
                        var edef = inter.entityDef;
                        var index = ldef.index * interactionTableWidth + edef.index;
                        interactionTable[index] = inter;
                        count++;
                    }
                }

                common.Printf($"interactionTable size: {size}\n");
                common.Printf($"{count} interactions\n");
            }

            // entities flagged as noDynamicInteractions will no longer make any
            generateAllInteractionsCalled = true;
        }

        public void FreeInteractions()
        {
            for (var i = 0; i < entityDefs.Count; i++)
            {
                var def = entityDefs[i];
                if (def == null) continue;
                // free all the interactions
                while (def.firstInteraction != null) def.firstInteraction.UnlinkAndFree();
            }
        }

        // Used for both light volumes and model volumes.
        // This does not clip the points by the planes, so some slop occurs.
        // tr.viewCount should be bumped before calling, allowing it to prevent double checking areas.
        // We might alternatively choose to do this with an area flow.
        public void PushVolumeIntoTree_r(IRenderEntity def, IRenderLight light, in Sphere sphere, int numPoints, in Vector3[] points, int nodeNum)
        {
            int i; bool front, back;

            if (nodeNum < 0)
            {
                var areaNum = -1 - nodeNum;
                var area = portalAreas[areaNum];
                if (area.viewCount == tr.viewCount) return; // already added a reference here
                area.viewCount = tr.viewCount;

                if (def != null) AddEntityRefToArea(def, area);
                if (light != null) AddLightRefToArea(light, area);
                return;
            }

            var node = areaNodes[nodeNum];

            // if we know that all possible children nodes only touch an area we have already marked, we can early out
            if (r_useNodeCommonChildren.Bool && node.commonChildrenArea != CHILDREN_HAVE_MULTIPLE_AREAS)
            {
                // note that we do NOT try to set a reference in this area yet, because the test volume may yet wind up being in the
                // solid part, which would cause bounds slightly poked into a wall to show up in the next room
                if (portalAreas[node.commonChildrenArea].viewCount == tr.viewCount) return;
            }

            // if the bounding sphere is completely on one side, don't
            // bother checking the individual points
            var sd = node.plane.Distance(sphere.Origin);
            if (sd >= sphere.Radius)
            {
                nodeNum = node.children0;
                if (nodeNum != 0) PushVolumeIntoTree_r(def, light, sphere, numPoints, points, nodeNum); // 0 = solid
                return;
            }
            if (sd <= -sphere.Radius)
            {
                nodeNum = node.children1;
                if (nodeNum != 0) PushVolumeIntoTree_r(def, light, sphere, numPoints, points, nodeNum); // 0 = solid
                return;
            }

            // exact check all the points against the node plane
            front = back = false;
            for (i = 0; i < numPoints; i++)
            {
                var d = points[i] * node.plane.Normal + node.plane.d;
                if (d >= 0f) front = true;
                else if (d <= 0f) back = true;
                if (back && front) break;
            }
            if (front)
            {
                nodeNum = node.children0;
                if (nodeNum != 0) PushVolumeIntoTree_r(def, light, sphere, numPoints, points, nodeNum); // 0 = solid
            }
            if (back)
            {
                nodeNum = node.children1;
                if (nodeNum != 0) PushVolumeIntoTree_r(def, light, sphere, numPoints, points, nodeNum); // 0 = solid
            }
        }

        public void PushVolumeIntoTree(IRenderEntity def, IRenderLight light, int numPoints, in Vector3[] points)
        {
            int i; float radSquared, lr;
            Vector3 mid = default, dir;

            if (areaNodes == null) return;

            // calculate a bounding sphere for the points
            mid.Zero();
            for (i = 0; i < numPoints; i++) mid += points[i];
            mid *= (1f / numPoints);

            radSquared = 0;
            for (i = 0; i < numPoints; i++)
            {
                dir = points[i] - mid;
                lr = dir * dir;
                if (lr > radSquared) radSquared = lr;
            }

            var sphere = new Sphere(mid, (float)Math.Sqrt(radSquared));
            PushVolumeIntoTree_r(def, light, sphere, numPoints, points, 0);
        }

        //===================================================================

        public override void DebugClearLines(int time)
        {
            //RB_ClearDebugLines(time);
            //RB_ClearDebugText(time);
        }

        public override void DebugLine(in Vector4 color, in Vector3 start, in Vector3 end, int lifetime = 0, bool depthTest = false)
        {
            //RB_AddDebugLine(color, start, end, lifetime, depthTest);
        }

        static float[] DebugArrow_arrowCos = new float[40];
        static float[] DebugArrow_arrowSin = new float[40];
        static int DebugArrow_arrowStep;
        public override void DebugArrow(in Vector4 color, in Vector3 start, in Vector3 end, int size, int lifetime = 0)
        {
            Vector3 forward, v1, v2;
            int i; float a, s;

            DebugLine(color, start, end, lifetime);

            if (r_debugArrowStep.Integer <= 10) return;
            // calculate sine and cosine when step size changes
            if (DebugArrow_arrowStep != r_debugArrowStep.Integer)
            {
                DebugArrow_arrowStep = r_debugArrowStep.Integer;
                for (i = 0, a = 0; a < 360f; a += DebugArrow_arrowStep, i++)
                {
                    DebugArrow_arrowCos[i] = MathX.Cos16(MathX.DEG2RAD(a));
                    DebugArrow_arrowSin[i] = MathX.Sin16(MathX.DEG2RAD(a));
                }
                DebugArrow_arrowCos[i] = DebugArrow_arrowCos[0];
                DebugArrow_arrowSin[i] = DebugArrow_arrowSin[0];
            }
            // draw a nice arrow
            forward = end - start;
            forward.Normalize();
            forward.NormalVectors(out var right, out var up);
            for (i = 0, a = 0; a < 360f; a += DebugArrow_arrowStep, i++)
            {
                s = 0.5f * size * DebugArrow_arrowCos[i];
                v1 = end - size * forward;
                v1 += s * right;
                s = 0.5f * size * DebugArrow_arrowSin[i];
                v1 += s * up;

                s = 0.5f * size * DebugArrow_arrowCos[i + 1];
                v2 = end - size * forward;
                v2 += s * right;
                s = 0.5f * size * DebugArrow_arrowSin[i + 1];
                v2 += s * up;

                DebugLine(color, v1, end, lifetime);
                DebugLine(color, v1, v2, lifetime);
            }
        }

        public override void DebugWinding(in Vector4 color, Winding w, in Vector3 origin, in Matrix3x3 axis, int lifetime = 0, bool depthTest = false)
        {
            if (w.NumPoints < 2) return;

            var lastPoint = origin + w[w.NumPoints - 1].ToVec3() * axis;
            for (var i = 0; i < w.NumPoints; i++)
            {
                var point = origin + w[i].ToVec3() * axis;
                DebugLine(color, lastPoint, point, lifetime, depthTest);
                lastPoint = point;
            }
        }

        public override void DebugCircle(in Vector4 color, in Vector3 origin, in Vector3 dir, float radius, int numSteps, int lifetime = 0, bool depthTest = false)
        {
            dir.OrthogonalBasis(out var left, out var up);
            left *= radius;
            up *= radius;
            var lastPoint = origin + up;
            for (var i = 1; i <= numSteps; i++)
            {
                var a = MathX.TWO_PI * i / numSteps;
                var point = origin + MathX.Sin16(a) * left + MathX.Cos16(a) * up;
                DebugLine(color, lastPoint, point, lifetime, depthTest);
                lastPoint = point;
            }
        }

        public override void DebugSphere(in Vector4 color, in Sphere sphere, int lifetime = 0, bool depthTest = false)
        {
            int i, j, n; float s, c; Vector3 p, lastp;

            var num = 360 / 15;
            var lastArray = stackalloc Vector3[num];
            lastArray[0] = sphere.Origin + new Vector3(0, 0, sphere.Radius);
            for (n = 1; n < num; n++) lastArray[n] = lastArray[0];

            for (i = 15; i <= 360; i += 15)
            {
                s = MathX.Sin16(MathX.DEG2RAD(i));
                c = MathX.Cos16(MathX.DEG2RAD(i));
                lastp.x = sphere.Origin.x;
                lastp.y = sphere.Origin.y + sphere.Radius * s;
                lastp.z = sphere.Origin.z + sphere.Radius * c;
                for (n = 0, j = 15; j <= 360; j += 15, n++)
                {
                    p.x = sphere.Origin.x + MathX.Sin16(MathX.DEG2RAD(j)) * sphere.Radius * s;
                    p.y = sphere.Origin.y + MathX.Cos16(MathX.DEG2RAD(j)) * sphere.Radius * s;
                    p.z = lastp.z;

                    DebugLine(color, lastp, p, lifetime, depthTest);
                    DebugLine(color, lastp, lastArray[n], lifetime, depthTest);

                    lastArray[n] = lastp;
                    lastp = p;
                }
            }
        }

        public override void DebugBounds(in Vector4 color, in Bounds bounds, in Vector3 org, int lifetime = 0)
        {
            int i;
            var v = stackalloc Vector3[8];

            if (bounds.IsCleared) return;

            for (i = 0; i < 8; i++)
            {
                v[i].x = org.x + bounds[(i ^ (i >> 1)) & 1].x;
                v[i].y = org.y + bounds[(i >> 1) & 1].y;
                v[i].z = org.z + bounds[(i >> 2) & 1].z;
            }
            for (i = 0; i < 4; i++)
            {
                DebugLine(color, v[i], v[(i + 1) & 3], lifetime);
                DebugLine(color, v[4 + i], v[4 + ((i + 1) & 3)], lifetime);
                DebugLine(color, v[i], v[4 + i], lifetime);
            }
        }

        public override void DebugBox(in Vector4 color, in Box box, int lifetime = 0)
        {
            box.ToPoints(out var v);
            for (var i = 0; i < 4; i++)
            {
                DebugLine(color, v[i], v[(i + 1) & 3], lifetime);
                DebugLine(color, v[4 + i], v[4 + ((i + 1) & 3)], lifetime);
                DebugLine(color, v[i], v[4 + i], lifetime);
            }
        }

        public override void DebugFrustum(in Vector4 color, Frustum frustum, bool showFromOrigin, int lifetime = 0)
        {
            int i;

            frustum.ToPoints(out var v);

            if (frustum.NearDistance > 0f)
            {
                for (i = 0; i < 4; i++) DebugLine(color, v[i], v[(i + 1) & 3], lifetime);
                if (showFromOrigin) for (i = 0; i < 4; i++) DebugLine(color, frustum.Origin, v[i], lifetime);
            }
            for (i = 0; i < 4; i++)
            {
                DebugLine(color, v[4 + i], v[4 + ((i + 1) & 3)], lifetime);
                DebugLine(color, v[i], v[4 + i], lifetime);
            }
        }


        // dir is the cone axis
        // radius1 is the radius at the apex
        // radius2 is the radius at apex+dir
        public override void DebugCone(in Vector4 color, in Vector3 apex, in Vector3 dir, float radius1, float radius2, int lifetime = 0)
        {
            int i; Vector3 top, p1, p2, lastp1, lastp2, d;
            Matrix3x3 axis = default;

            axis[2] = dir;
            axis[2].Normalize();
            axis[2].NormalVectors(out axis[0], out axis[1]);
            axis[1] = -axis[1];

            top = apex + dir;
            lastp2 = top + radius2 * axis[1];

            if (radius1 == 0f)
            {
                for (i = 20; i <= 360; i += 20)
                {
                    d = MathX.Sin16(MathX.DEG2RAD(i)) * axis[0] + MathX.Cos16(MathX.DEG2RAD(i)) * axis[1];
                    p2 = top + d * radius2;
                    DebugLine(color, lastp2, p2, lifetime);
                    DebugLine(color, p2, apex, lifetime);
                    lastp2 = p2;
                }
            }
            else
            {
                lastp1 = apex + radius1 * axis[1];
                for (i = 20; i <= 360; i += 20)
                {
                    d = MathX.Sin16(MathX.DEG2RAD(i)) * axis[0] + MathX.Cos16(MathX.DEG2RAD(i)) * axis[1];
                    p1 = apex + d * radius1;
                    p2 = top + d * radius2;
                    DebugLine(color, lastp1, p1, lifetime);
                    DebugLine(color, lastp2, p2, lifetime);
                    DebugLine(color, p1, p2, lifetime);
                    lastp1 = p1;
                    lastp2 = p2;
                }
            }
        }

        public override void DebugAxis(in Vector3 origin, in Matrix3x3 axis)
        {
            var start = origin;
            var end = start + axis[0] * 20f; DebugArrow(colorWhite, start, end, 2);
            end = start + axis[0] * -20f; DebugArrow(colorWhite, start, end, 2);
            end = start + axis[1] * +20f; DebugArrow(colorGreen, start, end, 2);
            end = start + axis[1] * -20f; DebugArrow(colorGreen, start, end, 2);
            end = start + axis[2] * +20f; DebugArrow(colorBlue, start, end, 2);
            end = start + axis[2] * -20f; DebugArrow(colorBlue, start, end, 2);
        }

        public override void DebugClearPolygons(int time)
        {
            //RB_ClearDebugPolygons(time);
        }

        public override void DebugPolygon(in Vector4 color, Winding winding, int lifeTime = 0, bool depthTest = false)
        {
            //RB_AddDebugPolygon(color, winding, lifeTime, depthTest );
        }

        public override void DebugScreenRect(in Vector4 color, ScreenRect rect, ViewDef viewDef, int lifetime = 0)
        {
            int i;
            float centerx, centery, dScale, hScale, vScale;
            Bounds bounds;
            var p = stackalloc Vector3[4];

            centerx = (viewDef.viewport.x2 - viewDef.viewport.x1) * 0.5f;
            centery = (viewDef.viewport.y2 - viewDef.viewport.y1) * 0.5f;

            dScale = r_znear.Float + 1f;
            hScale = dScale * MathX.Tan16(MathX.DEG2RAD(viewDef.renderView.fov_x * 0.5f));
            vScale = dScale * MathX.Tan16(MathX.DEG2RAD(viewDef.renderView.fov_y * 0.5f));

            bounds.b0.x = bounds.b1.x = dScale;
            bounds.b0.y = -(rect.x1 - centerx) / centerx * hScale;
            bounds.b1.y = -(rect.x2 - centerx) / centerx * hScale;
            bounds.b0.z = (rect.y1 - centery) / centery * vScale;
            bounds.b1.z = (rect.y2 - centery) / centery * vScale;

            for (i = 0; i < 4; i++)
            {
                p[i].x = bounds[0][0];
                p[i].y = bounds[(i ^ (i >> 1)) & 1].y;
                p[i].z = bounds[(i >> 1) & 1].z;
                p[i] = viewDef.renderView.vieworg + p[i] * viewDef.renderView.viewaxis;
            }
            for (i = 0; i < 4; i++) DebugLine(color, p[i], p[(i + 1) & 3], lifetime);
        }

        // returns the length of the given text
        public float DrawTextLength(string text, float scale, int len)
        {
            //return RB_DrawTextLength(text, scale, len);
            return 0;
        }

        // oriented on the viewaxis
        // align can be 0-left, 1-center (default), 2-right
        public override void DrawText(string text, in Vector3 origin, float scale, in Vector4 color, in Matrix3x3 viewAxis, int align = 1, int lifetime = 0, bool depthTest = false)
        {
            //RB_AddDebugText(text, origin, scale, color, viewAxis, align, lifetime, depthTest);
        }

        public override void RegenerateWorld()
        {
            R_RegenerateWorld_f(CmdArgs.Empty);
        }

        public bool R_GlobalShaderOverride(ref Material shader)
        {
            if (!shader.IsDrawn) return false;
            if (tr.primaryRenderView.globalMaterial != null) { shader = tr.primaryRenderView.globalMaterial; return true; }
            if (!string.IsNullOrEmpty(r_materialOverride.String)) { shader = declManager.FindMaterial(r_materialOverride.String); return true; }
            return false;
        }

        public Material R_RemapShaderBySkin(Material shader, DeclSkin skin, Material customShader)
        {

            if (shader == null) return null;

            // never remap surfaces that were originally nodraw, like collision hulls
            if (!shader.IsDrawn) return shader;

            if (customShader != null)
            {
                // this is sort of a hack, but cause deformed surfaces to map to empty surfaces, so the item highlight overlay doesn't highlight the autosprite surface
                if (shader.Deform != 0) return null;
                return customShader;
            }

            if (skin == null || shader == null) return shader;

            return skin.RemapShaderBySkin(shader);
        }
    }
}