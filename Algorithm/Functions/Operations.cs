using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithm.Functions
{
    internal static class Operations
    {
        /// <summary>
        /// Returns the remainder after division of a by m, 
        /// where a is the dividend and m is the divisor. 
        /// This function is often called the modulo operation
        /// </summary>
        /// <param name="dividend"> Dividend </param>
        /// <param name="divisor"> Divisor </param>
        /// <returns> Returns the remainder after division of a by m </returns>
        public static double Mod(double dividend, double divisor)
        {
            if (divisor == 0)
                throw new ArgumentException("division by zero");
            return (dividend - divisor * Math.Floor(dividend / divisor));
        }
    }
}
