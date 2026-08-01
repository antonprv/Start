### License TLDR:
This code is published under a license very similar to the **Non-Commercial with Attribution** one, but with a caveat: the owner of the intellectual property, me, **can** use it commercially.

### About the project
This is the source code for my general-purpose retro game framework. It features:
- dev console with cheat codes,
- custom C# DI solution,
- third-person character controller,
- third-person camera controller,
- modular Unreal Engine-like Mover Component,
- Bepu Physics 2 integration, sped up in the hot paths by a custom bit-hack math library (FastMath),
- streaming system for textures and meshes, with its own baking tool.

Detailed docs, split out so this file stays a quick overview:
- [`docs/FRAMEWORK.md`](docs/FRAMEWORK.md) - Dev Console, Mover Component, FastMath, Physics.
- [`docs/STREAMING.md`](docs/STREAMING.md) - texture/mesh streaming, the asset manifest, TextureBaker.
- [`docs/CAMERA.md`](docs/CAMERA.md) - All about the Camera Component.

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
- [ ] Mesh baker (LOD generation via an external simplifier) for the streaming system - textures are covered, meshes still need a single-LOD passthrough at best
