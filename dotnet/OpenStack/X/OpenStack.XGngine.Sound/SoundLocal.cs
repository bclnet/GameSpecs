namespace System.NumericsX.OpenStack.Gngine.Sound
{
    public interface IAudioBuffer
    {
        int Play(int dwPriority = 0, int dwFlags = 0);
        int Stop();
        int Reset();
        bool IsSoundPlaying();
        void SetVolume(float x);
    }
}
