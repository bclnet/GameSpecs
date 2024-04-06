using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Interpolate_Vector4
    {
        float startTime;
        float duration;
        Vector4 startValue;
        Vector4 endValue;
        float currentTime;
        Vector4 currentValue;

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Interpolate_Vector4()
        {
            currentTime = startTime = duration = 0;
            currentValue = default;
            startValue = endValue = currentValue;
        }
        */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(float startTime, float duration, in Vector4 startValue, in Vector4 endValue)
        {
            this.startTime = startTime;
            this.duration = duration;
            this.startValue = startValue;
            this.endValue = endValue;
            this.currentTime = startTime - 1;
            this.currentValue = startValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 GetCurrentValue(float time)
        {
            var deltaTime = time - startTime;
            if (time != currentTime)
            {
                currentTime = time;
                if (deltaTime <= 0) currentValue = startValue;
                else if (deltaTime >= duration) currentValue = endValue;
                else currentValue = startValue + (endValue - startValue) * ((float)deltaTime / duration);
            }
            return currentValue;
        }

        public bool IsDone(float time)
            => time >= startTime + duration;

        public float StartTime
        {
            get => startTime;
            set => startTime = value;
        }

        public float EndTime
            => startTime + duration;

        public float Duration
        {
            get => duration;
            set => duration = value;
        }
        
        public Vector4 StartValue
        {
            get => startValue;
            set => startValue = value;
        }

        public Vector4 EndValue
        {
            get => endValue;
            set => endValue = value;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InterpolateAccelDecelLinear_Vector4
    {
        float startTime;
        float accelTime;
        float linearTime;
        float decelTime;
        Vector4 startValue;
        Vector4 endValue;
        Extrapolate_Vector4 extrapolate; // = new();

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InterpolateAccelDecelLinear_Vector4()
        {
            startTime = accelTime = linearTime = decelTime = 0;
            startValue = default;
            endValue = startValue;
        }
        */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(float startTime, float accelTime, float decelTime, float duration, in Vector4 startValue, in Vector4 endValue)
        {
            this.startTime = startTime;
            this.accelTime = accelTime;
            this.decelTime = decelTime;
            this.startValue = startValue;
            this.endValue = endValue;
            if (duration <= 0f)
                return;

            if (this.accelTime + this.decelTime > duration)
            {
                this.accelTime = this.accelTime * duration / (this.accelTime + this.decelTime);
                this.decelTime = duration - this.accelTime;
            }
            this.linearTime = duration - this.accelTime - this.decelTime;
            Vector4 speed = (endValue - startValue) * (1000f / (this.linearTime + (this.accelTime + this.decelTime) * 0.5f));

            if (this.accelTime != 0) extrapolate.Init(startTime, this.accelTime, startValue, startValue - startValue, speed, EXTRAPOLATION.ACCELLINEAR);
            else if (this.linearTime != 0) extrapolate.Init(startTime, this.linearTime, startValue, startValue - startValue, speed, EXTRAPOLATION.LINEAR);
            else extrapolate.Init(startTime, this.decelTime, startValue, startValue - startValue, speed, EXTRAPOLATION.DECELLINEAR);
        }
        
        public void SetStartTime(float time)
        {
            startTime = time; Invalidate();
        }
        
        public void SetStartValue(in Vector4 startValue)
        {
            this.startValue = startValue; Invalidate();    
        }
        
        public void SetEndValue(in Vector4 endValue)
        {
            this.endValue = endValue; Invalidate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 GetCurrentValue(float time)
        {
            SetPhase(time);
            return extrapolate.GetCurrentValue(time);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 GetCurrentSpeed(float time)
        {
            SetPhase(time);
            return extrapolate.GetCurrentSpeed(time);
        }
        
        public bool IsDone(float time)
            => time >= startTime + accelTime + linearTime + decelTime;

        public float StartTime
            => startTime;
        
        public float EndTime
            => startTime + accelTime + linearTime + decelTime;
        
        public float Duration
            => accelTime + linearTime + decelTime;
        
        public float Acceleration
            => accelTime;
        
        public float Deceleration
            => decelTime;
        
        public Vector4 StartValue
            => startValue;
        
        public Vector4 EndValue
            => endValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Invalidate()
            => extrapolate.Init(0, 0, extrapolate.StartValue, extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.NONE);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetPhase(float time)
        {
            float deltaTime;

            deltaTime = time - startTime;
            if (deltaTime < accelTime)
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.ACCELLINEAR)
                    extrapolate.Init(startTime, accelTime, startValue, extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.ACCELLINEAR);
            }
            else if (deltaTime < accelTime + linearTime)
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.LINEAR)
                    extrapolate.Init(startTime + accelTime, linearTime, startValue + extrapolate.Speed * (accelTime * 0.001f * 0.5f), extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.LINEAR);
            }
            else
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.DECELLINEAR)
                    extrapolate.Init(startTime + accelTime + linearTime, decelTime, endValue - (extrapolate.Speed * (decelTime * 0.001f * 0.5f)), extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.DECELLINEAR);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InterpolateAccelDecelSine_Vector4
    {
        float startTime;
        float accelTime;
        float linearTime;
        float decelTime;
        Vector4 startValue;
        Vector4 endValue;
        Extrapolate_Vector4 extrapolate; // = new();

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InterpolateAccelDecelSine_Vector4()
        {
            startTime = accelTime = linearTime = decelTime = 0;
            startValue = default;
            endValue = startValue;
        }
        */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(float startTime, float accelTime, float decelTime, float duration, in Vector4 startValue, in Vector4 endValue)
        {
            this.startTime = startTime;
            this.accelTime = accelTime;
            this.decelTime = decelTime;
            this.startValue = startValue;
            this.endValue = endValue;

            if (duration <= 0f)
                return;

            if (this.accelTime + this.decelTime > duration)
            {
                this.accelTime = this.accelTime * duration / (this.accelTime + this.decelTime);
                this.decelTime = duration - this.accelTime;
            }
            this.linearTime = duration - this.accelTime - this.decelTime;
            Vector4 speed = (endValue - startValue) * (1000f / (this.linearTime + (this.accelTime + this.decelTime) * MathX.SQRT_1OVER2));

            if (this.accelTime == 0) extrapolate.Init(startTime, this.accelTime, startValue, startValue - startValue, speed, EXTRAPOLATION.ACCELSINE);
            else if (this.linearTime == 0) extrapolate.Init(startTime, this.linearTime, startValue, startValue - startValue, speed, EXTRAPOLATION.LINEAR);
            else extrapolate.Init(startTime, this.decelTime, startValue, startValue - startValue, speed, EXTRAPOLATION.DECELSINE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 GetCurrentValue(float time)
        {
            SetPhase(time);
            return extrapolate.GetCurrentValue(time);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 GetCurrentSpeed(float time)
        {
            SetPhase(time);
            return extrapolate.GetCurrentSpeed(time);
        }
        
        public bool IsDone(float time)
            => time >= startTime + accelTime + linearTime + decelTime;

        public float StartTime
        {
            get => startTime;
            set { startTime = value; Invalidate(); }
        }

        public float EndTime
            => startTime + accelTime + linearTime + decelTime;

        public float Duration
            => accelTime + linearTime + decelTime;

        public float Acceleration
            => accelTime;

        public float Deceleration
            => decelTime;

        public Vector4 StartValue
        {
            get => startValue;
            set { startValue = value; Invalidate(); }
        }

        public Vector4 EndValue
        {
            get => endValue;
            set { endValue = value; Invalidate(); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Invalidate()
            => extrapolate.Init(0, 0, extrapolate.StartValue, extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.NONE);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetPhase(float time)
        {
            var deltaTime = time - startTime;
            if (deltaTime < accelTime)
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.ACCELSINE)
                    extrapolate.Init(startTime, accelTime, startValue, extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.ACCELSINE);
            }
            else if (deltaTime < accelTime + linearTime)
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.LINEAR)
                    extrapolate.Init(startTime + accelTime, linearTime, startValue + extrapolate.Speed * (accelTime * 0.001f * MathX.SQRT_1OVER2), extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.LINEAR);
            }
            else
            {
                if (extrapolate.ExtrapolationType != EXTRAPOLATION.DECELSINE)
                    extrapolate.Init(startTime + accelTime + linearTime, decelTime, endValue - (extrapolate.Speed * (decelTime * 0.001f * MathX.SQRT_1OVER2)), extrapolate.BaseSpeed, extrapolate.Speed, EXTRAPOLATION.DECELSINE);
            }
        }
    }
}