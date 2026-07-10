// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Console.Commands;
using Framework.Console.Interfaces;
using Framework.Console.Types;

namespace Framework.Console.Core
{
    public class DevConsoleService : IDevConsole
    {
        public bool IsOpen { get; private set; }
        public string Marker => "[Console] ";

        public event Action MessagesChanged;

        private readonly Dictionary<string, IConsoleCommand> _commands = new();
        private readonly List<ConsoleMessage> _messages = new();

        private ConsoleMessageType _filter = ConsoleMessageType.All;
        private const int MaxMessages = 500;
        private bool _initialized;

        #region Initialization

        public void Initialize()
        {
            if ( _initialized ) return;
            _initialized = true;

            RegisterCommand( new HelpCommand( this, _commands ) );
            RegisterCommand( new ClearCommand( this ) );

            AddMessage( "Developer console initialized. Type 'help' for commands." );
        }

        #endregion

        #region IDevConsole

        public void Toggle() => IsOpen = !IsOpen;

        public void RegisterCommand( IConsoleCommand command )
        {
            string name = command.CommandName.ToLowerInvariant();

            if ( _commands.ContainsKey( name ) )
            {
                AddMessage( $"Command '{name}' is already registered.", ConsoleMessageType.Warning );
                return;
            }

            _commands[ name ] = command;
        }

        public void ExecuteCommand( string commandLine )
        {
            if ( string.IsNullOrWhiteSpace( commandLine ) ) return;

            AddMessage( commandLine, ConsoleMessageType.Command );

            string[] parts = commandLine.Trim().Split( ' ', StringSplitOptions.RemoveEmptyEntries );
            string name = parts[ 0 ].ToLowerInvariant();
            string[] args = parts.Skip( 1 ).ToArray();

            if ( _commands.TryGetValue( name, out var cmd ) )
                cmd.Execute( args );
            else
            {
                AddMessage( $"Unknown command: '{name}'", ConsoleMessageType.Error );
                AddMessage( "Type 'help' to see available commands." );
            }
        }

        public void AddMessage( string message, ConsoleMessageType type = ConsoleMessageType.Info )
        {
            _messages.Add( new ConsoleMessage( message, type, Marker ) );

            if ( _messages.Count > MaxMessages )
                _messages.RemoveRange( 0, _messages.Count - MaxMessages );

            MessagesChanged?.Invoke();
        }

        public string[] GetMessages() =>
            _messages
                .Where( m => ShouldShow( m.Type ) )
                .Select( m => m.Formatted )
                .ToArray();

        public void ClearMessages()
        {
            _messages.Clear();
            AddMessage( "Console cleared." );
        }

        public void SetFilter( ConsoleMessageType filter )
        {
            _filter = filter;
            AddMessage( $"Filter: {filter}", ConsoleMessageType.Success );
        }

        public ConsoleMessageType GetFilter() => _filter;

        #endregion

        #region private API

        private bool ShouldShow( ConsoleMessageType type )
        {
            if ( type == ConsoleMessageType.Command ) return true;
            if ( _filter == ConsoleMessageType.All ) return true;
            return type == _filter;
        }

        #endregion
    }
}
