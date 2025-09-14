# ObjParser

[![CI](https://github.com/stefangordon/ObjParser/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/stefangordon/ObjParser/actions/workflows/ci.yml)
[![Tests](https://img.shields.io/github/actions/workflow/status/stefangordon/ObjParser/ci.yml?branch=master&label=tests)](https://github.com/stefangordon/ObjParser/actions/workflows/ci.yml)

A small, dependency‑free C# library for reading and writing Wavefront OBJ/MTL files.

- Supports geometry: `v`, `vt`, `vn`, `f`, `l`, `p`
- Supports grouping/state: `g`, `s`, `o`
- Supports materials: `mtllib`, `usemtl`
- Target frameworks: `netstandard2.0`, `net8.0`

## Quick start

Install from source (or add the project as a reference):

```bash
git clone https://github.com/stefangordon/ObjParser.git
cd ObjParser
dotnet build
dotnet test
```

## Examples

### Read an OBJ and inspect bounds

```csharp
using ObjParser;

var obj = new ObjModel();
obj.Load("Example/sample.obj");

Console.WriteLine($"Vertices: {obj.Vertices.Count}, Faces: {obj.Faces.Count}");
var b = obj.Bounds;
Console.WriteLine($"Bounds: X[{b.XMin}, {b.XMax}] Y[{b.YMin}, {b.YMax}] Z[{b.ZMin}, {b.ZMax}]");
```

### Build a triangle and write it out

```csharp
using ObjParser;
using ObjParser.Types;

var obj = new ObjModel();

int v1 = obj.AddVertex(new Vertex { X = 0, Y = 0, Z = 0 });
int v2 = obj.AddVertex(new Vertex { X = 1, Y = 0, Z = 0 });
int v3 = obj.AddVertex(new Vertex { X = 0, Y = 1, Z = 0 });

obj.SetGroups("G1");
obj.SetSmoothingGroup("1");
obj.SetObjectName("Triangle");

obj.AddFace(new Face { VertexIndexList = new[] { v1, v2, v3 } });

obj.Save("triangle.obj", new[] { "Created with ObjParser" });
```

### Add UVs, lines, and points

```csharp
using ObjParser;
using ObjParser.Types;

var obj = new ObjModel();

int v1 = obj.AddVertex(new Vertex { X = 0, Y = 0, Z = 0 });
int v2 = obj.AddVertex(new Vertex { X = 1, Y = 0, Z = 0 });
int v3 = obj.AddVertex(new Vertex { X = 1, Y = 1, Z = 0 });
int v4 = obj.AddVertex(new Vertex { X = 0, Y = 1, Z = 0 });

int t1 = obj.AddTextureVertex(new TextureVertex { X = 0, Y = 0 });
int t2 = obj.AddTextureVertex(new TextureVertex { X = 1, Y = 0 });
int t3 = obj.AddTextureVertex(new TextureVertex { X = 0, Y = 1 });

obj.SetGroups("Edge", "Wire");
obj.AddLine(new LineElement { VertexIndexList = new[] { v1, v2, v3, v4 }, TextureVertexIndexList = new[] { 0, 0, 0, 0 } });

obj.SetGroups("Points");
obj.AddPoint(new PointElement { VertexIndexList = new[] { v1, v3 } });

obj.SetGroups("Textured");
obj.AddFace(new Face {
    VertexIndexList = new[] { v1, v2, v3 },
    TextureVertexIndexList = new[] { t1, t2, t3 }
});

obj.Save("example.obj", new[] { "Created with ObjParser" });
```

### Work with materials (MTL)

```csharp
using ObjParser;

// Read/write an MTL file
var mtl = new MaterialLibrary();
mtl.LoadMtl("Example/sample.mtl");
mtl.WriteMtlFile("materials_out.mtl", new[] { "Created with ObjParser" });

// Reference an MTL from an OBJ
var obj = new ObjModel();
obj.MaterialLibraryName = "materials_out.mtl";   // reference in written OBJ
obj.CurrentMaterialName = "Material";            // default material for subsequently added faces
```

### Grouped views

Build an in‑memory view of elements by OBJ group names (`g`):

```csharp
var groups = obj.BuildMeshGroups();
foreach (var g in groups)
{
    Console.WriteLine($"Group: {string.Join(" ", g.GroupNames)}  Faces: {g.Faces.Count}  Lines: {g.Lines.Count}  Points: {g.Points.Count}");
}
```

## Target frameworks

- `netstandard2.0` (runs on .NET Framework 4.6.1+, .NET Core 2.0+, Mono, Unity, etc.)
- `net8.0`

## Build and test locally

```bash
dotnet restore ObjParser.sln
dotnet build ObjParser.sln -c Release
dotnet test ObjParser_Tests/ObjParser_Tests.csproj -c Release
```

## License

MIT — see `LICENSE` for details.
