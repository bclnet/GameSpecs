using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class DrawWin
    {
        public Window win;
        public SimpleWindow simp;
    }

    public class SimpleWindow
    {
        public string name;
        protected UserInterfaceLocal gui;
        protected DeviceContext dc;
        protected int flags;
        protected Rectangle drawRect;         // overall rect
        protected Rectangle clientRect;         // client area
        protected Rectangle textRect;
        protected Vector2 origin;
        protected int fontNum;
        protected float matScalex;
        protected float matScaley;
        protected float borderSize;
        protected DeviceContext.ALIGN textAlign;
        protected float textAlignx;
        protected float textAligny;
        protected int textShadow;

        protected WinStr text;
        protected WinBool visible;
        protected internal WinRectangle rect;                // overall rect
        protected internal WinVec4 backColor;
        protected internal WinVec4 matColor;
        protected internal WinVec4 foreColor;
        protected internal WinVec4 borderColor;
        protected internal WinFloat textScale;
        protected internal WinFloat rotate;
        protected WinVec2 shear;
        protected WinBackground backGroundName;

        protected Material background;

        protected Window mParent;

        protected WinBool hideCursor;

        public SimpleWindow(Window win)
        {
            gui = win.Gui;
            dc = win.dc;
            drawRect = win.drawRect;
            clientRect = win.clientRect;
            textRect = win.textRect;
            origin = win.origin;
            fontNum = win.fontNum;
            name = win.name;
            matScalex = win.matScalex;
            matScaley = win.matScaley;
            borderSize = win.borderSize;
            textAlign = win.textAlign;
            textAlignx = win.textAlignx;
            textAligny = win.textAligny;
            background = win.background;
            flags = (int)win.flags;
            textShadow = win.textShadow;

            visible = win.visible;
            text = win.text;
            rect = win.rect;
            backColor = win.backColor;
            matColor = win.matColor;
            foreColor = win.foreColor;
            borderColor = win.borderColor;
            textScale = win.textScale;
            rotate = win.rotate;
            shear = win.shear;
            backGroundName = win.backGroundName;
            if (backGroundName.Length != 0)
            {
                background = declManager.FindMaterial(backGroundName);
                background.Sort = (float)SS.GUI;
                background.SetImageClassifications(1); // just for resource tracking
            }
            backGroundName.SetMaterialPtr(x => background = x);

            mParent = win.Parent;

            hideCursor = win.hideCursor;

            var parent = win.Parent;
            if (parent != null)
            {
                if (text.NeedsUpdate) parent.AddUpdateVar(text);
                if (visible.NeedsUpdate) parent.AddUpdateVar(visible);
                if (rect.NeedsUpdate) parent.AddUpdateVar(rect);
                if (backColor.NeedsUpdate) parent.AddUpdateVar(backColor);
                if (matColor.NeedsUpdate) parent.AddUpdateVar(matColor);
                if (foreColor.NeedsUpdate) parent.AddUpdateVar(foreColor);
                if (borderColor.NeedsUpdate) parent.AddUpdateVar(borderColor);
                if (textScale.NeedsUpdate) parent.AddUpdateVar(textScale);
                if (rotate.NeedsUpdate) parent.AddUpdateVar(rotate);
                if (shear.NeedsUpdate) parent.AddUpdateVar(shear);
                if (backGroundName.NeedsUpdate) parent.AddUpdateVar(backGroundName);
            }
        }

        public void StateChanged(bool redraw)
        {
            if (redraw && background != null && background.CinematicLength != 0) background.UpdateCinematic(gui.Time);
        }

        static Matrix3x3 SetupTransforms_trans = new();
        static Vector3 SetupTransforms_org = new();
        static Rotation SetupTransforms_rot = new();
        static Vector3 SetupTransforms_vec = new(0f, 0f, 1f);
        static Matrix3x3 SetupTransforms_smat = new();
        protected void SetupTransforms(float x, float y)
        {
            SetupTransforms_trans.Identity();
            SetupTransforms_org.Set(origin.x + x, origin.y + y, 0);
            if (rotate != null)
            {
                SetupTransforms_rot.Set(SetupTransforms_org, SetupTransforms_vec, rotate);
                SetupTransforms_trans = SetupTransforms_rot.ToMat3();
            }

            SetupTransforms_smat.Identity();
            if (shear.x != 0f || shear.y != 0f)
            {
                SetupTransforms_smat[0].y = shear.x; SetupTransforms_smat[1].x = shear.y;
                SetupTransforms_trans *= SetupTransforms_smat;
            }

            if (!SetupTransforms_trans.IsIdentity()) dc.SetTransformInfo(SetupTransforms_org, SetupTransforms_trans);
        }

        protected void DrawBackground(Rectangle drawRect)
        {
            if (backColor.w > 0f) dc.DrawFilledRect(drawRect.x, drawRect.y, drawRect.w, drawRect.h, backColor);

            if (background != null && matColor.w > 0f)
            {
                float scalex, scaley;
                if ((flags & Window.WIN_NATURALMAT) != 0) { scalex = drawRect.w / background.ImageWidth; scaley = drawRect.h / background.ImageHeight; }
                else { scalex = matScalex; scaley = matScaley; }
                dc.DrawMaterial(drawRect.x, drawRect.y, drawRect.w, drawRect.h, background, matColor, scalex, scaley);
            }
        }

        protected void DrawBorderAndCaption(Rectangle drawRect)
        {
            if ((flags & Window.WIN_BORDER) != 0 && borderSize != 0f) dc.DrawRect(drawRect.x, drawRect.y, drawRect.w, drawRect.h, borderSize, borderColor);
        }

        protected void CalcClientRect(float xofs, float yofs)
        {
            drawRect = rect;

            if ((flags & Window.WIN_INVERTRECT) != 0) { drawRect.x = rect.x - rect.w; drawRect.y = rect.y - rect.h; }

            drawRect.x += xofs; drawRect.y += yofs;

            clientRect = drawRect;
            if (rect.h > 0f && rect.w > 0f)
            {
                if ((flags & Window.WIN_BORDER) != 0 && borderSize != 0f)
                {
                    clientRect.x += borderSize; clientRect.y += borderSize;
                    clientRect.w -= borderSize; clientRect.h -= borderSize;
                }

                textRect = clientRect;
                textRect.x += 2f; textRect.y += 2f;
                textRect.w -= 2f; textRect.h -= 2f;
                textRect.x += textAlignx;
                textRect.y += textAligny;
            }
            origin.Set(rect.x + (rect.w / 2), rect.y + (rect.h / 2));
        }

        public void Redraw(float x, float y)
        {
            if (!visible)
                return;

            CalcClientRect(0, 0);
            dc.SetFont(fontNum);
            drawRect.Offset(x, y);
            clientRect.Offset(x, y);
            textRect.Offset(x, y);
            SetupTransforms(x, y);
            if ((flags & Window.WIN_NOCLIP) != 0) dc.EnableClipping(false);
            DrawBackground(drawRect);
            DrawBorderAndCaption(drawRect);
            if (textShadow != 0)
            {
                var shadowText = (string)text;
                var shadowRect = new Rectangle(textRect);

                shadowText = stringX.RemoveColors(shadowText);
                shadowRect.x += textShadow;
                shadowRect.y += textShadow;

                dc.DrawText(shadowText, textScale, textAlign, colorBlack, shadowRect, (flags & Window.WIN_NOWRAP) == 0, -1);
            }
            dc.DrawText(text, textScale, textAlign, foreColor, textRect, (flags & Window.WIN_NOWRAP) == 0, -1);
            dc.SetTransformInfo(Vector3.origin, Matrix3x3.identity);
            if ((flags & Window.WIN_NOCLIP) != 0) dc.EnableClipping(true);
            drawRect.Offset(-x, -y);
            clientRect.Offset(-x, -y);
            textRect.Offset(-x, -y);
        }

        enum OFFSET
        {
            RECT = 1,
            BACKCOLOR,
            MATCOLOR,
            FORECOLOR,
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
            if (wv == borderColor) ret = (int)OFFSET.BORDERCOLOR;
            if (wv == textScale) ret = (int)OFFSET.TEXTSCALE;
            if (wv == rotate) ret = (int)OFFSET.ROTATE;
            if (ret != -1) owner.simp = this;
            return ret;
        }

        public WinVar GetWinVarByName(string name)
        {
            WinVar retVar = null;
            if (string.Equals(name, "background", StringComparison.OrdinalIgnoreCase)) retVar = backGroundName;
            if (string.Equals(name, "visible", StringComparison.OrdinalIgnoreCase)) retVar = visible;
            if (string.Equals(name, "rect", StringComparison.OrdinalIgnoreCase)) retVar = rect;
            if (string.Equals(name, "backColor", StringComparison.OrdinalIgnoreCase)) retVar = backColor;
            if (string.Equals(name, "matColor", StringComparison.OrdinalIgnoreCase)) retVar = matColor;
            if (string.Equals(name, "foreColor", StringComparison.OrdinalIgnoreCase)) retVar = foreColor;
            if (string.Equals(name, "borderColor", StringComparison.OrdinalIgnoreCase)) retVar = borderColor;
            if (string.Equals(name, "textScale", StringComparison.OrdinalIgnoreCase)) retVar = textScale;
            if (string.Equals(name, "rotate", StringComparison.OrdinalIgnoreCase)) retVar = rotate;
            if (string.Equals(name, "shear", StringComparison.OrdinalIgnoreCase)) retVar = shear;
            if (string.Equals(name, "text", StringComparison.OrdinalIgnoreCase)) retVar = text;
            return retVar;
        }

        public Window Parent => mParent;

        public virtual void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(flags);
            savefile.WriteT(drawRect);
            savefile.WriteT(clientRect);
            savefile.WriteT(textRect);
            savefile.WriteT(origin);
            savefile.Write(fontNum);
            savefile.Write(matScalex);
            savefile.Write(matScaley);
            savefile.Write(borderSize);
            savefile.Write(textAlign);
            savefile.Write(textAlignx);
            savefile.Write(textAligny);
            savefile.Write(textShadow);

            text.WriteToSaveGame(savefile);
            visible.WriteToSaveGame(savefile);
            rect.WriteToSaveGame(savefile);
            backColor.WriteToSaveGame(savefile);
            matColor.WriteToSaveGame(savefile);
            foreColor.WriteToSaveGame(savefile);
            borderColor.WriteToSaveGame(savefile);
            textScale.WriteToSaveGame(savefile);
            rotate.WriteToSaveGame(savefile);
            shear.WriteToSaveGame(savefile);
            backGroundName.WriteToSaveGame(savefile);

            int stringLen;
            if (background != null) { stringLen = background.Name.Length; savefile.Write(stringLen); savefile.WriteASCII(background.Name, stringLen); }
            else { stringLen = 0; savefile.Write(stringLen); }
        }

        public virtual void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(out flags);
            savefile.ReadT(out drawRect);
            savefile.ReadT(out clientRect);
            savefile.ReadT(out textRect);
            savefile.ReadT(out origin);
            savefile.Read(out fontNum);
            savefile.Read(out matScalex);
            savefile.Read(out matScaley);
            savefile.Read(out borderSize);
            savefile.Read(out textAlign);
            savefile.Read(out textAlignx);
            savefile.Read(out textAligny);
            savefile.Read(out textShadow);

            text.ReadFromSaveGame(savefile);
            visible.ReadFromSaveGame(savefile);
            rect.ReadFromSaveGame(savefile);
            backColor.ReadFromSaveGame(savefile);
            matColor.ReadFromSaveGame(savefile);
            foreColor.ReadFromSaveGame(savefile);
            borderColor.ReadFromSaveGame(savefile);
            textScale.ReadFromSaveGame(savefile);
            rotate.ReadFromSaveGame(savefile);
            shear.ReadFromSaveGame(savefile);
            backGroundName.ReadFromSaveGame(savefile);

            savefile.Read(out int stringLen);
            if (stringLen > 0)
            {
                savefile.ReadASCII(out var backName, stringLen);
                background = declManager.FindMaterial(backName);
                background.Sort = (float)SS.GUI;
            }
            else background = null;
        }

        public int Size => 0;
    }
}