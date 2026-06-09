// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Console.Interfaces;
using Console.Types;
using Godot;

namespace Console.Commands
{
    public class StatFpsCommand : IConsoleCommand
    {
        private readonly IDevConsole _console;

        public string CommandName => "stat_fps";
        public string Description => "Print current FPS. Usage: stat_fps";

        public StatFpsCommand( IDevConsole console ) => _console = console;

        public void Execute( string[] args ) =>
            _console.AddMessage( $"FPS: {Engine.GetFramesPerSecond():F1}", ConsoleMessageType.Success );
    }
}
