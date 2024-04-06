using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    public unsafe class Polynomial
    {
        int degree;
        int allocated;
        float[] coefficient;
        const float EPSILON = 1e-6f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polynomial(in Polynomial p)
        {
            Resize(p.degree, false);
            for (var i = 0; i <= degree; i++) coefficient[i] = p.coefficient[i];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polynomial()
        {
            degree = -1;
            allocated = 0;
            coefficient = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polynomial(int d)
        {
            degree = -1;
            allocated = 0;
            coefficient = null;
            Resize(d, false);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polynomial(float a, float b)
        {
            degree = -1;
            allocated = 0;
            coefficient = null;
            Resize(1, false);
            coefficient[0] = b;
            coefficient[1] = a;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polynomial(float a, float b, float c)
        {
            degree = -1;
            allocated = 0;
            coefficient = null;
            Resize(2, false);
            coefficient[0] = c;
            coefficient[1] = b;
            coefficient[2] = a;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polynomial(float a, float b, float c, float d)
        {
            degree = -1;
            allocated = 0;
            coefficient = null;
            Resize(3, false);
            coefficient[0] = d;
            coefficient[1] = c;
            coefficient[2] = b;
            coefficient[3] = a;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polynomial(float a, float b, float c, float d, float e)
        {
            degree = -1;
            allocated = 0;
            coefficient = null;
            Resize(4, false);
            coefficient[0] = e;
            coefficient[1] = d;
            coefficient[2] = c;
            coefficient[3] = b;
            coefficient[4] = a;
        }

        public ref float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(index >= 0 && index <= degree);
                return ref coefficient[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Polynomial operator -(in Polynomial _)
        {
            Polynomial n = new(_);
            for (var i = 0; i <= _.degree; i++) n.coefficient[i] = -n.coefficient[i];
            return n;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Polynomial operator +(in Polynomial _, in Polynomial p)
        {
            int i; Polynomial n = new();

            if (_.degree > p.degree)
            {
                n.Resize(_.degree, false);
                for (i = 0; i <= p.degree; i++) n.coefficient[i] = _.coefficient[i] + p.coefficient[i];
                for (; i <= _.degree; i++) n.coefficient[i] = _.coefficient[i];
                n.degree = _.degree;
            }
            else if (p.degree > _.degree)
            {
                n.Resize(p.degree, false);
                for (i = 0; i <= _.degree; i++) n.coefficient[i] = _.coefficient[i] + p.coefficient[i];
                for (; i <= p.degree; i++) n.coefficient[i] = p.coefficient[i];
                n.degree = p.degree;
            }
            else
            {
                n.Resize(_.degree, false);
                n.degree = 0;
                for (i = 0; i <= _.degree; i++)
                {
                    n.coefficient[i] = _.coefficient[i] + p.coefficient[i];
                    if (n.coefficient[i] != 0f) n.degree = i;
                }
            }
            return n;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Polynomial operator -(in Polynomial _, in Polynomial p)
        {
            int i; Polynomial n = new();

            if (_.degree > p.degree)
            {
                n.Resize(_.degree, false);
                for (i = 0; i <= p.degree; i++) n.coefficient[i] = _.coefficient[i] - p.coefficient[i];
                for (; i <= _.degree; i++) n.coefficient[i] = _.coefficient[i];
                n.degree = _.degree;
            }
            else if (p.degree >= _.degree)
            {
                n.Resize(p.degree, false);
                for (i = 0; i <= _.degree; i++) n.coefficient[i] = _.coefficient[i] - p.coefficient[i];
                for (; i <= p.degree; i++) n.coefficient[i] = -p.coefficient[i];
                n.degree = p.degree;
            }
            else
            {
                n.Resize(_.degree, false);
                n.degree = 0;
                for (i = 0; i <= _.degree; i++)
                {
                    n.coefficient[i] = _.coefficient[i] - p.coefficient[i];
                    if (n.coefficient[i] != 0f) n.degree = i;
                }
            }
            return n;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Polynomial operator *(in Polynomial _, float s)
        {
            Polynomial n = new();

            if (s == 0f) n.degree = 0;
            else
            {
                n.Resize(_.degree, false);
                for (var i = 0; i <= _.degree; i++) n.coefficient[i] = _.coefficient[i] * s;
            }
            return n;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Polynomial operator /(in Polynomial _, float s)
        {
            float invs; Polynomial n = new();

            Debug.Assert(s != 0f);
            n.Resize(_.degree, false);
            invs = 1f / s;
            for (var i = 0; i <= _.degree; i++) n.coefficient[i] = _.coefficient[i] * invs;
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Polynomial p)                      // exact compare, no epsilon
        {
            if (degree != p.degree)
                return false;
            for (var i = 0; i <= degree; i++) if (coefficient[i] != p.coefficient[i]) return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Polynomial p, float epsilon) // compare with epsilon
        {
            if (degree != p.degree)
                return false;
            for (var i = 0; i <= degree; i++) if (MathX.Fabs(coefficient[i] - p.coefficient[i]) > epsilon) return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Polynomial _, in Polynomial p)                   // exact compare, no epsilon
            => _.Compare(p);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Polynomial _, in Polynomial p)                   // exact compare, no epsilon
            => !_.Compare(p);
        public override bool Equals(object obj)
            => obj is Polynomial q && Compare(q);
        public override int GetHashCode()
            => coefficient[0].GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
            => degree = 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero(int d)
        {
            Resize(d, false);
            for (var i = 0; i <= degree; i++) coefficient[i] = 0f;
        }

        public int Dimension                                   // get the degree of the polynomial
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => degree;
        }

        public int Degree                                  // get the degree of the polynomial
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => degree;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetValue(float x)                         // evaluate the polynomial with the given real value
        {
            var y = coefficient[0];
            var z = x;
            for (var i = 1; i <= degree; i++) { y += coefficient[i] * z; z *= x; }
            return y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Complex GetValue(in Complex x)                     // evaluate the polynomial with the given complex value
        {
            var y = new Complex(coefficient[0], 0f);
            var z = x;
            for (var i = 1; i <= degree; i++) { y += coefficient[i] * z; z *= x; }
            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polynomial GetDerivative()                                // get the first derivative of the polynomial
        {
            Polynomial n = new();

            if (degree == 0) return n;

            n.Resize(degree - 1, false);
            for (var i = 1; i <= degree; i++) n.coefficient[i - 1] = i * coefficient[i];
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polynomial GetAntiDerivative()                            // get the anti derivative of the polynomial
        {
            Polynomial n = new();

            if (degree == 0) return n;

            n.Resize(degree + 1, false);
            n.coefficient[0] = 0f;
            for (var i = 0; i <= degree; i++) n.coefficient[i + 1] = coefficient[i] / (i + 1);
            return n;
        }

        public int GetRoots(Complex* roots)                          // get all roots
        {
            int i, j; Complex x = new(), b, c;

            var coef = stackalloc Complex[degree + 1 + Complex.ALLOC16]; coef = (Complex*)_alloca16(coef);
            for (i = 0; i <= degree; i++) coef[i].Set(coefficient[i], 0f);

            for (i = degree - 1; i >= 0; i--)
            {
                x.Zero();
                Laguer(coef, i + 1, ref x);
                if (MathX.Fabs(x.i) < 2f * EPSILON * MathX.Fabs(x.r)) x.i = 0f;
                roots[i] = x;
                b = coef[i + 1];
                for (j = i; j >= 0; j--)
                {
                    c = coef[j];
                    coef[j] = b;
                    b = x * b + c;
                }
            }

            for (i = 0; i <= degree; i++) coef[i].Set(coefficient[i], 0f);
            for (i = 0; i < degree; i++) Laguer(coef, degree, ref roots[i]);

            for (i = 1; i < degree; i++)
            {
                x = roots[i];
                for (j = i - 1; j >= 0; j--)
                {
                    if (roots[j].r <= x.r) break;
                    roots[j + 1] = roots[j];
                }
                roots[j + 1] = x;
            }

            return degree;
        }
        public int GetRoots(float* roots)                             // get the real roots
        {
            switch (degree)
            {
                case 0: roots = default; return 0;
                case 1: return GetRoots1(coefficient[1], coefficient[0], roots);
                case 2: return GetRoots2(coefficient[2], coefficient[1], coefficient[0], roots);
                case 3: return GetRoots3(coefficient[3], coefficient[2], coefficient[1], coefficient[0], roots);
                case 4: return GetRoots4(coefficient[4], coefficient[3], coefficient[2], coefficient[1], coefficient[0], roots);
                default:
                    // The Abel-Ruffini theorem states that there is no general solution in radicals to polynomial equations of degree five or higher.
                    // A polynomial equation can be solved by radicals if and only if its Galois group is a solvable group.
                    var complexRoots = stackalloc Complex[degree + Complex.ALLOC16]; complexRoots = (Complex*)_alloca16(complexRoots);
                    GetRoots(complexRoots);

                    int i, num;
                    for (num = i = 0; i < degree; i++) if (complexRoots[i].i == 0f) { roots[i] = complexRoots[i].r; num++; }
                    return num;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRoots1(float a, float b, float* roots)
        {
            Debug.Assert(a != 0f);
            roots[0] = -b / a;
            return 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRoots2(float a, float b, float c, float* roots)
        {
            float inva, ds;

            if (a != 1f) { Debug.Assert(a != 0f); inva = 1f / a; c *= inva; b *= inva; }
            ds = b * b - 4f * c;
            if (ds < 0f) return 0;
            else if (ds > 0f) { ds = MathX.Sqrt(ds); roots[0] = 0.5f * (-b - ds); roots[1] = 0.5f * (-b + ds); return 2; }
            else { roots[0] = 0.5f * -b; return 1; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRoots3(float a, float b, float c, float d, float* roots)
        {
            float inva, f, g, halfg, ofs, ds, dist, angle, cs, ss, t;

            if (a != 1f) { Debug.Assert(a != 0f); inva = 1f / a; d *= inva; c *= inva; b *= inva; }

            f = (1f / 3f) * (3f * c - b * b);
            g = (1f / 27f) * (2f * b * b * b - 9f * c * b + 27f * d);
            halfg = 0.5f * g;
            ofs = (1f / 3f) * b;
            ds = 0.25f * g * g + (1f / 27f) * f * f * f;

            if (ds < 0f)
            {
                dist = MathX.Sqrt((-1f / 3f) * f);
                angle = (1f / 3f) * MathX.ATan(MathX.Sqrt(-ds), -halfg);
                cs = MathX.Cos(angle);
                ss = MathX.Sin(angle);
                roots[0] = 2f * dist * cs - ofs;
                roots[1] = -dist * (cs + MathX.SQRT_THREE * ss) - ofs;
                roots[2] = -dist * (cs - MathX.SQRT_THREE * ss) - ofs;
                return 3;
            }
            else if (ds > 0f)
            {
                ds = MathX.Sqrt(ds);
                t = -halfg + ds;
                if (t >= 0f) roots[0] = MathX.Pow(t, (1f / 3f));
                else roots[0] = -MathX.Pow(-t, (1f / 3f));
                t = -halfg - ds;
                if (t >= 0f) roots[0] += MathX.Pow(t, (1f / 3f));
                else roots[0] -= MathX.Pow(-t, (1f / 3f));
                roots[0] -= ofs;
                return 1;
            }
            else
            {
                t = halfg >= 0f ? -MathX.Pow(halfg, 1f / 3f) : MathX.Pow(-halfg, 1f / 3f);
                roots[0] = 2f * t - ofs;
                roots[1] = -t - ofs;
                roots[2] = roots[1];
                return 3;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRoots4(float a, float b, float c, float d, float e, float* roots)
        {
            int count; float inva, y, ds, r, s1, s2, t1, t2, tp, tm;
            var roots3 = stackalloc float[3];

            if (a != 1f) { Debug.Assert(a != 0f); inva = 1f / a; e *= inva; d *= inva; c *= inva; b *= inva; }

            count = 0;

            GetRoots3(1f, -c, b * d - 4f * e, -b * b * e + 4f * c * e - d * d, roots3);
            y = roots3[0];
            ds = 0.25f * b * b - c + y;

            if (ds < 0f) return 0;
            else if (ds > 0f)
            {
                r = MathX.Sqrt(ds);
                t1 = 0.75f * b * b - r * r - 2f * c;
                t2 = (4f * b * c - 8f * d - b * b * b) / (4f * r);
                tp = t1 + t2;
                tm = t1 - t2;

                if (tp >= 0f)
                {
                    s1 = MathX.Sqrt(tp);
                    roots[count++] = -0.25f * b + 0.5f * (r + s1);
                    roots[count++] = -0.25f * b + 0.5f * (r - s1);
                }
                if (tm >= 0f)
                {
                    s2 = MathX.Sqrt(tm);
                    roots[count++] = -0.25f * b + 0.5f * (s2 - r);
                    roots[count++] = -0.25f * b - 0.5f * (s2 + r);
                }
                return count;
            }
            else
            {
                t2 = y * y - 4f * e;
                if (t2 >= 0f)
                {
                    t2 = 2f * MathX.Sqrt(t2);
                    t1 = 0.75f * b * b - 2f * c;
                    if (t1 + t2 >= 0f)
                    {
                        s1 = MathX.Sqrt(t1 + t2);
                        roots[count++] = -0.25f * b + 0.5f * s1;
                        roots[count++] = -0.25f * b - 0.5f * s1;
                    }
                    if (t1 - t2 >= 0f)
                    {
                        s2 = MathX.Sqrt(t1 - t2);
                        roots[count++] = -0.25f * b + 0.5f * s2;
                        roots[count++] = -0.25f * b - 0.5f * s2;
                    }
                }
                return count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Resize(int d, bool keep)
        {
            var alloc = (d + 1 + 3) & ~3;
            if (alloc > allocated)
            {
                var ptr = new float[alloc];
                if (coefficient != null && keep) for (int i = 0; i <= degree; i++) ptr[i] = coefficient[i];
                allocated = alloc;
                coefficient = ptr;
            }
            degree = d;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public T ToFloatPtr<T>(FloatPtr<T> callback)
        //{
        //    fixed (float* _ = this.coefficient) return callback(_);
        //}

        public string ToString(int precision = 2)
        {
            fixed (float* _ = coefficient) return FloatArrayToString(_, Dimension, precision);
        }

        static readonly float[] Laguer_frac = new[] { 0f, 0.5f, 0.25f, 0.75f, 0.13f, 0.38f, 0.62f, 0.88f, 1f };
        int Laguer(Complex* coef, int degree, ref Complex r)
        {
            const int MT = 10, MAX_ITERATIONS = MT * 8;
            int i, j;
            float abx, abp, abm, err;
            Complex dx, cx, b, d = new(), f = new(), g, s, gps, gms, g2;

            for (i = 1; i <= MAX_ITERATIONS; i++)
            {
                b = coef[degree];
                err = b.Abs();
                d.Zero();
                f.Zero();
                abx = r.Abs();
                for (j = degree - 1; j >= 0; j--)
                {
                    f = r * f + d;
                    d = r * d + b;
                    b = r * b + coef[j];
                    err = b.Abs() + abx * err;
                }
                if (b.Abs() < err * EPSILON) return i;
                g = d / b;
                g2 = g * g;
                s = ((degree - 1) * (degree * (g2 - 2f * f / b) - g2)).Sqrt();
                gps = g + s;
                gms = g - s;
                abp = gps.Abs();
                abm = gms.Abs();
                if (abp < abm)
                    gps = gms;
                dx = Math.Max(abp, abm) > 0f
                    ? degree / gps
                    : MathX.Exp(MathX.Log(1f + abx)) * new Complex(MathX.Cos(i), MathX.Sin(i)); //: opt
                cx = r - dx;
                if (r == cx) return i;
                r = i % MT == 0
                    ? cx
                    : r - (Laguer_frac[i / MT] * dx);
            }
            return i;
        }
    }
}