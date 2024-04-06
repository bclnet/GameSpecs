using OpenStack.Graphics.DirectX;
using System.Collections.Generic;

namespace OpenStack.Graphics.Renderer1
{
    public struct OnDiskBufferData
    {
        public enum RenderSlotType //was:Resource/Enum/RenderSlotType
        {
            RENDER_SLOT_INVALID = -1,
            RENDER_SLOT_PER_VERTEX = 0,
            RENDER_SLOT_PER_INSTANCE = 1
        }

        public struct Attribute
        {
            public string SemanticName;
            public int SemanticIndex;
            public DXGI_FORMAT Format;
            public uint Offset;
            public int Slot;
            public RenderSlotType SlotType;
            public int InstanceStepRate;
        }

        public uint ElementCount;
        public uint ElementSizeInBytes; //stride for vertices. Type for indices
        public Attribute[] Attributes; //Vertex attribs. Empty for index buffers
        public byte[] Data;
    }
}
