using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Views.LayoutPanel;

namespace StereoKit.Maui.Handlers
{
    public partial class SKLayoutHandler : SKViewHandler<ILayout, PlatformView>
	{
		public void Add(IView view) => throw new NotImplementedException();
		public void Remove(IView view) => throw new NotImplementedException();
		public void Clear() => throw new NotImplementedException();
		public void Insert(int index, IView view) => throw new NotImplementedException();
		public void Update(int index, IView view) => throw new NotImplementedException();
		public void UpdateZIndex(IView view) => throw new NotImplementedException();

		protected override PlatformView CreatePlatformView() => new();
	}
}
