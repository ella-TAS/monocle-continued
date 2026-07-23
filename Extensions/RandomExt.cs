using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Monocle {
    public static class RandomExt {
        /// <summary>
        /// Randomly chooses one of the two provided values.
        /// </summary>
        /// <typeparam name="T">The type of values to choose from.</typeparam>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="a">First option.</param>
        /// <param name="b">Second option.</param>
        /// <returns>One of the provided values, chosen randomly.</returns>
        public static T Choose<T>(this Random random, T a, T b) {
            return Calc.GiveMe(random.Next(2), a, b);
        }

        /// <summary>
        /// Randomly chooses one of the three provided values.
        /// </summary>
        /// <typeparam name="T">The type of values to choose from.</typeparam>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="a">First option.</param>
        /// <param name="b">Second option.</param>
        /// <param name="c">Third option.</param>
        /// <returns>One of the provided values, chosen randomly.</returns>
        public static T Choose<T>(this Random random, T a, T b, T c) {
            return Calc.GiveMe(random.Next(3), a, b, c);
        }

        /// <summary>
        /// Randomly chooses one of the four provided values.
        /// </summary>
        /// <typeparam name="T">The type of values to choose from.</typeparam>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="a">First option.</param>
        /// <param name="b">Second option.</param>
        /// <param name="c">Third option.</param>
        /// <param name="d">Fourth option.</param>
        /// <returns>One of the provided values, chosen randomly.</returns>
        public static T Choose<T>(this Random random, T a, T b, T c, T d) {
            return Calc.GiveMe(random.Next(4), a, b, c, d);
        }

        /// <summary>
        /// Randomly chooses one of the five provided values.
        /// </summary>
        /// <typeparam name="T">The type of values to choose from.</typeparam>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="a">First option.</param>
        /// <param name="b">Second option.</param>
        /// <param name="c">Third option.</param>
        /// <param name="d">Fourth option.</param>
        /// <param name="e">Fifth option.</param>
        /// <returns>One of the provided values, chosen randomly.</returns>
        public static T Choose<T>(this Random random, T a, T b, T c, T d, T e) {
            return Calc.GiveMe(random.Next(5), a, b, c, d, e);
        }

        /// <summary>
        /// Randomly chooses one of the six provided values.
        /// </summary>
        /// <typeparam name="T">The type of values to choose from.</typeparam>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="a">First option.</param>
        /// <param name="b">Second option.</param>
        /// <param name="c">Third option.</param>
        /// <param name="d">Fourth option.</param>
        /// <param name="e">Fifth option.</param>
        /// <param name="f">Sixth option.</param>
        /// <returns>One of the provided values, chosen randomly.</returns>
        public static T Choose<T>(this Random random, T a, T b, T c, T d, T e, T f) {
            return Calc.GiveMe(random.Next(6), a, b, c, d, e, f);
        }

        /// <summary>
        /// Randomly chooses one value from the provided array.
        /// </summary>
        /// <typeparam name="T">The type of values to choose from.</typeparam>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="choices">The array of values to choose from.</param>
        /// <returns>One of the provided values, chosen randomly.</returns>
        /// <exception cref="ArgumentNullException">Thrown when choices is null.</exception>
        /// <exception cref="ArgumentException">Thrown when choices array is empty.</exception>
        public static T Choose<T>(this Random random, params T[] choices) {
            ArgumentNullException.ThrowIfNull(choices);
            if (choices.Length == 0)
                throw new ArgumentException("Choices array cannot be empty.", nameof(choices));

            return choices[random.Next(choices.Length)];
        }

        /// <summary>
        /// Randomly chooses one value from the provided list.
        /// </summary>
        /// <typeparam name="T">The type of values to choose from.</typeparam>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="choices">The list of values to choose from.</param>
        /// <returns>One of the provided values, chosen randomly.</returns>
        /// <exception cref="ArgumentNullException">Thrown when choices is null.</exception>
        /// <exception cref="ArgumentException">Thrown when choices list is empty.</exception>
        public static T Choose<T>(this Random random, List<T> choices) {
            ArgumentNullException.ThrowIfNull(choices);
            if (choices.Count == 0)
                throw new ArgumentException("Choices list cannot be empty.", nameof(choices));

            return choices[random.Next(choices.Count)];
        }

        /// <summary>
        /// Randomly chooses one value from the provided span.
        /// </summary>
        /// <typeparam name="T">The type of values to choose from.</typeparam>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="choices">The span of values to choose from.</param>
        /// <returns>One of the provided values, chosen randomly.</returns>
        /// <exception cref="ArgumentException">Thrown when choices span is empty.</exception>
        public static T Choose<T>(this Random random, ReadOnlySpan<T> choices) {
            if (choices.Length == 0)
                throw new ArgumentException("Choices span cannot be empty.", nameof(choices));

            return choices[random.Next(choices.Length)];
        }

        /// <summary>
        /// Returns a random integer between min (inclusive) and max (exclusive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Range(this Random random, int min, int max) {
            return min + random.Next(max - min);
        }

        /// <summary>
        /// Returns a random float between min (inclusive) and max (exclusive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Range(this Random random, float min, float max) {
            return min + random.NextFloat(max - min);
        }

        /// <summary>
        /// Returns a random Vector2, and x- and y-values of which are between min (inclusive) and max (exclusive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Vector2 Range(this Random random, Vector2 min, Vector2 max) {
            return min + new Vector2(random.NextFloat(max.X - min.X), random.NextFloat(max.Y - min.Y));
        }

        public static int Facing(this Random random) {
            return random.NextFloat() < 0.5f ? -1 : 1;
        }

        public static bool Chance(this Random random, float chance) {
            return random.NextFloat() < chance;
        }

        public static float NextFloat(this Random random) {
            return (float) random.NextDouble();
        }

        public static float NextFloat(this Random random, float max) {
            return random.NextFloat() * max;
        }

        public static float NextAngle(this Random random) {
            return random.NextFloat() * MathHelper.TwoPi;
        }

        private static readonly int[] shakeVectorOffsets = [-1, -1, 0, 1, 1];

        public static Vector2 ShakeVector(this Random random) {
            return new Vector2(random.Choose(shakeVectorOffsets), random.Choose(shakeVectorOffsets));
        }
    }
}
