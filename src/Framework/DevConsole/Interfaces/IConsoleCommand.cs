// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Framework.Console.Interfaces
{
    public interface IConsoleCommand
    {
        string CommandName  { get; }
        string Description  { get; }
        void   Execute( string[] args );
    }
}
