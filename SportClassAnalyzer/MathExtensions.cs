using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportClassAnalyzer
{

    public static class MathExtensions
    {
        private const double RadToDegFactor = 180.0 / Math.PI;
        private const double DegToRadFactor = Math.PI / 180.0;

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static double R2D(this double radians)
        {
            return radians * RadToDegFactor;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static double D2R(this double degrees)
        {
            return degrees * DegToRadFactor;
        }

    }
}
