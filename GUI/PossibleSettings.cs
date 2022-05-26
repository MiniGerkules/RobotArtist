using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    internal enum PossibleSettings
    {
        itersMinOverlap,
        minOverlap,
        maxOverlap,
        pixTol,
        pixTol2,
        pixTolBest,
        maxLen,
        brushWidth
    }

    internal static class SettingsExtensions
    {
        public static string GetDescription(this PossibleSettings setting)
        {
            return setting switch
            {
                PossibleSettings.itersMinOverlap => "Number of iterations",
                PossibleSettings.minOverlap => "Minimum overlap coefficient",
                PossibleSettings.maxOverlap => "Maximum overlap coefficient",
                PossibleSettings.pixTol => "Possible color deviation at the end",
                PossibleSettings.pixTol2 => "Possible color deviation on average",
                PossibleSettings.pixTolBest => "The error of taking a smear",
                PossibleSettings.maxLen => "Maximum stroke length (mm)",
                PossibleSettings.brushWidth => "Brush thickness (mm)",
                _ => "No text description"
            };
        }
    }
}
