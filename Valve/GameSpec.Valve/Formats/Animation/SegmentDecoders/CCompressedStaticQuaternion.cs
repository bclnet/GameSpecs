using System;
using System.IO;
using System.Linq;
using System.Numerics;
using GameSpec.Valve.Formats.Animation.SegmentDecoders;
using OpenStack.Graphics.Renderer.Animation;

namespace GameSpec.Valve.Formats.Blocks.Animation.SegmentDecoders
{
    public class CCompressedStaticQuaternion : AnimationSegmentDecoder
    {
        readonly Quaternion[] Data;

        public CCompressedStaticQuaternion(ArraySegment<byte> data, int[] wantedElements, int[] remapTable, ChannelAttribute channelAttribute) : base(remapTable, channelAttribute)
        {
            Data = wantedElements.Select(i => SegmentHelpers.ReadQuaternion(data.Slice(i * 6))).ToArray();
        }

        public override void Read(int frameIndex, Frame outFrame)
        {
            for (var i = 0; i < RemapTable.Length; i++) outFrame.SetAttribute(RemapTable[i], ChannelAttribute, Data[i]);
        }
    }
}
