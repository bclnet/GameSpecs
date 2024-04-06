using System.Collections.Generic;
using System.IO;
using System.NumericsX;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.Gngine.Gngine;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public class Trail
    {
        const int MAX_TRAIL_PTS = 20;

        public int lastUpdateTime;
        public int duration;

        public Vector3[] pts = new Vector3[MAX_TRAIL_PTS];
        public int numPoints;
    }

    public abstract class RenderModelTrail : RenderModelStatic
    {
        List<Trail> trails;
        int numActive;
        Bounds trailBounds;

        public RenderModelTrail();

        public virtual DynamicModel IsDynamicModel();
        public virtual bool IsLoaded();
        public virtual IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel);
        public virtual Bounds Bounds(RenderEntity ent);

        public int NewTrail(Vector3 pt, int duration);
        public void UpdateTrail(int index, Vector3 pt);
        public void DrawTrail(int index, RenderEntity ent, SrfTriangles tri, float globalAlpha);
    }

    public abstract class RenderModelLightning : RenderModelStatic
    {
        public virtual DynamicModel IsDynamicModel();
        public virtual bool IsLoaded();
        public virtual IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel);
        public virtual Bounds Bounds(RenderEntity ent);
    }
}