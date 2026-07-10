// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Physics;
using Godot;
using System;
using NumericsQuaternion = System.Numerics.Quaternion;
using NumericsVector3 = System.Numerics.Vector3;

namespace Physics
{
	/// <summary>A shape registered with Core, plus the local-space offset (if any) the pose needs to account for.</summary>
	public readonly struct BuiltShape
	{
		public readonly ShapeHandle Handle;

		/// <summary>
		/// Almost always <see cref="Vector3.Zero"/> - the one exception is a convex hull built
		/// from a point cloud, which Bepu internally re-centers on its own centroid. Pass this
		/// straight into <see cref="GodotShapeConverter.ToPhysicsTransform"/> and it's handled for you.
		/// </summary>
		public readonly Vector3 LocalOffset;

		public BuiltShape( ShapeHandle handle, Vector3 localOffset = default )
		{
			Handle = handle;
			LocalOffset = localOffset;
		}
	}

	/// <summary>
	/// Reads Godot's stock <see cref="Shape3D"/> resources - whether hand-placed in the editor
	/// (Box/Sphere/Capsule/Cylinder) or baked by a level generator like func_godot
	/// (ConvexPolygonShape3D per brush, or a single ConcavePolygonShape3D) - and registers the
	/// matching shape with a <see cref="Framework.Physics.PhysicsWorld"/>.
	///
	/// This class is the entire coupling point between Godot's shape resources and Core's shape
	/// API - everything else in the addon goes through here rather than talking to either side
	/// directly.
	/// </summary>
	public static class GodotShapeConverter
	{
		/// <summary>
		/// Builds a Core shape from any Godot <see cref="CollisionShape3D"/>'s Shape resource.
		/// Supports Box/Sphere/Capsule/Cylinder (hand-authored) and ConvexPolygonShape3D /
		/// ConcavePolygonShape3D (what func_godot bakes from map brushes). Pass
		/// <paramref name="mass"/> = 0 for kinematic/static bodies.
		/// </summary>
		public static BuiltShape FromCollisionShape3D( IPhysicsWorld world, CollisionShape3D source, float mass )
		{
			switch ( source.Shape )
			{
			case BoxShape3D box:
				return new BuiltShape( world.Core.AddBoxShape( ToNumerics( box.Size ) ) );

			case SphereShape3D sphere:
				return new BuiltShape( world.Core.AddSphereShape( sphere.Radius ) );

			case CapsuleShape3D capsule:
				{
					// Godot's Height is the total capsule height including both hemispherical caps;
					// Core's cylinderLength is just the straight segment between the cap centers.
					float cylinderLength = MathF.Max( 0.01f, capsule.Height - 2f * capsule.Radius );
					return new BuiltShape( world.Core.AddCapsuleShape( capsule.Radius, cylinderLength ) );
				}

			case CylinderShape3D cylinder:
				return new BuiltShape( world.Core.AddCylinderShape( cylinder.Radius, cylinder.Height ) );

			case ConvexPolygonShape3D convexPolygon:
				return FromConvexPolygonShape3D( world, convexPolygon, mass );

			case ConcavePolygonShape3D concavePolygon:
				return FromConcavePolygonShape3D( world, concavePolygon );

			default:
				throw new NotSupportedException( $"GodotShapeConverter: unsupported Godot shape '{source.Shape?.GetType().Name}'." );
			}
		}

		/// <summary>
		/// Builds a convex hull from a func_godot-generated (or hand-made) ConvexPolygonShape3D -
		/// what a single brush becomes when a solid class's Collision Shape Type is Convex.
		/// </summary>
		public static BuiltShape FromConvexPolygonShape3D( IPhysicsWorld world, ConvexPolygonShape3D shape, float mass )
		{
			Vector3[] godotPoints = shape.Points;
			NumericsVector3[] points = new NumericsVector3[ godotPoints.Length ];
			for ( int i = 0; i < godotPoints.Length; i++ )
				points[ i ] = ToNumerics( godotPoints[ i ] );

			ShapeHandle handle = world.Core.AddConvexHullShape( points, mass, out NumericsVector3 centroidOffset );
			return new BuiltShape( handle, ToGodot( centroidOffset ) );
		}

