// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.FastMath.Godot;
using Framework.Logger;
using Godot;

namespace Framework.Common.Draw
{
	public struct DebugShapeName
	{
		public const string Sphere = "DebugWireSphere";
		public const string Cube = "DebugWireCube";
		public const string SphereTemp = "DebugWireSphereTemp";
		public const string CubeTemp = "DebugWireCubeTemp";
	}

	#region Handle

	/// <summary>
	/// Opaque reference to a live debug shape.
	/// Returned by every Draw* method; use it to update color or start
	/// animations without holding a direct node reference.
	///
	/// Always check <see cref="IsValid"/> before calling any method -
	/// temporary shapes auto-release and invalidate the handle.
	/// </summary>
	public sealed class DebugHandle
	{
		internal MeshInstance3D Instance;  // null = released / invalid

		internal DebugHandle( MeshInstance3D instance ) => Instance = instance;

		/// <summary>True as long as the underlying node hasn't been released.</summary>
		public bool IsValid =>
		  Instance != null && GodotObject.IsInstanceValid( Instance );

		/// <summary>Invalidates the handle without destroying the node (called by Release).</summary>
		internal void Invalidate() => Instance = null;
	}

	#endregion

	#region Main class

	/// <summary>
	/// Runtime wire-shape debug drawing for Godot 4 C#.
	///
	/// Each shape is rendered as an ImmediateMesh (Lines primitive) on a
	/// MeshInstance3D node. All Draw methods return a <see cref="DebugHandle"/>
	/// that lets you change color or run blink / flash animations later.
	///
	/// Color change strategies
	/// ───────────────────────
	///   UpdateColor(handle, color)
	///     Instant one-shot color swap. Rebuilds only the vertex colors on the
	///     ImmediateMesh, not the geometry. Call it freely on any event.
	///
	///   Blink(handle, colorA, colorB, interval)
	///     Continuous alternating blink between two colors at a fixed interval.
	///     Stops automatically when the handle is released.
	///     Cancel with StopAnimation(handle).
	///
	///   Flash(handle, flashColor, duration, returnColor)
	///     One-shot color pop: switches to flashColor for `duration` seconds,
	///     then returns to returnColor. Use for hit-flash / alert effects.
	///
	/// IMPORTANT - call Initialize(sceneTree, rootNode) once at startup.
	/// </summary>
	public static class DrawDebugRuntime
	{
		#region Constants

		private const int DefaultSphereSegments = 24;
		private const int MaxPoolSize = 100;

		#endregion

		#region State

		private static readonly Queue<MeshInstance3D> _pool = new Queue<MeshInstance3D>();
		private static readonly Dictionary<MeshInstance3D, ActiveAnim> _anims = new Dictionary<MeshInstance3D, ActiveAnim>();

		// Stores the last-built vertex positions so UpdateColor can rebuild
		// only colors without recomputing geometry.
		private static readonly Dictionary<MeshInstance3D, Vector3[]> _positions =
		  new Dictionary<MeshInstance3D, Vector3[]>();

		private static StandardMaterial3D _lineMaterial;

		private static Node3D _cubeParent;
		private static Node3D _cubeTempParent;
		private static Node3D _sphereParent;
		private static Node3D _sphereTempParent;

		private static SceneTree _sceneTree;
		private static Node3D _root;
		private static bool _initialized;

		#endregion

		#region Animation state

		private enum AnimKind { Blink, Flash }

		private sealed class ActiveAnim
		{
			public AnimKind Kind;
			public Color ColorA;
			public Color ColorB;       // returnColor for Flash
			public float Interval;     // seconds per half-cycle (Blink) or full duration (Flash)
			public bool Phase;        // current blink phase
			public bool Cancelled;
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Must be called once before any draw method.
		/// Pass the active SceneTree and a persistent root Node3D that lives
		/// across scene changes (e.g. your autoload root).
		/// </summary>
		public static void Initialize( SceneTree sceneTree, Node3D root )
		{
			_sceneTree = sceneTree ?? throw new ArgumentNullException( nameof( sceneTree ) );
			_root = root ?? throw new ArgumentNullException( nameof( root ) );
			_initialized = true;
		}

		#endregion

		#region Draw API

		public static DebugHandle DrawWireCube( Vector3 center, Vector3 size, Color color )
		{
			if ( !AssertInitialized() )
				return null;

			EnsureParent( DebugShapeName.Cube, ref _cubeParent );

			MeshInstance3D mesh = GetPooledMesh( DebugShapeName.Cube );
			_cubeParent.AddChild( mesh );
			BuildCube( mesh, center, size, color );
			return new DebugHandle( mesh );
		}

