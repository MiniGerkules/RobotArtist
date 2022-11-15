using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace GUI.Settings {
    internal class SettingsWriter : SettingsManipulator {
        public static bool WriteSettings(AlgorithmSettings settings) {
            SaveFileDialog saveDialog = new() {
                FileName = $"{DateTime.Now:dd/mm/yyyy hh_mm}",
                DefaultExt = ".json",
                Filter = "Algorithm settings|*.json",
                InitialDirectory = GetPathToConfigsDir(),
            };

            if (saveDialog.ShowDialog() != true) return false;

            Dictionary<string, double> vals = new();
            var props = typeof(AlgorithmSettings).GetProperties();
            foreach (var prop in props)
                vals.Add(prop.Name, (double)prop.GetValue(settings));

            var options = new JsonSerializerOptions() { WriteIndented = true };
            var json = JsonSerializer.Serialize(vals, options);
            File.WriteAllText(saveDialog.FileName, json);

            return true;
        }
    }
}
