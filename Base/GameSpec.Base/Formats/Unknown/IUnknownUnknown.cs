//public enum ChunkType
//{
//    Node = 1,
//    Mesh,
//    Helper,
//}

//IGenericNode Root { get; }
//IEnumerable<IGenericNode> Nodes { get; }

//public interface IGenericNode
//{
//    string Name { get; }
//    ChunkType Type { get; }
//    IGenericNode Parent { get; }
//    IChunk Object { get; }
//    public IEnumerable<IGenericNode> Children { get; set; }
//}

//public interface IChunk
//{
//    ChunkType Type { get; }
//}

//public interface IChunkMesh : IChunk
//{
//    int Id { get; set; }
//    int MeshSubsets { get; set; }
//    int VerticesData { get; set; }
//    int VertsUVsData { get; set; }
//}