using GameSpec.Metadata;
using OpenStack;
using OpenStack.Graphics;
using OpenStack.Graphics.Controls;
using OpenStack.Graphics.OpenGL.Renderer1.Renderers;
using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using static GameSpec.Formats.Unknown.IUnknownFileObject;

namespace GameSpec.App.Explorer.Controls1
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

            HandleResize();
            Camera.SetLocation(new Vector3(200));
            Camera.LookAt(new Vector3(0));

            var texture = graphic.TextureManager.LoadTexture(source, out _);
            Renderers.Add(new TextureRenderer(graphic, texture));
        }

        readonly HashSet<TextureRenderer> Renderers = new();

        void OnPaint(object sender, RenderEventArgs e)
        {
            foreach (var renderer in Renderers) renderer.Render(e.Camera, RenderPass.Both);
        }
    }
}
