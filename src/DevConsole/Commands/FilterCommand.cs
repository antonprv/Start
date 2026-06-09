// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Console.Interfaces;
using Console.Types;

namespace Console.Commands
{
    public class FilterCommand : IConsoleCommand
    {
        private readonly IDevConsole _console;

        public string CommandName => "filter";
        public string Description => "Filter output. Usage: filter <log|warning|error|success|all>";

        public FilterCommand( IDevConsole console ) => _console = console;

        public void Execute( string[] args )
        {
            if ( args.Length < 1 )
            {
                _console.AddMessage( Description, ConsoleMessageType.Warning );
                _console.AddMessage( $"Current: {_console.GetFilter()}" );
                return;
            }

            var filter = args[0].ToLowerInvariant() switch
            {
                "log"     => ConsoleMessageType.Log,
                "warning" => ConsoleMessageType.Warning,
                "error"   => ConsoleMessageType.Error,
                "success" => ConsoleMessageType.Success,
                "all"     => ConsoleMessageType.All,
                _         => (ConsoleMessageType)(-1)
            };

            if ( (int)filter == -1 )
            {
                _console.AddMessage( $"Unknown filter: '{args[0]}'", ConsoleMessageType.Error );
                return;
            }

            _console.SetFilter( filter );
        }
    }
}
