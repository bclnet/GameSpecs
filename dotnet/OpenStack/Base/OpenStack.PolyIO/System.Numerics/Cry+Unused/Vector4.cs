//namespace System.Numerics.Unused
//{
//    public class Vector4
//    {
//        float _x;
//        float _y;
//        float _z;
//        float _w;

//        ByteArray xBA = new ByteArray();
//        ByteArray yBA = new ByteArray();
//        ByteArray zBA = new ByteArray();
//        ByteArray wBA = new ByteArray();

//        public float X { get => xBA.float1; set { _x = value; xBA.float1 = value; } }
//        public float Y { get => yBA.float1; set { _y = value; yBA.float1 = value; } }
//        public float Z { get => zBA.float1; set { _z = value; zBA.float1 = value; } }
//        public float W { get => wBA.float1; set { _w = value; wBA.float1 = value; } }

//        public int Xint { get => xBA.int1; set => xBA.int1 = value; }
//        public int Yint { get => yBA.int1; set => yBA.int1 = value; }
//        public int Zint { get => zBA.int1; set => zBA.int1 = value; }
//        public int Wint { get => wBA.int1; set => wBA.int1 = value; }

//        public uint Xuint { get => xBA.uint1; set => xBA.uint1 = value; }
//        public uint Yuint { get => yBA.uint1; set => yBA.uint1 = value; }
//        public uint Zuint { get => zBA.uint1; set => zBA.uint1 = value; }
//        public uint Wuint { get => wBA.uint1; set => wBA.uint1 = value; }

//        public Vector4(double x, double y, double z, double w) { _x = (float)x; _y = (float)y; _z = (float)z; _w = (float)w; }

//        public float this[int index]
//        {
//            get => index switch
//            {
//                0 => X,
//                1 => Y,
//                2 => Z,
//                3 => W,
//                _ => throw new ArgumentOutOfRangeException(nameof(index), "Indices must run from 0 to 3!"),
//            };
//            set
//            {
//                switch (index)
//                {
//                    case 0: X = value; break;
//                    case 1: Y = value; break;
//                    case 2: Z = value; break;
//                    case 3: W = value; break;
//                    default: throw new ArgumentOutOfRangeException(nameof(index), "Indices must run from 0 to 3!");
//                }
//            }
//        }

//        public Vector3 ToVector3()
//        {
//            var r = new Vector3();
//            if (_w == 0) { r.X = _x; r.Y = _y; r.Z = _z; }
//            else { r.X = _x / _w; r.Y = _y / _w; r.Z = _z / _w; }
//            return r;
//        }

//        //public void LogVector4()
//        //{
//        //    Log("=============================================");
//        //    Log($"x:{_x:F7}  y:{_y:F7}  z:{_z:F7} w:{_w:F7}");
//        //}

//        public override int GetHashCode()
//        {
//            unchecked
//            {
//                var hash = 17;
//                hash = hash * 23 + X.GetHashCode();
//                hash = hash * 23 + Y.GetHashCode();
//                hash = hash * 23 + Z.GetHashCode();
//                hash = hash * 23 + W.GetHashCode();
//                return hash;
//            }
//        }

//        public override bool Equals(object obj) => obj == null ? false : obj is Vector4 ? this == (Vector4)obj : false;
//        public override string ToString() => $"{X},{Y},{Z},{W}";
//        public static implicit operator Vector4(Vector3 vec3) => new Vector4(vec3.X, vec3.Y, vec3.Z, 0);
//        public static bool operator ==(Vector4 left, Vector4 right) => right is null ? left is null : left.X == right.X && left.Y == right.Y && left.Z == right.Z && left.W == right.W;
//        public static bool operator !=(Vector4 left, Vector4 right) => !(left == right);
//        public bool IsZero(float epsilon = 0) => Math.Abs(X) <= epsilon && Math.Abs(Y) <= epsilon && Math.Abs(Z) <= epsilon && Math.Abs(W) <= epsilon;
//        public float Dot(Vector4 v) => X * v.X + Y * v.Y + Z * v.Z + W * v.W;
//    }
//}
