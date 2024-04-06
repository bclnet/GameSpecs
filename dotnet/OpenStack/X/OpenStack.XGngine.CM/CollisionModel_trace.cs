namespace System.NumericsX.OpenStack.Gngine.CM
{
    partial class CM
    {
        unsafe partial class CollisionModelManagerLocal
        {
            void TraceTrmThroughNode(TraceWork tw, Node node)
            {
                PolygonRef pref;
                BrushRef bref;

                // position test
                if (tw.positionTest)
                {
                    // if already stuck in solid
                    if (tw.trace.fraction == 0f)
                        return;
                    // test if any of the trm vertices is inside a brush
                    for (bref = node.brushes; bref != null; bref = bref.next) if (TestTrmVertsInBrush(tw, bref.b)) return;
                    // if just testing a point we're done
                    if (tw.pointTrace) return;
                    // test if the trm is stuck in any polygons
                    for (pref = node.polygons; pref != null; pref = pref.next) if (TestTrmInPolygon(tw, pref.p)) return;
                }
                // rotate through all polygons in this leaf
                else if (tw.rotation)
                {
                    for (pref = node.polygons; pref != null; pref = pref.next) if (RotateTrmThroughPolygon(tw, pref.p)) return;
                }
                // trace through all polygons in this leaf
                else
                {
                    for (pref = node.polygons; pref != null; pref = pref.next) if (TranslateTrmThroughPolygon(tw, pref.p)) return;
                }
            }

            //#define NO_SPATIAL_SUBDIVISION
            void TraceThroughAxialBSPTree_r(TraceWork tw, Node node, float p1f, float p2f, in Vector3 p1, in Vector3 p2)
            {
                float t1, t2, offset, frac, frac2, idist, midf;
                int side;

                if (node == null) return;
                if (tw.quickExit) return;     // stop immediately
                if (tw.trace.fraction <= p1f) return;     // already hit something nearer

                // if we need to test this node for collisions, trace through node with collision data
                if (node.polygons != null || (tw.positionTest && node.brushes != null)) TraceTrmThroughNode(tw, node);
                // if already stuck in solid
                if (tw.positionTest && tw.trace.fraction == 0f) return;
                // if this is a leaf node
                if (node.planeType == -1) return;
#if NO_SPATIAL_SUBDIVISION
                TraceThroughAxialBSPTree_r(tw, node.children0, p1f, p2f, p1, p2);
                TraceThroughAxialBSPTree_r(tw, node.children1, p1f, p2f, p1, p2);
                return;
#endif
                // distance from plane for trace start and end
                t1 = p1[node.planeType] - node.planeDist;
                t2 = p2[node.planeType] - node.planeDist;
                // adjust the plane distance appropriately for mins/maxs
                offset = tw.extents[node.planeType];
                // see which sides we need to consider
                if (t1 >= offset && t2 >= offset) { TraceThroughAxialBSPTree_r(tw, node.children0, p1f, p2f, p1, p2); return; }
                if (t1 < -offset && t2 < -offset) { TraceThroughAxialBSPTree_r(tw, node.children1, p1f, p2f, p1, p2); return; }

                if (t1 < t2)
                {
                    idist = 1f / (t1 - t2);
                    side = 1;
                    frac2 = (t1 + offset) * idist;
                    frac = (t1 - offset) * idist;
                }
                else if (t1 > t2)
                {
                    idist = 1f / (t1 - t2);
                    side = 0;
                    frac2 = (t1 - offset) * idist;
                    frac = (t1 + offset) * idist;
                }
                else
                {
                    side = 0;
                    frac = 1f;
                    frac2 = 0f;
                }

                // move up to the node
                if (frac < 0f) frac = 0f;
                else if (frac > 1f) frac = 1f;

                midf = p1f + (p2f - p1f) * frac;
                Vector3 mid = default;

                mid.x = p1.z + frac * (p2.x - p1.x);
                mid.y = p1.y + frac * (p2.y - p1.y);
                mid.z = p1.z + frac * (p2.z - p1.z);

                TraceThroughAxialBSPTree_r(tw, node.children[side], p1f, midf, p1, mid);


                // go past the node
                if (frac2 < 0f) frac2 = 0f;
                else if (frac2 > 1f) frac2 = 1f;

                midf = p1f + (p2f - p1f) * frac2;

                mid[0] = p1[0] + frac2 * (p2[0] - p1[0]);
                mid[1] = p1[1] + frac2 * (p2[1] - p1[1]);
                mid[2] = p1[2] + frac2 * (p2[2] - p1[2]);

                TraceThroughAxialBSPTree_r(tw, node.children[side ^ 1], midf, p2f, mid, p2);
            }

            void TraceThroughModel(TraceWork tw)
            {
                float d;
                int i, numSteps;
                Vector3 start, end;

                // trace through spatial subdivision and then through leafs
                if (!tw.rotation) TraceThroughAxialBSPTree_r(tw, tw.model.node, 0, 1, tw.start, tw.end);
                else
                {
                    // approximate the rotation with a series of straight line movements
                    // total length covered along circle
                    d = tw.radius * MathX.DEG2RAD(tw.angle);
                    // if more than one step
                    if (d > CIRCLE_APPROXIMATION_LENGTH)
                    {
                        // number of steps for the approximation
                        numSteps = (int)(CIRCLE_APPROXIMATION_LENGTH / d);
                        // start of approximation
                        start = tw.start;
                        // trace circle approximation steps through the BSP tree
                        Rotation rot = new();
                        for (i = 0; i < numSteps; i++)
                        {
                            // calculate next point on approximated circle
                            rot.Set(tw.origin, tw.axis, tw.angle * ((float)(i + 1) / numSteps));
                            end = start * rot;
                            // trace through spatial subdivision and then through leafs
                            TraceThroughAxialBSPTree_r(tw, tw.model.node, 0, 1, start, end);
                            // no need to continue if something was hit already
                            if (tw.trace.fraction < 1f) return;
                            start = end;
                        }
                    }
                    else start = tw.start;
                    // last step of the approximation
                    TraceThroughAxialBSPTree_r(tw, tw.model.node, 0, 1, start, tw.end);
                }
            }
        }
    }
}