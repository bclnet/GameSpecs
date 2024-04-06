using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class SliderWindow : Window
    {
        WinFloat value;
        float low;
        float high;
        float thumbWidth;
        float thumbHeight;
        float stepSize;
        float lastValue;
        Rectangle thumbRect;
        Material thumbMat;
        bool vertical;
        bool verticalFlip;
        bool scrollbar;
        Window buddyWin;
        string thumbShader;

        WinStr cvarStr;
        CVar cvar;
        bool cvar_init;
        WinBool liveUpdate;
        WinStr cvarGroup;

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "stepsize", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "step", StringComparison.OrdinalIgnoreCase)) { stepSize = src.ParseFloat(); return true; }
            if (string.Equals(name, "low", StringComparison.OrdinalIgnoreCase)) { low = src.ParseFloat(); return true; }
            if (string.Equals(name, "high", StringComparison.OrdinalIgnoreCase)) { high = src.ParseFloat(); return true; }
            if (string.Equals(name, "vertical", StringComparison.OrdinalIgnoreCase)) { vertical = src.ParseBool(); return true; }
            if (string.Equals(name, "verticalflip", StringComparison.OrdinalIgnoreCase)) { verticalFlip = src.ParseBool(); return true; }
            if (string.Equals(name, "scrollbar", StringComparison.OrdinalIgnoreCase)) { scrollbar = src.ParseBool(); return true; }
            if (string.Equals(name, "thumbshader", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out thumbShader); declManager.FindMaterial(thumbShader); return true; }
            return base.ParseInternalVar(name, src);
        }

        public override WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
        {
            if (string.Equals(name, "value", StringComparison.OrdinalIgnoreCase)) return value;
            if (string.Equals(name, "cvar", StringComparison.OrdinalIgnoreCase)) return cvarStr;
            if (string.Equals(name, "liveUpdate", StringComparison.OrdinalIgnoreCase)) return liveUpdate;
            if (string.Equals(name, "cvarGroup", StringComparison.OrdinalIgnoreCase)) return cvarGroup;
            return base.GetWinVarByName(name, winLookup, owner);
        }

        new void CommonInit()
        {
            value = 0f;
            low = 0f;
            high = 100f;
            stepSize = 1f;
            thumbMat = declManager.FindMaterial("_default");
            buddyWin = null;
            cvar = null;
            cvar_init = false;
            liveUpdate = true;
            vertical = false;
            scrollbar = false;
            verticalFlip = false;
        }

        public SliderWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public SliderWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        public float Low => low;
        public float High => high;

        public float Value
        {
            get => this.value;
            set => this.value = value;
        }


        public virtual string HandleEvent(SysEvent ev, bool updateVisuals)
        {
            if (!(ev.evType == SE.KEY && ev.evValue2 != 0)) return "";

            var key = (Key)ev.evValue;

            if (ev.evValue2 != 0 && key == K_MOUSE1) { SetCapture(this); RouteMouseCoords(0f, 0f); return ""; }

            if (key == K_RIGHTARROW || key == K_KP_RIGHTARROW || (key == K_MOUSE2 && gui.CursorY > thumbRect.y)) value += stepSize;
            if (key == K_LEFTARROW || key == K_KP_LEFTARROW || (key == K_MOUSE2 && gui.CursorY < thumbRect.y)) value -= stepSize;

            if (buddyWin != null) buddyWin.HandleBuddyUpdate(this);
            else { gui.SetStateFloat(cvarStr, value); UpdateCvar(false); }

            return "";
        }

        public override void SetBuddy(Window buddy)
            => buddyWin = buddy;

        public override void PostParse()
        {
            base.PostParse();
            value = 0f;
            thumbMat = declManager.FindMaterial(thumbShader);
            thumbMat.Sort = (float)SS.GUI;
            thumbWidth = thumbMat.ImageWidth;
            thumbHeight = thumbMat.ImageHeight;
            flags |= (WIN_HOLDCAPTURE | WIN_CANFOCUS);
            InitCvar();
        }

        public void InitWithDefaults(string name, Rectangle rect, Vector4 foreColor, Vector4 matColor, string background, string thumbShader, bool vertical, bool scrollbar)
        {
            SetInitialState(name);
            this.rect = rect;
            this.foreColor = foreColor;
            this.matColor = matColor;
            thumbMat = declManager.FindMaterial(thumbShader);
            thumbMat.Sort = (float)SS.GUI;
            thumbWidth = thumbMat.ImageWidth;
            thumbHeight = thumbMat.ImageHeight;
            this.background = declManager.FindMaterial(background);
            this.background.Sort = (float)SS.GUI;
            this.vertical = vertical;
            this.scrollbar = scrollbar;
            flags |= WIN_HOLDCAPTURE;
        }

        public void SetRange(float low, float high, float step)
        {
            this.low = low;
            this.high = high;
            stepSize = step;
        }

        public override void Draw(int time, float x, float y)
        {
            Vector4 color = foreColor;

            if (cvar == null && buddyWin == null) return;

            if (thumbWidth == 0f || thumbHeight == 0f)
            {
                thumbWidth = thumbMat.ImageWidth;
                thumbHeight = thumbMat.ImageHeight;
            }

            UpdateCvar(true);
            if (value > high) value = high;
            else if (value < low) value = low;

            var range = high - low;
            if (range <= 0f) return;

            var thumbPos = range != 0 ? (value - low) / range : 0f;
            if (vertical)
            {
                if (verticalFlip) thumbPos = 1f - thumbPos;
                thumbPos *= drawRect.h - thumbHeight;
                thumbPos += drawRect.y;
                thumbRect.y = thumbPos;
                thumbRect.x = drawRect.x;
            }
            else
            {
                thumbPos *= drawRect.w - thumbWidth;
                thumbPos += drawRect.x;
                thumbRect.x = thumbPos;
                thumbRect.y = drawRect.y;
            }
            thumbRect.w = thumbWidth;
            thumbRect.h = thumbHeight;

            if (hover && !noEvents && Contains(gui.CursorX, gui.CursorY)) color = hoverColor;
            else hover = false;
            if ((flags & WIN_CAPTURE) != 0) { color = hoverColor; hover = true; }

            dc.DrawMaterial(thumbRect.x, thumbRect.y, thumbRect.w, thumbRect.h, thumbMat, color);
            if ((flags & WIN_FOCUS) != 0) dc.DrawRect(thumbRect.x + 1f, thumbRect.y + 1f, thumbRect.w - 2f, thumbRect.h - 2f, 1f, color);
        }

        public override void DrawBackground(Rectangle drawRect)
        {
            if (cvar == null && buddyWin == null) return;
            if (high - low <= 0f) return;

            var r = new Rectangle(drawRect);
            if (!scrollbar)
                if (vertical) { r.y += thumbHeight / 2f; r.h -= thumbHeight; }
                else { r.x += thumbWidth / 2f; r.w -= thumbWidth; }
            base.DrawBackground(r);
        }

        public override string RouteMouseCoords(float xd, float yd)
        {
            float pct;

            if ((flags & WIN_CAPTURE) == 0)
                return "";

            var r = new Rectangle(drawRect);
            r.x = actualX;
            r.y = actualY;
            r.x += thumbWidth / 2f;
            r.w -= thumbWidth;
            if (vertical)
            {
                r.y += thumbHeight / 2;
                r.h -= thumbHeight;
                if (gui.CursorY >= r.y && gui.CursorY <= r.Bottom)
                {
                    pct = (gui.CursorY - r.y) / r.h;
                    if (verticalFlip) pct = 1f - pct;
                    value = low + (high - low) * pct;
                }
                else if (gui.CursorY < r.y) value = verticalFlip ? high : low;
                else value = verticalFlip ? low : high;
            }
            else
            {
                r.x += thumbWidth / 2;
                r.w -= thumbWidth;
                if (gui.CursorX >= r.x && gui.CursorX <= r.Right)
                {
                    pct = (gui.CursorX - r.x) / r.w;
                    value = low + (high - low) * pct;
                }
                else if (gui.CursorX < r.x) value = low;
                else value = high;
            }

            if (buddyWin != null) buddyWin.HandleBuddyUpdate(this);
            else gui.SetStateFloat(cvarStr, value);
            UpdateCvar(false);

            return "";
        }

        public override void Activate(bool activate, ref string act)
        {
            base.Activate(activate, ref act);
            if (activate) UpdateCvar(true, true);
        }

        void InitCvar()
        {
            if (((string)cvarStr).Length == 0)
            {
                if (buddyWin == null) common.Warning($"SliderWindow::InitCvar: gui '{gui.SourceFile}' window '{name}' has an empty cvar string");
                cvar_init = true;
                cvar = null;
                return;
            }

            cvar = cvarSystem.Find(cvarStr);
            if (cvar == null) { common.Warning($"SliderWindow::InitCvar: gui '{gui.SourceFile}' window '{name}' references undefined cvar '{cvarStr}'"); cvar_init = true; return; }
        }

        // true: read the updated cvar from cvar system
        // false: write to the cvar system
        // force == true overrides liveUpdate 0
        void UpdateCvar(bool read, bool force = false)
        {
            if (buddyWin != null || cvar == null) return;
            if (force || liveUpdate)
            {
                value = cvar.Float;
                if (value != gui.State.GetFloat(cvarStr))
                    if (read) gui.SetStateFloat(cvarStr, value);
                    else { value = gui.State.GetFloat(cvarStr); cvar.Float = value; }
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