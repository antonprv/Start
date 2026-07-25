# Streaming: Usage Guide

## 1. Architecture Overview

The system consists of two layers, following the same design as the Bepu integration.

- **`src/Framework/Streaming/Core/`** (`Streaming.Integration.csproj`) - a pure .NET
  library with no dependency on Godot. This layer contains `StreamingWorld`
  (the streaming scheduler), `StreamableResource` (the base class for every streamable
  resource), `ChunkStorage` (chunk table and storage implementation), and
  `IChunkDataSource` (an abstraction for reading chunk data from disk).

- **`src/Setup/addons/Streaming/`** - a thin Godot integration layer containing
  `StreamingWorldNode` (autoload), `StreamableTexture2D`,
  `StreamableMeshInstance3D`, `GodotChunkDataSource`, and `Plugin.cs`.

The package **does not include an asset baker**. The baking tool must be implemented
specifically for your content pipeline (see section 4). Minimal working examples are
provided below.

## 2. Installation

1. Extract `src/` into your project, preserving the directory structure (it matches the
   existing `Physics` integration).

2. Add `StreamingWorldNode` as an autoload:

   **Project >> Project Settings >> Autoload**

   Select `StreamingWorldNode.cs` and register it under a name such as
   `StreamingWorld`.

3. Register `IStreamingWorld` in Zenjex the same way `IPhysicsWorld` is registered
   for `PhysicsWorldNode` (the code is intentionally omitted - follow the existing
   Physics integration).

   Also register `IAssetManifestService` as a regular singleton:

   ```csharp
   new AssetManifestService()
   ```

   Unlike `StreamingWorldNode`, this is **not** a `Node` and **does not** need to be
   an autoload. It has no dependency on the scene tree (see section 6).

4. If you're packaging the integration as a standalone Godot addon (rather than
   embedding it directly into your project), configure `Plugin.cfg` to use
   `Plugin.cs` as the addon entry point, exactly like the Physics addon.

After installation, the streaming budgets can be configured directly from the
`StreamingWorldNode` inspector:

- `_bytesPerTick`
- `_maxResourceUpdatesPerTick`
- `_fullDetailDistance`
- `_minDetailDistance`
- `_memoryBudgetMb`
- `_tickIntervalSeconds`

See section 7 for a detailed explanation of each setting.

## 3. On-Disk File Format

The container format written by `ChunkStorage.Pack()` and read by
`GodotChunkDataSource` is:

```text
[int32 chunkCount]
[chunkCount * (int64 offset, int32 size)]   // offset is relative to the beginning of the data section, NOT the beginning of the file
[raw chunk 0 bytes][raw chunk 1 bytes]...
```

Each chunk index represents a residency level.

**In this implementation, residency level `0` always represents the lowest-quality
version of the resource** (smallest texture mip or lowest-detail mesh LOD), while
`MaxResidency` represents the highest available quality.

This ordering is intentionally reversed compared to the texture streaming
implementation in the Godot 4.7 fork, where mip level `0` represents the highest
resolution. Keep this difference in mind when comparing the two implementations.

The Core library intentionally has no knowledge of the contents of individual chunks.
The binary layout of each chunk is entirely defined by the corresponding
`StreamableResource` implementation.

## 4. Asset Baking

### Textures - TextureBaker Utility

A ready-to-use baking tool is available in
`src/Framework/Streaming/TextureBaker/` (`TextureBaker.csproj`).

This is **not** a Godot EditorScript. Instead, it is a standalone .NET 8
WinForms/CLI application that compresses textures into BC1 (opaque textures)
or BC3 (textures with alpha, also known as DXT5) using the managed
BCnEncoder.NET library.

Since BCnEncoder.NET has **no native dependencies**, the baker can be built
and executed on both Windows and Linux CI runners.

BC1/BC3 (the S3TC family) were intentionally chosen because they are the only
compressed formats that work consistently across:

- Godot Compatibility (GLES3)
- Godot Vulkan renderer
- Windows
- Linux

without requiring separate asset pipelines for each platform.

