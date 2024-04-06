using System.NumericsX.OpenStack.Gngine.Framework.Async;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Framework.Framework;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
   unsafe partial class SessionLocal
    {
        static void Session_RescanSI_f(CmdArgs args)
        {
            sessLocal.mapSpawnData.serverInfo = cvarSystem.MoveCVarsToDict(CVAR.SERVERINFO);
            if (game != null && AsyncNetwork.server.IsActive) game.SetServerInfo(sessLocal.mapSpawnData.serverInfo);
        }

#if !ID_DEDICATED

        // Restart the server on a different map
        static void Session_Map_f(CmdArgs args)
        {
            var map = args[1];
            if (string.IsNullOrEmpty(map)) return;
            map = PathX.StripFileExtension(map);

            // make sure the level exists before trying to change, so that a typo at the server console won't end the game handle addon packs through reloadEngine
            var s = $"maps/{map}.map";
            var ff = fileSystem.FindFile(s, true);
            switch (ff)
            {
                case FIND.NO: common.Printf($"Can't find map {s}\n"); return;
                case FIND.ADDON:
                    common.Printf($"map {s} is in an addon pak - reloading\n");
                    CmdArgs rl_args = new();
                    rl_args.AppendArg("map");
                    rl_args.AppendArg(map);
                    cmdSystem.SetupReloadEngine(rl_args);
                    return;
                default: break;
            }

            cvarSystem.SetCVarBool("developer", false);
            sessLocal.StartNewGame(map, true);
        }

        // Restart the server on a different map in developer mode
        static void Session_DevMap_f(CmdArgs args)
        {
            var map = args[1];
            if (string.IsNullOrEmpty(map)) return;
            map = PathX.StripFileExtension(map);

            // make sure the level exists before trying to change, so that a typo at the server console won't end the game handle addon packs through reloadEngine
            var s = "maps/{map}.map";
            var ff = fileSystem.FindFile(s, true);
            switch (ff)
            {
                case FIND.NO: common.Printf($"Can't find map {s}\n"); return;
                case FIND.ADDON:
                    common.Printf($"map {s} is in an addon pak - reloading\n");
                    CmdArgs rl_args = new();
                    rl_args.AppendArg("devmap");
                    rl_args.AppendArg(map);
                    cmdSystem.SetupReloadEngine(rl_args);
                    return;
                default: break;
            }

            cvarSystem.SetCVarBool("developer", true);
            sessLocal.StartNewGame(map, true);
        }

        static void Session_TestMap_f(CmdArgs args)
        {
            var map = args[1];
            if (string.IsNullOrEmpty(map)) return;
            map = PathX.StripFileExtension(map);

            cmdSystem.BufferCommandText(CMD_EXEC.NOW, "disconnect");

            var s = $"dmap maps/{map}.map";
            cmdSystem.BufferCommandText(CMD_EXEC.NOW, s);

            s = $"devmap {map}";
            cmdSystem.BufferCommandText(CMD_EXEC.NOW, s);
        }

#endif

        static void Sess_WritePrecache_f(CmdArgs args)
        {
            if (args.Count != 2) { common.Printf("USAGE: writePrecache <execFile>\n"); return; }
            var str = args[1];
            str = PathX.DefaultFileExtension(str, ".cfg");
            var f = fileSystem.OpenFileWrite(str, "fs_configpath");
            declManager.WritePrecacheCommands(f);
            renderModelManager.WritePrecacheCommands(f);
            uiManager.WritePrecacheCommands(f);

            fileSystem.CloseFile(f);
        }

        static bool Session_PromptKey_f_recursed = false;
        static void Session_PromptKey_f(CmdArgs args)
        {
            string retkey;
            var valid = new bool[2];

            if (Session_PromptKey_f_recursed) { common.Warning("promptKey recursed - aborted"); return; }
            Session_PromptKey_f_recursed = true;

            do
            {
                // in case we're already waiting for an auth to come back to us ( may happen exceptionally )
                if (sessLocal.MaybeWaitOnCDKey() && sessLocal.CDKeysAreValid(true)) { Session_PromptKey_f_recursed = false; return; }

                // the auth server may have replied and set an error message, otherwise use a default
                var prompt_msg = sessLocal.AuthMsg;
                if (prompt_msg[0] == '\0') prompt_msg = common.LanguageDictGetString("#str_04308");
                retkey = sessLocal.MessageBox(MSG.CDKEY, prompt_msg, common.LanguageDictGetString("#str_04305"), true, null, null, true);
                if (retkey != null)
                {
                    if (sessLocal.CheckKey(retkey, false, valid))
                    {
                        // if all went right, then we may have sent an auth request to the master ( unless the prompt is used during a net connect )
                        var canExit = true;
                        // wait on auth reply, and got denied, prompt again
                        if (sessLocal.MaybeWaitOnCDKey() && !sessLocal.CDKeysAreValid(true))
                        {
                            // server says key is invalid - MaybeWaitOnCDKey was interrupted by a CDKeysAuthReply call, which has set the right error message the invalid keys have also been cleared in the process
                            sessLocal.MessageBox(MSG.OK, sessLocal.AuthMsg, common.LanguageDictGetString("#str_04310"), true, null, null, true);
                            canExit = false;
                        }
                        if (canExit)
                        {
                            // make sure that's saved on file
                            sessLocal.WriteCDKey();
                            sessLocal.MessageBox(MSG.OK, common.LanguageDictGetString("#str_04307"), common.LanguageDictGetString("#str_04305"), true, null, null, true);
                            break;
                        }
                    }
                    else
                    {
                        // offline check sees key invalid build a message about keys being wrong. do not attempt to change the current key state though (the keys may be valid, but user would have clicked on the dialog anyway, that kind of thing)
                        AsyncNetwork.BuildInvalidKeyMsg(out var msg, valid);
                        sessLocal.MessageBox(MSG.OK, msg, common.LanguageDictGetString("#str_04310"), true, null, null, true);
                    }
                }
                else if (args.Count == 2 && string.Equals(args[1], "force", StringComparison.OrdinalIgnoreCase))
                {
                    // cancelled in force mode
                    cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "quit\n");
                    cmdSystem.ExecuteCommandBuffer();
                }
            } while (retkey != null);
            Session_PromptKey_f_recursed = false;
        }

        static void Session_TestGUI_f(CmdArgs args)
            => sessLocal.TestGUI(args[1]);

        static string FindUnusedFileName(string format)
        {
            string filename = null;
            for (var i = 0; i < 999; i++)
            {
                filename = string.Format(format, i);
                var len = fileSystem.ReadFile(filename, out _, out _);
                if (len <= 0) return filename; // file doesn't exist
            }
            return filename;
        }

        static void Session_DemoShot_f(CmdArgs args)
        {
            if (args.Count != 2)
            {
                var filename = FindUnusedFileName("demos/shot%03i.demo");
                sessLocal.DemoShot(filename);
            }
            else sessLocal.DemoShot($"demos/shot_{args[1]}.demo");
        }

