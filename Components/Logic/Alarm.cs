using System;
using System.Collections.Generic;

namespace Monocle {
    public class Alarm : Component {
        public enum AlarmMode { Persist, Oneshot, Looping };

        public Action? OnComplete;

        public AlarmMode Mode { get; private set; }
        public float Duration { get; private set; }
        public float TimeLeft { get; private set; }

        #region Static

        private static Stack<Alarm> cached = new Stack<Alarm>();

        public static Alarm Create(AlarmMode mode, Action? onComplete, float duration = 1f, bool start = false) {
            Alarm alarm;
            if (cached.Count == 0)
                alarm = new Alarm();
            else
                alarm = cached.Pop();

            alarm.Init(mode, onComplete, duration, start);
            return alarm;
        }

        public static Alarm Set(Entity entity, float duration, Action? onComplete, AlarmMode alarmMode = AlarmMode.Oneshot) {
            Alarm alarm = Alarm.Create(alarmMode, onComplete, duration, true);
            entity.Add(alarm);
            return alarm;
        }

        #endregion

        private Alarm()
            : base(false, false) {

        }

        private void Init(AlarmMode mode, Action? onComplete, float duration = 1f, bool start = false) {
            if (duration <= 0)
                Logger.DebugCrash("Alarm", "Alarm duration cannot be less than zero");

            Mode = mode;
            Duration = duration;
            OnComplete = onComplete;

            Active = false;
            TimeLeft = 0;

            if (start)
                Start();
        }

        public override void Update() {
            TimeLeft -= Engine.DeltaTime;
            if (TimeLeft <= 0) {
                TimeLeft = 0;
                OnComplete?.Invoke();
                if (TimeLeft > 0)
                    return;

                if (Mode == AlarmMode.Looping)
                    Start();
                else if (Mode == AlarmMode.Oneshot)
                    RemoveSelf();
                else
                    Active = false;
            }
        }

        public override void Removed(Entity entity) {
            base.Removed(entity);
            cached.Push(this);
        }

        public void Start() {
            Active = true;
            TimeLeft = Duration;
        }

        public void Start(float duration) {
            if (duration <= 0)
                Logger.DebugCrash("Alarm", "Alarm duration cannot be <= 0");

            Duration = duration;
            Start();
        }

        public void Stop() {
            Active = false;
        }
    }
}
