using StereoKit;
using System;

namespace GameSpec.Graphics
{
    public class StereoKitMaterialBuilder : AbstractMaterialBuilder<Material, Tex>
    {
        public StereoKitMaterialBuilder(TextureManager<Tex> textureManager) : base(textureManager) { }

        Material _defaultMaterial;
        public override Material DefaultMaterial => _defaultMaterial != null ? _defaultMaterial : _defaultMaterial = BuildAutoMaterial(-1);

        Material BuildAutoMaterial(int type)
        {
            var m = new Material(null);
            return m;
        }

        public override Material BuildMaterial(object key)
        {
            switch (key)
            {
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}