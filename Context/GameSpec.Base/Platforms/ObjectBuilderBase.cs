using OpenStack.Graphics;

namespace GameSpec.Platforms
{
    public abstract class ObjectBuilderBase<Object, Material, Texture>
    {
        public abstract Object CreateObject(Object prefab);
        public abstract void EnsurePrefabContainerExists();
        public abstract Object BuildObject(object source, IMaterialManager<Material, Texture> materialManager);
    }
}
