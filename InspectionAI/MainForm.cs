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
using InspectionAI.Classes.AI;
using InspectionAI.Classes.Hardware;
using InspectionAI.Classes.Managers;
using InspectionAI.Classes.Models;

//using MvCameraControl;

namespace InspectionAI
{
    public partial class MainForm : Form
    {
        private bool isDragging = false;
        private Point lastCursor;
        private Point lastForm;
        private AppConfig appConfig;
        private DataLogger dataLogger;
        private bool isDatabaseConnected = false;
        private HIKCameraManager cameraManager;
        private PictureBox pbCameraFeed;
        private System.Windows.Forms.Timer inspectionTimer;
        private AIClient aiClient;
        private PLCController plcController;
        private CycleTimer cycleTimer;
        private ProductionCounter productionCounter;
        private bool isAutoRunEnabled = false;
        //private void TestSDK()
        //{
        //    try
        //    {
        //        // Initialize SDK
        //        int result = MvCameraControl.MV_CC_Initialize();
        //        if (result == MvCameraControl.MV_OK)
        //        {
        //            MessageBox.Show("HIKRobot SDK loaded successfully!");
        //        }
        //        else
        //        {
        //            MessageBox.Show($"SDK Init failed: {result}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"SDK Error: {ex.Message}");
        //    }
        //}

