using OpenStack.Graphics;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    // game:/Morrowind.bsa#Morrowind
    // http://192.168.1.3/ASSETS/Morrowind/Morrowind.bsa#Morrowind
    // game:/Skyrim*#SkyrimVR
    // game:/Fallout4*#Fallout4VR
    public class TestTexture : AbstractTest
    {
        class FixedMaterialInfo : IFixedMaterial
        {
            public string Name { get; set; }
            public string ShaderName { get; set; }
            public IDictionary<string, bool> GetShaderArgs() => null;
            public IDictionary<string, object> Data { get; set; }
            public string MainFilePath { get; set; }
            public string DarkFilePath { get; set; }
            public string DetailFilePath { get; set; }
            public string GlossFilePath { get; set; }
            public string GlowFilePath { get; set; }
            public string BumpFilePath { get; set; }
            public bool AlphaBlended { get; set; }
            public int SrcBlendMode { get; set; }
            public int DstBlendMode { get; set; }
            public bool AlphaTest { get; set; }
            public float AlphaCutoff { get; set; }
            public bool ZWrite { get; set; }
        }

        public TestTexture(UnityTest test) : base(test) { }

        public override void Start()
        {
            if (!string.IsNullOrEmpty(Test.Param1)) MakeTexture(Test.Param1);
            if (!string.IsNullOrEmpty(Test.Param2)) MakeCursor(Test.Param2);
        }

        GameObject MakeTexture(string path)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            obj.transform.rotation = Quaternion.Euler(-90f, 180f, -180f);
            var meshRenderer = obj.GetComponent<MeshRenderer>();
            meshRenderer.material = Graphic.MaterialManager.LoadMaterial(new FixedMaterialInfo { MainFilePath = path }, out var _);
            return obj;
        }

        void MakeCursor(string path) => Cursor.SetCursor(Graphic.LoadTexture(path, out var _), Vector2.zero, CursorMode.Auto);
    }
}