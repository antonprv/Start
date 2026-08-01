// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Framework.Components.Camera.Core.Types
{
	public enum CameraPreset
	{
		/// <summary>Build the trait list yourself - see CameraComponent.GetCustomTraits().</summary>
		Custom,

		/// <summary>Classic FPS camera: eye-height pivot, free mouse look, zero arm length.</summary>
		FirstPerson,

		/// <summary>First person with CameraOvershootTrait added - a bit of extra sway/kick on fast turns.</summary>
		FirstPersonOvershoot,

		/// <summary>Modern over-the-shoulder third person (Gears/RE4-style), with lag + overshoot.</summary>
		ThirdPersonShoulder,

		/// <summary>
		/// Elden Ring-style third person: near-centered high/back framing, narrower pitch range,
		/// fixed (non-zoomable) distance, and auto-recenter behind the character after a beat of
		/// no camera input. No lag/overshoot - see GeneralThirdPerson for that combination.
		/// </summary>
		ThirdPersonSoulslike,

		/// <summary>
		/// ThirdPersonSoulslike plus CameraLagTrait + CameraOvershootTrait - the default
		/// third-person camera for the game right now.
		/// </summary>
		GeneralThirdPerson,

		/// <summary>Fixed, angled top-down camera that follows the character - Diablo-style.</summary>
		TopDownFixed,

		/// <summary>Rotatable/zoomable orbit camera with edge-pan - Baldur's Gate 3-style.</summary>
		TopDownOrbit,

		/// <summary>Fixed-angle camera that pans by pushing the cursor to the screen edge - RTS-style.</summary>
		EdgeScrollTopDown
	}
}
