namespace System.Drawing
{
    /// <summary>
    /// ColorX
    /// </summary>
    public static class ColorX
    {
        public static string ToRGBA(uint color)
        {
            // palette colors are natively stored in ARGB format
            var a = color >> 24;
            var r = (color >> 16) & 0xFF;
            var g = (color >> 8) & 0xFF;
            var b = color & 0xFF;
            return $"R: {r} G: {g} B: {b}{(a < 255 ? $" A: {a} " : null)}";
        }
    }
}