		public static DebugHandle DrawWireSphere(
		  Vector3 center,
		  float radius,
		  Color color,
		  int segments = DefaultSphereSegments )
		{
			if ( !AssertInitialized() )
				return null;

			EnsureParent( DebugShapeName.Sphere, ref _sphereParent );

			MeshInstance3D mesh = GetPooledMesh( DebugShapeName.Sphere );
			_sphereParent.AddChild( mesh );
			BuildSphere( mesh, center, radius, segments, color );
			return new DebugHandle( mesh );
		}

		public static DebugHandle DrawTempWireCube(
		  Vector3 center,
		  Vector3 size,
		  Color color,
		  float duration = 1f )
		{
			if ( !AssertInitialized() )
				return null;

			EnsureParent( DebugShapeName.CubeTemp, ref _cubeTempParent );

			MeshInstance3D mesh = GetPooledMesh( DebugShapeName.CubeTemp );
			_cubeTempParent.AddChild( mesh );
			BuildCube( mesh, center, size, color );

			DebugHandle handle = new DebugHandle( mesh );
			ScheduleRelease( handle, duration );
			return handle;
		}

		public static DebugHandle DrawTempWireSphere(
		  Vector3 center,
		  float radius,
		  Color color,
		  int segments = DefaultSphereSegments,
		  float duration = 1f )
		{
			if ( !AssertInitialized() )
				return null;

			EnsureParent( DebugShapeName.SphereTemp, ref _sphereTempParent );

			MeshInstance3D mesh = GetPooledMesh( DebugShapeName.SphereTemp );
			_sphereTempParent.AddChild( mesh );
			BuildSphere( mesh, center, radius, segments, color );

			DebugHandle handle = new DebugHandle( mesh );
			ScheduleRelease( handle, duration );
			return handle;
		}

		#endregion

		#region Color API

		/// <summary>
		/// Instantly sets the shape's color.
		/// Safe to call at any time; does nothing if the handle is no longer valid.
		/// </summary>
		public static void UpdateColor( DebugHandle handle, Color color )
		{
			if ( !IsHandleValid( handle ) )
				return;

			StopAnimInternal( handle.Instance );
			ApplyColor( handle.Instance, color );
		}

		/// <summary>
		/// Continuously alternates between <paramref name="colorA"/> and
		/// <paramref name="colorB"/> every <paramref name="interval"/> seconds.
		/// Replaces any existing animation on this handle.
		/// Cancel with <see cref="StopAnimation"/>.
		/// </summary>
		public static void Blink(
		  DebugHandle handle,
		  Color colorA,
		  Color colorB,
		  float interval = 0.5f )
		{
			if ( !IsHandleValid( handle ) )
				return;

			StopAnimInternal( handle.Instance );

			ActiveAnim anim = new ActiveAnim
			{
				Kind = AnimKind.Blink,
				ColorA = colorA,
				ColorB = colorB,
				Interval = interval,
				Phase = false,
				Cancelled = false,
			};

			_anims[ handle.Instance ] = anim;
			ScheduleBlinkStep( handle, anim );
		}

		/// <summary>
		/// Switches to <paramref name="flashColor"/> for <paramref name="duration"/>
		/// seconds, then returns to <paramref name="returnColor"/>.
		/// Replaces any existing animation on this handle.
		/// </summary>
		public static void Flash(
		  DebugHandle handle,
		  Color flashColor,
		  float duration = 0.15f,
		  Color? returnColor = null )
		{
			if ( !IsHandleValid( handle ) )
				return;

			StopAnimInternal( handle.Instance );

			Color restoreColor = returnColor ?? GetCurrentColor( handle.Instance );

			ActiveAnim anim = new ActiveAnim
			{
				Kind = AnimKind.Flash,
				ColorA = flashColor,
				ColorB = restoreColor,
				Interval = duration,
				Cancelled = false,
			};

			_anims[ handle.Instance ] = anim;

			ApplyColor( handle.Instance, flashColor );

			SceneTreeTimer timer = _sceneTree.CreateTimer( duration, false );
			timer.Timeout += () =>
			{
				if ( anim.Cancelled || !IsHandleValid( handle ) )
					return;

				_anims.Remove( handle.Instance );
				ApplyColor( handle.Instance, restoreColor );
			};
		}

		/// <summary>
		/// Cancels any running Blink or Flash on the given handle and leaves
		/// the shape at its current color.
		/// </summary>
		public static void StopAnimation( DebugHandle handle )
		{
			if ( !IsHandleValid( handle ) )
				return;

			StopAnimInternal( handle.Instance );
		}

		#endregion

		#region Destroy / Clear

