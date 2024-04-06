using OpenStack.Graphics;
using OpenStack.Graphics.OpenGL.Renderer1;
using System;

namespace GameX.Platforms
{
    public class VulkenObjectBuilder : ObjectBuilderBase<object, GLRenderMaterial, int>
    {
        public override void EnsurePrefabContainerExists() { }
        public override object CreateObject(object prefab) => throw new NotImplementedException();
        public override object BuildObject(object source, IMaterialManager<GLRenderMaterial, int> materialManager) => throw new NotImplementedException();
    }
}