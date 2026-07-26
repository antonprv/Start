// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Framework.Common.Extensions
{
    public static class NodeExtensions
    {
        public static void SetEnabled( this Node node, bool value )
        {
            node.SetProcess( value );
            node.SetPhysicsProcess( value );
            node.SetProcessInput( value );
            node.SetProcessUnhandledInput( value );

            if ( node is Node3D node3D )
                node3D.Visible = value;
            else if ( node is Node2D node2D )
                node2D.Visible = value;
            else if ( node is Control control )
                control.Visible = value;
        }

        public static void EditorSafe( Action editorSafe )
        {
            if ( !Engine.IsEditorHint() )
                editorSafe.Invoke();
        }
    }
}