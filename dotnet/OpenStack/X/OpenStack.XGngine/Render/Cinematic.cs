using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    // cinematic states
    public enum CinStatus
    {
        FMV_IDLE,
        FMV_PLAY,           // play
        FMV_EOF,            // all other conditions, i.e. stop/EOF/abort
        FMV_ID_BLT,
        FMV_ID_IDLE,
        FMV_LOOPED,
        FMV_ID_WAIT
    }

    // a cinematic stream generates an image buffer, which the caller will upload to a texture
    public unsafe struct CinData
    {
        public int imageWidth, imageHeight; // will be a power of 2
        public byte* image;                // RGBA format, alpha will be 255
        public int status;
    }

    public unsafe class Cinematic
    {
        // temporary buffers used by all cinematics
        protected static int[] ROQ_YY_tab = new int[256];
        protected static int[] ROQ_UB_tab = new int[256];
        protected static int[] ROQ_UG_tab = new int[256];
        protected static int[] ROQ_VG_tab = new int[256];
        protected static int[] ROQ_VR_tab = new int[256];
        protected static byte* file = null;
        protected static ushort* vq2 = null;
        protected static ushort* vq4 = null;
        protected static ushort* vq8 = null;

        public virtual void Dispose()
            => Close();

        // initialize cinematic play back data
        public static void InitCinematic()
        {
            float t_ub, t_vr, t_ug, t_vg; int i;

            // generate YUV tables
            t_ub = (1.77200f / 2.0f) * (float)(1 << 6) + 0.5f;
            t_vr = (1.40200f / 2.0f) * (float)(1 << 6) + 0.5f;
            t_ug = (0.34414f / 2.0f) * (float)(1 << 6) + 0.5f;
            t_vg = (0.71414f / 2.0f) * (float)(1 << 6) + 0.5f;
            for (i = 0; i < 256; i++)
            {
                var x = (float)(2 * i - 255);

                ROQ_UB_tab[i] = (int)((t_ub * x) + (1 << 5));
                ROQ_VR_tab[i] = (int)((t_vr * x) + (1 << 5));
                ROQ_UG_tab[i] = (int)((-t_ug * x));
                ROQ_VG_tab[i] = (int)((-t_vg * x) + (1 << 5));
                ROQ_YY_tab[i] = (int)((i << 6) | (i >> 2));
            }

            
            file = (byte*)Marshal.AllocHGlobal(65536);
            vq2 = (ushort*)Marshal.AllocHGlobal(256 * 16 * 4 * sizeof(ushort));
            vq4 = (ushort*)Marshal.AllocHGlobal(256 * 64 * 4 * sizeof(ushort));
            vq8 = (ushort*)Marshal.AllocHGlobal(256 * 256 * 4 * sizeof(ushort));
        }

        // shutdown cinematic play back data
        public static void ShutdownCinematic()
        {
            Marshal.FreeHGlobal((IntPtr)file); file = null;
            Marshal.FreeHGlobal((IntPtr)vq2); vq2 = null;
            Marshal.FreeHGlobal((IntPtr)vq4); vq4 = null;
            Marshal.FreeHGlobal((IntPtr)vq8); vq8 = null;
        }

        // allocates and returns a private subclass that implements the methods
        // This should be used instead of new
        public static Cinematic Alloc() => throw new NotImplementedException(); // new CinematicLocal();

        // returns false if it failed to load
        public virtual bool InitFromFile(string qpath, bool looping)
            => false;

        // returns the length of the animation in milliseconds
        public virtual int AnimationLength
            => 0;

        // the pointers in cinData_t will remain valid until the next UpdateForTime() call
        public virtual CinData ImageForTime(int milliseconds)
        {
            CinData c = default;
            return c;
        }

        // closes the file and frees all allocated memory
        public virtual void Close() { }

        // closes the file and frees all allocated memory
        public virtual void ResetTime(int time) { }
    }

    public class SndWindow : Cinematic
    {
        public override bool InitFromFile(string qpath, bool looping)
        {
            showWaveform = qpath.Equals("waveform", StringComparison.OrdinalIgnoreCase);
            return true;
        }

        public override CinData ImageForTime(int milliseconds)
            => soundSystem.ImageForTime(milliseconds, showWaveform);

        public override int AnimationLength
            => -1;

        bool showWaveform;
    }
}