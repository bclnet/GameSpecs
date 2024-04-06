using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace System.Numerics
{
    [TestClass]
    public class Matrix4x4Tests
    {
        // Tests
        // SetRotationFromQuaternion
        // SetRotationFromMatrix3x3
        // SetTranslationFromVector3
        // SetScaleFromVector3
        // GetRotationAsQuaternion
        // GetRotationAsMatrix3x3
        // CreateFromQuaternion(Quaternion)

        const float DELTA = 0.000001f;

        [TestMethod]
        public void SetRotationAndTranslationFromQuaternion()
        {
            var quat = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
            var translation = new Vector3(2.0f, 3.0f, 4.0f);

            var matrix = Matrix4x4.CreateFromQuaternion(quat);
            matrix.Translation = translation;

            Assert.AreEqual(1.0, matrix.M11, DELTA);
            Assert.AreEqual(0.0, matrix.M21, DELTA);
            Assert.AreEqual(2.0, matrix.M41, DELTA);
            Assert.AreEqual(3.0, matrix.M42, DELTA);
            Assert.AreEqual(4.0, matrix.M43, DELTA);
        }

        [TestMethod]
        public void GetTransformFromParts_TwoParameters()
        {
            var vector3 = GetTestVector3();
            var rotation = GetTestMatrix33();

            var matrix = Polyfill.CreateTransformFromParts(vector3, rotation);
            Assert.AreEqual(0.11f, matrix.M11);
            var scale = matrix.GetScale();
        }

        [TestMethod]
        public void Matrix4x4_CreateScale_ProperElements()
        {
            var matrix = Matrix4x4.CreateScale(GetTestVector3()); // (0.5, 0.6, -0.5)

            Assert.AreEqual(0.5, matrix.M11, AssertX.DELTA);
            Assert.AreEqual(0.600000023, matrix.M22, AssertX.DELTA);
            Assert.AreEqual(-0.5, matrix.M33, AssertX.DELTA);
        }

        [TestMethod]
        public void Matrix4x4_Transpose()
        {
            var actual = GetTestMatrix4x4();
            var expected = Matrix4x4.Transpose(actual);

            Assert.AreEqual(expected.M11, actual.M11, AssertX.DELTA);
            Assert.AreEqual(expected.M21, actual.M12, AssertX.DELTA);
            Assert.AreEqual(expected.M31, actual.M13, AssertX.DELTA);
            Assert.AreEqual(expected.M41, actual.M14, AssertX.DELTA);
            Assert.AreEqual(expected.M12, actual.M21, AssertX.DELTA);
            Assert.AreEqual(expected.M22, actual.M22, AssertX.DELTA);
            Assert.AreEqual(expected.M32, actual.M23, AssertX.DELTA);
        }

        [TestMethod]
        public void ConvertMatrix3x4_To_Matrix4x4()
        {
            var buffer = TestHelper.GetBone1WorldToBoneBytes();

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            var m34 = reader.ReadMatrix3x4();
            var m = m34.ConvertToTransformMatrix();

            Assert.AreEqual(0, m.M11, DELTA);
            Assert.AreEqual(0, m.M12, DELTA);
            Assert.AreEqual(-1, m.M13, DELTA);
            Assert.AreEqual(0.0233046, m.M14, DELTA);
            Assert.AreEqual(0.9999999, m.M21, DELTA);
            Assert.AreEqual(-1.629207e-07, m.M22, DELTA);
            Assert.AreEqual(-3.264332e-22, m.M23, DELTA);
            Assert.AreEqual(-1.659635e-16, m.M24, DELTA);
            Assert.AreEqual(-1.629207e-07, m.M31, DELTA);
            Assert.AreEqual(-0.9999999, m.M32, DELTA);
            Assert.AreEqual(7.549789e-08, m.M33, DELTA);
            Assert.AreEqual(-2.778125e-09, m.M34, DELTA);
            Assert.AreEqual(0, m.M41, DELTA);
            Assert.AreEqual(0, m.M42, DELTA);
            Assert.AreEqual(0, m.M43, DELTA);
            Assert.AreEqual(1, m.M44, DELTA);
        }

        [TestMethod]
        public void CompareBPM_Bone1()
        {
            var expectedBPM = TestHelper.GetExpectedBone1BPM();

            var buffer = TestHelper.GetBone1WorldToBoneBytes();

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            Matrix4x4 actualBPM;
            Matrix4x4.Invert(reader.ReadMatrix3x4().ConvertToTransformMatrix(), out actualBPM);

            AssertX.AreEqual(expectedBPM, actualBPM, AssertX.DELTA);
        }

        Vector3 GetTestVector3()
           => new()
           {
               X = 0.5f,
               Y = 0.6f,
               Z = -0.5f
           };

        Matrix3x3 GetTestMatrix33()
            => new()
            {
                M11 = 0.11f,
                M12 = 0.12f,
                M13 = 0.13f,
                M21 = 0.21f,
                M22 = 0.22f,
                M23 = 0.23f,
                M31 = 0.31f,
                M32 = 0.32f,
                M33 = 0.33f
            };

        Matrix4x4 GetTestMatrix4x4()
            => new()
            {
                M11 = 0.11f,
                M12 = 0.12f,
                M13 = 0.13f,
                M14 = 1.0f,
                M21 = 0.21f,
                M22 = 0.22f,
                M23 = 0.23f,
                M24 = 2.0f,
                M31 = 0.31f,
                M32 = 0.32f,
                M33 = 0.33f,
                M34 = 3.0f,
                M41 = 10f,
                M42 = 11f,
                M43 = 12f,
                M44 = 1f
            };

        Matrix4x4 GetTestMatrix4x4WithTranslation()
            => new()
            {
                M11 = -1.000000f,
                M12 = 0.000001f,
                M13 = 0.000009f,
                M14 = 0.000000f,
                M21 = -0.000005f,
                M22 = -0.866025f,
                M23 = -0.500000f,
                M24 = 0.000000f,
                M31 = 0.000008f,
                M32 = -0.500000f,
                M33 = 0.866025f,
                M34 = 0.000000f,
                M41 = 183.048630f / 100,
                M42 = -244.434143f / 100,
                M43 = -154.250488f / 100,
                M44 = 0.000000f
            };
    }
}
