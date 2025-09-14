## How to run the Example app

This console app loads `sample.obj`/`sample.mtl`, crops the mesh to the left half along the X axis, and writes a new `sample_cropped.obj` next to the inputs.

### Prerequisites

- **.NET SDK 8.0+** — install from the [Microsoft .NET SDK download page](https://dotnet.microsoft.com/download).

### From the repo root

```bash
dotnet run --project Example
```

### From the `Example/` folder

```bash
cd Example
dotnet run
```

### Release build (optional)

```bash
dotnet run --project Example -c Release
```

### What it does / outputs

- Reads `sample.obj` and `sample.mtl` (these are copied to the output directory on build).
- Crops faces whose vertices are entirely on the left of the mid‑X plane.
- Writes `sample_cropped.obj` and ensures the referenced MTL is present.
- You will see console messages like:

```text
Wrote: <path>\sample_cropped.obj
Wrote/Updated MTL: <path>\sample.mtl
```

### Where to find the files

- Debug: `Example/bin/Debug/net8.0/`
- Release: `Example/bin/Release/net8.0/`

### Troubleshooting

- **Missing sample files**: If you see “Could not find sample.obj…”, make sure you’re running with `dotnet run` so the assets are copied to the output directory, or verify `sample.obj`, `sample.mtl`, and `sample.jpg` exist under the corresponding `bin/<Config>/net8.0/` folder.
- **IDE usage**: In Visual Studio, set `Example` as the startup project and press Run. Assets will be copied automatically.


