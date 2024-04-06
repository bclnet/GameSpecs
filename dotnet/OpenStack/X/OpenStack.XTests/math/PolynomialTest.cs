using System.Diagnostics;

namespace System.NumericsX
{
    public unsafe class PolynomialText
    {
        public static void Test()
        {
            int i, num; float value; Complex complexValue; Polynomial p;
            var roots = stackalloc float[4];
            var complexRoots = stackalloc Complex[4];

            p = new(-5f, 4f);
            num = p.GetRoots(roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(-5f, 4f, 3f);
            num = p.GetRoots(roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(1f, 4f, 3f, -2f);
            num = p.GetRoots(roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(5f, 4f, 3f, -2f);
            num = p.GetRoots(roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(-5f, 4f, 3f, 2f, 1f);
            num = p.GetRoots(roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(1f, 4f, 3f, -2f);
            num = p.GetRoots(complexRoots);
            for (i = 0; i < num; i++)
            {
                complexValue = p.GetValue(complexRoots[i]);
                Debug.Assert(MathX.Fabs(complexValue.r) < 1e-4f && MathX.Fabs(complexValue.i) < 1e-4f);
            }

            p = new(5f, 4f, 3f, -2f);
            num = p.GetRoots(complexRoots);
            for (i = 0; i < num; i++)
            {
                complexValue = p.GetValue(complexRoots[i]);
                Debug.Assert(MathX.Fabs(complexValue.r) < 1e-4f && MathX.Fabs(complexValue.i) < 1e-4f);
            }
        }
    }
}