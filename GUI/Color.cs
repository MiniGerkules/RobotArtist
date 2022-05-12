using System;
using System.Linq;

namespace GUI
{
    /// <summary>
    /// The class describes a color based on cyan, magenta, yellow, blue and white.
    /// </summary>
    class Color
    {
        private uint cyan, magenta, yellow, blue, white;

        public Color() : this((0, 0, 0, 0, 0))
        {
        }

        public Color((uint cyan, uint magenta, uint yellow, uint blue, uint white) colors)
        {
            (cyan, magenta, yellow, blue, white) = colors;
        }

        public Color(string[] colors)
        {
            (cyan, magenta, yellow, blue, white) = colors.Select(elem => uint.Parse(elem)).ToArray() switch { 
                var arr when arr.Length == 5 => (arr[0], arr[1], arr[2], arr[3], arr[4]),
                _ => throw new ArgumentException("The number of colors is not equal to 5!")
            };
        }

        //public static explicit operator BWColor(Color color)
        //{

        //}
    }
}
