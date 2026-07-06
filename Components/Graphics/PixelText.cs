using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Monocle {
    public class PixelText : GraphicsComponent {
        public enum HorizontalAlign { Left, Center, Right };
        public enum VerticalAlign { Top, Center, Bottom };

        private struct Char {
            public Vector2 Offset;
            public PixelFontCharacter CharData;
            public Rectangle Bounds;
        }

        private List<Char> characters = new List<Char>();
        private PixelFont font;
        private PixelFontSize size;
        private HorizontalAlign horizontalOrigin;
        private VerticalAlign verticalOrigin;
        private string text;
        private bool dirty;

        public PixelFont Font {
            get { return font; }
            set {
                if (value != font)
                    dirty = true;
                font = value;
            }
        }

        public float Size {
            get { return size.Size; }
            set {
                if (value != size.Size)
                    dirty = true;
                size = font.Get(value);
            }
        }

        public string Text {
            get { return text; }
            set {
                if (value != text)
                    dirty = true;
                text = value;
            }
        }

        public HorizontalAlign HorizontalOrigin {
            get { return horizontalOrigin; }
            set {
                horizontalOrigin = value;
                UpdateCentering();
            }
        }

        public VerticalAlign VerticalOrigin {
            get { return verticalOrigin; }
            set {
                verticalOrigin = value;
                UpdateCentering();
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public PixelText(PixelFont font, Vector2 position, string text, Color color, HorizontalAlign horizontalAlign = HorizontalAlign.Center, VerticalAlign verticalAlign = VerticalAlign.Center)
            : base(false) {
            Font = font;
            Text = text;
            Color = color;
            Text = text;
            size = Font.Sizes[0];
            Position = position;
            horizontalOrigin = horizontalAlign;
            verticalOrigin = verticalAlign;
            Refresh();
        }

        public void Refresh() {
            dirty = false;
            characters.Clear();

            var widest = 0;
            var lines = 1;
            var offset = Vector2.Zero;

            for (int i = 0; i < text.Length; i++) {
                // new line
                if (text[i] == '\n') {
                    offset.X = 0;
                    offset.Y += size.LineHeight;
                    lines++;
                }

                // add char
                var fontChar = size.Get(text[i]);
                if (fontChar != null) {
                    characters.Add(new Char() {
                        Offset = offset + new Vector2(fontChar.XOffset, fontChar.YOffset),
                        CharData = fontChar,
                        Bounds = fontChar.Texture.ClipRect,
                    });

                    offset.X += fontChar.XAdvance;
                    if (offset.X > widest)
                        widest = (int) offset.X;
                }
            }

            Width = widest;
            Height = lines * size.LineHeight;

            UpdateCentering();
        }

        private void UpdateCentering() {
            if (horizontalOrigin == HorizontalAlign.Left)
                Origin.X = 0;
            else if (horizontalOrigin == HorizontalAlign.Center)
                Origin.X = Width / 2;
            else
                Origin.X = Width;

            if (verticalOrigin == VerticalAlign.Top)
                Origin.Y = 0;
            else if (verticalOrigin == VerticalAlign.Center)
                Origin.Y = Height / 2;
            else
                Origin.Y = Height;

            Origin = Calc.Floor(Origin);
        }

        public override void Render() {
            if (dirty)
                Refresh();

            for (var i = 0; i < characters.Count; i++)
                characters[i].CharData.Texture.Draw(Position + characters[i].Offset * Scale.X, Origin, Color, Scale);
        }
    }
}
