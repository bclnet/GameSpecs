using System.Numerics;
using UnrealEngine.Framework;
using FDebug = UnrealEngine.Framework.Debug;
using FLogLevel = UnrealEngine.Framework.LogLevel;

namespace GameSpecUnreal.Tests
{
    public class TestTexture : AbstractTest
    {
        public TestTexture(UnrealTest test) : base(test) { }

        public override void Start()
        {
            if (!string.IsNullOrEmpty(Test.Param1)) MakeTexture(Test.Param1);
        }

        Actor MakeTexture(string path)
        {
            //World.GetFirstPlayerController().SetViewTarget(World.GetActor<Camera>("MainCamera"));

            //var texture = Texture2D.Load("/Game/Scenes/BasicTexture");
            var texture = Graphic.TextureManager.LoadTexture(path, out var _);

            var obj = new Actor();
            var mesh = new StaticMeshComponent(obj, setAsRoot: true);
            mesh.SetStaticMesh(StaticMesh.Plane);
            mesh.SetMaterial(0, Material.Load("/Game/Scenes/TextureMaterial"));
            mesh.CreateAndSetMaterialInstanceDynamic(0).SetTextureParameterValue("Texture", texture);
            mesh.SetWorldLocation(new Vector3(200.0f, 0.0f, 90.0f));
            mesh.SetWorldRotation(Maths.Euler(90.0f, 0.0f, 90.0f));

            //Debug.AddOnScreenMessage(-1, 5.0f, Color.PowderBlue, "Texture size: " + texture.GetSize());
            //Debug.AddOnScreenMessage(-1, 5.0f, Color.PowderBlue, "Pixel format: " + texture.GetPixelFormat());

            return obj;
        }
    }
}