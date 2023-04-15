using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using StereoKit.UIX.Controls;
using NView = StereoKit.UIX.Controls.View;
using MRect = Microsoft.Maui.Graphics.Rect;
using DSizeF = System.Drawing.SizeF;
using StereoKit.Controls.Controls;

namespace StereoKit.Maui.Platform
{
    public partial class WrapperView : ViewGroup, IMeasurable
    {
        //Lazy<SkiaGraphicsView> _drawableCanvas;
        //Lazy<MauiClipperView> _clipperView;
        NView? _content;
        SKMauiDrawable _mauiDrawable;

        public WrapperView() : base()
        {
            //_drawableCanvas = new Lazy<SkiaGraphicsView>(() =>
            //{
            //	var view = new SkiaGraphicsView()
            //	{
            //		Drawable = _mauiDrawable
            //	};
            //	view.Show();
            //	Children.Add(view);
            //	view.Lower();
            //	return view;
            //});

            //_clipperView = new Lazy<MauiClipperView>(() =>
            //{
            //	var clipper = new MauiClipperView();
            //	clipper.Show();
            //	Children.Add(clipper);
            //	SetupClipperView(clipper);
            //	return clipper;
            //});

            //LayoutUpdated += OnLayout;
        }

        public NView? Content
        {
            get => _content;
            set
            {
                UpdateContent(value, _content);
                _content = value;
            }
        }

        bool NeedToUpdateCanvas => /*_drawableCanvas.IsValueCreated ||*/ _mauiDrawable.Background != null || _mauiDrawable.Shape != null || _mauiDrawable.Border != null || _mauiDrawable.Shadow != null;

        public void UpdateBackground(Paint? paint)
        {
            _mauiDrawable.Background = paint;
            UpdateDrawableCanvas();
        }

        public void UpdateShape(IShape? shape)
        {
            _mauiDrawable.Shape = shape;
            UpdateDrawableCanvas();
        }

        public void UpdateBorder(IBorderStroke? border)
            => Border = border;

        void ShadowChanged()
        {
            //_mauiDrawable.Shadow = Shadow;
            //UpdateDrawableCanvas(true);
        }

        void ClipChanged()
        {
            //_mauiDrawable.Clip = Clip;
            //if (_clipperView.IsValueCreated || Clip != null)
            //	_clipperView.Value.Clip = Clip;
        }

        void BorderChanged()
        {
            //_mauiDrawable.Border = Border;
            //UpdateShape(Border?.Shape);
            //UpdateDrawableCanvas(Border != null);
        }

        void UpdateDrawableCanvas(bool geometryUpdate = false)
        {
            //if (NeedToUpdateCanvas)
            //{
            //	if (geometryUpdate)
            //		UpdateDrawableCanvasGeometry();
            //	_drawableCanvas.Value.Invalidate();
            //}
        }

        void OnLayout(object? sender, EventArgs e)
        {
            if (Content != null)
                Content.UpdateBounds(new Rect(0, 0, Size.Width, Size.Height));

            //if (_clipperView.IsValueCreated)
            //	_clipperView.Value.UpdateBounds(new TRect(0, 0, Size.Width, Size.Height));

            UpdateDrawableCanvas(true);
        }

        //void SetupClipperView(MauiClipperView clipper)
        //{
        //	if (_content != null)
        //	{
        //		Children.Remove(_content);
        //		clipper.Add(_content);
        //	}
        //}

        void UpdateContent(NView? newValue, NView? oldValue)
        {
            // remove content from WrapperView
            if (oldValue != null)
            {
                //if (_clipperView.IsValueCreated) _clipperView.Value.Remove(oldValue);
                //else
                Children.Remove(oldValue);
            }

            if (newValue != null)
            {
                //if (_clipperView.IsValueCreated) _clipperView.Value.Add(newValue);
                //else
                Children.Add(newValue);
            }
        }

        void UpdateDrawableCanvasGeometry()
        {
            //var bounds = new MRect(0, 0, SizeWidth, SizeHeight);
            //if (Shadow != null)
            //{
            //	var shadowThinkness = Shadow.GetShadowMargin();
            //	_mauiDrawable.ShadowThickness = shadowThinkness;
            //	bounds = bounds.ToDP().ExpandTo(shadowThinkness).ToPixel();
            //}
            //_drawableCanvas.Value.UpdateBounds(bounds);
        }

        DSizeF IMeasurable.Measure(double availableWidth, double availableHeight)
        {
            if (Content is IMeasurable measurable) return measurable.Measure(availableWidth, availableHeight);
            else if (Content != null) return Content.NaturalSize;
            else return NaturalSize;
        }
    }
}
