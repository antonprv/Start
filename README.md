### License TLDR: 
This code is pushed as a **Non-Commercial with attribution** license, with a caveat, that the owner of the intellectual property **can** use it commercially.

This is the source code for my general-purpose retro game framework. It features:
- dev console with cheat-codes, 
- custom C# DI Solution, 
- third person character controller
- third person camera controller
- modular Unreal Engine-like Mover Component

### Dev Console:
All you need to do is 
- Declare new ConsoleCommand, 
- Implement *IConsoleCommand* interface
- Create this command in the *DevConsoleNode.cs*

```csharp

	private void RegisterDefaultCommands()
	{
		Service.RegisterCommand( new FilterCommand( Service ) );
		Service.RegisterCommand( new SetFpsCommand( Service ) );
		Service.RegisterCommand( new StatFpsCommand( Service, _fpsTrackerNode ) );
		Service.RegisterCommand( new ShowDebugUIMessages( Service, _uiMessageNode ) );
		Service.RegisterCommand( new ExportLogsCommand( Service ) );

		Service.RegisterCommand( new NoclipCommand( Service, _moverComponent ) );
	}

```

### Mover Component
Define your character movement with traits, combine existing traits or mix in your own to create unique movement feel in your game.
This addon comes with following traits pre-configured:
(General)
- GravityTrait
- JumpTrait

(Hybrid of arcade quake-like and realistic)
- HybridAirControl
- HybridGroundTrait

(Quake)
- GroundAcceleration
- NoFriction
- QuakeAirStrafe (quake bunny hop)

(Realistic)
- ClampedAirControl
- GroundFriction
- RealisticGroundAcceleration
- SmoothStop

It also features a coyote jump timer, allowing player to be not so precsice with platforming.

These traits are already combined in the respective Presets: **HybridPreset**, **QuakePreset** and **RealisticPreset**. You can easily create your own though, these may either be a ready solution or a starting point.
Existing MoverComponent serves as an example of the Mover usage.

### TrenchBroom
This framework uses TrenchBroom as an external map editor.

### Installation
All external addons are in release section, you need to unpack them into src/Setup/addons folder

### Deploy
To deploy game, run deploy.bat from devops folder.
First command line argument is the game name, second is release/debug version, last argument is custom build path.
To build the project, use `Setup release` or `Setup debug`.

#### TODO:
- [ ] Full TrenchBroom integration, with custom entities
- [ ] Automate vertex color reapply on map builds by func_godot
- [ ] Add player start entity to place player there
- [ ] Integrate some level streaming solution
