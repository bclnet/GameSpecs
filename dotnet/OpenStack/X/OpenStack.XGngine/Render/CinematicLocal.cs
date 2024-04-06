using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.NumericsX.Jpeg;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using JSAMPARRAY_2 = System.Byte; // ptr to some rows (a 2-D sample array)

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public unsafe class CinematicLocal : Cinematic
    {
        const int CIN_system = 1;
        const int CIN_loop = 2;
        const int CIN_hold = 4;
        const int CIN_silent = 8;
        const int CIN_shader = 16;

        nint[] mcomp = new nint[256];
        byte*[] qStatus0;
        byte*[] qStatus1;
        string fileName;
        int CIN_WIDTH, CIN_HEIGHT;
        VFile iFile;
        CinStatus status;
        int tfps;
        int RoQPlayed;
        int ROQSize;
        int RoQFrameSize;
        int onQuad;
        int numQuads;
        int samplesPerLine;
        int roq_id;
        int screenDelta;
        byte* buf;
        int samplesPerPixel;                // defaults to 2
        uint xsize, ysize, maxsize, minsize;
        int normalBuffer0;
        int roq_flags;
        int roqF0;
        int roqF1;
        int t0;
        int t1;
        int roqFPS;
        int drawX, drawY;

        int animationLength;
        int startTime;
        float frameRate;

        byte* image;

        bool looping;
        bool dirty;
        bool half;
        bool smootheddouble;
        bool inMemory;

        public CinematicLocal()
        {
            image = null;
            status = CinStatus.FMV_EOF;
            buf = null;
            iFile = null;

            qStatus0 = new byte*[32768];
            qStatus1 = new byte*[32768];
        }

        public override void Dispose()
        {
            Close();

            qStatus0 = null;
            qStatus1 = null;
        }

        public override bool InitFromFile(string qpath, bool looping)
        {
            ushort RoQID;

            Close();

            inMemory = false;
            animationLength = 100000;

            fileName = !qpath.Contains('/') && !qpath.Contains('\\')
                ? $"video/{qpath}"
                : qpath;
            iFile = fileSystem.OpenFileRead(fileName);
            if (iFile == null) return false;
            ROQSize = iFile.Length;

            this.looping = looping;

            CIN_HEIGHT = DEFAULT_CIN_HEIGHT;
            CIN_WIDTH = DEFAULT_CIN_WIDTH;
            samplesPerPixel = 4;
            startTime = 0;
            buf = null;

            iFile.Read(file, 16);
            RoQID = (ushort)(file[0] + (file[1] * 256));

            frameRate = file[6];
            if (frameRate == 32f) frameRate = 1000f / 32f;

            if (RoQID == ROQ_FILE)
            {
                RoQ_init();
                status = CinStatus.FMV_PLAY;
                ImageForTime(0);
                status = looping ? CinStatus.FMV_PLAY : CinStatus.FMV_IDLE;
                return true;
            }

            RoQShutdown();
            return false;
        }

        public override CinData ImageForTime(int milliseconds)
        {
            CinData cinData = default;

            if (milliseconds < 0) milliseconds = 0;
            if (r_skipROQ.Bool || status == CinStatus.FMV_EOF || status == CinStatus.FMV_IDLE) return cinData;

            if (buf == null || startTime == -1)
            {
                if (startTime == -1) RoQReset();
                startTime = milliseconds;
            }

            tfps = (int)((milliseconds - startTime) * frameRate / 1000);
            if (tfps < 0) tfps = 0;
            if (tfps < numQuads) { RoQReset(); buf = null; status = CinStatus.FMV_PLAY; }

            if (buf == null) while (buf == null) RoQInterrupt();
            else while (tfps != numQuads && status == CinStatus.FMV_PLAY) RoQInterrupt();

            if (status == CinStatus.FMV_LOOPED)
            {
                status = CinStatus.FMV_PLAY;
                while (buf == null && status == CinStatus.FMV_PLAY) RoQInterrupt();
                startTime = milliseconds;
            }

            if (status == CinStatus.FMV_EOF)
                if (looping)
                {
                    RoQReset();
                    buf = null;
                    if (status == CinStatus.FMV_LOOPED) status = CinStatus.FMV_PLAY;
                    while (buf == null && status == CinStatus.FMV_PLAY) RoQInterrupt();
                    startTime = milliseconds;
                }
                else { status = CinStatus.FMV_IDLE; RoQShutdown(); }

            cinData.imageWidth = CIN_WIDTH;
            cinData.imageHeight = CIN_HEIGHT;
            cinData.status = (int)status;
            cinData.image = buf;

            return cinData;
        }

        public override int AnimationLength
            => animationLength;

        public override void Close()
        {
            if (image != null)
            {
                Marshal.FreeHGlobal((IntPtr)image);
                image = null;
                buf = null;
                status = CinStatus.FMV_EOF;
            }
            RoQShutdown();
        }

        public override void ResetTime(int time)
        {
            startTime = -1; //: backEnd.viewDef ? 1000 * backEnd.viewDef.floatTime : -1;
            status = CinStatus.FMV_PLAY;
        }

        void RoQ_init()
        {
            RoQPlayed = 24;

            // get frame rate
            roqFPS = file[6] + file[7] * 256;

            if (roqFPS == 0) roqFPS = 30;

            numQuads = -1;

            roq_id = file[8] + file[9] * 256;
            RoQFrameSize = file[10] + file[11] * 256 + file[12] * 65536;
            roq_flags = file[14] + file[15] * 256;
        }

        void blitVQQuad32fs(byte*[] status, byte* data)
        {
            ushort newd, celdata, code; uint index, i;

            newd = 0; celdata = 0; index = 0;

            do
            {
                if (newd == 0) { newd = 7; celdata = (ushort)(data[0] + data[1] * 256); data += 2; }
                else newd--;

                code = (ushort)(celdata & 0xc000);
                celdata <<= 2;

                switch (code)
                {
                    case 0x8000: blit8_32((byte*)&vq8[(*data) * 128], status[index], samplesPerLine); data++; index += 5; break; // vq code
                    case 0xc000: // drop
                        index++; // skip 8x8
                        for (i = 0; i < 4; i++)
                        {
                            if (newd == 0) { newd = 7; celdata = (ushort)(data[0] + data[1] * 256); data += 2; }
                            else newd--;

                            code = (ushort)(celdata & 0xc000);
                            celdata <<= 2;

                            // code in top two bits of code
                            switch (code)
                            {
                                case 0x8000: blit4_32((byte*)&vq4[(*data) * 32], status[index], samplesPerLine); data++; break; // 4x4 vq code
                                case 0xc000: // 2x2 vq code
                                    blit2_32((byte*)&vq2[(*data) * 8], status[index], samplesPerLine); data++;
                                    blit2_32((byte*)&vq2[(*data) * 8], status[index] + 8, samplesPerLine); data++;
                                    blit2_32((byte*)&vq2[(*data) * 8], status[index] + samplesPerLine * 2, samplesPerLine); data++;
                                    blit2_32((byte*)&vq2[(*data) * 8], status[index] + samplesPerLine * 2 + 8, samplesPerLine); data++; break;

                                case 0x4000: move4_32(status[index] + mcomp[(*data)], status[index], samplesPerLine); data++; break; // motion compensation
                            }
                            index++;
                        }
                        break;
                    case 0x4000: move8_32(status[index] + mcomp[*data], status[index], samplesPerLine); data++; index += 5; break; // motion compensation
                    case 0x0000: index += 5; break;
                }

            } while (status[index] != null);
        }

        void RoQShutdown()
        {
            if (status == CinStatus.FMV_IDLE) return;
            status = CinStatus.FMV_IDLE;

            if (iFile != null) { fileSystem.CloseFile(iFile); iFile = null; }

            fileName = "";
        }

        void RoQInterrupt()
        {
            byte* framedata;

            iFile.Read(file, RoQFrameSize + 8);
            if (RoQPlayed >= ROQSize)
            {
                if (looping) RoQReset();
                else status = CinStatus.FMV_EOF;
                return;
            }

            framedata = file;

        // new frame is ready
        redump:
            switch (roq_id)
            {
                case ROQ_QUAD_VQ:
                    if ((numQuads & 1) != 0) { normalBuffer0 = t1; RoQPrepMcomp(roqF0, roqF1); blitVQQuad32fs(qStatus1, framedata); buf = image + screenDelta; }
                    else { normalBuffer0 = t0; RoQPrepMcomp(roqF0, roqF1); blitVQQuad32fs(qStatus0, framedata); buf = image; }
                    // first frame
                    if (numQuads == 0) Unsafe.CopyBlock(image + screenDelta, image, (uint)(samplesPerLine * ysize));
                    numQuads++;
                    dirty = true;
                    break;
                case ROQ_CODEBOOK: decodeCodeBook(framedata, (ushort)roq_flags); break;
                case ZA_SOUND_MONO: break;
                case ZA_SOUND_STEREO: break;
                case ROQ_QUAD_INFO: if (numQuads == -1) { readQuadInfo(framedata); setupQuad(0, 0); } if (numQuads != 1) numQuads = 0; break;
                case ROQ_PACKET: inMemory = roq_flags != 0; RoQFrameSize = 0; break;         // for header
                case ROQ_QUAD_HANG: RoQFrameSize = 0; break;
                case ROQ_QUAD_JPEG: if (numQuads == 0) { normalBuffer0 = t0; JPEGBlit(image, framedata, RoQFrameSize); Unsafe.CopyBlock(image + screenDelta, image, (uint)(samplesPerLine * ysize)); numQuads++; } break;
                default: status = CinStatus.FMV_EOF; break;
            }

            // read in next frame data
            if (RoQPlayed >= ROQSize)
            {
                if (looping) RoQReset();
                else status = CinStatus.FMV_EOF;
                return;
            }

            framedata += RoQFrameSize;
            roq_id = framedata[0] + framedata[1] * 256;
            RoQFrameSize = framedata[2] + framedata[3] * 256 + framedata[4] * 65536;
            roq_flags = framedata[6] + framedata[7] * 256;
            roqF0 = framedata[7];
            roqF1 = framedata[6];

            if (RoQFrameSize > 65536 || roq_id == 0x1084) { common.DPrintf("roq_size>65536||roq_id==0x1084\n"); status = CinStatus.FMV_EOF; if (looping) RoQReset(); return; }
            if (inMemory && status != CinStatus.FMV_EOF) { inMemory = false; framedata += 8; goto redump; }

            // one more frame hits the dust
            //
            //	assert(RoQFrameSize <= 65536);
            //	r = Sys_StreamedRead( file, RoQFrameSize+8, 1, iFile );
            RoQPlayed += RoQFrameSize + 8;
        }

        static void move8_32(byte* src, byte* dst, int spl)
        {
#if true
            int* dsrc, ddst; int dspl;

            dsrc = (int*)src;
            ddst = (int*)dst;
            dspl = spl >> 2;

            ddst[0 * dspl + 0] = dsrc[0 * dspl + 0];
            ddst[0 * dspl + 1] = dsrc[0 * dspl + 1];
            ddst[0 * dspl + 2] = dsrc[0 * dspl + 2];
            ddst[0 * dspl + 3] = dsrc[0 * dspl + 3];
            ddst[0 * dspl + 4] = dsrc[0 * dspl + 4];
            ddst[0 * dspl + 5] = dsrc[0 * dspl + 5];
            ddst[0 * dspl + 6] = dsrc[0 * dspl + 6];
            ddst[0 * dspl + 7] = dsrc[0 * dspl + 7];

            ddst[1 * dspl + 0] = dsrc[1 * dspl + 0];
            ddst[1 * dspl + 1] = dsrc[1 * dspl + 1];
            ddst[1 * dspl + 2] = dsrc[1 * dspl + 2];
            ddst[1 * dspl + 3] = dsrc[1 * dspl + 3];
            ddst[1 * dspl + 4] = dsrc[1 * dspl + 4];
            ddst[1 * dspl + 5] = dsrc[1 * dspl + 5];
            ddst[1 * dspl + 6] = dsrc[1 * dspl + 6];
            ddst[1 * dspl + 7] = dsrc[1 * dspl + 7];

            ddst[2 * dspl + 0] = dsrc[2 * dspl + 0];
            ddst[2 * dspl + 1] = dsrc[2 * dspl + 1];
            ddst[2 * dspl + 2] = dsrc[2 * dspl + 2];
            ddst[2 * dspl + 3] = dsrc[2 * dspl + 3];
            ddst[2 * dspl + 4] = dsrc[2 * dspl + 4];
            ddst[2 * dspl + 5] = dsrc[2 * dspl + 5];
            ddst[2 * dspl + 6] = dsrc[2 * dspl + 6];
            ddst[2 * dspl + 7] = dsrc[2 * dspl + 7];

            ddst[3 * dspl + 0] = dsrc[3 * dspl + 0];
            ddst[3 * dspl + 1] = dsrc[3 * dspl + 1];
            ddst[3 * dspl + 2] = dsrc[3 * dspl + 2];
            ddst[3 * dspl + 3] = dsrc[3 * dspl + 3];
            ddst[3 * dspl + 4] = dsrc[3 * dspl + 4];
            ddst[3 * dspl + 5] = dsrc[3 * dspl + 5];
            ddst[3 * dspl + 6] = dsrc[3 * dspl + 6];
            ddst[3 * dspl + 7] = dsrc[3 * dspl + 7];

            ddst[4 * dspl + 0] = dsrc[4 * dspl + 0];
            ddst[4 * dspl + 1] = dsrc[4 * dspl + 1];
            ddst[4 * dspl + 2] = dsrc[4 * dspl + 2];
            ddst[4 * dspl + 3] = dsrc[4 * dspl + 3];
            ddst[4 * dspl + 4] = dsrc[4 * dspl + 4];
            ddst[4 * dspl + 5] = dsrc[4 * dspl + 5];
            ddst[4 * dspl + 6] = dsrc[4 * dspl + 6];
            ddst[4 * dspl + 7] = dsrc[4 * dspl + 7];

            ddst[5 * dspl + 0] = dsrc[5 * dspl + 0];
            ddst[5 * dspl + 1] = dsrc[5 * dspl + 1];
            ddst[5 * dspl + 2] = dsrc[5 * dspl + 2];
            ddst[5 * dspl + 3] = dsrc[5 * dspl + 3];
            ddst[5 * dspl + 4] = dsrc[5 * dspl + 4];
            ddst[5 * dspl + 5] = dsrc[5 * dspl + 5];
            ddst[5 * dspl + 6] = dsrc[5 * dspl + 6];
            ddst[5 * dspl + 7] = dsrc[5 * dspl + 7];

            ddst[6 * dspl + 0] = dsrc[6 * dspl + 0];
            ddst[6 * dspl + 1] = dsrc[6 * dspl + 1];
            ddst[6 * dspl + 2] = dsrc[6 * dspl + 2];
            ddst[6 * dspl + 3] = dsrc[6 * dspl + 3];
            ddst[6 * dspl + 4] = dsrc[6 * dspl + 4];
            ddst[6 * dspl + 5] = dsrc[6 * dspl + 5];
            ddst[6 * dspl + 6] = dsrc[6 * dspl + 6];
            ddst[6 * dspl + 7] = dsrc[6 * dspl + 7];

            ddst[7 * dspl + 0] = dsrc[7 * dspl + 0];
            ddst[7 * dspl + 1] = dsrc[7 * dspl + 1];
            ddst[7 * dspl + 2] = dsrc[7 * dspl + 2];
            ddst[7 * dspl + 3] = dsrc[7 * dspl + 3];
            ddst[7 * dspl + 4] = dsrc[7 * dspl + 4];
            ddst[7 * dspl + 5] = dsrc[7 * dspl + 5];
            ddst[7 * dspl + 6] = dsrc[7 * dspl + 6];
            ddst[7 * dspl + 7] = dsrc[7 * dspl + 7];
#else
            double* dsrc, ddst; int dspl;

            dsrc = (double*)src;
            ddst = (double*)dst;
            dspl = spl >> 3;

            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
#endif
        }

        static void move4_32(byte* src, byte* dst, int spl)
        {
#if true
            int* dsrc, ddst; int dspl;

            dsrc = (int*)src;
            ddst = (int*)dst;
            dspl = spl >> 2;

            ddst[0 * dspl + 0] = dsrc[0 * dspl + 0];
            ddst[0 * dspl + 1] = dsrc[0 * dspl + 1];
            ddst[0 * dspl + 2] = dsrc[0 * dspl + 2];
            ddst[0 * dspl + 3] = dsrc[0 * dspl + 3];

            ddst[1 * dspl + 0] = dsrc[1 * dspl + 0];
            ddst[1 * dspl + 1] = dsrc[1 * dspl + 1];
            ddst[1 * dspl + 2] = dsrc[1 * dspl + 2];
            ddst[1 * dspl + 3] = dsrc[1 * dspl + 3];

            ddst[2 * dspl + 0] = dsrc[2 * dspl + 0];
            ddst[2 * dspl + 1] = dsrc[2 * dspl + 1];
            ddst[2 * dspl + 2] = dsrc[2 * dspl + 2];
            ddst[2 * dspl + 3] = dsrc[2 * dspl + 3];

            ddst[3 * dspl + 0] = dsrc[3 * dspl + 0];
            ddst[3 * dspl + 1] = dsrc[3 * dspl + 1];
            ddst[3 * dspl + 2] = dsrc[3 * dspl + 2];
            ddst[3 * dspl + 3] = dsrc[3 * dspl + 3];
#else
            double* dsrc, ddst; int dspl;

            dsrc = (double*)src;
            ddst = (double*)dst;
            dspl = spl >> 3;

            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            dsrc += dspl;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
#endif
        }

        static void blit8_32(byte* src, byte* dst, int spl)
        {
#if true
            int* dsrc, ddst; int dspl;

            dsrc = (int*)src;
            ddst = (int*)dst;
            dspl = spl >> 2;

            ddst[0 * dspl + 0] = dsrc[0];
            ddst[0 * dspl + 1] = dsrc[1];
            ddst[0 * dspl + 2] = dsrc[2];
            ddst[0 * dspl + 3] = dsrc[3];
            ddst[0 * dspl + 4] = dsrc[4];
            ddst[0 * dspl + 5] = dsrc[5];
            ddst[0 * dspl + 6] = dsrc[6];
            ddst[0 * dspl + 7] = dsrc[7];

            ddst[1 * dspl + 0] = dsrc[8];
            ddst[1 * dspl + 1] = dsrc[9];
            ddst[1 * dspl + 2] = dsrc[10];
            ddst[1 * dspl + 3] = dsrc[11];
            ddst[1 * dspl + 4] = dsrc[12];
            ddst[1 * dspl + 5] = dsrc[13];
            ddst[1 * dspl + 6] = dsrc[14];
            ddst[1 * dspl + 7] = dsrc[15];

            ddst[2 * dspl + 0] = dsrc[16];
            ddst[2 * dspl + 1] = dsrc[17];
            ddst[2 * dspl + 2] = dsrc[18];
            ddst[2 * dspl + 3] = dsrc[19];
            ddst[2 * dspl + 4] = dsrc[20];
            ddst[2 * dspl + 5] = dsrc[21];
            ddst[2 * dspl + 6] = dsrc[22];
            ddst[2 * dspl + 7] = dsrc[23];

            ddst[3 * dspl + 0] = dsrc[24];
            ddst[3 * dspl + 1] = dsrc[25];
            ddst[3 * dspl + 2] = dsrc[26];
            ddst[3 * dspl + 3] = dsrc[27];
            ddst[3 * dspl + 4] = dsrc[28];
            ddst[3 * dspl + 5] = dsrc[29];
            ddst[3 * dspl + 6] = dsrc[30];
            ddst[3 * dspl + 7] = dsrc[31];

            ddst[4 * dspl + 0] = dsrc[32];
            ddst[4 * dspl + 1] = dsrc[33];
            ddst[4 * dspl + 2] = dsrc[34];
            ddst[4 * dspl + 3] = dsrc[35];
            ddst[4 * dspl + 4] = dsrc[36];
            ddst[4 * dspl + 5] = dsrc[37];
            ddst[4 * dspl + 6] = dsrc[38];
            ddst[4 * dspl + 7] = dsrc[39];

            ddst[5 * dspl + 0] = dsrc[40];
            ddst[5 * dspl + 1] = dsrc[41];
            ddst[5 * dspl + 2] = dsrc[42];
            ddst[5 * dspl + 3] = dsrc[43];
            ddst[5 * dspl + 4] = dsrc[44];
            ddst[5 * dspl + 5] = dsrc[45];
            ddst[5 * dspl + 6] = dsrc[46];
            ddst[5 * dspl + 7] = dsrc[47];

            ddst[6 * dspl + 0] = dsrc[48];
            ddst[6 * dspl + 1] = dsrc[49];
            ddst[6 * dspl + 2] = dsrc[50];
            ddst[6 * dspl + 3] = dsrc[51];
            ddst[6 * dspl + 4] = dsrc[52];
            ddst[6 * dspl + 5] = dsrc[53];
            ddst[6 * dspl + 6] = dsrc[54];
            ddst[6 * dspl + 7] = dsrc[55];

            ddst[7 * dspl + 0] = dsrc[56];
            ddst[7 * dspl + 1] = dsrc[57];
            ddst[7 * dspl + 2] = dsrc[58];
            ddst[7 * dspl + 3] = dsrc[59];
            ddst[7 * dspl + 4] = dsrc[60];
            ddst[7 * dspl + 5] = dsrc[61];
            ddst[7 * dspl + 6] = dsrc[62];
            ddst[7 * dspl + 7] = dsrc[63];
#else
            double* dsrc, ddst; int dspl;

            dsrc = (double*)src;
            ddst = (double*)dst;
            dspl = spl >> 3;

            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += 4;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += 4;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += 4;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += 4;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += 4;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += 4;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
            dsrc += 4;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            ddst[2] = dsrc[2];
            ddst[3] = dsrc[3];
#endif
        }

        static void blit4_32(byte* src, byte* dst, int spl)
        {
#if true
            int* dsrc, ddst; int dspl;

            dsrc = (int*)src;
            ddst = (int*)dst;
            dspl = spl >> 2;

            ddst[0 * dspl + 0] = dsrc[0];
            ddst[0 * dspl + 1] = dsrc[1];
            ddst[0 * dspl + 2] = dsrc[2];
            ddst[0 * dspl + 3] = dsrc[3];
            ddst[1 * dspl + 0] = dsrc[4];
            ddst[1 * dspl + 1] = dsrc[5];
            ddst[1 * dspl + 2] = dsrc[6];
            ddst[1 * dspl + 3] = dsrc[7];
            ddst[2 * dspl + 0] = dsrc[8];
            ddst[2 * dspl + 1] = dsrc[9];
            ddst[2 * dspl + 2] = dsrc[10];
            ddst[2 * dspl + 3] = dsrc[11];
            ddst[3 * dspl + 0] = dsrc[12];
            ddst[3 * dspl + 1] = dsrc[13];
            ddst[3 * dspl + 2] = dsrc[14];
            ddst[3 * dspl + 3] = dsrc[15];
#else
            double* dsrc, ddst; int dspl;

            dsrc = (double*)src;
            ddst = (double*)dst;
            dspl = spl >> 3;

            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            dsrc += 2;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            dsrc += 2;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
            dsrc += 2;
            ddst += dspl;
            ddst[0] = dsrc[0];
            ddst[1] = dsrc[1];
#endif
        }

        static void blit2_32(byte* src, byte* dst, int spl)
        {
#if true
            int* dsrc, ddst; int dspl;

            dsrc = (int*)src;
            ddst = (int*)dst;
            dspl = spl >> 2;

            ddst[0 * dspl + 0] = dsrc[0];
            ddst[0 * dspl + 1] = dsrc[1];
            ddst[1 * dspl + 0] = dsrc[2];
            ddst[1 * dspl + 1] = dsrc[3];
#else
            double* dsrc, ddst; int dspl;

            dsrc = (double*)src;
            ddst = (double*)dst;
            dspl = spl >> 3;

            ddst[0] = dsrc[0];
            ddst[dspl] = dsrc[1];
#endif
        }

        ushort yuv_to_rgb(int y, int u, int v)
        {
            int r, g, b, YY = ROQ_YY_tab[y];

            r = (YY + ROQ_VR_tab[v]) >> 9;
            g = (YY + ROQ_UG_tab[u] + ROQ_VG_tab[v]) >> 8;
            b = (YY + ROQ_UB_tab[u]) >> 9;

            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            if (r > 31) r = 31;
            if (g > 63) g = 63;
            if (b > 31) b = 31;

            return (ushort)(r << 11 + g << 5 + (b));
        }

        uint yuv_to_rgb24(int y, int u, int v)
        {
            int r, g, b, YY = ROQ_YY_tab[y];

            r = (YY + ROQ_VR_tab[v]) >> 6;
            g = (YY + ROQ_UG_tab[u] + ROQ_VG_tab[v]) >> 6;
            b = (YY + ROQ_UB_tab[u]) >> 6;

            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;

            return (uint)LittleInt(r + g << 8 + b << 16);
        }

        void decodeCodeBook(byte* input, ushort roq_flags)
        {
            int i, j, two, four;
            ushort* aptr, bptr, cptr, dptr;
            int y0, y1, y2, y3, cr, cb;
            uint* iaptr, ibptr, icptr, idptr;

            if (roq_flags == 0) two = four = 256;
            else
            {
                two = roq_flags >> 8;
                if (two == 0) two = 256;
                four = roq_flags & 0xff;
            }

            four *= 2;
            bptr = vq2;

            if (!half)
            {
                if (!smootheddouble)
                {
                    // normal height
                    if (samplesPerPixel == 2)
                    {
                        for (i = 0; i < two; i++)
                        {
                            y0 = *input++;
                            y1 = *input++;
                            y2 = *input++;
                            y3 = *input++;
                            cr = *input++;
                            cb = *input++;
                            *bptr++ = yuv_to_rgb(y0, cr, cb);
                            *bptr++ = yuv_to_rgb(y1, cr, cb);
                            *bptr++ = yuv_to_rgb(y2, cr, cb);
                            *bptr++ = yuv_to_rgb(y3, cr, cb);
                        }

                        cptr = vq4;
                        dptr = vq8;

                        for (i = 0; i < four; i++)
                        {
                            aptr = vq2 + (*input++) * 4;
                            bptr = vq2 + (*input++) * 4;
                            for (j = 0; j < 2; j++) VQ2TO4(ref aptr, ref bptr, cptr, dptr);
                        }
                    }
                    else if (samplesPerPixel == 4)
                    {
                        ibptr = (uint*)bptr;
                        for (i = 0; i < two; i++)
                        {
                            y0 = *input++;
                            y1 = *input++;
                            y2 = *input++;
                            y3 = *input++;
                            cr = *input++;
                            cb = *input++;
                            *ibptr++ = yuv_to_rgb24(y0, cr, cb);
                            *ibptr++ = yuv_to_rgb24(y1, cr, cb);
                            *ibptr++ = yuv_to_rgb24(y2, cr, cb);
                            *ibptr++ = yuv_to_rgb24(y3, cr, cb);
                        }

                        icptr = (uint*)vq4;
                        idptr = (uint*)vq8;

                        for (i = 0; i < four; i++)
                        {
                            iaptr = (uint*)vq2 + (*input++) * 4;
                            ibptr = (uint*)vq2 + (*input++) * 4;
                            for (j = 0; j < 2; j++) VQ2TO4(ref iaptr, ref ibptr, icptr, idptr);
                        }
                    }
                }
                else
                {
                    // double height, smoothed
                    if (samplesPerPixel == 2)
                    {
                        for (i = 0; i < two; i++)
                        {
                            y0 = *input++;
                            y1 = *input++;
                            y2 = *input++;
                            y3 = *input++;
                            cr = *input++;
                            cb = *input++;
                            *bptr++ = yuv_to_rgb(y0, cr, cb);
                            *bptr++ = yuv_to_rgb(y1, cr, cb);
                            *bptr++ = yuv_to_rgb(((y0 * 3) + y2) / 4, cr, cb);
                            *bptr++ = yuv_to_rgb(((y1 * 3) + y3) / 4, cr, cb);
                            *bptr++ = yuv_to_rgb((y0 + (y2 * 3)) / 4, cr, cb);
                            *bptr++ = yuv_to_rgb((y1 + (y3 * 3)) / 4, cr, cb);
                            *bptr++ = yuv_to_rgb(y2, cr, cb);
                            *bptr++ = yuv_to_rgb(y3, cr, cb);
                        }

                        cptr = vq4;
                        dptr = vq8;

                        for (i = 0; i < four; i++)
                        {
                            aptr = vq2 + (*input++) * 8;
                            bptr = vq2 + (*input++) * 8;
                            for (j = 0; j < 2; j++) { VQ2TO4(ref aptr, ref bptr, cptr, dptr); VQ2TO4(ref aptr, ref bptr, cptr, dptr); }
                        }
                    }
                    else if (samplesPerPixel == 4)
                    {
                        ibptr = (uint*)bptr;
                        for (i = 0; i < two; i++)
                        {
                            y0 = *input++;
                            y1 = *input++;
                            y2 = *input++;
                            y3 = *input++;
                            cr = *input++;
                            cb = *input++;
                            *ibptr++ = yuv_to_rgb24(y0, cr, cb);
                            *ibptr++ = yuv_to_rgb24(y1, cr, cb);
                            *ibptr++ = yuv_to_rgb24(((y0 * 3) + y2) / 4, cr, cb);
                            *ibptr++ = yuv_to_rgb24(((y1 * 3) + y3) / 4, cr, cb);
                            *ibptr++ = yuv_to_rgb24((y0 + (y2 * 3)) / 4, cr, cb);
                            *ibptr++ = yuv_to_rgb24((y1 + (y3 * 3)) / 4, cr, cb);
                            *ibptr++ = yuv_to_rgb24(y2, cr, cb);
                            *ibptr++ = yuv_to_rgb24(y3, cr, cb);
                        }

                        icptr = (uint*)vq4;
                        idptr = (uint*)vq8;

                        for (i = 0; i < four; i++)
                        {
                            iaptr = (uint*)vq2 + (*input++) * 8;
                            ibptr = (uint*)vq2 + (*input++) * 8;
                            for (j = 0; j < 2; j++) { VQ2TO4(ref iaptr, ref ibptr, icptr, idptr); VQ2TO4(ref iaptr, ref ibptr, icptr, idptr); }
                        }
                    }
                }
            }
            else
            {
                // 1/4 screen
                if (samplesPerPixel == 2)
                {
                    for (i = 0; i < two; i++)
                    {
                        y0 = *input; input += 2;
                        y2 = *input; input += 2;
                        cr = *input++;
                        cb = *input++;
                        *bptr++ = yuv_to_rgb(y0, cr, cb);
                        *bptr++ = yuv_to_rgb(y2, cr, cb);
                    }

                    cptr = vq4;
                    dptr = vq8;

                    for (i = 0; i < four; i++)
                    {
                        aptr = vq2 + (*input++) * 2;
                        bptr = vq2 + (*input++) * 2;
                        for (j = 0; j < 2; j++) VQ2TO2(ref aptr, ref bptr, cptr, dptr);
                    }
                }
                else if (samplesPerPixel == 4)
                {
                    ibptr = (uint*)bptr;
                    for (i = 0; i < two; i++)
                    {
                        y0 = *input; input += 2;
                        y2 = *input; input += 2;
                        cr = *input++;
                        cb = *input++;
                        *ibptr++ = yuv_to_rgb24(y0, cr, cb);
                        *ibptr++ = yuv_to_rgb24(y2, cr, cb);
                    }

                    icptr = (uint*)vq4;
                    idptr = (uint*)vq8;

                    for (i = 0; i < four; i++)
                    {
                        iaptr = (uint*)vq2 + (*input++) * 2;
                        ibptr = (uint*)vq2 + (*input++) * 2;
                        for (j = 0; j < 2; j++) VQ2TO2(ref iaptr, ref ibptr, icptr, idptr);
                    }
                }
            }
        }

        void recurseQuad(int startX, int startY, int quadSize, int xOff, int yOff)
        {
            byte* scroff;
            int bigx, bigy, lowx, lowy, useY;
            int offset;

            offset = screenDelta;

            lowx = lowy = 0;
            bigx = (int)xsize;
            bigy = (int)ysize;

            if (bigx > CIN_WIDTH) bigx = CIN_WIDTH;
            if (bigy > CIN_HEIGHT) bigy = CIN_HEIGHT;

            if (startX >= lowx && startX + quadSize <= bigx && (startY + quadSize) <= bigy && startY >= lowy && quadSize <= MAXSIZE)
            {
                useY = startY;
                scroff = image + (useY + ((CIN_HEIGHT - bigy) >> 1) + yOff) * samplesPerLine + ((startX + xOff) * samplesPerPixel);

                qStatus0[onQuad] = scroff;
                qStatus1[onQuad++] = scroff + offset;
            }

            if (quadSize != MINSIZE)
            {
                quadSize >>= 1;
                recurseQuad(startX, startY, quadSize, xOff, yOff);
                recurseQuad(startX + quadSize, startY, quadSize, xOff, yOff);
                recurseQuad(startX, startY + quadSize, quadSize, xOff, yOff);
                recurseQuad(startX + quadSize, startY + quadSize, quadSize, xOff, yOff);
            }
        }

        void setupQuad(int xOff, int yOff)
        {
            int numQuadCels, i, x, y;
            byte* temp;

            numQuadCels = CIN_WIDTH * CIN_HEIGHT / 16;
            numQuadCels += numQuadCels / 4 + numQuadCels / 16;
            numQuadCels += 64;                            // for overflow

            numQuadCels = (int)(xsize * ysize / 16);
            numQuadCels += numQuadCels / 4;
            numQuadCels += 64;                            // for overflow

            onQuad = 0;

            for (y = 0; y < (int)ysize; y += 16) for (x = 0; x < (int)xsize; x += 16) recurseQuad(x, y, 16, xOff, yOff);
            temp = null;
            for (i = (numQuadCels - 64); i < numQuadCels; i++) { qStatus0[i] = temp; qStatus1[i] = temp; } // eoq
        }

        void readQuadInfo(byte* qData)
        {
            xsize = (uint)(qData[0] + qData[1] * 256);
            ysize = (uint)(qData[2] + qData[3] * 256);
            maxsize = (uint)(qData[4] + qData[5] * 256);
            minsize = (uint)(qData[6] + qData[7] * 256);

            CIN_HEIGHT = (int)ysize;
            CIN_WIDTH = (int)xsize;

            samplesPerLine = CIN_WIDTH * samplesPerPixel;
            screenDelta = CIN_HEIGHT * samplesPerLine;

            if (image == null) image = (byte*)Marshal.AllocHGlobal(CIN_WIDTH * CIN_HEIGHT * samplesPerPixel * 2);

            half = false;
            smootheddouble = false;

            var imageL = ((IntPtr)image).ToInt64();
            t0 = (int)(0 - imageL + imageL + screenDelta);
            t1 = (int)(0 - (imageL + screenDelta) + imageL);

            drawX = CIN_WIDTH;
            drawY = CIN_HEIGHT;
        }

        void RoQPrepMcomp(int xoff, int yoff)
        {
            int i, j, x, y, temp, temp2;

            i = samplesPerLine;
            j = samplesPerPixel;
            if (xsize == (ysize * 4) && !half) { j += j; i += i; }

            for (y = 0; y < 16; y++)
            {
                temp2 = (y + yoff - 8) * i;
                for (x = 0; x < 16; x++) { temp = (x + xoff - 8) * j; mcomp[(x * 16) + y] = normalBuffer0 - (temp2 + temp); }
            }
        }

        void RoQReset()
        {
            iFile.Seek(0, FS_SEEK.SET);
            iFile.Read(file, 16);
            RoQ_init();
            status = CinStatus.FMV_LOOPED;
        }

        const int DEFAULT_CIN_WIDTH = 512;
        const int DEFAULT_CIN_HEIGHT = 512;
        const int MAXSIZE = 8;
        const int MINSIZE = 4;

        const int ROQ_FILE = 0x1084;
        const int ROQ_QUAD = 0x1000;
        const int ROQ_QUAD_INFO = 0x1001;
        const int ROQ_CODEBOOK = 0x1002;
        const int ROQ_QUAD_VQ = 0x1011;
        const int ROQ_QUAD_JPEG = 0x1012;
        const int ROQ_QUAD_HANG = 0x1013;
        const int ROQ_PACKET = 0x1030;
        const int ZA_SOUND_MONO = 0x1020;
        const int ZA_SOUND_STEREO = 0x1021;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void VQ2TO4(ref uint* a, ref uint* b, uint* c, uint* d)
        {
            *c++ = a[0];
            *d++ = a[0];
            *d++ = a[0];
            *c++ = a[1];
            *d++ = a[1];
            *d++ = a[1];
            *c++ = b[0];
            *d++ = b[0];
            *d++ = b[0];
            *c++ = b[1];
            *d++ = b[1];
            *d++ = b[1];
            *d++ = a[0];
            *d++ = a[0];
            *d++ = a[1];
            *d++ = a[1];
            *d++ = b[0];
            *d++ = b[0];
            *d++ = b[1];
            *d++ = b[1];
            a += 2; b += 2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void VQ2TO4(ref ushort* a, ref ushort* b, ushort* c, ushort* d)
        {
            *c++ = a[0];
            *d++ = a[0];
            *d++ = a[0];
            *c++ = a[1];
            *d++ = a[1];
            *d++ = a[1];
            *c++ = b[0];
            *d++ = b[0];
            *d++ = b[0];
            *c++ = b[1];
            *d++ = b[1];
            *d++ = b[1];
            *d++ = a[0];
            *d++ = a[0];
            *d++ = a[1];
            *d++ = a[1];
            *d++ = b[0];
            *d++ = b[0];
            *d++ = b[1];
            *d++ = b[1];
            a += 2; b += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void VQ2TO2(ref uint* a, ref uint* b, uint* c, uint* d)
        {
            *c++ = *a;
            *d++ = *a;
            *d++ = *a;
            *c++ = *b;
            *d++ = *b;
            *d++ = *b;
            *d++ = *a;
            *d++ = *a;
            *d++ = *b;
            *d++ = *b;
            a++; b++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void VQ2TO2(ref ushort* a, ref ushort* b, ushort* c, ushort* d)
        {
            *c++ = *a;
            *d++ = *a;
            *d++ = *a;
            *c++ = *b;
            *d++ = *b;
            *d++ = *b;
            *d++ = *a;
            *d++ = *a;
            *d++ = *b;
            *d++ = *b;
            a++; b++;
        }

        // jpeg error handling
        static jpeg_error_mgr jerr;

        bool JPEGBlit(byte* wStatus, byte* data, int datasize)
        {
            // This struct contains the JPEG decompression parameters and pointers to working space (which is allocated as needed by the JPEG library).
            jpeg_decompress_struct cinfo;
            // We use our private extension JPEG error handler. Note that this struct must live as long as the main JPEG parameter struct, to avoid dangling-pointer problems.

            fixed (jpeg_error_mgr* jerrP = &jerr)
            {
                // More stuff
                JSAMPARRAY_2** buffer;      // Output row buffer
                uint row_stride;     // physical row width in output buffer 

                // Step 1: allocate and initialize JPEG decompression object

                // We set up the normal JPEG error routines, then override error_exit.
                cinfo.err = jpeg_std_error(jerrP);

                // Now we can initialize the JPEG decompression object.
                jpeg_create_decompress(&cinfo);

                // Step 2: specify data source (eg, a file)

                jpeg_mem_src(&cinfo, data, datasize);

                // Step 3: read file parameters with jpeg_read_header()

                jpeg_read_header(&cinfo, TRUE);
                // We can ignore the return value from jpeg_read_header since
                //   (a) suspension is not possible with the stdio data source, and
                //   (b) we passed TRUE to reject a tables-only JPEG file as an error.
                // See libjpeg.doc for more info.

                // Step 4: set parameters for decompression

                // In this example, we don't need to change any of the defaults set by peg_read_header(), so we do nothing here.

                // Step 5: Start decompressor

                //cinfo.dct_method = J_DCT_METHOD.JDCT_IFAST;
                cinfo.dct_method = JDCT_FASTEST;
                cinfo.dither_mode = J_DITHER_MODE.JDITHER_NONE;
                cinfo.do_fancy_upsampling = FALSE;
                //cinfo.out_color_space = JCS_GRAYSCALE;

                jpeg_start_decompress(&cinfo);
                // We can ignore the return value since suspension is not possible with the stdio data source.

                // We may need to do some setup of our own at this point before reading the data.  After jpeg_start_decompress() we have the correct scaled
                // output image dimensions available, as well as the output colormap if we asked for color quantization.
                // In this example, we need to make an output work buffer of the right size.

                // JSAMPLEs per row in output buffer
                row_stride = (uint)(cinfo.output_width * cinfo.output_components);

                // Make a one-row-high sample array that will go away when done with image
                buffer = cinfo.mem->alloc_sarray((jpeg_common_struct*)&cinfo, JPOOL_IMAGE, row_stride, 1);

                // Step 6: while (scan lines remain to be read)
                //           jpeg_read_scanlines(...);

                // Here we use the library's state variable cinfo.output_scanline as the loop counter, so that we don't have to keep track ourselves.

                wStatus += (cinfo.output_height - 1) * row_stride;
                while (cinfo.output_scanline < cinfo.output_height)
                {
                    // jpeg_read_scanlines expects an array of pointers to scanlines.
                    // Here the array is only one element long, but you could ask for more than one scanline at a time if that's more convenient.
                    jpeg_read_scanlines(&cinfo, &buffer[0], 1);

                    // Assume put_scanline_someplace wants a pointer and sample count.
                    Unsafe.CopyBlock(wStatus, &buffer[0][0], row_stride);

                    wStatus -= row_stride;
                }

                // Step 7: Finish decompression

                jpeg_finish_decompress(&cinfo);
                // We can ignore the return value since suspension is not possible with the stdio data source.

                // Step 8: Release JPEG decompression object

                // This is an important step since it will release a good deal of memory.
                jpeg_destroy_decompress(&cinfo);

                // At this point you may want to check to see whether any corrupt-data warnings occurred (test whether jerr.pub.num_warnings is nonzero).
            }
            // And we're done!
            return true;
        }
    }
}