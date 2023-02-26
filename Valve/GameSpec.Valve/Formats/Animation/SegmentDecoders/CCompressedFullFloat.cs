using OpenStack.Graphics.Renderer.Animation;
using System;
using System.IO;
using System.Linq;

namespace GameSpec.Valve.Formats.Blocks.Animation.SegmentDecoders
{
    public class CCompressedFullFloat : AnimationSegmentDecoder
    {
        readonly float[] Data;

        public CCompressedFullFloat(ArraySegment<byte> data, int[] wantedElements, int[] remapTable, int elementCount, ChannelAttribute channelAttribute) : base(remapTable, channelAttribute)
        {
            const int elementSize = 4;
            var stride = elementCount * elementSize;
            Data = Enumerable.Range(0, data.Count / stride)
                .SelectMany(i => wantedElements.Select(j => BitConverter.ToSingle(data.Slice(i * stride + j * elementSize))).ToArray())
                .ToArray();
        }

        public override void Read(int frameIndex, Frame outFrame)
        {
            var offset = frameIndex * RemapTable.Length;
            for (var i = 0; i < RemapTable.Length; i++) outFrame.SetAttribute(RemapTable[i], ChannelAttribute, Data[offset + i]);
        }
    }
}
