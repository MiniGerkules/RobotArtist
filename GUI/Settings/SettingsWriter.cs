using System;
using System.IO;
using Microsoft.Win32;
using System.Text.Json;
using System.Collections.Generic;

namespace GUI.Settings {
    internal class SettingsWriter : SettingsManipulator {
        public static bool WriteSettings(AlgorithmSettings settings) {
            SaveFileDialog saveDialog = new() {
                FileName = $"{DateTime.Now:dd/mm/yyyy hh_mm}",
                DefaultExt = ".json",
                Filter = "Algorithm Settings|*.json",
                InitialDirectory = GetPathToConfigsDir(),
            };

            if (saveDialog.ShowDialog() == true) {
                WriteSettingsTo(saveDialog.FileName, settings);
                return true;
            } else {
                return false;
            }
        }

        public static void WriteSettingsToDefaultConf(AlgorithmSettings settings) {
            if (settings == null) return;

            var pathToFile = GetPathToDefaultConf();
            WriteSettingsTo(pathToFile, settings);
        }

        private static void WriteSettingsTo(string pathToFile, AlgorithmSettings settings) {
            Dictionary<string, double> vals = new();
            var props = typeof(AlgorithmSettings).GetProperties();
            foreach (var prop in props)
                vals.Add(prop.Name, (double)prop.GetValue(settings)!);

            var options = new JsonSerializerOptions() { WriteIndented = true };
            var json = JsonSerializer.Serialize(vals, options);

            File.WriteAllText(pathToFile, json);
        }
    }
}
