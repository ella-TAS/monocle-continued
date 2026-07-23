#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Monocle {
    /// <summary>
    /// Comprehensive utility class providing mathematical, string manipulation, random number generation,
    /// and various helper functions for game development.
    /// Modernized for .NET 9 with nullable reference types and performance optimizations.
    /// </summary>
    public static class Calc {
        public const float EPSILON = 0.00000001f;

        #region Enums

        /// <summary>
        /// Gets the number of values defined in the specified enum type.
        /// </summary>
        /// <param name="enumType">The enum type to analyze.</param>
        /// <returns>The number of values in the enum.</returns>
        /// <exception cref="ArgumentException">Thrown when the type is not an enum.</exception>
        public static int EnumLength(Type enumType) {
            ArgumentNullException.ThrowIfNull(enumType);
            if (!enumType.IsEnum)
                throw new ArgumentException($"Type {enumType.Name} is not an enum type.", nameof(enumType));

            return Enum.GetNames(enumType).Length;
        }

        /// <summary>
        /// Converts a string representation to the specified enum value.
        /// </summary>
        /// <typeparam name="T">The enum type to convert to.</typeparam>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The enum value corresponding to the string.</returns>
        /// <exception cref="ArgumentException">Thrown when the string cannot be converted to the enum type.</exception>
        public static T StringToEnum<T>(string value) where T : struct, Enum {
            ArgumentException.ThrowIfNullOrEmpty(value);

            if (Enum.TryParse<T>(value, out var result))
                return result;

            throw new ArgumentException($"The string '{value}' cannot be converted to enum type {typeof(T).Name}.", nameof(value));
        }

        /// <summary>
        /// Converts an array of string representations to an array of enum values.
        /// </summary>
        /// <typeparam name="T">The enum type to convert to.</typeparam>
        /// <param name="values">The string values to convert.</param>
        /// <returns>An array of enum values corresponding to the strings.</returns>
        /// <exception cref="ArgumentException">Thrown when any string cannot be converted to the enum type.</exception>
        public static T[] StringsToEnums<T>(string[] values) where T : struct, Enum {
            ArgumentNullException.ThrowIfNull(values);

            var result = new T[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = StringToEnum<T>(values[i]);
            return result;
        }

        /// <summary>
        /// Determines whether the specified string can be converted to the enum type.
        /// </summary>
        /// <typeparam name="T">The enum type to check against.</typeparam>
        /// <param name="value">The string value to check.</param>
        /// <returns>True if the string represents a valid enum value; otherwise, false.</returns>
        public static bool EnumHasString<T>(string value) where T : struct, Enum {
            if (string.IsNullOrEmpty(value))
                return false;

            return Enum.TryParse<T>(value, out _);
        }

        #endregion

        #region Strings

        /// <summary>
        /// Determines whether this string matches any of the specified strings using case-insensitive comparison.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="matches">The strings to compare against.</param>
        /// <returns>True if the string matches any of the provided matches; otherwise, false.</returns>
        public static bool IsIgnoreCase(this string str, params string[] matches) {
            if (string.IsNullOrEmpty(str))
                return false;

            foreach (var match in matches)
                if (str.Equals(match, StringComparison.InvariantCultureIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Converts an integer to a string with zero-padding to ensure minimum digits.
        /// </summary>
        /// <param name="num">The number to convert.</param>
        /// <param name="minDigits">The minimum number of digits in the result.</param>
        /// <returns>A zero-padded string representation of the number.</returns>
        /// <remarks>Consider using num.ToString($"D{minDigits}") for better performance.</remarks>
        public static string ToString(this int num, int minDigits) {
            return minDigits <= 0 ? num.ToString() : num.ToString($"D{minDigits}");
        }

        /// <summary>
        /// Splits text into lines that fit within the specified width when rendered with the given font.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="font">The font used for measuring text width.</param>
        /// <param name="maxLineWidth">The maximum width allowed for each line.</param>
        /// <param name="newLine">The character used to force line breaks.</param>
        /// <returns>An array of strings representing the wrapped lines.</returns>
        /// <exception cref="ArgumentNullException">Thrown when text or font is null.</exception>
        /// <exception cref="ArgumentException">Thrown when maxLineWidth is not positive.</exception>
        public static string[] SplitLines(string text, SpriteFont font, int maxLineWidth, char newLine = '\n') {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(font);
            if (maxLineWidth <= 0)
                throw new ArgumentException("Maximum line width must be positive.", nameof(maxLineWidth));

            var lines = new List<string>();

            foreach (var forcedLine in text.Split(newLine)) {
                var line = string.Empty;
                var words = forcedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words) {
                    var testLine = string.IsNullOrEmpty(line) ? word : $"{line} {word}";

                    if (font.MeasureString(testLine).X > maxLineWidth && !string.IsNullOrEmpty(line)) {
                        lines.Add(line);
                        line = word;
                    } else {
                        line = testLine;
                    }
                }

                lines.Add(line);
            }

            return lines.ToArray();
        }

        #endregion

        #region Count

        /// <summary>
        /// Counts how many of the provided values match the target value.
        /// </summary>
        /// <typeparam name="T">The type of values to compare.</typeparam>
        /// <param name="target">The value to count matches for.</param>
        /// <param name="a">First value to check.</param>
        /// <param name="b">Second value to check.</param>
        /// <returns>The number of values that match the target (0-2).</returns>
        public static int Count<T>(T target, T a, T b) where T : notnull {
            var count = 0;
            if (target.Equals(a)) count++;
            if (target.Equals(b)) count++;
            return count;
        }

        /// <summary>
        /// Counts how many of the provided values match the target value.
        /// </summary>
        /// <typeparam name="T">The type of values to compare.</typeparam>
        /// <param name="target">The value to count matches for.</param>
        /// <param name="a">First value to check.</param>
        /// <param name="b">Second value to check.</param>
        /// <param name="c">Third value to check.</param>
        /// <returns>The number of values that match the target (0-3).</returns>
        public static int Count<T>(T target, T a, T b, T c) where T : notnull {
            var count = 0;
            if (target.Equals(a)) count++;
            if (target.Equals(b)) count++;
            if (target.Equals(c)) count++;
            return count;
        }

        /// <summary>
        /// Counts how many of the provided values match the target value.
        /// </summary>
        /// <typeparam name="T">The type of values to compare.</typeparam>
        /// <param name="target">The value to count matches for.</param>
        /// <param name="a">First value to check.</param>
        /// <param name="b">Second value to check.</param>
        /// <param name="c">Third value to check.</param>
        /// <param name="d">Fourth value to check.</param>
        /// <returns>The number of values that match the target (0-4).</returns>
        public static int Count<T>(T target, T a, T b, T c, T d) where T : notnull {
            var count = 0;
            if (target.Equals(a)) count++;
            if (target.Equals(b)) count++;
            if (target.Equals(c)) count++;
            if (target.Equals(d)) count++;
            return count;
        }

        /// <summary>
        /// Counts how many of the provided values match the target value.
        /// </summary>
        /// <typeparam name="T">The type of values to compare.</typeparam>
        /// <param name="target">The value to count matches for.</param>
        /// <param name="a">First value to check.</param>
        /// <param name="b">Second value to check.</param>
        /// <param name="c">Third value to check.</param>
        /// <param name="d">Fourth value to check.</param>
        /// <param name="e">Fifth value to check.</param>
        /// <returns>The number of values that match the target (0-5).</returns>
        public static int Count<T>(T target, T a, T b, T c, T d, T e) where T : notnull {
            var count = 0;
            if (target.Equals(a)) count++;
            if (target.Equals(b)) count++;
            if (target.Equals(c)) count++;
            if (target.Equals(d)) count++;
            if (target.Equals(e)) count++;
            return count;
        }

        /// <summary>
        /// Counts how many of the provided values match the target value.
        /// </summary>
        /// <typeparam name="T">The type of values to compare.</typeparam>
        /// <param name="target">The value to count matches for.</param>
        /// <param name="a">First value to check.</param>
        /// <param name="b">Second value to check.</param>
        /// <param name="c">Third value to check.</param>
        /// <param name="d">Fourth value to check.</param>
        /// <param name="e">Fifth value to check.</param>
        /// <param name="f">Sixth value to check.</param>
        /// <returns>The number of values that match the target (0-6).</returns>
        public static int Count<T>(T target, T a, T b, T c, T d, T e, T f) where T : notnull {
            var count = 0;
            if (target.Equals(a)) count++;
            if (target.Equals(b)) count++;
            if (target.Equals(c)) count++;
            if (target.Equals(d)) count++;
            if (target.Equals(e)) count++;
            if (target.Equals(f)) count++;
            return count;
        }

        /// <summary>
        /// Counts how many values in the span match the target value.
        /// </summary>
        /// <typeparam name="T">The type of values to compare.</typeparam>
        /// <param name="target">The value to count matches for.</param>
        /// <param name="values">The span of values to check.</param>
        /// <returns>The number of values that match the target.</returns>
        public static int Count<T>(T target, ReadOnlySpan<T> values) where T : notnull {
            var count = 0;
            foreach (var value in values) {
                if (target.Equals(value))
                    count++;
            }
            return count;
        }

        #endregion

        #region Give Me

        /// <summary>
        /// Returns the value at the specified index from the provided arguments.
        /// </summary>
        /// <typeparam name="T">The type of values.</typeparam>
        /// <param name="index">The zero-based index of the value to return.</param>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is not 0 or 1.</exception>
        public static T GiveMe<T>(int index, T a, T b) {
            return index switch {
                0 => a,
                1 => b,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be 0 or 1.")
            };
        }

        /// <summary>
        /// Returns the value at the specified index from the provided arguments.
        /// </summary>
        /// <typeparam name="T">The type of values.</typeparam>
        /// <param name="index">The zero-based index of the value to return.</param>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <param name="c">Third value.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is not 0, 1, or 2.</exception>
        public static T GiveMe<T>(int index, T a, T b, T c) {
            return index switch {
                0 => a,
                1 => b,
                2 => c,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be 0, 1, or 2.")
            };
        }

        /// <summary>
        /// Returns the value at the specified index from the provided arguments.
        /// </summary>
        /// <typeparam name="T">The type of values.</typeparam>
        /// <param name="index">The zero-based index of the value to return.</param>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <param name="c">Third value.</param>
        /// <param name="d">Fourth value.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is not 0-3.</exception>
        public static T GiveMe<T>(int index, T a, T b, T c, T d) {
            return index switch {
                0 => a,
                1 => b,
                2 => c,
                3 => d,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be 0-3.")
            };
        }

        /// <summary>
        /// Returns the value at the specified index from the provided arguments.
        /// </summary>
        /// <typeparam name="T">The type of values.</typeparam>
        /// <param name="index">The zero-based index of the value to return.</param>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <param name="c">Third value.</param>
        /// <param name="d">Fourth value.</param>
        /// <param name="e">Fifth value.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is not 0-4.</exception>
        public static T GiveMe<T>(int index, T a, T b, T c, T d, T e) {
            return index switch {
                0 => a,
                1 => b,
                2 => c,
                3 => d,
                4 => e,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be 0-4.")
            };
        }

        /// <summary>
        /// Returns the value at the specified index from the provided arguments.
        /// </summary>
        /// <typeparam name="T">The type of values.</typeparam>
        /// <param name="index">The zero-based index of the value to return.</param>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <param name="c">Third value.</param>
        /// <param name="d">Fourth value.</param>
        /// <param name="e">Fifth value.</param>
        /// <param name="f">Sixth value.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is not 0-5.</exception>
        public static T GiveMe<T>(int index, T a, T b, T c, T d, T e, T f) {
            return index switch {
                0 => a,
                1 => b,
                2 => c,
                3 => d,
                4 => e,
                5 => f,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be 0-5.")
            };
        }

        #endregion

        #region Random

        /// <summary>
        /// Global random number generator instance. Thread-safe but not recommended for multi-threaded scenarios.
        /// </summary>
        public static Random Random { get; set; } = new();

        /// <summary>
        /// Stack for managing nested random number generator states.
        /// </summary>
        private static readonly Stack<Random> randomStack = new();

        /// <summary>
        /// Pushes the current random number generator onto the stack and sets a new one with the specified seed.
        /// </summary>
        /// <param name="newSeed">The seed for the new random number generator.</param>
        public static void PushRandom(int newSeed) {
            randomStack.Push(Random);
            Random = new Random(newSeed);
        }

        /// <summary>
        /// Pushes the current random number generator onto the stack and sets the specified one.
        /// </summary>
        /// <param name="random">The random number generator to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when random is null.</exception>
        public static void PushRandom(Random random) {
            ArgumentNullException.ThrowIfNull(random);
            randomStack.Push(Random);
            Random = random;
        }

        /// <summary>
        /// Pushes the current random number generator onto the stack and creates a new one with a random seed.
        /// </summary>
        public static void PushRandom() {
            randomStack.Push(Random);
            Random = new Random();
        }

        /// <summary>
        /// Pops the previous random number generator from the stack and restores it.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
        public static void PopRandom() {
            if (randomStack.Count == 0)
                throw new InvalidOperationException("No random number generator to pop from the stack.");

            Random = randomStack.Pop();
        }

        #endregion

        #region Colors

        public static Color Invert(this Color color) {
            return new Color(255 - color.R, 255 - color.G, 255 - color.B, color.A);
        }

        public static Color HexToColor(string hex) {
            if (hex.Length >= 6) {
                float r = (HexToByte(hex[0]) * 16 + HexToByte(hex[1])) / 255.0f;
                float g = (HexToByte(hex[2]) * 16 + HexToByte(hex[3])) / 255.0f;
                float b = (HexToByte(hex[4]) * 16 + HexToByte(hex[5])) / 255.0f;
                return new Color(r, g, b);
            }

            return Color.White;
        }

        #endregion

        #region Time

        public static string ShortGameplayFormat(this TimeSpan time) {
            if (time.TotalHours >= 1)
                return ((int) time.Hours) + ":" + time.ToString(@"mm\:ss\.fff");
            else
                return time.ToString(@"m\:ss\.fff");
        }

        public static string LongGameplayFormat(this TimeSpan time) {
            StringBuilder str = new StringBuilder();

            if (time.TotalDays >= 2) {
                str.Append((int) time.TotalDays);
                str.Append(" days, ");
            } else if (time.TotalDays >= 1)
                str.Append("1 day, ");

            str.Append((time.TotalHours - ((int) time.TotalDays * 24)).ToString("0.0"));
            str.Append(" hours");

            return str.ToString();
        }

        #endregion

        #region Math

        public const float Right = 0;
        public const float Up = -MathHelper.PiOver2;
        public const float Left = MathHelper.Pi;
        public const float Down = MathHelper.PiOver2;
        public const float UpRight = -MathHelper.PiOver4;
        public const float UpLeft = -MathHelper.PiOver4 - MathHelper.PiOver2;
        public const float DownRight = MathHelper.PiOver4;
        public const float DownLeft = MathHelper.PiOver4 + MathHelper.PiOver2;
        public const float DegToRad = MathHelper.Pi / 180f;
        public const float RadToDeg = 180f / MathHelper.Pi;
        public const float DtR = DegToRad;
        public const float RtD = RadToDeg;
        public const float Circle = MathHelper.TwoPi;
        public const float HalfCircle = MathHelper.Pi;
        public const float QuarterCircle = MathHelper.PiOver2;
        public const float EighthCircle = MathHelper.PiOver4;
        private const string Hex = "0123456789ABCDEF";

        public static int Digits(this int num) {
            int digits = 1;
            int target = 10;

            while (num >= target) {
                digits++;
                target *= 10;
            }

            return digits;
        }

        public static byte HexToByte(char c) {
            return (byte) Hex.IndexOf(char.ToUpper(c));
        }

        public static float Percent(float num, float zeroAt, float oneAt) {
            return MathHelper.Clamp((num - zeroAt) / oneAt, 0, 1);
        }

        public static float SignThreshold(float value, float threshold) {
            if (Math.Abs(value) >= threshold)
                return Math.Sign(value);
            else
                return 0;
        }

        public static float Min(params float[] values) {
            float min = values[0];
            for (int i = 1; i < values.Length; i++)
                min = MathHelper.Min(values[i], min);
            return min;
        }

        public static float Max(params float[] values) {
            float max = values[0];
            for (int i = 1; i < values.Length; i++)
                max = MathHelper.Max(values[i], max);
            return max;
        }

        public static float ToRad(this float f) {
            return f * DegToRad;
        }

        public static float ToDeg(this float f) {
            return f * RadToDeg;
        }

        public static int Axis(bool negative, bool positive, int both = 0) {
            if (negative) {
                if (positive)
                    return both;
                else
                    return -1;
            } else if (positive)
                return 1;
            else
                return 0;
        }

        public static int Clamp(int value, int min, int max) {
            return Math.Min(Math.Max(value, min), max);
        }

        public static float Clamp(float value, float min, float max) {
            return Math.Min(Math.Max(value, min), max);
        }

        public static float YoYo(float value) {
            if (value <= .5f)
                return value * 2;
            else
                return 1 - ((value - .5f) * 2);
        }

        public static float Map(float val, float min, float max, float newMin = 0, float newMax = 1) {
            return ((val - min) / (max - min)) * (newMax - newMin) + newMin;
        }

        public static float SineMap(float counter, float newMin, float newMax) {
            return Map((float) Math.Sin(counter), 01, 1, newMin, newMax);
        }

        public static float ClampedMap(float val, float min, float max, float newMin = 0, float newMax = 1) {
            return MathHelper.Clamp((val - min) / (max - min), 0, 1) * (newMax - newMin) + newMin;
        }

        public static float LerpSnap(float value1, float value2, float amount, float snapThreshold = .1f) {
            float ret = MathHelper.Lerp(value1, value2, amount);
            if (Math.Abs(ret - value2) < snapThreshold)
                return value2;
            else
                return ret;
        }

        public static float LerpClamp(float value1, float value2, float lerp) {
            return MathHelper.Lerp(value1, value2, MathHelper.Clamp(lerp, 0, 1));
        }

        public static Vector2 LerpSnap(Vector2 value1, Vector2 value2, float amount, float snapThresholdSq = .1f) {
            Vector2 ret = Vector2.Lerp(value1, value2, amount);
            if ((ret - value2).LengthSquared() < snapThresholdSq)
                return value2;
            else
                return ret;
        }

        public static float ReflectAngle(float angle, float axis = 0) {
            return -(angle + axis) - axis;
        }

        public static float ReflectAngle(float angleRadians, Vector2 axis) {
            return ReflectAngle(angleRadians, axis.Angle());
        }

        public static Vector2 ClosestPointOnLine(Vector2 lineA, Vector2 lineB, Vector2 closestTo) {
            Vector2 v = lineB - lineA;
            Vector2 w = closestTo - lineA;
            float t = Vector2.Dot(w, v) / Vector2.Dot(v, v);
            t = MathHelper.Clamp(t, 0, 1);

            return lineA + v * t;
        }

        public static float Snap(float value, float increment) {
            return (float) Math.Round(value / increment) * increment;
        }

        public static float Snap(float value, float increment, float offset) {
            return ((float) Math.Round((value - offset) / increment) * increment) + offset;
        }

        public static float WrapAngleDeg(float angleDegrees) {
            return (((angleDegrees * Math.Sign(angleDegrees) + 180) % 360) - 180) * Math.Sign(angleDegrees);
        }

        public static float WrapAngle(float angleRadians) {
            return (((angleRadians * Math.Sign(angleRadians) + MathHelper.Pi) % (MathHelper.Pi * 2)) - MathHelper.Pi) * Math.Sign(angleRadians);
        }

        public static Vector2 AngleToVector(float angleRadians, float length) {
            return new Vector2((float) Math.Cos(angleRadians) * length, (float) Math.Sin(angleRadians) * length);
        }

        public static float AngleApproach(float val, float target, float maxMove) {
            var diff = AngleDiff(val, target);
            if (Math.Abs(diff) < maxMove)
                return target;
            return val + MathHelper.Clamp(diff, -maxMove, maxMove);
        }

        public static float AngleLerp(float startAngle, float endAngle, float percent) {
            return startAngle + AngleDiff(startAngle, endAngle) * percent;
        }

        public static float Approach(float val, float target, float maxMove) {
            return val > target ? Math.Max(val - maxMove, target) : Math.Min(val + maxMove, target);
        }

        public static float AngleDiff(float radiansA, float radiansB) {
            float diff = radiansB - radiansA;

            while (diff > MathHelper.Pi) { diff -= MathHelper.TwoPi; }
            while (diff <= -MathHelper.Pi) { diff += MathHelper.TwoPi; }

            return diff;
        }

        public static float AbsAngleDiff(float radiansA, float radiansB) {
            return Math.Abs(AngleDiff(radiansA, radiansB));
        }

        public static int SignAngleDiff(float radiansA, float radiansB) {
            return Math.Sign(AngleDiff(radiansA, radiansB));
        }

        public static float Angle(Vector2 from, Vector2 to) {
            return (float) Math.Atan2(to.Y - from.Y, to.X - from.X);
        }

        public static Color ToggleColors(Color current, Color a, Color b) {
            if (current == a)
                return b;
            else
                return a;
        }

        public static float ShorterAngleDifference(float currentAngle, float angleA, float angleB) {
            if (Math.Abs(AngleDiff(currentAngle, angleA)) < Math.Abs(AngleDiff(currentAngle, angleB)))
                return angleA;
            else
                return angleB;
        }

        public static float ShorterAngleDifference(float currentAngle, float angleA, float angleB, float angleC) {
            if (Math.Abs(AngleDiff(currentAngle, angleA)) < Math.Abs(AngleDiff(currentAngle, angleB)))
                return ShorterAngleDifference(currentAngle, angleA, angleC);
            else
                return ShorterAngleDifference(currentAngle, angleB, angleC);
        }

        public static T[] Array<T>(params T[] items) {
            return items;
        }

        public static bool BetweenInterval(float val, float interval) {
            return val % (interval * 2) > interval;
        }

        public static bool OnInterval(float val, float prevVal, float interval) {
            return (int) (prevVal / interval) != (int) (val / interval);
        }


        #endregion

        #region Vector2

        public static Vector2 Toward(Vector2 from, Vector2 to, float length) {
            if (from == to)
                return Vector2.Zero;
            else
                return (to - from).SafeNormalize(length);
        }

        public static Vector2 Toward(Entity from, Entity to, float length) {
            return Toward(from.Position, to.Position, length);
        }

        public static Vector2[] ParseVector2List(string list, char seperator = '|') {
            var entries = list.Split(seperator);
            var data = new Vector2[entries.Length];

            for (int i = 0; i < entries.Length; i++) {
                var sides = entries[i].Split(',');
                data[i] = new Vector2(Convert.ToInt32(sides[0]), Convert.ToInt32(sides[1]));
            }

            return data;
        }

        #endregion

        #region CSV

        public static int[,] ReadCSVIntGrid(string csv, int width, int height) {
            int[,] data = new int[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    data[x, y] = -1;

            string[] lines = csv.Split('\n');
            for (int y = 0; y < height && y < lines.Length; y++) {
                string[] line = lines[y].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < width && x < line.Length; x++)
                    data[x, y] = Convert.ToInt32(line[x]);
            }

            return data;
        }

        public static int[] ReadCSVInt(string csv) {
            if (csv == "")
                return new int[0];

            string[] values = csv.Split(',');
            int[] ret = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
                ret[i] = Convert.ToInt32(values[i].Trim());

            return ret;
        }

        /// <summary>
        /// Read positive-integer CSV with some added tricks.
        /// Use - to add inclusive range. Ex: 3-6 = 3,4,5,6
        /// Use * to add multiple values. Ex: 4*3 = 4,4,4
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static int[] ReadCSVIntWithTricks(string csv) {
            if (csv == "")
                return new int[0];

            string[] values = csv.Split(',');
            List<int> ret = new List<int>();

            foreach (var val in values) {
                if (val.IndexOf('-') != -1) {
                    var split = val.Split('-');
                    int a = Convert.ToInt32(split[0]);
                    int b = Convert.ToInt32(split[1]);

                    for (int i = a; i != b; i += Math.Sign(b - a))
                        ret.Add(i);
                    ret.Add(b);
                } else if (val.IndexOf('*') != -1) {
                    var split = val.Split('*');
                    int a = Convert.ToInt32(split[0]);
                    int b = Convert.ToInt32(split[1]);

                    for (int i = 0; i < b; i++)
                        ret.Add(a);
                } else
                    ret.Add(Convert.ToInt32(val));
            }

            return ret.ToArray();
        }

        public static string[] ReadCSV(string csv) {
            if (csv == "")
                return new string[0];

            string[] values = csv.Split(',');
            for (int i = 0; i < values.Length; i++)
                values[i] = values[i].Trim();

            return values;
        }

        public static string IntGridToCSV(int[,] data) {
            StringBuilder str = new StringBuilder();

            List<int> line = new List<int>();
            int newLines = 0;

            for (int y = 0; y < data.GetLength(1); y++) {
                int empties = 0;

                for (int x = 0; x < data.GetLength(0); x++) {
                    if (data[x, y] == -1)
                        empties++;
                    else {
                        for (int i = 0; i < newLines; i++)
                            str.Append('\n');
                        for (int i = 0; i < empties; i++)
                            line.Add(-1);
                        empties = newLines = 0;

                        line.Add(data[x, y]);
                    }
                }

                if (line.Count > 0) {
                    str.Append(string.Join(",", line));
                    line.Clear();
                }

                newLines++;
            }

            return str.ToString();
        }

        #endregion

        #region Data Parse

        public static bool[,] GetBitData(string data, char rowSep = '\n') {
            int lengthX = 0;
            for (int i = 0; i < data.Length; i++) {
                if (data[i] == '1' || data[i] == '0')
                    lengthX++;
                else if (data[i] == rowSep)
                    break;
            }

            int lengthY = data.Count(c => c == '\n') + 1;

            bool[,] bitData = new bool[lengthX, lengthY];
            int x = 0;
            int y = 0;
            for (int i = 0; i < data.Length; i++) {
                switch (data[i]) {
                case '1':
                    bitData[x, y] = true;
                    x++;
                    break;

                case '0':
                    bitData[x, y] = false;
                    x++;
                    break;

                case '\n':
                    x = 0;
                    y++;
                    break;

                default:
                    break;
                }
            }

            return bitData;
        }

        public static void CombineBitData(bool[,] combineInto, string data, char rowSep = '\n') {
            int x = 0;
            int y = 0;
            for (int i = 0; i < data.Length; i++) {
                switch (data[i]) {
                case '1':
                    combineInto[x, y] = true;
                    x++;
                    break;

                case '0':
                    x++;
                    break;

                case '\n':
                    x = 0;
                    y++;
                    break;

                default:
                    break;
                }
            }
        }

        public static void CombineBitData(bool[,] combineInto, bool[,] data) {
            for (int i = 0; i < combineInto.GetLength(0); i++)
                for (int j = 0; j < combineInto.GetLength(1); j++)
                    if (data[i, j])
                        combineInto[i, j] = true;
        }

        public static int[] ConvertStringArrayToIntArray(string[] strings) {
            int[] ret = new int[strings.Length];
            for (int i = 0; i < strings.Length; i++)
                ret[i] = Convert.ToInt32(strings[i]);
            return ret;
        }

        public static float[] ConvertStringArrayToFloatArray(string[] strings) {
            float[] ret = new float[strings.Length];
            for (int i = 0; i < strings.Length; i++)
                ret[i] = Convert.ToSingle(strings[i], CultureInfo.InvariantCulture);
            return ret;
        }

        #endregion

        #region Save and Load Data

        public static bool FileExists(string filename) {
            return File.Exists(filename);
        }

        public static bool SaveFile<T>(T obj, string filename) where T : new() {
            Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);

            try {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, obj);
                stream.Close();
                return true;
            } catch {
                stream.Close();
                return false;
            }
        }

        public static bool LoadFile<T>(string filename, ref T data) where T : new() {
            Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            try {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T obj = (T) serializer.Deserialize(stream)!;
                stream.Close();
                data = obj;
                return true;
            } catch {
                stream.Close();
                return false;
            }
        }

        #endregion

        #region XML

        public static XmlDocument LoadContentXML(string filename) {
            XmlDocument xml = new XmlDocument();
            xml.Load(TitleContainer.OpenStream(Path.Combine(Engine.ContentDirectory, filename)));
            return xml;
        }

        public static XmlDocument LoadXML(string filename) {
            XmlDocument xml = new XmlDocument();
            using (var stream = File.OpenRead(filename))
                xml.Load(stream);
            return xml;
        }

        public static bool ContentXMLExists(string filename) {
            return File.Exists(Path.Combine(Engine.ContentDirectory, filename));
        }

        public static bool XMLExists(string filename) {
            return File.Exists(filename);
        }

        #endregion XML

        #region Sorting

        public static int SortLeftToRight(Entity a, Entity b) {
            return (int) ((a.X - b.X) * 100);
        }

        public static int SortRightToLeft(Entity a, Entity b) {
            return (int) ((b.X - a.X) * 100);
        }

        public static int SortTopToBottom(Entity a, Entity b) {
            return (int) ((a.Y - b.Y) * 100);
        }

        public static int SortBottomToTop(Entity a, Entity b) {
            return (int) ((b.Y - a.Y) * 100);
        }

        public static int SortByDepth(Entity a, Entity b) {
            return a.Depth - b.Depth;
        }

        public static int SortByDepthReversed(Entity a, Entity b) {
            return b.Depth - a.Depth;
        }

        #endregion

        #region Reflection

        public static Delegate? GetMethod<T>(object obj, string method) where T : class {
            var info = obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (info == null)
                return null;
            else
                return Delegate.CreateDelegate(typeof(T), obj, method);
        }

        #endregion

        public static string ConvertPath(string path) {
            return path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        public static string ReadNullTerminatedString(this System.IO.BinaryReader stream) {
            string str = "";
            char ch;
            while ((int) (ch = stream.ReadChar()) != 0)
                str = str + ch;
            return str;
        }

        public static IEnumerator Do(params IEnumerator[] numerators) {
            if (numerators.Length == 0)
                yield break;
            else if (numerators.Length == 1)
                yield return numerators[0];
            else {
                List<Coroutine> routines = new List<Coroutine>();
                foreach (var enumerator in numerators)
                    routines.Add(new Coroutine(enumerator));

                while (true) {
                    bool moving = false;
                    foreach (var routine in routines) {
                        routine.Update();
                        if (!routine.Finished)
                            moving = true;
                    }

                    if (moving)
                        yield return null;
                    else
                        break;
                }
            }
        }
    }
}
