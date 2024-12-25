namespace DisplayedAppSwitcher {
  partial class AboutBoxForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBoxForm));
      tableLayoutPanel = new TableLayoutPanel();
      logoPictureBox = new PictureBox();
      labelProductName = new Label();
      labelVersion = new Label();
      labelCopyright = new LinkLabel();
      labelCompanyName = new Label();
      textBoxDescription = new TextBox();
      okButton = new Button();
      tableLayoutPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
      SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      tableLayoutPanel.ColumnCount = 2;
      tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15.852334F));
      tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 84.14767F));
      tableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
      tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
      tableLayoutPanel.Controls.Add(labelVersion, 1, 1);
      tableLayoutPanel.Controls.Add(labelCopyright, 1, 2);
      tableLayoutPanel.Controls.Add(labelCompanyName, 1, 3);
      tableLayoutPanel.Controls.Add(textBoxDescription, 1, 4);
      tableLayoutPanel.Controls.Add(okButton, 1, 5);
      tableLayoutPanel.Dock = DockStyle.Fill;
      tableLayoutPanel.Location = new Point(17, 20);
      tableLayoutPanel.Margin = new Padding(7, 6, 7, 6);
      tableLayoutPanel.Name = "tableLayoutPanel";
      tableLayoutPanel.RowCount = 7;
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
      tableLayoutPanel.Size = new Size(921, 614);
      tableLayoutPanel.TabIndex = 0;
      // 
      // logoPictureBox
      // 
      logoPictureBox.Dock = DockStyle.Top;
      logoPictureBox.Image = (Image)resources.GetObject("logoPictureBox.Image");
      logoPictureBox.Location = new Point(7, 6);
      logoPictureBox.Margin = new Padding(7, 6, 7, 6);
      logoPictureBox.Name = "logoPictureBox";
      tableLayoutPanel.SetRowSpan(logoPictureBox, 6);
      logoPictureBox.Size = new Size(132, 132);
      logoPictureBox.TabIndex = 12;
      logoPictureBox.TabStop = false;
      // 
      // labelProductName
      // 
      labelProductName.Dock = DockStyle.Fill;
      labelProductName.Location = new Point(158, 0);
      labelProductName.Margin = new Padding(12, 0, 7, 0);
      labelProductName.MaximumSize = new Size(0, 40);
      labelProductName.Name = "labelProductName";
      labelProductName.Size = new Size(756, 40);
      labelProductName.TabIndex = 19;
      labelProductName.Text = "Product Name";
      labelProductName.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // labelVersion
      // 
      labelVersion.Dock = DockStyle.Fill;
      labelVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
      labelVersion.Location = new Point(158, 60);
      labelVersion.Margin = new Padding(12, 0, 7, 0);
      labelVersion.MaximumSize = new Size(0, 40);
      labelVersion.Name = "labelVersion";
      labelVersion.Size = new Size(756, 40);
      labelVersion.TabIndex = 0;
      labelVersion.Text = "Version";
      labelVersion.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // labelCopyright
      // 
      labelCopyright.Dock = DockStyle.Fill;
      labelCopyright.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
      labelCopyright.LinkColor = Color.FromArgb(0, 0, 192);
      labelCopyright.Location = new Point(158, 120);
      labelCopyright.Margin = new Padding(12, 0, 7, 0);
      labelCopyright.MaximumSize = new Size(0, 40);
      labelCopyright.Name = "labelCopyright";
      labelCopyright.Size = new Size(756, 40);
      labelCopyright.TabIndex = 21;
      labelCopyright.TabStop = true;
      labelCopyright.Text = "Copyright";
      labelCopyright.TextAlign = ContentAlignment.MiddleLeft;
      labelCopyright.LinkClicked += labelCopyright_LinkClicked;
      // 
      // labelCompanyName
      // 
      labelCompanyName.Dock = DockStyle.Fill;
      labelCompanyName.Location = new Point(158, 180);
      labelCompanyName.Margin = new Padding(12, 0, 7, 0);
      labelCompanyName.MaximumSize = new Size(0, 40);
      labelCompanyName.Name = "labelCompanyName";
      labelCompanyName.Size = new Size(756, 40);
      labelCompanyName.TabIndex = 22;
      labelCompanyName.Text = "Company Name";
      labelCompanyName.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // textBoxDescription
      // 
      textBoxDescription.Dock = DockStyle.Fill;
      textBoxDescription.Location = new Point(158, 246);
      textBoxDescription.Margin = new Padding(12, 6, 7, 6);
      textBoxDescription.Multiline = true;
      textBoxDescription.Name = "textBoxDescription";
      textBoxDescription.ReadOnly = true;
      textBoxDescription.ScrollBars = ScrollBars.Both;
      textBoxDescription.Size = new Size(756, 291);
      textBoxDescription.TabIndex = 23;
      textBoxDescription.TabStop = false;
      textBoxDescription.Text = "Description";
      // 
      // okButton
      // 
      okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      okButton.DialogResult = DialogResult.Cancel;
      okButton.Location = new Point(763, 549);
      okButton.Margin = new Padding(7, 6, 7, 6);
      okButton.Name = "okButton";
      okButton.Size = new Size(151, 48);
      okButton.TabIndex = 24;
      okButton.Text = "&OK";
      // 
      // AboutBoxForm
      // 
      AcceptButton = okButton;
      AutoScaleDimensions = new SizeF(12F, 30F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(955, 654);
      Controls.Add(tableLayoutPanel);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      Icon = (Icon)resources.GetObject("$this.Icon");
      Margin = new Padding(7, 6, 7, 6);
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "AboutBoxForm";
      Padding = new Padding(17, 20, 17, 20);
      ShowInTaskbar = false;
      StartPosition = FormStartPosition.CenterParent;
      Text = "About";
      tableLayoutPanel.ResumeLayout(false);
      tableLayoutPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
      ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.PictureBox logoPictureBox;
    private System.Windows.Forms.Label labelProductName;
    private System.Windows.Forms.Label labelVersion;
    private System.Windows.Forms.Label labelCompanyName;
    private System.Windows.Forms.TextBox textBoxDescription;
    private System.Windows.Forms.Button okButton;
    private LinkLabel labelCopyright;
  }
}
