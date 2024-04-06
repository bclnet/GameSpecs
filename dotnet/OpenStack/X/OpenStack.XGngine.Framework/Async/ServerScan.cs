using System.Collections.Generic;
using System.Linq;
using System.NumericsX.OpenStack.Gngine.UI;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework.Async
{
    // storage for incoming servers / server scan
    public class InServer
    {
        public Netadr adr;
        public int id;
        public int time;
    }

    // the menu gui uses a hard-coded control type to display a list of network games
    public class NetworkServer
    {
        public Netadr adr;
        public Dictionary<string, string> serverInfo;
        public int ping;
        public int id;          // idnet mode sends an id for each server in list
        public int clients;
        public string[] nickname = new string[Config.MAX_ASYNC_CLIENTS];
        public short[] pings = new short[Config.MAX_ASYNC_CLIENTS];
        public int[] rate = new int[Config.MAX_ASYNC_CLIENTS];
        public int challenge;
    }

    public enum ServerSort
    {
        SORT_PING,
        SORT_SERVERNAME,
        SORT_PLAYERS,
        SORT_GAMETYPE,
        SORT_MAP,
        SORT_GAME
    }

    public class ServerScan : List<NetworkServer>
    {
        static CVar gui_filter_password = new("gui_filter_password", "0", CVAR.GUI | CVAR.INTEGER | CVAR.ARCHIVE, "Password filter");
        static CVar gui_filter_players = new("gui_filter_players", "0", CVAR.GUI | CVAR.INTEGER | CVAR.ARCHIVE, "Players filter");
        static CVar gui_filter_gameType = new("gui_filter_gameType", "0", CVAR.GUI | CVAR.INTEGER | CVAR.ARCHIVE, "Gametype filter");
        static CVar gui_filter_idle = new("gui_filter_idle", "0", CVAR.GUI | CVAR.INTEGER | CVAR.ARCHIVE, "Idle servers filter");
        static CVar gui_filter_game = new("gui_filter_game", "0", CVAR.GUI | CVAR.INTEGER | CVAR.ARCHIVE, "Game filter");

        static readonly string[] _gameTypes = {
            "Deathmatch",
            "Tourney",
            "Team DM",
            "Last Man",
            "CTF",
            null
        };

        static ServerScan _serverScan = null;

        const int MAX_PINGREQUESTS = 32;     // how many servers to query at once
        const int REPLY_TIMEOUT = 999;       // how long should we wait for a reply from a game server
        const int INCOMING_TIMEOUT = 1500;       // when we got an incoming server list, how long till we decide the list is done
        const int REFRESH_START = 10000; // how long to wait when sending the initial refresh request

        ScanState scanState;

        bool incoming_net;  // set to true while new servers are fed through AddServer
        bool incoming_useTimeout;
        int incoming_lastTime;

        int lan_pingtime;   // holds the time of LAN scan

        // servers we're waiting for a reply from won't exceed MAX_PINGREQUESTS elements holds index of net_servers elements, indexed by 'from' string
        Dictionary<string, int> net_info = new();

        List<InServer> net_servers = new();
        // where we are in net_servers list for getInfo emissions ( NET_SCAN only )
        // we may either be waiting on MAX_PINGREQUESTS, or for net_servers to grow some more ( through AddServer )
        int cur_info;

        IUserInterface gui;
        IListGUI listGUI;

        ServerSort _sort;
        bool _sortAscending;
        List<int> _sortedServers;  // use ascending for the walking order

        string screenshot;
        int challenge;          // challenge for current scan

        int endWaitTime;        // when to stop waiting on a port init

        public enum ScanState
        {
            IDLE = 0,
            WAIT_ON_INIT,
            LAN_SCAN,
            NET_SCAN
        }

        public ScanState State
        {
            get => scanState;
            set { } //void SetState(scan_state a);
        }

        public ServerScan()
        {
            gui = null;
            _sort = ServerSort.SORT_PING;
            _sortAscending = true;
            challenge = 0;
            LocalClear();
        }

        void LocalClear()      // we need to clear some internal data as well
        {
            scanState = ScanState.IDLE;
            incoming_net = false;
            lan_pingtime = -1;
            net_info.Clear();
            net_servers.Clear();
            cur_info = 0;
            listGUI?.Clear();
            incoming_useTimeout = false;
            _sortedServers.Clear();
        }

        // clear
        public new void Clear()
        {
            LocalClear();
            base.Clear();
        }

        public void Shutdown()
        {
            gui = null;
            if (listGUI != null)
            {
                listGUI.Config(null, null);
                uiManager.FreeListGUI(listGUI);
                listGUI = null;
            }
            screenshot = string.Empty;
        }

        // prepare for a LAN scan. idAsyncClient does the network job (UDP broadcast), we do the storage
        public void SetupLANScan()
        {
            Clear();
            GUIUpdateSelected();
            scanState = ScanState.LAN_SCAN;
            challenge++;
            lan_pingtime = SysW.Milliseconds;
            common.DPrintf($"SetupLANScan with challenge {challenge}\n");
        }

        public bool InfoResponse(NetworkServer server)
        {
            if (scanState == ScanState.IDLE) return false;

            var serv = server.adr.ToString();

            if (server.challenge != challenge) { common.DPrintf($"ServerScan::InfoResponse - ignoring response from {serv}, wrong challenge {server.challenge}."); return false; }

            if (scanState == ScanState.NET_SCAN)
            {
                if (!net_info.TryGetValue(serv, out var value)) { common.DPrintf($"ServerScan::InfoResponse NET_SCAN: reply from unknown {serv}\n"); return false; }
                var id = value;
                net_info.Remove(serv);
                var iserv = net_servers[id];
                server.ping = SysW.Milliseconds - iserv.time;
                server.id = iserv.id;
            }
            else
            {
                server.ping = SysW.Milliseconds - lan_pingtime;
                server.id = 0;

                // check for duplicate servers
                for (var i = 0; i < Count; i++) if (this[i].adr == server.adr) { common.DPrintf($"ServerScan::InfoResponse LAN_SCAN: duplicate server {serv}\n"); return true; }
            }

            var si_map = server.serverInfo["si_map"];
            var mapDecl = declManager.FindType(DECL.MAPDEF, si_map, false);
            var mapDef = (DeclEntityDef)mapDecl;
            if (mapDef != null) { var mapName = common.LanguageDictGetString(mapDef.dict.Get("name", si_map)); server.serverInfo["si_mapName"] = mapName; }
            else server.serverInfo["si_mapName"] = si_map;

            var index = Count; Add(server);

            // for now, don't maintain sorting when adding new info response servers
            _sortedServers.Add(index);
            if (listGUI.IsConfigured && !IsFiltered(server)) GUIAdd(index, server);
            if (listGUI.GetSelection(out _, 0) == index) GUIUpdateSelected();

            return true;
        }

        // add an internet server - ( store a numeric id along with it )
        public void AddServer(int id, string srv)
        {
            incoming_net = true;
            incoming_lastTime = SysW.Milliseconds + INCOMING_TIMEOUT;
            var s = new InServer { id = id };
            // using IPs, not hosts
            if (!Netadr.TryParse(srv, out s.adr, false)) { common.DPrintf($"ServerScan::AddServer: failed to parse server {srv}\n"); return; }
            if (s.adr.port == 0) s.adr.port = Config.PORT_SERVER;

            net_servers.Add(s);
        }

        // we are done filling up the list of server entries
        public void EndServers()
        {
            incoming_net = false;
            _serverScan = this;
            _sortedServers.Sort(Cmp);
            ApplyFilter();
        }

        // we are going to feed server entries to be pinged. if timeout is true, use a timeout once we start AddServer to trigger EndServers and decide the scan is done
        public void StartServers(bool timeout)
        {
            incoming_net = true;
            incoming_useTimeout = timeout;
            incoming_lastTime = SysW.Milliseconds + REFRESH_START;
        }

        void EmitGetInfo(Netadr serv)
            => AsyncNetwork.client.GetServerInfo(serv);

        public int GetChallenge()
            => challenge;

        // scan the current list of servers - used for refreshes and while receiving a fresh list
        public void NetScan()
        {
            if (!AsyncNetwork.client.IsPortInitialized)
            {
                // if the port isn't open, initialize it, but wait for a short time to let the OS do whatever magic things it needs to do...
                AsyncNetwork.client.InitPort();
                // start the scan one second from now...
                scanState = ScanState.WAIT_ON_INIT;
                endWaitTime = SysW.Milliseconds + 1000;
                return;
            }

            // make sure the client port is open
            AsyncNetwork.client.InitPort();

            scanState = ScanState.NET_SCAN;
            challenge++;

            base.Clear();
            _sortedServers.Clear();
            cur_info = 0;
            net_info.Clear();
            listGUI.Clear();
            GUIUpdateSelected();
            common.DPrintf($"NetScan with challenge {challenge}\n");

            while (cur_info < Math.Min(net_servers.Count, MAX_PINGREQUESTS))
            {
                var serv = net_servers[cur_info].adr;
                EmitGetInfo(serv);
                net_servers[cur_info].time = SysW.Milliseconds;
                net_info[serv.ToString()] = cur_info;
                cur_info++;
            }
        }

        // called each game frame. Updates the scanner state, takes care of ongoing scans
        public void RunFrame()
        {
            if (scanState == ScanState.IDLE) return;

            if (scanState == ScanState.WAIT_ON_INIT) { if (SysW.Milliseconds >= endWaitTime) { scanState = ScanState.IDLE; NetScan(); } return; }

            var timeout_limit = SysW.Milliseconds - REPLY_TIMEOUT;

            if (scanState == ScanState.LAN_SCAN) { if (timeout_limit > lan_pingtime) { common.Printf("Scanned for servers on the LAN\n"); scanState = ScanState.IDLE; } return; }

            // if scan_state == NET_SCAN

            // check for timeouts
            var i = 0;
            while (i < net_info.Count)
                if (timeout_limit > net_servers[net_info.ElementAt(i).Value].time) { common.DPrintf($"timeout {net_info.ElementAt(i).Key}\n"); net_info.Remove(net_info.ElementAt(i).Key); }
                else i++;

            // possibly send more queries
            while (cur_info < net_servers.Count && net_info.Count < MAX_PINGREQUESTS)
            {
                var serv = net_servers[cur_info].adr;
                EmitGetInfo(serv);
                net_servers[cur_info].time = SysW.Milliseconds;
                net_info[serv.ToString()] = cur_info;
                cur_info++;
            }

            // update state
            if ((!incoming_net || (incoming_useTimeout && SysW.Milliseconds > incoming_lastTime)) && net_info.Count == 0)
            {
                EndServers();
                // the list is complete, we are no longer waiting for any getInfo replies
                common.Printf($"Scanned {cur_info} servers.\n");
                scanState = ScanState.IDLE;
            }
        }

        public bool GetBestPing(NetworkServer serv)
        {
            var ic = Count;
            if (ic == 0) return false;
            serv = this[0];
            for (var i = 0; i < ic; i++) if (this[i].ping < serv.ping) serv = this[i];
            return true;
        }

        public void GUIConfig(IUserInterface gui, string name)
        {
            this.gui = gui;
            if (listGUI == null) listGUI = uiManager.AllocListGUI();
            listGUI.Config(gui, name);
        }

        // update the GUI fields with information about the currently selected server
        public void GUIUpdateSelected()
        {
            if (gui == null) return;
            var i = listGUI.GetSelection(out _, 0);
            if (i == -1 || i >= Count)
            {
                gui.SetStateString("server_name", "");
                gui.SetStateString("player1", "");
                gui.SetStateString("player2", "");
                gui.SetStateString("player3", "");
                gui.SetStateString("player4", "");
                gui.SetStateString("player5", "");
                gui.SetStateString("player6", "");
                gui.SetStateString("player7", "");
                gui.SetStateString("player8", "");
                gui.SetStateString("server_map", "");
                gui.SetStateString("browser_levelshot", "");
                gui.SetStateString("server_gameType", "");
                gui.SetStateString("server_IP", "");
                gui.SetStateString("server_passworded", "");
            }
            else
            {
                gui.SetStateString("server_name", this[i].serverInfo["si_name"]);
                for (var j = 0; j < 8; j++) gui.SetStateString($"player{j + 1}", this[i].clients > j ? this[i].nickname[j] : string.Empty);
                gui.SetStateString("server_map", this[i].serverInfo.Get("si_mapName"));
                fileSystem.FindMapScreenshot(this[i].serverInfo.Get("si_map"), out var screenshot);
                gui.SetStateString("browser_levelshot", screenshot);
                gui.SetStateString("server_gameType", this[i].serverInfo.Get("si_gameType"));
                gui.SetStateString("server_IP", this[i].adr.ToString());
                gui.SetStateString("server_passworded", this[i].serverInfo.Get("si_usePass", "0") != "0" ? "PASSWORD REQUIRED" : string.Empty);
            }
        }

        void GUIAdd(int id, NetworkServer server)
        {
            var name = server.serverInfo.Get("si_name", $"{PlatformW.GAME_NAME} Server");
            var d3xp = string.Equals(server.serverInfo.Get("fs_game"), "d3xp", StringComparison.OrdinalIgnoreCase) || string.Equals(server.serverInfo.Get("fs_game_base"), "d3xp", StringComparison.OrdinalIgnoreCase);
            var mod = server.serverInfo.Get("fs_game")[0] != '\0';

            name += "\t";
            if (server.serverInfo.Get("sv_punkbuster")[0] == '1') name += "mtr_PB";
            name += "\t";
            if (d3xp) { name += "mtr_doom3XPIcon"; } // FIXME: even for a 'D3XP mod' could have a specific icon for this case
            else if (mod) name += "mtr_doom3Mod";
            else name += "mtr_doom3Icon";
            name += "\t";
            name += $"{server.clients}/{server.serverInfo.Get("si_maxPlayers")}\t";
            name += server.ping > -1 ? $"{server.ping}\t" : "na\t";
            name += server.serverInfo.Get("si_gametype");
            name += "\t";
            name += server.serverInfo.Get("si_mapName");
            name += "\t";
            listGUI.Add(id, name);
        }

        public void ApplyFilter()
        {
            listGUI.SetStateChanges(false);
            listGUI.Clear();
            for (var i = _sortAscending ? 0 : _sortedServers.Count - 1; _sortAscending ? i < _sortedServers.Count : i >= 0;)
            {
                var serv = this[_sortedServers[i]];
                if (!IsFiltered(serv)) GUIAdd(_sortedServers[i], serv);
                if (_sortAscending) i++;
                else i--;
            }
            GUIUpdateSelected();
            listGUI.SetStateChanges(true);
        }

        static bool IsFiltered(NetworkServer server)
        {
            // password filter
            var hasval = server.serverInfo.TryGetValue("si_usePass", out var keyval);
            if (hasval && gui_filter_password.Integer == 1)
            {
                // show passworded only
                if (keyval[0] == '0') return true;
            }
            else if (hasval && gui_filter_password.Integer == 2)
            {
                // show no password only
                if (keyval[0] != '0') return true;
            }
            // players filter
            hasval = server.serverInfo.TryGetValue("si_maxPlayers", out keyval);
            if (hasval)
            {
                if (gui_filter_players.Integer == 1 && server.clients == int.Parse(keyval)) return true;
                else if (gui_filter_players.Integer == 2 && (server.clients == 0 || server.clients == int.Parse(keyval))) return true;
            }
            // gametype filter
            hasval = server.serverInfo.TryGetValue("si_gameType", out keyval);
            if (hasval && gui_filter_gameType.Integer != 0)
            {
                var i = 0;
                while (_gameTypes[i] != null)
                {
                    if (string.Equals(keyval, _gameTypes[i], StringComparison.OrdinalIgnoreCase)) break;
                    i++;
                }
                if (_gameTypes[i] != null && i != gui_filter_gameType.Integer - 1) return true;
            }
            // idle server filter
            hasval = server.serverInfo.TryGetValue("si_idleServer", out keyval);
            if (hasval && gui_filter_idle.Integer == 0)
            {
                if (keyval == "1") return true;
            }

            // autofilter D3XP games if the user does not has the XP installed
            if (!fileSystem.HasD3XP && string.Equals(server.serverInfo.Get("fs_game"), "d3xp", StringComparison.OrdinalIgnoreCase)) return true;

            // filter based on the game doom or XP
            if (gui_filter_game.Integer == 1) // Only Doom
            {
                if (server.serverInfo.Get("fs_game") == null) return true;
            }
            else if (gui_filter_game.Integer == 2) // Only D3XP
            {
                if (string.Equals(server.serverInfo.Get("fs_game"), "d3xp", StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        static int Cmp(int a, int b)
        {
            int ret; string s1, s2;

            var serv1 = _serverScan[a];
            var serv2 = _serverScan[b];
            switch (_serverScan._sort)
            {
                case ServerSort.SORT_PING:
                    ret = serv1.ping < serv2.ping ? -1 : (serv1.ping > serv2.ping ? 1 : 0);
                    return ret;
                case ServerSort.SORT_SERVERNAME:
                    s1 = serv1.serverInfo.Get("si_name", "");
                    s2 = serv2.serverInfo.Get("si_name", "");
                    //return s1.IcmpNoColor(s2);
                    return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
                case ServerSort.SORT_PLAYERS:
                    ret = serv1.clients < serv2.clients ? -1 : (serv1.clients > serv2.clients ? 1 : 0);
                    return ret;
                case ServerSort.SORT_GAMETYPE:
                    s1 = serv1.serverInfo.Get("si_gameType", "");
                    s2 = serv2.serverInfo.Get("si_gameType", "");
                    return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
                case ServerSort.SORT_MAP:
                    s1 = serv1.serverInfo.Get("si_mapName", "");
                    s2 = serv2.serverInfo.Get("si_mapName", "");
                    return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
                case ServerSort.SORT_GAME:
                    s1 = serv1.serverInfo.Get("fs_game", "");
                    s2 = serv2.serverInfo.Get("fs_game", "");
                    return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
            }
            return 0;
        }

        // there is an internal toggle, call twice with same sort to switch
        public void SetSorting(ServerSort sort)
        {
            _serverScan = this;
            if (sort == _sort) _sortAscending = !_sortAscending;
            else
            {
                _sort = sort;
                _sortAscending = true; // is the default for any new sort
                _sortedServers.Sort(Cmp);
            }
            // trigger a redraw
            ApplyFilter();
        }
    }
}