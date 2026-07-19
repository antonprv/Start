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

		public override void _Ready()
		{
			_material = GetActiveMaterial( 0 ) as ShaderMaterial;
			ApplyAllShaderParameters();
		}

		// Exported setters run before _Ready() when the scene is deserialized, so
		// _material is still null at that point and SetShaderParameter is a no-op.
		// This pushes every backing field to the material once it actually exists.
		private void ApplyAllShaderParameters()
		{
			if ( _material is null )
				return;

			_material.SetShaderParameter( "time_of_day", _timeOfDay );
			_material.SetShaderParameter( "procedural_sky_enabled", _proceduralSkyEnabled );

			_material.SetShaderParameter( "sky_horizon_color_day", _skyHorizonColorDay );
			_material.SetShaderParameter( "sky_zenith_color_day", _skyZenithColorDay );
			_material.SetShaderParameter( "ground_horizon_color_day", _groundHorizonColorDay );
			_material.SetShaderParameter( "ground_color_day", _groundColorDay );
			_material.SetShaderParameter( "sky_horizon_color_night", _skyHorizonColorNight );
			_material.SetShaderParameter( "sky_zenith_color_night", _skyZenithColorNight );
			_material.SetShaderParameter( "ground_horizon_color_night", _groundHorizonColorNight );
			_material.SetShaderParameter( "ground_color_night", _groundColorNight );
			_material.SetShaderParameter( "horizon_blur", _horizonBlur );
			_material.SetShaderParameter( "sky_curve", _skyCurve );
			_material.SetShaderParameter( "ground_curve", _groundCurve );
			_material.SetShaderParameter( "day_night_range", _dayNightRange );

			_material.SetShaderParameter( "sun_enabled", _sunEnabled );
			_material.SetShaderParameter( "sun_color", _sunColor );
			_material.SetShaderParameter( "sun_size", _sunSize );
			_material.SetShaderParameter( "sun_sharpness", _sunSharpness );
			_material.SetShaderParameter( "sun_halo_color", _sunHaloColor );
			_material.SetShaderParameter( "sun_halo_size", _sunHaloSize );

			_material.SetShaderParameter( "moon_enabled", _moonEnabled );
			_material.SetShaderParameter( "moon_texture", _moonTexture );
			_material.SetShaderParameter( "moon_direction", _moonDirection );
			_material.SetShaderParameter( "moon_size", _moonSize );
			_material.SetShaderParameter( "moon_tint", _moonTint );
			_material.SetShaderParameter( "moon_halo_color", _moonHaloColor );
			_material.SetShaderParameter( "moon_halo_radius", _moonHaloRadius );
			_material.SetShaderParameter( "moon_halo_size", _moonHaloSize );
			_material.SetShaderParameter( "moon_halo_in_front", _moonHaloInFront );
			_material.SetShaderParameter( "moon_cloud_dimming", _moonCloudDimming );

			_material.SetShaderParameter( "clouds_enabled", _cloudsEnabled );
			_material.SetShaderParameter( "noise_texture", _noiseTexture );
			_material.SetShaderParameter( "cloud_color_day", _cloudColorDay );
			_material.SetShaderParameter( "cloud_color_night", _cloudColorNight );
			_material.SetShaderParameter( "cloud_density", _cloudDensity );
			_material.SetShaderParameter( "cloud_threshold", _cloudThreshold );
			_material.SetShaderParameter( "cloud_height", _cloudHeight );
			_material.SetShaderParameter( "cloud_tiling", _cloudTiling );
			_material.SetShaderParameter( "cloud_wind_speed", _cloudWindSpeed );
			_material.SetShaderParameter( "clouds_horizon_fade", _cloudsHorizonFade );
			_material.SetShaderParameter( "clouds_layer2_enabled", _cloudsLayer2Enabled );
			_material.SetShaderParameter( "cloud_height_2", _cloudHeight2 );
			_material.SetShaderParameter( "cloud_tiling_2", _cloudTiling2 );
			_material.SetShaderParameter( "cloud_wind_speed_2", _cloudWindSpeed2 );
			_material.SetShaderParameter( "cloud_density_2", _cloudDensity2 );
			_material.SetShaderParameter( "cloud_threshold_2", _cloudThreshold2 );

			_material.SetShaderParameter( "stars_enabled", _starsEnabled );
			_material.SetShaderParameter( "star_density", _starDensity );
			_material.SetShaderParameter( "star_size", _starSize );
			_material.SetShaderParameter( "star_brightness", _starBrightness );
			_material.SetShaderParameter( "stars_horizon_fade", _starsHorizonFade );
			_material.SetShaderParameter( "star_twinkle_enabled", _starTwinkleEnabled );
			_material.SetShaderParameter( "star_twinkle_speed", _starTwinkleSpeed );
		}

		// =========================================================================
		// Day / Night
		// =========================================================================
		[ExportGroup( "Day / Night" )]

		private float _timeOfDay = 0.5f;

		[Export( PropertyHint.Range, "0.0,1.0,0.001" )]
		public float TimeOfDay
		{
			get => _timeOfDay;
			set
			{
				_timeOfDay = value;
				_material?.SetShaderParameter( "time_of_day", value );
			}
		}

		private bool _proceduralSkyEnabled = true;

		[Export]
		public bool ProceduralSkyEnabled
		{
			get => _proceduralSkyEnabled;
			set
			{
				_proceduralSkyEnabled = value;
				_material?.SetShaderParameter( "procedural_sky_enabled", value );
			}
		}

		// =========================================================================
		// Sky
		// =========================================================================
		[ExportGroup( "Sky" )]

		private Color _skyHorizonColorDay = new Color( 0.85f, 0.72f, 0.55f );

		[Export]
		public Color SkyHorizonColorDay
		{
			get => _skyHorizonColorDay;
			set
			{
				_skyHorizonColorDay = value;
				_material?.SetShaderParameter( "sky_horizon_color_day", value );
			}
		}

		private Color _skyZenithColorDay = new Color( 0.15f, 0.25f, 0.55f );

		[Export]
		public Color SkyZenithColorDay
		{
			get => _skyZenithColorDay;
			set
			{
				_skyZenithColorDay = value;
				_material?.SetShaderParameter( "sky_zenith_color_day", value );
			}
		}

		private Color _groundHorizonColorDay = new Color( 0.55f, 0.45f, 0.35f );

		[Export]
		public Color GroundHorizonColorDay
		{
			get => _groundHorizonColorDay;
			set
			{
				_groundHorizonColorDay = value;
				_material?.SetShaderParameter( "ground_horizon_color_day", value );
			}
		}

		private Color _groundColorDay = new Color( 0.10f, 0.08f, 0.07f );

		[Export]
		public Color GroundColorDay
		{
			get => _groundColorDay;
			set
			{
				_groundColorDay = value;
				_material?.SetShaderParameter( "ground_color_day", value );
			}
		}

		private Color _skyHorizonColorNight = new Color( 0.05f, 0.06f, 0.12f );

		[Export]
		public Color SkyHorizonColorNight
		{
			get => _skyHorizonColorNight;
			set
			{
				_skyHorizonColorNight = value;
				_material?.SetShaderParameter( "sky_horizon_color_night", value );
			}
		}

		private Color _skyZenithColorNight = new Color( 0.01f, 0.01f, 0.04f );

		[Export]
		public Color SkyZenithColorNight
		{
			get => _skyZenithColorNight;
			set
			{
				_skyZenithColorNight = value;
				_material?.SetShaderParameter( "sky_zenith_color_night", value );
			}
		}

		private Color _groundHorizonColorNight = new Color( 0.03f, 0.03f, 0.05f );

		[Export]
		public Color GroundHorizonColorNight
		{
			get => _groundHorizonColorNight;
			set
			{
				_groundHorizonColorNight = value;
				_material?.SetShaderParameter( "ground_horizon_color_night", value );
			}
		}

		private Color _groundColorNight = new Color( 0.01f, 0.01f, 0.02f );

		[Export]
		public Color GroundColorNight
		{
			get => _groundColorNight;
			set
			{
				_groundColorNight = value;
				_material?.SetShaderParameter( "ground_color_night", value );
			}
		}

		private float _horizonBlur = 0.08f;

		[Export( PropertyHint.Range, "0.001,1.0" )]
		public float HorizonBlur
		{
			get => _horizonBlur;
			set
			{
				_horizonBlur = value;
				_material?.SetShaderParameter( "horizon_blur", value );
			}
		}

		private float _skyCurve = 1.0f;

		[Export( PropertyHint.Range, "0.1,8.0" )]
		public float SkyCurve
		{
			get => _skyCurve;
			set
			{
				_skyCurve = value;
				_material?.SetShaderParameter( "sky_curve", value );
			}
		}

		private float _groundCurve = 1.0f;

		[Export( PropertyHint.Range, "0.1,8.0" )]
		public float GroundCurve
		{
			get => _groundCurve;
			set
			{
				_groundCurve = value;
				_material?.SetShaderParameter( "ground_curve", value );
			}
		}

		private Vector2 _dayNightRange = new Vector2( -0.2f, 0.15f );

		[Export]
		public Vector2 DayNightRange
		{
			get => _dayNightRange;
			set
			{
				_dayNightRange = value;
				_material?.SetShaderParameter( "day_night_range", value );
			}
		}

		// =========================================================================
		// Sun
		// =========================================================================
		[ExportGroup( "Sun" )]

		private bool _sunEnabled = true;

		[Export]
		public bool SunEnabled
		{
			get => _sunEnabled;
			set
			{
				_sunEnabled = value;
				_material?.SetShaderParameter( "sun_enabled", value );
			}
		}

		private Color _sunColor = new Color( 1.0f, 0.95f, 0.85f );

		[Export]
		public Color SunColor
		{
			get => _sunColor;
			set
			{
				_sunColor = value;
				_material?.SetShaderParameter( "sun_color", value );
			}
		}

		private float _sunSize = 0.02f;

		[Export( PropertyHint.Range, "0.0001,0.5" )]
		public float SunSize
		{
			get => _sunSize;
			set
			{
				_sunSize = value;
				_material?.SetShaderParameter( "sun_size", value );
			}
		}

		private float _sunSharpness = 16.0f;

		[Export( PropertyHint.Range, "0.0,64.0" )]
		public float SunSharpness
		{
			get => _sunSharpness;
			set
			{
				_sunSharpness = value;
				_material?.SetShaderParameter( "sun_sharpness", value );
			}
		}

		private Color _sunHaloColor = new Color( 1.0f, 0.8f, 0.5f, 0.6f );

		[Export]
		public Color SunHaloColor
		{
			get => _sunHaloColor;
			set
			{
				_sunHaloColor = value;
				_material?.SetShaderParameter( "sun_halo_color", value );
			}
		}

		private float _sunHaloSize = 0.3f;

		[Export( PropertyHint.Range, "0.0,2.0" )]
		public float SunHaloSize
		{
			get => _sunHaloSize;
			set
			{
				_sunHaloSize = value;
				_material?.SetShaderParameter( "sun_halo_size", value );
			}
		}

		// =========================================================================
		// Moon
		// =========================================================================
		[ExportGroup( "Moon" )]

		private bool _moonEnabled = true;

		[Export]
		public bool MoonEnabled
		{
			get => _moonEnabled;
			set
			{
				_moonEnabled = value;
				_material?.SetShaderParameter( "moon_enabled", value );
			}
		}

		private Texture2D _moonTexture;

		[Export]
		public Texture2D MoonTexture
		{
			get => _moonTexture;
			set
			{
				_moonTexture = value;
				_material?.SetShaderParameter( "moon_texture", value );
			}
		}

		private Vector3 _moonDirection = new Vector3( 0.0f, -1.0f, 0.0f );

		[Export]
		public Vector3 MoonDirection
		{
			get => _moonDirection;
			set
			{
				_moonDirection = value;
				_material?.SetShaderParameter( "moon_direction", value );
			}
		}

		private float _moonSize = 0.05f;

		[Export( PropertyHint.Range, "0.001,0.5" )]
		public float MoonSize
		{
			get => _moonSize;
			set
			{
				_moonSize = value;
				_material?.SetShaderParameter( "moon_size", value );
			}
		}

		private Color _moonTint = new Color( 1.0f, 1.0f, 1.0f );

		[Export]
		public Color MoonTint
		{
			get => _moonTint;
			set
			{
				_moonTint = value;
				_material?.SetShaderParameter( "moon_tint", value );
			}
		}

		private Color _moonHaloColor = new Color( 0.604f, 0.694f, 1.0f, 0.416f );

		[Export]
		public Color MoonHaloColor
		{
			get => _moonHaloColor;
			set
			{
				_moonHaloColor = value;
				_material?.SetShaderParameter( "moon_halo_color", value );
			}
		}

		private float _moonHaloRadius = 0.15f;

		[Export( PropertyHint.Range, "0.0,2.0" )]
		public float MoonHaloRadius
		{
			get => _moonHaloRadius;
			set
			{
				_moonHaloRadius = value;
				_material?.SetShaderParameter( "moon_halo_radius", value );
			}
		}

		private float _moonHaloSize = 0.3f;

		[Export( PropertyHint.Range, "0.0,2.0" )]
		public float MoonHaloSize
		{
			get => _moonHaloSize;
			set
			{
				_moonHaloSize = value;
				_material?.SetShaderParameter( "moon_halo_size", value );
			}
		}

		private bool _moonHaloInFront = false;

		[Export]
		public bool MoonHaloInFront
		{
			get => _moonHaloInFront;
			set
			{
				_moonHaloInFront = value;
				_material?.SetShaderParameter( "moon_halo_in_front", value );
			}
		}

		private float _moonCloudDimming = 0.5f;

		[Export( PropertyHint.Range, "0.0,1.0" )]
		public float MoonCloudDimming
		{
			get => _moonCloudDimming;
			set
			{
				_moonCloudDimming = value;
				_material?.SetShaderParameter( "moon_cloud_dimming", value );
			}
		}

		// =========================================================================
		// Clouds
		// =========================================================================
		[ExportGroup( "Clouds" )]

		private bool _cloudsEnabled = true;

		[Export]
		public bool CloudsEnabled
		{
			get => _cloudsEnabled;
			set
			{
				_cloudsEnabled = value;
				_material?.SetShaderParameter( "clouds_enabled", value );
			}
		}

		private Texture2D _noiseTexture;

		[Export]
		public Texture2D NoiseTexture
		{
			get => _noiseTexture;
			set
			{
				_noiseTexture = value;
				_material?.SetShaderParameter( "noise_texture", value );
			}
		}

		private Color _cloudColorDay = new Color( 0.85f, 0.88f, 0.95f );

		[Export]
		public Color CloudColorDay
		{
			get => _cloudColorDay;
			set
			{
				_cloudColorDay = value;
				_material?.SetShaderParameter( "cloud_color_day", value );
			}
		}

		private Color _cloudColorNight = new Color( 0.18f, 0.20f, 0.32f );

		[Export]
		public Color CloudColorNight
		{
			get => _cloudColorNight;
			set
			{
				_cloudColorNight = value;
				_material?.SetShaderParameter( "cloud_color_night", value );
			}
		}

		private float _cloudDensity = 5.0f;

		[Export( PropertyHint.Range, "0.0,32.0" )]
		public float CloudDensity
		{
			get => _cloudDensity;
			set
			{
				_cloudDensity = value;
				_material?.SetShaderParameter( "cloud_density", value );
			}
		}

		private float _cloudThreshold = 0.3f;

		[Export( PropertyHint.Range, "0.0,1.0" )]
		public float CloudThreshold
		{
			get => _cloudThreshold;
			set
			{
				_cloudThreshold = value;
				_material?.SetShaderParameter( "cloud_threshold", value );
			}
		}

		private float _cloudHeight = 1.0f;

		[Export( PropertyHint.Range, "0.1,100.0" )]
		public float CloudHeight
		{
			get => _cloudHeight;
			set
			{
				_cloudHeight = value;
				_material?.SetShaderParameter( "cloud_height", value );
			}
		}

		private Vector2 _cloudTiling = new Vector2( 1.0f, 1.0f );

		[Export]
		public Vector2 CloudTiling
		{
			get => _cloudTiling;
			set
			{
				_cloudTiling = value;
				_material?.SetShaderParameter( "cloud_tiling", value );
			}
		}

		private Vector2 _cloudWindSpeed = new Vector2( 0.5f, 0.0f );

		[Export]
		public Vector2 CloudWindSpeed
		{
			get => _cloudWindSpeed;
			set
			{
				_cloudWindSpeed = value;
				_material?.SetShaderParameter( "cloud_wind_speed", value );
			}
		}

		private float _cloudsHorizonFade = 0.15f;

		[Export( PropertyHint.Range, "0.0,1.0" )]
		public float CloudsHorizonFade
		{
			get => _cloudsHorizonFade;
			set
			{
				_cloudsHorizonFade = value;
				_material?.SetShaderParameter( "clouds_horizon_fade", value );
			}
		}

		private bool _cloudsLayer2Enabled = false;

		[Export]
		public bool CloudsLayer2Enabled
		{
			get => _cloudsLayer2Enabled;
			set
			{
				_cloudsLayer2Enabled = value;
				_material?.SetShaderParameter( "clouds_layer2_enabled", value );
			}
		}

		private float _cloudHeight2 = 2.0f;

		[Export( PropertyHint.Range, "0.1,100.0" )]
		public float CloudHeight2
		{
			get => _cloudHeight2;
			set
			{
				_cloudHeight2 = value;
				_material?.SetShaderParameter( "cloud_height_2", value );
			}
		}

		private Vector2 _cloudTiling2 = new Vector2( 0.7f, 0.7f );

		[Export]
		public Vector2 CloudTiling2
		{
			get => _cloudTiling2;
			set
			{
				_cloudTiling2 = value;
				_material?.SetShaderParameter( "cloud_tiling_2", value );
			}
		}

		private Vector2 _cloudWindSpeed2 = new Vector2( -0.3f, 0.1f );

		[Export]
		public Vector2 CloudWindSpeed2
		{
			get => _cloudWindSpeed2;
			set
			{
				_cloudWindSpeed2 = value;
				_material?.SetShaderParameter( "cloud_wind_speed_2", value );
			}
		}

		private float _cloudDensity2 = 3.0f;

		[Export( PropertyHint.Range, "0.0,32.0" )]
		public float CloudDensity2
		{
			get => _cloudDensity2;
			set
			{
				_cloudDensity2 = value;
				_material?.SetShaderParameter( "cloud_density_2", value );
			}
		}

		private float _cloudThreshold2 = 0.4f;

		[Export( PropertyHint.Range, "0.0,1.0" )]
		public float CloudThreshold2
		{
			get => _cloudThreshold2;
			set
			{
				_cloudThreshold2 = value;
				_material?.SetShaderParameter( "cloud_threshold_2", value );
			}
		}

		// =========================================================================
		// Stars
		// =========================================================================
		[ExportGroup( "Stars" )]

		private bool _starsEnabled = true;

		[Export]
		public bool StarsEnabled
		{
			get => _starsEnabled;
			set
			{
				_starsEnabled = value;
				_material?.SetShaderParameter( "stars_enabled", value );
			}
		}

		private float _starDensity = 1.3f;

		[Export( PropertyHint.Range, "0.0,3.0,0.001" )]
		public float StarDensity
		{
			get => _starDensity;
			set
			{
				_starDensity = value;
				_material?.SetShaderParameter( "star_density", value );
			}
		}

		private float _starSize = 1.0f;

		[Export( PropertyHint.Range, "0.1,2.0,0.01" )]
		public float StarSize
		{
			get => _starSize;
			set
			{
				_starSize = value;
				_material?.SetShaderParameter( "star_size", value );
			}
		}

		private float _starBrightness = 1.0f;

		[Export( PropertyHint.Range, "0.0,2.0" )]
		public float StarBrightness
		{
			get => _starBrightness;
			set
			{
				_starBrightness = value;
				_material?.SetShaderParameter( "star_brightness", value );
			}
		}

		private float _starsHorizonFade = 0.15f;

		[Export( PropertyHint.Range, "0.0,1.0" )]
		public float StarsHorizonFade
		{
			get => _starsHorizonFade;
			set
			{
				_starsHorizonFade = value;
				_material?.SetShaderParameter( "stars_horizon_fade", value );
			}
		}

		private bool _starTwinkleEnabled = false;

		[Export]
		public bool StarTwinkleEnabled
		{
			get => _starTwinkleEnabled;
			set
			{
				_starTwinkleEnabled = value;
				_material?.SetShaderParameter( "star_twinkle_enabled", value );
			}
		}

		private float _starTwinkleSpeed = 0.5f;

		[Export( PropertyHint.Range, "0.0,3.0" )]
		public float StarTwinkleSpeed
		{
			get => _starTwinkleSpeed;
			set
			{
				_starTwinkleSpeed = value;
				_material?.SetShaderParameter( "star_twinkle_speed", value );
			}
		}
	}
}