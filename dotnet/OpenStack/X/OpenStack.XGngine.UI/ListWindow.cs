using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public enum TAB_TYPE
    {
        TEXT = 0,
        ICON = 1
    }

    public struct TabRect
    {
        public int x;
        public int w;
        public DeviceContext.ALIGN align;
        public int valign;
        public TAB_TYPE type;
        public Vector2 iconSize;
        public float iconVOffset;
    }

    public class ListWindow : Window
    {
        const int pixelOffset = 3; // Number of pixels above the text that the rect starts
        const int tabBorder = 4; // number of pixels between columns
        const int doubleClickSpeed = 300; // Time in milliseconds between clicks to register as a double-click

        List<TabRect> tabInfo = new();
        int top;
        float sizeBias;
        bool horizontal;
        string tabStopStr;
        string tabAlignStr;
        string tabVAlignStr;
        string tabTypeStr;
        string tabIconSizeStr;
        string tabIconVOffsetStr;
        Dictionary<string, Material> iconMaterials = new();
        bool multipleSel;

        List<string> listItems = new();
        SliderWindow scroller;
        List<int> currentSel;
        string listName;
        int clickTime;
        int typedTime;
        string typed;

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "horizontal", StringComparison.OrdinalIgnoreCase)) { horizontal = src.ParseBool(); return true; }
            if (string.Equals(name, "listname", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out listName); return true; }
            if (string.Equals(name, "tabstops", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out tabStopStr); return true; }
            if (string.Equals(name, "tabaligns", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out tabAlignStr); return true; }
            if (string.Equals(name, "multipleSel", StringComparison.OrdinalIgnoreCase)) { multipleSel = src.ParseBool(); return true; }
            if (string.Equals(name, "tabvaligns", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out tabVAlignStr); return true; }
            if (string.Equals(name, "tabTypes", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out tabTypeStr); return true; }
            if (string.Equals(name, "tabIconSizes", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out tabIconSizeStr); return true; }
            if (string.Equals(name, "tabIconVOffset", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out tabIconVOffsetStr); return true; }
            if (name.StartsWith("mtr_", StringComparison.OrdinalIgnoreCase))
            {
                ParseString(src, out var matName);
                var mat = declManager.FindMaterial(matName);
                mat.SetImageClassifications(1);    // just for resource tracking
                if (mat != null && !mat.TestMaterialFlag(MF.DEFAULTED)) mat.Sort = (float)SS.GUI;
                iconMaterials[name] = mat;
                return true;
            }
            return base.ParseInternalVar(name, src);
        }

        new void CommonInit()
        {
            typed = "";
            typedTime = 0;
            clickTime = 0;
            currentSel.Clear();
            top = 0;
            sizeBias = 0;
            horizontal = false;
            scroller = new SliderWindow(dc, gui);
            multipleSel = false;
        }

        public ListWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public ListWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        bool IsSelected(int index)
            => currentSel.FindIndex(x => x == index) >= 0;

        void SetCurrentSel(int sel)
        {
            currentSel.Clear();
            currentSel.Add(sel);
        }

        void ClearSelection(int sel)
        {
            var cur = currentSel.FindIndex(x => x == sel);
            if (cur >= 0) currentSel.RemoveAt(cur);
        }

        void AddCurrentSel(int sel)
            => currentSel.Add(sel);

        int GetCurrentSel()
           => currentSel.Count != 0 ? currentSel[0] : 0;

        public override string HandleEvent(SysEvent ev, Action<bool> updateVisuals)
        {
            // need to call this to allow proper focus and capturing on embedded children
            var ret = base.HandleEvent(ev, updateVisuals);

            var vert = MaxCharHeight;
            int numVisibleLines = (int)(textRect.h / vert);

            var key = (Key)ev.evValue;

            if (ev.evType == SE.KEY)
            {
                // We only care about key down, not up
                if (ev.evValue2 == 0) return ret;

                if (key == K_MOUSE1 || key == K_MOUSE2)
                    // If the user clicked in the scroller, then ignore it
                    if (scroller.Contains(gui.CursorX, gui.CursorY)) return ret;

                if (key == K_ENTER || key == K_KP_ENTER)
                {
                    RunScript(SCRIPT.ON_ENTER);
                    return cmd;
                }

                if (key == K_MWHEELUP) key = K_UPARROW;
                else if (key == K_MWHEELDOWN) key = K_DOWNARROW;

                if (key == K_MOUSE1)
                {
                    if (Contains(gui.CursorX, gui.CursorY))
                    {
                        var cur = (int)((gui.CursorY - actualY - pixelOffset) / vert) + top;
                        if (cur >= 0 && cur < listItems.Count)
                        {
                            if (multipleSel && KeyInput.IsDown(K_CTRL))
                            {
                                if (IsSelected(cur)) ClearSelection(cur);
                                else AddCurrentSel(cur);
                            }
                            else
                            {
                                if (IsSelected(cur) && (gui.Time < clickTime + doubleClickSpeed))
                                {
                                    // Double-click causes ON_ENTER to get run
                                    RunScript(SCRIPT.ON_ENTER);
                                    return cmd;
                                }
                                SetCurrentSel(cur);

                                clickTime = gui.Time;
                            }
                        }
                        else SetCurrentSel(listItems.Count - 1);
                    }
                }
                else if (key == K_UPARROW || key == K_PGUP || key == K_DOWNARROW || key == K_PGDN)
                {
                    var numLines = 1;
                    if (key == K_PGUP || key == K_PGDN) numLines = numVisibleLines / 2;
                    if (key == K_UPARROW || key == K_PGUP) numLines = -numLines;
                    if (KeyInput.IsDown(K_CTRL)) top += numLines;
                    else SetCurrentSel(GetCurrentSel() + numLines);
                }
                else return ret;
            }
            else if (ev.evType == SE.CHAR)
            {
                if (!stringX.CharIsPrintable((char)key)) return ret;

                if (gui.Time > typedTime + 1000) typed = "";
                typedTime = gui.Time;
                typed += key;

                for (var i = 0; i < listItems.Count; i++)
                    if (listItems[i].StartsWith(typed, StringComparison.OrdinalIgnoreCase))
                    {
                        SetCurrentSel(i);
                        break;
                    }
            }
            else return ret;

            if (GetCurrentSel() < 0) SetCurrentSel(0);
            if (GetCurrentSel() >= listItems.Count) SetCurrentSel(listItems.Count - 1);

            if (scroller.High > 0f)
            {
                if (!KeyInput.IsDown(K_CTRL))
                {
                    if (top > GetCurrentSel() - 1) top = GetCurrentSel() - 1;
                    if (top < GetCurrentSel() - numVisibleLines + 2) top = GetCurrentSel() - numVisibleLines + 2;
                }

                if (top > listItems.Count - 2) top = listItems.Count - 2;
                if (top < 0) top = 0;
                scroller.Value = top;
            }
            else
            {
                top = 0;
                scroller.Value = 0f;
            }

            if (key != K_MOUSE1)
            {
                // Send a fake mouse click event so onAction gets run in our parents
                var ev2 = system.GenerateMouseButtonEvent(1, true);
                base.HandleEvent(ev2, updateVisuals);
            }

            if (currentSel.Count > 0) for (var i = 0; i < currentSel.Count; i++) gui.SetStateInt($"{listName}_sel_{i}", currentSel[i]);
            else gui.SetStateInt($"{listName}_sel_0", 0);
            gui.SetStateInt($"{listName}_numsel", currentSel.Count);

            return ret;
        }

        public override void PostParse()
        {
            base.PostParse();

            InitScroller(horizontal);

            var tabStops = new List<int>();
            if (tabStopStr.Length != 0)
            {
                var src = new Parser(tabStopStr, tabStopStr.Length, "tabstops", LEXFL.NOFATALERRORS | LEXFL.NOSTRINGCONCAT | LEXFL.NOSTRINGESCAPECHARS);
                while (src.ReadToken(out var tok))
                {
                    if (tok == ",") continue;
                    tabStops.Add(intX.Parse(tok));
                }
            }
            var tabAligns = new List<int>();
            if (tabAlignStr.Length != 0)
            {
                var src = new Parser(tabAlignStr, tabAlignStr.Length, "tabaligns", LEXFL.NOFATALERRORS | LEXFL.NOSTRINGCONCAT | LEXFL.NOSTRINGESCAPECHARS);
                while (src.ReadToken(out var tok))
                {
                    if (tok == ",") continue;
                    tabAligns.Add(intX.Parse(tok));
                }
            }
            var tabVAligns = new List<int>();
            if (tabVAlignStr.Length != 0)
            {
                var src = new Parser(tabVAlignStr, tabVAlignStr.Length, "tabvaligns", LEXFL.NOFATALERRORS | LEXFL.NOSTRINGCONCAT | LEXFL.NOSTRINGESCAPECHARS);
                while (src.ReadToken(out var tok))
                {
                    if (tok == ",") continue;
                    tabVAligns.Add(intX.Parse(tok));
                }
            }

            var tabTypes = new List<int>();
            if (tabTypeStr.Length != 0)
            {
                var src = new Parser(tabTypeStr, tabTypeStr.Length, "tabtypes", LEXFL.NOFATALERRORS | LEXFL.NOSTRINGCONCAT | LEXFL.NOSTRINGESCAPECHARS);
                while (src.ReadToken(out var tok))
                {
                    if (tok == ",") continue;
                    tabTypes.Add(intX.Parse(tok));
                }
            }

            var tabSizes = new List<Vector2>();
            if (tabIconSizeStr.Length != 0)
            {
                var src = new Parser(tabIconSizeStr, tabIconSizeStr.Length, "tabiconsizes", LEXFL.NOFATALERRORS | LEXFL.NOSTRINGCONCAT | LEXFL.NOSTRINGESCAPECHARS);
                while (src.ReadToken(out var tok))
                {
                    if (tok == ",") continue;
                    Vector2 size;
                    size.x = intX.Parse(tok); src.ReadToken(out tok); src.ReadToken(out tok);
                    size.y = intX.Parse(tok);
                    tabSizes.Add(size);
                }
            }

            var tabIconVOffsets = new List<float>();
            if (tabIconVOffsetStr.Length != 0)
            {
                var src = new Parser(tabIconVOffsetStr, tabIconVOffsetStr.Length, "tabiconvoffsets", LEXFL.NOFATALERRORS | LEXFL.NOSTRINGCONCAT | LEXFL.NOSTRINGESCAPECHARS);
                while (src.ReadToken(out var tok))
                {
                    if (tok == ",") continue;
                    tabIconVOffsets.Add(intX.Parse(tok));
                }
            }

            var c = tabStops.Count;
            var doAligns = tabAligns.Count == tabStops.Count;
            for (var i = 0; i < c; i++)
            {
                var r = new TabRect
                {
                    x = tabStops[i],
                    align = (DeviceContext.ALIGN)(doAligns ? tabAligns[i] : 0),
                    valign = tabVAligns.Count > 0 && i < tabVAligns.Count ? tabVAligns[i] : 0,
                    type = tabTypes.Count > 0 && i < tabTypes.Count ? (TAB_TYPE)tabTypes[i] : TAB_TYPE.TEXT,
                    iconSize = tabSizes.Count > 0 && i < tabSizes.Count ? tabSizes[i] : Vector2.origin,
                    iconVOffset = tabIconVOffsets.Count > 0 && i < tabIconVOffsets.Count ? tabIconVOffsets[i] : 0,
                };
                r.w = i < c - 1 ? tabStops[i + 1] - r.x - tabBorder : -1;
                tabInfo.Add(r);
            }
            flags |= WIN_CANFOCUS;
        }

        // This is the same as in idEditWindow
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

        public override void Draw(int time, float x, float y)
        {
            Vector4 color;
            string work;
            var count = listItems.Count;
            Rectangle rect = textRect;
            var scale = textScale;
            var lineHeight = MaxCharHeight;

            var bottom = textRect.Bottom;
            var width = textRect.w;

            if (scroller.High > 0f)
                if (horizontal) bottom -= sizeBias;
                else { width -= sizeBias; rect.w = width; }

            if (noEvents || !Contains(gui.CursorX, gui.CursorY)) hover = false;

            for (var i = top; i < count; i++)
            {
                if (IsSelected(i))
                {
                    rect.h = lineHeight;
                    dc.DrawFilledRect(rect.x, rect.y + pixelOffset, rect.w, rect.h, borderColor);
                    if ((flags & WIN_FOCUS) != 0)
                    {
                        color = borderColor;
                        color.w = 1f;
                        dc.DrawRect(rect.x, rect.y + pixelOffset, rect.w, rect.h, 1f, color);
                    }
                }
                rect.y++;
                rect.h = lineHeight - 1;
                color = hover && !noEvents && Contains(rect, gui.CursorX, gui.CursorY) ? hoverColor : foreColor;
                rect.h = lineHeight + pixelOffset;
                rect.y--;

                if (tabInfo.Count > 0)
                {
                    var start = 0;
                    var tab = 0;
                    var stop = listItems[i].IndexOf('\t', 0);
                    while (start < listItems[i].Length)
                    {
                        if (tab >= tabInfo.Count) { common.Warning($"ListWindow::Draw: gui '{gui.SourceFile}' window '{name}' tabInfo.Count exceeded"); break; }
                        work = listItems[i][start..stop];

                        rect.x = textRect.x + tabInfo[tab].x;
                        rect.w = (tabInfo[tab].w == -1) ? width - tabInfo[tab].x : tabInfo[tab].w;
                        dc.PushClipRect(rect);

                        if (tabInfo[tab].type == TAB_TYPE.TEXT) dc.DrawText(work, scale, tabInfo[tab].align, color, rect, false, -1);
                        else if (tabInfo[tab].type == TAB_TYPE.ICON)
                            // leaving the icon name empty doesn't draw anything
                            if (work[0] != '\0')
                            {
                                var iconMat = iconMaterials.TryGetValue(work, out var hashMat) ? declManager.FindMaterial("_default") : hashMat;

                                var iconRect = new Rectangle
                                {
                                    w = tabInfo[tab].iconSize.x,
                                    h = tabInfo[tab].iconSize.y
                                };

                                if (tabInfo[tab].align == DeviceContext.ALIGN.LEFT) iconRect.x = rect.x;
                                else if (tabInfo[tab].align == DeviceContext.ALIGN.CENTER) iconRect.x = rect.x + rect.w / 2f - iconRect.w / 2f;
                                else if (tabInfo[tab].align == DeviceContext.ALIGN.RIGHT) iconRect.x = rect.x + rect.w - iconRect.w;

                                if (tabInfo[tab].valign == 0) iconRect.y = rect.y + tabInfo[tab].iconVOffset;  //Top
                                else if (tabInfo[tab].valign == 1) iconRect.y = rect.y + rect.h / 2f - iconRect.h / 2f + tabInfo[tab].iconVOffset; //Center
                                else if (tabInfo[tab].valign == 2) iconRect.y = rect.y + rect.h - iconRect.h + tabInfo[tab].iconVOffset; //Bottom
                                dc.DrawMaterial(iconRect.x, iconRect.y, iconRect.w, iconRect.h, iconMat, new Vector4(1f, 1f, 1f, 1f), 1f, 1f);
                            }

                        dc.PopClipRect();

                        start = stop + 1;
                        stop = listItems[i].IndexOf('\t', start);
                        if (stop < 0) stop = listItems[i].Length;
                        tab++;
                    }
                    rect.x = textRect.x;
                    rect.w = width;
                }
                else dc.DrawText(listItems[i], scale, 0, color, rect, false, -1);
                rect.y += lineHeight;
                if (rect.y > bottom) break;
            }
        }

        public override void Activate(bool activate, ref string act)
        {
            base.Activate(activate, ref act);
            if (activate)                UpdateList();
        }

        public override void HandleBuddyUpdate(Window buddy)
            => top = (int)scroller.Value;

        public void UpdateList()
        {
            listItems.Clear();
            for (var i = 0; i < MAX_LIST_ITEMS; i++)
                if (gui.State.TryGetString($"{listName}_item_{i}", "", out var str))
                {
                    if (str.Length != 0) listItems.Add(str);
                }
                else break;
            var vert = MaxCharHeight;
            var fit = (int)(textRect.h / vert);
            if (listItems.Count < fit) scroller.SetRange(0f, 0f, 1f);
            else scroller.SetRange(0f, listItems.Count - fit + 1f, 1f);

            SetCurrentSel(gui.State.GetInt($"{listName}_sel_0"));

            var value = scroller.Value;
            if (value > listItems.Count - 1) value = listItems.Count - 1;
            if (value < 0f) value = 0f;
            scroller.Value = value;
            top = (int)value;

            typedTime = 0;
            clickTime = 0;
            typed = "";
        }

        public override void StateChanged(bool redraw = false)
            => UpdateList();

        public override int Allocated => base.Allocated;

        public override WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
            => base.GetWinVarByName(name, winLookup, owner);
    }
}

