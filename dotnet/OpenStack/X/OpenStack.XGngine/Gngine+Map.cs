using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine
{
    unsafe static partial class Gngine
    {
        public static bool Doom3Quest_useScreenLayer => false;

        // performs radius cull first, then corner cull
        // Performs quick test before expensive test
        // Returns true if the box is outside the given global frustum, (positive sides are out)
        public static bool R_CullLocalBox(in Bounds bounds, float[] modelMatrix, int numPlanes, Plane[] planes)
        {
            if (R_RadiusCullLocalBox(bounds, modelMatrix, numPlanes, planes)) return true;
            return R_CornerCullLocalBox(bounds, modelMatrix, numPlanes, planes);
        }

        // A fast, conservative center-to-corner culling test
        // Returns true if the box is outside the given global frustum, (positive sides are out)
        public static bool R_RadiusCullLocalBox(in Bounds bounds, float[] modelMatrix, int numPlanes, Plane[] planes)
        {
            if (r_useCulling.Integer == 0) return false;

            // transform the surface bounds into world space
            var localOrigin = (bounds[0] + bounds[1]) * 0.5f;

            R_LocalPointToGlobal(modelMatrix, localOrigin, out var worldOrigin);

            var worldRadius = (bounds[0] - localOrigin).Length;   // FIXME: won't be correct for scaled objects

            float d; Plane frust;
            for (var i = 0; i < numPlanes; i++)
            {
                frust = planes[i];
                d = frust.Distance(worldOrigin);
                if (d > worldRadius) return true;    // culled
            }

            return false;       // no culled
        }
        // Tests all corners against the frustum.
        // Can still generate a few false positives when the box is outside a corner.
        // Returns true if the box is outside the given global frustum, (positive sides are out)
        public static bool R_CornerCullLocalBox(in Bounds bounds, float[] modelMatrix, int numPlanes, Plane[] planes)
        {
            int i, j;
            var transformed = stackalloc Vector3[8];
            var dists = stackalloc float[8];
            Vector3 v; Plane frust;

            // we can disable box culling for experimental timing purposes
            if (r_useCulling.Integer < 2) return false;

            // transform into world space
            for (i = 0; i < 8; i++)
            {
                v.x = bounds[i & 1].x;
                v.y = bounds[(i >> 1) & 1].y;
                v.z = bounds[(i >> 2) & 1].z;
                R_LocalPointToGlobal(modelMatrix, v, out transformed[i]);
            }

            // check against frustum planes
            for (i = 0; i < numPlanes; i++)
            {
                frust = planes[i];
                for (j = 0; j < 8; j++)
                {
                    dists[j] = frust.Distance(transformed[j]);
                    if (dists[j] < 0) break;
                }
                // all points were behind one of the planes
                if (j == 8) { tr.pc.c_box_cull_out++; return true; }
            }

            tr.pc.c_box_cull_in++;

            return false;       // not culled
        }

        public static void R_AxisToModelMatrix(in Matrix3x3 axis, in Vector3 origin, float[] modelMatrix)
        {
            modelMatrix[0] = axis[0].x;
            modelMatrix[4] = axis[1].x;
            modelMatrix[8] = axis[2].x;
            modelMatrix[12] = origin.x;

            modelMatrix[1] = axis[0].y;
            modelMatrix[5] = axis[1].y;
            modelMatrix[9] = axis[2].y;
            modelMatrix[13] = origin.y;

            modelMatrix[2] = axis[0].z;
            modelMatrix[6] = axis[1].z;
            modelMatrix[10] = axis[2].z;
            modelMatrix[14] = origin.z;

            modelMatrix[3] = 0f;
            modelMatrix[7] = 0f;
            modelMatrix[11] = 0f;
            modelMatrix[15] = 1f;
        }

        // note that many of these assume a normalized matrix, and will not work with scaled axis

        public static void R_GlobalPointToLocal(float[] modelMatrix, in Vector3 i, out Vector3 o)
        {
            fixed (float* matrix = modelMatrix)
            {
                Vector3 temp = default;
                MathX.VectorSubtract(i, &matrix[12], ref temp);

                o.x = MathX.DotProduct(temp, &matrix[0]);
                o.y = MathX.DotProduct(temp, &matrix[4]);
                o.z = MathX.DotProduct(temp, &matrix[8]);
            }
        }

        public static void R_GlobalVectorToLocal(float[] modelMatrix, in Vector3 i, out Vector3 o)
        {
            fixed (float* matrix = modelMatrix)
            {
                o.x = MathX.DotProduct(i, &matrix[0]);
                o.y = MathX.DotProduct(i, &matrix[4]);
                o.z = MathX.DotProduct(i, &matrix[8]);
            }
        }

        public static void R_GlobalPlaneToLocal(float[] modelMatrix, in Plane i, out Plane o)
        {
            fixed (float* matrix = modelMatrix)
            {
                o.a = MathX.DotProduct(i, &matrix[0]);
                o.b = MathX.DotProduct(i, &matrix[4]);
                o.c = MathX.DotProduct(i, &matrix[8]);
                o.d = i.d + matrix[12] * i.a + matrix[13] * i.b + matrix[14] * i.c;
            }
        }

        public static void R_PointTimesMatrix(float[] modelMatrix, Vector4 i, out Vector4 o)
        {
            o.x = i.x * modelMatrix[0] + i.y * modelMatrix[4] + i.z * modelMatrix[8] + modelMatrix[12];
            o.y = i.x * modelMatrix[1] + i.y * modelMatrix[5] + i.z * modelMatrix[9] + modelMatrix[13];
            o.z = i.x * modelMatrix[2] + i.y * modelMatrix[6] + i.z * modelMatrix[10] + modelMatrix[14];
            o.w = i.x * modelMatrix[3] + i.y * modelMatrix[7] + i.z * modelMatrix[11] + modelMatrix[15];
        }

        // FIXME: these assume no skewing or scaling transforms

        public static void R_LocalPointToGlobal(float[] modelMatrix, in Vector3 i, out Vector3 o)
        {
#if __GNUC__ && __SSE2__
            __m128 m0, m1, m2, m3;
            __m128 in0, in1, in2;
            float i0, i1, i2;
            i0 = in[0];
            i1 = in[1];
            i2 = in[2];

            m0 = _mm_loadu_ps(&modelMatrix[0]);
            m1 = _mm_loadu_ps(&modelMatrix[4]);
            m2 = _mm_loadu_ps(&modelMatrix[8]);
            m3 = _mm_loadu_ps(&modelMatrix[12]);

            in0 = _mm_load1_ps(&i0);
            in1 = _mm_load1_ps(&i1);
            in2 = _mm_load1_ps(&i2);

            m0 = _mm_mul_ps(m0, in0);
            m1 = _mm_mul_ps(m1, in1);
            m2 = _mm_mul_ps(m2, in2);

            m0 = _mm_add_ps(m0, m1);
            m0 = _mm_add_ps(m0, m2);
            m0 = _mm_add_ps(m0, m3);

            _mm_store_ss(&out[0], m0);
            m1 = (__m128)_mm_shuffle_epi32((__m128i)m0, 0x55);
            _mm_store_ss(&out[1], m1);
            m2 = _mm_movehl_ps(m2, m0);
            _mm_store_ss(&out[2], m2);
#else
            o.x = i.x * modelMatrix[0] + i.y * modelMatrix[4] + i.z * modelMatrix[8] + modelMatrix[12];
            o.y = i.x * modelMatrix[1] + i.y * modelMatrix[5] + i.z * modelMatrix[9] + modelMatrix[13];
            o.z = i.x * modelMatrix[2] + i.y * modelMatrix[6] + i.z * modelMatrix[10] + modelMatrix[14];
#endif
        }

        public static void R_LocalVectorToGlobal(float[] modelMatrix, in Vector3 i, out Vector3 o)
        {
            o.x = i.x * modelMatrix[0] + i.y * modelMatrix[4] + i.z * modelMatrix[8];
            o.y = i.x * modelMatrix[1] + i.y * modelMatrix[5] + i.z * modelMatrix[9];
            o.z = i.x * modelMatrix[2] + i.y * modelMatrix[6] + i.z * modelMatrix[10];
        }

        public static void R_LocalPlaneToGlobal(float[] modelMatrix, in Plane i, out Plane o)
        {
            o = default;
            R_LocalVectorToGlobal(modelMatrix, i.Normal, out o.Normal);

            var offset = modelMatrix[12] * o.a + modelMatrix[13] * o.b + modelMatrix[14] * o.c;
            o.d = i.d - offset;
        }

        // transform Z in eye coordinates to window coordinates
        public static void R_TransformEyeZToWin(float src_z, float[] projectionMatrix, out float dst_z)
        {
            float clip_z, clip_w;

            // projection
            clip_z = src_z * projectionMatrix[2 + 2 * 4] + projectionMatrix[2 + 3 * 4];
            clip_w = src_z * projectionMatrix[3 + 2 * 4] + projectionMatrix[3 + 3 * 4];

            if (clip_w <= 0f) dst_z = 0f; // clamp to near plane
            else { dst_z = clip_z / clip_w; dst_z = dst_z * 0.5f + 0.5f; } // convert to window coords
        }

        // -1 to 1 range in x, y, and z
        public static void R_GlobalToNormalizedDeviceCoordinates(in Vector3 global, out Vector3 ndc)
        {
            int i; Plane view = default, clip = default;

            // _D3XP added work on primaryView when no viewDef
            if (tr.viewDef == null)
            {
                for (i = 0; i < 4; i++) view[i] =
                    global.x * tr.primaryView.worldSpace.u.eyeViewMatrix2[i + 0 * 4] +
                    global.y * tr.primaryView.worldSpace.u.eyeViewMatrix2[i + 1 * 4] +
                    global.z * tr.primaryView.worldSpace.u.eyeViewMatrix2[i + 2 * 4] +
                    tr.primaryView.worldSpace.u.eyeViewMatrix2[i + 3 * 4];

                for (i = 0; i < 4; i++) clip[i] =
                    view.a * tr.primaryView.projectionMatrix[i + 0 * 4] +
                    view.b * tr.primaryView.projectionMatrix[i + 1 * 4] +
                    view.c * tr.primaryView.projectionMatrix[i + 2 * 4] +
                    view.d * tr.primaryView.projectionMatrix[i + 3 * 4];
            }
            else
            {
                for (i = 0; i < 4; i++) view[i] =
                    global.x * tr.viewDef.worldSpace.u.eyeViewMatrix2[i + 0 * 4] +
                    global.y * tr.viewDef.worldSpace.u.eyeViewMatrix2[i + 1 * 4] +
                    global.z * tr.viewDef.worldSpace.u.eyeViewMatrix2[i + 2 * 4] +
                    tr.viewDef.worldSpace.u.eyeViewMatrix2[i + 3 * 4];

                for (i = 0; i < 4; i++) clip[i] =
                    view.a * tr.viewDef.projectionMatrix[i + 0 * 4] +
                    view.b * tr.viewDef.projectionMatrix[i + 1 * 4] +
                    view.c * tr.viewDef.projectionMatrix[i + 2 * 4] +
                    view.d * tr.viewDef.projectionMatrix[i + 3 * 4];
            }

            ndc.x = clip.a / clip.d;
            ndc.y = clip.b / clip.d;
            ndc.z = (clip.c + clip.d) / (2 * clip.d);
        }

        public static void R_TransformModelToClip(in Vector3 src, float* modelMatrix, float[] projectionMatrix, out Plane eye, out Plane dst)
        {
            int i; eye = default; dst = default;

            for (i = 0; i < 4; i++) eye[i] =
                src.x * modelMatrix[i + 0 * 4] +
                src.y * modelMatrix[i + 1 * 4] +
                src.z * modelMatrix[i + 2 * 4] +
                1 * modelMatrix[i + 3 * 4];

            for (i = 0; i < 4; i++) dst[i] =
                eye.a * projectionMatrix[i + 0 * 4] +
                eye.b * projectionMatrix[i + 1 * 4] +
                eye.c * projectionMatrix[i + 2 * 4] +
                eye.d * projectionMatrix[i + 3 * 4];
        }

        // Clip to normalized device coordinates
        public static void R_TransformClipToDevice(in Plane clip, ViewDef view, out Vector3 normalized)
        {
            normalized.x = clip.a / clip.d;
            normalized.y = clip.b / clip.d;
            normalized.z = clip.c / clip.d;
        }

        public static void R_TransposeGLMatrix(float* i, float* o) //: o = new float[16];
        {
            int i2, j;
            for (i2 = 0; i2 < 4; i2++) for (j = 0; j < 4; j++) o[i2 * 4 + j] = i[j * 4 + i2];
        }

        // Sets up the world to view matrix for a given viewParm
        static float[] R_SetViewMatrix_flipMatrix = { // convert from our coordinate system (looking down X) to OpenGL's coordinate system (looking down -Z)
            0, 0, -1, 0,
            -1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 0, 1
        };
        public static void R_SetViewMatrix(ViewDef viewDef)
        {
            Vector3 origin; ViewEntity world;
            var viewerMatrix = stackalloc float[16];
            world = viewDef.worldSpace;
            world.memset();

            // the model matrix is an identity
            world.modelMatrix[0 * 4 + 0] = 1;
            world.modelMatrix[1 * 4 + 1] = 1;
            world.modelMatrix[2 * 4 + 2] = 1;

            for (var eye = 0; eye <= 2; ++eye)
            {
                // transform by the camera placement
                origin = viewDef.renderView.vieworg;

                if (eye < 2 && !Doom3Quest_useScreenLayer && !viewDef.renderView.forceMono)
                    origin += (eye == 0 ? 1f : -1f) * viewDef.renderView.viewaxis[1] *
                              (cvarSystem.GetCVarFloat("vr_ipd") / 2f) *
                              (100f / 2.54f * cvarSystem.GetCVarFloat("vr_scale"));

                viewerMatrix[0] = viewDef.renderView.viewaxis[0].x;
                viewerMatrix[4] = viewDef.renderView.viewaxis[0].y;
                viewerMatrix[8] = viewDef.renderView.viewaxis[0].z;
                viewerMatrix[12] = -origin.x * viewerMatrix[0] + -origin.y * viewerMatrix[4] + -origin.z * viewerMatrix[8];

                viewerMatrix[1] = viewDef.renderView.viewaxis[1].x;
                viewerMatrix[5] = viewDef.renderView.viewaxis[1].y;
                viewerMatrix[9] = viewDef.renderView.viewaxis[1].z;
                viewerMatrix[13] = -origin.x * viewerMatrix[1] + -origin.y * viewerMatrix[5] + -origin.z * viewerMatrix[9];

                viewerMatrix[2] = viewDef.renderView.viewaxis[2].x;
                viewerMatrix[6] = viewDef.renderView.viewaxis[2].y;
                viewerMatrix[10] = viewDef.renderView.viewaxis[2].z;
                viewerMatrix[14] = -origin.x * viewerMatrix[2] + -origin.y * viewerMatrix[6] + -origin.z * viewerMatrix[10];

                viewerMatrix[3] = 0f;
                viewerMatrix[7] = 0f;
                viewerMatrix[11] = 0f;
                viewerMatrix[15] = 1f;

                // convert from our coordinate system (looking down X) to OpenGL's coordinate system (looking down -Z)
                world.u.eyeViewGet(eye, matrix =>
                {
                    //fixed (float* flipMatrixF = R_SetViewMatrix_flipMatrix) 
                    myGlMultMatrix(viewerMatrix, R_SetViewMatrix_flipMatrix, matrix);
                });
            }
        }

        public static void myGlMultMatrix(float* a, float[] b, float* o) { fixed (float* _ = b) myGlMultMatrix(a, _, o); }
        public static void myGlMultMatrix(float* a, float* b, float* o)
        {
            o[0 * 4 + 0] = a[0 * 4 + 0] * b[0 * 4 + 0] + a[0 * 4 + 1] * b[1 * 4 + 0] + a[0 * 4 + 2] * b[2 * 4 + 0] + a[0 * 4 + 3] * b[3 * 4 + 0];
            o[0 * 4 + 1] = a[0 * 4 + 0] * b[0 * 4 + 1] + a[0 * 4 + 1] * b[1 * 4 + 1] + a[0 * 4 + 2] * b[2 * 4 + 1] + a[0 * 4 + 3] * b[3 * 4 + 1];
            o[0 * 4 + 2] = a[0 * 4 + 0] * b[0 * 4 + 2] + a[0 * 4 + 1] * b[1 * 4 + 2] + a[0 * 4 + 2] * b[2 * 4 + 2] + a[0 * 4 + 3] * b[3 * 4 + 2];
            o[0 * 4 + 3] = a[0 * 4 + 0] * b[0 * 4 + 3] + a[0 * 4 + 1] * b[1 * 4 + 3] + a[0 * 4 + 2] * b[2 * 4 + 3] + a[0 * 4 + 3] * b[3 * 4 + 3];
            o[1 * 4 + 0] = a[1 * 4 + 0] * b[0 * 4 + 0] + a[1 * 4 + 1] * b[1 * 4 + 0] + a[1 * 4 + 2] * b[2 * 4 + 0] + a[1 * 4 + 3] * b[3 * 4 + 0];
            o[1 * 4 + 1] = a[1 * 4 + 0] * b[0 * 4 + 1] + a[1 * 4 + 1] * b[1 * 4 + 1] + a[1 * 4 + 2] * b[2 * 4 + 1] + a[1 * 4 + 3] * b[3 * 4 + 1];
            o[1 * 4 + 2] = a[1 * 4 + 0] * b[0 * 4 + 2] + a[1 * 4 + 1] * b[1 * 4 + 2] + a[1 * 4 + 2] * b[2 * 4 + 2] + a[1 * 4 + 3] * b[3 * 4 + 2];
            o[1 * 4 + 3] = a[1 * 4 + 0] * b[0 * 4 + 3] + a[1 * 4 + 1] * b[1 * 4 + 3] + a[1 * 4 + 2] * b[2 * 4 + 3] + a[1 * 4 + 3] * b[3 * 4 + 3];
            o[2 * 4 + 0] = a[2 * 4 + 0] * b[0 * 4 + 0] + a[2 * 4 + 1] * b[1 * 4 + 0] + a[2 * 4 + 2] * b[2 * 4 + 0] + a[2 * 4 + 3] * b[3 * 4 + 0];
            o[2 * 4 + 1] = a[2 * 4 + 0] * b[0 * 4 + 1] + a[2 * 4 + 1] * b[1 * 4 + 1] + a[2 * 4 + 2] * b[2 * 4 + 1] + a[2 * 4 + 3] * b[3 * 4 + 1];
            o[2 * 4 + 2] = a[2 * 4 + 0] * b[0 * 4 + 2] + a[2 * 4 + 1] * b[1 * 4 + 2] + a[2 * 4 + 2] * b[2 * 4 + 2] + a[2 * 4 + 3] * b[3 * 4 + 2];
            o[2 * 4 + 3] = a[2 * 4 + 0] * b[0 * 4 + 3] + a[2 * 4 + 1] * b[1 * 4 + 3] + a[2 * 4 + 2] * b[2 * 4 + 3] + a[2 * 4 + 3] * b[3 * 4 + 3];
            o[3 * 4 + 0] = a[3 * 4 + 0] * b[0 * 4 + 0] + a[3 * 4 + 1] * b[1 * 4 + 0] + a[3 * 4 + 2] * b[2 * 4 + 0] + a[3 * 4 + 3] * b[3 * 4 + 0];
            o[3 * 4 + 1] = a[3 * 4 + 0] * b[0 * 4 + 1] + a[3 * 4 + 1] * b[1 * 4 + 1] + a[3 * 4 + 2] * b[2 * 4 + 1] + a[3 * 4 + 3] * b[3 * 4 + 1];
            o[3 * 4 + 2] = a[3 * 4 + 0] * b[0 * 4 + 2] + a[3 * 4 + 1] * b[1 * 4 + 2] + a[3 * 4 + 2] * b[2 * 4 + 2] + a[3 * 4 + 3] * b[3 * 4 + 2];
            o[3 * 4 + 3] = a[3 * 4 + 0] * b[0 * 4 + 3] + a[3 * 4 + 1] * b[1 * 4 + 3] + a[3 * 4 + 2] * b[2 * 4 + 3] + a[3 * 4 + 3] * b[3 * 4 + 3];
        }
    }
}