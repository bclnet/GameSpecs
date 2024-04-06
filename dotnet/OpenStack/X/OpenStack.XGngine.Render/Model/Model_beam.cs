using System;
using System.Diagnostics;
using System.NumericsX;
using static Gengine.Render.TR;

namespace Gengine.Render
{
    // This is a simple dynamic model that just creates a stretched quad between two points that faces the view, like a dynamic deform tube.
    public unsafe class RenderModelBeam : RenderModelStatic
    {
        const string beam_SnapshotName = "_beam_Snapshot_";

        public override DynamicModel IsDynamicModel
            => DynamicModel.DM_CONTINUOUS;	// regenerate for every view

        public override bool IsLoaded
            => true;	// don't ever need to load

        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            RenderModelStatic staticModel; SrfTriangles tri; ModelSurface surf;

            if (cachedModel != null) cachedModel = null;

            if (ent == null || view == null) return null;

            if (cachedModel != null)
            {
                Debug.Assert(cachedModel is RenderModelStatic);
                Debug.Assert(string.Equals(cachedModel.Name, beam_SnapshotName, StringComparison.OrdinalIgnoreCase));

                staticModel = (RenderModelStatic)cachedModel;
                surf = staticModel.Surface(0);
                tri = surf.geometry;
            }
            else
            {
                staticModel = new RenderModelStatic();
                staticModel.InitEmpty(beam_SnapshotName);

                tri = R_AllocStaticTriSurf();
                R_AllocStaticTriSurfVerts(tri, 4);
                R_AllocStaticTriSurfIndexes(tri, 6);

                tri.verts[0].Clear(); tri.verts[0].st.x = 0; tri.verts[0].st.y = 0;
                tri.verts[1].Clear(); tri.verts[1].st.x = 0; tri.verts[1].st.y = 1;
                tri.verts[2].Clear(); tri.verts[2].st.x = 1; tri.verts[2].st.y = 0;
                tri.verts[3].Clear(); tri.verts[3].st.x = 1; tri.verts[3].st.y = 1;

                tri.indexes[0] = 0; tri.indexes[1] = 2; tri.indexes[2] = 1; tri.indexes[3] = 2; tri.indexes[4] = 3; tri.indexes[5] = 1;

                tri.numVerts = 4;
                tri.numIndexes = 6;

                surf = new();
                surf.geometry = tri;
                surf.id = 0;
                surf.shader = tr.defaultMaterial;
                staticModel.AddSurface(surf);
            }

            Vector3 target = reinterpret.cast_vec3(ent.shaderParms, IRenderWorld.SHADERPARM_BEAM_END_X);

            // we need the view direction to project the minor axis of the tube as the view changes
            var modelMatrix = stackalloc float[16];
            R_AxisToModelMatrix(ent.axis, ent.origin, modelMatrix);
            R_GlobalPointToLocal(modelMatrix, view.renderView.vieworg, out var localView);
            R_GlobalPointToLocal(modelMatrix, target, out var localTarget);

            Vector3 major = localTarget;
            Vector3 minor = default;

            Vector3 mid = 0.5f * localTarget;
            Vector3 dir = mid - localView;
            minor.Cross(major, dir);
            minor.Normalize();
            if (ent.shaderParms[IRenderWorld.SHADERPARM_BEAM_WIDTH] != 0f) minor *= ent.shaderParms[IRenderWorld.SHADERPARM_BEAM_WIDTH] * 0.5f;

            var red = (byte)MathX.FtoiFast(ent.shaderParms[IRenderWorld.SHADERPARM_RED] * 255f);
            var green = (byte)MathX.FtoiFast(ent.shaderParms[IRenderWorld.SHADERPARM_GREEN] * 255f);
            var blue = (byte)MathX.FtoiFast(ent.shaderParms[IRenderWorld.SHADERPARM_BLUE] * 255f);
            var alpha = (byte)MathX.FtoiFast(ent.shaderParms[IRenderWorld.SHADERPARM_ALPHA] * 255f);

            tri.verts[0].xyz = minor; tri.verts[0].color0 = red; tri.verts[0].color1 = green; tri.verts[0].color2 = blue; tri.verts[0].color3 = alpha;
            tri.verts[1].xyz = -minor; tri.verts[1].color0 = red; tri.verts[1].color1 = green; tri.verts[1].color2 = blue; tri.verts[1].color3 = alpha;
            tri.verts[2].xyz = localTarget + minor; tri.verts[2].color0 = red; tri.verts[2].color1 = green; tri.verts[2].color2 = blue; tri.verts[2].color3 = alpha;
            tri.verts[3].xyz = localTarget - minor; tri.verts[3].color0 = red; tri.verts[3].color1 = green; tri.verts[3].color2 = blue; tri.verts[3].color3 = alpha;

            R_BoundTriSurf(tri);

            staticModel.bounds = tri.bounds;

            return staticModel;
        }

        public override Bounds Bounds(RenderEntity ent)
        {
            Bounds b = new();

            b.Zero();
            if (ent == null) b.ExpandSelf(8f);
            else
            {
                Vector3 target = reinterpret.cast_vec3(ent.shaderParms, IRenderWorld.SHADERPARM_BEAM_END_X);
                var modelMatrix = stackalloc float[16];
                R_AxisToModelMatrix(ent.axis, ent.origin, modelMatrix);
                R_GlobalPointToLocal(modelMatrix, target, out var localTarget);

                b.AddPoint(localTarget);
                if (ent.shaderParms[IRenderWorld.SHADERPARM_BEAM_WIDTH] != 0f) b.ExpandSelf(ent.shaderParms[IRenderWorld.SHADERPARM_BEAM_WIDTH] * 0.5f);
            }
            return b;
        }
    }
}