        public MainForm()
        {
            try { InitializeComponent();
                MessageBox.Show("InitializeComponent OK!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                throw;
            }


            //this.EnableRoundedCorners = true;
            pbCameraFeed = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Button btnTest = new Button
            {
                Name = "btnTestCamera",
                Text = "Test Camera",
                Location = new Point(12, 12),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTest.FlatAppearance.BorderSize = 0;
            btnTest.Click += btnTestCamera_Click;

            // Add to form (adjust parent container as needed)
            this.Controls.Add(btnTest);
            btnTest.BringToFront();

            // Add ke panel camera
            if (panelCameraView != null)
            {
                // Hide label placeholder
                if (lblCameraPlaceholder != null)
                    lblCameraPlaceholder.Visible = false;

                // Add picturebox
                panelCameraView.Controls.Add(pbCameraFeed);
                pbCameraFeed.BringToFront();
            }

            // ===== FORCE TAB CONTROL STYLING - IMMEDIATE =====
            if (tabControl1 != null)
            {
                // CRITICAL: Set these BEFORE Load event
                tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
                tabControl1.ItemSize = new Size(120, 32);
                tabControl1.Multiline = false;
                tabControl1.Alignment = TabAlignment.Top;
                tabControl1.SizeMode = TabSizeMode.Fixed;
                tabControl1.Padding = new Point(16, 6);
                tabControl1.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                tabControl1.BackColor = Color.FromArgb(37, 37, 38);

                // Attach event handlers
                tabControl1.DrawItem += TabControl1_DrawItem;
                tabControl1.Paint += TabControl1_Paint;

                // Force redraw
                tabControl1.Invalidate();
            }

            if (checkedListCameras != null)
            {
                checkedListCameras.Items.Clear();
                checkedListCameras.Items.Add("Camera_1 (10.0.0.101) - 60 FPS", true);
                checkedListCameras.Items.Add("Camera_2 (10.0.0.102) - 58 FPS", true);
                checkedListCameras.Items.Add("Camera_3 (10.0.0.103) - Disconnected", false);
            }

            MakeResponsive();
            InitializeManagers();
        }

        private void InitializeManagers()
        {

            // Log startup
            Logger.LogInfo("Application started");
            Logger.CleanOldLogs(30); // Keep last 30 days

            try
            {
                // Load configuration
                Logger.LogInfo("Loading configuration...");
                appConfig = ConfigManager.LoadConfig();
                Logger.LogInfo("Configuration loaded successfully");

                // Initialize database logger
                Logger.LogInfo("Connecting to database...");
                dataLogger = new DataLogger(appConfig.Database.GetConnectionString());

                // Test database connection
                string error;
                if (dataLogger.TestConnection(out error))
                {
                    dataLogger.InitializeDatabase();
                    isDatabaseConnected = true;
                    statusLabel.Text = "Ready | Database Connected";
                    Logger.LogInfo("Database connected successfully");
                }
                else
                {
                    isDatabaseConnected = false;
                    statusLabel.Text = "Ready | Database Offline";
                    Logger.LogWarning($"Database connection failed: {error}");
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Ready | Config Error";
                Logger.LogError("Configuration error", ex);
            }

            try
            {
                Logger.LogInfo("Initializing camera...");
                cameraManager = new HIKCameraManager();
                cameraManager.FrameReceived += CameraManager_FrameReceived;
                cameraManager.ErrorOccurred += (s, msg) => Logger.LogError($"Camera error: {msg}");

               
                if (cameraManager.Connect(0))
                {
                    statusLabel.Text += " | Camera OK";
                    Logger.LogInfo($"Camera connected: {cameraManager.CameraName}");
                }
                else
                {
                    statusLabel.Text += " | Cam Error";
                    Logger.LogWarning("Camera connection failed");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Camera initialization failed", ex);
            }

            try
            {
                // Initialize AI Client
                Logger.LogInfo("Connecting to AI server...");
                aiClient = new AIClient("http://localhost:5000");

                Task.Run(async () =>
                {
                    bool connected = await aiClient.ConnectAsync();

                    this.Invoke(new Action(() =>
                    {
                        if (connected)
                        {
                            statusLabel.Text += " | AI OK";
                            Logger.LogInfo("AI server connected");
                        }
                        else
                        {
                            statusLabel.Text += " | AI Offline";
                            Logger.LogWarning("AI server offline");
                        }
                    }));
                });
            }
            catch (Exception ex)
            {
                Logger.LogError("AI client initialization failed", ex);
            }

            try
            {
                // Initialize PLC Controller
                if (!string.IsNullOrEmpty(appConfig.Serial.PortName))
                {
                    Logger.LogInfo($"Connecting to PLC: {appConfig.Serial.PortName}");
                    plcController = new PLCController(appConfig.Serial.PortName, appConfig.Serial.BaudRate);
                    plcController.TriggerReceived += PlcController_TriggerReceived;
                    plcController.ErrorOccurred += PlcController_ErrorOccurred;

                    string plcError;
                    if (plcController.Connect(out plcError))
                    {
                        statusLabel.Text += " | PLC OK";
                        isAutoRunEnabled = appConfig.Serial.AutoRunOnTrigger;
                        Logger.LogInfo("PLC connected successfully");
                    }
                    else
                    {
                        statusLabel.Text += " | PLC Offline";
                        Logger.LogWarning($"PLC connection failed: {plcError}");
                    }
                }

                // Initialize timers and counters
                cycleTimer = new CycleTimer();
                productionCounter = new ProductionCounter();
                UpdateProductionCounterDisplay();

                Logger.LogInfo("All managers initialized");
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Ready | Init Error";
                Logger.LogError("Manager initialization failed", ex);
            }

        }

        private void MakeResponsive()
        {
            // Set minimum form size
            this.MinimumSize = new Size(900, 600);

            // SplitContainer responsive
            if (splitContainer1 != null)
            {
                splitContainer1.BackColor = Color.FromArgb(37, 37, 38);
                splitContainer1.Panel1.BackColor = Color.FromArgb(37, 37, 38);
                splitContainer1.Panel2.BackColor = Color.FromArgb(37, 37, 38);
                splitContainer1.SplitterWidth = 1;
                splitContainer1.Paint -= SplitContainer1_Paint;
                splitContainer1.Paint += SplitContainer1_Paint;
            }

            if (lblCameraPlaceholder != null)
            {
                lblCameraPlaceholder.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                lblCameraPlaceholder.Location = new Point(12, 10);
                lblCameraPlaceholder.Font = new Font("Segoe UI", 10F);
                lblCameraPlaceholder.AutoSize = true;
            }

            if (checkedListCameras != null)
            {
                checkedListCameras.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            }

            if (btnRefreshCameras != null)
            {
                btnRefreshCameras.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            }

            if (dgvResults != null && dgvResults.Columns.Count > 0)
            {
                dgvResults.Columns[dgvResults.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            }
        }

        private void PlcController_TriggerReceived(object sender, EventArgs e)
        {
            // PLC sent trigger signal
            if (!isAutoRunEnabled)
                return;

            // Invoke on UI thread
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => PlcController_TriggerReceived(sender, e)));
                return;
            }

            // Delay sebelum run (stabilization)
            System.Threading.Tasks.Task.Delay(appConfig.Serial.TriggerDelayMs).ContinueWith(_ =>
            {
                this.Invoke(new Action(() =>
                {
                    // Start cycle timer
                    cycleTimer.Start();

                    // Run inspection
                    RunSingleInspection();
                }));
            });
        }

        private void PlcController_ErrorOccurred(object sender, string error)
        {
            System.Diagnostics.Debug.WriteLine($"PLC Error: {error}");
        }

        /// <summary>
        /// Run single inspection (dipanggil dari PLC trigger atau manual)
        /// </summary>
        private async void RunSingleInspection()
        {
            try
            {
                if (cameraManager == null || aiClient == null)
                    return;

                if (!cameraManager.IsConnected)
                    return;

                // Capture frame
                Bitmap frame = cameraManager.CaptureFrame();
                if (frame == null)
                    return;

                // Call AI
                var aiResult = await aiClient.ProcessImageAsync(frame, cameraManager.CameraName);

                if (!aiResult.Success)
                {
                    frame.Dispose();
                    return;
                }

                // Stop cycle timer
                long cycleTimeMs = cycleTimer.Stop();

                // Determine status
                InspectionStatus status = aiResult.IsPass ? InspectionStatus.GOOD : InspectionStatus.NG;

                // Update production counter
                switch (status)
                {
                    case InspectionStatus.GOOD:
                        productionCounter.RecordGood();
                        break;
                    case InspectionStatus.NG:
                        productionCounter.RecordNG();
                        break;
                    case InspectionStatus.WARNING:
                        productionCounter.RecordWarning();
                        break;
                }

                // Create result
                var result = new InspectionResult
                {
                    SetNumber = $"SET_{DateTime.Now:yyyyMMddHHmmss}",
                    CameraId = cameraManager.CameraName,
                    Result = status,
                    InspectionTimeMs = (int)cycleTimeMs,
                    Timestamp = aiResult.Timestamp,
                    Detections = aiResult.Detections
                };

                // Save to database
                SaveInspectionResult(result);

                // Update UI
                UpdateStatusBar(result.Result);
                RefreshResultsTable();
                UpdateProductionCounterDisplay();

                // Draw bounding boxes
                if (aiResult.Detections.Count > 0)
                {
                    ImageProcessor.DrawBoundingBoxes(frame, aiResult.Detections);
                    DisplayAnnotatedFrame(frame);
                }

                // Send result to PLC
                if (plcController != null && plcController.IsConnected)
                {
                    if (status == InspectionStatus.GOOD)
                        plcController.SendPass();
                    else
                        plcController.SendFail();
                }

                frame.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Inspection error: {ex.Message}");
            }
        }

        private void UpdateProductionCounterDisplay()
        {
            if (productionCounter == null)
                return;

            // Update status label dengan counter
            if (lblProductionStats != null)
            {
                string text = productionCounter.GetSummaryText();

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => lblProductionStats.Text = text));
                }
                else
                {
                    lblProductionStats.Text = text;
                }
            }
        }

