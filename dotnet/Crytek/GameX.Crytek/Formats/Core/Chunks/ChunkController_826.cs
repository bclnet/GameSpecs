using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkController_826 : ChunkController
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            //Log($"ID is: {id}");
            ControllerType = (CtrlType)r.ReadUInt32();
            NumKeys = (int)r.ReadUInt32();
            ControllerFlags = r.ReadUInt32();
            ControllerID = r.ReadUInt32();
            Keys = new Key[NumKeys];
            for (var i = 0; i < NumKeys; i++)
            {
                ref Key key = ref Keys[i];
                // Will implement fully later. Not sure I understand the structure, or if it's necessary.
                key.Time = r.ReadInt32(); //Log($"Time {Keys[i].Time}");
                key.AbsPos = r.ReadVector3(); //Log($"Abs Pos: {Keys[i].AbsPos.X:F7}  {Keys[i].AbsPos.Y:F7}  {Keys[i].AbsPos.Z:F7}");
                key.RelPos = r.ReadVector3(); //Log($"Rel Pos: {Keys[i].RelPos.X:F7}  {Keys[i].RelPos.Y:F7}  {Keys[i].RelPos.Z:F7}");
            }
        }
    }
}