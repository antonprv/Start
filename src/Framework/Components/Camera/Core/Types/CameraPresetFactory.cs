// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core.Interfaces;
using Framework.Components.Camera.Presets;

namespace Framework.Components.Camera.Core.Types
{
	public static class CameraPresetFactory
	{
		private static readonly Dictionary<CameraPreset, Type> _typeList = new Dictionary<CameraPreset, Type>()
		{
			{ CameraPreset.EdgeScrollTopDown, typeof(EdgeScrollTopDownPreset) },
			{ CameraPreset.FirstPersonOvershoot, typeof(FirstPersonOvershootPreset) },
			{ CameraPreset.FirstPerson, typeof(FirstPersonPreset) },
			{ CameraPreset.GeneralThirdPerson, typeof(GeneralThirdPersonPreset) },
			{ CameraPreset.ThirdPersonShoulder, typeof(ThirdPersonShoulderPreset) },
			{ CameraPreset.ThirdPersonSoulslike, typeof(ThirdPersonSoulslikePreset) },
			{ CameraPreset.TopDownFixed, typeof(TopDownFixedPreset) },
			{ CameraPreset.TopDownOrbit, typeof(TopDownOrbitPreset) }
		};

		public static ICameraPreset Create( CameraPreset preset )
		{
			if ( _typeList.TryGetValue( preset, out Type type ) )
			{
				return (ICameraPreset)Activator.CreateInstance( type );
			}

			return null;
		}
	}
}
