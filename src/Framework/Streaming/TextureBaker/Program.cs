// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TextureBaker
{
    internal static class Program
    {
        [DllImport( "kernel32.dll" )]
        private static extern bool FreeConsole();

        [STAThread]
        private static int Main( string[] args )
        {
            bool appMode = args.Length > 0 && string.Equals( args[ 0 ], "app", StringComparison.OrdinalIgnoreCase );

            if ( !appMode )
                return CliBaker.Run( args );

            // The project is built with the console subsystem (OutputType=Exe in the csproj) on
            // purpose - a WinExe-subsystem build's stdout goes missing when invoked from an
            // existing terminal, which would break the CLI path from CI. That means launching
            // GUI mode still gets handed a console window; detach it here, before showing any
            // WinForms UI, so double-clicking a shortcut with the "app" argument never leaves a
            // blank console flashing behind the form.
            FreeConsole();

            ApplicationConfiguration.Initialize();
            Application.Run( new MainForm() );
            return 0;
        }
    }
}
