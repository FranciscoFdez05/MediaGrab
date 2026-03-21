namespace YTDownloader
{
    partial class Form1
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxURL = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxCarpetaDescargas = new System.Windows.Forms.TextBox();
            this.buttonFolder = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.ra = new System.Windows.Forms.Button();
            this.radioButtonMP4 = new System.Windows.Forms.RadioButton();
            this.radioButtonMP3 = new System.Windows.Forms.RadioButton();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "URL to download";
            // 
            // textBoxURL
            // 
            this.textBoxURL.Location = new System.Drawing.Point(16, 32);
            this.textBoxURL.Name = "textBoxURL";
            this.textBoxURL.Size = new System.Drawing.Size(442, 20);
            this.textBoxURL.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Downloads folder";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // textBoxCarpetaDescargas
            // 
            this.textBoxCarpetaDescargas.Location = new System.Drawing.Point(16, 93);
            this.textBoxCarpetaDescargas.Name = "textBoxCarpetaDescargas";
            this.textBoxCarpetaDescargas.Size = new System.Drawing.Size(442, 20);
            this.textBoxCarpetaDescargas.TabIndex = 3;
            // 
            // buttonFolder
            // 
            this.buttonFolder.Location = new System.Drawing.Point(464, 93);
            this.buttonFolder.Name = "buttonFolder";
            this.buttonFolder.Size = new System.Drawing.Size(75, 20);
            this.buttonFolder.TabIndex = 4;
            this.buttonFolder.Text = "Folder";
            this.buttonFolder.UseVisualStyleBackColor = true;
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(462, 32);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(75, 20);
            this.buttonClear.TabIndex = 5;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            // 
            // ra
            // 
            this.ra.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ra.Location = new System.Drawing.Point(16, 130);
            this.ra.Name = "ra";
            this.ra.Size = new System.Drawing.Size(128, 42);
            this.ra.TabIndex = 6;
            this.ra.Text = "Download";
            this.ra.UseVisualStyleBackColor = true;
            this.ra.Click += new System.EventHandler(this.button1_Click);
            // 
            // radioButtonMP4
            // 
            this.radioButtonMP4.AutoSize = true;
            this.radioButtonMP4.Location = new System.Drawing.Point(182, 130);
            this.radioButtonMP4.Name = "radioButtonMP4";
            this.radioButtonMP4.Size = new System.Drawing.Size(47, 17);
            this.radioButtonMP4.TabIndex = 7;
            this.radioButtonMP4.TabStop = true;
            this.radioButtonMP4.Text = "MP4";
            this.radioButtonMP4.UseVisualStyleBackColor = true;
            // 
            // radioButtonMP3
            // 
            this.radioButtonMP3.AutoSize = true;
            this.radioButtonMP3.Location = new System.Drawing.Point(182, 155);
            this.radioButtonMP3.Name = "radioButtonMP3";
            this.radioButtonMP3.Size = new System.Drawing.Size(47, 17);
            this.radioButtonMP3.TabIndex = 8;
            this.radioButtonMP3.TabStop = true;
            this.radioButtonMP3.Text = "MP3";
            this.radioButtonMP3.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(260, 139);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(277, 23);
            this.progressBar.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 187);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.radioButtonMP3);
            this.Controls.Add(this.radioButtonMP4);
            this.Controls.Add(this.ra);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.buttonFolder);
            this.Controls.Add(this.textBoxCarpetaDescargas);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxURL);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "YT Downloader";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxURL;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxCarpetaDescargas;
        private System.Windows.Forms.Button buttonFolder;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button ra;
        private System.Windows.Forms.RadioButton radioButtonMP4;
        private System.Windows.Forms.RadioButton radioButtonMP3;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}

