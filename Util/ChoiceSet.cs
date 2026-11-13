using System;
using System.Collections.Generic;

namespace Monocle {
    public class ChoiceSet<T> where T : notnull {
        public int TotalWeight { get; private set; }
        private readonly Dictionary<T, int> choices;

        public ChoiceSet() {
            choices = new Dictionary<T, int>();
            TotalWeight = 0;
        }

        /// <summary>
        /// Sets the weight of a choice
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="weight"></param>
        public void Set(T choice, int weight) {
            choices.TryGetValue(choice, out int oldWeight);
            TotalWeight -= oldWeight;

            if (weight <= 0) {
                choices.Remove(choice);
            } else {
                TotalWeight += weight;
                choices[choice] = weight;
            }
        }

        /// <summary>
        /// Sets the weight of a choice, or gets its weight
        /// </summary>
        /// <param name="choice"></param>
        /// <returns></returns>
        public int this[T choice] {
            get {
                choices.TryGetValue(choice, out int weight);
                return weight;
            }

            set {
                Set(choice, value);
            }
        }

        /// <summary>
        /// Sets the chance of a choice. The chance is calculated based on the current state of ChoiceSet, so if
        /// other choices are changed later the chance will not be guaranteed to remain the same
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="chance">A chance strictly between 0 and 1.0f</param>
        public void SetChance(T choice, float chance) {
            if (chance <= 0f || chance >= 1f) {
                Logger.DebugCrash("ChoiceSet", "chance must be strictly between 0 and 1");
                chance = Calc.Clamp(chance, 0f, 1f - Calc.EPSILON);
            }

            choices.TryGetValue(choice, out int oldWeight);
            TotalWeight -= oldWeight;

            int weight = (int) Math.Round(TotalWeight / (1f - chance) * chance);
            if (weight <= 0 && chance > 0)
                weight = 1;

            if (weight <= 0) {
                choices.Remove(choice);
            } else {
                TotalWeight += weight;
                choices[choice] = weight;
            }
        }

        /// <summary>
        /// Sets the chance of many choices. Takes the chance of any of the given choices being picked, not the chance of
        /// any individual choice. The chances are calculated based on the current state of ChoiceSet, so if
        /// other choices are changed later the chances will not be guaranteed to remain the same
        /// </summary>
        /// <param name="totalChance">A chance strictly between 0 and 1.0f</param>
        /// <param name="choices">Distinct elements</param>
        public void SetManyChance(float totalChance, params T[] choices) {
            if (totalChance <= 0f || totalChance >= 1f) {
                Logger.DebugCrash("ChoiceSet", "totalChance must be strictly between 0 and 1");
                totalChance = Calc.Clamp(totalChance, 0f, 1f - Calc.EPSILON);
            }

            if (choices.Length == 0)
                return;

            int oldTotalWeight = 0;
            foreach (T c in choices) {
                this.choices.TryGetValue(c, out int oldWeight);
                oldTotalWeight += oldWeight;
            }
            TotalWeight -= oldTotalWeight;

            int weight = (int) Math.Round((TotalWeight / (1f - totalChance)) * totalChance / choices.Length);
            if (weight <= 0 && totalChance > 0)
                weight = 1;

            if (weight <= 0) {
                foreach (T c in choices)
                    this.choices.Remove(c);
            } else {
                TotalWeight += weight * choices.Length;
                foreach (T c in choices)
                    this.choices[c] = weight;
            }
        }

        /// <summary>
        /// Chooses a random choice in the set
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public T? Get(Random random) {
            if (IsEmpty)
                return default;

            int at = random.Next(TotalWeight);

            foreach (KeyValuePair<T, int> kv in choices) {
                if (at < kv.Value)
                    return kv.Key;
                else
                    at -= kv.Value;
            }

            Logger.DebugCrash("ChoiceSet", "Random choice error!");
            return default;
        }

        /// <summary>
        /// Chooses a random choice in the set, using Calc.Random to choose
        /// </summary>
        /// <returns></returns>
        public T? Get() {
            return Get(Calc.Random);
        }

        public bool IsEmpty => TotalWeight <= 0;

        public void Log() {
            if (IsEmpty) {
                Logger.Log("empty");
                return;
            }
            foreach (KeyValuePair<T, int> kv in choices) {
                Logger.Log($"{kv.Key}: {kv.Value}");
            }
        }
    }
}
