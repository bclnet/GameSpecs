using System.Drawing;
using System.Numerics;
using UnrealEngine.Framework;

namespace Tests
{
    public class TestTexture : AbstractTest
    {
        //class FixedMaterialInfo : IFixedMaterial
        //{
        //    public string Name { get; set; }
        //    public string ShaderName { get; set; }
        //    public IDictionary<string, bool> GetShaderArgs() => null;
        //    public IDictionary<string, object> Data { get; set; }
        //    public string MainFilePath { get; set; }
        //    public string DarkFilePath { get; set; }
        //    public string DetailFilePath { get; set; }
        //    public string GlossFilePath { get; set; }
        //    public string GlowFilePath { get; set; }
        //    public string BumpFilePath { get; set; }
        //    public bool AlphaBlended { get; set; }
        //    public int SrcBlendMode { get; set; }
        //    public int DstBlendMode { get; set; }
        //    public bool AlphaTest { get; set; }
        //    public float AlphaCutoff { get; set; }
        //    public bool ZWrite { get; set; }
        //}

        public TestTexture(UnrealTest test) : base(test) { }

        public override void Start()
        {
            if (!string.IsNullOrEmpty(Test.Param1)) MakeTexture(Test.Param1);
        }

        Actor MakeTexture(string path)
        {
            //World.GetFirstPlayerController().SetViewTarget(World.GetActor<Camera>("MainCamera"));

            var texture = Texture2D.Load("/Game/Scene/BasicTexture");

            var obj = new Actor();
            var mesh = new StaticMeshComponent(obj, setAsRoot: true);
            mesh.SetStaticMesh(StaticMesh.Plane);
            mesh.SetMaterial(0, Material.Load("/Game/Scene/TextureMaterial"));
            mesh.CreateAndSetMaterialInstanceDynamic(0).SetTextureParameterValue("Texture", texture);
            mesh.SetWorldLocation(new Vector3(-800.0f, 0.0f, 0.0f));
            mesh.SetWorldRotation(Maths.Euler(90.0f, 0.0f, 90.0f));

            //Debug.AddOnScreenMessage(-1, 5.0f, Color.PowderBlue, "Texture size: " + texture.GetSize());
            //Debug.AddOnScreenMessage(-1, 5.0f, Color.PowderBlue, "Pixel format: " + texture.GetPixelFormat());

            return obj;
        }
    }
}