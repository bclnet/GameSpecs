using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class UserInterfaceLocal : IUserInterface
    {
        public static UserInterfaceManagerLocal uiManagerLocal = new();
        bool active;
        bool loading;
        internal bool interactive;
        bool uniqued;
        Dictionary<string, string> state;
        internal Window desktop;
        Window bindHandler;
        string source;
        string activateStr;
        string pendingCmd;
        string returnCmd;
        DateTime timeStamp;
        float cursorX;
        float cursorY;
        int time;
        int refs;

        public UserInterfaceLocal()
        {
            cursorX = cursorY = 0f;
            desktop = null;
            loading = false;
            active = false;
            interactive = false;
            uniqued = false;
            bindHandler = null;
            //so the reg eval in gui parsing doesn't get bogus values
            time = 0;
            refs = 1;
        }

        public string Name
            => source;

        public string Comment
            => desktop != null ? desktop.Comment : "";

        public bool IsInteractive
            => interactive;

        public bool InitFromFile(string qpath, bool rebuild = true, bool cache = true)
        {
            if (!string.IsNullOrEmpty(qpath)) return false; // FIXME: Memory leak!!

            loading = true;

            if (rebuild) desktop = new Window(this);
            else if (desktop == null) desktop = new Window(this);

            source = qpath;
            state["text"] = "Test Text!";

            var src = new Parser(LEXFL.NOFATALERRORS | LEXFL.NOSTRINGCONCAT | LEXFL.ALLOWMULTICHARLITERALS | LEXFL.ALLOWBACKSLASHSTRINGCONCAT);

            // Load the timestamp so reload guis will work correctly
            fileSystem.ReadFile(qpath, out timeStamp);

            src.LoadFile(qpath);

            if (src.IsLoaded)
            {
                while (src.ReadToken(out var token))
                    if (string.Equals(token, "windowDef", StringComparison.OrdinalIgnoreCase))
                    {
                        desktop.DC = uiManagerLocal.dc;
                        if (desktop.Parse(src, rebuild))
                        {
                            desktop.Flags = Window.WIN_DESKTOP;
                            desktop.FixupParms();
                        }
                        continue;
                    }

                state["name"] = qpath;
            }
            else
            {
                desktop.DC = uiManagerLocal.dc;
                desktop.Flags = Window.WIN_DESKTOP;
                desktop.name = "Desktop";
                desktop.text = $"Invalid GUI: {qpath}";
                desktop.rect = new Rectangle(0f, 0f, 640f, 480f);
                desktop.drawRect = desktop.rect;
                desktop.foreColor = new Vector4(1f, 1f, 1f, 1f);
                desktop.backColor = new Vector4(0f, 0f, 0f, 1f);
                desktop.SetupFromState();
                common.Warning("Couldn't load gui: '%s'", qpath);
                loading = false;
                return false;
            }

            interactive = desktop.Interactive;

            if (!uiManagerLocal.guis.Contains(this)) uiManagerLocal.guis.Add(this);

            loading = false;

            return true;
        }

        const float HandleEvent_virtualAspectRatio = (float)DeviceContext.VIRTUAL_WIDTH / (float)DeviceContext.VIRTUAL_HEIGHT; // 4:3
        public string HandleEvent(in SysEvent ev, int time, Action<bool> updateVisuals)
        {
            this.time = time;

            if (bindHandler != null && ev.evType == SE.KEY && ev.evValue2 == 1)
            {
                var ret = bindHandler.HandleEvent(ev, updateVisuals);
                bindHandler = null;
                return ret;
            }

            if (ev.evType == SE.MOUSE)
            {
                if (desktop == null || (desktop.Flags & Window.WIN_MENUGUI) != 0)
                {
                    // DG: this is a fullscreen GUI, scale the mousedelta added to cursorX/Y by 640/w, because the GUI pretends that everything is 640x480
                    //     even if the actual resolution is higher => mouse moved too fast
                    var w = renderSystem.ScreenWidth;
                    var h = renderSystem.ScreenHeight;
                    if (w <= 0f || h <= 0f)
                    {
                        w = DeviceContext.VIRTUAL_WIDTH;
                        h = DeviceContext.VIRTUAL_HEIGHT;
                    }

                    if (R.r_scaleMenusTo43.Bool)
                    {
                        // in case we're scaling menus to 4:3, we need to take that into account when scaling the mouse events.
                        // no, we can't just call uiManagerLocal.dc.GetFixScaleForMenu() or sth like that,
                        // because when we're here dc.SetMenuScaleFix(true) is not active and it'd just return (1, 1)!
                        var aspectRatio = (float)(w / h);
                        // widescreen (4:3 is 1.333 3:2 is 1.5, 16:10 is 1.6, 16:9 is 1.7778) => we need to modify cursorX scaling, by modifying w
                        if (aspectRatio > 1.4f) w *= (int)(HandleEvent_virtualAspectRatio / aspectRatio);
                        // portrait-mode, "thinner" than 5:4 (which is 1.25) => we need to scale cursorY via h
                        else if (aspectRatio < 1.24f) h *= (int)(aspectRatio / HandleEvent_virtualAspectRatio);
                    }

                    cursorX += ev.evValue * ((float)DeviceContext.VIRTUAL_WIDTH / w);
                    cursorY += ev.evValue2 * ((float)DeviceContext.VIRTUAL_HEIGHT / h);
                }
                else
                {
                    // not a fullscreen GUI but some ingame thing - no scaling needed
                    cursorX += ev.evValue;
                    cursorY += ev.evValue2;
                }

                if (cursorX < 0) cursorX = 0;
                if (cursorY < 0) cursorY = 0;
            }

            return desktop != null ? desktop.HandleEvent(ev, updateVisuals) : "";
        }

        public void HandleNamedEvent(string namedEvent)
            => desktop.RunNamedEvent(namedEvent);

        public void Redraw(int time)
        {
            if (R.r_skipGuiShaders.Integer > 5) return;
            if (!loading && desktop != null)
            {
                this.time = time;
                uiManagerLocal.dc.PushClipRect(uiManagerLocal.screenRect);
                desktop.Redraw(0, 0);
                uiManagerLocal.dc.PopClipRect();
            }
        }

        public void DrawCursor()
            => uiManagerLocal.dc.DrawCursor(ref cursorX, ref cursorY, desktop == null || (desktop.Flags & Window.WIN_MENUGUI) != 0 ? 32f : 64f);

        public Dictionary<string, string> State => state;
        public void DeleteStateVar(string varName) => state.Remove(varName);
        public void SetStateString(string varName, string value) => state[varName] = value;
        public void SetStateBool(string varName, bool value) => state.SetBool(varName, value);
        public void SetStateInt(string varName, int value) => state.SetInt(varName, value);
        public void SetStateFloat(string varName, float value) => state.SetFloat(varName, value);

        // Gets a gui state variable
        public string GetStateString(string varName, string defaultString = "") => state.GetString(varName, defaultString);
        public bool GetStateBool(string varName, string defaultString = "0") => state.GetBool(varName, defaultString);
        public int GetStateInt(string varName, string defaultString = "0") => state.GetInt(varName, defaultString);
        public float GetStateFloat(string varName, string defaultString = "0") => state.GetFloat(varName, defaultString);

        public void StateChanged(int time, bool redraw)
        {
            this.time = time;
            if (desktop != null)
            {
                // DG: little hack: allow game DLLs to do
                // ui.SetStateBool("scaleto43", true);
                // ui.StateChanged(gameLocal.time);
                // so we can force cursors.gui (crosshair) to be scaled, for example
                var scaleTo43 = false;
                if (state.TryGetBool("scaleto43", "0", out scaleTo43))
                    if (scaleTo43) desktop.Flags = Window.WIN_SCALETO43;
                    else desktop.ClearFlag(Window.WIN_SCALETO43);

                desktop.StateChanged(redraw);
            }
            interactive = !state.GetBool("noninteractive") && desktop != null && desktop.Interactive;
        }

        public string Activate(bool activate, int time)
        {
            this.time = time;
            active = activate;
            if (desktop != null)
            {
                activateStr = "";
                desktop.Activate(activate, ref activateStr);
                return activateStr;
            }
            return "";
        }

        public void Trigger(int time)
        {
            this.time = time;
            desktop?.Trigger();
        }

        public void ReadFromDemoFile(VFileDemo f)
        {
            f.ReadDict(state);
            source = state.GetString("name");

            if (desktop == null)
            {
                f.Log("creating new gui\n");
                desktop = new Window(this)
                {
                    Flags = Window.WIN_DESKTOP,
                    DC = uiManagerLocal.dc
                };
                desktop.ReadFromDemoFile(f);
            }
            else
            {
                f.Log("re-using gui\n");
                desktop.ReadFromDemoFile(f, false);
            }

            f.ReadFloat(out cursorX); f.ReadFloat(out cursorY);

            var add = true;
            var c = uiManagerLocal.demoGuis.Count;
            for (var i = 0; i < c; i++)
                if (uiManagerLocal.demoGuis[i] == this) { add = false; break; }

            if (add) uiManagerLocal.demoGuis.Add(this);
        }

        public void WriteToDemoFile(VFileDemo f)
        {
            f.WriteDict(state);
            desktop?.WriteToDemoFile(f);

            f.WriteFloat(cursorX);
            f.WriteFloat(cursorY);
        }

        public bool WriteToSaveGame(VFile savefile)
        {
            int len;

            var num = state.Count;
            savefile.Write(num);

            foreach (var kv in state)
            {
                len = kv.Key.Length; savefile.Write(len); savefile.WriteASCII(kv.Key, len);
                len = kv.Value.Length; savefile.Write(len); savefile.WriteASCII(kv.Value, len);
            }

            savefile.Write(active);
            savefile.Write(interactive);
            savefile.Write(uniqued);
            savefile.Write(time);
            len = activateStr.Length; savefile.Write(len); savefile.WriteASCII(activateStr, len);
            len = pendingCmd.Length; savefile.Write(len); savefile.WriteASCII(pendingCmd, len);
            len = returnCmd.Length; savefile.Write(len); savefile.WriteASCII(returnCmd, len);

            savefile.Write(cursorX); savefile.Write(cursorY);

            desktop.WriteToSaveGame(savefile);

            return true;
        }

        public bool ReadFromSaveGame(VFile savefile)
        {
            int num, i, len;

            savefile.Read(out num);

            state.Clear();
            for (i = 0; i < num; i++)
            {
                savefile.Read(out len); savefile.ReadASCII(out var key, len);
                savefile.Read(out len); savefile.ReadASCII(out var value, len);
                state[key] = value;
            }

            savefile.Read(out active);
            savefile.Read(out interactive);
            savefile.Read(out uniqued);
            savefile.Read(out time);

            savefile.Read(out len); savefile.ReadASCII(out activateStr, len);
            savefile.Read(out len); savefile.ReadASCII(out pendingCmd, len);
            savefile.Read(out len); savefile.ReadASCII(out returnCmd, len);

            savefile.Read(out cursorX); savefile.Read(out cursorY);

            desktop.ReadFromSaveGame(savefile);

            return true;
        }

        public int Size => 0;

        public void RecurseSetKeyBindingNames(Window window)
        {
            var v = window.GetWinVarByName("bind");
            if (v != null) SetStateString(v.Name, KeyInput.KeysFromBinding(v.Name));
            var i = 0;
            while (i < window.ChildCount)
            {
                var next = window.GetChild(i);
                if (next == null) RecurseSetKeyBindingNames(next);
                i++;
            }
        }

        public void SetKeyBindingNames()
        {
            if (desktop == null) return;
            // walk the windows
            RecurseSetKeyBindingNames(desktop);
        }

        public bool IsUniqued
        {
            get => uniqued;
            set => uniqued = value;
        }

        public void SetCursor(float x, float y)
        {
            cursorX = x;
            cursorY = y;
        }

        public float CursorX => cursorX;
        public float CursorY => cursorY;

        public Dictionary<string, string> StateDict => state;

        public string SourceFile => source;
        public DateTime TimeStamp => timeStamp;

        public Window Desktop => desktop;
        public void SetBindHandler(Window win) => bindHandler = win;
        public bool Active => active;
        public int Time
        {
            get => time;
            set => time = value;
        }

        public void ClearRefs() => refs = 0;
        public void AddRef() => refs++;
        public int Refs => refs;

        public ref string PendingCmd => ref pendingCmd;
        public ref string ReturnCmd => ref returnCmd;
    }

    public class UserInterfaceManagerLocal : IUserInterfaceManager
    {
        internal Rectangle screenRect;
        internal DeviceContext dc;
        internal List<UserInterfaceLocal> guis = new();
        internal List<UserInterfaceLocal> demoGuis = new();

        public void Init()
        {
            screenRect = new Rectangle(0f, 0f, 640f, 480f);
            dc.Init();
        }

        public void Shutdown()
        {
            guis.Clear();
            demoGuis.Clear();
            dc.Shutdown();
        }

        public void Touch(string name)
        {
            var gui = Alloc();
            gui.InitFromFile(name);
        }

        public void WritePrecacheCommands(VFile f)
        {
            var c = guis.Count;
            for (var i = 0; i < c; i++)
            {
                var str = $"touchGui {guis[i].Name}\n";
                common.Printf(str);
                f.Printf(str);
            }
        }

        public void SetSize(float width, float height)
            => dc.SetSize(width, height);

        public void BeginLevelLoad()
        {
            var c = guis.Count;
            for (var i = 0; i < c; i++) if ((guis[i].Desktop.Flags & Window.WIN_MENUGUI) == 0) guis[i].ClearRefs();
        }

        public void EndLevelLoad()
        {
            var c = guis.Count;
            for (var i = 0; i < c; i++)
                if (guis[i].Refs == 0)
                {
                    //common.Printf($"purging {guis[i].SourceFile}.\n");
                    // use this to make sure no materials still reference this gui
                    var remove = true;
                    for (var j = 0; j < declManager.GetNumDecls(DECL.MATERIAL); j++)
                    {
                        var material = (Material)declManager.DeclByIndex(DECL.MATERIAL, j, false);
                        if (material.GlobalGui == guis[i]) { remove = false; break; }
                    }
                    if (remove) { guis.RemoveAt(i); i--; c--; }
                }
        }

        public void Reload(bool all)
        {
            var c = guis.Count;
            for (var i = 0; i < c; i++)
            {
                if (!all)
                {
                    fileSystem.ReadFile(guis[i].SourceFile, out var ts);
                    if (ts <= guis[i].TimeStamp) continue;
                }

                guis[i].InitFromFile(guis[i].SourceFile);
                common.Printf($"reloading {guis[i].SourceFile}.\n");
            }
        }

        public void ListGuis()
        {
            common.Printf("\n   size   refs   name\n");
            var total = 0;
            var copies = 0;
            var unique = 0;
            var c = guis.Count;
            for (var i = 0; i < c; i++)
            {
                var gui = guis[i];
                var sz = gui.Size;
                var isUnique = guis[i].interactive;
                if (isUnique) unique++;
                else copies++;
                common.Printf($"{sz / 1024f:6.1}k {guis[i].Refs:4} ({(isUnique ? "unique" : "copy")}) {guis[i].SourceFile} ( {guis[i].desktop.NumTransitions} transitions )\n");
                total += sz;
            }
            common.Printf($"===========\n  {c} total Guis ( {copies} copies, {unique} unique ), {total / (1024f * 1024f):.2} total Mbytes");
        }

        public bool CheckGui(string qpath)
        {
            var file = fileSystem.OpenFileRead(qpath);
            if (file != null) { fileSystem.CloseFile(file); return true; }
            return false;
        }

        public IUserInterface Alloc()
            => new UserInterfaceLocal();

        public void DeAlloc(IUserInterface gui)
        {
            if (gui != null)
            {
                var c = guis.Count;
                for (var i = 0; i < c; i++) if (guis[i] == gui) { guis.RemoveAt(i); return; }
            }
        }

        public IUserInterface FindGui(string qpath, bool autoLoad = false, bool needInteractive = false, bool forceNotUnique = false)
        {
            var c = guis.Count;
            for (var i = 0; i < c; i++)
                if (string.Equals(guis[i].SourceFile, qpath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!forceNotUnique && (needInteractive || guis[i].IsInteractive)) break;
                    guis[i].AddRef();
                    return guis[i];
                }

            if (autoLoad)
            {
                var gui = Alloc();
                if (gui.InitFromFile(qpath)) { gui.IsUniqued = !forceNotUnique && needInteractive; return gui; }
            }
            return null;
        }

        public IUserInterface FindDemoGui(string qpath)
        {
            var c = demoGuis.Count;
            for (var i = 0; i < c; i++) if (string.Equals(demoGuis[i].SourceFile, qpath, StringComparison.OrdinalIgnoreCase)) return demoGuis[i];
            return null;
        }

        public IListGUI AllocListGUI()
            => new ListGUILocal();

        public void FreeListGUI(IListGUI listgui) { }
    }
}
