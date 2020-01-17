using System.Text;

namespace ObjParser.Types
{
    public class Material : IType
    {
        public string Name { get; set; }
        public Color AmbientReflectivity { get; set; }
        public Color DiffuseReflectivity { get; set; }
        public Color SpecularReflectivity { get; set; }
        public Color TransmissionFilter { get; set; }
        public Color EmissiveCoefficient { get; set; }
        public float SpecularExponent { get; set; }
        public float OpticalDensity { get; set; }
        public float Dissolve { get; set; }
        public float IlluminationModel { get; set; }

        public Material()
        {
            Name = "DefaultMaterial";
            AmbientReflectivity = new Color();
            DiffuseReflectivity = new Color();
            SpecularReflectivity = new Color();
            TransmissionFilter = new Color();
            EmissiveCoefficient = new Color();
            SpecularExponent = 0;
            OpticalDensity = 1.0f;
            Dissolve = 1.0f;
            IlluminationModel = 0;
        }

        public void LoadFromStringArray(string[] data)
        {
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("newmtl " + Name);

            b.AppendLine(string.Format("Ka {0}", AmbientReflectivity));
            b.AppendLine(string.Format("Kd {0}", DiffuseReflectivity));
            b.AppendLine(string.Format("Ks {0}", SpecularReflectivity));
            b.AppendLine(string.Format("Tf {0}", TransmissionFilter));
            b.AppendLine(string.Format("Ke {0}", EmissiveCoefficient));
            b.AppendLine(string.Format("Ns {0}", SpecularExponent));
            b.AppendLine(string.Format("Ni {0}", OpticalDensity));
            b.AppendLine(string.Format("d {0}", Dissolve));
            b.AppendLine(string.Format("illum {0}", IlluminationModel));

            return b.ToString();
        }
    }
}
