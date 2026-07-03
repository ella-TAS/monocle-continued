using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Monocle {
    /// <summary>
    /// Represents a 2D camera for rendering with support for position, rotation, zoom, and viewport management.
    /// Provides transformation matrices for converting between screen and world coordinates.
    /// </summary>
    public class Camera {
        private Matrix matrix = Matrix.Identity;
        private Matrix inverse = Matrix.Identity;
        private bool changed;

        private Vector2 position = Vector2.Zero;
        private Vector2 zoom = Vector2.One;
        private Vector2 origin = Vector2.Zero;
        private float angle = 0;

        /// <summary>
        /// The viewport defining the visible area and dimensions for this camera.
        /// </summary>
        public Viewport Viewport;

        /// <summary>
        /// Initializes a new Camera with viewport dimensions matching the current engine size.
        /// </summary>
        public Camera() {
            Viewport = new Viewport();
            Viewport.Width = Engine.Width;
            Viewport.Height = Engine.Height;
            UpdateMatrices();
        }

        /// <summary>
        /// Initializes a new Camera with the specified viewport dimensions.
        /// </summary>
        /// <param name="width">The width of the camera viewport in pixels.</param>
        /// <param name="height">The height of the camera viewport in pixels.</param>
        public Camera(int width, int height) {
            Viewport = new Viewport();
            Viewport.Width = width;
            Viewport.Height = height;
            UpdateMatrices();
        }

        /// <summary>
        /// Returns a string representation of the camera's current state including viewport, position, origin, zoom, and angle.
        /// </summary>
        /// <returns>A formatted string describing the camera's properties.</returns>
        public override string ToString() {
            return "Camera:\n\tViewport: { " + Viewport.X + ", " + Viewport.Y + ", " + Viewport.Width + ", " + Viewport.Height +
                " }\n\tPosition: { " + position.X + ", " + position.Y +
                " }\n\tOrigin: { " + origin.X + ", " + origin.Y +
                " }\n\tZoom: { " + zoom.X + ", " + zoom.Y +
                " }\n\tAngle: " + angle;
        }

        /// <summary>
        /// Updates the transformation and inverse transformation matrices based on current camera properties.
        /// Applies translation, rotation, scaling, and origin transformations in the correct order.
        /// </summary>
        private void UpdateMatrices() {
            matrix = Matrix.Identity *
                    Matrix.CreateTranslation(new Vector3(-new Vector2((int) Math.Floor(position.X), (int) Math.Floor(position.Y)), 0)) *
                    Matrix.CreateRotationZ(angle) *
                    Matrix.CreateScale(new Vector3(zoom, 1)) *
                    Matrix.CreateTranslation(new Vector3(new Vector2((int) Math.Floor(origin.X), (int) Math.Floor(origin.Y)), 0));

            inverse = Matrix.Invert(matrix);

            changed = false;
        }

        /// <summary>
        /// Copies all transformation properties from another camera instance.
        /// </summary>
        /// <param name="other">The camera to copy properties from.</param>
        public void CopyFrom(Camera other) {
            position = other.position;
            origin = other.origin;
            angle = other.angle;
            zoom = other.zoom;
            changed = true;
        }

        /// <summary>
        /// Gets the transformation matrix for converting from world coordinates to screen coordinates.
        /// Matrix is recalculated if camera properties have changed.
        /// </summary>
        public Matrix Matrix {
            get {
                if (changed)
                    UpdateMatrices();
                return matrix;
            }
        }

        /// <summary>
        /// Gets the inverse transformation matrix for converting from screen coordinates to world coordinates.
        /// Matrix is recalculated if camera properties have changed.
        /// </summary>
        public Matrix Inverse {
            get {
                if (changed)
                    UpdateMatrices();
                return inverse;
            }
        }

        /// <summary>
        /// Gets or sets the world position of the camera.
        /// </summary>
        public Vector2 Position {
            get { return position; }
            set {
                changed = true;
                position = value;
            }
        }

        /// <summary>
        /// Gets or sets the origin point for camera transformations in screen coordinates.
        /// This is the pivot point for rotation and scaling operations.
        /// </summary>
        public Vector2 Origin {
            get { return origin; }
            set {
                changed = true;
                origin = value;
            }
        }

        /// <summary>
        /// Gets or sets the X component of the camera's world position.
        /// </summary>
        public float X {
            get { return position.X; }
            set {
                changed = true;
                position.X = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y component of the camera's world position.
        /// </summary>
        public float Y {
            get { return position.Y; }
            set {
                changed = true;
                position.Y = value;
            }
        }

        /// <summary>
        /// Gets or sets the uniform zoom level of the camera.
        /// Values greater than 1.0 zoom in, values less than 1.0 zoom out.
        /// </summary>
        public float Zoom {
            get { return zoom.X; }
            set {
                changed = true;
                zoom.X = zoom.Y = value;
            }
        }

        /// <summary>
        /// Gets or sets the rotation angle of the camera in radians.
        /// </summary>
        public float Angle {
            get { return angle; }
            set {
                changed = true;
                angle = value;
            }
        }

        /// <summary>
        /// Gets or sets the world X coordinate of the left edge of the camera's view.
        /// </summary>
        public float Left {
            get {
                if (changed)
                    UpdateMatrices();
                return Vector2.Transform(Vector2.Zero, Inverse).X;
            }

            set {
                if (changed)
                    UpdateMatrices();
                X = Vector2.Transform(Vector2.UnitX * value, Matrix).X;
            }
        }

        /// <summary>
        /// Gets the world X coordinate of the right edge of the camera's view.
        /// Setting this property is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown when attempting to set this property.</exception>
        public float Right {
            get {
                if (changed)
                    UpdateMatrices();
                return Vector2.Transform(Vector2.UnitX * Viewport.Width, Inverse).X;
            }

            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the world Y coordinate of the top edge of the camera's view.
        /// </summary>
        public float Top {
            get {
                if (changed)
                    UpdateMatrices();
                return Vector2.Transform(Vector2.Zero, Inverse).Y;
            }

            set {
                if (changed)
                    UpdateMatrices();
                Y = Vector2.Transform(Vector2.UnitY * value, Matrix).Y;
            }
        }

        /// <summary>
        /// Gets the world Y coordinate of the bottom edge of the camera's view.
        /// Setting this property is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown when attempting to set this property.</exception>
        public float Bottom {
            get {
                if (changed)
                    UpdateMatrices();
                return Vector2.Transform(Vector2.UnitY * Viewport.Height, Inverse).Y;
            }

            set {
                throw new NotImplementedException();
            }
        }

        /*
         *  Utils
         */

        /// <summary>
        /// Sets the camera origin to the center of the viewport.
        /// This makes rotation and scaling occur around the center of the screen.
        /// </summary>
        public void CenterOrigin() {
            origin = new Vector2((float) Viewport.Width / 2, (float) Viewport.Height / 2);
            changed = true;
        }

        /// <summary>
        /// Rounds the camera position to the nearest integer values to prevent sub-pixel rendering artifacts.
        /// </summary>
        public void RoundPosition() {
            position.X = (float) Math.Round(position.X);
            position.Y = (float) Math.Round(position.Y);
            changed = true;
        }

        /// <summary>
        /// Converts a position from screen coordinates to world coordinates.
        /// </summary>
        /// <param name="position">The position in screen coordinates.</param>
        /// <returns>The position in world coordinates.</returns>
        public Vector2 ScreenToCamera(Vector2 position) {
            return Vector2.Transform(position, Inverse);
        }

        /// <summary>
        /// Converts a position from world coordinates to screen coordinates.
        /// </summary>
        /// <param name="position">The position in world coordinates.</param>
        /// <returns>The position in screen coordinates.</returns>
        public Vector2 CameraToScreen(Vector2 position) {
            return Vector2.Transform(position, Matrix);
        }

        /// <summary>
        /// Smoothly moves the camera toward a target position using linear interpolation.
        /// </summary>
        /// <param name="position">The target position to move toward.</param>
        /// <param name="ease">The interpolation factor between 0 and 1. Higher values move faster.</param>
        public void Approach(Vector2 position, float ease) {
            Position += (position - Position) * ease;
        }

        /// <summary>
        /// Smoothly moves the camera toward a target position with a maximum movement distance per call.
        /// </summary>
        /// <param name="position">The target position to move toward.</param>
        /// <param name="ease">The interpolation factor between 0 and 1. Higher values move faster.</param>
        /// <param name="maxDistance">The maximum distance the camera can move in a single call.</param>
        public void Approach(Vector2 position, float ease, float maxDistance) {
            Vector2 move = (position - Position) * ease;
            if (move.Length() > maxDistance)
                Position += Vector2.Normalize(move) * maxDistance;
            else
                Position += move;
        }
    }
}
