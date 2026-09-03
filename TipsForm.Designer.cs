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
            explanationText = new RichTextBox();
            bottomPanel = new TableLayoutPanel();
            doNotShowAgainCheckBox = new CheckBox();
            okButton = new Button();
            mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)screenshotPictureBox).BeginInit();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            //
            // mainPanel
            //
            mainPanel.AutoScroll = true;
            mainPanel.ColumnCount = 1;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.Controls.Add(titleLabel, 0, 0);
            mainPanel.Controls.Add(screenshotPictureBox, 0, 1);
            mainPanel.Controls.Add(explanationText, 0, 2);
            mainPanel.Controls.Add(bottomPanel, 0, 3);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(15, 15);
            mainPanel.Name = "mainPanel";
            mainPanel.RowCount = 4;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
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
            titleLabel.Text = "Tips";
            //
            // screenshotPictureBox
            //
            screenshotPictureBox.BorderStyle = BorderStyle.None;
            screenshotPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            screenshotPictureBox.Location = new Point(3, 45);
            screenshotPictureBox.Margin = new Padding(3, 0, 3, 0);
            screenshotPictureBox.Name = "screenshotPictureBox";
            // Sized to the image's own 918x357 aspect at the 200px row height, so Zoom
            // scales it edge to edge and has no spare box to centre it within.
            screenshotPictureBox.Size = new Size(514, 200);
            screenshotPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            screenshotPictureBox.TabIndex = 1;
            screenshotPictureBox.TabStop = false;
            screenshotPictureBox.Image = Properties.Resources.NewFeatures;
            //
            // explanationLabel
            //
            // A RichTextBox rather than a Label, because the setting names are bold while
            // the rest of the line is not, and a Label cannot mix fonts within its text.
            // Styled so it reads as static text: no border, no scrollbars, form
            // background, not focusable, and an arrow cursor instead of an I beam.
            // Content is built in TipsForm.BuildExplanation.
            explanationText.BackColor = SystemColors.Control;
            explanationText.BorderStyle = BorderStyle.None;
            explanationText.Cursor = Cursors.Default;
            explanationText.Dock = DockStyle.Fill;
            explanationText.Location = new Point(3, 264);
            explanationText.Margin = new Padding(3, 10, 3, 10);
            explanationText.Name = "explanationText";
            explanationText.ReadOnly = true;
            // Vertical shows a scroll bar only when the text actually overflows, so it
            // is invisible at normal scaling but stops the last lines being lost when
            // font scaling squeezes this row.
            explanationText.ScrollBars = RichTextBoxScrollBars.Vertical;
            explanationText.Size = new Size(630, 80);
            explanationText.TabIndex = 2;
            explanationText.TabStop = false;
            explanationText.Text = "";
            //
            // bottomPanel
            //
            // Two columns rather than free placement. The checkbox takes whatever is
            // left after the button has been given the width it needs, so narrowing the
            // window can never slide the button underneath the caption.
            bottomPanel.ColumnCount = 2;
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bottomPanel.Controls.Add(doNotShowAgainCheckBox, 0, 0);
            bottomPanel.Controls.Add(okButton, 1, 0);
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.Location = new Point(3, 415);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Padding = new Padding(0, 5, 0, 0);
            bottomPanel.RowCount = 1;
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            bottomPanel.Size = new Size(630, 45);
            bottomPanel.TabIndex = 3;
            //
            // doNotShowAgainCheckBox
            //
            // Sized to its own text and left in its own column, so the clickable area
            // matches the caption rather than stretching towards the button.
            doNotShowAgainCheckBox.AutoSize = true;
            doNotShowAgainCheckBox.Anchor = AnchorStyles.Left;
            doNotShowAgainCheckBox.Location = new Point(0, 12);
            doNotShowAgainCheckBox.Name = "doNotShowAgainCheckBox";
            doNotShowAgainCheckBox.Size = new Size(165, 24);
            doNotShowAgainCheckBox.TabIndex = 0;
            doNotShowAgainCheckBox.TextAlign = ContentAlignment.MiddleLeft;
            doNotShowAgainCheckBox.Text = "Do not show again (always available from About... > Tips)";
            doNotShowAgainCheckBox.UseVisualStyleBackColor = true;
            //
            // okButton
            //
            okButton.Anchor = AnchorStyles.Right;
            okButton.Margin = new Padding(9, 3, 0, 3);
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
            ClientSize = new Size(666, 620);
            Controls.Add(mainPanel);
            // Sizable rather than FixedDialog, so the window can be made smaller than the
            // content needs. The explanation then scrolls instead of losing its last lines.
            FormBorderStyle = FormBorderStyle.Sizable;
            // Chosen so the explanation keeps a readable height at the smallest size.
            // The picture and button rows are fixed, so everything taken off the window
            // comes out of the explanation: at 420 design units it collapses to one line.
            MinimumSize = new Size(560, 560);
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
        private RichTextBox explanationText;
        private TableLayoutPanel bottomPanel;
        private CheckBox doNotShowAgainCheckBox;
        private Button okButton;
    }
}