#if !ID_DEDICATED

        static void Session_RecordDemo_f(CmdArgs args)
        {
            if (args.Count != 2)
            {
                var filename = FindUnusedFileName("demos/demo%03i.demo");
                sessLocal.StartRecordingRenderDemo(filename);
            }
            else sessLocal.StartRecordingRenderDemo($"demos/{args[1]}.demo");
        }

        static void Session_CompressDemo_f(CmdArgs args)
        {
            if (args.Count == 2) sessLocal.CompressDemoFile("2", args[1]);
            else if (args.Count == 3) sessLocal.CompressDemoFile(args[2], args[1]);
            else common.Printf("use: CompressDemo <file> [scheme]\nscheme is the same as com_compressDemo, defaults to 2");
        }

        static void Session_StopRecordingDemo_f(CmdArgs args)
            => sessLocal.StopRecordingRenderDemo();

        static void Session_PlayDemo_f(CmdArgs args)
        {
            if (args.Count >= 2) sessLocal.StartPlayingRenderDemo($"demos/{args[1]}");
        }

        static void Session_TimeDemo_f(CmdArgs args)
        {
            if (args.Count >= 2) sessLocal.TimeRenderDemo($"demos/{args[1]}", args.Count > 2);
        }

        static void Session_TimeDemoQuit_f(CmdArgs args)
        {
            sessLocal.TimeRenderDemo($"demos/{args[1]}");
            // this allows hardware vendors to automate some testing
            if (sessLocal.timeDemo == TD.YES) sessLocal.timeDemo = TD.YES_THEN_QUIT;
        }

        static void Session_AVIDemo_f(CmdArgs args)
          => sessLocal.AVIRenderDemo($"demos/{args[1]}");

        static void Session_AVIGame_f(CmdArgs args)
            => sessLocal.AVIGame(args[1]);

        static void Session_AVICmdDemo_f(CmdArgs args)
            => sessLocal.AVICmdDemo(args[1]);

        static void Session_WriteCmdDemo_f(CmdArgs args)
        {
            if (args.Count == 1) sessLocal.WriteCmdDemo(FindUnusedFileName("demos/cmdDemo%03i.cdemo"));
            else if (args.Count == 2) sessLocal.WriteCmdDemo($"demos/{args[1]}.cdemo");
            else common.Printf("usage: writeCmdDemo [demoName]\n");
        }

        static void Session_PlayCmdDemo_f(CmdArgs args)
            => sessLocal.StartPlayingCmdDemo(args[1]);

        static void Session_TimeCmdDemo_f(CmdArgs args)
            => sessLocal.TimeCmdDemo(args[1]);

