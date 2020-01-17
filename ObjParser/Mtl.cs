using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ObjParser.Types;

namespace ObjParser
{
    public class Mtl
    {
        public List<Material> MaterialList;

        /// <summary>
        /// Constructor. Initializes VertexList, FaceList and TextureList.
        /// </summary>
        public Mtl()
        {
            MaterialList = new List<Material>();
        }

        /// <summary>
        /// Load .obj from a filepath.
        /// </summary>
        /// <param name="file"></param>
        public void LoadMtl(string path)
        {
            LoadMtl(File.ReadAllLines(path));
        }

        /// <summary>
        /// Load .obj from a stream.
        /// </summary>
        /// <param name="file"></param>
	    public void LoadMtl(Stream data)
        {
            using (var reader = new StreamReader(data))
            {
                LoadMtl(reader.ReadToEnd().Split(Environment.NewLine.ToCharArray()));
            }
        }

        /// <summary>
        /// Load .mtl from a list of strings.
        /// </summary>
        /// <param name="data"></param>
	    public void LoadMtl(IEnumerable<string> data)
        {
            foreach (var line in data)
            {
                processLine(line);
            }
        }

        public void WriteMtlFile(string path, string[] headerStrings)
        {
            using (var outStream = File.OpenWrite(path))
            using (var writer = new StreamWriter(outStream))
            {
                // Write some header data
                WriteHeader(writer, headerStrings);

                MaterialList.ForEach(v => writer.WriteLine(v));
            }
        }

        private void WriteHeader(StreamWriter writer, string[] headerStrings)
        {
            if (headerStrings == null || headerStrings.Length == 0)
            {
                writer.WriteLine("# Generated by ObjParser");
                return;
            }

            foreach (var line in headerStrings)
            {
                writer.WriteLine("# " + line);
            }
        }

        private Material CurrentMaterial()
        {
            if (MaterialList.Count > 0) return MaterialList.Last();
            return new Material();
        }

        /// <summary>
        /// Parses and loads a line from an OBJ file.
        /// Currently only supports V, VT, F and MTLLIB prefixes
        /// </summary>		
        private void processLine(string line)
        {
            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                Material currentMaterial = CurrentMaterial();
                Color c = new Color();
                switch (parts[0])
                {
                    case "newmtl":
                        currentMaterial = new Material
                        {
                            Name = parts[1]
                        };
                        MaterialList.Add(currentMaterial);
                        break;
                    case "Ka":
                        c.LoadFromStringArray(parts);
                        currentMaterial.AmbientReflectivity = c;
                        break;
                    case "Kd":
                        c.LoadFromStringArray(parts);
                        currentMaterial.DiffuseReflectivity = c;
                        break;
                    case "Ks":
                        c.LoadFromStringArray(parts);
                        currentMaterial.SpecularReflectivity = c;
                        break;
                    case "Ke":
                        c.LoadFromStringArray(parts);
                        currentMaterial.EmissiveCoefficient = c;
                        break;
                    case "Tf":
                        c.LoadFromStringArray(parts);
                        currentMaterial.TransmissionFilter = c;
                        break;
                    case "Ni":
                        currentMaterial.OpticalDensity = float.Parse(parts[1]);
                        break;
                    case "d":
                        currentMaterial.Dissolve = float.Parse(parts[1]);
                        break;
                    case "illum":
                        currentMaterial.IlluminationModel = int.Parse(parts[1]);
                        break;
                    case "Ns":
                        currentMaterial.SpecularExponent = float.Parse(parts[1]);
                        break;
                }
            }
        }

    }
}
