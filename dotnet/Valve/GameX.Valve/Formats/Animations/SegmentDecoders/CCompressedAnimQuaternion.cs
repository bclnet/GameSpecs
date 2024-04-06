using OpenStack.Graphics.Renderer1.Animations;
using System;

namespace GameX.Valve.Formats.Animations.SegmentDecoders
{
    //was:Resource/ResourceTypes/ModelAnimation/SegmentDecoders/CCompressedAnimQuaternion
    public class CCompressedAnimQuaternion : AnimationSegmentDecoder
    {
        readonly byte[] Data;

        public CCompressedAnimQuaternion(ArraySegment<byte> data, int[] wantedElements, int[] remapTable, int elementCount, ChannelAttribute channelAttribute) : base(remapTable, channelAttribute)
        {
            const int elementSize = 6;
            var stride = elementCount * elementSize;
            var elements = data.Count / stride;

            Data = new byte[remapTable.Length * elementSize * elements];
            var pos = 0;
            for (var i = 0; i < elements; i++)
                foreach (var j in wantedElements)
                {
                    data.Slice(i * stride + j * elementSize, elementSize).CopyTo(Data, pos);
                    pos += elementSize;
                }
        }

        public override void Read(int frameIndex, Frame outFrame)
        {
            const int elementSize = 6;
            var offset = frameIndex * RemapTable.Length * elementSize;
            for (var i = 0; i < RemapTable.Length; i++)
                outFrame.SetAttribute(
                    RemapTable[i],
                    ChannelAttribute,
                    SegmentHelpers.ReadQuaternion(new ReadOnlySpan<byte>(
                        Data,
                        offset + i * elementSize,
                        elementSize
                    ))
                );
        }
    }
}
