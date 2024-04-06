using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Numerics;

namespace OpenStack.PolyIOTests.System.IO
{
    [TestClass]
    public class BinaryReaderTests
    {
        const float DELTA = 0.0000001f;

        [TestMethod]
        public void ReadQuaternion_SingleType_0001()
        {
            var buffer = new byte[] {
                0x0, 0x0, 0x0, 0x0,
                0x0, 0x0, 0x0, 0x0,
                0x0, 0x0, 0x0, 0x0,
                0x0, 0x0, 0x80, 0x3F};

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadQuaternion();

            Assert.AreEqual(0, quat.X);
            Assert.AreEqual(0, quat.Y);
            Assert.AreEqual(0, quat.Z);
            Assert.AreEqual(1, quat.W);
        }

        [TestMethod]
        public void ReadQuaternion_HalfType_0001()
        {
            var buffer = new byte[] {
                0x0, 0x0,
                0x0, 0x0,
                0x0, 0x0,
                0x80, 0x3F};

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadHalfQuaternion();

            Assert.AreEqual(0, quat.X);
            Assert.AreEqual(0, quat.Y);
            Assert.AreEqual(0, quat.Z);
            Assert.AreEqual(1.875, quat.W);
        }

        [TestMethod]
        public void ReadQuaternion_SingleType_4Floats()
        {
            var buffer = new byte[] {
                0xA3, 0x1F, 0xD2, 0x3D,
                0x9D, 0x2B, 0x2A, 0x3C,
                0xD7, 0xFB, 0x48, 0x3F,
                0xFE, 0x58, 0x1C, 0x3F};

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadHalfQuaternion();

            Assert.AreEqual(0.0074577, quat.X, DELTA);
            Assert.AreEqual(1.4550781, quat.Y, DELTA);
            Assert.AreEqual(0.0594787, quat.Z, DELTA);
            Assert.AreEqual(1.0410156, quat.W, DELTA);
        }

        [TestMethod]
        public void ReadVector3()
        {
            var buffer = new byte[] {
                0xA3, 0x1F, 0xD2, 0x3D,
                0x9D, 0x2B, 0x2A, 0x3C,
                0xD7, 0xFB, 0x48, 0x3F,
                0xFE, 0x58, 0x1C, 0x3F};

            var expected = new Vector3(0.102599405f, 0.010386375f, 0.7850928f);

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            var vector = reader.ReadVector3();

            Assert.AreEqual(expected, vector);
        }

        [TestMethod]
        public void ReadMatrix3x4_Bone1W2B()
        {
            // GetBone1WorldToBoneBytes
            var buffer = new byte[] {
                0x84, 0x94, 0x5D, 0xA8, 0x68, 0x21, 0xA2, 0xB3, 0xFF, 0xFF, 0x7F, 0xBF, 0x4A, 0xE9, 0xBE, 0x3C,
                0xFE, 0xFF, 0x7F, 0x3F, 0x4A, 0xEF, 0x2E, 0xB4, 0x18, 0x51, 0xC5, 0x9B, 0xC4, 0x57, 0x3F, 0xA5,
                0x4B, 0xEF, 0x2E, 0xB4, 0xFF, 0xFF, 0x7F, 0xBF, 0x68, 0x21, 0xA2, 0x33, 0x4A, 0xE9, 0x3E, 0xB1
            };

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            var m = reader.ReadMatrix3x4();

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
        }
    }
}
