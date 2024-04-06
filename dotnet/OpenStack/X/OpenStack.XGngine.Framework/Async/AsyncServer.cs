using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Framework.Async.MsgChannel;
using static System.NumericsX.OpenStack.Gngine.Framework.Framework;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework.Async
{
    // states for the server's authorization process
    public enum AuthState : byte
    {
        CDK_WAIT = 0,   // we are waiting for a confirm/deny from auth this is subject to timeout if we don't hear from auth or a permanent wait if auth said so
        CDK_OK,
        CDK_ONLYLAN,
        CDK_PUREWAIT,
        CDK_PUREOK,
        CDK_MAXSTATES
    }

    // states from the auth server, while the client is in CDK_WAIT
    public enum AuthReply : byte
    {
        AUTH_NONE = 0,  // no reply yet
        AUTH_OK,        // this client is good
        AUTH_WAIT,      // wait - keep sending me srvAuth though
        AUTH_DENY,      // denied - don't send me anything about this client anymore
        AUTH_MAXSTATES
    }

    // message from auth to be forwarded back to the client some are locally hardcoded to save space, auth has the possibility to send a custom reply
    public enum AuthReplyMsg : byte
    {
        AUTH_REPLY_WAITING = 0, // waiting on an initial reply from auth
        AUTH_REPLY_UNKNOWN,     // client unknown to auth
        AUTH_REPLY_DENIED,      // access denied
        AUTH_REPLY_PRINT,       // custom message
        AUTH_REPLY_SRVWAIT,     // auth server replied and tells us he's working on it
        AUTH_REPLY_MAXSTATES
    }

    public class Challenge
    {
        public Netadr address;      // client address
        public int clientId;        // client identification
        public int challenge;       // challenge code
        public int time;            // time the challenge was created
        public int pingTime;        // time the challenge response was sent to client
        public bool connected;      // true if the client is connected
        public AuthState authState;     // local state regarding the client
        public AuthReply authReply;     // cd key check replies
        public AuthReplyMsg authReplyMsg;   // default auth messages
        public string authReplyPrint;   // custom msg
        public string guid;     // guid

        internal void memset()
        {
            throw new NotImplementedException();
        }
    }

    public enum ServerClientState
    {
        SCS_FREE,           // can be reused for a new connection
        SCS_ZOMBIE,         // client has been disconnected, but don't reuse connection for a couple seconds
        SCS_PUREWAIT,       // client needs to update it's pure checksums before we can go further
        SCS_CONNECTED,      // client is connected
        SCS_INGAME          // client is in the game
    }

    public class ServerClient
    {
        public int clientId;
        public ServerClientState clientState;
        public int clientPrediction;
        public int clientAheadTime;
        public int clientRate;
        public int clientPing;

        public int gameInitSequence;
        public int gameFrame;
        public int gameTime;

        public MsgChannel channel;
        public int lastConnectTime;
        public int lastEmptyTime;
        public int lastPingTime;
        public int lastSnapshotTime;
        public int lastPacketTime;
        public int lastInputTime;
        public int snapshotSequence;
        public int acknowledgeSnapshotSequence;
        public int numDuplicatedUsercmds;

        public string guid;  // Even Balance - M. Quinn
    }

    public class AsyncServer
    {
        const int MIN_RECONNECT_TIME = 2000;
        const int EMPTY_RESEND_TIME = 500;
        const int PING_RESEND_TIME = 500;
        const int NOINPUT_IDLE_TIME = 30000;
        const int HEARTBEAT_MSEC = 5 * 60 * 1000;

        // must be kept in sync with authReplyMsg_t
        static string[] authReplyMsg = {
            "#str_07204",   // "Waiting for authorization",
	        "#str_07205",   // "Client unknown to auth",
	        "#str_07206",   // "Access denied - CD Key in use",
	        "#str_07207",   // "Auth custom message", // placeholder - we propagate a message from the master
	        "#str_07208"    // "Authorize Server - Waiting for client"
        };

        static string[] authReplyStr = {
            "AUTH_NONE",
            "AUTH_OK",
            "AUTH_WAIT",
            "AUTH_DENY"
        };

        // MAX_CHALLENGES is made large to prevent a denial of service attack that could cycle all of them out before legitimate users connected
        public const int MAX_CHALLENGES = 1024;

        // if we don't hear from authorize server, assume it is down
        public const int AUTHORIZE_TIMEOUT = 5000;

        bool active;                        // true if server is active
        int realTime;                   // absolute time

        int serverTime;                 // local server time
        NetPort serverPort;                  // UDP port
        int serverId;                   // server identification
        int serverDataChecksum;         // checksum of the data used by the server
        int localClientNum;             // local client on listen server

        Challenge[] challenges = new Challenge[MAX_CHALLENGES]; // to prevent invalid IPs from connecting
        ServerClient[] clients = new ServerClient[Config.MAX_ASYNC_CLIENTS];   // clients
        Usercmd[][] userCmds = Enumerable.Repeat(new Usercmd[Config.MAX_ASYNC_CLIENTS], Config.MAX_USERCMD_BACKUP).ToArray();

        int gameInitId;                 // game initialization identification
        int gameFrame;                  // local game frame
        int gameTime;                   // local game time
        int gameTimeResidual;           // left over time from previous frame

        Netadr rconAddress;

        int nextHeartbeatTime;
        int nextAsyncStatsTime;

        bool serverReloadingEngine;     // flip-flop to not loop over when net_serverReloadEngine is on

        bool noRconOutput;              // for default rcon response when command is silent

        int lastAuthTime;               // global for auth server timeout

        // track the max outgoing rate over the last few secs to watch for spikes dependent on net_serverSnapshotDelay. 50ms, for a 3 seconds backlog . 60 samples
        static int stats_numsamples = 60;
        int[] stats_outrate = new int[stats_numsamples];
        int stats_current;
        int stats_average_sum;
        int stats_max;
        int stats_max_index;

        public AsyncServer()
        {
            active = false;
            realTime = 0;
            serverTime = 0;
            serverId = 0;
            serverDataChecksum = 0;
            localClientNum = -1;
            gameInitId = 0;
            gameFrame = 0;
            gameTime = 0;
            gameTimeResidual = 0;
            Array.Clear(challenges, 0, challenges.Length);
            Array.Clear(userCmds, 0, userCmds.Length);
            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) ClearClient(i);
            serverReloadingEngine = false;
            nextHeartbeatTime = 0;
            nextAsyncStatsTime = 0;
            noRconOutput = true;
            lastAuthTime = 0;
            Array.Clear(stats_outrate, 0, stats_outrate.Length);
            stats_current = 0;
            stats_average_sum = 0;
            stats_max = 0;
            stats_max_index = 0;
        }

        public bool InitPort()
        {
            int lastPort;

            // if this is the first time we have spawned a server, open the UDP port
            if (serverPort.Port == 0)
                if (cvarSystem.GetCVarInteger("net_port") != 0)
                {
                    if (!serverPort.InitForPort(cvarSystem.GetCVarInteger("net_port"))) { common.Printf($"Unable to open server on port {cvarSystem.GetCVarInteger("net_port")} (net_port)\n"); return false; }
                }
                else
                {
                    // scan for multiple ports, in case other servers are running on this IP already
                    for (lastPort = 0; lastPort < Config.NUM_SERVER_PORTS; lastPort++) if (serverPort.InitForPort(Config.PORT_SERVER + lastPort)) break;
                    if (lastPort >= Config.NUM_SERVER_PORTS) { common.Printf("Unable to open server network port.\n"); return false; }
                }
            return true;
        }

        public void ClosePort()
        {
            serverPort.Close();
            for (var i = 0; i < MAX_CHALLENGES; i++) challenges[i].authReplyPrint = string.Empty;
        }

        public void Spawn()
        {
            int i, size;
            byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            Netadr from;

            // shutdown any current game
            session.Stop();

            if (active) return;

            if (!InitPort()) return;

            // trash any currently pending packets
            while (serverPort.GetPacket(out from, msgBuf, out size, msgBuf.Length)) { }

            // reset cheats cvars
            if (!AsyncNetwork.allowCheats.Bool) cvarSystem.ResetFlaggedVariables(CVAR.CHEAT);

            Array.Clear(challenges, 0, challenges.Length);
            Array.Clear(userCmds, 0, userCmds.Length);
            for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) ClearClient(i);

            common.Printf($"Server spawned on port {serverPort.Port}.\n");

            // calculate a checksum on some of the essential data used
            serverDataChecksum = declManager.GetChecksum();

            // get a pseudo random server id, but don't use the id which is reserved for connectionless packets
            serverId = SysW.Milliseconds & CONNECTIONLESS_MESSAGE_ID_MASK;

            active = true;

            nextHeartbeatTime = 0;
            nextAsyncStatsTime = 0;

            ExecuteMapChange();
        }

        public void Kill()
        {
            int i, j;

            if (!active) return;

            // drop all clients
            for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) DropClient(i, "#str_07135");

            // send some empty messages to the zombie clients to make sure they disconnect
            for (j = 0; j < 4; j++)
            {
                for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
                    if (clients[i].clientState == ServerClientState.SCS_ZOMBIE)
                        if (clients[i].channel.UnsentFragmentsLeft) clients[i].channel.SendNextFragment(serverPort, serverTime);
                        else SendEmptyToClient(i, true);
                SysW.Sleep(10);
            }

            // reset any pureness
            fileSystem.ClearPureChecksums();

            active = false;

            // shutdown any current game
            session.Stop();
        }

        public void ExecuteMapChange()
        {
            int i;
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            bool addonReload = false;
            string bestGameType;

            Debug.Assert(active);

            // reset any pureness
            fileSystem.ClearPureChecksums();

            // make sure the map/gametype combo is good
            game.GetBestGameType(cvarSystem.GetCVarString("si_map"), cvarSystem.GetCVarString("si_gametype"), out bestGameType);
            cvarSystem.SetCVarString("si_gametype", bestGameType);

            // initialize map settings
            cmdSystem.BufferCommandText(CMD_EXEC.NOW, "rescanSI");

            var mapName = $"maps/{sessLocal.mapSpawnData.serverInfo.GetString("si_map")}.map";
            var ff = fileSystem.FindFile(mapName, !serverReloadingEngine);
            switch (ff)
            {
                case FIND.NO:
                    common.Printf("Can't find map {mapName}\n");
                    cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "disconnect\n");
                    return;
                case FIND.ADDON:
                    // NOTE: we have no problem with addon dependencies here because if the map is in an addon pack that's already on search list, then all it's deps are assumed to be on search as well
                    common.Printf("map {mapName} is in an addon pak - reloading\n");
                    addonReload = true;
                    break;
                default:
                    break;
            }

            // if we are asked to do a full reload, the strategy is completely different
            if (!serverReloadingEngine && (addonReload || AsyncNetwork.serverReloadEngine.Integer != 0))
            {
                if (AsyncNetwork.serverReloadEngine.Integer != 0) common.Printf("net_serverReloadEngine enabled - doing a full reload\n");
                // tell the clients to reconnect
                // FIXME: shouldn't they wait for the new pure list, then reload? in a lot of cases this is going to trigger two reloadEngines for the clients
                // one to restart, the other one to set paks right ( with addon for instance ) can fix by reconnecting without reloading and waiting for the server to tell..
                for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
                    if (clients[i].clientState >= ServerClientState.SCS_PUREWAIT && i != localClientNum)
                    {
                        msg.InitW(msgBuf);
                        msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.RELOAD);
                        SendReliableMessage(i, msg);
                        clients[i].clientState = ServerClientState.SCS_ZOMBIE; // so we don't bother sending a disconnect
                    }
                cmdSystem.BufferCommandText(CMD_EXEC.NOW, "reloadEngine");
                serverReloadingEngine = true; // don't get caught in endless loop
                cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "spawnServer\n");
                // decrease feature
                if (AsyncNetwork.serverReloadEngine.Integer > 0)
                    AsyncNetwork.serverReloadEngine.Integer--;
                return;
            }
            serverReloadingEngine = false;

            serverTime = 0;

            // initialize game id and time
            gameInitId ^= SysW.Milliseconds;   // NOTE: make sure the gameInitId is always a positive number because negative numbers have special meaning
            gameFrame = 0;
            gameTime = 0;
            gameTimeResidual = 0;
            Array.Clear(userCmds, 0, userCmds.Length);

            if (AsyncNetwork.serverDedicated.Integer == 0) InitLocalClient(0);
            else localClientNum = -1;

            // re-initialize all connected clients for the new map
            for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
                if (clients[i].clientState >= ServerClientState.SCS_PUREWAIT && i != localClientNum)
                {
                    InitClient(i, clients[i].clientId, clients[i].clientRate);

                    SendGameInitToClient(i);

                    if (sessLocal.mapSpawnData.serverInfo.GetBool("si_pure")) clients[i].clientState = ServerClientState.SCS_PUREWAIT;
                }

            // load map
            sessLocal.ExecuteMapChange();

            if (localClientNum >= 0) BeginLocalClient();
            else game.SetLocalClient(-1);

            if (sessLocal.mapSpawnData.serverInfo.GetInt("si_pure") != 0)
            {
                // lock down the pak list
                fileSystem.UpdatePureServerChecksums();
                // tell the clients so they can work out their pure lists
                for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) if (clients[i].clientState == ServerClientState.SCS_PUREWAIT && !SendReliablePureToClient(i)) clients[i].clientState = ServerClientState.SCS_CONNECTED;
            }

            // serverTime gets reset, force a heartbeat so timings restart
            MasterHeartbeat(true);
        }

        public int Port_
            => serverPort.Port;

        public Netadr BoundAdr
            => serverPort.Adr;

        public bool IsActive
            => active;
        public int Delay
            => gameTimeResidual;

        public int OutgoingRate
        {
            get
            {
                var rate = 0;
                for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
                {
                    var client = clients[i];
                    if (client.clientState >= ServerClientState.SCS_CONNECTED) rate += client.channel.OutgoingRate;
                }
                return rate;
            }
        }

        public int IncomingRate
        {
            get
            {
                var rate = 0;
                for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
                {
                    var client = clients[i];
                    if (client.clientState >= ServerClientState.SCS_CONNECTED) rate += client.channel.IncomingRate;
                }
                return rate;
            }
        }

        public bool IsClientInGame(int clientNum)
            => clients[clientNum].clientState >= ServerClientState.SCS_INGAME;

        public int GetClientPing(int clientNum)
        {
            var client = clients[clientNum];
            return client.clientState < ServerClientState.SCS_CONNECTED ? 99999 : client.clientPing;
        }

        public int GetClientPrediction(int clientNum)
        {
            var client = clients[clientNum];
            return client.clientState < ServerClientState.SCS_CONNECTED ? 99999 : client.clientPrediction;
        }

        public int GetClientTimeSinceLastPacket(int clientNum)
        {
            var client = clients[clientNum];
            return client.clientState < ServerClientState.SCS_CONNECTED ? 99999 : serverTime - client.lastPacketTime;
        }

        public int GetClientTimeSinceLastInput(int clientNum)
        {
            var client = clients[clientNum];
            return client.clientState < ServerClientState.SCS_CONNECTED ? 99999 : serverTime - client.lastInputTime;
        }

        public int GetClientOutgoingRate(int clientNum)
        {
            var client = clients[clientNum];
            return client.clientState < ServerClientState.SCS_CONNECTED ? -1 : client.channel.OutgoingRate;
        }

        public int GetClientIncomingRate(int clientNum)
        {
            var client = clients[clientNum];
            return client.clientState < ServerClientState.SCS_CONNECTED ? -1 : client.channel.IncomingRate;
        }

        public float GetClientOutgoingCompression(int clientNum)
        {
            var client = clients[clientNum];
            return client.clientState < ServerClientState.SCS_CONNECTED ? 0.0f : client.channel.OutgoingCompression;
        }

        public float GetClientIncomingCompression(int clientNum)
        {
            var client = clients[clientNum];
            return client.clientState < ServerClientState.SCS_CONNECTED ? 0.0f : client.channel.IncomingCompression;
        }

        public float GetClientIncomingPacketLoss(int clientNum)
        {
            var client = clients[clientNum];
            return client.clientState < ServerClientState.SCS_CONNECTED ? 0.0f : client.channel.IncomingPacketLoss;
        }

        public int NumClients
        {
            get
            {
                var ret = 0;
                for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) if (clients[i].clientState >= ServerClientState.SCS_CONNECTED) ret++;
                return ret;
            }
        }

        public int NumIdleClients
        {
            get
            {
                var ret = 0;
                for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) if (clients[i].clientState >= ServerClientState.SCS_CONNECTED && serverTime - clients[i].lastInputTime > NOINPUT_IDLE_TIME) ret++;
                return ret;
            }
        }

        public int LocalClientNum
            => localClientNum;

        void DuplicateUsercmds(int frame, int time)
        {
            var previousIndex = (frame - 1) & (Config.MAX_USERCMD_BACKUP - 1);
            var currentIndex = frame & (Config.MAX_USERCMD_BACKUP - 1);

            // duplicate previous user commands if no new commands are available for a client
            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                if (clients[i].clientState == ServerClientState.SCS_FREE) continue;
                if (AsyncNetwork.DuplicateUsercmd(userCmds[previousIndex][i], userCmds[currentIndex][i], frame, time)) clients[i].numDuplicatedUsercmds++;
            }
        }

        void ClearClient(int clientNum)
        {
            var client = clients[clientNum];
            client.clientId = 0;
            client.clientState = ServerClientState.SCS_FREE;
            client.clientPrediction = 0;
            client.clientAheadTime = 0;
            client.clientRate = 0;
            client.clientPing = 0;
            client.gameInitSequence = 0;
            client.gameFrame = 0;
            client.gameTime = 0;
            client.channel.Shutdown();
            client.lastConnectTime = 0;
            client.lastEmptyTime = 0;
            client.lastPingTime = 0;
            client.lastSnapshotTime = 0;
            client.lastPacketTime = 0;
            client.lastInputTime = 0;
            client.snapshotSequence = 0;
            client.acknowledgeSnapshotSequence = 0;
            client.numDuplicatedUsercmds = 0;
        }

        void InitClient(int clientNum, int clientId, int clientRate)
        {
            // clear the user info
            sessLocal.mapSpawnData.userInfo[clientNum].Clear(); // always start with a clean base

            // clear the server client
            var client = clients[clientNum];
            client.clientId = clientId;
            client.clientState = ServerClientState.SCS_CONNECTED;
            client.clientPrediction = 0;
            client.clientAheadTime = 0;
            client.gameInitSequence = -1;
            client.gameFrame = 0;
            client.gameTime = 0;
            client.channel.ResetRate();
            client.clientRate = clientRate != 0 ? clientRate : AsyncNetwork.serverMaxClientRate.Integer;
            client.channel.MaxOutgoingRate = Math.Min(AsyncNetwork.serverMaxClientRate.Integer, client.clientRate);
            client.clientPing = 0;
            client.lastConnectTime = serverTime;
            client.lastEmptyTime = serverTime;
            client.lastPingTime = serverTime;
            client.lastSnapshotTime = serverTime;
            client.lastPacketTime = serverTime;
            client.lastInputTime = serverTime;
            client.acknowledgeSnapshotSequence = 0;
            client.numDuplicatedUsercmds = 0;

            // clear the user commands
            for (var i = 0; i < Config.MAX_USERCMD_BACKUP; i++) userCmds[i][clientNum] = default;

            // let the game know a player connected
            game.ServerClientConnect(clientNum, client.guid);
        }

        void InitLocalClient(int clientNum)
        {
            Netadr badAddress = new();

            localClientNum = clientNum;
            InitClient(clientNum, 0, 0);
            badAddress.memset();
            badAddress.type = NA.BAD;
            clients[clientNum].channel.Init(badAddress, serverId);
            clients[clientNum].clientState = ServerClientState.SCS_INGAME;
            sessLocal.mapSpawnData.userInfo[clientNum] = cvarSystem.MoveCVarsToDict(CVAR.USERINFO);
        }

        void BeginLocalClient()
        {
            game.SetLocalClient(localClientNum);
            game.SetUserInfo(localClientNum, sessLocal.mapSpawnData.userInfo[localClientNum], false, false);
            game.ServerClientBegin(localClientNum);
        }

        void LocalClientInput()
        {
            if (localClientNum < 0) return;

            var index = gameFrame & (Config.MAX_USERCMD_BACKUP - 1);
            userCmds[index][localClientNum] = usercmdGen.GetDirectUsercmd();
            userCmds[index][localClientNum].gameFrame = gameFrame;
            userCmds[index][localClientNum].gameTime = gameTime;
            if (AsyncNetwork.UsercmdInputChanged(userCmds[(gameFrame - 1) & (Config.MAX_USERCMD_BACKUP - 1)][localClientNum], userCmds[index][localClientNum])) clients[localClientNum].lastInputTime = serverTime;
            clients[localClientNum].gameFrame = gameFrame;
            clients[localClientNum].gameTime = gameTime;
            clients[localClientNum].lastPacketTime = serverTime;
        }

        public void DropClient(int clientNum, string reason)
        {
            int i;
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            var client = clients[clientNum];
            if (client.clientState <= ServerClientState.SCS_ZOMBIE) return;

            if (client.clientState >= ServerClientState.SCS_PUREWAIT && clientNum != localClientNum)
            {
                msg.InitW(msgBuf);
                msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.DISCONNECT);
                msg.WriteInt(clientNum);
                msg.WriteString(reason);
                // clientNum so SCS_PUREWAIT client gets it's own disconnect msg
                for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) if (i == clientNum || clients[i].clientState >= ServerClientState.SCS_CONNECTED) SendReliableMessage(i, msg);
            }

            reason = common.LanguageDictGetString(reason);
            common.Printf($"client {clientNum} {reason}\n");
            cmdSystem.BufferCommandText(CMD_EXEC.NOW, $"addChatLine \"{sessLocal.mapSpawnData.userInfo[clientNum].GetString("ui_name")}^0 {reason}\"");

            // remove the player from the game
            game.ServerClientDisconnect(clientNum);

            client.clientState = ServerClientState.SCS_ZOMBIE;
        }

        void SendReliableMessage(int clientNum, BitMsg msg)                // checks for overflow and disconnects the faulty client
        {
            if (clientNum == localClientNum) return;
            if (!clients[clientNum].channel.SendReliableMessage(msg)) { clients[clientNum].channel.ClearReliableMessages(); DropClient(clientNum, "#str_07136"); }
        }

        void CheckClientTimeouts()
        {
            var zombieTimeout = serverTime - AsyncNetwork.serverZombieTimeout.Integer * 1000;
            var clientTimeout = serverTime - AsyncNetwork.serverClientTimeout.Integer * 1000;

            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                var client = clients[i];
                if (i == localClientNum) continue;

                if (client.lastPacketTime > serverTime) { client.lastPacketTime = serverTime; continue; }
                if (client.clientState == ServerClientState.SCS_ZOMBIE && client.lastPacketTime < zombieTimeout) { client.channel.Shutdown(); client.clientState = ServerClientState.SCS_FREE; continue; }
                if (client.clientState >= ServerClientState.SCS_PUREWAIT && client.lastPacketTime < clientTimeout) { DropClient(i, "#str_07137"); continue; }
            }
        }

        void SendPrintBroadcast(string s)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.PRINT);
            msg.WriteString(s);

            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) if (clients[i].clientState >= ServerClientState.SCS_CONNECTED) SendReliableMessage(i, msg);
        }

        void SendPrintToClient(int clientNum, string s)
        {

            var client = clients[clientNum];
            if (client.clientState < ServerClientState.SCS_CONNECTED) return;

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.PRINT);
            msg.WriteString(s);

            SendReliableMessage(clientNum, msg);
        }

        void SendUserInfoBroadcast(int userInfoNum, Dictionary<string, string> info, bool sendToAll = false)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            Dictionary<string, string> gameInfo;
            bool gameModifiedInfo;

            gameInfo = game.SetUserInfo(userInfoNum, info, false, true);
            if (gameInfo != null) gameModifiedInfo = true;
            else { gameModifiedInfo = false; gameInfo = info; }

            if (userInfoNum == localClientNum)
            {
                common.DPrintf("local user info modified by server\n");
                cvarSystem.SetCVarsFromDict(gameInfo);
                cvarSystem.ClearModifiedFlags(CVAR.USERINFO); // don't emit back
            }

            msg.InitW(msgBuf);
            msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.CLIENTINFO);
            msg.WriteByte(userInfoNum);
            msg.WriteBits(gameModifiedInfo || sendToAll ? 0 : 1, 1);

