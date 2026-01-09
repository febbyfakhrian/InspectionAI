using System;
using System.Drawing;
using System.Windows.Forms;
using InspectionAI.Classes.Managers;
using InspectionAI.Classes.Models;
using InspectionAI.Classes.Hardware;

namespace InspectionAI.Forms
{
    /// <summary>
    /// Settings Form untuk configure PLC, Camera, dll
    /// </summary>
    public partial class SettingsForm : Form
    {
        private AppConfig config;

        public SettingsForm(AppConfig currentConfig)
        {
            config = currentConfig;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.Size = new Size(600, 500);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // TabControl
            TabControl tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38),
                ForeColor = Color.White
            };

            // === TAB 1: PLC Settings ===
            TabPage tabPLC = new TabPage("PLC Settings");
            tabPLC.BackColor = Color.FromArgb(37, 37, 38);

            Label lblPort = new Label
            {
                Text = "COM Port:",
                Location = new Point(20, 20),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabPLC.Controls.Add(lblPort);

            ComboBox cmbPort = new ComboBox
            {
                Name = "cmbPort",
                Location = new Point(120, 17),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White
            };
            // Load available COM ports
            string[] ports = PLCController.GetAvailablePorts();
            if (ports.Length > 0)
                cmbPort.Items.AddRange(ports);
            else
                cmbPort.Items.Add("No COM ports available");
            tabPLC.Controls.Add(cmbPort);

            Label lblBaud = new Label
            {
                Text = "Baud Rate:",
                Location = new Point(20, 60),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabPLC.Controls.Add(lblBaud);

            ComboBox cmbBaud = new ComboBox
            {
                Name = "cmbBaud",
                Location = new Point(120, 57),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White
            };
            cmbBaud.Items.AddRange(new object[] { 9600, 19200, 38400, 57600, 115200 });
            tabPLC.Controls.Add(cmbBaud);

            CheckBox chkAutoRun = new CheckBox
            {
                Name = "chkAutoRun",
                Text = "Auto-run on PLC trigger",
                Location = new Point(20, 100),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabPLC.Controls.Add(chkAutoRun);

            Label lblDelay = new Label
            {
                Text = "Trigger Delay (ms):",
                Location = new Point(20, 140),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabPLC.Controls.Add(lblDelay);

            NumericUpDown numDelay = new NumericUpDown
            {
                Name = "numDelay",
                Location = new Point(150, 137),
                Width = 100,
                Minimum = 0,
                Maximum = 5000,
                Increment = 100,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White
            };
            tabPLC.Controls.Add(numDelay);

            Button btnTestPLC = new Button
            {
                Text = "Test Connection",
                Location = new Point(20, 180),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTestPLC.Click += BtnTestPLC_Click;
            tabPLC.Controls.Add(btnTestPLC);

            tabControl.TabPages.Add(tabPLC);

            // === TAB 2: Database Settings ===
            TabPage tabDB = new TabPage("Database");
            tabDB.BackColor = Color.FromArgb(37, 37, 38);

            Label lblServer = new Label
            {
                Text = "Server:",
                Location = new Point(20, 20),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabDB.Controls.Add(lblServer);

            TextBox txtServer = new TextBox
            {
                Name = "txtServer",
                Location = new Point(120, 17),
                Width = 200,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White
            };
            tabDB.Controls.Add(txtServer);

            Label lblDatabase = new Label
            {
                Text = "Database:",
                Location = new Point(20, 60),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabDB.Controls.Add(lblDatabase);

            TextBox txtDatabase = new TextBox
            {
                Name = "txtDatabase",
                Location = new Point(120, 57),
                Width = 200,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White
            };
            tabDB.Controls.Add(txtDatabase);

            Label lblUser = new Label
            {
                Text = "Username:",
                Location = new Point(20, 100),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabDB.Controls.Add(lblUser);

            TextBox txtUser = new TextBox
            {
                Name = "txtUser",
                Location = new Point(120, 97),
                Width = 200,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White
            };
            tabDB.Controls.Add(txtUser);

            Label lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(20, 140),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabDB.Controls.Add(lblPassword);

            TextBox txtPassword = new TextBox
            {
                Name = "txtPassword",
                Location = new Point(120, 137),
                Width = 200,
                UseSystemPasswordChar = true,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White
            };
            tabDB.Controls.Add(txtPassword);

            Button btnTestDB = new Button
            {
                Text = "Test Connection",
                Location = new Point(20, 180),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTestDB.Click += BtnTestDB_Click;
            tabDB.Controls.Add(btnTestDB);

            tabControl.TabPages.Add(tabDB);

            // === TAB 3: AI Settings ===
            TabPage tabAI = new TabPage("AI Server");
            tabAI.BackColor = Color.FromArgb(37, 37, 38);

            Label lblAIUrl = new Label
            {
                Text = "Server URL:",
                Location = new Point(20, 20),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabAI.Controls.Add(lblAIUrl);

            TextBox txtAIUrl = new TextBox
            {
                Name = "txtAIUrl",
                Location = new Point(120, 17),
                Width = 300,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                Text = "http://localhost:5000"
            };
            tabAI.Controls.Add(txtAIUrl);

            Label lblModelPath = new Label
            {
                Text = "Model Path:",
                Location = new Point(20, 60),
                ForeColor = Color.White,
                AutoSize = true
            };
            tabAI.Controls.Add(lblModelPath);

            TextBox txtModelPath = new TextBox
            {
                Name = "txtModelPath",
                Location = new Point(120, 57),
                Width = 300,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White
            };
            tabAI.Controls.Add(txtModelPath);

            Button btnBrowseModel = new Button
            {
                Text = "...",
                Location = new Point(425, 57),
                Size = new Size(40, 23),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBrowseModel.Click += BtnBrowseModel_Click;
            tabAI.Controls.Add(btnBrowseModel);

            tabControl.TabPages.Add(tabAI);

            this.Controls.Add(tabControl);

            // === BOTTOM BUTTONS ===
            Panel panelButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            Button btnSave = new Button
            {
                Text = "Save",
                Location = new Point(400, 15),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            panelButtons.Controls.Add(btnSave);

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(490, 15),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();
            panelButtons.Controls.Add(btnCancel);

            this.Controls.Add(panelButtons);
        }

        private void LoadSettings()
        {
            // Load PLC settings
            var cmbPort = this.Controls.Find("cmbPort", true)[0] as ComboBox;
            if (cmbPort != null && cmbPort.Items.Contains(config.Serial.PortName))
                cmbPort.SelectedItem = config.Serial.PortName;

            var cmbBaud = this.Controls.Find("cmbBaud", true)[0] as ComboBox;
            if (cmbBaud != null)
                cmbBaud.SelectedItem = config.Serial.BaudRate;

            var chkAutoRun = this.Controls.Find("chkAutoRun", true)[0] as CheckBox;
            if (chkAutoRun != null)
                chkAutoRun.Checked = config.Serial.AutoRunOnTrigger;

            var numDelay = this.Controls.Find("numDelay", true)[0] as NumericUpDown;
            if (numDelay != null)
                numDelay.Value = config.Serial.TriggerDelayMs;

            // Load Database settings
            var txtServer = this.Controls.Find("txtServer", true)[0] as TextBox;
            if (txtServer != null)
                txtServer.Text = config.Database.Server;

            var txtDatabase = this.Controls.Find("txtDatabase", true)[0] as TextBox;
            if (txtDatabase != null)
                txtDatabase.Text = config.Database.Database;

            var txtUser = this.Controls.Find("txtUser", true)[0] as TextBox;
            if (txtUser != null)
                txtUser.Text = config.Database.Username;

            var txtPassword = this.Controls.Find("txtPassword", true)[0] as TextBox;
            if (txtPassword != null)
                txtPassword.Text = config.Database.Password;

            // Load AI settings
            var txtModelPath = this.Controls.Find("txtModelPath", true)[0] as TextBox;
            if (txtModelPath != null)
                txtModelPath.Text = config.Model.ModelPath;
        }

        private void BtnTestPLC_Click(object sender, EventArgs e)
        {
            var cmbPort = this.Controls.Find("cmbPort", true)[0] as ComboBox;
            if (cmbPort == null || cmbPort.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string port = cmbPort.SelectedItem.ToString();
            string error;

            if (PLCController.TestPort(port, out error))
            {
                MessageBox.Show($"Connection successful!\nPort: {port}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Connection failed!\nPort: {port}\nError: {error}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTestDB_Click(object sender, EventArgs e)
        {
            var txtServer = this.Controls.Find("txtServer", true)[0] as TextBox;
            var txtDatabase = this.Controls.Find("txtDatabase", true)[0] as TextBox;
            var txtUser = this.Controls.Find("txtUser", true)[0] as TextBox;
            var txtPassword = this.Controls.Find("txtPassword", true)[0] as TextBox;

            string connString = $"Server={txtServer.Text};Database={txtDatabase.Text};Uid={txtUser.Text};Pwd={txtPassword.Text};";

            var testLogger = new DataLogger(connString);
            string error;

            if (testLogger.TestConnection(out error))
            {
                MessageBox.Show("Database connection successful!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Database connection failed!\n{error}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBrowseModel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "ONNX Models (*.onnx)|*.onnx|All Files (*.*)|*.*";
                dialog.Title = "Select Model File";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var txtModelPath = this.Controls.Find("txtModelPath", true)[0] as TextBox;
                    if (txtModelPath != null)
                        txtModelPath.Text = dialog.FileName;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Save PLC settings
                var cmbPort = this.Controls.Find("cmbPort", true)[0] as ComboBox;
                if (cmbPort != null && cmbPort.SelectedItem != null)
                    config.Serial.PortName = cmbPort.SelectedItem.ToString();

                var cmbBaud = this.Controls.Find("cmbBaud", true)[0] as ComboBox;
                if (cmbBaud != null && cmbBaud.SelectedItem != null)
                    config.Serial.BaudRate = (int)cmbBaud.SelectedItem;

                var chkAutoRun = this.Controls.Find("chkAutoRun", true)[0] as CheckBox;
                if (chkAutoRun != null)
                    config.Serial.AutoRunOnTrigger = chkAutoRun.Checked;

                var numDelay = this.Controls.Find("numDelay", true)[0] as NumericUpDown;
                if (numDelay != null)
                    config.Serial.TriggerDelayMs = (int)numDelay.Value;

                // Save Database settings
                var txtServer = this.Controls.Find("txtServer", true)[0] as TextBox;
                if (txtServer != null)
                    config.Database.Server = txtServer.Text;

                var txtDatabase = this.Controls.Find("txtDatabase", true)[0] as TextBox;
                if (txtDatabase != null)
                    config.Database.Database = txtDatabase.Text;

                var txtUser = this.Controls.Find("txtUser", true)[0] as TextBox;
                if (txtUser != null)
                    config.Database.Username = txtUser.Text;

                var txtPassword = this.Controls.Find("txtPassword", true)[0] as TextBox;
                if (txtPassword != null)
                    config.Database.Password = txtPassword.Text;

                // Save AI settings
                var txtModelPath = this.Controls.Find("txtModelPath", true)[0] as TextBox;
                if (txtModelPath != null)
                    config.Model.ModelPath = txtModelPath.Text;

                // Save to file
                ConfigManager.SaveConfig(config);

                MessageBox.Show("Settings saved! Please restart the application for changes to take effect.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}