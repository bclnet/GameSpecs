using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.OpenStack;
using CmHandle = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.CM
{
    partial class CM
    {
        unsafe partial class CollisionModelManagerLocal
        {
            // returns true if any of the trm vertices is inside the brush
            bool TestTrmVertsInBrush(TraceWork tw, Brush b)
            {
                int i, j, numVerts, bestPlane; float d, bestd;

                if (b.checkcount == this.checkCount) return false;
                b.checkcount = this.checkCount;

                if ((b.contents & tw.contents) == 0) return false;

                // if the brush bounds don't intersect the trace bounds
                if (!b.bounds.IntersectsBounds(tw.bounds)) return false;

                numVerts = tw.pointTrace ? 1 : tw.numVerts;

                for (j = 0; j < numVerts; j++)
                {
                    ref Vector3 p = ref tw.vertices[j].p;

                    // see if the point is inside the brush
                    bestPlane = 0;
                    bestd = -MathX.INFINITY;
                    for (i = 0; i < b.numPlanes; i++)
                    {
                        d = b.planes[i].Distance(p);
                        if (d >= 0f) break;
                        if (d > bestd) { bestd = d; bestPlane = i; }
                    }
                    if (i >= b.numPlanes)
                    {
                        tw.trace.fraction = 0f;
                        tw.trace.c.type = CONTACT.TRMVERTEX;
                        tw.trace.c.normal = b.planes[bestPlane].Normal;
                        tw.trace.c.dist = b.planes[bestPlane].Dist;
                        tw.trace.c.contents = b.contents;
                        tw.trace.c.material = b.material;
                        tw.trace.c.point = p;
                        tw.trace.c.modelFeature = 0;
                        tw.trace.c.trmFeature = j;
                        return true;
                    }
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void CM_SetTrmEdgeSidedness(ref Edge edge, Pluecker bpl, Pluecker epl, int bitNum)
            {
                if ((edge.sideSet & (1 << bitNum)) == 0)
                {
                    var fl = bpl.PermutedInnerProduct(epl);
                    edge.side = (uint)((edge.side & ~(1 << bitNum)) | (MathX.FLOATSIGNBITSET_(fl) << bitNum));
                    edge.sideSet |= (uint)(1 << bitNum);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void CM_SetTrmPolygonSidedness(ref Vertex v, Plane plane, int bitNum)
            {
                if ((v.sideSet & (1 << bitNum)) == 0)
                {
                    var fl = plane.Distance(v.p);
                    // cannot use float sign bit because it is undetermined when fl == 0f
                    if (fl < 0f) v.side |= (uint)(1 << bitNum);
                    else v.side &= (uint)~(1 << bitNum);
                    v.sideSet |= (uint)(1 << bitNum);
                }
            }

            // returns true if the trm intersects the polygon
            bool TestTrmInPolygon(TraceWork tw, Polygon p)
            {
                int i, j, k, edgeNum, flip, trmEdgeNum, bitNum, bestPlane; float d, bestd;
                var sides = stackalloc int[TraceModel.MAX_TRACEMODEL_VERTS];

                // if already checked this polygon
                if (p.checkcount == this.checkCount) return false;
                p.checkcount = this.checkCount;

                // if this polygon does not have the right contents behind it
                if ((p.contents & tw.contents) == 0) return false;

                // if the polygon bounds don't intersect the trace bounds
                if (!p.bounds.IntersectsBounds(tw.bounds)) return false;

                // bounds should cross polygon plane
                switch (tw.bounds.PlaneSide(p.plane))
                {
                    case PLANESIDE.CROSS: break;
                    case PLANESIDE.FRONT: if (tw.model.isConvex) { tw.quickExit = true; return true; } return false;
                    default: return false;
                }

                // if the trace model is convex, test if any polygon vertices are inside the trm
                if (tw.isConvex)
                    for (i = 0; i < p.numEdges; i++)
                    {
                        edgeNum = p.edges[i];
                        ref Edge edge = ref tw.model.edges[Math.Abs(edgeNum)];
                        // if this edge is already tested
                        if (edge.checkcount == this.checkCount) continue;

                        for (j = 0; j < 2; j++)
                        {
                            ref Vertex v = ref tw.model.vertices[edge.vertexNum[j]];
                            // if this vertex is already tested
                            if (v.checkcount == this.checkCount) continue;

                            bestPlane = 0;
                            bestd = -MathX.INFINITY;
                            for (k = 0; k < tw.numPolys; k++)
                            {
                                d = tw.polys[k].plane.Distance(v.p);
                                if (d >= 0f) break;
                                if (d > bestd) { bestd = d; bestPlane = k; }
                            }
                            if (k >= tw.numPolys)
                            {
                                tw.trace.fraction = 0f;
                                tw.trace.c.type = CONTACT.MODELVERTEX;
                                tw.trace.c.normal = -tw.polys[bestPlane].plane.Normal;
                                tw.trace.c.dist = -tw.polys[bestPlane].plane.Dist;
                                tw.trace.c.contents = p.contents;
                                tw.trace.c.material = p.material;
                                tw.trace.c.point = v.p;
                                tw.trace.c.modelFeature = edge.vertexNum[j];
                                tw.trace.c.trmFeature = 0;
                                return true;
                            }
                        }
                    }

                for (i = 0; i < p.numEdges; i++)
                {
                    edgeNum = p.edges[i];
                    ref Edge edge = ref tw.model.edges[Math.Abs(edgeNum)];
                    // reset sidedness cache if this is the first time we encounter this edge
                    if (edge.checkcount != this.checkCount) edge.sideSet = 0;
                    // pluecker coordinate for edge
                    tw.polygonEdgePlueckerCache[i].FromLine(tw.model.vertices[edge.vertexNum[0]].p, tw.model.vertices[edge.vertexNum[1]].p);
                    ref Vertex v = ref tw.model.vertices[edge.vertexNum[MathX.INTSIGNBITSET_(edgeNum)]];
                    // reset sidedness cache if this is the first time we encounter this vertex
                    if (v.checkcount != this.checkCount) v.sideSet = 0;
                    v.checkcount = this.checkCount;
                }

                // get side of polygon for each trm vertex
                for (i = 0; i < tw.numVerts; i++)
                {
                    d = p.plane.Distance(tw.vertices[i].p);
                    sides[i] = d < 0f ? -1 : 1;
                }

                // test if any trm edges go through the polygon
                for (i = 1; i <= tw.numEdges; i++)
                {
                    // if the trm edge does not cross the polygon plane
                    if (sides[tw.edges[i].vertexNum[0]] == sides[tw.edges[i].vertexNum[1]]) continue;
                    // check from which side to which side the trm edge goes
                    flip = MathX.INTSIGNBITSET_(sides[tw.edges[i].vertexNum[0]]);
                    // test if trm edge goes through the polygon between the polygon edges
                    for (j = 0; j < p.numEdges; j++)
                    {
                        edgeNum = p.edges[j];
                        ref Edge edge = ref tw.model.edges[Math.Abs(edgeNum)];
#if true
                        CM_SetTrmEdgeSidedness(ref edge, tw.edges[i].pl, tw.polygonEdgePlueckerCache[j], i);
                        if ((MathX.INTSIGNBITSET_(edgeNum) ^ ((edge.side >> i) & 1) ^ flip) != 0) break;
#else
                        d = tw.edges[i].pl.PermutedInnerProduct(tw.polygonEdgePlueckerCache[j]);
                        if (flip != 0) d = -d;
                        if (edgeNum > 0)
                        {
                            if (d <= 0f) break;
                        }
                        else
                        {
                            if (d >= 0f) break;
                        }
#endif
                    }
                    if (j >= p.numEdges)
                    {
                        tw.trace.fraction = 0f;
                        tw.trace.c.type = CONTACT.EDGE;
                        tw.trace.c.normal = p.plane.Normal;
                        tw.trace.c.dist = p.plane.Dist;
                        tw.trace.c.contents = p.contents;
                        tw.trace.c.material = p.material;
                        tw.trace.c.point = tw.vertices[tw.edges[i].vertexNum[!flip]].p;
                        tw.trace.c.modelFeature = reinterpret.cast(p);
                        tw.trace.c.trmFeature = i;
                        return true;
                    }
                }

                // test if any polygon edges go through the trm polygons
                for (i = 0; i < p.numEdges; i++)
                {
                    edgeNum = p.edges[i];
                    ref Edge edge = ref tw.model.edges[Math.Abs(edgeNum)];
                    if (edge.checkcount == this.checkCount) continue;
                    edge.checkcount = this.checkCount;

                    for (j = 0; j < tw.numPolys; j++)
                    {
#if true
                        ref Vertex v1 = ref tw.model.vertices[edge.vertexNum[0]];
                        CM_SetTrmPolygonSidedness(ref v1, tw.polys[j].plane, j);
                        ref Vertex v2 = ref tw.model.vertices[edge.vertexNum[1]];
                        CM_SetTrmPolygonSidedness(ref v2, tw.polys[j].plane, j);
                        // if the polygon edge does not cross the trm polygon plane
                        if ((((v1.side ^ v2.side) >> j) & 1) == 0) continue;
                        flip = (int)((v1.side >> j) & 1);
#else
                        float d1, d2;

                        v1 = tw.model.vertices + edge.vertexNum[0];
                        d1 = tw.polys[j].plane.Distance(v1.p);
                        v2 = tw.model.vertices + edge.vertexNum[1];
                        d2 = tw.polys[j].plane.Distance(v2.p);
                        // if the polygon edge does not cross the trm polygon plane
                        if ((d1 >= 0f && d2 >= 0f) || (d1 <= 0f && d2 <= 0f)) continue;
                        flip = false;
                        if (d1 < 0f) flip = true;
#endif
                        // test if polygon edge goes through the trm polygon between the trm polygon edges
                        for (k = 0; k < tw.polys[j].numEdges; k++)
                        {
                            trmEdgeNum = tw.polys[j].edges[k];
                            ref TrmEdge trmEdge = ref tw.edges[Math.Abs(trmEdgeNum)];
#if true
                            bitNum = Math.Abs(trmEdgeNum);
                            CM_SetTrmEdgeSidedness(ref edge, trmEdge.pl, tw.polygonEdgePlueckerCache[i], bitNum);
                            if ((MathX.INTSIGNBITSET_(trmEdgeNum) ^ ((edge.side >> bitNum) & 1) ^ flip) != 0) break;
#else
                            d = trmEdge.pl.PermutedInnerProduct(tw.polygonEdgePlueckerCache[i]);
                            if (flip != 0) d = -d;
                            if (trmEdgeNum > 0)
                            {
                                if (d <= 0f) break;
                            }
                            else
                            {
                                if (d >= 0f) break;
                            }
#endif
                        }
                        if (k >= tw.polys[j].numEdges)
                        {
                            tw.trace.fraction = 0f;
                            tw.trace.c.type = CONTACT.EDGE;
                            tw.trace.c.normal = -tw.polys[j].plane.Normal;
                            tw.trace.c.dist = -tw.polys[j].plane.Dist;
                            tw.trace.c.contents = p.contents;
                            tw.trace.c.material = p.material;
                            tw.trace.c.point = tw.model.vertices[edge.vertexNum[!flip]].p;
                            tw.trace.c.modelFeature = edgeNum;
                            tw.trace.c.trmFeature = j;
                            return true;
                        }
                    }
                }
                return false;
            }

            Node PointNode(in Vector3 p, Model model)
            {
                var node = model.node;
                while (node.planeType != -1)
                {
                    node = p[node.planeType] > node.planeDist ? node.children0 : node.children1;
                    Debug.Assert(node != null);
                }
                return node;
            }

            int PointContents(Vector3 p, CmHandle model)
            {
                int i; float d;

                var node = PointNode(p, this.models[model]);
                for (var bref = node.brushes; bref != null; bref = bref.next)
                {
                    var b = bref.b;
                    // test if the point is within the brush bounds
                    if (p.x < b.bounds[0].x || p.x > b.bounds[1].x ||
                        p.y < b.bounds[0].y || p.y > b.bounds[1].y ||
                        p.z < b.bounds[0].z || p.z > b.bounds[1].z)
                        continue;
                    // test if the point is inside the brush
                    var planes = b.planes;
                    for (i = 0; i < b.numPlanes; i++)
                    {
                        d = planes[i].Distance(p);
                        if (d >= 0) break;
                    }
                    if (i >= b.numPlanes) return b.contents;
                }
                return 0;
            }

            int TransformedPointContents(in Vector3 p, CmHandle model, in Vector3 origin, in Matrix3x3 modelAxis)
            {
                // subtract origin offset
                var p_l = p - origin;
                if (modelAxis.IsRotated()) p_l *= modelAxis;
                return PointContents(p_l, model);
            }


            int ContentsTrm(out Trace results, in Vector3 start, TraceModel trm, in Matrix3x3 trmAxis, int contentMask, CmHandle model, in Vector3 modelOrigin, in Matrix3x3 modelAxis)
            {
                int i; bool model_rotated, trm_rotated;
                Matrix3x3 invModelAxis = default, tmpAxis; Vector3 dir;
                TraceWork tw = new(); //: ALIGN16

                // fast point case
                if (trm == null || (
                    trm.bounds[1].x - trm.bounds[0].x <= 0f &&
                    trm.bounds[1].y - trm.bounds[0].y <= 0f &&
                    trm.bounds[1].z - trm.bounds[0].z <= 0f))
                {
                    results = new();
                    results.c.contents = TransformedPointContents(start, model, modelOrigin, modelAxis);
                    results.fraction = results.c.contents == 0 ? 0f : 1f;
                    results.endpos = start;
                    results.endAxis = trmAxis;
                    return results.c.contents;
                }

                this.checkCount++;

                tw.trace.fraction = 1f;
                tw.trace.c.contents = 0;
                tw.trace.c.type = CONTACT.NONE;
                tw.contents = contentMask;
                tw.isConvex = true;
                tw.rotation = false;
                tw.positionTest = true;
                tw.pointTrace = false;
                tw.quickExit = false;
                tw.numContacts = 0;
                tw.model = this.models[model];
                tw.start = start - modelOrigin;
                tw.end = tw.start;

                model_rotated = modelAxis.IsRotated();
                if (model_rotated) invModelAxis = modelAxis.Transpose();

                // setup trm structure
                SetupTrm(tw, trm);

                trm_rotated = trmAxis.IsRotated();

                // calculate vertex positions, rotate trm around the start position
                if (trm_rotated) for (i = 0; i < tw.numVerts; i++) tw.vertices[i].p *= trmAxis;
                // set trm at start position
                for (i = 0; i < tw.numVerts; i++) tw.vertices[i].p += tw.start;
                // rotate trm around model instead of rotating the model
                if (model_rotated) for (i = 0; i < tw.numVerts; i++) tw.vertices[i].p *= invModelAxis;

                // add offset to start point
                if (trm_rotated) { dir = trm.offset * trmAxis; tw.start += dir; tw.end += dir; }
                else { tw.start += trm.offset; tw.end += trm.offset; }
                // rotate trace instead of model
                if (model_rotated) { tw.start *= invModelAxis; tw.end *= invModelAxis; }


                // setup trm vertices, get axial trm size after rotations
                tw.size.Clear();
                for (i = 0; i < tw.numVerts; i++) tw.size.AddPoint(tw.vertices[i].p - tw.start);

                // setup trm edges, edge start, end and pluecker coordinate
                for (i = 1; i <= tw.numEdges; i++)
                {
                    tw.edges[i].start = tw.vertices[tw.edges[i].vertexNum[0]].p;
                    tw.edges[i].end = tw.vertices[tw.edges[i].vertexNum[1]].p;
                    tw.edges[i].pl.FromLine(tw.edges[i].start, tw.edges[i].end);
                }

                // setup trm polygons
                if (trm_rotated & model_rotated) { tmpAxis = trmAxis * invModelAxis; for (i = 0; i < tw.numPolys; i++) tw.polys[i].plane *= tmpAxis; }
                else if (trm_rotated) for (i = 0; i < tw.numPolys; i++) tw.polys[i].plane *= trmAxis;
                else if (model_rotated) for (i = 0; i < tw.numPolys; i++) tw.polys[i].plane *= invModelAxis;
                for (i = 0; i < tw.numPolys; i++) tw.polys[i].plane.FitThroughPoint(tw.edges[Math.Abs(tw.polys[i].edges[0])].start);

                // bounds for full trace, a little bit larger for epsilons
                for (i = 0; i < 3; i++)
                {
                    if (tw.start[i] < tw.end[i])
                    {
                        tw.bounds[0][i] = tw.start[i] + tw.size[0][i] - CM_BOX_EPSILON;
                        tw.bounds[1][i] = tw.end[i] + tw.size[1][i] + CM_BOX_EPSILON;
                    }
                    else
                    {
                        tw.bounds[0][i] = tw.end[i] + tw.size[0][i] - CM_BOX_EPSILON;
                        tw.bounds[1][i] = tw.start[i] + tw.size[1][i] + CM_BOX_EPSILON;
                    }
                    tw.extents[i] = MathX.Fabs(tw.size[MathX.Fabs(tw.size[0][i]) > MathX.Fabs(tw.size[1][i]) ? 0 : 1][i]) + CM_BOX_EPSILON;
                }

                // trace through the model
                TraceThroughModel(tw);

                results = tw.trace;
                results.fraction = results.c.contents == 0 ? 0f : 1f;
                results.endpos = start;
                results.endAxis = trmAxis;
                return results.c.contents;
            }

            // returns the contents the trm is stuck in or 0 if the trm is in free space
            public int Contents(in Vector3 start, TraceModel trm, in Matrix3x3 trmAxis, int contentMask, CmHandle model, in Vector3 modelOrigin, in Matrix3x3 modelAxis)
            {
                if (model < 0 || model > this.maxModels || model > MAX_SUBMODELS) { common.Printf("CollisionModelManagerLocal::Contents: invalid model handle\n"); return 0; }
                if (this.models == null || this.models[model] == null) { common.Printf("CollisionModelManagerLocal::Contents: invalid model\n"); return 0; }

                return ContentsTrm(out var results, start, trm, trmAxis, contentMask, model, modelOrigin, modelAxis);
            }
        }
    }
}