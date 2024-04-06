using CmHandle = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.CM
{
    internal class WindingList
    {
        public int numWindings;                 // number of windings
        public FixedWinding[] w = new FixedWinding[CM.MAX_WINDING_LIST];    // windings
        public Vector3 normal;                  // normal for all windings
        public Bounds bounds;                   // bounds of all windings in list
        public Vector3 origin;                  // origin for radius
        public float radius;                    // radius relative to origin for all windings
        public int contents;                    // winding surface contents
        public int primitiveNum;                // number of primitive the windings came from
    }

    internal class CM
    {
        public const float MIN_NODE_SIZE = 64.0f;
        public const int MAX_NODE_POLYGONS = 128;
        public const int CM_MAX_POLYGON_EDGES = 64;
        public const float CIRCLE_APPROXIMATION_LENGTH = 64.0f;

        public const int MAX_SUBMODELS = 2048;
        public const int TRACE_MODEL_HANDLE = MAX_SUBMODELS;

        public const int VERTEX_HASH_BOXSIZE = 1 << 6;    // must be power of 2
        public const int VERTEX_HASH_SIZE = VERTEX_HASH_BOXSIZE * VERTEX_HASH_BOXSIZE;
        public const int EDGE_HASH_SIZE = 1 << 14;

        public const int NODE_BLOCK_SIZE_SMALL = 8;
        public const int NODE_BLOCK_SIZE_LARGE = 256;
        public const int REFERENCE_BLOCK_SIZE_SMALL = 8;
        public const int REFERENCE_BLOCK_SIZE_LARGE = 256;

        public const int MAX_WINDING_LIST = 128;        // quite a few are generated at times
        public const float INTEGRAL_EPSILON = 0.01f;
        public const float VERTEX_EPSILON = 0.1f;
        public const float CHOP_EPSILON = 0.1f;

        #region Collision model

        public struct Vertex
        {
            public Vector3 p;                   // vertex point
            public int checkcount;              // for multi-check avoidance
            public uint side;                   // each bit tells at which side this vertex passes one of the trace model edges
            public uint sideSet;                // each bit tells if sidedness for the trace model edge has been calculated yet
        }

        public unsafe struct Edge
        {
            public int checkcount;              // for multi-check avoidance
            public ushort internal_;            // a trace model can never collide with public edges
            public ushort numUsers;             // number of polygons using this edge
            public uint side;                   // each bit tells at which side of this edge one of the trace model vertices passes
            public uint sideSet;                // each bit tells if sidedness for the trace model vertex has been calculated yet
            public fixed int vertexNum[2];      // start and end point of edge
            public Vector3 normal;              // edge normal
        }

        public class PolygonBlock
        {
            public int bytesRemaining;
            public object next;
        }

        public class Polygon
        {
            public Bounds bounds;               // polygon bounds
            public int checkcount;              // for multi-check avoidance
            public int contents;                // contents behind polygon
            public Material material;           // material
            public Plane plane;                 // polygon plane
            public int numEdges;                // number of edges
            public int[] edges;                 // variable sized, indexes into cm_edge_t list
        }

        public class PolygonRef
        {
            public Polygon p;                   // pointer to polygon
            public PolygonRef next;             // next polygon in chain
        }

        public class PolygonRefBlock
        {
            public PolygonRef nextRef;          // next polygon reference in block
            public PolygonRefBlock next;        // next block with polygon references
        }

        public struct BrushBlock
        {
            public int bytesRemaining;
            public object next;
        }

        public class Brush
        {
            public int checkcount;              // for multi-check avoidance
            public Bounds bounds;               // brush bounds
            public int contents;                // contents of brush
            public Material material;           // material
            public int primitiveNum;            // number of brush primitive
            public int numPlanes;               // number of bounding planes
            public Plane[] planes;              // variable sized
        }

        public class BrushRef
        {
            public Brush b;                     // pointer to brush
            public BrushRef next;               // next brush in chain
        }

        public class BrushRefBlock
        {
            public BrushRef nextRef;            // next brush reference in block
            public BrushRefBlock next;          // next block with brush references
        }

        public class Node
        {
            public int planeType;               // node axial plane type
            public float planeDist;             // node plane distance
            public PolygonRef polygons;         // polygons in node
            public BrushRef brushes;            // brushes in node
            public Node parent;                 // parent of this node
            public Node children0;              // node children
            public Node children1;              // node children
        }

        public class NodeBlock
        {
            public Node nextNode;               // next node in block
            public NodeBlock next;              // next block with nodes
        }

        public class Model
        {
            public string name;                 // model name
            public Bounds bounds;               // model bounds
            public int contents;                // all contents of the model ored together
            public bool isConvex;               // set if model is convex

            // model geometry
            public int maxVertices;             // size of vertex array
            public int numVertices;             // number of vertices
            public Vertex[] vertices;             // array with all vertices used by the model
            public int maxEdges;                // size of edge array
            public int numEdges;                // number of edges
            public Edge[] edges;                // array with all edges used by the model
            public Node node;                   // first node of spatial subdivision

            // blocks with allocated memory
            public NodeBlock nodeBlocks;        // list with blocks of nodes
            public PolygonRefBlock polygonRefBlocks; // list with blocks of polygon references
            public BrushRefBlock brushRefBlocks; // list with blocks of brush references
            public PolygonBlock polygonBlock;   // memory block with all polygons
            public BrushBlock brushBlock;       // memory block with all brushes

            // statistics
            public int numPolygons;
            public int polygonMemory;
            public int numBrushes;
            public int brushMemory;
            public int numNodes;
            public int numBrushRefs;
            public int numPolygonRefs;
            public int numInternalEdges;
            public int numSharpEdges;
            public int numRemovedPolys;
            public int numMergedPolys;
            public int usedMemory;
        }

        #endregion

        #region Data used during collision detection calculations

        public class TrmVertex
        {
            public int used;                        // true if this vertex is used for collision detection
            public Vector3 p;                       // vertex position
            public Vector3 endp;                    // end point of vertex after movement
            public int polygonSide;                 // side of polygon this vertex is on (rotational collision)
            public Pluecker pl;                     // pluecker coordinate for vertex movement
            public Vector3 rotationOrigin;          // rotation origin for this vertex
            public Bounds rotationBounds;           // rotation bounds for this vertex
        }

        public unsafe struct TrmEdge
        {
            public int used;                        // true when vertex is used for collision detection
            public Vector3 start;                   // start of edge
            public Vector3 end;                     // end of edge
            public fixed int vertexNum[2];          // indexes into cm_traceWork_t->vertices
            public Pluecker pl;                     // pluecker coordinate for edge
            public Vector3 cross;                   // (z,-y,x) of cross product between edge dir and movement dir
            public Bounds rotationBounds;           // rotation bounds for this edge
            public Pluecker plzaxis;                // pluecker coordinate for rotation about the z-axis
            public ushort bitNum;                   // vertex bit number
        }

        public unsafe struct TrmPolygon
        {
            public int used;
            public Plane plane;                     // polygon plane
            public int numEdges;                    // number of edges
            public fixed int edges[TraceModel.MAX_TRACEMODEL_POLYEDGES];    // index into cm_traceWork_t->edges
            public Bounds rotationBounds;           // rotation bounds for this polygon
        }

        public class TraceWork
        {
            public int numVerts;
            public TrmVertex[] vertices = new TrmVertex[TraceModel.MAX_TRACEMODEL_VERTS];  // trm vertices
            public int numEdges;
            public TrmEdge[] edges = new TrmEdge[TraceModel.MAX_TRACEMODEL_EDGES + 1];       // trm edges
            public int numPolys;
            public TrmPolygon[] polys = new TrmPolygon[TraceModel.MAX_TRACEMODEL_POLYS];    // trm polygons
            public Model model;                     // model colliding with
            public Vector3 start;                   // start of trace
            public Vector3 end;                     // end of trace
            public Vector3 dir;                     // trace direction
            public Bounds bounds;                   // bounds of full trace
            public Bounds size;                     // bounds of transformed trm relative to start
            public Vector3 extents;                 // largest of abs(size[0]) and abs(size[1]) for BSP trace
            public int contents;                    // ignore polygons that do not have any of these contents flags
            public Trace trace;                     // collision detection result

            public bool rotation;                   // true if calculating rotational collision
            public bool pointTrace;                 // true if only tracing a point
            public bool positionTest;               // true if not tracing but doing a position test
            public bool isConvex;                   // true if the trace model is convex
            public bool axisIntersectsTrm;          // true if the rotation axis intersects the trace model
            public bool getContacts;                // true if retrieving contacts
            public bool quickExit;                  // set to quickly stop the collision detection calculations

            public Vector3 origin;                  // origin of rotation in model space
            public Vector3 axis;                    // rotation axis in model space
            public Matrix3x3 matrix;                // rotates axis of rotation to the z-axis
            public float angle;                     // angle for rotational collision
            public float maxTan;                    // max tangent of half the positive angle used instead of fraction
            public float radius;                    // rotation radius of trm start
            public Rotation modelVertexRotation;    // inverse rotation for model vertices

            public ContactInfo contacts;            // array with contacts
            public int maxContacts;                 // max size of contact array
            public int numContacts;                 // number of contacts found

            public Plane heartPlane1;               // polygons should be near anough the trace heart planes
            public float maxDistFromHeartPlane1;
            public Plane heartPlane2;
            public float maxDistFromHeartPlane2;
            public Pluecker[] polygonEdgePlueckerCache = new Pluecker[CM_MAX_POLYGON_EDGES];
            public Pluecker[] polygonVertexPlueckerCache = new Pluecker[CM_MAX_POLYGON_EDGES];
            public Vector3[] polygonRotationOriginCache = new Vector3[CM_MAX_POLYGON_EDGES];
        }

        #endregion

        #region Collision Map

        public class ProcNode
        {
            public Plane plane;
            public int[] children = new int[2];                // negative numbers are (-1 - areaNumber), 0 = solid
        }

        #endregion

        internal partial class CollisionModelManagerLocal : ICollisionModelManager
        {
            string mapName;
            DateTime mapFileTime;
            int loaded;
            // for multi-check avoidance
            int checkCount;
            // models
            int maxModels;
            int numModels;
            Model[] models;
            // polygons and brush for trm model
            PolygonRef[] trmPolygons = new PolygonRef[TraceModel.MAX_TRACEMODEL_POLYS];
            BrushRef[] trmBrushes = new BrushRef[1];
            Material trmMaterial;
            // for data pruning
            int numProcNodes;
            ProcNode procNodes;
            // for retrieving contact points
            bool getContacts;
            ContactInfo contacts;
            int maxContacts;
            int numContacts;

            // load collision models from a map file
            public void LoadMap(MapFile mapFile);
            // frees all the collision models
            public void FreeMap();

            // get clip handle for model
            public CmHandle LoadModel(string modelName, bool precache);
            // sets up a trace model for collision with other trace models
            public CmHandle SetupTrmModel(TraceModel trm, Material material);
            // create trace model from a collision model, returns true if succesfull
            public bool TrmFromModel(string modelName, TraceModel trm);

            // name of the model
            public string GetModelName(CmHandle model);
            // bounds of the model
            public bool GetModelBounds(CmHandle model, Bounds bounds);
            // all contents flags of brushes and polygons ored together
            public bool GetModelContents(CmHandle model, out int contents);
            // get the vertex of a model
            public bool GetModelVertex(CmHandle model, int vertexNum, Vector3 vertex);
            // get the edge of a model
            public bool GetModelEdge(CmHandle model, int edgeNum, Vector3 start, Vector3 end);
            // get the polygon of a model
            public bool GetModelPolygon(CmHandle model, int polygonNum, FixedWinding winding);

            // translates a trm and reports the first collision if any
            public void Translation(out Trace results, Vector3 start, Vector3 end, TraceModel trm, Matrix3x3 trmAxis, int contentMask, CmHandle model, Vector3 modelOrigin, Matrix3x3 modelAxis);
            // rotates a trm and reports the first collision if any
            public void Rotation(Trace results, Vector3 start, Rotation rotation, TraceModel trm, Matrix3x3 trmAxis, int contentMask, CmHandle model, Vector3 modelOrigin, Matrix3x3 modelAxis);
            // returns the contents the trm is stuck in or 0 if the trm is in free space
            //public int Contents(in Vector3 start, TraceModel trm, in Matrix3x3 trmAxis, int contentMask, CmHandle model, in Vector3 modelOrigin, in Matrix3x3 modelAxis);
            // stores all contact points of the trm with the model, returns the number of contacts
            //public int Contacts(ContactInfo contacts, int maxContacts, in Vector3 start, in Vector6 dir, in float depth, in TraceModel trm, in Matrix3x3 trmAxis, int contentMask, CmHandle model, in Vector3 modelOrigin, in Matrix3x3 modelAxis);
            // test collision detection
            public void DebugOutput(Vector3 origin);
            // draw a model
            public void DrawModel(CmHandle model, Vector3 origin, Matrix3x3 axis, Vector3 viewOrigin, float radius);
            // print model information, use -1 handle for accumulated model info
            public void ModelInfo(CmHandle model);
            // list all loaded models
            public void ListModels();
            // write a collision model file for the map entity
            public bool WriteCollisionModelForMapEntity(MapEntity mapEnt, string filename, bool testTraceModel = true);

            //           private:			// CollisionMap_translate.cpp
            //int TranslateEdgeThroughEdge(idVec3 &cross, idPluecker &l1, idPluecker &l2, float* fraction);
            //           void TranslateTrmEdgeThroughPolygon(cm_traceWork_t* tw, cm_polygon_t* poly, cm_trmEdge_t* trmEdge);
            //           void TranslateTrmVertexThroughPolygon(cm_traceWork_t* tw, cm_polygon_t* poly, cm_trmVertex_t* v, int bitNum);
            //           void TranslatePointThroughPolygon(cm_traceWork_t* tw, cm_polygon_t* poly, cm_trmVertex_t* v);
            //           void TranslateVertexThroughTrmPolygon(cm_traceWork_t* tw, cm_trmPolygon_t* trmpoly, cm_polygon_t* poly, cm_vertex_t* v, idVec3 &endp, idPluecker &pl );
            //           bool TranslateTrmThroughPolygon(cm_traceWork_t* tw, cm_polygon_t* p);
            //           void SetupTranslationHeartPlanes(cm_traceWork_t* tw);
            //           void SetupTrm(cm_traceWork_t* tw, const idTraceModel* trm);

            //           private:			// CollisionMap_rotate.cpp
            //int CollisionBetweenEdgeBounds(cm_traceWork_t* tw, const idVec3 &va, const idVec3 &vb,
            //										const idVec3 &vc, const idVec3 &vd, float tanHalfAngle,
            //                                           idVec3 &collisionPoint, idVec3 &collisionNormal );
            //   int RotateEdgeThroughEdge(cm_traceWork_t* tw, const idPluecker &pl1,
            //   											const idVec3 &vc, const idVec3 &vd,
            //   											const float minTan, float &tanHalfAngle );
            //   int EdgeFurthestFromEdge(cm_traceWork_t* tw, const idPluecker &pl1,
            //   											const idVec3 &vc, const idVec3 &vd,
            //                                           float &tanHalfAngle, float &dir );
            //   void RotateTrmEdgeThroughPolygon(cm_traceWork_t* tw, cm_polygon_t* poly, cm_trmEdge_t* trmEdge);
            //           int RotatePointThroughPlane( const cm_traceWork_t* tw, const idVec3 &point, const idPlane &plane,
            //   											const float angle, const float minTan, float &tanHalfAngle );
            //   int PointFurthestFromPlane( const cm_traceWork_t* tw, const idVec3 &point, const idPlane &plane,
            //   											const float angle, float &tanHalfAngle, float &dir );
            //   int RotatePointThroughEpsilonPlane( const cm_traceWork_t* tw, const idVec3 &point, const idVec3 &endPoint,
            //   											const idPlane &plane, const float angle, const idVec3 &origin,
            //                                           float &tanHalfAngle, idVec3 &collisionPoint, idVec3 &endDir );
            //   void RotateTrmVertexThroughPolygon(cm_traceWork_t* tw, cm_polygon_t* poly, cm_trmVertex_t* v, int vertexNum);
            //           void RotateVertexThroughTrmPolygon(cm_traceWork_t* tw, cm_trmPolygon_t* trmpoly, cm_polygon_t* poly,
            //                                                   cm_vertex_t* v, idVec3 &rotationOrigin );
            //           bool RotateTrmThroughPolygon(cm_traceWork_t* tw, cm_polygon_t* p);
            //           void BoundsForRotation( const idVec3 &origin, const idVec3 &axis, const idVec3 &start, const idVec3 &end, idBounds &bounds );
            //   void Rotation180(trace_t* results, const idVec3 &rorg, const idVec3 &axis,
            //   									const float startAngle, const float endAngle, const idVec3 &start,
            //   									const idTraceModel* trm, const idMat3 &trmAxis, int contentMask,
            //                                   CmHandle model, const idVec3 &origin, const idMat3 &modelAxis );


            //   private:			// CollisionMap_trace.cpp
            //void TraceTrmThroughNode(cm_traceWork_t* tw, cm_node_t* node);
            //           void TraceThroughAxialBSPTree_r(cm_traceWork_t* tw, cm_node_t* node, float p1f, float p2f, idVec3 &p1, idVec3 &p2);
            //           void TraceThroughModel(cm_traceWork_t* tw);
            //           void RecurseProcBSP_r(trace_t* results, int parentNodeNum, int nodeNum, float p1f, float p2f, const idVec3 &p1, const idVec3 &p2 );

            //   private:			// CollisionMap_load.cpp
            //void Clear(void );
            //           void FreeTrmModelStructure(void );
            //           // model deallocation
            //           void RemovePolygonReferences_r(cm_node_t* node, cm_polygon_t* p);
            //           void RemoveBrushReferences_r(cm_node_t* node, cm_brush_t* b);
            //           void FreeNode(cm_node_t* node);
            //           void FreePolygonReference(cm_polygonRef_t* pref);
            //           void FreeBrushReference(cm_brushRef_t* bref);
            //           void FreePolygon(cm_model_t* model, cm_polygon_t* poly);
            //           void FreeBrush(cm_model_t* model, cm_brush_t* brush);
            //           void FreeTree_r(cm_model_t* model, cm_node_t* headNode, cm_node_t* node);
            //           void FreeModel(cm_model_t* model);
            //           // merging polygons
            //           void ReplacePolygons(cm_model_t* model, cm_node_t* node, cm_polygon_t* p1, cm_polygon_t* p2, cm_polygon_t* newp);
            //           cm_polygon_t* TryMergePolygons(cm_model_t* model, cm_polygon_t* p1, cm_polygon_t* p2);
            //           bool MergePolygonWithTreePolygons(cm_model_t* model, cm_node_t* node, cm_polygon_t* polygon);
            //           void MergeTreePolygons(cm_model_t* model, cm_node_t* node);
            //           // finding public edges
            //           bool PointInsidePolygon(cm_model_t* model, cm_polygon_t* p, idVec3 &v );
            //           void FindInternalEdgesOnPolygon(cm_model_t* model, cm_polygon_t* p1, cm_polygon_t* p2);
            //           void FindInternalPolygonEdges(cm_model_t* model, cm_node_t* node, cm_polygon_t* polygon);
            //           void FindInternalEdges(cm_model_t* model, cm_node_t* node);
            //           void FindContainedEdges(cm_model_t* model, cm_polygon_t* p);
            //           // loading of proc BSP tree
            //           void ParseProcNodes(idLexer* src);
            //           void LoadProcBSP( const char* name);
            //           // removal of contained polygons
            //           int R_ChoppedAwayByProcBSP(int nodeNum, idFixedWinding* w, const idVec3 &normal, const idVec3 &origin, const float radius);
            //           int ChoppedAwayByProcBSP( const idFixedWinding &w, const idPlane &plane, int contents);
            //           void ChopWindingListWithBrush(cm_windingList_t* list, cm_brush_t* b);
            //           void R_ChopWindingListWithTreeBrushes(cm_windingList_t* list, cm_node_t* node);
            //           idFixedWinding* WindingOutsideBrushes(idFixedWinding* w, const idPlane &plane, int contents, int patch, cm_node_t* headNode);
            //   // creation of axial BSP tree
            //   cm_model_t* AllocModel(void );
            //           cm_node_t* AllocNode(cm_model_t* model, int blockSize);
            //           cm_polygonRef_t* AllocPolygonReference(cm_model_t* model, int blockSize);
            //           cm_brushRef_t* AllocBrushReference(cm_model_t* model, int blockSize);
            //           cm_polygon_t* AllocPolygon(cm_model_t* model, int numEdges);
            //           cm_brush_t* AllocBrush(cm_model_t* model, int numPlanes);
            //           void AddPolygonToNode(cm_model_t* model, cm_node_t* node, cm_polygon_t* p);
            //           void AddBrushToNode(cm_model_t* model, cm_node_t* node, cm_brush_t* b);
            //           void SetupTrmModelStructure(void );
            //           void R_FilterPolygonIntoTree(cm_model_t* model, cm_node_t* node, cm_polygonRef_t* pref, cm_polygon_t* p);
            //           void R_FilterBrushIntoTree(cm_model_t* model, cm_node_t* node, cm_brushRef_t* pref, cm_brush_t* b);
            //           cm_node_t* R_CreateAxialBSPTree(cm_model_t* model, cm_node_t* node, const idBounds &bounds );
            //   cm_node_t* CreateAxialBSPTree(cm_model_t* model, cm_node_t* node);
            //           // creation of raw polygons
            //           void SetupHash(void);
            //           void ShutdownHash(void);
            //           void ClearHash(idBounds &bounds );
            //           int HashVec(const idVec3 &vec);
            //   int GetVertex(cm_model_t* model, const idVec3 &v, int* vertexNum);
            //           int GetEdge(cm_model_t* model, const idVec3 &v1, const idVec3 &v2, int* edgeNum, int v1num);
            //           void CreatePolygon(cm_model_t* model, idFixedWinding* w, const idPlane &plane, const idMaterial* material, int primitiveNum);
            //           void PolygonFromWinding(cm_model_t* model, idFixedWinding* w, const idPlane &plane, const idMaterial* material, int primitiveNum);
            //           void CalculateEdgeNormals(cm_model_t* model, cm_node_t* node);
            //           void CreatePatchPolygons(cm_model_t* model, idSurface_Patch &mesh, const idMaterial* material, int primitiveNum);
            //           void ConvertPatch(cm_model_t* model, const idMapPatch* patch, int primitiveNum);
            //           void ConvertBrushSides(cm_model_t* model, const idMapBrush* mapBrush, int primitiveNum);
            //           void ConvertBrush(cm_model_t* model, const idMapBrush* mapBrush, int primitiveNum);
            //           void PrintModelInfo( const cm_model_t* model);
            //           void AccumulateModelInfo(cm_model_t* model);
            //           void RemapEdges(cm_node_t* node, int* edgeRemap);
            //           void OptimizeArrays(cm_model_t* model);
            //           void FinishModel(cm_model_t* model);
            //           void BuildModels( const idMapFile* mapFile);
            //           CmHandle FindModel( const char* name);
            //           cm_model_t* CollisionModelForMapEntity( const idMapEntity* mapEnt); // brush/patch model from .map
            //           cm_model_t* LoadRenderModel( const char* fileName);                 // ASE/LWO models
            //           bool TrmFromModel_r(idTraceModel &trm, cm_node_t* node);
            //           bool TrmFromModel( const cm_model_t* model, idTraceModel &trm );

            //   private:			// CollisionMap_files.cpp
            //				// writing
            //void WriteNodes(idFile* fp, cm_node_t* node);
            //           int CountPolygonMemory(cm_node_t* node) const;
            //           void WritePolygons(idFile* fp, cm_node_t* node);
            //           int CountBrushMemory(cm_node_t* node) const;
            //           void WriteBrushes(idFile* fp, cm_node_t* node);
            //           void WriteCollisionModel(idFile* fp, cm_model_t* model);
            //           void WriteCollisionModelsToFile( const char* filename, int firstModel, int lastModel, unsigned int mapFileCRC);
            //           // loading
            //           cm_node_t* ParseNodes(idLexer* src, cm_model_t* model, cm_node_t* parent);
            //           void ParseVertices(idLexer* src, cm_model_t* model);
            //           void ParseEdges(idLexer* src, cm_model_t* model);
            //           void ParsePolygons(idLexer* src, cm_model_t* model);
            //           void ParseBrushes(idLexer* src, cm_model_t* model);
            //           bool ParseCollisionModel(idLexer* src);
            //           bool LoadCollisionModelFile( const char* name, unsigned int mapFileCRC);

            //           private:			// CollisionMap_debug
            //int ContentsFromString( const char*string ) const;
            //           const char* StringFromContents( const int contents ) const;
            //           void DrawEdge(cm_model_t* model, int edgeNum, const idVec3 &origin, const idMat3 &axis );
            //   void DrawPolygon(cm_model_t* model, cm_polygon_t* p, const idVec3 &origin, const idMat3 &axis,
            //   								const idVec3 &viewOrigin );
            //   void DrawNodePolygons(cm_model_t* model, cm_node_t* node, const idVec3 &origin, const idMat3 &axis,
            //   								const idVec3 &viewOrigin, const float radius);

            //           private:			// collision map data
           
        }

        // for debugging
        //extern idCVar cm_debugCollision;
    }
}