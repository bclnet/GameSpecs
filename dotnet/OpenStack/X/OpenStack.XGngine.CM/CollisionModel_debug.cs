using System.Text;
using static System.NumericsX.OpenStack.OpenStack;
using CmHandle = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.CM
{
    partial class CM
    {
        unsafe partial class CollisionModelManagerLocal
        {
            static string[] cm_contentsNameByIndex = {
                "none",							// 0
	            "solid",						// 1
	            "opaque",						// 2
	            "water",						// 3
	            "playerclip",					// 4
	            "monsterclip",					// 5
	            "moveableclip",					// 6
	            "ikclip",						// 7
	            "blood",						// 8
	            "body",							// 9
	            "corpse",						// 10
	            "trigger",						// 11
	            "aas_solid",					// 12
	            "aas_obstacle",					// 13
	            "flashlight_trigger",			// 14
	            null
            };

            static int[] cm_contentsFlagByIndex = {
                -1,								// 0
	            CONTENTS_SOLID,					// 1
	            CONTENTS_OPAQUE,				// 2
	            CONTENTS_WATER,					// 3
	            CONTENTS_PLAYERCLIP,			// 4
	            CONTENTS_MONSTERCLIP,			// 5
	            CONTENTS_MOVEABLECLIP,			// 6
	            CONTENTS_IKCLIP,				// 7
	            CONTENTS_BLOOD,					// 8
	            CONTENTS_BODY,					// 9
	            CONTENTS_CORPSE,				// 10
	            CONTENTS_TRIGGER,				// 11
	            CONTENTS_AAS_SOLID,				// 12
	            CONTENTS_AAS_OBSTACLE,			// 13
	            CONTENTS_FLASHLIGHT_TRIGGER,	// 14
	            0
            };

            static CVar cm_drawMask = new("cm_drawMask", "none", CVAR.GAME, "collision mask", cm_contentsNameByIndex, CmdArgs.ArgCompletion_String(cm_contentsNameByIndex));
            static CVar cm_drawColor = new("cm_drawColor", "1 0 0 .5", CVAR.GAME, "color used to draw the collision models");
            static CVar cm_drawFilled = new("cm_drawFilled", "0", CVAR.GAME | CVAR.BOOL, "draw filled polygons");
            static CVar cm_drawInternal = new("cm_drawInternal", "1", CVAR.GAME | CVAR.BOOL, "draw internal edges green");
            static CVar cm_drawNormals = new("cm_drawNormals", "0", CVAR.GAME | CVAR.BOOL, "draw polygon and edge normals");
            static CVar cm_backFaceCull = new("cm_backFaceCull", "0", CVAR.GAME | CVAR.BOOL, "cull back facing polygons");
            static CVar cm_debugCollision = new("cm_debugCollision", "0", CVAR.GAME | CVAR.BOOL, "debug the collision detection");

            static Vector4 cm_color;

            int ContentsFromString(string s)
            {
                int i, contents = 0;
                Lexer src = new(s, s.Length, "ContentsFromString");

                while (src.ReadToken(out var token))
                {
                    if (token == ",") continue;
                    for (i = 1; cm_contentsNameByIndex[i] != null; i++)
                        if (string.Equals(token, cm_contentsNameByIndex[i], StringComparison.OrdinalIgnoreCase)) { contents |= cm_contentsFlagByIndex[i]; break; }
                }

                return contents;
            }

            static StringBuilder StringFromContents_contentsString = new();
            string StringFromContents(int contents)
            {
                StringFromContents_contentsString.Clear();
                for (var i = 1; cm_contentsFlagByIndex[i] != 0; i++)
                    if ((contents & cm_contentsFlagByIndex[i]) != 0)
                    {
                        if (StringFromContents_contentsString.Length != 0) StringFromContents_contentsString.Append(',');
                        StringFromContents_contentsString.Append(cm_contentsNameByIndex[i]);
                    }

                return StringFromContents_contentsString.ToString();
            }

            void DrawEdge(Model model, int edgeNum, in Vector3 origin, in Matrix3x3 axis)
            {
                int side; Vector3 start, end, mid;

                var isRotated = axis.IsRotated();

                ref Edge edge = ref model.edges[Math.Abs(edgeNum)];
                side = edgeNum < 0 ? 1 : 0;

                start = model.vertices[edge.vertexNum[side]].p;
                end = model.vertices[edge.vertexNum[!side]].p;
                if (isRotated) { start *= axis; end *= axis; }
                start += origin; end += origin;

                if (edge.internal_ != 0)
                {
                    if (cm_drawInternal.Bool) session.rw.DebugArrow(colorGreen, start, end, 1);
                }
                else session.rw.DebugArrow(edge.numUsers > 2 ? colorBlue : cm_color, start, end, 1);

                if (cm_drawNormals.Bool)
                {
                    mid = (start + end) * 0.5f;
                    end = mid + 5 * (isRotated ? (axis * edge.normal) : edge.normal);
                    session.rw.DebugArrow(colorCyan, mid, end, 1);
                }
            }

            void DrawPolygon(Model model, Polygon p, in Vector3 origin, in Matrix3x3 axis, in Vector3 viewOrigin)
            {
                int i, edgeNum;
                Vector3 center, end, dir;

                if (cm_backFaceCull.Bool)
                {
                    edgeNum = p.edges[0];
                    ref Edge edge = ref model.edges[Math.Abs(edgeNum)];
                    dir = model.vertices[edge.vertexNum[0]].p - viewOrigin;
                    if (dir * p.plane.Normal > 0f) return;
                }

                if (cm_drawNormals.Bool)
                {
                    center = Vector3.origin;
                    for (i = 0; i < p.numEdges; i++)
                    {
                        edgeNum = p.edges[i];
                        ref Edge edge = ref model.edges[Math.Abs(edgeNum)];
                        center += model.vertices[edge.vertexNum[edgeNum < 0 ? 1 : 0]].p;
                    }
                    center *= 1f / p.numEdges;
                    if (axis.IsRotated()) { center = center * axis + origin; end = center + 5 * (axis * p.plane.Normal); }
                    else { center += origin; end = center + 5 * p.plane.Normal; }
                    session.rw.DebugArrow(colorMagenta, center, end, 1);
                }

                if (cm_drawFilled.Bool)
                {
                    FixedWinding winding = new();
                    for (i = p.numEdges - 1; i >= 0; i--)
                    {
                        edgeNum = p.edges[i];
                        ref Edge edge = ref model.edges[Math.Abs(edgeNum)];
                        winding += origin + model.vertices[edge.vertexNum[MathX.INTSIGNBITSET_(edgeNum)]].p * axis;
                    }
                    session.rw.DebugPolygon(cm_color, winding);
                }
                else
                    for (i = 0; i < p.numEdges; i++)
                    {
                        edgeNum = p.edges[i];
                        ref Edge edge = ref model.edges[Math.Abs(edgeNum)];
                        if (edge.checkcount == checkCount) continue;
                        edge.checkcount = checkCount;
                        DrawEdge(model, edgeNum, origin, axis);
                    }
            }

            void DrawNodePolygons(Model model, Node node, in Vector3 origin, in Matrix3x3 axis, in Vector3 viewOrigin, float radius)
            {
                Polygon p; PolygonRef pref;

                while (true)
                {
                    for (pref = node.polygons; pref != null; pref = pref.next)
                    {
                        p = pref.p;
                        // polygon bounds should overlap with trace bounds
                        if (radius != 0f && (
                            p.bounds[0].x > viewOrigin.x + radius || p.bounds[1].x < viewOrigin.x - radius ||
                            p.bounds[0].y > viewOrigin.y + radius || p.bounds[1].y < viewOrigin.y - radius ||
                            p.bounds[0].z > viewOrigin.z + radius || p.bounds[1].z < viewOrigin.z - radius))
                            continue;
                        if (p.checkcount == checkCount) continue;
                        if ((p.contents & cm_contentsFlagByIndex[cm_drawMask.Integer]) == 0) continue;

                        DrawPolygon(model, p, origin, axis, viewOrigin);
                        p.checkcount = checkCount;
                    }
                    if (node.planeType == -1) break;
                    if (radius != 0f && viewOrigin[node.planeType] > node.planeDist + radius) node = node.children0;
                    else if (radius != 0f && viewOrigin[node.planeType] < node.planeDist - radius) node = node.children1;
                    else { DrawNodePolygons(model, node.children1, origin, axis, viewOrigin, radius); node = node.children0; }
                }
            }

            void DrawModel(CmHandle handle, in Vector3 modelOrigin, in Matrix3x3 modelAxis, in Vector3 viewOrigin, float radius)
            {
                Model model; Vector3 viewPos;

                if (handle < 0 && handle >= numModels) return;

                if (cm_drawColor.IsModified)
                {
                    TextScanFormatted.Scan(cm_drawColor.String, "%f %f %f %f", out cm_color.x, out cm_color.y, out cm_color.z, out cm_color.w);
                    cm_drawColor.ClearModified();
                }

                model = models[handle];
                viewPos = (viewOrigin - modelOrigin) * modelAxis.Transpose();
                checkCount++;
                DrawNodePolygons(model, model.node, modelOrigin, modelAxis, viewPos, radius);
            }


            #region Speed test code

            static CVar cm_testCollision = new("cm_testCollision", "0", CVAR.GAME | CVAR.BOOL, "");
            static CVar cm_testRotation = new("cm_testRotation", "1", CVAR.GAME | CVAR.BOOL, "");
            static CVar cm_testModel = new("cm_testModel", "0", CVAR.GAME | CVAR.INTEGER, "");
            static CVar cm_testTimes = new("cm_testTimes", "1000", CVAR.GAME | CVAR.INTEGER, "");
            static CVar cm_testRandomMany = new("cm_testRandomMany", "0", CVAR.GAME | CVAR.BOOL, "");
            static CVar cm_testOrigin = new("cm_testOrigin", "0 0 0", CVAR.GAME, "");
            static CVar cm_testReset = new("cm_testReset", "0", CVAR.GAME | CVAR.BOOL, "");
            static CVar cm_testBox = new("cm_testBox", "-16 -16 0 16 16 64", CVAR.GAME, "");
            static CVar cm_testBoxRotation = new("cm_testBoxRotation", "0 0 0", CVAR.GAME, "");
            static CVar cm_testWalk = new("cm_testWalk", "1", CVAR.GAME | CVAR.BOOL, "");
            static CVar cm_testLength = new("cm_testLength", "1024", CVAR.GAME | CVAR.FLOAT, "");
            static CVar cm_testRadius = new("cm_testRadius", "64", CVAR.GAME | CVAR.FLOAT, "");
            static CVar cm_testAngle = new("cm_testAngle", "60", CVAR.GAME | CVAR.FLOAT, "");

            static uint total_translation;
            static uint min_translation = 999999;
            static uint max_translation = 0;
            static int num_translation = 0;
            static uint total_rotation;
            static uint min_rotation = 999999;
            static uint max_rotation = 0;
            static int num_rotation = 0;
            static Vector3 start;
            static Vector3[] testend;

            #endregion

            void DebugOutput(in Vector3 origin)
            {
                int i, k; uint t;
                Vector3 end;
                Angles boxAngles;
                Trace trace;

                if (!cm_testCollision.Bool) return;

                testend = new Vector3[cm_testTimes.Integer];

                if (cm_testReset.Bool || (cm_testWalk.Bool && !start.Compare(start)))
                {
                    total_translation = total_rotation = 0;
                    min_translation = min_rotation = 999999;
                    max_translation = max_rotation = 0;
                    num_translation = num_rotation = 0;
                    cm_testReset.Bool = false;
                }

                if (cm_testWalk.Bool) { start = origin; cm_testOrigin.String = $"{start.x:1.2} {start.y:1.2} {start.z:1.2}"; }
                else TextScanFormatted.Scan(cm_testOrigin.String, "%f %f %f", out start.x, out start.y, out start.z);

                Bounds bounds = default;
                TextScanFormatted.Scan(cm_testBox.String, "%f %f %f %f %f %f", out bounds[0].x, out bounds[0].y, out bounds[0].z, out bounds[1].x, out bounds[1].y, out bounds[1].z);
                TextScanFormatted.Scan(cm_testBoxRotation.String, "%f %f %f", out boxAngles.pitch, out boxAngles.yaw, out boxAngles.roll);
                var boxAxis = boxAngles.ToMat3();
                Matrix3x3 modelAxis = default;
                modelAxis.Identity();

                TraceModel itm = new(bounds);
                RandomX random = new(0);
                Timer timer;

                // if many traces in one random direction
                if (cm_testRandomMany.Bool)
                {
                    for (i = 0; i < 3; i++) testend[0][i] = start[i] + random.CRandomFloat() * cm_testLength.Float;
                    for (k = 1; k < cm_testTimes.Integer; k++) testend[k] = testend[0];
                }
                // many traces each in a different random direction
                else for (k = 0; k < cm_testTimes.Integer; k++) for (i = 0; i < 3; i++) testend[k][i] = start[i] + random.CRandomFloat() * cm_testLength.Float;

                // translational collision detection
                timer.Clear();
                timer.Start();
                for (i = 0; i < cm_testTimes.Integer; i++) Translation(trace, start, testend[i], itm, boxAxis, CONTENTS_SOLID | CONTENTS_PLAYERCLIP, cm_testModel.GetInteger(), vec3_origin, modelAxis);
                timer.Stop();
                t = timer.Milliseconds();
                if (t < min_translation) min_translation = t;
                if (t > max_translation) max_translation = t;
                num_translation++;
                total_translation += t;
                var buf = cm_testTimes.Integer > 9999
                    ? $"{(int)(cm_testTimes.Integer / 1000),3}K"
                    : $"{cm_testTimes.Integer,4}";
                common.Printf($"{buf} translations: {t,4} milliseconds, (min = {min_translation}, max = {max_translation}, av = {(float)total_translation / num_translation:1.1})\n");

                // if many traces in one random direction
                if (cm_testRandomMany.Bool)
                {
                    for (i = 0; i < 3; i++) testend[0][i] = start[i] + random.CRandomFloat() * cm_testRadius.Float;
                    for (k = 1; k < cm_testTimes.Integer; k++) testend[k] = testend[0];
                }
                // many traces each in a different random direction
                else for (k = 0; k < cm_testTimes.Integer; k++) for (i = 0; i < 3; i++) testend[k][i] = start[i] + random.CRandomFloat() * cm_testRadius.Float;

                if (cm_testRotation.Bool)
                {
                    // rotational collision detection
                    Vector3 vec = new(random.CRandomFloat(), random.CRandomFloat(), random.RandomFloat());
                    vec.Normalize();
                    Rotation rotation = new(Vector3.origin, vec, cm_testAngle.Float);

                    timer.Clear();
                    timer.Start();
                    for (i = 0; i < cm_testTimes.Integer; i++)
                    {
                        rotation.Origin = testend[i];
                        Rotation(out trace, start, rotation, itm, boxAxis, CONTENTS_SOLID | CONTENTS_PLAYERCLIP, cm_testModel.Integer, vector3.origin, modelAxis);
                    }
                    timer.Stop();
                    t = timer.Milliseconds();
                    if (t < min_rotation) min_rotation = t;
                    if (t > max_rotation) max_rotation = t;
                    num_rotation++;
                    total_rotation += t;
                    buf = cm_testTimes.Integer > 9999
                        ? $"{(int)(cm_testTimes.Integer / 1000),3}K"
                        : $"{cm_testTimes.Integer,4}";
                    common.Printf($"{buf} rotation: {t,4} milliseconds, (min = {min_rotation}, max = {max_rotation}, av = {(float)total_rotation / num_rotation:1.1})\n");
                }

                testend = null;
            }
        }
    }
}