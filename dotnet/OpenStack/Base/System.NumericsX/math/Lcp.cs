#define IGNORE_UNSATISFIABLE_VARIABLES
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    public abstract class LCP
    {
        public static LCP AllocSquare()        // A must be a square matrix
            => new LCP_Square { MaxIterations = 32 };

        public static LCP AllocSymmetric()     // A must be a symmetric matrix
            => new LCP_Symmetric { MaxIterations = 32 };

        public abstract bool Solve(out MatrixX A, out VectorX x, out VectorX b, out VectorX lo, out VectorX hi, out int boxIndex);

        public int MaxIterations
        {
            get => maxIterations;
            set => maxIterations = value;
        }

        protected int maxIterations;

        //static CVar cp_showFailures = new("lcp_showFailures", "0", CVAR.SYSTEM | CVAR.BOOL, "show LCP solver failures");

        internal const float BOUND_EPSILON = 1e-5f;
        internal const float ACCEL_EPSILON = 1e-5f;
        internal const float DELTA_ACCEL_EPSILON = 1e-9f;
        internal const float DELTA_FORCE_EPSILON = 1e-9f;
    }

    public class LCP_Square : LCP
    {
        public override unsafe bool Solve(ref MatrixX o_m, ref VectorX o_x, ref VectorX o_b, ref VectorX o_lo, ref VectorX o_hi, int[] o_boxIndex)
        {
            int i, j, n, limit, limitSide, boxStartIndex; float dir, maxStep, dot, s; string failed;

            // true when the matrix rows are 16 byte padded
            padded = ((o_m.NumRows + 3) & ~3) == o_m.NumColumns;

            Debug.Assert(padded || o_m.NumRows == o_m.NumColumns);
            Debug.Assert(o_x.Size == o_m.NumRows);
            Debug.Assert(o_b.Size == o_m.NumRows);
            Debug.Assert(o_lo.Size == o_m.NumRows);
            Debug.Assert(o_hi.Size == o_m.NumRows);

            // allocate memory for permuted input
            f.SetData(o_m.NumRows, VectorX.VECX_ALLOCA(o_m.NumRows));
            a.SetData(o_b.Size, VectorX.VECX_ALLOCA(o_b.Size));
            b.SetData(o_b.Size, VectorX.VECX_ALLOCA(o_b.Size));
            lo.SetData(o_lo.Size, VectorX.VECX_ALLOCA(o_lo.Size));
            hi.SetData(o_hi.Size, VectorX.VECX_ALLOCA(o_hi.Size));
            if (o_boxIndex != null)
            {
                boxIndex = stackalloc int[o_x.Size]; boxIndex = (float*)_alloca16(boxIndex);
                Unsafe.CopyBlock(boxIndex, o_boxIndex, (uint)o_x.Size * sizeof(int));
            }
            else
                boxIndex = null;

            // we override the const on o_m here but on exit the matrix is unchanged
            m.SetData(o_m.NumRows, o_m.NumColumns, const_cast<float*>(o_m[0]));
            f.Zero();
            a.Zero();
            b = o_b;
            lo = o_lo;
            hi = o_hi;

            // pointers to the rows of m
            rowPtrs = stackalloc float*[m.NumRows]; rowPtrs = (float**)_alloca16(rowPtrs);
            for (i = 0; i < m.NumRows; i++)
                rowPtrs[i] = m[i];

            // tells if a variable is at the low boundary, high boundary or inbetween
            side = stackalloc int[m.NumRows]; side = (int*)_alloca16(side);

            // index to keep track of the permutation
            permuted = stackalloc int[m.NumRows]; permuted = (int*)_alloca16(permuted);
            for (i = 0; i < m.NumRows; i++)
                permuted[i] = i;

            // permute input so all unbounded variables come first
            numUnbounded = 0;
            for (i = 0; i < m.NumRows; i++)
                if (lo[i] == -MathX.INFINITY && hi[i] == MathX.INFINITY)
                {
                    if (numUnbounded != i)
                        Swap(numUnbounded, i);
                    numUnbounded++;
                }

            // permute input so all variables using the boxIndex come last
            boxStartIndex = m.NumRows;
            if (boxIndex != null)
                for (i = m.NumRows - 1; i >= numUnbounded; i--)
                    if (boxIndex[i] >= 0 && (lo[i] != -MathX.INFINITY || hi[i] != MathX.INFINITY))
                    {
                        boxStartIndex--;
                        if (boxStartIndex != i)
                            Swap(boxStartIndex, i);
                    }

            // sub matrix for factorization
            clamped.SetData(m.NumRows, m.NumColumns, MatrixX.MATX_ALLOCA(m.NumRows * m.NumColumns));
            diagonal.SetData(m.NumRows, VectorX.VECX_ALLOCA(m.NumRows));

            // all unbounded variables are clamped
            numClamped = numUnbounded;

            // if there are unbounded variables
            if (numUnbounded != 0)
            {
                // factor and solve for unbounded variables
                if (!FactorClamped())
                {
                    Lib.Printf("LCP_Square::Solve: unbounded factorization failed\n");
                    return false;
                }
                b.ToFloatPtr(_ => SolveClamped(f, _));

                // if there are no bounded variables we are done
                if (numUnbounded == m.NumRows)
                {
                    o_x = f;    // the vector is not permuted
                    return true;
                }
            }

#if IGNORE_UNSATISFIABLE_VARIABLES
            int numIgnored = 0;
#endif

            // allocate for delta force and delta acceleration
            delta_f.SetData(m.NumRows, VectorX.VECX_ALLOCA(m.NumRows));
            delta_a.SetData(m.NumRows, VectorX.VECX_ALLOCA(m.NumRows));

            // solve for bounded variables
            failed = null;
            for (i = numUnbounded; i < m.NumRows; i++)
            {
                // once we hit the box start index we can initialize the low and high boundaries of the variables using the box index
                if (i == boxStartIndex)
                {
                    for (j = 0; j < boxStartIndex; j++)
                        o_x[permuted[j]] = f[j];
                    for (j = boxStartIndex; j < m.NumRows; j++)
                    {
                        s = o_x[boxIndex[j]];
                        if (lo[j] != -MathX.INFINITY)
                            lo[j] = -MathX.Fabs(lo[j] * s);
                        if (hi[j] != MathX.INFINITY)
                            hi[j] = MathX.Fabs(hi[j] * s);
                    }
                }

                // calculate acceleration for current variable
                ISimd._.Dot(dot, rowPtrs[i], f.ToFloatPtr(), i);
                a[i] = dot - b[i];

                // if already at the low boundary
                if (lo[i] >= -LCP_BOUND_EPSILON && a[i] >= -LCP_ACCEL_EPSILON)
                {
                    side[i] = -1;
                    continue;
                }

                // if already at the high boundary
                if (hi[i] <= LCP_BOUND_EPSILON && a[i] <= LCP_ACCEL_EPSILON)
                {
                    side[i] = 1;
                    continue;
                }

                // if inside the clamped region
                if (MathX.Fabs(a[i]) <= LCP_ACCEL_EPSILON)
                {
                    side[i] = 0;
                    AddClamped(i);
                    continue;
                }

                // drive the current variable into a valid region
                for (n = 0; n < maxIterations; n++)
                {
                    // direction to move
                    dir = a[i] <= 0f ? 1f : -1f; //: opt

                    // calculate force delta
                    CalcForceDelta(i, dir);

                    // calculate acceleration delta: delta_a = m * delta_f;
                    CalcAccelDelta(i);

                    // maximum step we can take
                    GetMaxStep(i, dir, maxStep, limit, limitSide);

                    if (maxStep <= 0f)
                    {
#if IGNORE_UNSATISFIABLE_VARIABLES
                        // ignore the current variable completely
                        lo[i] = hi[i] = 0f;
                        f[i] = 0f;
                        side[i] = -1;
                        numIgnored++;
#else
                        failed = va("invalid step size %.4f", maxStep);
#endif
                        break;
                    }

                    // change force
                    ChangeForce(i, maxStep);

                    // change acceleration
                    ChangeAccel(i, maxStep);

                    // clamp/unclamp the variable that limited this step
                    side[limit] = limitSide;
                    switch (limitSide)
                    {
                        case 0:
                            {
                                a[limit] = 0f;
                                AddClamped(limit);
                                break;
                            }
                        case -1:
                            {
                                f[limit] = lo[limit];
                                if (limit != i)
                                {
                                    RemoveClamped(limit);
                                }
                                break;
                            }
                        case 1:
                            {
                                f[limit] = hi[limit];
                                if (limit != i)
                                {
                                    RemoveClamped(limit);
                                }
                                break;
                            }
                    }

                    // if the current variable limited the step we can continue with the next variable
                    if (limit == i)
                    {
                        break;
                    }
                }

                if (n >= maxIterations)
                {
                    failed = $"max iterations {maxIterations}";
                    break;
                }

                if (failed)
                {
                    break;
                }
            }

#if IGNORE_UNSATISFIABLE_VARIABLES
            if (numIgnored)
                if (lcp_showFailures.GetBool())
                    G.common_Printf("LCP_Symmetric::Solve: %d of %d bounded variables ignored\n", numIgnored, m.NumRows - numUnbounded);
#endif

            // if failed clear remaining forces
            if (failed)
            {
                if (lcp_showFailures.GetBool())
                    G.common_Printf("LCP_Square::Solve: %s (%d of %d bounded variables ignored)\n", failed, m.NumRows - i, m.NumRows - numUnbounded);
                for (j = i; j < m.NumRows; j++)
                    f[j] = 0f;
            }

#if _DEBUG && false
	if ( !failed ) {
		// test whether or not the solution satisfies the complementarity conditions
		for ( i = 0; i < m.GetNumRows(); i++ ) {
			a[i] = -b[i];
			for ( j = 0; j < m.GetNumRows(); j++ ) {
				a[i] += rowPtrs[i][j] * f[j];
			}

			if ( f[i] == lo[i] ) {
				if ( lo[i] != hi[i] && a[i] < -LCP_ACCEL_EPSILON ) {
					int bah1 = 1;
				}
			} else if ( f[i] == hi[i] ) {
				if ( lo[i] != hi[i] && a[i] > LCP_ACCEL_EPSILON ) {
					int bah2 = 1;
				}
			} else if ( f[i] < lo[i] || f[i] > hi[i] || idMath::Fabs( a[i] ) > 1f ) {
				int bah3 = 1;
			}
		}
	}
#endif

            // unpermute result
            for (i = 0; i < f.Size; i++)
                o_x[permuted[i]] = f[i];

            // unpermute original matrix
            for (i = 0; i < m.NumRows; i++)
            {
                for (j = 0; j < m.NumRows; j++)
                    if (permuted[j] == i)
                        break;
                if (i != j)
                {
                    m.SwapColumns(i, j);
                    UnsafeX.Swap(ref permuted[i], ref permuted[j]);
                }
            }

            return true;
        }

        MatrixX m;                   // original matrix
        VectorX b;                   // right hand side
        VectorX lo, hi;              // low and high bounds
        VectorX f, a;                // force and acceleration
        VectorX delta_f, delta_a;    // delta force and delta acceleration
        MatrixX clamped;         // LU factored sub matrix for clamped variables
        VectorX diagonal;            // reciprocal of diagonal of U of the LU factored sub matrix for clamped variables
        int numUnbounded;       // number of unbounded variables
        int numClamped;         // number of clamped variables
        float[][] rowPtrs;            // pointers to the rows of m
        int[] boxIndex;          // box index
        int[] side;              // tells if a variable is at the low boundary = -1, high boundary = 1 or inbetween = 0
        int[] permuted;          // index to keep track of the permutation
        bool padded;                // set to true if the rows of the initial matrix are 16 byte padded

        bool FactorClamped()
        {
            int i, j, k;
            float s, d;

            for (i = 0; i < numClamped; i++)
            {
                memcpy(clamped[i], rowPtrs[i], numClamped * sizeof(float));
            }

            for (i = 0; i < numClamped; i++)
            {
                s = MathX.Fabs(clamped[i][i]);

                if (s == 0f)
                    return false;

                diagonal[i] = d = 1f / clamped[i][i];
                for (j = i + 1; j < numClamped; j++)
                    clamped[j][i] *= d;

                for (j = i + 1; j < numClamped; j++)
                {
                    d = clamped[j][i];
                    for (k = i + 1; k < numClamped; k++)
                        clamped[j][k] -= d * clamped[i][k];
                }
            }

            return true;
        }

        void SolveClamped(VectorX x, float[] b)
        {
            int i, j; float sum;

            // solve L
            for (i = 0; i < numClamped; i++)
            {
                sum = b[i];
                for (j = 0; j < i; j++)
                    sum -= clamped[i][j] * x[j];
                x[i] = sum;
            }

            // solve U
            for (i = numClamped - 1; i >= 0; i--)
            {
                sum = x[i];
                for (j = i + 1; j < numClamped; j++)
                    sum -= clamped[i][j] * x[j];
                x[i] = sum * diagonal[i];
            }
        }

        void Swap(int i, int j)
        {
            if (i == j)
                return;

            UnsafeX.Swap(ref rowPtrs[i], ref rowPtrs[j]);
            m.SwapColumns(i, j);
            b.SwapElements(i, j);
            lo.SwapElements(i, j);
            hi.SwapElements(i, j);
            a.SwapElements(i, j);
            f.SwapElements(i, j);
            if (boxIndex != null)
                UnsafeX.Swap(ref boxIndex[i], ref boxIndex[j]);
            UnsafeX.Swap(ref side[i], ref side[j]);
            UnsafeX.Swap(ref permuted[i], ref permuted[j]);
        }

        void AddClamped(int r)
        {
            int i, j; float sum;
            Debug.Assert(r >= numClamped);

            // add a row at the bottom and a column at the right of the factored matrix for the clamped variables

            Swap(numClamped, r);

            // add row to L
            for (i = 0; i < numClamped; i++)
            {
                sum = rowPtrs[numClamped][i];
                for (j = 0; j < i; j++)
                    sum -= clamped[numClamped][j] * clamped[j][i];
                clamped[numClamped][i] = sum * diagonal[i];
            }

            // add column to U
            for (i = 0; i <= numClamped; i++)
            {
                sum = rowPtrs[i][numClamped];
                for (j = 0; j < i; j++)
                    sum -= clamped[i][j] * clamped[j][numClamped];
                clamped[i][numClamped] = sum;
            }

            diagonal[numClamped] = 1f / clamped[numClamped][numClamped];

            numClamped++;
        }

        unsafe void RemoveClamped(int r)
        {
            int i, j; double diag, beta0, beta1, p0, p1, q0, q1, d;
            Debug.Assert(r < numClamped);

            numClamped--;

            // no need to swap and update the factored matrix when the last row and column are removed
            if (r == numClamped)
                return;

            var y0 = stackalloc float[numClamped]; y0 = (float*)_alloca16(y0);
            var z0 = stackalloc float[numClamped]; z0 = (float*)_alloca16(z0);
            var y1 = stackalloc float[numClamped]; y1 = (float*)_alloca16(y1);
            var z1 = stackalloc float[numClamped]; z1 = (float*)_alloca16(z1);

            // the row/column need to be subtracted from the factorization
            for (i = 0; i < numClamped; i++)
                y0[i] = -rowPtrs[i][r];

            Unsafe.InitBlock(y1, 0, (uint)numClamped * sizeof(float));
            y1[r] = 1f;

            Unsafe.InitBlock(z0, 0, (uint)numClamped * sizeof(float));
            z0[r] = 1f;

            for (i = 0; i < numClamped; i++)
                z1[i] = -rowPtrs[r][i];

            // swap the to be removed row/column with the last row/column
            Swap(r, numClamped);

            // the swapped last row/column need to be added to the factorization
            for (i = 0; i < numClamped; i++)
                y0[i] += rowPtrs[i][r];

            for (i = 0; i < numClamped; i++)
                z1[i] += rowPtrs[r][i];
            z1[r] = 0f;

            // update the beginning of the to be updated row and column
            for (i = 0; i < r; i++)
            {
                p0 = y0[i];
                beta1 = z1[i] * diagonal[i];

                clamped[i][r] += p0;
                for (j = i + 1; j < numClamped; j++)
                    z1[j] -= beta1 * clamped[i][j];
                for (j = i + 1; j < numClamped; j++)
                    y0[j] -= p0 * clamped[j][i];
                clamped[r][i] += beta1;
            }

            // update the lower right corner starting at r,r
            for (i = r; i < numClamped; i++)
            {
                diag = clamped[i][i];

                p0 = y0[i];
                p1 = z0[i];
                diag += p0 * p1;

                if (diag == 0f)
                {
                    Lib.Printf("LCP_Square::RemoveClamped: updating factorization failed\n");
                    return;
                }

                beta0 = p1 / diag;

                q0 = y1[i];
                q1 = z1[i];
                diag += q0 * q1;

                if (diag == 0f)
                {
                    Lib.Printf("LCP_Square::RemoveClamped: updating factorization failed\n");
                    return;
                }

                d = 1f / diag;
                beta1 = q1 * d;

                clamped[i][i] = diag;
                diagonal[i] = d;

                for (j = i + 1; j < numClamped; j++)
                {

                    d = clamped[i][j];

                    d += p0 * z0[j];
                    z0[j] -= beta0 * d;

                    d += q0 * z1[j];
                    z1[j] -= beta1 * d;

                    clamped[i][j] = d;
                }

                for (j = i + 1; j < numClamped; j++)
                {
                    d = clamped[j][i];

                    y0[j] -= p0 * d;
                    d += beta0 * y0[j];

                    y1[j] -= q0 * d;
                    d += beta1 * y1[j];

                    clamped[j][i] = d;
                }
            }
            return;
        }

        unsafe void CalcForceDelta(int d, float dir)
        {
            int i;

            delta_f[d] = dir;

            if (numClamped == 0)
                return;

            // get column d of matrix
            var ptr = stackalloc float[numClamped]; ptr = (float*)_alloca16(ptr);
            for (i = 0; i < numClamped; i++)
                ptr[i] = rowPtrs[i][d];

            // solve force delta
            SolveClamped(delta_f, ptr);

            // flip force delta based on direction
            if (dir > 0f)
                delta_f.ToFloatPtr(ptr =>
                {
                    for (i = 0; i < numClamped; i++)
                        ptr[i] = -ptr[i];
                });
        }

        void CalcAccelDelta(int d)
        {
            int j;
            float dot;

            // only the not clamped variables, including the current variable, can have a change in acceleration
            for (j = numClamped; j <= d; j++)
            {
                // only the clamped variables and the current variable have a force delta unequal zero
                ISimd._.Dot(dot, rowPtrs[j], delta_f.ToFloatPtr(), numClamped);
                delta_a[j] = dot + rowPtrs[j][d] * delta_f[d];
            }
        }
        void ChangeForce(int d, float step)
        {
            // only the clamped variables and current variable have a force delta unequal zero
            ISimd._.MulAdd(f.ToFloatPtr(), step, delta_f.ToFloatPtr(), numClamped);
            f[d] += step * delta_f[d];
        }
        void ChangeAccel(int d, float step)
        {
            // only the not clamped variables, including the current variable, can have an acceleration unequal zero
            ISimd._.MulAdd(a.ToFloatPtr() + numClamped, step, delta_a.ToFloatPtr() + numClamped, d - numClamped + 1);
        }

        void GetMaxStep(int d, float dir, out float maxStep, out int limit, out int limitSide)
        {
            int i;
            float s;

            // default to a full step for the current variable
            if (idMath::Fabs(delta_a[d]) > LCP_DELTA_ACCEL_EPSILON)
            {
                maxStep = -a[d] / delta_a[d];
            }
            else
            {
                maxStep = 0f;
            }
            limit = d;
            limitSide = 0;

            // test the current variable
            if (dir < 0f)
            {
                if (lo[d] != -idMath::INFINITY)
                {
                    s = (lo[d] - f[d]) / dir;
                    if (s < maxStep)
                    {
                        maxStep = s;
                        limitSide = -1;
                    }
                }
            }
            else
            {
                if (hi[d] != idMath::INFINITY)
                {
                    s = (hi[d] - f[d]) / dir;
                    if (s < maxStep)
                    {
                        maxStep = s;
                        limitSide = 1;
                    }
                }
            }

            // test the clamped bounded variables
            for (i = numUnbounded; i < numClamped; i++)
            {
                if (delta_f[i] < -LCP_DELTA_FORCE_EPSILON)
                {
                    // if there is a low boundary
                    if (lo[i] != -idMath::INFINITY)
                    {
                        s = (lo[i] - f[i]) / delta_f[i];
                        if (s < maxStep)
                        {
                            maxStep = s;
                            limit = i;
                            limitSide = -1;
                        }
                    }
                }
                else if (delta_f[i] > LCP_DELTA_FORCE_EPSILON)
                {
                    // if there is a high boundary
                    if (hi[i] != idMath::INFINITY)
                    {
                        s = (hi[i] - f[i]) / delta_f[i];
                        if (s < maxStep)
                        {
                            maxStep = s;
                            limit = i;
                            limitSide = 1;
                        }
                    }
                }
            }

            // test the not clamped bounded variables
            for (i = numClamped; i < d; i++)
            {
                if (side[i] == -1)
                {
                    if (delta_a[i] >= -LCP_DELTA_ACCEL_EPSILON)
                    {
                        continue;
                    }
                }
                else if (side[i] == 1)
                {
                    if (delta_a[i] <= LCP_DELTA_ACCEL_EPSILON)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
                // ignore variables for which the force is not allowed to take any substantial value
                if (lo[i] >= -LCP_BOUND_EPSILON && hi[i] <= LCP_BOUND_EPSILON)
                {
                    continue;
                }
                s = -a[i] / delta_a[i];
                if (s < maxStep)
                {
                    maxStep = s;
                    limit = i;
                    limitSide = 0;
                }
            }
        }
    }

    public class LCP_Symmetric : LCP
    {
        public override bool Solve(out MatrixX o_m, out VectorX o_x, out VectorX o_b, out VectorX o_lo, out VectorX o_hi, out int o_boxIndex)
        {
            int i, j, n, limit, limitSide, boxStartIndex;
            float dir, maxStep, dot, s;
            char* failed;

            // true when the matrix rows are 16 byte padded
            padded = ((o_m.GetNumRows() + 3) & ~3) == o_m.GetNumColumns();

            Debug.Asset(padded || o_m.GetNumRows() == o_m.GetNumColumns());
            Debug.Asset(o_x.GetSize() == o_m.GetNumRows());
            Debug.Asset(o_b.GetSize() == o_m.GetNumRows());
            Debug.Asset(o_lo.GetSize() == o_m.GetNumRows());
            Debug.Asset(o_hi.GetSize() == o_m.GetNumRows());

            // allocate memory for permuted input
            f.SetData(o_m.GetNumRows(), VECX_ALLOCA(o_m.GetNumRows()));
            a.SetData(o_b.GetSize(), VECX_ALLOCA(o_b.GetSize()));
            b.SetData(o_b.GetSize(), VECX_ALLOCA(o_b.GetSize()));
            lo.SetData(o_lo.GetSize(), VECX_ALLOCA(o_lo.GetSize()));
            hi.SetData(o_hi.GetSize(), VECX_ALLOCA(o_hi.GetSize()));
            if (o_boxIndex)
            {
                boxIndex = stackalloc int[o_x.Size]; boxIndex = (int*)_alloca16(boxIndex);
                memcpy(boxIndex, o_boxIndex, o_x.Size * sizeof(int));
            }
            else
            {
                boxIndex = null;
            }

            // we override the const on o_m here but on exit the matrix is unchanged
            m.SetData(o_m.GetNumRows(), o_m.GetNumColumns(), const_cast<float*>(o_m[0]));
            f.Zero();
            a.Zero();
            b = o_b;
            lo = o_lo;
            hi = o_hi;

            // pointers to the rows of m
            rowPtrs = stackalloc float*[m.NumRows]; rowPtrs = (float**)_alloca16(rowPtrs);
            for (i = 0; i < m.NumRows; i++)
            {
                rowPtrs[i] = m[i];
            }

            // tells if a variable is at the low boundary, high boundary or inbetween
            side = stackalloc int[m.NumRows]; side = (int*)_alloca16(side);

            // index to keep track of the permutation
            permuted = stackalloc int[m.NumRows]; permuted = (int*)_alloca16(permuted);
            for (i = 0; i < m.GetNumRows(); i++)
            {
                permuted[i] = i;
            }

            // permute input so all unbounded variables come first
            numUnbounded = 0;
            for (i = 0; i < m.GetNumRows(); i++)
            {
                if (lo[i] == -idMath::INFINITY && hi[i] == idMath::INFINITY)
                {
                    if (numUnbounded != i)
                    {
                        Swap(numUnbounded, i);
                    }
                    numUnbounded++;
                }
            }

            // permute input so all variables using the boxIndex come last
            boxStartIndex = m.GetNumRows();
            if (boxIndex)
            {
                for (i = m.GetNumRows() - 1; i >= numUnbounded; i--)
                {
                    if (boxIndex[i] >= 0 && (lo[i] != -idMath::INFINITY || hi[i] != idMath::INFINITY))
                    {
                        boxStartIndex--;
                        if (boxStartIndex != i)
                        {
                            Swap(boxStartIndex, i);
                        }
                    }
                }
            }

            // sub matrix for factorization
            clamped.SetData(m.GetNumRows(), m.GetNumColumns(), MATX_ALLOCA(m.GetNumRows() * m.GetNumColumns()));
            diagonal.SetData(m.GetNumRows(), VECX_ALLOCA(m.GetNumRows()));
            solveCache1.SetData(m.GetNumRows(), VECX_ALLOCA(m.GetNumRows()));
            solveCache2.SetData(m.GetNumRows(), VECX_ALLOCA(m.GetNumRows()));

            // all unbounded variables are clamped
            numClamped = numUnbounded;

            // if there are unbounded variables
            if (numUnbounded)
            {

                // factor and solve for unbounded variables
                if (!FactorClamped())
                {
                    idLib::common_Printf("idLCP_Symmetric::Solve: unbounded factorization failed\n");
                    return false;
                }
                SolveClamped(f, b.ToFloatPtr());

                // if there are no bounded variables we are done
                if (numUnbounded == m.GetNumRows())
                {
                    o_x = f;    // the vector is not permuted
                    return true;
                }
            }

# ifdef IGNORE_UNSATISFIABLE_VARIABLES
            int numIgnored = 0;
#endif

            // allocate for delta force and delta acceleration
            delta_f.SetData(m.GetNumRows(), VECX_ALLOCA(m.GetNumRows()));
            delta_a.SetData(m.GetNumRows(), VECX_ALLOCA(m.GetNumRows()));

            // solve for bounded variables
            failed = null;
            for (i = numUnbounded; i < m.GetNumRows(); i++)
            {

                clampedChangeStart = 0;

                // once we hit the box start index we can initialize the low and high boundaries of the variables using the box index
                if (i == boxStartIndex)
                {
                    for (j = 0; j < boxStartIndex; j++)
                    {
                        o_x[permuted[j]] = f[j];
                    }
                    for (j = boxStartIndex; j < m.GetNumRows(); j++)
                    {
                        s = o_x[boxIndex[j]];
                        if (lo[j] != -idMath::INFINITY)
                        {
                            lo[j] = -idMath::Fabs(lo[j] * s);
                        }
                        if (hi[j] != idMath::INFINITY)
                        {
                            hi[j] = idMath::Fabs(hi[j] * s);
                        }
                    }
                }

                // calculate acceleration for current variable
                ISimd._.Dot(dot, rowPtrs[i], f.ToFloatPtr(), i);
                a[i] = dot - b[i];

                // if already at the low boundary
                if (lo[i] >= -LCP_BOUND_EPSILON && a[i] >= -LCP_ACCEL_EPSILON)
                {
                    side[i] = -1;
                    continue;
                }

                // if already at the high boundary
                if (hi[i] <= LCP_BOUND_EPSILON && a[i] <= LCP_ACCEL_EPSILON)
                {
                    side[i] = 1;
                    continue;
                }

                // if inside the clamped region
                if (idMath::Fabs(a[i]) <= LCP_ACCEL_EPSILON)
                {
                    side[i] = 0;
                    AddClamped(i, false);
                    continue;
                }

                // drive the current variable into a valid region
                for (n = 0; n < maxIterations; n++)
                {

                    // direction to move
                    if (a[i] <= 0f)
                    {
                        dir = 1f;
                    }
                    else
                    {
                        dir = -1f;
                    }

                    // calculate force delta
                    CalcForceDelta(i, dir);

                    // calculate acceleration delta: delta_a = m * delta_f;
                    CalcAccelDelta(i);

                    // maximum step we can take
                    GetMaxStep(i, dir, maxStep, limit, limitSide);

                    if (maxStep <= 0f)
                    {
# ifdef IGNORE_UNSATISFIABLE_VARIABLES
                        // ignore the current variable completely
                        lo[i] = hi[i] = 0f;
                        f[i] = 0f;
                        side[i] = -1;
                        numIgnored++;
#else
                        failed = va("invalid step size %.4f", maxStep);
#endif
                        break;
                    }

                    // change force
                    ChangeForce(i, maxStep);

                    // change acceleration
                    ChangeAccel(i, maxStep);

                    // clamp/unclamp the variable that limited this step
                    side[limit] = limitSide;
                    switch (limitSide)
                    {
                        case 0:
                            {
                                a[limit] = 0f;
                                AddClamped(limit, (limit == i));
                                break;
                            }
                        case -1:
                            {
                                f[limit] = lo[limit];
                                if (limit != i)
                                {
                                    RemoveClamped(limit);
                                }
                                break;
                            }
                        case 1:
                            {
                                f[limit] = hi[limit];
                                if (limit != i)
                                {
                                    RemoveClamped(limit);
                                }
                                break;
                            }
                    }

                    // if the current variable limited the step we can continue with the next variable
                    if (limit == i)
                    {
                        break;
                    }
                }

                if (n >= maxIterations)
                {
                    failed = va("max iterations %d", maxIterations);
                    break;
                }

                if (failed)
                {
                    break;
                }
            }

# ifdef IGNORE_UNSATISFIABLE_VARIABLES
            if (numIgnored)
            {
                if (lcp_showFailures.GetBool())
                {
                    idLib::common_Printf("idLCP_Symmetric::Solve: %d of %d bounded variables ignored\n", numIgnored, m.GetNumRows() - numUnbounded);
                }
            }
#endif

            // if failed clear remaining forces
            if (failed)
            {
                if (lcp_showFailures.GetBool())
                {
                    idLib::common_Printf("idLCP_Symmetric::Solve: %s (%d of %d bounded variables ignored)\n", failed, m.GetNumRows() - i, m.GetNumRows() - numUnbounded);
                }
                for (j = i; j < m.GetNumRows(); j++)
                {
                    f[j] = 0f;
                }
            }

#if _DEBUG && false
        if ( !failed ) {
            // test whether or not the solution satisfies the complementarity conditions
            for ( i = 0; i < m.GetNumRows(); i++ ) {
                a[i] = -b[i];
                for ( j = 0; j < m.GetNumRows(); j++ ) {
                    a[i] += rowPtrs[i][j] * f[j];
                }

                if ( f[i] == lo[i] ) {
                    if ( lo[i] != hi[i] && a[i] < -LCP_ACCEL_EPSILON ) {
                        int bah1 = 1;
                    }
                } else if ( f[i] == hi[i] ) {
                    if ( lo[i] != hi[i] && a[i] > LCP_ACCEL_EPSILON ) {
                        int bah2 = 1;
                    }
                } else if ( f[i] < lo[i] || f[i] > hi[i] || idMath::Fabs( a[i] ) > 1f ) {
                    int bah3 = 1;
                }
            }
        }
#endif

            // unpermute result
            for (i = 0; i < f.GetSize(); i++)
            {
                o_x[permuted[i]] = f[i];
            }

            // unpermute original matrix
            for (i = 0; i < m.GetNumRows(); i++)
            {
                for (j = 0; j < m.GetNumRows(); j++)
                {
                    if (permuted[j] == i)
                    {
                        break;
                    }
                }
                if (i != j)
                {
                    m.SwapColumns(i, j);
                    idSwap(permuted[i], permuted[j]);
                }
            }

            return true;
        }

        MatrixX m;                   // original matrix
        VectorX b;                   // right hand side
        VectorX lo, hi;              // low and high bounds
        VectorX f, a;                // force and acceleration
        VectorX delta_f, delta_a;    // delta force and delta acceleration
        MatrixX clamped;         // LDLt factored sub matrix for clamped variables
        VectorX diagonal;            // reciprocal of diagonal of LDLt factored sub matrix for clamped variables
        VectorX solveCache1;     // intermediate result cached in SolveClamped
        VectorX solveCache2;     // "
        int numUnbounded;       // number of unbounded variables
        int numClamped;         // number of clamped variables
        int clampedChangeStart; // lowest row/column changed in the clamped matrix during an iteration
        float[][] rowPtrs;            // pointers to the rows of m
        int[] boxIndex;          // box index
        int[] side;              // tells if a variable is at the low boundary = -1, high boundary = 1 or inbetween = 0
        int[] permuted;          // index to keep track of the permutation
        bool padded;                // set to true if the rows of the initial matrix are 16 byte padded


        bool FactorClamped()
        {
            clampedChangeStart = 0;

            for (int i = 0; i < numClamped; i++)
            {
                memcpy(clamped[i], rowPtrs[i], numClamped * sizeof(float));
            }
            return ISimd._.MatX_LDLTFactor(clamped, diagonal, numClamped);
        }
        void SolveClamped(VectorX x, float[] b)
        {

            // solve L
            ISimd._.MatX_LowerTriangularSolve(clamped, solveCache1.ToFloatPtr(), b, numClamped, clampedChangeStart);

            // solve D
            ISimd._.Mul(solveCache2.ToFloatPtr(), solveCache1.ToFloatPtr(), diagonal.ToFloatPtr(), numClamped);

            // solve Lt
            ISimd._.MatX_LowerTriangularSolveTranspose(clamped, x.ToFloatPtr(), solveCache2.ToFloatPtr(), numClamped);

            clampedChangeStart = numClamped;
        }
        void Swap(int i, int j)
        {

            if (i == j)
            {
                return;
            }

            idSwap(rowPtrs[i], rowPtrs[j]);
            m.SwapColumns(i, j);
            b.SwapElements(i, j);
            lo.SwapElements(i, j);
            hi.SwapElements(i, j);
            a.SwapElements(i, j);
            f.SwapElements(i, j);
            if (boxIndex)
            {
                idSwap(boxIndex[i], boxIndex[j]);
            }
            idSwap(side[i], side[j]);
            idSwap(permuted[i], permuted[j]);
        }
        void AddClamped(int r, bool useSolveCache)
        {
            float d, dot;

            Debug.Asset(r >= numClamped);

            if (numClamped < clampedChangeStart)
            {
                clampedChangeStart = numClamped;
            }

            // add a row at the bottom and a column at the right of the factored
            // matrix for the clamped variables

            Swap(numClamped, r);

            // solve for v in L * v = rowPtr[numClamped]
            if (useSolveCache)
            {

                // the lower triangular solve was cached in SolveClamped called by CalcForceDelta
                memcpy(clamped[numClamped], solveCache2.ToFloatPtr(), numClamped * sizeof(float));
                // calculate row dot product
                ISimd._.Dot(dot, solveCache2.ToFloatPtr(), solveCache1.ToFloatPtr(), numClamped);

            }
            else
            {
                var v = stackalloc float[numClamped]; v = (float*)_alloca16(v);

                ISimd._.MatX_LowerTriangularSolve(clamped, v, rowPtrs[numClamped], numClamped);
                // add bottom row to L
                ISimd._.Mul(clamped[numClamped], v, diagonal.ToFloatPtr(), numClamped);
                // calculate row dot product
                ISimd._.Dot(dot, clamped[numClamped], v, numClamped);
            }

            // update diagonal[numClamped]
            d = rowPtrs[numClamped][numClamped] - dot;

            if (d == 0f)
            {
                idLib::common_Printf("idLCP_Symmetric::AddClamped: updating factorization failed\n");
                numClamped++;
                return;
            }

            clamped[numClamped][numClamped] = d;
            diagonal[numClamped] = 1f / d;

            numClamped++;
        }
        void RemoveClamped(int r)
        {
            int i, j, n;
            float* addSub, *original, *v, *ptr, *v1, *v2, dot;
            double sum, diag, newDiag, invNewDiag, p1, p2, alpha1, alpha2, beta1, beta2;

            Debug.Asset(r < numClamped);

            if (r < clampedChangeStart)
            {
                clampedChangeStart = r;
            }

            numClamped--;

            // no need to swap and update the factored matrix when the last row and column are removed
            if (r == numClamped)
            {
                return;
            }

            // swap the to be removed row/column with the last row/column
            Swap(r, numClamped);

            // update the factored matrix
            addSub = stackalloc float[numClamped]; addSub = (float*)_alloca16(addSub);

            if (r == 0)
            {

                if (numClamped == 1)
                {
                    diag = rowPtrs[0][0];
                    if (diag == 0f)
                    {
                        common.Printf("idLCP_Symmetric::RemoveClamped: updating factorization failed\n");
                        return;
                    }
                    clamped[0][0] = diag;
                    diagonal[0] = 1f / diag;
                    return;
                }

                // calculate the row/column to be added to the lower right sub matrix starting at (r, r)
                original = rowPtrs[numClamped];
                ptr = rowPtrs[r];
                addSub[0] = ptr[0] - original[numClamped];
                for (i = 1; i < numClamped; i++)
                {
                    addSub[i] = ptr[i] - original[i];
                }

            }
            else
            {
                v = stackalloc float[numClamped]; v = (float*)_alloca16(v);

                // solve for v in L * v = rowPtr[r]
                ISimd._.MatX_LowerTriangularSolve(clamped, v, rowPtrs[r], r);

                // update removed row
                ISimd._.Mul(clamped[r], v, diagonal.ToFloatPtr(), r);

                // if the last row/column of the matrix is updated
                if (r == numClamped - 1)
                {
                    // only calculate new diagonal
                    ISimd._.Dot(dot, clamped[r], v, r);
                    diag = rowPtrs[r][r] - dot;
                    if (diag == 0f)
                    {
                        idLib::common_Printf("idLCP_Symmetric::RemoveClamped: updating factorization failed\n");
                        return;
                    }
                    clamped[r][r] = diag;
                    diagonal[r] = 1f / diag;
                    return;
                }

                // calculate the row/column to be added to the lower right sub matrix starting at (r, r)
                for (i = 0; i < r; i++)
                {
                    v[i] = clamped[r][i] * clamped[i][i];
                }
                for (i = r; i < numClamped; i++)
                {
                    if (i == r)
                    {
                        sum = clamped[r][r];
                    }
                    else
                    {
                        sum = clamped[r][r] * clamped[i][r];
                    }
                    ptr = clamped[i];
                    for (j = 0; j < r; j++)
                    {
                        sum += ptr[j] * v[j];
                    }
                    addSub[i] = rowPtrs[r][i] - sum;
                }
            }

            // add row/column to the lower right sub matrix starting at (r, r)

            v1 = stackalloc float[numClamped]; v1 = (float*)_alloca16(v1);
            v2 = stackalloc float[numClamped]; v2 = (float*)_alloca16(v2);

            diag = idMath::SQRT_1OVER2;
            v1[r] = (0.5f * addSub[r] + 1f) * diag;
            v2[r] = (0.5f * addSub[r] - 1f) * diag;
            for (i = r + 1; i < numClamped; i++)
            {
                v1[i] = v2[i] = addSub[i] * diag;
            }

            alpha1 = 1f;
            alpha2 = -1f;

            // simultaneous update/downdate of the sub matrix starting at (r, r)
            n = clamped.GetNumColumns();
            for (i = r; i < numClamped; i++)
            {

                diag = clamped[i][i];
                p1 = v1[i];
                newDiag = diag + alpha1 * p1 * p1;

                if (newDiag == 0f)
                {
                    idLib::common_Printf("idLCP_Symmetric::RemoveClamped: updating factorization failed\n");
                    return;
                }

                alpha1 /= newDiag;
                beta1 = p1 * alpha1;
                alpha1 *= diag;

                diag = newDiag;
                p2 = v2[i];
                newDiag = diag + alpha2 * p2 * p2;

                if (newDiag == 0f)
                {
                    idLib::common_Printf("idLCP_Symmetric::RemoveClamped: updating factorization failed\n");
                    return;
                }

                clamped[i][i] = newDiag;
                diagonal[i] = invNewDiag = 1f / newDiag;

                alpha2 *= invNewDiag;
                beta2 = p2 * alpha2;
                alpha2 *= diag;

                // update column below diagonal (i,i)
                ptr = clamped.ToFloatPtr() + i;

                for (j = i + 1; j < numClamped - 1; j += 2)
                {

                    float sum0 = ptr[(j + 0) * n];
                    float sum1 = ptr[(j + 1) * n];

                    v1[j + 0] -= p1 * sum0;
                    v1[j + 1] -= p1 * sum1;

                    sum0 += beta1 * v1[j + 0];
                    sum1 += beta1 * v1[j + 1];

                    v2[j + 0] -= p2 * sum0;
                    v2[j + 1] -= p2 * sum1;

                    sum0 += beta2 * v2[j + 0];
                    sum1 += beta2 * v2[j + 1];

                    ptr[(j + 0) * n] = sum0;
                    ptr[(j + 1) * n] = sum1;
                }

                for (; j < numClamped; j++)
                {

                    sum = ptr[j * n];

                    v1[j] -= p1 * sum;
                    sum += beta1 * v1[j];

                    v2[j] -= p2 * sum;
                    sum += beta2 * v2[j];

                    ptr[j * n] = sum;
                }
            }
        }
        void CalcForceDelta(int d, float dir)
        {
            int i;
            float* ptr;

            delta_f[d] = dir;

            if (numClamped == 0)
            {
                return;
            }

            // solve force delta
            SolveClamped(delta_f, rowPtrs[d]);

            // flip force delta based on direction
            if (dir > 0f)
            {
                ptr = delta_f.ToFloatPtr();
                for (i = 0; i < numClamped; i++)
                {
                    ptr[i] = -ptr[i];
                }
            }
        }
        void CalcAccelDelta(int d)
        {
            int j; float dot;

            // only the not clamped variables, including the current variable, can have a change in acceleration
            for (j = numClamped; j <= d; j++)
            {
                // only the clamped variables and the current variable have a force delta unequal zero
                ISimd._.Dot(dot, rowPtrs[j], delta_f.ToFloatPtr(), numClamped);
                delta_a[j] = dot + rowPtrs[j][d] * delta_f[d];
            }
        }
        void ChangeForce(int d, float step)
        {
            // only the clamped variables and current variable have a force delta unequal zero
            ISimd._.MulAdd(f.ToFloatPtr(), step, delta_f.ToFloatPtr(), numClamped);
            f[d] += step * delta_f[d];
        }
        void ChangeAccel(int d, float step)
        {
            // only the not clamped variables, including the current variable, can have an acceleration unequal zero
            ISimd._.MulAdd(a.ToFloatPtr() + numClamped, step, delta_a.ToFloatPtr() + numClamped, d - numClamped + 1);
        }

        void GetMaxStep(int d, float dir, out float maxStep, out int limit, out int limitSide)
        {
            int i; float s;

            // default to a full step for the current variable
            maxStep = MathX.Fabs(delta_a[d]) > DELTA_ACCEL_EPSILON
                ? -a[d] / delta_a[d]
                : 0f; //: opt
            limit = d;
            limitSide = 0;

            // test the current variable
            if (dir < 0f)
            {
                if (lo[d] != -MathX.INFINITY)
                {
                    s = (lo[d] - f[d]) / dir;
                    if (s < maxStep)
                    {
                        maxStep = s;
                        limitSide = -1;
                    }
                }
            }
            else
            {
                if (hi[d] != MathX.INFINITY)
                {
                    s = (hi[d] - f[d]) / dir;
                    if (s < maxStep)
                    {
                        maxStep = s;
                        limitSide = 1;
                    }
                }
            }

            // test the clamped bounded variables
            for (i = numUnbounded; i < numClamped; i++)
                if (delta_f[i] < -DELTA_FORCE_EPSILON)
                {
                    // if there is a low boundary
                    if (lo[i] != -MathX.INFINITY)
                    {
                        s = (lo[i] - f[i]) / delta_f[i];
                        if (s < maxStep)
                        {
                            maxStep = s;
                            limit = i;
                            limitSide = -1;
                        }
                    }
                }
                else if (delta_f[i] > DELTA_FORCE_EPSILON)
                {
                    // if there is a high boundary
                    if (hi[i] != MathX.INFINITY)
                    {
                        s = (hi[i] - f[i]) / delta_f[i];
                        if (s < maxStep)
                        {
                            maxStep = s;
                            limit = i;
                            limitSide = 1;
                        }
                    }
                }

            // test the not clamped bounded variables
            for (i = numClamped; i < d; i++)
            {
                if (side[i] == -1)
                {
                    if (delta_a[i] >= -DELTA_ACCEL_EPSILON)
                        continue;
                }
                else if (side[i] == 1)
                {
                    if (delta_a[i] <= DELTA_ACCEL_EPSILON)
                        continue;
                }
                else
                    continue;
                // ignore variables for which the force is not allowed to take any substantial value
                if (lo[i] >= -BOUND_EPSILON && hi[i] <= BOUND_EPSILON)
                    continue;
                s = -a[i] / delta_a[i];
                if (s < maxStep)
                {
                    maxStep = s;
                    limit = i;
                    limitSide = 0;
                }
            }
        }
    }
}