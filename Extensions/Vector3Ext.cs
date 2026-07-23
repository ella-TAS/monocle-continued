using Microsoft.Xna.Framework;
using System;

namespace Monocle {
    public static class Vector3Ext {
        public static Vector3 RotateTowards(this Vector3 from, Vector3 target, float maxRotationRadians) {
            var c = Vector3.Cross(from, target);
            var alen = from.Length();
            var blen = target.Length();
            var w = (float) Math.Sqrt((alen * alen) * (blen * blen)) + Vector3.Dot(from, target);
            var q = new Quaternion(c.X, c.Y, c.Z, w);

            if (q.Length() <= maxRotationRadians)
                return target;

            q.Normalize();
            q *= maxRotationRadians;

            return Vector3.Transform(from, q);
        }

        public static Vector2 XZ(this Vector3 vector) {
            return new Vector2(vector.X, vector.Z);
        }


        public static Vector3 Approach(this Vector3 v, Vector3 target, float amount) {
            if (amount > (target - v).Length())
                return target;
            return v + (target - v).SafeNormalize() * amount;
        }

        public static Vector3 SafeNormalize(this Vector3 v) {
            var len = v.Length();
            if (len > Calc.EPSILON)
                return v / len;
            return Vector3.Zero;
        }
    }
}
