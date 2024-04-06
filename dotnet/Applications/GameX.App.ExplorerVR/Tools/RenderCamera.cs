using StereoKit;
using System;
using System.IO;
using Color = StereoKit.Color;
using IStepper = StereoKit.Framework.IStepper;

namespace GameX.App.Explorer.Tools
{
    public class RenderCamera : IStepper
    {
        public bool Enabled => true;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int FrameRate { get; private set; }

        public string folder = "Video";
        public Pose from;
        public Pose at;
        public float damping = 8;

        int _frameIndex;
        float _frameTime;
        Tex _frameSurface;
        Material _frameMaterial;
        bool _recording = false;
        Pose _renderFrom;
        bool _previewing = false;

        public RenderCamera(Pose startAt, int width = 500, int height = 500, int framerate = 12)
        {
            Width = width;
            Height = height;
            FrameRate = framerate;

            at = startAt;
            from = new Pose(at.position + V.XYZ(0, 0, 0.1f) * at.orientation, at.orientation);
            _renderFrom = at;
        }

        public bool Initialize()
        {
            _frameSurface = new Tex(TexType.Rendertarget, TexFormat.Rgba32);
            _frameSurface.SetSize(Width, Height);
            _frameSurface.AddZBuffer(TexFormat.Depth32);
            _frameMaterial = Default.MaterialUnlit.Copy();
            _frameMaterial[MatParamName.DiffuseTex] = _frameSurface;
            _frameMaterial.FaceCull = Cull.None;
            return true;
        }

        public void Shutdown() { }

        public void Step()
        {
            UI.PushId("RenderCameraWidget");
            UI.Handle("from", ref from, new Bounds(Vec3.One * 0.02f), true);
            UI.HandleBegin("at", ref at, new Bounds(Vec3.One * 0.02f), true);
            UI.ToggleAt("On", ref _previewing, new Vec3(4, -2, 0) * U.cm, new Vec2(8 * U.cm, UI.LineHeight));
            if (_previewing && UI.ToggleAt("Record", ref _recording, new Vec3(4, -6, 0) * U.cm, new Vec2(8 * U.cm, UI.LineHeight))) { _frameTime = Time.ElapsedUnscaledf; _frameIndex = 0; }
            UI.HandleEnd();
            UI.PopId();

            var fov = 10 + Math.Max(0, Math.Min(1, (Vec3.Distance(from.position, at.position) - 0.1f) / 0.2f)) * 110;
            Vec3 previewAt = at.position + at.orientation * Vec3.Up * 0.06f;
            Vec3 renderFrom = at.position + (at.position - from.position).Normalized * 0.06f;
            _renderFrom = Pose.Lerp(_renderFrom, new Pose(renderFrom, Quat.LookDir(at.position - from.position)), Time.Elapsedf * damping);

            Lines.Add(from.position, at.position, Color.White, 0.005f);
            from.orientation = at.orientation = Quat.LookDir(from.position - at.position);
            if (_previewing)
            {
                Hierarchy.Push(Matrix.TR(previewAt, Quat.LookAt(previewAt, Input.Head.position)));
                Default.MeshQuad.Draw(_frameMaterial, Matrix.S(V.XYZ(0.08f * ((float)Width / Height), 0.08f, 1)));
                Text.Add("" + (int)fov, Matrix.TS(-0.03f, 0, 0, 0.5f), TextAlign.CenterLeft);
                Hierarchy.Pop();
                Renderer.RenderTo(_frameSurface, _renderFrom.ToMatrix(), Matrix.Perspective(fov, (float)Width / Height, 0.01f, 100));
            }
            if (_recording) SaveFrame(FrameRate);
        }

        Color32[] buffer = null;
        void SaveFrame(int framerate)
        {
            var rateTime = 1.0f / framerate;
            if (_frameTime + rateTime < Time.TotalUnscaledf)
            {
                _frameTime = Time.TotalUnscaledf;
                _frameSurface.GetColors(ref buffer);

                Directory.CreateDirectory(folder);
                Stream writer = new FileStream($"{folder}/image{_frameIndex:D4}.bmp", FileMode.Create);
                WriteBitmap(writer, _frameSurface.Width, _frameSurface.Height, buffer);
                writer.Close();
                _frameIndex += 1;
            }
        }

        static void WriteBitmap(Stream stream, int width, int height, Color32[] imageData)
        {
            using var w = new BinaryWriter(stream);
            // define the bitmap file header
            w.Write((UInt16)0x4D42);                               // bfType;
            w.Write((UInt32)(14 + 40 + (width * height * 4)));     // bfSize;
            w.Write((UInt16)0);                                    // bfReserved1;
            w.Write((UInt16)0);                                    // bfReserved2;
            w.Write((UInt32)14 + 40);                              // bfOffBits;
            // define the bitmap information header
            w.Write((UInt32)40);                                   // biSize;
            w.Write((Int32)width);                                 // biWidth;
            w.Write((Int32)height);                                // biHeight;
            w.Write((UInt16)1);                                    // biPlanes;
            w.Write((UInt16)32);                                   // biBitCount;
            w.Write((UInt32)0);                                    // biCompression;
            w.Write((UInt32)(width * height * 4));                 // biSizeImage;
            w.Write((Int32)0);                                     // biXPelsPerMeter;
            w.Write((Int32)0);                                     // biYPelsPerMeter;
            w.Write((UInt32)0);                                    // biClrUsed;
            w.Write((UInt32)0);                                    // biClrImportant;
            // switch the image data from RGB to BGR
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var i = x + ((height - 1) - y) * width;
                    w.Write(imageData[i].b);
                    w.Write(imageData[i].g);
                    w.Write(imageData[i].r);
                    w.Write(imageData[i].a);
                }
        }
    }
}
