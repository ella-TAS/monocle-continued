using System;
using System.Collections.Generic;

namespace Monocle {
    /// <summary>
    /// An invisible entity that listens for input sequences and triggers registered cheat codes.
    /// Monitors input patterns and executes associated actions when cheat sequences are detected.
    /// </summary>
    public class CheatListener : Entity {
        /// <summary>
        /// The current accumulated input string being tracked for cheat detection.
        /// </summary>
        public string CurrentInput;

        /// <summary>
        /// Whether to log input and cheat activation to the console for debugging.
        /// </summary>
        public bool Logging;

        private List<Tuple<char, Func<bool>>> inputs;
        private List<Tuple<string, Action>> cheats;
        private int maxInput;

        /// <summary>
        /// Initializes a new CheatListener with no registered inputs or cheats.
        /// The entity is set to invisible since it only processes input without rendering.
        /// </summary>
        public CheatListener() {
            Visible = false;
            CurrentInput = "";

            inputs = new List<Tuple<char, Func<bool>>>();
            cheats = new List<Tuple<string, Action>>();
        }

        /// <summary>
        /// Updates the cheat listener by checking for input and processing cheat codes.
        /// Monitors registered input functions and maintains the input buffer for cheat detection.
        /// </summary>
        public override void Update() {
            //Detect input
            bool changed = false;
            foreach (var input in inputs) {
                if (input.Item2()) {
                    CurrentInput += input.Item1;
                    changed = true;
                }
            }

            //Handle changes
            if (changed) {
                if (CurrentInput.Length > maxInput)
                    CurrentInput = CurrentInput.Substring(CurrentInput.Length - maxInput);

                if (Logging)
                    Logger.Log(CurrentInput);

                foreach (var cheat in cheats) {
                    if (CurrentInput.Contains(cheat.Item1)) {
                        CurrentInput = "";
                        if (cheat.Item2 != null)
                            cheat.Item2();
                        cheats.Remove(cheat);

                        if (Logging)
                            Logger.Release("Monocle", "Cheat Activated: " + cheat.Item1);

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Registers a new cheat code with an optional action to execute when activated.
        /// The cheat is automatically removed after being triggered once.
        /// </summary>
        /// <param name="code">The input sequence that triggers this cheat.</param>
        /// <param name="onEntered">Optional action to execute when the cheat code is entered.</param>
        public void AddCheat(string code, Action onEntered = null) {
            cheats.Add(new Tuple<string, Action>(code, onEntered));
            maxInput = Math.Max(code.Length, maxInput);
        }

        /// <summary>
        /// Registers an input mapping that associates a character with a detection function.
        /// The checker function is called each frame to determine if the input is active.
        /// </summary>
        /// <param name="id">The character to add to the input string when the checker returns true.</param>
        /// <param name="checker">Function that returns true when this input is detected.</param>
        public void AddInput(char id, Func<bool> checker) {
            inputs.Add(new Tuple<char, Func<bool>>(id, checker));
        }
    }
}
