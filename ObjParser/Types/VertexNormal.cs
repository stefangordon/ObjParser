using System;
using System.Globalization;

namespace ObjParser.Types
{
	/// <summary>
	/// Vertex normal (vn) with components i, j, k.
	/// </summary>
	public class VertexNormal : IType
	{
		public const int MinimumDataLength = 4;
		public const string Prefix = "vn";

		/// <summary>i component.</summary>
		public double I { get; set; }
		/// <summary>j component.</summary>
		public double J { get; set; }
		/// <summary>k component.</summary>
		public double K { get; set; }
		/// <summary>1-based index assigned upon insertion into the model.</summary>
		public int Index { get; set; }

		/// <summary>Parse from tokenized vn line.</summary>
		public void LoadFromStringArray(string[] data)
		{
			if (data.Length < MinimumDataLength)
				throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

			if (!data[0].ToLower().Equals(Prefix))
				throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");

			double i, j, k;
			bool success;

			success = double.TryParse(data[1], NumberStyles.Any, CultureInfo.InvariantCulture, out i);
			if (!success) throw new ArgumentException("Could not parse I parameter as double");

			success = double.TryParse(data[2], NumberStyles.Any, CultureInfo.InvariantCulture, out j);
			if (!success) throw new ArgumentException("Could not parse J parameter as double");

			success = double.TryParse(data[3], NumberStyles.Any, CultureInfo.InvariantCulture, out k);
			if (!success) throw new ArgumentException("Could not parse K parameter as double");

			I = i;
			J = j;
			K = k;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "vn {0} {1} {2}", I, J, K);
		}
	}
}


