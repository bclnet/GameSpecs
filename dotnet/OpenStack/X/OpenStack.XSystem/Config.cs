namespace System.NumericsX.OpenStack
{
    public static class Config
    {
        // paths
        public const string BASE_GAMEDIR = "base";
        public const string BUILD_LIBRARY_SUFFIX = "/libdes_game.so";

        // CD Key file info
        // goes into BASE_GAMEDIR whatever the fs_game is set to two distinct files for easier win32 installer job
        public const string CDKEY_FILE = "doomkey";
        public const string XPKEY_FILE = "xpkey";
        public const string CDKEY_TEXT = "\n// Do not give this file to ANYONE.\n"
                                        + "// id Software or Zenimax will NEVER ask you to send this file to them.\n";

        public const string CONFIG_SPEC = "config.spec";

        // default idnet host address
        public const string IDNET_HOST = "idnet.ua-corp.com";

        // default idnet master port
        public const string IDNET_MASTER_PORT = "27650";

        // default network server port
        public const int PORT_SERVER = 27666;

        // broadcast scan this many ports after PORT_SERVER so a single machine can run multiple servers
        public const int NUM_SERVER_PORTS = 4;

        // use a different major for each game
        public const int ASYNC_PROTOCOL_MAJOR = 1;

        // Savegame Version
        // Update when you can no longer maintain compatibility with previous savegames
        // NOTE: a seperate core savegame version and game savegame version could be useful
        // 16: Doom v1.1
        // 17: Doom v1.2 / D3XP. Can still read old v16 with defaults for new data
        // 18: Doom3Quest v1.0
        public const int SAVEGAME_VERSION = 18;

        // <= Doom v1.1: 1. no DS_VERSION token ( default )
        // Doom v1.2: 2
        public const int RENDERDEMO_VERSION = 2;

        #region BUILD DEFINES

        public const int ASYNC_PROTOCOL_MINOR = 42;
        public const int ASYNC_PROTOCOL_VERSION = (ASYNC_PROTOCOL_MAJOR << 16) + ASYNC_PROTOCOL_MINOR;

        public const int MAX_ASYNC_CLIENTS = 32;

        public const int MAX_USERCMD_BACKUP = 256;
        public const int MAX_USERCMD_DUPLICATION = 25;
        public const int MAX_USERCMD_RELAY = 10;

        // index 0 is hardcoded to be the idnet master which leaves 4 to user customization
        public const int MAX_MASTER_SERVERS = 5;

        public const int MAX_NICKLEN = 32;

        // max number of servers that will be scanned for at a single IP address
        public const int MAX_SERVER_PORTS = 8;

        // special game init ids
        public const int GAME_INIT_ID_INVALID = -1;
        public const int GAME_INIT_ID_MAP_LOAD = -2;

        #endregion
    }
}