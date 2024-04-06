using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.System;
using System.Runtime.CompilerServices;
using System.Text;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class EditWindow : Window
    {
        public const int MAX_EDITFIELD = 4096;
        int maxChars;
        int paintOffset;
        int cursorPos;
        int cursorLine;
        int cvarMax;
        bool wrap;
        bool readonly_;
        bool numeric;
        string sourceFile;
        SliderWindow scroller;
        List<int> breaks = new();
        float sizeBias;
        int textIndex;
        int lastTextLength;
        bool forceScroll;
        WinBool password;

        WinStr cvarStr;
        CVar cvar;

        WinBool liveUpdate;
        WinStr cvarGroup;

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "maxchars", StringComparison.InvariantCultureIgnoreCase)) { maxChars = src.ParseInt(); return true; }
            if (string.Equals(name, "numeric", StringComparison.InvariantCultureIgnoreCase)) { numeric = src.ParseBool(); return true; }
            if (string.Equals(name, "wrap", StringComparison.InvariantCultureIgnoreCase)) { wrap = src.ParseBool(); return true; }
            if (string.Equals(name, "readonly", StringComparison.InvariantCultureIgnoreCase)) { readonly_ = src.ParseBool(); return true; }
            if (string.Equals(name, "forceScroll", StringComparison.InvariantCultureIgnoreCase)) { forceScroll = src.ParseBool(); return true; }
            if (string.Equals(name, "source", StringComparison.InvariantCultureIgnoreCase)) { ParseString(src, out sourceFile); return true; }
            if (string.Equals(name, "password", StringComparison.InvariantCultureIgnoreCase)) { password = src.ParseBool(); return true; }
            if (string.Equals(name, "cvarMax", StringComparison.InvariantCultureIgnoreCase)) { cvarMax = src.ParseInt(); return true; }
            return base.ParseInternalVar(name, src);
        }

        public override WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
        {
            if (string.Equals(name, "cvar", StringComparison.InvariantCultureIgnoreCase)) return cvarStr;
            if (string.Equals(name, "password", StringComparison.InvariantCultureIgnoreCase)) return password;
            if (string.Equals(name, "liveUpdate", StringComparison.InvariantCultureIgnoreCase)) return liveUpdate;
            if (string.Equals(name, "cvarGroup", StringComparison.InvariantCultureIgnoreCase)) return cvarGroup;
            return base.GetWinVarByName(name, winLookup, owner);
        }

        void CommonInit()
        {
            maxChars = 128;
            numeric = false;
            paintOffset = 0;
            cursorPos = 0;
            cursorLine = 0;
            cvarMax = 0;
            wrap = false;
            sourceFile = "";
            scroller = null;
            sizeBias = 0;
            lastTextLength = 0;
            forceScroll = false;
            password = false;
            cvar = null;
            liveUpdate = true;
            readonly_ = false;

            scroller = new SliderWindow(dc, gui);
        }

        public EditWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public EditWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        public override void GainFocus()
        {
            cursorPos = text.Length;
            EnsureCursorVisible();
        }

        public override void Draw(int time, float x, float y)
        {
            Vector4 color = foreColor;

            UpdateCvar(true);

            var len = text.Length;
            if (len != lastTextLength)
            {
                scroller.Value = 0f;
                EnsureCursorVisible();
                lastTextLength = len;
            }
            float scale = textScale;

            var buffer = password ? new string('*', text.Length) : (string)text;
            if (cursorPos > len) cursorPos = len;

            var rect = new Rectangle(textRect);

            rect.x -= paintOffset;
            rect.w += paintOffset;

            if (wrap && scroller.High > 0f)
            {
                var lineHeight = MaxCharHeight + 5;
                rect.y -= scroller.Value * lineHeight;
                rect.w -= sizeBias;
                rect.h = (breaks.Count + 1) * lineHeight;
            }

            if (hover && !noEvents && Contains(gui.CursorX, gui.CursorY)) color = hoverColor;
            else hover = false;
            if ((flags & WIN_FOCUS) != 0) color = hoverColor;

            dc.DrawText(buffer, scale, 0, color, rect, wrap, (flags & WIN_FOCUS) != 0 ? cursorPos : -1);
        }

        static readonly byte[] HandleEvent_buffer = new byte[MAX_EDITFIELD];
        public override string HandleEvent(SysEvent ev, Action<bool> updateVisuals)
        {
            var ret = "";

            if (wrap)
            {
                // need to call this to allow proper focus and capturing on embedded children
                ret = base.HandleEvent(ev, updateVisuals);
                if (!string.IsNullOrEmpty(ret)) return ret;
            }

            if (ev.evType != SE.CHAR && ev.evType != SE.KEY) return ret;

            var textB = Encoding.ASCII.GetBytes(text);
            Unsafe.CopyBlock(ref HandleEvent_buffer[0], ref textB[0], (uint)textB.Length);
            var key = (Key)ev.evValue;
            var len = text.Length;

            if (ev.evType == SE.CHAR)
            {
                if (ev.evValue == SysW.GetConsoleKey(false) || ev.evValue == SysW.GetConsoleKey(true)) return "";

                updateVisuals?.Invoke(true);

                if (maxChars != 0 && len > maxChars) len = maxChars;
                if ((key == K_ENTER || key == K_KP_ENTER) && ev.evValue2 != 0) { RunScript(SCRIPT.ON_ACTION); RunScript(SCRIPT.ON_ENTER); return cmd; }
                if (key == K_ESCAPE) { RunScript(SCRIPT.ON_ESC); return cmd; }
                if (readonly_) return "";

                if (key == (Key)('h' - 'a' + 1) || key == K_BACKSPACE)
                {   // ctrl-h is backspace
                    if (cursorPos > 0)
                    {
                        if (cursorPos >= len) { HandleEvent_buffer[len - 1] = 0; cursorPos = len - 1; }
                        else { Buffer.BlockCopy(HandleEvent_buffer, cursorPos, HandleEvent_buffer, cursorPos - 1, len + 1 - cursorPos); cursorPos--; }

                        text = Encoding.ASCII.GetString(HandleEvent_buffer);
                        UpdateCvar(false);
                        RunScript(SCRIPT.ON_ACTION);
                    }

                    return "";
                }

                // ignore any non printable chars (except enter when wrap is enabled)
                if (wrap && (key == K_ENTER || key == K_KP_ENTER)) { }
                else if (!stringX.CharIsPrintable((char)key)) return "";

                if (numeric && ((key < (Key)'0' || key > (Key)'9') && key != (Key)'.')) return "";

                if (dc.OverStrike)
                    if (maxChars != 0 && cursorPos >= maxChars) return "";
                    else
                    {
                        if ((len == MAX_EDITFIELD - 1) || (maxChars != 0 && len >= maxChars)) return "";
                        Buffer.BlockCopy(HandleEvent_buffer, cursorPos, HandleEvent_buffer, cursorPos + 1, len + 1 - cursorPos);
                    }

                HandleEvent_buffer[cursorPos] = (byte)key;

                text = Encoding.ASCII.GetString(HandleEvent_buffer);
                UpdateCvar(false);
                RunScript(SCRIPT.ON_ACTION);

                if (cursorPos < len + 1) cursorPos++;
                EnsureCursorVisible();

            }
            else if (ev.evType == SE.KEY && ev.evValue2 != 0)
            {
                updateVisuals?.Invoke(true);

                if (key == K_DEL)
                {
                    if (readonly_) return ret;
                    if (cursorPos < len)
                    {
                        Buffer.BlockCopy(HandleEvent_buffer, cursorPos + 1, HandleEvent_buffer, cursorPos, len - cursorPos);
                        HandleEvent_buffer[cursorPos] = (byte)key;
                        UpdateCvar(false);
                        RunScript(SCRIPT.ON_ACTION);
                    }
                    return ret;
                }

                if (key == K_RIGHTARROW)
                {
                    if (cursorPos < len)
                        if (KeyInput.IsDown(K_CTRL))
                        {
                            // skip to next word
                            while ((cursorPos < len) && (HandleEvent_buffer[cursorPos] != ' ')) cursorPos++;
                            while ((cursorPos < len) && (HandleEvent_buffer[cursorPos] == ' ')) cursorPos++;
                        }
                        else if (cursorPos < len) cursorPos++;

                    EnsureCursorVisible();
                    return ret;
                }

                if (key == K_LEFTARROW)
                {
                    if (KeyInput.IsDown(K_CTRL))
                    {
                        // skip to previous word
                        while ((cursorPos > 0) && (HandleEvent_buffer[cursorPos - 1] == ' ')) cursorPos--;
                        while ((cursorPos > 0) && (HandleEvent_buffer[cursorPos - 1] != ' ')) cursorPos--;
                    }
                    else if (cursorPos > 0) cursorPos--;

                    EnsureCursorVisible();
                    return ret;
                }

                if (key == K_HOME)
                {
                    cursorPos = KeyInput.IsDown(K_CTRL) || cursorLine <= 0 || cursorLine >= breaks.Count ? 0 : breaks[cursorLine];
                    EnsureCursorVisible();
                    return ret;
                }

                if (key == K_END)
                {
                    cursorPos = KeyInput.IsDown(K_CTRL) || cursorLine < -1 || cursorLine >= breaks.Count - 1 ? len : breaks[cursorLine + 1] - 1;
                    EnsureCursorVisible();
                    return ret;
                }

                if (key == K_INS)
                {
                    if (!readonly_) dc.OverStrike = !dc.OverStrike;
                    return ret;
                }

                if (key == K_DOWNARROW)
                {
                    if (KeyInput.IsDown(K_CTRL)) scroller.Value++;
                    else if (cursorLine < breaks.Count - 1)
                    {
                        var offset = cursorPos - breaks[cursorLine];
                        cursorPos = breaks[cursorLine + 1] + offset;
                        EnsureCursorVisible();
                    }
                }

                if (key == K_UPARROW)
                {
                    if (KeyInput.IsDown(K_CTRL)) scroller.Value--;
                    else if (cursorLine > 0)
                    {
                        var offset = cursorPos - breaks[cursorLine];
                        cursorPos = breaks[cursorLine - 1] + offset;
                        EnsureCursorVisible();
                    }
                }

                if (key == K_ENTER || key == K_KP_ENTER)
                {
                    RunScript(SCRIPT.ON_ACTION);
                    RunScript(SCRIPT.ON_ENTER);
                    return cmd;
                }

                if (key == K_ESCAPE)
                {
                    RunScript(SCRIPT.ON_ESC);
                    return cmd;
                }

            }
            else if (ev.evType == SE.KEY && ev.evValue2 == 0)
            {
                if (key == K_ENTER || key == K_KP_ENTER)
                {
                    RunScript(SCRIPT.ON_ENTERRELEASE);
                    return cmd;
                }
                else RunScript(SCRIPT.ON_ACTIONRELEASE);
            }

            return ret;
        }

        public override void PostParse()
        {
            base.PostParse();

            if (maxChars == 0) maxChars = 10;
            if (sourceFile.Length != 0)
            {
                fileSystem.ReadFile(sourceFile, out var buffer, out var _);
                text = Encoding.ASCII.GetString(buffer);
                fileSystem.FreeFile(buffer);
            }

            InitCvar();
            InitScroller(false);

            EnsureCursorVisible();

            flags |= WIN_CANFOCUS;
        }

        // This is the same as in ListWindow
        void InitScroller(bool horizontal)
        {
            var thumbImage = "guis/assets/scrollbar_thumb.tga";
            var barImage = "guis/assets/scrollbarv.tga";
            var scrollerName = "_scrollerWinV";

            if (horizontal)
            {
                barImage = "guis/assets/scrollbarh.tga";
                scrollerName = "_scrollerWinH";
            }

            var mat = declManager.FindMaterial(barImage);
            mat.Sort = (float)SS.GUI;
            sizeBias = mat.ImageWidth;

            var scrollRect = new Rectangle();
            if (horizontal)
            {
                sizeBias = mat.ImageHeight;
                scrollRect.x = 0;
                scrollRect.y = clientRect.h - sizeBias;
                scrollRect.w = clientRect.w;
                scrollRect.h = sizeBias;
            }
            else
            {
                scrollRect.x = clientRect.w - sizeBias;
                scrollRect.y = 0;
                scrollRect.w = sizeBias;
                scrollRect.h = clientRect.h;
            }

            scroller.InitWithDefaults(scrollerName, scrollRect, foreColor, matColor, mat.Name, thumbImage, !horizontal, true);
            InsertChild(scroller, null);
            scroller.SetBuddy(this);
        }

        public override int Allocated => base.Allocated;

        public override void HandleBuddyUpdate(Window buddy) { }

        void EnsureCursorVisible()
        {
            if (readonly_) cursorPos = -1;
            else if (maxChars == 1) cursorPos = 0;

            if (dc == null) return;

            SetFont();
            if (!wrap)
            {
                var cursorX = 0;
                if (password) cursorX = cursorPos * dc.CharWidth('*', textScale);
                else
                {
                    var i = 0;
                    while (i < text.Length && i < cursorPos)
                        if (stringX.IsColor((string)text, i)) i += 2;
                        else { cursorX += dc.CharWidth(((string)text)[i], textScale); i++; }
                }
                var maxWidth = MaxCharWidth;
                var left = cursorX - maxWidth;
                var right = (cursorX - textRect.w) + maxWidth;

                if (paintOffset > left) paintOffset = (int)(left - maxWidth * 6); // When we go past the left side, we want the text to jump 6 characters
                if (paintOffset < right) paintOffset = (int)right;
                if (paintOffset < 0) paintOffset = 0;
                scroller.SetRange(0f, 0f, 1f);
            }
            else
            {
                // Word wrap
                breaks.Clear();
                var rect = new Rectangle(textRect);
                rect.w -= sizeBias;
                dc.DrawText(text, textScale, textAlign, colorWhite, rect, true, (flags & WIN_FOCUS) != 0 ? cursorPos : -1, true, breaks);

                var fit = textRect.h / (MaxCharHeight + 5);
                if (fit < breaks.Count + 1) scroller.SetRange(0, breaks.Count + 1 - fit, 1);
                // The text fits completely in the box
                else scroller.SetRange(0f, 0f, 1f);

                if (forceScroll) scroller.Value = breaks.Count - fit;
                else if (readonly_) { }
                else
                {
                    cursorLine = 0;
                    for (var i = 1; i < breaks.Count; i++)
                        if (cursorPos >= breaks[i]) cursorLine = i;
                        else break;
                    var topLine = MathX.FtoiFast(scroller.Value);
                    if (cursorLine < topLine) scroller.Value = cursorLine;
                    else if (cursorLine >= topLine + fit) scroller.Value = (cursorLine - fit) + 1;
                }
            }
        }

        public override void Activate(bool activate, ref string act)
        {
            base.Activate(activate, ref act);
            if (activate)
            {
                UpdateCvar(true, true);
                EnsureCursorVisible();
            }
        }

        void InitCvar()
        {
            if (((string)cvarStr).Length == 0)
            {
                if (text.Name == null) common.Warning($"EditWindow::InitCvar: gui '{gui.SourceFile}' window '{name}' has an empty cvar string");
                cvar = null;
                return;
            }

            cvar = cvarSystem.Find(cvarStr);
            if (cvar == null)
            {
                common.Warning($"EditWindow::InitCvar: gui '{gui.SourceFile}' window '{name}' references undefined cvar '{cvarStr}'");
                return;
            }
        }

        // true: read the updated cvar from cvar system
        // false: write to the cvar system
        // force == true overrides liveUpdate 0
        void UpdateCvar(bool read, bool force = false)
        {
            if (force || liveUpdate)
                if (cvar == null)
                    if (read) text = cvar.String;
                    else
                    {
                        cvar.String = text;
                        if (cvarMax != 0 && cvar.Integer > cvarMax) cvar.Integer = cvarMax;
                    }
        }

        public override void RunNamedEvent(string eventName)
        {
            string ev, group;

            if (eventName.StartsWith("cvar read "))
            {
                ev = eventName;
                group = ev[10..];
                if (group == cvarGroup) UpdateCvar(true, true);
            }
            else if (eventName.StartsWith("cvar write "))
            {
                ev = eventName;
                group = ev[11..];
                if (group == cvarGroup) UpdateCvar(false, true);
            }
        }
    }
}