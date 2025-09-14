using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Text;
using ObjParser.Types;

namespace ObjParser
{
	/// <summary>
	/// Wavefront OBJ model: parses, stores, and writes geometry (v, vt, vn, f, l, p),
	/// materials (mtllib/usemtl), and grouping state (g, s, o).
	/// </summary>
	public class ObjModel
	{
		private readonly List<Vertex> _vertices;
		private readonly List<Face> _faces;
		private readonly List<TextureVertex> _textureVertices;
		private readonly List<VertexNormal> _normals;
		private readonly List<PointElement> _points;
		private readonly List<LineElement> _lines;

		// State-setting grouping attributes
		private string[] _currentGroups = Array.Empty<string>();
		private string? _currentSmoothing = null;
		private string? _currentObjectName = null;

		private BoundingBox _bounds;
		private bool _isBoundsDirty;

		/// <summary>Current material name (usemtl) applied to subsequently parsed elements.</summary>
		public string? CurrentMaterialName { get; set; }
		/// <summary>Material library filename (mtllib) for this model.</summary>
		public string? MaterialLibraryName { get; set; }

		/// <summary>Geometric vertices (v).</summary>
		public IReadOnlyList<Vertex> Vertices { get { return _vertices; } }
		/// <summary>Face elements (f).</summary>
		public IReadOnlyList<Face> Faces { get { return _faces; } }
		/// <summary>Texture vertices (vt).</summary>
		public IReadOnlyList<TextureVertex> TextureVertices { get { return _textureVertices; } }
		/// <summary>Vertex normals (vn).</summary>
		public IReadOnlyList<VertexNormal> Normals { get { return _normals; } }
		/// <summary>Point elements (p).</summary>
		public IReadOnlyList<PointElement> Points { get { return _points; } }
		/// <summary>Line elements (l).</summary>
		public IReadOnlyList<LineElement> Lines { get { return _lines; } }



