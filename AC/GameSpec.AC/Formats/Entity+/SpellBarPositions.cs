namespace GameSpec.AC.Formats.Entity
{
    public class SpellBarPositions
    {
        public SpellBarPositions(uint spellBarId, uint spellBarPositionId, uint spellId)
        {
            SpellBarId = spellBarId - 1;
            SpellBarPositionId = spellBarPositionId - 1;
            SpellId = spellId;
        }
        public uint SpellBarId { get; set; }
        public uint SpellBarPositionId { get; set; }
        public uint SpellId { get; set; }
    }
}
