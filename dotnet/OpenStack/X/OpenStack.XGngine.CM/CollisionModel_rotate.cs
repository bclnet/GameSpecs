using static System.NumericsX.OpenStack.OpenStack;
using CmHandle = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.CM
{
    partial class CM
    {
        // epsilon for round-off errors in epsilon calculations
        const float CM_PL_RANGE_EPSILON = 1e-4f;
        // if the collision point is this close to the rotation axis it is not considered a collision
        const float ROTATION_AXIS_EPSILON = CM_CLIP_EPSILON * 0.25f;

        // rotates a point about an arbitrary axis using the tangent of half the rotation angle
        static void CM_RotatePoint(ref Vector3 point, in Vector3 origin, in Vector3 axis, float tanHalfAngle)
        {
            double d, t, s, c;
            Vector3 proj, v1, v2;

            point -= origin;
            proj = axis * (point * axis);
            v1 = point - proj;
            v2 = axis.Cross(v1);

            // r = tan( a / 2 );
            // sin(a) = 2*r/(1+r*r);
            // cos(a) = (1-r*r)/(1+r*r);
            t = tanHalfAngle * tanHalfAngle;
            d = 1f / (1f + t);
            s = 2f * tanHalfAngle * d;
            c = (1f - t) * d;

            point = v1 * (float)c - v2 * (float)s + proj + origin;
        }

        // rotates an edge about an arbitrary axis using the tangent of half the rotation angle
        static void CM_RotateEdge(ref Vector3 start, ref Vector3 end, in Vector3 origin, in Vector3 axis, float tanHalfAngle)
        {
            double d, t, s, c;
            Vector3 proj, v1, v2;

            // r = tan( a / 2 );
            // sin(a) = 2*r/(1+r*r);
            // cos(a) = (1-r*r)/(1+r*r);
            t = tanHalfAngle * tanHalfAngle;
            d = 1f / (1f + t);
            s = 2f * tanHalfAngle * d;
            c = (1f - t) * d;

            start -= origin;
            proj = axis * (start * axis);
            v1 = start - proj;
            v2 = axis.Cross(v1);
            start = v1 * (float)c - v2 * (float)s + proj + origin;

            end -= origin;
            proj = axis * (end * axis);
            v1 = end - proj;
            v2 = axis.Cross(v1);
            end = v1 * (float)c - v2 * (float)s + proj + origin;
        }

        unsafe partial class CollisionModelManagerLocal
        {
            // verifies if the collision of two edges occurs between the edge bounds also calculates the collision point and collision plane normal if the collision occurs between the bounds
            bool CollisionBetweenEdgeBounds(TraceWork tw, in Vector3 va, in Vector3 vb, in Vector3 vc, in Vector3 vd, float tanHalfAngle, out Vector3 collisionPoint, out Vector3 collisionNormal)
            {
                float d1, d2, d;
                Vector3 at, bt, dir, dir1, dir2;
                Pluecker pl1 = new(), pl2 = new();

                at = va;
                bt = vb;
                if (tanHalfAngle != 0f) CM_RotateEdge(ref at, ref bt, tw.origin, tw.axis, tanHalfAngle);

                dir1 = (at - tw.origin).Cross(tw.axis);
                dir2 = (bt - tw.origin).Cross(tw.axis);
                dir = dir1 * dir1 > dir2 * dir2 ? dir1 : dir2;
                if (tw.angle < 0f) dir = -dir;

                pl1.FromLine(at, bt);
                pl2.FromRay(vc, dir);
                d1 = pl1.PermutedInnerProduct(pl2);
                pl2.FromRay(vd, dir);
                d2 = pl1.PermutedInnerProduct(pl2);
                if ((d1 > 0f && d2 > 0f) || (d1 < 0f && d2 < 0f)) return false;

                pl1.FromLine(vc, vd);
                pl2.FromRay(at, dir);
                d1 = pl1.PermutedInnerProduct(pl2);
                pl2.FromRay(bt, dir);
                d2 = pl1.PermutedInnerProduct(pl2);
                if ((d1 > 0f && d2 > 0f) || (d1 < 0f && d2 < 0f)) return false;

                // collision point on the edge at-bt
                dir1 = (vd - vc).Cross(dir);
                d = dir1 * vc;
                d1 = dir1 * at - d;
                d2 = dir1 * bt - d;
                if (d1 == d2) return false;
                collisionPoint = at + (d1 / (d1 - d2)) * (bt - at);

                // normal is cross product of the rotated edge va-vb and the edge vc-vd
                collisionNormal.Cross(bt - at, vd - vc);

                return true;
            }

            // calculates the tangent of half the rotation angle at which the edges collide
            bool RotateEdgeThroughEdge(TraceWork tw, in Pluecker pl1, in Vector3 vc, in Vector3 vd, float minTan, out float tanHalfAngle)
            {
                double v0, v1, v2, a, b, c, d, sqrtd, q, frac1, frac2;
                Vector3 ct, dt;
                Pluecker pl2 = new();

                /*

                a = start of line being rotated
                b = end of line being rotated
                pl1 = pluecker coordinate for line (a - b)
                pl2 = pluecker coordinate for edge we might collide with (c - d)
                t = rotation angle around the z-axis
                solve pluecker inner product for t of rotating line a-b and line l2

                // start point of rotated line during rotation
                an[0] = a[0] * cos(t) + a[1] * sin(t)
                an[1] = a[0] * -sin(t) + a[1] * cos(t)
                an[2] = a[2];
                // end point of rotated line during rotation
                bn[0] = b[0] * cos(t) + b[1] * sin(t)
                bn[1] = b[0] * -sin(t) + b[1] * cos(t)
                bn[2] = b[2];

                pl1[0] = a[0] * b[1] - b[0] * a[1];
                pl1[1] = a[0] * b[2] - b[0] * a[2];
                pl1[2] = a[0] - b[0];
                pl1[3] = a[1] * b[2] - b[1] * a[2];
                pl1[4] = a[2] - b[2];
                pl1[5] = b[1] - a[1];

                v[0] = (a[0] * cos(t) + a[1] * sin(t)) * (b[0] * -sin(t) + b[1] * cos(t)) - (b[0] * cos(t) + b[1] * sin(t)) * (a[0] * -sin(t) + a[1] * cos(t));
                v[1] = (a[0] * cos(t) + a[1] * sin(t)) * b[2] - (b[0] * cos(t) + b[1] * sin(t)) * a[2];
                v[2] = (a[0] * cos(t) + a[1] * sin(t)) - (b[0] * cos(t) + b[1] * sin(t));
                v[3] = (a[0] * -sin(t) + a[1] * cos(t)) * b[2] - (b[0] * -sin(t) + b[1] * cos(t)) * a[2];
                v[4] = a[2] - b[2];
                v[5] = (b[0] * -sin(t) + b[1] * cos(t)) - (a[0] * -sin(t) + a[1] * cos(t));

                pl2[0] * v[4] + pl2[1] * v[5] + pl2[2] * v[3] + pl2[4] * v[0] + pl2[5] * v[1] + pl2[3] * v[2] = 0;

                v[0] = (a[0] * cos(t) + a[1] * sin(t)) * (b[0] * -sin(t) + b[1] * cos(t)) - (b[0] * cos(t) + b[1] * sin(t)) * (a[0] * -sin(t) + a[1] * cos(t));
                v[0] = (a[1] * b[1] - a[0] * b[0]) * cos(t) * sin(t) + (a[0] * b[1] + a[1] * b[0] * cos(t)^2) - (a[1] * b[0]) - ((b[1] * a[1] - b[0] * a[0]) * cos(t) * sin(t) + (b[0] * a[1] + b[1] * a[0]) * cos(t)^2 - (b[1] * a[0]))
                v[0] = - (a[1] * b[0]) - ( - (b[1] * a[0]))
                v[0] = (b[1] * a[0]) - (a[1] * b[0])

                v[0] = (a[0]*b[1]) - (a[1]*b[0]);
                v[1] = (a[0]*b[2] - b[0]*a[2]) * cos(t) + (a[1]*b[2] - b[1]*a[2]) * sin(t);
                v[2] = (a[0]-b[0]) * cos(t) + (a[1]-b[1]) * sin(t);
                v[3] = (b[0]*a[2] - a[0]*b[2]) * sin(t) + (a[1]*b[2] - b[1]*a[2]) * cos(t);
                v[4] = a[2] - b[2];
                v[5] = (a[0]-b[0]) * sin(t) + (b[1]-a[1]) * cos(t);

                v[0] = (a[0]*b[1]) - (a[1]*b[0]);
                v[1] = (a[0]*b[2] - b[0]*a[2]) * cos(t) + (a[1]*b[2] - b[1]*a[2]) * sin(t);
                v[2] = (a[0]-b[0]) * cos(t) - (b[1]-a[1]) * sin(t);
                v[3] = (a[0]*b[2] - b[0]*a[2]) * -sin(t) + (a[1]*b[2] - b[1]*a[2]) * cos(t);
                v[4] = a[2] - b[2];
                v[5] = (a[0]-b[0]) * sin(t) + (b[1]-a[1]) * cos(t);

                v[0] = pl1[0];
                v[1] = pl1[1] * cos(t) + pl1[3] * sin(t);
                v[2] = pl1[2] * cos(t) - pl1[5] * sin(t);
                v[3] = pl1[3] * cos(t) - pl1[1] * sin(t);
                v[4] = pl1[4];
                v[5] = pl1[5] * cos(t) + pl1[2] * sin(t);

                pl2[0] * v[4] + pl2[1] * v[5] + pl2[2] * v[3] + pl2[4] * v[0] + pl2[5] * v[1] + pl2[3] * v[2] = 0;

                0 =	pl2[0] * pl1[4] +
                    pl2[1] * (pl1[5] * cos(t) + pl1[2] * sin(t)) +
                    pl2[2] * (pl1[3] * cos(t) - pl1[1] * sin(t)) +
                    pl2[4] * pl1[0] +
                    pl2[5] * (pl1[1] * cos(t) + pl1[3] * sin(t)) +
                    pl2[3] * (pl1[2] * cos(t) - pl1[5] * sin(t));

                v2 * cos(t) + v1 * sin(t) + v0 = 0;

                // rotation about the z-axis
                v0 = pl2[0] * pl1[4] + pl2[4] * pl1[0];
                v1 = pl2[1] * pl1[2] - pl2[2] * pl1[1] + pl2[5] * pl1[3] - pl2[3] * pl1[5];
                v2 = pl2[1] * pl1[5] + pl2[2] * pl1[3] + pl2[5] * pl1[1] + pl2[3] * pl1[2];

                // rotation about the x-axis
                //v0 = pl2[3] * pl1[2] + pl2[2] * pl1[3];
                //v1 = -pl2[5] * pl1[0] + pl2[4] * pl1[1] - pl2[1] * pl1[4] + pl2[0] * pl1[5];
                //v2 = pl2[4] * pl1[0] + pl2[5] * pl1[1] + pl2[0] * pl1[4] + pl2[1] * pl1[5];

                r = tan(t / 2);
                sin(t) = 2*r/(1+r*r);
                cos(t) = (1-r*r)/(1+r*r);

                v1 * 2 * r / (1 + r*r) + v2 * (1 - r*r) / (1 + r*r) + v0 = 0
                (v1 * 2 * r + v2 * (1 - r*r)) / (1 + r*r) = -v0
                (v1 * 2 * r + v2 - v2 * r*r) / (1 + r*r) = -v0
                v1 * 2 * r + v2 - v2 * r*r = -v0 * (1 + r*r)
                v1 * 2 * r + v2 - v2 * r*r = -v0 + -v0 * r*r
                (v0 - v2) * r * r + (2 * v1) * r + (v0 + v2) = 0;

                MrE gives Pluecker a banana.. good monkey

                */

                tanHalfAngle = tw.maxTan;

                // transform rotation axis to z-axis
                ct = (vc - tw.origin) * tw.matrix;
                dt = (vd - tw.origin) * tw.matrix;

                pl2.FromLine(ct, dt);

                v0 = pl2[0] * pl1[4] + pl2[4] * pl1[0];
                v1 = pl2[1] * pl1[2] - pl2[2] * pl1[1] + pl2[5] * pl1[3] - pl2[3] * pl1[5];
                v2 = pl2[1] * pl1[5] + pl2[2] * pl1[3] + pl2[5] * pl1[1] + pl2[3] * pl1[2];

                a = v0 - v2;
                b = v1;
                c = v0 + v2;
                if (a == 0f)
                {
                    if (b == 0f) return false;
                    frac1 = -c / (2f * b);
                    frac2 = 1e10;   // = tan( idMath::HALF_PI )
                }
                else
                {
                    d = b * b - c * a;
                    if (d <= 0f) return false;
                    sqrtd = Math.Sqrt(d);
                    q = b > 0f ? -b + sqrtd : -b - sqrtd;
                    frac1 = q / a;
                    frac2 = c / q;
                }

                if (tw.angle < 0f) { frac1 = -frac1; frac2 = -frac2; }

                // get smallest tangent for which a collision occurs
                if (frac1 >= minTan && frac1 < tanHalfAngle) tanHalfAngle = (float)frac1;
                if (frac2 >= minTan && frac2 < tanHalfAngle) tanHalfAngle = (float)frac2;

                if (tw.angle < 0f) tanHalfAngle = -tanHalfAngle;

                return true;
            }

            // calculates the direction of motion at the initial position, where dir < 0 means the edges move towards each other if the edges move away from each other the tangent of half the rotation angle at which
            // the edges are furthest apart is also calculated
            bool EdgeFurthestFromEdge(TraceWork tw, in Pluecker pl1, in Vector3 vc, in Vector3 vd, out float tanHalfAngle, out float dir)
            {
                double v0, v1, v2, a, b, c, d, sqrtd, q, frac1, frac2;
                Vector3 ct, dt;
                Pluecker pl2 = new();

                /*

                v2 * cos(t) + v1 * sin(t) + v0 = 0;

                // rotation about the z-axis
                v0 = pl2[0] * pl1[4] + pl2[4] * pl1[0];
                v1 = pl2[1] * pl1[2] - pl2[2] * pl1[1] + pl2[5] * pl1[3] - pl2[3] * pl1[5];
                v2 = pl2[1] * pl1[5] + pl2[2] * pl1[3] + pl2[5] * pl1[1] + pl2[3] * pl1[2];

                derivative:
                v1 * cos(t) - v2 * sin(t) = 0;

                r = tan(t / 2);
                sin(t) = 2*r/(1+r*r);
                cos(t) = (1-r*r)/(1+r*r);

                -v2 * 2 * r / (1 + r*r) + v1 * (1 - r*r)/(1+r*r);
                -v2 * 2 * r + v1 * (1 - r*r) / (1 + r*r) = 0;
                -v2 * 2 * r + v1 * (1 - r*r) = 0;
                (-v1) * r * r + (-2 * v2) * r + (v1) = 0;

                */

                tanHalfAngle = 0f;

                // transform rotation axis to z-axis
                ct = (vc - tw.origin) * tw.matrix;
                dt = (vd - tw.origin) * tw.matrix;

                pl2.FromLine(ct, dt);

                v0 = pl2[0] * pl1[4] + pl2[4] * pl1[0];
                v1 = pl2[1] * pl1[2] - pl2[2] * pl1[1] + pl2[5] * pl1[3] - pl2[3] * pl1[5];
                v2 = pl2[1] * pl1[5] + pl2[2] * pl1[3] + pl2[5] * pl1[1] + pl2[3] * pl1[2];

                // get the direction of motion at the initial position
                c = v0 + v2;
                dir = tw.angle > 0f
                    ? c > 0f ? (float)v1 : (float)-v1
                    : c > 0f ? (float)-v1 : (float)v1;
                // negative direction means the edges move towards each other at the initial position
                if (dir <= 0f) return true;

                a = -v1;
                b = -v2;
                c = v1;
                if (a == 0f)
                {
                    if (b == 0f) return false;
                    frac1 = -c / (2f * b);
                    frac2 = 1e10;   // = tan( idMath::HALF_PI )
                }
                else
                {
                    d = b * b - c * a;
                    if (d <= 0f) return false;
                    sqrtd = Math.Sqrt(d);
                    q = b > 0f ? -b + sqrtd : -b - sqrtd;
                    frac1 = q / a;
                    frac2 = c / q;
                }

                if (tw.angle < 0f) { frac1 = -frac1; frac2 = -frac2; }

                if (frac1 < 0f && frac2 < 0f) return false;

                tanHalfAngle = frac1 > frac2 ? (float)frac1 : (float)frac2;

                if (tw.angle < 0f) tanHalfAngle = -tanHalfAngle;

                return true;
            }

            void RotateTrmEdgeThroughPolygon(TraceWork tw, Polygon poly, TrmEdge trmEdge)
            {
                int i, j, edgeNum;
                float f1, f2, startTan, dir, tanHalfAngle;
                Vector3 collisionPoint, collisionNormal, origin, epsDir;
                Pluecker epsPl = new();
                Bounds bounds = new();

                // if the trm is convex and the rotation axis intersects the trm
                if (tw.isConvex && tw.axisIntersectsTrm)
                    // if both points are behind the polygon the edge cannot collide within a 180 degrees rotation
                    if (tw.vertices[trmEdge.vertexNum[0]].polygonSide & tw.vertices[trmEdge.vertexNum[1]].polygonSide) return;

                // if the trace model edge rotation bounds do not intersect the polygon bounds
                if (!trmEdge.rotationBounds.IntersectsBounds(poly.bounds)) return;

                // edge rotation bounds should cross polygon plane
                if (trmEdge.rotationBounds.PlaneSide(poly.plane) != PLANESIDE.CROSS) return;

                // check edges for a collision
                for (i = 0; i < poly.numEdges; i++)
                {
                    edgeNum = poly.edges[i];
                    ref Edge edge = ref tw.model.edges[Math.Abs(edgeNum)];

                    // if this edge is already checked
                    if (edge.checkcount == this.checkCount) continue;

                    // can never collide with internal edges
                    if (edge.internal_ != 0) continue;

                    ref Vertex v1 = ref tw.model.vertices[edge.vertexNum[MathX.INTSIGNBITSET_(edgeNum)]];
                    ref Vertex v2 = ref tw.model.vertices[edge.vertexNum[MathX.INTSIGNBITNOTSET_(edgeNum)]];

                    // edge bounds
                    for (j = 0; j < 3; j++)
                        if (v1.p[j] > v2.p[j]) { bounds[0][j] = v2.p[j]; bounds[1][j] = v1.p[j]; }
                        else { bounds[0][j] = v1.p[j]; bounds[1][j] = v2.p[j]; }

                    // if the trace model edge rotation bounds do not intersect the polygon edge bounds
                    if (!trmEdge.rotationBounds.IntersectsBounds(bounds)) continue;

                    f1 = trmEdge.pl.PermutedInnerProduct(tw.polygonEdgePlueckerCache[i]);

                    // pluecker coordinate for epsilon expanded edge
                    epsDir = edge.normal * (CM_CLIP_EPSILON + CM_PL_RANGE_EPSILON);
                    epsPl.FromLine(tw.model.vertices[edge.vertexNum[0]].p + epsDir, tw.model.vertices[edge.vertexNum[1]].p + epsDir);

                    f2 = trmEdge.pl.PermutedInnerProduct(epsPl);

                    // if the rotating edge is inbetween the polygon edge and the epsilon expanded edge
                    if ((f1 < 0f && f2 > 0f) || (f1 > 0f && f2 < 0f))
                    {
                        if (!EdgeFurthestFromEdge(tw, trmEdge.plzaxis, v1.p, v2.p, out startTan, out dir)) continue;

                        // moving towards the polygon edge so stop immediately
                        if (dir <= 0f) tanHalfAngle = 0f;
                        // never going to get beyond the start tangent during the current rotation
                        else if (MathX.Fabs(startTan) >= tw.maxTan) continue;
                        // collide with the epsilon expanded edge
                        else if (!RotateEdgeThroughEdge(tw, trmEdge.plzaxis, v1.p + epsDir, v2.p + epsDir, MathX.Fabs(startTan), out tanHalfAngle)) tanHalfAngle = startTan;
                    }
                    else
                    {
                        // collide with the epsilon expanded edge
                        epsDir = edge.normal * CM_CLIP_EPSILON;
                        if (!RotateEdgeThroughEdge(tw, trmEdge.plzaxis, v1.p + epsDir, v2.p + epsDir, 0f, out tanHalfAngle)) continue;
                    }

                    if (MathX.Fabs(tanHalfAngle) >= tw.maxTan) continue;

                    // check if the collision is between the edge bounds
                    if (!CollisionBetweenEdgeBounds(tw, trmEdge.start, trmEdge.end, v1.p, v2.p, tanHalfAngle, out collisionPoint, out collisionNormal)) continue;

                    // allow rotation if the rotation axis goes through the collisionPoint
                    origin = tw.origin + tw.axis * (tw.axis * (collisionPoint - tw.origin));
                    if ((collisionPoint - origin).LengthSqr < ROTATION_AXIS_EPSILON * ROTATION_AXIS_EPSILON) continue;

                    // fill in trace structure
                    tw.maxTan = MathX.Fabs(tanHalfAngle);
                    tw.trace.c.normal = collisionNormal;
                    tw.trace.c.normal.Normalize();
                    tw.trace.c.dist = tw.trace.c.normal * v1.p;
                    // make sure the collision plane faces the trace model
                    if ((tw.trace.c.normal * trmEdge.start) - tw.trace.c.dist < 0)
                    {
                        tw.trace.c.normal = -tw.trace.c.normal;
                        tw.trace.c.dist = -tw.trace.c.dist;
                    }
                    tw.trace.c.contents = poly.contents;
                    tw.trace.c.material = poly.material;
                    tw.trace.c.type = CONTACT_EDGE;
                    tw.trace.c.modelFeature = edgeNum;
                    tw.trace.c.trmFeature = trmEdge - tw.edges;
                    tw.trace.c.point = collisionPoint;
                    // if no collision can be closer
                    if (tw.maxTan == 0f) break;
                }
            }

            // calculates the tangent of half the rotation angle at which the point collides with the plane
            bool RotatePointThroughPlane(TraceWork tw, in Vector3 point, in Plane plane, float angle, float minTan, out float tanHalfAngle)
            {
                double v0, v1, v2, a, b, c, d, sqrtd, q, frac1, frac2;
                Vector3 p, normal;

                /*

                p[0] = point[0] * cos(t) + point[1] * sin(t)
                p[1] = point[0] * -sin(t) + point[1] * cos(t)
                p[2] = point[2];

                normal[0] * (p[0] * cos(t) + p[1] * sin(t)) +
                    normal[1] * (p[0] * -sin(t) + p[1] * cos(t)) +
                        normal[2] * p[2] + dist = 0

                normal[0] * p[0] * cos(t) + normal[0] * p[1] * sin(t) +
                    -normal[1] * p[0] * sin(t) + normal[1] * p[1] * cos(t) +
                        normal[2] * p[2] + dist = 0

                v2 * cos(t) + v1 * sin(t) + v0

                // rotation about the z-axis
                v0 = normal[2] * p[2] + dist
                v1 = normal[0] * p[1] - normal[1] * p[0]
                v2 = normal[0] * p[0] + normal[1] * p[1]

                r = tan(t / 2);
                sin(t) = 2*r/(1+r*r);
                cos(t) = (1-r*r)/(1+r*r);

                v1 * 2 * r / (1 + r*r) + v2 * (1 - r*r) / (1 + r*r) + v0 = 0
                (v1 * 2 * r + v2 * (1 - r*r)) / (1 + r*r) = -v0
                (v1 * 2 * r + v2 - v2 * r*r) / (1 + r*r) = -v0
                v1 * 2 * r + v2 - v2 * r*r = -v0 * (1 + r*r)
                v1 * 2 * r + v2 - v2 * r*r = -v0 + -v0 * r*r
                (v0 - v2) * r * r + (2 * v1) * r + (v0 + v2) = 0;

                */

                tanHalfAngle = tw.maxTan;

                // transform rotation axis to z-axis
                p = (point - tw.origin) * tw.matrix;
                d = plane.d + plane.Normal * tw.origin;
                normal = plane.Normal * tw.matrix;

                v0 = normal.z * p.z + d;
                v1 = normal.x * p.y - normal.y * p.x;
                v2 = normal.x * p.x + normal.y * p.y;

                a = v0 - v2;
                b = v1;
                c = v0 + v2;
                if (a == 0f)
                {
                    if (b == 0f) return false;
                    frac1 = -c / (2f * b);
                    frac2 = 1e10;   // = tan( idMath::HALF_PI )
                }
                else
                {
                    d = b * b - c * a;
                    if (d <= 0f) return false;
                    sqrtd = Math.Sqrt(d);
                    q = b > 0f ? -b + sqrtd : -b - sqrtd;
                    frac1 = q / a;
                    frac2 = c / q;
                }

                if (angle < 0f) { frac1 = -frac1; frac2 = -frac2; }

                // get smallest tangent for which a collision occurs
                if (frac1 >= minTan && frac1 < tanHalfAngle) tanHalfAngle = (float)frac1;
                if (frac2 >= minTan && frac2 < tanHalfAngle) tanHalfAngle = (float)frac2;

                if (angle < 0f) tanHalfAngle = -tanHalfAngle;

                return true;
            }

            // calculates the direction of motion at the initial position, where dir < 0 means the point moves towards the plane if the point moves away from the plane the tangent of half the rotation angle at which
            // the point is furthest away from the plane is also calculated
            bool PointFurthestFromPlane(TraceWork tw, in Vector3 point, in Plane plane, float angle, out float tanHalfAngle, out float dir)
            {
                double v1, v2, a, b, c, d, sqrtd, q, frac1, frac2;
                Vector3 p, normal;

                /*

                v2 * cos(t) + v1 * sin(t) + v0 = 0;

                // rotation about the z-axis
                v0 = normal[2] * p[2] + dist
                v1 = normal[0] * p[1] - normal[1] * p[0]
                v2 = normal[0] * p[0] + normal[1] * p[1]

                derivative:
                v1 * cos(t) - v2 * sin(t) = 0;

                r = tan(t / 2);
                sin(t) = 2*r/(1+r*r);
                cos(t) = (1-r*r)/(1+r*r);

                -v2 * 2 * r / (1 + r*r) + v1 * (1 - r*r)/(1+r*r);
                -v2 * 2 * r + v1 * (1 - r*r) / (1 + r*r) = 0;
                -v2 * 2 * r + v1 * (1 - r*r) = 0;
                (-v1) * r * r + (-2 * v2) * r + (v1) = 0;

                */

                tanHalfAngle = 0f;

                // transform rotation axis to z-axis
                p = (point - tw.origin) * tw.matrix;
                normal = plane.Normal * tw.matrix;

                v1 = normal.x * p.y - normal.y * p.x;
                v2 = normal.x * p.x + normal.y * p.y;

                // the point will always start at the front of the plane, therefore v0 + v2 > 0 is always true
                dir = angle < 0f ? (float)-v1 : (float)v1;
                // negative direction means the point moves towards the plane at the initial position
                if (dir <= 0f) return true;

                a = -v1;
                b = -v2;
                c = v1;
                if (a == 0f)
                {
                    if (b == 0f) return false;
                    frac1 = -c / (2f * b);
                    frac2 = 1e10;   // = tan( idMath::HALF_PI )
                }
                else
                {
                    d = b * b - c * a;
                    if (d <= 0f) return false;
                    sqrtd = Math.Sqrt(d);
                    q = b > 0f ? -b + sqrtd : -b - sqrtd;
                    frac1 = q / a;
                    frac2 = c / q;
                }

                if (angle < 0f) { frac1 = -frac1; frac2 = -frac2; }

                if (frac1 < 0f && frac2 < 0f) return false;

                tanHalfAngle = frac1 > frac2 ? (float)frac1 : (float)frac2;

                if (angle < 0f) tanHalfAngle = -tanHalfAngle;

                return true;
            }

            bool RotatePointThroughEpsilonPlane(TraceWork tw, in Vector3 point, in Vector3 endPoint, in Plane plane, float angle, in Vector3 origin, out float tanHalfAngle, out Vector3 collisionPoint, out Vector3 endDir)
            {
                float d, dir, startTan;
                Vector3 vec, startDir;
                Plane epsPlane;

                // epsilon expanded plane
                epsPlane = plane;
                epsPlane.Dist = epsPlane.Dist + CM_CLIP_EPSILON;

                // if the rotation sphere at the rotation origin is too far away from the polygon plane
                d = epsPlane.Distance(origin);
                vec = point - origin;
                if (d * d > vec * vec) return false;

                // calculate direction of motion at vertex start position
                startDir = (point - origin).Cross(tw.axis);
                if (angle < 0f) startDir = -startDir;
                // if moving away from plane at start position
                if (startDir * epsPlane.Normal >= 0f)
                {
                    // if end position is outside epsilon range
                    d = epsPlane.Distance(endPoint);
                    if (d >= 0f) return false;   // no collision
                    // calculate direction of motion at vertex end position
                    endDir = (endPoint - origin).Cross(tw.axis);
                    if (angle < 0f) endDir = -endDir;
                    // if also moving away from plane at end position
                    if (endDir * epsPlane.Normal > 0f) return false; // no collision
                }

                // if the start position is in the epsilon range
                d = epsPlane.Distance(point);
                if (d <= CM_PL_RANGE_EPSILON)
                {

                    // calculate tangent of half the rotation for which the vertex is furthest away from the plane
                    if (!PointFurthestFromPlane(tw, point, plane, angle, out startTan, out dir)) return false;

                    // moving towards the polygon plane so stop immediately
                    if (dir <= 0f) tanHalfAngle = 0f;
                    // never going to get beyond the start tangent during the current rotation
                    else if (MathX.Fabs(startTan) >= tw.maxTan) return false;
                    // calculate collision with epsilon expanded plane
                    else if (!RotatePointThroughPlane(tw, point, epsPlane, angle, MathX.Fabs(startTan), out tanHalfAngle)) tanHalfAngle = startTan;
                }
                // calculate collision with epsilon expanded plane
                else if (!RotatePointThroughPlane(tw, point, epsPlane, angle, 0f, out tanHalfAngle)) return false;

                // calculate collision point
                collisionPoint = point;
                if (tanHalfAngle != 0f) CM_RotatePoint(ref collisionPoint, tw.origin, tw.axis, tanHalfAngle);
                // calculate direction of motion at collision point
                endDir = (collisionPoint - origin).Cross(tw.axis);
                if (angle < 0f) endDir = -endDir;
                return true;
            }

            void RotateTrmVertexThroughPolygon(TraceWork tw, Polygon poly, TrmVertex v, int vertexNum)
            {
                int i;
                float tanHalfAngle;
                Vector3 endDir, collisionPoint;
                Pluecker pl = new();

                // if the trm vertex is behind the polygon plane it cannot collide with the polygon within a 180 degrees rotation
                if (tw.isConvex && tw.axisIntersectsTrm && v.polygonSide) return;

                // if the trace model vertex rotation bounds do not intersect the polygon bounds
                if (!v.rotationBounds.IntersectsBounds(poly.bounds)) return;

                // vertex rotation bounds should cross polygon plane
                if (v.rotationBounds.PlaneSide(poly.plane) != SIDE_CROSS) return;

                // rotate the vertex through the epsilon plane
                if (!RotatePointThroughEpsilonPlane(tw, v.p, v.endp, poly.plane, tw.angle, v.rotationOrigin, out tanHalfAngle, out collisionPoint, out endDir)) return;

                if (MathX.Fabs(tanHalfAngle) < tw.maxTan)
                {
                    // verify if 'collisionPoint' moving along 'endDir' moves between polygon edges
                    pl.FromRay(collisionPoint, endDir);
                    for (i = 0; i < poly.numEdges; i++)
                        if (poly.edges[i] < 0)
                        {
                            if (pl.PermutedInnerProduct(tw.polygonEdgePlueckerCache[i]) > 0f) return;
                        }
                        else
                        {
                            if (pl.PermutedInnerProduct(tw.polygonEdgePlueckerCache[i]) < 0f) return;
                        }
                    tw.maxTan = MathX.Fabs(tanHalfAngle);
                    // collision plane is the polygon plane
                    tw.trace.c.normal = poly.plane.Normal;
                    tw.trace.c.dist = poly.plane.Dist;
                    tw.trace.c.contents = poly.contents;
                    tw.trace.c.material = poly.material;
                    tw.trace.c.type = CONTACT_TRMVERTEX;
                    tw.trace.c.modelFeature = *reinterpret_cast<int*>(&poly);
                    tw.trace.c.trmFeature = v - tw.vertices;
                    tw.trace.c.point = collisionPoint;
                }
            }

            void RotateVertexThroughTrmPolygon(TraceWork tw, TrmPolygon trmpoly, Polygon poly, Vertex v, in Vector3 rotationOrigin)
            {
                int i, edgeNum;
                float tanHalfAngle;
                Vector3 dir, endp, endDir, collisionPoint;
                Pluecker pl;
                TrmEdge edge;

                // if the polygon vertex is behind the trm plane it cannot collide with the trm polygon within a 180 degrees rotation
                if (tw.isConvex && tw.axisIntersectsTrm && trmpoly.plane.Distance(v.p) < 0f) return;

                // if the model vertex is outside the trm polygon rotation bounds
                if (!trmpoly.rotationBounds.ContainsPoint(v.p)) return;

                // if the rotation axis goes through the polygon vertex
                dir = v.p - rotationOrigin;
                if (dir * dir < ROTATION_AXIS_EPSILON * ROTATION_AXIS_EPSILON) return;

                // calculate vertex end position
                endp = v.p;
                tw.modelVertexRotation.RotatePoint(ref endp);

                // rotate the vertex through the epsilon plane
                if (!RotatePointThroughEpsilonPlane(tw, v.p, endp, trmpoly.plane, -tw.angle, rotationOrigin, out tanHalfAngle, out collisionPoint, out endDir)) return;

                if (idMath::Fabs(tanHalfAngle) < tw.maxTan)
                {
                    // verify if 'collisionPoint' moving along 'endDir' moves between polygon edges
                    pl.FromRay(collisionPoint, endDir);
                    for (i = 0; i < trmpoly.numEdges; i++)
                    {
                        edgeNum = trmpoly.edges[i];
                        edge = tw.edges + abs(edgeNum);
                        if (edgeNum < 0)
                        {
                            if (pl.PermutedInnerProduct(edge.pl) > 0f)
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (pl.PermutedInnerProduct(edge.pl) < 0f)
                            {
                                return;
                            }
                        }
                    }
                    tw.maxTan = idMath::Fabs(tanHalfAngle);
                    // collision plane is the flipped trm polygon plane
                    tw.trace.c.normal = -trmpoly.plane.Normal();
                    tw.trace.c.dist = tw.trace.c.normal * v.p;
                    tw.trace.c.contents = poly.contents;
                    tw.trace.c.material = poly.material;
                    tw.trace.c.type = CONTACT_MODELVERTEX;
                    tw.trace.c.modelFeature = v - tw.model.vertices;
                    tw.trace.c.trmFeature = trmpoly - tw.polys;
                    tw.trace.c.point = v.p;
                }
            }

            /*
            ================
            idCollisionModelManagerLocal::RotateTrmThroughPolygon

              returns true if the polygon blocks the complete rotation
            ================
            */
            bool idCollisionModelManagerLocal::RotateTrmThroughPolygon(cm_traceWork_t* tw, cm_polygon_t* p)
            {
                int i, j, k, edgeNum;
                float d;
                cm_trmVertex_t* bv;
                cm_trmEdge_t* be;
                cm_trmPolygon_t* bp;
                cm_vertex_t* v;
                cm_edge_t* e;
                Vector3* rotationOrigin;

                // if already checked this polygon
                if (p.checkcount == idCollisionModelManagerLocal::checkCount)
                {
                    return false;
                }
                p.checkcount = idCollisionModelManagerLocal::checkCount;

                // if this polygon does not have the right contents behind it
                if (!(p.contents & tw.contents))
                {
                    return false;
                }

                // if the the trace bounds do not intersect the polygon bounds
                if (!tw.bounds.IntersectsBounds(p.bounds))
                {
                    return false;
                }

                // back face culling
                if (tw.isConvex)
                {
                    // if the center of the convex trm is behind the polygon plane
                    if (p.plane.Distance(tw.start) < 0f)
                    {
                        // if the rotation axis intersects the trace model
                        if (tw.axisIntersectsTrm)
                        {
                            return false;
                        }
                        else
                        {
                            // if the direction of motion at the start and end position of the
                            // center of the trm both go towards or away from the polygon plane
                            // or if the intersections of the rotation axis with the expanded heart planes
                            // are both in front of the polygon plane
                        }
                    }
                }

                // if the polygon is too far from the first heart plane
                d = p.bounds.PlaneDistance(tw.heartPlane1);
                if (idMath::Fabs(d) > tw.maxDistFromHeartPlane1)
                {
                    return false;
                }

                // rotation bounds should cross polygon plane
                switch (tw.bounds.PlaneSide(p.plane))
                {
                    case PLANESIDE_CROSS:
                        break;
                    case PLANESIDE_FRONT:
                        if (tw.model.isConvex)
                        {
                            tw.quickExit = true;
                            return true;
                        }
                    default:
                        return false;
                }

                for (i = 0; i < tw.numVerts; i++)
                {
                    bv = tw.vertices + i;
                    // calculate polygon side this vertex is on
                    d = p.plane.Distance(bv.p);
                    bv.polygonSide = FLOATSIGNBITSET(d);
                }

                for (i = 0; i < p.numEdges; i++)
                {
                    edgeNum = p.edges[i];
                    e = tw.model.edges + abs(edgeNum);
                    v = tw.model.vertices + e.vertexNum[INTSIGNBITSET(edgeNum)];

                    // pluecker coordinate for edge
                    tw.polygonEdgePlueckerCache[i].FromLine(tw.model.vertices[e.vertexNum[0]].p,
                                                                tw.model.vertices[e.vertexNum[1]].p);

                    // calculate rotation origin projected into rotation plane through the vertex
                    tw.polygonRotationOriginCache[i] = tw.origin + tw.axis * (tw.axis * (v.p - tw.origin));
                }
                // copy first to last so we can easily cycle through
                tw.polygonRotationOriginCache[p.numEdges] = tw.polygonRotationOriginCache[0];

                // fast point rotation
                if (tw.pointTrace)
                {
                    RotateTrmVertexThroughPolygon(tw, p, &tw.vertices[0], 0);
                }
                else
                {
                    // rotate trm vertices through polygon
                    for (i = 0; i < tw.numVerts; i++)
                    {
                        bv = tw.vertices + i;
                        if (bv.used)
                        {
                            RotateTrmVertexThroughPolygon(tw, p, bv, i);
                        }
                    }

                    // rotate trm edges through polygon
                    for (i = 1; i <= tw.numEdges; i++)
                    {
                        be = tw.edges + i;
                        if (be.used)
                        {
                            RotateTrmEdgeThroughPolygon(tw, p, be);
                        }
                    }

                    // rotate all polygon vertices through the trm
                    for (i = 0; i < p.numEdges; i++)
                    {
                        edgeNum = p.edges[i];
                        e = tw.model.edges + abs(edgeNum);

                        if (e.checkcount == idCollisionModelManagerLocal::checkCount)
                        {
                            continue;
                        }
                        // set edge check count
                        e.checkcount = idCollisionModelManagerLocal::checkCount;
                        // can never collide with internal edges
                        if (e.internal ) {
    continue;
}
// got to check both vertices because we skip internal edges
for (k = 0; k< 2; k++)
{

    v = tw.model.vertices + e.vertexNum[k ^ INTSIGNBITSET(edgeNum)];

    // if this vertex is already checked
    if (v.checkcount == idCollisionModelManagerLocal::checkCount)
    {
        continue;
    }
    // set vertex check count
    v.checkcount = idCollisionModelManagerLocal::checkCount;

    // if the vertex is outside the trm rotation bounds
    if (!tw.bounds.ContainsPoint(v.p))
    {
        continue;
    }

    rotationOrigin = &tw.polygonRotationOriginCache[i + k];

    for (j = 0; j<tw.numPolys; j++)
    {
        bp = tw.polys + j;
        if (bp.used)
        {
            RotateVertexThroughTrmPolygon(tw, bp, p, v, * rotationOrigin);
        }
    }
}
		}
	}

	return (tw.maxTan == 0f);
}

/*
================
idCollisionModelManagerLocal::BoundsForRotation

  only for rotations < 180 degrees
================
*/
void idCollisionModelManagerLocal::BoundsForRotation( const Vector3 &origin, const Vector3 &axis, const Vector3 &start, const Vector3 &end, idBounds &bounds )
{
    int i;
    float radiusSqr;
    Vector3 v1, v2;

    radiusSqr = (start - origin).LengthSqr();
    v1 = (start - origin).Cross(axis);
    v2 = (end - origin).Cross(axis);

    for (i = 0; i < 3; i++)
    {
        // if the derivative changes sign along this axis during the rotation from start to end
        if ((v1[i] > 0f && v2[i] < 0f) || (v1[i] < 0f && v2[i] > 0f))
        {
            if ((0.5f * (start[i] + end[i]) - origin[i]) > 0f)
            {
                bounds[0][i] = Min(start[i], end[i]);
                bounds[1][i] = origin[i] + idMath::Sqrt(radiusSqr * (1f - axis[i] * axis[i]));
            }
            else
            {
                bounds[0][i] = origin[i] - idMath::Sqrt(radiusSqr * (1f - axis[i] * axis[i]));
                bounds[1][i] = Max(start[i], end[i]);
            }
        }
        else if (start[i] > end[i])
        {
            bounds[0][i] = end[i];
            bounds[1][i] = start[i];
        }
        else
        {
            bounds[0][i] = start[i];
            bounds[1][i] = end[i];
        }
        // expand for epsilons
        bounds[0][i] -= CM_BOX_EPSILON;
        bounds[1][i] += CM_BOX_EPSILON;
    }
}

/*
================
idCollisionModelManagerLocal::Rotation180
================
*/
void idCollisionModelManagerLocal::Rotation180(trace_t* results, const Vector3 &rorg, const Vector3 &axis,
										const float startAngle, const float endAngle, const Vector3 &start,
										const idTraceModel* trm, const idMat3 &trmAxis, int contentMask,
                                        cmHandle_t model, const Vector3 &modelOrigin, const idMat3 &modelAxis )
{
    int i, j, edgeNum;
    float d, maxErr, initialTan;
    bool model_rotated, trm_rotated;
    Vector3 dir, dir1, dir2, tmp, vr, vup, org, at, bt;
    idMat3 invModelAxis, endAxis, tmpAxis;
    idRotation startRotation, endRotation;
    idPluecker plaxis;
    cm_trmPolygon_t* poly;
    cm_trmEdge_t* edge;
    cm_trmVertex_t* vert;
    ALIGN16( static cm_traceWork_t tw );

    if (model < 0 || model > MAX_SUBMODELS || model > idCollisionModelManagerLocal::maxModels)
    {
        common.Printf("idCollisionModelManagerLocal::Rotation180: invalid model handle\n");
        return;
    }
    if (!idCollisionModelManagerLocal::models[model])
    {
        common.Printf("idCollisionModelManagerLocal::Rotation180: invalid model\n");
        return;
    }

    idCollisionModelManagerLocal::checkCount++;

    tw.trace.fraction = 1f;
    tw.trace.c.contents = 0;
    tw.trace.c.type = CONTACT_NONE;
    tw.contents = contentMask;
    tw.isConvex = true;
    tw.rotation = true;
    tw.positionTest = false;
    tw.axisIntersectsTrm = false;
    tw.quickExit = false;
    tw.angle = endAngle - startAngle;
    assert(tw.angle > -180f && tw.angle < 180f);
    tw.angle = idMath::ClampFloat(-180f, 180f, tw.angle); // DG: enforce it for the rare cases the assert would trigger
    tw.maxTan = initialTan = idMath::Fabs(tan((idMath::PI / 360f) * tw.angle));
    tw.model = idCollisionModelManagerLocal::models[model];
    tw.start = start - modelOrigin;
    // rotation axis, axis is assumed to be normalized
    tw.axis = axis;
    assert(tw.axis[0] * tw.axis[0] + tw.axis[1] * tw.axis[1] + tw.axis[2] * tw.axis[2] > 0.99f);
    // rotation origin projected into rotation plane through tw.start
    tw.origin = rorg - modelOrigin;
    d = (tw.axis * tw.origin) - (tw.axis * tw.start);
    tw.origin = tw.origin - d * tw.axis;
    // radius of rotation
    tw.radius = (tw.start - tw.origin).Length();
    // maximum error of the circle approximation traced through the axial BSP tree
    d = tw.radius * tw.radius - (CIRCLE_APPROXIMATION_LENGTH * CIRCLE_APPROXIMATION_LENGTH * 0.25f);
    if (d > 0f)
    {
        maxErr = tw.radius - idMath::Sqrt(d);
    }
    else
    {
        maxErr = tw.radius;
    }

    model_rotated = modelAxis.IsRotated();
    if (model_rotated)
    {
        invModelAxis = modelAxis.Transpose();
        tw.axis *= invModelAxis;
        tw.origin *= invModelAxis;
    }

    startRotation.Set(tw.origin, tw.axis, startAngle);
    endRotation.Set(tw.origin, tw.axis, endAngle);

    // create matrix which rotates the rotation axis to the z-axis
    tw.axis.NormalVectors(vr, vup);
    tw.matrix[0][0] = vr[0];
    tw.matrix[1][0] = vr[1];
    tw.matrix[2][0] = vr[2];
    tw.matrix[0][1] = -vup[0];
    tw.matrix[1][1] = -vup[1];
    tw.matrix[2][1] = -vup[2];
    tw.matrix[0][2] = tw.axis[0];
    tw.matrix[1][2] = tw.axis[1];
    tw.matrix[2][2] = tw.axis[2];

    // if optimized point trace
    if (!trm || (trm.bounds[1][0] - trm.bounds[0][0] <= 0f &&
                    trm.bounds[1][1] - trm.bounds[0][1] <= 0f &&
                    trm.bounds[1][2] - trm.bounds[0][2] <= 0f))
    {

        if (model_rotated)
        {
            // rotate trace instead of model
            tw.start *= invModelAxis;
        }
        tw.end = tw.start;
        // if we start at a specific angle
        if (startAngle != 0f)
        {
            startRotation.RotatePoint(tw.start);
        }
        // calculate end position of rotation
        endRotation.RotatePoint(tw.end);

        // calculate rotation origin projected into rotation plane through the vertex
        tw.numVerts = 1;
        tw.vertices[0].p = tw.start;
        tw.vertices[0].endp = tw.end;
        tw.vertices[0].used = true;
        tw.vertices[0].rotationOrigin = tw.origin + tw.axis * (tw.axis * (tw.vertices[0].p - tw.origin));
        BoundsForRotation(tw.vertices[0].rotationOrigin, tw.axis, tw.start, tw.end, tw.vertices[0].rotationBounds);
        // rotation bounds
        tw.bounds = tw.vertices[0].rotationBounds;
        tw.numEdges = tw.numPolys = 0;

        // collision with single point
        tw.pointTrace = true;

        // extents is set to maximum error of the circle approximation traced through the axial BSP tree
        tw.extents[0] = tw.extents[1] = tw.extents[2] = maxErr + CM_BOX_EPSILON;

        // setup rotation heart plane
        tw.heartPlane1.SetNormal(tw.axis);
        tw.heartPlane1.FitThroughPoint(tw.start);
        tw.maxDistFromHeartPlane1 = CM_BOX_EPSILON;

        // trace through the model
        idCollisionModelManagerLocal::TraceThroughModel(&tw);

        // store results
        *results = tw.trace;
        results.endpos = start;
        if (tw.maxTan == initialTan)
        {
            results.fraction = 1f;
        }
        else
        {
            results.fraction = idMath::Fabs(atan(tw.maxTan) * (2f * 180f / idMath::PI) / tw.angle);
        }
        assert(results.fraction <= 1f);
        endRotation.Set(rorg, axis, startAngle + (endAngle - startAngle) * results.fraction);
        endRotation.RotatePoint(results.endpos);
        results.endAxis.Identity();

        if (results.fraction < 1f)
        {
            // rotate trace plane normal if there was a collision with a rotated model
            if (model_rotated)
            {
                results.c.normal *= modelAxis;
                results.c.point *= modelAxis;
            }
            results.c.point += modelOrigin;
            results.c.dist += modelOrigin * results.c.normal;
        }
        return;
    }

    tw.pointTrace = false;

    // setup trm structure
    idCollisionModelManagerLocal::SetupTrm(&tw, trm);

    trm_rotated = trmAxis.IsRotated();

    // calculate vertex positions
    if (trm_rotated)
    {
        for (i = 0; i < tw.numVerts; i++)
        {
            // rotate trm around the start position
            tw.vertices[i].p *= trmAxis;
        }
    }
    for (i = 0; i < tw.numVerts; i++)
    {
        // set trm at start position
        tw.vertices[i].p += tw.start;
    }
    if (model_rotated)
    {
        for (i = 0; i < tw.numVerts; i++)
        {
            tw.vertices[i].p *= invModelAxis;
        }
    }
    for (i = 0; i < tw.numVerts; i++)
    {
        tw.vertices[i].endp = tw.vertices[i].p;
    }
    // if we start at a specific angle
    if (startAngle != 0f)
    {
        for (i = 0; i < tw.numVerts; i++)
        {
            startRotation.RotatePoint(tw.vertices[i].p);
        }
    }
    for (i = 0; i < tw.numVerts; i++)
    {
        // end position of vertex
        endRotation.RotatePoint(tw.vertices[i].endp);
    }

    // add offset to start point
    if (trm_rotated)
    {
        tw.start += trm.offset * trmAxis;
    }
    else
    {
        tw.start += trm.offset;
    }
    // if the model is rotated
    if (model_rotated)
    {
        // rotate trace instead of model
        tw.start *= invModelAxis;
    }
    tw.end = tw.start;
    // if we start at a specific angle
    if (startAngle != 0f)
    {
        startRotation.RotatePoint(tw.start);
    }
    // calculate end position of rotation
    endRotation.RotatePoint(tw.end);

    // setup trm vertices
    for (vert = tw.vertices, i = 0; i < tw.numVerts; i++, vert++)
    {
        // calculate rotation origin projected into rotation plane through the vertex
        vert.rotationOrigin = tw.origin + tw.axis * (tw.axis * (vert.p - tw.origin));
        // calculate rotation bounds for this vertex
        BoundsForRotation(vert.rotationOrigin, tw.axis, vert.p, vert.endp, vert.rotationBounds);
        // if the rotation axis goes through the vertex then the vertex is not used
        d = (vert.p - vert.rotationOrigin).LengthSqr();
        if (d > ROTATION_AXIS_EPSILON * ROTATION_AXIS_EPSILON)
        {
            vert.used = true;
        }
    }

    // setup trm edges
    for (edge = tw.edges + 1, i = 1; i <= tw.numEdges; i++, edge++)
    {
        // if the rotation axis goes through both the edge vertices then the edge is not used
        if (tw.vertices[edge.vertexNum[0]].used | tw.vertices[edge.vertexNum[1]].used)
        {
            edge.used = true;
        }
        // edge start, end and pluecker coordinate
        edge.start = tw.vertices[edge.vertexNum[0]].p;
        edge.end = tw.vertices[edge.vertexNum[1]].p;
        edge.pl.FromLine(edge.start, edge.end);
        // pluecker coordinate for edge being rotated about the z-axis
        at = (edge.start - tw.origin) * tw.matrix;
        bt = (edge.end - tw.origin) * tw.matrix;
        edge.plzaxis.FromLine(at, bt);
        // get edge rotation bounds from the rotation bounds of both vertices
        edge.rotationBounds = tw.vertices[edge.vertexNum[0]].rotationBounds;
        edge.rotationBounds.AddBounds(tw.vertices[edge.vertexNum[1]].rotationBounds);
        // used to calculate if the rotation axis intersects the trm
        edge.bitNum = 0;
    }

    tw.bounds.Clear();

    // rotate trm polygon planes
    if (trm_rotated & model_rotated)
    {
        tmpAxis = trmAxis * invModelAxis;
        for (poly = tw.polys, i = 0; i < tw.numPolys; i++, poly++)
        {
            poly.plane *= tmpAxis;
        }
    }
    else if (trm_rotated)
    {
        for (poly = tw.polys, i = 0; i < tw.numPolys; i++, poly++)
        {
            poly.plane *= trmAxis;
        }
    }
    else if (model_rotated)
    {
        for (poly = tw.polys, i = 0; i < tw.numPolys; i++, poly++)
        {
            poly.plane *= invModelAxis;
        }
    }

    // setup trm polygons
    for (poly = tw.polys, i = 0; i < tw.numPolys; i++, poly++)
    {
        poly.used = true;
        // set trm polygon plane distance
        poly.plane.FitThroughPoint(tw.edges[abs(poly.edges[0])].start);
        // get polygon bounds from edge bounds
        poly.rotationBounds.Clear();
        for (j = 0; j < poly.numEdges; j++)
        {
            // add edge rotation bounds to polygon rotation bounds
            edge = &tw.edges[abs(poly.edges[j])];
            poly.rotationBounds.AddBounds(edge.rotationBounds);
        }
        // get trace bounds from polygon bounds
        tw.bounds.AddBounds(poly.rotationBounds);
    }

    // extents including the maximum error of the circle approximation traced through the axial BSP tree
    for (i = 0; i < 3; i++)
    {
        tw.size[0][i] = tw.bounds[0][i] - tw.start[i];
        tw.size[1][i] = tw.bounds[1][i] - tw.start[i];
        if (idMath::Fabs(tw.size[0][i]) > idMath::Fabs(tw.size[1][i]))
        {
            tw.extents[i] = idMath::Fabs(tw.size[0][i]) + maxErr + CM_BOX_EPSILON;
        }
        else
        {
            tw.extents[i] = idMath::Fabs(tw.size[1][i]) + maxErr + CM_BOX_EPSILON;
        }
    }

    // for back-face culling
    if (tw.isConvex)
    {
        if (tw.start == tw.origin)
        {
            tw.axisIntersectsTrm = true;
        }
        else
        {
            // determine if the rotation axis intersects the trm
            plaxis.FromRay(tw.origin, tw.axis);
            for (poly = tw.polys, i = 0; i < tw.numPolys; i++, poly++)
            {
                // back face cull polygons
                if (poly.plane.Normal() * tw.axis > 0f)
                {
                    continue;
                }
                // test if the axis goes between the polygon edges
                for (j = 0; j < poly.numEdges; j++)
                {
                    edgeNum = poly.edges[j];
                    edge = tw.edges + abs(edgeNum);
                    if (!(edge.bitNum & 2))
                    {
                        d = plaxis.PermutedInnerProduct(edge.pl);
                        edge.bitNum = FLOATSIGNBITSET(d) | 2;
                    }
                    if ((edge.bitNum ^ INTSIGNBITSET(edgeNum)) & 1)
                    {
                        break;
                    }
                }
                if (j >= poly.numEdges)
                {
                    tw.axisIntersectsTrm = true;
                    break;
                }
            }
        }
    }

    // setup rotation heart plane
    tw.heartPlane1.SetNormal(tw.axis);
    tw.heartPlane1.FitThroughPoint(tw.start);
    tw.maxDistFromHeartPlane1 = 0f;
    for (i = 0; i < tw.numVerts; i++)
    {
        d = idMath::Fabs(tw.heartPlane1.Distance(tw.vertices[i].p));
        if (d > tw.maxDistFromHeartPlane1)
        {
            tw.maxDistFromHeartPlane1 = d;
        }
    }
    tw.maxDistFromHeartPlane1 += CM_BOX_EPSILON;

    // inverse rotation to rotate model vertices towards trace model
    tw.modelVertexRotation.Set(tw.origin, tw.axis, -tw.angle);

    // trace through the model
    idCollisionModelManagerLocal::TraceThroughModel(&tw);

    // store results
    *results = tw.trace;
    results.endpos = start;
    if (tw.maxTan == initialTan)
    {
        results.fraction = 1f;
    }
    else
    {
        results.fraction = idMath::Fabs(atan(tw.maxTan) * (2f * 180f / idMath::PI) / tw.angle);
    }
    assert(results.fraction <= 1f);
    endRotation.Set(rorg, axis, startAngle + (endAngle - startAngle) * results.fraction);
    endRotation.RotatePoint(results.endpos);
    results.endAxis = trmAxis * endRotation.ToMat3();

    if (results.fraction < 1f)
    {
        // rotate trace plane normal if there was a collision with a rotated model
        if (model_rotated)
        {
            results.c.normal *= modelAxis;
            results.c.point *= modelAxis;
        }
        results.c.point += modelOrigin;
        results.c.dist += modelOrigin * results.c.normal;
    }
}

/*
================
idCollisionModelManagerLocal::Rotation
================
*/
void idCollisionModelManagerLocal::Rotation(trace_t* results, const Vector3 &start, const idRotation &rotation,
										const idTraceModel* trm, const idMat3 &trmAxis, int contentMask,
                                        cmHandle_t model, const Vector3 &modelOrigin, const idMat3 &modelAxis )
{
    Vector3 tmp;
    float maxa, stepa, a, lasta;

    assert(((byte*)&start) < ((byte*)results) || ((byte*)&start) > (((byte*)results) + sizeof(trace_t)));
    assert(((byte*)&trmAxis) < ((byte*)results) || ((byte*)&trmAxis) > (((byte*)results) + sizeof(trace_t)));

    memset(results, 0, sizeof( *results) );

    // if special position test
    if (rotation.GetAngle() == 0f)
    {
        idCollisionModelManagerLocal::ContentsTrm(results, start, trm, trmAxis, contentMask, model, modelOrigin, modelAxis);
        return;
    }

    if (rotation.GetAngle() >= 180f || rotation.GetAngle() <= -180f)
    {
        if (rotation.GetAngle() >= 360f)
        {
            maxa = 360f;
            stepa = 120f;         // three steps strictly < 180 degrees
        }
        else if (rotation.GetAngle() <= -360f)
        {
            maxa = -360f;
            stepa = -120f;        // three steps strictly < 180 degrees
        }
        else
        {
            maxa = rotation.GetAngle();
            stepa = rotation.GetAngle() * 0.5f; // two steps strictly < 180 degrees
        }
        for (lasta = 0f, a = stepa; fabs(a) < fabs(maxa) + 1f; lasta = a, a += stepa)
        {
            // partial rotation
            idCollisionModelManagerLocal::Rotation180(results, rotation.GetOrigin(), rotation.GetVec(), lasta, a, start, trm, trmAxis, contentMask, model, modelOrigin, modelAxis);
            // if there is a collision
            if (results.fraction < 1f)
            {
                // fraction of total rotation
                results.fraction = (lasta + stepa * results.fraction) / rotation.GetAngle();
                return;
            }
        }
        results.fraction = 1f;
        return;
    }

    idCollisionModelManagerLocal::Rotation180(results, rotation.GetOrigin(), rotation.GetVec(), 0f, rotation.GetAngle(), start, trm, trmAxis, contentMask, model, modelOrigin, modelAxis);

# ifdef _DEBUG
    // test for collisions
    if (cm_debugCollision.GetBool())
    {
        // if the trm is stuck in the model
        if (idCollisionModelManagerLocal::Contents(results.endpos, trm, results.endAxis, -1, model, modelOrigin, modelAxis) & contentMask)
        {
            trace_t tr;

            // test where the trm is stuck in the model
            idCollisionModelManagerLocal::Contents(results.endpos, trm, results.endAxis, -1, model, modelOrigin, modelAxis);
            // re-run collision detection to find out where it failed
            idCollisionModelManagerLocal::Rotation(&tr, start, rotation, trm, trmAxis, contentMask, model, modelOrigin, modelAxis);
        }
    }
#endif
}
