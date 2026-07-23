using Microsoft.Xna.Framework;
using System;

namespace Monocle {
    public static class Vector2Ext {
        public static Vector2 Sign(this Vector2 vec) {
            return new Vector2(Math.Sign(vec.X), Math.Sign(vec.Y));
        }

        public static Vector2 SafeNormalize(this Vector2 vec, float length = 1f, Vector2 ifZero = default) {
            float magnitude = vec.Length();
            if (magnitude <= Calc.EPSILON) {
                return ifZero * length;
            } else {
                return vec / magnitude * length;
            }
        }

        public static Vector2 Round(this Vector2 vec) {
            return new Vector2((float) Math.Round(vec.X), (float) Math.Round(vec.Y));
        }

        public static Point RoundToPoint(this Vector2 vec) {
            return new Point((int) Math.Round(vec.X), (int) Math.Round(vec.Y));
        }

        public static Vector2 Perpendicular(this Vector2 vector) {
            return new Vector2(-vector.Y, vector.X);
        }

        public static float Angle(this Vector2 vector) {
            return (float) Math.Atan2(vector.Y, vector.X);
        }

        public static Vector2 Clamp(this Vector2 val, float minX, float minY, float maxX, float maxY) {
            return new Vector2(MathHelper.Clamp(val.X, minX, maxX), MathHelper.Clamp(val.Y, minY, maxY));
        }

        public static Vector2 Floor(this Vector2 val) {
            return new Vector2((int) Math.Floor(val.X), (int) Math.Floor(val.Y));
        }

        public static Vector2 Ceiling(this Vector2 val) {
            return new Vector2((int) Math.Ceiling(val.X), (int) Math.Ceiling(val.Y));
        }

        public static Vector2 Abs(this Vector2 val) {
            return new Vector2(Math.Abs(val.X), Math.Abs(val.Y));
        }

        public static Vector2 Approach(this Vector2 val, Vector2 target, float maxMove) {
            if (maxMove <= 0 || val == target)
                return val;

            Vector2 diff = target - val;
            float length = diff.Length();

            if (length < maxMove)
                return target;
            else {
                diff.Normalize();
                return val + diff * maxMove;
            }
        }

        public static float DistanceTo(this Vector2 val, Vector2 other) {
            return Vector2.Distance(val, other);
        }

        public static Vector2 FourWayNormal(this Vector2 vec) {
            if (vec == Vector2.Zero)
                return Vector2.Zero;

            float angle = vec.Angle();
            angle = (float) Math.Floor((angle + MathHelper.PiOver2 / 2f) / MathHelper.PiOver2) * MathHelper.PiOver2;

            vec = Calc.AngleToVector(angle, 1f);
            if (Math.Abs(vec.X) < .5f)
                vec.X = 0;
            else
                vec.X = Math.Sign(vec.X);

            if (Math.Abs(vec.Y) < .5f)
                vec.Y = 0;
            else
                vec.Y = Math.Sign(vec.Y);

            return vec;
        }

        public static Vector2 EightWayNormal(this Vector2 vec) {
            if (vec == Vector2.Zero)
                return Vector2.Zero;

            float angle = vec.Angle();
            angle = (float) Math.Floor((angle + MathHelper.PiOver4 / 2f) / MathHelper.PiOver4) * MathHelper.PiOver4;

            vec = Calc.AngleToVector(angle, 1f);
            if (Math.Abs(vec.X) < .5f)
                vec.X = 0;
            else if (Math.Abs(vec.Y) < .5f)
                vec.Y = 0;

            return vec;
        }

        public static Vector2 SnappedNormal(this Vector2 vec, float slices) {
            float divider = MathHelper.TwoPi / slices;

            float angle = vec.Angle();
            angle = (float) Math.Floor((angle + divider / 2f) / divider) * divider;
            return Calc.AngleToVector(angle, 1f);
        }

        public static Vector2 Snapped(this Vector2 vec, float slices) {
            float divider = MathHelper.TwoPi / slices;

            float angle = vec.Angle();
            angle = (float) Math.Floor((angle + divider / 2f) / divider) * divider;
            return Calc.AngleToVector(angle, vec.Length());
        }

        public static Vector2 XComp(this Vector2 vec) {
            return Vector2.UnitX * vec.X;
        }

        public static Vector2 YComp(this Vector2 vec) {
            return Vector2.UnitY * vec.Y;
        }

        public static Vector2 Rotate(this Vector2 vec, float angleRadians) {
            return Calc.AngleToVector(vec.Angle() + angleRadians, vec.Length());
        }

        public static Vector2 RotateTowards(this Vector2 vec, float targetAngleRadians, float maxMoveRadians) {
            float angle = Calc.AngleApproach(vec.Angle(), targetAngleRadians, maxMoveRadians);
            return Calc.AngleToVector(angle, vec.Length());
        }

        public static Vector2 ToVector(this Point point) {
            return new Vector2(point.X, point.Y);
        }
    }
}
