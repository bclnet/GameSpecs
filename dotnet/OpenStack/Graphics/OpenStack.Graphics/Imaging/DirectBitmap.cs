using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenStack.Graphics.Imaging
{
    public class DirectBitmap : IDisposable
    {
        bool disposed;

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }

        public Bitmap Bitmap { get; private set; }
        public int[] Bits { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public void SetPixel(int x, int y, Color color)
            => Bits[x + (y * Width)] = color.ToArgb();

        public Color GetPixel(int x, int y)
            => Color.FromArgb(Bits[x + (y * Width)]);

        public void Save(string path)
            => Bitmap.Save(path, ImageFormat.Png);
    }
}
