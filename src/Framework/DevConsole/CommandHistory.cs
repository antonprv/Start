// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Framework.Console
{
    public class CommandHistory
    {
        private readonly int _capacity;
        private readonly List<string> _items = new();
        private int _index = -1;
        private string _cached = string.Empty;

        public CommandHistory( int capacity ) => _capacity = capacity;

        public void Add( string command )
        {
            _items.Add( command );
            if ( _items.Count > _capacity )
                _items.RemoveAt( 0 );
            _index = -1;
        }

        public string Up( string current )
        {
            if ( _items.Count == 0 ) return current;

            if ( _index == -1 )
            {
                _cached = current;
                _index = _items.Count - 1;
            }
            else if ( _index > 0 )
            {
                _index--;
            }

            return _items[ _index ];
        }

        public string Down()
        {
            if ( _index == -1 ) return _cached;

            _index++;

            if ( _index >= _items.Count )
            {
                _index = -1;
                return _cached;
            }

            return _items[ _index ];
        }
    }
}
