using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InspectionAI.Classes.Models;
using InspectionAI.Classes.Managers;

namespace InspectionAI.Forms
{
    public partial class LabelingForm : Form
    {
        private LabelingProject project;
        private List<string> imageFiles;
        private int currentImageIndex = 0;
        private Bitmap currentImage;
        private ImageAnnotation currentAnnotation;

        // Drawing state
        private bool isDrawing = false;
        private bool isEditing = false;
        private bool isPanning = false;
        private Point startPoint;
        private Point endPoint;
        private Point panStartPoint;
        private string selectedClass = "";
        private int selectedClassId = 0;
        private int selectedBoxIndex = -1;
        private int editHandle = -1; // -1: none, 0-3: corners, 4-7: edges, 8: move

        // Display
        private Panel imagePanel;
        private PictureBox pictureBox;
        private float zoomLevel = 1.0f;
        private float minZoomLevel = 0.01f; // Will be calculated on load

        public LabelingForm()
        {
            InitializeComponent();
            InitializeProject();

            this.KeyPreview = true;
            this.KeyDown += LabelingForm_KeyDown;
        }

        private void InitializeComponent()
        {
            this.Text = "Labeling Tool - LabelStudio Style";
            this.Size = new Size(1400, 900);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // === TOP TOOLBAR ===
            Panel toolbarTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            Label lblTitle = new Label
            {
                Text = "Labeling Tool",
                Location = new Point(15, 15),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };
            toolbarTop.Controls.Add(lblTitle);

            Button btnLoadImages = new Button
            {
                Text = "📁 Load Images",
                Location = new Point(200, 10),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLoadImages.Click += BtnLoadImages_Click;
            toolbarTop.Controls.Add(btnLoadImages);

            Label lblShortcuts = new Label
            {
                Text = "Shortcuts: [D/→] Next  [A/←] Prev  [Del] Delete  [Ctrl+Z] Undo  [Scroll] Zoom  [Space+Drag or Middle Mouse] Pan",
                Location = new Point(350, 18),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8)
            };
            toolbarTop.Controls.Add(lblShortcuts);

            this.Controls.Add(toolbarTop);

            // === LEFT PANEL (Image List & Classes) ===
            Panel panelLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(37, 37, 38)
            };

