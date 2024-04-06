using System.NumericsX.OpenStack;

namespace System.NumericsX
{
    public static class Program
    {
        static void Main()
        {
            // initialize math
            MathX.Init();

            // test idMatX
            //MatrixTest.Test();

            // test idPolynomial
            //Polynomial.Test();

            SimdTest.Test_f(new CmdArgs("test", false));
        }
    }
}