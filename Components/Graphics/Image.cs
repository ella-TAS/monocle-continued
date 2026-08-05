#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle {
    public class Image : GraphicsComponent {
        public MTexture Texture;
        public bool SnapToPixel;

        public Image(MTexture texture)
            : base(false) {
            Texture = texture;
            SnapToPixel = MonocleSettings.ImageSnapDefault;
        }

        internal Image(MTexture texture, bool active)
            : base(active) {
            Texture = texture;
        }

        public override void Render() {
            if (Texture != null) {
                // flip fix: respect trimmed transparent edges with FlipX/Y
                Vector2 flipOffset = Vector2.Zero;
                if (Effects != SpriteEffects.None) {
                    if (FlipX) {
                        flipOffset.X = Width - Texture.ClipRect.Width - 2f * Texture.DrawOffset.X;
                    }
                    if (FlipY) {
                        flipOffset.Y = Height - Texture.ClipRect.Height - 2f * Texture.DrawOffset.Y;
                    }
                    flipOffset = (flipOffset * Scale).Rotate(Rotation);
                }

                // snap the render position to the pixel grid
                Vector2 renderPos = RenderPosition + flipOffset;
                if (SnapToPixel) {
                    renderPos = renderPos.Round();
                }

                Texture.Draw(renderPos, Origin, Color, Scale, Rotation, Effects);
            }
        }

        public virtual float Width {
            get { return Texture.Width; }
        }

        public virtual float Height {
            get { return Texture.Height; }
        }

        public Image SetOrigin(float x, float y) {
            Origin.X = x;
            Origin.Y = y;
            return this;
        }

        public Image CenterOrigin() {
            Origin.X = Width / 2f;
            Origin.Y = Height / 2f;
            return this;
        }

        public Image JustifyOrigin(Vector2 at) {
            Origin.X = Width * at.X;
            Origin.Y = Height * at.Y;
            return this;
        }

        public Image JustifyOrigin(float x, float y) {
            Origin.X = Width * x;
            Origin.Y = Height * y;
            return this;
        }
    }
}
