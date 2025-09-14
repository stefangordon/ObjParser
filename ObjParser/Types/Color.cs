using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ObjParser.Types
{
    /// <summary>
    /// RGB color triple used by MTL materials.
    /// </summary>
    public class Color : IType
    {
        /// <summary>Red channel.</summary>
        public float R { get; set; }
        /// <summary>Green channel.</summary>
        public float G { get; set; }
        /// <summary>Blue channel.</summary>
        public float B { get; set; }

        public Color()
        {
            this.R = 1f;
            this.G = 1f;
            this.B = 1f;
        }

        public Color(float r, float g, float b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public void LoadFromStringArray(string[] data)
        {
            if (data.Length != 4) return;
            R = float.Parse(data[1], CultureInfo.InvariantCulture);
            G = float.Parse(data[2], CultureInfo.InvariantCulture);
            B = float.Parse(data[3], CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", R, G, B);
        }
    }
}
