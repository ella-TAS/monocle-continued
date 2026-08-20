using Microsoft.Xna.Framework;
using System;

namespace Monocle {
    public static class ColorExt {
        public static Color Invert(this Color color) {
            return new Color(255 - color.R, 255 - color.G, 255 - color.B, color.A);
        }

        public static string ToHex(this Color color) {
            return Convert.ToHexString([color.R, color.G, color.B]);
        }
    }
}
