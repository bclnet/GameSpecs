
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        struct Orientation
        {
            public Vector3 origin;
            public Matrix3x3 axis;
        }

        static void R_MirrorPoint(in Vector3 i, in Orientation surface, in Orientation camera, out Vector3 o)
        {
            int i2; float d; Vector3 local, transformed;

            local = i - surface.origin;
            transformed = Vector3.origin;
            for (i2 = 0; i2 < 3; i2++)
            {
                d = local * surface.axis[i2];
                transformed += d * camera.axis[i2];
            }
            o = transformed + camera.origin;
        }

        static void R_MirrorVector(in Vector3 i, in Orientation surface, in Orientation camera, out Vector3 o)
        {
            int i2; float d;

            o = Vector3.origin;
            for (i2 = 0; i2 < 3; i2++)
            {
                d = i * surface.axis[i2];
                o += d * camera.axis[i2];
            }
        }

        // Returns the plane for the first triangle in the surface. FIXME: check for degenerate triangle?
        static void R_PlaneForSurface(SrfTriangles tri, out Plane plane)
        {
            var tri_verts = tri.verts.Value; var tri_indexes = tri.indexes.Value;

            ref DrawVert v1 = ref tri_verts[tri_indexes[0]];
            ref DrawVert v2 = ref tri_verts[tri_indexes[1]];
            ref DrawVert v3 = ref tri_verts[tri_indexes[2]];
            plane = default;
            plane.FromPoints(v1.xyz, v2.xyz, v3.xyz);
        }

        // Check the surface for visibility on a per-triangle basis for cases when it is going to be VERY expensive to draw (subviews)
        // If not culled, also returns the bounding box of the surface in Normalized Device Coordinates, so it can be used to crop the scissor rect.
        // OPTIMIZE: we could also take exact portal passing into consideration
        public static bool R_PreciseCullSurface(DrawSurf drawSurf, out Bounds ndcBounds)
        {
            SrfTriangles tri; Plane clip, eye; int i, j; uint pointOr, pointAnd; Vector3 localView; FixedWinding w = new();

            tri = drawSurf.geoFrontEnd;
            var tri_verts = tri.verts.Value; var tri_indexes = tri.indexes.Value;

            pointOr = 0; unchecked { pointAnd = (uint)~0; }

            // get an exact bounds of the triangles for scissor cropping
            ndcBounds = default;

            for (i = 0; i < tri.numVerts; i++)
            {
                fixed (float* eyeViewMatrixF = drawSurf.space.u.eyeViewMatrix2) R_TransformModelToClip(tri_verts[i].xyz, eyeViewMatrixF, tr.viewDef.projectionMatrix, out eye, out clip);

                var pointFlags = 0U;
                for (j = 0; j < 3; j++)
                {
                    if (clip[j] >= clip[3]) pointFlags |= (uint)(1 << (j * 2));
                    else if (clip[j] <= -clip[3]) pointFlags |= (uint)(1 << (j * 2 + 1));
                }

                pointAnd &= pointFlags;
                pointOr |= pointFlags;
            }

            // trivially reject
            if (pointAnd != 0) return true;

            // backface and frustum cull
            R_GlobalPointToLocal(drawSurf.space.modelMatrix, tr.viewDef.renderView.vieworg, out localView);

            for (i = 0; i < tri.numIndexes; i += 3)
            {
                float dot; Vector3 dir, normal, d1, d2;

                ref Vector3 v1 = ref tri_verts[tri_indexes[i]].xyz;
                ref Vector3 v2 = ref tri_verts[tri_indexes[i + 1]].xyz;
                ref Vector3 v3 = ref tri_verts[tri_indexes[i + 2]].xyz;

                // this is a hack, because R_GlobalPointToLocal doesn't work with the non-normalized axis that we get from the gui view transform.  It doesn't hurt anything, because we know that all gui generated surfaces are front facing
                if (tr.guiRecursionLevel == 0)
                {
                    // we don't care that it isn't normalized, all we want is the sign
                    d1 = v2 - v1;
                    d2 = v3 - v1;
                    normal = d2.Cross(d1);
                    dir = v1 - localView;
                    dot = normal * dir;
                    if (dot >= 0.0f) return true;
                }

                // now find the exact screen bounds of the clipped triangle
                w.NumPoints = 3;
                R_LocalPointToGlobal(drawSurf.space.modelMatrix, v1, out w[0].ToVec3());
                R_LocalPointToGlobal(drawSurf.space.modelMatrix, v2, out w[1].ToVec3());
                R_LocalPointToGlobal(drawSurf.space.modelMatrix, v3, out w[2].ToVec3());
                w[0].s = w[0].t = w[1].s = w[1].t = w[2].s = w[2].t = 0.0f;

                for (j = 0; j < 4; j++) if (!w.ClipInPlace(-tr.viewDef.frustum[j], 0.1f)) break;
                for (j = 0; j < w.NumPoints; j++)
                {
                    R_GlobalToNormalizedDeviceCoordinates(w[j].ToVec3(), out var screen);
                    ndcBounds.AddPoint(screen);
                }
            }

            // if we don't enclose any area, return
            return ndcBounds.IsCleared;
        }

        static ViewDef R_MirrorViewBySurface(DrawSurf drawSurf)
        {
            ViewDef parms; Orientation surface = new(), camera = new();

            // copy the viewport size from the original
            //parms = R_FrameAllocT<ViewDef>();
            parms = new ViewDef(tr.viewDef);
            parms.renderView.viewID = 0;   // clear to allow player bodies to show up, and suppress view weapons

            parms.isSubview = true;
            parms.isMirror = true;

            // create plane axis for the portal we are seeing
            R_PlaneForSurface(drawSurf.geoFrontEnd, out var originalPlane);
            R_LocalPlaneToGlobal(drawSurf.space.modelMatrix, originalPlane, out var plane);

            surface.origin = plane.Normal * -plane[3];
            surface.axis[0] = plane.Normal;
            surface.axis[0].NormalVectors(out surface.axis[1], out surface.axis[2]);
            surface.axis[2] = -surface.axis[2];

            camera.origin = surface.origin;
            camera.axis[0] = -surface.axis[0];
            camera.axis[1] = surface.axis[1];
            camera.axis[2] = surface.axis[2];

            // set the mirrored origin and axis
            R_MirrorPoint(tr.viewDef.renderView.vieworg, surface, camera, out parms.renderView.vieworg);

            R_MirrorVector(tr.viewDef.renderView.viewaxis[0], surface, camera, out parms.renderView.viewaxis[0]);
            R_MirrorVector(tr.viewDef.renderView.viewaxis[1], surface, camera, out parms.renderView.viewaxis[1]);
            R_MirrorVector(tr.viewDef.renderView.viewaxis[2], surface, camera, out parms.renderView.viewaxis[2]);

            // make the view origin 16 units away from the center of the surface
            var viewOrigin = (drawSurf.geoFrontEnd.bounds[0] + drawSurf.geoFrontEnd.bounds[1]) * 0.5f;
            viewOrigin += (originalPlane.Normal * 16);

            R_LocalPointToGlobal(drawSurf.space.modelMatrix, viewOrigin, out parms.initialViewAreaOrigin);

            // set the mirror clip plane
            parms.numClipPlanes = 1;
            parms.clipPlanes[0] = -camera.axis[0];
            parms.clipPlanes[0].d = -(camera.origin * parms.clipPlanes[0].Normal);

            return parms;
        }

        static ViewDef R_XrayViewBySurface(DrawSurf drawSurf)
        {
            ViewDef parms;

            // copy the viewport size from the original
            //parms = (ViewDef)R_FrameAllocT<ViewDef>();
            parms = new ViewDef(tr.viewDef);
            parms.renderView.viewID = 0;   // clear to allow player bodies to show up, and suppress view weapons

            parms.isSubview = true;
            parms.isXraySubview = true;

            return parms;
        }

        static void R_DirectFrameBufferStart()
        {
            var cmd = (EmptyCommand)R_GetCommandBuffer(sizeof(EmptyCommand));
            cmd.commandId = RC.DIRECT_BUFFER_START;
        }

        static void R_DirectFrameBufferEnd()
        {
            var cmd = (EmptyCommand)R_GetCommandBuffer(sizeof(EmptyCommand));
            cmd.commandId = RC.DIRECT_BUFFER_END;
        }

        static void R_RemoteRender(DrawSurf surf, ref TextureStage stage)
        {
            ViewDef parms;

            // remote views can be reused in a single frame
            if (stage.dynamicFrameCount == tr.frameCount) return;

            // if the entity doesn't have a remoteRenderView, do nothing
            if (surf.space.entityDef.parms.remoteRenderView == null) return;

            // copy the viewport size from the original
            //parms = R_FrameAlloc<ViewDef>();
            parms = new ViewDef(tr.viewDef);

            parms.isSubview = true;
            parms.isMirror = false;

            parms.renderView = surf.space.entityDef.parms.remoteRenderView;
            parms.renderView.viewID = 0;   // clear to allow player bodies to show up, and suppress view weapons
            parms.initialViewAreaOrigin = parms.renderView.vieworg;

            tr.CropRenderSize(stage.width, stage.height, true);

            parms.renderView.x = 0;
            parms.renderView.y = 0;
            parms.renderView.width = SCREEN_WIDTH;
            parms.renderView.height = SCREEN_HEIGHT;

            tr.RenderViewToViewport(parms.renderView, parms.viewport);

            parms.scissor.x1 = 0;
            parms.scissor.y1 = 0;
            parms.scissor.x2 = (short)(parms.viewport.x2 - parms.viewport.x1);
            parms.scissor.y2 = (short)(parms.viewport.y2 - parms.viewport.y1);

            parms.superView = tr.viewDef;
            parms.subviewSurface = surf;

            parms.renderView.forceMono = true;

            R_DirectFrameBufferStart();

            // generate render commands for it
            R_RenderView(parms);

            // copy this rendering to the image
            stage.dynamicFrameCount = tr.frameCount;
            if (stage.image == null) stage.image = globalImages.scratchImage;

            tr.CaptureRenderToImage(stage.image.imgName);
            tr.UnCrop();

            R_DirectFrameBufferEnd();
        }

        static void R_MirrorRender(DrawSurf surf, ref TextureStage stage, ScreenRect scissor)
        {
            ViewDef parms;

            // remote views can be reused in a single frame
            if (stage.dynamicFrameCount == tr.frameCount) return;

            // issue a new view command
            parms = R_MirrorViewBySurface(surf);
            if (parms == null) return;

            tr.CropRenderSize(stage.width, stage.height, true);

            parms.renderView.x = 0;
            parms.renderView.y = 0;
            parms.renderView.width = R.SCREEN_WIDTH;
            parms.renderView.height = R.SCREEN_HEIGHT;

            tr.RenderViewToViewport(parms.renderView, parms.viewport);

            parms.scissor.x1 = 0;
            parms.scissor.y1 = 0;
            parms.scissor.x2 = (short)(parms.viewport.x2 - parms.viewport.x1);
            parms.scissor.y2 = (short)(parms.viewport.y2 - parms.viewport.y1);

            parms.superView = tr.viewDef;
            parms.subviewSurface = surf;

            // triangle culling order changes with mirroring
            parms.isMirror ^= tr.viewDef.isMirror;

            R_DirectFrameBufferStart();

            // generate render commands for it
            R_RenderView(parms);

            // copy this rendering to the image
            stage.dynamicFrameCount = tr.frameCount;
            stage.image = globalImages.scratchImage;

            tr.CaptureRenderToImage(stage.image.imgName);
            tr.UnCrop();

            R_DirectFrameBufferEnd();
        }

        static void R_XrayRender(DrawSurf surf, ref TextureStage stage, ScreenRect scissor)
        {
            ViewDef parms;

            // remote views can be reused in a single frame
            if (stage.dynamicFrameCount == tr.frameCount) return;

            // issue a new view command
            parms = R_XrayViewBySurface(surf);
            if (parms == null) return;

            tr.CropRenderSize(stage.width, stage.height, true);

            parms.renderView.x = 0;
            parms.renderView.y = 0;
            parms.renderView.width = R.SCREEN_WIDTH;
            parms.renderView.height = R.SCREEN_HEIGHT;

            tr.RenderViewToViewport(parms.renderView, parms.viewport);

            parms.scissor.x1 = 0;
            parms.scissor.y1 = 0;
            parms.scissor.x2 = (short)(parms.viewport.x2 - parms.viewport.x1);
            parms.scissor.y2 = (short)(parms.viewport.y2 - parms.viewport.y1);

            parms.superView = tr.viewDef;
            parms.subviewSurface = surf;

            // triangle culling order changes with mirroring
            parms.isMirror ^= tr.viewDef.isMirror;

            parms.renderView.forceMono = true;

            R_DirectFrameBufferStart();

            // generate render commands for it
            R_RenderView(parms);

            // copy this rendering to the image
            stage.dynamicFrameCount = tr.frameCount;
            stage.image = globalImages.scratchImage2;

            tr.CaptureRenderToImage(stage.image.imgName);
            tr.UnCrop();

            R_DirectFrameBufferEnd();
        }

        static bool R_GenerateSurfaceSubview(DrawSurf drawSurf)
        {
            ViewDef parms; Material shader;

            // for testing the performance hit
            if (r_skipSubviews.Bool) return false;
            if (R_PreciseCullSurface(drawSurf, out var ndcBounds)) return false;

            shader = drawSurf.material;

            // never recurse through a subview surface that we are already seeing through
            for (parms = tr.viewDef; parms != null; parms = parms.superView) if (parms.subviewSurface == null && parms.subviewSurface.geoFrontEnd == drawSurf.geoFrontEnd && parms.subviewSurface.space.entityDef == drawSurf.space.entityDef) break;
            if (parms != null) return false;

            // crop the scissor bounds based on the precise cull
            ScreenRect scissor = default;

            ref ScreenRect v = ref tr.viewDef.viewport;
            scissor.x1 = (short)(v.x1 + (int)((v.x2 - v.x1 + 1) * 0.5f * (ndcBounds[0].x + 1.0f)));
            scissor.y1 = (short)(v.y1 + (int)((v.y2 - v.y1 + 1) * 0.5f * (ndcBounds[0].y + 1.0f)));
            scissor.x2 = (short)(v.x1 + (int)((v.x2 - v.x1 + 1) * 0.5f * (ndcBounds[1].x + 1.0f)));
            scissor.y2 = (short)(v.y1 + (int)((v.y2 - v.y1 + 1) * 0.5f * (ndcBounds[1].y + 1.0f)));

            // nudge a bit for safety
            scissor.Expand();

            scissor.Intersect(tr.viewDef.scissor);

            if (scissor.IsEmpty) return false; // cropped out

            // see what kind of subview we are making
            if (shader.Sort != (float)SS.SUBVIEW)
            {
                for (var i = 0; i < shader.NumStages; i++)
                {
                    var stage = shader.GetStage(i);
                    switch (stage.texture.dynamic)
                    {
                        case DI.REMOTE_RENDER: R_RemoteRender(drawSurf, ref stage.texture); break;
                        case DI.MIRROR_RENDER: R_MirrorRender(drawSurf, ref stage.texture, scissor); break;
                        case DI.XRAY_RENDER: R_XrayRender(drawSurf, ref stage.texture, scissor); break;
                    }
                }
                return true;
            }

            // issue a new view command
            parms = R_MirrorViewBySurface(drawSurf);
            if (parms == null) return false;

            parms.scissor = scissor;
            parms.superView = tr.viewDef;
            parms.subviewSurface = drawSurf;

            // triangle culling order changes with mirroring
            parms.isMirror ^= tr.viewDef.isMirror;

            // generate render commands for it
            R_RenderView(parms);

            return true;
        }

        // If we need to render another view to complete the current view, generate it first.
        // It is important to do this after all drawSurfs for the current view have been generated, because it may create a subview which would change tr.viewCount.
        public static bool R_GenerateSubViews()
        {
            DrawSurf drawSurf; int i; bool subviews; Material shader;

            // for testing the performance hit
            if (r_skipSubviews.Bool) return false;
            subviews = false;

            // scan the surfaces until we either find a subview, or determine there are no more subview surfaces.
            for (i = 0; i < tr.viewDef.numDrawSurfs; i++)
            {
                drawSurf = tr.viewDef.drawSurfs[i];
                shader = drawSurf.material;

                if (shader == null || !shader.HasSubview) continue;
                if (R_GenerateSurfaceSubview(drawSurf)) subviews = true;
            }

            return subviews;
        }
    }
}
