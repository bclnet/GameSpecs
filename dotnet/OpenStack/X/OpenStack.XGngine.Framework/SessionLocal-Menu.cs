using System.Collections.Generic;
using System.Diagnostics;
using System.NumericsX.OpenStack.Gngine.Framework.Async;
using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.Gngine.UI;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Framework.Framework;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    unsafe partial class SessionLocal
    {
        public static CVar gui_configServerRate = new("gui_configServerRate", "0", CVAR.GUI | CVAR.ARCHIVE | CVAR.ROM | CVAR.INTEGER, "");

        public List<string> loadGameList = new();
        public List<string> modsList = new();

        public IUserInterface ActiveMenu
            => guiActive;

        public override void StartMenu(bool playIntro = false)
        {
            if (guiActive == guiMainMenu)
                return;

            // if we're playing a demo, esc kills it
            if (readDemo != null) UnloadMap();

            // pause the game sound world
            if (sw != null && !sw.IsPaused) sw.Pause();

            // start playing the menu sounds
            soundSystem.PlayingSoundWorld = menuSoundWorld;

            SetGUI(guiMainMenu, null);
            guiMainMenu.HandleNamedEvent(playIntro ? "playIntro" : "noIntro");

            guiMainMenu.SetStateString("game_list", common.LanguageDictGetString(fileSystem.HasD3XP ? "#str_07202" : "#str_07212"));

            console.Close();
        }

        public override void SetGUI(IUserInterface gui, HandleGuiCommand handle)
        {
            guiActive = gui;
            guiHandle = handle;
            if (guiMsgRestore != null) { common.DPrintf("SessionLocal::SetGUI: cleared an active message box\n"); guiMsgRestore = null; }
            if (guiActive == null) return;

            if (guiActive == guiMainMenu) { SetSaveGameGuiVars(); SetMainMenuGuiVars(); }
            else if (guiActive == guiRestartMenu) SetSaveGameGuiVars();

            SysEvent ev = default;
            ev.evType = SE.NONE;

            guiActive.HandleEvent(ev, com_frameTime);
            guiActive.Activate(true, com_frameTime);
        }

        public void ExitMenu()
        {
            guiActive = null;

            // go back to the game sounds
            soundSystem.PlayingSoundWorld = sw;

            // unpause the game sound world
            if (sw != null && sw.IsPaused) sw.UnPause();
        }

        public static void GetSaveGameList(out List<string> fileList, out List<(int ix, DateTime ts)> fileTimes)
        {
            fileTimes = new();
            // NOTE: no fs_game_base for savegames
            var game = cvarSystem.GetCVarString("fs_game");
            var files = fileSystem.ListFiles("savegames", ".save", false, false, game.Length != 0 ? game : null);

            fileList = files.List;
            fileSystem.FreeFileList(files);

            for (var i = 0; i < fileList.Count; i++)
            {
                fileSystem.ReadFile($"savegames/{fileList[i]}", out var timeStamp);
                fileList[i] = PathX.StripFileExtension(fileList[i].StripLeading("/"));
                fileTimes.Add((i, timeStamp));
            }

            fileTimes.Sort((a, b) => b.ts.CompareTo(a.ts));
        }

        public void SetSaveGameGuiVars()
        {
            string name;

            loadGameList.Clear();

            GetSaveGameList(out var fileList, out var fileTimes);

            loadGameList.SetNum(fileList.Count);
            for (var i = 0; i < fileList.Count; i++)
            {
                loadGameList[i] = fileList[fileTimes[i].ix];

                var src = new Lexer(LEXFL.NOERRORS | LEXFL.NOSTRINGCONCAT);
                if (src.LoadFile($"savegames/{loadGameList[i]}.txt")) { src.ReadToken(out var tok); name = tok; }
                else name = loadGameList[i];

                name += "\t";

                var date = SysW.TimeStampToStr(fileTimes[i].ts);
                name += date;

                guiActive.SetStateString($"loadgame_item_{i}", name);
            }
            guiActive.DeleteStateVar($"loadgame_item_{fileList.Count}");

            guiActive.SetStateString("loadgame_sel_0", "-1");
            guiActive.SetStateString("loadgame_shot", "guis/assets/blankLevelShot");
        }

        public void SetModsMenuGuiVars()
        {
            var list = fileSystem.ListMods();

            modsList.SetNum(list.NumMods);

            // Build the gui list
            for (var i = 0; i < list.NumMods; i++)
            {
                guiActive.SetStateString($"modsList_item_{i}", list.GetDescription(i));
                modsList[i] = list.GetMod(i);
            }
            guiActive.DeleteStateVar($"modsList_item_{list.NumMods}");
            guiActive.SetStateString("modsList_sel_0", "-1");

            fileSystem.FreeModList(list);
        }

        public void SetMainMenuSkin()
        {
            // skins
            var str = cvarSystem.GetCVarString("mod_validSkins");
            var uiSkin = cvarSystem.GetCVarString("ui_skin");
            string skin; int skinId = 1, count = 1;
            while (str.Length != 0)
            {
                var n = str.IndexOf(";");
                if (n >= 0) { skin = str[..n]; str = str[(str.Length - n - 2)..]; }
                else { skin = str; str = ""; }
                if (string.Equals(skin, uiSkin, StringComparison.OrdinalIgnoreCase)) skinId = count;
                count++;
            }

            for (var i = 0; i < count; i++)
                guiMainMenu.SetStateInt($"skin{i + 0}", 0);
            guiMainMenu.SetStateInt($"skin{skinId}", 1);
        }

        public static void SetPbMenuGuiVars() { }

        public void SetMainMenuGuiVars()
        {
            guiMainMenu.SetStateString("serverlist_sel_0", "-1");
            guiMainMenu.SetStateString("serverlist_selid_0", "-1");

            guiMainMenu.SetStateInt("com_machineSpec", 0);

            // "inetGame" will hold a hand-typed inet address, which is not archived to a cvar
            guiMainMenu.SetStateString("inetGame", "");

            // key bind names
            guiMainMenu.SetKeyBindingNames();

            // flag for in-game menu
            guiMainMenu.SetStateString("inGame", mapSpawned ? (IsMultiplayer ? "2" : "1") : "0");

            SetCDKeyGuiVars();
            guiMainMenu.SetStateString("nightmare", cvarSystem.GetCVarBool("g_nightmare") ? "1" : "0");
            guiMainMenu.SetStateString("browser_levelshot", "guis/assets/splash/pdtempa");

            SetMainMenuSkin();
            // Mods Menu
            SetModsMenuGuiVars();

            guiMsg.SetStateString("visible_hasxp", fileSystem.HasD3XP ? "1" : "0");

            guiMainMenu.SetStateString("driver_prompt", "0");

            SetPbMenuGuiVars();
        }

        static string TimeStampToFilename()
            => DateTime.Now.ToString("yyyyMMddhhmmss");

        public bool HandleSaveGameMenuCommand(CmdArgs args, int icmd)
        {
            var cmd = args[icmd - 1];

            if (string.Equals(cmd, "loadGame", StringComparison.OrdinalIgnoreCase))
            {
                var choice = guiActive.State.GetInt("loadgame_sel_0");
                if (choice >= 0 && choice < loadGameList.Count)
                    sessLocal.LoadGame(loadGameList[choice]);
                return true;
            }

            if (string.Equals(cmd, "createNewName", StringComparison.OrdinalIgnoreCase))
            {
                guiActive.SetStateString("saveGameName", $"Save: {GetSaveMapName(mapSpawnData.serverInfo.GetString("si_map"))}{TimeStampToFilename()}");
                guiActive.StateChanged(com_frameTime);
                return true;
            }

            if (string.Equals(cmd, "saveGame", StringComparison.OrdinalIgnoreCase))
            {
                //const char *saveGameName = TimeStampToFilename();//
                var saveGameName = guiActive.State.GetString("saveGameName");
                if (!string.IsNullOrEmpty(saveGameName))
                {
                    // First see if the file already exists unless they pass '1' to authorize the overwrite
                    if (icmd == args.Count || intX.Parse(args[icmd++]) == 0)
                    {
                        var saveFileName = saveGameName;
                        sessLocal.ScrubSaveGameFileName(ref saveFileName);
                        saveFileName = PathX.SetFileExtension($"savegames/{saveFileName}", ".save");

                        var game = cvarSystem.GetCVarString("fs_game");
                        var file = fileSystem.OpenFileRead(saveFileName, true, game.Length != 0 ? game : null);
                        if (file != null)
                        {
                            fileSystem.CloseFile(file);

                            // The file exists, see if it's an autosave
                            saveFileName = PathX.SetFileExtension(saveFileName, ".txt");
                            var src = new Lexer(LEXFL.NOERRORS | LEXFL.NOSTRINGCONCAT);
                            if (src.LoadFile(saveFileName))
                            {
                                src.ReadToken(out var tok); // Name
                                src.ReadToken(out tok); // Map
                                src.ReadToken(out tok); // Screenshot
                                if (!string.IsNullOrEmpty(tok)) { guiActive.HandleNamedEvent("autosaveOverwriteError"); return true; } // NOTE: base/ gui doesn't handle that one
                            }
                            guiActive.HandleNamedEvent("saveGameOverwrite");
                            return true;
                        }
                    }

                    sessLocal.SaveGame(saveGameName);
                    SetSaveGameGuiVars();
                    guiActive.StateChanged(com_frameTime);
                }
                return true;
            }

            if (string.Equals(cmd, "deleteGame", StringComparison.OrdinalIgnoreCase))
            {
                var choice = guiActive.State.GetInt("loadgame_sel_0");
                if (choice >= 0 && choice < loadGameList.Count)
                {
                    fileSystem.RemoveFile($"savegames/{loadGameList[choice]}.save");
                    fileSystem.RemoveFile($"savegames/{loadGameList[choice]}.tga");
                    fileSystem.RemoveFile($"savegames/{loadGameList[choice]}.txt");
                    SetSaveGameGuiVars();
                    guiActive.StateChanged(com_frameTime);
                }
                return true;
            }

            if (string.Equals(cmd, "updateSaveGameInfo", StringComparison.OrdinalIgnoreCase))
            {
                var choice = guiActive.State.GetInt("loadgame_sel_0");
                if (choice >= 0 && choice < loadGameList.Count)
                {
                    string saveName, description, screenshot;
                    var src = new Lexer(LEXFL.NOERRORS | LEXFL.NOSTRINGCONCAT);
                    if (src.LoadFile($"savegames/{loadGameList[choice]}.txt"))
                    {
                        src.ReadToken(out var tok); saveName = tok;
                        src.ReadToken(out tok); description = tok;
                        src.ReadToken(out tok); screenshot = tok;
                    }
                    else
                    {
                        saveName = loadGameList[choice];
                        description = loadGameList[choice];
                        screenshot = "";
                    }
                    if (screenshot.Length == 0) screenshot = $"savegames/{loadGameList[choice]}.tga";
                    var material = declManager.FindMaterial(screenshot);
                    material?.ReloadImages(false);
                    guiActive.SetStateString("loadgame_shot", screenshot);

                    saveName = stringX.RemoveColors(saveName);
                    guiActive.SetStateString("saveGameName", saveName);
                    guiActive.SetStateString("saveGameDescription", description);

                    fileSystem.ReadFile($"savegames/{loadGameList[choice]}.save", out var timeStamp);
                    var date = SysW.TimeStampToStr(timeStamp);
                    var tab = date.IndexOf("\t");
                    var time = date[(date.Length - tab - 2)..];
                    guiActive.SetStateString("saveGameDate", date[..tab]);
                    guiActive.SetStateString("saveGameTime", time);
                }
                return true;
            }

            return false;
        }

        // Executes any commands returned by the gui
        public void HandleRestartMenuCommands(string menuCommand)
        {
            // execute the command from the menu
            CmdArgs args = new();
            args.TokenizeString(menuCommand, false);
            for (var icmd = 0; icmd < args.Count;)
            {
                var cmd = args[icmd++];

                if (HandleSaveGameMenuCommand(args, icmd)) continue;
                if (string.Equals(cmd, "restart", StringComparison.OrdinalIgnoreCase))
                {
                    // If we can't load the autosave then just restart the map
                    if (!LoadGame(GetAutoSaveName(mapSpawnData.serverInfo.GetString("si_map")))) MoveToNewMap(mapSpawnData.serverInfo.GetString("si_map"));
                    continue;
                }
                if (string.Equals(cmd, "quit", StringComparison.OrdinalIgnoreCase)) { ExitMenu(); common.Quit(); return; }
                if (string.Equals(cmd, "exec", StringComparison.OrdinalIgnoreCase)) { cmdSystem.BufferCommandText(CMD_EXEC.APPEND, args[icmd++]); continue; }
                if (string.Equals(cmd, "play", StringComparison.OrdinalIgnoreCase)) { if (args.Count - icmd >= 1) sw.PlayShaderDirectly(args[icmd++]); continue; }
            }
        }

        // Executes any commands returned by the gui
        public void HandleIntroMenuCommands(string menuCommand)
        {
            // execute the command from the menu
            CmdArgs args = new();
            args.TokenizeString(menuCommand, false);
            for (var i = 0; i < args.Count;)
            {
                var cmd = args[i++];

                if (string.Equals(cmd, "startGame", StringComparison.OrdinalIgnoreCase)) { menuSoundWorld.ClearAllSoundEmitters(); ExitMenu(); continue; }
                if (string.Equals(cmd, "play", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Count - i >= 1) menuSoundWorld.PlayShaderDirectly(args[i++]);
                    continue;
                }
            }
        }

        public void UpdateMPLevelShot()
        {
            fileSystem.FindMapScreenshot(cvarSystem.GetCVarString("si_map"), out var screenshot);
            guiMainMenu.SetStateString("current_levelshot", screenshot);
        }

        // Executes any commands returned by the gui
        public void HandleMainMenuCommands(string menuCommand)
        {
            // execute the command from the menu
            CmdArgs args = new();
            args.TokenizeString(menuCommand, false);
            for (var icmd = 0; icmd < args.Count;)
            {
                var cmd = args[icmd++];

                if (HandleSaveGameMenuCommand(args, icmd)) continue;
                // always let the game know the command is being run
                game?.HandleMainMenuCommands(cmd, guiActive);
                if (string.Equals(cmd, "startGame", StringComparison.OrdinalIgnoreCase))
                {
                    cvarSystem.SetCVarInteger("g_skill", guiMainMenu.State.GetInt("skill"));
                    StartNewGame(icmd < args.Count ? args[icmd++] : "game/mars_city1");
                    // need to do this here to make sure com_frameTime is correct or the gui activates with a time that is "however long map load took" time in the past
                    common.GUIFrame(false, false);
                    SetGUI(guiIntro, null);
                    guiIntro.StateChanged(com_frameTime, true);
                    // stop playing the game sounds
                    soundSystem.PlayingSoundWorld = menuSoundWorld;
                    continue;
                }
                if (string.Equals(cmd, "quit", StringComparison.OrdinalIgnoreCase)) { ExitMenu(); common.Quit(); return; }
                if (string.Equals(cmd, "loadMod", StringComparison.OrdinalIgnoreCase))
                {
                    var choice = guiActive.State.GetInt("modsList_sel_0");
                    if (choice >= 0 && choice < modsList.Count)
                    {
                        cvarSystem.SetCVarString("fs_game", modsList[choice]);
                        cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "reloadEngine menu\n");
                    }
                }
                if (string.Equals(cmd, "UpdateServers", StringComparison.OrdinalIgnoreCase))
                {
                    if (guiActive.State.GetBool("lanSet")) cmdSystem.BufferCommandText(CMD_EXEC.NOW, "LANScan");
                    else AsyncNetwork.GetNETServers();
                    continue;
                }
                if (string.Equals(cmd, "RefreshServers", StringComparison.OrdinalIgnoreCase))
                {
                    if (guiActive.State.GetBool("lanSet")) cmdSystem.BufferCommandText(CMD_EXEC.NOW, "LANScan");
                    else AsyncNetwork.client.serverList.NetScan();
                    continue;
                }
                if (string.Equals(cmd, "FilterServers", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.serverList.ApplyFilter(); continue; }
                if (string.Equals(cmd, "sortServerName", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.serverList.SetSorting(ServerSort.SORT_SERVERNAME); continue; }
                if (string.Equals(cmd, "sortGame", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.serverList.SetSorting(ServerSort.SORT_GAME); continue; }
                if (string.Equals(cmd, "sortPlayers", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.serverList.SetSorting(ServerSort.SORT_PLAYERS); continue; }
                if (string.Equals(cmd, "sortPing", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.serverList.SetSorting(ServerSort.SORT_PING); continue; }
                if (string.Equals(cmd, "sortGameType", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.serverList.SetSorting(ServerSort.SORT_GAMETYPE); continue; }
                if (string.Equals(cmd, "sortMap", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.serverList.SetSorting(ServerSort.SORT_MAP); continue; }
                if (string.Equals(cmd, "serverList", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.serverList.GUIUpdateSelected(); continue; }
                if (string.Equals(cmd, "LANConnect", StringComparison.OrdinalIgnoreCase)) { var sel = guiActive.State.GetInt("serverList_selid_0"); cmdSystem.BufferCommandText(CMD_EXEC.NOW, $"Connect {sel}\n"); return; }
                if (string.Equals(cmd, "MAPScan", StringComparison.OrdinalIgnoreCase))
                {
                    var gametype = cvarSystem.GetCVarString("si_gameType");
                    if (string.IsNullOrEmpty(gametype) || string.Equals(gametype, "singleplayer", StringComparison.OrdinalIgnoreCase)) gametype = "Deathmatch";

                    int i, num;
                    var si_map = cvarSystem.GetCVarString("si_map");
                    Dictionary<string, string> dict;

                    guiMainMenu_MapList.Clear();
                    guiMainMenu_MapList.SetSelection(0);
                    num = fileSystem.NumMaps;
                    for (i = 0; i < num; i++)
                    {
                        dict = fileSystem.GetMapDecl(i);
                        if (dict != null && dict.GetBool(gametype))
                        {
                            var mapName = dict.GetString("name");
                            if (string.IsNullOrEmpty(mapName)) mapName = dict.GetString("path");
                            mapName = common.LanguageDictGetString(mapName);
                            guiMainMenu_MapList.Add(i, mapName);
                            if (string.Equals(si_map, dict.GetString("path"), StringComparison.OrdinalIgnoreCase))
                                guiMainMenu_MapList.SetSelection(guiMainMenu_MapList.Num - 1);
                        }
                    }
                    i = guiMainMenu_MapList.GetSelection(out _, 0);
                    dict = i >= 0 ? fileSystem.GetMapDecl(i) : null;
                    cvarSystem.SetCVarString("si_map", dict != null ? dict.GetString("path") : "");

                    // set the current level shot
                    UpdateMPLevelShot();
                    continue;
                }
                if (string.Equals(cmd, "click_mapList", StringComparison.OrdinalIgnoreCase))
                {
                    var mapNum = guiMainMenu_MapList.GetSelection(out _, 0);
                    var dict = fileSystem.GetMapDecl(mapNum);
                    if (dict != null) cvarSystem.SetCVarString("si_map", dict.GetString("path"));
                    UpdateMPLevelShot();
                    continue;
                }
                if (string.Equals(cmd, "inetConnect", StringComparison.OrdinalIgnoreCase))
                {
                    var s = guiMainMenu.State.GetString("inetGame");
                    // don't put the menu away if there isn't a valid selection
                    if (string.IsNullOrEmpty(s)) continue;
                    cmdSystem.BufferCommandText(CMD_EXEC.NOW, $"connect {s}");
                    return;
                }
                if (string.Equals(cmd, "startMultiplayer", StringComparison.OrdinalIgnoreCase))
                {
                    var dedicated = guiActive.State.GetInt("dedicated");
                    cvarSystem.SetCVarBool("net_LANServer", guiActive.State.GetBool("server_type"));
                    if (gui_configServerRate.Integer > 0)
                    {
                        // guess the best rate for upstream, number of internet clients
                        if (gui_configServerRate.Integer == 5 || cvarSystem.GetCVarBool("net_LANServer")) cvarSystem.SetCVarInteger("net_serverMaxClientRate", 25600);
                        else
                        {
                            // internet players
                            var n_clients = cvarSystem.GetCVarInteger("si_maxPlayers");
                            if (dedicated == 0) n_clients--;
                            var maxclients = 0;
                            switch (gui_configServerRate.Integer)
                            {
                                // 128 kbits
                                case 1: cvarSystem.SetCVarInteger("net_serverMaxClientRate", 8000); maxclients = 2; break;
                                // 256 kbits
                                case 2: cvarSystem.SetCVarInteger("net_serverMaxClientRate", 9500); maxclients = 3; break;
                                // 384 kbits
                                case 3: cvarSystem.SetCVarInteger("net_serverMaxClientRate", 10500); maxclients = 4; break;
                                // 512 and above..
                                case 4: cvarSystem.SetCVarInteger("net_serverMaxClientRate", 14000); maxclients = 4; break;
                            }
                            if (n_clients > maxclients)
                            {
                                if (MessageBox(MSG.OKCANCEL, string.Format(common.LanguageDictGetString("#str_04315"), dedicated != 0 ? maxclients : Math.Min(8, maxclients + 1)), common.LanguageDictGetString("#str_04316"), true, "OK")[0] == 0)
                                    continue;
                                cvarSystem.SetCVarInteger("si_maxPlayers", dedicated != 0 ? maxclients : Math.Min(8, maxclients + 1));
                            }
                        }
                    }

                    if (dedicated == 0 && !cvarSystem.GetCVarBool("net_LANServer") && cvarSystem.GetCVarInteger("si_maxPlayers") > 4)
                        // "Dedicated server mode is recommended for internet servers with more than 4 players. Continue in listen mode?"
                        if (MessageBox(MSG.YESNO, common.LanguageDictGetString("#str_00100625"), common.LanguageDictGetString("#str_00100626"), true, "yes")[0] == 0)
                            continue;

                    cvarSystem.SetCVarInteger("net_serverDedicated", dedicated != 0 ? 1 : 0);
                    ExitMenu();
                    // may trigger a reloadEngine - APPEND
                    cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "SpawnServer\n");
                    return;
                }
                if (string.Equals(cmd, "mpSkin", StringComparison.OrdinalIgnoreCase)) { if (args.Count - icmd >= 1) { cvarSystem.SetCVarString("ui_skin", args[icmd++]); SetMainMenuSkin(); } continue; }
                // if we aren't in a game, the menu can't be closed
                if (string.Equals(cmd, "close", StringComparison.OrdinalIgnoreCase)) { if (mapSpawned) ExitMenu(); continue; }
                if (string.Equals(cmd, "resetdefaults", StringComparison.OrdinalIgnoreCase)) { cmdSystem.BufferCommandText(CMD_EXEC.NOW, "exec default.cfg"); guiMainMenu.SetKeyBindingNames(); continue; }
                if (string.Equals(cmd, "bind", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Count - icmd >= 2)
                    {
                        var key = intX.Parse(args[icmd++]);
                        var bind = args[icmd++];
                        if (KeyInput.NumBinds(bind) >= 2 && !KeyInput.KeyIsBoundTo(key, bind)) KeyInput.UnbindBinding(bind);
                        KeyInput.SetBinding(key, bind);
                        guiMainMenu.SetKeyBindingNames();
                    }
                    continue;
                }
                if (string.Equals(cmd, "play", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Count - icmd >= 1)
                    {
                        var snd = args[icmd++]; var channel = 1;
                        if (snd.Length == 1) { channel = intX.Parse(snd); snd = args[icmd++]; }
                        menuSoundWorld.PlayShaderDirectly(snd, channel);
                    }
                    continue;
                }
                if (string.Equals(cmd, "music", StringComparison.OrdinalIgnoreCase)) { if (args.Count - icmd >= 1) menuSoundWorld.PlayShaderDirectly(args[icmd++], 2); continue; }
                // triggered from mainmenu or mpmain
                if (string.Equals(cmd, "sound", StringComparison.OrdinalIgnoreCase))
                {
                    var vcmd = args.Count - icmd >= 1 ? args[icmd++] : null;
                    if (string.IsNullOrEmpty(vcmd) || string.Equals(vcmd, "speakers", StringComparison.OrdinalIgnoreCase))
                    {
                        var old = cvarSystem.GetCVarInteger("s_numberOfSpeakers");
                        cmdSystem.BufferCommandText(CMD_EXEC.NOW, "s_restart\n");
                        if (old != cvarSystem.GetCVarInteger("s_numberOfSpeakers"))
#if true //_WIN32
                            MessageBox(MSG.OK, common.LanguageDictGetString("#str_04142"), common.LanguageDictGetString("#str_04141"), true);
#else
                            MessageBox(MSG.OK, common.LanguageDictGetString("#str_07230"), common.LanguageDictGetString("#str_04141"), true); // a message that doesn't mention the windows control panel
#endif
                    }
                    if (string.Equals(vcmd, "eax", StringComparison.OrdinalIgnoreCase))
                    {
                        if (cvarSystem.GetCVarBool("s_useEAXReverb"))
                        {
                            var efx = soundSystem.IsEFXAvailable;
                            switch (efx)
                            {
                                // when you restart
                                case 1: MessageBox(MSG.OK, common.LanguageDictGetString("#str_04137"), common.LanguageDictGetString("#str_07231"), true); break;
                                // disabled
                                case -1: cvarSystem.SetCVarBool("s_useEAXReverb", false); MessageBox(MSG.OK, common.LanguageDictGetString("#str_07233"), common.LanguageDictGetString("#str_07231"), true); break;
                                // not available
                                case 0: cvarSystem.SetCVarBool("s_useEAXReverb", false); MessageBox(MSG.OK, common.LanguageDictGetString("#str_07232"), common.LanguageDictGetString("#str_07231"), true); break;
                            }
                        }
                        // when you restart
                        else MessageBox(MSG.OK, common.LanguageDictGetString("#str_04137"), common.LanguageDictGetString("#str_07231"), true);
                    }
                    if (string.Equals(vcmd, "drivar", StringComparison.OrdinalIgnoreCase)) cmdSystem.BufferCommandText(CMD_EXEC.NOW, "s_restart\n");
                    continue;
                }
                if (string.Equals(cmd, "video", StringComparison.OrdinalIgnoreCase))
                {
                    var vcmd = args.Count - icmd >= 1 ? args[icmd++] : null;
                    if (string.Equals(vcmd, "restart", StringComparison.OrdinalIgnoreCase)) { guiActive.HandleNamedEvent("cvar write render"); cmdSystem.BufferCommandText(CMD_EXEC.NOW, "vid_restart\n"); }
                    continue;
                }
                if (string.Equals(cmd, "clearBind", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Count - icmd >= 1) { KeyInput.UnbindBinding(args[icmd++]); guiMainMenu.SetKeyBindingNames(); }
                    continue;
                }
                // FIXME: obsolete
                if (string.Equals(cmd, "chatdone", StringComparison.OrdinalIgnoreCase))
                {
                    var temp = guiActive.State.GetString("chattext");
                    temp += "\r";
                    guiActive.SetStateString("chattext", "");
                    continue;
                }
                if (string.Equals(cmd, "exec", StringComparison.OrdinalIgnoreCase))
                {
                    // Backup the language so we can restore it after defaults.
                    var lang = cvarSystem.GetCVarString("sys_lang");

                    cmdSystem.BufferCommandText(CMD_EXEC.NOW, args[icmd++]);
                    if (string.Equals("cvar_restart", args[icmd - 1], StringComparison.OrdinalIgnoreCase))
                    {
                        cmdSystem.BufferCommandText(CMD_EXEC.NOW, "exec default.cfg");

                        //Make sure that any r_brightness changes take effect
                        float bright = cvarSystem.GetCVarFloat("r_brightness");
                        cvarSystem.SetCVarFloat("r_brightness", 0f);
                        cvarSystem.SetCVarFloat("r_brightness", bright);

                        //Force user info modified after a reset to defaults
                        cvarSystem.SetModifiedFlags(CVAR.USERINFO);

                        guiActive.SetStateInt("com_machineSpec", 0);

                        //Restore the language
                        cvarSystem.SetCVarString("sys_lang", lang);

                    }
                    continue;
                }
                if (string.Equals(cmd, "loadBinds", StringComparison.OrdinalIgnoreCase)) { guiMainMenu.SetKeyBindingNames(); continue; }
                if (string.Equals(cmd, "systemCvars", StringComparison.OrdinalIgnoreCase)) { guiActive.HandleNamedEvent("cvar read render"); guiActive.HandleNamedEvent("cvar read sound"); continue; }
                // we can't do this from inside the HandleMainMenuCommands code, otherwise the message box stuff gets confused
                if (string.Equals(cmd, "SetCDKey", StringComparison.OrdinalIgnoreCase)) { cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "promptKey\n"); continue; }
                if (string.Equals(cmd, "CheckUpdate", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.SendVersionCheck(); continue; }
                if (string.Equals(cmd, "CheckUpdate2", StringComparison.OrdinalIgnoreCase)) { AsyncNetwork.client.SendVersionCheck(true); continue; }
                if (string.Equals(cmd, "checkKeys", StringComparison.OrdinalIgnoreCase))
                {
#if ID_ENFORCE_KEY
                    // not a strict check so you silently auth in the background without bugging the user
                    if (!session.CDKeysAreValid(false))
                    {
                        cmdSystem.BufferCommandText(CMD_EXEC.NOW, "promptKey force");
                        cmdSystem.ExecuteCommandBuffer();
                    }
#endif
                    continue;
                }
                // triggered from mainmenu or mpmain
                if (string.Equals(cmd, "punkbuster", StringComparison.OrdinalIgnoreCase))
                {
                    var vcmd = args.Count - icmd >= 1 ? args[icmd++] : null;
                    // filtering PB based on enabled/disabled
                    AsyncNetwork.client.serverList.ApplyFilter();
                    SetPbMenuGuiVars();
                    continue;
                }
            }
        }

        // Executes any commands returned by the gui
        public void HandleChatMenuCommands(string menuCommand)
        {
            // execute the command from the menu
            CmdArgs args = new();
            args.TokenizeString(menuCommand, false);
            for (var i = 0; i < args.Count;)
            {
                var cmd = args[i++];

                if (string.Equals(cmd, "chatactive", StringComparison.OrdinalIgnoreCase)) {                    /*chat.chatMode = CHAT_GLOBAL;*/                    continue; }
                if (string.Equals(cmd, "chatabort", StringComparison.OrdinalIgnoreCase)) {                    /*chat.chatMode = CHAT_NONE;*/                    continue; }
                if (string.Equals(cmd, "netready", StringComparison.OrdinalIgnoreCase)) { var b = cvarSystem.GetCVarBool("ui_ready"); cvarSystem.SetCVarBool("ui_ready", !b); continue; }
                if (string.Equals(cmd, "netstart", StringComparison.OrdinalIgnoreCase)) { cmdSystem.BufferCommandText(CMD_EXEC.NOW, "netcommand start\n"); continue; }
            }
        }

        // Executes any commands returned by the gui
        public void HandleInGameCommands(string menuCommand)
        {
            // execute the command from the menu
            CmdArgs args = new();
            args.TokenizeString(menuCommand, false);
            var cmd = args[0];
            if (string.Equals(cmd, "close", StringComparison.OrdinalIgnoreCase))
                if (guiActive != null)
                {
                    SysEvent ev = default;
                    ev.evType = SE.NONE;
                    guiActive.HandleEvent(ev, com_frameTime);
                    guiActive.Activate(false, com_frameTime);
                }
        }

        public void DispatchCommand(IUserInterface gui, string menuCommand, bool doIngame = true)
        {
            if (gui == null) gui = guiActive;
            if (gui == guiMainMenu) { HandleMainMenuCommands(menuCommand); return; }
            else if (gui == guiIntro) HandleIntroMenuCommands(menuCommand);
            else if (gui == guiMsg) HandleMsgCommands(menuCommand);
            else if (gui == guiTakeNotes) HandleNoteCommands(menuCommand);
            else if (gui == guiRestartMenu) HandleRestartMenuCommands(menuCommand);
            else if (game != null && guiActive != null && guiActive.State.GetBool("gameDraw"))
            {
                var cmd = game.HandleGuiCommands(menuCommand);
                if (cmd == null) guiActive = null;
                else if (string.Equals(cmd, "main", StringComparison.OrdinalIgnoreCase)) StartMenu();
                // pipe the GUI sound commands not handled by the game to the main menu code
                else if (cmd.StartsWith("sound ")) HandleMainMenuCommands(cmd);
            }
            else if (guiHandle != null)
            {
                if (guiHandle(menuCommand) != null) return;
            }
            else if (!doIngame) common.DPrintf($"SessionLocal::DispatchCommand: no dispatch found for command '{menuCommand}'\n");

            if (doIngame) HandleInGameCommands(menuCommand);
        }

        // Executes any commands returned by the gui
        public void MenuEvent(SysEvent ev)
        {
            if (guiActive == null) return;
            var menuCommand = guiActive.HandleEvent(ev, com_frameTime);
            if (string.IsNullOrEmpty(menuCommand))
            {
                // If the menu didn't handle the event, and it's a key down event for an F key, run the bind
                if (ev.evType == SE.KEY && ev.evValue2 == 1 && ev.evValue >= (int)K_F1 && ev.evValue <= (int)K_F12) KeyInput.ExecKeyBinding(ev.evValue);
                return;
            }

            DispatchCommand(guiActive, menuCommand);
        }

        public override void GuiFrameEvents()
        {
            // stop generating move and button commands when a local console or menu is active running here so SP, async networking and no game all go through it
            usercmdGen.InhibitUsercmd(INHIBIT.SESSION, console.Active || guiActive != null);

            var gui = guiTest ?? guiActive;
            if (gui == null) return;

            SysEvent ev = default;
            ev.evType = SE.NONE;
            var cmd = gui.HandleEvent(ev, com_frameTime);
            if (!string.IsNullOrEmpty(cmd)) DispatchCommand(guiActive, cmd);
        }

        bool BoxDialogSanityCheck()
        {
            if (!common.IsInitialized) { common.DPrintf("message box sanity check: !common.IsInitialized()\n"); return false; }
            if (guiMsg == null) return false;
            if (guiMsgRestore != null) { common.DPrintf("message box sanity check: recursed\n"); return false; }
            if (cvarSystem.GetCVarInteger("net_serverDedicated") != 0) { common.DPrintf("message box sanity check: not compatible with dedicated server\n"); return false; }
            return true;
        }

        public override string MessageBox(MSG type, string message, string title = null, bool wait = false, string fire_yes = null, string fire_no = null, bool network = false)
        {
            common.DPrintf($"MessageBox: {title} - {message}\n");
            if (!BoxDialogSanityCheck()) return null;

            guiMsg.SetStateString("title", title ?? "");
            guiMsg.SetStateString("message", message ?? "");
            if (type == MSG.WAIT) { guiMsg.SetStateString("visible_msgbox", "0"); guiMsg.SetStateString("visible_waitbox", "1"); }
            else { guiMsg.SetStateString("visible_msgbox", "1"); guiMsg.SetStateString("visible_waitbox", "0"); }
            guiMsg.SetStateString("visible_entry", "0");
            guiMsg.SetStateString("visible_cdkey", "0");
            switch (type)
            {
                case MSG.INFO:
                    guiMsg.SetStateString("mid", "");
                    guiMsg.SetStateString("visible_mid", "0");
                    guiMsg.SetStateString("visible_left", "0");
                    guiMsg.SetStateString("visible_right", "0");
                    break;
                case MSG.OK:
                    guiMsg.SetStateString("mid", common.LanguageDictGetString("#str_04339"));
                    guiMsg.SetStateString("visible_mid", "1");
                    guiMsg.SetStateString("visible_left", "0");
                    guiMsg.SetStateString("visible_right", "0");
                    break;
                case MSG.ABORT:
                    guiMsg.SetStateString("mid", common.LanguageDictGetString("#str_04340"));
                    guiMsg.SetStateString("visible_mid", "1");
                    guiMsg.SetStateString("visible_left", "0");
                    guiMsg.SetStateString("visible_right", "0");
                    break;
                case MSG.OKCANCEL:
                    guiMsg.SetStateString("left", common.LanguageDictGetString("#str_04339"));
                    guiMsg.SetStateString("right", common.LanguageDictGetString("#str_04340"));
                    guiMsg.SetStateString("visible_mid", "0");
                    guiMsg.SetStateString("visible_left", "1");
                    guiMsg.SetStateString("visible_right", "1");
                    break;
                case MSG.YESNO:
                    guiMsg.SetStateString("left", common.LanguageDictGetString("#str_04341"));
                    guiMsg.SetStateString("right", common.LanguageDictGetString("#str_04342"));
                    guiMsg.SetStateString("visible_mid", "0");
                    guiMsg.SetStateString("visible_left", "1");
                    guiMsg.SetStateString("visible_right", "1");
                    break;
                case MSG.PROMPT:
                    guiMsg.SetStateString("left", common.LanguageDictGetString("#str_04339"));
                    guiMsg.SetStateString("right", common.LanguageDictGetString("#str_04340"));
                    guiMsg.SetStateString("visible_mid", "0");
                    guiMsg.SetStateString("visible_left", "1");
                    guiMsg.SetStateString("visible_right", "1");
                    guiMsg.SetStateString("visible_entry", "1");
                    guiMsg.HandleNamedEvent("Prompt");
                    break;
                case MSG.CDKEY:
                    guiMsg.SetStateString("left", common.LanguageDictGetString("#str_04339"));
                    guiMsg.SetStateString("right", common.LanguageDictGetString("#str_04340"));
                    guiMsg.SetStateString("visible_msgbox", "0");
                    guiMsg.SetStateString("visible_cdkey", "1");
                    guiMsg.SetStateString("visible_hasxp", fileSystem.HasD3XP ? "1" : "0");
                    // the current cdkey / xpkey values may have bad/random data in them it's best to avoid printing them completely, unless the key is good
                    if (cdkey_state == CDKEY.OK) { guiMsg.SetStateString("str_cdkey", cdkey); guiMsg.SetStateString("visible_cdchk", "0"); }
                    else { guiMsg.SetStateString("str_cdkey", ""); guiMsg.SetStateString("visible_cdchk", "1"); }
                    guiMsg.SetStateString("str_cdchk", "");
                    if (xpkey_state == CDKEY.OK) { guiMsg.SetStateString("str_xpkey", xpkey); guiMsg.SetStateString("visible_xpchk", "0"); }
                    else { guiMsg.SetStateString("str_xpkey", ""); guiMsg.SetStateString("visible_xpchk", "1"); }
                    guiMsg.SetStateString("str_xpchk", "");
                    guiMsg.HandleNamedEvent("CDKey");
                    break;
                case MSG.WAIT: break;
                default: common.Printf("SessionLocal::MessageBox: unknown msg box type\n"); break;
            }
            msgFireBack[0] = fire_yes ?? "";
            msgFireBack[1] = fire_no ?? "";
            guiMsgRestore = guiActive;
            guiActive = guiMsg;
            guiMsg.SetCursor(325, 290);
            guiActive.Activate(true, com_frameTime);
            msgRunning = true;
            msgRetIndex = -1;

            if (wait)
            {
                // play one frame ignoring events so we don't get confused by parasite button releases
                msgIgnoreButtons = true;
                common.GUIFrame(true, network);
                msgIgnoreButtons = false;
                while (msgRunning) common.GUIFrame(true, network);
                // MSG_WAIT and other StopBox calls
                if (msgRetIndex < 0) return null;
                var state = guiMsg.State;
                if (type == MSG.PROMPT)
                {
                    if (msgRetIndex == 0) { state.TryGetString("str_entry", "", out msgFireBack[0]); return msgFireBack[0]; }
                    else return null;
                }
                else if (type == MSG.CDKEY)
                {
                    // the visible_ values distinguish looking at a valid key, or editing it
                    if (msgRetIndex == 0) { msgFireBack[0] = $"{state.GetString("visible_cdchk"),1};{state.GetString("str_cdkey"),16};{state.GetString("str_cdchk"),2};{state.GetString("visible_xpchk"),1};{state.GetString("str_xpkey"),16};{state.GetString("str_xpchk"),2}"; return msgFireBack[0]; }
                    else return null;
                }
                else return msgFireBack[msgRetIndex];
            }
            return null;
        }

        public override void DownloadProgressBox(BackgroundDownload bgl, string title, int progress_start = 0, int progress_end = 100)
        {
            int dlnow = 0, dltotal = 0, lapsed;
            var startTime = SysW.Milliseconds;
            string sNow, sTotal, sBW, sETA, sMsg;

            if (!BoxDialogSanityCheck()) return;

            guiMsg.SetStateString("visible_msgbox", "1");
            guiMsg.SetStateString("visible_waitbox", "0");

            guiMsg.SetStateString("visible_entry", "0");
            guiMsg.SetStateString("visible_cdkey", "0");

            guiMsg.SetStateString("mid", "Cancel");
            guiMsg.SetStateString("visible_mid", "1");
            guiMsg.SetStateString("visible_left", "0");
            guiMsg.SetStateString("visible_right", "0");

            guiMsg.SetStateString("title", title);
            guiMsg.SetStateString("message", "Connecting..");

            guiMsgRestore = guiActive;
            guiActive = guiMsg;
            msgRunning = true;

            while (true)
            {
                while (msgRunning)
                {
                    common.GUIFrame(true, false);
                    if (bgl.completed) { guiActive = guiMsgRestore; guiMsgRestore = null; return; }
                    else if (bgl.url.dltotal != dltotal || bgl.url.dlnow != dlnow)
                    {
                        dltotal = bgl.url.dltotal;
                        dlnow = bgl.url.dlnow;
                        lapsed = SysW.Milliseconds - startTime;
                        stringX.BestUnit(out sNow, "{0:.2}", dlnow, stringX.MEASURE.SIZE);
                        if (lapsed > 2000) stringX.BestUnit(out sBW, "{0:.1}", 1000f * dlnow / lapsed, stringX.MEASURE.BANDWIDTH);
                        else sBW = "-- KB/s";
                        if (dltotal != 0)
                        {
                            stringX.BestUnit(out sTotal, "{0:.2}", dltotal, stringX.MEASURE.SIZE);
                            if (lapsed < 2000) sMsg = $"{sNow} / {sTotal}";
                            else { sETA = $"{((float)dltotal / (float)dlnow - 1f) * lapsed / 1000} sec"; sMsg = $"{sNow} / {sTotal} ( {sBW} - {sETA} )"; }
                        }
                        else sMsg = lapsed < 2000 ? sNow : $"{sNow} - {sBW}";
                        guiMsg.SetStateString("progress", dltotal != 0 ? (progress_start + dlnow * (progress_end - progress_start) / dltotal).ToString() : "0");
                        guiMsg.SetStateString("message", sMsg);
                    }
                }
                // abort was used - tell the downloader and wait till final stop
                bgl.url.status = DL.ABORTING;
                guiMsg.SetStateString("title", "Aborting..");
                guiMsg.SetStateString("visible_mid", "0");
                // continue looping
                guiMsgRestore = guiActive;
                guiActive = guiMsg;
                msgRunning = true;
            }
        }

        public override void StopBox()
        {
            if (guiActive == guiMsg) HandleMsgCommands("stop");
        }

        public void HandleMsgCommands(string menuCommand)
        {
            Debug.Assert(guiActive == guiMsg);
            // "stop" works even on first frame
            if (string.Equals(menuCommand, "stop", StringComparison.OrdinalIgnoreCase))
            {
                // force hiding the current dialog
                guiActive = guiMsgRestore;
                guiMsgRestore = null;
                msgRunning = false;
                msgRetIndex = -1;
            }
            if (msgIgnoreButtons) { common.DPrintf("MessageBox HandleMsgCommands 1st frame ignore\n"); return; }
            if (string.Equals(menuCommand, "mid", StringComparison.OrdinalIgnoreCase) || string.Equals(menuCommand, "left", StringComparison.OrdinalIgnoreCase))
            {
                guiActive = guiMsgRestore;
                guiMsgRestore = null;
                msgRunning = false;
                msgRetIndex = 0;
                DispatchCommand(guiActive, msgFireBack[0]);
            }
            else if (string.Equals(menuCommand, "right", StringComparison.OrdinalIgnoreCase))
            {
                guiActive = guiMsgRestore;
                guiMsgRestore = null;
                msgRunning = false;
                msgRetIndex = 1;
                DispatchCommand(guiActive, msgFireBack[1]);
            }
        }

        const string NOTEDATFILE = "C:/notenumber.dat";
        public void HandleNoteCommands(string menuCommand)
        {
            guiActive = null;

            if (string.Equals(menuCommand, "note", StringComparison.OrdinalIgnoreCase) && mapSpawned)
            {
                VFile file = null;
                for (var tries = 0; tries < 10; tries++)
                {
                    file = fileSystem.OpenExplicitFileRead(NOTEDATFILE);
                    if (file != null) break;
                    SysW.Sleep(500);
                }
                var noteNumber = 1000;
                if (file != null) { file.ReadInt(out noteNumber); fileSystem.CloseFile(file); }

                int i; string noteNum, shotName, workName, fileName = "viewnotes/";
                List<string> fileList = new();

                string severity = null;

                var p = guiTakeNotes.State.GetString("notefile");
                if (string.IsNullOrEmpty(p)) p = cvarSystem.GetCVarString("ui_name");

                var extended = guiTakeNotes.State.GetBool("extended");
                if (extended)
                {
                    severity = guiTakeNotes.State.GetInt("severity") == 1 ? "WishList_Viewnotes/" : "MustFix_Viewnotes/";
                    fileName += severity;

                    var mapDecl = declManager.FindType(DECL.ENTITYDEF, mapSpawnData.serverInfo.GetString("si_map"), false);
                    var mapInfo = (DeclEntityDef)mapDecl;

                    if (mapInfo != null) fileName = $"{fileName}{mapInfo.dict.GetString("devname")}";
                    else fileName = PathX.StripFileExtension($"{fileName}{mapSpawnData.serverInfo.GetString("si_map")}");

                    var count = guiTakeNotes.State.GetInt("person_numsel");
                    if (count == 0) fileList.Add($"{fileName}/Nobody");
                    else
                        for (i = 0; i < count; i++)
                        {
                            var person = guiTakeNotes.State.GetInt($"person_sel_{i}");
                            workName = $"{fileName}/{guiTakeNotes.State.GetString($"person_item_{person}", "Nobody")}";
                            fileList.Add(workName);
                        }
                }
                else
                {
                    fileName = PathX.StripFileExtension($"{fileName}maps/{mapSpawnData.serverInfo.GetString("si_map")}");
                    fileList.Add(fileName);
                }

                var con = cvarSystem.GetCVarBool("con_noPrint");
                cvarSystem.SetCVarBool("con_noPrint", true);
                for (i = 0; i < fileList.Count; i++)
                {
                    workName = $"{fileList[i]}/{p}";
                    var workNote = noteNumber;
                    R.ScreenshotFilename(ref workNote, workName, out shotName);
                    noteNum = PathX.StripFileExtension(PathX.StripPath(shotName));
                    if (!string.IsNullOrEmpty(severity)) workName = $"{severity}viewNotes";
                    cmdSystem.BufferCommandText(CMD_EXEC.NOW, $"recordViewNotes \"{workName}\" \"{noteNum}\" \"{guiTakeNotes.State.GetString("note")}\"\n");
                    cmdSystem.ExecuteCommandBuffer();

                    UpdateScreen();
                    renderSystem.TakeScreenshot(renderSystem.ScreenWidth, renderSystem.ScreenHeight, shotName, 1, null);
                }
                noteNumber++;

                for (var tries = 0; tries < 10; tries++)
                {
                    file = fileSystem.OpenExplicitFileWrite("p:/viewnotes/notenumber.dat");
                    if (file != null) break;
                    SysW.Sleep(500);
                }
                if (file != null)
                {
                    file.WriteInt(noteNumber);
                    fileSystem.CloseFile(file);
                }

                cmdSystem.BufferCommandText(CMD_EXEC.NOW, "closeViewNotes\n");
                cvarSystem.SetCVarBool("con_noPrint", con);
            }
        }

        public override void SetCDKeyGuiVars()
        {
            if (guiMainMenu == null) return;
            guiMainMenu.SetStateString("str_d3key_state", common.LanguageDictGetString($"#str_071{(86 + cdkey_state)}"));
            guiMainMenu.SetStateString("str_xpkey_state", common.LanguageDictGetString($"#str_071{(86 + xpkey_state)}"));
        }
    }
}
