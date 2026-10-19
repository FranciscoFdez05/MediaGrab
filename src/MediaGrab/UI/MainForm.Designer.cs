namespace MediaGrab.UI
{
    partial class MainForm
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                downloadCancellation?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCheckYtDlp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuUpdateYtDlp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSupportedSites = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProjectPage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.menuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.labelUrl = new System.Windows.Forms.Label();
            this.textBoxUrl = new System.Windows.Forms.TextBox();
            this.buttonPaste = new System.Windows.Forms.Button();
            this.labelFolder = new System.Windows.Forms.Label();
            this.textBoxFolder = new System.Windows.Forms.TextBox();
            this.buttonBrowseFolder = new System.Windows.Forms.Button();
            this.optionsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.labelFormat = new System.Windows.Forms.Label();
            this.radioButtonMp4 = new System.Windows.Forms.RadioButton();
            this.radioButtonMp3 = new System.Windows.Forms.RadioButton();
            this.labelQuality = new System.Windows.Forms.Label();
            this.comboBoxQuality = new System.Windows.Forms.ComboBox();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.resultLayout = new System.Windows.Forms.TableLayoutPanel();
            this.labelResult = new System.Windows.Forms.Label();
            this.linkResultAction = new System.Windows.Forms.LinkLabel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusYtDlp = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip.SuspendLayout();
            this.layout.SuspendLayout();
            this.optionsLayout.SuspendLayout();
            this.resultLayout.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip
            //
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuTools,
            this.menuHelp});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(600, 24);
            this.menuStrip.TabIndex = 0;
            //
            // menuFile
            //
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuOpenFolder,
            this.menuFileSeparator,
            this.menuExit});
            this.menuFile.Name = "menuFile";
            this.menuFile.Text = "&Archivo";
            //
            // menuOpenFolder
            //
            this.menuOpenFolder.Name = "menuOpenFolder";
            this.menuOpenFolder.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.menuOpenFolder.Text = "Abrir carpeta de &descargas";
            this.menuOpenFolder.Click += new System.EventHandler(this.MenuOpenFolder_Click);
            //
            // menuFileSeparator
            //
            this.menuFileSeparator.Name = "menuFileSeparator";
            //
            // menuExit
            //
            this.menuExit.Name = "menuExit";
            this.menuExit.ShortcutKeyDisplayString = "Alt+F4";
            this.menuExit.Text = "&Salir";
            this.menuExit.Click += new System.EventHandler(this.MenuExit_Click);
            //
            // menuTools
            //
            this.menuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuCheckYtDlp,
            this.menuUpdateYtDlp});
            this.menuTools.Name = "menuTools";
            this.menuTools.Text = "&Herramientas";
            //
            // menuCheckYtDlp
            //
            this.menuCheckYtDlp.Name = "menuCheckYtDlp";
            this.menuCheckYtDlp.Text = "&Comprobar yt-dlp";
            this.menuCheckYtDlp.Click += new System.EventHandler(this.MenuCheckYtDlp_Click);
            //
            // menuUpdateYtDlp
            //
            this.menuUpdateYtDlp.Name = "menuUpdateYtDlp";
            this.menuUpdateYtDlp.Text = "&Actualizar yt-dlp";
            this.menuUpdateYtDlp.Click += new System.EventHandler(this.MenuUpdateYtDlp_Click);
            //
            // menuHelp
            //
            this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuSupportedSites,
            this.menuProjectPage,
            this.menuHelpSeparator,
            this.menuAbout});
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.Text = "A&yuda";
            //
            // menuSupportedSites
            //
            this.menuSupportedSites.Name = "menuSupportedSites";
            this.menuSupportedSites.Text = "&Sitios compatibles";
            this.menuSupportedSites.Click += new System.EventHandler(this.MenuSupportedSites_Click);
            //
            // menuProjectPage
            //
            this.menuProjectPage.Name = "menuProjectPage";
            this.menuProjectPage.Text = "&Página del proyecto";
            this.menuProjectPage.Click += new System.EventHandler(this.MenuProjectPage_Click);
            //
            // menuHelpSeparator
            //
            this.menuHelpSeparator.Name = "menuHelpSeparator";
            //
            // menuAbout
            //
            this.menuAbout.Name = "menuAbout";
            this.menuAbout.Text = "&Acerca de MediaGrab";
            this.menuAbout.Click += new System.EventHandler(this.MenuAbout_Click);
            //
            // layout
            //
            this.layout.AutoSize = true;
            this.layout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.layout.ColumnCount = 2;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.layout.Controls.Add(this.labelUrl, 0, 0);
            this.layout.Controls.Add(this.textBoxUrl, 0, 1);
            this.layout.Controls.Add(this.buttonPaste, 1, 1);
            this.layout.Controls.Add(this.labelFolder, 0, 2);
            this.layout.Controls.Add(this.textBoxFolder, 0, 3);
            this.layout.Controls.Add(this.buttonBrowseFolder, 1, 3);
            this.layout.Controls.Add(this.optionsLayout, 0, 4);
            this.layout.Controls.Add(this.buttonDownload, 0, 5);
            this.layout.Controls.Add(this.progressBar, 0, 6);
            this.layout.Controls.Add(this.resultLayout, 0, 7);
            this.layout.Dock = System.Windows.Forms.DockStyle.Top;
            this.layout.Location = new System.Drawing.Point(0, 24);
            this.layout.Name = "layout";
            this.layout.Padding = new System.Windows.Forms.Padding(13, 12, 13, 8);
            this.layout.RowCount = 8;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.Size = new System.Drawing.Size(600, 280);
            this.layout.TabIndex = 1;
            //
            // labelUrl
            //
            this.labelUrl.AutoSize = true;
            this.layout.SetColumnSpan(this.labelUrl, 2);
            this.labelUrl.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.labelUrl.Location = new System.Drawing.Point(16, 12);
            this.labelUrl.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(42, 15);
            this.labelUrl.TabIndex = 0;
            this.labelUrl.Text = "Enlace";
            //
            // textBoxUrl
            //
            this.textBoxUrl.AllowDrop = true;
            this.textBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUrl.Location = new System.Drawing.Point(16, 33);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(459, 23);
            this.textBoxUrl.TabIndex = 1;
            this.textBoxUrl.TextChanged += new System.EventHandler(this.TextBoxUrl_TextChanged);
            this.textBoxUrl.DragDrop += new System.Windows.Forms.DragEventHandler(this.Url_DragDrop);
            this.textBoxUrl.DragEnter += new System.Windows.Forms.DragEventHandler(this.Url_DragEnter);
            //
            // buttonPaste
            //
            this.buttonPaste.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonPaste.Location = new System.Drawing.Point(481, 31);
            this.buttonPaste.Name = "buttonPaste";
            this.buttonPaste.Size = new System.Drawing.Size(100, 27);
            this.buttonPaste.TabIndex = 2;
            this.buttonPaste.Text = "Pegar";
            this.toolTip.SetToolTip(this.buttonPaste, "Pegar el enlace copiado (Ctrl+V)");
            this.buttonPaste.UseVisualStyleBackColor = true;
            this.buttonPaste.Click += new System.EventHandler(this.ButtonPaste_Click);
            //
            // labelFolder
            //
            this.labelFolder.AutoSize = true;
            this.layout.SetColumnSpan(this.labelFolder, 2);
            this.labelFolder.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.labelFolder.Location = new System.Drawing.Point(16, 73);
            this.labelFolder.Margin = new System.Windows.Forms.Padding(3, 12, 3, 4);
            this.labelFolder.Name = "labelFolder";
            this.labelFolder.Size = new System.Drawing.Size(65, 15);
            this.labelFolder.TabIndex = 3;
            this.labelFolder.Text = "Guardar en";
            //
            // textBoxFolder
            //
            this.textBoxFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFolder.Location = new System.Drawing.Point(16, 94);
            this.textBoxFolder.Name = "textBoxFolder";
            this.textBoxFolder.Size = new System.Drawing.Size(459, 23);
            this.textBoxFolder.TabIndex = 4;
            //
            // buttonBrowseFolder
            //
            this.buttonBrowseFolder.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonBrowseFolder.Location = new System.Drawing.Point(481, 92);
            this.buttonBrowseFolder.Name = "buttonBrowseFolder";
            this.buttonBrowseFolder.Size = new System.Drawing.Size(100, 27);
            this.buttonBrowseFolder.TabIndex = 5;
            this.buttonBrowseFolder.Text = "Examinar...";
            this.toolTip.SetToolTip(this.buttonBrowseFolder, "Elegir la carpeta donde se guardan las descargas");
            this.buttonBrowseFolder.UseVisualStyleBackColor = true;
            this.buttonBrowseFolder.Click += new System.EventHandler(this.ButtonBrowseFolder_Click);
            //
            // optionsLayout
            //
            this.optionsLayout.AutoSize = true;
            this.optionsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.optionsLayout.ColumnCount = 6;
            this.layout.SetColumnSpan(this.optionsLayout, 2);
            this.optionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.optionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.optionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.optionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.optionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.optionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.optionsLayout.Controls.Add(this.labelFormat, 0, 0);
            this.optionsLayout.Controls.Add(this.radioButtonMp4, 1, 0);
            this.optionsLayout.Controls.Add(this.radioButtonMp3, 2, 0);
            this.optionsLayout.Controls.Add(this.labelQuality, 4, 0);
            this.optionsLayout.Controls.Add(this.comboBoxQuality, 5, 0);
            this.optionsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsLayout.Location = new System.Drawing.Point(13, 132);
            this.optionsLayout.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.optionsLayout.Name = "optionsLayout";
            this.optionsLayout.RowCount = 1;
            this.optionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.optionsLayout.Size = new System.Drawing.Size(574, 29);
            this.optionsLayout.TabIndex = 6;
            //
            // labelFormat
            //
            this.labelFormat.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelFormat.AutoSize = true;
            this.labelFormat.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.labelFormat.Location = new System.Drawing.Point(3, 7);
            this.labelFormat.Margin = new System.Windows.Forms.Padding(3, 0, 12, 0);
            this.labelFormat.Name = "labelFormat";
            this.labelFormat.Size = new System.Drawing.Size(51, 15);
            this.labelFormat.TabIndex = 0;
            this.labelFormat.Text = "Formato";
            //
            // radioButtonMp4
            //
            this.radioButtonMp4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonMp4.AutoSize = true;
            this.radioButtonMp4.Checked = true;
            this.radioButtonMp4.Location = new System.Drawing.Point(69, 5);
            this.radioButtonMp4.Name = "radioButtonMp4";
            this.radioButtonMp4.Size = new System.Drawing.Size(88, 19);
            this.radioButtonMp4.TabIndex = 1;
            this.radioButtonMp4.TabStop = true;
            this.radioButtonMp4.Text = "Vídeo (MP4)";
            this.toolTip.SetToolTip(this.radioButtonMp4, "Vídeo con audio");
            this.radioButtonMp4.UseVisualStyleBackColor = true;
            this.radioButtonMp4.CheckedChanged += new System.EventHandler(this.Format_CheckedChanged);
            //
            // radioButtonMp3
            //
            this.radioButtonMp3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonMp3.AutoSize = true;
            this.radioButtonMp3.Location = new System.Drawing.Point(163, 5);
            this.radioButtonMp3.Name = "radioButtonMp3";
            this.radioButtonMp3.Size = new System.Drawing.Size(89, 19);
            this.radioButtonMp3.TabIndex = 2;
            this.radioButtonMp3.Text = "Audio (MP3)";
            this.toolTip.SetToolTip(this.radioButtonMp3, "Solo el audio");
            this.radioButtonMp3.UseVisualStyleBackColor = true;
            this.radioButtonMp3.CheckedChanged += new System.EventHandler(this.Format_CheckedChanged);
            //
            // labelQuality
            //
            this.labelQuality.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelQuality.AutoSize = true;
            this.labelQuality.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.labelQuality.Location = new System.Drawing.Point(388, 7);
            this.labelQuality.Name = "labelQuality";
            this.labelQuality.Size = new System.Drawing.Size(47, 15);
            this.labelQuality.TabIndex = 3;
            this.labelQuality.Text = "Calidad";
            //
            // comboBoxQuality
            //
            this.comboBoxQuality.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.comboBoxQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxQuality.FormattingEnabled = true;
            this.comboBoxQuality.Location = new System.Drawing.Point(441, 3);
            this.comboBoxQuality.Name = "comboBoxQuality";
            this.comboBoxQuality.Size = new System.Drawing.Size(130, 23);
            this.comboBoxQuality.TabIndex = 4;
            this.toolTip.SetToolTip(this.comboBoxQuality, "Resolución máxima del vídeo o bitrate del audio");
            //
            // buttonDownload
            //
            this.layout.SetColumnSpan(this.buttonDownload, 2);
            this.buttonDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonDownload.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.buttonDownload.Location = new System.Drawing.Point(16, 177);
            this.buttonDownload.Margin = new System.Windows.Forms.Padding(3, 16, 3, 3);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(568, 40);
            this.buttonDownload.TabIndex = 7;
            this.buttonDownload.Text = "Descargar";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.ButtonDownload_Click);
            //
            // progressBar
            //
            this.layout.SetColumnSpan(this.progressBar, 2);
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(16, 232);
            this.progressBar.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(568, 8);
            this.progressBar.TabIndex = 8;
            //
            // resultLayout
            //
            this.resultLayout.AutoSize = true;
            this.resultLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.resultLayout.ColumnCount = 2;
            this.layout.SetColumnSpan(this.resultLayout, 2);
            this.resultLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.resultLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.resultLayout.Controls.Add(this.labelResult, 0, 0);
            this.resultLayout.Controls.Add(this.linkResultAction, 1, 0);
            this.resultLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultLayout.Location = new System.Drawing.Point(13, 249);
            this.resultLayout.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.resultLayout.Name = "resultLayout";
            this.resultLayout.RowCount = 1;
            this.resultLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.resultLayout.Size = new System.Drawing.Size(574, 22);
            this.resultLayout.TabIndex = 9;
            //
            // labelResult
            //
            this.labelResult.AutoEllipsis = true;
            this.labelResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelResult.Location = new System.Drawing.Point(3, 0);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(568, 22);
            this.labelResult.TabIndex = 0;
            this.labelResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // linkResultAction
            //
            this.linkResultAction.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.linkResultAction.AutoSize = true;
            this.linkResultAction.Location = new System.Drawing.Point(574, 3);
            this.linkResultAction.Name = "linkResultAction";
            this.linkResultAction.Size = new System.Drawing.Size(0, 15);
            this.linkResultAction.TabIndex = 1;
            this.linkResultAction.Visible = false;
            this.linkResultAction.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkResultAction_LinkClicked);
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.statusYtDlp});
            this.statusStrip.Location = new System.Drawing.Point(0, 304);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(600, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 2;
            //
            // statusLabel
            //
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(469, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "Listo";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // statusYtDlp
            //
            this.statusYtDlp.ForeColor = System.Drawing.SystemColors.GrayText;
            this.statusYtDlp.Name = "statusYtDlp";
            this.statusYtDlp.Size = new System.Drawing.Size(116, 17);
            this.statusYtDlp.Text = "Buscando yt-dlp...";
            this.statusYtDlp.Click += new System.EventHandler(this.StatusYtDlp_Click);
            //
            // MainForm
            //
            this.AcceptButton = this.buttonDownload;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(600, 326);
            this.Controls.Add(this.layout);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MediaGrab";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Url_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Url_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.layout.ResumeLayout(false);
            this.layout.PerformLayout();
            this.optionsLayout.ResumeLayout(false);
            this.optionsLayout.PerformLayout();
            this.resultLayout.ResumeLayout(false);
            this.resultLayout.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuOpenFolder;
        private System.Windows.Forms.ToolStripSeparator menuFileSeparator;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
        private System.Windows.Forms.ToolStripMenuItem menuTools;
        private System.Windows.Forms.ToolStripMenuItem menuCheckYtDlp;
        private System.Windows.Forms.ToolStripMenuItem menuUpdateYtDlp;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuSupportedSites;
        private System.Windows.Forms.ToolStripMenuItem menuProjectPage;
        private System.Windows.Forms.ToolStripSeparator menuHelpSeparator;
        private System.Windows.Forms.ToolStripMenuItem menuAbout;
        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.Label labelUrl;
        private System.Windows.Forms.TextBox textBoxUrl;
        private System.Windows.Forms.Button buttonPaste;
        private System.Windows.Forms.Label labelFolder;
        private System.Windows.Forms.TextBox textBoxFolder;
        private System.Windows.Forms.Button buttonBrowseFolder;
        private System.Windows.Forms.TableLayoutPanel optionsLayout;
        private System.Windows.Forms.Label labelFormat;
        private System.Windows.Forms.RadioButton radioButtonMp4;
        private System.Windows.Forms.RadioButton radioButtonMp3;
        private System.Windows.Forms.Label labelQuality;
        private System.Windows.Forms.ComboBox comboBoxQuality;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TableLayoutPanel resultLayout;
        private System.Windows.Forms.Label labelResult;
        private System.Windows.Forms.LinkLabel linkResultAction;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel statusYtDlp;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
