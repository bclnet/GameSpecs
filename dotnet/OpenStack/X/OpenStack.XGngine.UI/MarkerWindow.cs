using System.Collections.Generic;
using System.IO;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public struct MarkerData
    {
        public int time;
        public Material mat;
        public Rectangle rect;
    }

    public class MarkerWindow : Window
    {
        LogStats[] loggedStats = new LogStats[ISession.MAX_LOGGED_STATS];
        List<MarkerData> markerTimes = new();
        string statData;
        int numStats;
        uint[] imageBuff;
        Material markerMat;
        Material markerStop;
        Vector4 markerColor;
        int currentMarker;
        int currentTime;
        int stopTime;

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "markerMat", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out var str); markerMat = declManager.FindMaterial(str); markerMat.Sort = (float)SS.GUI; return true; }
            if (string.Equals(name, "markerStop", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out var str); markerStop = declManager.FindMaterial(str); markerStop.Sort = (float)SS.GUI; return true; }
            if (string.Equals(name, "markerColor", StringComparison.OrdinalIgnoreCase)) { ParseVec4(src, out markerColor); return true; }
            return base.ParseInternalVar(name, src);
        }

        new void CommonInit()
        {
            numStats = 0;
            currentTime = -1;
            currentMarker = -1;
            stopTime = -1;
            imageBuff = null;
            markerMat = null;
            markerStop = null;
        }

        public MarkerWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public MarkerWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        public override int Allocated => base.Allocated;

        public virtual string HandleEvent(SysEvent ev, bool updateVisuals)
        {
            if (!(ev.evType == SE.KEY && ev.evValue2 != 0))
                return "";

            var key = (Key)ev.evValue;
            if (ev.evValue2 != 0 && key == K_MOUSE1)
            {
                gui.Desktop.SetChildWinVarVal("markerText", "text", "");
                var c = markerTimes.Count;
                int i;
                for (i = 0; i < c; i++)
                {
                    var md = markerTimes[i];
                    if (md.rect.Contains(gui.CursorX, gui.CursorY))
                    {
                        currentMarker = i;
                        gui.SetStateInt("currentMarker", md.time);
                        stopTime = md.time;
                        gui.Desktop.SetChildWinVarVal("markerText", "text", $"Marker set at {md.time / 60 / 60:.2}:{md.time / 60 % 60:.2}");
                        gui.Desktop.SetChildWinVarVal("markerText", "visible", "1");
                        gui.Desktop.SetChildWinVarVal("markerBackground", "matcolor", "1 1 1 1");
                        gui.Desktop.SetChildWinVarVal("markerBackground", "text", "");
                        gui.Desktop.SetChildWinVarVal("markerBackground", "background", md.mat.Name);
                        break;
                    }
                }
                if (i == c)
                {
                    // no marker selected;
                    currentMarker = -1;
                    gui.SetStateInt("currentMarker", currentTime);
                    stopTime = currentTime;
                    gui.Desktop.SetChildWinVarVal("markerText", "text", $"Marker set at {currentTime / 60 / 60:.2}:{currentTime / 60 % 60:.2}");
                    gui.Desktop.SetChildWinVarVal("markerText", "visible", "1");
                    gui.Desktop.SetChildWinVarVal("markerBackground", "matcolor", "0 0 0 0");
                    gui.Desktop.SetChildWinVarVal("markerBackground", "text", "No Preview");
                }
                var pct = gui.State.GetFloat("loadPct");
                var len = gui.State.GetInt("loadLength");
                if (stopTime > len * pct) return "cmdDemoGotoMarker";
            }
            else if (key == K_MOUSE2)
            {
                stopTime = -1;
                gui.Desktop.SetChildWinVarVal("markerText", "text", "");
                gui.SetStateInt("currentMarker", -1);
                return "cmdDemoGotoMarker";
            }
            else if (key == K_SPACE) return "cmdDemoPauseFrame";
            return "";
        }

        public override void PostParse()
            => base.PostParse();

        const int HEALTH_MAX = 100;
        const int COMBAT_MAX = 100;
        const int RATE_MAX = 125;
        const int STAMINA_MAX = 12;

        public override void Draw(int time, float x, float y)
        {
            float pct;

            var r = new Rectangle(clientRect);
            var len = gui.State.GetInt("loadLength");
            if (len == 0)
                len = 1;
            if (numStats > 1)
            {
                var c = markerTimes.Count;
                if (c > 0)
                    for (var i = 0; i < c; i++)
                    {
                        var md = markerTimes[i];
                        if (md.rect.w == 0)
                        {
                            md.rect.x = r.x + r.w * ((float)md.time / len) - 8;
                            md.rect.y = r.y + r.h - 20;
                            md.rect.w = 16;
                            md.rect.h = 16;
                        }
                        dc.DrawMaterial(md.rect.x, md.rect.y, md.rect.w, md.rect.h, markerMat, markerColor);
                    }
            }

            r.y += 10;
            if (r.w > 0 && r.Contains(gui.CursorX, gui.CursorY))
            {
                pct = (gui.CursorX - r.x) / r.w;
                currentTime = (int)(len * pct);
                r.x = gui.CursorX > r.x + r.w - 40 ? gui.CursorX - 40 : gui.CursorX;
                r.y = gui.CursorY - 15;
                r.w = 40;
                r.h = 20;
                dc.DrawText($"{currentTime / 60 / 60:.2}:{currentTime / 60 % 60:.2}", 0.25f, 0, DeviceContext.colorWhite, r, false);
            }

            if (stopTime >= 0 && markerStop != null)
            {
                r = clientRect;
                r.y += (r.h - 32) / 2;
                pct = (float)stopTime / len;
                r.x += (r.w * pct) - 16;
                var color = new Vector4(1, 1, 1, 0.65f);
                dc.DrawMaterial(r.x, r.y, 32, 32, markerStop, color);
            }
        }

        public override string RouteMouseCoords(float xd, float yd)
        {
            var ret = base.RouteMouseCoords(xd, yd);

            int i, c = markerTimes.Count;
            int len = gui.State.GetInt("loadLength");
            if (len == 0) len = 1;
            for (i = 0; i < c; i++)
            {
                var md = markerTimes[i];
                if (md.rect.Contains(gui.CursorY, gui.CursorX))
                {
                    gui.Desktop.SetChildWinVarVal("markerBackground", "background", md.mat.Name);
                    gui.Desktop.SetChildWinVarVal("markerBackground", "matcolor", "1 1 1 1");
                    gui.Desktop.SetChildWinVarVal("markerBackground", "text", "");
                    break;
                }
            }

            if (i >= c)
                if (currentMarker == -1)
                {
                    gui.Desktop.SetChildWinVarVal("markerBackground", "matcolor", "0 0 0 0");
                    gui.Desktop.SetChildWinVarVal("markerBackground", "text", "No Preview");
                }
                else
                {
                    var md = markerTimes[currentMarker];
                    gui.Desktop.SetChildWinVarVal("markerBackground", "background", md.mat.Name);
                    gui.Desktop.SetChildWinVarVal("markerBackground", "matcolor", "1 1 1 1");
                    gui.Desktop.SetChildWinVarVal("markerBackground", "text", "");
                }
            return ret;
        }

        void Line(int x1, int y1, int x2, int y2, uint[] o, uint color)
        {
            var deltax = Math.Abs(x2 - x1);
            var deltay = Math.Abs(y2 - y1);
            var incx = x1 > x2 ? -1 : 1;
            var incy = y1 > y2 ? -1 : 1;
            int right, up, dir;
            if (deltax > deltay)
            {
                right = deltay * 2;
                up = right - deltax * 2;
                dir = right - deltax;
                while (deltax-- >= 0)
                {
                    Point(x1, y1, o, color);
                    x1 += incx;
                    y1 += dir > 0 ? incy : 0;
                    dir += dir > 0 ? up : right;
                }
            }
            else
            {
                right = deltax * 2;
                up = right - deltay * 2;
                dir = right - deltay;
                while (deltay-- >= 0)
                {
                    Point(x1, y1, o, color);
                    x1 += dir > 0 ? incx : 0;
                    y1 += incy;
                    dir += dir > 0 ? up : right;
                }
            }
        }

        void Point(int x, int y, uint[] o, uint color)
        {
            var index = (63 - y) * 512 + x;
            if (index >= 0 && index < 512 * 64) o[index] = color;
            else common.Warning($"Out of bounds on point {x} : {y}");
        }

        public unsafe override void Activate(bool activate, ref string act)
        {
            base.Activate(activate, ref act);
            if (activate)
            {
                int i;
                gui.Desktop.SetChildWinVarVal("markerText", "text", "");
                imageBuff = new uint[512 * 64];
                markerTimes.Clear();
                currentMarker = -1;
                currentTime = -1;
                stopTime = -1;
                statData = gui.State.GetString("statData");
                numStats = 0;
                if (statData.Length != 0)
                {
                    var file = fileSystem.OpenFileRead(statData);
                    if (file != null)
                    {
                        file.Read(out numStats);
                        file.ReadTMany(out loggedStats, numStats);
                        for (i = 0; i < numStats; i++)
                        {
                            if (loggedStats[i].health < 0) loggedStats[i].health = 0;
                            if (loggedStats[i].stamina < 0) loggedStats[i].stamina = 0;
                            if (loggedStats[i].heartRate < 0) loggedStats[i].heartRate = 0;
                            if (loggedStats[i].combat < 0) loggedStats[i].combat = 0;
                        }
                        fileSystem.CloseFile(file);
                    }
                }

                if (numStats > 1 && background != null)
                {
                    var markerPath = Path.GetDirectoryName(statData);
                    FileList markers;
                    markers = fileSystem.ListFiles(markerPath, ".tga", false, true);
                    for (i = 0; i < markers.NumFiles; i++)
                    {
                        var name = markers.GetFile(i);
                        var md = new MarkerData
                        {
                            mat = declManager.FindMaterial(name),
                            time = intX.Parse(Path.GetFileNameWithoutExtension(name))
                        };
                        md.mat.Sort = (float)SS.GUI;
                        markerTimes.Add(md);
                    }
                    fileSystem.FreeFileList(markers);
                    Array.Clear(imageBuff, 0, imageBuff.Length);
                    var step = 511f / (numStats - 1);
                    float x1, y1, x2, y2;
                    x1 = 0 - step;
                    for (i = 0; i < numStats - 1; i++)
                    {
                        x1 += step;
                        x2 = x1 + step;
                        y1 = 63 * ((float)loggedStats[i].health / HEALTH_MAX);
                        y2 = 63 * ((float)loggedStats[i + 1].health / HEALTH_MAX);
                        Line((int)x1, (int)y1, (int)x2, (int)y2, imageBuff, 0xff0000ff);
                        y1 = 63 * ((float)loggedStats[i].heartRate / RATE_MAX);
                        y2 = 63 * ((float)loggedStats[i + 1].heartRate / RATE_MAX);
                        Line((int)x1, (int)y1, (int)x2, (int)y2, imageBuff, 0xff00ff00);
                        // stamina not quite as high on graph so health does not get obscured with both at 100%
                        y1 = 62 * ((float)loggedStats[i].stamina / STAMINA_MAX);
                        y2 = 62 * ((float)loggedStats[i + 1].stamina / STAMINA_MAX);
                        Line((int)x1, (int)y1, (int)x2, (int)y2, imageBuff, 0xffff0000);
                        y1 = 63 * ((float)loggedStats[i].combat / COMBAT_MAX);
                        y2 = 63 * ((float)loggedStats[i + 1].combat / COMBAT_MAX);
                        Line((int)x1, (int)y1, (int)x2, (int)y2, imageBuff, 0xff00ffff);
                    }
                    var stage = background.GetStage(0);
                    fixed (void* imageBuffV = imageBuff) stage?.texture.image.UploadScratch(imageBuffV, 512, 64);
                }
            }
        }

        public override void MouseExit()
            => base.MouseExit();

        public override void MouseEnter()
        => base.MouseEnter();

    }
}