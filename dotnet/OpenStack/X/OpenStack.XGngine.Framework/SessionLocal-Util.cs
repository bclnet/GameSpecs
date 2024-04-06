using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Gngine;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    partial class SessionLocal
    {
        // these must be kept up to date with window Levelshot in guis/mainmenu.gui
        const int PREVIEW_X = 211;
        const int PREVIEW_Y = 31;
        const int PREVIEW_WIDTH = 398;
        const int PREVIEW_HEIGHT = 298;

        unsafe void RandomizeStack()
        {
            throw new NotImplementedException();
            //// attempt to force uninitialized stack memory bugs
            //const int bytes = 4000000;
            //byte* buf = stackalloc byte[bytes];

            //var fill = rand() & 255;
            //for (var i = 0; i < bytes; i++)
            //    buf[i] = fill;
        }

        static void Doom3Quest_setUseScreenLayer(int use) { }

        static void SetupScreenLayer()
        {
            var inMenu = ((SessionLocal)session).guiActive != null;
            var inGameGui = game != null && game.InGameGuiActive;
            var objectiveActive = game != null && game.ObjectiveSystemActive;
            var cinematic = game != null && game.InCinematic;
            var loading = ((SessionLocal)session).insideExecuteMapChange;

            Doom3Quest_setUseScreenLayer(
                (inMenu ? 1 : 0) +
                (inGameGui ? 2 : 0) +
                (objectiveActive ? 4 : 0) +
                (cinematic ? 8 : 0) +
                (loading ? 16 : 0));
        }

        const int FPS_FRAMES = 5;
        static int CalcFPS_fps = 0;
        static int[] CalcFPS_previousTimes = new int[FPS_FRAMES];
        static int CalcFPS_index;
        static int CalcFPS_previous;
        static int CalcFPS()
        {
            int i, total, t, frameTime;

            // don't use serverTime, because that will be drifting to correct for internet lag changes, timescales, timedemos, etc
            t = SysW.Milliseconds;
            frameTime = t - CalcFPS_previous;
            CalcFPS_previous = t;

            CalcFPS_previousTimes[CalcFPS_index % FPS_FRAMES] = frameTime;
            CalcFPS_index++;
            if (CalcFPS_index > FPS_FRAMES)
            {
                // average multiple frames together to smooth changes out a bit
                total = 0;
                for (i = 0; i < FPS_FRAMES; i++) total += CalcFPS_previousTimes[i];
                if (total == 0) total = 1;
                CalcFPS_fps = 10000 * FPS_FRAMES / total;
                CalcFPS_fps = (CalcFPS_fps + 5) / 10;
                //common.Printf($" FPS: {fps} ");
            }

            return CalcFPS_fps;
        }
    }
}