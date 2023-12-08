using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NPOI.SS.Formula.Functions;

namespace GUI.Settings {
    public record class AlgorithmSettings : IEnumerable<(PropertyInfo, object?)> {
        #region Settings
        /// <summary> Number of iterations </summary>
        public uint ItersMinOverlap { get; private set; } = 1;

        /// <summary> Minimum overlap coefficient </summary>
        public double MinOverlap { get; private set; } = 0.6;

        /// <summary> Maximum overlap coefficient </summary>
        public double MaxOverlap { get; private set; } = 0.8; // 1

        /// <summary> Possible Color deviation at the end </summary>
        public double PixTol { get; private set; } = 9;

        /// <summary> Possible Color deviation on average </summary>
        public double PixTol2 { get; private set; } = 100;

        /// <summary> The Error of taking a smear </summary>
        public double PixTolBest { get; private set; } = 4;

        /// <summary>
        /// The default width of the brush (use when there isn't PW command in PLT)
        /// </summary>
        public uint DefaultBrushWidth { get; private set; } = 2;

        /// <summary> Number of neibors for proportion classification </summary>
        public uint NumOfNeibForPropClass { get; private set; } = 10;

        /// <summary> Number of neibors for proportion regression </summary>
        public uint NumOfNeibForPropReg { get; private set; } = 45;

        /// <summary> Number of neibors for HSV-Color regression </summary>
        public uint NumOfNeibForHSVReg { get; private set; } = 14;

        /// <summary> Default height for generation of image (for GUITrace) </summary>
        public uint DefaultHeightOfGenImg { get; private set; } = 100;

        /// <summary> Default width for generation of image (for GUITrace) </summary>
        public uint DefaultWidthOfGenImg { get; private set; } = 100;

        /// <summary> Use RGB or CMYBW (8 paints) Color for drawing </summary>
        public bool UseColor8Paints { get; private set; } = false;
        /// <summary> Amount of colors </summary>
        public uint ColorsAmount { get; private set; } = 255;

        /// <summary> Amount of total iterations </summary>
        public uint AmountOfTotalIters { get; private set; } = 3;
        /// <summary> Blur image or not </summary>
        public bool DoBlur { get; private set; } = false;
        /// <summary> Go perpendicular to gradient or lengthwise </summary>
        public bool GoNormal { get; private set; } = true;
        /// <summary> Possible fault of Color on the canvas </summary>
        public uint CanvasColorFault { get; private set; } = 2;
        /// <summary> Minimul Length factor </summary>
        public int[]? MinLenFactor { get; private set; } = null;
        /// <summary> Maximum Length factor </summary>
        public uint MaxLenFactor { get; private set; } = 30;

        #endregion

        /// <summary> The number of Settings </summary>
        public readonly int numOfSettings = typeof(AlgorithmSettings).GetProperties().Length;

        public static string GetPropertyDesc(PropertyInfo property) => property.Name switch {
            nameof(ItersMinOverlap) => "Number of iterations",
            nameof(MinOverlap) => "Minimum overlap coefficient",
            nameof(MaxOverlap) => "Maximum overlap coefficient",
            nameof(PixTol) => "Possible Color deviation at the end",
            nameof(PixTol2) => "Possible Color deviation on average",
            nameof(PixTolBest) => "The Error of taking a smear",
            nameof(DefaultBrushWidth) => "The default width of the brush (use when there isn't PW command in PLT)",
            nameof(NumOfNeibForPropClass) => "Number of neibors for proportion classification",
            nameof(NumOfNeibForPropReg) => "Number of neibors for proportion regression",
            nameof(NumOfNeibForHSVReg) => "Number of neibors for HSV-Color regression",
            nameof(DefaultHeightOfGenImg) => "Default height for generation of image",
            nameof(DefaultWidthOfGenImg) => "Default width for generation of image",
            nameof(UseColor8Paints) => "Color for drawing a picture: true if CMYBW and false if RGB",
            nameof(ColorsAmount) => "Amount of colors used for drawing",
            nameof(AmountOfTotalIters) => "Amount of total iterations",
            nameof(DoBlur) => "Blur: true if need to blur a picture, false otherwise",
            nameof(GoNormal) => "GoNormal: true if go perpendicular to gradient and false if lengthwise",
            nameof(CanvasColorFault) => "Canvas Color fault",
            nameof(MinLenFactor) => "Minimum Length factor",
            nameof(MaxLenFactor) => "Maximum Length factor",
            _ => throw new FieldAccessException($"There aren't decription for a {property.Name} setting!")
        };

        public AlgorithmSettings() { }

        public AlgorithmSettings(Dictionary<PropertyInfo, object> settings) {
            foreach (var setting in settings) {
                setting.Key.SetValue(
                    this, Convert.ChangeType(setting.Value.ToString()?.Replace('.', ','), setting.Key.PropertyType)
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
