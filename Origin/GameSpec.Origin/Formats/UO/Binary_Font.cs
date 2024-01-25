using GameSpec.Formats;
using GameSpec.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Font : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Font(r));

        #region Headers

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            public static (string, int) Struct = ("<6i", sizeof(Header));
            public int Offset3Ddata;                // -1 = no
            public int OffsetCylinder;              // -1 = no
            public int OffsetProgressiveData;       // -1 = no
            public int OffsetClothesData;           // -1 = no
            public int OffsetCollisionSpheres;      // -1 = no
            public int OffsetPhysicsBox;            // -1 = no
        }

        #endregion

        public Binary_Font(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("Binary_Font", items: new List<MetaInfo> {
                })
            };
            return nodes;
        }
    }
}
