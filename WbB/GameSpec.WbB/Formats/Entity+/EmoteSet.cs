using GameSpec.WbB.Formats.Props;
using System.Collections.Generic;

namespace GameSpec.WbB.Formats.Entity
{
    public class EmoteSet
    {
        public EmoteCategory Category;
        public float Probability;
        public uint ClassId;
        public string Quest;
        public uint Style;
        public uint Substate;
        public uint VendorType;
        public float MinHealth;
        public float MaxHealth;
        public List<Emote> Emotes;
    }
}
