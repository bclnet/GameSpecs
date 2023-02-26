using OpenStack.Graphics.Renderer.Animation;

namespace GameSpec.Valve.Formats.Blocks.Animation
{
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
