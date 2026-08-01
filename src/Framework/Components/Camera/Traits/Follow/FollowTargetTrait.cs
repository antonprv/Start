// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Godot;

namespace Framework.Components.Camera.Traits.Follow
{
	/// <summary>
	/// Sets the rig's orbit pivot to the followed target's position plus a vertical eye-height
	/// offset. Used by every mode - what differs between an FPS camera and a Diablo camera is
	/// mostly EyeHeight (eye level vs. ground level) plus whichever rotation/distance traits
	/// come after this one.
	/// </summary>
	public class FollowTargetTrait : ICameraTrait
	{
		#region Properties

		/// <summary>Vertical offset added on top of the target's position.</summary>
		public float EyeHeight { get; set; } = 1.6f;

		/// <summary>
		/// Turn following off without removing the trait from the list - useful for toggling
		/// between a free/unleashed camera and a follow camera at runtime.
		/// </summary>
		public bool Enabled { get; set; } = true;

		#endregion

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			if ( !Enabled )
				return;

			state.PivotPosition = ctx.TargetPosition + ( Vector3.Up * EyeHeight );
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
