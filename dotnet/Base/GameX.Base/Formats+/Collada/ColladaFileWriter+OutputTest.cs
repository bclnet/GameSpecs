namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        void OutputTest()
        {
            //foreach (ChunkDataStream stream in CryData.Models[1].ChunkMap.Values.Where(a => a.ChunkType == ChunkTypeEnum.DataStream))
            //    if (stream.DataStreamType == DataStreamTypeEnum.TANGENTS)
            //    {
            //        foreach (var vec in stream.Tangents)
            //            Console.WriteLine("Tangent: {0:F6} {1:F6} {2:F6}", vec.x/127.0, vec.y/127.0, vec.z/127.0);
            //        Console.WriteLine($"Max x: {stream.Normals.Max(a => a.x)}");
            //        Console.WriteLine($"Max y: {stream.Normals.Max(a => a.y)}");
            //        Console.WriteLine($"Max z: {stream.Normals.Max(a => a.z)}");
            //    }
            //foreach (ChunkNode node in CryData.Models[1].NodeMap.Values)
            //{
            //    Console.WriteLine($"Node Chunk {node.Name} in model {node._model.FileName}");
            //    node.WriteChunk();
            //    Console.ReadKey();
            //}
            //foreach (ChunkNode node in CryData.Models[1].NodeMap.Values.Where(a => a.Name.Contains("Belly_Wing_Right_Decal")))
            //{
            //    node.WriteChunk();
            //    node.ParentNode.WriteChunk();
            //    node.ParentNode.ParentNode.WriteChunk();
            //    node.ParentNode.ParentNode.ParentNode.WriteChunk();
            //    Console.ReadKey();
            //}
            //foreach (var result in CryData.Models[0].SkinningInfo.BoneMapping)        // To see if the bone index > than the number of bones and bone weights
            //{
            //    for (var i = 0; i < 4; i++)
            //        if (result.Weight[i] > 0)
            //            Console.WriteLine($"Bone Weight: {result.Weight[i]}");
            //}
            //Console.WriteLine($"{CryData.Models[0].SkinningInfo.BoneMapping.Count} bone weights found");
        }
    }
}