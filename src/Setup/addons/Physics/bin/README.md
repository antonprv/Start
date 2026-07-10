# bin/

Drop the three DLLs built from `src/Framework/Physics.Core` here:

- Framework.Physics.Core.dll
- BepuPhysics.dll
- BepuUtilities.dll

See `src/Framework/Physics.Core/Build.md`. Nothing in this folder is source - Godot's build
never compiles anything here, it only links these three assemblies via `<Reference>` entries
in Setup.csproj (see the root README for the exact snippet).

This folder (and its .dll contents) should be committed to source control like any other binary
dependency, since not everyone on the team will want to build BepuPhysics2 from source just to
open the project.
