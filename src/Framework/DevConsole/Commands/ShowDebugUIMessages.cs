// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Console.Common;
using Framework.Console.Interfaces;
using Godot;

namespace Framework.Console.Commands
{
    public class ShowDebugUIMessages : IConsoleCommand
    {
        private readonly IDevConsole _console;
        private readonly Node _debugUINode;
        private bool _debugUIState;

        private string _usage;

        public string CommandName => "show_debug_ui_msg";
        public string Description => $"Show Debug UIMessages {_usage}";

        public ShowDebugUIMessages( IDevConsole console, Node debugUINode )
        {
            _console = console;
            _debugUINode = debugUINode;
        }

        public void Execute( string[] args )
        {
            if ( args.Length > 0 )
            {
                _console.AddMessage( $"Command error. {_usage}", Types.ConsoleMessageType.Warning );
                return;
            }

            _debugUINode.SetEnabled( _debugUIState );
            _debugUIState = !_debugUIState;
            _console.AddMessage( $"Set debug UI messages disbplay to {_debugUIState}" );
        }
    }
}