        // Add menu atau button untuk reset counter
        private void ResetProductionCounter()
        {
            if (productionCounter != null)
            {
                productionCounter.Reset();
                UpdateProductionCounterDisplay();
                MessageBox.Show("Production counter reset!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CameraManager_FrameReceived(object sender, Bitmap frame)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => CameraManager_FrameReceived(sender, frame)));
                return;
            }

            try
            {
                // Clone frame for display
                Bitmap displayFrame = ImageProcessor.CloneBitmap(frame);

                // Draw overlay (camera name, FPS, timestamp)
                ImageProcessor.DrawOverlay(displayFrame, cameraManager.CameraName, 60, DateTime.Now);

                if (pbCameraFeed != null)
                {
                    // Dispose old image
                    if (pbCameraFeed.Image != null)
                    {
                        var oldImage = pbCameraFeed.Image;
                        pbCameraFeed.Image = null;
                        oldImage.Dispose();
                    }

                    pbCameraFeed.Image = displayFrame;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Frame display error: {ex.Message}");
            }
        }
        private void SplitContainer1_Paint(object sender, PaintEventArgs e)
        {
            SplitContainer sc = sender as SplitContainer;
            if (sc == null) return;

            // Draw splitter dengan warna DARK GRAY (bukan putih!)
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(37, 37, 38)))
            {
                Rectangle rect = sc.SplitterRectangle;
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        private void toolStripBtnRun_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "● RUNNING | Inspection Active";
            statusLabel.ForeColor = Color.White;
            statusStrip1.BackColor = Color.FromArgb(0, 122, 204);

            toolStripBtnRun.Enabled = false;
            toolStripBtnStop.Enabled = true;

            if (lblCameraPlaceholder != null)
            {
                lblCameraPlaceholder.Text = "Inspection Running...";
                lblCameraPlaceholder.ForeColor = Color.Lime;
            }

            if (cameraManager != null)
            {
                cameraManager.StartGrabbing();
            }

            isAutoRunEnabled = true;

            if (plcController == null || !plcController.IsConnected)
            {
                StartInspectionTimer();
            }

            SimulateInspectionResult();
        }


        private async void SimulateInspectionResult()
        {
            try
            {
                if (cameraManager == null || aiClient == null)
                    return;

                if (!cameraManager.IsConnected)
                    return;

                // Capture frame
                Bitmap frame = cameraManager.CaptureFrame();
                if (frame == null)
                    return;

                // Call AI server
                var aiResult = await aiClient.ProcessImageAsync(frame, cameraManager.CameraName);

                if (!aiResult.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"AI Error: {aiResult.ErrorMessage}");
                    frame.Dispose();
                    return;
                }

                // Determine status
                InspectionStatus status = aiResult.IsPass ? InspectionStatus.GOOD : InspectionStatus.NG;

                // Create inspection result
                var result = new InspectionResult
                {
                    SetNumber = $"SET_{DateTime.Now:yyyyMMddHHmmss}",
                    CameraId = cameraManager.CameraName,
                    Result = status,
                    InspectionTimeMs = aiResult.InferenceTimeMs,
                    Timestamp = aiResult.Timestamp,
                    Detections = aiResult.Detections
                };

                // Save to database
                SaveInspectionResult(result);

                // Update UI
                UpdateStatusBar(result.Result);
                RefreshResultsTable();

                // Draw bounding boxes on frame
                if (aiResult.Detections.Count > 0)
                {
                    ImageProcessor.DrawBoundingBoxes(frame, aiResult.Detections);
                    DisplayAnnotatedFrame(frame);
                }

                frame.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Inspection error: {ex.Message}");
            }
        }

