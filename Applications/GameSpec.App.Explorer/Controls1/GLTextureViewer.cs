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

        //protected override void HandleResize()
        //{
        //    var source = Source is ITexture z ? z
        //        : Source is IRedirected<ITexture> y ? y.Value
        //        : null;
        //    if (source == null) return;
        //    Camera.SetViewportSize(source.Width, source.Height);
        //    RecalculatePositions();
        //}

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

            graphic.TextureManager.DeleteTexture(source);
            var texture = graphic.TextureManager.LoadTexture(source, out _, range);
            Renderers.Clear();
            Renderers.Add(new TextureRenderer(graphic, texture));
        }

        Range range = 0..;
        readonly HashSet<TextureRenderer> Renderers = new();

        void OnPaint(object sender, RenderEventArgs e)
        {
            HandleInput(Keyboard.GetState());
            foreach (var renderer in Renderers) renderer.Render(e.Camera, RenderPass.Both);
        }

        bool PrevPress, NextPress, ResetPress;
        public void HandleInput(KeyboardState keyboardState)
        {
            if (!PrevPress && keyboardState.IsKeyDown(Key.A)) PrevPress = true;
            else if (PrevPress && keyboardState.IsKeyUp(Key.A)) { PrevPress = false; MovePrev(); }
            if (!NextPress && keyboardState.IsKeyDown(Key.Z)) NextPress = true;
            else if (NextPress && keyboardState.IsKeyUp(Key.Z)) { NextPress = false; MoveNext(); }
            if (!ResetPress && keyboardState.IsKeyDown(Key.Space)) ResetPress = true;
            else if (ResetPress && keyboardState.IsKeyUp(Key.Space)) { ResetPress = false; MoveReset(); }
        }

        void MoveReset() { range = 0..; OnProperty(); }
        void MoveNext() { if (range.Start.Value < 10) range = new(range.Start.Value + 1, range.End); OnProperty(); }
        void MovePrev() { if (range.Start.Value > 0) range = new(range.Start.Value - 1, range.End); OnProperty(); }
    }
}
