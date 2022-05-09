using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    //: Entity+AnimData
    public class AnimData : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Anim ID: {AnimId:X8}", clickable: true),
                new MetadataInfo($"Low frame: {LowFrame}"),
                new MetadataInfo($"High frame: {HighFrame}"),
                new MetadataInfo($"Framerate: {Framerate}"),
            };
            return nodes;
        }

        public override string ToString() => $"AnimId: {AnimId:X8}, LowFrame: {LowFrame}, HighFrame: {HighFrame}, FrameRate: {Framerate}";
    }
}
