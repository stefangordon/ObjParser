using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ObjParser.Types
{
    public class Face : IType
    {
        public const int MinimumDataLength = 4;
        public const string Prefix = "f";

        public int[] VertexIndexList { get; set; }
        public int[] TextureVertexIndexList { get; set; }

        public void LoadFromStringArray(string[] data)
        {
            if (data.Length < MinimumDataLength)
                throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

            if (!data[0].ToLower().Equals(Prefix))
                throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");            

            int vcount = data.Count() - 1;
            VertexIndexList = new int[vcount];
            TextureVertexIndexList = new int[vcount];

			bool success;

            for (int i = 0; i < vcount; i++)
            {
                string[] parts = data[i + 1].Split('/');

                int vindex;
                success = int.TryParse(parts[0], out vindex);
                if (!success) throw new ArgumentException("Could not parse parameter as int");
                VertexIndexList[i] = vindex;

                if (parts.Count() > 1)
                {
                    success = int.TryParse(parts[1], out vindex);
                    if (!success) throw new ArgumentException("Could not parse parameter as int");
                    TextureVertexIndexList[i] = vindex;
                }
            }
        }

        // HACKHACK this will write invalid files if there are no texture vertices in
        // the faces, need to identify that and write an alternate format
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("f");

            for(int i = 0; i < VertexIndexList.Count(); i++)
            {
                b.AppendFormat(" {0}/{1}", VertexIndexList[i], TextureVertexIndexList[i]);                
            }

            return b.ToString();
        }
	}
}
