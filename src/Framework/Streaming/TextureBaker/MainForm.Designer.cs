using System;
using System.Windows.Forms;

namespace TextureBaker
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _inputBox = new TextBox();
            _outputBox = new TextBox();
            _assetNameBox = new TextBox();
            _formatBox = new ComboBox();
            _updateManifestBox = new CheckBox();
            _manifestBox = new TextBox();
            _bakeButton = new Button();
            _browseInputButton = new Button();
            _browseOutputButton = new Button();
            _browseManifestButton = new Button();
            _log = new TextBox();
            SuspendLayout();
            //
            // _inputBox
            //
            _inputBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _inputBox.Location = new System.Drawing.Point( 120, 16 );
            _inputBox.Name = "_inputBox";
            _inputBox.Size = new System.Drawing.Size( 360, 31 );
            _inputBox.TabIndex = 0;
            //
            // _outputBox
            //
            _outputBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _outputBox.Location = new System.Drawing.Point( 120, 51 );
            _outputBox.Name = "_outputBox";
            _outputBox.Size = new System.Drawing.Size( 360, 31 );
            _outputBox.TabIndex = 1;
            //
            // _assetNameBox
            //
            _assetNameBox.Location = new System.Drawing.Point( 120, 86 );
            _assetNameBox.Name = "_assetNameBox";
            _assetNameBox.Size = new System.Drawing.Size( 200, 31 );
            _assetNameBox.TabIndex = 2;
            //
            // _formatBox
            //
            _formatBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _formatBox.Items.AddRange( new object[] { "Auto (detect alpha)", "BC1 (opaque)", "BC3 (alpha)" } );
            _formatBox.Location = new System.Drawing.Point( 120, 121 );
            _formatBox.Name = "_formatBox";
            _formatBox.SelectedIndex = 0;
            _formatBox.Size = new System.Drawing.Size( 180, 33 );
            _formatBox.TabIndex = 3;
            //
            // _updateManifestBox
            //
            _updateManifestBox.AutoSize = true;
            _updateManifestBox.Checked = true;
            _updateManifestBox.Location = new System.Drawing.Point( 120, 156 );
            _updateManifestBox.Name = "_updateManifestBox";
            _updateManifestBox.TabIndex = 4;
            _updateManifestBox.Text = "Update manifest";
            //
            // _manifestBox
            //
            _manifestBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _manifestBox.Location = new System.Drawing.Point( 120, 181 );
            _manifestBox.Name = "_manifestBox";
            _manifestBox.Size = new System.Drawing.Size( 360, 31 );
            _manifestBox.TabIndex = 5;
            //
            // _bakeButton
            //
            _bakeButton.Location = new System.Drawing.Point( 120, 216 );
            _bakeButton.Name = "_bakeButton";
            _bakeButton.Size = new System.Drawing.Size( 100, 23 );
            _bakeButton.TabIndex = 6;
            _bakeButton.Text = "Bake";
            _bakeButton.Click += RunBake;
            //
            // _browseInputButton
            //
            _browseInputButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _browseInputButton.Location = new System.Drawing.Point( 486, 15 );
            _browseInputButton.Name = "_browseInputButton";
            _browseInputButton.Size = new System.Drawing.Size( 36, 23 );
            _browseInputButton.TabIndex = 7;
            _browseInputButton.Text = "...";
            _browseInputButton.Click += BrowseInput;
            //
            // _browseOutputButton
            //
            _browseOutputButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _browseOutputButton.Location = new System.Drawing.Point( 486, 50 );
            _browseOutputButton.Name = "_browseOutputButton";
            _browseOutputButton.Size = new System.Drawing.Size( 36, 23 );
            _browseOutputButton.TabIndex = 8;
            _browseOutputButton.Text = "...";
            _browseOutputButton.Click += BrowseOutput;
            //
            // _browseManifestButton
            //
            _browseManifestButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _browseManifestButton.Location = new System.Drawing.Point( 486, 180 );
            _browseManifestButton.Name = "_browseManifestButton";
            _browseManifestButton.Size = new System.Drawing.Size( 36, 23 );
            _browseManifestButton.TabIndex = 9;
            _browseManifestButton.Text = "...";
            _browseManifestButton.Click += BrowseManifest;
            //
            // _log
            //
            _log.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _log.Location = new System.Drawing.Point( 15, 255 );
            _log.Multiline = true;
            _log.Name = "_log";
            _log.ReadOnly = true;
            _log.ScrollBars = ScrollBars.Vertical;
            _log.Size = new System.Drawing.Size( 501, 115 );
            _log.TabIndex = 10;
            //
            // sourceLabel
            //
            _sourceLabel = new Label();
            _sourceLabel.Text = "Source texture:";
            _sourceLabel.Left = 15;
            _sourceLabel.Top = 19;
            _sourceLabel.Width = 95;
            //
            // outputLabel
            //
            _outputLabel = new Label();
            _outputLabel.Text = "Output (.stream):";
            _outputLabel.Left = 15;
            _outputLabel.Top = 54;
            _outputLabel.Width = 95;
            //
            // assetNameLabel
            //
            _assetNameLabel = new Label();
            _assetNameLabel.Text = "Asset name:";
            _assetNameLabel.Left = 15;
            _assetNameLabel.Top = 89;
            _assetNameLabel.Width = 95;
            //
            // formatLabel
            //
            _formatLabel = new Label();
            _formatLabel.Text = "Format:";
            _formatLabel.Left = 15;
            _formatLabel.Top = 124;
            _formatLabel.Width = 95;
            //
            // manifestLabel
            //
            _manifestLabel = new Label();
            _manifestLabel.Text = "Manifest:";
            _manifestLabel.Left = 15;
            _manifestLabel.Top = 184;
            _manifestLabel.Width = 95;
            //
            // MainForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF( 7F, 15F );
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size( 531, 385 );
            Controls.Add( _sourceLabel );
            Controls.Add( _outputLabel );
            Controls.Add( _assetNameLabel );
            Controls.Add( _formatLabel );
            Controls.Add( _manifestLabel );
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
            MinimumSize = new System.Drawing.Size( 480, 340 );
            Name = "MainForm";
            Text = "Texture Baker";
            ResumeLayout( false );
            PerformLayout();
        }

        #endregion

        private TextBox _inputBox;
        private TextBox _outputBox;
        private TextBox _assetNameBox;
        private ComboBox _formatBox;
        private CheckBox _updateManifestBox;
        private TextBox _manifestBox;
        private Button _bakeButton;
        private Button _browseInputButton;
        private Button _browseOutputButton;
        private Button _browseManifestButton;
        private TextBox _log;
        private Label _sourceLabel;
        private Label _outputLabel;
        private Label _assetNameLabel;
        private Label _formatLabel;
        private Label _manifestLabel;
    }
}