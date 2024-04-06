using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
	public class Curve_Vector4
	{
		protected List<float> times;          // knots
		protected List<Vector4> values;         // knot values

		protected int currentIndex; // cached index for fast lookup
		protected bool changed;     // set whenever the curve changes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Curve_Vector4()
		{
			currentIndex = -1;
			changed = false;
		}

		/// <summary>
		/// add a timed/value pair to the spline
		/// returns the index to the inserted pair
		/// </summary>
		/// <param name="time">The time.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual int AddValue(float time, in Vector4 value)
		{
			var i = IndexForTime(time);
			times.Insert(i, time);
			values.Insert(i, value);
			changed = true;
			return i;
		}

		public virtual void RemoveIndex(int index)
		{
			values.RemoveAt(index); times.RemoveAt(index); changed = true;
		}

		public virtual void Clear()
		{
			values.Clear(); times.Clear(); currentIndex = -1; changed = true;
		}

		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual Vector4 GetCurrentValue(float time)
		{
			var i = IndexForTime(time);
			return i >= values.Count
				? values[^1]
				: values[i];
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual Vector4 GetCurrentFirstDerivative(float time)
			=> values[0] - values[0];

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual Vector4 GetCurrentSecondDerivative(float time)
			=> values[0] - values[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual bool IsDone(float time)
			=> time >= times[times.Count - 1];

		public int NumValues
			=> values.Count;

		public void SetValue(int index, in Vector4 value)
		{
			values[index] = value; changed = true;
		}

		public Vector4 GetValue(int index)
			=> values[index];

		public Vector4 GetValueAddress(int index)
			=> values[index];

		public float GetTime(int index)
			=> times[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetLengthForTime(float time)
		{
			var length = 0f;
			var index = IndexForTime(time);
			for (var i = 0; i < index; i++) length += RombergIntegral(times[i], times[i + 1], 5);
			length += RombergIntegral(times[index], time, 5);
			return length;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe float GetTimeForLength(float length, float epsilon = 0.1f)
		{
			if (length <= 0f) return times[0];

			var accumLength = stackalloc float[values.Count]; accumLength = (float*)_alloca16(accumLength);
			var totalLength = 0f;
			int index;
			for (index = 0; index < values.Count - 1; index++)
			{
				totalLength += GetLengthBetweenKnots(index, index + 1);
				accumLength[index] = totalLength;
				if (length < accumLength[index]) break;
			}

			if (index >= values.Count - 1) return times[^1];

			float len0, len1;
			if (index == 0) { len0 = length; len1 = accumLength[0]; }
			else { len0 = length - accumLength[index - 1]; len1 = accumLength[index] - accumLength[index - 1]; }

			// invert the arc length integral using Newton's method
			var t = (times[index + 1] - times[index]) * len0 / len1;
			for (var i = 0; i < 32; i++)
			{
				var diff = RombergIntegral(times[index], times[index] + t, 5) - len0;
				if (MathX.Fabs(diff) <= epsilon) return times[index] + t;
				t -= diff / GetSpeed(times[index] + t);
			}
			return times[index] + t;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetLengthBetweenKnots(int i0, int i1)
		{
			var length = 0f;
			for (var i = i0; i < i1; i++) length += RombergIntegral(times[i], times[i + 1], 5);
			return length;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MakeUniform(float totalTime)
		{
			var n = times.Count - 1;
			for (var i = 0; i <= n; i++) times[i] = i * totalTime / n;
			changed = true;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void SetConstantSpeed(float totalTime)
		{
			var length = stackalloc float[values.Count]; length = (float*)_alloca16(length);
			var totalLength = 0f;
			int i; for (i = 0; i < values.Count - 1; i++) { length[i] = GetLengthBetweenKnots(i, i + 1); totalLength += length[i]; }
			var scale = totalTime / totalLength;
			float t; for (t = 0f, i = 0; i < times.Count - 1; i++) { times[i] = t; t += scale * length[i]; }
			times[^1] = totalTime;
			changed = true;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ShiftTime(float deltaTime)
		{
			for (var i = 0; i < times.Count; i++) times[i] += deltaTime;
			changed = true;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Translate(in Vector4 translation)
		{
			for (var i = 0; i < values.Count; i++) values[i] += translation;
			changed = true;
		}

		/// <summary>
		/// find the index for the first time greater than or equal to the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected int IndexForTime(float time)
		{
			if (currentIndex >= 0 && currentIndex <= times.Count)
			{
				// use the cached index if it is still valid
				if (currentIndex == 0)
				{
					if (time <= times[currentIndex]) return currentIndex;
				}
				else if (currentIndex == times.Count)
				{
					if (time > times[currentIndex - 1]) return currentIndex;
				}
				else if (time > times[currentIndex - 1] && time <= times[currentIndex]) return currentIndex;
				else if (time > times[currentIndex] && (currentIndex + 1 == times.Count || time <= times[currentIndex + 1])) { currentIndex++; return currentIndex; } // use the next index
			}

			// use binary search to find the index for the given time
			var len = times.Count;
			var mid = len;
			var offset = 0;
			var res = 0;
			while (mid > 0)
			{
				mid = len >> 1;
				if (time == times[offset + mid]) return offset + mid;
				else if (time > times[offset + mid]) { offset += mid; len -= mid; res = 1; }
				else { len -= mid; res = 0; }
			}
			currentIndex = offset + res;
			return currentIndex;
		}

		protected float TimeForIndex(int index)
		{
			var n = times.Count - 1;
			if (index < 0) return times[0] + index * (times[1] - times[0]);
			else if (index > n) return times[n] + (index - n) * (times[n] - times[n - 1]);
			return times[index];
		}

		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected Vector4 ValueForIndex(int index)
		{
			var n = values.Count - 1;
			if (index < 0) return values[0] + index * (values[1] - values[0]);
			else if (index > n) return values[n] + (index - n) * (values[n] - values[n - 1]);
			return values[index];
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected float GetSpeed(float time)
		{
			var value = GetCurrentFirstDerivative(time);
			float speed; int i;
			for (speed = 0f, i = 0; i < Vector4.Dimension; i++) speed += value[i] * value[i];
			return MathX.Sqrt(speed);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe float RombergIntegral(float t0, float t1, int order)
		{
			var temp0 = stackalloc float[order]; temp0 = (float*)_alloca16(temp0);
			var temp1 = stackalloc float[order]; temp1 = (float*)_alloca16(temp1);

			var delta = t1 - t0;
			temp0[0] = 0.5f * delta * (GetSpeed(t0) + GetSpeed(t1));

			int i, j, k, m, n;
			for (i = 2, m = 1; i <= order; i++, m *= 2, delta *= 0.5f)
			{
				// approximate using the trapezoid rule
				var sum = 0f;
				for (j = 1; j <= m; j++) sum += GetSpeed(t0 + delta * (j - 0.5f));

				// Richardson extrapolation
				temp1[0] = 0.5f * (temp0[0] + delta * sum);
				for (k = 1, n = 4; k < i; k++, n *= 4) temp1[k] = (n * temp1[k - 1] - temp0[k - 1]) / (n - 1);

				for (j = 0; j < i; j++) temp0[j] = temp1[j];
			}
			return temp0[order - 1];
		}
	}

	/// <summary>
	/// Bezier Curve template.
	/// The degree of the polynomial equals the number of knots minus one.
	/// </summary>
	/// <typeparam name="Vector4"></typeparam>
	public class Curve_Bezier_Vector4 : Curve_Vector4
	{
		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentValue(float time)
		{
			var bvals = stackalloc float[values.Count]; bvals = (float*)_alloca16(bvals);

			Basis(values.Count, time, bvals);
			var v = bvals[0] * values[0];
			for (var i = 1; i < values.Count; i++) v += bvals[i] * values[i];
			return v;
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentFirstDerivative(float time)
		{
			var bvals = stackalloc float[values.Count]; bvals = (float*)_alloca16(bvals);

			BasisFirstDerivative(values.Count, time, bvals);
			var v = bvals[0] * values[0];
			for (var i = 1; i < values.Count; i++) v += bvals[i] * values[i];
			var d = (times[^1] - times[0]);
			return (float)(values.Count - 1) / d * v;
		}
		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentSecondDerivative(float time)
		{
			var bvals = stackalloc float[values.Count]; bvals = (float*)_alloca16(bvals);

			BasisSecondDerivative(values.Count, time, bvals);
			var v = bvals[0] * values[0];
			for (var i = 1; i < values.Count; i++) v += bvals[i] * values[i];
			var d = (times[^1] - times[0]);
			return (float)(values.Count - 2) * (values.Count - 1) / (d * d) * v;
		}

		/// <summary>
		/// bezier basis functions
		/// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void Basis(int order, float t, float* bvals)
		{
			bvals[0] = 1f;
			var d = order - 1;
			if (d <= 0) return;

			var c = stackalloc float[d + 1]; c = (float*)_alloca16(c);
			var s = (float)(t - times[0]) / (times[^1] - times[0]);
			var o = 1f - s;
			var ps = s;
			var po = o;

			int i, j;
			for (i = 1; i < d; i++) c[i] = 1f;
			for (i = 1; i < d; i++)
			{
				c[i - 1] = 0f;
				var c1 = c[i];
				c[i] = 1f;
				for (j = i + 1; j <= d; j++) { var c2 = c[j]; c[j] = c1 + c[j - 1]; c1 = c2; }
				bvals[i] = c[d] * ps;
				ps *= s;
			}
			for (i = d - 1; i >= 0; i--) { bvals[i] *= po; po *= o; }
			bvals[d] = ps;
		}

		/// <summary>
		/// first derivative of bezier basis functions
		/// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisFirstDerivative(int order, float t, float* bvals)
		{
			Basis(order - 1, t, bvals + 1);
			bvals[0] = 0f;
			for (var i = 0; i < order - 1; i++) bvals[i] -= bvals[i + 1];
		}

		/// <summary>
		/// second derivative of bezier basis functions
		/// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisSecondDerivative(int order, float t, float* bvals)
		{
			BasisFirstDerivative(order - 1, t, bvals + 1);
			bvals[0] = 0f;
			for (var i = 0; i < order - 1; i++) bvals[i] -= bvals[i + 1];
		}
	}

	public class Curve_QuadraticBezier_Vector4 : Curve_Vector4
	{
		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentValue(float time)
		{
			var bvals = stackalloc float[3];
			Debug.Assert(values.Count == 3);
			Basis(time, bvals);
			return (bvals[0] * values[0] + bvals[1] * values[1] + bvals[2] * values[2]);
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentFirstDerivative(float time)
		{
			var bvals = stackalloc float[3];
			Debug.Assert(values.Count == 3);
			BasisFirstDerivative(time, bvals);
			var d = times[2] - times[0];
			return (bvals[0] * values[0] + bvals[1] * values[1] + bvals[2] * values[2]) / d;
		}

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentSecondDerivative(float time)
		{
			var bvals = stackalloc float[3];
			Debug.Assert(values.Count == 3);
			BasisSecondDerivative(time, bvals);
			var d = times[2] - times[0];
			return (bvals[0] * values[0] + bvals[1] * values[1] + bvals[2] * values[2]) / (d * d);
		}

		/// <summary>
		/// quadratic bezier basis functions
		/// </summary>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void Basis(float t, float* bvals)
		{
			var s1 = (float)(t - times[0]) / (times[2] - times[0]);
			var s2 = s1 * s1;
			bvals[0] = s2 - 2f * s1 + 1f;
			bvals[1] = -2f * s2 + 2f * s1;
			bvals[2] = s2;
		}

		/// <summary>
		/// first derivative of quadratic bezier basis functions
		/// </summary>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisFirstDerivative(float t, float* bvals)
		{
			var s1 = (float)(t - times[0]) / (times[2] - times[0]);
			bvals[0] = 2f * s1 - 2f;
			bvals[1] = -4f * s1 + 2f;
			bvals[2] = 2f * s1;
		}

		/// <summary>
		/// second derivative of quadratic bezier basis functions
		/// </summary>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisSecondDerivative(float t, float* bvals)
		{
			bvals[0] = 2f;
			bvals[1] = -4f;
			bvals[2] = 2f;
		}
	}

	public class Curve_CubicBezier_Vector4 : Curve_Vector4
	{
		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <returns></returns>
		public unsafe override Vector4 GetCurrentValue(float time)
		{
			var bvals = stackalloc float[4];
			Debug.Assert(values.Count == 4);
			Basis(time, bvals);
			return (bvals[0] * values[0] + bvals[1] * values[1] + bvals[2] * values[2] + bvals[3] * values[3]);
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentFirstDerivative(float time)
		{
			var bvals = stackalloc float[4];
			Debug.Assert(values.Count == 4);
			BasisFirstDerivative(time, bvals);
			var d = times[3] - times[0];
			return (bvals[0] * values[0] + bvals[1] * values[1] + bvals[2] * values[2] + bvals[3] * values[3]) / d;
		}

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentSecondDerivative(float time)
		{
			var bvals = stackalloc float[4];
			Debug.Assert(values.Count == 4);
			BasisSecondDerivative(time, bvals);
			var d = times[3] - times[0];
			return (bvals[0] * values[0] + bvals[1] * values[1] + bvals[2] * values[2] + bvals[3] * values[3]) / (d * d);
		}

		/// <summary>
		/// cubic bezier basis functions
		/// </summary>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void Basis(float t, float* bvals)
		{
			var s1 = (float)(t - times[0]) / (times[3] - times[0]);
			var s2 = s1 * s1;
			var s3 = s2 * s1;
			bvals[0] = -s3 + 3f * s2 - 3f * s1 + 1f;
			bvals[1] = 3f * s3 - 6f * s2 + 3f * s1;
			bvals[2] = -3f * s3 + 3f * s2;
			bvals[3] = s3;
		}

		/// <summary>
		/// first derivative of cubic bezier basis functions
		/// </summary>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisFirstDerivative(float t, float* bvals)
		{
			var s1 = (float)(t - times[0]) / (times[3] - times[0]);
			var s2 = s1 * s1;
			bvals[0] = -3f * s2 + 6f * s1 - 3f;
			bvals[1] = 9f * s2 - 12f * s1 + 3f;
			bvals[2] = -9f * s2 + 6f * s1;
			bvals[3] = 3f * s2;
		}

		/// <summary>
		/// second derivative of cubic bezier basis functions
		/// </summary>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisSecondDerivative(float t, float* bvals)
		{
			var s1 = (float)(t - times[0]) / (times[3] - times[0]);
			bvals[0] = -6f * s1 + 6f;
			bvals[1] = 18f * s1 - 12f;
			bvals[2] = -18f * s1 + 6f;
			bvals[3] = 6f * s1;
		}
	}

	public class Curve_Spline_Vector4 : Curve_Vector4
	{
		public enum BT { FREE, CLAMPED, CLOSED };

		protected BT boundaryType;
		protected float closeTime;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Curve_Spline_Vector4()
		{
			boundaryType = BT.FREE;
			closeTime = 0f;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool IsDone(float time)
			=> boundaryType != BT.CLOSED && time >= times[^1];

		public virtual BT BoundaryType
		{
			get => boundaryType;
			set { boundaryType = value; changed = true; }
		}

		public virtual float CloseTime
		{
			get => boundaryType == BT.CLOSED ? closeTime : 0f;
			set { closeTime = value; changed = true; }
		}

		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected new Vector4 ValueForIndex(int index)
		{
			var n = values.Count - 1;
			if (index < 0)
			{
				if (boundaryType == BT.CLOSED) return values[values.Count + index % values.Count];
				else return values[0] + index * (values[1] - values[0]);
			}
			else if (index > n)
			{
				if (boundaryType == BT.CLOSED) return values[index % values.Count];
				else return values[n] + (index - n) * (values[n] - values[n - 1]);
			}
			return values[index];
		}

		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected new float TimeForIndex(int index)
		{
			var n = times.Count - 1;
			if (index < 0)
			{
				if (boundaryType == BT.CLOSED) return (index / times.Count) * (times[n] + closeTime) - (times[n] + closeTime - times[times.Count + index % times.Count]);
				else return times[0] + index * (times[1] - times[0]);
			}
			else if (index > n)
			{
				if (boundaryType == BT.CLOSED) return (index / times.Count) * (times[n] + closeTime) + times[index % times.Count];
				else return times[n] + (index - n) * (times[n] - times[n - 1]);
			}
			return times[index];
		}

		/// <summary>
		/// return the clamped time based on the boundary Vector4
		/// </summary>
		/// <param name="t">The t.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected float ClampedTime(float t)
		{
			if (boundaryType == BT.CLAMPED)
			{
				if (t < times[0]) return times[0];
				else if (t >= times[^1]) return times[^1];
			}
			return t;
		}
	}

	public class Curve_NaturalCubicSpline_Vector4 : Curve_Spline_Vector4
	{
		protected List<Vector4> b = new();
		protected List<Vector4> c = new();
		protected List<Vector4> d = new();

		public override void Clear()
		{
			base.Clear(); values.Clear(); b.Clear(); c.Clear(); d.Clear();
		}

		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
		public override Vector4 GetCurrentValue(float time)
		{
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			var s = time - TimeForIndex(i);
			Setup();
			return (values[i] + s * (b[i] + s * (c[i] + s * d[i])));
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Vector4 GetCurrentFirstDerivative(float time)
		{
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			var s = time - TimeForIndex(i);
			Setup();
			return (b[i] + s * (2f * c[i] + 3f * s * d[i]));
		}

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Vector4 GetCurrentSecondDerivative(float time)
		{
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			var s = time - TimeForIndex(i);
			Setup();
			return 2f * c[i] + 6f * s * d[i];
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Setup()
		{
			if (changed)
			{
				switch (boundaryType)
				{
					case BT.FREE: SetupFree(); break;
					case BT.CLAMPED: SetupClamped(); break;
					case BT.CLOSED: SetupClosed(); break;
				}
				changed = false;
			}
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void SetupFree()
		{
			int i; float inv;

			var d0 = stackalloc float[values.Count - 1]; d0 = (float*)_alloca16(d0);
			var d1 = stackalloc float[values.Count - 1]; d1 = (float*)_alloca16(d1);
			var alpha = stackalloc Vector4[values.Count - 1]; alpha = (Vector4*)_alloca16(alpha);
			var beta = stackalloc float[values.Count]; beta = (float*)_alloca16(beta);
			var gamma = stackalloc float[values.Count - 1]; gamma = (float*)_alloca16(gamma);
			var delta = stackalloc Vector4[values.Count]; delta = (Vector4*)_alloca16(delta);

			for (i = 0; i < values.Count - 1; i++) d0[i] = times[i + 1] - times[i];
			for (i = 1; i < values.Count - 1; i++) d1[i] = times[i + 1] - times[i - 1];
			for (i = 1; i < values.Count - 1; i++)
			{
				Vector4 sum = 3f * (d0[i - 1] * values[i + 1] - d1[i] * values[i] + d0[i] * values[i - 1]);
				inv = 1f / (d0[i - 1] * d0[i]);
				alpha[i] = inv * sum;
			}

			beta[0] = 1f;
			gamma[0] = 0f;
			delta[0] = values[0] - values[0];

			for (i = 1; i < values.Count - 1; i++)
			{
				beta[i] = 2f * d1[i] - d0[i - 1] * gamma[i - 1];
				inv = 1f / beta[i];
				gamma[i] = inv * d0[i];
				delta[i] = inv * (alpha[i] - d0[i - 1] * delta[i - 1]);
			}
			beta[values.Count - 1] = 1f;
			delta[values.Count - 1] = values[0] - values[0];

			b.AssureSize(values.Count);
			c.AssureSize(values.Count);
			d.AssureSize(values.Count);

			c[values.Count - 1] = values[0] - values[0];

			for (i = values.Count - 2; i >= 0; i--)
			{
				c[i] = delta[i] - gamma[i] * c[i + 1];
				inv = 1f / d0[i];
				b[i] = inv * (values[i + 1] - values[i]) - (1f / 3f) * d0[i] * (c[i + 1] + 2f * c[i]);
				d[i] = (1f / 3f) * inv * (c[i + 1] - c[i]);
			}
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void SetupClamped()
		{
			int i; float inv;

			var d0 = stackalloc float[values.Count - 1]; d0 = (float*)_alloca16(d0);
			var d1 = stackalloc float[values.Count - 1]; d1 = (float*)_alloca16(d1);
			var alpha = stackalloc Vector4[values.Count - 1]; alpha = (Vector4*)_alloca16(alpha);
			var beta = stackalloc float[values.Count]; beta = (float*)_alloca16(beta);
			var gamma = stackalloc float[values.Count - 1]; gamma = (float*)_alloca16(gamma);
			var delta = stackalloc Vector4[values.Count]; delta = (Vector4*)_alloca16(delta);

			for (i = 0; i < values.Count - 1; i++) d0[i] = times[i + 1] - times[i];
			for (i = 1; i < values.Count - 1; i++) d1[i] = times[i + 1] - times[i - 1];

			inv = 1f / d0[0];
			alpha[0] = 3f * (inv - 1f) * (values[1] - values[0]);
			inv = 1f / d0[values.Count - 2];
			alpha[values.Count - 1] = 3f * (1f - inv) * (values[values.Count - 1] - values[values.Count - 2]);

			for (i = 1; i < values.Count - 1; i++)
			{
				Vector4 sum = 3f * (d0[i - 1] * values[i + 1] - d1[i] * values[i] + d0[i] * values[i - 1]);
				inv = 1f / (d0[i - 1] * d0[i]);
				alpha[i] = inv * sum;
			}

			beta[0] = 2f * d0[0];
			gamma[0] = 0.5f;
			inv = 1f / beta[0];
			delta[0] = inv * alpha[0];

			for (i = 1; i < values.Count - 1; i++)
			{
				beta[i] = 2f * d1[i] - d0[i - 1] * gamma[i - 1];
				inv = 1f / beta[i];
				gamma[i] = inv * d0[i];
				delta[i] = inv * (alpha[i] - d0[i - 1] * delta[i - 1]);
			}

			beta[values.Count - 1] = d0[values.Count - 2] * (2f - gamma[values.Count - 2]);
			inv = 1f / beta[values.Count - 1];
			delta[values.Count - 1] = inv * (alpha[values.Count - 1] - d0[values.Count - 2] * delta[values.Count - 2]);

			b.AssureSize(values.Count);
			c.AssureSize(values.Count);
			d.AssureSize(values.Count);

			c[values.Count - 1] = delta[values.Count - 1];

			for (i = values.Count - 2; i >= 0; i--)
			{
				c[i] = delta[i] - gamma[i] * c[i + 1];
				inv = 1f / d0[i];
				b[i] = inv * (values[i + 1] - values[i]) - (1f / 3f) * d0[i] * (c[i + 1] + 2f * c[i]);
				d[i] = (1f / 3f) * inv * (c[i + 1] - c[i]);
			}
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void SetupClosed()
		{
			int i, j; float c0, c1; MatrixX mat = new(); VectorX x = new();

			var d0 = stackalloc float[values.Count - 1]; d0 = (float*)_alloca16(d0);
			x.SetData(values.Count, VectorX.VECX_ALLOCA(values.Count));
			mat.SetData(values.Count, values.Count, MatrixX.MATX_ALLOCA(values.Count * values.Count));

			b.AssureSize(values.Count);
			c.AssureSize(values.Count);
			d.AssureSize(values.Count);

			for (i = 0; i < values.Count - 1; i++) d0[i] = times[i + 1] - times[i];

			// matrix of system
			mat[0][0] = 1f;
			mat[0][values.Count - 1] = -1f;
			for (i = 1; i <= values.Count - 2; i++)
			{
				mat[i][i - 1] = d0[i - 1];
				mat[i][i] = 2f * (d0[i - 1] + d0[i]);
				mat[i][i + 1] = d0[i];
			}
			mat[values.Count - 1][values.Count - 2] = d0[values.Count - 2];
			mat[values.Count - 1][0] = 2f * (d0[values.Count - 2] + d0[0]);
			mat[values.Count - 1][1] = d0[0];

			// right-hand side
			c[0].Zero();
			for (i = 1; i <= values.Count - 2; i++)
			{
				c0 = 1f / d0[i];
				c1 = 1f / d0[i - 1];
				c[i] = 3f * (c0 * (values[i + 1] - values[i]) - c1 * (values[i] - values[i - 1]));
			}
			c0 = 1f / d0[0];
			c1 = 1f / d0[values.Count - 2];
			c[values.Count - 1] = 3f * (c0 * (values[1] - values[0]) - c1 * (values[0] - values[values.Count - 2]));

			// solve system for each dimension
			mat.LU_Factor(null);
			for (i = 0; i < Vector4.Dimension; i++)
			{
				for (j = 0; j < values.Count; j++) x[j] = c[j][i];
				mat.LU_Solve(ref x, x, null);
				for (j = 0; j < values.Count; j++) c.Ptr()[j][i] = x[j];
			}

			for (i = 0; i < values.Count - 1; i++)
			{
				c0 = 1f / d0[i];
				b[i] = c0 * (values[i + 1] - values[i]) - (1f / 3f) * (c[i + 1] + 2f * c[i]) * d0[i];
				d[i] = (1f / 3f) * c0 * (c[i + 1] - c[i]);
			}
		}
	}

	public class Curve_CatmullRomSpline_Vector4 : Curve_Spline_Vector4
	{
		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentValue(float time)
		{
			if (times.Count == 1) return values[0];

			var bvals = stackalloc float[4];
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			Basis(i - 1, clampedTime, bvals);
			var v = values[0] - values[0];
			for (var j = 0; j < 4; j++)
			{
				var k = i + j - 2;
				v += bvals[j] * ValueForIndex(k);
			}
			return v;
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentFirstDerivative(float time)
		{
			if (times.Count == 1) return values[0] - values[0];

			var bvals = stackalloc float[4];
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			BasisFirstDerivative(i - 1, clampedTime, bvals);
			var v = values[0] - values[0];
			for (var j = 0; j < 4; j++)
			{
				var k = i + j - 2;
				v += bvals[j] * ValueForIndex(k);
			}
			var d = TimeForIndex(i) - TimeForIndex(i - 1);
			return v / d;
		}

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentSecondDerivative(float time)
		{
			if (times.Count == 1) return values[0] - values[0];

			var bvals = stackalloc float[4];
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			BasisSecondDerivative(i - 1, clampedTime, bvals);
			var v = values[0] - values[0];
			int j, k;
			for (j = 0; j < 4; j++)
			{
				k = i + j - 2;
				v += bvals[j] * ValueForIndex(k);
			}
			var d = TimeForIndex(i) - TimeForIndex(i - 1);
			return v / (d * d);
		}

		/// <summary>
		/// spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void Basis(int index, float t, float* bvals)
		{
			var s = (float)(t - TimeForIndex(index)) / (TimeForIndex(index + 1) - TimeForIndex(index));
			bvals[0] = ((-s + 2f) * s - 1f) * s * 0.5f;             // -0.5f s * s * s + s * s - 0.5f * s
			bvals[1] = (((3f * s - 5f) * s) * s + 2f) * 0.5f; // 1.5f * s * s * s - 2.5f * s * s + 1f
			bvals[2] = ((-3f * s + 4f) * s + 1f) * s * 0.5f;      // -1.5f * s * s * s - 2f * s * s + 0.5f s
			bvals[3] = ((s - 1f) * s * s) * 0.5f;                     // 0.5f * s * s * s - 0.5f * s * s
		}

		/// <summary>
		/// first derivative of spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisFirstDerivative(int index, float t, float* bvals)
		{
			var s = (float)(t - TimeForIndex(index)) / (TimeForIndex(index + 1) - TimeForIndex(index));
			bvals[0] = (-1.5f * s + 2f) * s - 0.5f;                       // -1.5f * s * s + 2f * s - 0.5f
			bvals[1] = (4.5f * s - 5f) * s;                               // 4.5f * s * s - 5f * s
			bvals[2] = (float)((-4.5 * s + 4f) * s + 0.5f);                        // -4.5 * s * s + 4f * s + 0.5f
			bvals[3] = 1.5f * s * s - s;                                    // 1.5f * s * s - s
		}

		/// <summary>
		/// second derivative of spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisSecondDerivative(int index, float t, float* bvals)
		{
			var s = (float)(t - TimeForIndex(index)) / (TimeForIndex(index + 1) - TimeForIndex(index));
			bvals[0] = -3f * s + 2f;
			bvals[1] = 9f * s - 5f;
			bvals[2] = -9f * s + 4f;
			bvals[3] = 3f * s - 1f;
		}
	}

	public class Curve_KochanekBartelsSpline_Vector4 : Curve_Spline_Vector4
	{
		protected List<float> tension = new();
		protected List<float> continuity = new();
		protected List<float> bias = new();

		/// <summary>
		/// add a timed/value pair to the spline
		/// returns the index to the inserted pair
		/// </summary>
		/// <param name="time">The time.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int AddValue(float time, in Vector4 value)
		{
			var i = IndexForTime(time);
			times.Insert(i, time);
			values.Insert(i, value);
			tension.Insert(i, 0f);
			continuity.Insert(i, 0f);
			bias.Insert(i, 0f);
			return i;
		}

		/// <summary>
		/// add a timed/value pair to the spline
		/// returns the index to the inserted pair
		/// </summary>
		/// <param name="time">The time.</param>
		/// <param name="value">The value.</param>
		/// <param name="tension">The tension.</param>
		/// <param name="continuity">The continuity.</param>
		/// <param name="bias">The bias.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual int AddValue(float time, in Vector4 value, float tension, float continuity, float bias)
		{
			var i = IndexForTime(time);
			times.Insert(i, time);
			values.Insert(i, value);
			this.tension.Insert(i, tension);
			this.continuity.Insert(i, continuity);
			this.bias.Insert(i, bias);
			return i;
		}

		public override void RemoveIndex(int index)
		{
			values.RemoveAt(index); times.RemoveAt(index); tension.RemoveAt(index); continuity.RemoveAt(index); bias.RemoveAt(index);
		}

		public override void Clear()
		{
			values.Clear(); times.Clear(); tension.Clear(); continuity.Clear(); bias.Clear(); currentIndex = -1;
		}

		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentValue(float time)
		{
			if (times.Count == 1) return values[0];

			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			TangentsForIndex(i - 1, out var t0, out var t1);
			var bvals = stackalloc float[4];
			Basis(i - 1, clampedTime, bvals);
			var v = bvals[0] * ValueForIndex(i - 1);
			v += bvals[1] * ValueForIndex(i);
			v += bvals[2] * t0;
			v += bvals[3] * t1;
			return v;
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentFirstDerivative(float time)
		{
			if (times.Count == 1) return values[0] - values[0];

			var bvals = stackalloc float[4];
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			TangentsForIndex(i - 1, out var t0, out var t1);
			BasisFirstDerivative(i - 1, clampedTime, bvals);
			var v = bvals[0] * ValueForIndex(i - 1);
			v += bvals[1] * ValueForIndex(i);
			v += bvals[2] * t0;
			v += bvals[3] * t1;
			var d = (TimeForIndex(i) - TimeForIndex(i - 1));
			return v / d;
		}

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentSecondDerivative(float time)
		{
			if (times.Count == 1) return values[0] - values[0];

			var bvals = stackalloc float[4];
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			TangentsForIndex(i - 1, out var t0, out var t1);
			BasisSecondDerivative(i - 1, clampedTime, bvals);
			var v = bvals[0] * ValueForIndex(i - 1);
			v += bvals[1] * ValueForIndex(i);
			v += bvals[2] * t0;
			v += bvals[3] * t1;
			var d = (TimeForIndex(i) - TimeForIndex(i - 1));
			return v / (d * d);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void TangentsForIndex(int index, out Vector4 t0, out Vector4 t1)
		{
			var delta = ValueForIndex(index + 1) - ValueForIndex(index);
			var dt = TimeForIndex(index + 1) - TimeForIndex(index);

			var omt = 1f - tension[index];
			var omc = 1f - continuity[index];
			var opc = 1f + continuity[index];
			var omb = 1f - bias[index];
			var opb = 1f + bias[index];
			var adj = 2f * dt / (TimeForIndex(index + 1) - TimeForIndex(index - 1));
			var s0 = 0.5f * adj * omt * opc * opb;
			var s1 = 0.5f * adj * omt * omc * omb;

			// outgoing tangent at first point
			t0 = s1 * delta + s0 * (ValueForIndex(index) - ValueForIndex(index - 1));

			omt = 1f - tension[index + 1];
			omc = 1f - continuity[index + 1];
			opc = 1f + continuity[index + 1];
			omb = 1f - bias[index + 1];
			opb = 1f + bias[index + 1];
			adj = 2f * dt / (TimeForIndex(index + 2) - TimeForIndex(index));
			s0 = 0.5f * adj * omt * omc * opb;
			s1 = 0.5f * adj * omt * opc * omb;

			// incoming tangent at second point
			t1 = s1 * (ValueForIndex(index + 2) - ValueForIndex(index + 1)) + s0 * delta;
		}

		/// <summary>
		/// spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void Basis(int index, float t, float* bvals)
		{
			var s = (float)(t - TimeForIndex(index)) / (TimeForIndex(index + 1) - TimeForIndex(index));
			bvals[0] = ((2f * s - 3f) * s) * s + 1f;              // 2f * s * s * s - 3f * s * s + 1f
			bvals[1] = ((-2f * s + 3f) * s) * s;                    // -2f * s * s * s + 3f * s * s
			bvals[2] = ((s - 2f) * s) * s + s;                        // s * s * s - 2f * s * s + s
			bvals[3] = ((s - 1f) * s) * s;                            // s * s * s - s * s
		}

		/// <summary>
		/// first derivative of spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisFirstDerivative(int index, float t, float* bvals)
		{
			var s = (float)(t - TimeForIndex(index)) / (TimeForIndex(index + 1) - TimeForIndex(index));
			bvals[0] = (6f * s - 6f) * s;                               // 6f * s * s - 6f * s
			bvals[1] = (-6f * s + 6f) * s;                          // -6f * s * s + 6f * s
			bvals[2] = (3f * s - 4f) * s + 1f;                        // 3f * s * s - 4f * s + 1f
			bvals[3] = (3f * s - 2f) * s;                               // 3f * s * s - 2f * s
		}

		/// <summary>
		/// second derivative of spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisSecondDerivative(int index, float t, float* bvals)
		{
			var s = (float)(t - TimeForIndex(index)) / (TimeForIndex(index + 1) - TimeForIndex(index));
			bvals[0] = 12f * s - 6f;
			bvals[1] = -12f * s + 6f;
			bvals[2] = 6f * s - 4f;
			bvals[3] = 6f * s - 2f;
		}
	}

	public class Curve_BSpline_Vector4 : Curve_Spline_Vector4
	{
		protected int order;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Curve_BSpline_Vector4()
			=> order = 4;   // default to cubic

		public virtual int Order
		{
			get => order;
			set { Debug.Assert(value > 0 && value < 10); order = value; }
		}

		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Vector4 GetCurrentValue(float time)
		{
			if (times.Count == 1) return values[0];

			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			var v = values[0] - values[0];
			for (var j = 0; j < order; j++)
			{
				var k = i + j - (order >> 1);
				v += Basis(k - 2, order, clampedTime) * ValueForIndex(k);
			}
			return v;
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Vector4 GetCurrentFirstDerivative(float time)
		{
			if (times.Count == 1) return values[0];

			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			var v = values[0] - values[0];
			for (var j = 0; j < order; j++)
			{
				var k = i + j - (order >> 1);
				v += BasisFirstDerivative(k - 2, order, clampedTime) * ValueForIndex(k);
			}
			return v;
		}

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Vector4 GetCurrentSecondDerivative(float time)
		{
			if (times.Count == 1) return values[0];

			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			var v = values[0] - values[0];
			for (var j = 0; j < order; j++)
			{
				var k = i + j - (order >> 1);
				v += BasisSecondDerivative(k - 2, order, clampedTime) * ValueForIndex(k);
			}
			return v;
		}


		/// <summary>
		/// spline basis function
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="order">The order.</param>
		/// <param name="t">The t.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected float Basis(int index, int order, float t)
		{
			if (order <= 1) return TimeForIndex(index) < t && t <= TimeForIndex(index + 1) ? 1f : 0f;

			var sum = 0f;
			var d1 = TimeForIndex(index + order - 1) - TimeForIndex(index);
			if (d1 != 0f) sum += (float)(t - TimeForIndex(index)) * Basis(index, order - 1, t) / d1;

			var d2 = TimeForIndex(index + order) - TimeForIndex(index + 1);
			if (d2 != 0f) sum += (float)(TimeForIndex(index + order) - t) * Basis(index + 1, order - 1, t) / d2;
			return sum;
		}

		/// <summary>
		/// first derivative of spline basis function
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="order">The order.</param>
		/// <param name="t">The t.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected float BasisFirstDerivative(int index, int order, float t)
			=> (Basis(index, order - 1, t) - Basis(index + 1, order - 1, t)) *
				(order - 1) / (TimeForIndex(index + (order - 1) - 2) - TimeForIndex(index - 2));

		/// <summary>
		/// second derivative of spline basis function
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="order">The order.</param>
		/// <param name="t">The t.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected float BasisSecondDerivative(int index, int order, float t)
			=> (BasisFirstDerivative(index, order - 1, t) - BasisFirstDerivative(index + 1, order - 1, t)) *
				(order - 1) / (TimeForIndex(index + (order - 1) - 2) - TimeForIndex(index - 2));
	}

	public class Curve_UniformCubicBSpline_Vector4 : Curve_BSpline_Vector4
	{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Curve_UniformCubicBSpline_Vector4()
			=> order = 4;  // always cubic

		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentValue(float time)
		{
			if (times.Count == 1) return values[0];

			var bvals = stackalloc float[4];
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			Basis(i - 1, clampedTime, bvals);
			var v = values[0] - values[0];
			for (var j = 0; j < 4; j++)
			{
				var k = i + j - 2;
				v += bvals[j] * ValueForIndex(k);
			}
			return v;
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentFirstDerivative(float time)
		{
			if (times.Count == 1) return values[0] - values[0];

			var bvals = stackalloc float[4];
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			BasisFirstDerivative(i - 1, clampedTime, bvals);
			var v = values[0] - values[0];
			for (var j = 0; j < 4; j++)
			{
				var k = i + j - 2;
				v += bvals[j] * ValueForIndex(k);
			}
			var d = TimeForIndex(i) - TimeForIndex(i - 1);
			return v / d;
		}

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentSecondDerivative(float time)
		{
			if (times.Count == 1) return values[0] - values[0];

			var bvals = stackalloc float[4];
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			BasisSecondDerivative(i - 1, clampedTime, bvals);
			var v = values[0] - values[0];
			for (var j = 0; j < 4; j++)
			{
				var k = i + j - 2;
				v += bvals[j] * ValueForIndex(k);
			}
			var d = TimeForIndex(i) - TimeForIndex(i - 1);
			return v / (d * d);
		}

		/// <summary>
		/// spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void Basis(int index, float t, float* bvals)
		{
			var s = (float)(t - TimeForIndex(index)) / (TimeForIndex(index + 1) - TimeForIndex(index));
			bvals[0] = (((-s + 3f) * s - 3f) * s + 1f) * (1f / 6f);
			bvals[1] = (((3f * s - 6f) * s) * s + 4f) * (1f / 6f);
			bvals[2] = (((-3f * s + 3f) * s + 3f) * s + 1f) * (1f / 6f);
			bvals[3] = (s * s * s) * (1f / 6f);
		}

		/// <summary>
		/// first derivative of spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisFirstDerivative(int index, float t, float* bvals)
		{
			var s = (float)(t - TimeForIndex(index)) / (TimeForIndex(index + 1) - TimeForIndex(index));
			bvals[0] = -0.5f * s * s + s - 0.5f;
			bvals[1] = 1.5f * s * s - 2f * s;
			bvals[2] = -1.5f * s * s + s + 0.5f;
			bvals[3] = 0.5f * s * s;
		}

		/// <summary>
		/// second derivative of spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisSecondDerivative(int index, float t, float* bvals)
		{
			var s = (float)(t - TimeForIndex(index)) / (TimeForIndex(index + 1) - TimeForIndex(index));
			bvals[0] = -s + 1f;
			bvals[1] = 3f * s - 2f;
			bvals[2] = -3f * s + 1f;
			bvals[3] = s;
		}
	}

	public class Curve_NonUniformBSpline_Vector4 : Curve_BSpline_Vector4
	{
		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentValue(float time)
		{
			if (times.Count == 1) return values[0];

			var bvals = stackalloc float[order]; bvals = (float*)_alloca16(bvals);
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			Basis(i - 1, order, clampedTime, bvals);
			var v = values[0] - values[0];
			for (var j = 0; j < order; j++)
			{
				var k = i + j - (order >> 1);
				v += bvals[j] * ValueForIndex(k);
			}
			return v;
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentFirstDerivative(float time)
		{
			if (times.Count == 1) return values[0] - values[0];

			var bvals = stackalloc float[order]; bvals = (float*)_alloca16(bvals);
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			BasisFirstDerivative(i - 1, order, clampedTime, bvals);
			var v = values[0] - values[0];
			for (var j = 0; j < order; j++)
			{
				var k = i + j - (order >> 1);
				v += bvals[j] * ValueForIndex(k);
			}
			return v;
		}

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentSecondDerivative(float time)
		{
			if (times.Count == 1) return values[0] - values[0];

			var bvals = stackalloc float[order]; bvals = (float*)_alloca16(bvals);
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			BasisSecondDerivative(i - 1, order, clampedTime, bvals);
			var v = values[0] - values[0];
			for (var j = 0; j < order; j++)
			{
				var k = i + j - (order >> 1);
				v += bvals[j] * ValueForIndex(k);
			}
			return v;
		}

		/// <summary>
		/// spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="order">The order.</param>
		/// <param name="t">The t.</param>
		/// <param name="bvals">The bvals.</param>
		/// <returns></returns>
		protected unsafe void Basis(int index, int order, float t, float* bvals)
		{
			bvals[order - 1] = 1f;
			for (var r = 2; r <= order; r++)
			{
				var i = index - r + 1;
				bvals[order - r] = 0f;
				for (var s = order - r + 1; s < order; s++)
				{
					i++;
					var omega = (float)(t - TimeForIndex(i)) / (TimeForIndex(i + r - 1) - TimeForIndex(i));
					bvals[s - 1] += (1f - omega) * bvals[s];
					bvals[s] *= omega;
				}
			}
		}

		/// <summary>
		/// first derivative of spline basis functions
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="order">The order.</param>
		/// <param name="">The .</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisFirstDerivative(int index, int order, float t, float* bvals)
		{
			Basis(index, order - 1, t, bvals + 1);
			bvals[0] = 0f;
			int i;
			for (i = 0; i < order - 1; i++)
			{
				bvals[i] -= bvals[i + 1];
				bvals[i] *= (order - 1) / (TimeForIndex(index + i + (order - 1) - 2) - TimeForIndex(index + i - 2));
			}
			bvals[i] *= (order - 1) / (TimeForIndex(index + i + (order - 1) - 2) - TimeForIndex(index + i - 2));
		}

		/// <summary>
		/// second derivative of spline basis functions
		/// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void BasisSecondDerivative(int index, int order, float t, float* bvals)
		{
			BasisFirstDerivative(index, order - 1, t, bvals + 1);
			bvals[0] = 0f;
			int i;
			for (i = 0; i < order - 1; i++)
			{
				bvals[i] -= bvals[i + 1];
				bvals[i] *= (order - 1) / (TimeForIndex(index + i + (order - 1) - 2) - TimeForIndex(index + i - 2));
			}
			bvals[i] *= (order - 1) / (TimeForIndex(index + i + (order - 1) - 2) - TimeForIndex(index + i - 2));
		}
	}

	public class Curve_NURBS_Vector4 : Curve_NonUniformBSpline_Vector4
	{
		protected List<float> weights = new();
		
		/// <summary>
		/// add a timed/value pair to the spline
		/// returns the index to the inserted pair
		/// </summary>
		/// <param name="time">The time.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int AddValue(float time, in Vector4 value)
		{
			var i = IndexForTime(time);
			times.Insert(i, time);
			values.Insert(i, value);
			weights.Insert(i, 1f);
			return i;
		}

		/// <summary>
		/// add a timed/value pair to the spline
		/// returns the index to the inserted pair
		/// </summary>
		/// <param name="time">The time.</param>
		/// <param name="value">The value.</param>
		/// <param name="weight">The weight.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual int AddValue(float time, in Vector4 value, float weight)
		{
			var i = IndexForTime(time);
			times.Insert(i, time);
			values.Insert(i, value);
			weights.Insert(i, weight);
			return i;
		}

		public override void RemoveIndex(int index)
		{
			values.RemoveAt(index); times.RemoveAt(index); weights.RemoveAt(index);
		}

		public override void Clear()
		{
			values.Clear(); times.Clear(); weights.Clear(); currentIndex = -1;
		}

		/// <summary>
		/// get the value for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentValue(float time)
		{
			if (times.Count == 1) return values[0];

			var bvals = stackalloc float[order]; bvals = (float*)_alloca16(bvals);
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			Basis(i - 1, order, clampedTime, bvals);
			var v = values[0] - values[0];
			var w = 0f;
			for (var j = 0; j < order; j++)
			{
				var k = i + j - (order >> 1);
				var b = bvals[j] * WeightForIndex(k);
				w += b;
				v += b * ValueForIndex(k);
			}
			return v / w;
		}

		/// <summary>
		/// get the first derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentFirstDerivative(float time)
		{
			if (times.Count == 1) return values[0];

			var bvals = stackalloc float[order]; bvals = (float*)_alloca16(bvals);
			var d1vals = stackalloc float[order]; d1vals = (float*)_alloca16(d1vals);
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			Basis(i - 1, order, clampedTime, bvals);
			BasisFirstDerivative(i - 1, order, clampedTime, d1vals);
			Vector4 vb, vd1; vb = vd1 = values[0] - values[0];
			float wb, wd1; wb = wd1 = 0f;
			for (var j = 0; j < order; j++)
			{
				var k = i + j - (order >> 1);
				var w = WeightForIndex(k);
				var b = bvals[j] * w;
				var d1 = d1vals[j] * w;
				wb += b;
				wd1 += d1;
				var v = ValueForIndex(k);
				vb += b * v;
				vd1 += d1 * v;
			}
			return (wb * vd1 - vb * wd1) / (wb * wb);
		}

		/// <summary>
		/// get the second derivative for the given time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe override Vector4 GetCurrentSecondDerivative(float time)
		{
			if (times.Count == 1) return values[0];

			var bvals = stackalloc float[order]; bvals = (float*)_alloca16(bvals);
			var d1vals = stackalloc float[order]; d1vals = (float*)_alloca16(d1vals);
			var d2vals = stackalloc float[order]; d2vals = (float*)_alloca16(d2vals);
			var clampedTime = ClampedTime(time);
			var i = IndexForTime(clampedTime);
			Basis(i - 1, order, clampedTime, bvals);
			BasisFirstDerivative(i - 1, order, clampedTime, d1vals);
			BasisSecondDerivative(i - 1, order, clampedTime, d2vals);
			Vector4 vb, vd1, vd2; vb = vd1 = vd2 = values[0] - values[0];
			float wb, wd1, wd2; wb = wd1 = wd2 = 0f;
			for (var j = 0; j < order; j++)
			{
				var k = i + j - (order >> 1);
				var w = WeightForIndex(k);
				var b = bvals[j] * w;
				var d1 = d1vals[j] * w;
				var d2 = d2vals[j] * w;
				wb += b;
				wd1 += d1;
				wd2 += d2;
				var v = ValueForIndex(k);
				vb += b * v;
				vd1 += d1 * v;
				vd2 += d2 * v;
			}
			return ((wb * wb) * (wb * vd2 - vb * wd2) - (wb * vd1 - vb * wd1) * 2f * wb * wd1) / (wb * wb * wb * wb);
		}

		/// <summary>
		/// get the weight for the given index
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		float WeightForIndex(int index)
		{
			var n = weights.Count - 1;
			if (index < 0)
			{
				if (boundaryType == BT.CLOSED) return weights[weights.Count + index % weights.Count];
				else return weights[0] + index * (weights[1] - weights[0]);
			}
			else if (index > n)
			{
				if (boundaryType == BT.CLOSED) return weights[index % weights.Count];
				else return weights[n] + (index - n) * (weights[n] - weights[n - 1]);
			}
			return weights[index];
		}
	}
}