#if ID_CLIENTINFO_TAGS
            msg.WriteInt(sessLocal.mapSpawnData.userInfo[userInfoNum].Checksum());
            common.DPrintf($"broadcast for client {userInfoNum}: 0x{sessLocal.mapSpawnData.userInfo[userInfoNum].Checksum():x}\n");
            sessLocal.mapSpawnData.userInfo[userInfoNum].Print();
#endif

            msg.WriteDeltaDict(gameInfo, gameModifiedInfo || sendToAll ? null : sessLocal.mapSpawnData.userInfo[userInfoNum]);

            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) if (clients[i].clientState >= ServerClientState.SCS_CONNECTED && (sendToAll || i != userInfoNum || gameModifiedInfo)) SendReliableMessage(i, msg);

            sessLocal.mapSpawnData.userInfo[userInfoNum] = gameInfo;
        }

        // if the game modifies userInfo, it will call this through command system we then need to get the info from the game, and broadcast to clients (using DeltaDict and our current mapSpawnData as a base)
        public void UpdateUI(int clientNum)
        {
            var info = game.GetUserInfo(clientNum);
            if (info != null) { common.Warning("AsyncServer::UpdateUI: no info from game\n"); return; }

            SendUserInfoBroadcast(clientNum, info, true);
        }

        void SendUserInfoToClient(int clientNum, int userInfoNum, Dictionary<string, string> info)
        {
            if (clients[clientNum].clientState < ServerClientState.SCS_CONNECTED) return;

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.CLIENTINFO);
            msg.WriteByte(userInfoNum);
            msg.WriteBits(0, 1);

