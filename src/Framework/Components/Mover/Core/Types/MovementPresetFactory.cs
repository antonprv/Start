// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Presets;

namespace Framework.Components.Mover.Core.Types
{
	public static class MovementPresetFactory
	{
		private static readonly Dictionary<MovementPreset, Type> _typeList = new Dictionary<MovementPreset, Type>()
		{
			{ MovementPreset.Doom3, typeof(Doom3Preset) },
			{ MovementPreset.Hybrid, typeof(HybridPreset) },
			{ MovementPreset.Quake, typeof(QuakePreset) },
			{ MovementPreset.QuakeStrafeDoom2016, typeof(QuakeStrafeDoom2016Preset) },
			{ MovementPreset.Realistic, typeof(RealisticPreset) },
		};

		public static IMovementPreset Create( MovementPreset preset )
		{
			if ( _typeList.TryGetValue( preset, out Type type ) )
			{
				return (IMovementPreset)Activator.CreateInstance( type );
			}

			return null;
		}
	}
}
