using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjParser.Types
{
    /// <summary>
    /// Texture vertex (vt). Supports 1D (u), 2D (u, v) and 3D (u, v, w) texture coordinates.
    /// </summary>
    public class TextureVertex : IType
    {
        public const int MinimumDataLength = 2;
        public const string Prefix = "vt";

        /// <summary>u coordinate.</summary>
        public double X { get; set; }

        /// <summary>v coordinate. Defaults to 0 if omitted.</summary>
        public double Y { get; set; }

        // Optional third component for 3D textures
        /// <summary>Optional w coordinate for 3D textures.</summary>
        public double? Z { get; set; }

		/// <summary>1-based index assigned upon insertion into the model.</summary>
		public int Index { get; set; }

		public void LoadFromStringArray(string[] data)
        {
            if (data.Length < MinimumDataLength)
                throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

            if (!data[0].ToLower().Equals(Prefix))
                throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");

            bool success;

            double x, y = 0.0;

            success = double.TryParse(data[1], NumberStyles.Any, CultureInfo.InvariantCulture, out x);
            if (!success) throw new ArgumentException("Could not parse X parameter as double");

            if (data.Length > 2)
            {
                success = double.TryParse(data[2], NumberStyles.Any, CultureInfo.InvariantCulture, out y);
                if (!success) throw new ArgumentException("Could not parse Y parameter as double");
            }
            X = x;
            Y = y;

            if (data.Length > 3)
            {
                double z;
                if (double.TryParse(data[3], NumberStyles.Any, CultureInfo.InvariantCulture, out z))
                {
                    Z = z;
                }
            }
        }

        public override string ToString()
        {
            if (Z.HasValue)
                return string.Format(CultureInfo.InvariantCulture, "vt {0} {1} {2}", X, Y, Z.Value);
            return string.Format(CultureInfo.InvariantCulture, "vt {0} {1}", X, Y);
        }
    }
}