ETC2 primarily targets mobile GPUs, while BC7 requires DX10-style DDS headers
and cannot be assumed to be universally supported. For that reason, BC7 is
intentionally not included in this baker.

The project can be built like any regular .NET application.

After every build, the binaries are automatically copied into

```
src/Setup/tools/TextureBaker/
```

via the `CopyToGodotTools` target defined in the project file. This means there
is no need to manually copy the executable before testing it from either the
editor or the command line.

### Two Modes, One Executable

```text
TextureBaker.exe --input=Art/wall_albedo.png --output=Streamed/wall_albedo.stream
TextureBaker.exe --input=Art/Textures --output=Streamed/Textures      # recursively process an entire directory
TextureBaker.exe app                                                   # launch the GUI
```

When started **without arguments** (or with `--input` / `--output`), the
application behaves as a regular command-line utility and returns a standard
exit code (`0` on success, non-zero on failure). This makes it easy to integrate
into CI/CD pipelines and automatically fail builds when invalid assets are
detected.

When started with the single argument

```text
app
```

the WinForms GUI is launched for quick manual testing.

Typical workflow:

```
Select texture
        ↓
Output .stream path is filled automatically
        ↓
Press Bake
        ↓
Review the log
        ↓
Assign the generated file to StreamableTexture2D
```

The output format can either be detected automatically (`Auto` scans the image
for partial alpha) or selected explicitly:

- CLI:

```text
--format=bc1
--format=bc3
```

- GUI:

Use the format dropdown.

The baker references the same `Streaming.Integration.csproj` used by the runtime
and writes files through the same `ChunkStorage.Pack()` implementation.

Because both the runtime and the baking tool share exactly the same
serialization code, they **cannot accidentally diverge** in their understanding
of the container format.

### Meshes

`GodotStreamableMesh.PrepareChunk()` expects each LOD level to contain data in
the following layout:

```text
[int32 vertexCount][int32 indexCount]
[vertexCount * (float px,py,pz, nx,ny,nz, u,v)]
[indexCount * int32]
```

A mesh baker is **not included yet**.

Generating a proper LOD chain requires an external mesh simplification library
(such as **meshoptimizer**).

Until such a baker is implemented, you can simply pack a single mesh into
`ChunkStorage.Pack()` (`MaxResidency == 0`). The result is a fully functional
`ArrayMesh` that does not stream between LOD levels, allowing you to add
progressive LOD support later without changing the runtime.

## 5. Scene Integration

Both components now reference assets by **asset name** rather than by file path.

The actual `res://...stream` path is resolved once during `_Ready()` through
`IAssetManifestService` (see section 6).

### Textures

Add a `StreamableTexture2D` (`Node3D`) next to the object that requires the
texture.

Set **Asset Name** (for example `wall_albedo`) instead of a file path, then
either subscribe to the `TextureUpdated` event or poll `CurrentTexture`.

```csharp
public override void _Ready()
{
    _streamableTexture.TextureUpdated += tex =>
        _material.SetShaderParameter("albedo", tex);
}
```

`CurrentTexture` remains `null` until the first chunk has finished streaming,
so keep that in mind if you attempt to use it immediately from `_Ready()`.

If the specified asset name cannot be found in the manifest, the component logs
an error and becomes inactive.

This behavior is intentional.

A missing baked asset is considered a content pipeline error and should fail
loudly instead of silently returning `null`.

### Meshes

`StreamableMeshInstance3D` derives directly from `MeshInstance3D`.

Simply specify **Asset Name** instead of assigning a `Mesh` in the Inspector.

If you use `MaterialOverride`, it continues to work normally-the material is
not reset when the streamed mesh changes.

Both components automatically register themselves with `StreamingWorld`
through dependency injection (`IStreamingWorld` +
`IAssetManifestService`) and unregister themselves in `_ExitTree()`.

## 6. Asset Names and the Manifest (Cook Directory)

The idea is simple: `StreamableTexture2D` and `StreamableMeshInstance3D`
reference assets by **name**, not by file path.

For example:

```
wall_albedo
```

instead of:

```
res://Cook/Textures/Walls/wall_albedo.stream
```

