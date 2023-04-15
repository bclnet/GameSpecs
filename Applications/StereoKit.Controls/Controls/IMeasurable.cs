using System.Drawing;

namespace StereoKit.Controls.Controls
{
    public interface IMeasurable
    {
        SizeF Measure(double availableWidth, double availableHeight);
    }
}