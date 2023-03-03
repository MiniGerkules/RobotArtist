using System.IO;
using System.Reflection;

namespace GUI.Settings {
    internal class SettingsManipulator {
        private static readonly string defaultConfDir = "configs";
        private static readonly string defaultFile = "default.json";

        public static string GetPathToConfigsDir() {
            string initDir = Assembly.GetExecutingAssembly().Location;
            initDir = initDir.Remove(initDir.LastIndexOf(Path.DirectorySeparatorChar));

            return Path.Combine(initDir, defaultConfDir);
        }

        public static string GetPathToDefaultConf() {
            var pathToConfDir = GetPathToConfigsDir();
            return Path.Combine(pathToConfDir, defaultFile);
        }
    }
}
