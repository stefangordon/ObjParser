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
    /// Face element (f). Supports vertex indices with optional texture and normal indices.
    /// </summary>
    public class Face : IType
    {
        public const int MinimumDataLength = 4;
        public const string Prefix = "f";

        /// <summary>Material in effect when this face was parsed (usemtl).</summary>
        public string? MaterialName { get; set; }
        /// <summary>Active OBJ group names (g) in effect when this face was parsed.</summary>
        public string[] GroupNames { get; set; }
        /// <summary>Smoothing group (s) in effect when this face was parsed.</summary>
        public string? SmoothingGroup { get; set; }
        /// <summary>Object name (o) in effect when this face was parsed.</summary>
        public string? ObjectName { get; set; }
        /// <summary>1-based vertex indices per corner (required).</summary>
        public int[] VertexIndexList { get; set; }
        /// <summary>1-based texture vertex indices per corner (optional).</summary>
        public int[] TextureVertexIndexList { get; set; }
        /// <summary>1-based normal indices per corner (optional).</summary>
        public int[] NormalIndexList { get; set; }

        public Face()
        {
            VertexIndexList = Array.Empty<int>();
            TextureVertexIndexList = Array.Empty<int>();
            NormalIndexList = Array.Empty<int>();
            GroupNames = Array.Empty<string>();
        }

        public void LoadFromStringArray(string[] data)
        {
            if (data.Length < MinimumDataLength)
                throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

            if (!data[0].ToLower().Equals(Prefix))
                throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");

            int vcount = data.Count() - 1;
            VertexIndexList = new int[vcount];
            TextureVertexIndexList = new int[vcount];
            NormalIndexList = new int[vcount];

			bool success;

            for (int i = 0; i < vcount; i++)
            {
                string[] parts = data[i + 1].Split('/');

                int vindex;
                success = int.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out vindex);
                if (!success) throw new ArgumentException("Could not parse parameter as int");
                VertexIndexList[i] = vindex;

                if (parts.Count() > 1)
                {
                    success = int.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out vindex);
                    if (success) {
                        TextureVertexIndexList[i] = vindex;
                    }
                }

                if (parts.Count() > 2)
                {
                    success = int.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out vindex);
                    if (success) {
                        NormalIndexList[i] = vindex;
                    }
                }
            }

            // Enforce consistency per spec: if any texture present, all must be present; same for normals
            bool anyTex = TextureVertexIndexList.Any(t => t > 0);
            bool anyNorm = NormalIndexList.Any(n => n > 0);
            if (anyTex)
            {
                for (int i = 0; i < TextureVertexIndexList.Length; i++)
                {
                    if (TextureVertexIndexList[i] == 0)
                        throw new ArgumentException("Inconsistent texture indices in face: all or none must be specified.");
                }
            }
            if (anyNorm)
            {
                for (int i = 0; i < NormalIndexList.Length; i++)
                {
                    if (NormalIndexList[i] == 0)
                        throw new ArgumentException("Inconsistent normal indices in face: all or none must be specified.");
                }
            }
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("f");

            bool hasTexture = TextureVertexIndexList != null && TextureVertexIndexList.Any(t => t > 0);
            bool hasNormal = NormalIndexList != null && NormalIndexList.Any(n => n > 0);

            for (int i = 0; i < VertexIndexList.Length; i++)
            {
                if (hasTexture || hasNormal)
                {
                    string vt = (TextureVertexIndexList != null && hasTexture && i < TextureVertexIndexList.Length && TextureVertexIndexList[i] > 0)
                        ? TextureVertexIndexList[i].ToString(CultureInfo.InvariantCulture)
                        : string.Empty;
                    string vn = (NormalIndexList != null && hasNormal && i < NormalIndexList.Length && NormalIndexList[i] > 0)
                        ? NormalIndexList[i].ToString(CultureInfo.InvariantCulture)
                        : string.Empty;
                    if (hasTexture && hasNormal)
                        b.AppendFormat(CultureInfo.InvariantCulture, " {0}/{1}/{2}", VertexIndexList[i], vt, vn);
                    else if (hasTexture && !hasNormal)
                        b.AppendFormat(CultureInfo.InvariantCulture, " {0}/{1}", VertexIndexList[i], vt);
                    else // no texture, but normal present ⇒ v//vn
                        b.AppendFormat(CultureInfo.InvariantCulture, " {0}//{1}", VertexIndexList[i], vn);
                }
                else
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, " {0}", VertexIndexList[i]);
                }
            }

            return b.ToString();
        }
    }
}
