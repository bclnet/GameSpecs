using OpenStack.Graphics;
using OpenStack.Graphics.OpenGL;
using System;

namespace GameSpec.Graphics
{
    public class OpenGLObjectBuilder : AbstractObjectBuilder<object, Material, int>
    {
        public override void EnsurePrefabContainerExists() { }
        public override object CreateObject(object prefab) => throw new NotImplementedException();
        public override object BuildObject(object source, IMaterialManager<Material, int> materialManager) => throw new NotImplementedException();
    }
}