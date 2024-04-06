using OpenStack;
using OpenStack.Graphics;
using OpenStack.Graphics.Controls;
using OpenStack.Graphics.OpenGL.Renderer1.Renderers;
using OpenStack.Graphics.Renderer1;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using Key = OpenTK.Input.Key;

namespace GameX.App.Explorer.Controls1
{
    //was:Renderer/GLTextureViewer
    public class GLTextureViewer : GLViewerControl
    {
        public GLTextureViewer()
        {
            GLPaint += OnPaint;
            Unloaded += (s, e) => { GLPaint -= OnPaint; };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty GraphicProperty = DependencyProperty.Register(nameof(Graphic), typeof(object), typeof(GLTextureViewer),
            new PropertyMetadata((d, e) => (d as GLTextureViewer).OnProperty()));

        public IOpenGraphic Graphic
        {
            get => GetValue(GraphicProperty) as IOpenGraphic;
            set => SetValue(GraphicProperty, value);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(GLTextureViewer),
            new PropertyMetadata((d, e) => (d as GLTextureViewer).OnProperty()));

        public object Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        protected override void HandleResize()
        {
            var source = Source is ITexture z ? z
                : Source is IRedirected<ITexture> y ? y.Value
                : null;
            if (source == null) return;
            if (source.Width > 1024 || source.Height > 1024 || false) { base.HandleResize(); return; }
            Camera.SetViewportSize(source.Width, source.Height);
            RecalculatePositions();
        }

        void OnProperty()
        {
            if (Graphic == null || Source == null) return;
            var graphic = Graphic as IOpenGLGraphic;
            var source = Source is ITexture z ? z
                : Source is IRedirected<ITexture> y ? y.Value
                : null;
            if (source == null) return;
            source.Select(Id);

            HandleResize();
            Camera.SetLocation(new Vector3(200));
            Camera.LookAt(new Vector3(0));

            graphic.TextureManager.DeleteTexture(source);
            var texture = graphic.TextureManager.LoadTexture(source, out _, rng);
            Renderers.Clear();
            Renderers.Add(new TextureRenderer(graphic, texture) { Background = background });
        }

        bool background;
        Range rng = 0..;
        readonly HashSet<TextureRenderer> Renderers = new();

        void OnPaint(object sender, RenderEventArgs e)
        {
            HandleInput(Keyboard.GetState());
            foreach (var renderer in Renderers) renderer.Render(e.Camera, RenderPass.Both);
        }

        Key[] Keys = new[] { Key.Q, Key.W, Key.A, Key.Z, Key.Space, Key.Tilde };
        HashSet<Key> KeyDowns = new();
        int Id = 0;

        public void HandleInput(KeyboardState keyboardState)
        {
            foreach (var key in Keys)
                if (!KeyDowns.Contains(key) && keyboardState.IsKeyDown(key)) KeyDowns.Add(key);
            foreach (var key in KeyDowns)
                if (keyboardState.IsKeyUp(key))
                {
                    KeyDowns.Remove(key);
                    switch (key)
                    {
                        case Key.W: Select(++Id); break;
                        case Key.Q: Select(--Id); break;
                        case Key.A: MovePrev(); break;
                        case Key.Z: MoveNext(); ; break;
                        case Key.Space: MoveReset(); break;
                        case Key.Tilde: ToggleBackground(); break;
                    }
                }
        }

        void Select(int id)
        {
            var source = Source is ITexture z ? z
                : Source is IRedirected<ITexture> y ? y.Value
                : null;
            if (source == null) return;
            source.Select(id);
            OnProperty();
            Views.FileExplorer.Instance.OnInfoUpdated();
        }
        void MoveReset() { Id = 0; rng = 0..; OnProperty(); }
        void MoveNext() { if (rng.Start.Value < 10) rng = new(rng.Start.Value + 1, rng.End); OnProperty(); }
        void MovePrev() { if (rng.Start.Value > 0) rng = new(rng.Start.Value - 1, rng.End); OnProperty(); }
        void ToggleBackground() { background = !background; OnProperty(); }
    }
}
