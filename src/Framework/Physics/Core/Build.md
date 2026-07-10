# Building Physics.Core (Framework.Physics)

This project is deliberately **outside** `src/Setup` (the Godot project) and is **not**
referenced by `Setup.csproj` as a `<ProjectReference>` - only as three binary `<Reference>`s.
Build it on its own and drop the resulting DLLs where the Godot addon expects them:

```bash
cd src/Framework/Physics.Core
dotnet build -c Release
cp bin/Release/net8.0/Physics.dll        ../../Setup/addons/framework_physics/bin/
cp bin/Release/net8.0/BepuPhysics.dll    ../../Setup/addons/framework_physics/bin/
cp bin/Release/net8.0/BepuUtilities.dll  ../../Setup/addons/framework_physics/bin/
```

(Or point a small `publish`/`copy` script at this - whatever fits your existing build pipeline.
The point is just: these three DLLs need to exist in `addons/framework_physics/bin/` before
you open/build the Godot project.)

You only need to redo this when you actually change something in `Physics.csproj` or the
vendored Bepu source - which should be rare. Everyday work on gameplay code never touches this
project and never triggers a rebuild of it; Godot's own incremental build only ever links the
DLLs that are already sitting in `addons/framework_physics/bin/`.

## If you add or touch vendored files

`Physics.csproj` explicitly excludes `Vendor\**\*.cs` from its own compile items
(`<Compile Remove>`) - this is required, not cosmetic. Microsoft.NET.Sdk projects implicitly
glob every `.cs` file under the project directory into their own assembly; without that
exclusion, every file under `Vendor/BepuPhysics` and `Vendor/BepuUtilities` gets compiled
**twice** - once directly into `Physics.dll` via the implicit glob, and once into
`BepuPhysics.dll`/`BepuUtilities.dll` via the `<ProjectReference>` - producing duplicate type
definitions that surface as `CS0121 ("ambiguous call")` errors on ordinary extension methods
throughout the vendored source, with nothing pointing at the actual cause. If you ever restructure
this project, keep that exclusion (or the equivalent `<EnableDefaultCompileItems>false</EnableDefaultCompileItems>`
approach with explicit includes).
