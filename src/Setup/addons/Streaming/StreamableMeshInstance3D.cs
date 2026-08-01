// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Logger;
using Framework.Streaming;

using Godot;
using System;
using Zenjex;

using GArray = Godot.Collections.Array;
namespace Streaming
{
	/// <summary>
	/// A MeshInstance3D whose geometry LOD streams in as the camera gets closer, instead of the
	/// whole triangle soup being resident from the moment its scene loads. Chunk 0 (always
	/// resident once registered) should be the coarsest LOD your baker can produce, so there is
	/// never a frame with no mesh at all - only ever "a coarser one than we'd like".
	///
	/// Each chunk's bytes are one LOD level packed as:
	///   [int32 vertexCount][int32 indexCount]
	///   [vertexCount * (float px,py,pz, nx,ny,nz, u,v)]
	///   [indexCount * int32]
	/// A baker producing these from a glTF/FBX source would typically run the source mesh through
	/// an external simplifier (e.g. meshoptimizer) once per LOD level offline, then hand the
	/// resulting arrays to ChunkStorage.Pack - this class only has to know how to read them back.
	/// </summary>
	[GlobalClass]
	public partial class StreamableMeshInstance3D : MeshInstance3D
	{
		[ExportGroup( "Source" )]
		[Export] private string _assetName = string.Empty;

		private IStreamingWorld _streamingWorld = null!;
		private IAssetManifestService _manifest = null!;
		private GodotStreamableMesh? _resource;

		[Inject]
		private void Construct( IStreamingWorld world, IAssetManifestService manifest )
		{
			_streamingWorld = world;
			_manifest = manifest;
		}

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		public override void _Ready()
		{
			if ( string.IsNullOrEmpty( _assetName ) )
			{
				GameLogger.LogError( $"{Name}: StreamableMeshInstance3D requires an AssetName." );
				return;
			}

			string url;
			try
			{
				url = _manifest.Resolve( _assetName );
			}
			catch ( System.Collections.Generic.KeyNotFoundException ex )
			{
				GameLogger.LogError( $"{Name}: {ex.Message}" );
				return;
			}

			_resource = new GodotStreamableMesh(
				_streamingWorld.Core,
				url,
				new System.Numerics.Vector3( GlobalPosition.X, GlobalPosition.Y, GlobalPosition.Z ),
				arrayMesh => Mesh = arrayMesh );
		}

		public override void _ExitTree() => _resource?.Unregister();

		public override void _Process( double delta )
		{
			if ( _resource != null )
				_resource.WorldPosition = new System.Numerics.Vector3( GlobalPosition.X, GlobalPosition.Y, GlobalPosition.Z );
		}
	}

	/// <summary>Decoded payload for one LOD level, prepared off the main thread.</summary>
	internal readonly struct DecodedMeshLevel
	{
		public readonly Vector3[] Positions;
		public readonly Vector3[] Normals;
		public readonly Vector2[] Uvs;
		public readonly int[] Indices;

		public DecodedMeshLevel( Vector3[] positions, Vector3[] normals, Vector2[] uvs, int[] indices )
		{
			Positions = positions;
			Normals = normals;
			Uvs = uvs;
			Indices = indices;
		}
	}

	/// <summary>Core-facing half of <see cref="StreamableMeshInstance3D"/> - decode stays engine-agnostic data, only the final ArrayMesh build touches Godot.</summary>
	internal sealed class GodotStreamableMesh : StreamableResource
	{
		private readonly Action<ArrayMesh> _onMeshReplaced;

		public GodotStreamableMesh(
			StreamingWorld world,
			string url,
			System.Numerics.Vector3 worldPosition,
			Action<ArrayMesh> onMeshReplaced )
			: base( world, url, StreamableKind.Mesh, worldPosition )
		{
			_onMeshReplaced = onMeshReplaced;
		}

		protected override object? PrepareChunk( int level, ReadOnlyMemory<byte> chunkData )
		{
			try
			{
				return Decode( chunkData.Span );
			}
			catch ( Exception ex )
			{
				GameLogger.LogError( $"Streaming: failed to decode LOD {level} of '{Handle}' ({ex.Message})." );
				return null;
			}
		}

		protected override void ApplyPreparedChunk( int level, object? prepared )
		{
			if ( prepared is not DecodedMeshLevel decoded )
				return;

			var arrays = new GArray();
			arrays.Resize( (int)Mesh.ArrayType.Max );
			arrays[ (int)Mesh.ArrayType.Vertex ] = decoded.Positions;
			arrays[ (int)Mesh.ArrayType.Normal ] = decoded.Normals;
			arrays[ (int)Mesh.ArrayType.TexUV ] = decoded.Uvs;
			arrays[ (int)Mesh.ArrayType.Index ] = decoded.Indices;

			var mesh = new ArrayMesh();
			mesh.AddSurfaceFromArrays( Mesh.PrimitiveType.Triangles, arrays );
			_onMeshReplaced( mesh );
		}

		private static DecodedMeshLevel Decode( ReadOnlySpan<byte> data )
		{
			int offset = 0;
			int vertexCount = ReadInt32( data, ref offset );
			int indexCount = ReadInt32( data, ref offset );

			var positions = new Vector3[ vertexCount ];
			var normals = new Vector3[ vertexCount ];
			var uvs = new Vector2[ vertexCount ];

			for ( int i = 0; i < vertexCount; i++ )
			{
				positions[ i ] = new Vector3( ReadFloat( data, ref offset ), ReadFloat( data, ref offset ), ReadFloat( data, ref offset ) );
				normals[ i ] = new Vector3( ReadFloat( data, ref offset ), ReadFloat( data, ref offset ), ReadFloat( data, ref offset ) );
				uvs[ i ] = new Vector2( ReadFloat( data, ref offset ), ReadFloat( data, ref offset ) );
			}

			var indices = new int[ indexCount ];
			for ( int i = 0; i < indexCount; i++ )
				indices[ i ] = ReadInt32( data, ref offset );

			return new DecodedMeshLevel( positions, normals, uvs, indices );
		}

		private static int ReadInt32( ReadOnlySpan<byte> data, ref int offset )
		{
			int value = BitConverter.ToInt32( data.Slice( offset, 4 ) );
			offset += 4;
			return value;
		}

		private static float ReadFloat( ReadOnlySpan<byte> data, ref int offset )
		{
			float value = BitConverter.ToSingle( data.Slice( offset, 4 ) );
			offset += 4;
			return value;
		}
	}
}
