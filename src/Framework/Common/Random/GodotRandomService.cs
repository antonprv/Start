// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.FastMath.Godot;
using Godot;

namespace Framework.Common.Random
{
    public class GodotRandomService : IRandomService
    {
        #region State

        private int _lastInt;
        private bool _hasLastInt;

        private float _lastFloat;
        private bool _hasLastFloat;

        #endregion

        #region IRandomService

        public int Range( int inclusiveMin, int exclusiveMax, bool nonRepeating = false ) =>
          nonRepeating
            ? NonRepeatingInt( inclusiveMin, exclusiveMax )
            : (int)GD.RandRange( inclusiveMin, exclusiveMax - 1 );   // GD.RandRange is inclusive on both ends

        public float Range( float inclusiveMin, float inclusiveMax, bool nonRepeating = false ) =>
          nonRepeating
            ? NonRepeatingFloat( inclusiveMin, inclusiveMax )
            : (float)GD.RandRange( inclusiveMin, inclusiveMax );

        #endregion

        #region Non-repeating helpers

        private int NonRepeatingInt( int inclusiveMin, int exclusiveMax )
        {
            int count = exclusiveMax - inclusiveMin;
            if ( count <= 1 )
                return inclusiveMin;

            int value;
            do
            {
                value = (int)GD.RandRange( inclusiveMin, exclusiveMax - 1 );
            }
            while ( _hasLastInt && value == _lastInt );

            _lastInt = value;
            _hasLastInt = true;
            return value;
        }

        private float NonRepeatingFloat( float inclusiveMin, float inclusiveMax )
        {
            if ( inclusiveMin >= inclusiveMax )
                return inclusiveMin;

            float value;
            do
            {
                value = (float)GD.RandRange( inclusiveMin, inclusiveMax );
            }
            while ( _hasLastFloat && FMath.IsNearlyEqual( value, _lastFloat ) );

            _lastFloat = value;
            _hasLastFloat = true;
            return value;
        }

        #endregion
    }
}
