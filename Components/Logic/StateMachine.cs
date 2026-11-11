using System;
using System.Collections;

namespace Monocle {
    public class StateMachine : Component {
        private int state;
        private Action?[] begins;
        private Func<int>[] updates;
        private Action?[] ends;
        private Func<IEnumerator>?[] coroutines;
        private Coroutine currentCoroutine;

        public bool ChangedStates;
        public bool Log;
        public int PreviousState { get; private set; }
        public bool Locked;

        public StateMachine(int maxStates = 10)
            : base(true, false) {
            PreviousState = state = -1;

            begins = new Action[maxStates];
            updates = new Func<int>[maxStates];
            ends = new Action[maxStates];
            coroutines = new Func<IEnumerator>[maxStates];

            currentCoroutine = new Coroutine();
            currentCoroutine.RemoveOnComplete = false;
        }

        public override void Added(Entity entity) {
            base.Added(entity);

            if (entity.Scene != null && state == -1)
                State = 0;
        }

        public override void EntityAdded(Scene scene) {
            base.EntityAdded(scene);

            if (state == -1)
                State = 0;
        }

        public int State {
            get { return state; }
            set {
                if (value >= updates.Length || value < 0) {
                    Logger.DebugCrash("StateMachine", "StateMachine state out of range");
                    return;
                }

                if (!Locked && state != value) {
                    if (Log)
                        Logger.Log("Enter State " + value + " (leaving " + state + ")");

                    ChangedStates = true;
                    PreviousState = state;
                    state = value;

                    if (PreviousState != -1 && ends[PreviousState] != null) {
                        if (Log)
                            Logger.Log("Calling End " + PreviousState);
                        ends[PreviousState]!();
                    }

                    if (begins[state] != null) {
                        if (Log)
                            Logger.Log("Calling Begin " + state);
                        begins[state]!();
                    }

                    if (coroutines[state] != null) {
                        if (Log)
                            Logger.Log("Starting Coroutine " + state);
                        currentCoroutine.Replace(coroutines[state]!());
                    } else
                        currentCoroutine.Cancel();
                }
            }
        }

        public void ForceState(int toState) {
            if (state != toState)
                State = toState;
            else {
                if (Log)
                    Logger.Log("Enter State " + toState + " (leaving " + state + ")");

                ChangedStates = true;
                PreviousState = state;
                state = toState;

                if (PreviousState != -1 && ends[PreviousState] != null) {
                    if (Log)
                        Logger.Log("Calling End " + state);
                    ends[PreviousState]!();
                }

                if (begins[state] != null) {
                    if (Log)
                        Logger.Log("Calling Begin " + state);
                    begins[state]!();
                }

                if (coroutines[state] != null) {
                    if (Log)
                        Logger.Log("Starting Coroutine " + state);
                    currentCoroutine.Replace(coroutines[state]!());
                } else
                    currentCoroutine.Cancel();
            }
        }

        public void SetCallbacks(int state, Func<int> onUpdate, Func<IEnumerator>? coroutine = null, Action? begin = null, Action? end = null) {
            updates[state] = onUpdate;
            begins[state] = begin;
            ends[state] = end;
            coroutines[state] = coroutine;
        }

        public override void Update() {
            ChangedStates = false;

            if (updates[state] != null)
                State = updates[state]();
            if (currentCoroutine.Active) {
                currentCoroutine.Update();
                if (!ChangedStates && Log && currentCoroutine.Finished)
                    Logger.Log("Finished Coroutine " + state);
            }
        }

        public static implicit operator int(StateMachine s) {
            return s.state;
        }

        public void LogAllStates() {
            for (int i = 0; i < updates.Length; i++)
                LogState(i);
        }

        public void LogState(int index) {
            Logger.Log("State " + index + ": "
                + (updates[index] != null ? "U" : "")
                + (begins[index] != null ? "B" : "")
                + (ends[index] != null ? "E" : "")
                + (coroutines[index] != null ? "C" : ""));
        }
    }
}
