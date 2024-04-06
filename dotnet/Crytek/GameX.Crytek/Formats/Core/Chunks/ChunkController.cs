using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkController : Chunk    // cccc000d:  Controller chunk
    {
        public CtrlType ControllerType;
        public int NumKeys;
        public uint ControllerFlags;    // technically a bitstruct to identify a cycle or a loop.
        public uint ControllerID;       // Unique id based on CRC32 of bone name.  Ver 827 only?
        public Key[] Keys;              // array length NumKeys.  Ver 827?

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Number of Keys: {NumKeys}, Controller ID: {ControllerID:X}, Controller Type: {ControllerType}, Controller Flags: {ControllerFlags}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** Controller Chunk ***");
            Log($"Version:                 {Version:X}");
            Log($"ID:                      {ID:X}");
            Log($"Number of Keys:          {NumKeys}");
            Log($"Controller Type:         {ControllerType}");
            Log($"Conttroller Flags:       {ControllerFlags}");
            Log($"Controller ID:           {ControllerID}");
            for (var i = 0; i < NumKeys; i++)
            {
                Log($"        Key {i}:       Time: {Keys[i].Time}");
                Log($"        AbsPos {i}:    {Keys[i].AbsPos.X:F7}, {Keys[i].AbsPos.Y:F7}, {Keys[i].AbsPos.Z:F7}");
                Log($"        RelPos {i}:    {Keys[i].RelPos.X:F7}, {Keys[i].RelPos.Y:F7}, {Keys[i].RelPos.Z:F7}");
            }
        }
#endif
        #endregion
    }
}