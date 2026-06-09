// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Console;
using Console.Commands;
using Console.Core;
using Game.Code.Services.Input;
using Godot;
using Zenjex;

namespace Game.Code.Common.DevConsole
{
	public partial class DevConsoleNode : CanvasLayer
	{
		[Export] public int MaxHistory = 50;

		[Export] public Panel RootPanel;
		[Export] public RichTextLabel OutputLabel;
		[Export] public LineEdit InputField;
		[Export] public Button SubmitButton;

		public DevConsoleService Service { get; } = new();

		private CommandHistory _history;
		private IInputService _inputService;

		[Inject]
		private void Construct( IInputService inputService )
		{
			_inputService = inputService;
		}

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		public override void _Ready()
		{
			_history = new CommandHistory( MaxHistory );

			if ( RootPanel == null )
				BuildScene();

			Service.Initialize();
			RegisterDefaultCommands();

			Service.MessagesChanged += RefreshOutput;

			SubmitButton?.Connect( BaseButton.SignalName.Pressed, Callable.From( Submit ) );
			InputField?.Connect( LineEdit.SignalName.TextSubmitted, Callable.From<string>( _ => Submit() ) );

			SetConsoleVisible( false );
		}

		public override void _ExitTree() =>
			Service.MessagesChanged -= RefreshOutput;

		public override void _Input( InputEvent e )
		{
			if ( _inputService.IsConsolePressed() )
			{
				Service.Toggle();
				SetConsoleVisible( Service.IsOpen );
				GetViewport().SetInputAsHandled();
				return;
			}

			if ( !Service.IsOpen ) return;

			if ( e is InputEventKey { Pressed: true } key )
			{
				switch ( key.Keycode )
				{
				case Key.Up:
					InputField!.Text = _history.Up( InputField.Text );
					InputField.CaretColumn = InputField.Text.Length;
					GetViewport().SetInputAsHandled();
					break;

				case Key.Down:
					InputField!.Text = _history.Down();
					InputField.CaretColumn = InputField.Text.Length;
					GetViewport().SetInputAsHandled();
					break;
				}
			}
		}

		private void Submit()
		{
			string cmd = InputField?.Text.Trim() ?? string.Empty;
			if ( string.IsNullOrWhiteSpace( cmd ) ) return;

			Service.ExecuteCommand( cmd );
			_history.Add( cmd );

			if ( InputField != null )
				InputField.Text = string.Empty;

			ScrollToBottom();
			InputField?.GrabFocus();
		}

		private void SetConsoleVisible( bool visible )
		{
			RootPanel?.SetVisible( visible );

			if ( visible )
			{
				RefreshOutput();
				ScrollToBottom();
				InputField?.GrabFocus();
				if ( InputField != null )
					InputField.Text = string.Empty;
			}
		}

		private void RefreshOutput()
		{
			if ( OutputLabel == null ) return;

			OutputLabel.Text = string.Join( "\n", Service.GetMessages() );
		}

		private void ScrollToBottom()
		{
			if ( OutputLabel == null ) return;
			OutputLabel.CallDeferred(
				RichTextLabel.MethodName.ScrollToLine,
				OutputLabel.GetLineCount() - 1 );
		}

		private void RegisterDefaultCommands()
		{
			Service.RegisterCommand( new FilterCommand( Service ) );
			Service.RegisterCommand( new SetFpsCommand( Service ) );
			Service.RegisterCommand( new StatFpsCommand( Service ) );
			Service.RegisterCommand( new ExportLogsCommand( Service ) );
		}

		private void BuildScene()
		{
			Layer = 100;

			RootPanel = new Panel();
			RootPanel.AnchorTop = 0f;
			RootPanel.AnchorBottom = 0.4f;
			RootPanel.AnchorLeft = 0f;
			RootPanel.AnchorRight = 1f;
			RootPanel.AddThemeStyleboxOverride( "panel", new StyleBoxFlat
			{
				BgColor = new Color( 0f, 0f, 0f, 0.88f ),
				CornerRadiusBottomLeft = 4,
				CornerRadiusBottomRight = 4
			} );
			AddChild( RootPanel );

			var vbox = new VBoxContainer();
			vbox.SetAnchorsPreset( Control.LayoutPreset.FullRect );
			vbox.AddThemeConstantOverride( "separation", 4 );
			RootPanel.AddChild( vbox );

			var header = new Label { Text = $"Developer Console  (press {InputNames.CallConsole} to close)" };
			header.AddThemeColorOverride( "font_color", Colors.White );
			vbox.AddChild( header );

			OutputLabel = new RichTextLabel
			{
				BbcodeEnabled = true,
				ScrollFollowing = true,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			OutputLabel.AddThemeColorOverride( "default_color", Colors.White );
			OutputLabel.AddThemeFontSizeOverride( "normal_font_size", 15 );
			vbox.AddChild( OutputLabel );

			var hbox = new HBoxContainer();
			vbox.AddChild( hbox );

			InputField = new LineEdit
			{
				PlaceholderText = "Enter command…",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				CustomMinimumSize = new Vector2( 0, 32 )
			};
			InputField.AddThemeFontSizeOverride( "font_size", 15 );
			hbox.AddChild( InputField );

			SubmitButton = new Button
			{
				Text = "Submit",
				CustomMinimumSize = new Vector2( 80, 32 )
			};
			hbox.AddChild( SubmitButton );
		}
	}
}
