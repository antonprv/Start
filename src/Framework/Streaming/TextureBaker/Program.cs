// Created by Anton Piruev in 2026.
// Texture Baker is a sandalone utility released under MIT license

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
            bool appMode = args.Length > 0 && 
                string.Equals( args[ 0 ], "app", StringComparison.OrdinalIgnoreCase );

            if ( !appMode )
                return CliBaker.Run( args );
            
            FreeConsole();

            ApplicationConfiguration.Initialize();
            Application.Run( new MainForm() );
            return 0;
        }
    }
}