The actual `res://...stream` path is resolved through an asset manifest-a flat
lookup table generated by the baking tool and loaded once by
`AssetManifestService`.

### Why Use Asset Names Instead of Paths?

Assets are frequently moved or reorganized during development.

If every component stored a physical file path, every scene referencing that
asset would need to be updated after a rename or directory restructure.

Using asset names completely decouples scenes from the project's folder layout.
Only the manifest needs to be regenerated; scenes remain untouched.

### Manifest Format

`Framework.Streaming.AssetManifest` (Core, `AssetManifest.cs`) uses a minimal
binary format instead of JSON or MessagePack:

```text
[int32 version]
[int32 count]
[count * (string key, string value)]
```

Strings are written using the standard
`BinaryWriter.Write(string)` / `BinaryReader.ReadString()`
implementation (length-prefixed UTF-8).

This keeps both serialization and loading extremely fast.

The entire manifest is loaded into a `Dictionary<string, string>` in a single
pass, with no random-access file operations.

An important design decision is that `AssetManifest.Save()` and
`AssetManifest.Load()` operate on **Stream** objects rather than file paths.

The Core library never opens files directly-the same philosophy used by
`IChunkDataSource`.

At runtime, the manifest is loaded through Godot's `FileAccess`
(`AssetManifestService.cs`) rather than `System.IO.File`.

This distinction is essential because, after exporting a project, the `res://`
filesystem resides inside a `.pck` archive and is therefore invisible to
standard file APIs.

The baking tool, on the other hand, runs as a normal .NET application on the
developer's machine or a CI agent, so using `System.IO` there is perfectly
appropriate (`ManifestTool.cs`).

### Generating the Manifest

When running the baker in batch mode (`--input` points to a directory), provide
the following additional arguments:

```text
TextureBaker.exe --input=Art/Textures --output=Cook --manifest=Cook/manifest.bin --res-prefix=res://Cook/
```

Every successfully baked asset adds an entry in the following form:

```
asset_name_without_extension
        ↓
res://Cook/<relative path>.stream
```

The batch mode **always rebuilds the manifest from scratch**.

It never appends to an existing file.

This is intentional: if a source texture is renamed or deleted, the old entry
should disappear instead of leaving a stale reference pointing to a file that
will never be generated again.

If you bake multiple directories independently, either:

- generate a separate manifest for each run, or
- perform a single bake over the common root directory.

### Asset Name Collisions

Duplicate asset names are treated as a **build error**.

The manifest key consists solely of the filename without its extension or
directory.

For example:

```
Textures/Walls/albedo.png
Textures/Floors/albedo.png
```

would both produce the key

```
albedo
```

Instead of silently overwriting one entry with another (and later wondering why
the wrong texture appears in-game), the baker immediately reports an error and
asks you to rename one of the files.

Since this design assumes globally unique asset names, it is much better to
enforce that rule during CI than to discover the problem at runtime.

### GUI Mode

When baking a single texture through the GUI (`TextureBaker.exe app`), the
baker updates exactly **one** manifest entry.

If the manifest already exists, the corresponding record is replaced.

Otherwise, a new manifest is created.

The baker automatically converts the generated `.stream` file path into a
`res://...` path by searching for `project.godot` up to twelve directory levels
above the output location.

If no Godot project is found, the manifest is left unchanged and a message is
written to the log.

In that case, the entry can still be added later through the CLI or manually.

### Loading the Manifest at Runtime

`AssetManifestService.EnsureLoaded()` loads and parses the manifest exactly
once.

This can happen lazily on the first call to `Resolve()`, or explicitly during a
loading screen by calling `EnsureLoaded()` yourself.

Doing so avoids paying even the small parsing cost during gameplay, especially
for projects containing thousands of assets.

After the manifest has been loaded, every subsequent `Resolve()` call is simply
a `Dictionary.TryGetValue()` lookup.

No additional disk access occurs.

Each `StreamableTexture2D` and `StreamableMeshInstance3D` performs exactly one
lookup during `_Ready()`.

Since `_Ready()` is called once per instantiated scene, each asset path is
resolved only once after the level has loaded.

