using System;
using System.Drawing;
using System.Windows.Forms;
using InspectionAI.Classes.Managers;

namespace InspectionAI.Forms
{
    /// <summary>
    /// Dialog for camera settings (Exposure, Gain, White Balance)
    /// </summary>
    public class CameraSettingsDialog : Form
    {
        private HIKCameraManager camera;

        // Controls
        private TrackBar trkExposure;
        private TrackBar trkGain;
        private Label lblExposureValue;
        private Label lblGainValue;
        private CheckBox chkAutoExposure;
        private CheckBox chkAutoGain;
        private Button btnWhiteBalance;
        private Button btnApply;
        private Button btnCancel;
        private Label lblCameraName;

        // Ranges
        private float exposureMin, exposureMax;
        private float gainMin, gainMax;

        public CameraSettingsDialog(HIKCameraManager cameraManager)
        {
            this.camera = cameraManager;
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Camera Settings";
            this.Size = new Size(500, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            int yPos = 20;

            // Camera name
            lblCameraName = new Label
            {
                Text = $"Camera: {camera.CameraName}",
                Location = new Point(20, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            this.Controls.Add(lblCameraName);
            yPos += 40;

            // === EXPOSURE ===
            Label lblExposure = new Label
            {
                Text = "Exposure Time (μs):",
                Location = new Point(20, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White
            };
            this.Controls.Add(lblExposure);

            lblExposureValue = new Label
            {
                Text = "0",
                Location = new Point(420, yPos),
                Width = 60,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 0)
            };
            this.Controls.Add(lblExposureValue);
            yPos += 30;

            trkExposure = new TrackBar
            {
                Location = new Point(20, yPos),
                Width = 400,
                Minimum = 0,
                Maximum = 1000,
                TickFrequency = 100,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            trkExposure.ValueChanged += TrkExposure_ValueChanged;
            this.Controls.Add(trkExposure);
            yPos += 50;

            chkAutoExposure = new CheckBox
            {
                Text = "Auto Exposure",
                Location = new Point(20, yPos),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };
            chkAutoExposure.CheckedChanged += ChkAutoExposure_CheckedChanged;
            this.Controls.Add(chkAutoExposure);
            yPos += 40;

            // === GAIN ===
            Label lblGain = new Label
            {
                Text = "Gain (dB):",
                Location = new Point(20, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White
            };
            this.Controls.Add(lblGain);

            lblGainValue = new Label
            {
                Text = "0",
                Location = new Point(420, yPos),
                Width = 60,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 0)
            };
            this.Controls.Add(lblGainValue);
            yPos += 30;

            trkGain = new TrackBar
            {
                Location = new Point(20, yPos),
                Width = 400,
                Minimum = 0,
                Maximum = 240, // 24.0 dB * 10
                TickFrequency = 20,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            trkGain.ValueChanged += TrkGain_ValueChanged;
            this.Controls.Add(trkGain);
            yPos += 50;

            chkAutoGain = new CheckBox
            {
                Text = "Auto Gain",
                Location = new Point(20, yPos),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };
            chkAutoGain.CheckedChanged += ChkAutoGain_CheckedChanged;
            this.Controls.Add(chkAutoGain);
            yPos += 40;

            // === WHITE BALANCE ===
            btnWhiteBalance = new Button
            {
                Text = "⚪ Execute White Balance",
                Location = new Point(20, yPos),
                Width = 460,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnWhiteBalance.Click += BtnWhiteBalance_Click;
            this.Controls.Add(btnWhiteBalance);
            yPos += 50;

            // === BUTTONS ===
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(260, yPos),
                Width = 100,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 0, 0),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            this.Controls.Add(btnCancel);

            btnApply = new Button
            {
                Text = "Apply",
                Location = new Point(370, yPos),
                Width = 110,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.OK,
                Cursor = Cursors.Hand
            };
            this.Controls.Add(btnApply);

            this.AcceptButton = btnApply;
            this.CancelButton = btnCancel;
        }

        private void LoadCurrentSettings()
        {
            // Get ranges
            var expRange = camera.GetExposureRange();
            exposureMin = expRange.min;
            exposureMax = expRange.max;

            var gainRange = camera.GetGainRange();
            gainMin = gainRange.min;
            gainMax = gainRange.max;

            // Set trackbar range (scale to 0-1000 for exposure)
            trkExposure.Minimum = 0;
            trkExposure.Maximum = 1000;

            // Set current values
            float expNormalized = (camera.Exposure - exposureMin) / (exposureMax - exposureMin);
            trkExposure.Value = (int)(expNormalized * 1000);
            lblExposureValue.Text = $"{camera.Exposure:F0}";

            // Gain (scale to 0-240 for 0-24dB)
            trkGain.Minimum = 0;
            trkGain.Maximum = (int)(gainMax * 10);
            trkGain.Value = (int)(camera.Gain * 10);
            lblGainValue.Text = $"{camera.Gain:F1}";

            // Auto modes
            chkAutoExposure.Checked = camera.AutoExposure;
            chkAutoGain.Checked = camera.AutoGain;

            // Disable trackbars if auto
            trkExposure.Enabled = !camera.AutoExposure;
            trkGain.Enabled = !camera.AutoGain;
        }

        private void TrkExposure_ValueChanged(object sender, EventArgs e)
        {
            // Convert trackbar value (0-1000) to actual exposure range
            float normalized = trkExposure.Value / 1000f;
            float exposure = exposureMin + (normalized * (exposureMax - exposureMin));

            lblExposureValue.Text = $"{exposure:F0}";

            // Apply immediately
            if (!chkAutoExposure.Checked)
            {
                camera.SetExposure(exposure);
            }
        }

        private void TrkGain_ValueChanged(object sender, EventArgs e)
        {
            float gain = trkGain.Value / 10f;
            lblGainValue.Text = $"{gain:F1}";

            // Apply immediately
            if (!chkAutoGain.Checked)
            {
                camera.SetGain(gain);
            }
        }

        private void ChkAutoExposure_CheckedChanged(object sender, EventArgs e)
        {
            trkExposure.Enabled = !chkAutoExposure.Checked;
            camera.SetAutoExposure(chkAutoExposure.Checked);

            if (!chkAutoExposure.Checked)
            {
                // Refresh current value
                System.Threading.Thread.Sleep(100);
                float expNormalized = (camera.Exposure - exposureMin) / (exposureMax - exposureMin);
                trkExposure.Value = (int)(expNormalized * 1000);
            }
        }

        private void ChkAutoGain_CheckedChanged(object sender, EventArgs e)
        {
            trkGain.Enabled = !chkAutoGain.Checked;
            camera.SetAutoGain(chkAutoGain.Checked);

            if (!chkAutoGain.Checked)
            {
                // Refresh current value
                System.Threading.Thread.Sleep(100);
                trkGain.Value = (int)(camera.Gain * 10);
            }
        }

        private void BtnWhiteBalance_Click(object sender, EventArgs e)
        {
            btnWhiteBalance.Enabled = false;
            btnWhiteBalance.Text = "⏳ Executing...";

            System.Threading.Tasks.Task.Run(() =>
            {
                camera.SetWhiteBalance();
                System.Threading.Thread.Sleep(1000);

                this.Invoke(new Action(() =>
                {
                    btnWhiteBalance.Enabled = true;
                    btnWhiteBalance.Text = "⚪ Execute White Balance";
                    MessageBox.Show("White balance completed!", "Camera Settings",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            });
        }
    }
}