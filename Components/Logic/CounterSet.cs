using System;
using System.Collections.Generic;

namespace Monocle {
    public class CounterSet<T> : Component where T : notnull {
        private Dictionary<T, float> counters;
        private float timer;

        public CounterSet()
            : base(true, false) {
            counters = new Dictionary<T, float>();
        }

        public float this[T index] {
            get {
                if (counters.TryGetValue(index, out float value))
                    return Math.Max(value - timer, 0);
                else
                    return 0;
            }

            set {
                counters[index] = timer + value;
            }
        }

        public bool Check(T index) {
            if (counters.TryGetValue(index, out float value))
                return value - timer > 0;
            else
                return false;
        }

        public override void Update() {
            timer += Engine.DeltaTime;
        }
    }
}
