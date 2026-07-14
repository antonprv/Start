// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Framework.Logger;
using Godot;

namespace Physics
{
	/// <summary>
	/// A lightweight trigger volume that never touches the physics engine - no static/kinematic
	/// body, no narrow-phase overlap generation on <see cref="Framework.Physics.PhysicsWorld"/>'s
	/// <c>Step</c> every tick. Use this instead of <see cref="BepuTriggerArea3D"/> when you just
	/// need Enter/Exit for a single tracked <see cref="Target"/> (typically the player) and don't
	/// need the volume to physically interact with anything.
	///
	/// Each physics tick runs a two-stage check against <see cref="ShapeSource"/>:
	///
	/// 1. Broad check - is <see cref="Target"/>'s current world position inside the shape? Pure
	///    point-in-shape math, no allocations, dirt cheap. This runs every tick and is all that
	///    happens while the target stays fully inside or fully outside.
	/// 2. Confirm sweep - only on a tick where the broad check flips does it do the one extra bit
	///    of work: a swept test of the segment from the target's last sampled position to its
	///    current one against the exact shape. This confirms the crossing actually happened rather
	///    than trusting a single point sample, and catches a fast target tunneling all the way
	///    through a thin volume within one tick. Enter/Exit only fire once this confirms.
	///
	/// Supports Box/Sphere/Capsule shapes on <see cref="ShapeSource"/>. The target is treated as a
	/// point - if you need to account for its own collision radius, inflate the shape accordingly.
	/// </summary>
	[GlobalClass]
	public partial class BepuTriggerAreaSimple : Node3D
	{
		[ExportGroup( "Source" )]
		[Export] public CollisionShape3D ShapeSource { get; set; } = null!;
		[Export] public Node3D Target { get; set; } = null!;

		[Signal] public delegate void BodyEnteredEventHandler( Node3D body );
		[Signal] public delegate void BodyExitedEventHandler( Node3D body );

		public bool IsInside { get; private set; }

		private Vector3 _lastSampledPosition;
		private bool _hasSample;

		public override void _PhysicsProcess( double delta )
		{
			if ( ShapeSource?.Shape == null || Target == null )
				return;

			// One GlobalTransform/GlobalPosition read each per tick - both are interop calls into
			// Godot, so grab them once and reuse the cached inverse for every test below instead of
			// re-fetching per check.
			Transform3D inverse = ShapeSource.GlobalTransform.AffineInverse();
			Vector3 currentPosition = Target.GlobalPosition;
			Vector3 currentLocal = inverse * currentPosition;

			// First tick after entering the tree (or after Target/ShapeSource got assigned) - just
			// take the initial sample, no previous position exists yet to sweep from.
			if ( !_hasSample )
			{
				_hasSample = true;
				_lastSampledPosition = currentPosition;
				IsInside = Contains( currentLocal, ShapeSource.Shape );
				return;
			}

			bool coarseInside = Contains( currentLocal, ShapeSource.Shape );

			if ( !IsInside && coarseInside )
			{
				if ( Intersects( inverse * _lastSampledPosition, currentLocal, ShapeSource.Shape ) )
				{
					IsInside = true;
					OnBodyEnter();
				}
			}
			else if ( IsInside && !coarseInside )
			{
				if ( !Intersects( inverse * _lastSampledPosition, currentLocal, ShapeSource.Shape ) )
				{
					IsInside = false;
					OnBodyExit();
				}
			}

			_lastSampledPosition = currentPosition;
		}

		protected virtual void OnBodyEnter()
		{
			EmitSignal( SignalName.BodyEntered, Target );
		}

		protected virtual void OnBodyExit()
		{
			EmitSignal( SignalName.BodyExited, Target );
		}

		private bool Contains( Vector3 local, Shape3D shape )
		{
			switch ( shape )
			{
			case BoxShape3D box:
				Vector3 half = box.Size * 0.5f;
				return FMath.Abs( local.X ) <= half.X &&
					FMath.Abs( local.Y ) <= half.Y &&
					FMath.Abs( local.Z ) <= half.Z;

			case SphereShape3D sphere:
				return local.LengthSquared() <= sphere.Radius * sphere.Radius;

			case CapsuleShape3D capsule:
				{
					float halfSegment = FMath.Max( 0f, capsule.Height * 0.5f - capsule.Radius );
					Vector3 closest = new Vector3( 0f, FMath.Clamp( local.Y, -halfSegment, halfSegment ), 0f );
					return ( local - closest ).LengthSq() <= capsule.Radius * capsule.Radius;
				}

			default:
				GameLogger.LogError( $"{Name}: TriggerArea3D doesn't support shape " +
					$"'{shape?.GetType().Name}'. Use Box/Sphere/Capsule on ShapeSource." );
				return false;
			}
		}

