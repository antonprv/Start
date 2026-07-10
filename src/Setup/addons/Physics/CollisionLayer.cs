// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;

namespace Physics
{
	/// <summary>
	/// Semantic names for collision layer bits, for editor-friendly [Export] fields on physics
	/// components. Framework.Physics has no opinion on layer naming - it only ever sees the
	/// raw <see cref="uint"/> value - so feel free to add/rename/reorganize these for your game
	/// without touching Core at all.
	/// </summary>
	[Flags]
	public enum CollisionLayer : uint
	{
		None = 0,
		World = 1u << 0,
		Character = 1u << 1,
		Projectile = 1u << 2,
		Trigger = 1u << 3,
		Prop = 1u << 4,
		Debris = 1u << 5,
		All = 0xFFFFFFFFu
	}
}
