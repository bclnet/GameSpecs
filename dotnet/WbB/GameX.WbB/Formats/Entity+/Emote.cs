using GameX.WbB.Formats.Props;

namespace GameX.WbB.Formats.Entity
{
    /// <summary>
    /// Emotes are similar to a data-driven event system
    /// </summary>
    public class Emote
    {
        public EmoteType Type;
        public float Delay;
        public float Extent;
        public ulong Amount;
        public ulong HeroXP;
        public ulong Min;
        public ulong Max;
        public double MinFloat;
        public double MaxFloat;
        public uint Stat;
        public uint Motion;
        public PlayScript PScript;
        public Sound Sound;
        public CreateProfile CreateProfile;
        public Frame Frame;
        public uint SpellId;
        public string TestString;
        public string Message;
        public double Percent;
        public int Display;
        public int Wealth;
        public int Loot;
        public int LootType;
        public Position Position;
    }
}
