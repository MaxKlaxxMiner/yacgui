namespace YacGui
{
  sealed partial class MainForm
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.pictureBoxMain = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBoxMain
      // 
      this.pictureBoxMain.Cursor = System.Windows.Forms.Cursors.Hand;
      this.pictureBoxMain.Location = new System.Drawing.Point(12, 12);
      this.pictureBoxMain.Name = "pictureBoxMain";
      this.pictureBoxMain.Size = new System.Drawing.Size(1350, 450);
      this.pictureBoxMain.TabIndex = 0;
      this.pictureBoxMain.TabStop = false;
      this.pictureBoxMain.Click += new System.EventHandler(this.pictureBoxMain_Click);
      this.pictureBoxMain.DoubleClick += new System.EventHandler(this.pictureBoxMain_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
      this.ClientSize = new System.Drawing.Size(1373, 474);
      this.Controls.Add(this.pictureBoxMain);
      this.Name = "MainForm";
      this.Text = "yacgui";
      this.Load += new System.EventHandler(this.MainForm_Load);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox pictureBoxMain;
  }
}

