// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace Setup.Engine.Common.Debug.UI
{
	public enum MessageColor
	{
		White,
		Red,
		Yellow,
		Green,
		Blue,
		Cyan,
		Orange
	}

	public interface IUIMessage
	{
		void Send( string message );
		void Send( string message, MessageColor color );
		void ClearMessages();
	}
}