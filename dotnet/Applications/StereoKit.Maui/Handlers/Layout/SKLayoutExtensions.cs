using Microsoft.Maui;
using System.Linq;

namespace StereoKit.Maui.Handlers
{
    internal static class SKLayoutExtensions
    {
        public static IOrderedEnumerable<IView> OrderByZIndex(this ILayout layout) => layout.OrderBy(v => v.ZIndex);

        public static int GetLayoutHandlerIndex(this ILayout layout, IView view)
            => layout.Count switch
            {
                0 => -1,
                1 => view == layout[0] ? 0 : -1,
                _ => layout.OrderByZIndex().ToList().IndexOf(view),
            };
    }
}
