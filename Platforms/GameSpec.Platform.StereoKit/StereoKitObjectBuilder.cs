using OpenStack.Graphics;
using StereoKit;
using System;

namespace GameSpec.Graphics
{
    public class StereoKitObjectBuilder : AbstractObjectBuilder<object, Material, Tex>
    {
        public override void EnsurePrefabContainerExists() { }
        public override object CreateObject(object prefab) => throw new NotImplementedException();
        public override object BuildObject(object source, IMaterialManager<Material, Tex> materialManager) => throw new NotImplementedException();
    }
}