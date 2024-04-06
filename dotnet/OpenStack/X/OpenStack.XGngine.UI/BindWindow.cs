using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class BindWindow : Window
    {
        WinStr bindName;
        bool waitingOnKey;

        void CommonInit()
        {
            bindName = "";
            waitingOnKey = false;
        }

        public override WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
        {
            if (string.Equals(name, "bind", StringComparison.OrdinalIgnoreCase)) return bindName;
            return base.GetWinVarByName(name, winLookup, owner);
        }

        public BindWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public BindWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        public virtual string HandleEvent(SysEvent ev, bool updateVisuals)
        {
            if (!(ev.evType == SE.KEY && ev.evValue2 != 0)) return "";

            var key = (Key)ev.evValue;

            if (waitingOnKey) { waitingOnKey = false; return key == K_ESCAPE ? $"clearbind \"{bindName.Name}\"" : $"bind {key} \"{bindName.Name}\""; }
            else if (key == K_MOUSE1) { waitingOnKey = true; gui.SetBindHandler(this); return ""; }
            return "";
        }

        public override void PostParse()
        {
            base.PostParse();
            bindName.SetGuiInfo(gui.StateDict, bindName);
            bindName.Update();
            flags |= WIN_HOLDCAPTURE | WIN_CANFOCUS;
        }

        public override void Draw(int time, float x, float y)
        {
            var color = (Vector4)foreColor;

            var str = waitingOnKey ? common.LanguageDictGetString("#str_07000")
                : bindName.Length != 0 ? bindName
                : common.LanguageDictGetString("#str_07001");

            if (waitingOnKey || (hover && !noEvents && Contains(gui.CursorX, gui.CursorY))) color = hoverColor;
            else hover = false;

            dc.DrawText(str, textScale, textAlign, color, textRect, false, -1);
        }

        public override void Activate(bool activate, ref string act)
        {
            base.Activate(activate, ref act);
            bindName.Update();
        }
    }
}
