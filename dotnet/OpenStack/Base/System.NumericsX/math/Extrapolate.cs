using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    public enum EXTRAPOLATION
    {
        NONE = 0x01,    // no extrapolation, covered distance = duration * 0.001 * ( baseSpeed )
        LINEAR = 0x02,  // linear extrapolation, covered distance = duration * 0.001 * ( baseSpeed + speed )
        ACCELLINEAR = 0x04, // linear acceleration, covered distance = duration * 0.001 * ( baseSpeed + 0.5 * speed )
        DECELLINEAR = 0x08, // linear deceleration, covered distance = duration * 0.001 * ( baseSpeed + 0.5 * speed )
        ACCELSINE = 0x10,   // sinusoidal acceleration, covered distance = duration * 0.001 * ( baseSpeed + sqrt( 0.5 ) * speed )
        DECELSINE = 0x20,   // sinusoidal deceleration, covered distance = duration * 0.001 * ( baseSpeed + sqrt( 0.5 ) * speed )
        NOSTOP = 0x40   // do not stop at startTime + duration
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Extrapolate_Vector4
    {
        EXTRAPOLATION extrapolationType;
        float startTime;
        float duration;
        Vector4 startValue;
        Vector4 baseSpeed;
        Vector4 speed;
        float currentTime;
        Vector4 currentValue;

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Extrapolate_Vector4()
        {
            extrapolationType = EXTRAPOLATION.NONE;
            startTime = duration = 0f;
            startValue = default;
            baseSpeed = default;
            speed = default;
            currentTime = -1;
            currentValue = startValue;
        }
        */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(float startTime, float duration, in Vector4 startValue, in Vector4 baseSpeed, in Vector4 speed, EXTRAPOLATION extrapolationType)
        {
            this.extrapolationType = extrapolationType;
            this.startTime = startTime;
            this.duration = duration;
            this.startValue = startValue;
            this.baseSpeed = baseSpeed;
            this.speed = speed;
            currentTime = -1;
            currentValue = startValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 GetCurrentValue(float time)
        {
            float deltaTime, s;
            if (time == currentTime) return currentValue;
            currentTime = time;
            if (time < startTime) return startValue;

            if ((extrapolationType & EXTRAPOLATION.NOSTOP) == 0 && (time > startTime + duration)) time = startTime + duration;

            switch (extrapolationType & ~EXTRAPOLATION.NOSTOP)
            {
                case EXTRAPOLATION.NONE:
                    deltaTime = (time - startTime) * 0.001f;
                    currentValue = startValue + deltaTime * baseSpeed;
                    break;
                case EXTRAPOLATION.LINEAR:
                    deltaTime = (time - startTime) * 0.001f;
                    currentValue = startValue + deltaTime * (baseSpeed + speed);
                    break;
                case EXTRAPOLATION.ACCELLINEAR:
                    if (duration == 0) currentValue = startValue;
                    else { deltaTime = (time - startTime) / duration; s = (0.5f * deltaTime * deltaTime) * (duration * 0.001f); currentValue = startValue + deltaTime * baseSpeed + s * speed; }
                    break;
                case EXTRAPOLATION.DECELLINEAR:
                    if (duration == 0) currentValue = startValue;
                    else { deltaTime = (time - startTime) / duration; s = (deltaTime - (0.5f * deltaTime * deltaTime)) * (duration * 0.001f); currentValue = startValue + deltaTime * baseSpeed + s * speed; }
                    break;
                case EXTRAPOLATION.ACCELSINE:
                    if (duration == 0) currentValue = startValue;
                    else { deltaTime = (time - startTime) / duration; s = (1f - MathX.Cos(deltaTime * MathX.HALF_PI)) * duration * 0.001f * MathX.SQRT_1OVER2; currentValue = startValue + deltaTime * baseSpeed + s * speed; }
                    break;
                case EXTRAPOLATION.DECELSINE:
                    if (duration == 0) currentValue = startValue;
                    else { deltaTime = (time - startTime) / duration; s = MathX.Sin(deltaTime * MathX.HALF_PI) * duration * 0.001f * MathX.SQRT_1OVER2; currentValue = startValue + deltaTime * baseSpeed + s * speed; }
                    break;
            }
            return currentValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 GetCurrentSpeed(float time)
        {
            if (time < startTime || duration == 0) return startValue - startValue;
            if ((extrapolationType & EXTRAPOLATION.NOSTOP) == 0 && (time > startTime + duration)) return startValue - startValue;

            float deltaTime, s;
            switch (extrapolationType & ~EXTRAPOLATION.NOSTOP)
            {
                case EXTRAPOLATION.NONE: return baseSpeed;
                case EXTRAPOLATION.LINEAR: return baseSpeed + speed;
                case EXTRAPOLATION.ACCELLINEAR: deltaTime = (time - startTime) / duration; s = deltaTime; return baseSpeed + s * speed;
                case EXTRAPOLATION.DECELLINEAR: deltaTime = (time - startTime) / duration; s = 1f - deltaTime; return baseSpeed + s * speed;
                case EXTRAPOLATION.ACCELSINE: deltaTime = (time - startTime) / duration; s = MathX.Sin(deltaTime * MathX.HALF_PI); return baseSpeed + s * speed;
                case EXTRAPOLATION.DECELSINE: deltaTime = (time - startTime) / duration; s = MathX.Cos(deltaTime * MathX.HALF_PI); return baseSpeed + s * speed;
                default: return baseSpeed;
            }
        }

        public bool IsDone(float time)
            => (extrapolationType & EXTRAPOLATION.NOSTOP) == 0 && time >= startTime + duration;

        public float StartTime
        {
            get => startTime;
            set { startTime = value; currentTime = -1; }
        }

        public float EndTime
            => (extrapolationType & EXTRAPOLATION.NOSTOP) == 0 && duration > 0 ? startTime + duration : 0;

        public float Duration
            => duration;

        public Vector4 StartValue
        {
            get => startValue;
            set { startValue = value; currentTime = -1; }
        }

        public Vector4 BaseSpeed
            => baseSpeed;
    
        public Vector4 Speed
            => speed;

        public EXTRAPOLATION ExtrapolationType
            => extrapolationType;
    }
}