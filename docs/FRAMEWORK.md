# Framework

Detailed reference for the framework's core systems. For streaming (textures/meshes), see
[`STREAMING.md`](STREAMING.md) - it's big enough to warrant its own file. Everything else is here.

## Dev Console
All you need to do is
- declare a new ConsoleCommand,
- implement the *IConsoleCommand* interface,
- create this command in *DevConsoleNode.cs*.
```csharp
	private void RegisterDefaultCommands()
	{
		Service.RegisterCommand( new FilterCommand( Service ) );
		Service.RegisterCommand( new SetFpsCommand( Service ) );
		Service.RegisterCommand( new ToggleVsyncCommand( Service ) );
		Service.RegisterCommand( new StatFpsCommand( Service, _fpsTrackerNode ) );
		Service.RegisterCommand( new ShowDebugUIMessages( Service, _uiMessageNode ) );
		Service.RegisterCommand( new ExportLogsCommand( Service ) );

		Service.RegisterCommand( new NoclipCommand( Service, _moverComponent ) );
	}
```
`help` and `clear` don't need registering here - the console service registers those two itself, every command you add above shows up in `help` automatically.

## Mover Component
Define your character movement with traits. Combine existing traits or mix in your own to create a unique movement feel in your game.
This addon comes with the following traits pre-configured:

(Common - used by nearly every preset)
- GravityTrait
- JumpTrait *(buffered + coyote by default)*

(Hybrid - arcade lerp-based control)
- HybridAirControlTrait
- HybridGroundTrait

(Quake)
- GroundAccelerationTrait
- NoFrictionTrait
- QuakeAirStrafeTrait *(bunny hop)*

(Realistic - Source-engine inspired)
- ClampedAirControlTrait
- GroundFrictionTrait
- RealisticGroundAccelTrait
- SmoothStopTrait

(Doom 3 - ported from idPhysics_Player, ground/air walk phase only)
- Doom3AccelerateTrait
- Doom3FrictionTrait
- Doom3JumpTrait *(unbuffered, no coyote - matches the original)*

(Custom - hand-picked blends, not a straight port of any one game)
- Doom3GroundAccelTrait
- StrafeAirControlTrait

It also features a coyote jump timer and jump buffering, allowing the player to be not so **precise** with platforming.

These traits are already combined in five ready presets: **HybridPreset**, **QuakePreset**, **RealisticPreset**, **Doom3Preset**, and **QuakeStrafeDoom2016Preset** (Quake-style strafe momentum + a Doom 3 acceleration curve + a Doom 2016-style generous air cap - a custom blend, not a port of any single game). You can easily create your own though; these may either be a ready solution or a starting point.
The existing MoverComponent serves as an example of Mover usage.

## FastMath
A small custom math library (`src/Framework/Math/`) used throughout the framework wherever a
hot per-frame path can trade a little precision for a lot of speed - most Mover traits and the
Physics integration layer (see below) call into it. It's the Quake III fast-inverse-square-root
family of tricks generalized: reinterpret an IEEE-754 float's bits as an integer, exploit that
this is approximately proportional to its log2, and do the expensive operation (sqrt, pow, sin/cos/atan2)
as cheap integer/bit arithmetic plus a tuned correction step instead of a library call. Every
function documents a measured error bound, and most have both a default **Fast** mode and an
optional **Precise** mode for when a hot path turns out to need the extra accuracy after all.

Split into three projects mirroring the same Core/engine-adapter pattern as Physics and Streaming:
- `FastMath.Core` - the scalar bit-hack primitives (`FastInvSqrt`, `FastPow`, `FastAtan`/`FastAtan2`, ...), no vector types at all.
- `FastMath.Godot` / `FastMath.Numerics` - `Vector2/3`, `Quaternion`, and Euler-angle overloads for Godot's own math types and `System.Numerics` respectively, each calling straight back into `FastMath.Core`'s scalar functions rather than reimplementing the bit-hacks twice.
- `FastMath.Tests` - NUnit tests checking the documented error bounds actually hold (square root, trigonometry, Euler conversion).

## Physics
Physics runs on **Bepu Physics 2** (a native .NET physics library) instead of Godot's built-in
engine. The vendored Bepu source itself is optimized for older hardware and large demanding simulations with FastMath. All glue code lives in two layers,
the same split used by every other systems-level integration in this framework:
- `src/Framework/Physics/Core/` (`Physics.Integration.csproj`) - a plain .NET project with no
  Godot dependency, wrapping Bepu's `Simulation` behind an engine-agnostic `PhysicsWorld`: shape
  creation (box/sphere/capsule/cylinder/convex hull/triangle mesh), dynamic/kinematic/static
  bodies, a character controller (`MoveCharacter`), and sweep queries (`SweepProjectile`,
  `SweepSphereCast`). Built standalone, outside the Godot project, and referenced by `Setup.csproj`
  only as binary `<Reference>`s (`Physics.Integration.dll`, `BepuPhysics.dll`, `BepuUtilities.dll`)
  - see `Build.md` in that folder for the build/copy step.
- `src/Setup/addons/Physics/` (deployed as the `framework_physics` addon) - the Godot-facing
  components that ground the simulation in the scene tree: `BepuStaticBody3D`, `BepuRigidBody3D`,
  `BepuCharacterBody3D`, `BepuTriggerArea3D`, `PhysicsWorldNode` (the autoload driving the
  simulation), and `GodotShapeConverter`.

The integration layer leans on **FastMath** (see above) in its hottest per-step paths rather than
`System.Numerics`/`MathF`: the character controller's move/sweep code normalizes vectors and
checks the max-floor-angle via `FastNormalized()`/`FastCos`, and per-body linear/angular damping
each physics step goes through `FastPow`.
