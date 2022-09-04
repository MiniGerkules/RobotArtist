using System;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Collections.Generic;

namespace GUI {
    internal class SettingsLoader {
        public static AlgorithmSettings LoadSettings() {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "Algorithm settings|*.json";

            string initDir = Assembly.GetExecutingAssembly().Location;
            initDir = initDir.Remove(initDir.LastIndexOf(Path.DirectorySeparatorChar));
            fileDialog.InitialDirectory = Path.Combine(initDir, "configs");

            if (fileDialog.ShowDialog() == false)
                throw new Exception("You don't choose the file with settings!");

            string jsonContent = File.ReadAllText(fileDialog.FileName);
            JsonNode json = JsonNode.Parse(jsonContent);

            return ParseJSON(json);
        }

        private static AlgorithmSettings ParseJSON(JsonNode json) {
            Type settingsType = typeof(AlgorithmSettings);
            var constructor = settingsType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                new Type[] { typeof(Dictionary<PropertyInfo, double>) }
            );

            if (constructor == null)
                throw new Exception("Can't create algorithm settings!");

            var props = settingsType.GetProperties();
            Dictionary<PropertyInfo, double> values = new();
            foreach (var prop in props) {
                JsonNode setting = json[prop.Name];
                if (setting != null)
                    values[prop] = (double)setting;
            }

            return (AlgorithmSettings)constructor.Invoke(new object[] { values });
        }
    }
}
