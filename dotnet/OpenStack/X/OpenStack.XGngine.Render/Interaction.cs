using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public unsafe partial class Interaction : IInteraction
    {
        // If we know that we are "off to the side" of an infinite shadow volume, we can draw it without caps in zpass mode
        static bool R_PotentiallyInsideInfiniteShadow(SrfTriangles occluder, in Vector3 localView, in Vector3 localLight)
        {
            Bounds exp = default;

            // expand the bounds to account for the near clip plane, because the view could be mathematically outside, but if the near clip plane chops a volume edge, the zpass rendering would fail.
            var znear = r_znear.Float;
            if (tr.viewDef.renderView.cramZNear) znear *= 0.25f;
            var stretch = znear * 2;  // in theory, should vary with FOV
            exp.b0.x = occluder.bounds.b0.x - stretch;
            exp.b0.y = occluder.bounds.b0.y - stretch;
            exp.b0.z = occluder.bounds.b0.z - stretch;
            exp.b1.x = occluder.bounds.b1.x + stretch;
            exp.b1.y = occluder.bounds.b1.y + stretch;
            exp.b1.z = occluder.bounds.b1.z + stretch;

            if (exp.ContainsPoint(localView)) return true;
            if (exp.ContainsPoint(localLight)) return true;

            // if the ray from localLight to localView intersects a face of the expanded bounds, we will be inside the projection
            var ray = localView - localLight;

            // intersect the ray from the view to the light with the near side of the bounds
            for (var axis = 0; axis < 3; axis++)
            {
                float d, frac; Vector3 hit;

                if (localLight[axis] < exp[0][axis])
                {
                    if (localView[axis] < exp[0][axis]) continue;
                    d = exp[0][axis] - localLight[axis];
                    frac = d / ray[axis];
                    hit = localLight + frac * ray;
                    hit[axis] = exp[0][axis];
                }
                else if (localLight[axis] > exp[1][axis])
                {
                    if (localView[axis] > exp[1][axis]) continue;
                    d = exp[1][axis] - localLight[axis];
                    frac = d / ray[axis];
                    hit = localLight + frac * ray;
                    hit[axis] = exp[1][axis];
                }
                else continue;

                if (exp.ContainsPoint(hit)) return true;
            }

            // the view is definitely not inside the projected shadow
            return false;
        }

        // makes sure all necessary light surfaces and shadow surfaces are created, and calls R_LinkLightSurf() for each one
        // If the model doesn't have any surfaces that need interactions with this type of light, it can be skipped, but we might need to instantiate the dynamic model to find out
        public void AddActiveInteraction()
        {
            ViewLight vLight;
            ViewEntity vEntity;
            ScreenRect shadowScissor, lightScissor;
            Vector3 localLightOrigin, localViewOrigin;

            vLight = lightDef.viewLight;
            vEntity = entityDef.viewEntity;

            // do not waste time culling the interaction frustum if there will be no shadows
            // use the entity scissor rectangle
            if (!HasShadows) shadowScissor = vEntity.scissorRect;  // culling does not seem to be worth it for static world models
                                                                   // use the light scissor rectangle
            else if (entityDef.parms.hModel.IsStaticWorldModel) shadowScissor = vLight.scissorRect;
            else
            {
                // try to cull the interaction this will also cull the case where the light origin is inside the view frustum and the entity bounds are outside the view frustum
                if (CullInteractionByViewFrustum(tr.viewDef.viewFrustum)) return;
                // calculate the shadow scissor rectangle
                shadowScissor = CalcInteractionScissorRectangle(tr.viewDef.viewFrustum);
            }

            // get out before making the dynamic model if the shadow scissor rectangle is empty
            if (shadowScissor.IsEmpty) return;

            // We will need the dynamic surface created to make interactions, even if the model itself wasn't visible.  This just returns a cached value after it has been generated once in the view.
            var model = R_EntityDefDynamicModel(entityDef);
            if (model == null || model.NumSurfaces <= 0) return;

            // the dynamic model may have changed since we built the surface list
            if (!IsDeferred && entityDef.dynamicModelFrameCount != dynamicModelFrameCount) FreeSurfaces();
            dynamicModelFrameCount = entityDef.dynamicModelFrameCount;

            // actually create the interaction if needed, building light and shadow surfaces as needed
            if (IsDeferred) CreateInteraction(model);

            fixed (float* _ = vEntity.modelMatrix) R_GlobalPointToLocal(_, lightDef.globalLightOrigin, out localLightOrigin);
            fixed (float* _ = vEntity.modelMatrix) R_GlobalPointToLocal(_, tr.viewDef.renderView.vieworg, out localViewOrigin);

            // calculate the scissor as the intersection of the light and model rects
            // this is used for light triangles, but not for shadow triangles
            lightScissor = vLight.scissorRect;
            lightScissor.Intersect(vEntity.scissorRect);

            var lightScissorsEmpty = lightScissor.IsEmpty;

            // for each surface of this entity / light interaction
            for (var i = 0; i < numSurfaces; i++)
            {
                var sint = surfaces[i];

                // see if the base surface is visible, we may still need to add shadows even if empty
                if (!lightScissorsEmpty && sint.ambientTris != null && sint.ambientTris.ambientViewCount == tr.viewCount)
                {
                    // make sure we have created this interaction, which may have been deferred on a previous use that only needed the shadow
                    if (sint.lightTris == LIGHT_TRIS_DEFERRED)
                    {
                        sint.lightTris = R_CreateLightTris(vEntity.entityDef, sint.ambientTris, vLight.lightDef, sint.shader, ref sint.cullInfo);
                        R_FreeInteractionCullInfo(sint.cullInfo);
                    }

                    var lightTris = sint.lightTris;
                    if (lightTris != null)
                    {
                        // try to cull before adding FIXME: this may not be worthwhile. We have already done culling on the ambient, but individual surfaces may still be cropped somewhat more
                        if (!R_CullLocalBox(lightTris.bounds, vEntity.modelMatrix, 5, tr.viewDef.frustum))
                        {
                            // make sure the original surface has its ambient cache created
                            if (!R_CreateAmbientCache(sint.ambientTris, sint.shader.ReceivesLighting)) continue; // skip if we were out of vertex memory reference the original surface's ambient cache GAB NOTE: we are in cache "reuse" mode
                            lightTris.ambientCache = sint.ambientTris.ambientCache;

                            // Even if we reuse the original surface ambient cache, we nevertheless need to compute a local index cache
                            if (!R_CreateIndexCache(lightTris)) continue; // skip if we were out of vertex memory

                            // touch the ambient surface so it won't get purged
                            vertexCache.Touch(lightTris.ambientCache);
                            vertexCache.Touch(lightTris.indexCache);

                            // add the surface to the light list

                            var shader = sint.shader;
                            R_GlobalShaderOverride(shader);

                            // there will only be localSurfaces if the light casts shadows and there are surfaces with NOSELFSHADOW
                            if (sint.shader.Coverage == MC.TRANSLUCENT) R_LinkLightSurf(ref vLight.translucentInteractions, lightTris, vEntity, lightDef, shader, lightScissor, false);
                            else if (!lightDef.parms.noShadows && sint.shader.TestMaterialFlag(MF.NOSELFSHADOW)) R_LinkLightSurf(ref vLight.localInteractions, lightTris, vEntity, lightDef, shader, lightScissor, false);
                            else R_LinkLightSurf(ref vLight.globalInteractions, lightTris, vEntity, lightDef, shader, lightScissor, false);
                        }
                    }
                }

                var shadowTris = sint.shadowTris;
                // the shadows will always have to be added, unless we can tell they are from a surface in an unconnected area
                if (shadowTris != null)
                {
                    // check for view specific shadow suppression (player shadows, etc)
                    if (!r_skipSuppress.Bool)
                    {
                        if (entityDef.parms.suppressShadowInViewID != 0 && entityDef.parms.suppressShadowInViewID == tr.viewDef.renderView.viewID) continue;
                        if (entityDef.parms.suppressShadowInLightID != 0 && entityDef.parms.suppressShadowInLightID == lightDef.parms.lightId) continue;
                    }

                    // cull static shadows that have a non-empty bounds. dynamic shadows that use the turboshadow code will not have valid bounds, because the perspective projection extends them to infinity
                    if (r_useShadowCulling.Bool && !shadowTris.bounds.IsCleared && R_CullLocalBox(shadowTris.bounds, vEntity.modelMatrix, 5, tr.viewDef.frustum)) continue;

                    // If the tri have shadowVertexes (eg. precomputed shadows)
                    if (shadowTris.shadowVertexes != null)
                    {
                        // Create its shadow cache
                        if (!R_CreatePrivateShadowCache(shadowTris)) continue; // skip if we were out of vertex memory
                                                                               // And its index cache
                        if (!R_CreateIndexCache(shadowTris)) continue; // skip if we were out of vertex memory
                    }
                    // Otherwise this is dynamic shadows
                    else
                    {
                        // Make sure the original surface has its shadow cache created
                        if (!R_CreateVertexProgramShadowCache(sint.ambientTris)) continue; // skip if we were out of vertex memory
                                                                                           // reference the original surface's shadow cache. GAB NOTE: we are in cache "reuse" mode
                        shadowTris.shadowCache = sint.ambientTris.shadowCache;

                        // Even if we reuse the original surface shadow cache, we nevertheless need to compute a local index cache
                        if (!R_CreateIndexCache(shadowTris)) continue; // skip if we were out of vertex memory
                    }

                    // In the end, touch the shadow surface so it won't get purged
                    vertexCache.Touch(shadowTris.shadowCache);
                    vertexCache.Touch(shadowTris.indexCache);

                    // see if we can avoid using the shadow volume caps
                    var inside = R_PotentiallyInsideInfiniteShadow(sint.ambientTris, localViewOrigin, localLightOrigin);

                    if (sint.shader.TestMaterialFlag(MF.NOSELFSHADOW)) R_LinkLightSurf(ref vLight.localShadows, shadowTris, vEntity, lightDef, null, shadowScissor, inside);
                    else R_LinkLightSurf(ref vLight.globalShadows, shadowTris, vEntity, lightDef, null, shadowScissor, inside);
                }
            }
        }


        // actually create the interaction
        // Called when a entityDef and a lightDef are both present in a portalArea, and might be visible.Performs cull checking before doing the expensive computations.
        // References tr.viewCount so lighting surfaces will only be created if the ambient surface is visible, otherwise it will be marked as deferred.
        // The results of this are cached and valid until the light or entity change.
        void CreateInteraction(IRenderModel model)
        {
            Material lightShader = lightDef.lightShader;
            Material shader;
            bool interactionGenerated;
            Bounds bounds;

            tr.pc.c_createInteractions++;

            bounds = model.Bounds(entityDef.parms);

            // if it doesn't contact the light frustum, none of the surfaces will
            if (R_CullLocalBox(bounds, entityDef.modelMatrix, 6, lightDef.frustum)) { MakeEmpty(); return; }

            // use the turbo shadow path
            ShadowGen shadowGen = SG_DYNAMIC;

            // really large models, like outside terrain meshes, should use the more exactly culled static shadow path instead of the turbo shadow path. FIXME: this is a HACK, we should probably have a material flag.
            if (bounds[1].x - bounds[0].x > 3000) shadowGen = SG_STATIC;

            // create slots for each of the model's surfaces
            numSurfaces = model.NumSurfaces;
            surfaces = new SurfaceInteraction[numSurfaces];

            interactionGenerated = false;

            // check each surface in the model
            for (var c = 0; c < model.NumSurfaces; c++)
            {
                ModelSurface surf;
                SrfTriangles tri;

                surf = model.Surface(c);

                tri = surf.geometry;
                if (tri == null) continue;

                // determine the shader for this surface, possibly by skinning
                shader = surf.shader;
                shader = R_RemapShaderBySkin(shader, entityDef.parms.customSkin, entityDef.parms.customShader);
                if (shader == null) continue;

                // try to cull each surface
                if (R_CullLocalBox(tri.bounds, entityDef.modelMatrix, 6, lightDef.frustum)) continue;

                var sint = surfaces[c];
                sint.shader = shader;

                // save the ambient tri pointer so we can reject lightTri interactions when the ambient surface isn't in view, and we can get shared vertex and shadow data from the source surface
                sint.ambientTris = tri;

                // "invisible ink" lights and shaders
                if (shader.Spectrum != lightShader.Spectrum) continue;

                // generate a lighted surface and add it
                if (shader.ReceivesLighting)
                {
                    sint.lightTris = tri.ambientViewCount == tr.viewCount
                        // this will be calculated when sint.ambientTris is actually in view
                        ? R_CreateLightTris(entityDef, tri, lightDef, shader, ref sint.cullInfo)
                        : LIGHT_TRIS_DEFERRED;
                    interactionGenerated = true;
                }

                // if the interaction has shadows and this surface casts a shadow
                if (HasShadows && shader.SurfaceCastsShadow && tri.silEdges != null)
                {
                    // if the light has an optimized shadow volume, don't create shadows for any models that are part of the base areas
                    if (lightDef.parms.prelightModel == null || !model.IsStaticWorldModel || !r_useOptimizedShadows.Bool)
                    {
                        // this is the only place during gameplay (outside the utilities) that R_CreateShadowVolume() is called
                        sint.shadowTris = R_CreateShadowVolume(entityDef, tri, lightDef, shadowGen, sint.cullInfo);
                        if (sint.shadowTris != null)
                            if (shader.Coverage != MC.OPAQUE || (!r_skipSuppress.Bool && entityDef.parms.suppressSurfaceInViewID != 0))
                            {
                                // if any surface is a shadow-casting perforated or translucent surface, or the base surface is suppressed in the view (world weapon shadows) we can't use
                                // the external shadow optimizations because we can see through some of the faces
                                sint.shadowTris.numShadowIndexesNoCaps = sint.shadowTris.numIndexes;
                                sint.shadowTris.numShadowIndexesNoFrontCaps = sint.shadowTris.numIndexes;
                            }
                        interactionGenerated = true;
                    }
                }

                // free the cull information when it's no longer needed
                if (sint.lightTris != LIGHT_TRIS_DEFERRED) R_FreeInteractionCullInfo(sint.cullInfo);
            }

            // if none of the surfaces generated anything, don't even bother checking?
            if (!interactionGenerated) MakeEmpty();
        }

        // try to determine if the entire interaction, including shadows, is guaranteed to be outside the view frustum
        static Vector4[] CullInteractionByViewFrustum_colors = { colorRed, colorGreen, colorBlue, colorYellow, colorMagenta, colorCyan, colorWhite, colorPurple };

        bool CullInteractionByViewFrustum(Frustum viewFrustum)
        {
            if (!r_useInteractionCulling.Bool) return false;
            if (frustumState == FrustumState.FRUSTUM_INVALID) return false;
            if (frustumState == FrustumState.FRUSTUM_UNINITIALIZED)
            {
                frustum.FromProjection(new Box(entityDef.referenceBounds, entityDef.parms.origin, entityDef.parms.axis), lightDef.globalLightOrigin, MAX_WORLD_SIZE);
                if (!frustum.IsValid) { frustumState = FrustumState.FRUSTUM_INVALID; return false; }
                frustum.ConstrainToBox(lightDef.parms.pointLight
                    ? new Box(lightDef.parms.origin, lightDef.parms.lightRadius, lightDef.parms.axis)
                    : new Box(lightDef.frustumTris.bounds));
                frustumState = FrustumState.FRUSTUM_VALID;
            }
            if (!viewFrustum.IntersectsFrustum(frustum)) return true;
            if (r_showInteractionFrustums.Integer != 0)
            {
                tr.viewDef.renderWorld.DebugFrustum(CullInteractionByViewFrustum_colors[lightDef.index & 7], frustum, r_showInteractionFrustums.Integer > 1);
                if (r_showInteractionFrustums.Integer > 2) tr.viewDef.renderWorld.DebugBox(colorWhite, new Box(entityDef.referenceBounds, entityDef.parms.origin, entityDef.parms.axis));
            }
            return false;
        }

        // determine the minimum scissor rect that will include the interaction shadows projected to the bounds of the light
        ScreenRect CalcInteractionScissorRectangle(Frustum viewFrustum)
        {
            Bounds projectionBounds = default; ScreenRect portalRect = default, scissorRect;

            if (r_useInteractionScissors.Integer == 0) return lightDef.viewLight.scissorRect;

            // this is the code from Cass at nvidia, it is more precise, but slower
            if (r_useInteractionScissors.Integer < 0) return R_CalcIntersectionScissor(lightDef, entityDef, tr.viewDef);

            // frustum must be initialized and valid
            if (frustumState == FrustumState.FRUSTUM_UNINITIALIZED || frustumState == FrustumState.FRUSTUM_INVALID) return lightDef.viewLight.scissorRect;

            // calculate scissors for the portals through which the interaction is visible
            if (r_useInteractionScissors.Integer > 1)
            {
                AreaNumRef area;
                if (frustumState == FrustumState.FRUSTUM_VALID)
                {
                    // retrieve all the areas the interaction frustum touches
                    for (var r = entityDef.entityRefs; r != null; r = r.ownerNext)
                    {
                        area = entityDef.world.areaNumRefAllocator.Alloc();
                        area.areaNum = r.area.areaNum;
                        area.next = frustumAreas;
                        frustumAreas = area;
                    }
                    frustumAreas = tr.viewDef.renderWorld.FloodFrustumAreas(frustum, frustumAreas);
                    frustumState = FrustumState.FRUSTUM_VALIDAREAS;
                }

                portalRect.Clear();
                for (area = frustumAreas; area != null; area = area.next) portalRect.Union(entityDef.world.GetAreaScreenRect(area.areaNum));
                portalRect.Intersect(lightDef.viewLight.scissorRect);
            }
            else portalRect = lightDef.viewLight.scissorRect;

            // early out if the interaction is not visible through any portals
            if (portalRect.IsEmpty) return portalRect;

            // calculate bounds of the interaction frustum projected into the view frustum
            viewFrustum.ClippedProjectionBounds(frustum, lightDef.parms.pointLight
                ? new Box(lightDef.parms.origin, lightDef.parms.lightRadius, lightDef.parms.axis)
                : new Box(lightDef.frustumTris.bounds), projectionBounds);

            if (projectionBounds.IsCleared) return portalRect;

            // derive a scissor rectangle from the projection bounds
            scissorRect = R_ScreenRectFromViewFrustumBounds(projectionBounds);

            // intersect with the portal crossing scissor rectangle
            scissorRect.Intersect(portalRect);

            if (r_showInteractionScissors.Integer > 0) R_ShowColoredScreenRect(scissorRect, lightDef.index);

            return scissorRect;
        }
    }
}