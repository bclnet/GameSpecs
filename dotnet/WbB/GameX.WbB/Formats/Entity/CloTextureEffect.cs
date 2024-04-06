using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class CloTextureEffect
    {
        /// <summary>
        /// Texture portal.dat 0x05000000
        /// </summary>
        public readonly uint OldTexture;
        /// <summary>
        /// Texture portal.dat 0x05000000
        /// </summary>
        public readonly uint NewTexture;

        public CloTextureEffect(BinaryReader r)
        {
            OldTexture = r.ReadUInt32();
            NewTexture = r.ReadUInt32();
        }

        //: Entity.ClothingTextureEffect
        public override string ToString() => $"OldTex: {OldTexture:X8}, NewTex: {NewTexture:X8}";
    }
}
