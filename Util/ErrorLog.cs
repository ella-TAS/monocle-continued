using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Monocle {
    public static class ErrorLog {
        public const string Marker = "<<<<<==========================================>>>>>";
        public const string TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        public static string LogPath => Path.Combine(AppContext.BaseDirectory, "error_log.txt");

        public static void Write(Exception e) {
            try {
                Write(e.ToString());
            } catch {
                // something is on fire
            }
        }

        public static void Write(string str) {
            StringBuilder s = new StringBuilder();

            Logger.Release("ErrorLog", "=====>>> GAME CRASH <<<=====\n" + str);

            //Get the previous contents
            string content = "";
            if (File.Exists(LogPath)) {
                content = File.ReadAllText(LogPath);

                if (!content.Contains(Marker))
                    content = "";
            }

            //Header
            s.Append("     ");
            if (Engine.Instance != null)
                s.Append(Engine.Instance.Title);
            else
                s.Append("Monocle Engine");
            s.AppendLine(" Error Log");
            s.AppendLine(Marker);
            s.AppendLine();

            //Version Number
            if (Engine.Instance?.Version != null) {
                s.AppendLine($"Ver {Engine.Instance.Version} on {Environment.OSVersion}");
            }

            //Datetime
            s.AppendLine(DateTime.Now.ToString(TIME_FORMAT));

            //String
            s.AppendLine(str);

            //If the file wasn't empty, preserve the old errors
            if (content != "") {
                int at = content.IndexOf(Marker) + Marker.Length;
                string after = content.Substring(at);
                s.AppendLine(after);
            }

            File.WriteAllText(LogPath, s.ToString());
        }

        public static bool TryOpen() {
            if (File.Exists(LogPath)) {
                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = LogPath,
                        UseShellExecute = true
                    });
                    return true;
                } catch {
                    Logger.Release("ErrorLog", "Unable to open error_log.txt after crash");
                }
            }

            return false;
        }
    }
}
