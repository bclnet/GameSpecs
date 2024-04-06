using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkMtlName : Chunk  // cccc0014:  provides material name as used in the .mtl file
    {
        /// <summary>
        /// Type of Material associated with this name
        /// </summary>
        public MtlNameType MatType;
        /// <summary>
        /// Name of the Material
        /// </summary>
        public string Name;
        public MtlNamePhysicsType[] PhysicsType;
        /// <summary>
        /// Number of Materials in this name (Max: 66)
        /// </summary>
        public int NumChildren;
        public uint[] ChildIDs;
        public uint NFlags2;

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Material Name: {Name}, Number of Children: {NumChildren}, Material Type: {MatType}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log("*** START MATERIAL NAMES ***");
            Log($"    ChunkType:           {ChunkType} ({ChunkType:X})");
            Log($"    Material Name:       {Name}");
            Log($"    Material ID:         {ID:X}");
            Log($"    Version:             {Version:X}");
            Log($"    Number of Children:  {NumChildren}");
            Log($"    Material Type:       {MatType} ({MatType:X})");
            foreach (var physicsType in PhysicsType) Log($"    Physics Type:        {physicsType} ({physicsType:X})");
            Log("*** END MATERIAL NAMES ***");
        }
#endif
        #endregion
    }
}