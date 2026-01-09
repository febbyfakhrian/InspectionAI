using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace InspectionAI.Forms
{
    /// <summary>
    /// NG Image Viewer - View saved NG images
    /// </summary>
    public partial class NGImageViewer : Form
    {
        private string ngFolder = "NG_Images";
        private string[] imageFiles;
        private int currentIndex = 0;

        public NGImageViewer()
        {
            InitializeComponent();
            LoadImages();
        }

        private void InitializeComponent()
        {
            this.Text = "NG Image Gallery";
            this.Size = new Size(900, 700);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            // PictureBox untuk display image
            PictureBox pictureBox = new PictureBox
            {
                Name = "pictureBox",
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            this.Controls.Add(pictureBox);

            // Panel untuk controls (bottom)
            Panel panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // Label info
            Label lblInfo = new Label
            {
                Name = "lblInfo",
                Text = "No images",
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.White
            };
            panelBottom.Controls.Add(lblInfo);

            // Button Previous
            Button btnPrev = new Button
            {
                Text = "< Previous",
                Location = new Point(300, 25),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPrev.Click += (s, e) => ShowPreviousImage();
            panelBottom.Controls.Add(btnPrev);

            // Button Next
            Button btnNext = new Button
            {
                Text = "Next >",
                Location = new Point(410, 25),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNext.Click += (s, e) => ShowNextImage();
            panelBottom.Controls.Add(btnNext);

            // Button Delete
            Button btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(520, 25),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.Click += BtnDelete_Click;
            panelBottom.Controls.Add(btnDelete);

            // Button Close
            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(760, 25),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();
            panelBottom.Controls.Add(btnClose);

            this.Controls.Add(panelBottom);
        }

        private void LoadImages()
        {
            if (!Directory.Exists(ngFolder))
            {
                Directory.CreateDirectory(ngFolder);
            }

            imageFiles = Directory.GetFiles(ngFolder, "*.jpg");

            if (imageFiles.Length > 0)
            {
                currentIndex = 0;
                ShowCurrentImage();
            }
            else
            {
                var lblInfo = this.Controls.Find("lblInfo", true)[0] as Label;
                if (lblInfo != null)
                    lblInfo.Text = "No NG images found";
            }
        }

        private void ShowCurrentImage()
        {
            if (imageFiles == null || imageFiles.Length == 0)
                return;

            try
            {
                var pictureBox = this.Controls.Find("pictureBox", true)[0] as PictureBox;
                var lblInfo = this.Controls.Find("lblInfo", true)[0] as Label;

                if (pictureBox != null)
                {
                    // Dispose old image
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                    }

                    // Load new image
                    pictureBox.Image = Image.FromFile(imageFiles[currentIndex]);
                }

                if (lblInfo != null)
                {
                    string fileName = Path.GetFileName(imageFiles[currentIndex]);
                    lblInfo.Text = $"Image {currentIndex + 1} of {imageFiles.Length}: {fileName}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowNextImage()
        {
            if (imageFiles == null || imageFiles.Length == 0)
                return;

            currentIndex = (currentIndex + 1) % imageFiles.Length;
            ShowCurrentImage();
        }

        private void ShowPreviousImage()
        {
            if (imageFiles == null || imageFiles.Length == 0)
                return;

            currentIndex = (currentIndex - 1 + imageFiles.Length) % imageFiles.Length;
            ShowCurrentImage();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (imageFiles == null || imageFiles.Length == 0)
                return;

            var result = MessageBox.Show("Delete this NG image?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    File.Delete(imageFiles[currentIndex]);
                    LoadImages(); // Reload
                    ShowCurrentImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting image: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Cleanup
            var pictureBox = this.Controls.Find("pictureBox", true)[0] as PictureBox;
            if (pictureBox != null && pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
        }
    }
}