            Label lblImages = new Label
            {
                Text = "📷 Images",
                Location = new Point(15, 15),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            panelLeft.Controls.Add(lblImages);

            ListBox listImages = new ListBox
            {
                Name = "listImages",
                Location = new Point(15, 45),
                Size = new Size(250, 250),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            listImages.SelectedIndexChanged += ListImages_SelectedIndexChanged;
            panelLeft.Controls.Add(listImages);

            Label lblClasses = new Label
            {
                Text = "🏷️ Classes",
                Location = new Point(15, 310),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            panelLeft.Controls.Add(lblClasses);

            Button btnAddClass = new Button
            {
                Text = "+ Add Class",
                Location = new Point(15, 340),
                Size = new Size(250, 28),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddClass.Click += BtnAddClass_Click;
            panelLeft.Controls.Add(btnAddClass);

            ListBox listClasses = new ListBox
            {
                Name = "listClasses",
                Location = new Point(15, 375),
                Size = new Size(250, 180),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            listClasses.SelectedIndexChanged += ListClasses_SelectedIndexChanged;
            panelLeft.Controls.Add(listClasses);

            Label lblHelp = new Label
            {
                Text = "💡 Tip: Press 1-9 for quick\nclass selection",
                Location = new Point(15, 565),
                Size = new Size(250, 40),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8)
            };
            panelLeft.Controls.Add(lblHelp);

            this.Controls.Add(panelLeft);

            // === CENTER (Image Display) ===
            Panel panelCenter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            // Image container
            imagePanel = new Panel
            {
                Name = "imagePanel",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20),
                AutoScroll = true,
                BorderStyle = BorderStyle.None
            };
            imagePanel.Resize += ImagePanel_Resize;

            pictureBox = new PictureBox
            {
                Name = "pictureBox",
                SizeMode = PictureBoxSizeMode.Normal,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;
            pictureBox.Paint += PictureBox_Paint;
            pictureBox.MouseWheel += PictureBox_MouseWheel;

            imagePanel.Controls.Add(pictureBox);
            panelCenter.Controls.Add(imagePanel);

            // Bottom controls
            Panel bottomControls = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(37, 37, 38)
            };

            Button btnPrev = new Button
            {
                Text = "◄ Previous",
                Location = new Point(250, 15),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPrev.Click += (s, e) => NavigateImage(-1);
            bottomControls.Controls.Add(btnPrev);

            Label lblProgress = new Label
            {
                Name = "lblProgress",
                Text = "0 / 0",
                Location = new Point(375, 22),
                Size = new Size(150, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            bottomControls.Controls.Add(lblProgress);

            Button btnNext = new Button
            {
                Text = "Next ►",
                Location = new Point(440, 15),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNext.Click += (s, e) => NavigateImage(1);
            bottomControls.Controls.Add(btnNext);

            // Zoom controls
            Label lblZoomValue = new Label
            {
                Name = "lblZoomValue",
                Text = "100%",
                Location = new Point(20, 22),
                AutoSize = true,
                ForeColor = Color.White
            };
            bottomControls.Controls.Add(lblZoomValue);

            Button btnZoomOut = new Button
            {
                Text = "−",
                Location = new Point(65, 18),
                Size = new Size(30, 26),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnZoomOut.Click += (s, e) => AdjustZoom(-0.1f);
            bottomControls.Controls.Add(btnZoomOut);

            Button btnZoomReset = new Button
            {
                Text = "Fit",
                Location = new Point(100, 18),
                Size = new Size(50, 26),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnZoomReset.Click += (s, e) => ResetToFit();
            bottomControls.Controls.Add(btnZoomReset);

            Button btnZoomIn = new Button
            {
                Text = "+",
                Location = new Point(155, 18),
                Size = new Size(30, 26),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnZoomIn.Click += (s, e) => AdjustZoom(0.1f);
            bottomControls.Controls.Add(btnZoomIn);

            panelCenter.Controls.Add(bottomControls);
            this.Controls.Add(panelCenter);

            // === RIGHT PANEL (Annotations) ===
            Panel panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.FromArgb(37, 37, 38)
            };

            Label lblAnnotations = new Label
            {
                Text = "📦 Annotations",
                Location = new Point(15, 15),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            panelRight.Controls.Add(lblAnnotations);

            ListBox listAnnotations = new ListBox
            {
                Name = "listAnnotations",
                Location = new Point(15, 45),
                Size = new Size(270, 350),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            listAnnotations.SelectedIndexChanged += ListAnnotations_SelectedIndexChanged;
            panelRight.Controls.Add(listAnnotations);

            Button btnDeleteBox = new Button
            {
                Text = "🗑️ Delete Selected",
                Location = new Point(15, 405),
                Size = new Size(270, 30),
                BackColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteBox.Click += BtnDeleteAnnotation_Click;
            panelRight.Controls.Add(btnDeleteBox);

            Button btnClearAll = new Button
            {
                Text = "Clear All",
                Location = new Point(15, 440),
                Size = new Size(270, 26),
                BackColor = Color.FromArgb(100, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClearAll.Click += BtnClearAll_Click;
            panelRight.Controls.Add(btnClearAll);

            Label lblExport = new Label
            {
                Text = "💾 Export",
                Location = new Point(15, 485),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            panelRight.Controls.Add(lblExport);

            Button btnExportYOLO = new Button
            {
                Text = "Export YOLO",
                Location = new Point(15, 515),
                Size = new Size(270, 32),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExportYOLO.Click += BtnExportYOLO_Click;
            panelRight.Controls.Add(btnExportYOLO);

            Button btnExportJSON = new Button
            {
                Text = "Export JSON",
                Location = new Point(15, 552),
                Size = new Size(270, 32),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExportJSON.Click += BtnExportJSON_Click;
            panelRight.Controls.Add(btnExportJSON);

            Button btnSave = new Button
            {
                Text = "💾 Save Project (Ctrl+S)",
                Location = new Point(15, 595),
                Size = new Size(270, 35),
                BackColor = Color.FromArgb(0, 150, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnSave.Click += BtnSaveProject_Click;
            panelRight.Controls.Add(btnSave);

            Label lblStats = new Label
            {
                Name = "lblStats",
                Text = "📊 Stats:\nTotal: 0 | Annotated: 0\nBoxes: 0",
                Location = new Point(15, 645),
                Size = new Size(270, 60),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8)
            };
            panelRight.Controls.Add(lblStats);

            Button btnTogglePreview = new Button
            {
                Name = "btnTogglePreview",
                Text = "👁️ Show ROI Preview",
                Location = new Point(15, 720),
                Size = new Size(270, 28),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTogglePreview.Click += BtnTogglePreview_Click;
            panelRight.Controls.Add(btnTogglePreview);

            this.Controls.Add(panelRight);

            // === PREVIEW PANEL ===
            Panel panelPreview = new Panel
            {
                Name = "panelPreview",
                Dock = DockStyle.Bottom,
                Height = 150,
                BackColor = Color.FromArgb(37, 37, 38),
                Visible = false
            };

            Label lblPreview = new Label
            {
                Text = "📸 ROI Preview",
                Location = new Point(15, 10),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            panelPreview.Controls.Add(lblPreview);

            FlowLayoutPanel flowPreview = new FlowLayoutPanel
            {
                Name = "flowPreview",
                Location = new Point(15, 35),
                Size = new Size(panelPreview.Width - 30, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 30),
                WrapContents = false
            };
            panelPreview.Controls.Add(flowPreview);

            panelRight.Controls.Add(panelPreview);
        }

        private void InitializeProject()
        {
            project = new LabelingProject
            {
                ProjectName = "New Project",
                ProjectPath = ""
            };

            project.Classes.Add("screw_good");
            project.Classes.Add("screw_ng");
            project.Classes.Add("solder_ok");
            project.Classes.Add("solder_ng");
            project.Classes.Add("label_good");
            project.Classes.Add("label_ng");

            UpdateClassList();
        }

        // === ZOOM & DISPLAY ===
        private float CalculateFitZoom()
        {
            if (currentImage == null || imagePanel == null)
                return 1.0f;

            int availableWidth = imagePanel.ClientSize.Width - 40;
            int availableHeight = imagePanel.ClientSize.Height - 40;

            if (availableWidth <= 0 || availableHeight <= 0)
                return 1.0f;

            float scaleX = (float)availableWidth / currentImage.Width;
            float scaleY = (float)availableHeight / currentImage.Height;

            // Use MINIMUM to fit entire image
            return Math.Min(scaleX, scaleY);
        }

        private void ResetToFit()
        {
            if (currentImage == null) return;

            float fitZoom = CalculateFitZoom();
            zoomLevel = fitZoom;
            minZoomLevel = fitZoom;

            UpdateImageDisplay();
            CenterImage();

            var lblZoomValue = this.Controls.Find("lblZoomValue", true).FirstOrDefault() as Label;
            if (lblZoomValue != null)
                lblZoomValue.Text = $"{(int)(zoomLevel * 100)}%";
        }

        private void AdjustZoom(float delta)
        {
            float newZoom = zoomLevel + delta;

            // Limit to fit zoom
            if (newZoom < minZoomLevel) newZoom = minZoomLevel;
            if (newZoom > 10.0f) newZoom = 10.0f;

            SetZoom(newZoom);
        }

        private void SetZoom(float zoom)
        {
            if (zoom < minZoomLevel) zoom = minZoomLevel;
            if (zoom > 10.0f) zoom = 10.0f;

            zoomLevel = zoom;
            UpdateImageDisplay();

            // Center if at fit level
            if (Math.Abs(zoom - minZoomLevel) < 0.01f)
            {
                CenterImage();
            }

            var lblZoomValue = this.Controls.Find("lblZoomValue", true).FirstOrDefault() as Label;
            if (lblZoomValue != null)
                lblZoomValue.Text = $"{(int)(zoomLevel * 100)}%";
        }

        private void UpdateImageDisplay()
        {
            if (currentImage == null) return;

            int newWidth = (int)(currentImage.Width * zoomLevel);
            int newHeight = (int)(currentImage.Height * zoomLevel);


            pictureBox.Size = new Size(newWidth, newHeight);
            if (pictureBox.Image != null && pictureBox.Image != currentImage)
            {
                pictureBox.Image.Dispose();
            }

            pictureBox.Image = currentImage;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Invalidate();
        }

        private void CenterImage()
        {
            if (pictureBox == null || imagePanel == null)
                return;

            int x = Math.Max(0, (imagePanel.ClientSize.Width - pictureBox.Width) / 2);
            int y = Math.Max(0, (imagePanel.ClientSize.Height - pictureBox.Height) / 2);

            pictureBox.Location = new Point(x, y);
        }

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (currentImage == null) return;

            // ===== REMOVE CTRL CHECK - Direct zoom on scroll =====
            // OLD: if ((Control.ModifierKeys & Keys.Control) == Keys.Control)

            // Save old zoom
            float oldZoom = zoomLevel;

            // Calculate mouse position in IMAGE coordinates (before zoom)
            Point mousePictureBox = e.Location;
            float imgX = mousePictureBox.X / oldZoom;
            float imgY = mousePictureBox.Y / oldZoom;

            // Adjust zoom
            float delta = e.Delta > 0 ? 0.1f : -0.1f;
            float newZoom = zoomLevel + delta;

            // Clamp zoom
            if (newZoom < minZoomLevel) newZoom = minZoomLevel;
            if (newZoom > 10.0f) newZoom = 10.0f;

            zoomLevel = newZoom;

            // Update display
            int newWidth = (int)(currentImage.Width * zoomLevel);
            int newHeight = (int)(currentImage.Height * zoomLevel);
            pictureBox.Size = new Size(newWidth, newHeight);

            // Calculate new position to keep mouse point stationary
            float newMouseX = imgX * zoomLevel;
            float newMouseY = imgY * zoomLevel;

            // Adjust pictureBox position so mouse stays over same image point
            int offsetX = mousePictureBox.X - (int)newMouseX;
            int offsetY = mousePictureBox.Y - (int)newMouseY;

            pictureBox.Location = new Point(
                pictureBox.Location.X + offsetX,
                pictureBox.Location.Y + offsetY
            );

            // Update zoom label
            var lblZoomValue = this.Controls.Find("lblZoomValue", true).FirstOrDefault() as Label;
            if (lblZoomValue != null)
                lblZoomValue.Text = $"{(int)(zoomLevel * 100)}%";

            pictureBox.Invalidate();

            // Mark as handled
            if (e is HandledMouseEventArgs hme)
            {
                hme.Handled = true;
            }
        }

        private void ImagePanel_Resize(object sender, EventArgs e)
        {
            if (currentImage != null)
            {
                // Recalculate fit zoom
                float newFit = CalculateFitZoom();

                // If at fit level, update
                if (Math.Abs(zoomLevel - minZoomLevel) < 0.05f)
                {
                    minZoomLevel = newFit;
                    zoomLevel = newFit;
                    UpdateImageDisplay();
                    CenterImage();

                    var lblZoomValue = this.Controls.Find("lblZoomValue", true).FirstOrDefault() as Label;
                    if (lblZoomValue != null)
                        lblZoomValue.Text = $"{(int)(zoomLevel * 100)}%";
                }
                else
                {
                    minZoomLevel = newFit;
                }
            }
        }

        // === LOAD IMAGES ===
        private void BtnLoadImages_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select folder containing images";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadImagesFromFolder(dialog.SelectedPath);
                }
            }
        }

        private void LoadImagesFromFolder(string folderPath)
        {
            try
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
                imageFiles = Directory.GetFiles(folderPath)
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToList();

                if (imageFiles.Count == 0)
                {
                    MessageBox.Show("No images found", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var listImages = this.Controls.Find("listImages", true)[0] as ListBox;
                listImages.Items.Clear();

                foreach (var file in imageFiles)
                    listImages.Items.Add(Path.GetFileName(file));

                currentImageIndex = 0;
                LoadCurrentImage();
                UpdateStatistics();

                Logger.LogInfo($"Loaded {imageFiles.Count} images");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("Failed to load images", ex);
            }
        }

        // === IMAGE NAVIGATION ===
        private void ListImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listImages = sender as ListBox;
            if (listImages.SelectedIndex >= 0)
            {
                currentImageIndex = listImages.SelectedIndex;
                LoadCurrentImage();
            }
        }

        private void NavigateImage(int direction)
        {
            if (imageFiles == null || imageFiles.Count == 0) return;

            SaveCurrentAnnotation();

            currentImageIndex += direction;
            if (currentImageIndex < 0) currentImageIndex = imageFiles.Count - 1;
            if (currentImageIndex >= imageFiles.Count) currentImageIndex = 0;

            LoadCurrentImage();

            var listImages = this.Controls.Find("listImages", true)[0] as ListBox;
            listImages.SelectedIndex = currentImageIndex;
        }

        private void LoadCurrentImage()
        {
            if (imageFiles == null || imageFiles.Count == 0) return;

            try
            {
                if (currentImage != null)
                    currentImage.Dispose();

                currentImage = new Bitmap(imageFiles[currentImageIndex]);

                // Calculate and apply FIT zoom
                float fitZoom = CalculateFitZoom();
                MessageBox.Show($"Image: {currentImage.Width}x{currentImage.Height}\n" +
                   $"Panel: {imagePanel.ClientSize.Width}x{imagePanel.ClientSize.Height}\n" +
                   $"Fit Zoom: {fitZoom:F3}");

                zoomLevel = fitZoom;
                minZoomLevel = fitZoom;

                UpdateImageDisplay();
                CenterImage();

                LoadAnnotationForCurrentImage();

                var lblProgress = this.Controls.Find("lblProgress", true)[0] as Label;
                lblProgress.Text = $"{currentImageIndex + 1} / {imageFiles.Count}";

                var lblZoomValue = this.Controls.Find("lblZoomValue", true).FirstOrDefault() as Label;
                if (lblZoomValue != null)
                    lblZoomValue.Text = $"{(int)(zoomLevel * 100)}%";

                pictureBox.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("Failed to load image", ex);
            }
        }

        private void LoadAnnotationForCurrentImage()
        {
            string imagePath = imageFiles[currentImageIndex];
            currentAnnotation = project.Annotations.FirstOrDefault(a => a.ImagePath == imagePath);

            if (currentAnnotation == null)
            {
                currentAnnotation = new ImageAnnotation
                {
                    ImagePath = imagePath,
                    ImageWidth = currentImage.Width,
                    ImageHeight = currentImage.Height
                };
                project.Annotations.Add(currentAnnotation);
            }

            UpdateAnnotationList();
            UpdateStatistics();
        }

        private void SaveCurrentAnnotation()
        {
            // Already saved in currentAnnotation object
        }

        private void UpdateAnnotationList()
        {
            var listAnnotations = this.Controls.Find("listAnnotations", true)[0] as ListBox;
            listAnnotations.Items.Clear();

            if (currentAnnotation != null)
            {
                for (int i = 0; i < currentAnnotation.Boxes.Count; i++)
                {
                    var box = currentAnnotation.Boxes[i];
                    listAnnotations.Items.Add($"#{i + 1} {box.ClassName} ({box.Width}x{box.Height})");
                }
            }

            var panelPreview = this.Controls.Find("panelPreview", true).FirstOrDefault() as Panel;
            if (panelPreview != null && panelPreview.Visible)
            {
                UpdateROIPreview();
            }
        }

        private void ListAnnotations_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listAnnotations = sender as ListBox;
            selectedBoxIndex = listAnnotations.SelectedIndex;
            pictureBox.Invalidate();
        }

        private void UpdateStatistics()
        {
            var lblStats = this.Controls.Find("lblStats", true).FirstOrDefault() as Label;
            if (lblStats != null && imageFiles != null)
            {
                int total = imageFiles.Count;
                int annotated = project.Annotations.Count(a => a.Boxes.Count > 0);
                int totalBoxes = project.Annotations.Sum(a => a.Boxes.Count);

                lblStats.Text = $"📊 Stats:\nTotal: {total} | Annotated: {annotated}\nBoxes: {totalBoxes}";
            }
        }

        // === CLASS MANAGEMENT ===
        private void UpdateClassList()
        {
            var listClasses = this.Controls.Find("listClasses", true)[0] as ListBox;
            listClasses.Items.Clear();

            for (int i = 0; i < project.Classes.Count; i++)
            {
                listClasses.Items.Add($"{i + 1}. {project.Classes[i]}");
            }

            if (project.Classes.Count > 0)
                listClasses.SelectedIndex = 0;
        }

        private void ListClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listClasses = sender as ListBox;
            if (listClasses.SelectedIndex >= 0)
            {
                selectedClass = project.Classes[listClasses.SelectedIndex];
                selectedClassId = listClasses.SelectedIndex;
            }
        }

        private void BtnAddClass_Click(object sender, EventArgs e)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Add Class",
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            Label textLabel = new Label()
            {
                Left = 20,
                Top = 20,
                Text = "Enter class name:",
                ForeColor = Color.White,
                AutoSize = true
            };

            TextBox textBox = new TextBox()
            {
                Left = 20,
                Top = 50,
                Width = 340,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };

            Button btnOK = new Button()
            {
                Text = "OK",
                Left = 260,
                Width = 100,
                Top = 80,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOK.Click += (s, ev) => { prompt.Close(); };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(btnOK);
            prompt.AcceptButton = btnOK;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                string className = textBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(className))
                {
                    if (!project.Classes.Contains(className))
                    {
                        project.Classes.Add(className);
                        UpdateClassList();
                        Logger.LogInfo($"Added class: {className}");
                    }
                }
            }
        }

        // === DRAWING & EDITING ===
        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentImage == null) return;

            // Pan with Space or Middle Mouse
            if (e.Button == MouseButtons.Middle ||
       (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Space) == Keys.Space))
            {
                // Allow pan anytime (not just when zoomed)
                isPanning = true;
                panStartPoint = e.Location;
                pictureBox.Cursor = Cursors.Hand;
                return;
            }

            Point imgPoint = ScreenToImage(e.Location);

            if (e.Button == MouseButtons.Left)
            {
                editHandle = GetHandleAtPoint(imgPoint, out int boxIndex);

                if (editHandle >= 0)
                {
                    isEditing = true;
                    selectedBoxIndex = boxIndex;
                    startPoint = imgPoint;
                    return;
                }

                int clickedBox = GetBoxAtPoint(imgPoint);
                if (clickedBox >= 0)
                {
                    isEditing = true;
                    editHandle = 8;
                    selectedBoxIndex = clickedBox;
                    startPoint = imgPoint;

                    var listAnnotations = this.Controls.Find("listAnnotations", true)[0] as ListBox;
                    listAnnotations.SelectedIndex = clickedBox;
                    return;
                }

                if (string.IsNullOrEmpty(selectedClass))
                {
                    MessageBox.Show("Please select a class first!", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                isDrawing = true;
                startPoint = imgPoint;
                endPoint = imgPoint;
            }
            else if (e.Button == MouseButtons.Right)
            {
                DeleteBoxAtPosition(imgPoint);
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                int dx = e.X - panStartPoint.X;
                int dy = e.Y - panStartPoint.Y;

                // Calculate new position
                int newX = pictureBox.Location.X + dx;
                int newY = pictureBox.Location.Y + dy;

                // Set location directly (smooth)
                pictureBox.Location = new Point(newX, newY);

                // Update pan start for next move
                panStartPoint = e.Location;
                return;
            }

            Point imgPoint = ScreenToImage(e.Location);

            if (isDrawing)
            {
                endPoint = imgPoint;
                pictureBox.Invalidate();
            }
            else if (isEditing && selectedBoxIndex >= 0)
            {
                var box = currentAnnotation.Boxes[selectedBoxIndex];
                Rectangle rect = box.ToRectangle();

                int dx = imgPoint.X - startPoint.X;
                int dy = imgPoint.Y - startPoint.Y;

                switch (editHandle)
                {
                    case 0:
                        rect.X += dx; rect.Y += dy;
                        rect.Width -= dx; rect.Height -= dy;
                        break;
                    case 1:
                        rect.Y += dy; rect.Width += dx; rect.Height -= dy;
                        break;
                    case 2:
                        rect.Width += dx; rect.Height += dy;
                        break;
                    case 3:
                        rect.X += dx; rect.Width -= dx; rect.Height += dy;
                        break;
                    case 4: rect.Y += dy; rect.Height -= dy; break;
                    case 5: rect.Width += dx; break;
                    case 6: rect.Height += dy; break;
                    case 7: rect.X += dx; rect.Width -= dx; break;
                    case 8: rect.X += dx; rect.Y += dy; break;
                }

                if (rect.Width < 10) rect.Width = 10;
                if (rect.Height < 10) rect.Height = 10;

                var newBox = new BoundingBox(box.ClassName, box.ClassId, rect,
                    currentImage.Width, currentImage.Height);
                currentAnnotation.Boxes[selectedBoxIndex] = newBox;

                startPoint = imgPoint;
                pictureBox.Invalidate();
            }
            else
            {
                UpdateCursor(imgPoint);
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                isPanning = false;
                pictureBox.Cursor = Cursors.Default;
                return;
            }

            if (isDrawing && e.Button == MouseButtons.Left)
            {
                isDrawing = false;
                Point imgPoint = ScreenToImage(e.Location);
                endPoint = imgPoint;

                Rectangle rect = NormalizeRectangle(startPoint, endPoint);

                if (rect.Width > 5 && rect.Height > 5)
                {
                    var box = new BoundingBox(selectedClass, selectedClassId, rect,
                        currentImage.Width, currentImage.Height);
                    currentAnnotation.Boxes.Add(box);

                    UpdateAnnotationList();
                    UpdateStatistics();
                    pictureBox.Invalidate();

                    Logger.LogInfo($"Added box #{currentAnnotation.Boxes.Count}: {selectedClass}");
                }
            }

            if (isEditing)
            {
                isEditing = false;
                editHandle = -1;
                UpdateAnnotationList();
                UpdateStatistics();
            }
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (currentAnnotation == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            for (int i = 0; i < currentAnnotation.Boxes.Count; i++)
            {
                var box = currentAnnotation.Boxes[i];
                Rectangle rect = ImageToScreen(box.ToRectangle());

                bool isSelected = (i == selectedBoxIndex);
                Color boxColor = box.IsDefect ? Color.FromArgb(255, 100, 100) : Color.FromArgb(100, 255, 100);

                if (isSelected)
                    boxColor = Color.Yellow;

                using (Pen pen = new Pen(boxColor, isSelected ? 3 : 2))
                {
                    g.DrawRectangle(pen, rect);
                }

                string number = $"#{i + 1}";
                using (Font font = new Font("Arial", 9, FontStyle.Bold))
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, boxColor)))
                using (SolidBrush textBrush = new SolidBrush(Color.Black))
                {
                    SizeF numSize = g.MeasureString(number, font);
                    int badgeSize = (int)Math.Max(numSize.Width, numSize.Height) + 6;

                    g.FillEllipse(bgBrush, rect.X - badgeSize / 2, rect.Y - badgeSize / 2, badgeSize, badgeSize);
                    g.DrawString(number, font, textBrush,
                        rect.X - numSize.Width / 2,
                        rect.Y - numSize.Height / 2);
                }

                using (Font font = new Font("Segoe UI", 9, FontStyle.Bold))
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(200, boxColor)))
                using (SolidBrush textBrush = new SolidBrush(Color.Black))
                {
                    SizeF textSize = g.MeasureString(box.ClassName, font);
                    g.FillRectangle(bgBrush, rect.X, rect.Y - textSize.Height - 4, textSize.Width + 6, textSize.Height + 4);
                    g.DrawString(box.ClassName, font, textBrush, rect.X + 3, rect.Y - textSize.Height - 2);
                }

                if (isSelected)
                {
                    DrawHandles(g, rect);
                }
            }

            if (isDrawing)
            {
                Rectangle rect = ImageToScreen(NormalizeRectangle(startPoint, endPoint));
                using (Pen pen = new Pen(Color.Yellow, 2))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawRectangle(pen, rect);
                }
            }
        }

        private void DrawHandles(Graphics g, Rectangle rect)
        {
            int handleSize = 8;
            Point[] handles = new Point[]
            {
                new Point(rect.Left, rect.Top),
                new Point(rect.Right, rect.Top),
                new Point(rect.Right, rect.Bottom),
                new Point(rect.Left, rect.Bottom),
                new Point(rect.Left + rect.Width/2, rect.Top),
                new Point(rect.Right, rect.Top + rect.Height/2),
                new Point(rect.Left + rect.Width/2, rect.Bottom),
                new Point(rect.Left, rect.Top + rect.Height/2)
            };

            using (SolidBrush brush = new SolidBrush(Color.Yellow))
            using (Pen pen = new Pen(Color.Black, 1))
            {
                foreach (Point handle in handles)
                {
                    Rectangle handleRect = new Rectangle(
                        handle.X - handleSize / 2,
                        handle.Y - handleSize / 2,
                        handleSize, handleSize
                    );
                    g.FillRectangle(brush, handleRect);
                    g.DrawRectangle(pen, handleRect);
                }
            }
        }

        // === COORDINATE CONVERSION ===
        private Point ScreenToImage(Point screenPoint)
        {
            int x = (int)(screenPoint.X / zoomLevel);
            int y = (int)(screenPoint.Y / zoomLevel);
            return new Point(x, y);
        }

        private Point ImageToScreen(Point imagePoint)
        {
            int x = (int)(imagePoint.X * zoomLevel);
            int y = (int)(imagePoint.Y * zoomLevel);
            return new Point(x, y);
        }

        private Rectangle ImageToScreen(Rectangle imageRect)
        {
            return new Rectangle(
                (int)(imageRect.X * zoomLevel),
                (int)(imageRect.Y * zoomLevel),
                (int)(imageRect.Width * zoomLevel),
                (int)(imageRect.Height * zoomLevel)
            );
        }

        private Rectangle NormalizeRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p2.X - p1.X),
                Math.Abs(p2.Y - p1.Y)
            );
        }

        // === EDIT HELPERS ===
        private int GetHandleAtPoint(Point pt, out int boxIndex)
        {
            boxIndex = -1;
            int handleSize = 12;

            for (int i = 0; i < currentAnnotation.Boxes.Count; i++)
            {
                Rectangle rect = currentAnnotation.Boxes[i].ToRectangle();

                Point[] handles = new Point[]
                {
                    new Point(rect.Left, rect.Top),
                    new Point(rect.Right, rect.Top),
                    new Point(rect.Right, rect.Bottom),
                    new Point(rect.Left, rect.Bottom),
                    new Point(rect.Left + rect.Width/2, rect.Top),
                    new Point(rect.Right, rect.Top + rect.Height/2),
                    new Point(rect.Left + rect.Width/2, rect.Bottom),
                    new Point(rect.Left, rect.Top + rect.Height/2)
                };

                for (int h = 0; h < handles.Length; h++)
                {
                    Rectangle handleRect = new Rectangle(
                        handles[h].X - handleSize / 2,
                        handles[h].Y - handleSize / 2,
                        handleSize, handleSize
                    );

                    if (handleRect.Contains(pt))
                    {
                        boxIndex = i;
                        return h;
                    }
                }
            }

            return -1;
        }

        private int GetBoxAtPoint(Point pt)
        {
            for (int i = currentAnnotation.Boxes.Count - 1; i >= 0; i--)
            {
                Rectangle rect = currentAnnotation.Boxes[i].ToRectangle();
                if (rect.Contains(pt))
                    return i;
            }
            return -1;
        }

        private void UpdateCursor(Point imgPoint)
        {
            int handle = GetHandleAtPoint(imgPoint, out int _);

            if (handle >= 0)
            {
                Cursor[] cursors = new Cursor[]
                {
                    Cursors.SizeNWSE,
                    Cursors.SizeNESW,
                    Cursors.SizeNWSE,
                    Cursors.SizeNESW,
                    Cursors.SizeNS,
                    Cursors.SizeWE,
                    Cursors.SizeNS,
                    Cursors.SizeWE
                };
                pictureBox.Cursor = cursors[handle];
            }
            else if (GetBoxAtPoint(imgPoint) >= 0)
            {
                pictureBox.Cursor = Cursors.SizeAll;
            }
            else
            {
                pictureBox.Cursor = Cursors.Cross;
            }
        }

        // === DELETE ===
        private void DeleteBoxAtPosition(Point imgPoint)
        {
            int boxIndex = GetBoxAtPoint(imgPoint);
            if (boxIndex >= 0)
            {
                currentAnnotation.Boxes.RemoveAt(boxIndex);
                UpdateAnnotationList();
                UpdateStatistics();
                pictureBox.Invalidate();
                Logger.LogInfo($"Deleted box #{boxIndex + 1}");
            }
        }

        private void BtnDeleteAnnotation_Click(object sender, EventArgs e)
        {
            var listAnnotations = this.Controls.Find("listAnnotations", true)[0] as ListBox;
            if (listAnnotations.SelectedIndex >= 0 && currentAnnotation != null)
            {
                currentAnnotation.Boxes.RemoveAt(listAnnotations.SelectedIndex);
                UpdateAnnotationList();
                UpdateStatistics();
                pictureBox.Invalidate();
                Logger.LogInfo("Deleted selected box");
            }
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            if (currentAnnotation != null && currentAnnotation.Boxes.Count > 0)
            {
                var result = MessageBox.Show(
                    $"Delete all {currentAnnotation.Boxes.Count} boxes from this image?",
                    "Confirm Clear All",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    currentAnnotation.Boxes.Clear();
                    UpdateAnnotationList();
                    UpdateStatistics();
                    pictureBox.Invalidate();
                    Logger.LogInfo("Cleared all boxes");
                }
            }
        }

        // === KEYBOARD SHORTCUTS ===
        private void LabelingForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                case Keys.D:
                    if (!e.Control) NavigateImage(1);
                    e.Handled = true;
                    break;

                case Keys.Left:
                case Keys.A:
                    if (!e.Control) NavigateImage(-1);
                    e.Handled = true;
                    break;

                case Keys.Delete:
                    if (selectedBoxIndex >= 0)
                        BtnDeleteAnnotation_Click(null, null);
                    e.Handled = true;
                    break;

                case Keys.Z:
                    if (e.Control) UndoLastBox();
                    e.Handled = true;
                    break;

                case Keys.S:
                    if (e.Control) QuickSave();
                    e.Handled = true;
                    break;

                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                    int classIndex = (int)e.KeyCode - (int)Keys.D1;
                    SelectClassByIndex(classIndex);
                    e.Handled = true;
                    break;

                case Keys.Oemplus:
                case Keys.Add:
                    AdjustZoom(0.1f);
                    e.Handled = true;
                    break;

                case Keys.OemMinus:
                case Keys.Subtract:
                    AdjustZoom(-0.1f);
                    e.Handled = true;
                    break;
            }
        }

