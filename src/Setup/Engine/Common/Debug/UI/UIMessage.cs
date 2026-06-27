// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Common.Extensions;

using Godot;
using Setup.Engine.Common.Debug.UI;
using System.Collections.Generic;

namespace Game.Code.Common.Debug.UI
{
	public partial class UIMessage : RichTextLabel, IUIMessage
	{
		#region Serialized Fields

		[Export] public int MaxMessages = 50;
		[Export] public float MessageLifetime = 10f;

		#endregion

		#region ConditionalExpression

		public override void _EnterTree() => this.DestroyIfNotDebug();

		#endregion

		#region Interface Implementation

		public void Send( string message )
		{
			Send( message, MessageColor.White );
		}

		public void Send( string message, MessageColor color )
		{
			if ( _messages.Count >= MaxMessages )
			{
				RemoveOldestMessage();
			}

			var entry = new MessageEntry
			{
				Text = message,
				Color = color
			};

			entry.Timer = GetTree().CreateTimer( MessageLifetime );
			entry.Timer.Timeout += () => RemoveMessage( entry );

			_messages.AddLast( entry );
			RebuildText();
		}

		public void ClearMessages()
		{
			foreach ( var msg in _messages )
			{
				if ( msg.Timer != null && IsInstanceValid( msg.Timer ) )
					msg.Timer.Free();
			}

			_messages.Clear();
			Text = string.Empty;
		}

		#endregion

		#region Private

		private readonly LinkedList<MessageEntry> _messages = new();

		private class MessageEntry
		{
			public string Text;
			public MessageColor Color;
			public SceneTreeTimer Timer;
		}

		public override void _Ready()
		{
			BbcodeEnabled = true;
			ScrollFollowing = true;
			AutowrapMode = TextServer.AutowrapMode.WordSmart;
		}

		private void RemoveMessage( MessageEntry entry )
		{
			_messages.Remove( entry );
			RebuildText();
		}

		private void RemoveOldestMessage()
		{
			var oldest = _messages.First;
			if ( oldest == null ) return;

			if ( oldest.Value.Timer != null && IsInstanceValid( oldest.Value.Timer ) )
				oldest.Value.Timer.Free();

			_messages.RemoveFirst();
		}

		private void RebuildText()
		{
			Text = string.Empty;

			foreach ( var msg in _messages )
			{
				AppendText( $"[color={GetColorName( msg.Color )}]{msg.Text}[/color]\n" );
			}
		}

		private static string GetColorName( MessageColor color ) => color switch
		{
			MessageColor.Red => "red",
			MessageColor.Yellow => "yellow",
			MessageColor.Green => "green",
			MessageColor.Blue => "blue",
			MessageColor.Cyan => "cyan",
			MessageColor.Orange => "orange",
			_ => "white"
		};

		#endregion
	}
}