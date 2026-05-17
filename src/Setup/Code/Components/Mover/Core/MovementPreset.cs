// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Code.Components.Mover
{
	/// <summary>
	/// Editor-configurable movement preset.
	/// Fill the Traits array with MovementTraitResource instances,
	/// then assign this resource to MoverComponent.
	///
	/// Equivalent of a UE DataAsset that drives the Mover component.
	///
	/// Can also be created in code via the static preset factories:
	///   QuakePreset.Build(), RealisticPreset.Build(), HybridPreset.Build().
	/// </summary>
	[GlobalClass]
	public partial class MovementPreset : Resource
	{
		[Export] public Array<MovementTraitResource> Traits { get; set; } = new();

		/// <summary>Returns a mutable list of trait interfaces ready for MovementMotor.</summary>
		public List<IMovementTrait> BuildTraits() =>
			Traits.Cast<IMovementTrait>().ToList();
	}
}
