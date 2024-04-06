using System.NumericsX.OpenStack;
using static System.NumericsX.Platform;
using static System.NumericsX.OpenStack.OpenStack;
using System.Runtime.CompilerServices;
using System.Linq;
using p_generic = System.NumericsX.SimdGeneric;
using p_simd = System.NumericsX.Simd;
using static System.NumericsX.PlatformNative;

namespace System.NumericsX
{
    public unsafe class SimdTest
    {
        const int COUNT = 1024;     // data count
        const int NUMTESTS = 2048;      // number of tests

        const long RANDOM_SEED = 1013904223L;    //((int)idLib::sys->GetClockTicks())

        static TimeSpan baseClocks = default;

        static void StartRecordTime(out DateTime start)
            => start = DateTime.Now;

        static void StopRecordTime(out DateTime end)
            => end = DateTime.Now;

        static void GetBest(DateTime start, DateTime end, ref TimeSpan best)
        {
            if (best == default || (end - start) < best) best = end - start;
        }

        static void PrintClocks(string s, int count, TimeSpan clocks, TimeSpan otherClocks = default)
        {
            Printf(s);
            Printf(new string(' ', Math.Max(0, 48 - stringX.LengthWithoutColors(s))));
            clocks -= baseClocks;
            if (otherClocks != default && clocks != default)
            {
                otherClocks -= baseClocks;
                var p = (int)((otherClocks.Ticks - clocks.Ticks) * 100f / otherClocks.Ticks);
                Printf($"c = {count,4}, clcks = {clocks.Ticks,5}, {p}%\n");
            }
            else Printf($"c = {count,4}, clcks = {clocks.Ticks,5}\n");
        }

        static void GetBaseClocks()
        {
            DateTime start, end; TimeSpan bestClocks = default;
            for (var i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
            }
            baseClocks = bestClocks;
        }

        static void TestAdd()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fdst0 = stackalloc float[COUNT];
            var fdst1 = stackalloc float[COUNT];
            var fsrc0 = stackalloc float[COUNT];
            var fsrc1 = stackalloc float[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                fsrc0[i] = srnd.CRandomFloat() * 10f;
                fsrc1[i] = srnd.CRandomFloat() * 10f;
            }

