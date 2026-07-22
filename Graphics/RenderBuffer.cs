using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle {
    public class RenderBuffer {
        public RenderTarget2D Target;
        public static implicit operator RenderTarget2D(RenderBuffer buffer) => buffer.Target;

        public int Width {
            get => Target.Width;
            set => Resize(value, Height);
        }

        public int Height {
            get => Target.Height;
            set => Resize(Width, value);
        }

        public Rectangle Bounds => Target.Bounds;

        public RenderBuffer() { }

        public RenderBuffer(int width, int height) {
            Resize(width, height);
        }

        public void Resize(int newWidth, int newHeight) {
            if (Target != null && Width == newWidth && Height == newHeight) {
                return;
            }

            Target?.Dispose();
            Target = new RenderTarget2D(Engine.Instance.GraphicsDevice, newWidth, newHeight);
        }

        public void Dispose() {
            Target?.Dispose();
            Target = null;
        }
    }
}