		/// <summary>
		/// Builds a static BVH-accelerated mesh from a func_godot-generated (or hand-made)
		/// ConcavePolygonShape3D - what an entire solid class becomes when its Collision Shape
		/// Type is Concave. Static only.
		/// </summary>
		public static BuiltShape FromConcavePolygonShape3D( IPhysicsWorld world, ConcavePolygonShape3D shape )
		{
			Vector3[] faces = shape.Data; // flat triangle soup, already in local space
			NumericsVector3[] points = new NumericsVector3[ faces.Length ];
			for ( int i = 0; i < faces.Length; i++ )
				points[ i ] = ToNumerics( faces[ i ] );

			ShapeHandle handle = world.Core.AddTriangleMeshShape( points, NumericsVector3.One );
			return new BuiltShape( handle );
		}

		/// <summary>Builds a static mesh directly from a MeshInstance3D's surface arrays (no baked ConcavePolygonShape3D resource needed).</summary>
		public static BuiltShape FromMeshInstance3D( IPhysicsWorld world, MeshInstance3D meshInstance, Vector3 scale )
		{
			Mesh mesh = meshInstance.Mesh ?? throw new ArgumentException( "MeshInstance3D has no Mesh resource.", nameof( meshInstance ) );

			int triangleCount = 0;
			for ( int surface = 0; surface < mesh.GetSurfaceCount(); surface++ )
				triangleCount += mesh.SurfaceGetArrays( surface )[ (int)Mesh.ArrayType.Index ].AsInt32Array().Length / 3;

			NumericsVector3[] points = new NumericsVector3[ triangleCount * 3 ];
			int writeIndex = 0;

			for ( int surface = 0; surface < mesh.GetSurfaceCount(); surface++ )
			{
				Godot.Collections.Array arrays = mesh.SurfaceGetArrays( surface );
				Vector3[] vertices = arrays[ (int)Mesh.ArrayType.Vertex ].AsVector3Array();
				int[] indices = arrays[ (int)Mesh.ArrayType.Index ].AsInt32Array();

				for ( int i = 0; i < indices.Length; i++ )
					points[ writeIndex++ ] = ToNumerics( vertices[ indices[ i ] ] );
			}

			ShapeHandle handle = world.Core.AddTriangleMeshShape( points, ToNumerics( scale ) );
			return new BuiltShape( handle );
		}

		/// <summary>
		/// Builds the correct pose for a shape, accounting for <see cref="BuiltShape.LocalOffset"/>
		/// (non-zero only for convex hulls, which Bepu re-centers on its own centroid).
		/// </summary>
		public static PhysicsTransform ToPhysicsTransform( Transform3D globalTransform, Vector3 localOffset = default )
		{
			NumericsQuaternion orientation = ToNumerics( globalTransform.Basis.GetRotationQuaternion() );
			NumericsVector3 rotatedOffset = NumericsVector3.Transform( ToNumerics( localOffset ), orientation );
			return new PhysicsTransform( ToNumerics( globalTransform.Origin ) + rotatedOffset, orientation );
		}

		public static NumericsVector3 ToNumerics( Vector3 v ) => new NumericsVector3( v.X, v.Y, v.Z );
		public static Vector3 ToGodot( NumericsVector3 v ) => new Vector3( v.X, v.Y, v.Z );
		public static NumericsQuaternion ToNumerics( Quaternion q ) => new NumericsQuaternion( q.X, q.Y, q.Z, q.W );
		public static Quaternion ToGodot( NumericsQuaternion q ) => new Quaternion( q.X, q.Y, q.Z, q.W );
	}
}