		private bool Intersects( Vector3 from, Vector3 to, Shape3D shape )
		{
			switch ( shape )
			{
			case BoxShape3D box:
				return SegmentIntersectsBox( from, to, box.Size * 0.5f );

			case SphereShape3D sphere:
				return SegmentIntersectsSphere( from, to, sphere.Radius );

			case CapsuleShape3D capsule:
				float halfSegment = FMath.Max( 0f, capsule.Height * 0.5f - capsule.Radius );
				return SegmentIntersectsCapsule( from, to, capsule.Radius, halfSegment );

			default:
				return false; // already logged in Contains() on the same tick
			}
		}

		#region Geometry

		private static bool SegmentIntersectsBox( Vector3 from, Vector3 to, Vector3 halfExtents )
		{
			Vector3 direction = to - from;
			float tEnter = 0f;
			float tExit = 1f;

			if ( !ClipAxis( from.X, direction.X, halfExtents.X, ref tEnter, ref tExit ) ) return false;
			if ( !ClipAxis( from.Y, direction.Y, halfExtents.Y, ref tEnter, ref tExit ) ) return false;
			if ( !ClipAxis( from.Z, direction.Z, halfExtents.Z, ref tEnter, ref tExit ) ) return false;

			return true;
		}

		private static bool ClipAxis( float origin, float direction, float halfExtent, ref float tEnter, ref float tExit )
		{
			const float epsilon = 1e-8f;

			if ( FMath.Abs( direction ) < epsilon )
				return origin >= -halfExtent && origin <= halfExtent;

			float invDirection = 1f / direction;
			float t1 = ( -halfExtent - origin ) * invDirection;
			float t2 = ( halfExtent - origin ) * invDirection;

			if ( t1 > t2 )
				(t1, t2) = (t2, t1);

			tEnter = FMath.Max( tEnter, t1 );
			tExit = FMath.Min( tExit, t2 );

			return tEnter <= tExit;
		}

		private static bool SegmentIntersectsSphere( Vector3 from, Vector3 to, float radius )
		{
			Vector3 direction = to - from;
			float a = direction.FastLength();

			if ( a < 1e-12f )
				return from.LengthSq() <= radius * radius;

			float b = 2f * from.FastDot( direction );
			float c = from.FastLength() - radius * radius;
			float discriminant = b * b - 4f * a * c;

			if ( discriminant < 0f )
				return false;

			float sqrtDiscriminant = FMath.FastSqrt( discriminant );
			float t0 = ( -b - sqrtDiscriminant ) / ( 2f * a );
			float t1 = ( -b + sqrtDiscriminant ) / ( 2f * a );

			// segment [0,1] overlaps the root interval [t0,t1]
			return t1 >= 0f && t0 <= 1f;
		}

		private static bool SegmentIntersectsCapsule( Vector3 from, Vector3 to, float radius, float halfHeight )
		{
			Vector3 capsuleFrom = new Vector3( 0f, -halfHeight, 0f );
			Vector3 capsuleTo = new Vector3( 0f, halfHeight, 0f );

			float distanceSquared = SegmentSegmentDistanceSquared( from, to, capsuleFrom, capsuleTo );
			return distanceSquared <= radius * radius;
		}

		/// <summary>
		/// Closest distance squared between segment p1-q1 and segment p2-q2. Standard closest-point
		/// construction (Ericson, Real-Time Collision Detection, 5.1.9) - used here to test the
		/// target's movement segment against the capsule's central axis segment.
		/// </summary>
		private static float SegmentSegmentDistanceSquared( Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2 )
		{
			const float epsilon = 1e-8f;

			Vector3 d1 = q1 - p1;
			Vector3 d2 = q2 - p2;
			Vector3 r = p1 - p2;
			float a = d1.FastDot( d1 );
			float e = d2.FastDot( d2 );
			float f = d2.FastDot( r );

			float s, t;

			if ( a <= epsilon && e <= epsilon )
			{
				s = 0f;
				t = 0f;
			}
			else if ( a <= epsilon )
			{
				s = 0f;
				t = FMath.Clamp( f / e, 0f, 1f );
			}
			else
			{
				float c = d1.FastDot( r );

				if ( e <= epsilon )
				{
					t = 0f;
					s = FMath.Clamp( -c / a, 0f, 1f );
				}
				else
				{
					float b = d1.FastDot( d2 );
					float denom = a * e - b * b;

					s = denom > epsilon ? FMath.Clamp( ( b * f - c * e ) / denom, 0f, 1f ) : 0f;
					t = ( b * s + f ) / e;

					if ( t < 0f )
					{
						t = 0f;
						s = FMath.Clamp( -c / a, 0f, 1f );
					}
					else if ( t > 1f )
					{
						t = 1f;
						s = FMath.Clamp( ( b - c ) / a, 0f, 1f );
					}
				}
			}

			Vector3 closest1 = p1 + d1 * s;
			Vector3 closest2 = p2 + d2 * t;
			return ( closest1 - closest2 ).FastLength();
		}

		#endregion
	}
}
