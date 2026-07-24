// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TextureBaker
{
    /// <summary>
    /// Quick single-texture baking for editor iteration: pick a source, pick (or accept the
    /// auto-suggested) output path inside the Godot project, bake, then reference the asset by
    /// name from a StreamableTexture2D to test in-game straight away - "Update manifest" (on by
    /// default) upserts just this one entry into the same manifest file the CLI's batch mode
    /// produces, so a texture baked here for a quick test is immediately resolvable by name
    /// without hand-editing anything. Not meant to replace the CLI's batch mode for a full
    /// project bake - see CliBaker for that.
    /// </summary>
    public sealed class MainForm : Form
    {
        private readonly TextBox _inputBox = new TextBox { Left = 120, Top = 16, Width = 360, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        private readonly TextBox _outputBox = new TextBox { Left = 120, Top = 51, Width = 360, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        private readonly TextBox _assetNameBox = new TextBox { Left = 120, Top = 86, Width = 200 };
        private readonly ComboBox _formatBox = new ComboBox { Left = 120, Top = 121, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox _updateManifestBox = new CheckBox { Text = "Update manifest", Left = 120, Top = 156, Width = 160, Checked = true };
        private readonly TextBox _manifestBox = new TextBox { Left = 120, Top = 181, Width = 360, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        private readonly Button _bakeButton = new Button { Text = "Bake", Left = 120, Top = 216, Width = 100 };
        private readonly Button _browseInputButton = new Button { Text = "...", Left = 486, Top = 15, Width = 30, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        private readonly Button _browseOutputButton = new Button { Text = "...", Left = 486, Top = 50, Width = 30, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        private readonly Button _browseManifestButton = new Button { Text = "...", Left = 486, Top = 180, Width = 30, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        private readonly TextBox _log = new TextBox
        {
            Left = 15, Top = 255, Width = 501, Height = 115,
            Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };

        public MainForm()
        {
            Text = "Texture Baker";
            ClientSize = new System.Drawing.Size( 531, 385 );
            MinimumSize = new System.Drawing.Size( 480, 340 );

            Controls.Add( new Label { Text = "Source texture:", Left = 15, Top = 19, Width = 95 } );
            Controls.Add( new Label { Text = "Output (.stream):", Left = 15, Top = 54, Width = 95 } );
            Controls.Add( new Label { Text = "Asset name:", Left = 15, Top = 89, Width = 95 } );
            Controls.Add( new Label { Text = "Format:", Left = 15, Top = 124, Width = 95 } );
            Controls.Add( new Label { Text = "Manifest:", Left = 15, Top = 184, Width = 95 } );

            _formatBox.Items.AddRange( new object[] { "Auto (detect alpha)", "BC1 (opaque)", "BC3 (alpha)" } );
            _formatBox.SelectedIndex = 0;

            _browseInputButton.Click += ( _, _ ) => BrowseInput();
            _browseOutputButton.Click += ( _, _ ) => BrowseOutput();
            _browseManifestButton.Click += ( _, _ ) => BrowseManifest();
            _bakeButton.Click += ( _, _ ) => RunBake();

            Controls.Add( _inputBox );
            Controls.Add( _outputBox );
            Controls.Add( _assetNameBox );
            Controls.Add( _formatBox );
            Controls.Add( _updateManifestBox );
            Controls.Add( _manifestBox );
            Controls.Add( _bakeButton );
            Controls.Add( _browseInputButton );
            Controls.Add( _browseOutputButton );
            Controls.Add( _browseManifestButton );
            Controls.Add( _log );
        }

        private void BrowseInput()
        {
            using var dialog = new OpenFileDialog { Filter = "Textures|*.png;*.jpg;*.jpeg;*.tga;*.bmp|All files|*.*" };
            if ( dialog.ShowDialog( this ) != DialogResult.OK )
                return;

            _inputBox.Text = dialog.FileName;

            if ( string.IsNullOrWhiteSpace( _assetNameBox.Text ) )
                _assetNameBox.Text = Path.GetFileNameWithoutExtension( dialog.FileName );

            if ( string.IsNullOrWhiteSpace( _outputBox.Text ) )
            {
                string suggested = Path.ChangeExtension( dialog.FileName, ".stream" );
                _outputBox.Text = suggested;
                SuggestManifestPath( suggested );
            }
        }

        private void BrowseOutput()
        {
            using var dialog = new SaveFileDialog { Filter = "Streamed texture|*.stream", FileName = _outputBox.Text };
            if ( dialog.ShowDialog( this ) != DialogResult.OK )
                return;

            _outputBox.Text = dialog.FileName;
            SuggestManifestPath( dialog.FileName );
        }

        private void BrowseManifest()
        {
            using var dialog = new SaveFileDialog { Filter = "Manifest|*.bin", FileName = _manifestBox.Text };
            if ( dialog.ShowDialog( this ) == DialogResult.OK )
                _manifestBox.Text = dialog.FileName;
        }

        private void SuggestManifestPath( string outputPath )
        {
            if ( !string.IsNullOrWhiteSpace( _manifestBox.Text ) )
                return;

            string? directory = Path.GetDirectoryName( outputPath );
            if ( !string.IsNullOrEmpty( directory ) )
                _manifestBox.Text = Path.Combine( directory, "manifest.bin" );
        }

        private void RunBake()
        {
            if ( string.IsNullOrWhiteSpace( _inputBox.Text ) || string.IsNullOrWhiteSpace( _outputBox.Text ) )
            {
                Log( "Choose both a source texture and an output path first." );
                return;
            }

            if ( _updateManifestBox.Checked && string.IsNullOrWhiteSpace( _assetNameBox.Text ) )
            {
                Log( "Asset name is required to update the manifest - fill it in or untick \"Update manifest\"." );
                return;
            }

            BakeFormat format = _formatBox.SelectedIndex switch
            {
                1 => BakeFormat.Bc1,
                2 => BakeFormat.Bc3,
                _ => BakeFormat.Auto,
            };

            _bakeButton.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                // Synchronous on purpose: this tool bakes one texture at a time for quick
                // iteration, not whole-project batches (that's the CLI's job) - a moment's
                // UI freeze on a single image is an acceptable trade for not needing a
                // background-thread/Invoke dance in a small utility form.
                BakeResult result = TextureBakerCore.Bake( _inputBox.Text, _outputBox.Text, format );
                Log( $"OK: {result.Format}, {result.MipCount} mips, {result.TotalBytes} bytes -> {result.OutputPath}" );

                if ( _updateManifestBox.Checked )
                    UpdateManifest( result.OutputPath );
            }
            catch ( Exception ex )
            {
                Log( $"FAIL: {ex.Message}" );
            }
            finally
            {
                Cursor = Cursors.Default;
                _bakeButton.Enabled = true;
            }
        }

        private void UpdateManifest( string outputPath )
        {
            if ( string.IsNullOrWhiteSpace( _manifestBox.Text ) )
            {
                Log( "No manifest path set - skipped updating it (texture still baked fine, just not resolvable by name yet)." );
                return;
            }

            if ( !ManifestTool.TryToResPath( outputPath, out string resPath ) )
            {
                Log( $"Couldn't find a project.godot above '{outputPath}' - can't derive a res:// path automatically, so the manifest wasn't updated. Bake somewhere inside the Godot project, or add the entry with the CLI instead." );
                return;
            }

            string assetName = _assetNameBox.Text.Trim();
            Dictionary<string, string> entries = ManifestTool.LoadOrEmpty( _manifestBox.Text );

            if ( entries.TryGetValue( assetName, out string? existing ) && existing != resPath )
                Log( $"Note: '{assetName}' already pointed at {existing} - overwriting with {resPath}." );

            entries[ assetName ] = resPath;
            ManifestTool.Save( _manifestBox.Text, entries );
            Log( $"Manifest: '{assetName}' -> {resPath} ({entries.Count} total entries in {_manifestBox.Text})" );
        }

        private void Log( string message ) => _log.AppendText( message + Environment.NewLine );
    }
}
