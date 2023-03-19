using Microsoft.Maui;
using System;

namespace StereoKit.Maui.Handlers
{
    public partial class SKToolbarHandler : SKElementHandler<IToolbar, object>
    {
        protected override object CreatePlatformElement() => throw new NotImplementedException();

        public static void MapTitle(ISKToolbarHandler arg1, IToolbar arg2) { }
    }
}