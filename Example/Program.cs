using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ObjParser;
using ObjParser.Types;

// This example demonstrates how to:
// 1) Load an OBJ with its MTL
// 2) Crop the geometry by a plane (middle along X)
// 3) Remap indices and write the result to new OBJ/MTL files
//
// It uses the sample files located in this directory: sample.obj, sample.mtl, sample.jpg.
// The project file copies these assets to the output directory on build.

static class Program
{
    static int Main(string[] args)
    {
        try
        {
            // Resolve input/output paths (run from bin folder or repo root). The csproj copies the samples to output.
            string baseDir = AppContext.BaseDirectory;
            string inputObjPath = Path.Combine(baseDir, "sample.obj");
            string inputMtlPath = Path.Combine(baseDir, "sample.mtl");

            if (!File.Exists(inputObjPath))
            {
                Console.Error.WriteLine("Could not find sample.obj at: " + inputObjPath);
                return 1;
            }
            if (!File.Exists(inputMtlPath))
            {
                Console.Error.WriteLine("Could not find sample.mtl at: " + inputMtlPath);
                return 1;
            }

            // 1) Load OBJ
            var obj = new ObjModel();
            obj.Load(inputObjPath);

            // If the OBJ references an MTL, keep the same reference name so the new OBJ points to the same MTL file name
            // We will simply copy the MTL alongside the cropped OBJ (no material filtering necessary in this simple sample).
            // If the input did not specify mtllib, set it to the local sample file name.
            obj.MaterialLibraryName ??= Path.GetFileName(inputMtlPath);

            // Compute crop plane: split in the middle along X
            var bounds = obj.Bounds; // uses axis-aligned bounding box over vertices
            double midX = (bounds.XMin + bounds.XMax) * 0.5;

            // 2) Build a cropped model that only includes geometry where all face vertices are on one half.
            // Here we keep faces whose all vertex X <= midX (left half). Adjust comparison to choose the other half.
            var cropped = CropByX(obj, keepLeftHalf: true, boundaryX: midX);
            cropped.MaterialLibraryName = obj.MaterialLibraryName; // preserve material library reference

            // 3) Write outputs next to inputs
            string outputObjPath = Path.Combine(baseDir, "sample_cropped.obj");
            string outputMtlPath = Path.Combine(baseDir, Path.GetFileName(obj.MaterialLibraryName!));

            // Write OBJ with a helpful header
            cropped.Save(outputObjPath, new[]
            {
                "Example crop: kept faces with all vertices X <= midX",
                $"midX = {midX.ToString(CultureInfo.InvariantCulture)}",
            });

            // Also write/copy MTL (for simplicity, just copy input MTL as-is)
            if (!File.Exists(outputMtlPath) || !PathsEqual(inputMtlPath, outputMtlPath))
            {
                File.Copy(inputMtlPath, outputMtlPath, overwrite: true);
            }

            Console.WriteLine("Wrote: " + outputObjPath);
            Console.WriteLine("Wrote/Updated MTL: " + outputMtlPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 2;
        }
    }

    /// <summary>
    /// Builds a new OBJ that keeps only the faces entirely on one side of a vertical plane X = boundaryX.
    /// - keepLeftHalf: when true, keep faces where all vertices have X <= boundaryX; otherwise X >= boundaryX.
    /// - Remaps vertex/texture/normal indices and copies only referenced elements.
    /// - Preserves grouping state (g, s, o) and per-face material (usemtl) where applicable.
    /// </summary>
    private static ObjModel CropByX(ObjModel source, bool keepLeftHalf, double boundaryX)
    {
        // 1) Decide which source vertices are referenced by kept faces
        var keepFace = new bool[source.Faces.Count];
        var keepVertex = new bool[source.Vertices.Count];
        var keepTex = new bool[source.TextureVertices.Count];
        var keepNorm = new bool[source.Normals.Count];

        for (int f = 0; f < source.Faces.Count; f++)
        {
            var face = source.Faces[f];
            bool allOnSide = true;
            for (int i = 0; i < face.VertexIndexList.Length; i++)
            {
                int vIdx1 = face.VertexIndexList[i]; // 1-based
                var v = source.Vertices[vIdx1 - 1];
                bool onSide = keepLeftHalf ? (v.X <= boundaryX) : (v.X >= boundaryX);
                if (!onSide)
                {
                    allOnSide = false;
                    break;
                }
            }
            if (allOnSide)
            {
                keepFace[f] = true;
                // Mark used vertices/tex/normals
                for (int i = 0; i < face.VertexIndexList.Length; i++)
                {
                    keepVertex[face.VertexIndexList[i] - 1] = true;
                    if (face.TextureVertexIndexList != null && face.TextureVertexIndexList.Length > i && face.TextureVertexIndexList[i] > 0)
                        keepTex[face.TextureVertexIndexList[i] - 1] = true;
                    if (face.NormalIndexList != null && face.NormalIndexList.Length > i && face.NormalIndexList[i] > 0)
                        keepNorm[face.NormalIndexList[i] - 1] = true;
                }
            }
        }

        // 2) Build index remapping tables (1-based -> 1-based continuous)
        int[] vertexMap = BuildIndexRemap(keepVertex);
        int[] texMap = BuildIndexRemap(keepTex);
        int[] normMap = BuildIndexRemap(keepNorm);

        // 3) Create the new OBJ with only kept elements
        var dst = new ObjModel();
        // Copy material library reference; actual file is copied by caller
        dst.MaterialLibraryName = source.MaterialLibraryName;

        // Add vertices
        for (int i = 0; i < source.Vertices.Count; i++)
        {
            if (!keepVertex[i]) continue;
            var sv = source.Vertices[i];
            dst.AddVertex(new Vertex { X = sv.X, Y = sv.Y, Z = sv.Z, W = sv.W });
        }

        // Add texture vertices
        for (int i = 0; i < source.TextureVertices.Count; i++)
        {
            if (!keepTex[i]) continue;
            var st = source.TextureVertices[i];
            dst.AddTextureVertex(new TextureVertex { X = st.X, Y = st.Y, Z = st.Z });
        }

        // Add normals
        for (int i = 0; i < source.Normals.Count; i++)
        {
            if (!keepNorm[i]) continue;
            var sn = source.Normals[i];
            dst.AddNormal(new VertexNormal { I = sn.I, J = sn.J, K = sn.K });
        }

        // Recreate faces with remapped indices and preserved grouping/material info
        for (int f = 0; f < source.Faces.Count; f++)
        {
            if (!keepFace[f]) continue;
            var sf = source.Faces[f];

            var df = new Face
            {
                MaterialName = sf.MaterialName,
                GroupNames = sf.GroupNames,
                SmoothingGroup = sf.SmoothingGroup,
                ObjectName = sf.ObjectName,
                VertexIndexList = RemapList(sf.VertexIndexList, vertexMap),
                TextureVertexIndexList = (sf.TextureVertexIndexList != null && sf.TextureVertexIndexList.Length == sf.VertexIndexList.Length)
                    ? RemapListAllowZero(sf.TextureVertexIndexList, texMap)
                    : new int[sf.VertexIndexList.Length],
                NormalIndexList = (sf.NormalIndexList != null && sf.NormalIndexList.Length == sf.VertexIndexList.Length)
                    ? RemapListAllowZero(sf.NormalIndexList, normMap)
                    : new int[sf.VertexIndexList.Length]
            };

            dst.AddFace(df);
        }

        return dst;
    }

    private static int[] BuildIndexRemap(bool[] keep)
    {
        var remap = new int[keep.Length];
        int next = 1;
        for (int i = 0; i < keep.Length; i++)
        {
            remap[i] = keep[i] ? next++ : 0;
        }
        return remap; // maps old 1-based index-1 to new 1-based index; 0 means not kept
    }

    private static int[] RemapList(int[] src, int[] map)
    {
        var dst = new int[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            int old1 = src[i];
            int new1 = map[old1 - 1];
            if (new1 == 0)
                throw new InvalidOperationException("Encountered reference to a vertex that was not kept.");
            dst[i] = new1;
        }
        return dst;
    }

    private static int[] RemapListAllowZero(int[] src, int[] map)
    {
        var dst = new int[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            int old1 = src[i];
            if (old1 <= 0)
            {
                dst[i] = 0;
            }
            else
            {
                dst[i] = map[old1 - 1]; // may be 0 if not kept
            }
        }
        return dst;
    }

    private static bool PathsEqual(string a, string b)
    {
        try
        {
            return Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                 .Equals(Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}

