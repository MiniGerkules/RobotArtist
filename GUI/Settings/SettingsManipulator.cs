using System.IO;
using System.Reflection;

namespace GUI.Settings {
    internal class SettingsManipulator {
        public static string defaultConfDir = "configs";
        public static string defaultFile = "default.json";

        public static string GetPathToConfigsDir() {
            string initDir = Assembly.GetExecutingAssembly().Location;
            initDir = initDir.Remove(initDir.LastIndexOf(Path.DirectorySeparatorChar));

            return Path.Combine(initDir, defaultConfDir);
        }
    }
}
