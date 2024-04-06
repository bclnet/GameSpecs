using OpenStack.Graphics.Renderer1.Animations;
using System;
using System.Numerics;

namespace OpenStack.Graphics.Renderer1
{
    //was:Render/AnimationController
    public class AnimationController
    {
        readonly FrameCache frameCache;
        Action<IAnimation, int> updateHandler = (_, __) => { };
        IAnimation activeAnimation;
        float Time;
        bool shouldUpdate;

        public IAnimation ActiveAnimation => activeAnimation;
        public bool IsPaused;
        public int Frame
        {
            get => activeAnimation != null && activeAnimation.FrameCount != 0
                ? (int)Math.Round(Time * activeAnimation.Fps) % activeAnimation.FrameCount
                : 0;
            set
            {
                if (activeAnimation != null)
                {
                    Time = activeAnimation.Fps != 0
                        ? value / activeAnimation.Fps
                        : 0f;
                    shouldUpdate = true;
                }
            }
        }

        public AnimationController(ISkeleton skeleton) => frameCache = new FrameCache(skeleton);

        public bool Update(float timeStep)
        {
            if (activeAnimation == null) return false;

            if (IsPaused)
            {
                var res = shouldUpdate;
                shouldUpdate = false;
                return res;
            }
            Time += timeStep;
            updateHandler(activeAnimation, Frame);
            shouldUpdate = false;
            return true;
        }

        public void SetAnimation(IAnimation animation)
        {
            frameCache.Clear();
            activeAnimation = animation;
            Time = 0f;
            updateHandler(activeAnimation, -1);
        }

        public void PauseLastFrame()
        {
            IsPaused = true;
            Frame = activeAnimation == null ? 0 : activeAnimation.FrameCount - 1;
        }

        public Matrix4x4[] GetAnimationMatrices(ISkeleton skeleton)
            => IsPaused
            ? activeAnimation.GetAnimationMatrices(frameCache, Frame, skeleton)
            : activeAnimation.GetAnimationMatrices(frameCache, Time, skeleton);

        public void RegisterUpdateHandler(Action<IAnimation, int> handler) => updateHandler = handler;
    }
}
