using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Monocle {
    public static class Logger {
        public const string TIME_FORMAT = "yyyy-MM-dd HH:mm:ss.fffff";
        public static string Timestamp => DateTime.Now.ToString(TIME_FORMAT);
        public static string LogPath => Path.Combine(AppContext.BaseDirectory, "log.txt");

        public static void Initialize() {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(new StreamWriter(LogPath, append: false)));
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;

            if (Engine.Instance?.Title != null && Engine.Instance.Version != null) {
                Release("Monocle", Engine.Instance.Title + " v" + Engine.Instance.Version);
            } else {
                Release("Monocle", "Starting Game");
            }
        }

        public static void Release(string origin, string message) {
            Trace.WriteLine($"[{Timestamp}] [{origin}] {message}");
        }

        public static void Debug(string message, string end = "\n", string origin = null) {
#if DEBUG
            Trace.Write($"[{Timestamp}] [{origin ?? GetCallerInfo()}] {message}{end}");
#endif
        }

        public static void Log(params object[] obj) {
            Debug(string.Join(" ", obj.Select(o => o?.ToString() ?? "<null>")));
        }

        public static void TimeLog(object obj) {
            Debug($"[{Timestamp}] {obj ?? "<null>"}");
        }

        public static void LogEach<T>(IEnumerable<T> collection) {
            string origin = GetCallerInfo();
            foreach (T o in collection)
                Debug(o?.ToString() ?? "<null>", origin: origin);
        }

        public static void Dissect(object obj, int indent = 0) {
            string origin = GetCallerInfo();
            string prefix = new string(' ', indent);

            if (obj == null) {
                Debug(prefix + "<null>", origin: origin);
                return;
            }

            Debug(prefix + obj.GetType().Name + " {", origin: origin);
            foreach (FieldInfo v in obj.GetType().GetFields()) {
                object val = v.GetValue(obj);
                if (val is IEnumerable enumerable and not string) {
                    Debug(prefix + "  " + v.Name + ": " + val + " {", origin: origin);
                    foreach (object child in enumerable) {
                        Dissect(child, indent + 4);
                    }
                    Debug(prefix + "  }", origin: origin);
                } else {
                    Debug(prefix + "  " + v.Name + ": " + val, origin: origin);
                }
            }
            Debug(prefix + "}", origin: origin);
        }

        public static Stopwatch StartTimer() {
            return Stopwatch.StartNew();
        }

        public static void EndTimer(Stopwatch stopwatch) {
            stopwatch.Stop();
            Debug($"Timer: {stopwatch.ElapsedTicks} ticks, {stopwatch.ElapsedMilliseconds} ms");
        }

        private static string GetCallerInfo() {
            StackTrace trace = new StackTrace(true);

            foreach (StackFrame frame in trace.GetFrames()) {
                string file = frame.GetFileName();

                if (file == null)
                    continue;

                file = Path.GetFileName(file);
                if (!file.Equals("Logger.cs", StringComparison.OrdinalIgnoreCase))
                    return $"{file}:{frame.GetFileLineNumber()}";
            }

            return "";
        }
    }
}
