using System;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Collections.Generic;

namespace GUI.Settings {
    internal class SettingsReader : SettingsManipulator {
        public static AlgorithmSettings LoadSettings() {
            OpenFileDialog fileDialog = new() {
                Filter = "Algorithm settings|*.json",
                InitialDirectory = GetPathToConfigsDir(),
            };
            
            if (fileDialog.ShowDialog() == false)
                return null;

            var json = GetJsonFrom(fileDialog.FileName);
            return ParseJSON(json);
        }

        public static AlgorithmSettings LoadDefaultSettings() {
            var pathToConfDir = GetPathToConfigsDir();
            var fileName = Path.Combine(pathToConfDir, defaultFile);

            if (File.Exists(fileName)) return ParseJSON(GetJsonFrom(fileName));
            else return new();
        }

        private static JsonNode GetJsonFrom(string fileName) {
            string jsonContent = File.ReadAllText(fileName);
            return JsonNode.Parse(jsonContent);
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
