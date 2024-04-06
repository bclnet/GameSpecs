using System.Numerics;

namespace OpenStack.Graphics.Renderer1.Animations
{
    public interface IAnimation
    {
        string Name { get; }
        float Fps { get; }
        int FrameCount { get; }
        void DecodeFrame(int frameIndex, Frame outFrame);
        Matrix4x4[] GetAnimationMatrices(FrameCache frameCache, int frameIndex, ISkeleton skeleton);
        Matrix4x4[] GetAnimationMatrices(FrameCache frameCache, float time, ISkeleton skeleton);
    }
}
