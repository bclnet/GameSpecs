using GameSpec.Formats;
using GameSpec.Metadata;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Cliloc : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Cliloc(r));

        #region Headers

        [StructLayout(LayoutKind.Sequential)]
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

        Hashtable Table;

        public Binary_Cliloc(BinaryReader r)
        {
            var length = r.BaseStream.Length;
            r.Skip(6);
            while (r.BaseStream.Position < length)
            {
                var id = r.ReadUInt32();
                var text = r.ReadL16AString();
                Table[id] = text;
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("Binary_Cliloc", items: new List<MetaInfo> {
                    //new MetaInfo($"Obj: {Obj}"),
                })
            };
            return nodes;
        }
    }
}