        private void SelectClassByIndex(int index)
        {
            var listClasses = this.Controls.Find("listClasses", true)[0] as ListBox;
            if (listClasses != null && index < project.Classes.Count)
            {
                listClasses.SelectedIndex = index;
            }
        }

        private void UndoLastBox()
        {
            if (currentAnnotation != null && currentAnnotation.Boxes.Count > 0)
            {
                currentAnnotation.Boxes.RemoveAt(currentAnnotation.Boxes.Count - 1);
                UpdateAnnotationList();
                UpdateStatistics();
                pictureBox.Invalidate();

                this.Text = "Labeling Tool - Undo!";
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
                {
                    if (!this.IsDisposed)
                        this.Invoke(new Action(() => this.Text = "Labeling Tool - LabelStudio Style"));
                });
            }
        }

        private string lastSavedPath = "";

        private void QuickSave()
        {
            if (string.IsNullOrEmpty(lastSavedPath))
            {
                BtnSaveProject_Click(null, null);
            }
            else
            {
                try
                {
                    SaveCurrentAnnotation();
                    AnnotationExporter.SaveProject(project, lastSavedPath);

                    this.Text = "Labeling Tool - Saved! ✓";
                    System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
                    {
                        if (!this.IsDisposed)
                            this.Invoke(new Action(() => this.Text = "Labeling Tool - LabelStudio Style"));
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Save failed: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // === EXPORT ===
        private void BtnExportYOLO_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select output folder for YOLO export";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        SaveCurrentAnnotation();
                        AnnotationExporter.ExportToYOLO(project, dialog.SelectedPath);

                        MessageBox.Show(
                            $"YOLO export completed!\n\n" +
                            $"Files created:\n" +
                            $"- classes.txt\n" +
                            $"- labels/*.txt ({project.Annotations.Count} files)\n\n" +
                            $"Location: {dialog.SelectedPath}",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        Logger.LogInfo($"YOLO export to {dialog.SelectedPath}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.LogError("YOLO export failed", ex);
                    }
                }
            }
        }

        private void BtnExportJSON_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON Files (*.json)|*.json";
                dialog.FileName = "annotations.json";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        SaveCurrentAnnotation();
                        AnnotationExporter.ExportToJSON(project, dialog.FileName);

                        MessageBox.Show("JSON export completed!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Logger.LogInfo($"JSON export to {dialog.FileName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.LogError("JSON export failed", ex);
                    }
                }
            }
        }

