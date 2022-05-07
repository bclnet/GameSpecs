using OpenStack.Graphics;

namespace GameSpec.Graphics
{
    public abstract class AbstractMaterialBuilder<Material, Texture>
    {
        protected ITextureManager<Texture> _textureManager;

        public AbstractMaterialBuilder(ITextureManager<Texture> textureManager) => _textureManager = textureManager;

        public float? NormalGeneratorIntensity = 0.75f;
        public abstract Material DefaultMaterial { get; }
        public abstract Material BuildMaterial(object key);
    }
}
