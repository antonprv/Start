// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Common.Debug.ConsoleCommands;
using Engine.Common.Debug.UI;
using Engine.Components.Mover;
using Engine.Services.Input;
using Framework.Common.Extensions;
using Framework.Common.Time;
using Framework.Console;
using Framework.Console.Commands;
using Framework.Console.Core;
using Game.Code.Common.Debug.UI;
using Godot;
using Zenjex;

namespace Game.Code.Common.DevConsole
{
	public partial class DevConsoleNode : CanvasLayer
	{
		[ExportGroup( "References" )]
		[Export] private FpsTracker _fpsTrackerNode;
		[Export] private UIMessage _uiMessageNode;

		[ExportGroup( "Console parameters" )]
		[Export] public int MaxHistory = 50;

		private Panel _rootPanel;
		private RichTextLabel _outputLabel;
		private LineEdit _inputField;
		private Button _submitButton;

		public DevConsoleService Service { get; } = new();

		private CommandHistory _history;

		private IInputService _inputService;
		private ITimeService _timeService;
		private IMoverComponent _moverComponent;

		[Inject]
		private void Construct(
			IInputService inputService,
			ITimeService timeService,
			IMoverComponent moverComponent
		)
		{
			_inputService = inputService;
			_timeService = timeService;
			_moverComponent = moverComponent;
		}

		public override void _EnterTree()
		{
			this.DestroyIfNotDebug();

			DiContainer.Instance.Inject( this );
		}

		public override void _Ready()
		{
			_history = new CommandHistory( MaxHistory );

			if ( _rootPanel == null )
				BuildScene();

			Service.Initialize();
			RegisterDefaultCommands();

			Service.MessagesChanged += RefreshOutput;

			_submitButton?.Connect( BaseButton.SignalName.Pressed, Callable.From( Submit ) );
			_inputField?.Connect( LineEdit.SignalName.TextSubmitted, Callable.From<string>( _ => Submit() ) );

			SetConsoleVisible( false );
		}

		public override void _ExitTree()
		{
			Service.MessagesChanged -= RefreshOutput;
		}

		public override void _Input( InputEvent e )
		{
			if ( _inputService.IsConsolePressed() )
			{
				Service.Toggle();
				SetConsoleVisible( Service.IsOpen );
				GetViewport().SetInputAsHandled();

				_inputService.CapturePlayerInput = !Service.IsOpen;

				if ( Service.IsOpen )
					_timeService.StopTimeGlobal();
				else
					_timeService.StartTimeGlobal();

				return;
			}

			if ( !Service.IsOpen ) return;

			if ( e is InputEventKey { Pressed: true } key )
			{
				switch ( key.Keycode )
				{
				case Key.Up:
					_inputField!.Text = _history.Up( _inputField.Text );
					_inputField.CaretColumn = _inputField.Text.Length;
					GetViewport().SetInputAsHandled();
					break;

				case Key.Down:
					_inputField!.Text = _history.Down();
					_inputField.CaretColumn = _inputField.Text.Length;
					GetViewport().SetInputAsHandled();
					break;
				}
			}
		}

		#region Commands Registration

		private void RegisterDefaultCommands()
		{
			Service.RegisterCommand( new FilterCommand( Service ) );
			Service.RegisterCommand( new SetFpsCommand( Service ) );
			Service.RegisterCommand( new ToggleVsyncCommand( Service ) );
			Service.RegisterCommand( new StatFpsCommand( Service, _fpsTrackerNode ) );
			Service.RegisterCommand( new ShowDebugUIMessages( Service, _uiMessageNode ) );
			Service.RegisterCommand( new ExportLogsCommand( Service ) );

			Service.RegisterCommand( new NoclipCommand( Service, _moverComponent ) );
		}

		#endregion

		private void Submit()
		{
			string cmd = _inputField?.Text.Trim() ?? string.Empty;
			if ( string.IsNullOrWhiteSpace( cmd ) ) return;

			Service.ExecuteCommand( cmd );
			_history.Add( cmd );

			if ( _inputField != null )
				_inputField.Text = string.Empty;

			ScrollToBottom();
			_inputField?.CallDeferred( Control.MethodName.GrabFocus );
		}

		private void SetConsoleVisible( bool visible )
		{
			_rootPanel?.SetVisible( visible );

			if ( visible )
			{
				RefreshOutput();
				ScrollToBottom();
				_inputField?.GrabFocus();
				if ( _inputField != null )
					_inputField.Text = string.Empty;
			}
		}

		private void RefreshOutput()
		{
			if ( _outputLabel == null ) return;

			_outputLabel.Text = string.Join( "\n", Service.GetMessages() );
		}

		private void ScrollToBottom()
		{
			if ( _outputLabel == null ) return;
			_outputLabel.CallDeferred(
				RichTextLabel.MethodName.ScrollToLine,
				_outputLabel.GetLineCount() - 1 );
		}

		private void BuildScene()
		{
			Layer = 100;

			_rootPanel = new Panel();
			_rootPanel.AnchorTop = 0f;
			_rootPanel.AnchorBottom = 0.4f;
			_rootPanel.AnchorLeft = 0f;
			_rootPanel.AnchorRight = 1f;
			_rootPanel.AddThemeStyleboxOverride( "panel", new StyleBoxFlat
			{
				BgColor = new Color( 0f, 0f, 0f, 0.88f ),
				CornerRadiusBottomLeft = 4,
				CornerRadiusBottomRight = 4
			} );
			AddChild( _rootPanel );

			var vbox = new VBoxContainer();
			vbox.SetAnchorsPreset( Control.LayoutPreset.FullRect );
			vbox.AddThemeConstantOverride( "separation", 4 );
			_rootPanel.AddChild( vbox );

			var header = new Label { Text = $"Developer Console  (press {InputNames.CallConsole} to close)" };
			header.AddThemeColorOverride( "font_color", Colors.White );
			vbox.AddChild( header );

			_outputLabel = new RichTextLabel
			{
				BbcodeEnabled = true,
				ScrollFollowing = true,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};

			_outputLabel.AddThemeColorOverride( "default_color", Colors.White );
			_outputLabel.AddThemeFontSizeOverride( "normal_font_size", 15 );
			vbox.AddChild( _outputLabel );

			var hbox = new HBoxContainer();
			vbox.AddChild( hbox );

			_inputField = new LineEdit
			{
				PlaceholderText = "Enter command…",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				CustomMinimumSize = new Vector2( 0, 32 ),
				KeepEditingOnTextSubmit = true
			};
			_inputField.AddThemeFontSizeOverride( "font_size", 15 );
			hbox.AddChild( _inputField );

			_submitButton = new Button
			{
				Text = "Submit",
				CustomMinimumSize = new Vector2( 80, 32 )
			};
			hbox.AddChild( _submitButton );
		}
	}
}
