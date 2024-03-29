﻿using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace GUI.Settings {
    public record class AlgorithmSettings : IEnumerable<(PropertyInfo, object?)> {
        #region Settings
        /// <summary> Number of iterations </summary>
        public uint ItersMinOverlap { get; private set; } = 1;

        /// <summary> Minimum overlap coefficient </summary>
        public double MinOverlap { get; private set; } = 0.6;

        /// <summary> Maximum overlap coefficient </summary>
        public double MaxOverlap { get; private set; } = 1;

        /// <summary> Possible color deviation at the end </summary>
        public double PixTol { get; private set; } = 9;

        /// <summary> Possible color deviation on average </summary>
        public double PixTol2 { get; private set; } = 100;

        /// <summary> The error of taking a smear </summary>
        public double PixTolBest { get; private set; } = 4;

        /// <summary>
        /// The default width of the brush (use when there isn't PW command in PLT)
        /// </summary>
        public uint DefaultBrushWidth { get; private set; } = 4;

        /// <summary> Number of neibors for proportion classification </summary>
        public uint NumOfNeibForPropClass { get; private set; } = 10;

        /// <summary> Number of neibors for proportion regression </summary>
        public uint NumOfNeibForPropReg { get; private set; } = 45;

        /// <summary> Number of neibors for HSV-color regression </summary>
        public uint NumOfNeibForHSVReg { get; private set; } = 14;
        #endregion

        /// <summary> The number of settings </summary>
        public readonly int numOfSettings = typeof(AlgorithmSettings).GetProperties().Length;

        public static string GetPropertyDesc(PropertyInfo property) => property.Name switch {
            nameof(ItersMinOverlap) => "Number of iterations",
            nameof(MinOverlap) => "Minimum overlap coefficient",
            nameof(MaxOverlap) => "Maximum overlap coefficient",
            nameof(PixTol) => "Possible color deviation at the end",
            nameof(PixTol2) => "Possible color deviation on average",
            nameof(PixTolBest) => "The Error of taking a smear",
            nameof(DefaultBrushWidth) => "The default width of the brush (use when there isn't PW command in PLT)",
            nameof(NumOfNeibForPropClass) => "Number of neibors for proportion classification",
            nameof(NumOfNeibForPropReg) => "Number of neibors for proportion regression",
            nameof(NumOfNeibForHSVReg) => "Number of neibors for HSV-color regression",
            _ => throw new FieldAccessException($"There aren't decription for a {property.Name} setting!")
        };

        public AlgorithmSettings() { }

        public AlgorithmSettings(Dictionary<PropertyInfo, object> settings) {
            foreach (var setting in settings) {
                setting.Key.SetValue(
                    this, Convert.ChangeType(setting.Value, setting.Key.PropertyType)
                );
            }
        }

        public IEnumerator<(PropertyInfo, object?)> GetEnumerator() {
            var props = typeof(AlgorithmSettings).
                            GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in props)
                yield return (prop, prop.GetValue(this));
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
