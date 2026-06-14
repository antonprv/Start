### License TLDR: 
This code is published under a license very similar to the **Non-Commercial with Attribution** one, but with a caveat: the owner of the intellectual property, me, **can** use it commercially.

### About the project
This is the source code for my general-purpose retro game framework. It features:
- dev console with cheat codes, 
- custom C# DI solution, 
- third-person character controller,
- third-person camera controller,
- modular Unreal Engine-like Mover Component.

### Dev Console:
All you need to do is 
- declare a new ConsoleCommand, 
- implement the *IConsoleCommand* interface,
- create this command in *DevConsoleNode.cs*.

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
Define your character movement with traits. Combine existing traits or mix in your own to create a unique movement feel in your game.
This addon comes with the following traits pre-configured:

(General)
- GravityTrait
- JumpTrait

(Hybrid of arcade Quake-like and realistic)
- HybridAirControl
- HybridGroundTrait

(Quake)
- GroundAcceleration
- NoFriction
- QuakeAirStrafe (Quake bunny hop)

(Realistic)
- ClampedAirControl
- GroundFriction
- RealisticGroundAcceleration
- SmoothStop

It also features a coyote jump timer, allowing the player to be not so **precise** with platforming.

These traits are already combined in the respective presets: **HybridPreset**, **QuakePreset**, and **RealisticPreset**. You can easily create your own though; these may either be a ready solution or a starting point.
The existing MoverComponent serves as an example of Mover usage.

### TrenchBroom
This framework uses TrenchBroom as an external map editor.

### Installation
All external addons are in the release section. You need to unpack them into the `src/Setup/addons` folder.

### Deploy
To deploy the game, run `deploy.bat` from the `devops` folder.
The first command-line argument is the game name, the second is the release/debug version, and the last argument is the custom build path.
To build the project, use `Setup release` or `Setup debug`.

#### TODO:
- [ ] Full TrenchBroom integration with custom entities
- [ ] Automate vertex color reapplication on map builds by func_godot
- [ ] Add player start entity to place the player there
- [ ] Integrate some level streaming solution