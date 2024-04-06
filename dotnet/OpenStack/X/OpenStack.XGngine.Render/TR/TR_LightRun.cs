using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        // Modifies the shaderParms on all the lights so the level designers can easily test different color schemes
        public static void R_ModulateLights_f(CmdArgs args)
        {
            if (tr.primaryWorld == null) return;
            if (args.Count != 4) { common.Printf("usage: modulateLights <redFloat> <greenFloat> <blueFloat>\n"); return; }

            int i;
            var modulate = stackalloc float[3];
            for (i = 0; i < 3; i++) modulate[i] = floatX.Parse(args[i + 1]);

            var count = 0;
            for (i = 0; i < tr.primaryWorld.lightDefs.Count; i++)
            {
                var light = tr.primaryWorld.lightDefs[i];
                if (light != null)
                {
                    count++;
                    for (var j = 0; j < 3; j++) light.parms.shaderParms[j] *= modulate[j];
                }
            }
            common.Printf($"modulated {count} lights\n");
        }

        //======================================================================================

        // Creates all needed model references in portal areas, chaining them to both the area and the entityDef. Bumps tr.viewCount.
        public static void R_CreateEntityRefs(IRenderEntity def)
        {
            int i; Vector3 v;
            var transformed = stackalloc Vector3[8];

            if (def.parms.hModel == null) def.parms.hModel = renderModelManager.DefaultModel();

            // if the entity hasn't been fully specified due to expensive animation calcs for md5 and particles, use the provided conservative bounds.
            def.referenceBounds = def.parms.callback != null ? def.parms.bounds : def.parms.hModel.Bounds(def.parms);

            // some models, like empty particles, may not need to be added at all
            if (def.referenceBounds.IsCleared) return;

            if (r_showUpdates.Bool && (
                def.referenceBounds[1].x - def.referenceBounds[0].x > 1024 ||
                def.referenceBounds[1].y - def.referenceBounds[0].y > 1024))
                common.Printf($"big entityRef: {def.referenceBounds[1].x - def.referenceBounds[0].x},{def.referenceBounds[1].y - def.referenceBounds[0].y}\n");

            for (i = 0; i < 8; i++)
            {
                v.x = def.referenceBounds[i & 1][0];
                v.y = def.referenceBounds[(i >> 1) & 1][1];
                v.z = def.referenceBounds[(i >> 2) & 1][2];
                R_LocalPointToGlobal(def.modelMatrix, v, out transformed[i]);
            }

            // bump the view count so we can tell if an area already has a reference
            tr.viewCount++;

            // push these points down the BSP tree into areas
            def.world.PushVolumeIntoTree(def, null, 8, transformed);
        }

        //=================================================================================

        #region CREATE LIGHT REFS

        // All values are reletive to the origin
        // Assumes that right and up are not normalized
        // This is also called by dmap during map processing.
        public static void R_SetLightProject(Plane[] lightProject, in Vector3 origin, in Vector3 targetPoint, in Vector3 rightVector, in Vector3 upVector, in Vector3 start, in Vector3 stop)
        {
            float dist;
            float scale;
            float rLen, uLen;
            Vector3 normal;
            float ofs;
            Vector3 right, up;
            Vector3 startGlobal;
            Vector4 targetGlobal = default;

            right = rightVector;
            rLen = right.Normalize();
            up = upVector;
            uLen = up.Normalize();
            normal = up.Cross(right);
            //normal = right.Cross( up );
            normal.Normalize();

            dist = targetPoint * normal; //  - ( origin * normal );
            if (dist < 0) { dist = -dist; normal = -normal; }

            scale = (0.5f * dist) / rLen;
            right *= scale;
            scale = -(0.5f * dist) / uLen;
            up *= scale;

            lightProject[2] = normal; lightProject[2].d = -(origin * lightProject[2].Normal);
            lightProject[0] = right; lightProject[0].d = -(origin * lightProject[0].Normal);
            lightProject[1] = up; lightProject[1].d = -(origin * lightProject[1].Normal);

            // now offset to center
            targetGlobal.ToVec3() = targetPoint + origin;
            targetGlobal[3] = 1;
            ofs = 0.5f - (targetGlobal * lightProject[0].ToVec4()) / (targetGlobal * lightProject[2].ToVec4());
            lightProject[0].ToVec4() += ofs * lightProject[2].ToVec4();
            ofs = 0.5f - (targetGlobal * lightProject[1].ToVec4()) / (targetGlobal * lightProject[2].ToVec4());
            lightProject[1].ToVec4() += ofs * lightProject[2].ToVec4();

            // set the falloff vector
            normal = stop - start;
            dist = normal.Normalize();
            if (dist <= 0) dist = 1;
            lightProject[3] = normal * (1.0f / dist);
            startGlobal = start + origin;
            lightProject[3].d = -(startGlobal * lightProject[3].Normal);
        }

        // Creates plane equations from the light projection, positive sides face out of the light
        static void R_SetLightFrustum(Plane[] lightProject, Plane[] frustum)
        {
            int i;

            // we want the planes of s=0, s=q, t=0, and t=q
            frustum[0] = lightProject[0];
            frustum[1] = lightProject[1];
            frustum[2] = lightProject[2] - lightProject[0];
            frustum[3] = lightProject[2] - lightProject[1];

            // we want the planes of s=0 and s=1 for front and rear clipping planes
            frustum[4] = lightProject[3];

            frustum[5] = lightProject[3]; frustum[5].d -= 1.0f;
            frustum[5] = -frustum[5];

            for (i = 0; i < 6; i++) { frustum[i] = -frustum[i]; frustum[i].d /= frustum[i].Normalize(); }
        }

        static void R_FreeLightDefFrustum(IRenderLight ldef)
        {
            // free the frustum tris
            if (ldef.frustumTris != null)
            {
                R_FreeStaticTriSurf(ldef.frustumTris);
                ldef.frustumTris = null;
            }
            // free frustum windings
            for (var i = 0; i < 6; i++) if (ldef.frustumWindings[i] != null) ldef.frustumWindings[i] = null;
        }

        // Fills everything in based on light.parms
        public static void R_DeriveLightData(IRenderLight light)
        {
            int i;

            // decide which light shader we are going to use
            if (light.parms.shader != null) light.lightShader = light.parms.shader;
            if (light.lightShader == null) light.lightShader = declManager.FindMaterial(light.parms.pointLight ? "lights/defaultPointLight" : "lights/defaultProjectedLight");

            // get the falloff image
            light.falloffImage = light.lightShader.LightFalloffImage;
            if (light.falloffImage == null)
            {
                // use the falloff from the default shader of the correct type
                // projected lights by default don't diminish with distance
                var defaultShader = declManager.FindMaterial(light.parms.pointLight ? "lights/defaultPointLight" : "lights/defaultProjectedLight");
                light.falloffImage = defaultShader.LightFalloffImage;
            }

            // set the projection
            if (!light.parms.pointLight) R_SetLightProject(light.lightProject, Vector3.origin /*light.parms.origin*/, light.parms.target, light.parms.right, light.parms.up, light.parms.start, light.parms.end); // projected light
            else
            {
                // point light
                Array.Clear(light.lightProject, 0, light.lightProject.Length);
                light.lightProject[0].a = 0.5f / light.parms.lightRadius.x;
                light.lightProject[1].b = 0.5f / light.parms.lightRadius.y;
                light.lightProject[3].c = 0.5f / light.parms.lightRadius.z;
                light.lightProject[0].d = 0.5f;
                light.lightProject[1].d = 0.5f;
                light.lightProject[2].d = 1.0f;
                light.lightProject[3].d = 0.5f;
            }

            // set the frustum planes
            R_SetLightFrustum(light.lightProject, light.frustum);

            // rotate the light planes and projections by the axis
            R_AxisToModelMatrix(light.parms.axis, light.parms.origin, light.modelMatrix);

            for (i = 0; i < 6; i++) { var temp = light.frustum[i]; R_LocalPlaneToGlobal(light.modelMatrix, temp, out light.frustum[i]); }
            for (i = 0; i < 4; i++) { var temp = light.lightProject[i]; R_LocalPlaneToGlobal(light.modelMatrix, temp, out light.lightProject[i]); }

            // adjust global light origin for off center projections and parallel projections we are just faking parallel by making it a very far off center for now
            if (light.parms.parallel)
            {
                var dir = light.parms.lightCenter;
                if (dir.Normalize() == 0f) dir.z = 1f; // make point straight up if not specified
                light.globalLightOrigin = light.parms.origin + dir * 100000;
            }
            else light.globalLightOrigin = light.parms.origin + light.parms.axis * light.parms.lightCenter;

            R_FreeLightDefFrustum(light);

            light.frustumTris = R_PolytopeSurface(6, light.frustum, light.frustumWindings);

            // a projected light will have one shadowFrustum, a point light will have six unless the light center is outside the box
            R_MakeShadowFrustums(light);
        }

        const int MAX_LIGHT_VERTS = 40;
        public static void R_CreateLightRefs(IRenderLight light)
        {
            int i; SrfTriangles tri; var points = stackalloc Vector3[MAX_LIGHT_VERTS];

            tri = light.frustumTris;

            // because a light frustum is made of only six intersecting planes, we should never be able to get a stupid number of points...
            if (tri.numVerts > MAX_LIGHT_VERTS) common.Error($"R_CreateLightRefs: {tri.numVerts} points in frustumTris!");
            for (i = 0; i < tri.numVerts; i++) points[i] = tri.verts[i].xyz;

            if (R.r_showUpdates.Bool && (
                tri.bounds[1].x - tri.bounds[0].x > 1024 ||
                tri.bounds[1].y - tri.bounds[0].y > 1024))
                common.Printf($"big lightRef: {tri.bounds[1].x - tri.bounds[0].x},{tri.bounds[1].y - tri.bounds[0].y}\n");

            // determine the areaNum for the light origin, which may let us cull the light if it is behind a closed door
            // it is debatable if we want to use the entity origin or the center offset origin, but we definitely don't want to use a parallel offset origin
            light.areaNum = light.world.PointInArea(light.globalLightOrigin);
            if (light.areaNum == -1) light.areaNum = light.world.PointInArea(light.parms.origin);

            // bump the view count so we can tell if an area already has a reference
            tr.viewCount++;

            // if we have a prelight model that includes all the shadows for the major world occluders, we can limit the area references to those visible through the portals from the light center.
            // We can't do this in the normal case, because shadows are cast from back facing triangles, which may be in areas not directly visible to the light projection center.
            if (light.parms.prelightModel != null && R.r_useLightPortalFlow.Bool && light.lightShader.LightCastsShadows) light.world.FlowLightThroughPortals(light);
            // push these points down the BSP tree into areas
            else light.world.PushVolumeIntoTree(null, light, tri.numVerts, points);
        }

        // Called by the editor and dmap to operate on light volumes
        static void R_RenderLightFrustum(RenderLight renderLight, Plane[] lightFrustum)
        {
            RenderLightLocal fakeLight = new();

            fakeLight.parms = renderLight;

            R_DeriveLightData(fakeLight);

            R_FreeStaticTriSurf(fakeLight.frustumTris);

            for (var i = 0; i < 6; i++) lightFrustum[i] = fakeLight.frustum[i];
        }

        #endregion

        //=================================================================================

        static bool WindingCompletelyInsideLight(Winding w, IRenderLight ldef)
        {
            int i, j;

            for (i = 0; i < w.NumPoints; i++)
                for (j = 0; j < 6; j++)
                {
                    var d = w[i].ToVec3() * ldef.frustum[j].Normal + ldef.frustum[j].d;
                    if (d > 0f) return false;
                }
            return true;
        }

        // When a fog light is created or moved, see if it completely encloses any portals, which may allow them to be fogged closed.
        public static void R_CreateLightDefFogPortals(IRenderLight ldef)
        {
            AreaReference lref; PortalArea area;

            ldef.foggedPortals = null;

            if (!ldef.lightShader.IsFogLight) return;

            // some fog lights will explicitly disallow portal fogging
            if (ldef.lightShader.TestMaterialFlag(MF.NOPORTALFOG)) return;

            for (lref = ldef.references; lref != null; lref = lref.ownerNext)
            {
                // check all the models in this area
                area = lref.area;

                Portal prt; DoublePortal dp;
                for (prt = area.portals; prt != null; prt = prt.next)
                {
                    dp = prt.doublePortal;
                    // we only handle a single fog volume covering a portal this will never cause incorrect drawing, but it may fail to cull a portal
                    if (dp.fogLight != null) continue;
                    if (WindingCompletelyInsideLight(prt.w, ldef))
                    {
                        dp.fogLight = ldef;
                        dp.nextFoggedPortal = ldef.foggedPortals;
                        ldef.foggedPortals = dp;
                    }
                }
            }
        }

        // Frees all references and lit surfaces from the light
        public static void R_FreeLightDefDerivedData(IRenderLight ldef)
        {
            AreaReference lref, nextRef;

            // rmove any portal fog references
            for (DoublePortal dp = ldef.foggedPortals; dp != null; dp = dp.nextFoggedPortal) dp.fogLight = null;

            // free all the interactions
            while (ldef.firstInteraction != null) ldef.firstInteraction.UnlinkAndFree();

            // free all the references to the light
            for (lref = ldef.references; lref != null; lref = nextRef)
            {
                nextRef = lref.ownerNext;

                // unlink from the area
                lref.areaNext.areaPrev = lref.areaPrev;
                lref.areaPrev.areaNext = lref.areaNext;

                // put it back on the free list for reuse
                ldef.world.areaReferenceAllocator.Free(lref);
            }
            ldef.references = null;

            R_FreeLightDefFrustum(ldef);
        }

        // Used by both RE_FreeEntityDef and RE_UpdateEntityDef Does not actually free the entityDef.
        public static void R_FreeEntityDefDerivedData(IRenderEntity def, bool keepDecals, bool keepCachedDynamicModel)
        {
            int i;
            AreaReference next;

            // demo playback needs to free the joints, while normal play leaves them in the control of the game
            if (session.readDemo != null)
            {
                if (def.parms.joints != null) def.parms.joints = null;
                if (def.parms.callbackData != null) def.parms.callbackData = null;
                for (i = 0; i < MAX_RENDERENTITY_GUI; i++) if (def.parms.gui[i] != null) def.parms.gui[i] = null;
            }

            // free all the interactions
            while (def.firstInteraction != null) def.firstInteraction.UnlinkAndFree();

            // clear the dynamic model if present
            if (def.dynamicModel != null) def.dynamicModel = null;

            if (!keepDecals)
            {
                R_FreeEntityDefDecals(def);
                R_FreeEntityDefOverlay(def);
            }

            if (!keepCachedDynamicModel) def.cachedDynamicModel = null;

            // free the entityRefs from the areas
            for (var r = def.entityRefs; r != null; r = next)
            {
                next = r.ownerNext;

                // unlink from the area
                r.areaNext.areaPrev = r.areaPrev;
                r.areaPrev.areaNext = r.areaNext;

                // put it back on the free list for reuse
                def.world.areaReferenceAllocator.Free(r);
            }
            def.entityRefs = null;
        }

        // If we know the reference bounds stays the same, we only need to do this on entity update, not the full R_FreeEntityDefDerivedData
        public static void R_ClearEntityDefDynamicModel(IRenderEntity def)
        {
            // free all the interaction surfaces
            for (var inter = def.firstInteraction; inter != null && !inter.IsEmpty; inter = inter.entityNext) inter.FreeSurfaces();

            // clear the dynamic model if present
            if (def.dynamicModel != null) def.dynamicModel = null;
        }

        public static void R_FreeEntityDefDecals(RenderEntityLocal def)
        {
            while (def.decals != null)
            {
                var next = def.decals.Next();
                RenderModelDecal.Free(ref def.decals);
                def.decals = next;
            }
        }

        public static void R_FreeEntityDefFadedDecals(RenderEntityLocal def, int time)
            => def.decals = RenderModelDecal.RemoveFadedDecals(def.decals, time);

        public static void R_FreeEntityDefOverlay(RenderEntityLocal def)
        {
            if (def.overlay != null) { RenderModelOverlay.Free(def.overlay); def.overlay = null; }
        }

        // ReloadModels and RegenerateWorld call this
        // FIXME: need to do this for all worlds
        public static void R_FreeDerivedData()
        {
            int i, j;
            IRenderWorld rw;
            IRenderEntity def;
            IRenderLight light;

            for (j = 0; j < tr.worlds.Count; j++)
            {
                rw = tr.worlds[j];

                for (i = 0; i < rw.entityDefs.Count; i++)
                {
                    def = rw.entityDefs[i];
                    if (def == null) continue;
                    R_FreeEntityDefDerivedData(def, false, false);
                }

                for (i = 0; i < rw.lightDefs.Count; i++)
                {
                    light = rw.lightDefs[i];
                    if (light == null) continue;
                    R_FreeLightDefDerivedData(light);
                }
            }
        }

        public static void R_CheckForEntityDefsUsingModel(IRenderModel model)
        {
            int i, j;
            IRenderWorld rw;
            IRenderEntity def;

            for (j = 0; j < tr.worlds.Count; j++)
            {
                rw = tr.worlds[j];
                for (i = 0; i < rw.entityDefs.Count; i++)
                {
                    def = rw.entityDefs[i];
                    if (def == null) continue;
                    // this should never happen but Radiant messes it up all the time so just free the derived data
                    if (def.parms.hModel == model) R_FreeEntityDefDerivedData(def, false, false);
                }
            }
        }

        // ReloadModels and RegenerateWorld call this. FIXME: need to do this for all worlds
        public static void R_ReCreateWorldReferences()
        {
            int i, j;
            RenderWorldLocal rw;
            IRenderEntity def;
            IRenderLight light;

            // let the interaction generation code know this shouldn't be optimized for a particular view
            tr.viewDef = null;

            for (j = 0; j < tr.worlds.Count; j++)
            {
                rw = (RenderWorldLocal)tr.worlds[j];
                for (i = 0; i < rw.entityDefs.Count; i++)
                {
                    def = rw.entityDefs[i];
                    if (def == null) continue;
                    // the world model entities are put specifically in a single area, instead of just pushing their bounds into the tree
                    if (i < rw.numPortalAreas) rw.AddEntityRefToArea(def, rw.portalAreas[i]);
                    else R_CreateEntityRefs(def);
                }

                for (i = 0; i < rw.lightDefs.Count; i++)
                {
                    light = rw.lightDefs[i];
                    if (light == null) continue;
                    var parms = light.parms;
                    light.world.FreeLightDef(i);
                    rw.UpdateLightDef(i, parms);
                }
            }
        }

        // Frees and regenerates all references and interactions, which must be done when switching between display list mode and immediate mode
        public static void R_RegenerateWorld_f(CmdArgs args)
        {
            R_FreeDerivedData();
            // watch how much memory we allocate
            tr.staticAllocCount = 0;
            R_ReCreateWorldReferences();
            common.Printf($"Regenerated world, staticAllocCount = {tr.staticAllocCount}.\n");
        }
    }
}
