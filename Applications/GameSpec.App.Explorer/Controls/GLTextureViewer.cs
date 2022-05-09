using OpenStack;
using OpenStack.Graphics;
using OpenStack.Graphics.Controls;
using OpenStack.Graphics.OpenGL.Renderers;
using OpenStack.Graphics.Renderer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GameSpec.Metadata.View
{
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

        void OnProperty()
        {
            if (Graphic == null || Source == null) return;
            var graphic = Graphic as IOpenGLGraphic;
            var source = Source is ITextureInfo z ? z
                : Source is IRedirected<ITextureInfo> y ? y.Value
                : null;
            if (source == null) return;
            var texture = graphic.TextureManager.LoadTexture(source, out var _);
            Renderers.Add(new TextureRenderer(graphic, texture));
        }

        HashSet<TextureRenderer> Renderers { get; } = new HashSet<TextureRenderer>();

        void OnPaint(object sender, RenderEventArgs e)
        {
            foreach (var renderer in Renderers) renderer.Render(e.Camera, RenderPass.Both);
        }
    }
}