#endif

        static void Session_Disconnect_f(CmdArgs args)
        {
            sessLocal.Stop();
            sessLocal.StartMenu();
            soundSystem?.SetMute(false);
        }

#if !ID_DEDICATED

        static void Session_ExitCmdDemo_f(CmdArgs args)
        {
            if (sessLocal.cmdDemoFile == null)
            {
                common.Printf("not reading from a cmdDemo\n");
                return;
            }
            fileSystem.CloseFile(sessLocal.cmdDemoFile);
            common.Printf($"Command demo exited at logIndex {sessLocal.logIndex}\n");
            sessLocal.cmdDemoFile = null;
        }

#endif

        void LoadGame_f(CmdArgs args)
        {
            console.Close();
            sessLocal.LoadGame(args.Count < 2 || string.Equals(args[1], "quick", StringComparison.OrdinalIgnoreCase)
                ? common.LanguageDictGetString("#str_07178")
                : args[1]);
        }

        void SaveGame_f(CmdArgs args)
        {
            if (args.Count < 2 || string.Equals(args[1], "quick", StringComparison.OrdinalIgnoreCase))
            {
                var saveName = common.LanguageDictGetString("#str_07178");
                if (sessLocal.SaveGame(saveName)) common.Printf($"{saveName}\n");
            }
            else if (sessLocal.SaveGame(args[1])) common.Printf($"Saved {args[1]}\n");
        }

        void TakeViewNotes_f(CmdArgs args)
            => sessLocal.TakeNotes(args.Count > 1 ? args[1] : string.Empty);

        void TakeViewNotes2_f(CmdArgs args)
            => sessLocal.TakeNotes(args.Count > 1 ? args[1] : string.Empty, true);

        void Session_Hitch_f(CmdArgs args)
        {
            var sw = soundSystem.PlayingSoundWorld;
            if (sw != null) { soundSystem.SetMute(true); sw.Pause(); ISystem.EnterCriticalSection(); }
            SysW.Sleep(args.Count == 2 ? int.Parse(args[1]) : 100);
            if (sw != null) { ISystem.LeaveCriticalSection(); sw.UnPause(); soundSystem.SetMute(false); }
        }
    }
}