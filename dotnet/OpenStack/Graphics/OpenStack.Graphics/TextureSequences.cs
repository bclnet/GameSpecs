using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics
{
    /// <summary>
    /// TextureSequences
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{OpenStack.Graphics.TextureSequences.Sequence}" />
    public class TextureSequences : List<TextureSequences.Sequence>
    {
        /// <summary>
        /// Frame
        /// </summary>
        public class Frame
        {
            /// <summary>
            /// Gets or sets the images.
            /// </summary>
            /// <value>
            /// The images.
            /// </value>
            public Image[] Images { get; set; }

            /// <summary>
            /// Gets or sets the display time.
            /// </summary>
            /// <value>
            /// The display time.
            /// </value>
            public float DisplayTime { get; set; }
        }

        /// <summary>
        /// Image
        /// </summary>
        public class Image
        {
            /// <summary>
            /// Gets or sets the cropped min.
            /// </summary>
            /// <value>
            /// The cropped min.
            /// </value>
            public Vector2 CroppedMin { get; set; }
            /// <summary>
            /// Gets or sets the cropped max.
            /// </summary>
            /// <value>
            /// The cropped max.
            /// </value>
            public Vector2 CroppedMax { get; set; }
            /// <summary>
            /// Gets or sets the uncropped min.
            /// </summary>
            /// <value>
            /// The uncropped min.
            /// </value>
            public Vector2 UncroppedMin { get; set; }
            /// <summary>
            /// Gets or sets the uncropped max.
            /// </summary>
            /// <value>
            /// The uncropped max.
            /// </value>
            public Vector2 UncroppedMax { get; set; }
            /// <summary>
            /// Gets a cropped rect.
            /// </summary>
            /// <value>
            /// The rect.
            /// </value>
            public Vector4<int> GetCroppedRect(int width, int height) => new Vector4<int>((int)(CroppedMin.X * width), (int)(CroppedMin.Y * height), (int)(CroppedMax.X * width), (int)(CroppedMax.Y * height));
            /// <summary>
            /// Gets an uncropped rect.
            /// </summary>
            /// <value>
            /// The rect.
            /// </value>
            public Vector4<int> GetUncroppedRect(int width, int height) => new Vector4<int>((int)(UncroppedMin.X * width), (int)(UncroppedMin.Y * height), (int)(UncroppedMax.X * width), (int)(UncroppedMax.Y * height));
        }

        /// <summary>
        /// Sequence
        /// </summary>
        public class Sequence
        {
            /// <summary>
            /// Gets or sets the frames.
            /// </summary>
            /// <value>
            /// The frames.
            /// </value>
            public IList<Frame> Frames { get; set; }
            /// <summary>
            /// Gets or sets the frames per second.
            /// </summary>
            /// <value>
            /// The frames per second.
            /// </value>
            public float FramesPerSecond { get; set; }
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }
            public bool Clamp { get; set; }
            public bool AlphaCrop { get; set; }
            public bool NoColor { get; set; }
            public bool NoAlpha { get; set; }
            public Dictionary<string, float> FloatParams { get; } = new Dictionary<string, float>();
        }
    }
}