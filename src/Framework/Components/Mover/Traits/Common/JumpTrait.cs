// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;

using Godot;

namespace Framework.Components.Mover.Traits.Common
{
    [GlobalClass]
    public partial class JumpTrait : MovementTraitResource
    {
        private float _jumpBuffer; // seconds remaining in jump buffer window
        private float _coyote;     // seconds remaining in coyote time window

        public override void PreProcess( ref MovementContext ctx )
        {
            // Fill the buffer window each time the button is pressed
            if ( ctx.JumpRequested )
                _jumpBuffer = ctx.Profile.JumpBufferTime;

            // Refresh coyote time while grounded; drain it while airborne
            if ( ctx.IsOnFloor )
                _coyote = ctx.Profile.CoyoteTime;
            else
                _coyote -= ctx.Delta;   // ctx.Delta is injected by MovementMotor before PreProcess
        }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            _jumpBuffer -= delta;

            // Fire the jump if both windows are still open
            if ( _jumpBuffer > 0f && _coyote > 0f )
            {
                // Compute launch speed from the current gravity so the
                // configured jump height stays constant even if gravity changes.
                float gravity = MathF.Max( 0.0001f, -ctx.Gravity.Y );
                float jumpSpeed = MathF.Sqrt( 2f * gravity * ctx.Profile.JumpHeight );
                velocity.Y = jumpSpeed;

                _jumpBuffer = 0f;
                _coyote = 0f;

                ctx.JumpConsumed = true;
            }
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
