using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    partial class ConsoleLocal
    {
        static void SCR_DrawTextLeftAlign(ref float y, string text, params string[] args)
        {
            var str = args.Length == 0 ? text : string.Format(text, args);
            renderSystem.DrawSmallStringExt(0, (int)(y + 2), str, colorWhite, true, localConsole.charSetShader);
            y += R.SMALLCHAR_HEIGHT + 4;
        }

        static void SCR_DrawTextRightAlign(ref float y, string text, params string[] args)
        {
            var str = args.Length == 0 ? text : string.Format(text, args);
            renderSystem.DrawSmallStringExt(635 - str.Length * R.SMALLCHAR_WIDTH, (int)(y + 2), str, colorWhite, true, localConsole.charSetShader);
            y += R.SMALLCHAR_HEIGHT + 4;
        }

        const int FPS_FRAMES = 5;

        static int[] SCR_DrawFPS_previousTimes = new int[FPS_FRAMES];
        static int SCR_DrawFPS_index;
        static int SCR_DrawFPS_fps = 0;
        static int SCR_DrawFPS_previous;
        static float SCR_DrawFPS(float y)
        {
            var new_y = MathX.FtoiFast(y) + 300;

            // don't use serverTime, because that will be drifting to correct for internet lag changes, timescales, timedemos, etc
            var t = SysW.Milliseconds;
            var frameTime = t - SCR_DrawFPS_previous;
            SCR_DrawFPS_previous = t;

            SCR_DrawFPS_previousTimes[SCR_DrawFPS_index % FPS_FRAMES] = frameTime;
            SCR_DrawFPS_index++;
            if (SCR_DrawFPS_index > FPS_FRAMES)
            {
                // average multiple frames together to smooth changes out a bit
                var total = 0;
                for (var i = 0; i < FPS_FRAMES; i++) total += SCR_DrawFPS_previousTimes[i];
                if (total == 0) total = 1;
                SCR_DrawFPS_fps = 10000 * FPS_FRAMES / total;
                SCR_DrawFPS_fps = (SCR_DrawFPS_fps + 5) / 10;

                var s = $"{SCR_DrawFPS_fps}fps";
                var w = s.Length * R.SMALLCHAR_WIDTH;

                renderSystem.DrawSmallStringExt((634 / 2) - w, new_y, s, colorWhite, true, localConsole.charSetShader);
            }

            return y + R.BIGCHAR_HEIGHT + 4;
        }

#if false
        static float SCR_DrawMemoryUsage(float y)
        {
            MemoryStats allocs, frees;

            Mem_GetStats(allocs);
            SCR_DrawTextRightAlign(ref y, $"total allocated memory: {allocs.num:4}, {allocs.totalSize >> 10:4}kB");

            Mem_GetFrameStats(allocs, frees);
            SCR_DrawTextRightAlign(ref y, $"frame alloc: {allocs.num:4}, {allocs.totalSize >> 10:4}kB  frame free: {frees.num:4}d, {frees.totalSize >> 10:4}kB");

            Mem_ClearFrameStats();

            return y;
        }
#endif

#if false
        static float SCR_DrawAsyncStats(float y)
        {
            int i, outgoingRate, incomingRate; float outgoingCompression, incomingCompression;

            if (AsyncNetwork.server.IsActive)
            {
                SCR_DrawTextRightAlign(ref y, $"server delay = {AsyncNetwork.server.Delay} msec");
                SCR_DrawTextRightAlign(ref y, $"total outgoing rate = {AsyncNetwork.server.OutgoingRate >> 10} KB/s");
                SCR_DrawTextRightAlign(ref y, $"total incoming rate = {AsyncNetwork.server.IncomingRate >> 10} KB/s");

                for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
                {
                    outgoingRate = AsyncNetwork.server.GetClientOutgoingRate(i);
                    incomingRate = AsyncNetwork.server.GetClientIncomingRate(i);
                    outgoingCompression = AsyncNetwork.server.GetClientOutgoingCompression(i);
                    incomingCompression = AsyncNetwork.server.GetClientIncomingCompression(i);

                    if (outgoingRate != -1 && incomingRate != -1) SCR_DrawTextRightAlign(y, $"client {i}: out-rate = {outgoingRate} B/s ({outgoingCompression:-2.1}%), in-rate = %d B/s ({incomingCompression:-2.1}%)");
                }

                AsyncNetwork.server.GetAsyncStatsAvgMsg(out var msg);
                SCR_DrawTextRightAlign(ref y, msg);
            }
            else if (AsyncNetwork.client.IsActive)
            {
                outgoingRate = AsyncNetwork.client.GetOutgoingRate;
                incomingRate = AsyncNetwork.client.GetIncomingRate;
                outgoingCompression = AsyncNetwork.client.GetOutgoingCompression;
                incomingCompression = AsyncNetwork.client.GetIncomingCompression;

                if (outgoingRate != -1 && incomingRate != -1) SCR_DrawTextRightAlign(ref y, $"out-rate = {outgoingRate} B/s ({outgoingCompression:-2.1}%), in rate = {incomingRate} B/s ({incomingCompression:-2.1}%)");

                SCR_DrawTextRightAlign(ref y, $"packet loss = {(int)AsyncNetwork.client.IncomingPacketLoss}%, client prediction = {AsyncNetwork.client.Prediction}");
                SCR_DrawTextRightAlign(ref y, $"predicted frames: {AsyncNetwork.client.PredictedFrames}");
            }

            return y;
        }

#endif

        static float SCR_DrawSoundDecoders(float y)
        {
            int index, numActiveDecoders;

            index = -1;
            numActiveDecoders = 0;
            while ((index = soundSystem.GetSoundDecoderInfo(index, out var decoderInfo)) != -1)
            {
                var localTime = decoderInfo.current44kHzTime - decoderInfo.start44kHzTime;
                var sampleTime = decoderInfo.num44kHzSamples / decoderInfo.numChannels;
                var percent = localTime > sampleTime
                    ? decoderInfo.looping ? (localTime % sampleTime) * 100 / sampleTime : 100
                    : localTime * 100 / sampleTime;
                SCR_DrawTextLeftAlign(ref y, $"{numActiveDecoders:3}: {percent:3}% ({decoderInfo.lastVolume:1.2}) {decoderInfo.format}: {decoderInfo.name} ({decoderInfo.numBytes >> 10}kB)");
                numActiveDecoders++;
            }
            return y;
        }
    }
}
