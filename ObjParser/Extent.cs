namespace ObjParser
{
	public class BoundingBox
	{
		/// <summary>Maximum X value of the bounding box.</summary>
		public double XMax { get; set; }
		/// <summary>Minimum X value of the bounding box.</summary>
		public double XMin { get; set; }
		/// <summary>Maximum Y value of the bounding box.</summary>
		public double YMax { get; set; }
		/// <summary>Minimum Y value of the bounding box.</summary>
		public double YMin { get; set; }
		/// <summary>Maximum Z value of the bounding box.</summary>
		public double ZMax { get; set; }
		/// <summary>Minimum Z value of the bounding box.</summary>
		public double ZMin { get; set; }

		/// <summary>Size along the X axis (XMax - XMin).</summary>
		public double XSize { get { return XMax - XMin; } }
		/// <summary>Size along the Y axis (YMax - YMin).</summary>
		public double YSize { get { return YMax - YMin; } }
		/// <summary>Size along the Z axis (ZMax - ZMin).</summary>
		public double ZSize { get { return ZMax - ZMin; } }
	}
}
