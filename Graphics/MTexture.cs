#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Monocle {
    /// <summary>
    /// Represents a texture or a portion of a texture with additional metadata for drawing.
    /// Provides functionality for texture atlasing, sub-textures, and drawing offsets.
    /// Modernized for .NET 9 with nullable reference types and improved patterns.
    /// </summary>
    public class MTexture {
        /// <summary>
        /// Creates a new MTexture from a file on disk.
        /// </summary>
        /// <param name="filename">The path to the texture file.</param>
        /// <returns>A new MTexture instance loaded from the file.</returns>
        /// <exception cref="ArgumentException">Thrown when filename is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Engine.Instance is null.</exception>
        public static MTexture FromFile(string filename) {
            ArgumentException.ThrowIfNullOrEmpty(filename);
            if (Engine.Instance?.GraphicsDevice == null)
                throw new InvalidOperationException("Engine.Instance or GraphicsDevice is not initialized.");

            using var fileStream = TitleContainer.OpenStream(filename);
            var texture = Texture2D.FromStream(Engine.Instance.GraphicsDevice, fileStream);
            return new MTexture(texture);
        }

        /// <summary>
        /// Initializes a new instance of MTexture with default values.
        /// Note: This creates an incomplete texture that requires manual initialization.
        /// </summary>
        public MTexture() {
            Texture = null!; // Will be set by caller
            AtlasPath = null;
            ClipRect = Rectangle.Empty;
            DrawOffset = Vector2.Zero;
            Width = 0;
            Height = 0;
        }

        /// <summary>
        /// Initializes a new MTexture from a Texture2D, using the entire texture.
        /// </summary>
        /// <param name="texture">The Texture2D to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when texture is null.</exception>
        public MTexture(Texture2D texture) {
            ArgumentNullException.ThrowIfNull(texture);

            Texture = texture;
            AtlasPath = null;
            ClipRect = new Rectangle(0, 0, texture.Width, texture.Height);
            DrawOffset = Vector2.Zero;
            Width = ClipRect.Width;
            Height = ClipRect.Height;
            SetUtil();
        }

        /// <summary>
        /// Initializes a new MTexture as a sub-texture of a parent MTexture.
        /// </summary>
        /// <param name="parent">The parent MTexture to create a sub-texture from.</param>
        /// <param name="x">The x coordinate of the sub-texture within the parent.</param>
        /// <param name="y">The y coordinate of the sub-texture within the parent.</param>
        /// <param name="width">The width of the sub-texture.</param>
        /// <param name="height">The height of the sub-texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when parent is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are not positive.</exception>
        public MTexture(MTexture parent, int x, int y, int width, int height) {
            ArgumentNullException.ThrowIfNull(parent);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            Texture = parent.Texture;
            AtlasPath = null;

            ClipRect = parent.GetRelativeRect(x, y, width, height);
            DrawOffset = new Vector2(-Math.Min(x - parent.DrawOffset.X, 0), -Math.Min(y - parent.DrawOffset.Y, 0));
            Width = width;
            Height = height;
            SetUtil();
        }

        /// <summary>
        /// Initializes a new MTexture as a sub-texture of a parent MTexture using a Rectangle.
        /// </summary>
        /// <param name="parent">The parent MTexture to create a sub-texture from.</param>
        /// <param name="clipRect">The rectangle defining the sub-texture area.</param>
        /// <exception cref="ArgumentNullException">Thrown when parent is null.</exception>
        /// <exception cref="ArgumentException">Thrown when clipRect has non-positive dimensions.</exception>
        public MTexture(MTexture parent, Rectangle clipRect)
            : this(parent, clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height) {
        }

        /// <summary>
        /// Initializes a new MTexture with atlas information and custom draw offset.
        /// </summary>
        /// <param name="parent">The parent MTexture to create a sub-texture from.</param>
        /// <param name="atlasPath">The path identifier within the atlas.</param>
        /// <param name="clipRect">The rectangle defining the sub-texture area.</param>
        /// <param name="drawOffset">The offset to apply when drawing this texture.</param>
        /// <param name="width">The logical width of the texture.</param>
        /// <param name="height">The logical height of the texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when parent is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are not positive.</exception>
        public MTexture(MTexture parent, string? atlasPath, Rectangle clipRect, Vector2 drawOffset, int width, int height) {
            ArgumentNullException.ThrowIfNull(parent);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            Texture = parent.Texture;
            AtlasPath = atlasPath;

            ClipRect = parent.GetRelativeRect(clipRect);
            DrawOffset = drawOffset;
            Width = width;
            Height = height;
            SetUtil();
        }

        /// <summary>
        /// Initializes a new MTexture with atlas path information.
        /// </summary>
        /// <param name="parent">The parent MTexture to create a sub-texture from.</param>
        /// <param name="atlasPath">The path identifier within the atlas.</param>
        /// <param name="clipRect">The rectangle defining the sub-texture area.</param>
        /// <exception cref="ArgumentNullException">Thrown when parent is null.</exception>
        /// <exception cref="ArgumentException">Thrown when clipRect has non-positive dimensions.</exception>
        public MTexture(MTexture parent, string? atlasPath, Rectangle clipRect)
            : this(parent, clipRect) {
            AtlasPath = atlasPath;
        }

        /// <summary>
        /// Initializes a new MTexture with custom frame dimensions and draw offset.
        /// Useful for animated sprites where the logical size differs from the texture size.
        /// </summary>
        /// <param name="texture">The Texture2D to wrap.</param>
        /// <param name="drawOffset">The offset to apply when drawing this texture.</param>
        /// <param name="frameWidth">The logical width of each frame.</param>
        /// <param name="frameHeight">The logical height of each frame.</param>
        /// <exception cref="ArgumentNullException">Thrown when texture is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when frameWidth or frameHeight are not positive.</exception>
        public MTexture(Texture2D texture, Vector2 drawOffset, int frameWidth, int frameHeight) {
            ArgumentNullException.ThrowIfNull(texture);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameWidth);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameHeight);

            Texture = texture;
            ClipRect = new Rectangle(0, 0, texture.Width, texture.Height);
            DrawOffset = drawOffset;
            Width = frameWidth;
            Height = frameHeight;
            SetUtil();
        }

        /// <summary>
        /// Creates a new solid-color texture with the specified dimensions and color.
        /// </summary>
        /// <param name="width">The width of the texture in pixels.</param>
        /// <param name="height">The height of the texture in pixels.</param>
        /// <param name="color">The solid color to fill the texture with.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are not positive.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Engine.Instance is null.</exception>
        public MTexture(int width, int height, Color color) {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
            if (Engine.Instance?.GraphicsDevice == null)
                throw new InvalidOperationException("Engine.Instance or GraphicsDevice is not initialized.");

            Texture = new Texture2D(Engine.Instance.GraphicsDevice, width, height);
            var colors = new Color[width * height];
            Array.Fill(colors, color);
            Texture.SetData(colors);

            ClipRect = new Rectangle(0, 0, width, height);
            DrawOffset = Vector2.Zero;
            Width = width;
            Height = height;
            SetUtil();
        }

        /// <summary>
        /// Calculates and sets utility properties like UV coordinates and center point.
        /// </summary>
        private void SetUtil() {
            Center = new Vector2(Width, Height) * 0.5f;
            if (Texture != null) {
                LeftUV = ClipRect.Left / (float) Texture.Width;
                RightUV = ClipRect.Right / (float) Texture.Width;
                TopUV = ClipRect.Top / (float) Texture.Height;
                BottomUV = ClipRect.Bottom / (float) Texture.Height;
            } else {
                LeftUV = RightUV = TopUV = BottomUV = 0f;
            }
        }

        /// <summary>
        /// Disposes the underlying texture and sets it to null.
        /// Warning: This will affect all MTextures sharing the same Texture2D.
        /// </summary>
        public void Unload() {
            Texture?.Dispose();
            Texture = null!;
        }

        /// <summary>
        /// Creates a sub-texture from this texture or applies the sub-texture data to an existing MTexture.
        /// </summary>
        /// <param name="x">The x coordinate of the sub-texture.</param>
        /// <param name="y">The y coordinate of the sub-texture.</param>
        /// <param name="width">The width of the sub-texture.</param>
        /// <param name="height">The height of the sub-texture.</param>
        /// <param name="applyTo">Optional existing MTexture to apply the sub-texture data to.</param>
        /// <returns>A new MTexture representing the sub-texture, or the modified applyTo parameter.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are not positive.</exception>
        public MTexture GetSubtexture(int x, int y, int width, int height, MTexture? applyTo = null) {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            if (applyTo == null)
                return new MTexture(this, x, y, width, height);
            else {
                applyTo.Texture = Texture;
                applyTo.AtlasPath = null;

                applyTo.ClipRect = GetRelativeRect(x, y, width, height);
                applyTo.DrawOffset = new Vector2(-Math.Min(x - DrawOffset.X, 0), -Math.Min(y - DrawOffset.Y, 0));
                applyTo.Width = width;
                applyTo.Height = height;
                applyTo.SetUtil();

                return applyTo;
            }
        }

        /// <summary>
        /// Creates a sub-texture from this texture using a Rectangle.
        /// </summary>
        /// <param name="rect">The rectangle defining the sub-texture area.</param>
        /// <returns>A new MTexture representing the sub-texture.</returns>
        /// <exception cref="ArgumentException">Thrown when rect has non-positive dimensions.</exception>
        public MTexture GetSubtexture(Rectangle rect) {
            return new MTexture(this, rect);
        }

        /// <summary>
        /// Disposes the underlying texture.
        /// Warning: This will affect all MTextures sharing the same Texture2D.
        /// </summary>
        public void Dispose() {
            Texture?.Dispose();
        }

        #region Properties

        /// <summary>
        /// The underlying Texture2D that this MTexture references.
        /// </summary>
        public Texture2D? Texture { get; private set; }

        /// <summary>
        /// The rectangle within the texture that this MTexture represents.
        /// </summary>
        public Rectangle ClipRect { get; private set; }

        /// <summary>
        /// The path identifier within a texture atlas, if applicable.
        /// </summary>
        public string? AtlasPath { get; private set; }

        /// <summary>
        /// The offset to apply when drawing this texture.
        /// </summary>
        public Vector2 DrawOffset { get; private set; }

        /// <summary>
        /// The logical width of this texture.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The logical height of this texture.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The center point of this texture (Width/2, Height/2).
        /// </summary>
        public Vector2 Center { get; private set; }

        /// <summary>
        /// The left UV coordinate (0.0 to 1.0) within the texture.
        /// </summary>
        public float LeftUV { get; private set; }

        /// <summary>
        /// The right UV coordinate (0.0 to 1.0) within the texture.
        /// </summary>
        public float RightUV { get; private set; }

        /// <summary>
        /// The top UV coordinate (0.0 to 1.0) within the texture.
        /// </summary>
        public float TopUV { get; private set; }

        /// <summary>
        /// The bottom UV coordinate (0.0 to 1.0) within the texture.
        /// </summary>
        public float BottomUV { get; private set; }

        #endregion

        #region Helpers

        public override string ToString() {
            if (AtlasPath != null)
                return AtlasPath;
            else
                return "MTexture [" + (Texture?.Width ?? 0) + " x " + (Texture?.Height ?? 0) + "]";
        }

        public Rectangle GetRelativeRect(Rectangle rect) {
            return GetRelativeRect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public Rectangle GetRelativeRect(int x, int y, int width, int height) {
            int atX = (int) (ClipRect.X - DrawOffset.X + x);
            int atY = (int) (ClipRect.Y - DrawOffset.Y + y);

            int rX = (int) MathHelper.Clamp(atX, ClipRect.Left, ClipRect.Right);
            int rY = (int) MathHelper.Clamp(atY, ClipRect.Top, ClipRect.Bottom);
            int rW = Math.Max(0, Math.Min(atX + width, ClipRect.Right) - rX);
            int rH = Math.Max(0, Math.Min(atY + height, ClipRect.Bottom) - rY);

            return new Rectangle(rX, rY, rW, rH);
        }


        public int TotalPixels {
            get { return Width * Height; }
        }

        #endregion

        #region Draw

        public void Draw(Vector2 position) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                if (Texture != null)
                    Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, Color.White, 0, -DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void Draw(Vector2 position, Vector2 origin) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                if (Texture != null)
                    Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, Color.White, 0, origin - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void Draw(Vector2 position, Vector2 origin, Color color) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, origin - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void Draw(Vector2 position, Vector2 origin, Color color, float scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, origin - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void Draw(Vector2 position, Vector2 origin, Color color, float scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, origin - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void Draw(Vector2 position, Vector2 origin, Color color, float scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, origin - DrawOffset, scale, flip, 0);
        }

        public void Draw(Vector2 position, Vector2 origin, Color color, Vector2 scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, origin - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void Draw(Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, origin - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void Draw(Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, origin - DrawOffset, scale, flip, 0);
        }

        public void Draw(Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation, Rectangle clip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, GetRelativeRect(clip), color, rotation, origin - DrawOffset, scale, SpriteEffects.None, 0);
        }

        #endregion

        #region Draw Centered

        public void DrawCentered(Vector2 position) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, Color.White, 0, Center - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawCentered(Vector2 position, Color color) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, Center - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawCentered(Vector2 position, Color color, float scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, Center - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawCentered(Vector2 position, Color color, float scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, Center - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawCentered(Vector2 position, Color color, float scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, Center - DrawOffset, scale, flip, 0);
        }

        public void DrawCentered(Vector2 position, Color color, Vector2 scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, Center - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawCentered(Vector2 position, Color color, Vector2 scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, Center - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawCentered(Vector2 position, Color color, Vector2 scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, Center - DrawOffset, scale, flip, 0);
        }

        #endregion

        #region Draw Justified

        public void DrawJustified(Vector2 position, Vector2 justify) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, Color.White, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawJustified(Vector2 position, Vector2 justify, Color color) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawJustified(Vector2 position, Vector2 justify, Color color, float scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawJustified(Vector2 position, Vector2 justify, Color color, float scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawJustified(Vector2 position, Vector2 justify, Color color, float scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, flip, 0);
        }

        public void DrawJustified(Vector2 position, Vector2 justify, Color color, Vector2 scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawJustified(Vector2 position, Vector2 justify, Color color, Vector2 scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawJustified(Vector2 position, Vector2 justify, Color color, Vector2 scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif
            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, flip, 0);
        }

        #endregion

        #region Draw Outline

        public void DrawOutline(Vector2 position) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, -DrawOffset, 1f, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, Color.White, 0, -DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawOutline(Vector2 position, Vector2 origin) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, origin - DrawOffset, 1f, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, Color.White, 0, origin - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawOutline(Vector2 position, Vector2 origin, Color color) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, origin - DrawOffset, 1f, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, origin - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawOutline(Vector2 position, Vector2 origin, Color color, float scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, origin - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, origin - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutline(Vector2 position, Vector2 origin, Color color, float scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, origin - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, origin - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutline(Vector2 position, Vector2 origin, Color color, float scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, origin - DrawOffset, scale, flip, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, origin - DrawOffset, scale, flip, 0);
        }

        public void DrawOutline(Vector2 position, Vector2 origin, Color color, Vector2 scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, origin - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, origin - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutline(Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, origin - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, origin - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutline(Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, origin - DrawOffset, scale, flip, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, origin - DrawOffset, scale, flip, 0);
        }

        #endregion

        #region Draw Outline Centered

        public void DrawOutlineCentered(Vector2 position) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, Center - DrawOffset, 1f, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, Color.White, 0, Center - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawOutlineCentered(Vector2 position, Color color) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, Center - DrawOffset, 1f, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, Center - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawOutlineCentered(Vector2 position, Color color, float scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, Center - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, Center - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutlineCentered(Vector2 position, Color color, float scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, Center - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, Center - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutlineCentered(Vector2 position, Color color, float scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, Center - DrawOffset, scale, flip, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, Center - DrawOffset, scale, flip, 0);
        }

        public void DrawOutlineCentered(Vector2 position, Color color, Vector2 scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, Center - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, Center - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutlineCentered(Vector2 position, Color color, Vector2 scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, Center - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, Center - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutlineCentered(Vector2 position, Color color, Vector2 scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, Center - DrawOffset, scale, flip, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, Center - DrawOffset, scale, flip, 0);
        }

        #endregion

        #region Draw Outline Justified

        public void DrawOutlineJustified(Vector2 position, Vector2 justify) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, 1f, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, Color.White, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawOutlineJustified(Vector2 position, Vector2 justify, Color color) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, 1f, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, 1f, SpriteEffects.None, 0);
        }

        public void DrawOutlineJustified(Vector2 position, Vector2 justify, Color color, float scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutlineJustified(Vector2 position, Vector2 justify, Color color, float scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutlineJustified(Vector2 position, Vector2 justify, Color color, float scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, flip, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, flip, 0);
        }

        public void DrawOutlineJustified(Vector2 position, Vector2 justify, Color color, Vector2 scale) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, 0, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutlineJustified(Vector2 position, Vector2 justify, Color color, Vector2 scale, float rotation) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, SpriteEffects.None, 0);
        }

        public void DrawOutlineJustified(Vector2 position, Vector2 justify, Color color, Vector2 scale, float rotation, SpriteEffects flip) {
#if DEBUG
            if (Texture?.IsDisposed == true)
                throw new Exception("Texture2D Is Disposed");
#endif

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        if (Texture != null)
                            Monocle.Draw.SpriteBatch.Draw(Texture, position + new Vector2(i, j), ClipRect, Color.Black, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, flip, 0);

            if (Texture != null)
                Monocle.Draw.SpriteBatch.Draw(Texture, position, ClipRect, color, rotation, new Vector2(Width * justify.X, Height * justify.Y) - DrawOffset, scale, flip, 0);
        }

        #endregion
    }
}
