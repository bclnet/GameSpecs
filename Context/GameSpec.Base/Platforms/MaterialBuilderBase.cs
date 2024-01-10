using OpenStack.Graphics;

namespace GameSpec.Platforms
{
    public abstract class MaterialBuilderBase<Material, Texture>
    {
        protected ITextureManager<Texture> TextureManager;

        public MaterialBuilderBase(ITextureManager<Texture> textureManager) => TextureManager = textureManager;

        public float? NormalGeneratorIntensity = 0.75f;
        public abstract Material DefaultMaterial { get; }
        public abstract Material BuildMaterial(object key);
    }
}
