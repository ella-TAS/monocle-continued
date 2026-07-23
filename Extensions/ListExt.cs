using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Monocle {
    public static class ListExt {
        public static Vector2 ClosestTo(this List<Vector2> list, Vector2 to) {
            Vector2 best = list[0];
            float distSq = Vector2.DistanceSquared(list[0], to);

            for (int i = 1; i < list.Count; i++) {
                float d = Vector2.DistanceSquared(list[i], to);
                if (d < distSq) {
                    distSq = d;
                    best = list[i];
                }
            }

            return best;
        }

        public static Vector2 ClosestTo(this Vector2[] list, Vector2 to) {
            Vector2 best = list[0];
            float distSq = Vector2.DistanceSquared(list[0], to);

            for (int i = 1; i < list.Length; i++) {
                float d = Vector2.DistanceSquared(list[i], to);
                if (d < distSq) {
                    distSq = d;
                    best = list[i];
                }
            }

            return best;
        }

        public static Vector2 ClosestTo(this Vector2[] list, Vector2 to, out int index) {
            index = 0;
            Vector2 best = list[0];
            float distSq = Vector2.DistanceSquared(list[0], to);

            for (int i = 1; i < list.Length; i++) {
                float d = Vector2.DistanceSquared(list[i], to);
                if (d < distSq) {
                    index = i;
                    distSq = d;
                    best = list[i];
                }
            }

            return best;
        }

        public static void Shuffle<T>(this List<T> list, Random random) {
            int i = list.Count;
            int j;
            T t;

            while (--i > 0) {
                t = list[i];
                list[i] = list[j = random.Next(i + 1)];
                list[j] = t;
            }
        }

        public static void Shuffle<T>(this List<T> list) {
            list.Shuffle(Calc.Random);
        }

        public static void ShuffleSetFirst<T>(this List<T> list, Random random, T first) {
            int amount = 0;
            while (list.Contains(first)) {
                list.Remove(first);
                amount++;
            }

            list.Shuffle(random);

            for (int i = 0; i < amount; i++)
                list.Insert(0, first);
        }

        public static void ShuffleSetFirst<T>(this List<T> list, T first) {
            list.ShuffleSetFirst(Calc.Random, first);
        }

        public static void ShuffleNotFirst<T>(this List<T> list, Random random, T notFirst) {
            int amount = 0;
            while (list.Contains(notFirst)) {
                list.Remove(notFirst);
                amount++;
            }

            list.Shuffle(random);

            for (int i = 0; i < amount; i++)
                list.Insert(random.Next(list.Count - 1) + 1, notFirst);
        }

        public static void ShuffleNotFirst<T>(this List<T> list, T notFirst) {
            list.ShuffleNotFirst<T>(Calc.Random, notFirst);
        }

        public static bool IsInRange<T>(this T[] array, int index) {
            return index >= 0 && index < array.Length;
        }

        public static bool IsInRange<T>(this List<T> list, int index) {
            return index >= 0 && index < list.Count;
        }

        public static T[] VerifyLength<T>(this T[] array, int length) {
            if (array == null)
                return new T[length];
            else if (array.Length != length) {
                T[] newArray = new T[length];
                for (int i = 0; i < Math.Min(length, array.Length); i++)
                    newArray[i] = array[i];
                return newArray;
            } else
                return array;
        }

        public static T[][] VerifyLength<T>(this T[][] array, int length0, int length1) {
            array = VerifyLength<T[]>(array, length0);
            for (int i = 0; i < array.Length; i++)
                array[i] = VerifyLength<T>(array[i], length1);
            return array;
        }

        public static T At<T>(this T[,] arr, Pnt at) {
            return arr[at.X, at.Y];
        }
    }
}
