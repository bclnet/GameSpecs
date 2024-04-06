using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public float x;    // horiz position
        public float y;    // vert position
        public float w;    // width
        public float h;    // height;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle(in Vector4 v)
        {
            x = v.x;
            y = v.y;
            w = v.z;
            h = v.w;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle(in Rectangle r)
        {
            x = r.x;
            y = r.y;
            w = r.w;
            h = r.h;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle(float ix, float iy, float iw, float ih)
        {
            x = ix; y = iy; w = iw; h = ih;
        }

        public float Bottom
            => y + h;

        public float Right
            => x + w;

        public void Offset(float x, float y)
        {
            this.x += x;
            this.y += y;
        }

        public bool Contains(float xt, float yt)
        {
            if (w == 0f && h == 0f) return false;
            if (xt >= x && xt <= Right && yt >= y && yt <= Bottom) return true;
            return false;
        }

        public void Empty()
            => x = y = w = h = 0f;

        public void ClipAgainst(in Rectangle r, bool sizeOnly)
        {
            if (!sizeOnly)
            {
                if (x < r.x) x = r.x;
                if (y < r.y) y = r.y;
            }
            if (x + w > r.x + r.w) w = (r.x + r.w) - x;
            if (y + h > r.y + r.h) h = (r.y + r.h) - y;
        }

        public void Rotate(float a, out Rectangle o)
        {
            float c, s; Vector3 p1 = new(), p2 = new(), p3 = new(), center = new();

            center.Set((x + w) / 2f, (y + h) / 2f, 0);
            p1.Set(x, y, 0f);
            p2.Set(Right, y, 0f);
            p3.Set(x, Bottom, 0f);
            if (a != 0f)
            {
                s = (float)Math.Sin(MathX.DEG2RAD(a));
                c = (float)Math.Cos(MathX.DEG2RAD(a));
            }
            else s = c = 0f;
            Platform.RotateVector(ref p1, center, a, c, s);
            Platform.RotateVector(ref p2, center, a, c, s);
            Platform.RotateVector(ref p3, center, a, c, s);
            o = new();
            o.x = p1.x;
            o.y = p1.y;
            o.w = (p2 - p1).Length;
            o.h = (p3 - p1).Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle operator +(Rectangle _, in Rectangle a)
        {
            _.x += a.x;
            _.y += a.y;
            _.w += a.w;
            _.h += a.h;
            return _;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle operator -(Rectangle _, in Rectangle a)
        {
            _.x -= a.x;
            _.y -= a.y;
            _.w -= a.w;
            _.h -= a.h;
            return _;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle operator /(Rectangle _, in Rectangle a)
        {
            _.x /= a.x;
            _.y /= a.y;
            _.w /= a.w;
            _.h /= a.h;
            return _;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle operator *(Rectangle _, float a)
        {
            var inva = 1f / a;
            _.x *= inva;
            _.y *= inva;
            _.w *= inva;
            _.h *= inva;
            return _;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Rectangle _, in Rectangle a)
            => _.x == a.x && _.y == a.y && _.w == a.w && a.h != 0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Rectangle _, in Rectangle a)
            => _.x != a.x && _.y != a.y && _.w != a.w && a.h == 0f;
        public override bool Equals(object obj)
            => obj is Rectangle q && this == q;
        public override int GetHashCode()
            => base.GetHashCode();

        public unsafe ref float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (float* p = &x) return ref p[index]; }
        }

        public override string ToString()
            => $"{x:2} {y:2} {w:2} {h:2}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector4 ToVec4()
            => ref reinterpret.cast_vec4(this);
    }

    public class Region
    {
        protected List<Rectangle> rects = new();

        public void Empty()
            => rects.Clear();

        public bool Contains(float xt, float yt)
        {
            var c = rects.Count;
            for (var i = 0; i < c; i++) if (rects[i].Contains(xt, yt)) return true;
            return false;
        }

        public void AddRect(float x, float y, float w, float h)
            => rects.Add(new Rectangle(x, y, w, h));

        public int RectCount()
             => rects.Count;

        public Rectangle? GetRect(int index)
            => index >= 0 && index < rects.Count ? rects[index] : null;
    }
}