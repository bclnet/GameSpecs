using System.Collections.Generic;
using System.Diagnostics;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public enum WOP_TYPE
    {
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
        MOD,
        TABLE,
        GT,
        GE,
        LT,
        LE,
        EQ,
        NE,
        AND,
        OR,
        VAR,
        VARS,
        VARF,
        VARI,
        VARB,
        COND
    }

    public enum WEXP_REG
    {
        TIME,
        NUM_PREDEFINED
    }

    public class WexpOp
    {
        public WOP_TYPE opType;
        public object a;
        public int b, c, d;
    }

    public struct RegEntry
    {
        public RegEntry(string name, Register.REGTYPE type)
        {
            this.name = name;
            this.type = type;
            index = 0;
        }
        public string name;
        public Register.REGTYPE type;
        public int index;
    }

    public class TimeLineEvent
    {
        public int time;
        public GuiScriptList event_ = new();
        public bool pending;
        public int Size => 0 + event_.Size;
    }

    public class RVNamedEvent
    {
        public RVNamedEvent(string name)
        {
            mEvent = new GuiScriptList();
            mName = name;
        }
        public int Size => 0 + mEvent.Size;

        public string mName;
        public GuiScriptList mEvent;
    }

    public class TransitionData
    {
        public WinVar data;
        public int offset;
        public InterpolateAccelDecelLinear_Vector4 interp;
    }

    public class Window
    {
        public const uint WIN_CHILD = 0x00000001;
        public const uint WIN_CAPTION = 0x00000002;
        public const uint WIN_BORDER = 0x00000004;
        public const uint WIN_SIZABLE = 0x00000008;
        public const uint WIN_MOVABLE = 0x00000010;
        public const uint WIN_FOCUS = 0x00000020;
        public const uint WIN_CAPTURE = 0x00000040;
        public const uint WIN_HCENTER = 0x00000080;
        public const uint WIN_VCENTER = 0x00000100;
        public const uint WIN_MODAL = 0x00000200;
        public const uint WIN_INTRANSITION = 0x00000400;
        public const uint WIN_CANFOCUS = 0x00000800;
        public const uint WIN_SELECTED = 0x00001000;
        public const uint WIN_TRANSFORM = 0x00002000;
        public const uint WIN_HOLDCAPTURE = 0x00004000;
        public const uint WIN_NOWRAP = 0x00008000;
        public const uint WIN_NOCLIP = 0x00010000;
        public const uint WIN_INVERTRECT = 0x00020000;
        public const uint WIN_NATURALMAT = 0x00040000;
        public const uint WIN_NOCURSOR = 0x00080000;
        public const uint WIN_MENUGUI = 0x00100000;
        public const uint WIN_ACTIVE = 0x00200000;
        public const uint WIN_SHOWCOORDS = 0x00400000;
        public const uint WIN_SHOWTIME = 0x00800000;
        public const uint WIN_WANTENTER = 0x01000000;

        public const uint WIN_DESKTOP = 0x10000000;

        public const uint WIN_SCALETO43 = 0x20000000; // DG: for the "scaleto43" window flag (=> scale window to 4:3 with "empty" bars left/right or above/below)

        public const string CAPTION_HEIGHT = "16.0";
        public const string SCROLLER_SIZE = "16.0";
        public const int SCROLLBAR_SIZE = 16;

        public const int MAX_WINDOW_NAME = 32;
        public const int MAX_LIST_ITEMS = 1024;

        public const int MAX_EXPRESSION_OPS = 4096;
        public const int MAX_EXPRESSION_REGISTERS = 4096;

        public const string DEFAULT_BACKCOLOR = "1 1 1 1";
        public const string DEFAULT_FORECOLOR = "0 0 0 1";
        public const string DEFAULT_BORDERCOLOR = "0 0 0 1";
        public const string DEFAULT_TEXTSCALE = "0.4";

        public enum SCRIPT
        {
            ON_MOUSEENTER = 0,
            ON_MOUSEEXIT,
            ON_ACTION,
            ON_ACTIVATE,
            ON_DEACTIVATE,
            ON_ESC,
            ON_FRAME,
            ON_TRIGGER,
            ON_ACTIONRELEASE,
            ON_ENTER,
            ON_ENTERRELEASE,
            SCRIPT_COUNT
        }

        public enum ADJUST
        {
            MOVE = 0,
            TOP,
            RIGHT,
            BOTTOM,
            LEFT,
            TOPLEFT,
            BOTTOMRIGHT,
            TOPRIGHT,
            BOTTOMLEFT
        }

        public static readonly string[] ScriptNames = {
            "onMouseEnter",
            "onMouseExit",
            "onAction",
            "onActivate",
            "onDeactivate",
            "onESC",
            "onEvent",
            "onTrigger",
            "onActionRelease",
            "onEnter",
            "onEnterRelease"
        };

        // made RegisterVars a member of idWindow
        public static readonly RegEntry[] RegisterVars =
        {
            new RegEntry("forecolor", Register.REGTYPE.VEC4),
            new RegEntry("hovercolor", Register.REGTYPE.VEC4),
            new RegEntry("backcolor", Register.REGTYPE.VEC4),
            new RegEntry("bordercolor", Register.REGTYPE.VEC4),
            new RegEntry("rect", Register.REGTYPE.RECTANGLE),
            new RegEntry("matcolor", Register.REGTYPE.VEC4),
            new RegEntry("scale", Register.REGTYPE.VEC2),
            new RegEntry("translate", Register.REGTYPE.VEC2),
            new RegEntry("rotate", Register.REGTYPE.FLOAT),
            new RegEntry("textscale", Register.REGTYPE.FLOAT),
            new RegEntry("visible", Register.REGTYPE.BOOL),
            new RegEntry("noevents", Register.REGTYPE.BOOL),
            new RegEntry("text", Register.REGTYPE.STRING),
            new RegEntry("background", Register.REGTYPE.STRING),
            new RegEntry("runscript", Register.REGTYPE.STRING),
            new RegEntry("varbackground", Register.REGTYPE.STRING),
            new RegEntry("cvar", Register.REGTYPE.STRING),
            new RegEntry("choices", Register.REGTYPE.STRING),
            new RegEntry("choiceVar", Register.REGTYPE.STRING),
            new RegEntry("bind", Register.REGTYPE.STRING),
            new RegEntry("modelRotate", Register.REGTYPE.VEC4),
            new RegEntry("modelOrigin", Register.REGTYPE.VEC4),
            new RegEntry("lightOrigin", Register.REGTYPE.VEC4),
            new RegEntry("lightColor", Register.REGTYPE.VEC4),
            new RegEntry("viewOffset", Register.REGTYPE.VEC4),
            new RegEntry("hideCursor", Register.REGTYPE.BOOL)
        };

        protected float actualX;                    // physical coords
        protected float actualY;                    // ''
        protected int childID;                  // this childs id
        protected internal uint flags;             // visible, focus, mouseover, cursor, border, etc..
        protected int lastTimeRun;              //
        protected internal Rectangle drawRect;           // overall rect
        protected internal Rectangle clientRect;         // client area
        protected internal Vector2 origin;

        protected int timeLine;                 // time stamp used for various fx
        protected float xOffset;
        protected float yOffset;
        protected float forceAspectWidth;
        protected float forceAspectHeight;
        protected internal float matScalex;
        protected internal float matScaley;
        protected internal float borderSize;
        protected internal float textAlignx;
        protected internal float textAligny;
        protected internal string name;
        protected string comment;
        protected internal Vector2 shear;

        protected internal byte textShadow;
        protected internal byte fontNum;
        protected DeviceContext.CURSOR cursor;
        protected internal DeviceContext.ALIGN textAlign;

        protected WinBool noTime;
        protected internal WinBool visible;
        protected WinBool noEvents;
        protected internal WinRectangle rect;                // overall rect
        protected internal WinVec4 backColor;
        protected internal WinVec4 matColor;
        protected internal WinVec4 foreColor;
        protected WinVec4 hoverColor;
        protected internal WinVec4 borderColor;
        protected internal WinFloat textScale;
        protected internal WinFloat rotate;
        protected internal WinStr text;
        protected internal WinBackground backGroundName;

        protected List<WinVar> definedVars = new();
        protected List<WinVar> updateVars = new();

        protected internal Rectangle textRect;           // text extented rect
        protected internal Material background;         // background asset

        protected Window parent;                // parent window
        protected List<Window> children = new();        // child windows
        protected List<DrawWin> drawWindows = new();

        protected Window focusedChild;          // if a child window has the focus
        protected Window captureChild;          // if a child window has mouse capture
        protected Window overChild;         // if a child window has mouse capture
        protected bool hover;

        protected internal DeviceContext dc;

        protected UserInterfaceLocal gui;

        protected static readonly CVar gui_debug = new("gui_debug", "0", CVAR.GUI | CVAR.BOOL, "");
        protected static readonly CVar gui_edit = new("gui_edit", "0", CVAR.GUI | CVAR.BOOL, "");

        protected GuiScriptList[] scripts = new GuiScriptList[(int)SCRIPT.SCRIPT_COUNT];
        protected bool[] saveTemps;

        protected List<TimeLineEvent> timeLineEvents = new();
        protected List<TransitionData> transitions = new();

        protected static bool[] registerIsTemporary = new bool[MAX_EXPRESSION_REGISTERS]; // statics to assist during parsing

        protected List<WexpOp> ops = new();             // evaluate to make expressionRegisters
        protected List<float> expressionRegisters = new();
        protected List<WexpOp> saveOps = new();             // evaluate to make expressionRegisters
        protected List<RVNamedEvent> namedEvents = new();       //  added named events
        protected List<float> saveRegs = new();

        protected RegisterList regList;

        protected internal WinBool hideCursor;

        public virtual WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
        {
            WinVar retVar = null;
            owner?.Invoke(null);
            if (string.Equals(name, "notime", StringComparison.OrdinalIgnoreCase)) retVar = noTime;
            if (string.Equals(name, "background", StringComparison.OrdinalIgnoreCase)) retVar = backGroundName;
            if (string.Equals(name, "visible", StringComparison.OrdinalIgnoreCase)) retVar = visible;
            if (string.Equals(name, "rect", StringComparison.OrdinalIgnoreCase)) retVar = rect;
            if (string.Equals(name, "backColor", StringComparison.OrdinalIgnoreCase)) retVar = backColor;
            if (string.Equals(name, "matColor", StringComparison.OrdinalIgnoreCase)) retVar = matColor;
            if (string.Equals(name, "foreColor", StringComparison.OrdinalIgnoreCase)) retVar = foreColor;
            if (string.Equals(name, "hoverColor", StringComparison.OrdinalIgnoreCase)) retVar = hoverColor;
            if (string.Equals(name, "borderColor", StringComparison.OrdinalIgnoreCase)) retVar = borderColor;
            if (string.Equals(name, "textScale", StringComparison.OrdinalIgnoreCase)) retVar = textScale;
            if (string.Equals(name, "rotate", StringComparison.OrdinalIgnoreCase)) retVar = rotate;
            if (string.Equals(name, "noEvents", StringComparison.OrdinalIgnoreCase)) retVar = noEvents;
            if (string.Equals(name, "text", StringComparison.OrdinalIgnoreCase)) retVar = text;
            if (string.Equals(name, "backGroundName", StringComparison.OrdinalIgnoreCase)) retVar = backGroundName;
            if (string.Equals(name, "hidecursor", StringComparison.OrdinalIgnoreCase)) retVar = hideCursor;

            var key = name;
            var guiVar = key.Contains(WinVar.VAR_GUIPREFIX);
            var c = definedVars.Count;
            for (var i = 0; i < c; i++)
                if (string.Equals(name, guiVar ? definedVars[i].Name : definedVars[i].Name, StringComparison.OrdinalIgnoreCase))
                {
                    retVar = definedVars[i];
                    break;
                }

            if (retVar != null)
            {
                if (winLookup && name[0] != '$') DisableRegister(name);
                if (parent != null) owner?.Invoke(parent.FindChildByName(name));
                return retVar;
            }

            var len = key.Length;
            if (len > 5 && guiVar)
            {
                var var = (WinVar)new WinStr();
                var.Init(name, this);
                definedVars.Add(var);
                return var;
            }
            else if (winLookup)
            {
                var n = key.IndexOf("::");
                if (n > 0)
                {
                    var winName = key.Substring(0, n);
                    var var = key[(key.Length - n - 2)..];
                    var win = Gui.Desktop.FindChildByName(winName);
                    if (win != null)
                        if (win.win != null) return win.win.GetWinVarByName(var, false, owner);
                        else { owner?.Invoke(win); return win.simp.GetWinVarByName(var); }
                }
            }
            return null;
        }

        void CommonInit()
        {
            childID = 0;
            flags = 0;
            lastTimeRun = 0;
            origin.Zero();
            fontNum = 0;
            timeLine = -1;
            xOffset = yOffset = 0f;
            cursor = 0;
            forceAspectWidth = 640f;
            forceAspectHeight = 480f;
            matScalex = 1f;
            matScaley = 1f;
            borderSize = 0f;
            noTime = false;
            visible = true;
            textAlign = 0;
            textAlignx = 0f;
            textAligny = 0f;
            noEvents = false;
            rotate = 0;
            shear.Zero();
            textScale = 0.35f;
            backColor.Zero();
            foreColor = new Vector4(1f, 1f, 1f, 1f);
            hoverColor = new Vector4(1f, 1f, 1f, 1f);
            matColor = new Vector4(1f, 1f, 1f, 1f);
            borderColor.Zero();
            background = null;
            backGroundName = "";
            focusedChild = null;
            captureChild = null;
            overChild = null;
            parent = null;
            saveOps = null;
            saveRegs = null;
            timeLine = -1;
            textShadow = 0;
            hover = false;

            for (var i = 0; i < scripts.Length; i++) scripts[i] = null;

            hideCursor = false;
        }

        public Window(UserInterfaceLocal gui)
        {
            this.dc = null;
            this.gui = gui;
            CommonInit();
        }
        public Window(DeviceContext dc, UserInterfaceLocal gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        //public int Size => 0;
        public virtual int Allocated => 0;

        public DeviceContext DC
        {
            get => dc;
            set
            {
                dc = value;
                //if ((flags & WIN_DESKTOP) != 0)
                dc.SetSize(forceAspectWidth, forceAspectHeight);
                var c = children.Count;
                for (var i = 0; i < c; i++)
                    children[i].DC = value;
            }
        }

        public void CleanUp()
        {
            // ensure the register list gets cleaned up
            regList.Reset();

            // Cleanup the named events
            namedEvents.Clear();

            // Cleanup the operations and update vars (if it is not fixed, orphane register references are possible)
            ops.Clear();
            updateVars.Clear();

            drawWindows.Clear();
            children.Clear();
            definedVars.Clear();
            timeLineEvents.Clear();
            for (var i = 0; i < scripts.Length; i++) scripts[i] = null;
            CommonInit();
        }

        public Window SetFocus(Window w, bool scripts = true)
        {
            // only one child can have the focus
            Window lastFocus = null;
            if ((w.flags & WIN_CANFOCUS) != 0)
            {
                lastFocus = gui.Desktop.focusedChild;
                if (lastFocus != null) { lastFocus.flags &= ~WIN_FOCUS; lastFocus.LoseFocus(); }

                // call on lose focus (calling this broke all sorts of guis)
                //if (scripts && lastFocus != null) lastFocus.RunScript(SCRIPT.ON_MOUSEEXIT);
                // call on gain focus (calling this broke all sorts of guis)
                //if (scripts && w!=null) w.RunScript(ON_MOUSEENTER);

                w.flags |= WIN_FOCUS;
                w.GainFocus();
                gui.Desktop.focusedChild = w;
            }

            return lastFocus;
        }

        public Window SetCapture(Window w)
        {
            // only one child can have the focus
            Window last = null;
            var c = children.Count;
            for (var i = 0; i < c; i++)
                if ((children[i].flags & WIN_CAPTURE) != 0)
                {
                    last = children[i];
                    //last.flags &= ~WIN_CAPTURE;
                    last.LoseCapture();
                    break;
                }

            w.flags |= WIN_CAPTURE;
            w.GainCapture();
            gui.Desktop.captureChild = w;
            return last;
        }

        public void SetParent(Window w)
            => parent = w;

        public void SetFlag(uint f)
            => flags |= f;

        public void ClearFlag(uint f)
            => flags &= ~f;

        public uint Flags
        {
            get => flags;
            set => flags = value;
        }

        public void Move(float x, float y)
        {
            var rct = new Rectangle(rect) { x = x, y = y };
            var reg = RegList.FindReg("rect");
            reg?.Enable(false);
            rect = rct;
        }

        public void BringToTop(Window w)
        {
            if (w != null && (w.flags & WIN_MODAL) == 0) return;

            var c = children.Count;
            for (var i = 0; i < c; i++)
                if (children[i] == w)
                {
                    // this is it move from i - 1 to 0 to i to 1 then shove this one into 0
                    for (var j = i + 1; j < c; j++) children[j - 1] = children[j];
                    children[c - 1] = w;
                    break;
                }
        }

        public void Size(float x, float y, float w, float h)
        {
            rect = new Rectangle(rect)
            {
                x = x,
                y = y,
                w = w,
                h = h
            };
            CalcClientRect(0, 0);
        }

        public void SetupFromState()
        {
            background = null;

            SetupBackground();

            if (borderSize != 0) flags |= WIN_BORDER;
            if (regList.FindReg("rotate") != null || regList.FindReg("shear") != null) flags |= WIN_TRANSFORM;

            CalcClientRect(0, 0);
            if (scripts[(int)SCRIPT.ON_ACTION] != null) { cursor = DeviceContext.CURSOR.HAND; flags |= WIN_CANFOCUS; }
        }

        public void SetupBackground()
        {
            if (backGroundName.Length != 0)
            {
                background = declManager.FindMaterial(backGroundName);
                background.SetImageClassifications(1);  // just for resource tracking
                if (background != null && !background.TestMaterialFlag(MF.DEFAULTED)) background.Sort = (float)SS.GUI;
            }
            backGroundName.SetMaterialPtr(x => background = x);
        }

        static readonly DrawWin FindChildByName_dw = new();
        public DrawWin FindChildByName(string name)
        {
            if (string.Equals(this.name, name, StringComparison.OrdinalIgnoreCase))
            {
                FindChildByName_dw.simp = null;
                FindChildByName_dw.win = this;
                return FindChildByName_dw;
            }
            var c = drawWindows.Count;
            for (var i = 0; i < c; i++)
                if (drawWindows[i].win != null)
                {
                    if (string.Equals(drawWindows[i].win.name, name, StringComparison.OrdinalIgnoreCase)) return drawWindows[i];
                    var win = drawWindows[i].win.FindChildByName(name);
                    if (win != null) return win;
                }
                else if (string.Equals(drawWindows[i].simp.name, name, StringComparison.OrdinalIgnoreCase)) return drawWindows[i];
            return null;
        }

        public SimpleWindow FindSimpleWinByName(string name)
        {
            var c = drawWindows.Count;
            for (var i = 0; i < c; i++)
            {
                if (drawWindows[i].simp == null)
                    continue;
                if (string.Equals(drawWindows[i].simp.name, name, StringComparison.OrdinalIgnoreCase))
                    return drawWindows[i].simp;
            }
            return null;
        }

        public Window Parent => parent;

        public UserInterfaceLocal Gui => gui;

        public bool Contains(float x, float y)
        {
            var r = new Rectangle(drawRect) { x = actualX, y = actualY };
            return r.Contains(x, y);
        }

        public string GetStrPtrByName(string name)
            => null;

        enum OFFSET
        {
            RECT = 1,
            BACKCOLOR,
            MATCOLOR,
            FORECOLOR,
            HOVERCOVER,
            BORDERCOLOR,
            TEXTSCALE,
            ROTATE,
        }

        public int GetWinVarOffset(WinVar wv, DrawWin owner)
        {
            var ret = -1;
            if (wv == rect) ret = (int)OFFSET.RECT;
            if (wv == backColor) ret = (int)OFFSET.BACKCOLOR;
            if (wv == matColor) ret = (int)OFFSET.MATCOLOR;
            if (wv == foreColor) ret = (int)OFFSET.FORECOLOR;
            if (wv == hoverColor) ret = (int)OFFSET.HOVERCOVER;
            if (wv == borderColor) ret = (int)OFFSET.BORDERCOLOR;
            if (wv == textScale) ret = (int)OFFSET.TEXTSCALE;
            if (wv == rotate) ret = (int)OFFSET.ROTATE;
            if (ret != -1) { owner.win = this; return ret; }

            for (var i = 0; i < drawWindows.Count; i++)
            {
                ret = drawWindows[i].win != null ? drawWindows[i].win.GetWinVarOffset(wv, owner) : drawWindows[i].simp.GetWinVarOffset(wv, owner);
                if (ret != -1) break;
            }

            return ret;
        }

        public float MaxCharHeight { get { SetFont(); return dc.MaxCharHeight(textScale); } }

        public float MaxCharWidth { get { SetFont(); return dc.MaxCharWidth(textScale); } }

        public void SetFont() => dc.SetFont(fontNum);

        public void SetInitialState(string name)
        {
            this.name = name;
            matScalex = 1f;
            matScaley = 1f;
            forceAspectWidth = 640f;
            forceAspectHeight = 480f;
            noTime = false;
            visible = true;
            flags = 0;
        }

        public void AddChild(Window win)
            => win.childID = children.Add_(win);

        public void DebugDraw(int time, float x, float y)
        {
            if (dc != null)
            {
                dc.EnableClipping(false);
                if (gui_debug.Integer == 1) dc.DrawRect(drawRect.x, drawRect.y, drawRect.w, drawRect.h, 1, DeviceContext.colorRed);
                else if (gui_debug.Integer == 2)
                {
                    var str = (string)text;
                    var buff = (str.Length != 0 ? $"{str}\n" : "") +
                        $"Rect: {rect.x:0.1}, {rect.y:0.1}, {rect.w:0.1}, {rect.h:0.1}\n" +
                        $"Draw Rect: {drawRect.x:0.1}, {drawRect.y:0.1}, {drawRect.w:0.1}, {drawRect.h:0.1}\n" +
                        $"Client Rect: {clientRect.x:0.1}, {clientRect.y:0.1}, {clientRect.w:0.1}, {clientRect.h:0.1}\n" +
                        $"Cursor: {gui.CursorX:0.1} : {gui.CursorY:0.1}\n";
                    dc.DrawText(buff, textScale, textAlign, foreColor, textRect, true);
                }
                dc.EnableClipping(true);
            }
        }

        public void CalcClientRect(float xofs, float yofs)
        {
            drawRect = rect;

            if ((flags & WIN_INVERTRECT) != 0)
            {
                drawRect.x = rect.x - rect.w;
                drawRect.y = rect.y - rect.h;
            }

            if ((flags & (WIN_HCENTER | WIN_VCENTER)) != 0 && parent != null)
                // in this case treat xofs and yofs as absolute top left coords and ignore the original positioning
                if ((flags & WIN_HCENTER) != 0) drawRect.x = (parent.rect.w - rect.w) / 2;
                else drawRect.y = (parent.rect.h - rect.h) / 2;

            drawRect.x += xofs;
            drawRect.y += yofs;

            clientRect = drawRect;
            if (rect.h > 0f && rect.w > 0f)
            {
                if ((flags & WIN_BORDER) != 0 && borderSize != 0f)
                {
                    clientRect.x += borderSize;
                    clientRect.y += borderSize;
                    clientRect.w -= borderSize;
                    clientRect.h -= borderSize;
                }

                textRect = clientRect;
                textRect.x += 2f;
                textRect.w -= 2f;
                textRect.y += 2f;
                textRect.h -= 2f;

                textRect.x += textAlignx;
                textRect.y += textAligny;
            }
            origin.Set(rect.x + (rect.w / 2), rect.y + (rect.h / 2));
        }

        public void DrawBorderAndCaption(Rectangle drawRect)
        {
            if ((flags & WIN_BORDER) != 0 && borderSize != 0 && borderColor.w != 0) dc.DrawRect(drawRect.x, drawRect.y, drawRect.w, drawRect.h, borderSize, borderColor);
        }

        //public void DrawCaption(int time, float x, float y);

        static Matrix3x3 SetupTransforms_trans;
        static Vector3 SetupTransforms_org;
        static Rotation SetupTransforms_rot;
        static Vector3 SetupTransforms_vec = new(0, 0, 1);
        static Matrix3x3 SetupTransforms_smat;
        public void SetupTransforms(float x, float y)
        {
            SetupTransforms_trans.Identity();
            SetupTransforms_org.Set(origin.x + x, origin.y + y, 0);

            if (rotate != null)
            {
                SetupTransforms_rot.Set(SetupTransforms_org, SetupTransforms_vec, rotate);
                SetupTransforms_trans = SetupTransforms_rot.ToMat3();
            }

            if (shear.x != 0 || shear.y != 0)
            {
                SetupTransforms_smat.Identity();
                SetupTransforms_smat[0][1] = shear.x;
                SetupTransforms_smat[1][0] = shear.y;
                SetupTransforms_trans *= SetupTransforms_smat;
            }

            if (!SetupTransforms_trans.IsIdentity()) dc.SetTransformInfo(SetupTransforms_org, SetupTransforms_trans);
        }

        public bool Contains(Rectangle sr, float x, float y)
        {
            var r = new Rectangle(sr);
            r.x += actualX - drawRect.x; r.y += actualY - drawRect.y;
            return r.Contains(x, y);
        }

        public string Name
            => name;

        public virtual bool Parse(Parser src, bool rebuild = true)
        {
            Token token2;
            string work;

            if (rebuild) CleanUp();

            DrawWin dwt = new();

            timeLineEvents.Clear();
            transitions.Clear();
            namedEvents.Clear();

            src.ExpectTokenType(TT.NAME, 0, out var token);
            SetInitialState(token);
            src.ExpectTokenString("{");
            src.ExpectAnyToken(out token);

            var ret = true;

            // attach a window wrapper to the window if the gui editor is running
#if ID_ALLOW_TOOLS
            if ((C.com_editors & EDITOR_GUI) != 0) new rvGEWindowWrapper(this, rvGEWindowWrapper.WT_NORMAL);
#endif

            while (token != "}")
            {
                // track what was parsed so we can maintain it for the guieditor
                src.SetMarker();

                if (token == "windowDef" || token == "animationDef")
                {
                    if (token == "animationDef")
                    {
                        visible = false;
                        rect = new Rectangle(0, 0, 0, 0);
                    }
                    src.ExpectTokenType(TT.NAME, 0, out token); token2 = token; src.UnreadToken(token);
                    var dw = FindChildByName(token2);
                    if (dw != null && dw.win != null)
                    {
                        SaveExpressionParseState();
                        dw.win.Parse(src, rebuild);
                        RestoreExpressionParseState();
                    }
                    else
                    {
                        var win = new Window(dc, gui);
                        SaveExpressionParseState();
                        win.Parse(src, rebuild);
                        RestoreExpressionParseState();
                        win.SetParent(this);
                        dwt.simp = null;
                        dwt.win = null;
                        if (win.IsSimple)
                        {
                            var simple = new SimpleWindow(win);
                            dwt.simp = simple;
                            drawWindows.Add(dwt);
                        }
                        else
                        {
                            AddChild(win);
                            SetFocus(win, false);
                            dwt.win = win;
                            drawWindows.Add(dwt);
                        }
                    }
                }
                else if (token == "editDef")
                {
                    var win = new EditWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "choiceDef")
                {
                    var win = new ChoiceWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "sliderDef")
                {
                    var win = new SliderWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "markerDef")
                {
                    var win = new MarkerWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "bindDef")
                {
                    var win = new BindWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "listDef")
                {
                    var win = new ListWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "fieldDef")
                {
                    var win = new FieldWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "renderDef")
                {
                    var win = new RenderWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "gameSSDDef")
                {
                    var win = new GameSSDWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "gameBearShootDef")
                {
                    var win = new GameBearShootWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                else if (token == "gameBustOutDef")
                {
                    var win = new GameBustOutWindow(dc, gui);
                    SaveExpressionParseState();
                    win.Parse(src, rebuild);
                    RestoreExpressionParseState();
                    AddChild(win);
                    win.SetParent(this);
                    dwt.simp = null;
                    dwt.win = win;
                    drawWindows.Add(dwt);
                }
                //  added new onEvent
                else if (token == "onNamedEvent")
                {
                    // Read the event name
                    if (!src.ReadToken(out token)) { src.Error("Expected event name"); return false; }

                    var ev = new RVNamedEvent(token);
                    src.SetMarker();

                    if (!ParseScript(src, ev.mEvent)) { ret = false; break; }

                    // If we are in the gui editor then add the internal var to the the wrapper
#if ID_ALLOW_TOOLS
                    if ((C.com_editors & EDITOR.GUI) != 0)
                    {
                        // Grab the string from the last marker
                        src.GetStringFromMarker(out var str, false);

                        // Parse it one more time to knock unwanted tabs out
                        var src2 = new Lexer(str, str.Length, "", src.Flags);
                        src2.ParseBracedSectionExact(out var o, 1);

                        // Save the script
                        rvGEWindowWrapper.GetWrapper(this).ScriptDict[$"onEvent {token}"] = o;
                    }
#endif
                    namedEvents.Add(ev);
                }
                else if (token == "onTime")
                {
                    var ev = new TimeLineEvent();

                    if (!src.ReadToken(out token)) { src.Error("Unexpected end of file"); return false; }
                    ev.time = intX.Parse(token);

                    // reset the mark since we dont want it to include the time
                    src.SetMarker();

                    if (!ParseScript(src, ev.event_, ev.time)) { ret = false; break; }

                    // add the script to the wrappers script list If we are in the gui editor then add the internal var to the the wrapper
#if ID_ALLOW_TOOLS
                    if ((C.com_editors & EDITOR.GUI) != 0)
                    {
                        // Grab the string from the last marker
                        src.GetStringFromMarker(out var str, false);

                        // Parse it one more time to knock unwanted tabs out
                        var src2 = new Lexer(str, str.Length, "", src.Flags);
                        src2.ParseBracedSectionExact(out var o, 1);

                        // Save the script
                        rvGEWindowWrapper.GetWrapper(this).ScriptDict[$"onTime {ev.time}"] = o;
                    }
#endif
                    // this is a timeline event
                    ev.pending = true;
                    timeLineEvents.Add(ev);
                }

                else if (token == "definefloat")
                {
                    src.ReadToken(out token); work = ((string)token).ToLowerInvariant();
                    var varf = new WinFloat { Name = work };
                    definedVars.Add(varf);

                    // add the float to the editors wrapper dict Set the marker after the float name
                    src.SetMarker();

                    // Read in the float
                    regList.AddReg(work, Register.REGTYPE.FLOAT, src, this, varf);

                    // If we are in the gui editor then add the float to the defines
#if ID_ALLOW_TOOLS
                    if ((C.com_editors & EDITOR.GUI) != 0)
                    {
                        // Grab the string from the last marker and save it in the wrapper
                        src.GetStringFromMarker(out var str, true);
                        rvGEWindowWrapper.GetWrapper(this).VariableDict[$"definefloat\t\"{token}\""] = str;
                    }
#endif
                }
                else if (token == "definevec4")
                {
                    src.ReadToken(out token); work = ((string)token).ToLowerInvariant();
                    var var = new WinVec4 { Name = work };

                    // set the marker so we can determine what was parsed set the marker after the vec4 name
                    src.SetMarker();

                    // FIXME: how about we add the var to the desktop instead of this window so it won't get deleted
                    //        when this window is destoyed which even happens during parsing with simple windows ?
                    //definedVars.Add(var);
                    gui.Desktop.definedVars.Add(var);
                    gui.Desktop.regList.AddReg(work, Register.REGTYPE.VEC4, src, gui.Desktop, var);

                    // store the original vec4 for the editor If we are in the gui editor then add the float to the defines
#if ID_ALLOW_TOOLS
                    if ((C.com_editors & EDITOR.GUI)!=0)
                    {
                        // Grab the string from the last marker and save it in the wrapper
                        src.GetStringFromMarker(out var str, true);
                        rvGEWindowWrapper.GetWrapper(this).VariableDict[$"definevec4\t\"{token}\""] = str;
                    }
#endif
                }
                else if (token == "float")
                {
                    src.ReadToken(out token); work = ((string)token).ToLowerInvariant();
                    var varf = new WinFloat { Name = work };
                    definedVars.Add(varf);

                    // add the float to the editors wrapper dict set the marker to after the float name
                    src.SetMarker();

                    // Parse the float
                    regList.AddReg(work, Register.REGTYPE.FLOAT, src, this, varf);

                    // If we are in the gui editor then add the float to the defines
#if ID_ALLOW_TOOLS
                    if ((C.com_editors & EDITOR.GUI) != 0)
                    {
                        // Grab the string from the last marker and save it in the wrapper
                        src.GetStringFromMarker(out var str, true);
                        rvGEWindowWrapper.GetWrapper(this).VariableDict[$"float\t\"{token}\""] = str;
                    }
#endif
                }
                else if (ParseScriptEntry(token, src))
                {
                    // add the script to the wrappers script list If we are in the gui editor then add the internal var to the the wrapper
#if ID_ALLOW_TOOLS
                    if ((C.com_editors & EDITOR.GUI) != 0)
                    {
                        // Grab the string from the last marker
                        src.GetStringFromMarker(out var str, false);

                        // Parse it one more time to knock unwanted tabs out
                        var src2 = new Lexer(str, str.Length, "", src.Flags);
                        src2.ParseBracedSectionExact(out var o, 1);

                        // Save the script
                        rvGEWindowWrapper.GetWrapper(this).ScriptDict[token] = o;
                    }
#endif
                }
                else if (ParseInternalVar(token, src))
                {
                    // gui editor support If we are in the gui editor then add the internal var to the the wrapper
#if ID_ALLOW_TOOLS
                    if ((C.com_editors & EDITOR.GUI) != 0)
                    {
                        src.GetStringFromMarker(out var str);
                        rvGEWindowWrapper.GetWrapper(this).SetStateKey(token, str, false);
                    }
#endif
                }
                else
                {
                    ParseRegEntry(token, src);
                    // hook into the main window parsing for the gui editor If we are in the gui editor then add the internal var to the the wrapper
#if ID_ALLOW_TOOLS
                    if ((C.com_editors & EDITOR.GUI) != 0)
                    {
                        src.GetStringFromMarker(out var str);
                        rvGEWindowWrapper.GetWrapper(this).SetStateKey(token, str, false);
                    }
#endif
                }
                if (!src.ReadToken(out token)) { src.Error("Unexpected end of file"); ret = false; break; }
            }

            if (ret) EvalRegs(-1, true);

            SetupFromState();
            PostParse();

            // hook into the main window parsing for the gui editor If we are in the gui editor then add the internal var to the the wrapper
#if ID_ALLOW_TOOLS
            if ((C.com_editors & EDITOR.GUI) != 0) rvGEWindowWrapper.GetWrapper(this).Finish();
#endif
            return ret;
        }

        static bool HandleEvent_actionDownRun;
        static bool HandleEvent_actionUpRun;
        public virtual string HandleEvent(SysEvent ev, Action<bool> updateVisuals = null)
        {
            cmd = "";

            if ((flags & WIN_DESKTOP) != 0)
            {
                HandleEvent_actionDownRun = false;
                HandleEvent_actionUpRun = false;
                if (expressionRegisters.Count != 0 && ops.Count != 0) EvalRegs();
                RunTimeEvents(gui.Time);
                CalcRects(0, 0);
                dc.SetCursor(DeviceContext.CURSOR.ARROW);
            }

            if (visible && !noEvents)
            {
                if (ev.evType == SE.KEY)
                {
                    EvalRegs(-1, true);
                    updateVisuals?.Invoke(true);

                    if (ev.evValue == (int)K_MOUSE1)
                    {
                        if (ev.evValue2 == 0 && CaptureChild != null) { CaptureChild.LoseCapture(); gui.Desktop.captureChild = null; return ""; }

                        var c = children.Count;
                        while (--c >= 0)
                            if (children[c].visible && children[c].Contains(children[c].drawRect, gui.CursorX, gui.CursorY) && !children[c].noEvents)
                            {
                                var child = children[c];
                                if (ev.evValue2 != 0)
                                {
                                    BringToTop(child);
                                    SetFocus(child);
                                    if ((child.flags & WIN_HOLDCAPTURE) != 0) SetCapture(child);
                                }
                                if (child.Contains(child.clientRect, gui.CursorX, gui.CursorY))
                                {
                                    //if ((gui_edit.Bool && (child.flags & WIN_SELECTED) != 0) || (!gui_edit.Bool && (child.flags & WIN_MOVABLE) != 0))
                                    //    SetCapture(child);
                                    SetFocus(child);
                                    var childRet = child.HandleEvent(ev, updateVisuals);
                                    if (!string.IsNullOrEmpty(childRet)) return childRet;
                                    if ((child.flags & WIN_MODAL) != 0) return "";
                                }
                                else if (ev.evValue2 != 0)
                                {
                                    SetFocus(child);
                                    var capture = true;
                                    if (capture && ((child.flags & WIN_MOVABLE) != 0 || gui_edit.Bool)) SetCapture(child);
                                    return "";
                                }
                            }
                        if (ev.evValue2 != 0 && !HandleEvent_actionDownRun)
                            HandleEvent_actionDownRun = RunScript(SCRIPT.ON_ACTION);
                        else if (!HandleEvent_actionUpRun)
                            HandleEvent_actionUpRun = RunScript(SCRIPT.ON_ACTIONRELEASE);
                    }
                    else if (ev.evValue == (int)K_MOUSE2)
                    {
                        if (ev.evValue2 == 0 && CaptureChild != null)
                        {
                            CaptureChild.LoseCapture();
                            gui.Desktop.captureChild = null;
                            return "";
                        }

                        var c = children.Count;
                        while (--c >= 0)
                            if (children[c].visible && children[c].Contains(children[c].drawRect, gui.CursorX, gui.CursorY) && !children[c].noEvents)
                            {
                                var child = children[c];
                                if (ev.evValue2 != 0)
                                {
                                    BringToTop(child);
                                    SetFocus(child);
                                }
                                if (child.Contains(child.clientRect, gui.CursorX, gui.CursorY) || CaptureChild == child)
                                {
                                    if ((gui_edit.Bool && (child.flags & WIN_SELECTED) != 0) || (!gui_edit.Bool && (child.flags & WIN_MOVABLE) != 0))
                                        SetCapture(child);
                                    var childRet = child.HandleEvent(ev, updateVisuals);
                                    if (!string.IsNullOrEmpty(childRet)) return childRet;
                                    if ((child.flags & WIN_MODAL) != 0) return "";
                                }
                            }
                    }
                    else if (ev.evValue == (int)K_MOUSE3)
                    {
                        if (gui_edit.Bool)
                        {
                            var c = children.Count;
                            for (var i = 0; i < c; i++)
                                if (children[i].drawRect.Contains(gui.CursorX, gui.CursorY))
                                    if (ev.evValue2 != 0)
                                    {
                                        children[i].flags ^= WIN_SELECTED;
                                        if ((children[i].flags & WIN_SELECTED) != 0) { flags &= ~WIN_SELECTED; return "childsel"; }
                                    }
                        }
                    }
                    else if (ev.evValue == (int)K_TAB && ev.evValue2 != 0)
                    {
                        if (FocusedChild != null)
                        {
                            var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                            if (!string.IsNullOrEmpty(childRet)) return childRet;

                            // If the window didn't handle the tab, then move the focus to the next window or the previous window if shift is held down
                            var direction = KeyInput.IsDown(K_SHIFT) ? -1 : 1;

                            var currentFocus = FocusedChild;
                            var child = FocusedChild;
                            var parent = child.Parent;
                            while (parent != null)
                            {
                                var foundFocus = false;
                                var recurse = false;
                                var index = 0;
                                if (child != null) index = parent.GetChildIndex(child) + direction;
                                else if (direction < 0) index = parent.ChildCount - 1;
                                while (index < parent.ChildCount && index >= 0)
                                {
                                    var testWindow = parent.GetChild(index);
                                    // we managed to wrap around and get back to our starting window
                                    if (testWindow == currentFocus) { foundFocus = true; break; }
                                    if (testWindow != null && !testWindow.noEvents && testWindow.visible)
                                        if ((testWindow.flags & WIN_CANFOCUS) != 0) { SetFocus(testWindow); foundFocus = true; break; }
                                        else if (testWindow.ChildCount > 0) { parent = testWindow; child = null; recurse = true; break; }
                                    index += direction;
                                }
                                // We found a child to focus on
                                if (foundFocus) break;
                                // We found a child with children
                                else if (recurse) continue;
                                else
                                {
                                    // We didn't find anything, so go back up to our parent
                                    child = parent;
                                    parent = child.Parent;
                                    // We got back to the desktop, so wrap around but don't actually go to the desktop
                                    if (parent == gui.Desktop) { parent = null; child = null; }
                                }
                            }
                        }
                    }
                    else if (ev.evValue == (int)K_ESCAPE && ev.evValue2 != 0)
                    {
                        if (FocusedChild != null)
                        {
                            var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                            if (!string.IsNullOrEmpty(childRet)) return childRet;
                        }
                        RunScript(SCRIPT.ON_ESC);
                    }
                    else if (ev.evValue == (int)K_ENTER)
                    {
                        if (FocusedChild != null)
                        {
                            var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                            if (!string.IsNullOrEmpty(childRet)) return childRet;
                        }
                        if ((flags & WIN_WANTENTER) != 0) RunScript(ev.evValue2 != 0 ? SCRIPT.ON_ACTION : SCRIPT.ON_ACTIONRELEASE);
                    }
                    else if (FocusedChild != null)
                    {
                        var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                        if (!string.IsNullOrEmpty(childRet)) return childRet;
                    }
                }
                else if (ev.evType == SE.MOUSE)
                {
                    updateVisuals?.Invoke(true);
                    var mouseRet = RouteMouseCoords(ev.evValue, ev.evValue2);
                    if (!string.IsNullOrEmpty(mouseRet)) return mouseRet;
                }
                else if (ev.evType == SE.NONE) { }
                else if (ev.evType == SE.CHAR)
                {
                    if (FocusedChild != null)
                    {
                        var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                        if (!string.IsNullOrEmpty(childRet)) return childRet;
                    }
                }
            }

            gui.ReturnCmd = cmd;
            if (gui.PendingCmd.Length != 0)
            {
                gui.ReturnCmd += " ; ";
                gui.ReturnCmd += gui.PendingCmd;
                gui.PendingCmd = string.Empty;
            }
            cmd = "";
            return gui.ReturnCmd;
        }

        public void CalcRects(float x, float y)
        {
            CalcClientRect(0, 0);
            drawRect.Offset(x, y);
            clientRect.Offset(x, y);
            actualX = drawRect.x;
            actualY = drawRect.y;
            var c = drawWindows.Count;
            for (var i = 0; i < c; i++) drawWindows[i].win?.CalcRects(clientRect.x + xOffset, clientRect.y + yOffset);
            drawRect.Offset(-x, -y);
            clientRect.Offset(-x, -y);
        }

        public virtual void Redraw(float x, float y)
        {
            if (R.r_skipGuiShaders.Integer == 1 || dc == null) return;

            var time = gui.Time;

            if ((flags & WIN_DESKTOP) != 0 && R.r_skipGuiShaders.Integer != 3) RunTimeEvents(time);

            if (R.r_skipGuiShaders.Integer == 2) return;

            var fixupFor43 = false;
            if (!cvarSystem.GetCVarBool("draw_pda")) // only do the following if we aren't drawing pda
                if ((flags & WIN_DESKTOP) != 0) // DG: allow scaling menus to 4:3
                                                // only scale desktop windows (will automatically scale its sub-windows) that EITHER have the scaleto43 flag set OR are fullscreen menus and r_scaleMenusTo43 is 1
                    if ((flags & WIN_SCALETO43) != 0 || ((flags & WIN_MENUGUI) != 0 && R.r_scaleMenusTo43.Bool)) { fixupFor43 = true; dc.SetMenuScaleFix(true); }

            if ((flags & WIN_SHOWTIME) != 0) dc.DrawText($" {(float)(time - timeLine) / 1000:0.1} seconds\n{gui.State.GetString("name")}", 0.35f, 0, DeviceContext.colorWhite, new Rectangle(100, 0, 80, 80), false);

            if ((flags & WIN_SHOWCOORDS) != 0)
            {
                dc.EnableClipping(false);
                dc.DrawText($"x: {(int)rect.x} y: {(int)rect.y}  cursorx: {(int)gui.CursorX} cursory: {(int)gui.CursorY}", 0.25f, 0, DeviceContext.colorWhite, new Rectangle(0, 0, 100, 20), false);
                dc.EnableClipping(true);
            }

            if (!visible)
            {
                if (fixupFor43) dc.SetMenuScaleFix(false); // DG: gotta reset that before returning this function
                return;
            }

            CalcClientRect(0, 0);

            SetFont();
            //if (flags & WIN_DESKTOP) {
            // see if this window forces a new aspect ratio
            dc.SetSize(forceAspectWidth, forceAspectHeight);
            //}

            //FIXME: go to screen coord tracking
            drawRect.Offset(x, y);
            clientRect.Offset(x, y);
            textRect.Offset(x, y);
            actualX = drawRect.x;
            actualY = drawRect.y;

            dc.GetTransformInfo(out var oldOrg, out var oldTrans);

            SetupTransforms(x, y);
            DrawBackground(drawRect);
            DrawBorderAndCaption(drawRect);

            if ((flags & WIN_NOCLIP) == 0) dc.PushClipRect(clientRect);

            if (R.r_skipGuiShaders.Integer < 5) Draw(time, x, y);

            if (gui_debug.Integer != 0) DebugDraw(time, x, y);

            var c = drawWindows.Count;
            for (var i = 0; i < c; i++)
                if (drawWindows[i].win != null) drawWindows[i].win.Redraw(clientRect.x + xOffset, clientRect.y + yOffset);
                else drawWindows[i].simp.Redraw(clientRect.x + xOffset, clientRect.y + yOffset);

            // Put transforms back to what they were before the children were processed
            dc.SetTransformInfo(oldOrg, oldTrans);

            if ((flags & WIN_NOCLIP) == 0) dc.PopClipRect();

            if (gui_edit.Bool || ((flags & WIN_DESKTOP) != 0 && (flags & WIN_NOCURSOR) == 0 && !hideCursor && (gui.Active || (flags & WIN_MENUGUI) != 0)))
            {
                dc.SetTransformInfo(Vector3.origin, Matrix3x3.identity);
                gui.DrawCursor();
            }

            if (gui_debug.Integer != 0 && (flags & WIN_DESKTOP) != 0)
            {
                dc.EnableClipping(false);
                dc.DrawText($"x: {gui.CursorX:1} y: {gui.CursorY:1}", 0.25f, 0, DeviceContext.colorWhite, new Rectangle(0, 0, 100, 20), false);
                dc.DrawText(gui.SourceFile, 0.25f, 0, DeviceContext.colorWhite, new Rectangle(0, 20, 300, 20), false);
                dc.EnableClipping(true);
            }

            if (fixupFor43) dc.SetMenuScaleFix(false); // DG: gotta reset that before returning this function

            drawRect.Offset(-x, -y);
            clientRect.Offset(-x, -y);
            textRect.Offset(-x, -y);
        }

        public virtual void ArchiveToDictionary(Dictionary<string, string> dict, bool useNames = true)
        {
            //FIXME: rewrite without state
            var c = children.Count;
            for (var i = 0; i < c; i++) children[i].ArchiveToDictionary(dict);
        }

        public virtual void InitFromDictionary(Dictionary<string, string> dict, bool byName = true)
        {
            //FIXME: rewrite without state
            var c = children.Count;
            for (var i = 0; i < c; i++) children[i].InitFromDictionary(dict);
        }

        public virtual void PostParse() { }

        public virtual void Activate(bool activate, ref string act)
        {
            var n = activate ? SCRIPT.ON_ACTIVATE : SCRIPT.ON_DEACTIVATE;

            //  make sure win vars are updated before activation
            UpdateWinVars();

            RunScript(n);
            var c = children.Count;
            for (var i = 0; i < c; i++) children[i].Activate(activate, ref act);

            if (act.Length != 0)
                act += " ; ";
        }

        public virtual void Trigger()
        {
            RunScript(SCRIPT.ON_TRIGGER);
            var c = children.Count;
            for (var i = 0; i < c; i++) children[i].Trigger();
            StateChanged(true);
        }

        public virtual void GainFocus() { }

        public virtual void LoseFocus() { }

        public virtual void GainCapture() { }

        public virtual void LoseCapture()
            => flags &= ~WIN_CAPTURE;

        public virtual void Sized() { }

        public virtual void Moved() { }

        public virtual void Draw(int time, float x, float y)
        {
            if (text.Length == 0)
                return;
            if (textShadow != 0)
            {
                var shadowText = text;
                var shadowRect = new Rectangle(textRect);

                shadowText.RemoveColors();
                shadowRect.x += textShadow;
                shadowRect.y += textShadow;

                dc.DrawText(shadowText, textScale, textAlign, DeviceContext.colorBlack, shadowRect, (flags & WIN_NOWRAP) == 0, -1);
            }
            dc.DrawText(text, textScale, textAlign, foreColor, textRect, (flags & WIN_NOWRAP) == 0, -1);

            if (gui_edit.Bool)
            {
                dc.EnableClipping(false);
                dc.DrawText($"x: {(int)rect.x}  y: {(int)rect.y}", 0.25f, 0, DeviceContext.colorWhite, new Rectangle(rect.x, rect.y - 15, 100, 20), false);
                dc.DrawText($"w: {(int)rect.w}  h: {(int)rect.h}", 0.25f, 0, DeviceContext.colorWhite, new Rectangle(rect.x + rect.w, rect.w + rect.h + 5, 100, 20), false);
                dc.EnableClipping(true);
            }
        }

        public virtual void MouseExit()
        {
            if (noEvents) return;
            RunScript(SCRIPT.ON_MOUSEEXIT);
        }

        public virtual void MouseEnter()
        {
            if (noEvents) return;
            RunScript(SCRIPT.ON_MOUSEENTER);
        }

        public virtual void DrawBackground(Rectangle drawRect)
        {
            if (backColor.w != 0) dc.DrawFilledRect(drawRect.x, drawRect.y, drawRect.w, drawRect.h, backColor);

            if (background != null && matColor.w != 0)
            {
                float scalex, scaley;
                if ((flags & WIN_NATURALMAT) != 0) { scalex = drawRect.w / background.ImageWidth; scaley = drawRect.h / background.ImageHeight; }
                else { scalex = matScalex; scaley = matScaley; }
                dc.DrawMaterial(drawRect.x, drawRect.y, drawRect.w, drawRect.h, background, matColor, scalex, scaley);
            }
        }

        public virtual string RouteMouseCoords(float xd, float yd)
        {
            string str;
            //FIXME: unkludge this whole mechanism
            if (CaptureChild != null) return CaptureChild.RouteMouseCoords(xd, yd);

            if (xd == -2000f || yd == -2000f) return "";

            var c = children.Count;
            while (c > 0)
            {
                var child = children[--c];
                if (child.visible && !child.noEvents && child.Contains(child.drawRect, gui.CursorX, gui.CursorY))
                {
                    dc.SetCursor(child.cursor);
                    child.hover = true;

                    if (overChild != child)
                    {
                        if (overChild != null)
                        {
                            overChild.MouseExit();
                            str = overChild.cmd;
                            if (str.Length != 0) { gui.Desktop.AddCommand(str); overChild.cmd = ""; }
                        }
                        overChild = child;
                        overChild.MouseEnter();
                        str = overChild.cmd;
                        if (str.Length != 0) { gui.Desktop.AddCommand(str); overChild.cmd = ""; }
                    }
                    else if ((child.flags & WIN_HOLDCAPTURE) == 0) child.RouteMouseCoords(xd, yd);
                    return "";
                }
            }
            if (overChild != null)
            {
                overChild.MouseExit();
                str = overChild.cmd;
                if (str.Length != 0) { gui.Desktop.AddCommand(str); overChild.cmd = ""; }
                overChild = null;
            }
            return "";
        }

        public virtual void SetBuddy(Window buddy) { }

        public virtual void HandleBuddyUpdate(Window buddy) { }

        public virtual void StateChanged(bool redraw)
        {
            UpdateWinVars();

            if (expressionRegisters.Count != 0 && ops.Count != 0) EvalRegs();

            var c = drawWindows.Count;
            for (var i = 0; i < c; i++)
                if (drawWindows[i].win != null) drawWindows[i].win.StateChanged(redraw);
                else drawWindows[i].simp.StateChanged(redraw);

            if (redraw)
            {
                if ((flags & WIN_DESKTOP) != 0) Redraw(0, 0);
                if (background != null && background.CinematicLength != 0) background.UpdateCinematic(gui.Time);
            }
        }

        public virtual void ReadFromDemoFile(VFileDemo f, bool rebuild = true)
        {
            // should never hit unless we re-enable WRITE_GUIS
#if !WRITE_GUIS
            Debug.Assert(false);
#else

            if (rebuild) CommonInit();

            f.SetLog(true, "window1");
            backGroundName = f.ReadHashString();
            f.SetLog(true, backGroundName);
            background = backGroundName[0] != 0 ? declManager.FindMaterial(backGroundName) : null;
            f.ReadUnsignedChar(cursor);
            f.ReadUnsignedInt(flags);
            f.ReadInt(timeLine);
            f.ReadInt(lastTimeRun);
            idRectangle rct = rect;
            f.ReadFloat(rct.x);
            f.ReadFloat(rct.y);
            f.ReadFloat(rct.w);
            f.ReadFloat(rct.h);
            f.ReadFloat(drawRect.x);
            f.ReadFloat(drawRect.y);
            f.ReadFloat(drawRect.w);
            f.ReadFloat(drawRect.h);
            f.ReadFloat(clientRect.x);
            f.ReadFloat(clientRect.y);
            f.ReadFloat(clientRect.w);
            f.ReadFloat(clientRect.h);
            f.ReadFloat(textRect.x);
            f.ReadFloat(textRect.y);
            f.ReadFloat(textRect.w);
            f.ReadFloat(textRect.h);
            f.ReadFloat(xOffset);
            f.ReadFloat(yOffset);
            int i, c;

            idStr work;
            if (rebuild)
            {
                f.SetLog(true, (work + "-scripts"));
                for (i = 0; i < SCRIPT_COUNT; i++)
                {
                    bool b;
                    f.ReadBool(b);
                    if (b)
                    {
                        delete scripts[i];
                        scripts[i] = new idGuiScriptList;
                        scripts[i].ReadFromDemoFile(f);
                    }
                }

                f.SetLog(true, (work + "-timelines"));
                f.ReadInt(c);
                for (i = 0; i < c; i++)
                {
                    idTimeLineEvent* tl = new idTimeLineEvent;
                    f.ReadInt(tl.time);
                    f.ReadBool(tl.pending);
                    tl.event.ReadFromDemoFile(f);
			        if (rebuild) timeLineEvents.Append(tl);
			        else {
				        assert(i<timeLineEvents.Num());
				        timeLineEvents[i].time = tl.time;
				        timeLineEvents[i].pending = tl.pending;
			        }
		        }
	        }

	f.SetLog(true, (work + "-transitions"));
f.ReadInt(c);
for (i = 0; i < c; i++)
{
    idTransitionData td;
    td.data = NULL;
    f.ReadInt(td.offset);

    float startTime, accelTime, linearTime, decelTime;
    idVec4 startValue, endValue;
    f.ReadFloat(startTime);
    f.ReadFloat(accelTime);
    f.ReadFloat(linearTime);
    f.ReadFloat(decelTime);
    f.ReadVec4(startValue);
    f.ReadVec4(endValue);
    td.interp.Init(startTime, accelTime, decelTime, accelTime + linearTime + decelTime, startValue, endValue);

    // read this for correct data padding with the win32 savegames
    // the extrapolate is correctly initialized through the above Init call
    int extrapolationType;
    float duration;
    idVec4 baseSpeed, speed;
    float currentTime;
    idVec4 currentValue;
    f.ReadInt(extrapolationType);
    f.ReadFloat(startTime);
    f.ReadFloat(duration);
    f.ReadVec4(startValue);
    f.ReadVec4(baseSpeed);
    f.ReadVec4(speed);
    f.ReadFloat(currentTime);
    f.ReadVec4(currentValue);

    transitions.Append(td);
}

f.SetLog(true, (work + "-regstuff"));
if (rebuild)
{
    f.ReadInt(c);
    for (i = 0; i < c; i++)
    {
        wexpOp_t w;
        f.ReadInt((int &)w.opType);
        f.ReadInt(w.a);
        f.ReadInt(w.b);
        f.ReadInt(w.c);
        f.ReadInt(w.d);
        ops.Append(w);
    }

    f.ReadInt(c);
    for (i = 0; i < c; i++)
    {
        float ff;
        f.ReadFloat(ff);
        expressionRegisters.Append(ff);
    }

    regList.ReadFromDemoFile(f);

}
f.SetLog(true, (work + "-children"));
f.ReadInt(c);
for (i = 0; i < c; i++)
{
    if (rebuild)
    {
        idWindow* win = new idWindow(dc, gui);
        win.ReadFromDemoFile(f);
        AddChild(win);
    }
    else
    {
        for (int j = 0; j < c; j++)
        {
            if (children[j].childID == i)
            {
                children[j].ReadFromDemoFile(f, rebuild);
                break;
            }
            else
            {
                continue;
            }
        }
    }
}
#endif
        }

        public virtual void WriteToDemoFile(VFileDemo f)
        {
            // should never hit unless we re-enable WRITE_GUIS
#if !WRITE_GUIS
            Debug.Assert(false);
#else

    f.SetLog(true, "window");
    f.WriteHashString(backGroundName);
    f.SetLog(true, backGroundName);
    f.WriteUnsignedChar(cursor);
    f.WriteUnsignedInt(flags);
    f.WriteInt(timeLine);
    f.WriteInt(lastTimeRun);
    idRectangle rct = rect;
    f.WriteFloat(rct.x);
    f.WriteFloat(rct.y);
    f.WriteFloat(rct.w);
    f.WriteFloat(rct.h);
    f.WriteFloat(drawRect.x);
    f.WriteFloat(drawRect.y);
    f.WriteFloat(drawRect.w);
    f.WriteFloat(drawRect.h);
    f.WriteFloat(clientRect.x);
    f.WriteFloat(clientRect.y);
    f.WriteFloat(clientRect.w);
    f.WriteFloat(clientRect.h);
    f.WriteFloat(textRect.x);
    f.WriteFloat(textRect.y);
    f.WriteFloat(textRect.w);
    f.WriteFloat(textRect.h);
    f.WriteFloat(xOffset);
    f.WriteFloat(yOffset);
    idStr work;
    f.SetLog(true, work);

    int i, c;

    f.SetLog(true, (work + "-transitions"));
    c = transitions.Num();
    f.WriteInt(c);
    for (i = 0; i < c; i++)
    {
        f.WriteInt(0);
        f.WriteInt(transitions[i].offset);

        f.WriteFloat(transitions[i].interp.GetStartTime());
        f.WriteFloat(transitions[i].interp.GetAccelTime());
        f.WriteFloat(transitions[i].interp.GetLinearTime());
        f.WriteFloat(transitions[i].interp.GetDecelTime());
        f.WriteVec4(transitions[i].interp.GetStartValue());
        f.WriteVec4(transitions[i].interp.GetEndValue());

        // write to keep win32 render demo format compatiblity - we don't actually read them back anymore
        f.WriteInt(transitions[i].interp.GetExtrapolate().GetExtrapolationType());
        f.WriteFloat(transitions[i].interp.GetExtrapolate().GetStartTime());
        f.WriteFloat(transitions[i].interp.GetExtrapolate().GetDuration());
        f.WriteVec4(transitions[i].interp.GetExtrapolate().GetStartValue());
        f.WriteVec4(transitions[i].interp.GetExtrapolate().GetBaseSpeed());
        f.WriteVec4(transitions[i].interp.GetExtrapolate().GetSpeed());
        f.WriteFloat(transitions[i].interp.GetExtrapolate().GetCurrentTime());
        f.WriteVec4(transitions[i].interp.GetExtrapolate().GetCurrentValue());
    }

    f.SetLog(true, (work + "-regstuff"));

    f.SetLog(true, (work + "-children"));
    c = children.Num();
    f.WriteInt(c);
    for (i = 0; i < c; i++)
    {
        for (int j = 0; j < c; j++)
        {
            if (children[j].childID == i)
            {
                children[j].WriteToDemoFile(f);
                break;
            }
            else
            {
                continue;
            }
        }
    }
#endif
        }

        // SaveGame support
        public void WriteSaveGameString(string s, VFile savefile)
        {
            var len = s.Length;

            savefile.Write(len); savefile.WriteASCII(s, len);
        }

        public void WriteSaveGameTransition(TransitionData trans, VFile savefile)
        {
            var dw = new DrawWin { simp = null, win = null };
            var offset = gui.Desktop.GetWinVarOffset(trans.data, dw);
            var winName = dw.win != null || dw.simp != null
                ? dw.win != null ? dw.win.Name : dw.simp.name
                : "";
            var fdw = gui.Desktop.FindChildByName(winName);
            if (offset != -1 && fdw != null && (fdw.win != null || fdw.simp != null))
            {
                savefile.Write(offset);
                WriteSaveGameString(winName, savefile);
                savefile.WriteT(trans.interp);
            }
            else
            {
                offset = -1;
                savefile.Write(offset);
            }
        }

        public virtual void WriteToSaveGame(VFile savefile)
        {
            int i;

            WriteSaveGameString(cmd, savefile);

            savefile.Write(actualX);
            savefile.Write(actualY);
            savefile.Write(childID);
            savefile.Write(flags);
            savefile.Write(lastTimeRun);
            savefile.WriteT(drawRect);
            savefile.WriteT(clientRect);
            savefile.WriteT(origin);
            savefile.Write(fontNum);
            savefile.Write(timeLine);
            savefile.Write(xOffset);
            savefile.Write(yOffset);
            savefile.Write(cursor);
            savefile.Write(forceAspectWidth);
            savefile.Write(forceAspectHeight);
            savefile.Write(matScalex);
            savefile.Write(matScaley);
            savefile.Write(borderSize);
            savefile.Write(textAlign);
            savefile.Write(textAlignx);
            savefile.Write(textAligny);
            savefile.Write(textShadow);
            savefile.WriteT(shear);

            WriteSaveGameString(name, savefile);
            WriteSaveGameString(comment, savefile);

            // WinVars
            noTime.WriteToSaveGame(savefile);
            visible.WriteToSaveGame(savefile);
            rect.WriteToSaveGame(savefile);
            backColor.WriteToSaveGame(savefile);
            matColor.WriteToSaveGame(savefile);
            foreColor.WriteToSaveGame(savefile);
            hoverColor.WriteToSaveGame(savefile);
            borderColor.WriteToSaveGame(savefile);
            textScale.WriteToSaveGame(savefile);
            noEvents.WriteToSaveGame(savefile);
            rotate.WriteToSaveGame(savefile);
            text.WriteToSaveGame(savefile);
            backGroundName.WriteToSaveGame(savefile);
            hideCursor.WriteToSaveGame(savefile);

            // Defined Vars
            for (i = 0; i < definedVars.Count; i++)
                definedVars[i].WriteToSaveGame(savefile);

            savefile.WriteT(textRect);

            // Window pointers saved as the child ID of the window
            var winID = focusedChild != null ? focusedChild.childID : -1;
            savefile.Write(winID);

            winID = captureChild != null ? captureChild.childID : -1;
            savefile.Write(winID);

            winID = overChild != null ? overChild.childID : -1;
            savefile.Write(winID);

            // Scripts
            for (i = 0; i < (int)SCRIPT.SCRIPT_COUNT; i++)
                scripts[i]?.WriteToSaveGame(savefile);

            // TimeLine Events
            for (i = 0; i < timeLineEvents.Count; i++)
                if (timeLineEvents[i] != null)
                {
                    savefile.Write(timeLineEvents[i].pending);
                    savefile.Write(timeLineEvents[i].time);
                    timeLineEvents[i].event_?.WriteToSaveGame(savefile);
                }

            // Transitions
            var num = transitions.Count;
            savefile.Write(num);
            for (i = 0; i < transitions.Count; i++) WriteSaveGameTransition(transitions[i], savefile);

            // Named Events
            for (i = 0; i < namedEvents.Count; i++)
                if (namedEvents[i] != null)
                {
                    WriteSaveGameString(namedEvents[i].mName, savefile);
                    namedEvents[i].mEvent?.WriteToSaveGame(savefile);
                }

            // regList
            regList.WriteToSaveGame(savefile);

            // Save children
            for (i = 0; i < drawWindows.Count; i++)
            {
                var window = drawWindows[i];
                if (window.simp != null) window.simp.WriteToSaveGame(savefile);
                else if (window.win != null) window.win.WriteToSaveGame(savefile);
            }
        }

        public void ReadSaveGameString(out string s, VFile savefile)
        {
            savefile.Read(out int len);
            if (len < 0) common.Warning("Window::ReadSaveGameString: invalid length");
            savefile.ReadASCII(out s, len);
        }

        public void ReadSaveGameTransition(TransitionData trans, VFile savefile)
        {
            savefile.Read(out int offset);
            if (offset != -1)
            {
                ReadSaveGameString(out var winName, savefile);
                savefile.ReadT(out trans.interp);
                trans.data = null;
                trans.offset = offset;
                if (winName.Length != 0)
                {
                    var strVar = new WinStr();
                    strVar.Set(winName);
                    trans.data = (WinVar)strVar;
                }
            }
        }

        public virtual void ReadFromSaveGame(VFile savefile)
        {
            int i;

            transitions.Clear();

            ReadSaveGameString(out cmd, savefile);

            savefile.Read(out actualX);
            savefile.Read(out actualY);
            savefile.Read(out childID);
            savefile.Read(out flags);
            savefile.Read(out lastTimeRun);
            savefile.ReadT(out drawRect);
            savefile.ReadT(out clientRect);
            savefile.ReadT(out origin);
            savefile.Read(out fontNum);
            savefile.Read(out timeLine);
            savefile.Read(out xOffset);
            savefile.Read(out yOffset);
            savefile.Read(out cursor);
            savefile.Read(out forceAspectWidth);
            savefile.Read(out forceAspectHeight);
            savefile.Read(out matScalex);
            savefile.Read(out matScaley);
            savefile.Read(out borderSize);
            savefile.Read(out textAlign);
            savefile.Read(out textAlignx);
            savefile.Read(out textAligny);
            savefile.Read(out textShadow);
            savefile.ReadT(out shear);

            ReadSaveGameString(out name, savefile);
            ReadSaveGameString(out comment, savefile);

            // WinVars
            noTime.ReadFromSaveGame(savefile);
            visible.ReadFromSaveGame(savefile);
            rect.ReadFromSaveGame(savefile);
            backColor.ReadFromSaveGame(savefile);
            matColor.ReadFromSaveGame(savefile);
            foreColor.ReadFromSaveGame(savefile);
            hoverColor.ReadFromSaveGame(savefile);
            borderColor.ReadFromSaveGame(savefile);
            textScale.ReadFromSaveGame(savefile);
            noEvents.ReadFromSaveGame(savefile);
            rotate.ReadFromSaveGame(savefile);
            text.ReadFromSaveGame(savefile);
            backGroundName.ReadFromSaveGame(savefile);

            if (session.SaveGameVersion >= 17) hideCursor.ReadFromSaveGame(savefile);
            else hideCursor = false;

            // Defined Vars
            for (i = 0; i < definedVars.Count; i++) definedVars[i].ReadFromSaveGame(savefile);

            savefile.ReadT(out textRect);

            // Window pointers saved as the child ID of the window
            var winID = -1;
            savefile.Read(out winID);
            for (i = 0; i < children.Count; i++) if (children[i].childID == winID) focusedChild = children[i];
            savefile.Read(out winID);
            for (i = 0; i < children.Count; i++) if (children[i].childID == winID) captureChild = children[i];
            savefile.Read(out winID);
            for (i = 0; i < children.Count; i++) if (children[i].childID == winID) overChild = children[i];

            // Scripts
            for (i = 0; i < (int)SCRIPT.SCRIPT_COUNT; i++) scripts[i]?.ReadFromSaveGame(savefile);

            // TimeLine Events
            for (i = 0; i < timeLineEvents.Count; i++)
                if (timeLineEvents[i] != null)
                {
                    savefile.Read(out timeLineEvents[i].pending);
                    savefile.Read(out timeLineEvents[i].time);
                    timeLineEvents[i].event_?.ReadFromSaveGame(savefile);
                }

            // Transitions
            int num;
            savefile.Read(out num);
            for (i = 0; i < num; i++)
            {
                var trans = new TransitionData { data = null };
                ReadSaveGameTransition(trans, savefile);
                if (trans.data != null) transitions.Add(trans);
            }

            // Named Events
            for (i = 0; i < namedEvents.Count; i++)
                if (namedEvents[i] != null)
                {
                    ReadSaveGameString(out namedEvents[i].mName, savefile);
                    namedEvents[i].mEvent?.ReadFromSaveGame(savefile);
                }

            // regList
            regList.ReadFromSaveGame(savefile);

            // Read children
            for (i = 0; i < drawWindows.Count; i++)
            {
                var window = drawWindows[i];
                if (window.simp != null) window.simp.ReadFromSaveGame(savefile);
                else if (window.win != null) window.win.ReadFromSaveGame(savefile);
            }

            if ((flags & WIN_DESKTOP) != 0) FixupTransitions();
        }

        public void FixupTransitions()
        {
            int i, c = transitions.Count;
            for (i = 0; i < c; i++)
            {
                var dw = gui.Desktop.FindChildByName((WinStr)transitions[i].data);
                transitions[i].data = null;
                if (dw != null && (dw.win != null || dw.simp != null))
                    if (dw.win != null)
                    {
                        if (transitions[i].offset == (int)OFFSET.RECT) transitions[i].data = dw.win.rect;
                        else if (transitions[i].offset == (int)OFFSET.BACKCOLOR) transitions[i].data = dw.win.backColor;
                        else if (transitions[i].offset == (int)OFFSET.MATCOLOR) transitions[i].data = dw.win.matColor;
                        else if (transitions[i].offset == (int)OFFSET.FORECOLOR) transitions[i].data = dw.win.foreColor;
                        else if (transitions[i].offset == (int)OFFSET.BORDERCOLOR) transitions[i].data = dw.win.borderColor;
                        else if (transitions[i].offset == (int)OFFSET.TEXTSCALE) transitions[i].data = dw.win.textScale;
                        else if (transitions[i].offset == (int)OFFSET.ROTATE) transitions[i].data = dw.win.rotate;
                    }
                    else
                    {
                        if (transitions[i].offset == (int)OFFSET.RECT) transitions[i].data = dw.simp.rect;
                        else if (transitions[i].offset == (int)OFFSET.BACKCOLOR) transitions[i].data = dw.simp.backColor;
                        else if (transitions[i].offset == (int)OFFSET.MATCOLOR) transitions[i].data = dw.simp.matColor;
                        else if (transitions[i].offset == (int)OFFSET.FORECOLOR) transitions[i].data = dw.simp.foreColor;
                        else if (transitions[i].offset == (int)OFFSET.BORDERCOLOR) transitions[i].data = dw.simp.borderColor;
                        else if (transitions[i].offset == (int)OFFSET.TEXTSCALE) transitions[i].data = dw.simp.textScale;
                        else if (transitions[i].offset == (int)OFFSET.ROTATE) transitions[i].data = dw.simp.rotate;
                    }
                if (transitions[i].data == null) { transitions.RemoveAt(i); i--; c--; }
            }
            for (c = 0; c < children.Count; c++) children[c].FixupTransitions();
        }

        public virtual void HasAction() { }

        public virtual void HasScripts() { }

        public void FixupParms()
        {
            int i, c = children.Count;

            for (i = 0; i < c; i++) children[i].FixupParms();
            for (i = 0; i < (int)SCRIPT.SCRIPT_COUNT; i++) scripts[i]?.FixupParms(this);

            c = timeLineEvents.Count;
            for (i = 0; i < c; i++) timeLineEvents[i].event_.FixupParms(this);

            c = namedEvents.Count;
            for (i = 0; i < c; i++) namedEvents[i].mEvent.FixupParms(this);

            c = ops.Count;
            for (i = 0; i < c; i++)
                if (ops[i].b == -2)
                {
                    // need to fix this up
                    var p = (WinStr)ops[i].a;
                    var var_ = GetWinVarByName(p, true);
                    ops[i].a = var_;
                    ops[i].b = -1;
                }

            if ((flags & WIN_DESKTOP) != 0) CalcRects(0, 0);
        }

        //public void GetScriptString(string name, string o);
        //public void SetScriptParams();

        public bool HasOps => ops.Count > 0;

        static float[] EvalRegs_regs = new float[MAX_EXPRESSION_REGISTERS];
        static Window EvalRegs_lastEval = null;
        public float EvalRegs(int test = -1, bool force = false)
        {
            if (!force && test >= 0 && test < MAX_EXPRESSION_REGISTERS && EvalRegs_lastEval == this) return EvalRegs_regs[test];

            EvalRegs_lastEval = this;

            if (expressionRegisters.Count != 0)
            {
                regList.SetToRegs(EvalRegs_regs);
                EvaluateRegisters(EvalRegs_regs);
                regList.GetFromRegs(EvalRegs_regs);
            }
            return test >= 0 && test < MAX_EXPRESSION_REGISTERS ? EvalRegs_regs[test] : 0;
        }

        public void StartTransition()
            => flags |= WIN_INTRANSITION;

        public void AddTransition(WinVar dest, Vector4 from, Vector4 to, int time, float accelTime, float decelTime)
        {
            var data = new TransitionData { data = dest };
            data.interp.Init(gui.Time, accelTime * time, decelTime * time, time, from, to);
            transitions.Add(data);
        }

        public void ResetTime(int time)
        {
            timeLine = gui.Time - time;

            int i, c = timeLineEvents.Count;
            for (i = 0; i < c; i++) if (timeLineEvents[i].time >= time) timeLineEvents[i].pending = true;

            noTime = false;

            c = transitions.Count;
            for (i = 0; i < c; i++)
            {
                var data = transitions[i];
                if (data.interp.IsDone(gui.Time) && data.data != null) { transitions.RemoveAt(i); i--; c--; }
            }
        }

        public void ResetCinematics()
            => background?.ResetCinematicTime(gui.Time);

        public int NumTransitions
        {
            get
            {
                var c = transitions.Count;
                for (var i = 0; i < children.Count; i++) c += children[i].NumTransitions;
                return c;
            }
        }

        public bool ParseScript(Parser src, GuiScriptList list, int? timeParm = null, bool allowIf = false)
        {
            var ifElseBlock = false;

            Token token;

            // scripts start with { ( unless parm is true ) and have ; separated command lists.. commands are command,
            // arg.. basically we want everything between the { } as it will be interpreted at run time
            if (allowIf)
            {
                src.ReadToken(out token); if (string.Equals(token, "if", StringComparison.OrdinalIgnoreCase)) ifElseBlock = true;
                src.UnreadToken(token);
                if (!ifElseBlock && !src.ExpectTokenString("{")) return false;
            }
            else if (!src.ExpectTokenString("{")) return false;

            var nest = 0;
            while (true)
            {
                if (!src.ReadToken(out token)) { src.Error("Unexpected end of file"); return false; }

                if (token == "{") nest++;
                if (token == "}" && nest-- <= 0) return true;

                var gs = new GuiScript();
                if (string.Equals(token, "if", StringComparison.OrdinalIgnoreCase))
                {
                    gs.conditionReg = ParseExpression(src);
                    ParseScript(src, gs.ifList = new GuiScriptList(), null);
                    if (src.ReadToken(out token))
                        // pass true to indicate we are parsing an else condition
                        if (token == "else") ParseScript(src, gs.elseList = new GuiScriptList(), null, true);
                        else src.UnreadToken(token);

                    list.Add(gs);

                    // if we are parsing an else if then return out so the initial "if" parser can handle the rest of the tokens
                    if (ifElseBlock) return true;
                    continue;
                }
                else src.UnreadToken(token);

                // empty { } is not allowed
                if (token == "{") { src.Error("Unexpected {"); return false; }

                gs.Parse(src);
                list.Add(gs);
            }
        }

        protected void SaveExpressionParseState()
        {
            saveTemps = new bool[MAX_EXPRESSION_REGISTERS];
            Array.Copy(registerIsTemporary, saveTemps, MAX_EXPRESSION_REGISTERS);
        }
        protected void RestoreExpressionParseState()
        {
            Array.Copy(saveTemps, registerIsTemporary, MAX_EXPRESSION_REGISTERS);
            saveTemps = null;
        }
        protected bool ParseScriptEntry(string name, Parser src)
        {
            for (var i = 0; i < (int)SCRIPT.SCRIPT_COUNT; i++)
                if (string.Equals(name, ScriptNames[i], StringComparison.OrdinalIgnoreCase))
                {
                    scripts[i] = new GuiScriptList();
                    return ParseScript(src, scripts[i]);
                }
            return false;
        }

        public bool RunScript(SCRIPT n)
            => n >= SCRIPT.ON_MOUSEENTER && n < SCRIPT.SCRIPT_COUNT && RunScriptList(scripts[(int)n]);

        public bool RunScriptList(GuiScriptList src)
        {
            if (src == null) return false;
            src.Execute(this);
            return true;
        }

        //public void SetRegs(string key, string val);

        // Returns a register index
        public int ParseExpression(Parser src, WinVar var = null, object component = null)
            => ParseExpressionPriority(src, TOP_PRIORITY, var);

        public int ExpressionConstant(float f)
        {
            int i;

            for (i = (int)WEXP_REG.NUM_PREDEFINED; i < expressionRegisters.Count; i++) if (!registerIsTemporary[i] && expressionRegisters[i] == f) return i;
            if (expressionRegisters.Count == MAX_EXPRESSION_REGISTERS) { common.Warning($"expressionConstant: gui {gui.SourceFile} hit MAX_EXPRESSION_REGISTERS"); return 0; }

            var c = expressionRegisters.Count;
            if (i > c) while (i > c) { expressionRegisters.Add(-9999999); i--; }

            i = expressionRegisters.Add_(f);
            registerIsTemporary[i] = false;
            return i;
        }

        public RegisterList RegList => regList;

        public void AddCommand(string cmd)
        {
            var str = this.cmd;
            if (str.Length != 0) str += $" ; {cmd}";
            else str = cmd;
            this.cmd = str;
        }

        public void AddUpdateVar(WinVar var) => updateVars.AddUnique(var);

        public bool Interactive
        {
            get
            {
                if (scripts[(int)SCRIPT.ON_ACTION] != null) return true;
                var c = children.Count;
                for (var i = 0; i < c; i++) if (children[i].Interactive) return true;
                return false;
            }
        }

        public bool ContainsStateVars
        {
            get
            {
                if (updateVars.Count != 0) return true;
                var c = children.Count;
                for (var i = 0; i < c; i++) if (children[i].ContainsStateVars) return true;
                return false;
            }
        }

        public void SetChildWinVarVal(string name, string var, string val)
        {
            var dw = FindChildByName(name);
            WinVar wv = null;
            if (dw?.simp != null) wv = dw.simp.GetWinVarByName(var);
            else if (dw?.win != null) wv = dw.win.GetWinVarByName(var);
            if (wv != null)
            {
                wv.Set(val);
                wv.Eval = false;
            }
        }

        public Window FocusedChild
            => (flags & WIN_DESKTOP) != 0 ? gui.Desktop.focusedChild : null;

        public Window CaptureChild
            => (flags & WIN_DESKTOP) != 0 ? gui.Desktop.captureChild : null;

        public string Comment
        {
            get => comment;
            set => comment = value;
        }

        public string cmd;

        public virtual void RunNamedEvent(string eventName)
        {
            int i, c;

            // Find and run the event
            c = namedEvents.Count;
            for (i = 0; i < c; i++)
            {
                if (string.Equals(namedEvents[i].mName, eventName, StringComparison.OrdinalIgnoreCase)) continue;

                UpdateWinVars();

                // Make sure we got all the current values for stuff
                if (expressionRegisters.Count != 0 && ops.Count != 0) EvalRegs(-1, true);

                RunScriptList(namedEvents[i].mEvent);

                break;
            }

            // Run the event in all the children as well
            c = children.Count;
            for (i = 0; i < c; i++) children[i].RunNamedEvent(eventName);
        }

        public void AddDefinedVar(WinVar var)
            => definedVars.AddUnique(var);

        // Finds the window under the given point
        public Window FindChildByPoint(float x, float y, Window below) => FindChildByPoint(x, y, ref below);
        public Window FindChildByPoint(float x, float y, ref Window below)
        {
            var c = children.Count;

            // If we are looking for a window below this one then the next window should be good, but this one wasnt it
            if (below == this) { below = null; return null; }

            if (!Contains(drawRect, x, y)) return null;

            for (var i = c - 1; i >= 0; i--)
            {
                var found = children[i].FindChildByPoint(x, y, ref below);
                if (found != null)
                {
                    if (below != null) continue;
                    return found;
                }
            }

            return this;
        }

        // Returns the index of the given child window
        public int GetChildIndex(Window window)
        {
            for (var find = 0; find < drawWindows.Count; find++) if (drawWindows[find].win == window) return find;
            return -1;
        }

        // Returns the number of children
        public int ChildCount
            => drawWindows.Count;

        // Returns the child window at the given index
        public Window GetChild(int index)
            => drawWindows[index].win;

        // Removes the child from the list of children. Note that the child window being removed must still be deallocated by the caller
        public void RemoveChild(Window win)
        {
            int find;

            // Remove the child window
            children.Remove(win);

            for (find = 0; find < drawWindows.Count; find++) if (drawWindows[find].win == win) { drawWindows.RemoveAt(find); break; }
        }

        // Inserts the given window as a child into the given location in the zorder.
        public bool InsertChild(Window win, Window before)
        {
            AddChild(win);

            win.parent = this;
            var dwt = new DrawWin { simp = null, win = win };

            // If not inserting before anything then just add it at the end
            if (before != null)
            {
                var index = GetChildIndex(before);
                if (index != -1) { drawWindows.Insert(index, dwt); return true; }
            }

            drawWindows.Add(dwt);
            return true;
        }

        public void ScreenToClient(ref Rectangle rect)
        {
            int x, y; Window p;

            for (p = this, x = 0, y = 0; p != null; p = p.parent) { x += (int)p.rect.x; y += (int)p.rect.y; }
            rect.x -= x;
            rect.y -= y;
        }

        public void ClientToScreen(ref Rectangle rect)
        {
            int x, y; Window p;

            for (p = this, x = 0, y = 0; p != null; p = p.parent) { x += (int)p.rect.x; y += (int)p.rect.y; }
            rect.x += x;
            rect.y += y;
        }

        // The editor only has a dictionary to work with so the easiest way to push the values of the dictionary onto the window is for the window to interpret the
        // dictionary as if were a file being parsed.
        public bool UpdateFromDictionary(Dictionary<string, string> dict)
        {
            SetDefaults();

            // Clear all registers since they will get recreated
            regList.Reset();
            expressionRegisters.Clear();
            ops.Clear();

            foreach (var kv in dict)
            {
                // Special case name
                if (string.Equals(kv.Key, "name", StringComparison.OrdinalIgnoreCase)) { name = kv.Value; continue; }

                var src = new Parser(kv.Value, kv.Value.Length, "", LEXFL.NOFATALERRORS | LEXFL.NOSTRINGCONCAT | LEXFL.ALLOWMULTICHARLITERALS | LEXFL.ALLOWBACKSLASHSTRINGCONCAT);
                // Kill the old register since the parse reg entry will add a new one
                if (!ParseInternalVar(kv.Key, src) && !ParseRegEntry(kv.Key, src)) continue;
            }

            EvalRegs(-1, true);

            SetupFromState();
            PostParse();

            return true;
        }

        // Set the window do a default window with no text, no background and default colors, etc..
        protected void SetDefaults()
        {
            forceAspectWidth = 640f;
            forceAspectHeight = 480f;
            matScalex = 1;
            matScaley = 1;
            borderSize = 0;
            noTime = false;
            visible = true;
            textAlign = 0;
            textAlignx = 0;
            textAligny = 0;
            noEvents = false;
            rotate = 0;
            shear.Zero();
            textScale = 0.35f;
            backColor.Zero();
            foreColor = new Vector4(1, 1, 1, 1);
            hoverColor = new Vector4(1, 1, 1, 1);
            matColor = new Vector4(1, 1, 1, 1);
            borderColor.Zero();
            text = "";
            background = null;
            backGroundName = "";
        }

        protected bool IsSimple
        {
            get
            {
                // dont do simple windows when in gui editor
                if ((C.com_editors & EDITOR.GUI) != 0) return false;
                if (ops.Count != 0) return false;
                if ((flags & (WIN_HCENTER | WIN_VCENTER)) != 0) return false;
                if (children.Count != 0 || drawWindows.Count != 0) return false;
                for (var i = 0; i < (int)SCRIPT.SCRIPT_COUNT; i++) if (scripts[i] != null) return false;
                if (timeLineEvents.Count != 0) return false;
                if (namedEvents.Count != 0) return false;
                return true;
            }
        }

        protected void UpdateWinVars()
        {
            var c = updateVars.Count;
            for (var i = 0; i < c; i++) updateVars[i].Update();
        }

        protected void DisableRegister(string name)
        {
            var reg = RegList.FindReg(name);
            reg?.Enable(false);
        }

        protected void Transition()
        {
            int i, c = transitions.Count;
            var clear = true;

            for (i = 0; i < c; i++)
            {
                var data = transitions[i];
                var data_data = data.data;
                if (data.interp.IsDone(gui.Time) && data_data != null)
                {
                    if (data_data is WinVec4) data.data = (WinVec4)data.interp.EndValue;
                    else if (data_data is WinFloat) data.data = (WinFloat)data.interp.EndValue.x;
                    else data.data = (WinRectangle)data.interp.EndValue;
                }
                else
                {
                    clear = false;
                    if (data.data != null)
                    {
                        if (data_data is WinVec4) data.data = (WinVec4)data.interp.GetCurrentValue(gui.Time);
                        else if (data_data is WinFloat) data.data = (WinFloat)data.interp.GetCurrentValue(gui.Time).x;
                        else data.data = (WinRectangle)data.interp.GetCurrentValue(gui.Time);
                    }
                    else common.Warning($"Invalid transitional data for window {Name} in gui {gui.SourceFile}");
                }
            }

            if (clear)
            {
                transitions.SetNum(0, false);
                flags &= ~WIN_INTRANSITION;
            }
        }

        protected void Time()
        {
            if (noTime) return;

            if (timeLine == -1) timeLine = gui.Time;

            cmd = "";

            var c = timeLineEvents.Count;
            if (c > 0)
                for (int i = 0; i < c; i++)
                    if (timeLineEvents[i].pending && gui.Time - timeLine >= timeLineEvents[i].time)
                    {
                        timeLineEvents[i].pending = false;
                        RunScriptList(timeLineEvents[i].event_);
                    }
            if (gui.Active) gui.PendingCmd += cmd;
        }

        protected bool RunTimeEvents(int time)
        {
            if (time - lastTimeRun < IUsercmd.USERCMD_MSEC) { /*common.Printf($"Skipping gui time events at {time}\n");*/ return false; }

            lastTimeRun = time;

            UpdateWinVars();

            if (expressionRegisters.Count != 0 && ops.Count != 0) EvalRegs();
            if ((flags & WIN_INTRANSITION) != 0) Transition();

            Time();

            // renamed ON_EVENT to ON_FRAME
            RunScript(SCRIPT.ON_FRAME);

            var c = children.Count;
            for (var i = 0; i < c; i++) children[i].RunTimeEvents(time);

            return true;
        }

        //protected void Dump();

        protected int ExpressionTemporary()
        {
            if (expressionRegisters.Count == MAX_EXPRESSION_REGISTERS) { common.Warning($"expressionTemporary: gui {gui.SourceFile} hit MAX_EXPRESSION_REGISTERS"); return 0; }
            var i = expressionRegisters.Count;
            registerIsTemporary[i] = true;
            i = expressionRegisters.Add_(0);
            return i;
        }

        protected WexpOp ExpressionOp()
        {
            if (ops.Count == MAX_EXPRESSION_OPS) { common.Warning($"expressionOp: gui {gui.SourceFile} hit MAX_EXPRESSION_OPS"); return ops[0]; }
            WexpOp wop = new();
            var i = ops.Count; ops.Add(wop);
            return ops[i];
        }

        protected int EmitOp(object a, int b, WOP_TYPE opType, Action<WexpOp> opp = null)
        {
#if false
            // optimize away identity operations
            if (opType == WexpOpType.WOP_TYPE_ADD)
            {
                if (!registerIsTemporary[a] && shaderRegisters[a] == 0) return b;
                if (!registerIsTemporary[b] && shaderRegisters[b] == 0) return a;
                if (!registerIsTemporary[a] && !registerIsTemporary[b]) return ExpressionConstant(shaderRegisters[a] + shaderRegisters[b]);
            }
            if (opType == WexpOpType.WOP_TYPE_MULTIPLY)
            {
                if (!registerIsTemporary[a] && shaderRegisters[a] == 1) return b;
                if (!registerIsTemporary[a] && shaderRegisters[a] == 0) return a;
                if (!registerIsTemporary[b] && shaderRegisters[b] == 1) return a;
                if (!registerIsTemporary[b] && shaderRegisters[b] == 0) return b;
                if (!registerIsTemporary[a] && !registerIsTemporary[b]) return ExpressionConstant(shaderRegisters[a] * shaderRegisters[b]);
            }
#endif
            var op = ExpressionOp();

            op.opType = opType;
            op.a = a;
            op.b = b;
            op.c = ExpressionTemporary();

            opp?.Invoke(op);
            return op.c;
        }

        protected int ParseEmitOp(Parser src, object a, WOP_TYPE opType, int priority, Action<WexpOp> opp = null)
        {
            var b = ParseExpressionPriority(src, priority);
            return EmitOp(a, b, opType, opp);
        }

        // Returns a register index
        protected int ParseTerm(Parser src, WinVar var = null, int component = 0)
        {
            object a; int b;

            src.ReadToken(out var token);

            if (token == "(") { a = ParseExpression(src); src.ExpectTokenString(")"); return (int)a; }

            if (string.Equals(token, "time", StringComparison.OrdinalIgnoreCase)) return (int)WEXP_REG.TIME;

            // parse negative numbers
            if (token == "-")
            {
                src.ReadToken(out token);
                if (token.type == TT.NUMBER || token == ".") return ExpressionConstant(-(float)token.FloatValue);
                src.Warning($"Bad negative number '{token}'");
                return 0;
            }

            if (token.type == TT.NUMBER || token == "." || token == "-") return ExpressionConstant((float)token.FloatValue);

            // see if it is a table name
            var table = (DeclTable)declManager.FindType(DECL.TABLE, token, false);
            if (table != null)
            {
                a = table.Index;
                // parse a table expression
                src.ExpectTokenString("[");
                b = ParseExpression(src);
                src.ExpectTokenString("]");
                return EmitOp(a, b, WOP_TYPE.TABLE);
            }

            if (var == null) var = GetWinVarByName(token, true);
            if (var != null)
            {
                a = var;
                //assert(dynamic_cast<idWinVec4*>(var));
                var.Init(token, this);
                b = component;
                if (var is WinVec4)
                {
                    if (src.ReadToken(out token))
                        if (token == "[") { b = ParseExpression(src); src.ExpectTokenString("]"); }
                        else src.UnreadToken(token);
                    return EmitOp(a, b, WOP_TYPE.VAR);
                }
                else if (var is WinFloat) return EmitOp(a, b, WOP_TYPE.VARF);
                else if (var is WinInt) return EmitOp(a, b, WOP_TYPE.VARI);
                else if (var is WinBool) return EmitOp(a, b, WOP_TYPE.VARB);
                else if (var is WinStr) return EmitOp(a, b, WOP_TYPE.VARS);
                else src.Warning($"Var expression not vec4, float or int '{token}'");
                return 0;
            }
            else
            {
                // ugly but used for post parsing to fixup named vars
                var p = (string)token;
                a = p;
                b = -2;
                return EmitOp(a, b, WOP_TYPE.VAR);
            }
        }

        // Returns a register index
        const int TOP_PRIORITY = 4;
        protected int ParseExpressionPriority(Parser src, int priority, WinVar var = null, int component = 0)
        {
            int a;

            if (priority == 0) return ParseTerm(src, var, component);

            a = ParseExpressionPriority(src, priority - 1, var, component);

            if (!src.ReadToken(out var token)) return a; // we won't get EOF in a real file, but we can when parsing from generated strings

            if (priority == 1 && token == "*") return ParseEmitOp(src, a, WOP_TYPE.MULTIPLY, priority);
            if (priority == 1 && token == "/") return ParseEmitOp(src, a, WOP_TYPE.DIVIDE, priority);
            if (priority == 1 && token == "%") return ParseEmitOp(src, a, WOP_TYPE.MOD, priority); // implied truncate both to integer
            if (priority == 2 && token == "+") return ParseEmitOp(src, a, WOP_TYPE.ADD, priority);
            if (priority == 2 && token == "-") return ParseEmitOp(src, a, WOP_TYPE.SUBTRACT, priority);
            if (priority == 3 && token == ">") return ParseEmitOp(src, a, WOP_TYPE.GT, priority);
            if (priority == 3 && token == ">=") return ParseEmitOp(src, a, WOP_TYPE.GE, priority);
            if (priority == 3 && token == "<") return ParseEmitOp(src, a, WOP_TYPE.LT, priority);
            if (priority == 3 && token == "<=") return ParseEmitOp(src, a, WOP_TYPE.LE, priority);
            if (priority == 3 && token == "==") return ParseEmitOp(src, a, WOP_TYPE.EQ, priority);
            if (priority == 3 && token == "!=") return ParseEmitOp(src, a, WOP_TYPE.NE, priority);
            if (priority == 4 && token == "&&") return ParseEmitOp(src, a, WOP_TYPE.AND, priority);
            if (priority == 4 && token == "||") return ParseEmitOp(src, a, WOP_TYPE.OR, priority);
            if (priority == 4 && token == "?")
            {
                WexpOp oop = default;
                var o = ParseEmitOp(src, a, WOP_TYPE.COND, priority, x => oop = x);
                if (!src.ReadToken(out token)) return o;
                if (token == ":") { a = ParseExpressionPriority(src, priority - 1, var); oop.d = a; }
                return o;
            }

            // assume that anything else terminates the expression not too robust error checking...
            src.UnreadToken(token);
            return a;
        }

        // Parameters are taken from the localSpace and the renderView, then all expressions are evaluated, leaving the shader registers
        // set to their apropriate values.
        protected void EvaluateRegisters(float[] registers)
        {
            int i, b; WexpOp op;

            var erc = expressionRegisters.Count;
            var oc = ops.Count;
            // copy the constants
            for (i = (int)WEXP_REG.NUM_PREDEFINED; i < erc; i++) registers[i] = expressionRegisters[i];

            // copy the local and global parameters
            registers[(int)WEXP_REG.TIME] = gui.Time;

            for (i = 0; i < oc; i++)
            {
                op = ops[i];
                if (op.b == -2) continue;
                switch (op.opType)
                {
                    case WOP_TYPE.ADD: registers[op.c] = registers[(int)op.a] + registers[op.b]; break;
                    case WOP_TYPE.SUBTRACT: registers[op.c] = registers[(int)op.a] - registers[op.b]; break;
                    case WOP_TYPE.MULTIPLY: registers[op.c] = registers[(int)op.a] * registers[op.b]; break;
                    case WOP_TYPE.DIVIDE:
                        if (registers[op.b] == 0f) { common.Warning($"Divide by zero in window '{Name}' in {gui.SourceFile}"); registers[op.c] = registers[(int)op.a]; }
                        else registers[op.c] = registers[(int)op.a] / registers[op.b];
                        break;
                    case WOP_TYPE.MOD:
                        b = (int)registers[op.b];
                        b = b != 0 ? b : 1;
                        registers[op.c] = (int)registers[(int)op.a] % b;
                        break;
                    case WOP_TYPE.TABLE:
                        {
                            var table = (DeclTable)declManager.DeclByIndex(DECL.TABLE, (int)op.a);
                            registers[op.c] = table.TableLookup(registers[op.b]);
                        }
                        break;
                    case WOP_TYPE.GT: registers[op.c] = registers[(int)op.a] > registers[op.b] ? 1 : 0; break;
                    case WOP_TYPE.GE: registers[op.c] = registers[(int)op.a] >= registers[op.b] ? 1 : 0; break;
                    case WOP_TYPE.LT: registers[op.c] = registers[(int)op.a] < registers[op.b] ? 1 : 0; break;
                    case WOP_TYPE.LE: registers[op.c] = registers[(int)op.a] <= registers[op.b] ? 1 : 0; break;
                    case WOP_TYPE.EQ: registers[op.c] = registers[(int)op.a] == registers[op.b] ? 1 : 0; break;
                    case WOP_TYPE.NE: registers[op.c] = registers[(int)op.a] != registers[op.b] ? 1 : 0; break;
                    case WOP_TYPE.COND: registers[op.c] = registers[(int)op.a] != 0 ? registers[op.b] : registers[op.d]; break;
                    case WOP_TYPE.AND: registers[op.c] = registers[(int)op.a] != 0 && registers[op.b] != 0 ? 1 : 0; break;
                    case WOP_TYPE.OR: registers[op.c] = registers[(int)op.a] != 0 || registers[op.b] != 0 ? 1 : 0; break;
                    case WOP_TYPE.VAR:
                        if (op.a == null) { registers[op.c] = 0; break; }
                        // grabs vector components 
                        if (op.b >= 0 && registers[op.b] >= 0 && registers[op.b] < 4) { var var = (WinVec4)op.a; registers[op.c] = ((Vector4)var)[(int)registers[op.b]]; }
                        else registers[op.c] = ((WinVar)op.a).x;
                        break;
                    case WOP_TYPE.VARS:
                        if (op.a != null) { var var = (WinStr)op.a; registers[op.c] = floatX.Parse(var); }
                        else registers[op.c] = 0;
                        break;
                    case WOP_TYPE.VARF:
                        if (op.a != null) { var var = (WinFloat)op.a; registers[op.c] = var; }
                        else registers[op.c] = 0;
                        break;
                    case WOP_TYPE.VARI:
                        if (op.a != null) { var var = (WinInt)op.a; registers[op.c] = var; }
                        else registers[op.c] = 0;
                        break;
                    case WOP_TYPE.VARB:
                        if (op.a != null) { var var = (WinBool)op.a; registers[op.c] = var ? 1 : 0; }
                        else registers[op.c] = 0;
                        break;
                    default: common.FatalError("R_EvaluateExpression: bad opcode"); break;
                }
            }
        }

        protected void ParseBracedExpression(Parser src)
        {
            src.ExpectTokenString("{"); ParseExpression(src); src.ExpectTokenString("}");
        }

        protected bool ParseRegEntry(string name, Parser src)
        {
            var work = name.ToLowerInvariant();
            var var = GetWinVarByName(work, false);
            if (var != null)
                for (var i = 0; i < RegisterVars.Length; i++)
                    if (string.Equals(work, RegisterVars[i].name, StringComparison.OrdinalIgnoreCase))
                    {
                        regList.AddReg(work, RegisterVars[i].type, src, this, var);
                        return true;
                    }

            // not predefined so just read the next token and add it to the state
            if (src.ReadToken(out var tok))
            {
                if (var != null) { var.Set(tok); return true; }
                switch (tok.type)
                {
                    case TT.NUMBER:
                        if ((tok.subtype & Token.TT_INTEGER) != 0) definedVars.Add(new WinInt(intX.Parse(tok)) { Name = work });
                        else if ((tok.subtype & Token.TT_FLOAT) != 0) definedVars.Add(new WinFloat(floatX.Parse(tok)) { Name = work });
                        else definedVars.Add(new WinStr(tok) { Name = name });
                        break;
                    default: definedVars.Add(new WinStr(tok) { Name = name }); break;
                }
            }
            return true;
        }

        protected virtual bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "showtime", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_SHOWTIME; return true; }
            if (string.Equals(name, "showcoords", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_SHOWCOORDS; return true; }
            if (string.Equals(name, "scaleto43", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_SCALETO43; return true; } // DG: added this window flag for Windows that should be scaled to 4:3 (with "empty" bars left/right or above/below)
            if (string.Equals(name, "forceaspectwidth", StringComparison.OrdinalIgnoreCase)) { forceAspectWidth = src.ParseFloat(); return true; }
            if (string.Equals(name, "forceaspectheight", StringComparison.OrdinalIgnoreCase)) { forceAspectHeight = src.ParseFloat(); return true; }
            if (string.Equals(name, "matscalex", StringComparison.OrdinalIgnoreCase)) { matScalex = src.ParseFloat(); return true; }
            if (string.Equals(name, "matscaley", StringComparison.OrdinalIgnoreCase)) { matScaley = src.ParseFloat(); return true; }
            if (string.Equals(name, "bordersize", StringComparison.OrdinalIgnoreCase)) { borderSize = src.ParseFloat(); return true; }
            if (string.Equals(name, "nowrap", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_NOWRAP; return true; }
            if (string.Equals(name, "shadow", StringComparison.OrdinalIgnoreCase)) { textShadow = (byte)src.ParseInt(); return true; }
            if (string.Equals(name, "textalign", StringComparison.OrdinalIgnoreCase)) { textAlign = (DeviceContext.ALIGN)src.ParseInt(); return true; }
            if (string.Equals(name, "textalignx", StringComparison.OrdinalIgnoreCase)) { textAlignx = src.ParseFloat(); return true; }
            if (string.Equals(name, "textaligny", StringComparison.OrdinalIgnoreCase)) { textAligny = src.ParseFloat(); return true; }
            if (string.Equals(name, "shear", StringComparison.OrdinalIgnoreCase))
            {
                shear.x = src.ParseFloat();
                src.ReadToken(out var tok); if (tok == ",") { src.Error("Expected comma in shear definiation"); return false; }
                shear.y = src.ParseFloat();
                return true;
            }
            if (string.Equals(name, "wantenter", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_WANTENTER; return true; }
            if (string.Equals(name, "naturalmatscale", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_NATURALMAT; return true; }
            if (string.Equals(name, "noclip", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_NOCLIP; return true; }
            if (string.Equals(name, "nocursor", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_NOCURSOR; return true; }
            if (string.Equals(name, "menugui", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_MENUGUI; return true; }
            if (string.Equals(name, "modal", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_MODAL; return true; }
            if (string.Equals(name, "invertrect", StringComparison.OrdinalIgnoreCase)) { if (src.ParseBool()) flags |= WIN_INVERTRECT; return true; }
            if (string.Equals(name, "name", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out name); return true; }
            if (string.Equals(name, "play", StringComparison.OrdinalIgnoreCase)) { common.Warning("play encountered during gui parse.. see Robert\n"); ParseString(src, out var _); return true; }
            if (string.Equals(name, "comment", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out comment); return true; }
            if (string.Equals(name, "font", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out var fontStr); fontNum = (byte)dc.FindFont(fontStr); return true; }
            return false;
        }

        protected void ParseString(Parser src, out string o)
            => o = src.ReadToken(out var tok) ? tok : null;

        protected void ParseVec4(Parser src, out Vector4 o)
        {
            src.ReadToken(out var tok); o.x = floatX.Parse(tok); src.ExpectTokenString(",");
            src.ReadToken(out tok); o.y = floatX.Parse(tok); src.ExpectTokenString(",");
            src.ReadToken(out tok); o.z = floatX.Parse(tok); src.ExpectTokenString(",");
            src.ReadToken(out tok); o.w = floatX.Parse(tok);
        }

        //protected void ConvertRegEntry(string name, Parser src, out string o, int tabs);
    }
}