using System.NumericsX.OpenStack.Gngine.UI;
using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        const float CHECK_BOUNDS_EPSILON = 1f;

        // Create it if needed, or return the existing one
        public static bool R_CreateAmbientCache(SrfTriangles tri, bool needsLighting)
        {
            // Check for errors
            if (tri.verts == null) { common.Error("R_CreateAmbientCache: Tri have no vertices\n"); return false; }
            // If there is no ambient cache, let's compute it
            else if (tri.ambientCache == null)
            {
                // we are going to use it for drawing, so make sure we have the tangents and normals
                if (needsLighting && !tri.tangentsCalculated) R_DeriveTangents(tri);

                // Build the ambient cache
                fixed (DrawVert* _ = tri.verts) vertexCache.Alloc(_, tri.numVerts * sizeof(DrawVert), out tri.ambientCache, false);

                // Check for errors
                if (tri.ambientCache == null) { common.Error("R_CreateAmbientCache: Unable to create an ambient cache\n"); return false; }
            }
            else { } // There is already an ambiant cache. Let's reuse it.
            return true;
        }

        // This is used only for a specific light
        public static bool R_CreateIndexCache(SrfTriangles tri)
        {
            // Check for errors
            if (tri.indexes == null) { common.Error("R_CreateIndexCache: Tri have no indices\n"); return false; }
            // If there is no index cache, let's compute it
            else if (tri.indexCache == null)
            {
                // Build the index cache
                fixed (GlIndex* _ = tri.indexes) vertexCache.Alloc(_, tri.numIndexes * sizeof(GlIndex), out tri.indexCache, true);

                // Check for errors
                if (tri.indexCache == null) { common.Error("R_CreateIndexCache: Unable to create an index cache\n"); return false; }
            }
            else { } // There is already an index cache. Let's reuse it.
            return true;
        }

        // This is used only for a specific light
        public static bool R_CreatePrivateShadowCache(SrfTriangles tri)
        {
            // Check for errors
            if (tri.shadowVertexes == null) { common.Error("R_CreatePrivateShadowCache: Tri have no shadow vertices\n"); return false; }
            // If there is no ambient cache, let's compute it
            else if (tri.shadowCache == null)
            {
                // Build the shadow cache
                fixed (ShadowCache* _ = tri.shadowVertexes) vertexCache.Alloc(_, tri.numVerts * sizeof(ShadowCache), out tri.shadowCache, false);

                // Check for errors
                if (tri.shadowCache == null) { common.Error("R_CreatePrivateShadowCache: Unable to create a vertex cache\n"); return false; }
            }
            else { } // There is already a shadow cache. Let's reuse it.
            return true;
        }

        // This is constant for any number of lights, the vertex program takes care of projecting the verts to infinity.
        public static bool R_CreateVertexProgramShadowCache(SrfTriangles tri)
        {
            // Check for errors
            if (tri.verts == null) { common.Error("R_CreateVertexProgramShadowCache: Tri have no vertices\n"); return false; }
            // If there is no shadow cache, let's compute it
            else if (tri.shadowCache == null)
            {
                // Build the temporary precomputed shadow vertices
                var temp = stackalloc ShadowCache[(tri.numVerts * 2) + ShadowCache.ALLOC16]; temp = (ShadowCache*)_alloca16(temp);
                fixed (DrawVert* _ = tri.verts) Simd.CreateVertexProgramShadowCache(&temp->xyz, _, tri.numVerts);

                // Build the shadow cache
                vertexCache.Alloc(temp, tri.numVerts * 2 * sizeof(ShadowCache), out tri.shadowCache, false);

                // Check for errors
                if (tri.shadowCache == null) { common.Error("R_CreateVertexProgramShadowCache: Unable to create a vertex cache\n"); return false; }
            }
            else { } // There is already a shadow cache. Let's reuse it.
            return true;
        }

        static void R_WobbleskyTexGen(DrawSurf surf, Vector3 viewOrg)
        {
            var parms = surf.material.TexGenRegisters;

            float wobbleDegrees = surf.shaderRegisters[parms[0]],
                wobbleSpeed = surf.shaderRegisters[parms[1]],
                rotateSpeed = surf.shaderRegisters[parms[2]];

            wobbleDegrees = wobbleDegrees * MathX.PI / 180;
            wobbleSpeed = wobbleSpeed * 2 * MathX.PI / 60;
            rotateSpeed = rotateSpeed * 2 * MathX.PI / 60;

            // very ad-hoc "wobble" transform
            float a = tr.viewDef.floatTime * wobbleSpeed,
                s = (float)Math.Sin(a) * (float)Math.Sin(wobbleDegrees),
                c = (float)Math.Cos(a) * (float)Math.Sin(wobbleDegrees),
                z = (float)Math.Cos(wobbleDegrees);

            var axis = stackalloc Vector3[3];

            axis[2].x = c; axis[2].y = s; axis[2].z = z;

            axis[1].x = -(float)Math.Sin(a * 2) * (float)Math.Sin(wobbleDegrees);
            axis[1].z = -s * (float)Math.Sin(wobbleDegrees);
            axis[1].y = (float)Math.Sqrt(1f - (axis[1].x * axis[1].x + axis[1].z * axis[1].z));

            // make the second vector exactly perpendicular to the first
            axis[1] -= (axis[2] * axis[1]) * axis[2];
            axis[1].Normalize();

            // construct the third with a cross
            axis[0].Cross(axis[1], axis[2]);

            // add the rotate
            s = (float)Math.Sin(rotateSpeed * tr.viewDef.floatTime);
            c = (float)Math.Cos(rotateSpeed * tr.viewDef.floatTime);

            surf.wobbleTransform[0] = axis[0].x * c + axis[1].x * s;
            surf.wobbleTransform[4] = axis[0].y * c + axis[1].y * s;
            surf.wobbleTransform[8] = axis[0].z * c + axis[1].z * s;

            surf.wobbleTransform[1] = axis[1].x * c - axis[0].x * s;
            surf.wobbleTransform[5] = axis[1].y * c - axis[0].y * s;
            surf.wobbleTransform[9] = axis[1].z * c - axis[0].z * s;

            surf.wobbleTransform[2] = axis[2].x;
            surf.wobbleTransform[6] = axis[2].y;
            surf.wobbleTransform[10] = axis[2].z;

            surf.wobbleTransform[3] = surf.wobbleTransform[7] = surf.wobbleTransform[11] = 0f;
            surf.wobbleTransform[12] = surf.wobbleTransform[13] = surf.wobbleTransform[14] = 0f;
        }

        // If the entityDef isn't already on the viewEntity list, create a viewEntity and add it to the list with an empty scissor rect.
        // This does not instantiate dynamic models for the entity yet.
        public static ViewEntity R_SetEntityDefViewEntity(IRenderEntity def)
        {
            ViewEntity vModel;

            if (def.viewCount == tr.viewCount) return def.viewEntity;
            def.viewCount = tr.viewCount;

            // set the model and modelview matricies
            vModel = R_ClearedFrameAllocT<ViewEntity>();
            vModel.entityDef = def;

            // the scissorRect will be expanded as the model bounds is accepted into visible portal chains
            vModel.scissorRect.Clear();

            // copy the model and weapon depth hack for back-end use
            vModel.modelDepthHack = def.parms.modelDepthHack;
            vModel.weaponDepthHack = def.parms.weaponDepthHack;

            R_AxisToModelMatrix(def.parms.axis, def.parms.origin, vModel.modelMatrix);

            // we may not have a viewDef if we are just creating shadows at entity creation time
            if (tr.viewDef != null)
                fixed (float* a = vModel.modelMatrix)
                {
                    fixed (float* b = tr.viewDef.worldSpace.u.eyeViewMatrix0, c = vModel.u.eyeViewMatrix0) myGlMultMatrix(a, b, c);
                    fixed (float* b = tr.viewDef.worldSpace.u.eyeViewMatrix1, c = vModel.u.eyeViewMatrix1) myGlMultMatrix(a, b, c);
                    fixed (float* b = tr.viewDef.worldSpace.u.eyeViewMatrix2, c = vModel.u.eyeViewMatrix2) myGlMultMatrix(a, b, c);

                    vModel.next = tr.viewDef.viewEntitys;
                    tr.viewDef.viewEntitys = vModel;
                }

            def.viewEntity = vModel;

            return vModel;
        }

        const float INSIDE_LIGHT_FRUSTUM_SLOP = 32;

        // this needs to be greater than the dist from origin to corner of near clip plane
        static bool R_TestPointInViewLight(Vector3 org, IRenderLight light)
        {
            for (var i = 0; i < 6; i++)
            {
                var d = light.frustum[i].Distance(org);
                if (d > INSIDE_LIGHT_FRUSTUM_SLOP) return false;
            }
            return true;
        }

        // Assumes positive sides face outward
        static bool R_PointInFrustum(Vector3 p, Plane[] planes, int numPlanes)
        {
            for (var i = 0; i < numPlanes; i++)
            {
                var d = planes[i].Distance(p);
                if (d > 0) return false;
            }
            return true;
        }

        // If the lightDef isn't already on the viewLight list, create a viewLight and add it to the list with an empty scissor rect.
        public static ViewLight R_SetLightDefViewLight(IRenderLight light)
        {
            ViewLight vLight;

            if (light.viewCount == tr.viewCount) return light.viewLight;
            light.viewCount = tr.viewCount;

            // add to the view light chain
            vLight = R_ClearedFrameAllocT<ViewLight>();
            vLight.lightDef = light;

            // the scissorRect will be expanded as the light bounds is accepted into visible portal chains
            vLight.scissorRect.Clear();

            // calculate the shadow cap optimization states
            vLight.viewInsideLight = R_TestPointInViewLight(tr.viewDef.renderView.vieworg, light);
            if (!vLight.viewInsideLight)
            {
                vLight.viewSeesShadowPlaneBits = 0;
                for (var i = 0; i < light.numShadowFrustums; i++)
                {
                    var d = light.shadowFrustums[i].planes[5].Distance(tr.viewDef.renderView.vieworg);
                    if (d < INSIDE_LIGHT_FRUSTUM_SLOP) vLight.viewSeesShadowPlaneBits |= 1 << i;
                }
            }
            // this should not be referenced in this case
            else vLight.viewSeesShadowPlaneBits = 63;

            // see if the light center is in view, which will allow us to cull invisible shadows
            vLight.viewSeesGlobalLightOrigin = R_PointInFrustum(light.globalLightOrigin, tr.viewDef.frustum, 4);

            // copy data used by backend
            vLight.globalLightOrigin = light.globalLightOrigin;
            vLight.lightProject[0] = light.lightProject[0];
            vLight.lightProject[1] = light.lightProject[1];
            vLight.lightProject[2] = light.lightProject[2];
            vLight.lightProject[3] = light.lightProject[3];
            vLight.fogPlane = light.frustum[5];
            vLight.frustumTris = light.frustumTris;
            vLight.falloffImage = light.falloffImage;
            vLight.lightShader = light.lightShader;
            vLight.shaderRegisters = null;    // allocated and evaluated in R_AddLightSurfaces

            // link the view light
            vLight.next = tr.viewDef.viewLights;
            tr.viewDef.viewLights = vLight;

            light.viewLight = vLight;

            return vLight;
        }

        //===============================================================================================================

        public static void R_LinkLightSurf(ref DrawSurf link, SrfTriangles tri, ViewEntity space, IRenderLight light, Material shader, ScreenRect scissor, bool viewInsideShadow)
        {
            DrawSurf drawSurf;

            if (space == null) space = tr.viewDef.worldSpace;

            drawSurf = new DrawSurf();
            drawSurf.geoFrontEnd = tri;
            drawSurf.ambientCache = tri.ambientCache;
            drawSurf.indexCache = tri.indexCache;
            drawSurf.shadowCache = tri.shadowCache;
            drawSurf.numIndexes = tri.numIndexes;

            drawSurf.numShadowIndexesNoFrontCaps = tri.numShadowIndexesNoFrontCaps;
            drawSurf.numShadowIndexesNoCaps = tri.numShadowIndexesNoCaps;
            drawSurf.shadowCapPlaneBits = tri.shadowCapPlaneBits;

            drawSurf.space = space;
            drawSurf.material = shader;

            drawSurf.scissorRect = scissor;
            drawSurf.dsFlags = 0;
            if (viewInsideShadow) drawSurf.dsFlags |= DrawSurf.DSF_VIEW_INSIDE_SHADOW;

            // shadows won't have a shader
            if (shader == null) drawSurf.shaderRegisters = null;
            else
            {
                // process the shader expressions for conditionals / color / texcoords
                var constRegs = shader.ConstantRegisters();
                // this shader has only constants for parameters
                if (constRegs != null) drawSurf.shaderRegisters = constRegs;
                else
                {
                    var regs = new float[shader.NumRegisters]; // FIXME: share with the ambient surface?
                    drawSurf.shaderRegisters = regs;
                    shader.EvaluateRegisters(regs, space.entityDef.parms.shaderParms, tr.viewDef, space.entityDef.parms.referenceSound);
                }
            }

            // actually link it in
            drawSurf.nextOnLight = link; link = drawSurf;
        }

        static ScreenRect R_ClippedLightScissorRectangle(ViewLight vLight)
        {
            int i, j;
            IRenderLight light = vLight.lightDef;
            ScreenRect r = new();
            FixedWinding w;

            r.Clear();

            for (i = 0; i < 6; i++)
            {
                var ow = light.frustumWindings[i];

                // projected lights may have one of the frustums degenerated
                if (ow == null) continue;

                // the light frustum planes face out from the light, so the planes that have the view origin on the negative
                // side will be the "back" faces of the light, which must have some fragment inside the portalStack to be visible
                if (light.frustum[i].Distance(tr.viewDef.renderView.vieworg) >= 0) continue;

                w = (FixedWinding)ow;

                // now check the winding against each of the frustum planes
                for (j = 0; j < 5; j++) if (!w.ClipInPlace(-tr.viewDef.frustum[j])) break;

                // project these points to the screen and add to bounds
                for (j = 0; j < w.NumPoints; j++)
                {
                    Plane eye, clip;
                    fixed (float* _ = tr.viewDef.worldSpace.u.eyeViewMatrix2) R_TransformModelToClip(w[j].ToVec3(), _, tr.viewDef.projectionMatrix, out eye, out clip);

                    if (clip.d <= 0.01f) clip.d = 0.01f;

                    R_TransformClipToDevice(clip, tr.viewDef, out var ndc);

                    var windowX = 0.5f * (1f + ndc.x) * (tr.viewDef.viewport.x2 - tr.viewDef.viewport.x1);
                    var windowY = 0.5f * (1f + ndc.y) * (tr.viewDef.viewport.y2 - tr.viewDef.viewport.y1);

                    if (windowX > tr.viewDef.scissor.x2) windowX = tr.viewDef.scissor.x2;
                    else if (windowX < tr.viewDef.scissor.x1) windowX = tr.viewDef.scissor.x1;
                    if (windowY > tr.viewDef.scissor.y2) windowY = tr.viewDef.scissor.y2;
                    else if (windowY < tr.viewDef.scissor.y1) windowY = tr.viewDef.scissor.y1;

                    r.AddPoint(windowX, windowY);
                }
            }

            // add the fudge boundary
            r.Expand();

            return r;
        }

        // The light screen bounds will be used to crop the scissor rect during stencil clears and interaction drawing
        static int c_clippedLight, c_unclippedLight;

        static ScreenRect R_CalcLightScissorRectangle(ViewLight vLight)
        {
            if (vLight.lightDef.parms.pointLight)
            {
                Bounds bounds = default;
                var lightDef = vLight.lightDef;
                tr.viewDef.viewFrustum.ProjectionBounds(new Box(lightDef.parms.origin, lightDef.parms.lightRadius, lightDef.parms.axis), bounds);
                return R_ScreenRectFromViewFrustumBounds(bounds);
            }

            if (r_useClippedLightScissors.Integer == 2) return R_ClippedLightScissorRectangle(vLight);

            ScreenRect r = default;
            r.Clear();

            var tri = vLight.lightDef.frustumTris;
            for (var i = 0; i < tri.numVerts; i++)
            {
                Plane eye, clip;
                fixed (float* _ = tr.viewDef.worldSpace.u.eyeViewMatrix2) R_TransformModelToClip(tri.verts[i].xyz, _, tr.viewDef.projectionMatrix, out eye, out clip);

                // if it is near clipped, clip the winding polygons to the view frustum
                if (clip.d <= 1)
                {
                    c_clippedLight++;
                    if (r_useClippedLightScissors.Integer != 0) return R_ClippedLightScissorRectangle(vLight);
                    else
                    {
                        r.x1 = r.y1 = 0;
                        r.x2 = (short)((tr.viewDef.viewport.x2 - tr.viewDef.viewport.x1) - 1);
                        r.y2 = (short)((tr.viewDef.viewport.y2 - tr.viewDef.viewport.y1) - 1);
                        return r;
                    }
                }

                R_TransformClipToDevice(clip, tr.viewDef, out var ndc);

                var windowX = 0.5f * (1f + ndc.x) * (tr.viewDef.viewport.x2 - tr.viewDef.viewport.x1);
                var windowY = 0.5f * (1f + ndc.y) * (tr.viewDef.viewport.y2 - tr.viewDef.viewport.y1);

                if (windowX > tr.viewDef.scissor.x2) windowX = tr.viewDef.scissor.x2;
                else if (windowX < tr.viewDef.scissor.x1) windowX = tr.viewDef.scissor.x1;
                if (windowY > tr.viewDef.scissor.y2) windowY = tr.viewDef.scissor.y2;
                else if (windowY < tr.viewDef.scissor.y1) windowY = tr.viewDef.scissor.y1;

                r.AddPoint(windowX, windowY);
            }

            // add the fudge boundary
            r.Expand();

            c_unclippedLight++;

            return r;
        }

        // Calc the light shader values, removing any light from the viewLight list if it is determined to not have any visible effect due to being flashed off or turned off.
        // Adds entities to the viewEntity list if they are needed for shadow casting. Add any precomputed shadow volumes.
        // Removes lights from the viewLights list if they are completely turned off, or completely off screen. Create any new interactions needed between the viewLights and the viewEntitys due to game movement
        static Random R_AddLightSurfaces_random = new();
        public static void R_AddLightSurfaces()
        {
            ViewLight vLight;
            IRenderLight light;
            ViewLight ptr;

            // go through each visible light, possibly removing some from the list
            ptr = tr.viewDef.viewLights;
            while (ptr != null)
            {
                vLight = ptr;
                light = vLight.lightDef;

                var lightShader = light.lightShader;
                if (lightShader == null) common.Error("R_AddLightSurfaces: NULL lightShader");

                // see if we are suppressing the light in this view
                if (!r_skipSuppress.Bool)
                {
                    if (light.parms.suppressLightInViewID != 0 && light.parms.suppressLightInViewID == tr.viewDef.renderView.viewID)
                    {
                        ptr = vLight.next;
                        light.viewCount = -1;
                        continue;
                    }
                    if (light.parms.allowLightInViewID != 0 && light.parms.allowLightInViewID != tr.viewDef.renderView.viewID)
                    {
                        ptr = vLight.next;
                        light.viewCount = -1;
                        continue;
                    }
                }

                // evaluate the light shader registers
                var lightRegs = new float[lightShader.NumRegisters];
                vLight.shaderRegisters = lightRegs;
                lightShader.EvaluateRegisters(lightRegs, light.parms.shaderParms, tr.viewDef, light.parms.referenceSound);

                // if this is a purely additive light and no stage in the light shader evaluates to a positive light value, we can completely skip the light
                if (!lightShader.IsFogLight && !lightShader.IsBlendLight)
                {
                    int lightStageNum;
                    for (lightStageNum = 0; lightStageNum < lightShader.NumStages; lightStageNum++)
                    {
                        var lightStage = lightShader.GetStage(lightStageNum);

                        // ignore stages that fail the condition
                        if (lightRegs[lightStage.conditionRegister] == 0) continue;

                        fixed (int* registers = lightStage.color.registers)
                        {
                            // snap tiny values to zero to avoid lights showing up with the wrong color
                            if (lightRegs[registers[0]] < 0.001f) lightRegs[registers[0]] = 0f;
                            if (lightRegs[registers[1]] < 0.001f) lightRegs[registers[1]] = 0f;
                            if (lightRegs[registers[2]] < 0.001f) lightRegs[registers[2]] = 0f;

                            // FIXME:	when using the following values the light shows up bright red when using nvidia drivers/hardware this seems to have been fixed ?
                            //lightRegs[registers[0]] = 1.5143074e-005f;
                            //lightRegs[registers[1]] = 1.5483369e-005f;
                            //lightRegs[registers[2]] = 1.7014690e-005f;

                            if (lightRegs[registers[0]] > 0f ||
                                lightRegs[registers[1]] > 0f ||
                                lightRegs[registers[2]] > 0f) break;
                        }
                    }
                    if (lightStageNum == lightShader.NumStages)
                    {
                        // we went through all the stages and didn't find one that adds anything remove the light from the viewLights list, and change its frame marker
                        // so interaction generation doesn't think the light is visible and create a shadow for it
                        ptr = vLight.next;
                        light.viewCount = -1;
                        continue;
                    }
                }

                if (r_useLightScissors.Bool)
                {
                    // calculate the screen area covered by the light frustum which will be used to crop the stencil cull
                    var scissorRect = R_CalcLightScissorRectangle(vLight);
                    // intersect with the portal crossing scissor rectangle
                    vLight.scissorRect.Intersect(scissorRect);

                    if (r_showLightScissors.Bool) R_ShowColoredScreenRect(vLight.scissorRect, light.index);
                }

#if false
                // this never happens, because CullLightByPortals() does a more precise job
                // this light doesn't touch anything on screen, so remove it from the list
                if (vLight.scissorRect.IsEmpty) { *ptr = vLight.next; continue; }
#endif

                // this one stays on the list
                ptr = vLight.next;

                // if we are doing a soft-shadow novelty test, regenerate the light with a random offset every time
                if (r_lightSourceRadius.Float != 0f)
                    for (var i = 0; i < 3; i++) light.globalLightOrigin[i] += r_lightSourceRadius.Float * (-1 + 2 * (R_AddLightSurfaces_random.Next() & 0xfff) / (float)0xfff);

                // create interactions with all entities the light may touch, and add viewEntities that may cast shadows, even if they aren't directly visible.  Any real work
                // will be deferred until we walk through the viewEntities
                tr.viewDef.renderWorld.CreateLightDefInteractions(light);
                tr.pc.c_viewLights++;

                // fog lights will need to draw the light frustum triangles, so make sure they
                // are in the vertex cache
                if (lightShader.IsFogLight)
                {
                    if (!R_CreateAmbientCache(light.frustumTris, false) || !R_CreateIndexCache(light.frustumTris)) continue; // skip if we are out of vertex memory
                    // touch the surface so it won't get purged
                    vertexCache.Touch(light.frustumTris.ambientCache);
                    vertexCache.Touch(light.frustumTris.indexCache);
                }

                // add the prelight shadows for the static world geometry
                if (light.parms.prelightModel != null && r_useOptimizedShadows.Bool)
                {
                    if (light.parms.prelightModel.NumSurfaces == 0) common.Error($"no surfs in prelight model '{light.parms.prelightModel.Name}'");

                    var tri = light.parms.prelightModel.Surface(0).geometry;
                    if (tri.shadowVertexes == null) common.Error($"R_AddLightSurfaces: prelight model '{light.parms.prelightModel.Name}' without shadowVertexes");

                    // these shadows will all have valid bounds, and can be culled normally
                    if (r_useShadowCulling.Bool && R_CullLocalBox(tri.bounds, tr.viewDef.worldSpace.modelMatrix, 5, tr.viewDef.frustum)) continue;

                    // if we have been purged, re-upload the shadowVertexes
                    if (!R_CreatePrivateShadowCache(tri) || !R_CreateIndexCache(tri)) continue; // skip if we are out of vertex memory

                    // touch the shadow surface so it won't get purged
                    vertexCache.Touch(tri.shadowCache);
                    vertexCache.Touch(tri.indexCache);

                    R_LinkLightSurf(ref vLight.globalShadows, tri, null, light, null, vLight.scissorRect, true); // FIXME?
                }
            }
        }

        //===============================================================================================================

        public static bool R_IssueEntityDefCallback(IRenderEntity def)
        {
            bool update;
            Bounds oldBounds = default;
            var checkBounds = r_checkBounds.Bool;

            if (checkBounds) oldBounds = def.referenceBounds;

            def.archived = false;    // will need to be written to the demo file
            tr.pc.c_entityDefCallbacks++;
            update = def.parms.callback(def.parms, tr.viewDef != null ? tr.viewDef.renderView : null);

            if (def.parms.hModel == null) { common.Error("R_IssueEntityDefCallback: dynamic entity callback didn't set model"); return false; }

            if (checkBounds && (
                oldBounds[0].x > def.referenceBounds[0].x + CHECK_BOUNDS_EPSILON ||
                oldBounds[0].y > def.referenceBounds[0].y + CHECK_BOUNDS_EPSILON ||
                oldBounds[0].z > def.referenceBounds[0].z + CHECK_BOUNDS_EPSILON ||
                oldBounds[1].x < def.referenceBounds[1].x - CHECK_BOUNDS_EPSILON ||
                oldBounds[1].y < def.referenceBounds[1].y - CHECK_BOUNDS_EPSILON ||
                oldBounds[1].z < def.referenceBounds[1].z - CHECK_BOUNDS_EPSILON))
                common.Printf($"entity {def.index} callback extended reference bounds\n");

            return update;
        }

        // Issues a deferred entity callback if necessary. If the model isn't dynamic, it returns the original. Returns the cached dynamic model if present, otherwise creates it and any necessary overlays
        public static IRenderModel R_EntityDefDynamicModel(IRenderEntity def)
        {
            // allow deferred entities to construct themselves
            var callbackUpdate = def.parms.callback != null ? R_IssueEntityDefCallback(def) : false;

            var model = def.parms.hModel;

            if (model == null) common.Error("R_EntityDefDynamicModel: NULL model");

            if (model.IsDynamicModel == DynamicModel.DM_STATIC) { def.dynamicModel = null; def.dynamicModelFrameCount = 0; return model; }

            // continously animating models (particle systems, etc) will have their snapshot updated every single view
            if (callbackUpdate || (model.IsDynamicModel == DynamicModel.DM_CONTINUOUS && def.dynamicModelFrameCount != tr.frameCount)) R_ClearEntityDefDynamicModel(def);

            // if we don't have a snapshot of the dynamic model, generate it now
            if (def.dynamicModel == null)
            {
                // instantiate the snapshot of the dynamic model, possibly reusing memory from the cached snapshot
                def.cachedDynamicModel = model.InstantiateDynamicModel(def.parms, tr.viewDef, def.cachedDynamicModel);

                if (def.cachedDynamicModel != null)
                {
                    // add any overlays to the snapshot of the dynamic model
                    if (def.overlay != null && !r_skipOverlays.Bool) def.overlay.AddOverlaySurfacesToModel(def.cachedDynamicModel);
                    else RenderModelOverlay.RemoveOverlaySurfacesFromModel(def.cachedDynamicModel);

                    if (r_checkBounds.Bool)
                    {
                        var b = def.cachedDynamicModel.Bounds();
                        if (
                            b[0].x < def.referenceBounds[0].x - CHECK_BOUNDS_EPSILON ||
                            b[0].y < def.referenceBounds[0].y - CHECK_BOUNDS_EPSILON ||
                            b[0].z < def.referenceBounds[0].z - CHECK_BOUNDS_EPSILON ||
                            b[1].x > def.referenceBounds[1].x + CHECK_BOUNDS_EPSILON ||
                            b[1].y > def.referenceBounds[1].y + CHECK_BOUNDS_EPSILON ||
                            b[1].z > def.referenceBounds[1].z + CHECK_BOUNDS_EPSILON)
                            common.Printf($"entity {def.index} dynamic model exceeded reference bounds\n");
                    }
                }

                def.dynamicModel = def.cachedDynamicModel;
                def.dynamicModelFrameCount = tr.frameCount;
            }

            // set model depth hack value
            if (def.dynamicModel != null && model.DepthHack != 0f && tr.viewDef != null)
            {
                Plane eye, clip;
                fixed (float* _ = tr.viewDef.worldSpace.u.eyeViewMatrix2) R_TransformModelToClip(def.parms.origin, _, tr.viewDef.projectionMatrix, out eye, out clip);
                R_TransformClipToDevice(clip, tr.viewDef, out var ndc);
                def.parms.modelDepthHack = model.DepthHack * (1f - ndc.z);
            }

            // FIXME: if any of the surfaces have deforms, create a frame-temporary model with references to the undeformed surfaces.  This would allow deforms to be light interacting.
            return def.dynamicModel;
        }

        static float[] R_AddDrawSurf_refRegs = new float[MAX_EXPRESSION_REGISTERS];  // don't put on stack, or VC++ will do a page touch
        public static void R_AddDrawSurf(SrfTriangles tri, ViewEntity space, RenderEntity renderEntity, Material shader, ScreenRect scissor)
        {
            float[] shaderParms;
            var generatedShaderParms = new float[Material.MAX_ENTITY_SHADER_PARMS];

            var drawSurf = R_ClearedFrameAllocT<DrawSurf>();
            drawSurf.geoFrontEnd = tri;

            drawSurf.ambientCache = tri.ambientCache;
            drawSurf.indexCache = tri.indexCache;
            drawSurf.shadowCache = tri.shadowCache;
            drawSurf.numIndexes = tri.numIndexes;

            drawSurf.numShadowIndexesNoFrontCaps = tri.numShadowIndexesNoFrontCaps;
            drawSurf.numShadowIndexesNoCaps = tri.numShadowIndexesNoCaps;
            drawSurf.shadowCapPlaneBits = tri.shadowCapPlaneBits;

            drawSurf.space = space;
            drawSurf.material = shader;

            drawSurf.scissorRect = scissor;
            drawSurf.sort = shader.Sort + tr.sortOffset;
            drawSurf.dsFlags = 0;

            // bumping this offset each time causes surfaces with equal sort orders to still deterministically draw in the order they are added
            tr.sortOffset += 0.000001f;

            // if it doesn't fit, resize the list
            if (tr.viewDef.numDrawSurfs == tr.viewDef.maxDrawSurfs)
            {
                var old = tr.viewDef.drawSurfs;
                int count;
                if (tr.viewDef.maxDrawSurfs == 0) { tr.viewDef.maxDrawSurfs = INITIAL_DRAWSURFS; count = 0; }
                else { count = tr.viewDef.maxDrawSurfs; tr.viewDef.maxDrawSurfs *= 2; }
                tr.viewDef.drawSurfs = new DrawSurf[tr.viewDef.maxDrawSurfs];
                UnsafeX.ArrayCopy(tr.viewDef.drawSurfs, old, count);
            }
            tr.viewDef.drawSurfs[tr.viewDef.numDrawSurfs] = drawSurf;
            tr.viewDef.numDrawSurfs++;

            // process the shader expressions for conditionals / color / texcoords
            var constRegs = shader.ConstantRegisters();
            // shader only uses constant values
            if (constRegs != null) drawSurf.shaderRegisters = constRegs;
            else
            {
                var regs = new float[shader.NumRegisters];
                drawSurf.shaderRegisters = regs;

                // a reference shader will take the calculated stage color value from another shader and use that for the parm0-parm3 of the current shader, which allows a stage of
                // a light model and light flares to pick up different flashing tables from different light shaders
                if (renderEntity.referenceShader != null)
                {
                    // evaluate the reference shader to find our shader parms
                    renderEntity.referenceShader.EvaluateRegisters(R_AddDrawSurf_refRegs, renderEntity.shaderParms, tr.viewDef, renderEntity.referenceSound);
                    var pStage = renderEntity.referenceShader.GetStage(0);

                    UnsafeX.ArrayCopy(generatedShaderParms, renderEntity.shaderParms, renderEntity.shaderParms.Length);
                    generatedShaderParms[0] = R_AddDrawSurf_refRegs[pStage.color.registers[0]];
                    generatedShaderParms[1] = R_AddDrawSurf_refRegs[pStage.color.registers[1]];
                    generatedShaderParms[2] = R_AddDrawSurf_refRegs[pStage.color.registers[2]];

                    shaderParms = generatedShaderParms;
                }
                // evaluate with the entityDef's shader parms
                else shaderParms = renderEntity.shaderParms;

                var oldFloatTime = 0f;
                var oldTime = 0;
                if (space.entityDef != null && space.entityDef.parms.timeGroup != 0)
                {
                    oldFloatTime = tr.viewDef.floatTime;
                    oldTime = tr.viewDef.renderView.time;

                    tr.viewDef.floatTime = game.GetTimeGroupTime(space.entityDef.parms.timeGroup) * 0.001f;
                    tr.viewDef.renderView.time = game.GetTimeGroupTime(space.entityDef.parms.timeGroup);
                }

                shader.EvaluateRegisters(regs, shaderParms, tr.viewDef, renderEntity.referenceSound);

                if (space.entityDef != null && space.entityDef.parms.timeGroup != 0)
                {
                    tr.viewDef.floatTime = oldFloatTime;
                    tr.viewDef.renderView.time = oldTime;
                }
            }

            // check for deformations
            R_DeformDrawSurf(drawSurf);

            // skybox surfaces need a dynamic texgen
            switch (shader.Texgen)
            {
                case TG.WOBBLESKY_CUBE: R_WobbleskyTexGen(drawSurf, tr.viewDef.renderView.vieworg); break;
                // Be sure to init the wobbleTransform to identity for cube-map based surfaces that does not use it (it will be passed to the shader)
                case TG.SKYBOX_CUBE:
                case TG.DIFFUSE_CUBE:
                case TG.REFLECT_CUBE:
                    Matrix4x4.identity.Fixed(matrix =>
                    {
                        fixed (float* _ = drawSurf.wobbleTransform) Unsafe.CopyBlock(_, matrix, 16 * sizeof(float));
                    }); break;
                default: break;
            }

            // check for gui surfaces
            IUserInterface gui = null;

            if (space.entityDef == null) gui = shader.GlobalGui;
            else
            {
                var guiNum = shader.EntityGui - 1;
                if (guiNum >= 0 && guiNum < IRenderWorld.MAX_RENDERENTITY_GUI) gui = renderEntity.gui[guiNum];
                if (gui == null) gui = shader.GlobalGui;
            }

            if (gui != null)
            {
                // force guis on the fast time
                var oldFloatTime = tr.viewDef.floatTime;
                var oldTime = tr.viewDef.renderView.time;

                tr.viewDef.floatTime = game.GetTimeGroupTime(1) * 0.001f;
                tr.viewDef.renderView.time = game.GetTimeGroupTime(1);

                if (!R_PreciseCullSurface(drawSurf, out var ndcBounds))
                {
                    // did we ever use this to forward an entity color to a gui that didn't set color?
                    //UnsafeX.ArrayCopy(tr.guiShaderParms, shaderParms, shaderParms.Length);
                    R_RenderGuiSurf(gui, drawSurf);
                }

                tr.viewDef.floatTime = oldFloatTime;
                tr.viewDef.renderView.time = oldTime;
            }

            // we can't add subviews at this point, because that would increment tr.viewCount, messing up the rest of the surface adds for this view
        }

        // Adds surfaces for the given viewEntity
        // Walks through the viewEntitys list and creates drawSurf_t for each surface of each viewEntity that has a non-empty scissorRect
        static void R_AddAmbientDrawsurfs(ViewEntity vEntity)
        {
            int i, total;
            IRenderEntity def;
            SrfTriangles tri;
            IRenderModel model;
            Material shader;

            def = vEntity.entityDef;
            model = def.dynamicModel ?? def.parms.hModel;

            // add all the surfaces
            total = model.NumSurfaces;
            for (i = 0; i < total; i++)
            {
                var surf = model.Surface(i);

                // for debugging, only show a single surface at a time
                if (r_singleSurface.Integer >= 0 && i != r_singleSurface.Integer) continue;

                tri = surf.geometry;
                if (tri == null || tri.numIndexes == 0) continue;
                shader = surf.shader;
                shader = R_RemapShaderBySkin(shader, def.parms.customSkin, def.parms.customShader);

                R_GlobalShaderOverride(shader);

                if (shader == null || !shader.IsDrawn) continue;

                // debugging tool to make sure we are have the correct pre-calculated bounds
                if (r_checkBounds.Bool)
                {
                    int j, k;
                    for (j = 0; j < tri.numVerts; j++)
                    {
                        for (k = 0; k < 3; k++)
                        {
                            if (tri.verts[j].xyz[k] > tri.bounds[1][k] + CHECK_BOUNDS_EPSILON || tri.verts[j].xyz[k] < tri.bounds[0][k] - CHECK_BOUNDS_EPSILON) { common.Printf($"bad tri.bounds on {def.parms.hModel.Name}:{shader.Name}\n"); break; }
                            if (tri.verts[j].xyz[k] > def.referenceBounds[1][k] + CHECK_BOUNDS_EPSILON || tri.verts[j].xyz[k] < def.referenceBounds[0][k] - CHECK_BOUNDS_EPSILON) { common.Printf($"bad referenceBounds on {def.parms.hModel.Name}:{shader.Name}\n"); break; }
                        }
                        if (k != 3) break;
                    }
                }

                if (!R_CullLocalBox(tri.bounds, vEntity.modelMatrix, 5, tr.viewDef.frustum))
                {
                    def.visibleCount = tr.viewCount;

                    // make sure we have an ambient cache
                    // don't add anything if the vertex cache was too full to give us an ambient cache
                    if (!R_CreateAmbientCache(tri, shader.ReceivesLighting)) return;
                    // skip if we are out of vertex memory
                    if (!R_CreateIndexCache(tri)) continue;

                    // touch it so it won't get purged
                    vertexCache.Touch(tri.ambientCache);
                    vertexCache.Touch(tri.indexCache);

                    // add the surface for drawing
                    R_AddDrawSurf(tri, vEntity, vEntity.entityDef.parms, shader, vEntity.scissorRect);

                    // ambientViewCount is used to allow light interactions to be rejected if the ambient surface isn't visible at all
                    tri.ambientViewCount = tr.viewCount;
                }
            }

            // add the lightweight decal surfaces
            for (var decal = def.decals; decal != null; decal = decal.Next())
                decal.AddDecalDrawSurf(vEntity);
        }

        static ScreenRect R_CalcEntityScissorRectangle(ViewEntity vEntity)
        {
            Bounds bounds = default;
            var def = vEntity.entityDef;
            tr.viewDef.viewFrustum.ProjectionBounds(new Box(def.referenceBounds, def.parms.origin, def.parms.axis), bounds);
            return R_ScreenRectFromViewFrustumBounds(bounds);
        }

        // Here is where dynamic models actually get instantiated, and necessary interactions get created.  This is all done on a sort-by-model basis
        // to keep source data in cache (most likely L2) as any interactions and shadows are generated, since dynamic models will typically be lit by two or more lights.
        public static void R_AddModelSurfaces()
        {
            ViewEntity vEntity;
            Interaction inter, next;
            IRenderModel model;

            // clear the ambient surface list
            tr.viewDef.numDrawSurfs = 0;
            tr.viewDef.maxDrawSurfs = 0;  // will be set to INITIAL_DRAWSURFS on R_AddDrawSurf

            // go through each entity that is either visible to the view, or to any light that intersects the view (for shadows)
            for (vEntity = tr.viewDef.viewEntitys; vEntity != null; vEntity = vEntity.next)
            {
                if (r_useEntityScissors.Bool)
                {
                    // calculate the screen area covered by the entity
                    var scissorRect = R_CalcEntityScissorRectangle(vEntity);
                    // intersect with the portal crossing scissor rectangle
                    vEntity.scissorRect.Intersect(scissorRect);

                    if (r_showEntityScissors.Bool) R_ShowColoredScreenRect(vEntity.scissorRect, vEntity.entityDef.index);
                }

                var oldFloatTime = 0f; var oldTime = 0;
                game.SelectTimeGroup(vEntity.entityDef.parms.timeGroup);

                if (vEntity.entityDef.parms.timeGroup != 0)
                {
                    oldFloatTime = tr.viewDef.floatTime;
                    oldTime = tr.viewDef.renderView.time;

                    tr.viewDef.floatTime = game.GetTimeGroupTime(vEntity.entityDef.parms.timeGroup) * 0.001f; tr.viewDef.renderView.time = game.GetTimeGroupTime(vEntity.entityDef.parms.timeGroup);
                }

                if (tr.viewDef.isXraySubview && vEntity.entityDef.parms.xrayIndex == 1)
                {
                    if (vEntity.entityDef.parms.timeGroup != 0) { tr.viewDef.floatTime = oldFloatTime; tr.viewDef.renderView.time = oldTime; }
                    continue;
                }
                else if (!tr.viewDef.isXraySubview && vEntity.entityDef.parms.xrayIndex == 2)
                {
                    if (vEntity.entityDef.parms.timeGroup != 0) { tr.viewDef.floatTime = oldFloatTime; tr.viewDef.renderView.time = oldTime; }
                    continue;
                }

                // add the ambient surface if it has a visible rectangle
                if (!vEntity.scissorRect.IsEmpty)
                {
                    model = R_EntityDefDynamicModel(vEntity.entityDef);
                    if (model == null || model.NumSurfaces <= 0)
                    {
                        if (vEntity.entityDef.parms.timeGroup != 0) { tr.viewDef.floatTime = oldFloatTime; tr.viewDef.renderView.time = oldTime; }
                        continue;
                    }

                    R_AddAmbientDrawsurfs(vEntity);
                    tr.pc.c_visibleViewEntities++;
                }
                else tr.pc.c_shadowViewEntities++;

                // for all the entity / light interactions on this entity, add them to the view
                if (tr.viewDef.isXraySubview)
                {
                    if (vEntity.entityDef.parms.xrayIndex == 2)
                        for (inter = (Interaction)vEntity.entityDef.firstInteraction; inter != null && !inter.IsEmpty; inter = next)
                        {
                            next = (Interaction)inter.entityNext;
                            if (inter.lightDef.viewCount != tr.viewCount) continue;
                            inter.AddActiveInteraction();
                        }
                }
                else
                {
                    // all empty interactions are at the end of the list so once the first is encountered all the remaining interactions are empty
                    for (inter = (Interaction)vEntity.entityDef.firstInteraction; inter != null && !inter.IsEmpty; inter = next)
                    {
                        next = (Interaction)inter.entityNext;

                        // skip any lights that aren't currently visible this is run after any lights that are turned off have already been removed from the viewLights list, and had their viewCount cleared
                        if (inter.lightDef.viewCount != tr.viewCount) continue;
                        inter.AddActiveInteraction();
                    }
                }

                if (vEntity.entityDef.parms.timeGroup != 0) { tr.viewDef.floatTime = oldFloatTime; tr.viewDef.renderView.time = oldTime; }
            }
        }

        public static void R_RemoveUnecessaryViewLights()
        {
            ViewLight vLight;

            // go through each visible light
            for (vLight = tr.viewDef.viewLights; vLight != null; vLight = vLight.next)
                // if the light didn't have any lit surfaces visible, there is no need to draw any of the shadows.  We still keep the vLight for debugging draws
                if (vLight.localInteractions == null && vLight.globalInteractions == null && vLight.translucentInteractions == null)
                {
                    vLight.localShadows = null;
                    vLight.globalShadows = null;
                }

            if (r_useShadowSurfaceScissor.Bool)
            {
                // shrink the light scissor rect to only intersect the surfaces that will actually be drawn. This doesn't seem to actually help, perhaps because the surface scissor
                // rects aren't actually the surface, but only the portal clippings.
                for (vLight = tr.viewDef.viewLights; vLight != null; vLight = vLight.next)
                {
                    DrawSurf surf; ScreenRect surfRect = default;

                    if (!vLight.lightShader.LightCastsShadows) continue;

                    surfRect.Clear();

                    for (surf = vLight.globalInteractions; surf != null; surf = surf.nextOnLight) surfRect.Union(surf.scissorRect);
                    for (surf = vLight.localShadows; surf != null; surf = surf.nextOnLight) surf.scissorRect.Intersect(surfRect);
                    for (surf = vLight.localInteractions; surf != null; surf = surf.nextOnLight) surfRect.Union(surf.scissorRect);
                    for (surf = vLight.globalShadows; surf != null; surf = surf.nextOnLight) surf.scissorRect.Intersect(surfRect);
                    for (surf = vLight.translucentInteractions; surf != null; surf = surf.nextOnLight) surfRect.Union(surf.scissorRect);

                    vLight.scissorRect.Intersect(surfRect);
                }
            }
        }
    }

    unsafe partial class RenderWorldLocal
    {
        // When a lightDef is determined to effect the view (contact the frustum and non-0 light), it will check to make sure that it has interactions for all the entityDefs that it might possibly contact.
        //
        // This does not guarantee that all possible interactions for this light are generated, only that the ones that may effect the current view are generated. so it does need to be called every view.
        //
        // This does not cause entityDefs to create dynamic models, all work is done on the referenceBounds.
        // 
        // All entities that have non-empty interactions with viewLights will have viewEntities made for them and be put on the viewEntity list,
        // even if their surfaces aren't visible, because they may need to cast shadows.
        //
        // Interactions are usually removed when a entityDef or lightDef is modified, unless the change is known to not effect them, so there is no danger of getting a stale interaction, we just need to
        // check that needed ones are created.
        //
        // An interaction can be at several levels:
        //
        // Don't interact (but share an area) (numSurfaces = 0)
        // Entity reference bounds touches light frustum, but surfaces haven't been generated (numSurfaces = -1)
        // Shadow surfaces have been generated, but light surfaces have not.  The shadow surface may still be empty due to bounds being conservative. Both shadow and light surfaces have been generated.  Either or both surfaces may still be empty due to conservative bounds.
        public void CreateLightDefInteractions(IRenderLight ldef)
        {
            AreaReference eref, lref; IRenderEntity edef; PortalArea area; Interaction inter;

            for (lref = ldef.references; lref != null; lref = lref.ownerNext)
            {
                area = lref.area;

                // check all the models in this area
                for (eref = area.entityRefs.areaNext; eref != area.entityRefs; eref = eref.areaNext)
                {
                    edef = eref.entity;

                    // if the entity doesn't have any light-interacting surfaces, we could skip this, but we don't want to instantiate dynamic models yet, so we can't check that on most things

                    // if the entity isn't viewed
                    if (tr.viewDef != null && edef.viewCount != tr.viewCount)
                    {
                        // if the light doesn't cast shadows, skip
                        if (!ldef.lightShader.LightCastsShadows) continue;
                        // if we are suppressing its shadow in this view, skip
                        if (!r_skipSuppress.Bool)
                        {
                            if (edef.parms.suppressShadowInViewID != 0 && edef.parms.suppressShadowInViewID == tr.viewDef.renderView.viewID) continue;
                            if (edef.parms.suppressShadowInLightID != 0 && edef.parms.suppressShadowInLightID == ldef.parms.lightId) continue;
                        }
                    }

                    // some big outdoor meshes are flagged to not create any dynamic interactions when the level designer knows that nearby moving lights shouldn't actually hit them
                    if (edef.parms.noDynamicInteractions && edef.world.generateAllInteractionsCalled) continue;

                    // if any of the edef's interaction match this light, we don't
                    // need to consider it.
                    if (r_useInteractionTable.Bool && this.interactionTable != null)
                    {
                        // allocating these tables may take several megs on big maps, but it saves 3% to 5% of the CPU time.  The table is updated at interaction::AllocAndLink() and interaction::UnlinkAndFree()
                        var index = ldef.index * this.interactionTableWidth + edef.index;
                        inter = this.interactionTable[index];
                        if (inter != null)
                        {
                            // if this entity wasn't in view already, the scissor rect will be empty, so it will only be used for shadow casting
                            if (!inter.IsEmpty) R_SetEntityDefViewEntity(edef);
                            continue;
                        }
                    }
                    else
                    {
                        // scan the doubly linked lists, which may have several dozen entries
                        // we could check either model refs or light refs for matches, but it is assumed that there will be less lights in an area than models
                        // so the entity chains should be somewhat shorter (they tend to be fairly close).
                        for (inter = (Interaction)edef.firstInteraction; inter != null; inter = (Interaction)inter.entityNext) if (inter.lightDef == ldef) break;

                        // if we already have an interaction, we don't need to do anything
                        if (inter != null)
                        {
                            // if this entity wasn't in view already, the scissor rect will be empty, so it will only be used for shadow casting
                            if (!inter.IsEmpty) R_SetEntityDefViewEntity(edef);
                            continue;
                        }
                    }

                    // create a new interaction, but don't do any work other than bbox to frustum culling
                    inter = Interaction.AllocAndLink(edef, ldef);

                    // do a check of the entity reference bounds against the light frustum, trying to avoid creating a viewEntity if it hasn't been already
                    float[] modelMatrix = new float[16], m;

                    if (edef.viewCount == tr.viewCount) m = edef.viewEntity.modelMatrix;
                    else { R_AxisToModelMatrix(edef.parms.axis, edef.parms.origin, modelMatrix); m = modelMatrix; }

                    if (R_CullLocalBox(edef.referenceBounds, m, 6, ldef.frustum)) { inter.MakeEmpty(); continue; }

                    // we will do a more precise per-surface check when we are checking the entity
                    // if this entity wasn't in view already, the scissor rect will be empty, so it will only be used for shadow casting
                    R_SetEntityDefViewEntity(edef);
                }
            }
        }
    }
}