## 7. Budget Configuration

`StreamingBudget` can be configured directly from the
`StreamingWorldNode` Inspector or constructed manually at runtime (for example,
when switching to a more aggressive streaming profile during loading screens).

| Property | Description |
|----------|-------------|
| `BytesPerTick` | Maximum number of bytes that may begin loading during a single `Update()` call. |
| `MaxResourceUpdatesPerTick` | Maximum number of resources that may be processed per update, regardless of the byte budget. |
| `FullDetailDistance` / `MinDetailDistance` | Distance range used to linearly interpolate the target residency level. |
| `UnusedChunkLifetime` | How long an unused chunk remains resident before being unloaded. |
| `MemoryBudgetBytes` | Global memory budget for all streamed resources. When exceeded, the scheduler lowers the target residency of distant resources before scheduling additional loads. |

Example: temporarily disable streaming limits during a loading screen.

```csharp
_streamingWorldNode.Core.Budget =
    new StreamingBudget(bytesPerTick: long.MaxValue);

// Wait for several Update() ticks until all resources are loaded...

_streamingWorldNode.Core.Budget = normalBudget;
```

## 8. Notes and Limitations

The following limitations are either intentional design decisions or current
implementation constraints. Reading this section is strongly recommended before
integrating the streaming system into a production project.

- **Every residency change fully recreates the texture.**

  Increasing or decreasing residency does **not** upload individual mip levels
  into an existing GPU texture. Instead, the entire texture is recreated each
  time.

  This is the same approach used internally by Godot's
  `ImageTexture.SetImage()`, which ultimately recreates the underlying GPU
  resource through `texture_2d_create()` and `texture_replace()`.

  This is therefore **not** a limitation of this integration, but rather a
  consequence of the current public Godot 4.7 API.

  As residency increases, each subsequent quality upgrade becomes progressively
  more expensive.

- **Partial mip updates through `RenderingDevice` are intentionally avoided.**

  Several Godot 4.x releases contain known issues when updating non-zero mip
  levels directly through `RenderingDevice`.

  If you plan to switch to this approach in the future, first verify that the
  issue has been resolved in the exact engine version you are targeting.

- **Only a single viewer is supported.**

  `SetViewer()` accepts a single world-space position, typically the active
  camera.

  Supporting split-screen rendering or multiple simultaneous points of interest
  would require extending `StreamingWorld`, which is currently outside the scope
  of this implementation.

- **`StreamingWorld.Update()` is designed to run at a reduced frequency.**

  By default, updates occur every `_tickIntervalSeconds` (0.1 seconds) rather
  than every `_Process()` frame.

  Fast-moving cameras (for example, sprinting in an FPS) may require a shorter
  update interval.

- **TextureBaker currently targets Windows only.**

  This limitation applies **only** to the packaging tool.

  The runtime and the BC1/BC3 texture format work identically on both Windows
  and Linux.

  The Windows restriction exists solely because the current implementation uses
  WinForms.

  If Linux-based texture baking becomes necessary (for example, on a Linux CI
  runner), a console-only version can be implemented by reusing
  `TextureBakerCore` and `DdsMipReader` without `UseWindowsForms`.

- **A mesh baker has not been implemented yet.**

  Section 4 documents the expected chunk format and describes the minimal
  single-LOD workflow.

  Generating complete LOD chains (for example, using meshoptimizer) remains a
  separate task.

- **Known race condition (safe, but not ideal).**

  `ChunkStorage._loaded` is written from the background loading thread
  (`LoadChunkAsync()`) while simultaneously being read and cleaned up from the
  main thread (`ReleaseStaleChunks()` / `Update()`) without synchronization.

  In the worst case, this can trigger an unnecessary second disk read for a
  chunk that has just finished loading because `_loaded[i]` and
  `_lastAccess[i]` are updated independently.

  This **cannot corrupt data**.

  The only observable effect is a harmless redundant disk read.

  Locking was intentionally avoided because `GetLoadedChunk()` is a hot path,
  and the cost of synchronization outweighs the extremely small race window.
