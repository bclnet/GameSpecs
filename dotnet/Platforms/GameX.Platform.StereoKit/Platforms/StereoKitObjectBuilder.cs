using OpenStack.Graphics;
using StereoKit;
using System;

namespace GameX.Platforms
{
    public class StereoKitObjectBuilder : ObjectBuilderBase<object, Material, Tex>
    {
        public override void EnsurePrefabContainerExists() { }
        public override object CreateObject(object prefab) => throw new NotImplementedException();
        public override object BuildObject(object source, IMaterialManager<Material, Tex> materialManager) => throw new NotImplementedException();
    }
}