// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Console.Interfaces;
using Console.Types;
using Godot;

namespace Console.Commands
{
    public class ToggleVsyncCommand : IConsoleCommand
    {
        private readonly IDevConsole _console;

        public string CommandName => "toggle_vsync";
        public string Description => "Toggle VSync mode. Usage: toggle_vsync [disabled|enabled|adaptive|mailbox]";

        public ToggleVsyncCommand( IDevConsole console ) => _console = console;

        public void Execute( string[] args )
        {
            if ( args.Length < 1 )
            {
                // Toggle between Enabled and Disabled if no argument provided
                DisplayServer.VSyncMode current = DisplayServer.WindowGetVsyncMode();

                if ( current == DisplayServer.VSyncMode.Disabled )
                {
                    DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Enabled );
                    _console.AddMessage( "VSync: Enabled", ConsoleMessageType.Success );
                }
                else
                {
                    DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Disabled );
                    _console.AddMessage( "VSync: Disabled", ConsoleMessageType.Success );
                }

                return;
            }

            string arg = args[ 0 ].ToLower();

            switch ( arg )
            {
                case "disabled":
                case "off":
                case "0":
                    DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Disabled );
                    _console.AddMessage( "VSync: Disabled", ConsoleMessageType.Success );
                    break;

                case "enabled":
                case "on":
                case "1":
                    DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Enabled );
                    _console.AddMessage( "VSync: Enabled", ConsoleMessageType.Success );
                    break;

                case "adaptive":
                case "2":
                    DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Adaptive );
                    _console.AddMessage( "VSync: Adaptive", ConsoleMessageType.Success );
                    break;

                case "mailbox":
                case "3":
                    DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Mailbox );
                    _console.AddMessage( "VSync: Mailbox", ConsoleMessageType.Success );
                    break;

                default:
                    _console.AddMessage(
                        $"Invalid value: '{args[ 0 ]}'. Use: disabled, enabled, adaptive, or mailbox.",
                        ConsoleMessageType.Error
                    );
                    break;
            }
        }
    }
}