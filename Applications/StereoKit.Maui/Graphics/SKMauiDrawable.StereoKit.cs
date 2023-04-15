using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MRect = Microsoft.Maui.Graphics.Rect;

namespace StereoKit.Maui
{
	public class SKMauiDrawable : IDrawable
	{
		public IShadow? Shadow { get; set; }

		public Paint? Background { get; set; }

		public IBorderStroke? Border { get; set; }

		public Thickness ShadowThickness { get; set; }

		public IShape? Shape { get; set; }

		public IShape? Clip { get; set; }

		PathF GetBoundaryPath(MRect bounds)
		{
			if (Clip != null)
				return Clip.PathForBounds(bounds);

			if (Shape != null)
				return Shape.PathForBounds(bounds);

			var path = new PathF();
			path.AppendRectangle(bounds);
			return path;
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.SaveState();
			canvas.Translate((float)ShadowThickness.Left, (float)ShadowThickness.Top);
			RectF drawBounds = new MRect(0, 0, dirtyRect.Width - ShadowThickness.HorizontalThickness, dirtyRect.Height - ShadowThickness.VerticalThickness);
			var drawablePath = GetBoundaryPath(drawBounds);

			if (Shadow != null)
			{
				canvas.SaveState();
				var color = Shadow.Paint.ToColor() != null ? Shadow.Paint.ToColor()!.MultiplyAlpha(Shadow.Opacity) : Colors.Black.MultiplyAlpha(Shadow.Opacity);
				canvas.FillColor = color;
				canvas.SetShadow(
						new SizeF((float)Shadow.Offset.X, (float)Shadow.Offset.Y),
						(int)Shadow.Radius,
						color);
				canvas.FillPath(drawablePath);
				canvas.RestoreState();

				canvas.SaveState();
				canvas.StrokeColor = Colors.Transparent;
				canvas.DrawPath(drawablePath);
				canvas.ClipPath(drawablePath, WindingMode.EvenOdd);
				canvas.RestoreState();
			}

			if (Background != null)
			{
				canvas.SaveState();
				canvas.SetFillPaint(Background, drawBounds);
				canvas.FillPath(drawablePath);
				canvas.RestoreState();
			}

			if (Border != null)
			{
				canvas.SaveState();
				var borderPath = Border.Shape?.PathForBounds(drawBounds);
				if (borderPath != null && Border.StrokeThickness > 0)
				{
					canvas.MiterLimit = Border.StrokeMiterLimit;
					canvas.StrokeColor = Border.Stroke.ToColor();
					canvas.StrokeDashPattern = Border.StrokeDashPattern;
					canvas.StrokeLineCap = Border.StrokeLineCap;
					canvas.StrokeLineJoin = Border.StrokeLineJoin;
					canvas.StrokeSize = (float)Border.StrokeThickness;
					canvas.DrawPath(borderPath);
				}
				canvas.RestoreState();
			}
			canvas.RestoreState();
		}
	}
}