		/// <summary>
		/// Destroys all permanent (non-temp) shapes with the given base name.
		/// </summary>
		public static void DestroyByName( string name )
		{
			if ( !AssertInitialized() )
				return;

			if ( name == DebugShapeName.SphereTemp || name == DebugShapeName.CubeTemp )
				throw new InvalidOperationException( "Destruction of temporary shapes is not allowed." );

			Node3D parent = name == DebugShapeName.Sphere ? _sphereParent : _cubeParent;
			if ( parent == null )
				return;

			foreach ( Node child in parent.GetChildren() )
			{
				if ( child is MeshInstance3D m )
					m.QueueFree();
			}

			parent.QueueFree();

			if ( name == DebugShapeName.Sphere ) _sphereParent = null;
			else _cubeParent = null;
		}

		/// <summary>
		/// Clears the pool and destroys all debug nodes.
		/// </summary>
		public static void Clear()
		{
			if ( !AssertInitialized() )
				return;

			_anims.Clear();
			_positions.Clear();

			while ( _pool.Count > 0 )
			{
				MeshInstance3D m = _pool.Dequeue();
				if ( GodotObject.IsInstanceValid( m ) )
					m.QueueFree();
			}

			DestroyParent( ref _sphereParent );
			DestroyParent( ref _cubeTempParent );
			DestroyParent( ref _cubeParent );
			DestroyParent( ref _sphereTempParent );
		}

		#endregion

		#region Shape Building

		private static void BuildCube( MeshInstance3D instance, Vector3 center, Vector3 size, Color color )
		{
			Vector3 h = size * 0.5f;

			Vector3[] v =
			{
		center + new Vector3(-h.X, -h.Y, -h.Z),
		center + new Vector3( h.X, -h.Y, -h.Z),
		center + new Vector3( h.X, -h.Y,  h.Z),
		center + new Vector3(-h.X, -h.Y,  h.Z),
		center + new Vector3(-h.X,  h.Y, -h.Z),
		center + new Vector3( h.X,  h.Y, -h.Z),
		center + new Vector3( h.X,  h.Y,  h.Z),
		center + new Vector3(-h.X,  h.Y,  h.Z),
	  };

			Vector3[] positions =
			{
		v[0], v[1],  v[1], v[2],  v[2], v[3],  v[3], v[0],
		v[4], v[5],  v[5], v[6],  v[6], v[7],  v[7], v[4],
		v[0], v[4],  v[1], v[5],  v[2], v[6],  v[3], v[7],
	  };

			_positions[ instance ] = positions;
			instance.Mesh = BuildLineMesh( positions, color );
		}

		private static void BuildSphere(
		  MeshInstance3D instance,
		  Vector3 center,
		  float radius,
		  int segments,
		  Color color )
		{
			List<Vector3> list = new List<Vector3>();

			int latSegs = FMath.Max( 2, segments / 2 );
			int lonSegs = FMath.Max( 3, segments );

			for ( int lat = 1; lat < latSegs; lat++ )
			{
				float a = MathF.PI * lat / latSegs;
				float y = MathF.Cos( a );
				float r = MathF.Sin( a );

				AddCirclePoints(
				  center + Vector3.Up * y * radius,
				  Vector3.Right, Vector3.Back,
				  r * radius, lonSegs, list );
			}

			for ( int lon = 0; lon < lonSegs; lon++ )
			{
				float a = MathF.PI * 2f * lon / lonSegs;
				Vector3 axA = new Vector3( MathF.Cos( a ), 0f, MathF.Sin( a ) );
				AddCirclePoints( center, axA, Vector3.Up, radius, latSegs * 2, list );
			}

			Vector3[] positions = list.ToArray();
			_positions[ instance ] = positions;
			instance.Mesh = BuildLineMesh( positions, color );
		}

		private static void AddCirclePoints(
		  Vector3 center,
		  Vector3 axisA,
		  Vector3 axisB,
		  float radius,
		  int segments,
		  List<Vector3> positions )
		{
			float step = MathF.PI * 2f / segments;

			// Lines primitive expects vertex pairs: each edge = two vertices.
			for ( int i = 0; i < segments; i++ )
			{
				float a0 = step * i;
				float a1 = step * ( i + 1 );
				positions.Add( center + ( axisA * MathF.Cos( a0 ) + axisB * MathF.Sin( a0 ) ) * radius );
				positions.Add( center + ( axisA * MathF.Cos( a1 ) + axisB * MathF.Sin( a1 ) ) * radius );
			}
		}

		#endregion

		#region Mesh / Color Helpers

		private static ImmediateMesh BuildLineMesh( Vector3[] positions, Color color )
		{
			EnsureMaterial();
			var mat = (StandardMaterial3D)_lineMaterial.Duplicate();

			ImmediateMesh mesh = new ImmediateMesh();
			mesh.SurfaceBegin( Mesh.PrimitiveType.Lines, mat );

			foreach ( Vector3 p in positions )
			{
				mesh.SurfaceSetColor( color );  // ← Добавить!
				mesh.SurfaceAddVertex( p );
			}

			mesh.SurfaceEnd();
			return mesh;
		}

