using System;
using System.Globalization;
using System.Text;

namespace ObjParser.Types
{
	/// <summary>
	/// Line element (l). Supports optional texture vertex indices.
	/// </summary>
	public class LineElement : IType
	{
		public const int MinimumDataLength = 3;
		public const string Prefix = "l";

		/// <summary>1-based vertex indices for the line vertices (minimum two).</summary>
		public int[] VertexIndexList { get; set; }
		/// <summary>Optional 1-based texture vertex indices per vertex.</summary>
		public int[] TextureVertexIndexList { get; set; }

		public LineElement()
		{
			VertexIndexList = Array.Empty<int>();
			TextureVertexIndexList = Array.Empty<int>();
		}
		/// <summary>Active group names (g) in effect when parsed.</summary>
		public string[]? GroupNames { get; set; }
		/// <summary>Smoothing group (s) in effect when parsed.</summary>
		public string? SmoothingGroup { get; set; }
		/// <summary>Object name (o) in effect when parsed.</summary>
		public string? ObjectName { get; set; }

		public void LoadFromStringArray(string[] data)
		{
			if (data.Length < MinimumDataLength)
				throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

			if (!data[0].ToLower().Equals(Prefix))
				throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");

			int count = data.Length - 1;
			VertexIndexList = new int[count];
			TextureVertexIndexList = new int[count];
			for (int i = 0; i < count; i++)
			{
				string token = data[i + 1];
				string[] parts = token.Split('/');
				int vindex;
				bool success = int.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out vindex);
				if (!success) throw new ArgumentException("Could not parse parameter as int");
				VertexIndexList[i] = vindex;
				if (parts.Length > 1)
				{
					int tindex;
					if (int.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out tindex))
					{
						TextureVertexIndexList[i] = tindex;
					}
				}
			}
		}

		public override string ToString()
		{
			var b = new StringBuilder();
			b.Append("l");
			for (int i = 0; i < VertexIndexList.Length; i++)
			{
				if (TextureVertexIndexList != null && i < TextureVertexIndexList.Length && TextureVertexIndexList[i] > 0)
					b.AppendFormat(CultureInfo.InvariantCulture, " {0}/{1}", VertexIndexList[i], TextureVertexIndexList[i]);
				else
					b.AppendFormat(CultureInfo.InvariantCulture, " {0}", VertexIndexList[i]);
			}
			return b.ToString();
		}
	}
}


