using GameX.Origin.Formats.U9;
using System;
using static OpenStack.Debug;

namespace GameX.Origin.Games.U9
{
    public static class Database
    {
        public static PakFile PakFile;

        internal static FamilyGame Ensure(FamilyGame game)
        {
            PakFile = game.Family.OpenPakFile(new Uri("game:/#U9"));
            PakFile.LoadFileObject<Binary_Palette>("static/ankh.pal");
            return game;
        }
    }
}
