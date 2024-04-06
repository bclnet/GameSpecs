using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    //: Entity+AnimData
    public class AnimData : IHaveMetaInfo
    {
        public readonly uint AnimId;
        public readonly int LowFrame;
        public readonly int HighFrame;
        /// <summary>
        /// Negative framerates play animation in reverse
        /// </summary>
        public readonly float Framerate;

        //: Entity+AnimData
        public AnimData(uint animationId, int lowFrame, int highFrame, float framerate)
        {
            AnimId = animationId;
            LowFrame = lowFrame;
            HighFrame = highFrame;
            Framerate = framerate;
        }
        public AnimData(BinaryReader r)
        {
            AnimId = r.ReadUInt32();
            LowFrame = r.ReadInt32();
            HighFrame = r.ReadInt32();
            Framerate = r.ReadSingle();
        }

        //: Entity.AnimData
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Anim ID: {AnimId:X8}", clickable: true),
                new MetaInfo($"Low frame: {LowFrame}"),
                new MetaInfo($"High frame: {HighFrame}"),
                new MetaInfo($"Framerate: {Framerate}"),
            };
            return nodes;
        }

        public override string ToString() => $"AnimId: {AnimId:X8}, LowFrame: {LowFrame}, HighFrame: {HighFrame}, FrameRate: {Framerate}";
    }
}
