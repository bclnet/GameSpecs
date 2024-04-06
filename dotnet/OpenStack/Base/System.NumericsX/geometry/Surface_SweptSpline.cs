using System.Runtime.CompilerServices;

namespace System.NumericsX
{
    public class Surface_SweptSpline : Surface
    {
        protected Curve_Spline_Vector4 spline;
        protected Curve_Spline_Vector4 sweptSpline;

        public Surface_SweptSpline()
        {
            spline = null;
            sweptSpline = null;
        }

        public void SetSpline(Curve_Spline_Vector4 spline)
            => this.spline = spline;

        public void SetSweptSpline(Curve_Spline_Vector4 sweptSpline)
            => this.sweptSpline = sweptSpline;

        // Sets the swept spline to a NURBS circle.
        public void SetSweptCircle(float radius)
        {
            var nurbs = new Curve_NURBS_Vector4();
            nurbs.Clear();
            nurbs.AddValue(0f, new Vector4(radius, radius, 0f, 0.00f));
            nurbs.AddValue(100f, new Vector4(-radius, radius, 0f, 0.25f));
            nurbs.AddValue(200f, new Vector4(-radius, -radius, 0f, 0.50f));
            nurbs.AddValue(300f, new Vector4(radius, -radius, 0f, 0.75f));
            nurbs.BoundaryType = Curve_NURBS_Vector4.BT.CLOSED;
            nurbs.CloseTime = 100f;
            sweptSpline = nurbs;
        }

        // tesselate the surface
        public void Tessellate(int splineSubdivisions, int sweptSplineSubdivisions)
        {
            int i, j, offset, baseOffset, splineDiv, sweptSplineDiv, i0, i1, j0, j1; float totalTime, t;
            Vector4 splinePos, splineD1; Matrix3x3 splineMat = new();

            if (spline == null || sweptSpline == null) { base.Clear(); return; }

            var verts_ = verts.SetNum(splineSubdivisions * sweptSplineSubdivisions, false);

            // calculate the points and first derivatives for the swept spline
            totalTime = sweptSpline.GetTime(sweptSpline.NumValues - 1) - sweptSpline.GetTime(0) + sweptSpline.CloseTime;
            sweptSplineDiv = sweptSpline.BoundaryType == Curve_Spline_Vector4.BT.CLOSED ? sweptSplineSubdivisions : sweptSplineSubdivisions - 1;
            baseOffset = (splineSubdivisions - 1) * sweptSplineSubdivisions;
            for (i = 0; i < sweptSplineSubdivisions; i++)
            {
                t = totalTime * i / sweptSplineDiv;
                splinePos = sweptSpline.GetCurrentValue(t);
                splineD1 = sweptSpline.GetCurrentFirstDerivative(t);
                verts_[baseOffset + i].xyz = splinePos.ToVec3();
                verts_[baseOffset + i].st.x = splinePos.w;
                verts_[baseOffset + i].tangents0 = splineD1.ToVec3();
            }

            // sweep the spline
            totalTime = spline.GetTime(spline.NumValues - 1) - spline.GetTime(0) + spline.CloseTime;
            splineDiv = spline.BoundaryType == Curve_Spline_Vector4.BT.CLOSED ? splineSubdivisions : splineSubdivisions - 1;
            splineMat.Identity();
            for (i = 0; i < splineSubdivisions; i++)
            {
                t = totalTime * i / splineDiv;

                splinePos = spline.GetCurrentValue(t);
                splineD1 = spline.GetCurrentFirstDerivative(t);

                GetFrame(splineMat, splineD1.ToVec3(), out splineMat);

                offset = i * sweptSplineSubdivisions;
                for (j = 0; j < sweptSplineSubdivisions; j++)
                {
                    var v = verts_[offset + j];
                    v.xyz = splinePos.ToVec3() + verts_[baseOffset + j].xyz * splineMat;
                    v.st.x = verts_[baseOffset + j].st[0]; v.st.y = splinePos.w;
                    v.tangents0 = verts_[baseOffset + j].tangents0 * splineMat; v.tangents1 = splineD1.ToVec3();
                    v.normal = v.tangents1.Cross(v.tangents0); v.normal.Normalize();
                    v.color0 = v.color1 = v.color2 = v.color3 = 0;
                }
            }

            indexes.SetNum(splineDiv * sweptSplineDiv * 2 * 3, false);

            // create indexes for the triangles
            for (offset = i = 0; i < splineDiv; i++)
            {
                i0 = (i + 0) * sweptSplineSubdivisions;
                i1 = (i + 1) % splineSubdivisions * sweptSplineSubdivisions;

                for (j = 0; j < sweptSplineDiv; j++)
                {

                    j0 = (j + 0);
                    j1 = (j + 1) % sweptSplineSubdivisions;

                    indexes[offset++] = i0 + j0;
                    indexes[offset++] = i0 + j1;
                    indexes[offset++] = i1 + j1;

                    indexes[offset++] = i1 + j1;
                    indexes[offset++] = i1 + j0;
                    indexes[offset++] = i0 + j0;
                }
            }

            GenerateEdgeIndexes();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Clear()
        {
            base.Clear();
            spline = null;
            sweptSpline = null;
        }

        protected void GetFrame(in Matrix3x3 previousFrame, in Vector3 dir, out Matrix3x3 newFrame)
        {
            float wx, wy, wz;
            float xx, yy, yz;
            float xy, xz, zz;
            float x2, y2, z2;
            float a, c, s, x, y, z;
            Vector3 d, v; Matrix3x3 axis = new();

            d = dir; d.Normalize();
            v = d.Cross(previousFrame[2]); v.Normalize();

            a = MathX.ACos(previousFrame[2] * d) * 0.5f;
            c = MathX.Cos(a);
            s = MathX.Sqrt(1f - c * c);

            x = v[0] * s;
            y = v[1] * s;
            z = v[2] * s;

            x2 = x + x;
            y2 = y + y;
            z2 = z + z;
            xx = x * x2;
            xy = x * y2;
            xz = x * z2;
            yy = y * y2;
            yz = y * z2;
            zz = z * z2;
            wx = c * x2;
            wy = c * y2;
            wz = c * z2;

            axis[0].x = 1f - (yy + zz); axis[0].y = xy - wz; axis[0].z = xz + wy;
            axis[1].x = xy + wz; axis[1].y = 1f - (xx + zz); axis[1].z = yz - wx;
            axis[2].x = xz - wy; axis[2].y = yz + wx; axis[2].z = 1f - (xx + yy);

            newFrame = previousFrame * axis;

            newFrame[2] = dir; newFrame[2].Normalize();
            newFrame[1].Cross(newFrame[2], newFrame[0]); newFrame[1].Normalize();
            newFrame[0].Cross(newFrame[1], newFrame[2]); newFrame[0].Normalize();
        }
    }
}