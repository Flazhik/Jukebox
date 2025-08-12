using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using UnityEngine;

namespace JukeboxCore.Utils
{
    public static class PathsUtils
    {
        private static readonly List<string> ReservedWords = new()
        {
            "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
            "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
            "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };
        
        public static readonly string ApplicationPath = 
                Path.Combine(Directory.GetParent(Application.dataPath)?.FullName);

        public static readonly string AssemblyPath =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        public static readonly string MusicPath = Path.Combine(ApplicationPath, "CyberGrind", "Music");
        
        public static readonly string BepInExConfigPath = Path.Combine(Paths.ConfigPath, "Jukebox", "config.cfg");
        
        public static string CoerceValidFileName(string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = $@"[{invalidChars}]+";

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");

            return ReservedWords
                .Select(reservedWord => $"^{reservedWord}\\.")
                .Aggregate(sanitisedNamePart, (current, reservedWordPattern) =>
                    Regex.Replace(current, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase));
        }
    }
}