#if ID_CLIENTINFO_TAGS
            msg.WriteInt(0);
            common.DPrintf($"user info {userInfoNum} to client {clientNum}: null base\n");
#endif

            msg.WriteDeltaDict(info, null);

            SendReliableMessage(clientNum, msg);
        }
        void SendSyncedCvarsBroadcast(Dictionary<string, string> cvars)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.SYNCEDCVARS);
            msg.WriteDeltaDict(cvars, sessLocal.mapSpawnData.syncedCVars);

            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) if (clients[i].clientState >= ServerClientState.SCS_CONNECTED) SendReliableMessage(i, msg);

            sessLocal.mapSpawnData.syncedCVars = cvars;
        }

        void SendSyncedCvarsToClient(int clientNum, Dictionary<string, string> cvars)
        {
            if (clients[clientNum].clientState < ServerClientState.SCS_CONNECTED) return;

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.SYNCEDCVARS);
            msg.WriteDeltaDict(cvars, null);

            SendReliableMessage(clientNum, msg);
        }

        void SendApplySnapshotToClient(int clientNum, int sequence)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.APPLYSNAPSHOT);
            msg.WriteInt(sequence);

            SendReliableMessage(clientNum, msg);
        }

        bool SendEmptyToClient(int clientNum, bool force = false)
        {
            var client = clients[clientNum];

            if (client.lastEmptyTime > realTime) client.lastEmptyTime = realTime;

            if (!force && (realTime - client.lastEmptyTime < EMPTY_RESEND_TIME)) return false;

            if (AsyncNetwork.verbose.Integer != 0) common.Printf($"sending empty to client {clientNum}: gameInitId = {gameInitId}, gameFrame = {gameFrame}, gameTime = {gameTime}\n");

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteInt(gameInitId);
            msg.WriteByte((byte)SERVER_UNRELIABLE_MESSAGE.EMPTY);

            client.channel.SendMessage(serverPort, serverTime, msg);
            client.lastEmptyTime = realTime;

            return true;
        }

        bool SendPingToClient(int clientNum)
        {
            var client = clients[clientNum];

            if (client.lastPingTime > realTime) client.lastPingTime = realTime;

            if (realTime - client.lastPingTime < PING_RESEND_TIME) return false;

            if (AsyncNetwork.verbose.Integer == 2) common.Printf($"pinging client {clientNum}: gameInitId = {gameInitId}, gameFrame = {gameFrame}, gameTime = {gameTime}\n");

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteInt(gameInitId);
            msg.WriteByte((byte)SERVER_UNRELIABLE_MESSAGE.PING);
            msg.WriteInt(realTime);

            client.channel.SendMessage(serverPort, serverTime, msg);
            client.lastPingTime = realTime;

            return true;
        }

        void SendGameInitToClient(int clientNum)
        {
            if (AsyncNetwork.verbose.Integer != 0) common.Printf($"sending gameinit to client {clientNum}: gameInitId = {gameInitId}, gameFrame = {gameFrame}, gameTime = {gameTime}\n");

            var client = clients[clientNum];

            // clear the unsent fragments. might flood winsock but that's ok
            while (client.channel.UnsentFragmentsLeft) client.channel.SendNextFragment(serverPort, serverTime);

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteInt(gameInitId);
            msg.WriteByte((byte)SERVER_UNRELIABLE_MESSAGE.GAMEINIT);
            msg.WriteInt(gameFrame);
            msg.WriteInt(gameTime);
            msg.WriteDeltaDict(sessLocal.mapSpawnData.serverInfo, null);
            client.gameInitSequence = client.channel.SendMessage(serverPort, serverTime, msg);
        }

        bool SendSnapshotToClient(int clientNum)
        {
            int i, j, index, numUsercmds;
            Usercmd? last;
            byte[] clientInPVS = new byte[Config.MAX_ASYNC_CLIENTS >> 3];

            var client = clients[clientNum];

            if (serverTime - client.lastSnapshotTime < AsyncNetwork.serverSnapshotDelay.Integer) return false;

            if (AsyncNetwork.verbose.Integer == 2) common.Printf($"sending snapshot to client {clientNum}: gameInitId = {gameInitId}, gameFrame = {gameFrame}, gameTime = {gameTime}\n");

            // how far is the client ahead of the server minus the packet delay
            client.clientAheadTime = client.gameTime - (gameTime + gameTimeResidual);

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            // write the snapshot
            msg.InitW(msgBuf);
            msg.WriteInt(gameInitId);
            msg.WriteByte((byte)SERVER_UNRELIABLE_MESSAGE.SNAPSHOT);
            msg.WriteInt(client.snapshotSequence);
            msg.WriteInt(gameFrame);
            msg.WriteInt(gameTime);
            msg.WriteByte(MathX.ClampChar(client.numDuplicatedUsercmds));
            msg.WriteShort(MathX.ClampShort(client.clientAheadTime));

            // write the game snapshot
            game.ServerWriteSnapshot(clientNum, client.snapshotSequence, msg, clientInPVS, Config.MAX_ASYNC_CLIENTS);

            // write the latest user commands from the other clients in the PVS to the snapshot
            for (last = null, i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                client = clients[i];

                if (client.clientState == ServerClientState.SCS_FREE || i == clientNum) continue;

                // if the client is not in the PVS
                if ((clientInPVS[i >> 3] & (1 << (i & 7))) == 0) continue;

                var maxRelay = MathX.ClampInt(1, Config.MAX_USERCMD_RELAY, AsyncNetwork.serverMaxUsercmdRelay.Integer);

                // Max( 1, to always send at least one cmd, which we know we have because we call DuplicateUsercmds in RunFrame
                numUsercmds = Math.Max(1, Math.Min(client.gameFrame, gameFrame + maxRelay) - gameFrame);
                msg.WriteByte(i);
                msg.WriteByte(numUsercmds);
                for (j = 0; j < numUsercmds; j++) { index = (gameFrame + j) & (Config.MAX_USERCMD_BACKUP - 1); AsyncNetwork.WriteUserCmdDelta(msg, userCmds[index][i], last); last = userCmds[index][i]; }
            }
            msg.WriteByte(Config.MAX_ASYNC_CLIENTS);

            client.channel.SendMessage(serverPort, serverTime, msg);
            client.lastSnapshotTime = serverTime;
            client.snapshotSequence++;
            client.numDuplicatedUsercmds = 0;

            return true;
        }

        void ProcessUnreliableClientMessage(int clientNum, BitMsg msg)
        {
            int i, acknowledgeSequence, clientGameInitId, clientGameFrame, numUsercmds, index; Usercmd? last;

            var client = clients[clientNum];

            if (client.clientState == ServerClientState.SCS_ZOMBIE) return;

            acknowledgeSequence = msg.ReadInt();
            clientGameInitId = msg.ReadInt();

            // while loading a map the client may send empty messages to keep the connection alive
            if (clientGameInitId == Config.GAME_INIT_ID_MAP_LOAD) { if (AsyncNetwork.verbose.Integer != 0) common.Printf($"ignore unreliable msg from client {clientNum}, gameInitId == ID_MAP_LOAD\n"); return; }

            // check if the client is in the right game
            if (clientGameInitId != gameInitId)
            {
                if (acknowledgeSequence > client.gameInitSequence)
                {
                    // the client is connected but not in the right game
                    client.clientState = ServerClientState.SCS_CONNECTED;

                    // send game init to client
                    SendGameInitToClient(clientNum);

                    if (sessLocal.mapSpawnData.serverInfo.GetBool("si_pure"))
                    {
                        client.clientState = ServerClientState.SCS_PUREWAIT;
                        if (!SendReliablePureToClient(clientNum)) client.clientState = ServerClientState.SCS_CONNECTED;
                    }
                }
                else if (AsyncNetwork.verbose.Integer != 0) common.Printf($"ignore unreliable msg from client {clientNum}, wrong gameInit, old sequence\n");
                return;
            }

            client.acknowledgeSnapshotSequence = msg.ReadInt();

            if (client.clientState == ServerClientState.SCS_CONNECTED)
            {
                // the client is in the right game
                client.clientState = ServerClientState.SCS_INGAME;

                // send the user info of other clients
                for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) if (clients[i].clientState >= ServerClientState.SCS_CONNECTED && i != clientNum) SendUserInfoToClient(clientNum, i, sessLocal.mapSpawnData.userInfo[i]);

                // send synchronized cvars to client
                SendSyncedCvarsToClient(clientNum, sessLocal.mapSpawnData.syncedCVars);

                SendEnterGameToClient(clientNum);

                // get the client running in the game
                game.ServerClientBegin(clientNum);

                // write any reliable messages to initialize the client game state
                game.ServerWriteInitialReliableMessages(clientNum);
            }
            else if (client.clientState == ServerClientState.SCS_INGAME)
            {
                // apply the last snapshot the client received
                if (game.ServerApplySnapshot(clientNum, client.acknowledgeSnapshotSequence)) SendApplySnapshotToClient(clientNum, client.acknowledgeSnapshotSequence);
            }

            // process the unreliable message
            var id = (CLIENT_UNRELIABLE_MESSAGE)msg.ReadByte();
            switch (id)
            {
                case CLIENT_UNRELIABLE_MESSAGE.EMPTY:
                    if (AsyncNetwork.verbose.Integer != 0) common.Printf($"received empty message for client {clientNum}\n");
                    break;
                case CLIENT_UNRELIABLE_MESSAGE.PINGRESPONSE:
                    client.clientPing = realTime - msg.ReadInt();
                    break;
                case CLIENT_UNRELIABLE_MESSAGE.USERCMD:
                    client.clientPrediction = msg.ReadShort();

                    // read user commands
                    clientGameFrame = msg.ReadInt();
                    numUsercmds = msg.ReadByte();
                    for (last = null, i = clientGameFrame - numUsercmds + 1; i <= clientGameFrame; i++)
                    {
                        index = i & (Config.MAX_USERCMD_BACKUP - 1);
                        AsyncNetwork.ReadUserCmdDelta(msg, ref userCmds[index][clientNum], last);
                        userCmds[index][clientNum].gameFrame = i;
                        userCmds[index][clientNum].duplicateCount = 0;
                        if (AsyncNetwork.UsercmdInputChanged(userCmds[(i - 1) & (Config.MAX_USERCMD_BACKUP - 1)][clientNum], userCmds[index][clientNum])) client.lastInputTime = serverTime;
                        last = userCmds[index][clientNum];
                    }

                    if (last != null)
                    {
                        client.gameFrame = last.Value.gameFrame;
                        client.gameTime = last.Value.gameTime;
                    }

                    if (AsyncNetwork.verbose.Integer == 2) common.Printf($"received user command for client {clientNum}, gameInitId = {clientGameInitId}, gameFrame, {client.gameFrame} gameTime {client.gameTime}\n");
                    break;
                default:
                    common.Printf($"unknown unreliable message {id} from client {clientNum}\n");
                    break;
            }
        }

        void ProcessReliableClientMessages(int clientNum)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            var client = clients[clientNum];

            msg.InitW(msgBuf);

            while (client.channel.GetReliableMessage(msg))
            {
                var id = (CLIENT_RELIABLE_MESSAGE)msg.ReadByte();
                switch (id)
                {
                    case CLIENT_RELIABLE_MESSAGE.CLIENTINFO:
                        Dictionary<string, string> info = new();
                        msg.ReadDeltaDict(info, sessLocal.mapSpawnData.userInfo[clientNum]);
                        SendUserInfoBroadcast(clientNum, info);
                        break;
                    case CLIENT_RELIABLE_MESSAGE.PRINT:
                        msg.ReadString(out var s);
                        common.Printf("{s}\n");
                        break;
                    case CLIENT_RELIABLE_MESSAGE.DISCONNECT:
                        DropClient(clientNum, "#str_07138");
                        break;
                    case CLIENT_RELIABLE_MESSAGE.PURE:
                        // we get this message once the client has successfully updated it's pure list
                        ProcessReliablePure(clientNum, msg);
                        break;
                    default:
                        // pass reliable message on to game code
                        game.ServerProcessReliableMessage(clientNum, msg);
                        break;
                }
            }
        }

        void ProcessAuthMessage(BitMsg msg)
        {
            int i; string replyPrintMsg = null;

            var replyMsg = AuthReplyMsg.AUTH_REPLY_WAITING;
            var reply = (AuthReply)msg.ReadByte();
            if (reply <= 0 || reply >= AuthReply.AUTH_MAXSTATES) { common.DPrintf($"auth: invalid reply {reply}\n"); return; }
            var clientId = msg.ReadShort();
            msg.ReadNetadr(out var client_from);
            msg.ReadString(out var client_guid);
            if (reply != AuthReply.AUTH_OK)
            {
                replyMsg = (AuthReplyMsg)msg.ReadByte();
                if (replyMsg <= 0 || replyMsg >= AuthReplyMsg.AUTH_REPLY_MAXSTATES) { common.DPrintf($"auth: invalid reply msg {replyMsg}\n"); return; }
                if (replyMsg == AuthReplyMsg.AUTH_REPLY_PRINT) msg.ReadString(out replyPrintMsg);
            }

            lastAuthTime = serverTime;

            // no message parsing below

            for (i = 0; i < MAX_CHALLENGES; i++)
            {
                if (!challenges[i].connected && challenges[i].clientId == clientId)
                {
                    // return if something is wrong. break if we have found a valid auth
                    if (string.IsNullOrEmpty(challenges[i].guid)) { common.DPrintf($"auth: client {challenges[i].address} has no guid yet\n"); return; }
                    if (challenges[i].guid == client_guid) { common.DPrintf($"auth: client {challenges[i].address} {challenges[i].guid} not matched, auth server says guid {client_guid}\n"); return; }
                    // let auth work when server and master don't see the same IP
                    if (client_from != challenges[i].address) common.DPrintf($"auth: matched guid '{client_guid}' for != IPs {client_from} and {challenges[i].address}\n");
                    break;
                }
            }
            if (i >= MAX_CHALLENGES) { common.DPrintf($"auth: failed client lookup {client_from} {client_guid}\n"); return; }

            if (challenges[i].authState != AuthState.CDK_WAIT) { common.DWarning($"auth: challenge 0x{challenges[i].challenge:x} {challenges[i].address} authState {challenges[i].authState} != CDK_WAIT"); return; }

            challenges[i].guid = client_guid;
            if (reply == AuthReply.AUTH_OK) { challenges[i].authState = AuthState.CDK_OK; common.Printf($"client {client_from} {client_guid} is authed\n"); }
            else
            {
                var lmsg = replyMsg != AuthReplyMsg.AUTH_REPLY_PRINT
                    ? authReplyMsg[(int)replyMsg]
                    : replyPrintMsg;
                // maybe localize it
                lmsg = common.LanguageDictGetString(lmsg);
                common.DPrintf($"auth: client {client_from} {client_guid} - {authReplyStr[(int)reply]} {lmsg}\n");
                challenges[i].authReply = reply;
                challenges[i].authReplyMsg = replyMsg;
                challenges[i].authReplyPrint = replyPrintMsg;
            }
        }
        void ProcessChallengeMessage(Netadr from, BitMsg msg)
        {
            int i, clientId, oldest, oldestTime;
            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            clientId = msg.ReadInt();

            oldest = 0;
            oldestTime = 0x7fffffff;

            // see if we already have a challenge for this ip
            for (i = 0; i < MAX_CHALLENGES; i++)
            {
                if (!challenges[i].connected && from == challenges[i].address && clientId == challenges[i].clientId) break;
                if (challenges[i].time < oldestTime) { oldestTime = challenges[i].time; oldest = i; }
            }

            if (i >= MAX_CHALLENGES)
            {
                var rand = new Random();
                // this is the first time this client has asked for a challenge
                i = oldest;
                challenges[i].address = from;
                challenges[i].clientId = clientId;
                challenges[i].challenge = (rand.Next() << 16) ^ rand.Next() ^ serverTime;
                challenges[i].time = serverTime;
                challenges[i].connected = false;
                challenges[i].authState = AuthState.CDK_WAIT;
                challenges[i].authReply = AuthReply.AUTH_NONE;
                challenges[i].authReplyMsg = AuthReplyMsg.AUTH_REPLY_WAITING;
                challenges[i].authReplyPrint = string.Empty;
                challenges[i].guid = string.Empty;
            }
            challenges[i].pingTime = serverTime;

            common.Printf($"sending challenge 0x{challenges[i].challenge:x} to {from}\n");

            outMsg.InitW(msgBuf);
            outMsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            outMsg.WriteString("challengeResponse");
            outMsg.WriteInt(challenges[i].challenge);
            outMsg.WriteShort(serverId);
            outMsg.WriteString(cvarSystem.GetCVarString("fs_game_base"));
            outMsg.WriteString(cvarSystem.GetCVarString("fs_game"));

            serverPort.SendPacket(from, outMsg.DataW, outMsg.Size);

#if ID_ENFORCE_KEY_CLIENT
            // no CD Key check for LAN clients
            if (SysW.IsLANAddress(from)) challenges[i].authState = AuthState.CDK_OK;
            else
            {
                if (AsyncNetwork.LANServer.Bool) { common.Printf($"net_LANServer is enabled. Client {SysW.NetAdrToString(from)} is not a LAN address, will be rejected\n"); challenges[i].authState = AuthState.CDK_ONLYLAN; }
                else
                {
                    // emit a cd key confirmation request
                    outMsg.BeginWriting();
                    outMsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
                    outMsg.WriteString("srvAuth");
                    outMsg.WriteInt(Config.ASYNC_PROTOCOL_VERSION);
                    outMsg.WriteNetadr(from);
                    outMsg.WriteInt(-1); // this identifies "challenge" auth vs "connect" auth protocol 1.37 addition
                    outMsg.WriteByte(fileSystem.RunningD3XP() ? 1 : 0);
                    serverPort.SendPacket(AsyncNetwork.MasterAddress, outMsg.DataW, outMsg.Size);
                }
            }
#else
            if (!from.IsLANAddress) common.Printf($"Build Does not have CD Key Enforcement enabled. Client {from} is not a LAN address, but will be accepted\n");
            challenges[i].authState = AuthState.CDK_OK;
#endif
        }

        bool SendPureServerMessage(Netadr to)                                      // returns false if no pure paks on the list
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            int[] serverChecksums = new int[IVFileSystem.MAX_PURE_PAKS];
            int i;

            fileSystem.GetPureServerChecksums(serverChecksums);
            // happens if you run fully expanded assets with si_pure 1
            if (serverChecksums[0] == 0) { common.Warning("pure server has no pak files referenced"); return false; }
            common.DPrintf($"client {to}: sending pure pak list\n");

            // send our list of required paks
            msg.InitW(msgBuf);
            msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            msg.WriteString("pureServer");

            i = 0;
            while (serverChecksums[i] != 0) msg.WriteInt(serverChecksums[i++]);
            msg.WriteInt(0);

            serverPort.SendPacket(to, msg.DataW, msg.Size);
            return true;
        }

        bool SendReliablePureToClient(int clientNum)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            int[] serverChecksums = new int[IVFileSystem.MAX_PURE_PAKS];
            int i;

            fileSystem.GetPureServerChecksums(serverChecksums);
            // happens if you run fully expanded assets with si_pure 1
            if (serverChecksums[0] == 0) { common.Warning("pure server has no pak files referenced"); return false; }

            common.DPrintf($"client {clientNum}: sending pure pak list (reliable channel) @ gameInitId {gameInitId}\n");

            msg.InitW(msgBuf);
            msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.PURE);

            msg.WriteInt(gameInitId);

            i = 0;
            while (serverChecksums[i] != 0) msg.WriteInt(serverChecksums[i++]);
            msg.WriteInt(0);

            SendReliableMessage(clientNum, msg);

            return true;
        }

        int ValidateChallenge(Netadr from, int challenge, int clientId)    // returns -1 if validate failed
        {
            int i;
            for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                var client = clients[i];
                if (client.clientState == ServerClientState.SCS_FREE) continue;
                if (from == client.channel.RemoteAddress && (clientId == client.clientId || from.port == client.channel.RemoteAddress.port))
                {
                    if (serverTime - client.lastConnectTime < MIN_RECONNECT_TIME) { common.Printf($"{from}: reconnect rejected : too soon\n"); return -1; }
                    break;
                }
            }

            for (i = 0; i < MAX_CHALLENGES; i++) if (from == challenges[i].address && from.port == challenges[i].address.port && challenge == challenges[i].challenge) break;
            if (i == MAX_CHALLENGES) { PrintOOB(from, (int)SERVER_PRINT.BADCHALLENGE, "#str_04840"); return -1; }
            return i;
        }

        void ProcessConnectMessage(Netadr from, BitMsg msg)
        {
            int clientNum = 0, protocol, clientDataChecksum, challenge, clientId, ping, clientRate;
            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            string guid, password;
            int i, ichallenge, islot, numClients;

            protocol = msg.ReadInt();

            // check the protocol version - that's a msg back to a client, we don't know about it's localization, so send english
            if (protocol != Config.ASYNC_PROTOCOL_VERSION) { PrintOOB(from, (int)SERVER_PRINT.BADPROTOCOL, $"server uses protocol {Config.ASYNC_PROTOCOL_MAJOR}.{Config.ASYNC_PROTOCOL_MINOR }\n"); return; }

            clientDataChecksum = msg.ReadInt();
            challenge = msg.ReadInt();
            clientId = msg.ReadShort();
            clientRate = msg.ReadInt();

            // check the client data - only for non pure servers
            if (sessLocal.mapSpawnData.serverInfo.GetInt("si_pure") == 0 && clientDataChecksum != serverDataChecksum) { PrintOOB(from, (int)SERVER_PRINT.MISC, "#str_04842"); return; }

            if ((ichallenge = ValidateChallenge(from, challenge, clientId)) == -1) return;

            msg.ReadString(out guid);

            switch (challenges[ichallenge].authState)
            {
                case AuthState.CDK_PUREWAIT:
                    SendPureServerMessage(from);
                    return;
                case AuthState.CDK_ONLYLAN:
                    common.DPrintf($"{from}: not a lan client\n");
                    PrintOOB(from, (int)SERVER_PRINT.MISC, "#str_04843");
                    return;
                case AuthState.CDK_WAIT:
                    if (challenges[ichallenge].authReply == AuthReply.AUTH_NONE && Math.Min(serverTime - lastAuthTime, serverTime - challenges[ichallenge].time) > AUTHORIZE_TIMEOUT) { common.DPrintf($"{from}: Authorize server timed out\n"); break; } // will continue with the connecting process
                    var msg2 = challenges[ichallenge].authReplyMsg != AuthReplyMsg.AUTH_REPLY_PRINT
                        ? authReplyMsg[(int)challenges[ichallenge].authReplyMsg]
                        : challenges[ichallenge].authReplyPrint;
                    var lmsg = common.LanguageDictGetString(msg2);

                    common.DPrintf($"{from}: {lmsg}\n");

                    if (challenges[ichallenge].authReplyMsg == AuthReplyMsg.AUTH_REPLY_UNKNOWN || challenges[ichallenge].authReplyMsg == AuthReplyMsg.AUTH_REPLY_WAITING)
                    {
                        // the client may be trying to connect to us in LAN mode, and the server disagrees. let the client know so it would switch to authed connection
                        BitMsg outMsg2 = new(); byte[] msgBuf2 = new byte[MAX_MESSAGE_SIZE];
                        outMsg2.InitW(msgBuf2);
                        outMsg2.WriteShort(CONNECTIONLESS_MESSAGE_ID);
                        outMsg2.WriteString("authrequired");
                        serverPort.SendPacket(from, outMsg2.DataW, outMsg2.Size);
                    }

                    PrintOOB(from, (int)SERVER_PRINT.MISC, msg2);

                    // update the guid in the challenges
                    challenges[ichallenge].guid = guid;

                    // once auth replied denied, stop sending further requests
                    if (challenges[ichallenge].authReply != AuthReply.AUTH_DENY)
                    {
                        // emit a cd key confirmation request
                        outMsg.InitW(msgBuf);
                        outMsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
                        outMsg.WriteString("srvAuth");
                        outMsg.WriteInt(Config.ASYNC_PROTOCOL_VERSION);
                        outMsg.WriteNetadr(from);
                        outMsg.WriteInt(clientId);
                        outMsg.WriteString(guid);
                        // protocol 1.37 addition
                        outMsg.WriteByte(fileSystem.RunningD3XP ? 1 : 0);
                        serverPort.SendPacket(AsyncNetwork.MasterAddress, outMsg.DataW, outMsg.Size);
                    }
                    return;
                default:
                    Debug.Assert(challenges[ichallenge].authState == AuthState.CDK_OK || challenges[ichallenge].authState == AuthState.CDK_PUREOK);
                    break;
            }

            numClients = 0;
            for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                var client = clients[i];
                if (client.clientState >= ServerClientState.SCS_PUREWAIT) numClients++;
            }

            // game may be passworded, client banned by IP or GUID
            // if authState == CDK_PUREOK, the check was already performed once before entering pure checks. but meanwhile, the max players may have been reached
            msg.ReadString(out password);
            var reply = game.ServerAllowClient(numClients, from, guid, password, out var reason);
            if (reply != AllowReply.ALLOW_YES)
            {
                common.DPrintf($"game denied connection for {from}\n");

                // SERVER_PRINT_GAMEDENY passes the game opcode through. Don't use PrintOOB
                outMsg.InitW(msgBuf);
                outMsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
                outMsg.WriteString("print");
                outMsg.WriteInt((int)SERVER_PRINT.GAMEDENY);
                outMsg.WriteInt((int)reply);
                outMsg.WriteString(reason);
                serverPort.SendPacket(from, outMsg.DataW, outMsg.Size);
                return;
            }

            // enter pure checks if necessary
            if (sessLocal.mapSpawnData.serverInfo.GetInt("si_pure") != 0 && challenges[ichallenge].authState != AuthState.CDK_PUREOK)
                if (SendPureServerMessage(from)) { challenges[ichallenge].authState = AuthState.CDK_PUREWAIT; return; }

            // push back decl checksum here when running pure. just an additional safe check
            if (sessLocal.mapSpawnData.serverInfo.GetInt("si_pure") != 0 && clientDataChecksum != serverDataChecksum) { PrintOOB(from, (int)SERVER_PRINT.MISC, "#str_04844"); return; }

            ping = serverTime - challenges[ichallenge].pingTime;
            common.Printf($"challenge from {from} connecting with {ping} ping\n");
            challenges[ichallenge].connected = true;

            // find a slot for the client
            for (islot = 0; islot < 3; islot++)
            {
                for (clientNum = 0; clientNum < Config.MAX_ASYNC_CLIENTS; clientNum++)
                {
                    var client = clients[clientNum];
                    if (islot == 0)
                    {
                        // if this slot uses the same IP and port
                        if (from == client.channel.RemoteAddress && (clientId == client.clientId || from.port == client.channel.RemoteAddress.port)) break;
                    }
                    else if (islot == 1)
                    {
                        // if this client is not connected and the slot uses the same IP
                        if (client.clientState >= ServerClientState.SCS_PUREWAIT) continue;
                        if (from == client.channel.RemoteAddress) break;
                    }
                    else if (islot == 2)
                        // if this slot is free
                        if (client.clientState == ServerClientState.SCS_FREE) break;
                }

                if (clientNum < Config.MAX_ASYNC_CLIENTS)
                {
                    // initialize
                    clients[clientNum].channel.Init(from, serverId);
                    clients[clientNum].guid = guid; //clients[clientNum].guid[11] = 0;
                    break;
                }
            }

            // if no free spots available
            if (clientNum >= Config.MAX_ASYNC_CLIENTS) { PrintOOB(from, (int)SERVER_PRINT.MISC, "#str_04845"); return; }

            common.Printf($"sending connect response to {from}\n");

            // send connect response message
            outMsg.InitW(msgBuf);
            outMsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            outMsg.WriteString("connectResponse");
            outMsg.WriteInt(clientNum);
            outMsg.WriteInt(gameInitId);
            outMsg.WriteInt(gameFrame);
            outMsg.WriteInt(gameTime);
            outMsg.WriteDeltaDict(sessLocal.mapSpawnData.serverInfo, null);

            serverPort.SendPacket(from, outMsg.DataW, outMsg.Size);

            InitClient(clientNum, clientId, clientRate);

            clients[clientNum].gameInitSequence = 1;
            clients[clientNum].snapshotSequence = 1;

            // clear the challenge struct so a reconnect from this client IP starts clean
            challenges[ichallenge].memset();
        }

        bool VerifyChecksumMessage(int clientNum, Netadr from, BitMsg msg, out string reply) // if from is NULL, clientNum is used for error messages
        {
            int i, numChecksums;
            int[] checksums = new int[IVFileSystem.MAX_PURE_PAKS], serverChecksums = new int[IVFileSystem.MAX_PURE_PAKS];

            // pak checksums, in a 0-terminated list
            numChecksums = 0;
            do
            {
                i = msg.ReadInt();
                checksums[numChecksums++] = i;
                // just to make sure a broken client doesn't crash us
                if (numChecksums >= IVFileSystem.MAX_PURE_PAKS) { common.Warning($"MAX_PURE_PAKS ({IVFileSystem.MAX_PURE_PAKS}) exceeded in AsyncServer::ProcessPureMessage\n"); reply = "#str_07144"; return false; }
            } while (i != 0);
            numChecksums--;

            fileSystem.GetPureServerChecksums(serverChecksums);
            Debug.Assert(serverChecksums[0] != 0);

            for (i = 0; serverChecksums[i] != 0; i++)
            {
                if (checksums[i] != serverChecksums[i]) { common.DPrintf($"client {(from != null ? from.ToString() : $"{clientNum}")}: pak missing (0x{serverChecksums[i]:x})\n"); reply = $"pak missing (0x{serverChecksums[i]:x})\n"; return false; }
            }
            if (checksums[i] != 0) { common.DPrintf($"client {(from != null ? from.ToString() : $"{clientNum}")}: extra pak file referenced (0x{checksums[i]:X})\n"); reply = $"extra pak file referenced (0x{checksums[i]:x})\n"; return false; }
            reply = null;
            return true;
        }

        void ProcessPureMessage(Netadr from, BitMsg msg)
        {
            int iclient, challenge, clientId;

            challenge = msg.ReadInt();
            clientId = msg.ReadShort();

            if ((iclient = ValidateChallenge(from, challenge, clientId)) == -1) return;
            if (challenges[iclient].authState != AuthState.CDK_PUREWAIT) { common.DPrintf($"client {from}: got pure message, not in CDK_PUREWAIT\n"); return; }
            if (!VerifyChecksumMessage(iclient, from, msg, out var reply)) { PrintOOB(from, (int)SERVER_PRINT.MISC, reply); return; }

            common.DPrintf($"client {from}: passed pure checks\n");
            challenges[iclient].authState = AuthState.CDK_PUREOK; // next connect message will get the client through completely
        }

        void ProcessReliablePure(int clientNum, BitMsg msg)
        {
            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            int clientGameInitId;

            clientGameInitId = msg.ReadInt();
            if (clientGameInitId != gameInitId) { common.DPrintf($"client {clientNum}: ignoring reliable pure from an old gameInit ({clientGameInitId})\n"); return; }

            if (clients[clientNum].clientState != ServerClientState.SCS_PUREWAIT)
            {
                // should not happen unless something is very wrong. still, don't let this crash us, just get rid of the client
                common.DPrintf($"client {clientNum}: got reliable pure while != SCS_PUREWAIT, sending a reload\n");
                outMsg.InitW(msgBuf);
                outMsg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.RELOAD);
                SendReliableMessage(clientNum, msg);
                // go back to SCS_CONNECTED to sleep on the client until it goes away for a reconnect
                clients[clientNum].clientState = ServerClientState.SCS_CONNECTED;
                return;
            }

            if (!VerifyChecksumMessage(clientNum, null, msg, out var reply)) { DropClient(clientNum, reply); return; }
            common.DPrintf($"client {clientNum}: passed pure checks (reliable channel)\n");
            clients[clientNum].clientState = ServerClientState.SCS_CONNECTED;
        }

        public void RemoteConsoleOutput(string s)
        {
            noRconOutput = false;
            PrintOOB(rconAddress, (int)SERVER_PRINT.RCON, s);
        }

        static void RConRedirect(string s)
            => AsyncNetwork.server.RemoteConsoleOutput(s);

        void ProcessRemoteConsoleMessage(Netadr from, BitMsg msg)
        {
            BitMsg outMsg = new(); byte[] msgBuf = new byte[952];
            string s;

            if (string.IsNullOrEmpty(AsyncNetwork.serverRemoteConsolePassword.String)) { PrintOOB(from, (int)SERVER_PRINT.MISC, "#str_04846"); return; }

            msg.ReadString(out s);

            if (!string.Equals(s, AsyncNetwork.serverRemoteConsolePassword.String, StringComparison.OrdinalIgnoreCase)) { PrintOOB(from, (int)SERVER_PRINT.MISC, "#str_04847"); return; }

            msg.ReadString(out s);

            common.Printf($"rcon from {from}: {s}\n");

            rconAddress = from;
            noRconOutput = true;
            common.BeginRedirect(msgBuf, msgBuf.Length, RConRedirect);

            cmdSystem.BufferCommandText(CMD_EXEC.NOW, s);

            common.EndRedirect();

            if (noRconOutput) PrintOOB(rconAddress, (int)SERVER_PRINT.RCON, "#str_04848");
        }

        void ProcessGetInfoMessage(Netadr from, BitMsg msg)
        {
            if (!IsActive) return;

            common.DPrintf($"Sending info response to {from}\n");

            var challenge = msg.ReadInt();

            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            outMsg.InitW(msgBuf);
            outMsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            outMsg.WriteString("infoResponse");
            outMsg.WriteInt(challenge);
            outMsg.WriteInt(Config.ASYNC_PROTOCOL_VERSION);
            outMsg.WriteDeltaDict(sessLocal.mapSpawnData.serverInfo, null);

            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                var client = clients[i];
                if (client.clientState < ServerClientState.SCS_CONNECTED) continue;

                outMsg.WriteByte(i);
                outMsg.WriteShort(client.clientPing);
                outMsg.WriteInt(client.channel.MaxOutgoingRate);
                outMsg.WriteString(sessLocal.mapSpawnData.userInfo[i].GetString("ui_name", "Player"));
            }
            outMsg.WriteByte(Config.MAX_ASYNC_CLIENTS);
            // Stradex: Originally Doom3 did outMsg.WriteLong( fileSystem.GetOSMask() ); here
            //          dhewm3 eliminated GetOSMask() and WriteLong() became WriteInt() as it's supposed to write an int32
            //          Sending -1 (instead of nothing at all) restores compatibility with id's masterserver.
            outMsg.WriteInt(-1);

            serverPort.SendPacket(from, outMsg.DataW, outMsg.Size);
        }

        // see (client) "getInfo" . (server) "infoResponse" . (client)ProcessGetInfoMessage        
        public void PrintLocalServerInfo()
        {
            common.Printf($"server '{sessLocal.mapSpawnData.serverInfo.GetString("si_name")}' IP = {serverPort.Adr}\nprotocol {Config.ASYNC_PROTOCOL_MAJOR}.{Config.ASYNC_PROTOCOL_MINOR}\n");
            sessLocal.mapSpawnData.serverInfo.Print();
            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                var client = clients[i];
                if (client.clientState < ServerClientState.SCS_CONNECTED) continue;
                common.Printf($"client {i:2}: {sessLocal.mapSpawnData.userInfo[i].GetString("ui_name", "Player")}, ping = {client.clientPing}, rate = {client.channel.MaxOutgoingRate}\n");
            }
        }

        bool ConnectionlessMessage(Netadr from, BitMsg msg)
        {
            msg.ReadString(out var s, Platform.MAX_STRING_CHARS * 2); // M. Quinn - Even Balance - PB Packets need more than 1024

            // info request
            if (string.Equals(s, "getInfo", StringComparison.OrdinalIgnoreCase)) { ProcessGetInfoMessage(from, msg); return false; }

            // remote console
            if (string.Equals(s, "rcon", StringComparison.OrdinalIgnoreCase)) { ProcessRemoteConsoleMessage(from, msg); return true; }

            if (!active) { PrintOOB(from, (int)SERVER_PRINT.MISC, "#str_04849"); return false; }

            // challenge from a client
            if (string.Equals(s, "challenge", StringComparison.OrdinalIgnoreCase)) { ProcessChallengeMessage(from, msg); return false; }

            // connect from a client
            if (string.Equals(s, "connect", StringComparison.OrdinalIgnoreCase)) { ProcessConnectMessage(from, msg); return false; }

            // pure mesasge from a client
            if (string.Equals(s, "pureClient", StringComparison.OrdinalIgnoreCase)) { ProcessPureMessage(from, msg); return false; }

            // download request
            if (string.Equals(s, "downloadRequest", StringComparison.OrdinalIgnoreCase)) ProcessDownloadRequestMessage(from, msg);

            // auth server
            if (string.Equals(s, "auth", StringComparison.OrdinalIgnoreCase))
            {
                if (from != AsyncNetwork.MasterAddress) { common.Printf($"auth: bad source {from}\n"); return false; }
                if (AsyncNetwork.LANServer.Bool) common.Printf("auth message from master. net_LANServer is enabled, ignored.\n");
                ProcessAuthMessage(msg);
                return false;
            }

            return false;
        }

        bool ProcessMessage(Netadr from, BitMsg msg)
        {
            int id = msg.ReadShort();

            // check for a connectionless message
            if (id == CONNECTIONLESS_MESSAGE_ID) return ConnectionlessMessage(from, msg);

            if (msg.RemaingData < 4) { common.DPrintf($"{from}: tiny packet\n"); return false; }

            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            // find out which client the message is from
            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                var client = clients[i];
                if (client.clientState == ServerClientState.SCS_FREE) continue;

                // This does not compare the UDP port, because some address translating routers will change that at arbitrary times.
                if (from != client.channel.RemoteAddress || id != client.clientId) continue;

                // make sure it is a valid, in sequence packet
                if (!client.channel.Process(from, serverTime, msg, out var _)) return false;       // out of order, duplicated, fragment, etc.

                // zombie clients still need to do the channel processing to make sure they don't need to retransmit the final reliable message, but they don't do any other processing
                if (client.clientState == ServerClientState.SCS_ZOMBIE) return false;

                client.lastPacketTime = serverTime;

                ProcessReliableClientMessages(i);
                ProcessUnreliableClientMessage(i, msg);

                return false;
            }

            // if we received a sequenced packet from an address we don't recognize, send an out of band disconnect packet to it
            outMsg.InitW(msgBuf);
            outMsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            outMsg.WriteString("disconnect");
            serverPort.SendPacket(from, outMsg.DataW, outMsg.Size);

            return false;
        }

        public void SendReliableGameMessage(int clientNum, BitMsg msg)
        {
            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            outMsg.InitW(msgBuf);
            outMsg.WriteByte((int)SERVER_RELIABLE_MESSAGE.GAME);
            outMsg.WriteData(msg.DataW, 0, msg.Size);

            if (clientNum >= 0 && clientNum < Config.MAX_ASYNC_CLIENTS) { if (clients[clientNum].clientState == ServerClientState.SCS_INGAME) SendReliableMessage(clientNum, outMsg); return; }
            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                if (clients[i].clientState != ServerClientState.SCS_INGAME) continue;
                SendReliableMessage(i, outMsg);
            }
        }

        public void SendReliableGameMessageExcluding(int clientNum, BitMsg msg)
        {
            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            Debug.Assert(clientNum >= 0 && clientNum < Config.MAX_ASYNC_CLIENTS);

            outMsg.InitW(msgBuf);
            outMsg.WriteByte((int)SERVER_RELIABLE_MESSAGE.GAME);
            outMsg.WriteData(msg.DataW, 0, msg.Size);

            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                if (i == clientNum || clients[i].clientState != ServerClientState.SCS_INGAME) continue;
                SendReliableMessage(i, outMsg);
            }
        }

        public void LocalClientSendReliableMessage(BitMsg msg)
        {
            if (localClientNum < 0) { common.Printf("LocalClientSendReliableMessage: no local client\n"); return; }
            game.ServerProcessReliableMessage(localClientNum, msg);
        }

        public void ProcessConnectionLessMessages()
        {
            if (serverPort.Port == 0) return;

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            while (serverPort.GetPacket(out var from, msgBuf, out var size, msgBuf.Length))
            {
                msg.InitW(msgBuf);
                msg.Size = size;
                msg.BeginReading();
                var id = msg.ReadShort();
                if (id == CONNECTIONLESS_MESSAGE_ID) ConnectionlessMessage(from, msg);
            }
        }

        int UpdateTime(int clamp)
        {
            var time = SysW.Milliseconds;
            var msec = MathX.ClampInt(0, clamp, time - realTime);
            realTime = time;
            serverTime += msec;
            return msec;
        }

        public void RunFrame()
        {
            int i;
            bool newPacket;
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            int outgoingRate, incomingRate;
            float outgoingCompression, incomingCompression;

            var msec = UpdateTime(100);

            if (serverPort.Port == 0) return;

            if (!active) { ProcessConnectionLessMessages(); return; }

            gameTimeResidual += msec;

            // spin in place processing incoming packets until enough time lapsed to run a new game frame
            do
            {
                do
                {
                    // blocking read with game time residual timeout
                    newPacket = serverPort.GetPacketBlocking(out var from, msgBuf, out var size, msgBuf.Length, IUsercmd.USERCMD_MSEC - gameTimeResidual - 1);
                    if (newPacket)
                    {
                        msg.InitW(msgBuf);
                        msg.Size = size;
                        msg.BeginReading();
                        if (ProcessMessage(from, msg)) return; // return because rcon was used
                    }

                    msec = UpdateTime(100);
                    gameTimeResidual += msec;
                } while (newPacket);
            } while (gameTimeResidual < IUsercmd.USERCMD_MSEC);

            // send heart beat to master servers
            MasterHeartbeat();

            // check for clients that timed out
            CheckClientTimeouts();

            if (AsyncNetwork.idleServer.Bool == (NumClients == 0 || NumIdleClients != NumClients))
            {
                AsyncNetwork.idleServer.Bool = !AsyncNetwork.idleServer.Bool;
                // the need to propagate right away, only this
                sessLocal.mapSpawnData.serverInfo["si_idleServer"] = AsyncNetwork.idleServer.String;
                game.SetServerInfo(sessLocal.mapSpawnData.serverInfo);
            }

            // make sure the time doesn't wrap
            if (serverTime > 0x70000000) { ExecuteMapChange(); return; }

            // check for synchronized cvar changes
            if ((cvarSystem.GetModifiedFlags() & CVAR.NETWORKSYNC) != 0)
            {
                var newCvars = cvarSystem.MoveCVarsToDict(CVAR.NETWORKSYNC);
                SendSyncedCvarsBroadcast(newCvars);
                cvarSystem.ClearModifiedFlags(CVAR.NETWORKSYNC);
            }

            // check for user info changes of the local client
            if ((cvarSystem.GetModifiedFlags() & CVAR.USERINFO) != 0)
            {
                if (localClientNum >= 0)
                {
                    game.ThrottleUserInfo();
                    var newInfo = cvarSystem.MoveCVarsToDict(CVAR.USERINFO);
                    SendUserInfoBroadcast(localClientNum, newInfo);
                }
                cvarSystem.ClearModifiedFlags(CVAR.USERINFO);
            }

            // advance the server game
            while (gameTimeResidual >= IUsercmd.USERCMD_MSEC)
            {
                // sample input for the local client
                LocalClientInput();

                // duplicate usercmds for clients if no new ones are available
                DuplicateUsercmds(gameFrame, gameTime);

                // advance game
                var ret = game.RunFrame(userCmds[gameFrame & (Config.MAX_USERCMD_BACKUP - 1)]);

                AsyncNetwork.ExecuteSessionCommand(ret.sessionCommand);

                // update time
                gameFrame++;
                gameTime += IUsercmd.USERCMD_MSEC;
                gameTimeResidual -= IUsercmd.USERCMD_MSEC;
            }

            // duplicate usercmds so there is always at least one available to send with snapshots
            DuplicateUsercmds(gameFrame, gameTime);

            // send snapshots to connected clients
            for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                var client = clients[i];
                if (client.clientState == ServerClientState.SCS_FREE || i == localClientNum) continue;

                // modify maximum rate if necesary
                if (AsyncNetwork.serverMaxClientRate.IsModified) client.channel.MaxOutgoingRate = Math.Min(client.clientRate, AsyncNetwork.serverMaxClientRate.Integer);

                // if the channel is not yet ready to send new data
                if (!client.channel.ReadyToSend(serverTime)) continue;

                // send additional message fragments if the last message was too large to send at once
                if (client.channel.UnsentFragmentsLeft) { client.channel.SendNextFragment(serverPort, serverTime); continue; }

                if (client.clientState == ServerClientState.SCS_INGAME) { if (!SendSnapshotToClient(i)) SendPingToClient(i); }
                else SendEmptyToClient(i);
            }

            if (C.com_showAsyncStats.Bool)
            {
                UpdateAsyncStatsAvg();

                // dedicated will verbose to console
                if (AsyncNetwork.serverDedicated.Bool && serverTime >= nextAsyncStatsTime)
                {
                    common.Printf($"delay = {Delay} msec, total outgoing rate = {OutgoingRate >> 10} KB/s, total incoming rate = {IncomingRate >> 10} KB/s\n");

                    for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
                    {
                        outgoingRate = GetClientOutgoingRate(i);
                        incomingRate = GetClientIncomingRate(i);
                        outgoingCompression = GetClientOutgoingCompression(i);
                        incomingCompression = GetClientIncomingCompression(i);

                        if (outgoingRate != -1 && incomingRate != -1) common.Printf($"client {i}: out rate = {outgoingRate} B/s (% {outgoingCompression:-2.1}%), in rate = {incomingRate} B/s (% {incomingCompression:-2.1}%)\n");
                    }

                    GetAsyncStatsAvgMsg(out var msg2);
                    common.Printf($"{msg}\n");

                    nextAsyncStatsTime = serverTime + 1000;
                }
            }

            AsyncNetwork.serverMaxClientRate.ClearModified();
        }

        public void PacifierUpdate()
        {
            if (!IsActive) return;

            realTime = SysW.Milliseconds;
            ProcessConnectionLessMessages();
            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
                if (clients[i].clientState >= ServerClientState.SCS_PUREWAIT)
                {
                    if (clients[i].channel.UnsentFragmentsLeft) clients[i].channel.SendNextFragment(serverPort, serverTime);
                    else SendEmptyToClient(i);
                }
        }

        void PrintOOB(Netadr to, int opcode, string s)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            msg.WriteString("print");
            msg.WriteInt(opcode);
            msg.WriteString(s);
            serverPort.SendPacket(to, msg.DataW, msg.Size);
        }

        public void MasterHeartbeat(bool force = false)
        {
            if (AsyncNetwork.LANServer.Bool) { if (force) common.Printf("net_LANServer is enabled. Not sending heartbeats\n"); return; }
            if (force) nextHeartbeatTime = 0;
            // not yet
            if (serverTime < nextHeartbeatTime) return;

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            nextHeartbeatTime = serverTime + HEARTBEAT_MSEC;
            for (var i = 0; i < Config.MAX_MASTER_SERVERS; i++)
                if (AsyncNetwork.GetMasterAddress(i, out var adr))
                {
                    common.Printf($"Sending heartbeat to {adr}\n");
                    msg.InitW(msgBuf);
                    msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
                    msg.WriteString("heartbeat");
                    serverPort.SendPacket(adr, msg.DataW, msg.Size);
                }
        }

        void SendEnterGameToClient(int clientNum)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteByte((byte)SERVER_RELIABLE_MESSAGE.ENTERGAME);
            SendReliableMessage(clientNum, msg);
        }

        public void UpdateAsyncStatsAvg()
        {
            stats_average_sum -= stats_outrate[stats_current];
            stats_outrate[stats_current] = AsyncNetwork.server.OutgoingRate;
            if (stats_outrate[stats_current] > stats_max) { stats_max = stats_outrate[stats_current]; stats_max_index = stats_current; }
            else if (stats_current == stats_max_index)
            {
                // find the new max
                stats_max = 0;
                for (var i = 0; i < stats_numsamples; i++) if (stats_outrate[i] > stats_max) { stats_max = stats_outrate[i]; stats_max_index = i; }
            }
            stats_average_sum += stats_outrate[stats_current];
            stats_current++; stats_current %= stats_numsamples;
        }

        public void GetAsyncStatsAvgMsg(out string msg)
            => msg = $"avrg out: {stats_average_sum / stats_numsamples} B/s - max {stats_max} B/s ( over {AsyncNetwork.serverSnapshotDelay.Integer * stats_numsamples} ms )";

        void ProcessDownloadRequestMessage(Netadr from, BitMsg msg)
        {
            int iclient, numPaks, i;
            int[] dlSize = new int[IVFileSystem.MAX_PURE_PAKS];  // sizes
            List<string> pakNames = new(); // relative path
            List<string> pakURLs = new(); // game URLs

            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            BitMsg tmpMsg = new(); byte[] tmpBuf = new byte[MAX_MESSAGE_SIZE];
            int voidSlots = 0;              // to count and verbose the right number of paks requested for downloads

            int challenge = msg.ReadInt(),
                clientId = msg.ReadShort(),
                dlRequest = msg.ReadInt();

            if ((iclient = ValidateChallenge(from, challenge, clientId)) == -1) return;

            if (challenges[iclient].authState != AuthState.CDK_PUREWAIT) { common.DPrintf($"client {from}: got download request message, not in CDK_PUREWAIT\n"); return; }

            string pakbuf = null;
            pakNames.Add(pakbuf);
            numPaks = 1;

            // read the checksums, build path names and pass that to the game code
            var dlPakChecksum = msg.ReadInt();
            while (dlPakChecksum != 0)
            {
                if ((dlSize[numPaks] = fileSystem.ValidateDownloadPakForChecksum(dlPakChecksum, pakbuf)) == 0)
                {
                    // we pass an empty token to the game so our list doesn't get offset
                    common.Warning($"client requested an unknown pak 0x{dlPakChecksum:x}");
                    pakbuf = string.Empty;
                    voidSlots++;
                }
                pakNames.Add(pakbuf);
                numPaks++;
                dlPakChecksum = msg.ReadInt();
            }

            var paklist = string.Empty;
            for (i = 0; i < pakNames.Count; i++)
            {
                if (i > 0)
                    paklist += ";";
                paklist += pakNames[i];
            }

            // read the message and pass it to the game code
            common.DPrintf($"got download request for {numPaks - voidSlots} paks - {paklist}\n");

            outMsg.InitW(msgBuf);
            outMsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            outMsg.WriteString("downloadInfo");
            outMsg.WriteInt(dlRequest);
            if (!game.DownloadRequest(from.ToString(), challenges[iclient].guid, paklist, pakbuf))
            {
                common.DPrintf("game: no downloads\n");
                outMsg.WriteByte((byte)SERVER_DL.NONE);
                serverPort.SendPacket(from, outMsg.DataW, outMsg.Size);
                return;
            }

            SERVER_DL type = 0;

            foreach (var token in pakbuf.Split(';'))
                if (type == 0) type = (SERVER_DL)int.Parse(token);
                else if (type == SERVER_DL.REDIRECT)
                {
                    common.DPrintf($"download request: redirect to URL {token}\n");
                    outMsg.WriteByte((int)SERVER_DL.REDIRECT);
                    outMsg.WriteString(token);
                    serverPort.SendPacket(from, outMsg.DataW, outMsg.Size);
                    return;
                }
                else if (type == SERVER_DL.LIST) pakURLs.Add(token);
                else { common.DPrintf($"wrong op type {type}\n"); break; }

            if (type == SERVER_DL.LIST)
            {
                int totalDlSize = 0, numActualPaks = 0;

                // put the answer packet together
                outMsg.WriteByte((int)SERVER_DL.LIST);

                tmpMsg.InitW(tmpBuf);

                for (i = 0; i < pakURLs.Count; i++)
                {
                    tmpMsg.BeginWriting();
                    if (dlSize[i] == 0 || pakURLs[i].Length == 0)
                    {
                        // still send the relative path so the client knows what it missed
                        tmpMsg.WriteByte((byte)SERVER_PAK.NO);
                        tmpMsg.WriteString(pakNames[i]);
                    }
                    else
                    {
                        totalDlSize += dlSize[i];
                        numActualPaks++;
                        tmpMsg.WriteByte((byte)SERVER_PAK.YES);
                        tmpMsg.WriteString(pakNames[i]);
                        tmpMsg.WriteString(pakURLs[i]);
                        tmpMsg.WriteInt(dlSize[i]);
                    }

                    // keep last 5 bytes for an 'end of message' - SERVER_PAK_END and the totalDlSize long
                    if (outMsg.RemainingSpace - tmpMsg.Size > 5) outMsg.WriteData(tmpMsg.DataW, 0, tmpMsg.Size);
                    else { outMsg.WriteByte((byte)SERVER_PAK.END); break; }
                }
                // put a closure even if size not exceeded
                if (i == pakURLs.Count) outMsg.WriteByte((byte)SERVER_PAK.END);
                common.DPrintf($"download request: download {numActualPaks} paks, {totalDlSize} bytes\n");

                serverPort.SendPacket(from, outMsg.DataW, outMsg.Size);
            }
        }
    }
}