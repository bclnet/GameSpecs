using OpenStack.Graphics.Renderer1.Animations;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameX.Valve.Formats.Animations.SegmentDecoders
{
    //was:Resource/ResourceTypes/ModelAnimation/SegmentDecoders/CCompressedStaticVector3
    public class CCompressedStaticVector3 : AnimationSegmentDecoder
    {
        readonly Vector3[] Data;

        public CCompressedStaticVector3(ArraySegment<byte> data, int[] wantedElements, int[] remapTable, ChannelAttribute channelAttribute) : base(remapTable, channelAttribute)
        {
            Data = wantedElements.Select(i =>
            {
                var offset = i * (3 * 2);
                return new Vector3(
                    (float)BitConverterX.ToHalf(data.Slice(offset + (0 * 2))),
                    (float)BitConverterX.ToHalf(data.Slice(offset + (1 * 2))),
                    (float)BitConverterX.ToHalf(data.Slice(offset + (2 * 2)))
                );
            }).ToArray();
        }

        public override void Read(int frameIndex, Frame outFrame)
        {
            for (var i = 0; i < RemapTable.Length; i++) outFrame.SetAttribute(RemapTable[i], ChannelAttribute, Data[i]);
        }
    }
}
