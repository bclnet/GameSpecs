using OpenStack.Graphics.Renderer1.Animations;

namespace GameX.Valve.Formats.Animations
{
    //was:Resource/ResourceTypes/ModelAnimation/AnimationSegmentDecoder
    public abstract class AnimationSegmentDecoder
    {
        public int[] RemapTable { get; }
        public ChannelAttribute ChannelAttribute { get; }

        protected AnimationSegmentDecoder(int[] remapTable, ChannelAttribute channelAttribute)
        {
            RemapTable = remapTable;
            ChannelAttribute = channelAttribute;
        }

        public abstract void Read(int frameIndex, Frame outFrame);
    }
}
