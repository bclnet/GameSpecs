using System.NumericsX.OpenStack.Gngine.Framework;
using System;
using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.IRenderWorld;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        // The ambientCache is on the stack, so we don't want to leave a reference to it that would try to be freed later.  Create the ambientCache immediately.
        static void R_FinishDeform(DrawSurf drawSurf, SrfTriangles newTri, DrawVert* ac)
        {
            if (newTri == null) return;

            // generate current normals, tangents, and bitangents We might want to support the possibility of deform functions generating
            // explicit normals, and we might also want to allow the cached deformInfo optimization for these.
            // FIXME: this doesn't work, because the deformed surface is just the ambient one, and there isn't an opportunity to generate light interactions
            if (drawSurf.material.ReceivesLighting) { newTri.verts = ac; R_DeriveTangents(newTri, false); newTri.verts = null; }
            newTri.ambientCache = vertexCache.AllocFrameTemp(ac, newTri.numVerts * DrawVert.SizeOf, false);
            fixed (void* newTri_indexes_ = newTri.indexes) newTri.indexCache = vertexCache.AllocFrameTemp(newTri_indexes_, newTri.numIndexes * sizeof(GlIndex), true);

            drawSurf.geoFrontEnd = newTri;
            drawSurf.ambientCache = newTri.ambientCache;
            drawSurf.indexCache = newTri.indexCache;
            drawSurf.numIndexes = newTri.numIndexes;
            drawSurf.numShadowIndexesNoFrontCaps = newTri.numShadowIndexesNoFrontCaps;
            drawSurf.numShadowIndexesNoCaps = newTri.numShadowIndexesNoCaps;
            drawSurf.shadowCapPlaneBits = newTri.shadowCapPlaneBits;
        }

        // Assuming all the triangles for this shader are independant quads, rebuild them as forward facing sprites
        static void R_AutospriteDeform(DrawSurf surf)
        {
            int i; float radius; Vector3 mid, delta, left, up; SrfTriangles tri, newTri;

            tri = surf.geoFrontEnd;

            if ((tri.numVerts & 3) != 0) { common.Warning("R_AutospriteDeform: shader had odd vertex count"); return; }
            if (tri.numIndexes != (tri.numVerts >> 2) * 6) { common.Warning("R_AutospriteDeform: autosprite had odd index count"); return; }

            R_GlobalVectorToLocal(surf.space.modelMatrix, tr.viewDef.renderView.viewaxis[1], out var leftDir);
            R_GlobalVectorToLocal(surf.space.modelMatrix, tr.viewDef.renderView.viewaxis[2], out var upDir);
            if (tr.viewDef.isMirror) leftDir = Vector3.origin - leftDir;

            // this srfTriangles_t and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = R_ClearedFrameAllocT<SrfTriangles>();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = new GlIndex[newTri.numIndexes];

            var ac = stackalloc DrawVert[newTri.numVerts + DrawVert.ALLOC16]; ac = (DrawVert*)_alloca16(ac);
            var v = tri.verts;
            for (i = 0; i < tri.numVerts; i += 4)
            {
                // find the midpoint
                mid.x = 0.25f * (v[i + 0].xyz.x + v[i + 1].xyz.x + v[i + 2].xyz.x + v[i + 3].xyz.x);
                mid.y = 0.25f * (v[i + 0].xyz.y + v[i + 1].xyz.y + v[i + 2].xyz.y + v[i + 3].xyz.y);
                mid.z = 0.25f * (v[i + 1].xyz.z + v[i + 1].xyz.z + v[i + 2].xyz.z + v[i + 3].xyz.z);

                delta = v[i + 0].xyz - mid;
                radius = delta.Length * 0.707f;
                left = leftDir * radius;
                up = upDir * radius;

                ac[i + 0].xyz = mid + left + up; ac[i + 0].st.x = 0; ac[i + 0].st.y = 0;
                ac[i + 1].xyz = mid - left + up; ac[i + 1].st.x = 1; ac[i + 1].st.y = 0;
                ac[i + 2].xyz = mid - left - up; ac[i + 2].st.x = 1; ac[i + 2].st.y = 1;
                ac[i + 3].xyz = mid + left - up; ac[i + 3].st.x = 0; ac[i + 3].st.y = 1;

                newTri.indexes[6 * (i >> 2) + 0] = i;
                newTri.indexes[6 * (i >> 2) + 1] = i + 1;
                newTri.indexes[6 * (i >> 2) + 2] = i + 2;

                newTri.indexes[6 * (i >> 2) + 3] = i;
                newTri.indexes[6 * (i >> 2) + 4] = i + 2;
                newTri.indexes[6 * (i >> 2) + 5] = i + 3;
            }

            R_FinishDeform(surf, newTri, ac);
        }

        // will pivot a rectangular quad along the center of its long axis
        // Note that a geometric tube with even quite a few sides tube will almost certainly render much faster than this, so this should only be for faked volumetric tubes.
        // Make sure this is used with twosided translucent shaders, because the exact side order may not be correct.
        static (int x, int y)[] R_TubeDeform_edgeVerts = {
            (0, 1),
            (1, 2),
            (2, 0),
            (3, 4),
            (4, 5),
            (5, 3)
        };
        static void R_TubeDeform(DrawSurf surf)
        {
            int i, j, indexes; SrfTriangles tri;

            tri = surf.geoFrontEnd;

            if ((tri.numVerts & 3) != 0) common.Error("R_AutospriteDeform: shader had odd vertex count");
            if (tri.numIndexes != (tri.numVerts >> 2) * 6) common.Error("R_AutospriteDeform: autosprite had odd index count");

            // we need the view direction to project the minor axis of the tube as the view changes
            R_GlobalPointToLocal(surf.space.modelMatrix, tr.viewDef.renderView.vieworg, out var localView);

            // this srfTriangles_t and all its indexes and caches are in frame memory, and will be automatically disposed of
            var newTri = new SrfTriangles();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = new GlIndex[newTri.numIndexes];
            fixed (void* _d = newTri.indexes, _s = tri.indexes) Unsafe.CopyBlock(_d, _s, (uint)(newTri.numIndexes * sizeof(GlIndex)));

            var ac = stackalloc DrawVert[newTri.numVerts + DrawVert.ALLOC16]; ac = (DrawVert*)_alloca16(ac);

            // this is a lot of work for two triangles... we could precalculate a lot if it is an issue, but it would mess up the shader abstraction
            var lengths = stackalloc float[2];
            var nums = stackalloc int[2];
            var mid = stackalloc Vector3[2];
            Vector3 major, minor = default;
            //ref DrawVert v1, v2;
            for (i = 0, indexes = 0; i < tri.numVerts; i += 4, indexes += 6)
            {
                // identify the two shortest edges out of the six defined by the indexes
                nums[0] = nums[1] = 0;
                lengths[0] = lengths[1] = 999999;

                for (j = 0; j < 6; j++)
                {
                    ref DrawVert v1 = ref tri.verts[tri.indexes[i + R_TubeDeform_edgeVerts[j].x]];
                    ref DrawVert v2 = ref tri.verts[tri.indexes[i + R_TubeDeform_edgeVerts[j].y]];

                    var l = (v1.xyz - v2.xyz).Length;
                    if (l < lengths[0]) { nums[1] = nums[0]; lengths[1] = lengths[0]; nums[0] = j; lengths[0] = l; }
                    else if (l < lengths[1]) { nums[1] = j; lengths[1] = l; }
                }

                // find the midpoints of the two short edges, which will give us the major axis in object coordinates
                for (j = 0; j < 2; j++)
                {
                    ref DrawVert v1 = ref tri.verts[tri.indexes[i + R_TubeDeform_edgeVerts[nums[j]].x]];
                    ref DrawVert v2 = ref tri.verts[tri.indexes[i + R_TubeDeform_edgeVerts[nums[j]].y]];

                    mid[j].x = 0.5f * (v1.xyz[0] + v2.xyz[0]);
                    mid[j].y = 0.5f * (v1.xyz[1] + v2.xyz[1]);
                    mid[j].z = 0.5f * (v1.xyz[2] + v2.xyz[2]);
                }

                // find the vector of the major axis
                major = mid[1] - mid[0];

                // re-project the points
                for (j = 0; j < 2; j++)
                {
                    var i1 = tri.indexes[i + R_TubeDeform_edgeVerts[nums[j]].x];
                    var i2 = tri.indexes[i + R_TubeDeform_edgeVerts[nums[j]].y];

                    var av1 = &ac[i1]; *av1 = tri.verts[i1];
                    var av2 = &ac[i2]; *av2 = tri.verts[i2];

                    var l = 0.5f * lengths[j];

                    // cross this with the view direction to get minor axis
                    Vector3 dir = mid[j] - localView;
                    minor.Cross(major, dir);
                    minor.Normalize();

                    if (j != 0) { av1->xyz = mid[j] - l * minor; av2->xyz = mid[j] + l * minor; }
                    else { av1->xyz = mid[j] + l * minor; av2->xyz = mid[j] - l * minor; }
                }
            }

            R_FinishDeform(surf, newTri, ac);
        }

        const int MAX_TRI_WINDING_INDEXES = 16;
        static int R_WindingFromTriangles(SrfTriangles tri, GlIndex* indexes)
        {
            int i, j, k, l;

            indexes[0] = tri.indexes[0];
            var numIndexes = 1;
            var numTris = tri.numIndexes / 3;

            do
            {
                // find an edge that goes from the current index to another index that isn't already used, and isn't an internal edge
                for (i = 0; i < numTris; i++)
                {
                    for (j = 0; j < 3; j++)
                    {
                        if (tri.indexes[i * 3 + j] != indexes[numIndexes - 1]) continue;
                        var next = tri.indexes[i * 3 + (j + 1) % 3];

                        // make sure it isn't already used
                        if (numIndexes == 1)
                        {
                            if (next == indexes[0]) continue;
                        }
                        else
                        {
                            for (k = 1; k < numIndexes; k++) if (indexes[k] == next) break;
                            if (k != numIndexes) continue;
                        }

                        // make sure it isn't an interior edge
                        for (k = 0; k < numTris; k++)
                        {
                            if (k == i) continue;
                            for (l = 0; l < 3; l++)
                            {
                                var a = tri.indexes[k * 3 + l];
                                if (a != next) continue;
                                var b = tri.indexes[k * 3 + (l + 1) % 3];
                                if (b != indexes[numIndexes - 1]) continue;

                                // this is an interior edge
                                break;
                            }
                            if (l != 3) break;
                        }
                        if (k != numTris) continue;

                        // add this to the list
                        indexes[numIndexes] = next;
                        numIndexes++;
                        break;
                    }
                    if (j != 3) break;
                }
                if (numIndexes == tri.numVerts) break;
            } while (i != numTris);

            return numIndexes;
        }

        static GlIndex[] R_FlareDeform_triIndexes = {
            0,4,5,  0,5,6, 0,6,7, 0,7,1, 1,7,8, 1,8,9,
            15,4,0, 15,0,3, 3,0,1, 3,1,2, 2,1,9, 2,9,10,
            14,15,3, 14,3,13, 13,3,2, 13,2,12, 12,2,11, 11,2,10
        };

        static void R_FlareDeform(DrawSurf surf)
        {
            int j; float dot;
            SrfTriangles tri, newTri; Plane plane = new();

            tri = surf.geoFrontEnd;

            // FIXME: temp hack for flares on tripleted models
            if (tri.numVerts != 4 || tri.numIndexes != 6) { common.DPrintf("R_FlareDeform: not a single quad\n"); return; }

            // this srfTriangles_t and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = new SrfTriangles();
            newTri.numVerts = 16;
            newTri.numIndexes = 18 * 3;
            newTri.indexes = new GlIndex[newTri.numIndexes];

            var ac = stackalloc DrawVert[newTri.numVerts + DrawVert.ALLOC16]; ac = (DrawVert*)_alloca16(ac);

            // find the plane
            if (!plane.FromPoints(tri.verts[tri.indexes[0]].xyz, tri.verts[tri.indexes[1]].xyz, tri.verts[tri.indexes[2]].xyz)) { common.Warning("R_FlareDeform: plane.FromPoints failed"); return; }

            // if viewer is behind the plane, draw nothing
            R_GlobalPointToLocal(surf.space.modelMatrix, tr.viewDef.renderView.vieworg, out var localViewer);
            var distFromPlane = localViewer * plane.Normal + plane[3];
            if (distFromPlane <= 0) { newTri.numIndexes = 0; surf.geoFrontEnd = newTri; return; }

            var center = tri.verts[0].xyz;
            for (j = 1; j < tri.numVerts; j++) center += tri.verts[j].xyz;
            center *= 1f / tri.numVerts;

            var dir = localViewer - center;
            dir.Normalize();

            dot = dir * plane.Normal;

            // set vertex colors based on plane angle
            var color = (int)(dot * 8 * 256);
            if (color > 255) color = 255;
            for (j = 0; j < newTri.numVerts; j++)
            {
                ac[j].color0 = ac[j].color1 = ac[j].color2 = (byte)color;
                ac[j].color3 = 255;
            }

            var spread = surf.shaderRegisters[surf.material.GetDeformRegister(0)] * R.r_flareSize.Float;
            var edgeDir = stackalloc (Vector3 x, Vector3 y, Vector3 z)[4];
            var indexes = stackalloc GlIndex[MAX_TRI_WINDING_INDEXES];
            var numIndexes = R_WindingFromTriangles(tri, indexes);

            // only deal with quads
            if (numIndexes != 4) return;

            int i;
            // calculate vector directions
            for (i = 0; i < 4; i++)
            {
                ac[i].xyz = tri.verts[indexes[i]].xyz;
                ac[i].st.x = ac[i].st.y = 0.5f;

                var toEye = tri.verts[indexes[i]].xyz - localViewer;
                toEye.Normalize();

                var d1 = tri.verts[indexes[(i + 1) % 4]].xyz - localViewer;
                d1.Normalize();
                edgeDir[i].y.Cross(toEye, d1);
                edgeDir[i].y.Normalize();
                edgeDir[i].y = Vector3.origin - edgeDir[i].y;

                var d2 = tri.verts[indexes[(i + 3) % 4]].xyz - localViewer;
                d2.Normalize();
                edgeDir[i].x.Cross(toEye, d2);
                edgeDir[i].x.Normalize();

                edgeDir[i].z = edgeDir[i].x + edgeDir[i].y;
                edgeDir[i].z.Normalize();
            }

            // build all the points
            ac[4].xyz = tri.verts[indexes[0]].xyz + spread * edgeDir[0].x;
            ac[4].st.x = 0f; ac[4].st.y = 0.5f;

            ac[5].xyz = tri.verts[indexes[0]].xyz + spread * edgeDir[0].z;
            ac[5].st.x = 0f; ac[5].st.y = 0f;

            ac[6].xyz = tri.verts[indexes[0]].xyz + spread * edgeDir[0].y;
            ac[6].st.x = 0.5f; ac[6].st.y = 0f;

            ac[7].xyz = tri.verts[indexes[1]].xyz + spread * edgeDir[1].x;
            ac[7].st.x = 0.5f; ac[7].st.y = 0f;

            ac[8].xyz = tri.verts[indexes[1]].xyz + spread * edgeDir[1].z;
            ac[8].st.x = 1f; ac[8].st.y = 0f;

            ac[9].xyz = tri.verts[indexes[1]].xyz + spread * edgeDir[1].y;
            ac[9].st.x = 1f; ac[9].st.y = 0.5f;

            ac[10].xyz = tri.verts[indexes[2]].xyz + spread * edgeDir[2].x;
            ac[10].st.x = 1f; ac[10].st.y = 0.5f;

            ac[11].xyz = tri.verts[indexes[2]].xyz + spread * edgeDir[2].z;
            ac[11].st.x = 1f; ac[11].st.y = 1f;

            ac[12].xyz = tri.verts[indexes[2]].xyz + spread * edgeDir[2].y;
            ac[12].st.x = 0.5f; ac[12].st.y = 1f;

            ac[13].xyz = tri.verts[indexes[3]].xyz + spread * edgeDir[3].x;
            ac[13].st.x = 0.5f; ac[13].st.y = 1f;

            ac[14].xyz = tri.verts[indexes[3]].xyz + spread * edgeDir[3].z;
            ac[14].st.x = 0f; ac[14].st.y = 1f;

            ac[15].xyz = tri.verts[indexes[3]].xyz + spread * edgeDir[3].y;
            ac[15].st.x = 0f; ac[15].st.y = 0.5f;

            for (i = 4; i < 16; i++)
            {
                dir = ac[i].xyz - localViewer;
                var len = dir.Normalize();
                var ang = dir * plane.Normal;
                var newLen = -(distFromPlane / ang);
                if (newLen > 0 && newLen < len) ac[i].xyz = localViewer + dir * newLen;
                ac[i].st.x = 0f; ac[i].st.y = 0.5f;
            }

            fixed (void* d = newTri.indexes, s = R_FlareDeform_triIndexes) Unsafe.CopyBlock(d, s, (uint)R_FlareDeform_triIndexes.Length * sizeof(int));

            R_FinishDeform(surf, newTri, ac);
        }

        // Expands the surface along it's normals by a shader amount
        static void R_ExpandDeform(DrawSurf surf)
        {
            int i; SrfTriangles tri, newTri;

            tri = surf.geoFrontEnd;

            // this srfTriangles_t and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = new SrfTriangles();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = tri.indexes;

            var ac = stackalloc DrawVert[newTri.numVerts + DrawVert.ALLOC16]; ac = (DrawVert*)_alloca16(ac);

            var dist = surf.shaderRegisters[surf.material.GetDeformRegister(0)];
            for (i = 0; i < tri.numVerts; i++)
            {
                ac[i] = tri.verts[i];
                ac[i].xyz = tri.verts[i].xyz + tri.verts[i].normal * dist;
            }

            R_FinishDeform(surf, newTri, ac);
        }

        // Moves the surface along the X axis, mostly just for demoing the deforms
        static void R_MoveDeform(DrawSurf surf)
        {
            int i; SrfTriangles tri, newTri;

            tri = surf.geoFrontEnd;

            // this SrfTriangles and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = new SrfTriangles();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = tri.indexes;

            var ac = stackalloc DrawVert[newTri.numVerts + DrawVert.ALLOC16]; ac = (DrawVert*)_alloca16(ac);

            var dist = surf.shaderRegisters[surf.material.GetDeformRegister(0)];
            for (i = 0; i < tri.numVerts; i++)
            {
                ac[i] = tri.verts[i];
                ac[i].xyz[0] += dist;
            }

            R_FinishDeform(surf, newTri, ac);
        }

        //=====================================================================================

        // Turbulently deforms the XYZ, S, and T values
        static void R_TurbulentDeform(DrawSurf surf)
        {
            int i; SrfTriangles tri, newTri;

            tri = surf.geoFrontEnd;

            // this SrfTriangles and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = new SrfTriangles();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = tri.indexes;

            var ac = stackalloc DrawVert[newTri.numVerts + DrawVert.ALLOC16]; ac = (DrawVert*)_alloca16(ac);

            var table = (DeclTable)surf.material.DeformDecl;
            var range = surf.shaderRegisters[surf.material.GetDeformRegister(0)];
            var timeOfs = surf.shaderRegisters[surf.material.GetDeformRegister(1)];
            var domain = surf.shaderRegisters[surf.material.GetDeformRegister(2)];
            var tOfs = 0.5f;

            for (i = 0; i < tri.numVerts; i++)
            {
                var f = tri.verts[i].xyz.x * 0.003f
                    + tri.verts[i].xyz.y * 0.007f
                    + tri.verts[i].xyz.z * 0.011f;

                f = timeOfs + domain * f;
                f += timeOfs;

                ac[i] = tri.verts[i];

                ac[i].st[0] += range * table.TableLookup(f);
                ac[i].st[1] += range * table.TableLookup(f + tOfs);
            }

            R_FinishDeform(surf, newTri, ac);
        }

        //=====================================================================================

        const int MAX_EYEBALL_TRIS = 10;
        const int MAX_EYEBALL_ISLANDS = 6;

        struct EyeIsland
        {
            public fixed int tris[MAX_EYEBALL_TRIS];
            public int numTris;
            public Bounds bounds;
            public Vector3 mid;
        }

        static void AddTriangleToIsland_r(SrfTriangles tri, int triangleNum, bool* usedList, ref EyeIsland island)
        {
            int a, b, c;

            usedList[triangleNum] = true;

            // add to the current island
            if (island.numTris == MAX_EYEBALL_TRIS) common.Error("MAX_EYEBALL_TRIS");
            island.tris[island.numTris] = triangleNum;
            island.numTris++;

            // recurse into all neighbors
            a = tri.indexes[triangleNum * 3];
            b = tri.indexes[triangleNum * 3 + 1];
            c = tri.indexes[triangleNum * 3 + 2];

            island.bounds.AddPoint(tri.verts[a].xyz);
            island.bounds.AddPoint(tri.verts[b].xyz);
            island.bounds.AddPoint(tri.verts[c].xyz);

            var numTri = tri.numIndexes / 3;
            for (var i = 0; i < numTri; i++)
                if (!usedList[i] && (
                    tri.indexes[i * 3 + 0] == a || tri.indexes[i * 3 + 1] == a || tri.indexes[i * 3 + 2] == a
                    || tri.indexes[i * 3 + 0] == b || tri.indexes[i * 3 + 1] == b || tri.indexes[i * 3 + 2] == b
                    || tri.indexes[i * 3 + 0] == c || tri.indexes[i * 3 + 1] == c || tri.indexes[i * 3 + 2] == c))
                    AddTriangleToIsland_r(tri, i, usedList, ref island);
        }

        // Each eyeball surface should have an separate upright triangle behind it, long end pointing out the eye, and another single triangle in front of the eye for the focus point.
        static void R_EyeballDeform(DrawSurf surf)
        {
            int i, j, k;
            SrfTriangles tri;
            SrfTriangles newTri;
            var islands = stackalloc EyeIsland[MAX_EYEBALL_ISLANDS];
            int numIslands;
            var triUsed = stackalloc bool[MAX_EYEBALL_ISLANDS * MAX_EYEBALL_TRIS];

            tri = surf.geoFrontEnd;

            // separate all the triangles into islands
            var numTri = tri.numIndexes / 3;
            if (numTri > MAX_EYEBALL_ISLANDS * MAX_EYEBALL_TRIS) { common.Printf("R_EyeballDeform: too many triangles in surface"); return; }
            Unsafe.InitBlockUnaligned(triUsed, 0, MAX_EYEBALL_ISLANDS * MAX_EYEBALL_TRIS * sizeof(bool));

            for (numIslands = 0; numIslands < MAX_EYEBALL_ISLANDS; numIslands++)
            {
                islands[numIslands].numTris = 0;
                islands[numIslands].bounds.Clear();
                for (i = 0; i < numTri; i++)
                    if (!triUsed[i]) { AddTriangleToIsland_r(tri, i, triUsed, ref islands[numIslands]); break; }
                if (i == numTri) break;
            }

            // assume we always have two eyes, two origins, and two targets
            if (numIslands != 3) { common.Printf($"R_EyeballDeform: {numIslands} triangle islands\n"); return; }

            // this SrfTriangles and all its indexes and caches are in frame memory, and will be automatically disposed of
            // the surface cannot have more indexes or verts than the original
            newTri = new SrfTriangles();
            //Unsafe.InitBlockUnaligned(newTri, 0, 0);
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = new GlIndex[tri.numIndexes];
            var ac = stackalloc DrawVert[tri.numVerts + DrawVert.ALLOC16]; ac = (DrawVert*)_alloca16(ac);

            newTri.numIndexes = 0;

            // decide which islands are the eyes and points
            for (i = 0; i < numIslands; i++) islands[i].mid = islands[i].bounds.Center;

            // the closest single triangle point will be the eye origin and the next-to-farthest will be the focal point
            Vector3 origin, focus;
            var originIsland = 0;
            var dist = stackalloc float[MAX_EYEBALL_ISLANDS];
            var sortOrder = stackalloc int[MAX_EYEBALL_ISLANDS];
            for (i = 0; i < numIslands; i++)
            {
                ref EyeIsland island = ref islands[i];
                if (island.numTris == 1) continue;

                for (j = 0; j < numIslands; j++)
                {
                    dist[j] = (islands[j].mid - island.mid).Length;
                    sortOrder[j] = j;
                    for (k = j - 1; k >= 0; k--)
                        if (dist[k] > dist[k + 1])
                        {
                            var temp = sortOrder[k]; sortOrder[k] = sortOrder[k + 1]; sortOrder[k + 1] = temp;
                            var ftemp = dist[k]; dist[k] = dist[k + 1]; dist[k + 1] = ftemp;
                        }
                }

                originIsland = sortOrder[1];
                origin = islands[originIsland].mid;

                focus = islands[sortOrder[2]].mid;

                // determine the projection directions based on the origin island triangle
                var dir = focus - origin;
                dir.Normalize();

                ref Vector3 p1 = ref tri.verts[tri.indexes[islands[originIsland].tris[0] + 0]].xyz;
                ref Vector3 p2 = ref tri.verts[tri.indexes[islands[originIsland].tris[0] + 1]].xyz;
                ref Vector3 p3 = ref tri.verts[tri.indexes[islands[originIsland].tris[0] + 2]].xyz;

                var v1 = p2 - p1; v1.Normalize();
                var v2 = p3 - p1; v2.Normalize();

                // texVec[0] will be the normal to the origin triangle
                Vector3 texVec0 = default, texVec1 = default;
                texVec0.Cross(v1, v2);
                texVec1.Cross(texVec0, dir);

                texVec0 -= dir * (texVec0 * dir); texVec0.Normalize();
                texVec1 -= dir * (texVec1 * dir); texVec1.Normalize();

                // emit these triangles, generating the projected texcoords
                for (j = 0; j < islands[i].numTris; j++)
                    for (k = 0; k < 3; k++)
                    {
                        var index = tri.indexes[(islands[i].tris[j] * 3) + k];
                        newTri.indexes[newTri.numIndexes++] = index;
                        var local = tri.verts[index].xyz - origin;
                        ac[index].xyz = tri.verts[index].xyz;
                        ac[index].st.x = 0.5f + local * texVec0; ac[index].st.y = 0.5f + local * texVec1;
                    }
            }

            R_FinishDeform(surf, newTri, ac);
        }

        //==========================================================================================

        // Emit particles from the surface instead of drawing it
        static void R_ParticleDeform(DrawSurf surf, bool useArea)
        {
            var renderEntity = surf.space.entityDef.parms;
            var viewDef = tr.viewDef;
            var particleSystem = (DeclParticle)surf.material.DeformDecl;

            if (r_skipParticles.Bool) return;

#if false
            // the entire system has faded out
            if (renderEntity.shaderParms[SHADERPARM_PARTICLE_STOPTIME] && viewDef.renderView.time * 0.001f >= renderEntity.shaderParms[SHADERPARM_PARTICLE_STOPTIME]) return;
#endif

            // calculate the area of all the triangles
            var numSourceTris = surf.geoFrontEnd.numIndexes / 3;
            var totalArea = 0f;
            var srcTri = surf.geoFrontEnd;

            Span<float> sourceTriAreas = useArea ? stackalloc float[numSourceTris] : null;
            if (useArea)
            {
                var triNum = 0;
                for (var i = 0; i < srcTri.numIndexes; i += 3, triNum++)
                {
                    var area = Winding.TriangleArea(srcTri.verts[srcTri.indexes[i]].xyz, srcTri.verts[srcTri.indexes[i + 1]].xyz, srcTri.verts[srcTri.indexes[i + 2]].xyz);
                    sourceTriAreas[triNum] = totalArea;
                    totalArea += area;
                }
            }

            // create the particles almost exactly the way idRenderModelPrt does
            ParticleGen g = default;
            g.renderEnt = renderEntity;
            g.renderView = viewDef.renderView;
            g.origin.Zero();
            g.axis = Matrix3x3.identity;

            for (var currentTri = 0; currentTri < (useArea ? 1 : numSourceTris); currentTri++)
            {
                for (var stageNum = 0; stageNum < particleSystem.stages.Count; stageNum++)
                {
                    var stage = particleSystem.stages[stageNum];

                    // hidden - just for gui particle editor use
                    if (stage.material == null || stage.cycleMsec == 0 || stage.hidden) continue;

                    // we interpret stage.totalParticles as "particles per map square area" so the systems look the same on different size surfaces
                    var totalParticles = useArea ? (int)(stage.totalParticles * totalArea / 4096f) : stage.totalParticles;
                    var count = totalParticles * stage.NumQuadsPerParticle();

                    // allocate a srfTriangles in temp memory that can hold all the particles
                    SrfTriangles tri;

                    tri = new SrfTriangles();
                    tri.numVerts = 4 * count;
                    tri.numIndexes = 6 * count;
                    tri.verts = new DrawVert[tri.numVerts];
                    tri.indexes = new GlIndex[tri.numIndexes];

                    // just always draw the particles
                    tri.bounds = stage.bounds;

                    tri.numVerts = 0;

                    RandomX steppingRandom = default, steppingRandom2 = default;

                    var stageAge = (int)(g.renderView.time + renderEntity.shaderParms[SHADERPARM_TIMEOFFSET] * 1000 - stage.timeOffset * 1000);
                    var stageCycle = stageAge / stage.cycleMsec;

                    // some particles will be in this cycle, some will be in the previous cycle
                    steppingRandom.Seed = ((stageCycle << 10) & RandomX.MAX_RAND) ^ (int)(renderEntity.shaderParms[SHADERPARM_DIVERSITY] * RandomX.MAX_RAND);
                    steppingRandom2.Seed = (((stageCycle - 1) << 10) & RandomX.MAX_RAND) ^ (int)(renderEntity.shaderParms[SHADERPARM_DIVERSITY] * RandomX.MAX_RAND);

                    for (var index = 0; index < totalParticles; index++)
                    {
                        g.index = index;

                        // bump the random
                        steppingRandom.RandomInt();
                        steppingRandom2.RandomInt();

                        // calculate local age for this index
                        var bunchOffset = (int)(stage.particleLife * 1000 * stage.spawnBunching * index / totalParticles);

                        var particleAge = stageAge - bunchOffset;
                        var particleCycle = particleAge / stage.cycleMsec;
                        // before the particleSystem spawned or cycled systems will only run cycle times
                        if (particleCycle < 0 || (stage.cycles != 0f && particleCycle >= stage.cycles)) continue;

                        g.random = particleCycle == stageCycle ? steppingRandom : steppingRandom2;

                        var inCycleTime = particleAge - particleCycle * stage.cycleMsec;

                        // don't fire any more particles
                        if (renderEntity.shaderParms[SHADERPARM_PARTICLE_STOPTIME] != 0f && g.renderView.time - inCycleTime >= renderEntity.shaderParms[SHADERPARM_PARTICLE_STOPTIME] * 1000) continue;

                        // supress particles before or after the age clamp
                        g.frac = inCycleTime / (stage.particleLife * 1000);
                        // <0f yet to be spawned or 1f> this particle is in the deadTime band
                        if (g.frac < 0f || g.frac > 1f) continue;

                        // locate the particle origin and axis somewhere on the surface
                        var pointTri = currentTri;

                        // select a triangle based on an even area distribution
                        if (useArea) pointTri = BinSearch.LessEqual<float>(sourceTriAreas, numSourceTris, g.random.RandomFloat() * totalArea);

                        // now pick a random point inside pointTri
                        ref DrawVert v1 = ref srcTri.verts[srcTri.indexes[pointTri * 3 + 0]];
                        ref DrawVert v2 = ref srcTri.verts[srcTri.indexes[pointTri * 3 + 1]];
                        ref DrawVert v3 = ref srcTri.verts[srcTri.indexes[pointTri * 3 + 2]];

                        var f1 = g.random.RandomFloat();
                        var f2 = g.random.RandomFloat();
                        var f3 = g.random.RandomFloat();

                        var ft = 1f / (f1 + f2 + f3 + 0.0001f);

                        f1 *= ft;
                        f2 *= ft;
                        f3 *= ft;

                        g.origin = v1.xyz * f1 + v2.xyz * f2 + v3.xyz * f3;
                        g.axis[0] = v1.tangents0 * f1 + v2.tangents0 * f2 + v3.tangents0 * f3;
                        g.axis[1] = v1.tangents1 * f1 + v2.tangents1 * f2 + v3.tangents1 * f3;
                        g.axis[2] = v1.normal * f1 + v2.normal * f2 + v3.normal * f3;

                        //-----------------------

                        // this is needed so aimed particles can calculate origins at different times
                        g.originalRandom = g.random;

                        g.age = g.frac * stage.particleLife;

                        // if the particle doesn't get drawn because it is faded out or beyond a kill region, don't increment the verts
                        tri.numVerts += stage.CreateParticle(g, tri.verts + tri.numVerts);
                    }

                    if (tri.numVerts > 0)
                    {
                        // build the index list
                        var indexes = 0;
                        for (var i = 0; i < tri.numVerts; i += 4)
                        {
                            tri.indexes[indexes + 0] = i;
                            tri.indexes[indexes + 1] = i + 2;
                            tri.indexes[indexes + 2] = i + 3;
                            tri.indexes[indexes + 3] = i;
                            tri.indexes[indexes + 4] = i + 3;
                            tri.indexes[indexes + 5] = i + 1;
                            indexes += 6;
                        }
                        tri.numIndexes = indexes;
                        tri.ambientCache = vertexCache.AllocFrameTemp(tri.verts, tri.numVerts * sizeof(DrawVert), false);
                        tri.indexCache = vertexCache.AllocFrameTemp(tri.indexes, tri.numIndexes * sizeof(GlIndex), true);

                        // add the drawsurf
                        R_AddDrawSurf(tri, surf.space, renderEntity, stage.material, surf.scissorRect);
                    }
                }
            }
        }

        //========================================================================================

        public static void R_DeformDrawSurf(DrawSurf drawSurf)
        {
            if (drawSurf.material == null || r_skipDeforms.Bool) return;
            switch (drawSurf.material.Deform)
            {
                case DFRM.NONE: return;
                case DFRM.SPRITE: R_AutospriteDeform(drawSurf); break;
                case DFRM.TUBE: R_TubeDeform(drawSurf); break;
                case DFRM.FLARE: R_FlareDeform(drawSurf); break;
                case DFRM.EXPAND: R_ExpandDeform(drawSurf); break;
                case DFRM.MOVE: R_MoveDeform(drawSurf); break;
                case DFRM.TURB: R_TurbulentDeform(drawSurf); break;
                case DFRM.EYEBALL: R_EyeballDeform(drawSurf); break;
                case DFRM.PARTICLE: R_ParticleDeform(drawSurf, true); break;
                case DFRM.PARTICLE2: R_ParticleDeform(drawSurf, false); break;
            }
        }
    }
}
