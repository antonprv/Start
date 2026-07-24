using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextureBaker
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void BrowseInput( object sender, EventArgs e )
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

        private void BrowseOutput( object sender, EventArgs e )
        {
            using var dialog = new SaveFileDialog { Filter = "Streamed texture|*.stream", FileName = _outputBox.Text };
            if ( dialog.ShowDialog( this ) != DialogResult.OK )
                return;

            _outputBox.Text = dialog.FileName;
            SuggestManifestPath( dialog.FileName );
        }

        private void BrowseManifest( object sender, EventArgs e )
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

        private void RunBake( object sender, EventArgs e )
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
