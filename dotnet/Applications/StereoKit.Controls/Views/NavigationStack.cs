using StereoKit.UIX.Controls;
using System.Collections.Generic;

namespace StereoKit.Maui.Platform
{
    public class NavigationStack : Stack<View>
    {
        public LayoutParamPolicies HeightSpecification { get; set; }
        public LayoutParamPolicies WidthSpecification { get; set; }
        public ResizePolicyType WidthResizePolicy { get; set; }
        public ResizePolicyType HeightResizePolicy { get; set; }

        //public void Clear() { }
        public void Push(View content, bool v) => Push(content);
    }
}