        private void BtnSaveProject_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Project Files (*.json)|*.json";
                dialog.FileName = string.IsNullOrEmpty(lastSavedPath)
                    ? "project.json"
                    : Path.GetFileName(lastSavedPath);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        SaveCurrentAnnotation();
                        project.ProjectPath = dialog.FileName;
                        lastSavedPath = dialog.FileName;
                        AnnotationExporter.SaveProject(project, dialog.FileName);

                        MessageBox.Show("Project saved successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Logger.LogInfo($"Project saved to {dialog.FileName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Save failed: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.LogError("Project save failed", ex);
                    }
                }
            }
        }

        private void BtnTogglePreview_Click(object sender, EventArgs e)
        {
            var panelPreview = this.Controls.Find("panelPreview", true).FirstOrDefault() as Panel;
            var btn = sender as Button;

            if (panelPreview != null)
            {
                panelPreview.Visible = !panelPreview.Visible;
                btn.Text = panelPreview.Visible ? "👁️ Hide ROI Preview" : "👁️ Show ROI Preview";

                if (panelPreview.Visible)
                    UpdateROIPreview();
            }
        }

        private void UpdateROIPreview()
        {
            var flowPreview = this.Controls.Find("flowPreview", true).FirstOrDefault() as FlowLayoutPanel;
            if (flowPreview == null || currentImage == null || currentAnnotation == null)
                return;

            flowPreview.Controls.Clear();

            foreach (var box in currentAnnotation.Boxes)
            {
                try
                {
                    Rectangle rect = box.ToRectangle();

                    if (rect.X < 0) rect.X = 0;
                    if (rect.Y < 0) rect.Y = 0;
                    if (rect.Right > currentImage.Width) rect.Width = currentImage.Width - rect.X;
                    if (rect.Bottom > currentImage.Height) rect.Height = currentImage.Height - rect.Y;

                    if (rect.Width <= 0 || rect.Height <= 0)
                        continue;

                    Bitmap crop = new Bitmap(rect.Width, rect.Height);
                    using (Graphics g = Graphics.FromImage(crop))
                    {
                        g.DrawImage(currentImage,
                            new Rectangle(0, 0, rect.Width, rect.Height),
                            rect, GraphicsUnit.Pixel);
                    }

                    Bitmap thumb = new Bitmap(100, 100);
                    using (Graphics g = Graphics.FromImage(thumb))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(crop, 0, 0, 100, 100);
                    }

                    Panel previewBox = new Panel
                    {
                        Size = new Size(110, 110),
                        BackColor = box.IsDefect ? Color.FromArgb(80, 0, 0) : Color.FromArgb(0, 80, 0),
                        Margin = new Padding(5)
                    };

                    PictureBox pb = new PictureBox
                    {
                        Image = thumb,
                        SizeMode = PictureBoxSizeMode.CenterImage,
                        Size = new Size(100, 80),
                        Location = new Point(5, 5)
                    };
                    previewBox.Controls.Add(pb);

                    Label lbl = new Label
                    {
                        Text = box.ClassName,
                        Location = new Point(5, 85),
                        Size = new Size(100, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 7)
                    };
                    previewBox.Controls.Add(lbl);

                    flowPreview.Controls.Add(previewBox);

                    crop.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.LogError("ROI preview error", ex);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            SaveCurrentAnnotation();

            if (currentImage != null)
                currentImage.Dispose();
        }
    }
}