//namespace System.Numerics.Unused
//{
//    public class Vector2
//    {
//        float _x;
//        float _y;

//        ByteArray xBA = new ByteArray();
//        ByteArray yBA = new ByteArray();

//        public float X { get => xBA.float1; set { _x = value; xBA.float1 = value; } }
//        public float Y { get => yBA.float1; set { _y = value; yBA.float1 = value; } }

//        public int Xint { get => xBA.int1; set => xBA.int1 = value; }
//        public int Yint { get => yBA.int1; set => yBA.int1 = value; }

//        public uint Xuint { get => xBA.uint1; set => xBA.uint1 = value; }
//        public uint Yuint { get => yBA.uint1; set => yBA.uint1 = value; }

//        public Vector2() { }
//        public Vector2(double x, double y) { _x = (float)x; _y = (float)y; }
//        public Vector2(Vector2 vector) { _x = vector.X; _y = vector.Y; }

//        public float this[int index]
//        {
//            get => index switch
//            {
//                0 => X,
//                1 => Y,
//                _ => throw new ArgumentOutOfRangeException(nameof(index), "Indices must run from 0 to 1."),
//            };
//            set
//            {
//                switch (index)
//                {
//                    case 0: X = value; break;
//                    case 1: Y = value; break;
//                    default: throw new ArgumentOutOfRangeException(nameof(index), "Indices must run from 0 to 1.");
//                }
//            }
//        }

//        public override int GetHashCode()
//        {
//            unchecked
//            {
//                var hash = 17;
//                hash = hash * 23 + X.GetHashCode();
//                hash = hash * 23 + Y.GetHashCode();
//                return hash;
//            }
//        }

//        public override bool Equals(object obj) => obj == null ? false : obj is Vector2 ? this == (Vector2)obj : false;
//        public override string ToString() => $"{X},{Y}";
//        public static bool operator ==(Vector2 left, Vector2 right) => right is null ? left is null : left.X == right.X && left.Y == right.Y;
//        public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);
//        public bool IsZero(float epsilon = 0) => Math.Abs(X) <= epsilon && Math.Abs(Y) <= epsilon;
//        public float Dot(Vector2 v) => X * v.X + Y * v.Y;
//    }
//}
