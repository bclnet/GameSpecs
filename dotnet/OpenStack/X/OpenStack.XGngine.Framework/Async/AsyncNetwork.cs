using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Framework.C;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework.Async
{
    // unreliable server . client messages
    public enum SERVER_UNRELIABLE_MESSAGE
    {
        EMPTY = 0,
        PING,
        GAMEINIT,
        SNAPSHOT
    }

    // reliable server . client messages
    public enum SERVER_RELIABLE_MESSAGE
    {
        PURE = 0,
        RELOAD,
        CLIENTINFO,
        SYNCEDCVARS,
        PRINT,
        DISCONNECT,
        APPLYSNAPSHOT,
        GAME,
        ENTERGAME
    }

    // unreliable client . server messages
    public enum CLIENT_UNRELIABLE_MESSAGE : byte
    {
        EMPTY = 0,
        PINGRESPONSE,
        USERCMD
    }

    // reliable client . server messages
    public enum CLIENT_RELIABLE_MESSAGE : byte
    {
        PURE = 0,
        CLIENTINFO,
        PRINT,
        DISCONNECT,
        GAME
    }

    // server print messages
    public enum SERVER_PRINT
    {
        MISC = 0,
        BADPROTOCOL,
        RCON,
        GAMEDENY,
        BADCHALLENGE
    }

    public enum SERVER_DL
    {
        REDIRECT = 1,
        LIST,
        NONE
    }

    public enum SERVER_PAK
    {
        NO = 0,
        YES,
        END
    }

    public struct Master
    {
        public CVar var;
        public Netadr address;
        public bool resolved;
    }

    public class AsyncNetwork
    {
        public AsyncNetwork() { }

        public static void Init()
        {
            realTime = 0;

            Array.Clear(masters, 0, masters.Length);
            masters[0].var = master0;
            masters[1].var = master1;
            masters[2].var = master2;
            masters[3].var = master3;
            masters[4].var = master4;

            cmdSystem.AddCommand("spawnServer", SpawnServer_f, CMD_FL.SYSTEM, "spawns a server", CmdArgs.ArgCompletion_MapName);
            cmdSystem.AddCommand("nextMap", NextMap_f, CMD_FL.SYSTEM, "loads the next map on the server");
            cmdSystem.AddCommand("connect", Connect_f, CMD_FL.SYSTEM, "connects to a server");
            cmdSystem.AddCommand("reconnect", Reconnect_f, CMD_FL.SYSTEM, "reconnect to the last server we tried to connect to");
            cmdSystem.AddCommand("serverInfo", GetServerInfo_f, CMD_FL.SYSTEM, "shows server info");
            cmdSystem.AddCommand("LANScan", GetLANServers_f, CMD_FL.SYSTEM, "scans LAN for servers");
            cmdSystem.AddCommand("listServers", ListServers_f, CMD_FL.SYSTEM, "lists scanned servers");
            cmdSystem.AddCommand("rcon", RemoteConsole_f, CMD_FL.SYSTEM, "sends remote console command to server");
            cmdSystem.AddCommand("heartbeat", Heartbeat_f, CMD_FL.SYSTEM, "send a heartbeat to the the master servers");
            cmdSystem.AddCommand("kick", Kick_f, CMD_FL.SYSTEM, "kick a client by connection number");
            cmdSystem.AddCommand("checkNewVersion", CheckNewVersion_f, CMD_FL.SYSTEM, "check if a new version of the game is available");
            cmdSystem.AddCommand("updateUI", UpdateUI_f, CMD_FL.SYSTEM, "internal - cause a sync down of game-modified userinfo");
        }

        public static void Shutdown()
        {
            client.serverList.Shutdown();
            client.DisconnectFromServer();
            client.ClearServers();
            client.ClosePort();
            server.Kill();
            server.ClosePort();
        }

        public static bool IsActive
            => server.IsActive || client.IsActive;

        public static void RunFrame()
        {
            if (console.Active) { SysW.GrabMouseCursor(false); usercmdGen.InhibitUsercmd(INHIBIT.ASYNC, true); }
            else { SysW.GrabMouseCursor(true); usercmdGen.InhibitUsercmd(INHIBIT.ASYNC, false); }
            client.RunFrame();
            server.RunFrame();
        }

        public static void WriteUserCmdDelta(BitMsg msg, Usercmd cmd, Usercmd? base_)
        {
            if (base_ != null)
            {
                msg.WriteDeltaIntCounter(base_.Value.gameTime, cmd.gameTime);
                msg.WriteDeltaByte(base_.Value.buttons, cmd.buttons);
                msg.WriteDeltaShort(base_.Value.mx, cmd.mx);
                msg.WriteDeltaShort(base_.Value.my, cmd.my);
                msg.WriteDeltaChar(base_.Value.forwardmove, cmd.forwardmove);
                msg.WriteDeltaChar(base_.Value.rightmove, cmd.rightmove);
                msg.WriteDeltaChar(base_.Value.upmove, cmd.upmove);
                msg.WriteDeltaShort(base_.Value.angles0, cmd.angles0);
                msg.WriteDeltaShort(base_.Value.angles1, cmd.angles1);
                msg.WriteDeltaShort(base_.Value.angles2, cmd.angles2);
                return;
            }

            msg.WriteInt(cmd.gameTime);
            msg.WriteByte(cmd.buttons);
            msg.WriteShort(cmd.mx);
            msg.WriteShort(cmd.my);
            msg.WriteChar(cmd.forwardmove);
            msg.WriteChar(cmd.rightmove);
            msg.WriteChar(cmd.upmove);
            msg.WriteShort(cmd.angles0);
            msg.WriteShort(cmd.angles1);
            msg.WriteShort(cmd.angles2);
        }

        public static void ReadUserCmdDelta(BitMsg msg, ref Usercmd cmd, Usercmd? base_)
        {
            cmd = default;
            if (base_ != null)
            {
                cmd.gameTime = msg.ReadDeltaIntCounter(base_.Value.gameTime);
                cmd.buttons = msg.ReadDeltaByte(base_.Value.buttons);
                cmd.mx = msg.ReadDeltaShort(base_.Value.mx);
                cmd.my = msg.ReadDeltaShort(base_.Value.my);
                cmd.forwardmove = msg.ReadDeltaChar(base_.Value.forwardmove);
                cmd.rightmove = msg.ReadDeltaChar(base_.Value.rightmove);
                cmd.upmove = msg.ReadDeltaChar(base_.Value.upmove);
                cmd.angles0 = msg.ReadDeltaShort(base_.Value.angles0);
                cmd.angles1 = msg.ReadDeltaShort(base_.Value.angles1);
                cmd.angles2 = msg.ReadDeltaShort(base_.Value.angles2);
                return;
            }

            cmd.gameTime = msg.ReadInt();
            cmd.buttons = msg.ReadByte();
            cmd.mx = msg.ReadShort();
            cmd.my = msg.ReadShort();
            cmd.forwardmove = msg.ReadChar();
            cmd.rightmove = msg.ReadChar();
            cmd.upmove = msg.ReadChar();
            cmd.angles0 = msg.ReadShort();
            cmd.angles1 = msg.ReadShort();
            cmd.angles2 = msg.ReadShort();
        }

        public static bool DuplicateUsercmd(Usercmd previousUserCmd, Usercmd currentUserCmd, int frame, int time)
        {
            if (currentUserCmd.gameTime <= previousUserCmd.gameTime)
            {
                currentUserCmd = previousUserCmd;
                currentUserCmd.gameFrame = frame;
                currentUserCmd.gameTime = time;
                currentUserCmd.duplicateCount++;

                if (currentUserCmd.duplicateCount > Config.MAX_USERCMD_DUPLICATION)
                {
                    //currentUserCmd.buttons = unchecked((byte)(currentUserCmd.buttons & ~Usercmd.BUTTON_ATTACK));
                    currentUserCmd.buttons &= unchecked((byte)~Usercmd.BUTTON_ATTACK);
                    if (Math.Abs(currentUserCmd.forwardmove) > 2) currentUserCmd.forwardmove >>= 1;
                    if (Math.Abs(currentUserCmd.rightmove) > 2) currentUserCmd.rightmove >>= 1;
                    if (Math.Abs(currentUserCmd.upmove) > 2) currentUserCmd.upmove >>= 1;
                }

                return true;
            }
            return false;
        }

        public static bool UsercmdInputChanged(Usercmd previousUserCmd, Usercmd currentUserCmd)
            => previousUserCmd.buttons != currentUserCmd.buttons ||
                previousUserCmd.forwardmove != currentUserCmd.forwardmove ||
                previousUserCmd.rightmove != currentUserCmd.rightmove ||
                previousUserCmd.upmove != currentUserCmd.upmove ||
                previousUserCmd.angles0 != currentUserCmd.angles0 ||
                previousUserCmd.angles1 != currentUserCmd.angles1 ||
                previousUserCmd.angles2 != currentUserCmd.angles2;

        // returns true if the corresponding master is set to something (and could be resolved)
        public static bool GetMasterAddress(int index, out Netadr adr)
        {
            if (masters[index].var == null || string.IsNullOrEmpty(masters[index].var.String)) { adr = default; return false; }
            if (!masters[index].resolved || masters[index].var.IsModified)
            {
                masters[index].var.ClearModified();
                if (!Netadr.TryParse(masters[index].var.String, out masters[index].address, true))
                {
                    common.Printf($"Failed to resolve master{index}: {masters[index].var.String}\n");
                    masters[index].address.memset();
                    masters[index].resolved = true;
                    adr = default;
                    return false;
                }
                if (masters[index].address.port == 0) masters[index].address.port = ushort.Parse(Config.IDNET_MASTER_PORT);
                masters[index].resolved = true;
            }
            adr = masters[index].address;
            return true;
        }

        // get the hardcoded idnet master, equivalent to GetMasterAddress( 0, .. )
        public static Netadr MasterAddress
        {
            get { GetMasterAddress(0, out var ret); return masters[0].address; }
        }

        public static void GetNETServers()
            => client.GetNETServers();

        public static void ExecuteSessionCommand(string sessCmd)
        {
            if (!string.IsNullOrEmpty(sessCmd) && string.Equals(sessCmd, "game_startmenu", StringComparison.OrdinalIgnoreCase))
                session.SetGUI(game.StartMenu, null);
        }

        public static AsyncServer server = new();
        public static AsyncClient client = new();

        public static CVar verbose = new("net_verbose", "0", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "1 = verbose output, 2 = even more verbose output", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));                     // verbose output
        public static CVar allowCheats = new("net_allowCheats", "0", CVAR.SYSTEM | CVAR.BOOL | CVAR.NETWORKSYNC, "Allow cheats in network game");                 // allow cheats
#if ID_DEDICATED
        public static CVar serverDedicated = new("net_serverDedicated", "1", CVAR.SERVERINFO | CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT | CVAR.ROM, "");             // if set run a dedicated server
#else
        public static CVar serverDedicated = new("net_serverDedicated", "1", CVAR.SERVERINFO | CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT | CVAR.ROM, "");             // if set run a dedicated server
#endif
        public static CVar serverSnapshotDelay = new("net_serverSnapshotDelay", "50", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "delay between snapshots in milliseconds");         // number of milliseconds between snapshots
        public static CVar serverMaxClientRate = new("net_serverMaxClientRate", "16000", CVAR.SYSTEM | CVAR.INTEGER | CVAR.ARCHIVE | CVAR.NOCHEAT, "maximum rate to a client in bytes/sec");         // maximum outgoing rate to clients
        public static CVar clientMaxRate = new("net_clientMaxRate", "16000", CVAR.SYSTEM | CVAR.INTEGER | CVAR.ARCHIVE | CVAR.NOCHEAT, "maximum rate requested by client from server in bytes/sec");                   // maximum rate from server requested by client
        public static CVar serverMaxUsercmdRelay = new("net_serverMaxUsercmdRelay", "5", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "maximum number of usercmds from other clients the server relays to a client", 1, Config.MAX_USERCMD_RELAY, CmdArgs.ArgCompletion_Integer(1, Config.MAX_USERCMD_RELAY));           // maximum number of usercmds relayed to other clients
        public static CVar serverZombieTimeout = new("net_serverZombieTimeout", "5", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "disconnected client timeout in seconds");         // time out in seconds for zombie clients
        public static CVar serverClientTimeout = new("net_serverClientTimeout", "40", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "client time out in seconds");         // time out in seconds for connected clients
        public static CVar clientServerTimeout = new("net_clientServerTimeout", "40", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "server time out in seconds");         // time out in seconds for server
        public static CVar serverDrawClient = new("net_serverDrawClient", "-1", CVAR.SYSTEM | CVAR.INTEGER, "number of client for which to draw view on server");                // the server draws the view of this client
        public static CVar serverRemoteConsolePassword = new("net_serverRemoteConsolePassword", "", CVAR.SYSTEM | CVAR.NOCHEAT, "remote console password"); // remote console password
        public static CVar clientPrediction = new("net_clientPrediction", "16", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "additional client side prediction in milliseconds");                // how many additional milliseconds the clients runs ahead
        public static CVar clientMaxPrediction = new("net_clientMaxPrediction", "1000", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "maximum number of milliseconds a client can predict ahead of server.");         // max milliseconds into the future a client can run prediction
        public static CVar clientUsercmdBackup = new("net_clientUsercmdBackup", "5", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "number of usercmds to resend");         // how many usercmds the client sends from previous frames
        public static CVar clientRemoteConsoleAddress = new("net_clientRemoteConsoleAddress", "localhost", CVAR.SYSTEM | CVAR.NOCHEAT, "remote console address");      // remote console address
        public static CVar clientRemoteConsolePassword = new("net_clientRemoteConsolePassword", "", CVAR.SYSTEM | CVAR.NOCHEAT, "remote console password"); // remote console password
        public static CVar master0 = new("net_master0", $"{Config.IDNET_HOST}:{Config.IDNET_MASTER_PORT}", CVAR.SYSTEM | CVAR.ROM, "idnet master server address");                     // idnet master server
        public static CVar master1 = new("net_master1", "", CVAR.SYSTEM | CVAR.ARCHIVE, "1st master server address");                     // 1st master server
        public static CVar master2 = new("net_master2", "", CVAR.SYSTEM | CVAR.ARCHIVE, "2nd master server address");                     // 2nd master server
        public static CVar master3 = new("net_master3", "", CVAR.SYSTEM | CVAR.ARCHIVE, "3rd master server address");                     // 3rd master server
        public static CVar master4 = new("net_master4", "", CVAR.SYSTEM | CVAR.ARCHIVE, "4th master server address");                     // 4th master server
        public static CVar LANServer = new("net_LANServer", "0", CVAR.SYSTEM | CVAR.BOOL | CVAR.NOCHEAT, "config LAN games only - affects clients and servers");                       // LAN mode
        public static CVar serverReloadEngine = new("net_serverReloadEngine", "0", CVAR.SYSTEM | CVAR.INTEGER | CVAR.NOCHEAT, "perform a full reload on next map restart (including flushing referenced pak files) - decreased if > 0");              // reload engine on map change instead of growing the referenced paks
        //public static CVar serverAllowServerMod = new;            // let a pure server start with a different game code than what is referenced in game code
        public static CVar idleServer = new("si_idleServer", "0", CVAR.SYSTEM | CVAR.BOOL | CVAR.INIT | CVAR.SERVERINFO, "game clients are idle");                      // serverinfo reply, indicates all clients are idle
        public static CVar clientDownload = new("net_clientDownload", "1", CVAR.SYSTEM | CVAR.INTEGER | CVAR.ARCHIVE, "client pk4 downloads policy: 0 - never, 1 - ask, 2 - always (will still prompt for binary code)");                  // preferred download policy

        // same message used for offline check and network reply
        public static void BuildInvalidKeyMsg(out string msg, bool[] valid)
        {
            msg = string.Empty;
            if (!valid[0]) msg += common.LanguageDictGetString("#str_07194");
            if (fileSystem.HasD3XP && !valid[1])
            {
                if (msg.Length != 0) msg += "\n";
                msg += common.LanguageDictGetString("#str_07195");
            }
            msg += "\n";
            msg += common.LanguageDictGetString("#str_04304");
        }

        static int realTime;
        static Master[] masters = new Master[Config.MAX_MASTER_SERVERS];    // master1 etc.

        static void SpawnServer_f(CmdArgs args)
        {
            if (args.Count > 1) cvarSystem.SetCVarString("si_map", args[1]);

            // don't let a server spawn with singleplayer game type - it will crash
            if (string.Equals(cvarSystem.GetCVarString("si_gameType"), "singleplayer", StringComparison.OrdinalIgnoreCase))
                cvarSystem.SetCVarString("si_gameType", "deathmatch");
            com_asyncInput.Bool = false;
            // make sure the current system state is compatible with net_serverDedicated
            switch (cvarSystem.GetCVarInteger("net_serverDedicated"))
            {
                case 0:
                case 2:
                    if (!renderSystem.IsOpenGLRunning) common.Warning($"OpenGL is not running, net_serverDedicated == {cvarSystem.GetCVarInteger("net_serverDedicated")}");
                    break;
                case 1:
                    if (renderSystem.IsOpenGLRunning) { SysW.ShowConsole(1, false); renderSystem.ShutdownOpenGL(); }
                    soundSystem.SetMute(true);
                    soundSystem.ShutdownHW();
                    break;
            }
            // use serverMapRestart if we already have a running server
            if (server.IsActive) cmdSystem.BufferCommandText(CMD_EXEC.NOW, "serverMapRestart");
            else server.Spawn();
        }

        static void NextMap_f(CmdArgs args)
            => server.ExecuteMapChange();

        static void Connect_f(CmdArgs args)
        {
            if (server.IsActive) { common.Printf("already running a server\n"); return; }
            if (args.Count != 2) { common.Printf("USAGE: connect <serverName>\n"); return; }
            com_asyncInput.Bool = false;
            client.ConnectToServer(args[1]);
        }

        static void Reconnect_f(CmdArgs args)
            => client.Reconnect();

        static void GetServerInfo_f(CmdArgs args)
            => client.GetServerInfo(args[1]);

        static void GetLANServers_f(CmdArgs args)
            => client.GetLANServers();

        static void ListServers_f(CmdArgs args)
            => client.ListServers();

        static void RemoteConsole_f(CmdArgs args)
            => client.RemoteConsole(args.Args());

        static void Heartbeat_f(CmdArgs args)
        {
            if (!server.IsActive) { common.Printf("server is not running\n"); return; }
            server.MasterHeartbeat(true);
        }

        static void Kick_f(CmdArgs args)
        {
            if (!server.IsActive) { common.Printf("server is not running\n"); return; }

            var clientId = args[1];
            if (!stringX.IsNumeric(clientId)) { common.Printf("usage: kick <client number>\n"); return; }
            var clientNum = int.Parse(clientId);

            if (server.LocalClientNum == clientNum) { common.Printf("can't kick the host\n"); return; }

            server.DropClient(clientNum, "#str_07134");
        }

        static void CheckNewVersion_f(CmdArgs args)
            => client.SendVersionCheck();

        static void UpdateUI_f(CmdArgs args)
        {
            if (args.Count != 2) { common.Warning("AsyncNetwork::UpdateUI_f: wrong arguments\n"); return; }
            if (!server.IsActive) { common.Warning("AsyncNetwork::UpdateUI_f: server is not active\n"); return; }
            var clientNum = int.Parse(args.Args(1));
            server.UpdateUI(clientNum);
        }
    }
}