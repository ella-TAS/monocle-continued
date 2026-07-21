using Microsoft.Xna.Framework;

namespace Monocle {
    public class NineSliceBox : Component {
        public Vector2 Position;
        public int Width;
        public int Height;
        private MTexture[,] nineSlice;
        private int tileSize;

        public NineSliceBox(MTexture texture, int boxWidth, int boxHeight, Vector2 position = default, int tileSize = 8) : base(false, true) {
            Position = position;
            Width = boxWidth;
            Height = boxHeight;
            this.tileSize = tileSize;
            nineSlice = new MTexture[3, 3];
            for (int y = 0; y < 3; y++) {
                for (int x = 0; x < 3; x++) {
                    nineSlice[x, y] = texture.GetSubtexture(new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize));
                }
            }
        }

        public override void Render() {
            base.Render();

            Vector2 offset = Vector2.Zero;

            // first row
            nineSlice[0, 0].Draw(Position + offset);
            offset.X += tileSize;

            for (; offset.X < Width - tileSize; offset.X += tileSize) {
                nineSlice[1, 0].Draw(Position + offset);
            }

            offset.X = Width - tileSize;
            nineSlice[2, 0].Draw(Position + offset);
            offset.Y += tileSize;

            // middle rows
            for (; offset.Y < Height - tileSize; offset.Y += tileSize) {
                offset.X = 0f;
                nineSlice[0, 1].Draw(Position + offset);
                offset.X += tileSize;

                for (; offset.X < Width - tileSize; offset.X += tileSize) {
                    nineSlice[1, 1].Draw(Position + offset);
                }

                offset.X = Width - tileSize;
                nineSlice[2, 1].Draw(Position + offset);
            }

            // last row
            offset = new Vector2(0f, Height - tileSize);
            nineSlice[0, 2].Draw(Position + offset);
            offset.X += tileSize;

            for (; offset.X < Width - tileSize; offset.X += tileSize) {
                nineSlice[1, 2].Draw(Position + offset);
            }

            offset.X = Width - tileSize;
            nineSlice[2, 2].Draw(Position + offset);
        }
    }
}
