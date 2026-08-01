// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.
//
// Values ported 1:1 from id Software's Doom 3 GPL source:
//   neo/game/physics/Physics_Player.cpp / .h
//
// IMPORTANT — UNITS:
// Doom 3 works in "Doom units" where 1 unit = 1 inch (id Tech 4 convention).
// PM_STOPSPEED = 100.0f therefore means "100 inches/sec", not "100 m/s".
// Our engine works in meters. To keep the *feel* identical while using
// sane meter-scale numbers, every speed/accel constant below is stored
// exactly as it appears in the original source (DoomUnits), plus a single
// conversion factor. Traits multiply by InchesToMeters when reading them
// so behavior matches the original at 1:1 scale.
//
// If you'd rather work purely in Doom units (e.g. treat 1 Godot meter as
// 1 Doom inch, like the original game did), set InchesToMeters = 1f and
// scale your level geometry/camera FOV accordingly instead — that is
// actually closer to "don't touch the algorithm, don't touch the numbers".

namespace Framework.Components.Mover.Core
{
	public static class Doom3Constants
	{
		// Physics_Player.cpp, lines 38-52
		public const float PM_STOPSPEED = 100.0f;
		public const float PM_SWIMSCALE = 0.5f;
		public const float PM_LADDERSPEED = 100.0f;
		public const float PM_STEPSCALE = 1.0f;

		public const float PM_ACCELERATE = 10.0f;
		public const float PM_AIRACCELERATE = 1.0f;
		public const float PM_WATERACCELERATE = 4.0f;
		public const float PM_FLYACCELERATE = 8.0f;

		public const float PM_FRICTION = 6.0f;
		public const float PM_AIRFRICTION = 0.0f;
		public const float PM_WATERFRICTION = 1.0f;
		public const float PM_FLYFRICTION = 3.0f;
		public const float PM_NOCLIPFRICTION = 12.0f;

		// Physics_Player.cpp, lines 54-55
		public const float MIN_WALK_NORMAL = 0.7f;   // can't walk on very steep slopes
		public const float OVERCLIP = 1.001f;

		// Physics.h — DEFAULT_GRAVITY (neo/game/physics/Physics.h)
		public const float DEFAULT_GRAVITY = 1066.0f; // inches / sec^2

		/// <summary>
		/// 1 Doom unit == 1 inch == 0.0254 m. All PM_* constants above are in
		/// Doom units/sec (or /sec^2). Traits multiply by this to land in meters.
		/// Set to 1f instead if you want to run the game in raw Doom-unit scale.
		/// </summary>
		public const float InchesToMeters = 0.0254f;
	}
}
