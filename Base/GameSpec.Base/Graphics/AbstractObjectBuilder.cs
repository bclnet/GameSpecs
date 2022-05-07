using OpenStack.Graphics;

namespace GameSpec.Graphics
{
    public abstract class AbstractObjectBuilder<Object, Material, Texture>
    {
        public abstract Object CreateObject(Object prefab);
        public abstract void EnsurePrefabContainerExists();
        public abstract Object BuildObject(object source, IMaterialManager<Material, Texture> materialManager);
    }
}