            Printf("====================================\n");

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Add(fdst0, 4f, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Add(float + float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Add(fdst1, 4f, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Add(float + float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Addv(fdst0, fsrc0, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Add(float[] + float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Addv(fdst1, fsrc0, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Add(float[] + float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestSub()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fdst0 = stackalloc float[COUNT];
            var fdst1 = stackalloc float[COUNT];
            var fsrc0 = stackalloc float[COUNT];
            var fsrc1 = stackalloc float[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                fsrc0[i] = srnd.CRandomFloat() * 10f;
                fsrc1[i] = srnd.CRandomFloat() * 10f;
            }

            Printf("====================================\n");

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Sub(fdst0, 4f, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Sub(float + float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Sub(fdst1, 4f, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Sub(float + float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Subv(fdst0, fsrc0, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Sub(float[] + float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Subv(fdst1, fsrc0, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Sub(float[] + float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestMul()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fdst0 = stackalloc float[COUNT];
            var fdst1 = stackalloc float[COUNT];
            var fsrc0 = stackalloc float[COUNT];
            var fsrc1 = stackalloc float[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                fsrc0[i] = srnd.CRandomFloat() * 10f;
                fsrc1[i] = srnd.CRandomFloat() * 10f;
            }

            Printf("====================================\n");

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Mul(fdst0, 4f, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Mul(float * float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Mul(fdst1, 4f, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Mul(float * float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Mulv(fdst0, fsrc0, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Mul(float[] * float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Mulv(fdst1, fsrc0, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Mul(float[] * float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestDiv()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fdst0 = stackalloc float[COUNT];
            var fdst1 = stackalloc float[COUNT];
            var fsrc0 = stackalloc float[COUNT];
            var fsrc1 = stackalloc float[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                fsrc0[i] = srnd.CRandomFloat() * 10f;
                do fsrc1[i] = srnd.CRandomFloat() * 10f;
                while (MathX.Fabs(fsrc1[i]) < 0.1f);
            }

            Printf("====================================\n");

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Div(fdst0, 4f, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Div(float * float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Div(fdst1, 4f, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Div(float * float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Divv(fdst0, fsrc0, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Div(float[] * float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Divv(fdst1, fsrc0, fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-3f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Div(float[] * float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestMulAdd()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fdst0 = stackalloc float[COUNT];
            var fdst1 = stackalloc float[COUNT];
            var fsrc0 = stackalloc float[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++) fsrc0[i] = srnd.CRandomFloat() * 10f;

            Printf("====================================\n");

            for (j = 0; j < 50 && j < COUNT; j++)
            {
                bestClocksGeneric = default;
                for (i = 0; i < NUMTESTS; i++)
                {
                    for (var k = 0; k < COUNT; k++) fdst0[k] = k;
                    StartRecordTime(out start);
                    p_generic.MulAdd(fdst0, 0.123f, fsrc0, j);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                PrintClocks($"generic.MulAdd(float * float[{j,2}])", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (i = 0; i < NUMTESTS; i++)
                {
                    for (var k = 0; k < COUNT; k++) fdst1[k] = k;
                    StartRecordTime(out start);
                    p_simd.MulAdd(fdst1, 0.123f, fsrc0, j);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
                result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MulAdd(float * float[{j,2}]) {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        static void TestMulSub()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fdst0 = stackalloc float[COUNT];
            var fdst1 = stackalloc float[COUNT];
            var fsrc0 = stackalloc float[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++) fsrc0[i] = srnd.CRandomFloat() * 10f;

            Printf("====================================\n");

            for (j = 0; j < 50 && j < COUNT; j++)
            {
                bestClocksGeneric = default;
                for (i = 0; i < NUMTESTS; i++)
                {
                    for (var k = 0; k < COUNT; k++) fdst0[k] = k;
                    StartRecordTime(out start);
                    p_generic.MulSub(fdst0, 0.123f, fsrc0, j);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                PrintClocks($"generic.MulSub(float * float[{j,2}])", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (i = 0; i < NUMTESTS; i++)
                {
                    for (var k = 0; k < COUNT; k++) fdst1[k] = k;
                    StartRecordTime(out start);
                    p_simd.MulSub(fdst1, 0.123f, fsrc0, j);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
                result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MulSub(float * float[{j,2}]) {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        static void TestDot()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fdst0 = stackalloc float[COUNT];
            var fdst1 = stackalloc float[COUNT];
            var fsrc0 = stackalloc float[COUNT];
            var fsrc1 = stackalloc float[COUNT];
            var v3src0 = stackalloc Vector3[COUNT];
            var v3src1 = stackalloc Vector3[COUNT];
            var v3constant = new Vector3(1f, 2f, 3f);
            var v4src0 = stackalloc Plane[COUNT];
            var v4constant = new Plane(1f, 2f, 3f, 4f);
            var drawVerts = stackalloc DrawVert[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                fsrc0[i] = srnd.CRandomFloat() * 10f;
                fsrc1[i] = srnd.CRandomFloat() * 10f;
                v3src0[i].x = srnd.CRandomFloat() * 10f;
                v3src0[i].y = srnd.CRandomFloat() * 10f;
                v3src0[i].z = srnd.CRandomFloat() * 10f;
                v3src1[i].x = srnd.CRandomFloat() * 10f;
                v3src1[i].y = srnd.CRandomFloat() * 10f;
                v3src1[i].z = srnd.CRandomFloat() * 10f;
                v4src0[i] = v3src0[i];
                v4src0[i].d = srnd.CRandomFloat() * 10f;
                drawVerts[i].xyz = v3src0[i];
            }

            Printf("====================================\n");

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Dotcv(fdst0, v3constant, v3src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Dot(Vector3 * Vector3[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Dotcv(fdst1, v3constant, v3src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Dot(Vector3 * Vector3[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Dotcp(fdst0, v3constant, v4src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Dot(Vector3 * Plane[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Dotcp(fdst1, v3constant, v4src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Dot(Vector3 * Plane[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);


            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Dotcd(fdst0, v3constant, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Dot(Vector3 * DrawVert[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Dotcd(fdst1, v3constant, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Dot(Vector3 * DrawVert[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Dotpv(fdst0, v4constant, v3src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Dot(Plane * Vector3[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Dotpv(fdst1, v4constant, v3src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Dot(Plane * Vector3[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Dotpp(fdst0, v4constant, v4src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Dot(Plane * Plane[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Dotpp(fdst1, v4constant, v4src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Dot(Plane * Plane[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Dotpd(fdst0, v4constant, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Dot(Plane * DrawVert[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Dotpd(fdst1, v4constant, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-5f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Dot(Plane * DrawVert[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Dotvv(fdst0, v3src0, v3src1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Dot(Vector3[] * Vector3[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Dotvv(fdst1, v3src0, v3src1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (MathX.Fabs(fdst0[i] - fdst1[i]) > 1e-4f) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Dot(Vector3[] * Vector3[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            Printf("====================================\n");

            float dot1 = 0f, dot2 = 0f;
            for (j = 0; j < 50 && j < COUNT; j++)
            {
                bestClocksGeneric = default;
                for (i = 0; i < NUMTESTS; i++)
                {
                    StartRecordTime(out start);
                    p_generic.Dotff(out dot1, fsrc0, fsrc1, j);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                PrintClocks($"generic.Dot(float[{j,2}] * float[{j,2}])", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (i = 0; i < NUMTESTS; i++)
                {
                    StartRecordTime(out start);
                    p_simd.Dotff(out dot2, fsrc0, fsrc1, j);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }
                result = MathX.Fabs(dot1 - dot2) < 1e-4f ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.Dot(float[{j,2}] * float[{j,2}]) {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        static void TestCompare()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fsrc0 = stackalloc float[COUNT];
            var bytedst = stackalloc byte[COUNT];
            var bytedst2 = stackalloc byte[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++) fsrc0[i] = srnd.CRandomFloat() * 10f;

            Printf("====================================\n");

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.CmpGT(bytedst, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CmpGT(float[] >= float)", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.CmpGT(bytedst2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (bytedst[i] != bytedst2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CmpGT(float[] >= float) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.InitBlock(bytedst, 0, (uint)COUNT);
                StartRecordTime(out start);
                p_generic.CmpGTb(bytedst, 2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CmpGT(2, float[] >= float)", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.InitBlock(bytedst2, 0, (uint)COUNT);
                StartRecordTime(out start);
                p_simd.CmpGTb(bytedst2, 2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (bytedst[i] != bytedst2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CmpGT(2, float[] >= float) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            // ======================

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.CmpGE(bytedst, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CmpGE(float[] >= float)", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.CmpGE(bytedst2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (bytedst[i] != bytedst2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CmpGE(float[] >= float) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.InitBlock(bytedst, 0, (uint)COUNT);
                StartRecordTime(out start);
                p_generic.CmpGEb(bytedst, 2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CmpGE( 2, float[] >= float )", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.InitBlock(bytedst2, 0, (uint)COUNT);
                StartRecordTime(out start);
                p_simd.CmpGEb(bytedst2, 2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (bytedst[i] != bytedst2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CmpGE(2, float[] >= float) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            // ======================

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.CmpLT(bytedst, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CmpLT(float[] >= float)", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.CmpLT(bytedst2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (bytedst[i] != bytedst2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CmpLT(float[] >= float) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.InitBlock(bytedst, 0, (uint)COUNT);
                StartRecordTime(out start);
                p_generic.CmpLTb(bytedst, 2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CmpLT(2, float[] >= float)", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.InitBlock(bytedst2, 0, (uint)COUNT);
                StartRecordTime(out start);
                p_simd.CmpLTb(bytedst2, 2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (bytedst[i] != bytedst2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CmpLT(2, float[] >= float) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            // ======================

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.CmpLE(bytedst, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CmpLE(float[] >= float)", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.CmpLE(bytedst2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (bytedst[i] != bytedst2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CmpLE(float[] >= float) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.InitBlock(bytedst, 0, COUNT);
                StartRecordTime(out start);
                p_generic.CmpLEb(bytedst, 2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CmpLE(2, float[] >= float)", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.InitBlock(bytedst2, 0, COUNT);
                StartRecordTime(out start);
                p_simd.CmpLEb(bytedst2, 2, fsrc0, 0f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (bytedst[i] != bytedst2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CmpLE(2, float[] >= float) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestMinMax()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fsrc0 = stackalloc float[COUNT];
            var v2src0 = stackalloc Vector2[COUNT];
            var v3src0 = stackalloc Vector3[COUNT];
            var drawVerts = stackalloc DrawVert[COUNT];
            var indexes = stackalloc int[COUNT];
            float min = 0f, max = 0f, min2 = 0f, max2 = 0f;
            Vector2 v2min = default, v2max = default, v2min2 = default, v2max2 = default;
            Vector3 vmin = default, vmax = default, vmin2 = default, vmax2 = default;
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                fsrc0[i] = srnd.CRandomFloat() * 10f;
                v2src0[i].x = srnd.CRandomFloat() * 10f;
                v2src0[i].y = srnd.CRandomFloat() * 10f;
                v3src0[i].x = srnd.CRandomFloat() * 10f;
                v3src0[i].y = srnd.CRandomFloat() * 10f;
                v3src0[i].z = srnd.CRandomFloat() * 10f;
                drawVerts[i].xyz = v3src0[i];
                indexes[i] = i;
            }

            Printf("====================================\n");

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                min = MathX.INFINITY; max = -MathX.INFINITY;
                StartRecordTime(out start);
                p_generic.MinMaxf(out min, out max, fsrc0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MinMax(float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.MinMaxf(out min2, out max2, fsrc0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            result = min == min2 && max == max2 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MinMax(float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.MinMax2(out v2min, out v2max, v2src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MinMax(Vector2[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.MinMax2(out v2min2, out v2max2, v2src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            result = v2min == v2min2 && v2max == v2max2 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MinMax(Vector2[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.MinMax3(out vmin, out vmax, v3src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MinMax(Vector3[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.MinMax3(out vmin2, out vmax2, v3src0, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            result = vmin == vmin2 && vmax == vmax2 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MinMax(Vector3[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.MinMaxd(out vmin, out vmax, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MinMax(DrawVert[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.MinMaxd(out vmin2, out vmax2, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            result = vmin == vmin2 && vmax == vmax2 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MinMax(DrawVert[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.MinMaxdi(out vmin, out vmax, drawVerts, indexes, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MinMax(DrawVert[], indexes[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.MinMaxdi(out vmin2, out vmax2, drawVerts, indexes, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            result = vmin == vmin2 && vmax == vmax2 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MinMax(DrawVert[], indexes[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestClamp()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fdst0 = stackalloc float[COUNT];
            var fdst1 = stackalloc float[COUNT];
            var fsrc0 = stackalloc float[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
                fsrc0[i] = srnd.CRandomFloat() * 10f;

            Printf("====================================\n");

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.Clamp(fdst0, fsrc0, -1f, 1f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Clamp(float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.Clamp(fdst1, fsrc0, -1f, 1f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++)
                if (fdst0[i] != fdst1[i])
                    break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Clamp(float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.ClampMin(fdst0, fsrc0, -1f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.ClampMin(float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.ClampMin(fdst1, fsrc0, -1f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (fdst0[i] != fdst1[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.ClampMin(float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);


            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.ClampMax(fdst0, fsrc0, 1f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.ClampMax(float[])", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.ClampMax(fdst1, fsrc0, 1f, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (fdst0[i] != fdst1[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.ClampMax(float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestMemcpy()
        {
            int i, j;
            var test0 = stackalloc byte[8192];
            var test1 = stackalloc byte[8192];

            var random = new RandomX(RANDOM_SEED);

            Printf("====================================\n");

            for (i = 5; i < 8192; i += 31)
            {
                for (j = 0; j < i; j++) test0[j] = (byte)random.RandomInt(255);
                p_simd.Memcpy(test1, test0, 8192);
                for (j = 0; j < i; j++) if (test1[j] != test0[j]) { Printf($"   simd.Memcpy() {S_COLOR_RED}X\n"); return; }
            }
            Printf("   simd.Memcpy() ok\n");
        }

        static void TestMemset()
        {
            int i, j, k;
            var test = stackalloc byte[8192];
            for (i = 0; i < 8192; i++)
                test[i] = 0;

            for (i = 5; i < 8192; i += 31)
                for (j = -1; j <= 1; j++)
                {
                    p_simd.Memset(test, j, i);
                    for (k = 0; k < i; k++) if (test[k] != (byte)j) { Printf($"   simd.Memset() {S_COLOR_RED}X\n"); return; }
                }
            Printf("   simd.Memset() ok\n");
        }

        const float MATX_SIMD_EPSILON = 1e-5f;
        static void TestMatXMultiplyVecX()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            string result;

            var mat = new MatrixX();
            var src = new VectorX(6);
            var dst = new VectorX(6);
            var tst = new VectorX(6);

            src[0] = 1f;
            src[1] = 2f;
            src[2] = 3f;
            src[3] = 4f;
            src[4] = 5f;
            src[5] = 6f;

            Printf("================= NxN * Nx1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(i, i, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyVecX {i}x{i}*{i}x1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyVecX {i}x{i}*{i}x1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= Nx6 * 6x1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(i, 6, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyVecX {i}x6*6x1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyVecX {i}x6*6x1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= 6xN * Nx1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(6, i, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyVecX 6x{i}*{i}x1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyVecX 6x{i}*{i}x1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        static void TestMatXMultiplyAddVecX()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            string result;
            var mat = new MatrixX();
            var src = new VectorX(6);
            var dst = new VectorX(6);
            var tst = new VectorX(6);

            src[0] = 1f;
            src[1] = 2f;
            src[2] = 3f;
            src[3] = 4f;
            src[4] = 5f;
            src[5] = 6f;

            Printf("================= NxN * Nx1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(i, i, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyAddVecX {i}x{i}*{i}1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyAddVecX {i}x{i}*{i}1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= Nx6 * 6x1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(i, 6, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyAddVecX {i}x6*6x1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyAddVecX {i}x6*6x1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= 6xN * Nx1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(6, i, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyAddVecX 6x{i}*{i}x1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyAddVecX 6x{i}*{i}x1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        static void TestMatXTransposeMultiplyVecX()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            string result;
            var mat = new MatrixX();
            var src = new VectorX(6);
            var dst = new VectorX(6);
            var tst = new VectorX(6);

            src[0] = 1f;
            src[1] = 2f;
            src[2] = 3f;
            src[3] = 4f;
            src[4] = 5f;
            src[5] = 6f;

            Printf("================= Nx6 * Nx1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(i, 6, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_TransposeMultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_TransposeMulVecX {i}x6*{i}x1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_simd.MatX_TransposeMultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_TransposeMulVecX {i}x6*{i}x1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= 6xN * 6x1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(6, i, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_TransposeMultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_TransposeMulVecX 6x{i}*6x1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_simd.MatX_TransposeMultiplyVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_TransposeMulVecX 6x{i}*6x1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        static void TestMatXTransposeMultiplyAddVecX()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            string result;
            var mat = new MatrixX();
            var src = new VectorX(6);
            var dst = new VectorX(6);
            var tst = new VectorX(6);

            src[0] = 1f;
            src[1] = 2f;
            src[2] = 3f;
            src[3] = 4f;
            src[4] = 5f;
            src[5] = 6f;

            Printf("================= Nx6 * Nx1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(i, 6, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_TransposeMultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_TransposeMulAddVecX {i}x6*{i}x1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_simd.MatX_TransposeMultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_TransposeMulAddVecX {i}x6*{i}x1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= 6xN * 6x1 ===================\n");

            for (i = 1; i <= 6; i++)
            {
                mat.Random(6, i, RANDOM_SEED, -10f, 10f);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_generic.MatX_TransposeMultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_TransposeMulAddVecX 6x{i}*6x1", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    dst.Zero();
                    StartRecordTime(out start);
                    p_simd.MatX_TransposeMultiplyAddVecX(dst, mat, src);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_TransposeMulAddVecX 6x{i}*6x1 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        const float TEST_VALUE_RANGE = 10f;
        const float MATX_MATX_SIMD_EPSILON = 1e-4f;
        static void TestMatXMultiplyMatX()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            string result;
            MatrixX m1 = new(), m2 = new(), dst = new(), tst;

            Printf("================= NxN * Nx6 ===================\n");

            // NxN * Nx6
            for (i = 1; i <= 5; i++)
            {
                m1.Random(i, i, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                m2.Random(i, 6, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                dst.SetSize(i, 6);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyMatX {i}x{i}*{i}x6", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyMatX {i}x{i}*{i}x6 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= 6xN * Nx6 ===================\n");

            // 6xN * Nx6
            for (i = 1; i <= 5; i++)
            {
                m1.Random(6, i, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                m2.Random(i, 6, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                dst.SetSize(6, 6);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyMatX 6x{i}*{i}x6", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyMatX 6x{i}*{i}x6 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= Nx6 * 6xN ===================\n");

            // Nx6 * 6xN
            for (i = 1; i <= 5; i++)
            {
                m1.Random(i, 6, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                m2.Random(6, i, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                dst.SetSize(i, i);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyMatX {i}x6*6x{i}", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyMatX {i}x6*6x{i} {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= 6x6 * 6xN ===================\n");

            // 6x6 * 6xN
            for (i = 1; i <= 6; i++)
            {
                m1.Random(6, 6, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                m2.Random(6, i, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                dst.SetSize(6, i);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_generic.MatX_MultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_MultiplyMatX 6x6*6x{i}", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_simd.MatX_MultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_MultiplyMatX 6x6*6x{i} {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        static void TestMatXTransposeMultiplyMatX()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            string result;
            MatrixX m1 = new(), m2 = new(), dst = new(), tst;

            Printf("================= Nx6 * NxN ===================\n");

            // Nx6 * NxN
            for (i = 1; i <= 5; i++)
            {
                m1.Random(i, 6, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                m2.Random(i, i, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                dst.SetSize(6, i);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_generic.MatX_TransposeMultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_TransMultiplyMatX {i}x6*{i}x{i}", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_simd.MatX_TransposeMultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_TransMultiplyMatX {i}x6*{i}x{i} {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }

            Printf("================= 6xN * 6x6 ===================\n");

            // 6xN * 6x6
            for (i = 1; i <= 6; i++)
            {
                m1.Random(6, i, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                m2.Random(6, 6, RANDOM_SEED, -TEST_VALUE_RANGE, TEST_VALUE_RANGE);
                dst.SetSize(i, 6);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_generic.MatX_TransposeMultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = dst;

                PrintClocks($"generic.MatX_TransMultiplyMatX 6x{i}*6x6", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    p_simd.MatX_TransposeMultiplyMatX(dst, m1, m2);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = dst.Compare(tst, MATX_MATX_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_TransMultiplyMatX 6x{i}*6x6 {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        const float MATX_LTS_SIMD_EPSILON = 1f;
        const int MATX_LTS_SOLVE_SIZE = 100;
        static void TestMatXLowerTriangularSolve()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            string result;
            MatrixX L = new();
            VectorX x = new(), b = new(), tst;

            Printf("====================================\n");

            L.Random(MATX_LTS_SOLVE_SIZE, MATX_LTS_SOLVE_SIZE, 0, -1f, 1f);
            x.SetSize(MATX_LTS_SOLVE_SIZE);
            b.Random(MATX_LTS_SOLVE_SIZE, 0, -1f, 1f);

            for (i = 1; i < MATX_LTS_SOLVE_SIZE; i++)
            {
                x.Zero(i);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    fixed (float* xF = x.p, bF = b.p) p_generic.MatX_LowerTriangularSolve(L, xF, bF, i);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = x;
                x.Zero();

                PrintClocks($"generic.MatX_LowerTriangularSolve {i}x{i}", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    fixed (float* xF = x.p, bF = b.p) p_simd.MatX_LowerTriangularSolve(L, xF, bF, i);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = x.Compare(tst, MATX_LTS_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_LowerTriangularSolve {i}x{i} {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        static void TestMatXLowerTriangularSolveTranspose()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            string result;
            MatrixX L = new();
            VectorX x = new(), b = new(), tst;

            Printf("====================================\n");

            L.Random(MATX_LTS_SOLVE_SIZE, MATX_LTS_SOLVE_SIZE, 0, -1f, 1f);
            x.SetSize(MATX_LTS_SOLVE_SIZE);
            b.Random(MATX_LTS_SOLVE_SIZE, 0, -1f, 1f);

            for (i = 1; i < MATX_LTS_SOLVE_SIZE; i++)
            {
                x.Zero(i);

                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    fixed (float* xF = x.p, bF = b.p) p_generic.MatX_LowerTriangularSolveTranspose(L, xF, bF, i);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }
                tst = x;
                x.Zero();

                PrintClocks($"generic.MatX_LowerTriangularSolveT {i}x{i}", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    StartRecordTime(out start);
                    fixed (float* xF = x.p, bF = b.p) p_simd.MatX_LowerTriangularSolveTranspose(L, xF, bF, i);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = x.Compare(tst, MATX_LTS_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_LowerTriangularSolveT {i}x{i} {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        const float MATX_LDLT_SIMD_EPSILON = 0.1f;
        const int MATX_LDLT_FACTOR_SOLVE_SIZE = 64;
        static void TestMatXLDLTFactor()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            string result;
            MatrixX src = new(), original = new(), mat1 = new(), mat2 = new();
            VectorX invDiag1 = new(), invDiag2 = new();

            Printf("====================================\n");

            original.SetSize(MATX_LDLT_FACTOR_SOLVE_SIZE, MATX_LDLT_FACTOR_SOLVE_SIZE);
            src.Random(MATX_LDLT_FACTOR_SOLVE_SIZE, MATX_LDLT_FACTOR_SOLVE_SIZE, 0, -1f, 1f);
            src.TransposeMultiply(original, src);

            for (i = 1; i < MATX_LDLT_FACTOR_SOLVE_SIZE; i++)
            {
                bestClocksGeneric = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    mat1 = original;
                    invDiag1.Zero(MATX_LDLT_FACTOR_SOLVE_SIZE);
                    StartRecordTime(out start);
                    p_generic.MatX_LDLTFactor(mat1, invDiag1, i);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksGeneric);
                }

                PrintClocks($"generic.MatX_LDLTFactor {i}x{i}", 1, bestClocksGeneric);

                bestClocksSIMD = default;
                for (j = 0; j < NUMTESTS; j++)
                {
                    mat2 = original;
                    invDiag2.Zero(MATX_LDLT_FACTOR_SOLVE_SIZE);
                    StartRecordTime(out start);
                    p_simd.MatX_LDLTFactor(mat2, invDiag2, i);
                    StopRecordTime(out end);
                    GetBest(start, end, ref bestClocksSIMD);
                }

                result = mat1.Compare(mat2, MATX_LDLT_SIMD_EPSILON) && invDiag1.Compare(invDiag2, MATX_LDLT_SIMD_EPSILON) ? "ok" : $"{S_COLOR_RED}X";
                PrintClocks($"   simd.MatX_LDLTFactor {i}x{i} {result}", 1, bestClocksSIMD, bestClocksGeneric);
            }
        }

        static void TestBlendJoints()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var baseJoints = stackalloc JointQuat[COUNT];
            var joints1 = stackalloc JointQuat[COUNT];
            var joints2 = stackalloc JointQuat[COUNT];
            var blendJoints = stackalloc JointQuat[COUNT];
            var index = stackalloc int[COUNT];
            var lerp = 0.3f;
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                Angles angles = new();
                angles[0] = srnd.CRandomFloat() * 180f;
                angles[1] = srnd.CRandomFloat() * 180f;
                angles[2] = srnd.CRandomFloat() * 180f;
                baseJoints[i].q = angles.ToQuat();
                baseJoints[i].t[0] = srnd.CRandomFloat() * 10f;
                baseJoints[i].t[1] = srnd.CRandomFloat() * 10f;
                baseJoints[i].t[2] = srnd.CRandomFloat() * 10f;
                angles[0] = srnd.CRandomFloat() * 180f;
                angles[1] = srnd.CRandomFloat() * 180f;
                angles[2] = srnd.CRandomFloat() * 180f;
                blendJoints[i].q = angles.ToQuat();
                blendJoints[i].t[0] = srnd.CRandomFloat() * 10f;
                blendJoints[i].t[1] = srnd.CRandomFloat() * 10f;
                blendJoints[i].t[2] = srnd.CRandomFloat() * 10f;
                index[i] = i;
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < COUNT; j++) joints1[j] = baseJoints[j];
                StartRecordTime(out start);
                p_generic.BlendJoints(joints1, blendJoints, lerp, index, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.BlendJoints()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < COUNT; j++) joints2[j] = baseJoints[j];
                StartRecordTime(out start);
                p_simd.BlendJoints(joints2, blendJoints, lerp, index, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++)
            {
                if (!joints1[i].t.Compare(joints2[i].t, 1e-3f)) break;
                if (!joints1[i].q.Compare(joints2[i].q, 1e-2f)) break;
            }
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.BlendJoints() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestConvertJointQuatsToJointMats()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var baseJoints = stackalloc JointQuat[COUNT];
            var joints1 = stackalloc JointMat[COUNT];
            var joints2 = stackalloc JointMat[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                Angles angles;
                angles.pitch = srnd.CRandomFloat() * 180f;
                angles.yaw = srnd.CRandomFloat() * 180f;
                angles.roll = srnd.CRandomFloat() * 180f;
                baseJoints[i].q = angles.ToQuat();
                baseJoints[i].t[0] = srnd.CRandomFloat() * 10f;
                baseJoints[i].t[1] = srnd.CRandomFloat() * 10f;
                baseJoints[i].t[2] = srnd.CRandomFloat() * 10f;
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.ConvertJointQuatsToJointMats(joints1, baseJoints, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.ConvertJointQuatsToJointMats()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.ConvertJointQuatsToJointMats(joints2, baseJoints, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (!joints1[i].Compare(joints2[i], 1e-4f)) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.ConvertJointQuatsToJointMats() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestConvertJointMatsToJointQuats()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var baseJoints = stackalloc JointMat[COUNT];
            var joints1 = stackalloc JointQuat[COUNT];
            var joints2 = stackalloc JointQuat[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                Angles angles;
                angles.pitch = srnd.CRandomFloat() * 180f;
                angles.yaw = srnd.CRandomFloat() * 180f;
                angles.roll = srnd.CRandomFloat() * 180f;
                baseJoints[i].SetRotation(angles.ToMat3());
                Vector3 v;
                v.x = srnd.CRandomFloat() * 10f;
                v.y = srnd.CRandomFloat() * 10f;
                v.z = srnd.CRandomFloat() * 10f;
                baseJoints[i].SetTranslation(v);
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.ConvertJointMatsToJointQuats(joints1, baseJoints, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.ConvertJointMatsToJointQuats()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.ConvertJointMatsToJointQuats(joints2, baseJoints, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++)
            {
                if (!joints1[i].q.Compare(joints2[i].q, 1e-4f)) { Printf($"ConvertJointMatsToJointQuats: broken q {i}\n"); break; }
                if (!joints1[i].t.Compare(joints2[i].t, 1e-4f)) { Printf($"ConvertJointMatsToJointQuats: broken t {i}\n"); break; }
            }
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.ConvertJointMatsToJointQuats() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestTransformJoints()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var joints = stackalloc JointMat[COUNT + 1];
            var joints1 = stackalloc JointMat[COUNT + 1];
            var joints2 = stackalloc JointMat[COUNT + 1];
            var parents = stackalloc int[COUNT + 1];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i <= COUNT; i++)
            {
                Angles angles;
                angles.pitch = srnd.CRandomFloat() * 180f;
                angles.yaw = srnd.CRandomFloat() * 180f;
                angles.roll = srnd.CRandomFloat() * 180f;
                joints[i].SetRotation(angles.ToMat3());
                Vector3 v;
                v.x = srnd.CRandomFloat() * 2f;
                v.y = srnd.CRandomFloat() * 2f;
                v.z = srnd.CRandomFloat() * 2f;
                joints[i].SetTranslation(v);
                parents[i] = i - 1;
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j <= COUNT; j++) joints1[j] = joints[j];
                StartRecordTime(out start);
                p_generic.TransformJoints(joints1, parents, 1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.TransformJoints()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j <= COUNT; j++) joints2[j] = joints[j];
                StartRecordTime(out start);
                p_simd.TransformJoints(joints2, parents, 1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (!joints1[i + 1].Compare(joints2[i + 1], 1e-4f)) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.TransformJoints() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestUntransformJoints()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var joints = stackalloc JointMat[COUNT + 1];
            var joints1 = stackalloc JointMat[COUNT + 1];
            var joints2 = stackalloc JointMat[COUNT + 1];
            var parents = stackalloc int[COUNT + 1];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i <= COUNT; i++)
            {
                Angles angles;
                angles.pitch = srnd.CRandomFloat() * 180f;
                angles.yaw = srnd.CRandomFloat() * 180f;
                angles.roll = srnd.CRandomFloat() * 180f;
                joints[i].SetRotation(angles.ToMat3());
                Vector3 v;
                v.x = srnd.CRandomFloat() * 2f;
                v.y = srnd.CRandomFloat() * 2f;
                v.z = srnd.CRandomFloat() * 2f;
                joints[i].SetTranslation(v);
                parents[i] = i - 1;
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j <= COUNT; j++) joints1[j] = joints[j];
                StartRecordTime(out start);
                p_generic.UntransformJoints(joints1, parents, 1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.UntransformJoints()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j <= COUNT; j++) joints2[j] = joints[j];
                StartRecordTime(out start);
                p_simd.UntransformJoints(joints2, parents, 1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (!joints1[i + 1].Compare(joints2[i + 1], 1e-4f)) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.UntransformJoints() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        const int NUMJOINTS = 64;
        const int NUMVERTS = COUNT / 2;
        static void TestTransformVerts()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var drawVerts1 = stackalloc DrawVert[NUMVERTS];
            var drawVerts2 = stackalloc DrawVert[NUMVERTS];
            var joints = stackalloc JointMat[NUMJOINTS];
            var weights = stackalloc Vector4[COUNT];
            var weightIndex = stackalloc int[COUNT * 2];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < NUMJOINTS; i++)
            {
                Angles angles;
                angles.pitch = srnd.CRandomFloat() * 180f;
                angles.yaw = srnd.CRandomFloat() * 180f;
                angles.roll = srnd.CRandomFloat() * 180f;
                joints[i].SetRotation(angles.ToMat3());
                Vector3 v;
                v.x = srnd.CRandomFloat() * 2f;
                v.y = srnd.CRandomFloat() * 2f;
                v.z = srnd.CRandomFloat() * 2f;
                joints[i].SetTranslation(v);
            }

            for (i = 0; i < COUNT; i++)
            {
                weights[i].x = srnd.CRandomFloat() * 2f;
                weights[i].y = srnd.CRandomFloat() * 2f;
                weights[i].z = srnd.CRandomFloat() * 2f;
                weights[i].w = srnd.CRandomFloat();
                weightIndex[i * 2 + 0] = (i * NUMJOINTS / COUNT) * sizeof(JointMat);
                weightIndex[i * 2 + 1] = i & 1;
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.TransformVerts(drawVerts1, NUMVERTS, joints, weights, weightIndex, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.TransformVerts()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.TransformVerts(drawVerts2, NUMVERTS, joints, weights, weightIndex, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < NUMVERTS; i++) if (!drawVerts1[i].xyz.Compare(drawVerts2[i].xyz, 0.5f)) break;
            result = (i >= NUMVERTS) ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.TransformVerts() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestTracePointCull()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var planes = stackalloc Plane[4];
            var drawVerts = stackalloc DrawVert[COUNT];
            var cullBits1 = stackalloc byte[COUNT];
            var cullBits2 = stackalloc byte[COUNT];
            byte totalOr1 = 0, totalOr2 = 0;
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            planes[0].SetNormal(new Vector3(1, 0, 0));
            planes[1].SetNormal(new Vector3(-1, 0, 0));
            planes[2].SetNormal(new Vector3(0, 1, 0));
            planes[3].SetNormal(new Vector3(0, -1, 0));
            planes[0].d = -5.3f;
            planes[1].d = 5.3f;
            planes[2].d = -3.4f;
            planes[3].d = 3.4f;

            for (i = 0; i < COUNT; i++) for (j = 0; j < 3; j++) drawVerts[i].xyz[j] = srnd.CRandomFloat() * 10f;

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.TracePointCull(cullBits1, out totalOr1, 0f, planes, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.TracePointCull()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.TracePointCull(cullBits2, out totalOr2, 0f, planes, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (cullBits1[i] != cullBits2[i]) break;
            result = i >= COUNT && totalOr1 == totalOr2 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.TracePointCull() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestDecalPointCull()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var planes = stackalloc Plane[6];
            var drawVerts = stackalloc DrawVert[COUNT];
            var cullBits1 = stackalloc byte[COUNT];
            var cullBits2 = stackalloc byte[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            planes[0].SetNormal(new Vector3(1, 0, 0));
            planes[1].SetNormal(new Vector3(-1, 0, 0));
            planes[2].SetNormal(new Vector3(0, 1, 0));
            planes[3].SetNormal(new Vector3(0, -1, 0));
            planes[4].SetNormal(new Vector3(0, 0, 1));
            planes[5].SetNormal(new Vector3(0, 0, -1));
            planes[0].d = -5.3f;
            planes[1].d = 5.3f;
            planes[2].d = -4.4f;
            planes[3].d = 4.4f;
            planes[4].d = -3.5f;
            planes[5].d = 3.5f;

            for (i = 0; i < COUNT; i++) for (j = 0; j < 3; j++) drawVerts[i].xyz[j] = srnd.CRandomFloat() * 10f;

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.DecalPointCull(cullBits1, planes, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.DecalPointCull()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.DecalPointCull(cullBits2, planes, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (cullBits1[i] != cullBits2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.DecalPointCull() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestOverlayPointCull()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var planes = stackalloc Plane[2];
            var drawVerts = stackalloc DrawVert[COUNT];
            var cullBits1 = stackalloc byte[COUNT];
            var cullBits2 = stackalloc byte[COUNT];
            var texCoords1 = stackalloc Vector2[COUNT];
            var texCoords2 = stackalloc Vector2[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            planes[0].SetNormal(new Vector3(0.3f, 0.2f, 0.9f));
            planes[1].SetNormal(new Vector3(0.9f, 0.2f, 0.3f));
            planes[0].d = -5.3f;
            planes[1].d = -4.3f;

            for (i = 0; i < COUNT; i++) for (j = 0; j < 3; j++) drawVerts[i].xyz[j] = srnd.CRandomFloat() * 10f;

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.OverlayPointCull(cullBits1, texCoords1, planes, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.OverlayPointCull()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.OverlayPointCull(cullBits2, texCoords2, planes, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++)
            {
                if (cullBits1[i] != cullBits2[i]) break;
                if (!texCoords1[i].Compare(texCoords2[i], 1e-4f)) break;
            }
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.OverlayPointCull() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestDeriveTriPlanes()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var drawVerts1 = stackalloc DrawVert[COUNT];
            var drawVerts2 = stackalloc DrawVert[COUNT];
            var planes1 = stackalloc Plane[COUNT];
            var planes2 = stackalloc Plane[COUNT];
            var indexes = stackalloc int[COUNT * 3];
            string result;

            var srnd = new RandomX(RANDOM_SEED);

            for (i = 0; i < COUNT; i++)
            {
                for (j = 0; j < 3; j++) drawVerts1[i].xyz[j] = srnd.CRandomFloat() * 10f;
                for (j = 0; j < 2; j++) drawVerts1[i].st[j] = srnd.CRandomFloat();
                drawVerts2[i] = drawVerts1[i];
            }

            for (i = 0; i < COUNT; i++)
            {
                indexes[i * 3 + 0] = (i + 0) % COUNT;
                indexes[i * 3 + 1] = (i + 1) % COUNT;
                indexes[i * 3 + 2] = (i + 2) % COUNT;
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.DeriveTriPlanesi(planes1, drawVerts1, COUNT, indexes, COUNT * 3);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.DeriveTriPlanes()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.DeriveTriPlanesi(planes2, drawVerts2, COUNT, indexes, COUNT * 3);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (!planes1[i].Compare(planes2[i], 1e-1f, 1e-1f)) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.DeriveTriPlanes() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestDeriveTangents()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var drawVerts1 = stackalloc DrawVert[COUNT];
            var drawVerts2 = stackalloc DrawVert[COUNT];
            var planes1 = stackalloc Plane[COUNT];
            var planes2 = stackalloc Plane[COUNT];
            var indexes = stackalloc int[COUNT * 3];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                for (j = 0; j < 3; j++) drawVerts1[i].xyz[j] = srnd.CRandomFloat() * 10f;
                for (j = 0; j < 2; j++) drawVerts1[i].st[j] = srnd.CRandomFloat();
                drawVerts2[i] = drawVerts1[i];
            }

            for (i = 0; i < COUNT; i++)
            {
                indexes[i * 3 + 0] = (i + 0) % COUNT;
                indexes[i * 3 + 1] = (i + 1) % COUNT;
                indexes[i * 3 + 2] = (i + 2) % COUNT;
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.DeriveTangentsi(planes1, drawVerts1, COUNT, indexes, COUNT * 3);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.DeriveTangents()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.DeriveTangentsi(planes2, drawVerts2, COUNT, indexes, COUNT * 3);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++)
            {
                Vector3 v1, v2;

                v1 = drawVerts1[i].normal; v1.Normalize();
                v2 = drawVerts2[i].normal; v2.Normalize();
                if (!v1.Compare(v2, 1e-1f)) { Printf($"DeriveTangents: broken at normal {i}\n -- expecting {v1} got {v2}"); break; }
                v1 = drawVerts1[i].tangents0; v1.Normalize();
                v2 = drawVerts2[i].tangents0; v2.Normalize();
                if (!v1.Compare(v2, 1e-1f)) { Printf($"DeriveTangents: broken at tangent0 {i} -- expecting {v1} got {v2}\n"); break; }
                v1 = drawVerts1[i].tangents1; v1.Normalize();
                v2 = drawVerts2[i].tangents1; v2.Normalize();
                if (!v1.Compare(v2, 1e-1f)) { Printf($"DeriveTangents: broken at tangent1 {i} -- expecting {v1} got {v2}\n"); break; }
                if (!planes1[i].Compare(planes2[i], 1e-1f, 1e-1f)) break;
            }
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.DeriveTangents() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestDeriveUnsmoothedTangents()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var drawVerts1 = stackalloc DrawVert[COUNT];
            var drawVerts2 = stackalloc DrawVert[COUNT];
            var dominantTris = stackalloc DominantTri[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                for (j = 0; j < 3; j++) drawVerts1[i].xyz[j] = srnd.CRandomFloat() * 10f;
                for (j = 0; j < 2; j++) drawVerts1[i].st[j] = srnd.CRandomFloat();
                drawVerts2[i] = drawVerts1[i];

                dominantTris[i].v2 = (i + 1 + srnd.RandomInt(8)) % COUNT;
                dominantTris[i].v3 = (i + 9 + srnd.RandomInt(8)) % COUNT;
                dominantTris[i].normalizationScale[0] = srnd.CRandomFloat();
                dominantTris[i].normalizationScale[1] = srnd.CRandomFloat();
                dominantTris[i].normalizationScale[2] = srnd.CRandomFloat();
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.DeriveUnsmoothedTangents(drawVerts1, dominantTris, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.DeriveUnsmoothedTangents()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.DeriveUnsmoothedTangents(drawVerts2, dominantTris, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++)
            {
                Vector3 v1, v2;
                v1 = drawVerts1[i].normal; v1.Normalize();
                v2 = drawVerts2[i].normal; v2.Normalize();
                if (!v1.Compare(v2, 1e-1f)) break;
                v1 = drawVerts1[i].tangents0; v1.Normalize();
                v2 = drawVerts2[i].tangents0; v2.Normalize();
                if (!v1.Compare(v2, 1e-1f)) break;
                v1 = drawVerts1[i].tangents1; v1.Normalize();
                v2 = drawVerts2[i].tangents1; v2.Normalize();
                if (!v1.Compare(v2, 1e-1f)) break;
            }
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.DeriveUnsmoothedTangents() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestNormalizeTangents()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var drawVerts1 = stackalloc DrawVert[COUNT];
            var drawVerts2 = stackalloc DrawVert[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    drawVerts1[i].normal[j] = srnd.CRandomFloat() * 10f;
                    drawVerts1[i].tangents0[j] = srnd.CRandomFloat() * 10f;
                    drawVerts1[i].tangents1[j] = srnd.CRandomFloat() * 10f;
                }
                drawVerts2[i] = drawVerts1[i];
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.NormalizeTangents(drawVerts1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.NormalizeTangents()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.NormalizeTangents(drawVerts2, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++)
            {
                if (!drawVerts1[i].normal.Compare(drawVerts2[i].normal, 1e-2f)) break;
                if (!drawVerts1[i].tangents0.Compare(drawVerts2[i].tangents0, 1e-2f)) break;
                if (!drawVerts1[i].tangents1.Compare(drawVerts2[i].tangents1, 1e-2f)) break;
                // since we're doing a lot of unaligned work, added this check to make sure xyz wasn't getting overwritten
                if (!drawVerts1[i].xyz.Compare(drawVerts2[i].xyz, 1e-2f)) break;
            }
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.NormalizeTangents() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestGetTextureSpaceLightVectors()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var drawVerts = stackalloc DrawVert[COUNT];
            var indexes = stackalloc int[COUNT * 3];
            var lightVectors1 = stackalloc Vector3[COUNT];
            var lightVectors2 = stackalloc Vector3[COUNT];
            Vector3 lightOrigin;
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
                for (j = 0; j < 3; j++)
                {
                    drawVerts[i].xyz[j] = srnd.CRandomFloat() * 100f;
                    drawVerts[i].normal[j] = srnd.CRandomFloat();
                    drawVerts[i].tangents0[j] = srnd.CRandomFloat();
                    drawVerts[i].tangents1[j] = srnd.CRandomFloat();
                }

            for (i = 0; i < COUNT; i++)
            {
                indexes[i * 3 + 0] = (i + 0) % COUNT;
                indexes[i * 3 + 1] = (i + 1) % COUNT;
                indexes[i * 3 + 2] = (i + 2) % COUNT;
            }

            lightOrigin.x = srnd.CRandomFloat() * 100f;
            lightOrigin.y = srnd.CRandomFloat() * 100f;
            lightOrigin.z = srnd.CRandomFloat() * 100f;

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.CreateTextureSpaceLightVectors(lightVectors1, lightOrigin, drawVerts, COUNT, indexes, COUNT * 3);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CreateTextureSpaceLightVectors()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.CreateTextureSpaceLightVectors(lightVectors2, lightOrigin, drawVerts, COUNT, indexes, COUNT * 3);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (!lightVectors1[i].Compare(lightVectors2[i], 1e-4f)) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CreateTextureSpaceLightVectors() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestGetSpecularTextureCoords()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var drawVerts = stackalloc DrawVert[COUNT];
            var texCoords1 = stackalloc Vector4[COUNT];
            var texCoords2 = stackalloc Vector4[COUNT];
            var indexes = stackalloc int[COUNT * 3];
            Vector3 lightOrigin, viewOrigin;
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
                for (j = 0; j < 3; j++)
                {
                    drawVerts[i].xyz[j] = srnd.CRandomFloat() * 100f;
                    drawVerts[i].normal[j] = srnd.CRandomFloat();
                    drawVerts[i].tangents0[j] = srnd.CRandomFloat();
                    drawVerts[i].tangents1[j] = srnd.CRandomFloat();
                }

            for (i = 0; i < COUNT; i++)
            {
                indexes[i * 3 + 0] = (i + 0) % COUNT;
                indexes[i * 3 + 1] = (i + 1) % COUNT;
                indexes[i * 3 + 2] = (i + 2) % COUNT;
            }

            lightOrigin.x = srnd.CRandomFloat() * 100f;
            lightOrigin.y = srnd.CRandomFloat() * 100f;
            lightOrigin.z = srnd.CRandomFloat() * 100f;
            viewOrigin.x = srnd.CRandomFloat() * 100f;
            viewOrigin.y = srnd.CRandomFloat() * 100f;
            viewOrigin.z = srnd.CRandomFloat() * 100f;

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.CreateSpecularTextureCoords(texCoords1, lightOrigin, viewOrigin, drawVerts, COUNT, indexes, COUNT * 3);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CreateSpecularTextureCoords()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.CreateSpecularTextureCoords(texCoords2, lightOrigin, viewOrigin, drawVerts, COUNT, indexes, COUNT * 3);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (!texCoords1[i].Compare(texCoords2[i], 1e-2f)) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CreateSpecularTextureCoords() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestCreateShadowCache()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var drawVerts = stackalloc DrawVert[COUNT];
            var vertexCache1 = stackalloc Vector4[COUNT * 2];
            var vertexCache2 = stackalloc Vector4[COUNT * 2];
            var originalVertRemap = stackalloc int[COUNT];
            var vertRemap1 = stackalloc int[COUNT];
            var vertRemap2 = stackalloc int[COUNT];
            Vector3 lightOrigin;
            int numVerts1 = 0, numVerts2 = 0;
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                drawVerts[i].xyz.x = srnd.CRandomFloat() * 100f;
                drawVerts[i].xyz.y = srnd.CRandomFloat() * 100f;
                drawVerts[i].xyz.z = srnd.CRandomFloat() * 100f;
                originalVertRemap[i] = (srnd.CRandomFloat() > 0f) ? -1 : 0;
            }
            lightOrigin.x = srnd.CRandomFloat() * 100f;
            lightOrigin.y = srnd.CRandomFloat() * 100f;
            lightOrigin.z = srnd.CRandomFloat() * 100f;

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < COUNT; j++) vertRemap1[j] = originalVertRemap[j];
                StartRecordTime(out start);
                numVerts1 = p_generic.CreateShadowCache(vertexCache1, vertRemap1, lightOrigin, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CreateShadowCache()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < COUNT; j++) vertRemap2[j] = originalVertRemap[j];
                StartRecordTime(out start);
                numVerts2 = p_simd.CreateShadowCache(vertexCache2, vertRemap2, lightOrigin, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++)
            {
                if (i < (numVerts1 / 2))
                {
                    if (!vertexCache1[i * 2 + 0].Compare(vertexCache2[i * 2 + 0], 1e-2f)) break;
                    if (!vertexCache1[i * 2 + 1].Compare(vertexCache2[i * 2 + 1], 1e-2f)) break;
                }
                if (vertRemap1[i] != vertRemap2[i]) break;
            }

            result = i >= COUNT && numVerts1 == numVerts2 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CreateShadowCache() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_generic.CreateVertexProgramShadowCache(vertexCache1, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.CreateVertexProgramShadowCache()", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                p_simd.CreateVertexProgramShadowCache(vertexCache2, drawVerts, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++)
            {
                if (!vertexCache1[i * 2 + 0].Compare(vertexCache2[i * 2 + 0], 1e-2f)) break;
                if (!vertexCache1[i * 2 + 1].Compare(vertexCache2[i * 2 + 1], 1e-2f)) break;
            }
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.CreateVertexProgramShadowCache() {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        const float SOUND_UPSAMPLE_EPSILON = 1f;
        static void TestSoundUpSampling()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var pcm = stackalloc short[Simd.MIXBUFFER_SAMPLES * 2];
            var ogg0 = stackalloc float[Simd.MIXBUFFER_SAMPLES * 2];
            var ogg1 = stackalloc float[Simd.MIXBUFFER_SAMPLES * 2];
            var samples1 = stackalloc float[Simd.MIXBUFFER_SAMPLES * 2];
            var samples2 = stackalloc float[Simd.MIXBUFFER_SAMPLES * 2];
            var ogg = stackalloc float*[2];
            int kHz, numSpeakers;
            string result;

            var srnd = new RandomX(RANDOM_SEED);

            for (i = 0; i < Simd.MIXBUFFER_SAMPLES * 2; i++)
            {
                pcm[i] = (short)(srnd.RandomInt(1 << 16) - (1 << 15));
                ogg0[i] = srnd.RandomFloat();
                ogg1[i] = srnd.RandomFloat();
            }

            ogg[0] = ogg0;
            ogg[1] = ogg1;

            for (numSpeakers = 1; numSpeakers <= 2; numSpeakers++)
            {
                for (kHz = 11025; kHz <= 44100; kHz *= 2)
                {
                    bestClocksGeneric = default;
                    for (i = 0; i < NUMTESTS; i++)
                    {
                        StartRecordTime(out start);
                        p_generic.UpSamplePCMTo44kHz(samples1, pcm, Simd.MIXBUFFER_SAMPLES * numSpeakers * kHz / 44100, kHz, numSpeakers);
                        StopRecordTime(out end);
                        GetBest(start, end, ref bestClocksGeneric);
                    }
                    PrintClocks($"generic.UpSamplePCMTo44kHz({kHz}, {numSpeakers})", Simd.MIXBUFFER_SAMPLES * numSpeakers * kHz / 44100, bestClocksGeneric);

                    bestClocksSIMD = default;
                    for (i = 0; i < NUMTESTS; i++)
                    {
                        StartRecordTime(out start);
                        p_simd.UpSamplePCMTo44kHz(samples2, pcm, Simd.MIXBUFFER_SAMPLES * numSpeakers * kHz / 44100, kHz, numSpeakers);
                        StopRecordTime(out end);
                        GetBest(start, end, ref bestClocksSIMD);
                    }

                    for (i = 0; i < Simd.MIXBUFFER_SAMPLES * numSpeakers; i++) if (MathX.Fabs(samples1[i] - samples2[i]) > SOUND_UPSAMPLE_EPSILON) break;
                    result = i >= Simd.MIXBUFFER_SAMPLES * numSpeakers ? "ok" : $"{S_COLOR_RED}X";
                    PrintClocks($"   simd.UpSamplePCMTo44kHz({kHz}, {numSpeakers}) {result}", Simd.MIXBUFFER_SAMPLES * numSpeakers * kHz / 44100, bestClocksSIMD, bestClocksGeneric);
                }
            }

            for (numSpeakers = 1; numSpeakers <= 2; numSpeakers++)
                for (kHz = 11025; kHz <= 44100; kHz *= 2)
                {
                    bestClocksGeneric = default;
                    for (i = 0; i < NUMTESTS; i++)
                    {
                        StartRecordTime(out start);
                        p_generic.UpSampleOGGTo44kHz(samples1, ogg, Simd.MIXBUFFER_SAMPLES * numSpeakers * kHz / 44100, kHz, numSpeakers);
                        StopRecordTime(out end);
                        GetBest(start, end, ref bestClocksGeneric);
                    }
                    PrintClocks($"generic.UpSampleOGGTo44kHz({kHz}, {numSpeakers})", Simd.MIXBUFFER_SAMPLES * numSpeakers * kHz / 44100, bestClocksGeneric);

                    bestClocksSIMD = default;
                    for (i = 0; i < NUMTESTS; i++)
                    {
                        StartRecordTime(out start);
                        p_simd.UpSampleOGGTo44kHz(samples2, ogg, Simd.MIXBUFFER_SAMPLES * numSpeakers * kHz / 44100, kHz, numSpeakers);
                        StopRecordTime(out end);
                        GetBest(start, end, ref bestClocksSIMD);
                    }

                    for (i = 0; i < Simd.MIXBUFFER_SAMPLES * numSpeakers; i++) if (MathX.Fabs(samples1[i] - samples2[i]) > SOUND_UPSAMPLE_EPSILON) break;
                    result = i >= Simd.MIXBUFFER_SAMPLES ? "ok" : $"{S_COLOR_RED}X";
                    PrintClocks($"   simd.UpSampleOGGTo44kHz({kHz}, {numSpeakers}) {result}", Simd.MIXBUFFER_SAMPLES * numSpeakers * kHz / 44100, bestClocksSIMD, bestClocksGeneric);
                }
        }

        const float SOUND_MIX_EPSILON = 2f;
        static void TestSoundMixing()
        {
            int i, j;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var origMixBuffer = stackalloc float[Simd.MIXBUFFER_SAMPLES * 6];
            var mixBuffer1 = stackalloc float[Simd.MIXBUFFER_SAMPLES * 6];
            var mixBuffer2 = stackalloc float[Simd.MIXBUFFER_SAMPLES * 6];
            var samples = stackalloc float[Simd.MIXBUFFER_SAMPLES * 6];
            var outSamples1 = stackalloc short[Simd.MIXBUFFER_SAMPLES * 6];
            var outSamples2 = stackalloc short[Simd.MIXBUFFER_SAMPLES * 6];
            var lastV = new float[6];
            var currentV = new float[6];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < 6; i++)
            {
                lastV[i] = srnd.CRandomFloat();
                currentV[i] = srnd.CRandomFloat();
            }

            for (i = 0; i < Simd.MIXBUFFER_SAMPLES * 6; i++)
            {
                origMixBuffer[i] = srnd.CRandomFloat();
                samples[i] = srnd.RandomInt((1 << 16)) - (1 << 15);
            }

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer1[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_generic.MixSoundTwoSpeakerMono(mixBuffer1, samples, Simd.MIXBUFFER_SAMPLES, lastV, currentV);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MixSoundTwoSpeakerMono()", Simd.MIXBUFFER_SAMPLES, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer2[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_simd.MixSoundTwoSpeakerMono(mixBuffer2, samples, Simd.MIXBUFFER_SAMPLES, lastV, currentV);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < Simd.MIXBUFFER_SAMPLES * 6; i++) if (MathX.Fabs(mixBuffer1[i] - mixBuffer2[i]) > SOUND_MIX_EPSILON) break;
            result = i >= Simd.MIXBUFFER_SAMPLES * 6 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MixSoundTwoSpeakerMono() {result}", Simd.MIXBUFFER_SAMPLES, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer1[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_generic.MixSoundTwoSpeakerStereo(mixBuffer1, samples, Simd.MIXBUFFER_SAMPLES, lastV, currentV);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MixSoundTwoSpeakerStereo()", Simd.MIXBUFFER_SAMPLES, bestClocksGeneric);


            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer2[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_simd.MixSoundTwoSpeakerStereo(mixBuffer2, samples, Simd.MIXBUFFER_SAMPLES, lastV, currentV);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < Simd.MIXBUFFER_SAMPLES * 6; i++) if (MathX.Fabs(mixBuffer1[i] - mixBuffer2[i]) > SOUND_MIX_EPSILON) break;
            result = i >= Simd.MIXBUFFER_SAMPLES * 6 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MixSoundTwoSpeakerStereo() {result}", Simd.MIXBUFFER_SAMPLES, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer1[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_generic.MixSoundSixSpeakerMono(mixBuffer1, samples, Simd.MIXBUFFER_SAMPLES, lastV, currentV);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MixSoundSixSpeakerMono()", Simd.MIXBUFFER_SAMPLES, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer2[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_simd.MixSoundSixSpeakerMono(mixBuffer2, samples, Simd.MIXBUFFER_SAMPLES, lastV, currentV);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < Simd.MIXBUFFER_SAMPLES * 6; i++) if (MathX.Fabs(mixBuffer1[i] - mixBuffer2[i]) > SOUND_MIX_EPSILON) break;
            result = i >= Simd.MIXBUFFER_SAMPLES * 6 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MixSoundSixSpeakerMono() {result}", Simd.MIXBUFFER_SAMPLES, bestClocksSIMD, bestClocksGeneric);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer1[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_generic.MixSoundSixSpeakerStereo(mixBuffer1, samples, Simd.MIXBUFFER_SAMPLES, lastV, currentV);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MixSoundSixSpeakerStereo()", Simd.MIXBUFFER_SAMPLES, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer2[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_simd.MixSoundSixSpeakerStereo(mixBuffer2, samples, Simd.MIXBUFFER_SAMPLES, lastV, currentV);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < Simd.MIXBUFFER_SAMPLES * 6; i++) if (MathX.Fabs(mixBuffer1[i] - mixBuffer2[i]) > SOUND_MIX_EPSILON) break;
            result = i >= Simd.MIXBUFFER_SAMPLES * 6 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MixSoundSixSpeakerStereo() {result}", Simd.MIXBUFFER_SAMPLES, bestClocksSIMD, bestClocksGeneric);

            for (i = 0; i < Simd.MIXBUFFER_SAMPLES * 6; i++) origMixBuffer[i] = srnd.RandomInt((1 << 17)) - (1 << 16);

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer1[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_generic.MixedSoundToSamples(outSamples1, mixBuffer1, Simd.MIXBUFFER_SAMPLES * 6);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.MixedSoundToSamples()", Simd.MIXBUFFER_SAMPLES, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                for (j = 0; j < Simd.MIXBUFFER_SAMPLES * 6; j++) mixBuffer2[j] = origMixBuffer[j];
                StartRecordTime(out start);
                p_simd.MixedSoundToSamples(outSamples2, mixBuffer2, Simd.MIXBUFFER_SAMPLES * 6);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < Simd.MIXBUFFER_SAMPLES * 6; i++) if (outSamples1[i] != outSamples2[i]) break;
            result = i >= Simd.MIXBUFFER_SAMPLES * 6 ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.MixedSoundToSamples() {result}", Simd.MIXBUFFER_SAMPLES, bestClocksSIMD, bestClocksGeneric);
        }

        static void TestMath()
        {
            int i;
            DateTime start, end; TimeSpan bestClocks;

            Printf("====================================\n");

            var tst = -1f;
            var tst2 = 1f;
            var testvar = 1f;
            var rnd = new RandomX();

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = Math.Abs(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("          fabs(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                var tmp = *(int*)&tst;
                tmp &= 0x7FFFFFFF;
                tst = *(float*)&tmp;
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("    MathX.Fabs(tst)", 1, bestClocks);

            bestClocks = default;
            tst = 10f + 100f * rnd.RandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = (float)Math.Sqrt(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst * 0.01f;
                tst = 10f + 100f * rnd.RandomFloat();
            }
            PrintClocks("          sqrt(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.RandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Sqrt(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.RandomFloat();
            }
            PrintClocks("    MathX.Sqrt(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.RandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Sqrt16(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.RandomFloat();
            }
            PrintClocks("  MathX.Sqrt16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.RandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = (float)MathX.Sqrt64(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.RandomFloat();
            }
            PrintClocks("  MathX.Sqrt64(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.RandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst *= MathX.RSqrt(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.RandomFloat();
            }
            PrintClocks("   MathX.RSqrt(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Sin(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("     MathX.Sin(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Sin16(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("   MathX.Sin16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Cos(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("     MathX.Cos(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Cos16(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("   MathX.Cos16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                MathX.SinCos(tst, out tst, out tst2);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("  MathX.SinCos(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                MathX.SinCos16(tst, out tst, out tst2);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("MathX.SinCos16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Tan(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("     MathX.Tan(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Tan16(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("   MathX.Tan16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.ASin(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst * (1f / MathX.PI);
                tst = rnd.CRandomFloat();
            }
            PrintClocks("    MathX.ASin(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.ASin16(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst * (1f / MathX.PI);
                tst = rnd.CRandomFloat();
            }
            PrintClocks("  MathX.ASin16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.ACos(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst * (1f / MathX.PI);
                tst = rnd.CRandomFloat();
            }
            PrintClocks("    MathX.ACos(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.ACos16(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst * (1f / MathX.PI);
                tst = rnd.CRandomFloat();
            }
            PrintClocks("  MathX.ACos16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.ATan(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("    MathX.ATan(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.ATan16(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("  MathX.ATan16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Pow(2.7f, tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst * 0.1f;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("     MathX.Pow(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Pow16(2.7f, tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst * 0.1f;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("   MathX.Pow16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Exp(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst * 0.1f;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("     MathX.Exp(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                tst = MathX.Exp16(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst * 0.1f;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("   MathX.Exp16(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                tst = (float)Math.Abs(tst) + 1f;
                StartRecordTime(out start);
                tst = MathX.Log(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("     MathX.Log(tst)", 1, bestClocks);

            bestClocks = default;
            tst = rnd.CRandomFloat();
            for (i = 0; i < NUMTESTS; i++)
            {
                tst = (float)Math.Abs(tst) + 1f;
                StartRecordTime(out start);
                tst = MathX.Log16(tst);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
                testvar = (testvar + tst) * tst;
                tst = rnd.CRandomFloat();
            }
            PrintClocks("   MathX.Log16(tst)", 1, bestClocks);

            Printf($"testvar = {testvar}\n");

            Matrix3x3 resultMat3;
            Quat fromQuat, toQuat, resultQuat = new();
            CQuat cq;
            Angles ang;

            fromQuat = new Angles(30, 45, 0).ToQuat();
            toQuat = new Angles(45, 0, 0).ToQuat();
            cq = new Angles(30, 45, 0).ToQuat().ToCQuat();
            ang = new Angles(30, 40, 50);

            bestClocks = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                resultMat3 = fromQuat.ToMat3();
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
            }
            PrintClocks("      Quat.ToMat3()", 1, bestClocks);

            bestClocks = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                resultQuat.Slerp(fromQuat, toQuat, 0.3f);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
            }
            PrintClocks("       Quat.Slerp()", 1, bestClocks);

            bestClocks = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                resultQuat = cq.ToQuat();
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
            }
            PrintClocks("     CQuat.ToQuat()", 1, bestClocks);

            bestClocks = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                resultQuat = ang.ToQuat();
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
            }
            PrintClocks("    Angles.ToQuat()", 1, bestClocks);

            bestClocks = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                StartRecordTime(out start);
                resultMat3 = ang.ToMat3();
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocks);
            }
            PrintClocks("    Angles.ToMat3()", 1, bestClocks);
        }

        static void TestNegate()
        {
            int i;
            DateTime start, end; TimeSpan bestClocksGeneric, bestClocksSIMD;
            var fsrc0 = stackalloc float[COUNT];
            var fsrc1 = stackalloc float[COUNT];
            var fsrc2 = stackalloc float[COUNT];
            string result;

            var srnd = new RandomX(RANDOM_SEED);
            for (i = 0; i < COUNT; i++)
            {
                fsrc0[i] = fsrc1[i] = fsrc2[i] = srnd.CRandomFloat() * 10f;
                //fsrc1[i] = srnd.CRandomFloat() * 10f;
            }

            Printf("====================================\n");

            bestClocksGeneric = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.CopyBlock(&fsrc1[0], &fsrc0[0], COUNT * sizeof(float));
                StartRecordTime(out start);
                p_generic.Negate16(fsrc1, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksGeneric);
            }
            PrintClocks("generic.Negate16( float[] )", COUNT, bestClocksGeneric);

            bestClocksSIMD = default;
            for (i = 0; i < NUMTESTS; i++)
            {
                Unsafe.CopyBlock(&fsrc2[0], &fsrc0[0], COUNT * sizeof(float));
                StartRecordTime(out start);
                p_simd.Negate16(fsrc2, COUNT);
                StopRecordTime(out end);
                GetBest(start, end, ref bestClocksSIMD);
            }

            for (i = 0; i < COUNT; i++) if (fsrc1[i] != fsrc2[i]) break;
            result = i >= COUNT ? "ok" : $"{S_COLOR_RED}X";
            PrintClocks($"   simd.Negate16(float[]) {result}", COUNT, bestClocksSIMD, bestClocksGeneric);
        }

        internal static void Test_f(CmdArgs args)
        {
#if _WIN32
            SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);
#endif
#if true
            if (!string.IsNullOrEmpty(args[1]))
            {
                var cpuid = GetProcessorId();
                var argString = args.Args().Replace(" ", "");

                if (string.Equals(argString, "MMX", StringComparison.OrdinalIgnoreCase))
                {
                    if ((cpuid & CPUID.MMX) == 0) { Printf("CPU does not support MMX\n"); return; }
                    SimdMMX.Activate();
                }
                else if (string.Equals(argString, "3DNow", StringComparison.OrdinalIgnoreCase))
                {
                    if ((cpuid & CPUID.MMX) == 0 || (cpuid & CPUID._3DNOW) == 0) { Printf("CPU does not support MMX & 3DNow\n"); return; }
                    Simd3DNow.Activate();
                }
                else if (string.Equals(argString, "SSE", StringComparison.OrdinalIgnoreCase))
                {
                    if ((cpuid & CPUID.MMX) == 0 || (cpuid & CPUID.SSE) == 0) { Printf("CPU does not support MMX & SSE\n"); return; }
                    SimdSSE.Activate();
                }
                else if (string.Equals(argString, "SSE2", StringComparison.OrdinalIgnoreCase))
                {
                    if ((cpuid & CPUID.MMX) == 0 || (cpuid & CPUID.SSE) == 0 || (cpuid & CPUID.SSE2) == 0) { Printf("CPU does not support MMX & SSE & SSE2\n"); return; }
                    SimdSSE2.Activate();
                }
                else if (string.Equals(argString, "SSE3", StringComparison.OrdinalIgnoreCase))
                {
                    if ((cpuid & CPUID.MMX) == 0 || (cpuid & CPUID.SSE) == 0 || (cpuid & CPUID.SSE2) == 0 || (cpuid & CPUID.SSE3) == 0) { Printf("CPU does not support MMX & SSE & SSE2 & SSE3\n"); return; }
                    SimdSSE3.Activate();
                }
                else if (string.Equals(argString, "AltiVec", StringComparison.OrdinalIgnoreCase))
                {
                    if ((cpuid & CPUID.ALTIVEC) == 0) { Printf("CPU does not support AltiVec\n"); return; }
                    SimdAltiVec.Activate();
                }
                else { Printf("invalid argument, use: MMX, 3DNow, SSE, SSE2, SSE3, AltiVec\n"); return; }
            }
#endif

            SetRefreshOnPrint(true);

            Printf($"using {p_simd.Name} for SIMD processing\n");
            GetBaseClocks();

            TestMath();
            TestAdd();
            TestSub();
            TestMul();
            TestDiv();
            TestMulAdd();
            TestMulSub();
            TestDot();
            TestCompare();
            TestMinMax();
            TestClamp();
            TestMemcpy();
            TestMemset();
            TestNegate();

            TestMatXMultiplyVecX();
            TestMatXMultiplyAddVecX();
            TestMatXTransposeMultiplyVecX();
            TestMatXTransposeMultiplyAddVecX();
            TestMatXMultiplyMatX();
            TestMatXTransposeMultiplyMatX();
            TestMatXLowerTriangularSolve();
            TestMatXLowerTriangularSolveTranspose();
            TestMatXLDLTFactor();

            Printf("====================================\n");

            TestBlendJoints();
            TestConvertJointQuatsToJointMats();
            TestConvertJointMatsToJointQuats();
            TestTransformJoints();
            TestUntransformJoints();
            TestTransformVerts();
            TestTracePointCull();
            TestDecalPointCull();
            TestOverlayPointCull();
            TestDeriveTriPlanes();
            TestDeriveTangents();
            TestDeriveUnsmoothedTangents();
            TestNormalizeTangents();
            TestGetTextureSpaceLightVectors();
            TestGetSpecularTextureCoords();
            TestCreateShadowCache();

            Printf("====================================\n");

            TestSoundUpSampling();
            TestSoundMixing();

            SetRefreshOnPrint(false);

#if _WIN32
            SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_NORMAL);
#endif
        }
    }
}
