using Microsoft.Xna.Framework;

namespace Monocle {
    public static class RectangleExt {
        public static Point TopLeft(this Rectangle rect) {
            return rect.Location;
        }

        public static Point TopRight(this Rectangle rect) {
            return new Point(rect.Right, rect.Y);
        }

        public static Point BottomLeft(this Rectangle rect) {
            return new Point(rect.X, rect.Bottom);
        }

        public static Point BottomRight(this Rectangle rect) {
            return new Point(rect.Right, rect.Bottom);
        }

        public static Rectangle ClampTo(this Rectangle rect, Rectangle clamp) {
            if (rect.X < clamp.X) {
                rect.Width -= clamp.X - rect.X;
                rect.X = clamp.X;
            }

            if (rect.Y < clamp.Y) {
                rect.Height -= clamp.Y - rect.Y;
                rect.Y = clamp.Y;
            }

            if (rect.Right > clamp.Right)
                rect.Width = clamp.Right - rect.X;
            if (rect.Bottom > clamp.Bottom)
                rect.Height = clamp.Bottom - rect.Y;

            return rect;
        }
    }
}
