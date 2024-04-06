using System;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenStack.Graphics.Renderer1.Animations;

namespace GameX.Valve.Formats.Animations.SegmentDecoders
{
    //was:Resource/ResourceTypes/ModelAnimation/SegmentDecoders/CCompressedStaticQuaternion
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
