using System.Collections.Generic;

namespace GameX.Valve.Formats.Animations
{
    //was:Resource/ResourceTypes/ModelAnimation/AnimationFrameBlock
    public class AnimationFrameBlock
    {
        public int StartFrame { get; }
        public int EndFrame { get; }
        public long[] SegmentIndexArray { get; }

        public AnimationFrameBlock(IDictionary<string, object> frameBlock)
        {
            StartFrame = frameBlock.GetInt32("m_nStartFrame");
            EndFrame = frameBlock.GetInt32("m_nEndFrame");
            SegmentIndexArray = frameBlock.GetInt64Array("m_segmentIndexArray");
        }
    }
}
