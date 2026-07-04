using System;
using System.IO;
using System.Text;

namespace Monocle {
    public static class ErrorLog {
        public const string Marker = "==========================================";
        public static string LogPath => Path.Combine(AppContext.BaseDirectory, "error_log.txt");

        public static void Write(Exception e) {
            Write(e.ToString());
        }

        public static void Write(string str) {
            StringBuilder s = new StringBuilder();

            Logger.Release("ErrorLog", "=====>>> GAME CRASH <<<=====\n" + str);

            //Get the previous contents
            string content = "";
            if (File.Exists(LogPath)) {
                StreamReader tr = new StreamReader(LogPath);
                content = tr.ReadToEnd();
                tr.Close();

                if (!content.Contains(Marker))
                    content = "";
            }

            //Header
            if (Engine.Instance != null)
                s.Append(Engine.Instance.Title);
            else
                s.Append("Monocle Engine");
            s.AppendLine(" Error Log");
            s.AppendLine(Marker);
            s.AppendLine();

            //Version Number
            if (Engine.Instance.Version != null) {
                s.Append("Ver ");
                s.AppendLine(Engine.Instance.Version.ToString());
            }

            //Datetime
            s.AppendLine(DateTime.Now.ToString());

            //String
            s.AppendLine(str);

            //If the file wasn't empty, preserve the old errors
            if (content != "") {
                int at = content.IndexOf(Marker) + Marker.Length;
                string after = content.Substring(at);
                s.AppendLine(after);
            }

            TextWriter tw = new StreamWriter(LogPath, false);
            tw.Write(s.ToString());
            tw.Close();
        }

        public static bool TryOpen() {
            if (File.Exists(LogPath)) {
                try {
                    System.Diagnostics.Process.Start(LogPath);
                    return true;
                } catch {
                    Logger.Release("ErrorLog", "Unable to open error_log.txt after crash");
                }
            }

            return false;
        }
    }
}