		/// <summary>Axis-aligned bounding box extents for all vertices.</summary>
		public BoundingBox Bounds
		{
			get
			{
				if (_isBoundsDirty) UpdateBounds();
				return _bounds;
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
	    public ObjModel()
	    {
            _vertices = new List<Vertex>();
            _faces = new List<Face>();
            _textureVertices = new List<TextureVertex>();
            _normals = new List<VertexNormal>();
            _points = new List<PointElement>();
            _lines = new List<LineElement>();
            _bounds = new BoundingBox { XMax = 0, XMin = 0, YMax = 0, YMin = 0, ZMax = 0, ZMin = 0 };
            _isBoundsDirty = true;
        }

        /// <summary>
        /// Load .obj from a filepath.
        /// </summary>
        /// <param name="path">Path to an OBJ file on disk.</param>
        public void Load(string path)
        {
            Load(File.ReadAllLines(path));
        }

        /// <summary>
        /// Load .obj from a stream.
        /// </summary>
        /// <param name="data">Stream containing OBJ text.</param>
	    public void Load(Stream data)
	    {
            using (var reader = new StreamReader(data))
            {
                Load(reader.ReadToEnd().Split(Environment.NewLine.ToCharArray()));
            }
	    }

        /// <summary>
        /// Load .obj from a list of strings.
        /// </summary>
        /// <param name="data"></param>
	    public void Load(IEnumerable<string> data)
	    {
            foreach (var line in data)
            {
                ProcessLine(line);
            }

            UpdateBounds();
        }

		/// <summary>Write this OBJ to disk.</summary>
		/// <param name="path">Destination filesystem path.</param>
		/// <param name="headers">Optional header comments to include (without leading '#').</param>
		public void Save(string path, IEnumerable<string>? headers = null)
		{
			using (var outStream = File.Create(path))
			using (var writer = new StreamWriter(outStream, new UTF8Encoding(false)))
			{
				// Write some header data
			    WriteHeader(writer, headers);

				if (!string.IsNullOrEmpty(MaterialLibraryName))
				{
					writer.WriteLine("mtllib " + MaterialLibraryName);
				}

				for (int i = 0; i < _vertices.Count; i++)
					writer.WriteLine(_vertices[i].ToString());
				for (int i = 0; i < _textureVertices.Count; i++)
					writer.WriteLine(_textureVertices[i].ToString());
				for (int i = 0; i < _normals.Count; i++)
					writer.WriteLine(_normals[i].ToString());
				string lastMaterialName = "";
				string[] lastGroups = Array.Empty<string>();
				string? lastSmoothing = null;
				string? lastObjectName = null;
				foreach (Face face in _faces) {
					WriteGroupingState(writer, face.GroupNames, face.SmoothingGroup, face.ObjectName, ref lastGroups, ref lastSmoothing, ref lastObjectName);
					if (face.MaterialName != null && !face.MaterialName.Equals(lastMaterialName)) {
						writer.WriteLine("usemtl " + face.MaterialName);
						lastMaterialName = face.MaterialName;
					}
					writer.WriteLine(face);
				}
				for (int i = 0; i < _lines.Count; i++) {
					var le = _lines[i];
					WriteGroupingState(writer, le.GroupNames, le.SmoothingGroup, le.ObjectName, ref lastGroups, ref lastSmoothing, ref lastObjectName);
					writer.WriteLine(le.ToString());
				}
				for (int i = 0; i < _points.Count; i++) {
					var pe = _points[i];
					WriteGroupingState(writer, pe.GroupNames, pe.SmoothingGroup, pe.ObjectName, ref lastGroups, ref lastSmoothing, ref lastObjectName);
					writer.WriteLine(pe.ToString());
				}
			}
		}

		private void WriteGroupingState(StreamWriter writer, string[]? groups, string? smoothing, string? objectName,
			ref string[] lastGroups, ref string? lastSmoothing, ref string? lastObjectName)
		{
			// Groups: normalize null to empty and emit clear ("g") when transitioning to empty
			var normalizedGroups = (groups == null) ? Array.Empty<string>() : groups;
			if (!lastGroups.SequenceEqual(normalizedGroups))
			{
				if (normalizedGroups.Length > 0)
					writer.WriteLine("g " + string.Join(" ", normalizedGroups));
				else
					writer.WriteLine("g");
				lastGroups = normalizedGroups;
			}

			// Smoothing: normalize null/empty to "off"
			string normalizedSmoothing = string.IsNullOrEmpty(smoothing) ? "off" : smoothing!;
			string lastNormalizedSmoothing = string.IsNullOrEmpty(lastSmoothing) ? "off" : lastSmoothing!;
			if (!string.Equals(lastNormalizedSmoothing, normalizedSmoothing, StringComparison.Ordinal))
			{
				writer.WriteLine("s " + normalizedSmoothing);
				lastSmoothing = smoothing;
			}

			// Object: only emit when non-empty changes occur (no clear syntax in OBJ spec)
			if (!string.Equals(objectName, lastObjectName, StringComparison.Ordinal))
			{
				if (!string.IsNullOrEmpty(objectName))
				{
					writer.WriteLine("o " + objectName);
				}
				lastObjectName = objectName;
			}
		}

	    private void WriteHeader(StreamWriter writer, IEnumerable<string>? headerStrings)
	    {
	        if (headerStrings == null)
	        {
	            writer.WriteLine("# Generated by ObjParser");
	            return;
	        }

	        foreach (var line in headerStrings)
	        {
	            writer.WriteLine("# " + line);
	        }
	    }

	    /// <summary>
		/// Updates the cached axis-aligned bounding box
		/// </summary>
		private void UpdateBounds()
		{
            // If there are no vertices then size should be 0.
	        if (_vertices.Count == 0)
	        {
	            _bounds = new BoundingBox
	            {
                    XMax = 0,
                    XMin = 0,
                    YMax = 0,
                    YMin = 0,
                    ZMax = 0,
                    ZMin = 0
	            };
	            _isBoundsDirty = false;
	            return;
	        }

			double xmin = _vertices[0].X, xmax = _vertices[0].X;
			double ymin = _vertices[0].Y, ymax = _vertices[0].Y;
			double zmin = _vertices[0].Z, zmax = _vertices[0].Z;
			for (int i = 1; i < _vertices.Count; i++)
			{
				var v = _vertices[i];
				if (v.X < xmin) xmin = v.X; else if (v.X > xmax) xmax = v.X;
				if (v.Y < ymin) ymin = v.Y; else if (v.Y > ymax) ymax = v.Y;
				if (v.Z < zmin) zmin = v.Z; else if (v.Z > zmax) zmax = v.Z;
			}
			_bounds = new BoundingBox { XMin = xmin, XMax = xmax, YMin = ymin, YMax = ymax, ZMin = zmin, ZMax = zmax };
			_isBoundsDirty = false;
		}

		/// <summary>
		/// Parses and loads a line from an OBJ file.
		/// Currently supports v, vt, vn, f, l, p, mtllib, usemtl
		/// </summary>
		private void ProcessLine(string line)
		{
			if (string.IsNullOrWhiteSpace(line)) return;
			int hashIndex = line.IndexOf('#');
			if (hashIndex >= 0) line = line.Substring(0, hashIndex);
			line = line.Trim();
			if (line.Length == 0) return;

			string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0) return;

			switch (parts[0])
			{
				case "usemtl":
					if (parts.Length >= 2) CurrentMaterialName = parts[1];
					break;
				case "mtllib":
					if (parts.Length >= 2) MaterialLibraryName = parts[1];
					break;
				case "g":
					{
						_currentGroups = parts.Skip(1).ToArray();
						break;
					}
				case "s":
					{
						_currentSmoothing = parts.Length > 1 ? parts[1] : null;
						break;
					}
				case "o":
					{
						_currentObjectName = parts.Length > 1 ? parts[1] : null;
						break;
					}
				case "v":
					{
						Vertex v = new Vertex();
						v.LoadFromStringArray(parts);
						AddVertex(v);
						break;
					}
				case "f":
					{
						Face f = new Face();
						f.LoadFromStringArray(parts);
						if (f.MaterialName == null) f.MaterialName = CurrentMaterialName;
						f.GroupNames = _currentGroups;
						f.SmoothingGroup = _currentSmoothing;
						f.ObjectName = _currentObjectName;
						AddFace(f);
						break;
					}
				case "vt":
					{
						TextureVertex vt = new TextureVertex();
						vt.LoadFromStringArray(parts);
						AddTextureVertex(vt);
						break;
					}
				case "vn":
					{
						VertexNormal vn = new VertexNormal();
						vn.LoadFromStringArray(parts);
						AddNormal(vn);
						break;
					}
				case "p":
					{
						PointElement p = new PointElement();
						p.LoadFromStringArray(parts);
						p.GroupNames = _currentGroups;
						p.SmoothingGroup = _currentSmoothing;
						p.ObjectName = _currentObjectName;
						AddPoint(p);
						break;
					}
				case "l":
					{
						LineElement l = new LineElement();
						l.LoadFromStringArray(parts);
						l.GroupNames = _currentGroups;
						l.SmoothingGroup = _currentSmoothing;
						l.ObjectName = _currentObjectName;
						AddLine(l);
						break;
					}
			}
		}

		// Mutator API
		// Grouping state setters for programmatic group creation
		/// <summary>
		/// Sets the current active OBJ group names ("g"). New elements inherit these names by default.
		/// </summary>
		public void SetGroups(params string[] groupNames)
		{
			_currentGroups = groupNames ?? Array.Empty<string>();
		}

		/// <summary>
		/// Clears the current active OBJ group names ("g"). New elements will have no groups unless specified.
		/// </summary>
		public void ClearGroups()
		{
			_currentGroups = Array.Empty<string>();
		}

		/// <summary>
		/// Sets the current active smoothing group ("s"). Pass values like "off" or "1".
		/// </summary>
		public void SetSmoothingGroup(string smoothing)
		{
			_currentSmoothing = smoothing;
		}

		/// <summary>
		/// Clears the current active smoothing group. New elements will treat smoothing as off.
		/// </summary>
		public void ClearSmoothingGroup()
		{
			_currentSmoothing = null;
		}

		/// <summary>
		/// Sets the current active object name ("o"). New elements inherit this name by default.
		/// </summary>
		public void SetObjectName(string objectName)
		{
			_currentObjectName = objectName;
		}

		/// <summary>
		/// Clears the current active object name.
		/// </summary>
		public void ClearObjectName()
		{
			_currentObjectName = null;
		}
		/// <summary>Add a geometric vertex (v).</summary>
		/// <param name="vertex">Vertex to add.</param>
		/// <returns>1-based index assigned to the vertex.</returns>
		public int AddVertex(Vertex vertex)
		{
			_vertices.Add(vertex);
			vertex.Index = _vertices.Count;
			_isBoundsDirty = true;
			return vertex.Index;
		}

		/// <summary>Add a texture vertex (vt).</summary>
		/// <param name="textureVertex">Texture vertex to add.</param>
		/// <returns>1-based index assigned to the texture vertex.</returns>
		public int AddTextureVertex(TextureVertex textureVertex)
		{
			_textureVertices.Add(textureVertex);
			textureVertex.Index = _textureVertices.Count;
			return textureVertex.Index;
		}

		/// <summary>Add a face (f). Normalizes negative indices and validates ranges.</summary>
		/// <param name="face">Face to add.</param>
		/// <returns>1-based index of the new face.</returns>
		public int AddFace(Face face)
		{
			if (face.VertexIndexList == null || face.VertexIndexList.Length < 3)
				throw new ArgumentException("Face must contain at least 3 vertices.");
			// Inherit current grouping state when unspecified
			if (face.GroupNames == null) face.GroupNames = _currentGroups;
			if (face.SmoothingGroup == null) face.SmoothingGroup = _currentSmoothing;
			if (face.ObjectName == null) face.ObjectName = _currentObjectName;
			// Resolve negative vertex indices per OBJ spec (-1 is last defined vertex)
			for (int i = 0; i < face.VertexIndexList.Length; i++)
			{
				int idx = face.VertexIndexList[i];
				if (idx < 0)
				{
					idx = _vertices.Count + 1 + idx;
					face.VertexIndexList[i] = idx;
				}
				if (idx <= 0 || idx > _vertices.Count)
					throw new ArgumentOutOfRangeException("face.VertexIndexList", "Vertex index out of range.");
			}
			// Strict: if any positive texture vertex index is present, require vt list to exist and be in range
			if (face.TextureVertexIndexList != null && face.TextureVertexIndexList.Length > 0)
			{
				if (face.TextureVertexIndexList.Length != face.VertexIndexList.Length)
					throw new ArgumentException("Texture vertex index list length must match vertex index list length.");
				bool anyPresent = false;
				for (int i = 0; i < face.TextureVertexIndexList.Length; i++)
				{
					int t = face.TextureVertexIndexList[i];
					if (t != 0) anyPresent = true;
					if (t < 0)
					{
						int resolved = _textureVertices.Count + 1 + t;
						face.TextureVertexIndexList[i] = resolved;
					}
				}
				if (anyPresent)
				{
					if (_textureVertices.Count == 0)
						throw new ArgumentOutOfRangeException("face.TextureVertexIndexList", "Texture vertex indices present but no texture vertices defined.");
					for (int i = 0; i < face.TextureVertexIndexList.Length; i++)
					{
						int t = face.TextureVertexIndexList[i];
						if (t < 0 || t > _textureVertices.Count)
							throw new ArgumentOutOfRangeException("face.TextureVertexIndexList", "Texture vertex index out of range.");
					}
				}
			}
			// Normals: resolve negatives and validate ranges (lenient when no normals are defined)
			if (face.NormalIndexList != null && face.NormalIndexList.Length > 0)
			{
				if (face.NormalIndexList.Length != face.VertexIndexList.Length)
					throw new ArgumentException("Normal index list length must match vertex index list length.");
				bool anyPresent = false;
				for (int i = 0; i < face.NormalIndexList.Length; i++)
				{
					if (face.NormalIndexList[i] != 0) anyPresent = true;
				}
				if (anyPresent && _normals.Count > 0)
				{
					// Only resolve negatives and validate when normals exist in the model
					for (int i = 0; i < face.NormalIndexList.Length; i++)
					{
						int n = face.NormalIndexList[i];
						if (n < 0)
						{
							int resolved = _normals.Count + 1 + n;
							face.NormalIndexList[i] = resolved;
						}
						int current = face.NormalIndexList[i];
						if (current < 0 || current > _normals.Count)
							throw new ArgumentOutOfRangeException("face.NormalIndexList", "Normal index out of range.");
					}
				}
				// If any present but no normals defined, allow and skip strict validation to match legacy behavior
			}
			if (face.MaterialName == null) face.MaterialName = CurrentMaterialName;
			_faces.Add(face);
			return _faces.Count;
		}

		/// <summary>Remove a face by 1-based element index.</summary>
		/// <param name="index1Based">1-based face index.</param>
		public void RemoveFace(int index1Based)
		{
			int i = index1Based - 1;
			if (i < 0 || i >= _faces.Count) throw new ArgumentOutOfRangeException(nameof(index1Based));
			_faces.RemoveAt(i);
		}

		/// <summary>Remove a vertex by 1-based element index. Fails if referenced by a face.</summary>
		/// <param name="index1Based">1-based vertex index.</param>
		public void RemoveVertex(int index1Based)
		{
			int i = index1Based - 1;
			if (i < 0 || i >= _vertices.Count) throw new ArgumentOutOfRangeException(nameof(index1Based));
			foreach (var face in _faces)
			{
				for (int k = 0; k < face.VertexIndexList.Length; k++)
				{
					if (face.VertexIndexList[k] == index1Based)
						throw new InvalidOperationException("Cannot remove vertex that is referenced by a face.");
				}
			}
			_vertices.RemoveAt(i);
			for (int k = i; k < _vertices.Count; k++) _vertices[k].Index = k + 1;
			_isBoundsDirty = true;
		}

		/// <summary>Clear all elements and reset size.</summary>
		public void Clear()
		{
			_vertices.Clear();
			_textureVertices.Clear();
			_faces.Clear();
			_normals.Clear();
			_points.Clear();
			_lines.Clear();
			_isBoundsDirty = true;
		}

		/// <summary>Add a vertex normal (vn).</summary>
		/// <param name="normal">Normal to add.</param>
		/// <returns>1-based index assigned to the normal.</returns>
		public int AddNormal(VertexNormal normal)
		{
			_normals.Add(normal);
			normal.Index = _normals.Count;
			return normal.Index;
		}

		/// <summary>Add a point element (p). Normalizes negative indices and validates ranges.</summary>
		/// <param name="p">Point element to add.</param>
		/// <returns>1-based index of the new point element.</returns>
		public int AddPoint(PointElement p)
		{
			// Inherit current grouping state when unspecified
			if (p.GroupNames == null) p.GroupNames = _currentGroups;
			if (p.SmoothingGroup == null) p.SmoothingGroup = _currentSmoothing;
			if (p.ObjectName == null) p.ObjectName = _currentObjectName;
			// Normalize negative vertex indices
			for (int i = 0; i < p.VertexIndexList.Length; i++)
			{
				int idx = p.VertexIndexList[i];
				if (idx < 0) p.VertexIndexList[i] = _vertices.Count + 1 + idx;
				if (p.VertexIndexList[i] <= 0 || p.VertexIndexList[i] > _vertices.Count)
					throw new ArgumentOutOfRangeException("p.VertexIndexList", "Vertex index out of range.");
			}
			_points.Add(p);
			return _points.Count;
		}

		/// <summary>Add a line element (l). Normalizes negative indices and validates ranges.</summary>
		/// <param name="l">Line element to add.</param>
		/// <returns>1-based index of the new line element.</returns>
		public int AddLine(LineElement l)
		{
			// Inherit current grouping state when unspecified
			if (l.GroupNames == null) l.GroupNames = _currentGroups;
			if (l.SmoothingGroup == null) l.SmoothingGroup = _currentSmoothing;
			if (l.ObjectName == null) l.ObjectName = _currentObjectName;
			for (int i = 0; i < l.VertexIndexList.Length; i++)
			{
				int idx = l.VertexIndexList[i];
				if (idx < 0) l.VertexIndexList[i] = _vertices.Count + 1 + idx;
				if (l.VertexIndexList[i] <= 0 || l.VertexIndexList[i] > _vertices.Count)
					throw new ArgumentOutOfRangeException("l.VertexIndexList", "Vertex index out of range.");
			}
			if (l.TextureVertexIndexList != null && l.TextureVertexIndexList.Length > 0)
			{
				for (int i = 0; i < l.TextureVertexIndexList.Length; i++)
				{
					int t = l.TextureVertexIndexList[i];
					if (t < 0) l.TextureVertexIndexList[i] = _textureVertices.Count + 1 + t;
					if (l.TextureVertexIndexList[i] < 0 || (_textureVertices.Count > 0 && l.TextureVertexIndexList[i] > _textureVertices.Count))
						throw new ArgumentOutOfRangeException("l.TextureVertexIndexList", "Texture vertex index out of range.");
				}
			}
			_lines.Add(l);
			return _lines.Count;
		}

		// Convenience grouping view by `g` group names
		/// <summary>
		/// A view of elements grouped by their OBJ group names ("g"), without duplicating data.
		/// </summary>
		public class MeshGroup
		{
			/// <summary>Group name tokens from the OBJ "g" statement.</summary>
			public string[] GroupNames { get; set; }
			/// <summary>Faces that belong to this group.</summary>
			public List<Face> Faces { get; private set; }
			/// <summary>Line elements that belong to this group.</summary>
			public List<LineElement> Lines { get; private set; }
			/// <summary>Point elements that belong to this group.</summary>
			public List<PointElement> Points { get; private set; }
			internal int FirstSeenOrder { get; set; }

			/// <summary>Create an empty mesh group.</summary>
			public MeshGroup()
			{
				GroupNames = Array.Empty<string>();
				Faces = new List<Face>();
				Lines = new List<LineElement>();
				Points = new List<PointElement>();
			}
		}

		/// <summary>
		/// Builds a stable-ordered list of groups keyed by their OBJ group names ("g").
		/// </summary>
		/// <summary>Builds a stable-ordered list of mesh groups keyed by OBJ group names.</summary>
		/// <returns>Groups in the order first encountered while reading/building.</returns>
		public IReadOnlyList<MeshGroup> BuildMeshGroups()
		{
			string KeyOf(string[]? names)
			{
				if (names == null || names.Length == 0) return string.Empty;
				return string.Join(" ", names);
			}

			int order = 0;
			var map = new Dictionary<string, MeshGroup>(StringComparer.Ordinal);

			void EnsureAndAdd(string[]? names, Action<MeshGroup> add)
			{
				var key = KeyOf(names);
				if (!map.TryGetValue(key, out var grp))
				{
					grp = new MeshGroup { GroupNames = names ?? Array.Empty<string>(), FirstSeenOrder = order++ };
					map[key] = grp;
				}
				add(grp);
			}

			foreach (var f in _faces) EnsureAndAdd(f.GroupNames, g => g.Faces.Add(f));
			foreach (var l in _lines) EnsureAndAdd(l.GroupNames, g => g.Lines.Add(l));
			foreach (var p in _points) EnsureAndAdd(p.GroupNames, g => g.Points.Add(p));

			return map.Values.OrderBy(g => g.FirstSeenOrder).ToList();
		}

	}
}

