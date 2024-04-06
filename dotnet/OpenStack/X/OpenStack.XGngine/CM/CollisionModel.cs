using System.NumericsX.OpenStack.Gngine.Render;
using CmHandle = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.CM
{
    // contact type //: ContactType
    public enum CONTACT
    {
        NONE,                           // no contact
        EDGE,                           // trace model edge hits model edge
        MODELVERTEX,                    // model vertex hits trace model polygon
        TRMVERTEX                       // trace model vertex hits model polygon
    }

    // contact info // : 
    public class ContactInfo
    {
        public CONTACT type;            // contact type
        public Vector3 point;           // point of contact
        public Vector3 normal;          // contact plane normal
        public float dist;          // contact plane distance
        public int contents;        // contents at other side of surface
        public Material material;       // surface material
        public int modelFeature;    // contact feature on model
        public int trmFeature;      // contact feature on trace model
        public int entityNum;       // entity the contact surface is a part of
        public int id;              // id of clip model the contact surface is part of
    }

    // trace result
    public struct Trace
    {
        public float fraction;      // fraction of movement completed, 1.0 = didn't hit anything
        public Vector3 endpos;          // final position of trace model
        public Matrix3x3 endAxis;       // final axis of trace model
        public ContactInfo c;               // contact information, only valid if fraction < 1.0
    }

    public interface ICollisionModelManager
    {
        public const float CM_CLIP_EPSILON = 0.25f;           // always stay this distance away from any model
        public const float CM_BOX_EPSILON = 1f;           // should always be larger than clip epsilon
        public const float CM_MAX_TRACE_DIST = 4096f;         // maximum distance a trace model may be traced, point traces are unlimited

        // Loads collision models from a map file.
        void LoadMap(MapFile mapFile);
        // Frees all the collision models.
        void FreeMap();

        // Gets the clip handle for a model.
        CmHandle LoadModel(string modelName, bool precache);
        // Sets up a trace model for collision with other trace models.
        CmHandle SetupTrmModel(TraceModel trm, Material material);
        // Creates a trace model from a collision model, returns true if succesfull.
        bool TrmFromModel(string modelName, TraceModel trm);

        // Gets the name of a model.
        string GetModelName(CmHandle model);
        // Gets the bounds of a model.
        bool GetModelBounds(CmHandle model, Bounds bounds);
        // Gets all contents flags of brushes and polygons of a model ored together.
        bool GetModelContents(CmHandle model, out int contents);
        // Gets a vertex of a model.
        bool GetModelVertex(CmHandle model, int vertexNum, Vector3 vertex);
        // Gets an edge of a model.
        bool GetModelEdge(CmHandle model, int edgeNum, Vector3 start, Vector3 end);
        // Gets a polygon of a model.
        bool GetModelPolygon(CmHandle model, int polygonNum, FixedWinding winding);

        // Translates a trace model and reports the first collision if any.
        void Translation(Trace results, Vector3 start, Vector3 end, TraceModel trm, Matrix3x3 trmAxis, int contentMask, CmHandle model, Vector3 modelOrigin, Matrix3x3 modelAxis);
        // Rotates a trace model and reports the first collision if any.
        void Rotation(Trace results, Vector3 start, Rotation rotation, TraceModel trm, Matrix3x3 trmAxis, int contentMask, CmHandle model, Vector3 modelOrigin, Matrix3x3 modelAxis);
        // Returns the contents touched by the trace model or 0 if the trace model is in free space.
        int Contents(Vector3 start, TraceModel trm, Matrix3x3 trmAxis, int contentMask, CmHandle model, Vector3 modelOrigin, Matrix3x3 modelAxis);
        // Stores all contact points of the trace model with the model, returns the number of contacts.
        int Contacts(ContactInfo contacts, int maxContacts, Vector3 start, Vector6 dir, float depth, TraceModel trm, Matrix3x3 trmAxis, int contentMask, CmHandle model, Vector3 modelOrigin, Matrix3x3 modelAxis);

        // Tests collision detection.
        void DebugOutput(Vector3 origin);
        // Draws a model.
        void DrawModel(CmHandle model, Vector3 modelOrigin, Matrix3x3 modelAxis, Vector3 viewOrigin, float radius);
        // Prints model information, use -1 handle for accumulated model info.
        void ModelInfo(CmHandle model);
        // Lists all loaded models.
        void ListModels();
        // Writes a collision model file for the given map entity.
        bool WriteCollisionModelForMapEntity(MapEntity mapEnt, string filename, bool testTraceModel = true);
    }
}