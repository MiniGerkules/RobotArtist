using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace GUI.Settings {
    internal class AlgorithmSettings : IEnumerable<(PropertyInfo, object)> {
        public uint ItersMinOverlap { get; private set; }   // Number of iterations
        public double MinOverlap { get; private set; }      // Minimum overlap coefficient
        public double MaxOverlap { get; private set; }      // Maximum overlap coefficient
        public double PixTol { get; private set; }          // Possible color deviation at the end
        public double PixTol2 { get; private set; }         // Possible color deviation on average
        public double PixTolBest { get; private set; }      // The Error of taking a smear
        public uint BrushWidth { get; private set; }        // The width of the brush

        public readonly int numOfSettings;                  // The number of settings

        public static string GetPropertyDesc(PropertyInfo property) => property.Name switch {
            "ItersMinOverlap" => "Number of iterations",
            "MinOverlap" => "Minimum overlap coefficient",
            "MaxOverlap" => "Maximum overlap coefficient",
            "PixTol" => "Possible color deviation at the end",
            "PixTol2" => "Possible color deviation on average",
            "PixTolBest" => "The Error of taking a smear",
            "BrushWidth" => "The width of the brush",
            _ => throw new FieldAccessException($"There aren't decription for {property.Name} setting!")
        };

        public AlgorithmSettings(uint itersMinOverlap = 1, double minOverlap = 0.6,
                                 double maxOverlap = 1, double pixTol = 9,
                                 double pixTol2 = 100, double pixTolBest = 4,
                                 uint brushWidth = 4) {
            ItersMinOverlap = itersMinOverlap;
            MinOverlap = minOverlap;
            MaxOverlap = maxOverlap;
            PixTol = pixTol;
            PixTol2 = pixTol2;
            PixTolBest = pixTolBest;
            BrushWidth = brushWidth;

            numOfSettings = typeof(AlgorithmSettings).GetProperties().Length;
        }

        public AlgorithmSettings(Dictionary<PropertyInfo, object> settings) : this() {
            foreach (var setting in settings)
                setting.Key.SetValue(this, setting.Value);
        }

        public AlgorithmSettings(AlgorithmSettings toCopy) {
            var settings = typeof(AlgorithmSettings).GetProperties();
            foreach (var setting in settings)
                setting.SetValue(this, setting.GetValue(toCopy));
        }

        public IEnumerator<(PropertyInfo, object)> GetEnumerator() {
            var props = typeof(AlgorithmSettings).GetProperties(BindingFlags.Instance |
                                                                BindingFlags.Public);

            foreach (var prop in props)
                yield return (prop, prop.GetValue(this));
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            } else if (obj is null || obj is not AlgorithmSettings) {
                return false;
            } else {
                AlgorithmSettings other = (AlgorithmSettings)obj;
                return ItersMinOverlap == other.ItersMinOverlap && MinOverlap == other.MinOverlap &&
                       MaxOverlap == other.MaxOverlap && PixTol == other.PixTol &&
                       PixTol2 == other.PixTol2 && PixTolBest == other.PixTolBest &&
                       BrushWidth == other.BrushWidth;
            }
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }
    }
}
