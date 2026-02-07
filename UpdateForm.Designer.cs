namespace DisplayedAppSwitcher {
    partial class UpdateForm {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent() {
            labelHeading = new Label();
            labelVersionInfo = new Label();
            progressBar = new ProgressBar();
            labelProgress = new Label();
            btnRemindNextTime = new Button();
            btnRemind7Days = new Button();
            btnUpdateNow = new Button();
            SuspendLayout();
            //
            // labelHeading
            //
            labelHeading.AutoSize = true;
            labelHeading.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            labelHeading.Location = new Point(24, 20);
            labelHeading.Name = "labelHeading";
            labelHeading.Size = new Size(195, 32);
            labelHeading.TabIndex = 0;
            labelHeading.Text = "Update Available";
            //
            // labelVersionInfo
            //
            labelVersionInfo.AutoSize = true;
            labelVersionInfo.Location = new Point(24, 64);
            labelVersionInfo.Name = "labelVersionInfo";
            labelVersionInfo.Size = new Size(300, 40);
            labelVersionInfo.TabIndex = 1;
            labelVersionInfo.Text = "Version X.Y.Z is available.\r\nYou are currently running vA.B.C.";
            //
            // progressBar
            //
            progressBar.Location = new Point(24, 120);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(400, 23);
            progressBar.TabIndex = 2;
            progressBar.Visible = false;
            //
            // labelProgress
            //
            labelProgress.AutoSize = true;
            labelProgress.Location = new Point(24, 148);
            labelProgress.Name = "labelProgress";
            labelProgress.Size = new Size(0, 20);
            labelProgress.TabIndex = 3;
            labelProgress.Visible = false;
            //
            // btnRemindNextTime
            //
            btnRemindNextTime.Location = new Point(24, 185);
            btnRemindNextTime.Name = "btnRemindNextTime";
            btnRemindNextTime.Size = new Size(155, 36);
            btnRemindNextTime.TabIndex = 4;
            btnRemindNextTime.Text = "Remind Me Next Time";
            btnRemindNextTime.UseVisualStyleBackColor = true;
            btnRemindNextTime.Click += btnRemindNextTime_Click;
            //
            // btnRemind7Days
            //
            btnRemind7Days.Location = new Point(185, 185);
            btnRemind7Days.Name = "btnRemind7Days";
            btnRemind7Days.Size = new Size(138, 36);
            btnRemind7Days.TabIndex = 5;
            btnRemind7Days.Text = "Remind in 7 Days";
            btnRemind7Days.UseVisualStyleBackColor = true;
            btnRemind7Days.Click += btnRemind7Days_Click;
            //
            // btnUpdateNow
            //
            btnUpdateNow.Location = new Point(329, 185);
            btnUpdateNow.Name = "btnUpdateNow";
            btnUpdateNow.Size = new Size(95, 36);
            btnUpdateNow.TabIndex = 6;
            btnUpdateNow.Text = "Update Now";
            btnUpdateNow.UseVisualStyleBackColor = true;
            btnUpdateNow.Click += btnUpdateNow_Click;
            //
            // UpdateForm
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnRemindNextTime;
            ClientSize = new Size(450, 240);
            Controls.Add(labelHeading);
            Controls.Add(labelVersionInfo);
            Controls.Add(progressBar);
            Controls.Add(labelProgress);
            Controls.Add(btnRemindNextTime);
            Controls.Add(btnRemind7Days);
            Controls.Add(btnUpdateNow);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UpdateForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Update Available";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelHeading;
        private Label labelVersionInfo;
        private ProgressBar progressBar;
        private Label labelProgress;
        private Button btnRemindNextTime;
        private Button btnRemind7Days;
        private Button btnUpdateNow;
    }
}
