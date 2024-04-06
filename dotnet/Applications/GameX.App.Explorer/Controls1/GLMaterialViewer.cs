using OpenStack;
using OpenStack.Graphics;
using OpenStack.Graphics.Controls;
using OpenStack.Graphics.OpenGL.Renderer1.Renderers;
using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GameX.App.Explorer.Controls1
{
    //was:Renderer/GLMaterialViewer
    public class GLMaterialViewer : GLViewerControl
    {
        public GLMaterialViewer()
        {
            GLPaint += OnPaint;
            Unloaded += (a, b) => { GLPaint -= OnPaint; };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty GraphicProperty = DependencyProperty.Register(nameof(Graphic), typeof(object), typeof(GLMaterialViewer),
            new PropertyMetadata((d, e) => (d as GLMaterialViewer).OnProperty()));

        public IOpenGraphic Graphic
        {
            get => GetValue(GraphicProperty) as IOpenGraphic;
            set => SetValue(GraphicProperty, value);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(GLMaterialViewer),
            new PropertyMetadata((d, e) => (d as GLMaterialViewer).OnProperty()));

        public object Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        void OnProperty()
        {
            if (Graphic == null || Source == null) return;
            var graphic = Graphic as IOpenGLGraphic;
            var source = Source is IMaterial z ? z
                : Source is IRedirected<IMaterial> y ? y.Value
                : null;
            if (source == null) return;
            var material = graphic.MaterialManager.LoadMaterial(source, out var _);
            Renderers.Add(new MaterialRenderer(graphic, material));
        }

        readonly HashSet<MaterialRenderer> Renderers = new();

        void OnPaint(object sender, RenderEventArgs e)
        {
            foreach (var renderer in Renderers) renderer.Render(e.Camera, RenderPass.Both);
        }
    }
}
