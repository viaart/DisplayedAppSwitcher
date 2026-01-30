namespace DisplayedAppSwitcher {
    partial class TipsForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TipsForm));
            mainPanel = new TableLayoutPanel();
            titleLabel = new Label();
            screenshotPictureBox = new PictureBox();
            explanationLabel = new Label();
            bottomPanel = new Panel();
            doNotShowAgainCheckBox = new CheckBox();
            okButton = new Button();
            mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)screenshotPictureBox).BeginInit();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            //
            // mainPanel
            //
            mainPanel.ColumnCount = 1;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.Controls.Add(titleLabel, 0, 0);
            mainPanel.Controls.Add(screenshotPictureBox, 0, 1);
            mainPanel.Controls.Add(explanationLabel, 0, 2);
            mainPanel.Controls.Add(bottomPanel, 0, 3);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(15, 15);
            mainPanel.Name = "mainPanel";
            mainPanel.RowCount = 4;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 248F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainPanel.Size = new Size(636, 420);
            mainPanel.TabIndex = 0;
            //
            // titleLabel
            //
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            titleLabel.Location = new Point(3, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Padding = new Padding(0, 0, 0, 10);
            titleLabel.Size = new Size(261, 42);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "New Feature: Auto-Positioning";
            //
            // screenshotPictureBox
            //
            screenshotPictureBox.BorderStyle = BorderStyle.FixedSingle;
            screenshotPictureBox.Dock = DockStyle.Fill;
            screenshotPictureBox.Location = new Point(3, 45);
            screenshotPictureBox.Name = "screenshotPictureBox";
            screenshotPictureBox.Size = new Size(632, 240);
            screenshotPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            screenshotPictureBox.TabIndex = 1;
            screenshotPictureBox.TabStop = false;
            screenshotPictureBox.Image = Properties.Resources.AutoPositionFeature;
            //
            // explanationLabel
            //
            explanationLabel.Dock = DockStyle.Fill;
            explanationLabel.Location = new Point(3, 264);
            explanationLabel.Name = "explanationLabel";
            explanationLabel.Padding = new Padding(0, 10, 0, 10);
            explanationLabel.Size = new Size(630, 80);
            explanationLabel.TabIndex = 2;
            explanationLabel.Text = "The Zoom secondary window can now be automatically positioned to the same monitor where JW Library's secondary window is displayed.\r\n\r\nThis feature is enabled by default. Use the tray icon to disable it if desired.";
            //
            // bottomPanel
            //
            bottomPanel.Controls.Add(doNotShowAgainCheckBox);
            bottomPanel.Controls.Add(okButton);
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.Location = new Point(3, 415);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Padding = new Padding(0, 5, 0, 0);
            bottomPanel.Size = new Size(630, 45);
            bottomPanel.TabIndex = 3;
            //
            // doNotShowAgainCheckBox
            //
            doNotShowAgainCheckBox.AutoSize = true;
            doNotShowAgainCheckBox.Location = new Point(0, 12);
            doNotShowAgainCheckBox.Name = "doNotShowAgainCheckBox";
            doNotShowAgainCheckBox.Size = new Size(165, 24);
            doNotShowAgainCheckBox.TabIndex = 0;
            doNotShowAgainCheckBox.Text = "Do not show again";
            doNotShowAgainCheckBox.UseVisualStyleBackColor = true;
            //
            // okButton
            //
            okButton.Anchor = AnchorStyles.Right;
            okButton.Location = new Point(530, 5);
            okButton.Name = "okButton";
            okButton.Size = new Size(100, 35);
            okButton.TabIndex = 1;
            okButton.Text = "Got it!";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            //
            // TipsForm
            //
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(666, 450);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TipsForm";
            Padding = new Padding(15);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tips - Displayed App Switcher";
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)screenshotPictureBox).EndInit();
            bottomPanel.ResumeLayout(false);
            bottomPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel mainPanel;
        private Label titleLabel;
        private PictureBox screenshotPictureBox;
        private Label explanationLabel;
        private Panel bottomPanel;
        private CheckBox doNotShowAgainCheckBox;
        private Button okButton;
    }
}
