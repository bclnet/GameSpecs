#if false
namespace GameSpec
{
    public partial class FamilyManager
    {
        static string[] FamilyKeys = new[] { "Blizzard", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Blizzard",
            //GameId = "SC", // StarCraft
            //GameId = "D2R", // Diablo II: Resurrected
            //Missing: GameId = "W3", // Warcraft III: Reign of Chaos
            //Missing: GameId = "WOW", // World of Warcraft
            //Missing: GameId = "WOWC", // World of Warcraft: Classic
            //GameId = "SC2", // StarCraft II: Wings of Liberty
            GameId = "D3", // Diablo III
            //Missing: GameId = "HS", // Hearthstone
            //Missing: GameId = "HOTS", // Heroes of the Storm
            //Missing: GameId = "OW", // Overwatch
            //Missing: GameId = "DI", // Diablo Immortal
            //Missing: GameId = "OW2", // Overwatch 2
            //Missing: GameId = "D4", // Diablo IV
        };
    }
}
#endif