        private void DisplayAnnotatedFrame(Bitmap frame)
        {
            if (pbCameraFeed == null)
                return;

            // Clone and display
            Bitmap display = ImageProcessor.CloneBitmap(frame);

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    if (pbCameraFeed.Image != null)
                    {
                        var old = pbCameraFeed.Image;
                        pbCameraFeed.Image = null;
                        old.Dispose();
                    }
                    pbCameraFeed.Image = display;
                }));
            }
            else
            {
                if (pbCameraFeed.Image != null)
                {
                    var old = pbCameraFeed.Image;
                    pbCameraFeed.Image = null;
                    old.Dispose();
                }
                pbCameraFeed.Image = display;
            }
        }

        private void SaveInspectionResult(InspectionResult result)
        {
            if (!isDatabaseConnected)
                return; // Skip if database not connected

            try
            {
                int id = dataLogger.InsertInspectionResult(result);
                System.Diagnostics.Debug.WriteLine($"Saved inspection ID: {id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
            }
        }

        private void UpdateStatusBar(InspectionStatus status)
        {
            switch (status)
            {
                case InspectionStatus.GOOD:
                    statusStrip1.BackColor = Color.FromArgb(0, 150, 0); // Green
                    statusLabel.Text = "● GOOD | All Checks Passed";
                    break;

                case InspectionStatus.NG:
                    statusStrip1.BackColor = Color.FromArgb(200, 0, 0); // Red
                    statusLabel.Text = "● NG | Defect Detected";
                    SoundAlertManager.PlayNGAlert();
                    break;

                case InspectionStatus.WARNING:
                    statusStrip1.BackColor = Color.FromArgb(255, 140, 0); // Orange
                    statusLabel.Text = "● WARNING | Low Confidence";
                    SoundAlertManager.PlayWarningAlert();
                    break;
            }

            // Auto-return to blue after 2 seconds
            System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        statusStrip1.BackColor = Color.FromArgb(0, 122, 204); // Blue
                        statusLabel.Text = "● RUNNING | Inspection Active";
                    }));
                }
            });
        }

        private void RefreshResultsTable()
        {
            if (!isDatabaseConnected)
                return;

            try
            {
                var dt = dataLogger.GetRecentResults(50); // Last 50 results

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        dgvResults.DataSource = dt;
                    }));
                }
                else
                {
                    dgvResults.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Refresh error: {ex.Message}");
            }
        }

        private void toolStripBtnStop_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Ready | Idle";
            statusLabel.ForeColor = Color.White;
            statusStrip1.BackColor = Color.FromArgb(62, 62, 66);

            toolStripBtnRun.Enabled = true;
            toolStripBtnStop.Enabled = false;

            if (lblCameraPlaceholder != null)
            {
                lblCameraPlaceholder.Text = "Camera Feed - Ready";
                lblCameraPlaceholder.ForeColor = Color.White;
            }

            // Stop camera grabbing
            if (cameraManager != null)
            {
                cameraManager.StopGrabbing();
            }
            isAutoRunEnabled = false;
            if (cycleTimer != null && cycleTimer.IsRunning)
            {
                cycleTimer.Stop();
            }

            StopInspectionTimer();

            // Clear camera display
            if (pbCameraFeed != null && pbCameraFeed.Image != null)
            {
                var oldImage = pbCameraFeed.Image;
                pbCameraFeed.Image = null;
                oldImage.Dispose();
            }
        }

        private void StartInspectionTimer()
        {
            if (inspectionTimer == null)
            {
                inspectionTimer = new System.Windows.Forms.Timer();
                inspectionTimer.Interval = 3000; // 3 seconds
                inspectionTimer.Tick += InspectionTimer_Tick;
            }

            inspectionTimer.Start();
        }

        private void StopInspectionTimer()
        {
            if (inspectionTimer != null)
            {
                inspectionTimer.Stop();
            }
        }

        private void InspectionTimer_Tick(object sender, EventArgs e)
        {
            // Run inspection simulation
            SimulateInspectionResult();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Stop everything
            if (cameraManager != null)
            {
                cameraManager.StopGrabbing();
                cameraManager.Disconnect();
            }

            if (inspectionTimer != null)
            {
                inspectionTimer.Stop();
                inspectionTimer.Dispose();
            }

            // Cleanup images
            if (pbCameraFeed != null && pbCameraFeed.Image != null)
            {
                pbCameraFeed.Image.Dispose();
            }

            if (plcController != null)
            {
                plcController.Disconnect();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // ===== APPLY DARK THEME =====

            // MenuStrip
            if (menuStrip1 != null)
            {
                menuStrip1.Renderer = new DarkMenuRenderer();
                menuStrip1.BackColor = Color.Transparent;
                menuStrip1.ForeColor = Color.White;
            }

            // ToolStrip
            if (toolStrip1 != null)
            {
                toolStrip1.Renderer = new DarkMenuRenderer();
                toolStrip1.BackColor = Color.FromArgb(45, 45, 48);
                toolStrip1.ForeColor = Color.White;
            }

            // StatusStrip
            if (statusStrip1 != null)
            {
                statusStrip1.Renderer = new DarkMenuRenderer();
                statusStrip1.BackColor = Color.FromArgb(62, 62, 66);
            }

            statusLabel.Text = "Ready | Idle";
            statusLabel.ForeColor = Color.White;

            if (splitContainer1 != null)
            {
                splitContainer1.BackColor = Color.FromArgb(37, 37, 38);
                splitContainer1.Panel1.BackColor = Color.FromArgb(37, 37, 38);
                splitContainer1.Panel2.BackColor = Color.FromArgb(37, 37, 38);
                splitContainer1.SplitterWidth = 1;

                splitContainer1.Paint -= SplitContainer1_Paint;
                splitContainer1.Paint += SplitContainer1_Paint;
            }

            // ===== CAMERAS TAB - ENABLE CUSTOM DRAW =====
            if (checkedListCameras != null)
            {
                // ENABLE custom drawing untuk checkbox dark theme
                checkedListCameras.DrawMode = DrawMode.OwnerDrawFixed;
                checkedListCameras.DrawItem += CheckedListCameras_DrawItem;

                checkedListCameras.BackColor = Color.FromArgb(37, 37, 38);
                checkedListCameras.ForeColor = Color.White;
                checkedListCameras.Items.Clear();
                checkedListCameras.Items.Add("Camera_1 (10.0.0.101) - 60 FPS", true);
                checkedListCameras.Items.Add("Camera_2 (10.0.0.102) - 58 FPS", true);
                checkedListCameras.Items.Add("Camera_3 (10.0.0.103) - Disconnected", false);
            }

            if (label1 != null)
            {
                label1.BackColor = Color.Transparent;
                label1.ForeColor = Color.White;
            }

            if (btnRefreshCameras != null)
            {
                btnRefreshCameras.BackColor = Color.FromArgb(62, 62, 66);
                btnRefreshCameras.ForeColor = Color.White;
                btnRefreshCameras.FlatStyle = FlatStyle.Flat;
                // CHANGE: Border jadi DARK GRAY (bukan abu terang yang terlihat putih!)
                btnRefreshCameras.FlatAppearance.BorderColor = Color.FromArgb(45, 45, 48);
                btnRefreshCameras.FlatAppearance.BorderSize = 1;
            }

            // ===== NG GALLERY TAB =====
            if (flowPanelNG != null)
            {
                flowPanelNG.BackColor = Color.FromArgb(37, 37, 38);
            }

            if (label2 != null)
            {
                label2.BackColor = Color.Transparent;
                label2.ForeColor = Color.White;
            }

            // ===== DATAGRIDVIEW =====
            ApplyDataGridViewDarkTheme();

            if (dgvResults != null)
            {
                dgvResults.Rows.Clear();
                dgvResults.Rows.Add("1245", "Good", "NG", "Good", "✗ FAIL", "14:35:40", "Screw Missing");
                dgvResults.Rows.Add("1246", "Good", "Good", "Good", "✓ PASS", "14:35:43", "-");
                dgvResults.Rows.Add("1247", "Good", "Good", "Good", "✓ PASS", "14:35:45", "-");

                foreach (DataGridViewRow row in dgvResults.Rows)
                {
                    if (row.Cells[4].Value?.ToString().Contains("FAIL") == true)
                    {
                        row.Cells[4].Style.ForeColor = Color.FromArgb(220, 53, 69);
                        row.Cells[4].Style.Font = new Font(dgvResults.Font, FontStyle.Bold);
                    }
                    else if (row.Cells[4].Value?.ToString().Contains("PASS") == true)
                    {
                        row.Cells[4].Style.ForeColor = Color.LimeGreen;
                        row.Cells[4].Style.Font = new Font(dgvResults.Font, FontStyle.Bold);
                    }
                }
            }
        }

        // ===== CUSTOM CHECKBOX DRAWING - DARK GRAY (BUKAN PUTIH!) =====
        private void CheckedListCameras_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Background - dark gray
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(37, 37, 38)))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            // Checkbox kotak
            int checkSize = 14;
            int checkX = e.Bounds.X + 4;
            int checkY = e.Bounds.Y + (e.Bounds.Height - checkSize) / 2;
            Rectangle checkBox = new Rectangle(checkX, checkY, checkSize, checkSize);

            // Checkbox background - DARK GRAY (bukan putih!)
            using (SolidBrush checkBg = new SolidBrush(Color.FromArgb(62, 62, 66)))
            {
                e.Graphics.FillRectangle(checkBg, checkBox);
            }

            // Checkbox border - DARK GRAY (bukan abu terang yang keliatan putih!)
            using (Pen borderPen = new Pen(Color.FromArgb(80, 80, 84), 1))
            {
                e.Graphics.DrawRectangle(borderPen, checkBox);
            }

            // Draw checkmark jika checked
            bool isChecked = checkedListCameras.GetItemChecked(e.Index);
            if (isChecked)
            {
                // Checkmark warna BLUE (terlihat jelas!)
                using (Pen checkPen = new Pen(Color.FromArgb(0, 122, 204), 2))
                {
                    e.Graphics.DrawLine(checkPen, checkX + 3, checkY + 7, checkX + 6, checkY + 10);
                    e.Graphics.DrawLine(checkPen, checkX + 6, checkY + 10, checkX + 11, checkY + 4);
                }
            }

            // Draw text
            string text = checkedListCameras.Items[e.Index].ToString();
            Color textColor = text.Contains("Disconnected") ? Color.FromArgb(150, 150, 150) : Color.White;

            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(text, e.Font, textBrush, e.Bounds.X + 22, e.Bounds.Y + 2);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (tabControl1 != null)
            {
                tabControl1.Invalidate();
                tabControl1.Refresh();
            }
        }

        // ===== TAB CONTROL - GARIS DARK GRAY (BUKAN PUTIH!) =====
        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl == null) return;

            Rectangle tabBounds = e.Bounds;
            bool isSelected = (e.Index == tabControl.SelectedIndex);

            Color bgColor, textColor;

            if (isSelected)
            {
                // ACTIVE TAB
                bgColor = Color.FromArgb(37, 37, 38);
                textColor = Color.White;
            }
            else
            {
                // INACTIVE TAB
                bgColor = Color.FromArgb(63, 63, 70);
                textColor = Color.FromArgb(200, 200, 200);
            }

            // Fill tab background
            using (SolidBrush bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, tabBounds);
            }

            // Draw text
            string tabText = tabControl.TabPages[e.Index].Text;
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(tabText, tabControl.Font, textBrush, tabBounds, sf);
            }

            // Draw border - DARK GRAY (bukan abu terang yang keliatan putih!)
            if (!isSelected)
            {
                // CHANGE: Warna border jadi DARK GRAY!
                using (Pen borderPen = new Pen(Color.FromArgb(45, 45, 48), 1))
                {
                    e.Graphics.DrawRectangle(borderPen,
                        tabBounds.X, tabBounds.Y,
                        tabBounds.Width - 1, tabBounds.Height - 1);
                }
            }
        }

        private void TabControl1_Paint(object sender, PaintEventArgs e)
        {
            TabControl tab = sender as TabControl;
            if (tab == null) return;

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(37, 37, 38)))
            {
                e.Graphics.FillRectangle(brush, tab.ClientRectangle);
            }
        }

        private void ApplyDataGridViewDarkTheme()
        {
            if (dgvResults == null) return;

            dgvResults.EnableHeadersVisualStyles = false;

            dgvResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvResults.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 45, 48);

            dgvResults.DefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
            dgvResults.DefaultCellStyle.ForeColor = Color.White;
            dgvResults.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvResults.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvResults.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);

            dgvResults.BackgroundColor = Color.FromArgb(30, 30, 30);
            dgvResults.GridColor = Color.FromArgb(62, 62, 66);
            dgvResults.BorderStyle = BorderStyle.None;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                btnMaximize.Text = "❐";
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                btnMaximize.Text = "□";
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panelTopBar_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            lastCursor = Cursor.Position;
            lastForm = this.Location;
        }

        private void panelTopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(lastCursor));
                this.Location = Point.Add(lastForm, new Size(diff));
            }
        }

        private void panelTopBar_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void panelTopBar_DoubleClick(object sender, EventArgs e)
        {
            btnMaximize_Click(sender, e);
        }

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            panelTopBar_MouseDown(sender, e);
        }

        private void menuStrip1_MouseMove(object sender, MouseEventArgs e)
        {
            panelTopBar_MouseMove(sender, e);
        }

        private void menuStrip1_MouseUp(object sender, MouseEventArgs e)
        {
            panelTopBar_MouseUp(sender, e);
        }

        private const int RESIZE_BORDER = 8;

        private enum ResizeDirection
        {
            None = 0,
            Left = 1,
            TopLeft = 2,
            Top = 3,
            TopRight = 4,
            Right = 5,
            BottomRight = 6,
            Bottom = 7,
            BottomLeft = 8
        }

        private ResizeDirection GetResizeDirection(Point point)
        {
            if (point.X < RESIZE_BORDER && point.Y < RESIZE_BORDER)
                return ResizeDirection.TopLeft;
            if (point.X > this.Width - RESIZE_BORDER && point.Y < RESIZE_BORDER)
                return ResizeDirection.TopRight;
            if (point.X < RESIZE_BORDER && point.Y > this.Height - RESIZE_BORDER)
                return ResizeDirection.BottomLeft;
            if (point.X > this.Width - RESIZE_BORDER && point.Y > this.Height - RESIZE_BORDER)
                return ResizeDirection.BottomRight;

            if (point.X < RESIZE_BORDER)
                return ResizeDirection.Left;
            if (point.X > this.Width - RESIZE_BORDER)
                return ResizeDirection.Right;
            if (point.Y < RESIZE_BORDER)
                return ResizeDirection.Top;
            if (point.Y > this.Height - RESIZE_BORDER)
                return ResizeDirection.Bottom;

            return ResizeDirection.None;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (isDragging || this.WindowState == FormWindowState.Maximized)
                return;

            ResizeDirection dir = GetResizeDirection(e.Location);

            switch (dir)
            {
                case ResizeDirection.Left:
                case ResizeDirection.Right:
                    this.Cursor = Cursors.SizeWE;
                    break;
                case ResizeDirection.Top:
                case ResizeDirection.Bottom:
                    this.Cursor = Cursors.SizeNS;
                    break;
                case ResizeDirection.TopLeft:
                case ResizeDirection.BottomRight:
                    this.Cursor = Cursors.SizeNWSE;
                    break;
                case ResizeDirection.TopRight:
                case ResizeDirection.BottomLeft:
                    this.Cursor = Cursors.SizeNESW;
                    break;
                default:
                    this.Cursor = Cursors.Default;
                    break;
            }
        }

        private ResizeDirection currentResizeDir = ResizeDirection.None;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (this.WindowState == FormWindowState.Maximized)
                return;

            currentResizeDir = GetResizeDirection(e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            currentResizeDir = ResizeDirection.None;
            this.Cursor = Cursors.Default;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 1;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            if (m.Msg == WM_NCHITTEST && this.WindowState == FormWindowState.Normal)
            {
                base.WndProc(ref m);

                if ((int)m.Result == HTCLIENT)
                {
                    Point screenPoint = new Point(m.LParam.ToInt32());
                    Point clientPoint = this.PointToClient(screenPoint);

                    ResizeDirection dir = GetResizeDirection(clientPoint);

                    switch (dir)
                    {
                        case ResizeDirection.Left:
                            m.Result = (IntPtr)HTLEFT;
                            return;
                        case ResizeDirection.TopLeft:
                            m.Result = (IntPtr)HTTOPLEFT;
                            return;
                        case ResizeDirection.Top:
                            m.Result = (IntPtr)HTTOP;
                            return;
                        case ResizeDirection.TopRight:
                            m.Result = (IntPtr)HTTOPRIGHT;
                            return;
                        case ResizeDirection.Right:
                            m.Result = (IntPtr)HTRIGHT;
                            return;
                        case ResizeDirection.BottomRight:
                            m.Result = (IntPtr)HTBOTTOMRIGHT;
                            return;
                        case ResizeDirection.Bottom:
                            m.Result = (IntPtr)HTBOTTOM;
                            return;
                        case ResizeDirection.BottomLeft:
                            m.Result = (IntPtr)HTBOTTOMLEFT;
                            return;
                    }
                }
                return;
            }

            base.WndProc(ref m);
        }

        private void OpenLabelingTool()
        {
            try
            {
                var labelingForm = new Forms.LabelingForm();
                labelingForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening labeling tool: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblCameraPlaceholder_Click(object sender, EventArgs e)
        {

        }

        private void labelingToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLabelingTool();
        }

        private void OpenSettings()
        {
            try
            {
                var settingsForm = new Forms.SettingsForm(appConfig);
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Reload config
                    appConfig = ConfigManager.LoadConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add menu handler
        
        private void settingsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OpenSettings();
        }

        // Add these methods

        private void GenerateReport()
        {
            if (!isDatabaseConnected || dataLogger == null)
            {
                MessageBox.Show("Database not connected!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "HTML Report (*.html)|*.html|CSV Report (*.csv)|*.csv|Text Report (*.txt)|*.txt";
                dialog.FileName = $"Report_{DateTime.Now:yyyyMMdd}";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var reportGen = new ReportGenerator(dataLogger);
                        bool success = false;

                        string ext = Path.GetExtension(dialog.FileName).ToLower();
                        switch (ext)
                        {
                            case ".html":
                                success = reportGen.GenerateHTMLReport(DateTime.Now, dialog.FileName);
                                break;
                            case ".csv":
                                success = reportGen.GenerateCSVReport(DateTime.Now.AddDays(-7), DateTime.Now, dialog.FileName);
                                break;
                            case ".txt":
                                success = reportGen.GenerateDailyReport(DateTime.Now, dialog.FileName);
                                break;
                        }

                        if (success)
                        {
                            MessageBox.Show("Report generated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Open file
                            System.Diagnostics.Process.Start(dialog.FileName);
                        }
                        else
                        {
                            MessageBox.Show("Failed to generate report", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.LogError("Report generation failed", ex);
                    }
                }
            }
        }

        private void OpenNGImageGallery()
        {
            try
            {
                var viewer = new Forms.NGImageViewer();
                viewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening NG gallery: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("Failed to open NG gallery", ex);
            }
        }

        private void ViewLogs()
        {
            try
            {
                string logFile = Logger.GetTodayLogFile();

                if (File.Exists(logFile))
                {
                    System.Diagnostics.Process.Start("notepad.exe", logFile);
                }
                else
                {
                    MessageBox.Show("No log file found for today", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Menu handlers
        private void generateReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        private void nGGalleryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenNGImageGallery();
        }

        private void viewLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewLogs();
        }
        private void OpenDashboard()
        {
            try
            {
                if (productionCounter == null)
                {
                    MessageBox.Show("Production counter not initialized", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var dashboard = new Forms.DashboardForm(productionCounter);
                dashboard.Show(); // Non-modal untuk bisa lihat main form juga
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening dashboard: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("Failed to open dashboard", ex);
            }
        }

        private void dashboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDashboard();
        }
        private void btnTestCamera_Click(object sender, EventArgs e)
        {
            try
            {
                // Test discovery
                var cameras = HIKCameraManager.DiscoverCameras();
                MessageBox.Show($"Found {cameras.Count} camera(s)!");

                if (cameras.Count == 0)
                {
                    MessageBox.Show("No cameras found!\n\n" +
                                  "Check:\n" +
                                  "- Camera plugged in (USB)\n" +
                                  "- Camera powered ON\n" +
                                  "- MVS software can see it");
                    return;
                }

                // Test connect
                var cam = new HIKCameraManager();

                // Subscribe to errors
                cam.ErrorOccurred += (s, msg) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Camera Error: {msg}", "Error");
                    }));
                };

                if (!cam.Connect(0))
                {
                    MessageBox.Show("Failed to connect!");
                    return;
                }

                MessageBox.Show($"✓ Connected!\n\nCamera: {cam.CameraName}\nSN: {cam.SerialNumber}");

                // Test settings
                MessageBox.Show($"Current Settings:\n" +
                               $"Exposure: {cam.Exposure}μs\n" +
                               $"Gain: {cam.Gain}dB\n" +
                               $"Auto Exposure: {cam.AutoExposure}\n" +
                               $"Auto Gain: {cam.AutoGain}");

                // Disconnect
                cam.Disconnect();
                MessageBox.Show("✓ Test complete!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Test failed!\n\n{ex.Message}\n\n{ex.StackTrace}",
                               "Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }
    }
}