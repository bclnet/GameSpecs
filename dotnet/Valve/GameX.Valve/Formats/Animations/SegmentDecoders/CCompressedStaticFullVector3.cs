using OpenStack.Graphics.Renderer1.Animations;
using System;
using System.Linq;
using System.Numerics;

namespace GameX.Valve.Formats.Animations.SegmentDecoders
{
    //was:Resource/ResourceTypes/ModelAnimation/SegmentDecoders/CCompressedStaticFullVector3
    public class CCompressedStaticFullVector3 : AnimationSegmentDecoder
    {
        readonly Vector3[] Data;

        public CCompressedStaticFullVector3(ArraySegment<byte> data, int[] wantedElements, int[] remapTable, ChannelAttribute channelAttribute) : base(remapTable, channelAttribute)
        {
            Data = wantedElements.Select(i =>
            {
                var offset = i * (3 * 4);
                return new Vector3(
                    BitConverter.ToSingle(data.Slice(offset + (0 * 4))),
                    BitConverter.ToSingle(data.Slice(offset + (1 * 4))),
                    BitConverter.ToSingle(data.Slice(offset + (2 * 4)))
                );
            }).ToArray();
        }

        public override void Read(int frameIndex, Frame outFrame)
        {
            for (var i = 0; i < RemapTable.Length; i++) outFrame.SetAttribute(RemapTable[i], ChannelAttribute, Data[i]);
        }
    }
}
