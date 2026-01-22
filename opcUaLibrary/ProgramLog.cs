using System;
using System.IO;

namespace opcUaPlc
{
    public static class ProgramLog
    {
        public static void Log(string path, string message)
        {
            try
            {
                // Assicura che la cartella esista
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                bool fileExists = File.Exists(path);
                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    if (!fileExists)
                    {
                        sw.WriteLine("Timestamp,Message");
                    }
                    // Escape delle virgolette per il formato CSV
                    string cleanMessage = message.Replace("\"", "\"\"");
                    sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},\"{cleanMessage}\"");
                }
            }
            catch { /* Ignora errori di logging per non bloccare il programma */ }
        }
    }
}