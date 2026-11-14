using System;
using System.Collections.Generic;
using System.Linq;

namespace Monocle {
    public class ChoiceSetFloat<T> where T : notnull {
        public float TotalWeight { get; private set; }
        private readonly Dictionary<T, float> choices;

        public ChoiceSetFloat() {
            choices = new Dictionary<T, float>();
            TotalWeight = 0f;
        }

        /// <summary>
        /// Sets the weight of a choice
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="weight"></param>
        public void Set(T choice, float weight) {
            choices.TryGetValue(choice, out float oldWeight);
            TotalWeight -= oldWeight;

            if (weight <= 0f) {
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
        public float this[T choice] {
            get {
                choices.TryGetValue(choice, out float weight);
                return weight;
            }

            set {
                Set(choice, value);
            }
        }

        /// <summary>
        /// Sets the chance of a choice. The chance is calculated based on the current state of ChoiceSetFloat, so if
        /// other choices are changed later the chance will not be guaranteed to remain the same
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="chance">A chance strictly between 0 and 1.0f</param>
        public void SetChance(T choice, float chance) {
            if (chance <= 0f || chance >= 1f) {
                Logger.DebugCrash("ChoiceSetFloat", "chance must be strictly between 0 and 1");
                chance = Calc.Clamp(chance, 0f, 1f - Calc.EPSILON);
            }

            choices.TryGetValue(choice, out float oldWeight);
            TotalWeight -= oldWeight;

            float weight = TotalWeight / (1f - chance) * chance;

            if (weight <= 0f) {
                choices.Remove(choice);
            } else {
                TotalWeight += weight;
                choices[choice] = weight;
            }
        }

        /// <summary>
        /// Sets the chance of many choices. Takes the chance of any of the given choices being picked, not the chance of
        /// any individual choice. The chances are calculated based on the current state of ChoiceSetFloat, so if
        /// other choices are changed later the chances will not be guaranteed to remain the same
        /// </summary>
        /// <param name="totalChance">A chance strictly between 0 and 1.0f</param>
        /// <param name="choices">Distinct elements</param>
        public void SetManyChance(float totalChance, params T[] choices) {
            if (totalChance <= 0f || totalChance >= 1f) {
                Logger.DebugCrash("ChoiceSetFloat", "totalChance must be strictly between 0 and 1");
                totalChance = Calc.Clamp(totalChance, 0f, 1f - Calc.EPSILON);
            }

            if (choices.Length == 0)
                return;

            float oldTotalWeight = 0f;
            foreach (T c in choices) {
                this.choices.TryGetValue(c, out float oldWeight);
                oldTotalWeight += oldWeight;
            }
            TotalWeight -= oldTotalWeight;

            float weight = TotalWeight / (1f - totalChance) * totalChance / choices.Length;

            if (weight <= 0f) {
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

            float at = random.NextFloat(TotalWeight);

            foreach (KeyValuePair<T, float> kv in choices) {
                if (at < kv.Value)
                    return kv.Key;
                else
                    at -= kv.Value;
            }

            Logger.Release("ChoiceSetFloat", "Random choice error!");
            return choices.LastOrDefault().Key;
        }

        /// <summary>
        /// Chooses a random choice in the set, using Calc.Random to choose
        /// </summary>
        /// <returns></returns>
        public T? Get() {
            return Get(Calc.Random);
        }

        public bool IsEmpty => TotalWeight <= Calc.EPSILON;

        public void Recalculate() {
            TotalWeight = choices.Sum(kv => kv.Value);
            if (TotalWeight <= Calc.EPSILON) {
                TotalWeight = 0f;
            }
        }

        public void Log() {
            if (IsEmpty) {
                Logger.Log("empty");
                return;
            }

            foreach (KeyValuePair<T, float> kv in choices) {
                Logger.Log($"{kv.Key}: {kv.Value}");
            }

            float total = choices.Sum(kv => kv.Value);
            Logger.Log($"Total weight: {TotalWeight}");
            Logger.Log($"Actual total: {total}, error: {MathF.Abs(total - TotalWeight)}");
        }
    }
}
