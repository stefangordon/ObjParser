using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjParser.Types
{
    /// <summary>
    /// Geometric vertex (v). Supports optional homogeneous coordinate (w).
    /// </summary>
    public class Vertex : IType
    {
        public const int MinimumDataLength = 4;
        public const string Prefix = "v";

        /// <summary>X coordinate.</summary>
        public double X { get; set; }

        /// <summary>Y coordinate.</summary>
        public double Y { get; set; }

        /// <summary>Z coordinate.</summary>
        public double Z { get; set; }

        // Optional homogeneous coordinate (weight)
        /// <summary>Optional homogeneous weight (w) for rational geometry.</summary>
        public double? W { get; set; }

        /// <summary>1-based index assigned upon insertion into the model.</summary>
        public int Index { get; set; }

		public void LoadFromStringArray(string[] data)
        {
            if (data.Length < MinimumDataLength)
                throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

            if (!data[0].ToLower().Equals(Prefix))
                throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");

            bool success;

            double x, y, z;

            success = double.TryParse(data[1], NumberStyles.Any, CultureInfo.InvariantCulture, out x);
            if (!success) throw new ArgumentException("Could not parse X parameter as double");

            success = double.TryParse(data[2], NumberStyles.Any, CultureInfo.InvariantCulture, out y);
            if (!success) throw new ArgumentException("Could not parse Y parameter as double");

            success = double.TryParse(data[3], NumberStyles.Any, CultureInfo.InvariantCulture, out z);
            if (!success) throw new ArgumentException("Could not parse Z parameter as double");

            X = x;
            Y = y;
            Z = z;

            // Optional w (weight)
            if (data.Length > 4)
            {
                double w;
                if (double.TryParse(data[4], NumberStyles.Any, CultureInfo.InvariantCulture, out w))
                {
                    W = w;
                }
            }
        }

        public override string ToString()
        {
            if (W.HasValue)
                return string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2} {3}", X, Y, Z, W.Value);
            return string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2}", X, Y, Z);
        }
    }
}
