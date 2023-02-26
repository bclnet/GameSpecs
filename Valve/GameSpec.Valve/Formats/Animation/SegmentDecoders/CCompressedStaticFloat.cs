using OpenStack.Graphics.Renderer.Animation;
using System;
using System.IO;
using System.Linq;

namespace GameSpec.Valve.Formats.Blocks.Animation.SegmentDecoders
{
    public class CCompressedStaticFloat : AnimationSegmentDecoder
    {
        readonly float[] Data;

        public CCompressedStaticFloat(ArraySegment<byte> data, int[] wantedElements, int[] remapTable, ChannelAttribute channelAttribute) : base(remapTable, channelAttribute)
        {
            Data = wantedElements.Select(i => BitConverter.ToSingle(data.Slice(i * 4))).ToArray();
        }

        public override void Read(int frameIndex, Frame outFrame)
        {
            for (var i = 0; i < RemapTable.Length; i++) outFrame.SetAttribute(RemapTable[i], ChannelAttribute, Data[i]);
        }
    }
}
