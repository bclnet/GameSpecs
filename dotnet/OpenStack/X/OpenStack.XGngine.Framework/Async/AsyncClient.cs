using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.NumericsX.OpenStack.Gngine.UI;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Framework.Async.MsgChannel;
using static System.NumericsX.OpenStack.Gngine.Framework.Framework;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework.Async
{
    public enum ClientState
    {
        CS_DISCONNECTED,
        CS_PURERESTART,
        CS_CHALLENGING,
        CS_CONNECTING,
        CS_CONNECTED,
        CS_INGAME
    }

    public enum AuthKeyMsg : byte
    {
        AUTHKEY_BADKEY,
        AUTHKEY_GUID
    }

    public enum AuthBadKeyStatus : byte
    {
        AUTHKEY_BAD_INVALID,
        AUTHKEY_BAD_BANNED,
        AUTHKEY_BAD_INUSE,
        AUTHKEY_BAD_MSG
    }

    public enum ClientUpdateState
    {
        UPDATE_NONE,
        UPDATE_SENT,
        UPDATE_READY,
        UPDATE_DLING,
        UPDATE_DONE
    }

    public struct PakDlEntry
    {
        public string url;
        public string filename;
        public int size;
        public int checksum;
    }

    public class AsyncClient
    {
        const int SETUP_CONNECTION_RESEND_TIME = 1000;
        const int EMPTY_RESEND_TIME = 500;
        const int PREDICTION_FAST_ADJUST = 4;

        public ServerScan serverList;

        bool active;                        // true if client is active
        int realTime;                   // absolute time

        int clientTime;                 // client local time
        NetPort clientPort;                    // UDP port
        int clientId;                   // client identification
        int clientDataChecksum;         // checksum of the data used by the client
        int clientNum;                  // client number on server
        ClientState clientState;                // client state
        int clientPrediction;           // how far the client predicts ahead
        int clientPredictTime;          // prediction time used to send user commands

        Netadr serverAddress;               // IP address of server
        int serverId;                   // server identification
        int serverChallenge;            // challenge from server
        int serverMessageSequence;      // sequence number of last server message

        Netadr lastRconAddress;         // last rcon address we emitted to
        int lastRconTime;               // when last rcon emitted

        MsgChannel channel;                 // message channel to server
        int lastConnectTime;            // last time a connect message was sent
        int lastEmptyTime;              // last time an empty message was sent
        int lastPacketTime;             // last time a packet was received from the server
        int lastSnapshotTime;           // last time a snapshot was received

        int snapshotSequence;           // sequence number of the last received snapshot
        int snapshotGameFrame;          // game frame number of the last received snapshot
        int snapshotGameTime;           // game time of the last received snapshot

        int gameInitId;                 // game initialization identification
        int gameFrame;                  // local game frame
        int gameTime;                   // local game time
        int gameTimeResidual;           // left over time from previous frame

        Usercmd[][] userCmds = Enumerable.Repeat(new Usercmd[Config.MAX_USERCMD_BACKUP], Config.MAX_ASYNC_CLIENTS).ToArray();

        IUserInterface guiNetMenu;

        ClientUpdateState updateState;
        int updateSentTime;
        string updateMSG;
        string updateURL;
        bool updateDirectDownload;
        string updateFile;
        DL_FILE updateMime;
        string updateFallback;
        bool showUpdateMessage;

        BackgroundDownload backgroundDownload;
        int dltotal;
        int dlnow;

        int lastFrameDelta;

        int dlRequest;      // randomized number to keep track of the requests
        int[] dlChecksums = new int[IVFileSystem.MAX_PURE_PAKS]; // 0-terminated, first element is the game pak checksum or 0
        int dlCount;        // total number of paks we request download for ( including the game pak )
        List<PakDlEntry> dlList = new();            // list of paks to download, with url and name
        int currentDlSize;
        int totalDlSize;    // for partial progress stuff

        public AsyncClient()
        {
            guiNetMenu = null;
            updateState = ClientUpdateState.UPDATE_NONE;
            Clear();
        }

        void Clear()
        {
            active = false;
            realTime = 0;
            clientTime = 0;
            clientId = 0;
            clientDataChecksum = 0;
            clientNum = 0;
            clientState = ClientState.CS_DISCONNECTED;
            clientPrediction = 0;
            clientPredictTime = 0;
            serverId = 0;
            serverChallenge = 0;
            serverMessageSequence = 0;
            lastConnectTime = -9999;
            lastEmptyTime = -9999;
            lastPacketTime = -9999;
            lastSnapshotTime = -9999;
            snapshotGameFrame = 0;
            snapshotGameTime = 0;
            snapshotSequence = 0;
            gameInitId = Config.GAME_INIT_ID_INVALID;
            gameFrame = 0;
            gameTimeResidual = 0;
            gameTime = 0;
            Array.Clear(userCmds, 0, userCmds.Length);
            backgroundDownload.completed = true;
            lastRconTime = 0;
            showUpdateMessage = false;
            lastFrameDelta = 0;

            dlRequest = -1;
            dlCount = -1;
            Array.Clear(dlChecksums, 0, IVFileSystem.MAX_PURE_PAKS);
            currentDlSize = 0;
            totalDlSize = 0;
        }

        public void Shutdown()
        {
            guiNetMenu = null;
            updateMSG = null;
            updateURL = null;
            updateFile = null;
            updateFallback = null;
            backgroundDownload.url.url = null;
            dlList.Clear();
        }

        public bool InitPort()
        {
            // if this is the first time we connect to a server, open the UDP port
            if (clientPort.Port == 0 && !clientPort.InitForPort(NetPort.PORT_ANY)) { common.Printf("Couldn't open client network port.\n"); return false; }
            // maintain it valid between connects and ui manager reloads
            guiNetMenu = uiManager.FindGui("guis/netmenu.gui", true, false, true);
            return true;
        }

        public void ClosePort()
            => clientPort.Close();


        void ClearPendingPackets()
        {
            var msgBuf = new byte[MAX_MESSAGE_SIZE];
            while (clientPort.GetPacket(out _, msgBuf, out var _, msgBuf.Length)) { }
        }

        string HandleGuiCommandInternal(string cmd)
        {
            if (cmd == "abort" || cmd == "pure_abort") { common.DPrintf("connection aborted\n"); cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect"); return ""; }
            else common.DWarning($"AsyncClient::HandleGuiCommand: unknown cmd {cmd}");
            return null;
        }

        static string HandleGuiCommand(string cmd)
            => AsyncNetwork.client.HandleGuiCommandInternal(cmd);

        public void ConnectToServer(Netadr adr)
        {
            // shutdown any current game. that includes network disconnect
            session.Stop();

            if (!InitPort()) return;

            if (cvarSystem.GetCVarBool("net_serverDedicated")) { common.Printf("Can't connect to a server as dedicated\n"); return; }

            // trash any currently pending packets
            ClearPendingPackets();

            serverAddress = adr;

            // clear the client state
            Clear();

            // get a pseudo random client id, but don't use the id which is reserved for connectionless packets
            clientId = (int)SysW.Milliseconds & CONNECTIONLESS_MESSAGE_ID_MASK;

            // calculate a checksum on some of the essential data used
            clientDataChecksum = declManager.GetChecksum();

            // start challenging the server
            clientState = ClientState.CS_CHALLENGING;

            active = true;

            guiNetMenu = uiManager.FindGui("guis/netmenu.gui", true, false, true);
            guiNetMenu.SetStateString("status", $"{common.LanguageDictGetString("#str_06749")}{adr}");
            session.SetGUI(guiNetMenu, HandleGuiCommand);
        }

        public void ConnectToServer(string address)
        {
            Netadr adr;
            if (stringX.IsNumeric(address))
            {
                var serverNum = int.Parse(address);
                if (serverNum < 0 || serverNum >= serverList.Count) { session.MessageBox(MSG.OK, $"{common.LanguageDictGetString("#str_06733")}{serverNum}", common.LanguageDictGetString("#str_06735"), true); return; }
                adr = serverList[serverNum].adr;
            }
            else
            {
                if (!Netadr.TryParse(address, out adr, true)) { session.MessageBox(MSG.OK, $"{common.LanguageDictGetString("#str_06734")}{address}", common.LanguageDictGetString("#str_06735"), true); return; }
            }
            if (adr.port == 0) adr.port = Config.PORT_SERVER;

            common.Printf($"\"{address}\" resolved to {adr}\n");

            ConnectToServer(adr);
        }

        public void Reconnect()
            => ConnectToServer(serverAddress);

        public void DisconnectFromServer()
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            if (clientState >= ClientState.CS_CONNECTED)
            {
                // if we were actually connected, clear the pure list
                fileSystem.ClearPureChecksums();

                // send reliable disconnect to server
                msg.InitW(msgBuf);
                msg.WriteByte((byte)CLIENT_RELIABLE_MESSAGE.DISCONNECT);
                msg.WriteString("disconnect");

                if (!channel.SendReliableMessage(msg)) common.Error("client.server reliable messages overflow\n");

                SendEmptyToServer(true);
                SendEmptyToServer(true);
                SendEmptyToServer(true);
            }

            if (clientState != ClientState.CS_PURERESTART) { channel.Shutdown(); clientState = ClientState.CS_DISCONNECTED; }

            active = false;
        }

        public void GetServerInfo(Netadr adr)
        {
            if (!InitPort()) return;

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            msg.WriteString("getInfo");
            msg.WriteInt(serverList.GetChallenge());    // challenge

            clientPort.SendPacket(adr, msg.DataW, msg.Size);
        }

        public void GetServerInfo(string address)
        {
            Netadr adr;

            if (!string.IsNullOrEmpty(address))
            {
                if (!Netadr.TryParse(address, out adr, true)) { common.Printf($"Couldn't get server address for \"{address}\"\n"); return; }
            }
            else if (active) adr = serverAddress;
            // used to be a Sys_StringToNetAdr( "localhost", &adr, true ); and send a packet over loopback but this breaks with net_ip ( typically, for multi-homed servers )
            else if (AsyncNetwork.server.IsActive) { AsyncNetwork.server.PrintLocalServerInfo(); return; }
            else { common.Printf("no server found\n"); return; }

            if (adr.port == 0) adr.port = Config.PORT_SERVER;

            GetServerInfo(adr);
        }

        public void GetLANServers()
        {
            if (!InitPort()) return;

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            int i; Netadr broadcastAddress = new();

            AsyncNetwork.LANServer.Bool = true;

            serverList.SetupLANScan();

            msg.InitW(msgBuf);
            msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            msg.WriteString("getInfo");
            msg.WriteInt(serverList.GetChallenge());

            broadcastAddress.type = NA.BROADCAST;
            for (i = 0; i < Config.MAX_SERVER_PORTS; i++)
            {
                broadcastAddress.port = (ushort)(Config.PORT_SERVER + i);
                clientPort.SendPacket(broadcastAddress, msg.DataW, msg.Size);
            }
        }

        public void GetNETServers()
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            AsyncNetwork.LANServer.Bool = false;

            // NetScan only clears GUI and results, not the stored list
            serverList.Clear();
            serverList.NetScan();
            serverList.StartServers(true);

            msg.InitW(msgBuf);
            msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            msg.WriteString("getServers");
            msg.WriteInt(Config.ASYNC_PROTOCOL_VERSION);
            msg.WriteString(cvarSystem.GetCVarString("fs_game"));
            msg.WriteBits(cvarSystem.GetCVarInteger("gui_filter_password"), 2);
            msg.WriteBits(cvarSystem.GetCVarInteger("gui_filter_players"), 2);
            msg.WriteBits(cvarSystem.GetCVarInteger("gui_filter_gameType"), 2);

            if (AsyncNetwork.GetMasterAddress(0, out var adr)) clientPort.SendPacket(adr, msg.DataW, msg.Size);
        }

        public void ListServers()
        {
            for (var i = 0; i < serverList.Count; i++) common.Printf($"{i:3}: {serverList[i].serverInfo["si_name"]} {serverList[i].ping}ms ({serverList[i].adr})\n");
        }

        public void ClearServers()
            => serverList.Clear();

        public void RemoteConsole(string command)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            if (!InitPort()) return;

            Netadr adr;
            if (active) adr = serverAddress;
            else Netadr.TryParse(AsyncNetwork.clientRemoteConsoleAddress.String, out adr, true);

            if (adr.port == 0)
                adr.port = Config.PORT_SERVER;

            lastRconAddress = adr;
            lastRconTime = realTime;

            msg.InitW(msgBuf);
            msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            msg.WriteString("rcon");
            msg.WriteString(AsyncNetwork.clientRemoteConsolePassword.String);
            msg.WriteString(command);

            clientPort.SendPacket(adr, msg.DataW, msg.Size);
        }

        public bool IsPortInitialized
            => clientPort.Port != 0;

        public bool IsActive
            => active;

        public int LocalClientNum
            => clientNum;

        public int PredictedFrames
            => lastFrameDelta;

        public int Prediction
            => clientState < ClientState.CS_CONNECTED ? -1 : clientPrediction;

        public int TimeSinceLastPacket
            => clientState < ClientState.CS_CONNECTED ? -1 : clientTime - lastPacketTime;

        public int OutgoingRate
            => clientState < ClientState.CS_CONNECTED ? -1 : channel.OutgoingRate;

        public int IncomingRate
            => clientState < ClientState.CS_CONNECTED ? -1 : channel.IncomingRate;

        public float OutgoingCompression
            => clientState < ClientState.CS_CONNECTED ? 0f : channel.OutgoingCompression;

        public float GetIncomingCompression
            => clientState < ClientState.CS_CONNECTED ? 0f : channel.IncomingCompression;

        public float IncomingPacketLoss
            => clientState < ClientState.CS_CONNECTED ? 0f : channel.IncomingPacketLoss;

        void DuplicateUsercmds(int frame, int time)
        {
            var previousIndex = (frame - 1) & (Config.MAX_USERCMD_BACKUP - 1);
            var currentIndex = frame & (Config.MAX_USERCMD_BACKUP - 1);

            // duplicate previous user commands if no new commands are available for a client
            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) AsyncNetwork.DuplicateUsercmd(userCmds[previousIndex][i], userCmds[currentIndex][i], frame, time);
        }

        void SendUserInfoToServer()
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            if (clientState < ClientState.CS_CONNECTED) return;

            var info = cvarSystem.MoveCVarsToDict(CVAR.USERINFO);

            // send reliable client info to server
            msg.InitW(msgBuf);
            msg.WriteByte((int)CLIENT_RELIABLE_MESSAGE.CLIENTINFO);
            msg.WriteDeltaDict(info, sessLocal.mapSpawnData.userInfo[clientNum]);

            if (!channel.SendReliableMessage(msg)) common.Error("client.server reliable messages overflow\n");

            sessLocal.mapSpawnData.userInfo[clientNum] = info;
        }

        void SendEmptyToServer(bool force = false, bool mapLoad = false)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            if (lastEmptyTime > realTime) lastEmptyTime = realTime;

            if (!force && (realTime - lastEmptyTime < EMPTY_RESEND_TIME)) return;

            if (AsyncNetwork.verbose.Integer != 0) common.Printf($"sending empty to server, gameInitId = {(mapLoad ? Config.GAME_INIT_ID_MAP_LOAD : gameInitId)}\n");

            msg.InitW(msgBuf);
            msg.WriteInt(serverMessageSequence);
            msg.WriteInt(mapLoad ? Config.GAME_INIT_ID_MAP_LOAD : gameInitId);
            msg.WriteInt(snapshotSequence);
            msg.WriteByte((byte)CLIENT_UNRELIABLE_MESSAGE.EMPTY);

            channel.SendMessage(clientPort, clientTime, msg);

            while (channel.UnsentFragmentsLeft) channel.SendNextFragment(clientPort, clientTime);

            lastEmptyTime = realTime;
        }

        void SendPingResponseToServer(int time)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            if (AsyncNetwork.verbose.Integer == 2) common.Printf($"sending ping response to server, gameInitId = {gameInitId}\n");

            msg.InitW(msgBuf);
            msg.WriteInt(serverMessageSequence);
            msg.WriteInt(gameInitId);
            msg.WriteInt(snapshotSequence);
            msg.WriteByte((byte)CLIENT_UNRELIABLE_MESSAGE.PINGRESPONSE);
            msg.WriteInt(time);

            channel.SendMessage(clientPort, clientTime, msg);
            while (channel.UnsentFragmentsLeft) channel.SendNextFragment(clientPort, clientTime);
        }

        void SendUsercmdsToServer()
        {
            int i, numUsercmds, index; Usercmd? last;
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            if (AsyncNetwork.verbose.Integer == 2) common.Printf($"sending usercmd to server: gameInitId = {gameInitId}, gameFrame = {gameFrame}, gameTime = {gameTime}\n");

            // generate user command for this client
            index = gameFrame & (Config.MAX_USERCMD_BACKUP - 1);
            userCmds[index][clientNum] = usercmdGen.GetDirectUsercmd();
            userCmds[index][clientNum].gameFrame = gameFrame;
            userCmds[index][clientNum].gameTime = gameTime;

            // send the user commands to the server
            msg.InitW(msgBuf);
            msg.WriteInt(serverMessageSequence);
            msg.WriteInt(gameInitId);
            msg.WriteInt(snapshotSequence);
            msg.WriteByte((byte)CLIENT_UNRELIABLE_MESSAGE.USERCMD);
            msg.WriteShort(clientPrediction);

            numUsercmds = MathX.ClampInt(0, 10, AsyncNetwork.clientUsercmdBackup.Integer) + 1;

            // write the user commands
            msg.WriteInt(gameFrame);
            msg.WriteByte(numUsercmds);
            for (last = null, i = gameFrame - numUsercmds + 1; i <= gameFrame; i++)
            {
                index = i & (Config.MAX_USERCMD_BACKUP - 1);
                AsyncNetwork.WriteUserCmdDelta(msg, userCmds[index][clientNum], last);
                last = userCmds[index][clientNum];
            }

            channel.SendMessage(clientPort, clientTime, msg);
            while (channel.UnsentFragmentsLeft) channel.SendNextFragment(clientPort, clientTime);
        }

        void InitGame(int serverGameInitId, int serverGameFrame, int serverGameTime, Dictionary<string, string> serverSI)
        {
            gameInitId = serverGameInitId;
            gameFrame = snapshotGameFrame = serverGameFrame;
            gameTime = snapshotGameTime = serverGameTime;
            gameTimeResidual = 0;
            Array.Clear(userCmds, 0, userCmds.Length);

            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++) sessLocal.mapSpawnData.userInfo[i].Clear();

            sessLocal.mapSpawnData.serverInfo = serverSI;
        }

        void ProcessUnreliableServerMessage(BitMsg msg)
        {
            int i, j, index, numDuplicatedUsercmds, aheadOfServer, numUsercmds, delta, serverGameInitId, serverGameFrame, serverGameTime; Usercmd? last;

            serverGameInitId = msg.ReadInt();

            var id = (SERVER_UNRELIABLE_MESSAGE)msg.ReadByte();
            switch (id)
            {
                case SERVER_UNRELIABLE_MESSAGE.EMPTY:
                    if (AsyncNetwork.verbose.Integer != 0) common.Printf("received empty message from server\n");
                    break;
                case SERVER_UNRELIABLE_MESSAGE.PING:
                    if (AsyncNetwork.verbose.Integer == 2) common.Printf("received ping message from server\n");
                    SendPingResponseToServer(msg.ReadInt());
                    break;
                case SERVER_UNRELIABLE_MESSAGE.GAMEINIT:
                    Dictionary<string, string> serverSI = new();

                    serverGameFrame = msg.ReadInt();
                    serverGameTime = msg.ReadInt();
                    msg.ReadDeltaDict(serverSI, null);
                    var pureWait = serverSI.Get("si_pure", "0") != "0";

                    InitGame(serverGameInitId, serverGameFrame, serverGameTime, serverSI);

                    channel.ResetRate();

                    if (AsyncNetwork.verbose.Integer != 0) common.Printf($"received gameinit, gameInitId = {gameInitId}, gameFrame = {gameFrame}, gameTime = {gameTime}\n");

                    // mute sound
                    soundSystem.SetMute(true);

                    // ensure chat icon goes away when the GUI is changed...
                    //cvarSystem.SetCVarBool("ui_chat", false);

                    if (pureWait)
                    {
                        guiNetMenu = uiManager.FindGui("guis/netmenu.gui", true, false, true);
                        session.SetGUI(guiNetMenu, HandleGuiCommand);
                        session.MessageBox(MSG.ABORT, common.LanguageDictGetString("#str_04317"), common.LanguageDictGetString("#str_04318"), false, "pure_abort");
                    }
                    // load map
                    else { session.SetGUI(null, null); sessLocal.ExecuteMapChange(); }

                    break;
                case SERVER_UNRELIABLE_MESSAGE.SNAPSHOT:
                    // if the snapshot is from a different game
                    if (serverGameInitId != gameInitId) { if (AsyncNetwork.verbose.Integer != 0) common.Printf("ignoring snapshot with != gameInitId\n"); break; }

                    snapshotSequence = msg.ReadInt();
                    snapshotGameFrame = msg.ReadInt();
                    snapshotGameTime = msg.ReadInt();
                    numDuplicatedUsercmds = msg.ReadByte();
                    aheadOfServer = msg.ReadShort();

                    // read the game snapshot
                    game.ClientReadSnapshot(clientNum, snapshotSequence, snapshotGameFrame, snapshotGameTime, numDuplicatedUsercmds, aheadOfServer, msg);

                    // read user commands of other clients from the snapshot
                    for (last = null, i = msg.ReadByte(); i < Config.MAX_ASYNC_CLIENTS; i = msg.ReadByte())
                    {
                        numUsercmds = msg.ReadByte();
                        if (numUsercmds > Config.MAX_USERCMD_RELAY) { common.Error($"snapshot {snapshotSequence} contains too many user commands for client {i}"); break; }
                        for (j = 0; j < numUsercmds; j++)
                        {
                            index = (snapshotGameFrame + j) & (Config.MAX_USERCMD_BACKUP - 1);
                            AsyncNetwork.ReadUserCmdDelta(msg, ref userCmds[index][i], last);
                            userCmds[index][i].gameFrame = snapshotGameFrame + j;
                            userCmds[index][i].duplicateCount = 0;
                            last = userCmds[index][i];
                        }
                        // clear all user commands after the ones just read from the snapshot
                        for (j = numUsercmds; j < Config.MAX_USERCMD_BACKUP; j++)
                        {
                            index = (snapshotGameFrame + j) & (Config.MAX_USERCMD_BACKUP - 1);
                            userCmds[index][i].gameFrame = 0;
                            userCmds[index][i].gameTime = 0;
                        }
                    }

                    // if this is the first snapshot after a game init was received
                    if (clientState == ClientState.CS_CONNECTED)
                    {
                        gameTimeResidual = 0;
                        clientState = ClientState.CS_INGAME;
                        Debug.Assert(sessLocal.ActiveMenu == null);
                        if (AsyncNetwork.verbose.Integer != 0) common.Printf($"received first snapshot, gameInitId = {gameInitId}, gameFrame {snapshotGameFrame} gameTime {snapshotGameTime}\n");
                    }

                    // if the snapshot is newer than the clients current game time
                    if (gameTime < snapshotGameTime || gameTime > snapshotGameTime + AsyncNetwork.clientMaxPrediction.Integer)
                    {
                        gameFrame = snapshotGameFrame;
                        gameTime = snapshotGameTime;
                        gameTimeResidual = MathX.ClampInt(-AsyncNetwork.clientMaxPrediction.Integer, AsyncNetwork.clientMaxPrediction.Integer, gameTimeResidual);
                        clientPredictTime = MathX.ClampInt(-AsyncNetwork.clientMaxPrediction.Integer, AsyncNetwork.clientMaxPrediction.Integer, clientPredictTime);
                    }

                    // adjust the client prediction time based on the snapshot time
                    clientPrediction -= (1 - (MathX.INTSIGNBITSET_(aheadOfServer - AsyncNetwork.clientPrediction.Integer) << 1));
                    clientPrediction = MathX.ClampInt(AsyncNetwork.clientPrediction.Integer, AsyncNetwork.clientMaxPrediction.Integer, clientPrediction);
                    delta = gameTime - (snapshotGameTime + clientPrediction);
                    clientPredictTime -= (delta / PREDICTION_FAST_ADJUST) + (1 - (MathX.INTSIGNBITSET_(delta) << 1));

                    lastSnapshotTime = clientTime;

                    if (AsyncNetwork.verbose.Integer == 2) common.Printf($"received snapshot, gameInitId = {gameInitId}, gameFrame = {gameFrame}, gameTime = {gameTime}\n");

                    if (numDuplicatedUsercmds != 0 && (AsyncNetwork.verbose.Integer == 2)) common.Printf($"server duplicated {numDuplicatedUsercmds} user commands before snapshot {snapshotGameFrame}\n");
                    break;
                default:
                    common.Printf($"unknown unreliable server message {id}\n");
                    break;
            }
        }

        void ProcessReliableMessagePure(BitMsg msg)
        {
            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            int[] inChecksums = new int[IVFileSystem.MAX_PURE_PAKS];
            int i, serverGameInitId;

            session.SetGUI(null, null);

            serverGameInitId = msg.ReadInt();

            if (serverGameInitId != gameInitId) { common.DPrintf($"ignoring pure server checksum from an outdated gameInitId ({serverGameInitId})\n"); return; }

            if (!ValidatePureServerChecksums(serverAddress, msg)) return;

            if (AsyncNetwork.verbose.Integer != 0) common.Printf("received new pure server info. ExecuteMapChange and report back\n");

            // it is now ok to load the next map with updated pure checksums
            sessLocal.ExecuteMapChange(true);

            // upon receiving our pure list, the server will send us SCS_INGAME and we'll start getting snapshots
            fileSystem.GetPureServerChecksums(inChecksums);
            outMsg.InitW(msgBuf);
            outMsg.WriteByte((byte)CLIENT_RELIABLE_MESSAGE.PURE);

            outMsg.WriteInt(gameInitId);

            i = 0;
            while (inChecksums[i] != 0) outMsg.WriteInt(inChecksums[i++]);
            outMsg.WriteInt(0);

            if (!channel.SendReliableMessage(outMsg)) common.Error("client.server reliable messages overflow\n");
        }

        void ReadLocalizedServerString(BitMsg msg, out string o, int maxLen = Platform.MAX_STRING_CHARS)
        {
            msg.ReadString(out o, maxLen);
            // look up localized string. if the message is not an #str_ format, we'll just get it back unchanged
            o = common.LanguageDictGetString(o);
        }

        void ProcessReliableServerMessages()
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            SERVER_RELIABLE_MESSAGE id;

            msg.InitW(msgBuf);

            while (channel.GetReliableMessage(msg))
            {
                id = (SERVER_RELIABLE_MESSAGE)msg.ReadByte();
                switch (id)
                {
                    case SERVER_RELIABLE_MESSAGE.CLIENTINFO:
                        {
                            var clientNum = msg.ReadByte();
                            var info = sessLocal.mapSpawnData.userInfo[clientNum];
                            var haveBase = msg.ReadBits(1) != 0;

#if ID_CLIENTINFO_TAGS
                            int checksum = info.Checksum();
                            int srv_checksum = msg.ReadInt();
                            if (checksum != srv_checksum) { common.DPrintf($"SERVER_RELIABLE_MESSAGE_CLIENTINFO {clientNum} (haveBase: {(haveBase ? "true" : "false")}): != checksums srv: 0x{checksum:x} local: 0x{srv_checksum:x}\n"); info.Print(); }
                            else common.DPrintf($"SERVER_RELIABLE_MESSAGE_CLIENTINFO {clientNum} (haveBase: {(haveBase ? "true" : "false")}): checksums ok 0x{checksum:x}\n");
#endif

                            msg.ReadDeltaDict(info, haveBase ? info : null);

                            // server forces us to a different userinfo
                            if (clientNum == this.clientNum)
                            {
                                common.DPrintf("local user info modified by server\n");
                                cvarSystem.SetCVarsFromDict(info);
                                cvarSystem.ClearModifiedFlags(CVAR.USERINFO); // don't emit back
                            }
                            game.SetUserInfo(clientNum, info, true, false);
                            break;
                        }
                    case SERVER_RELIABLE_MESSAGE.SYNCEDCVARS:
                        {
                            var info = sessLocal.mapSpawnData.syncedCVars;
                            msg.ReadDeltaDict(info, info);
                            cvarSystem.SetCVarsFromDict(info);
                            if (!AsyncNetwork.allowCheats.Bool) cvarSystem.ResetFlaggedVariables(CVAR.CHEAT);
                            break;
                        }
                    case SERVER_RELIABLE_MESSAGE.PRINT:
                        {
                            msg.ReadString(out var s);
                            common.Printf($"{s}\n");
                            break;
                        }
                    case SERVER_RELIABLE_MESSAGE.DISCONNECT:
                        {
                            int clientNum; string s;

                            clientNum = msg.ReadInt();
                            ReadLocalizedServerString(msg, out s);
                            if (clientNum == this.clientNum)
                            {
                                session.Stop();
                                session.MessageBox(MSG.OK, s, common.LanguageDictGetString("#str_04319"), true);
                                session.StartMenu();
                            }
                            else
                            {
                                common.Printf("client {clientNum} {s}\n");
                                cmdSystem.BufferCommandText(CMD_EXEC.NOW, $"addChatLine \"{sessLocal.mapSpawnData.userInfo[clientNum]["ui_name"]}^0 {s}\"");
                                sessLocal.mapSpawnData.userInfo[clientNum].Clear();
                            }
                            break;
                        }
                    case SERVER_RELIABLE_MESSAGE.APPLYSNAPSHOT:
                        {
                            int sequence;
                            sequence = msg.ReadInt();
                            if (!game.ClientApplySnapshot(clientNum, sequence)) { session.Stop(); common.Error($"couldn't apply snapshot {sequence}"); }
                            break;
                        }
                    case SERVER_RELIABLE_MESSAGE.PURE:
                        {
                            ProcessReliableMessagePure(msg);
                            break;
                        }
                    case SERVER_RELIABLE_MESSAGE.RELOAD:
                        {
                            if (AsyncNetwork.verbose.Bool) common.Printf("got MESSAGE_RELOAD from server\n");
                            // simply reconnect, so that if the server restarts in pure mode we can get the right list and avoid spurious reloads
                            cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "reconnect\n");
                            break;
                        }
                    case SERVER_RELIABLE_MESSAGE.ENTERGAME:
                        {
                            SendUserInfoToServer();
                            game.SetUserInfo(clientNum, sessLocal.mapSpawnData.userInfo[clientNum], true, false);
                            cvarSystem.ClearModifiedFlags(CVAR.USERINFO);
                            break;
                        }
                    default:
                        {
                            // pass reliable message on to game code
                            game.ClientProcessReliableMessage(clientNum, msg);
                            break;
                        }
                }
            }
        }

        void ProcessChallengeResponseMessage(Netadr from, BitMsg msg)
        {
            string serverGame, serverGameBase;

            if (clientState != ClientState.CS_CHALLENGING) { common.Printf("Unwanted challenge response received.\n"); return; }

            serverChallenge = msg.ReadInt();
            serverId = msg.ReadShort();
            msg.ReadString(out serverGameBase);
            msg.ReadString(out serverGame);

            // the server is running a different game... we need to reload in the correct fs_game even pure pak checks would fail if we didn't, as there are files we may not even see atm
            // NOTE: we could read the pure list from the server at the same time and set it up for the restart (if the client can restart directly with the right pak order, then we avoid an extra reloadEngine later..)
            if (string.Equals(cvarSystem.GetCVarString("fs_game_base"), serverGameBase, StringComparison.OrdinalIgnoreCase) || string.Equals(cvarSystem.GetCVarString("fs_game"), serverGame, StringComparison.OrdinalIgnoreCase))
            {
                // bug #189 - if the server is running ROE and ROE is not locally installed, refuse to connect or we might crash
                if (!fileSystem.HasD3XP && (string.Equals(serverGameBase, "d3xp", StringComparison.OrdinalIgnoreCase) || string.Equals(serverGame, "d3xp", StringComparison.OrdinalIgnoreCase)))
                {
                    common.Printf("The server is running Doom3: Resurrection of Evil expansion pack. RoE is not installed on this client. Aborting the connection..\n");
                    cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "disconnect\n");
                    return;
                }
                common.Printf($"The server is running a different mod ({serverGameBase}-{serverGame}). Restarting..\n");
                cvarSystem.SetCVarString("fs_game_base", serverGameBase);
                cvarSystem.SetCVarString("fs_game", serverGame);
                cmdSystem.BufferCommandText(CMD_EXEC.NOW, "reloadEngine");
                cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "reconnect\n");
                return;
            }

            common.Printf($"received challenge response 0x{serverChallenge:x} from {from}\n");

            // start sending connect packets instead of challenge request packets
            clientState = ClientState.CS_CONNECTING;
            lastConnectTime = -9999;

            // take this address as the new server address.  This allows
            // a server proxy to hand off connections to multiple servers
            serverAddress = from;
        }

        void ProcessConnectResponseMessage(Netadr from, BitMsg msg)
        {
            int serverGameInitId, serverGameFrame, serverGameTime;
            Dictionary<string, string> serverSI = new();

            if (clientState >= ClientState.CS_CONNECTED) { common.Printf("Duplicate connect received.\n"); return; }
            if (clientState != ClientState.CS_CONNECTING) { common.Printf("Connect response packet while not connecting.\n"); return; }
            if (from != serverAddress) { common.Printf("Connect response from a different server.\n"); common.Printf($"{from} should have been {serverAddress}\n"); return; }

            common.Printf($"received connect response from {from}\n");

            channel.Init(from, clientId);
            clientNum = msg.ReadInt();
            clientState = ClientState.CS_CONNECTED;
            lastPacketTime = -9999;

            serverGameInitId = msg.ReadInt();
            serverGameFrame = msg.ReadInt();
            serverGameTime = msg.ReadInt();
            msg.ReadDeltaDict(serverSI, null);

            InitGame(serverGameInitId, serverGameFrame, serverGameTime, serverSI);

            // load map
            session.SetGUI(null, null);
            sessLocal.ExecuteMapChange();

            clientPredictTime = clientPrediction = MathX.ClampInt(0, AsyncNetwork.clientMaxPrediction.Integer, clientTime - lastConnectTime);
        }

        void ProcessDisconnectMessage(Netadr from, BitMsg msg)
        {
            if (clientState == ClientState.CS_DISCONNECTED) { common.Printf("Disconnect packet while not connected.\n"); return; }
            if (from != serverAddress) { common.Printf("Disconnect packet from unknown server.\n"); return; }
            session.Stop();
            session.MessageBox(MSG.OK, common.LanguageDictGetString("#str_04320"), null, true);
            session.StartMenu();
        }

        void ProcessInfoResponseMessage(Netadr from, BitMsg msg)
        {
            int i, protocol, index;

            var verbose = from.type == NA.LOOPBACK || cvarSystem.GetCVarBool("developer");

            var serverInfo = new NetworkServer
            {
                clients = 0,
                adr = from,
                challenge = msg.ReadInt() // challenge
            };
            protocol = msg.ReadInt();
            if (protocol != Config.ASYNC_PROTOCOL_VERSION) { common.Printf($"server {serverInfo.adr} ignored - protocol {protocol >> 16}.{protocol & 0xffff}, expected {Config.ASYNC_PROTOCOL_MAJOR}.{Config.ASYNC_PROTOCOL_MINOR}\n"); return; }
            msg.ReadDeltaDict(serverInfo.serverInfo, null);

            if (verbose) { common.Printf($"server IP = {serverInfo.adr}\n"); serverInfo.serverInfo.Print(); }
            for (i = msg.ReadByte(); i < Config.MAX_ASYNC_CLIENTS; i = msg.ReadByte())
            {
                serverInfo.pings[serverInfo.clients] = msg.ReadShort();
                serverInfo.rate[serverInfo.clients] = msg.ReadInt();
                msg.ReadString(out serverInfo.nickname[serverInfo.clients], Config.MAX_NICKLEN);
                if (verbose) common.Printf($"client {i:2}: {serverInfo.nickname[serverInfo.clients]}, ping = {serverInfo.pings[serverInfo.clients]}, rate = {serverInfo.rate[serverInfo.clients]}\n");
                serverInfo.clients++;
            }
            index = serverList.InfoResponse(serverInfo) ? 1 : 0;

            common.Printf($"{index}: server {serverInfo.adr} - protocol {protocol >> 16}.{protocol & 0xffff} - {serverInfo.serverInfo["si_name"]}\n");
        }

        void ProcessPrintMessage(Netadr from, BitMsg msg)
        {
            var opcode = (SERVER_PRINT)msg.ReadInt();
            var game_opcode = opcode == SERVER_PRINT.GAMEDENY
                ? (AllowReply)msg.ReadInt()
                : AllowReply.ALLOW_YES;
            ReadLocalizedServerString(msg, out var s, Platform.MAX_STRING_CHARS);
            common.Printf("{s}\n");
            guiNetMenu.SetStateString("status", s);
            if (opcode == SERVER_PRINT.GAMEDENY)
            {
                if (game_opcode == AllowReply.ALLOW_BADPASS)
                {
                    var retpass = session.MessageBox(MSG.PROMPT, common.LanguageDictGetString("#str_04321"), s, true, "passprompt_ok");
                    ClearPendingPackets();
                    guiNetMenu.SetStateString("status", common.LanguageDictGetString("#str_04322"));
                    if (retpass != null) { cvarSystem.SetCVarString("password", ""); cvarSystem.SetCVarString("password", retpass); }
                    else cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                }
                else if (game_opcode == AllowReply.ALLOW_NO)
                {
                    session.MessageBox(MSG.OK, s, common.LanguageDictGetString("#str_04323"), true);
                    ClearPendingPackets();
                    cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                }
                // ALLOW_NOTYET just keeps running as usual. The GUI has an abort button
            }
            else if (opcode == SERVER_PRINT.BADCHALLENGE && clientState >= ClientState.CS_CONNECTING)
                cmdSystem.BufferCommandText(CMD_EXEC.NOW, "reconnect");
        }

        void ProcessServersListMessage(Netadr from, BitMsg msg)
        {
            if (AsyncNetwork.MasterAddress != from) { common.DPrintf($"received a server list from {from} - not a valid master\n"); return; }
            while (msg.RemaingData != 0)
            {
                int a = msg.ReadByte(), b = msg.ReadByte(), c = msg.ReadByte(), d = msg.ReadByte();
                serverList.AddServer(serverList.Count, $"{a}.{b}.{c}.{d}:{msg.ReadShort()}");
            }
        }

        void ProcessAuthKeyMessage(Netadr from, BitMsg msg)
        {
            string auth_msg = null;

            if (clientState != ClientState.CS_CONNECTING && !session.WaitingForGameAuth) { common.Printf("clientState != CS_CONNECTING, not waiting for game auth, authKey ignored\n"); return; }

            var authMsg = (AuthKeyMsg)msg.ReadByte();
            if (authMsg == AuthKeyMsg.AUTHKEY_BADKEY)
            {
                var valid = new[] { true, true };
                var key_index = 0;
                var authBadStatus = (AuthBadKeyStatus)msg.ReadByte();
                switch (authBadStatus)
                {
                    case AuthBadKeyStatus.AUTHKEY_BAD_INVALID:
                        valid[0] = msg.ReadByte() == 1;
                        valid[1] = msg.ReadByte() == 1;
                        AsyncNetwork.BuildInvalidKeyMsg(out auth_msg, valid);
                        break;
                    case AuthBadKeyStatus.AUTHKEY_BAD_BANNED:
                        key_index = msg.ReadByte();
                        auth_msg = common.LanguageDictGetString($"#str_0719{6 + key_index:1}");
                        auth_msg += "\n";
                        auth_msg += common.LanguageDictGetString("#str_04304");
                        valid[key_index] = false;
                        break;
                    case AuthBadKeyStatus.AUTHKEY_BAD_INUSE:
                        key_index = msg.ReadByte();
                        auth_msg = common.LanguageDictGetString($"#str_0719{8 + key_index:1}");
                        auth_msg += "\n";
                        auth_msg += common.LanguageDictGetString("#str_04304");
                        valid[key_index] = false;
                        break;
                    case AuthBadKeyStatus.AUTHKEY_BAD_MSG:
                        // a general message explaining why this key is denied. no specific use for this atm. let's not clear the keys either
                        msg.ReadString(out auth_msg);
                        break;
                }
                common.DPrintf($"auth deny: {auth_msg}\n");

                // keys to be cleared. applies to both net connect and game auth
                session.ClearCDKey(valid);

                // get rid of the bad key - at least that's gonna annoy people who stole a fake key
                if (clientState == ClientState.CS_CONNECTING)
                    while (true)
                    {
                        // here we use the auth status message
                        var retkey = session.MessageBox(MSG.CDKEY, auth_msg, common.LanguageDictGetString("#str_04325"), true);
                        if (retkey != null)
                        {
                            if (session.CheckKey(retkey, true, valid)) cmdSystem.BufferCommandText(CMD_EXEC.NOW, "reconnect");
                            // build a more precise message about the offline check failure
                            else { AsyncNetwork.BuildInvalidKeyMsg(out auth_msg, valid); session.MessageBox(MSG.OK, auth_msg, common.LanguageDictGetString("#str_04327"), true); continue; }
                        }
                        else cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                        break;
                    }
                else
                    // forward the auth status information to the session code
                    session.CDKeysAuthReply(false, auth_msg);
            }
            else
            {
                msg.ReadString(out auth_msg);
                cvarSystem.SetCVarString("com_guid", auth_msg);
                common.Printf($"guid set to {auth_msg}\n");
                session.CDKeysAuthReply(true, null);
            }
        }

        void ProcessVersionMessage(Netadr from, BitMsg msg)
        {
            if (updateState != ClientUpdateState.UPDATE_SENT) { common.Printf("ProcessVersionMessage: version reply, != UPDATE_SENT\n"); return; }

            common.Printf("A new version is available\n");
            msg.ReadString(out updateMSG);
            updateDirectDownload = msg.ReadByte() != 0;
            msg.ReadString(out updateURL);
            updateMime = (DL_FILE)msg.ReadByte();
            msg.ReadString(out updateFallback);
            updateState = ClientUpdateState.UPDATE_READY;
        }

        bool ValidatePureServerChecksums(Netadr from, BitMsg msg)
        {
            int i, numChecksums, numMissingChecksums;
            int[] inChecksums = new int[IVFileSystem.MAX_PURE_PAKS];
            int[] missingChecksums = new int[IVFileSystem.MAX_PURE_PAKS];

            // read checksums
            // pak checksums, in a 0-terminated list
            numChecksums = 0;
            do
            {
                i = msg.ReadInt();
                inChecksums[numChecksums++] = i;
                // just to make sure a broken message doesn't crash us
                if (numChecksums >= IVFileSystem.MAX_PURE_PAKS) { common.Warning($"MAX_PURE_PAKS ({IVFileSystem.MAX_PURE_PAKS}) exceeded in AsyncClient.ProcessPureMessage\n"); return false; }
            } while (i != 0);
            inChecksums[numChecksums] = 0;

            var reply = fileSystem.SetPureServerChecksums(inChecksums, missingChecksums);
            switch (reply)
            {
                case PURE.RESTART:
                    // need to restart the filesystem with a different pure configuration
                    cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                    // restart with the right FS configuration and get back to the server
                    clientState = ClientState.CS_PURERESTART;
                    fileSystem.SetRestartChecksums(inChecksums);
                    cmdSystem.BufferCommandText(CMD_EXEC.NOW, "reloadEngine");
                    return false;
                case PURE.MISSING:
                    {
                        var checksums = string.Empty;
                        i = 0;
                        while (missingChecksums[i] != 0) checksums += $"0x{missingChecksums[i++]:x} ";
                        numMissingChecksums = i;

                        if (AsyncNetwork.clientDownload.Integer == 0)
                        {
                            // never any downloads
                            var message = $"{common.LanguageDictGetString("#str_07210")}{from}";

                            if (numMissingChecksums > 0) message += $"{common.LanguageDictGetString("#str_06751")}{numMissingChecksums}{checksums}";

                            common.Printf(message);
                            cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                            session.MessageBox(MSG.OK, message, common.LanguageDictGetString("#str_06735"), true);
                        }
                        else
                        {
                            // we are already connected, reconnect to negociate the paks in connectionless mode
                            if (clientState >= ClientState.CS_CONNECTED) { cmdSystem.BufferCommandText(CMD_EXEC.NOW, "reconnect"); return false; }
                            // ask the server to send back download info
                            common.DPrintf($"missing {numMissingChecksums} paks: {checksums}\n");
                            // store the requested downloads
                            GetDownloadRequest(missingChecksums, numMissingChecksums);
                            // build the download request message. NOTE: in a specific function?
                            BitMsg dlmsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
                            dlmsg.InitW(msgBuf);
                            dlmsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
                            dlmsg.WriteString("downloadRequest");
                            dlmsg.WriteInt(serverChallenge);
                            dlmsg.WriteShort(clientId);
                            // used to make sure the server replies to the same download request
                            dlmsg.WriteInt(dlRequest);
                            // special case the code pak - if we have a 0 checksum then we don't need to download it. 0-terminated list of missing paks
                            i = 0;
                            while (missingChecksums[i] != 0) dlmsg.WriteInt(missingChecksums[i++]);
                            dlmsg.WriteInt(0);
                            clientPort.SendPacket(from, dlmsg.DataW, dlmsg.Size);
                        }
                        return false;
                    }
                default:
                    break;
            }
            return true;
        }

        void ProcessPureMessage(Netadr from, BitMsg msg)
        {
            if (clientState != ClientState.CS_CONNECTING) { common.Printf("clientState != CS_CONNECTING, pure msg ignored\n"); return; }

            if (!ValidatePureServerChecksums(from, msg)) return;

            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            int i; int[] inChecksums = new int[IVFileSystem.MAX_PURE_PAKS];

            fileSystem.GetPureServerChecksums(inChecksums);
            outMsg.InitW(msgBuf);
            outMsg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            outMsg.WriteString("pureClient");
            outMsg.WriteInt(serverChallenge);
            outMsg.WriteShort(clientId);
            i = 0;
            while (inChecksums[i] != 0) outMsg.WriteInt(inChecksums[i++]);
            outMsg.WriteInt(0);

            clientPort.SendPacket(from, outMsg.DataW, outMsg.Size);
        }

        void ConnectionlessMessage(Netadr from, BitMsg msg)
        {
            msg.ReadString(out var s, Platform.MAX_STRING_CHARS * 2);

            // info response from a server, are accepted from any source
            if (string.Equals(s, "infoResponse", StringComparison.OrdinalIgnoreCase)) { ProcessInfoResponseMessage(from, msg); return; }

            // from master server:
            if (from == AsyncNetwork.MasterAddress)
            {
                // server list
                if (string.Equals(s, "servers", StringComparison.OrdinalIgnoreCase)) { ProcessServersListMessage(from, msg); return; }
                if (string.Equals(s, "authKey", StringComparison.OrdinalIgnoreCase)) { ProcessAuthKeyMessage(from, msg); return; }
                if (string.Equals(s, "newVersion", StringComparison.OrdinalIgnoreCase)) { ProcessVersionMessage(from, msg); return; }
            }

            // ignore if not from the current/last server
            if (from != serverAddress && (lastRconTime + 10000 < realTime || from != lastRconAddress)) { common.DPrintf($"got message '{s}' from bad source: {from}\n"); return; }

            // challenge response from the server we are connecting to
            if (string.Equals(s, "challengeResponse", StringComparison.OrdinalIgnoreCase)) { ProcessChallengeResponseMessage(from, msg); return; }

            // connect response from the server we are connecting to
            if (string.Equals(s, "connectResponse", StringComparison.OrdinalIgnoreCase)) { ProcessConnectResponseMessage(from, msg); return; }

            // a disconnect message from the server, which will happen if the server dropped the connection but is still getting packets from this client
            if (string.Equals(s, "disconnect", StringComparison.OrdinalIgnoreCase)) { ProcessDisconnectMessage(from, msg); return; }

            // print request from server
            if (string.Equals(s, "print", StringComparison.OrdinalIgnoreCase)) { ProcessPrintMessage(from, msg); return; }

            // server pure list
            if (string.Equals(s, "pureServer", StringComparison.OrdinalIgnoreCase)) { ProcessPureMessage(from, msg); return; }

            if (string.Equals(s, "downloadInfo", StringComparison.OrdinalIgnoreCase)) { ProcessDownloadInfoMessage(from, msg); }

            if (string.Equals(s, "authrequired", StringComparison.OrdinalIgnoreCase))
            {
                // server telling us that he's expecting an auth mode connect, just in case we're trying to connect in LAN mode
                if (AsyncNetwork.LANServer.Bool) { common.Warning($"server {from} requests master authorization for this client. Turning off LAN mode\n"); AsyncNetwork.LANServer.Bool = false; }
            }

            common.DPrintf($"ignored message from {from}: {s}\n");
        }

        void ProcessMessage(Netadr from, BitMsg msg)
        {
            var id = msg.ReadShort();

            // check for a connectionless packet
            if (id == CONNECTIONLESS_MESSAGE_ID) { ConnectionlessMessage(from, msg); return; }

            if (clientState < ClientState.CS_CONNECTED) return;     // can't be a valid sequenced packet

            if (msg.RemaingData < 4) { common.DPrintf($"{from}: tiny packet\n"); return; }

            // is this a packet from the server
            if (from != channel.RemoteAddress || id != serverId) { common.DPrintf($"{from}: sequenced server packet without connection\n"); return; }

            if (!channel.Process(from, clientTime, msg, out serverMessageSequence)) return;     // out of order, duplicated, fragment, etc.

            lastPacketTime = clientTime;
            ProcessReliableServerMessages();
            ProcessUnreliableServerMessage(msg);
        }

        void SetupConnection()
        {
            if (clientTime - lastConnectTime < SETUP_CONNECTION_RESEND_TIME) return;

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            if (clientState == ClientState.CS_CHALLENGING)
            {
                common.Printf($"sending challenge to {serverAddress}\n");
                msg.InitW(msgBuf);
                msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
                msg.WriteString("challenge");
                msg.WriteInt(clientId);
                clientPort.SendPacket(serverAddress, msg.DataW, msg.Size);
            }
            else if (clientState == ClientState.CS_CONNECTING)
            {
                common.Printf($"sending connect to {serverAddress} with challenge 0x{serverChallenge:x}\n");
                msg.InitW(msgBuf);
                msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
                msg.WriteString("connect");
                msg.WriteInt(Config.ASYNC_PROTOCOL_VERSION);
                msg.WriteInt(clientDataChecksum);
                msg.WriteInt(serverChallenge);
                msg.WriteShort(clientId);
                msg.WriteInt(cvarSystem.GetCVarInteger("net_clientMaxRate"));
                msg.WriteString(cvarSystem.GetCVarString("com_guid"));
                msg.WriteString(cvarSystem.GetCVarString("password"), -1, false);
                // do not make the protocol depend on PB
                msg.WriteShort(0);
                clientPort.SendPacket(serverAddress, msg.DataW, msg.Size);
#if ID_ENFORCE_KEY_CLIENT
                if (AsyncNetwork.LANServer.Bool) common.Printf("net_LANServer is set, connecting in LAN mode\n");
                else
                {
                    // emit a cd key authorization request modified at protocol 1.37 for XP key addition
                    msg.BeginWriting();
                    msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
                    msg.WriteString("clAuth");
                    msg.WriteInt(Config.ASYNC_PROTOCOL_VERSION);
                    msg.WriteNetadr(serverAddress);
                    // if we don't have a com_guid, this will request a direct reply from auth with it
                    msg.WriteByte(cvarSystem.GetCVarString("com_guid")[0] != 0 ? 1 : 0);
                    // send the main key, and flag an extra byte to add XP key
                    msg.WriteString(session.GetCDKey(false));
                    var xpkey = session.GetCDKey(true);
                    msg.WriteByte(xpkey != null ? 1 : 0);
                    if (xpkey != null) msg.WriteString(xpkey);
                    clientPort.SendPacket(AsyncNetwork.MasterAddress, msg.DataW, msg.Size);
                }
#else
                if (!serverAddress.IsLANAddress) common.Printf($"Build Does not have CD Key Enforcement enabled. The Server ({serverAddress}) is not within the lan addresses. Attemting to connect.\n");
                common.Printf("Not Testing key.\n");
#endif
            }
            else
                return;

            lastConnectTime = clientTime;
        }

        public void SendReliableGameMessage(BitMsg msg)
        {
            if (clientState < ClientState.CS_INGAME) return;

            BitMsg outMsg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];
            outMsg.InitW(msgBuf);
            outMsg.WriteByte((byte)CLIENT_RELIABLE_MESSAGE.GAME);
            outMsg.WriteData(msg.DataW, 0, msg.Size);
            if (!channel.SendReliableMessage(outMsg)) common.Error("client.server reliable messages overflow\n");
        }

        void Idle()
        {
            // also need to read mouse for the connecting guis
            usercmdGen.GetDirectUsercmd();

            SendEmptyToServer();
        }

        int UpdateTime(int clamp)
        {
            var time = (int)SysW.Milliseconds;
            var msec = MathX.ClampInt(0, clamp, time - realTime);
            realTime = time;
            clientTime += msec;
            return msec;
        }

        public void RunFrame()
        {
            int msec, size; bool newPacket; Netadr from;
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msec = UpdateTime(100);

            if (clientPort.Port == 0) return;

            // handle ongoing pk4 downloads and patch downloads
            HandleDownloads();

            gameTimeResidual += msec;

            // spin in place processing incoming packets until enough time lapsed to run a new game frame
            do
            {
                do
                {
                    // blocking read with game time residual timeout
                    newPacket = clientPort.GetPacketBlocking(out from, msgBuf, out size, msgBuf.Length, IUsercmd.USERCMD_MSEC - (gameTimeResidual + clientPredictTime) - 1);
                    if (newPacket)
                    {
                        msg.InitW(msgBuf);
                        msg.Size = size;
                        msg.BeginReading();
                        ProcessMessage(from, msg);
                    }

                    msec = UpdateTime(100);
                    gameTimeResidual += msec;

                } while (newPacket);

            } while (gameTimeResidual + clientPredictTime < IUsercmd.USERCMD_MSEC);

            // update server list
            serverList.RunFrame();

            if (clientState == ClientState.CS_DISCONNECTED) { usercmdGen.GetDirectUsercmd(); gameTimeResidual = IUsercmd.USERCMD_MSEC - 1; clientPredictTime = 0; return; }
            if (clientState == ClientState.CS_PURERESTART) { clientState = ClientState.CS_DISCONNECTED; Reconnect(); gameTimeResidual = IUsercmd.USERCMD_MSEC - 1; clientPredictTime = 0; return; }

            // if not connected setup a connection
            if (clientState < ClientState.CS_CONNECTED) { usercmdGen.GetDirectUsercmd(); SetupConnection(); gameTimeResidual = IUsercmd.USERCMD_MSEC - 1; clientPredictTime = 0; return; } // also need to read mouse for the connecting guis

            if (CheckTimeout()) return;

            // if not yet in the game send empty messages to keep data flowing through the channel
            if (clientState < ClientState.CS_INGAME) { Idle(); gameTimeResidual = 0; return; }

            // check for user info changes
            if ((cvarSystem.GetModifiedFlags() & CVAR.USERINFO) != 0) { game.ThrottleUserInfo(); SendUserInfoToServer(); game.SetUserInfo(clientNum, sessLocal.mapSpawnData.userInfo[clientNum], true, false); cvarSystem.ClearModifiedFlags(CVAR.USERINFO); }

            if (gameTimeResidual + clientPredictTime >= IUsercmd.USERCMD_MSEC) lastFrameDelta = 0;

            // generate user commands for the predicted time
            while (gameTimeResidual + clientPredictTime >= IUsercmd.USERCMD_MSEC)
            {
                // send the user commands of this client to the server
                SendUsercmdsToServer();

                // update time
                gameFrame++;
                gameTime += IUsercmd.USERCMD_MSEC;
                gameTimeResidual -= IUsercmd.USERCMD_MSEC;

                // run from the snapshot up to the local game frame
                while (snapshotGameFrame < gameFrame)
                {
                    lastFrameDelta++;

                    // duplicate usercmds for clients if no new ones are available
                    DuplicateUsercmds(snapshotGameFrame, snapshotGameTime);

                    // indicate the last prediction frame before a render
                    bool lastPredictFrame = (snapshotGameFrame + 1 >= gameFrame && gameTimeResidual + clientPredictTime < IUsercmd.USERCMD_MSEC);

                    // run client prediction
                    var ret = game.ClientPrediction(clientNum, userCmds[snapshotGameFrame & (Config.MAX_USERCMD_BACKUP - 1)], lastPredictFrame);

                    AsyncNetwork.ExecuteSessionCommand(ret.sessionCommand);

                    snapshotGameFrame++;
                    snapshotGameTime += IUsercmd.USERCMD_MSEC;
                }
            }
        }

        public void PacifierUpdate()
        {
            if (!IsActive) return;
            realTime = SysW.Milliseconds;
            SendEmptyToServer(false, true);
        }

        public void SendVersionCheck(bool fromMenu = false)
        {
            if (updateState != ClientUpdateState.UPDATE_NONE && !fromMenu) { common.DPrintf("up-to-date check was already performed\n"); return; }

            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            InitPort();
            msg.InitW(msgBuf);
            msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            msg.WriteString("versionCheck");
            msg.WriteInt(Config.ASYNC_PROTOCOL_VERSION);
            msg.WriteString(cvarSystem.GetCVarString("si_version"));
            msg.WriteString(cvarSystem.GetCVarString("com_guid"));
            clientPort.SendPacket(AsyncNetwork.MasterAddress, msg.DataW, msg.Size);

            common.DPrintf("sent a version check request\n");

            updateState = ClientUpdateState.UPDATE_SENT;
            updateSentTime = clientTime;
            showUpdateMessage = fromMenu;
        }

        /// <summary>
        /// Sends the version dl update.
        /// sending those packets is not strictly necessary.just a way to tell the update server about what is going on.allows the update server to have a more precise view of the overall
        /// network load for the updates
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        void SendVersionDLUpdate(int state)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            msg.WriteString("versionDL");
            msg.WriteInt(Config.ASYNC_PROTOCOL_VERSION);
            msg.WriteShort(state);
            clientPort.SendPacket(AsyncNetwork.MasterAddress, msg.DataW, msg.Size);
        }

        void HandleDownloads()
        {
            if (updateState == ClientUpdateState.UPDATE_SENT && clientTime > updateSentTime + 2000)
            {
                // timing out on no reply
                updateState = ClientUpdateState.UPDATE_DONE;
                if (showUpdateMessage) { session.MessageBox(MSG.OK, common.LanguageDictGetString("#str_04839"), common.LanguageDictGetString("#str_04837"), true); showUpdateMessage = false; }
                common.DPrintf("No update available\n");
            }
            else if (backgroundDownload.completed)
            {
                // only enter these if the download slot is free
                if (updateState == ClientUpdateState.UPDATE_READY)
                {
                    if (session.MessageBox(MSG.YESNO, updateMSG, common.LanguageDictGetString("#str_04330"), true, "yes")[0] != 0)
                    {
                        if (!updateDirectDownload) { system.OpenURL(updateURL, true); updateState = ClientUpdateState.UPDATE_DONE; }
                        else
                        {
                            // we're just creating the file at toplevel inside fs_savepath                            
                            var f = (VFile_Permanent)(fileSystem.OpenFileWrite(Path.GetFileName(updateFile)));
                            dltotal = 0;
                            dlnow = 0;

                            backgroundDownload.completed = false;
                            backgroundDownload.opcode = DLTYPE.URL;
                            backgroundDownload.f = f;
                            backgroundDownload.url.status = DL.WAIT;
                            backgroundDownload.url.dlnow = 0;
                            backgroundDownload.url.dltotal = 0;
                            backgroundDownload.url.url = updateURL;
                            fileSystem.BackgroundDownload(backgroundDownload);

                            updateState = ClientUpdateState.UPDATE_DLING;
                            SendVersionDLUpdate(0);
                            session.DownloadProgressBox(backgroundDownload, $"Downloading {updateFile}\n");
                            updateState = ClientUpdateState.UPDATE_DONE;
                            if (backgroundDownload.url.status == DL.DONE)
                            {
                                SendVersionDLUpdate(1);
                                var fullPath = f.FullPath;
                                fileSystem.CloseFile(f);
                                if (session.MessageBox(MSG.YESNO, common.LanguageDictGetString("#str_04331"), common.LanguageDictGetString("#str_04332"), true, "yes")[0] != 0)
                                {
                                    if (updateMime == DL_FILE.EXEC) system.StartProcess(fullPath, true);
                                    else system.OpenURL($"file://{fullPath}", true);
                                }
                                else session.MessageBox(MSG.OK, $"{common.LanguageDictGetString("#str_04333")}{fullPath}", common.LanguageDictGetString("#str_04334"), true);
                            }
                            else
                            {
                                if (backgroundDownload.url.dlerror[0] != 0) common.Warning($"update download failed. curl error: {backgroundDownload.url.dlerror}");
                                SendVersionDLUpdate(2);
                                var name = f.Name;
                                fileSystem.CloseFile(f);
                                fileSystem.RemoveFile(name);
                                session.MessageBox(MSG.OK, common.LanguageDictGetString("#str_04335"), common.LanguageDictGetString("#str_04336"), true);
                                if (updateFallback.Length != 0) system.OpenURL(updateFallback, true);
                                else common.Printf("no fallback URL\n");
                            }
                        }
                    }
                    else updateState = ClientUpdateState.UPDATE_DONE;
                }
                else if (dlList.Count != 0)
                {
                    int numPaks = dlList.Count, pakCount = 1, progress_start, progress_end;
                    currentDlSize = 0;

                    do
                    {
                        // ignore empty files
                        if (dlList[0].url[0] == '\0') { dlList.RemoveAt(0); continue; }
                        common.Printf($"start download for {dlList[0].url}\n");

                        var f = (VFile_Permanent)fileSystem.MakeTemporaryFile();
                        if (f == null) { common.Warning("could not create temporary file"); dlList.Clear(); return; }

                        backgroundDownload.completed = false;
                        backgroundDownload.opcode = DLTYPE.URL;
                        backgroundDownload.f = f;
                        backgroundDownload.url.status = DL.WAIT;
                        backgroundDownload.url.dlnow = 0;
                        backgroundDownload.url.dltotal = dlList[0].size;
                        backgroundDownload.url.url = dlList[0].url;
                        fileSystem.BackgroundDownload(backgroundDownload);
                        // "Downloading %s"
                        var dltitle = string.Format(common.LanguageDictGetString("#str_07213"), dlList[0].filename);
                        if (numPaks > 1) dltitle += $" ({pakCount}/{numPaks})";
                        if (totalDlSize != 0) { progress_start = (int)((float)currentDlSize * 100.0f / (float)totalDlSize); progress_end = (int)((float)(currentDlSize + dlList[0].size) * 100.0f / (float)totalDlSize); }
                        else { progress_start = 0; progress_end = 100; }
                        session.DownloadProgressBox(backgroundDownload, dltitle, progress_start, progress_end);
                        if (backgroundDownload.url.status == DL.DONE)
                        {
                            const int CHUNK_SIZE = 1024 * 1024;

                            common.Printf("file downloaded\n");
                            var finalPath = cvarSystem.GetCVarString("fs_savepath");
                            finalPath = Path.Combine(finalPath, dlList[0].filename);
                            fileSystem.CreateOSPath(finalPath);
                            // do the final copy ourselves so we do by small chunks in case the file is big
                            var saveas = fileSystem.OpenExplicitFileWrite(finalPath);
                            var buf = new byte[CHUNK_SIZE];
                            f.Seek(0, FS_SEEK.END);
                            var remainlen = f.Tell;
                            f.Seek(0, FS_SEEK.SET);
                            while (remainlen != 0)
                            {
                                var readlen = Math.Min(remainlen, CHUNK_SIZE);
                                var retlen = f.Read(buf, readlen);
                                if (retlen != readlen) common.FatalError($"short read {retlen} of {readlen} in FileSystem::HandleDownload");
                                retlen = saveas.Write(buf, readlen);
                                if (retlen != readlen) common.FatalError($"short write {retlen} of {readlen} in FileSystem::HandleDownload");
                                remainlen -= readlen;
                            }
                            fileSystem.CloseFile(f);
                            fileSystem.CloseFile(saveas);
                            common.Printf($"saved as {finalPath}\n");
                            buf = null;

                            // add that file to our paks list
                            var checksum = fileSystem.AddZipFile(dlList[0].filename);

                            // verify the checksum to be what the server says
                            if (checksum == 0 || checksum != dlList[0].checksum)
                            {
                                // "pak is corrupted ( checksum 0x%x, expected 0x%x )"
                                session.MessageBox(MSG.OK, $"{common.LanguageDictGetString("#str_07214")}{checksum}{dlList[0].checksum}", "Download failed", true);
                                fileSystem.RemoveFile(dlList[0].filename);
                                dlList.Clear();
                                return;
                            }

                            currentDlSize += dlList[0].size;
                        }
                        else
                        {
                            common.Warning($"download failed: {dlList[0].url}");
                            if (backgroundDownload.url.dlerror[0] != 0) common.Warning($"curl error: {backgroundDownload.url.dlerror}");
                            // "The download failed or was cancelled". "Download failed"
                            session.MessageBox(MSG.OK, common.LanguageDictGetString("#str_07215"), common.LanguageDictGetString("#str_07216"), true);
                            dlList.Clear();
                            return;
                        }

                        pakCount++;
                        dlList.RemoveAt(0);
                    } while (dlList.Count != 0);

                    // all downloads successful - do the dew
                    cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "reconnect\n");
                }
            }
        }

        // pass NULL for the keys you don't care to auth for
        // returns false if internet link doesn't appear to be available
        public bool SendAuthCheck(string cdkey, string xpkey)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_MESSAGE_SIZE];

            msg.InitW(msgBuf);
            msg.WriteShort(CONNECTIONLESS_MESSAGE_ID);
            msg.WriteString("gameAuth");
            msg.WriteInt(Config.ASYNC_PROTOCOL_VERSION);
            msg.WriteByte(cdkey != null ? 1 : 0);
            msg.WriteString(cdkey != null ? cdkey : string.Empty);
            msg.WriteByte(xpkey != null ? 1 : 0);
            msg.WriteString(xpkey != null ? xpkey : string.Empty);
            InitPort();
            clientPort.SendPacket(AsyncNetwork.MasterAddress, msg.DataW, msg.Size);
            return true;
        }

        bool CheckTimeout()
        {
            if (lastPacketTime > 0 && (lastPacketTime + AsyncNetwork.clientServerTimeout.Integer * 1000 < clientTime))
            {
                session.StopBox();
                session.MessageBox(MSG.OK, common.LanguageDictGetString("#str_04328"), common.LanguageDictGetString("#str_04329"), true);
                cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                return true;
            }
            return false;
        }

        void ProcessDownloadInfoMessage(Netadr from, BitMsg msg)
        {
            string buf;
            int srvDlRequest = msg.ReadInt();
            var infoType = (SERVER_DL)msg.ReadByte();
            SERVER_PAK pakDl;
            int pakIndex;

            PakDlEntry entry;
            bool gotAllFiles = true;
            string sizeStr;
            bool gotGame = false;

            if (dlRequest == -1 || srvDlRequest != dlRequest) { common.Warning("bad download id from server, ignored"); return; }
            // mark the dlRequest as dead now whatever how we process it
            dlRequest = -1;

            if (infoType == SERVER_DL.REDIRECT)
            {
                msg.ReadString(out buf);
                cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                // "You are missing required pak files to connect to this server.\nThe server gave a web page though:\n%s\nDo you want to go there now?". "Missing required files"
                if (session.MessageBox(MSG.YESNO, $"{common.LanguageDictGetString("#str_07217")}{buf}", common.LanguageDictGetString("#str_07218"), true, "yes")[0] != 0) system.OpenURL(buf, true);
            }
            else if (infoType == SERVER_DL.LIST)
            {
                cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                if (dlList.Count != 0) { common.Warning("tried to process a download list while already busy downloading things"); return; }
                // read the URLs, check against what we requested, prompt for download
                pakIndex = -1;
                totalDlSize = 0;
                do
                {
                    pakIndex++;
                    pakDl = (SERVER_PAK)msg.ReadByte();
                    if (pakDl == SERVER_PAK.YES)
                    {
                        if (pakIndex == 0) gotGame = true;
                        msg.ReadString(out entry.filename);
                        msg.ReadString(out entry.url);
                        entry.size = msg.ReadInt();
                        // checksums are not transmitted, we read them from the dl request we sent
                        entry.checksum = dlChecksums[pakIndex];
                        totalDlSize += entry.size;
                        dlList.Add(entry);
                        common.Printf($"download {entry.filename} from {entry.url} ( 0x{entry.checksum:x} )\n");
                    }
                    else if (pakDl == SERVER_PAK.NO)
                    {
                        msg.ReadString(out buf);
                        entry.filename = buf;
                        entry.url = string.Empty;
                        entry.size = 0;
                        entry.checksum = 0;
                        dlList.Add(entry);
                        // first pak is game pak, only fail it if we actually requested it
                        if (pakIndex != 0 || dlChecksums[0] != 0) { common.Printf($"no download offered for {entry.filename} ( 0x{dlChecksums[pakIndex]:x} )\n"); gotAllFiles = false; }
                    }
                    else Debug.Assert(pakDl == SERVER_PAK.END);
                } while (pakDl != SERVER_PAK.END);
                if (dlList.Count < dlCount) { common.Printf($"{(dlCount - dlList.Count)} files were ignored by the server\n"); gotAllFiles = false; }
                stringX.BestUnit(out sizeStr, "{0:.2}", totalDlSize, stringX.MEASURE.SIZE);
                cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                // was no downloadable stuff for us. "Can't connect to the pure server: no downloads offered". "Missing required files"
                if (totalDlSize == 0) { dlList.Clear(); session.MessageBox(MSG.OK, common.LanguageDictGetString("#str_07219"), common.LanguageDictGetString("#str_07218"), true); return; }
                var asked = false;
                if (gotGame)
                {
                    asked = true;
                    // "You need to download game code to connect to this server. Are you sure? You should only answer yes if you trust the server administrators.". "Missing game binaries"
                    if (string.IsNullOrEmpty(session.MessageBox(MSG.YESNO, common.LanguageDictGetString("#str_07220"), common.LanguageDictGetString("#str_07221"), true, "yes"))) { dlList.Clear(); return; }
                }
                if (!gotAllFiles)
                {
                    asked = true;
                    // "The server only offers to download some of the files required to connect ( %s ). Download anyway?". "Missing required files"
                    if (string.IsNullOrEmpty(session.MessageBox(MSG.YESNO, $"{common.LanguageDictGetString("#str_07222")}{sizeStr}", common.LanguageDictGetString("#str_07218"), true, "yes"))) { dlList.Clear(); return; }
                }
                if (!asked && AsyncNetwork.clientDownload.Integer == 1)
                {
                    // "You need to download some files to connect to this server ( %s ), proceed?". "Missing required files"
                    if (string.IsNullOrEmpty(session.MessageBox(MSG.YESNO, $"{common.LanguageDictGetString("#str_07224")}{sizeStr}", common.LanguageDictGetString("#str_07218"), true, "yes"))) { dlList.Clear(); return; }
                }
            }
            else
            {
                cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");
                // "You are missing some files to connect to this server, and the server doesn't provide downloads.". "Missing required files"
                session.MessageBox(MSG.OK, common.LanguageDictGetString("#str_07223"), common.LanguageDictGetString("#str_07218"), true);
            }
        }

        int GetDownloadRequest(int[] checksums, int count)
        {
            Debug.Assert(checksums[count] == 0); // 0-terminated
            if (!Enumerable.SequenceEqual(dlChecksums, checksums))
            {
                RandomX newreq = new();
                dlChecksums = checksums;
                newreq.Seed = (int)SysW.Milliseconds;
                dlRequest = newreq.RandomInt();
                dlCount = count;
                return dlRequest;
            }
            // this is the same dlRequest, we haven't heard from the server. keep the same id
            return dlRequest;
        }
    }
}