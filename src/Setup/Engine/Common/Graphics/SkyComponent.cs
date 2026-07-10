// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Setup.Engine.Common.Graphics
{
	[Tool]
	public partial class SkyComponent : MeshInstance3D
	{
		[Export] private Node3D _followTarget;

		private ShaderMaterial _material;

		public override void _Ready() =>
			_material = GetActiveMaterial( 0 ) as ShaderMaterial;

		// =========================================================================
		// Day / Night
		// =========================================================================
		[ExportGroup( "Day / Night" )]

		[Export( PropertyHint.Range, "0.0,1.0,0.001" )]
		public float TimeOfDay
		{
			get => _material?.GetShaderParameter( "time_of_day" ).As<float>() ?? 0.5f;
			set => _material?.SetShaderParameter( "time_of_day", value );
		}

		[Export]
		public bool ProceduralSkyEnabled
		{
			get => _material?.GetShaderParameter( "procedural_sky_enabled" ).As<bool>() ?? true;
			set => _material?.SetShaderParameter( "procedural_sky_enabled", value );
		}

		// =========================================================================
		// Sky
		// =========================================================================
		[ExportGroup( "Sky" )]

		[Export]
		public Color SkyHorizonColorDay
		{
			get => _material?.GetShaderParameter( "sky_horizon_color_day" ).As<Color>() ?? new Color( 0.85f, 0.72f, 0.55f );
			set => _material?.SetShaderParameter( "sky_horizon_color_day", value );
		}

		[Export]
		public Color SkyZenithColorDay
		{
			get => _material?.GetShaderParameter( "sky_zenith_color_day" ).As<Color>() ?? new Color( 0.15f, 0.25f, 0.55f );
			set => _material?.SetShaderParameter( "sky_zenith_color_day", value );
		}

		[Export]
		public Color GroundHorizonColorDay
		{
			get => _material?.GetShaderParameter( "ground_horizon_color_day" ).As<Color>() ?? new Color( 0.55f, 0.45f, 0.35f );
			set => _material?.SetShaderParameter( "ground_horizon_color_day", value );
		}

		[Export]
		public Color GroundColorDay
		{
			get => _material?.GetShaderParameter( "ground_color_day" ).As<Color>() ?? new Color( 0.10f, 0.08f, 0.07f );
			set => _material?.SetShaderParameter( "ground_color_day", value );
		}

		[Export]
		public Color SkyHorizonColorNight
		{
			get => _material?.GetShaderParameter( "sky_horizon_color_night" ).As<Color>() ?? new Color( 0.05f, 0.06f, 0.12f );
			set => _material?.SetShaderParameter( "sky_horizon_color_night", value );
		}

		[Export]
		public Color SkyZenithColorNight
		{
			get => _material?.GetShaderParameter( "sky_zenith_color_night" ).As<Color>() ?? new Color( 0.01f, 0.01f, 0.04f );
			set => _material?.SetShaderParameter( "sky_zenith_color_night", value );
		}

		[Export]
		public Color GroundHorizonColorNight
		{
			get => _material?.GetShaderParameter( "ground_horizon_color_night" ).As<Color>() ?? new Color( 0.03f, 0.03f, 0.05f );
			set => _material?.SetShaderParameter( "ground_horizon_color_night", value );
		}

		[Export]
		public Color GroundColorNight
		{
			get => _material?.GetShaderParameter( "ground_color_night" ).As<Color>() ?? new Color( 0.01f, 0.01f, 0.02f );
			set => _material?.SetShaderParameter( "ground_color_night", value );
		}

		[Export( PropertyHint.Range, "0.001,1.0" )]
		public float HorizonBlur
		{
			get => _material?.GetShaderParameter( "horizon_blur" ).As<float>() ?? 0.08f;
			set => _material?.SetShaderParameter( "horizon_blur", value );
		}

		[Export( PropertyHint.Range, "0.1,8.0" )]
		public float SkyCurve
		{
			get => _material?.GetShaderParameter( "sky_curve" ).As<float>() ?? 1.0f;
			set => _material?.SetShaderParameter( "sky_curve", value );
		}

		[Export( PropertyHint.Range, "0.1,8.0" )]
		public float GroundCurve
		{
			get => _material?.GetShaderParameter( "ground_curve" ).As<float>() ?? 1.0f;
			set => _material?.SetShaderParameter( "ground_curve", value );
		}

		[Export]
		public Vector2 DayNightRange
		{
			get => _material?.GetShaderParameter( "day_night_range" ).As<Vector2>() ?? new Vector2( -0.2f, 0.15f );
			set => _material?.SetShaderParameter( "day_night_range", value );
		}

		// =========================================================================
		// Sun
		// =========================================================================
		[ExportGroup( "Sun" )]

		[Export]
		public bool SunEnabled
		{
			get => _material?.GetShaderParameter( "sun_enabled" ).As<bool>() ?? true;
			set => _material?.SetShaderParameter( "sun_enabled", value );
		}

		[Export]
		public Color SunColor
		{
			get => _material?.GetShaderParameter( "sun_color" ).As<Color>() ?? new Color( 1.0f, 0.95f, 0.85f );
			set => _material?.SetShaderParameter( "sun_color", value );
		}

		[Export( PropertyHint.Range, "0.0001,0.5" )]
		public float SunSize
		{
			get => _material?.GetShaderParameter( "sun_size" ).As<float>() ?? 0.02f;
			set => _material?.SetShaderParameter( "sun_size", value );
		}

		[Export( PropertyHint.Range, "0.0,64.0" )]
		public float SunSharpness
		{
			get => _material?.GetShaderParameter( "sun_sharpness" ).As<float>() ?? 16.0f;
			set => _material?.SetShaderParameter( "sun_sharpness", value );
		}

		[Export]
		public Color SunHaloColor
		{
			get => _material?.GetShaderParameter( "sun_halo_color" ).As<Color>() ?? new Color( 1.0f, 0.8f, 0.5f, 0.6f );
			set => _material?.SetShaderParameter( "sun_halo_color", value );
		}

		[Export( PropertyHint.Range, "0.0,2.0" )]
		public float SunHaloSize
		{
			get => _material?.GetShaderParameter( "sun_halo_size" ).As<float>() ?? 0.3f;
			set => _material?.SetShaderParameter( "sun_halo_size", value );
		}

		// =========================================================================
		// Moon
		// =========================================================================
		[ExportGroup( "Moon" )]

		[Export]
		public bool MoonEnabled
		{
			get => _material?.GetShaderParameter( "moon_enabled" ).As<bool>() ?? true;
			set => _material?.SetShaderParameter( "moon_enabled", value );
		}

		[Export]
		public Texture2D MoonTexture
		{
			get => _material?.GetShaderParameter( "moon_texture" ).As<Texture2D>();
			set => _material?.SetShaderParameter( "moon_texture", value );
		}

		[Export]
		public Vector3 MoonDirection
		{
			get => _material?.GetShaderParameter( "moon_direction" ).As<Vector3>() ?? new Vector3( 0.0f, -1.0f, 0.0f );
			set => _material?.SetShaderParameter( "moon_direction", value );
		}

		[Export( PropertyHint.Range, "0.001,0.5" )]
		public float MoonSize
		{
			get => _material?.GetShaderParameter( "moon_size" ).As<float>() ?? 0.05f;
			set => _material?.SetShaderParameter( "moon_size", value );
		}

		[Export]
		public Color MoonTint
		{
			get => _material?.GetShaderParameter( "moon_tint" ).As<Color>() ?? new Color( 1.0f, 1.0f, 1.0f );
			set => _material?.SetShaderParameter( "moon_tint", value );
		}

		// =========================================================================
		// Clouds
		// =========================================================================
		[ExportGroup( "Clouds" )]

		[Export]
		public bool CloudsEnabled
		{
			get => _material?.GetShaderParameter( "clouds_enabled" ).As<bool>() ?? true;
			set => _material?.SetShaderParameter( "clouds_enabled", value );
		}

		[Export]
		public Texture2D NoiseTexture
		{
			get => _material?.GetShaderParameter( "noise_texture" ).As<Texture2D>();
			set => _material?.SetShaderParameter( "noise_texture", value );
		}

		[Export]
		public Color CloudColorDay
		{
			get => _material?.GetShaderParameter( "cloud_color_day" ).As<Color>() ?? new Color( 0.85f, 0.88f, 0.95f );
			set => _material?.SetShaderParameter( "cloud_color_day", value );
		}

		[Export]
		public Color CloudColorNight
		{
			get => _material?.GetShaderParameter( "cloud_color_night" ).As<Color>() ?? new Color( 0.18f, 0.20f, 0.32f );
			set => _material?.SetShaderParameter( "cloud_color_night", value );
		}

		[Export( PropertyHint.Range, "0.0,32.0" )]
		public float CloudDensity
		{
			get => _material?.GetShaderParameter( "cloud_density" ).As<float>() ?? 5.0f;
			set => _material?.SetShaderParameter( "cloud_density", value );
		}

		[Export( PropertyHint.Range, "0.0,1.0" )]
		public float CloudThreshold
		{
			get => _material?.GetShaderParameter( "cloud_threshold" ).As<float>() ?? 0.3f;
			set => _material?.SetShaderParameter( "cloud_threshold", value );
		}

		[Export( PropertyHint.Range, "0.1,100.0" )]
		public float CloudHeight
		{
			get => _material?.GetShaderParameter( "cloud_height" ).As<float>() ?? 1.0f;
			set => _material?.SetShaderParameter( "cloud_height", value );
		}

		[Export]
		public Vector2 CloudTiling
		{
			get => _material?.GetShaderParameter( "cloud_tiling" ).As<Vector2>() ?? new Vector2( 1.0f, 1.0f );
			set => _material?.SetShaderParameter( "cloud_tiling", value );
		}

		[Export]
		public Vector2 CloudWindSpeed
		{
			get => _material?.GetShaderParameter( "cloud_wind_speed" ).As<Vector2>() ?? new Vector2( 0.5f, 0.0f );
			set => _material?.SetShaderParameter( "cloud_wind_speed", value );
		}

		[Export( PropertyHint.Range, "0.0,1.0" )]
		public float CloudsHorizonFade
		{
			get => _material?.GetShaderParameter( "clouds_horizon_fade" ).As<float>() ?? 0.15f;
			set => _material?.SetShaderParameter( "clouds_horizon_fade", value );
		}

		[Export]
		public bool CloudsLayer2Enabled
		{
			get => _material?.GetShaderParameter( "clouds_layer2_enabled" ).As<bool>() ?? false;
			set => _material?.SetShaderParameter( "clouds_layer2_enabled", value );
		}

		[Export( PropertyHint.Range, "0.1,100.0" )]
		public float CloudHeight2
		{
			get => _material?.GetShaderParameter( "cloud_height_2" ).As<float>() ?? 2.0f;
			set => _material?.SetShaderParameter( "cloud_height_2", value );
		}

		[Export]
		public Vector2 CloudTiling2
		{
			get => _material?.GetShaderParameter( "cloud_tiling_2" ).As<Vector2>() ?? new Vector2( 0.7f, 0.7f );
			set => _material?.SetShaderParameter( "cloud_tiling_2", value );
		}

		[Export]
		public Vector2 CloudWindSpeed2
		{
			get => _material?.GetShaderParameter( "cloud_wind_speed_2" ).As<Vector2>() ?? new Vector2( -0.3f, 0.1f );
			set => _material?.SetShaderParameter( "cloud_wind_speed_2", value );
		}

		[Export( PropertyHint.Range, "0.0,32.0" )]
		public float CloudDensity2
		{
			get => _material?.GetShaderParameter( "cloud_density_2" ).As<float>() ?? 3.0f;
			set => _material?.SetShaderParameter( "cloud_density_2", value );
		}

		[Export( PropertyHint.Range, "0.0,1.0" )]
		public float CloudThreshold2
		{
			get => _material?.GetShaderParameter( "cloud_threshold_2" ).As<float>() ?? 0.4f;
			set => _material?.SetShaderParameter( "cloud_threshold_2", value );
		}

		// =========================================================================
		// Stars
		// =========================================================================
		[ExportGroup( "Stars" )]

		[Export]
		public bool StarsEnabled
		{
			get => _material?.GetShaderParameter( "stars_enabled" ).As<bool>() ?? true;
			set => _material?.SetShaderParameter( "stars_enabled", value );
		}

		[Export( PropertyHint.Range, "0.0,3.0,0.001" )]
		public float StarDensity
		{
			get => _material?.GetShaderParameter( "star_density" ).As<float>() ?? 1.3f;
			set => _material?.SetShaderParameter( "star_density", value );
		}

		[Export( PropertyHint.Range, "0.1,2.0,0.01" )]
		public float StarSize
		{
			get => _material?.GetShaderParameter( "star_size" ).As<float>() ?? 1.0f;
			set => _material?.SetShaderParameter( "star_size", value );
		}

		[Export( PropertyHint.Range, "0.0,2.0" )]
		public float StarBrightness
		{
			get => _material?.GetShaderParameter( "star_brightness" ).As<float>() ?? 1.0f;
			set => _material?.SetShaderParameter( "star_brightness", value );
		}

		[Export( PropertyHint.Range, "0.0,1.0" )]
		public float StarsHorizonFade
		{
			get => _material?.GetShaderParameter( "stars_horizon_fade" ).As<float>() ?? 0.15f;
			set => _material?.SetShaderParameter( "stars_horizon_fade", value );
		}

		[Export]
		public bool StarTwinkleEnabled
		{
			get => _material?.GetShaderParameter( "star_twinkle_enabled" ).As<bool>() ?? false;
			set => _material?.SetShaderParameter( "star_twinkle_enabled", value );
		}

		[Export( PropertyHint.Range, "0.0,3.0" )]
		public float StarTwinkleSpeed
		{
			get => _material?.GetShaderParameter( "star_twinkle_speed" ).As<float>() ?? 0.5f;
			set => _material?.SetShaderParameter( "star_twinkle_speed", value );
		}
	}
}