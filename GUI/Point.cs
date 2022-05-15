using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    internal struct Point2D
    {
        private uint X { get; }
        private uint Y { get; }

        public Point2D(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public Point2D(uint[] coords) : this(coords[0], coords[1])
        {
        }
    }
}