		/// <summary>
		/// Rebuilds only the vertex colors of an existing ImmediateMesh.
		/// Geometry is read from the cached _positions table - no recomputation.
		/// </summary>
		private static void ApplyColor( MeshInstance3D instance, Color color )
		{
			if ( !_positions.TryGetValue( instance, out Vector3[] positions ) ) return;

			instance.Mesh = BuildLineMesh( positions, color );
		}

		/// <summary>
		/// Reads the color of the first vertex of the current mesh.
		/// Falls back to white if the mesh isn't an ImmediateMesh or has no surfaces.
		/// </summary>
		private static Color GetCurrentColor( MeshInstance3D instance )
		{
			// The color lives in the ImmediateMesh surface material set via SurfaceBegin,
			// not in SurfaceOverrideMaterial (which is empty). Read it from the mesh.
			if ( instance.Mesh is ImmediateMesh im && im.GetSurfaceCount() > 0 )
			{
				if ( im.SurfaceGetMaterial( 0 ) is StandardMaterial3D mat )
					return mat.AlbedoColor;
			}

			return Colors.White;
		}

		#endregion

		#region Animation Internals

		private static void ScheduleBlinkStep( DebugHandle handle, ActiveAnim anim )
		{
			SceneTreeTimer timer = _sceneTree.CreateTimer( anim.Interval, false );
			timer.Timeout += () =>
			{
				if ( anim.Cancelled ) return;
				if ( !IsHandleValid( handle ) ) return;

				anim.Phase = !anim.Phase;
				ApplyColor( handle.Instance, anim.Phase ? anim.ColorB : anim.ColorA );

				// Schedule next step
				ScheduleBlinkStep( handle, anim );
			};
		}

		private static void StopAnimInternal( MeshInstance3D instance )
		{
			if ( !_anims.TryGetValue( instance, out ActiveAnim anim ) ) return;

			anim.Cancelled = true;
			_anims.Remove( instance );
		}

		#endregion

		#region Pool Management

		private static MeshInstance3D GetPooledMesh( string name )
		{
			MeshInstance3D instance = null;

			while ( _pool.Count > 0 )
			{
				MeshInstance3D candidate = _pool.Dequeue();
				if ( GodotObject.IsInstanceValid( candidate ) )
				{
					instance = candidate;
					break;
				}
			}

			if ( instance == null )
			{
				instance = new MeshInstance3D();
				instance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
			}

			instance.Name = name;
			instance.Visible = true;
			return instance;
		}

		private static void Release( DebugHandle handle )
		{
			if ( !IsHandleValid( handle ) ) return;

			MeshInstance3D instance = handle.Instance;

			StopAnimInternal( instance );
			_positions.Remove( instance );

			instance.Visible = false;
			instance.GetParent()?.RemoveChild( instance );

			if ( _pool.Count < MaxPoolSize )
				_pool.Enqueue( instance );
			else
				instance.QueueFree();

			handle.Invalidate();
		}

		private static void ScheduleRelease( DebugHandle handle, float duration )
		{
			SceneTreeTimer timer = _sceneTree.CreateTimer( duration, false );
			timer.Timeout += () => Release( handle );
		}

		#endregion

		#region Parent Management

		private static void EnsureParent( string name, ref Node3D field )
		{
			if ( field != null && GodotObject.IsInstanceValid( field ) ) return;

			field = new Node3D();
			field.Name = name + "_Parent";

			// Use CallDeferred so this is safe even when called from _Ready
			// (Godot blocks AddChild during child setup). The mesh is added
			// to the parent node directly — that is safe before the parent
			// enters the tree — and will appear as soon as the parent does.
			if ( _root.IsInsideTree() )
				_root.CallDeferred( Node.MethodName.AddChild, field );
			else
				_root.AddChild( field );
		}

		private static void DestroyParent( ref Node3D field )
		{
			if ( field != null && GodotObject.IsInstanceValid( field ) )
				field.QueueFree();
			field = null;
		}

		#endregion

		#region Material

		private static void EnsureMaterial()
		{
			if ( _lineMaterial != null ) return;

			_lineMaterial = new StandardMaterial3D
			{
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				VertexColorUseAsAlbedo = true,
				Transparency = BaseMaterial3D.TransparencyEnum.Disabled,
				CullMode = BaseMaterial3D.CullModeEnum.Disabled,
				NoDepthTest = false,
			};
		}

		#endregion

		#region Guards

		private static bool IsHandleValid( DebugHandle handle ) =>
		  handle != null && handle.IsValid;

		private static bool AssertInitialized()
		{
			if ( _initialized ) return true;

			GameLogger.LogError(
			  $"{nameof( DrawDebugRuntime )}.{nameof( Initialize )} must be called before using draw methods." );

			return false;
		}

		#endregion
	}

	#endregion
}