using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ObjParser.Types
{
	/// <summary>
	/// Point element (p).
	/// </summary>
	public class PointElement : IType
	{
		public const int MinimumDataLength = 2;
		public const string Prefix = "p";

		/// <summary>1-based vertex indices for points.</summary>
		public int[] VertexIndexList { get; set; }

		public PointElement()
		{
			VertexIndexList = Array.Empty<int>();
			GroupNames = Array.Empty<string>();
		}
		/// <summary>Active group names (g) in effect when parsed.</summary>
		public string[] GroupNames { get; set; }
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
			for (int i = 0; i < count; i++)
			{
				int vindex;
				bool success = int.TryParse(data[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out vindex);
				if (!success) throw new ArgumentException("Could not parse parameter as int");
				VertexIndexList[i] = vindex;
			}
		}

		public override string ToString()
		{
			StringBuilder b = new StringBuilder();
			b.Append("p");
			for (int i = 0; i < VertexIndexList.Length; i++)
			{
				b.AppendFormat(CultureInfo.InvariantCulture, " {0}", VertexIndexList[i]);
			}
			return b.ToString();
		}
	}
}


