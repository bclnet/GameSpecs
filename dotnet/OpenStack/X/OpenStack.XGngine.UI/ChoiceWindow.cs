using System.Collections.Generic;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class ChoiceWindow : Window
    {
        int currentChoice;
        int choiceType;
        string latchedChoices;
        WinStr choicesStr;
        string latchedVals;
        WinStr choiceVals;
        List<string> choices = new();
        List<string> values = new();

        WinStr guiStr;
        WinStr cvarStr;
        CVar cvar;
        MultiWinVar updateStr;

        WinBool liveUpdate;
        WinStr updateGroup;

        new void CommonInit()
        {
            currentChoice = 0;
            choiceType = 0;
            cvar = null;
            liveUpdate = true;
            choices.Clear();
        }

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "choicetype", StringComparison.OrdinalIgnoreCase)) { choiceType = src.ParseInt(); return true; }
            if (string.Equals(name, "currentchoice", StringComparison.OrdinalIgnoreCase)) { currentChoice = src.ParseInt(); return true; }
            return base.ParseInternalVar(name, src);
        }

        public override WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
        {
            if (string.Equals(name, "choices", StringComparison.OrdinalIgnoreCase)) return choicesStr;
            if (string.Equals(name, "values", StringComparison.OrdinalIgnoreCase)) return choiceVals;
            if (string.Equals(name, "cvar", StringComparison.OrdinalIgnoreCase)) return cvarStr;
            if (string.Equals(name, "gui", StringComparison.OrdinalIgnoreCase)) return guiStr;
            if (string.Equals(name, "liveUpdate", StringComparison.OrdinalIgnoreCase)) return liveUpdate;
            if (string.Equals(name, "updateGroup", StringComparison.OrdinalIgnoreCase)) return updateGroup;
            return base.GetWinVarByName(name, winLookup, owner);
        }

        public ChoiceWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public ChoiceWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        public override string HandleEvent(SysEvent ev, Action<bool> updateVisuals)
        {
            Key key; bool runAction = false, runAction2 = false;

            if (ev.evType == SE.KEY)
            {
                key = (Key)ev.evValue;

                if (key == K_RIGHTARROW || key == K_KP_RIGHTARROW || key == K_MOUSE1)
                {
                    // never affects the state, but we want to execute script handlers anyway
                    if (ev.evValue2 == 0) { RunScript(SCRIPT.ON_ACTIONRELEASE); return cmd; }
                    currentChoice++;
                    if (currentChoice >= choices.Count) currentChoice = 0;
                    runAction = true;
                }

                if (key == K_LEFTARROW || key == K_KP_LEFTARROW || key == K_MOUSE2)
                {
                    // never affects the state, but we want to execute script handlers anyway
                    if (ev.evValue2 == 0) { RunScript(SCRIPT.ON_ACTIONRELEASE); return cmd; }
                    currentChoice--;
                    if (currentChoice < 0) currentChoice = choices.Count - 1;
                    runAction = true;
                }

                // is a key release with no action catch
                if (ev.evValue2 == 0) return "";
            }
            else if (ev.evType == SE.CHAR)
            {
                key = (Key)ev.evValue;

                var potentialChoice = -1;
                for (var i = 0; i < choices.Count; i++)
                    if (char.ToUpperInvariant((char)key) == char.ToUpperInvariant(choices[i][0]))
                        if (i < currentChoice && potentialChoice < 0) potentialChoice = i;
                        else if (i > currentChoice) { potentialChoice = -1; currentChoice = i; break; }
                if (potentialChoice >= 0) currentChoice = potentialChoice;

                runAction = true;
                runAction2 = true;
            }
            else return "";

            if (runAction) RunScript(SCRIPT.ON_ACTION);

            if (choiceType == 0) cvarStr.Set($"{currentChoice}");
            else if (values.Count != 0) cvarStr.Set(values[currentChoice]);
            else cvarStr.Set(choices[currentChoice]);

            UpdateVars(false);

            if (runAction2) RunScript(SCRIPT.ON_ACTIONRELEASE);

            return cmd;
        }

        public override void PostParse()
        {
            base.PostParse();

            // DG: HACKHACKFUCKINGUGLYHACK: overwrite resolution list to support more resolutions and widescreen and stuff.
            var injectResolutions = false;
            var injectCustomMode = true;

            // Mods that have their own video settings menu can tell dhewm3 to replace the "choices" and "values" entries in their choiceDef with the resolutions supported by
            // dhewm3 (and corresponding modes). So if we add new video modes to dhewm3, they'll automatically appear in the menu without changing the .gui
            // To enable this, the mod authors only need to add an "injectResolutions 1" entry to their resolution choiceDef. By default, the first entry will be "r_custom*"
            // for r_mode -1, which means "custom resolution, use r_customWidth and r_customHeight". If that entry shoud be disabled for the mod, just add another entry:
            // "injectCustomResolutionMode 0"
            var wv = GetWinVarByName("injectResolutions");
            if (wv != null)
            {
                var val = wv.ToString();
                if (val != "0")
                {
                    injectResolutions = true;
                    wv = GetWinVarByName("injectCustomResolutionMode");
                    if (wv != null)
                    {
                        val = wv.ToString();
                        if (val == "0") injectCustomMode = false;
                    }
                }
            }
            // always enable this for base/ and d3xp/ mainmenu.gui (like we did before)
            else if (Name == "OS2Primary" && cvarStr == "r_mode" && (string.Equals(Gui.SourceFile, "guis/demo_mainmenu.gui", StringComparison.OrdinalIgnoreCase) || string.Equals(Gui.SourceFile, "guis/mainmenu.gui", StringComparison.OrdinalIgnoreCase)))
                injectResolutions = true;

            if (injectResolutions)
            {
                choicesStr.Set(R_GetVidModeListString(injectCustomMode));
                choiceVals.Set(R_GetVidModeValsString(injectCustomMode));
            }
            // DG end

            UpdateChoicesAndVals();

            InitVars();
            UpdateChoice();
            UpdateVars(false);

            flags |= WIN_CANFOCUS;
        }

        public override void Draw(int time, float x, float y)
        {
            var color = foreColor;

            UpdateChoicesAndVals();
            UpdateChoice();

            // FIXME: It'd be really cool if textAlign worked, but a lot of the guis have it set wrong because it used to not work
            textAlign = 0;

            if (textShadow != 0)
            {
                var shadowText = choices[currentChoice];
                var shadowRect = new Rectangle(textRect);

                shadowText = stringX.RemoveColors(shadowText);
                shadowRect.x += textShadow;
                shadowRect.y += textShadow;

                dc.DrawText(shadowText, textScale, textAlign, colorBlack, shadowRect, false, -1);
            }

            if (hover && !noEvents && Contains(gui.CursorX, gui.CursorY)) color = hoverColor;
            else hover = false;
            if ((flags & WIN_FOCUS) != 0) color = hoverColor;

            dc.DrawText(choices[currentChoice], textScale, textAlign, color, textRect, false, -1);
        }

        public override void Activate(bool activate, ref string act)
        {
            base.Activate(activate, ref act);
            // sets the gui state based on the current choice the window contains
            if (activate) UpdateChoice();
        }

        public override void RunNamedEvent(string eventName)
        {
            string event_, group;

            if (eventName.StartsWith("cvar read "))
            {
                event_ = eventName;
                group = event_[10..];
                if (group == updateGroup) UpdateVars(true, true);
            }
            else if (eventName.StartsWith("cvar write "))
            {
                event_ = eventName;
                group = event_[11..];
                if (group == updateGroup) UpdateVars(false, true);
            }
        }

        void UpdateChoice()
        {
            if (updateStr.Count == 0)
                return;
            UpdateVars(true);
            updateStr.Update();
            if (choiceType == 0)
            {
                // ChoiceType 0 stores current as an integer in either cvar or gui If both cvar and gui are defined then cvar wins, but they are both updated
                if (updateStr[0].NeedsUpdate) currentChoice = intX.Parse(updateStr[0].ToString());
                ValidateChoice();
            }
            else
            {
                // ChoiceType 1 stores current as a cvar string
                var c = values.Count != 0 ? values.Count : choices.Count;
                int i;
                for (i = 0; i < c; i++) if (string.Equals(cvarStr.ToString(), values.Count != 0 ? values[i] : choices[i], StringComparison.OrdinalIgnoreCase)) break;
                if (i == c) i = 0;
                currentChoice = i;
                ValidateChoice();
            }
        }

        void ValidateChoice()
        {
            if (currentChoice < 0 || currentChoice >= choices.Count) currentChoice = 0;
            if (choices.Count == 0) choices.Add("No Choices Defined");
        }

        void InitVars()
        {
            if (cvarStr.Length != 0)
            {
                cvar = cvarSystem.Find(cvarStr);
                if (cvar == null) { if (cvarStr.ToString() == "s_driver" && cvarStr.ToString() == "net_serverAllowServerMod") common.Warning($"ChoiceWindow::InitVars: gui '{gui.SourceFile}' window '{name}' references undefined cvar '{cvarStr}'"); return; }
                updateStr.Add(cvarStr);
            }
            if (guiStr.Length != 0) updateStr.Add(guiStr);
            updateStr.SetGuiInfo(gui.StateDict);
            updateStr.Update();
        }

        // true: read the updated cvar from cvar system, gui from dict
        // false: write to the cvar system, to the gui dict
        // force == true overrides liveUpdate 0
        void UpdateVars(bool read, bool force = false)
        {
            if (force || liveUpdate)
            {
                if (cvar != null && cvarStr.NeedsUpdate)
                    if (read) cvarStr.Set(cvar.String);
                    else cvar.String = cvarStr;
                if (!read && guiStr.NeedsUpdate) guiStr.Set(currentChoice.ToString());
            }
        }

        // update the lists whenever the WinVar have changed
        void UpdateChoicesAndVals()
        {
            Lexer src = new(); Token token; var str2 = "";

            if (string.Equals(latchedChoices, choicesStr, StringComparison.OrdinalIgnoreCase))
            {
                choices.Clear();
                src.FreeSource();
                src.Flags = LEXFL.NOFATALERRORS | LEXFL.ALLOWPATHNAMES | LEXFL.ALLOWMULTICHARLITERALS | LEXFL.ALLOWBACKSLASHSTRINGCONCAT;
                src.LoadMemory(choicesStr, "<ChoiceList>");
                if (src.IsLoaded)
                {
                    while (src.ReadToken(out token))
                    {
                        if (token == ";")
                        {
                            if (str2.Length != 0) { str2 = common.LanguageDictGetString(str2.StripTrailingWhitespace()); choices.Add(str2); str2 = ""; }
                            continue;
                        }
                        str2 += token;
                        str2 += " ";
                    }
                    if (str2.Length != 0) choices.Add(str2.StripTrailingWhitespace());
                }
                latchedChoices = choicesStr;
            }
            if (choiceVals.Length != 0 && string.Equals(latchedVals, choiceVals, StringComparison.OrdinalIgnoreCase))
            {
                values.Clear();
                src.FreeSource();
                src.Flags = LEXFL.ALLOWPATHNAMES | LEXFL.ALLOWMULTICHARLITERALS | LEXFL.ALLOWBACKSLASHSTRINGCONCAT;
                src.LoadMemory(choiceVals, "<ChoiceVals>");
                str2 = "";
                var negNum = false;
                if (src.IsLoaded)
                {
                    while (src.ReadToken(out token))
                    {
                        if (token == "-") { negNum = true; continue; }
                        if (token == ";")
                        {
                            if (str2.Length != 0) { str2 = str2.StripTrailingWhitespace(); values.Add(str2); str2 = ""; }
                            continue;
                        }
                        if (negNum) { str2 += "-"; negNum = false; }
                        str2 += token;
                        str2 += " ";
                    }
                    if (str2.Length != 0) values.Add(str2.StripTrailingWhitespace());
                }
                if (choices.Count != values.Count) common.Warning($"ChoiceWindow:: gui '{gui.SourceFile}' window '{name}' has value count unequal to choices count");
                latchedVals = choiceVals;
            }
        }
    }
}
