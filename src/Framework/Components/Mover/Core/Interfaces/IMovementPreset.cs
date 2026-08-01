// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.
//
// Wires up the Phase 1 (ground/air walk) port of idPhysics_Player.
// Order mirrors idPhysics_Player::MovePlayer() dispatch for PM_NORMAL:
// CheckJump before Friction/Accelerate inside WalkMove, gravity integration
// inside SlideMove. Our pipeline runs PreProcess -> Process(in list order)
// -> PostProcess, so trait order below reproduces call order:
//   CheckJump (may consume the jump) -> Friction -> Accelerate -> Gravity.
//
// NOT YET PORTED (Phase 2+, see project roadmap):
//   WaterMove / WaterJumpMove / LadderMove / CheckDuck / CheckWaterJump,
//   slick-surface + knockback accel swap, step-up/step-down sliding,
//   idPhysics_RigidBody, idPhysics_AF (ragdolls), idPhysics_Monster,
//   idPhysics_Parametric.

using Framework.Components.Mover.Core.Interfaces;

namespace Framework.Components.Mover.Presets
{
	public interface IMovementPreset
	{
		List<IMovementTrait> Build();
		void SetDefaultProfile( IMovementProfile profile );
	}
}