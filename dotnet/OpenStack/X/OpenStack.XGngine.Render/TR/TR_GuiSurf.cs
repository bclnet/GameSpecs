using System.NumericsX.OpenStack.Gngine.UI;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        // Calculates two axis for the surface sutch that a point dotted against the axis will give a 0.0 to 1.0 range in S and T when inside the gui surface
        public static void R_SurfaceToTextureAxis(SrfTriangles tri, ref Vector3 origin, Vector3* axis)
        {
            float* d0 = stackalloc float[5], d1 = stackalloc float[5];
            var bounds = stackalloc (float x, float y)[2];
            var boundsOrg = stackalloc float[2];
            float v, area, inva; DrawVert a, b, c;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            // find the bounds of the texture
            bounds[0].x = bounds[1].x = 999999;
            bounds[0].y = bounds[1].y = -999999;
            for (var i = 0; i < tri.numVerts; i++)
            {
                v = tri_verts[i].st.x;
                if (v < bounds[0].x) bounds[0].x = v;
                if (v > bounds[1].x) bounds[1].x = v;

                v = tri_verts[i].st.y;
                if (v < bounds[0].y) bounds[0].y = v;
                if (v > bounds[1].y) bounds[1].y = v;
            }

            // use the floor of the midpoint as the origin of the surface, which will prevent a slight misalignment from throwing it an entire cycle off
            boundsOrg[0] = (float)Math.Floor((bounds[0].x + bounds[1].x) * 0.5f);
            boundsOrg[1] = (float)Math.Floor((bounds[0].y + bounds[1].y) * 0.5f);

            // determine the world S and T vectors from the first drawSurf triangle
            a = tri_verts[tri_indexes[0]]; b = tri_verts[tri_indexes[1]]; c = tri_verts[tri_indexes[2]];

            MathX.VectorSubtract(b.xyz, a.xyz, d0);
            d0[3] = b.st.x - a.st.x; d0[4] = b.st.y - a.st.y;
            MathX.VectorSubtract(c.xyz, a.xyz, d1);
            d1[3] = c.st.x - a.st.x; d1[4] = c.st.y - a.st.y;

            area = d0[3] * d1[4] - d0[4] * d1[3];
            if (area == 0f)
            {
                origin.Zero();
                axis[0].Zero();
                axis[1].Zero();
                axis[2].Zero();
                return; // degenerate
            }
            inva = 1f / area;

            axis[0].x = (d0[0] * d1[4] - d0[4] * d1[0]) * inva;
            axis[0].y = (d0[1] * d1[4] - d0[4] * d1[1]) * inva;
            axis[0].z = (d0[2] * d1[4] - d0[4] * d1[2]) * inva;

            axis[1].x = (d0[3] * d1[0] - d0[0] * d1[3]) * inva;
            axis[1].y = (d0[3] * d1[1] - d0[1] * d1[3]) * inva;
            axis[1].z = (d0[3] * d1[2] - d0[2] * d1[3]) * inva;

            Plane plane = default;
            plane.FromPoints(a.xyz, b.xyz, c.xyz);
            axis[2].x = plane[0]; axis[2].y = plane[1]; axis[2].z = plane[2];

            // take point 0 and project the vectors to the texture origin
            MathX.VectorMA(a.xyz, boundsOrg[0] - a.st.x, axis[0], out origin);
            MathX.VectorMA(origin, boundsOrg[1] - a.st.y, axis[1], out origin);
        }

        // Create a texture space on the given surface and call the GUI generator to create quads for it.
        public static void R_RenderGuiSurf(IUserInterface gui, DrawSurf drawSurf)
        {
            Vector3 origin = default; Vector3* axis = stackalloc Vector3[3];
            var tr_ = (RenderSystemLocal)tr;

            // for testing the performance hit
            if (r_skipGuiShaders.Integer == 1) return;

            // don't allow an infinite recursion loop
            if (tr_.guiRecursionLevel == 4) return;

            tr.pc.c_guiSurfs++;

            // create the new matrix to draw on this surface
            R_SurfaceToTextureAxis(drawSurf.geoFrontEnd, ref origin, axis);

            float* guiModelMatrix = stackalloc float[16], modelMatrix = stackalloc float[16];

            guiModelMatrix[00] = axis[0].x / 640f;
            guiModelMatrix[04] = axis[1].x / 480f;
            guiModelMatrix[08] = axis[2].x;
            guiModelMatrix[12] = origin.x;

            guiModelMatrix[01] = axis[0].y / 640f;
            guiModelMatrix[05] = axis[1].y / 480f;
            guiModelMatrix[09] = axis[2].y;
            guiModelMatrix[13] = origin.y;

            guiModelMatrix[02] = axis[0].z / 640f;
            guiModelMatrix[06] = axis[1].z / 480f;
            guiModelMatrix[10] = axis[2].z;
            guiModelMatrix[14] = origin.z;

            guiModelMatrix[3] = 0f;
            guiModelMatrix[7] = 0f;
            guiModelMatrix[11] = 0f;
            guiModelMatrix[15] = 1f;

            myGlMultMatrix(guiModelMatrix, drawSurf.space.modelMatrix, modelMatrix);

            tr_.guiRecursionLevel++;

            // call the gui, which will call the 2D drawing functions
            tr_.guiModel.Clear();
            gui.Redraw(tr.viewDef.renderView.time);
            tr_.guiModel.EmitToCurrentView(modelMatrix, drawSurf.space.weaponDepthHack);
            tr_.guiModel.Clear();

            tr_.guiRecursionLevel--;
        }

        // Reloads any guis that have had their file timestamps changed. An optional "all" parameter will cause all models to reload, even if they are not out of date.
        // Should we also reload the map models?
        public static void R_ReloadGuis_f(CmdArgs args)
        {
            if (string.Equals(args[1], "all", StringComparison.OrdinalIgnoreCase)) { common.Printf("Reloading all gui files...\n"); uiManager.Reload(true); }
            else { common.Printf("Checking for changed gui files...\n"); uiManager.Reload(false); }
        }

        public static void R_ListGuis_f(CmdArgs args)
            => uiManager.ListGuis();
    }
}
