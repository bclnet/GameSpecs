using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    public class Surface_Patch : Surface
    {
        protected int width;            // width of patch
        protected int height;           // height of patch
        protected int maxWidth;     // maximum width allocated for
        protected int maxHeight;        // maximum height allocated for
        protected bool expanded;        // true if vertices are spaced out

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Surface_Patch()
        {
            height = width = maxHeight = maxWidth = 0;
            expanded = false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Surface_Patch(int maxPatchWidth, int maxPatchHeight)
        {
            width = height = 0;
            maxWidth = maxPatchWidth;
            maxHeight = maxPatchHeight;
            verts.SetNum(maxWidth * maxHeight);
            expanded = false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Surface_Patch(Surface_Patch patch)
        {
            throw new NotImplementedException();
        }

        public void SetSize(int patchWidth, int patchHeight)
        {
            if (patchWidth < 1 || patchWidth > maxWidth) FatalError("Surface_Patch::SetSize: invalid patchWidth");
            if (patchHeight < 1 || patchHeight > maxHeight) FatalError("Surface_Patch::SetSize: invalid patchHeight");
            width = patchWidth;
            height = patchHeight;
            verts.SetNum(width * height, false);
        }

        public int Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => width;
        }

        public int Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => height;
        }

        // subdivide the patch mesh based on error
        public void Subdivide(float maxHorizontalError, float maxVerticalError, float maxLength, bool genNormals = false)
        {
            int i, j, k, l; float maxHorizontalErrorSqr, maxVerticalErrorSqr, maxLengthSqr;
            Vector3 prevxyz = new(), nextxyz = new(), midxyz = new(), delta; DrawVert prev, next, mid;

            // generate normals for the control mesh
            if (genNormals) GenerateNormals();

            maxHorizontalErrorSqr = MathX.Square(maxHorizontalError);
            maxVerticalErrorSqr = MathX.Square(maxVerticalError);
            maxLengthSqr = MathX.Square(maxLength);

            Expand();

            // horizontal subdivisions
            for (j = 0; j + 2 < width; j += 2)
            {
                // check subdivided midpoints against control points
                for (i = 0; i < height; i++)
                {
                    for (l = 0; l < 3; l++)
                    {
                        prevxyz[l] = verts[i * maxWidth + j + 1].xyz[l] - verts[i * maxWidth + j].xyz[l];
                        nextxyz[l] = verts[i * maxWidth + j + 2].xyz[l] - verts[i * maxWidth + j + 1].xyz[l];
                        midxyz[l] = (verts[i * maxWidth + j].xyz[l] + verts[i * maxWidth + j + 1].xyz[l] * 2f + verts[i * maxWidth + j + 2].xyz[l]) * 0.25f;
                    }

                    if (maxLength > 0f)
                    {
                        // if the span length is too long, force a subdivision
                        if (prevxyz.LengthSqr > maxLengthSqr || nextxyz.LengthSqr > maxLengthSqr) break;
                    }
                    // see if this midpoint is off far enough to subdivide
                    delta = verts[i * maxWidth + j + 1].xyz - midxyz;
                    if (delta.LengthSqr > maxHorizontalErrorSqr) break;
                }

                if (i == height) continue;   // didn't need subdivision

                if (width + 2 >= maxWidth) ResizeExpanded(maxHeight, maxWidth + 4);

                // insert two columns and replace the peak
                width += 2;

                for (i = 0; i < height; i++)
                {
                    LerpVert(verts[i * maxWidth + j], verts[i * maxWidth + j + 1], out prev);
                    LerpVert(verts[i * maxWidth + j + 1], verts[i * maxWidth + j + 2], out next);
                    LerpVert(prev, next, out mid);

                    for (k = width - 1; k > j + 3; k--) verts[i * maxWidth + k] = verts[i * maxWidth + k - 2];
                    verts[i * maxWidth + j + 1] = prev;
                    verts[i * maxWidth + j + 2] = mid;
                    verts[i * maxWidth + j + 3] = next;
                }

                // back up and recheck this set again, it may need more subdivision
                j -= 2;
            }

            // vertical subdivisions
            for (j = 0; j + 2 < height; j += 2)
            {
                // check subdivided midpoints against control points
                for (i = 0; i < width; i++)
                {
                    for (l = 0; l < 3; l++)
                    {
                        prevxyz[l] = verts[(j + 1) * maxWidth + i].xyz[l] - verts[j * maxWidth + i].xyz[l];
                        nextxyz[l] = verts[(j + 2) * maxWidth + i].xyz[l] - verts[(j + 1) * maxWidth + i].xyz[l];
                        midxyz[l] = (verts[j * maxWidth + i].xyz[l] + verts[(j + 1) * maxWidth + i].xyz[l] * 2f + verts[(j + 2) * maxWidth + i].xyz[l]) * 0.25f;
                    }

                    if (maxLength > 0f)
                    {
                        // if the span length is too long, force a subdivision
                        if (prevxyz.LengthSqr > maxLengthSqr || nextxyz.LengthSqr > maxLengthSqr) break;
                    }
                    // see if this midpoint is off far enough to subdivide
                    delta = verts[(j + 1) * maxWidth + i].xyz - midxyz;
                    if (delta.LengthSqr > maxVerticalErrorSqr) break;
                }

                if (i == width) continue;   // didn't need subdivision

                if (height + 2 >= maxHeight) ResizeExpanded(maxHeight + 4, maxWidth);

                // insert two columns and replace the peak
                height += 2;

                for (i = 0; i < width; i++)
                {
                    LerpVert(verts[j * maxWidth + i], verts[(j + 1) * maxWidth + i], out prev);
                    LerpVert(verts[(j + 1) * maxWidth + i], verts[(j + 2) * maxWidth + i], out next);
                    LerpVert(prev, next, out mid);

                    for (k = height - 1; k > j + 3; k--) verts[k * maxWidth + i] = verts[(k - 2) * maxWidth + i];
                    verts[(j + 1) * maxWidth + i] = prev;
                    verts[(j + 2) * maxWidth + i] = mid;
                    verts[(j + 3) * maxWidth + i] = next;
                }

                // back up and recheck this set again, it may need more subdivision
                j -= 2;
            }

            PutOnCurve();

            RemoveLinearColumnsRows();

            Collapse();

            // normalize all the lerped normals
            if (genNormals) for (i = 0; i < width * height; i++) verts[i].normal.Normalize();

            GenerateIndexes();
        }

        // subdivide the patch up to an explicit number of horizontal and vertical subdivisions
        public void SubdivideExplicit(int horzSubdivisions, int vertSubdivisions, bool genNormals, bool removeLinear = false)
        {
            int i, j, k, l;
            DrawVert[][] sample = new DrawVert[3][];
            int outWidth = ((width - 1) / 2 * horzSubdivisions) + 1;
            int outHeight = ((height - 1) / 2 * vertSubdivisions) + 1;
            DrawVert[] dv = new DrawVert[outWidth * outHeight];

            // generate normals for the control mesh
            if (genNormals) GenerateNormals();

            var baseCol = 0;
            for (i = 0; i + 2 < width; i += 2)
            {
                var baseRow = 0;
                for (j = 0; j + 2 < height; j += 2)
                {
                    for (k = 0; k < 3; k++) for (l = 0; l < 3; l++) sample[k][l] = verts[((j + l) * width) + i + k];
                    SampleSinglePatch(sample, baseCol, baseRow, outWidth, horzSubdivisions, vertSubdivisions, dv);
                    baseRow += vertSubdivisions;
                }
                baseCol += horzSubdivisions;
            }
            verts.SetNum(outWidth * outHeight);
            for (i = 0; i < outWidth * outHeight; i++) verts[i] = dv[i];

            width = maxWidth = outWidth;
            height = maxHeight = outHeight;
            expanded = false;

            if (removeLinear) { Expand(); RemoveLinearColumnsRows(); Collapse(); }

            // normalize all the lerped normals
            if (genNormals) for (i = 0; i < width * height; i++) verts[i].normal.Normalize();

            GenerateIndexes();
        }

        // put the approximation points on the curve
        // Expects an expanded patch.
        void PutOnCurve()
        {
            int i, j; DrawVert prev, next;

            Debug.Assert(expanded == true);
            // put all the approximating points on the curve
            for (i = 0; i < width; i++)
                for (j = 1; j < height; j += 2)
                {
                    LerpVert(verts[j * maxWidth + i], verts[(j + 1) * maxWidth + i], out prev);
                    LerpVert(verts[j * maxWidth + i], verts[(j - 1) * maxWidth + i], out next);
                    LerpVert(prev, next, out var v); verts[j * maxWidth + i] = v;
                }

            for (j = 0; j < height; j++)
                for (i = 1; i < width; i += 2)
                {
                    LerpVert(verts[j * maxWidth + i], verts[j * maxWidth + i + 1], out prev);
                    LerpVert(verts[j * maxWidth + i], verts[j * maxWidth + i - 1], out next);
                    LerpVert(prev, next, out var v); verts[j * maxWidth + i] = v;
                }
        }

        // remove columns and rows with all points on one line
        // Expects an expanded patch.
        void RemoveLinearColumnsRows()
        {
            int i, j, k; float len, maxLength; Vector3 proj, dir;

            Debug.Assert(expanded == true);
            for (j = 1; j < width - 1; j++)
            {
                maxLength = 0;
                for (i = 0; i < height; i++)
                {
                    ProjectPointOntoVector(verts[i * maxWidth + j].xyz, verts[i * maxWidth + j - 1].xyz, verts[i * maxWidth + j + 1].xyz, out proj);
                    dir = verts[i * maxWidth + j].xyz - proj;
                    len = dir.LengthSqr;
                    if (len > maxLength) maxLength = len;
                }
                if (maxLength < MathX.Square(0.2f))
                {
                    width--;
                    for (i = 0; i < height; i++) for (k = j; k < width; k++) verts[i * maxWidth + k] = verts[i * maxWidth + k + 1];
                    j--;
                }
            }
            for (j = 1; j < height - 1; j++)
            {
                maxLength = 0;
                for (i = 0; i < width; i++)
                {
                    ProjectPointOntoVector(verts[j * maxWidth + i].xyz, verts[(j - 1) * maxWidth + i].xyz, verts[(j + 1) * maxWidth + i].xyz, out proj);
                    dir = verts[j * maxWidth + i].xyz - proj;
                    len = dir.LengthSqr;
                    if (len > maxLength) maxLength = len;
                }
                if (maxLength < MathX.Square(0.2f))
                {
                    height--;
                    for (i = 0; i < width; i++) for (k = j; k < height; k++) verts[k * maxWidth + i] = verts[(k + 1) * maxWidth + i];
                    j--;
                }
            }
        }

        // resize verts buffer
        void ResizeExpanded(int height, int width)
        {
            int i, j;

            Debug.Assert(expanded == true);
            if (height <= maxHeight && width <= maxWidth) return;
            if (height * width > maxHeight * maxWidth) verts.SetNum(height * width);
            // space out verts for new height and width
            for (j = maxHeight - 1; j >= 0; j--) for (i = maxWidth - 1; i >= 0; i--) verts[j * width + i] = verts[j * maxWidth + i];
            maxHeight = height;
            maxWidth = width;
        }

        // space points out over maxWidth * maxHeight buffer
        void Expand()
        {
            int i, j;

            if (expanded) FatalError("Surface_Patch::Expand: patch alread expanded");
            expanded = true;
            verts.SetNum(maxWidth * maxHeight, false);
            if (width != maxWidth) for (j = height - 1; j >= 0; j--) for (i = width - 1; i >= 0; i--) verts[j * maxWidth + i] = verts[j * width + i];
        }

        // move all points to the start of the verts buffer
        void Collapse()
        {
            int i, j;

            if (!expanded) FatalError("Surface_Patch::Collapse: patch not expanded");
            expanded = false;
            if (width != maxWidth) for (j = 0; j < height; j++) for (i = 0; i < width; i++) verts[j * width + i] = verts[j * maxWidth + i];
            verts.SetNum(width * height, false);
        }

        // project a point onto a vector to calculate maximum curve error
        static void ProjectPointOntoVector(in Vector3 point, in Vector3 vStart, in Vector3 vEnd, out Vector3 vProj)
        {
            var pVec = point - vStart;
            var vec = vEnd - vStart;
            vec.Normalize();
            // project onto the directional vector for this segment
            vProj = vStart + (pVec * vec) * vec;
        }

        // Handles all the complicated wrapping and degenerate cases
        // Expects a Not expanded patch.
        // generate normals
        const float COPLANAR_EPSILON = 0.1f;
        static readonly int[][] GenerateNormals_neighbors = {
            new[]{0,1}, new[]{1,1}, new[]{1,0}, new[]{1,-1}, new[]{0,-1}, new[]{-1,-1}, new[]{-1,0}, new[]{-1,1}
        };
        unsafe void GenerateNormals()
        {
            int i, j, k, dist, count, x, y;
            Vector3 norm, sum, base_, delta, temp; Vector3* around = stackalloc Vector3[8];
            bool wrapWidth, wrapHeight; bool* good = stackalloc bool[8];

            Debug.Assert(expanded == false);

            // if all points are coplanar, set all normals to that plane
            var verts_ = verts.Ptr();
            Vector3* extent = stackalloc Vector3[3];
            float offset;

            extent[0] = verts_[width - 1].xyz - verts_[0].xyz;
            extent[1] = verts_[(height - 1) * width + width - 1].xyz - verts_[0].xyz;
            extent[2] = verts_[(height - 1) * width].xyz - verts_[0].xyz;

            norm = extent[0].Cross(extent[1]);
            if (norm.LengthSqr == 0f)
            {
                norm = extent[0].Cross(extent[2]);
                if (norm.LengthSqr == 0f) norm = extent[1].Cross(extent[2]);
            }

            // wrapped patched may not get a valid normal here
            if (norm.Normalize() != 0f)
            {
                offset = verts_[0].xyz * norm;
                for (i = 1; i < width * height; i++)
                {
                    var d = verts_[i].xyz * norm;
                    if (MathX.Fabs(d - offset) > COPLANAR_EPSILON) break;
                }

                if (i == width * height)
                {
                    // all are coplanar
                    for (i = 0; i < width * height; i++) verts_[i].normal = norm;
                    return;
                }
            }

            // check for wrapped edge cases, which should smooth across themselves
            wrapWidth = false;
            for (i = 0; i < height; i++)
            {
                delta = verts_[i * width].xyz - verts_[i * width + width - 1].xyz;
                if (delta.LengthSqr > MathX.Square(1f)) break;
            }
            if (i == height) wrapWidth = true;

            wrapHeight = false;
            for (i = 0; i < width; i++)
            {
                delta = verts_[i].xyz - verts_[(height - 1) * width + i].xyz;
                if (delta.LengthSqr > MathX.Square(1f)) break;
            }
            if (i == width) wrapHeight = true;

            for (i = 0; i < width; i++)
                for (j = 0; j < height; j++)
                {
                    count = 0;
                    base_ = verts_[j * width + i].xyz;
                    for (k = 0; k < 8; k++)
                    {
                        around[k] = Vector3.origin;
                        good[k] = false;

                        for (dist = 1; dist <= 3; dist++)
                        {
                            x = i + GenerateNormals_neighbors[k][0] * dist;
                            y = j + GenerateNormals_neighbors[k][1] * dist;
                            if (wrapWidth)
                            {
                                if (x < 0) x = width - 1 + x;
                                else if (x >= width) x = 1 + x - width;
                            }
                            if (wrapHeight)
                            {
                                if (y < 0) y = height - 1 + y;
                                else if (y >= height) y = 1 + y - height;
                            }

                            if (x < 0 || x >= width || y < 0 || y >= height) break; // edge of patch
                            temp = verts_[y * width + x].xyz - base_;
                            if (temp.Normalize() == 0f) continue;               // degenerate edge, get more dist
                            else { good[k] = true; around[k] = temp; break; }   // good edge
                        }
                    }

                    sum = Vector3.origin;
                    for (k = 0; k < 8; k++)
                    {
                        if (!good[k] || !good[(k + 1) & 7]) continue;   // didn't get two points
                        norm = around[(k + 1) & 7].Cross(around[k]);
                        if (norm.Normalize() == 0f) continue;
                        sum += norm;
                        count++;
                    }
                    if (count == 0) { count = 1; /*Lib.Printf("bad normal\n");*/ }
                    verts_[j * width + i].normal = sum;
                    verts_[j * width + i].normal.Normalize();
                }
        }

        // generate triangle indexes
        void GenerateIndexes()
        {
            int i, j, v1, v2, v3, v4, index;

            indexes.SetNum((width - 1) * (height - 1) * 2 * 3, false);
            index = 0;
            for (i = 0; i < width - 1; i++)
                for (j = 0; j < height - 1; j++)
                {
                    v1 = j * width + i;
                    v2 = v1 + 1;
                    v3 = v1 + width + 1;
                    v4 = v1 + width;
                    indexes[index++] = v1;
                    indexes[index++] = v3;
                    indexes[index++] = v2;
                    indexes[index++] = v1;
                    indexes[index++] = v4;
                    indexes[index++] = v3;
                }

            GenerateEdgeIndexes();
        }

        // lerp point from two patch point
        void LerpVert(in DrawVert a, in DrawVert b, out DrawVert o)
        {
            o = new();
            o.xyz[0] = 0.5f * (a.xyz[0] + b.xyz[0]);
            o.xyz[1] = 0.5f * (a.xyz[1] + b.xyz[1]);
            o.xyz[2] = 0.5f * (a.xyz[2] + b.xyz[2]);
            o.normal[0] = 0.5f * (a.normal[0] + b.normal[0]);
            o.normal[1] = 0.5f * (a.normal[1] + b.normal[1]);
            o.normal[2] = 0.5f * (a.normal[2] + b.normal[2]);
            o.st[0] = 0.5f * (a.st[0] + b.st[0]);
            o.st[1] = 0.5f * (a.st[1] + b.st[1]);
        }

        // sample a single 3x3 patch
        void SampleSinglePatchPoint(DrawVert[][] ctrl, float u, float v, out DrawVert o)
        {
            float[,] vCtrl = new float[3, 8]; int vPoint, axis;

            o = new();
            // find the control points for the v coordinate
            for (vPoint = 0; vPoint < 3; vPoint++)
                for (axis = 0; axis < 8; axis++)
                {
                    float a, b, c, qA, qB, qC;
                    if (axis < 3)
                    {
                        a = ctrl[0][vPoint].xyz[axis];
                        b = ctrl[1][vPoint].xyz[axis];
                        c = ctrl[2][vPoint].xyz[axis];
                    }
                    else if (axis < 6)
                    {
                        a = ctrl[0][vPoint].normal[axis - 3];
                        b = ctrl[1][vPoint].normal[axis - 3];
                        c = ctrl[2][vPoint].normal[axis - 3];
                    }
                    else
                    {
                        a = ctrl[0][vPoint].st[axis - 6];
                        b = ctrl[1][vPoint].st[axis - 6];
                        c = ctrl[2][vPoint].st[axis - 6];
                    }
                    qA = a - 2f * b + c;
                    qB = 2f * b - 2f * a;
                    qC = a;
                    vCtrl[vPoint, axis] = qA * u * u + qB * u + qC;
                }

            // interpolate the v value
            for (axis = 0; axis < 8; axis++)
            {
                float a, b, c, qA, qB, qC;

                a = vCtrl[0, axis];
                b = vCtrl[1, axis];
                c = vCtrl[2, axis];
                qA = a - 2f * b + c;
                qB = 2f * b - 2f * a;
                qC = a;

                if (axis < 3) o.xyz[axis] = qA * v * v + qB * v + qC;
                else if (axis < 6) o.normal[axis - 3] = qA * v * v + qB * v + qC;
                else o.st[axis - 6] = qA * v * v + qB * v + qC;
            }
        }

        void SampleSinglePatch(DrawVert[][] ctrl, int baseCol, int baseRow, int width, int horzSub, int vertSub, DrawVert[] o)
        {
            int i, j; float u, v;

            horzSub++;
            vertSub++;
            for (i = 0; i < horzSub; i++)
                for (j = 0; j < vertSub; j++)
                {
                    u = (float)i / (horzSub - 1);
                    v = (float)j / (vertSub - 1);
                    SampleSinglePatchPoint(ctrl, u, v, out var z); o[((baseRow + j) * width) + i + baseCol] = z;
                }
        }
    }
}