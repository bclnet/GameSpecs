namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class FieldWindow : Window
    {
        int cursorPos;
        int lastTextLength;
        int lastCursorPos;
        int paintOffset;
        bool showCursor;
        string cursorVar;

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "cursorvar", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out cursorVar); return true; }
            if (string.Equals(name, "showcursor", StringComparison.OrdinalIgnoreCase)) { showCursor = src.ParseBool(); return true; }
            return base.ParseInternalVar(name, src);
        }

        new void CommonInit()
        {
            cursorPos = 0;
            lastTextLength = 0;
            lastCursorPos = 0;
            paintOffset = 0;
            showCursor = false;
        }

        public FieldWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public FieldWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        void CalcPaintOffset(int len)
        {
            lastCursorPos = cursorPos;
            lastTextLength = len;
            paintOffset = 0;
            var tw = dc.TextWidth(text, textScale, -1);
            if (tw < textRect.w) return;
            while (tw > textRect.w && len > 0)
            {
                tw = dc.TextWidth(text, textScale, --len);
                paintOffset++;
            }
        }

        public override void Draw(int time, float x, float y)
        {
            var textS = (string)text;
            var scale = (float)textScale;
            var len = textS.Length;
            cursorPos = gui.State.GetInt(cursorVar);
            if (len != lastTextLength || cursorPos != lastCursorPos) CalcPaintOffset(len);
            var rect = new Rectangle(textRect);
            if (paintOffset >= len) paintOffset = 0;
            if (cursorPos > len) cursorPos = len;
            dc.DrawText(textS[paintOffset..], scale, 0, foreColor, rect, false, (flags & WIN_FOCUS) != 0 || showCursor ? cursorPos - paintOffset : -1);
        }
    }
}

