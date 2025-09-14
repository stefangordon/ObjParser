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
    /// MTL material definition.
    /// </summary>
    public class Material : IType
    {
        /// <summary>Material name (newmtl).</summary>
        public string Name { get; set; }
        /// <summary>Ambient reflectivity (Ka).</summary>
        public Color AmbientReflectivity { get; set; }
        /// <summary>Diffuse reflectivity (Kd).</summary>
        public Color DiffuseReflectivity { get; set; }
        /// <summary>Specular reflectivity (Ks).</summary>
        public Color SpecularReflectivity { get; set; }
        /// <summary>Transmission filter (Tf).</summary>
        public Color TransmissionFilter { get; set; }
        /// <summary>Emissive coefficient (Ke).</summary>
        public Color EmissiveCoefficient { get; set; }
        /// <summary>Specular exponent (Ns).</summary>
        public float SpecularExponent { get; set; }
        /// <summary>Optical density / index of refraction (Ni).</summary>
        public float OpticalDensity { get; set; }
        /// <summary>Dissolve (d).</summary>
        public float Dissolve { get; set; }
        /// <summary>Illumination model (illum).</summary>
        public int IlluminationModel { get; set; }

        public Material()
        {
            this.Name = "DefaultMaterial";
            this.AmbientReflectivity = new Color();
            this.DiffuseReflectivity = new Color();
            this.SpecularReflectivity = new Color();
            this.TransmissionFilter = new Color(0f, 0f, 0f);
            this.EmissiveCoefficient = new Color();
            this.SpecularExponent = 0;
            this.OpticalDensity = 1.0f;
            this.Dissolve = 1.0f;
            this.IlluminationModel = 0;
        }

        public void LoadFromStringArray(string[] data)
        {
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("newmtl " + Name);

            b.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ka {0}", AmbientReflectivity));
            b.AppendLine(string.Format(CultureInfo.InvariantCulture, "Kd {0}", DiffuseReflectivity));
            b.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ks {0}", SpecularReflectivity));
            b.AppendLine(string.Format(CultureInfo.InvariantCulture, "Tf {0}", TransmissionFilter));
            b.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ke {0}", EmissiveCoefficient));
            b.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ns {0}", SpecularExponent));
            b.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ni {0}", OpticalDensity));
            b.AppendLine(string.Format(CultureInfo.InvariantCulture, "d {0}", Dissolve));
            b.AppendLine(string.Format(CultureInfo.InvariantCulture, "illum {0}", IlluminationModel));

            return b.ToString();
        }
    }
}
