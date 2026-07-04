using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Monocle {
    public static class Logger {
        public const string TIME_FORMAT = "yyyy-dd-MM HH:mm:ss.fffff";
        public static string LogPath => Path.Combine(AppContext.BaseDirectory, "log.txt");

        public static void Initialize() {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(new StreamWriter(LogPath, append: false)));
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;

            Release("Monocle", "Starting Game Engine");
        }

        public static void Release(string origin, object obj) {
            Trace.WriteLine(
                $"[{DateTime.Now.ToString(TIME_FORMAT)}] [{origin}] {obj ?? "<null>"}"
            );
        }

        public static void Debug(string message, string end = "\n") {
#if DEBUG
            Trace.Write($"[{DateTime.Now.ToString(TIME_FORMAT)}] {message}{end}");
#endif
        }

        public static void Log(params object[] obj) {
            string log = "";
            foreach (object o in obj) {
                log += o?.ToString() ?? "<null>";
                log += ' ';
            }
            Debug(log);
        }

        public static void Log() {
            Debug("Log");
        }

        public static void TimeLog(object obj) {
            Debug($"[{DateTime.Now:yyyy-dd-MM HH:mm:ss}] {obj ?? "<null>"}");
        }

        public static void LogEach<T>(IEnumerable<T> collection) {
            foreach (T o in collection)
                Debug(o?.ToString() ?? "<null>");
        }

        public static void Dissect(object obj) {
            if (obj == null) {
                Debug("<null>");
                return;
            }

            Debug(obj.GetType().Name + " { ");
            foreach (FieldInfo v in obj.GetType().GetFields())
                Debug(v.Name + ": " + v.GetValue(obj) + ", ");
            Debug(" }");
        }

        private static Stopwatch stopwatch;

        public static void StartTimer() {
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public static void EndTimer() {
            if (stopwatch != null) {
                stopwatch.Stop();
                Debug($"Timer: {stopwatch.ElapsedTicks} ticks, {stopwatch.ElapsedMilliseconds} ms");
                stopwatch = null;
            }
        }
    }
}
