using System;
namespace GameSpec.Unreal.Formats.Core2
{
    public partial class FArchive
    {
        static Game GForceGame = Game.UNKNOWN;
        static Platform GForcePlatform = Platform.UNKNOWN;

        Game _game;
        int _gameSetCount;
        Game Game
        {
            get => _game;
            set { _game = value; _gameSetCount++; }
        }
        void CheckGameCollision()
        {
            if (_gameSetCount > 1) throw new Exception($"DetectGame collision: detected {_gameSetCount} titles, Ver={ArVer}, LicVer={ArLicenseeVer}");
        }

        Platform Platform;

        public int ArVer;
        public int ArLicenseeVer;
        public bool ReverseBytes;
    }
}