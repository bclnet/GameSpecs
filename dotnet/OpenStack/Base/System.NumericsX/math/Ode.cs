namespace System.NumericsX
{
    public delegate void DeriveFunction(float t, object userData, float[] state, float[] derivatives);

    public abstract class ODE
    {
        public abstract float Evaluate(float[] state, float[] newState, float t0, float t1);

        protected int dimension;            // dimension in floats allocated for
        protected DeriveFunction derive;    // derive function
        protected object userData;          // client data
    }

    public class ODE_Euler : ODE
    {
        protected float[] derivatives;      // space to store derivatives

        public ODE_Euler(int dim, DeriveFunction dr, object ud)
        {
            dimension = dim;
            derivatives = new float[dim];
            derive = dr;
            userData = ud;
        }

        public override float Evaluate(float[] state, float[] newState, float t0, float t1)
        {
            derive(t0, userData, state, derivatives);
            var delta = t1 - t0;
            for (var i = 0; i < dimension; i++) newState[i] = state[i] + delta * derivatives[i];
            return delta;
        }
    }

    public class ODE_Midpoint : ODE
    {
        protected float[] tmpState;
        protected float[] derivatives;      // space to store derivatives

        public ODE_Midpoint(int dim, DeriveFunction dr, object ud)
        {
            dimension = dim;
            tmpState = new float[dim];
            derivatives = new float[dim];
            derive = dr;
            userData = ud;
        }

        public override float Evaluate(float[] state, float[] newState, float t0, float t1)
        {
            int i;
            var delta = t1 - t0;
            var halfDelta = delta * 0.5F;
            // first step
            derive(t0, userData, state, derivatives);
            for (i = 0; i < dimension; i++) tmpState[i] = state[i] + halfDelta * derivatives[i];
            // second step
            derive(t0 + halfDelta, userData, tmpState, derivatives);
            for (i = 0; i < dimension; i++) newState[i] = state[i] + delta * derivatives[i];
            return delta;
        }
    }

    public class ODE_RK4 : ODE
    {
        protected float[] tmpState;
        protected float[] d1;              // derivatives
        protected float[] d2;
        protected float[] d3;
        protected float[] d4;

        public ODE_RK4(int dim, DeriveFunction dr, object ud)
        {
            dimension = dim;
            derive = dr;
            userData = ud;
            tmpState = new float[dim];
            d1 = new float[dim];
            d2 = new float[dim];
            d3 = new float[dim];
            d4 = new float[dim];
        }
        public override float Evaluate(float[] state, float[] newState, float t0, float t1)
        {
            int i;
            var delta = t1 - t0;
            var halfDelta = delta * 0.5F;
            // first step
            derive(t0, userData, state, d1);
            for (i = 0; i < dimension; i++) tmpState[i] = state[i] + halfDelta * d1[i];
            // second step
            derive(t0 + halfDelta, userData, tmpState, d2);
            for (i = 0; i < dimension; i++) tmpState[i] = state[i] + halfDelta * d2[i];
            // third step
            derive(t0 + halfDelta, userData, tmpState, d3);
            for (i = 0; i < dimension; i++) tmpState[i] = state[i] + delta * d3[i];
            // fourth step
            derive(t0 + delta, userData, tmpState, d4);
            var sixthDelta = delta * (1.0F / 6.0F);
            for (i = 0; i < dimension; i++) newState[i] = state[i] + sixthDelta * (d1[i] + 2.0F * (d2[i] + d3[i]) + d4[i]);
            return delta;
        }
    }

    public class ODE_RK4Adaptive : ODE
    {
        protected float maxError;     // maximum allowed error
        protected float[] tmpState;
        protected float[] d1;              // derivatives
        protected float[] d1half;
        protected float[] d2;
        protected float[] d3;
        protected float[] d4;

        public ODE_RK4Adaptive(int dim, DeriveFunction dr, object ud)
        {
            dimension = dim;
            derive = dr;
            userData = ud;
            maxError = 0.01f;
            tmpState = new float[dim];
            d1 = new float[dim];
            d1half = new float[dim];
            d2 = new float[dim];
            d3 = new float[dim];
            d4 = new float[dim];
        }

        public override float Evaluate(float[] state, float[] newState, float t0, float t1)
        {
            int i, n;
            float max, error;

            var delta = t1 - t0;
            for (n = 0; n < 4; n++)
            {
                var halfDelta = delta * 0.5F;
                var fourthDelta = delta * 0.25F;
                float sixthDelta;
                // first step of first half delta
                derive(t0, userData, state, d1);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + fourthDelta * d1[i];
                // second step of first half delta
                derive(t0 + fourthDelta, userData, tmpState, d2);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + fourthDelta * d2[i];
                // third step of first half delta
                derive(t0 + fourthDelta, userData, tmpState, d3);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + halfDelta * d3[i];
                // fourth step of first half delta
                derive(t0 + halfDelta, userData, tmpState, d4);
                sixthDelta = halfDelta * (1.0F / 6.0F);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + sixthDelta * (d1[i] + 2.0F * (d2[i] + d3[i]) + d4[i]);

                // first step of second half delta
                derive(t0 + halfDelta, userData, tmpState, d1half);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + fourthDelta * d1half[i];
                // second step of second half delta
                derive(t0 + halfDelta + fourthDelta, userData, tmpState, d2);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + fourthDelta * d2[i];
                // third step of second half delta
                derive(t0 + halfDelta + fourthDelta, userData, tmpState, d3);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + halfDelta * d3[i];
                // fourth step of second half delta
                derive(t0 + delta, userData, tmpState, d4);
                sixthDelta = halfDelta * (1.0F / 6.0F);
                for (i = 0; i < dimension; i++) newState[i] = state[i] + sixthDelta * (d1[i] + 2.0F * (d2[i] + d3[i]) + d4[i]);

                // first step of full delta
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + halfDelta * d1[i];
                // second step of full delta
                derive(t0 + halfDelta, userData, tmpState, d2);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + halfDelta * d2[i];
                // third step of full delta
                derive(t0 + halfDelta, userData, tmpState, d3);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + delta * d3[i];
                // fourth step of full delta
                derive(t0 + delta, userData, tmpState, d4);
                sixthDelta = delta * (1.0F / 6.0F);
                for (i = 0; i < dimension; i++) tmpState[i] = state[i] + sixthDelta * (d1[i] + 2.0F * (d2[i] + d3[i]) + d4[i]);

                // get max estimated error
                max = 0.0F;
                for (i = 0; i < dimension; i++)
                {
                    error = MathX.Fabs((newState[i] - tmpState[i]) / (delta * d1[i] + 1e-10F));
                    if (error > max) max = error;
                }
                error = max / maxError;

                if (error <= 1.0f) return delta * 4.0F;
                if (delta <= 1e-7F) return delta;
                delta *= 0.25F;
            }
            return delta;
        }

        void SetMaxError(float err)
        {
            if (err > 0.0f) maxError = err;
        }
    }
}