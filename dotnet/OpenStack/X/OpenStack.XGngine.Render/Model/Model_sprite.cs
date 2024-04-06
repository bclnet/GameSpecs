using System;
using System.Diagnostics;
using System.NumericsX;
using static Gengine.Render.TR;

namespace Gengine.Render
{
    public unsafe class RenderModelSprite : RenderModelStatic
    {
        const string sprite_SnapshotName = "_sprite_Snapshot_";

        public override DynamicModel IsDynamicModel
            => DynamicModel.DM_CONTINUOUS;

        public override bool IsLoaded
            => true;

        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            RenderModelStatic staticModel;
            SrfTriangles tri;
            ModelSurface surf;

            if (cachedModel != null && !r_useCachedDynamicModels.Bool) { cachedModel.Dispose(); cachedModel = null; }

            if (ent == null || view == null) { cachedModel.Dispose(); cachedModel = null; }

            if (cachedModel != null)
            {
                Debug.Assert(cachedModel is RenderModelStatic);
                Debug.Assert(string.Equals(cachedModel.Name, sprite_SnapshotName, StringComparison.OrdinalIgnoreCase));

                staticModel = (RenderModelStatic)cachedModel;
                surf = staticModel.Surface(0);
                tri = surf.geometry;
            }
            else
            {
                staticModel = new RenderModelStatic();
                staticModel.InitEmpty(sprite_SnapshotName);

                tri = R_AllocStaticTriSurf();
                R_AllocStaticTriSurfVerts(tri, 4);
                R_AllocStaticTriSurfIndexes(tri, 6);

                tri.verts[0].Clear();
                tri.verts[0].normal.Set(1f, 0f, 0f);
                tri.verts[0].tangents0.Set(0f, 1f, 0f);
                tri.verts[0].tangents1.Set(0f, 0f, 1f);
                tri.verts[0].st.x = 0f;
                tri.verts[0].st.y = 0f;

                tri.verts[1].Clear();
                tri.verts[1].normal.Set(1f, 0f, 0f);
                tri.verts[1].tangents0.Set(0f, 1f, 0f);
                tri.verts[1].tangents1.Set(0f, 0f, 1f);
                tri.verts[1].st.x = 1f;
                tri.verts[1].st.y = 0f;

                tri.verts[2].Clear();
                tri.verts[2].normal.Set(1f, 0f, 0f);
                tri.verts[2].tangents0.Set(0f, 1f, 0f);
                tri.verts[2].tangents1.Set(0f, 0f, 1f);
                tri.verts[2].st.x = 1f;
                tri.verts[2].st.y = 1f;

                tri.verts[3].Clear();
                tri.verts[3].normal.Set(1f, 0f, 0f);
                tri.verts[3].tangents0.Set(0f, 1f, 0f);
                tri.verts[3].tangents1.Set(0f, 0f, 1f);
                tri.verts[3].st.x = 0f;
                tri.verts[3].st.y = 1f;

                tri.indexes[0] = 0;
                tri.indexes[1] = 1;
                tri.indexes[2] = 3;
                tri.indexes[3] = 1;
                tri.indexes[4] = 2;
                tri.indexes[5] = 3;

                tri.numVerts = 4;
                tri.numIndexes = 6;

                surf = new();
                surf.geometry = tri;
                surf.id = 0;
                surf.shader = tr.defaultMaterial;
                staticModel.AddSurface(surf);
            }

            var red = (byte)MathX.FtoiFast(ent.shaderParms[IRenderWorld.SHADERPARM_RED] * 255f);
            var green = (byte)MathX.FtoiFast(ent.shaderParms[IRenderWorld.SHADERPARM_GREEN] * 255f);
            var blue = (byte)MathX.FtoiFast(ent.shaderParms[IRenderWorld.SHADERPARM_BLUE] * 255f);
            var alpha = (byte)MathX.FtoiFast(ent.shaderParms[IRenderWorld.SHADERPARM_ALPHA] * 255f);

            var right = new Vector3(0f, ent.shaderParms[IRenderWorld.SHADERPARM_SPRITE_WIDTH] * 0.5f, 0f);
            var up = new Vector3(0f, 0f, ent.shaderParms[IRenderWorld.SHADERPARM_SPRITE_HEIGHT] * 0.5f);

            tri.verts[0].xyz = up + right;
            tri.verts[0].color0 = red;
            tri.verts[0].color1 = green;
            tri.verts[0].color2 = blue;
            tri.verts[0].color3 = alpha;

            tri.verts[1].xyz = up - right;
            tri.verts[1].color0 = red;
            tri.verts[1].color1 = green;
            tri.verts[1].color2 = blue;
            tri.verts[1].color3 = alpha;

            tri.verts[2].xyz = -right - up;
            tri.verts[2].color0 = red;
            tri.verts[2].color1 = green;
            tri.verts[2].color2 = blue;
            tri.verts[2].color3 = alpha;

            tri.verts[3].xyz = right - up;
            tri.verts[3].color0 = red;
            tri.verts[3].color1 = green;
            tri.verts[3].color2 = blue;
            tri.verts[3].color3 = alpha;

            R_BoundTriSurf(tri);

            staticModel.bounds = tri.bounds;

            return staticModel;
        }

        public override Bounds Bounds(RenderEntity ent)
        {
            Bounds b = new();
            b.Zero();
            b.ExpandSelf(ent == null ? 8f : Math.Max(ent.shaderParms[IRenderWorld.SHADERPARM_SPRITE_WIDTH], ent.shaderParms[IRenderWorld.SHADERPARM_SPRITE_HEIGHT]) * 0.5f);
            return b;
        }
    }
}