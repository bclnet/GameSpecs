using GameX.Id.Formats.Q;
using System;
using static OpenStack.Debug;

namespace GameX.Id.Games.Q
{
    public static class Database
    {
        public static PakFile PakFile;

        internal static FamilyGame Ensure(FamilyGame game)
        {
            PakFile = game.Family.OpenPakFile(new Uri("game:/#Q"));
            PakFile.LoadFileObject<Binary_Lump>("PAK0.PAK:gfx/palette.lmp");
            PakFile.LoadFileObject<Binary_Lump>("PAK0.PAK:gfx/colormap.lmp");
            return game;
        }
    }
}
