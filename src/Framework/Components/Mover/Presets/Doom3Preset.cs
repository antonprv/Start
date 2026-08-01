// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Traits.Common;
using Framework.Components.Mover.Traits.Doom3;

namespace Framework.Components.Mover.Presets
{
	internal class Doom3Preset : IMovementPreset
	{
		public List<IMovementTrait> Build() => new List<IMovementTrait>()
		{
			new Doom3JumpTrait(),
			new Doom3FrictionTrait(),
			new Doom3AccelerateTrait(),
			new GravityTrait()
		};

		/// <summary>
		/// MaxSpeed here should be set to your walkSpeed cvar equivalent
		/// (Doom 3 default pm_walkspeed = 76 inches/sec ≈ 1.93 m/s at 1:1 scale
		/// via Doom3Constants.InchesToMeters - feels slow because Doom 3's
		/// "run" is actually the default; pm_speed / sprint is a separate cvar
		/// not ported yet).
		/// </summary>
		public void SetDefaultProfile( IMovementProfile profile )
		{
			profile.MaxSpeed = 76f * Core.Doom3Constants.InchesToMeters;
		}
	}
}
