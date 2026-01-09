using System;
using System.Drawing;
using System.Windows.Forms;
using InspectionAI.Classes.Managers;
using InspectionAI.Classes.Hardware;

namespace InspectionAI.Forms
{
    /// <summary>
    /// Dashboard Form dengan charts
    /// </summary>
    public partial class DashboardForm : Form
    {
        private ProductionCounter counter;
        private Timer refreshTimer;

        public DashboardForm(ProductionCounter productionCounter)
        {
            counter = productionCounter;
            InitializeComponent();
            StartAutoRefresh();
        }

        private void InitializeComponent()
        {
            this.Text = "Production Dashboard";
            this.Size = new Size(1000, 700);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main panel dengan custom paint
            Panel panelMain = new Panel
            {
                Name = "panelMain",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            panelMain.Paint += PanelMain_Paint;
            this.Controls.Add(panelMain);

            // Bottom panel dengan buttons
            Panel panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            Button btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(20, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => RefreshCharts();
            panelBottom.Controls.Add(btnRefresh);

            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(860, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();
            panelBottom.Controls.Add(btnClose);

            this.Controls.Add(panelBottom);
        }

        private void PanelMain_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var stats = counter.GetStatistics();

            // Title
            using (Font titleFont = new Font("Segoe UI", 24, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                g.DrawString("Production Dashboard", titleFont, brush, 20, 20);
            }

            // Statistics summary
            using (Font font = new Font("Segoe UI", 12))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                string summary = $"Session: {stats.SessionDuration.Hours}h {stats.SessionDuration.Minutes}m | " +
                                $"Total: {stats.TotalCount} | Good: {stats.GoodCount} | NG: {stats.NgCount}";
                g.DrawString(summary, font, brush, 20, 65);
            }

            // === PIE CHART (Good/NG Ratio) ===
            DrawPieChartSection(g, new Rectangle(50, 120, 250, 250));

            // === GAUGE (Pass Rate) ===
            DrawGaugeSection(g, new Rectangle(350, 120, 250, 250));

            // === BAR CHART (Hourly production) ===
            DrawBarChartSection(g, new Rectangle(650, 120, 300, 250));

            // === LINE CHART (Trend) ===
            DrawLineChartSection(g, new Rectangle(50, 400, 900, 200));
        }

        private void DrawPieChartSection(Graphics g, Rectangle bounds)
        {
            var stats = counter.GetStatistics();

            // Title
            using (Font font = new Font("Segoe UI", 14, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                g.DrawString("Good/NG Ratio", font, brush, bounds.X, bounds.Y - 30);
            }

            // Chart
            ChartManager.DrawPieChart(g, bounds, stats.GoodCount, stats.NgCount, stats.WarningCount);

            // Legend
            int legendY = bounds.Bottom + 20;
            DrawLegendItem(g, bounds.X, legendY, Color.FromArgb(0, 150, 0), $"Good: {stats.GoodCount} ({stats.GoodRate:F1}%)");
            DrawLegendItem(g, bounds.X, legendY + 25, Color.FromArgb(200, 0, 0), $"NG: {stats.NgCount} ({stats.NgRate:F1}%)");
        }

        private void DrawGaugeSection(Graphics g, Rectangle bounds)
        {
            var stats = counter.GetStatistics();

            // Title
            using (Font font = new Font("Segoe UI", 14, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                g.DrawString("Pass Rate", font, brush, bounds.X, bounds.Y - 30);
            }

            // Gauge
            ChartManager.DrawGauge(g, bounds, (float)stats.GoodRate);
        }

        private void DrawBarChartSection(Graphics g, Rectangle bounds)
        {
            // Title
            using (Font font = new Font("Segoe UI", 14, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                g.DrawString("Last 10 Batches", font, brush, bounds.X, bounds.Y - 30);
            }

            // Sample data (TODO: Replace dengan real data dari database)
            var values = new System.Collections.Generic.List<int> { 45, 52, 48, 50, 47, 55, 49, 51, 53, 50 };
            int maxValue = 60;

            ChartManager.DrawBarChart(g, bounds, values, maxValue, Color.FromArgb(0, 122, 204));
        }

        private void DrawLineChartSection(Graphics g, Rectangle bounds)
        {
            // Title
            using (Font font = new Font("Segoe UI", 14, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                g.DrawString("Production Trend", font, brush, bounds.X, bounds.Y - 30);
            }

            // Sample data (TODO: Replace dengan real data)
            var values = new System.Collections.Generic.List<float> { 92.5f, 93.2f, 91.8f, 94.5f, 95.1f, 93.8f, 94.2f, 95.5f, 94.8f, 95.2f };
            float maxValue = 100f;

            ChartManager.DrawLineChart(g, bounds, values, maxValue, Color.FromArgb(0, 200, 100));

            // Y-axis labels
            using (Font font = new Font("Segoe UI", 9))
            using (SolidBrush brush = new SolidBrush(Color.Gray))
            {
                for (int i = 0; i <= 4; i++)
                {
                    float value = maxValue * (4 - i) / 4f;
                    int y = bounds.Y + (i * bounds.Height / 4) - 8;
                    g.DrawString($"{value:F0}%", font, brush, bounds.X - 35, y);
                }
            }
        }

        private void DrawLegendItem(Graphics g, int x, int y, Color color, string text)
        {
            // Color box
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, x, y, 15, 15);
            }

            // Text
            using (Font font = new Font("Segoe UI", 10))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                g.DrawString(text, font, brush, x + 20, y - 2);
            }
        }

        private void StartAutoRefresh()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 5000; // Refresh every 5 seconds
            refreshTimer.Tick += (s, e) => RefreshCharts();
            refreshTimer.Start();
        }

        private void RefreshCharts()
        {
            var panelMain = this.Controls.Find("panelMain", false)[0];
            if (panelMain != null)
            {
                panelMain.Invalidate(); // Trigger repaint
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (refreshTimer != null)
            {
                refreshTimer.Stop();
                refreshTimer.Dispose();
            }
        }

        // Add PictureBox untuk mini chart
        private void InitializeMiniChart()
        {
            // Create PictureBox for chart (di tab Statistics atau panel kanan)
            PictureBox pbChart = new PictureBox
            {
                Name = "pbMiniChart",
                Size = new Size(300, 200),
                Location = new Point(20, 20), // Adjust sesuai layout
                BackColor = Color.FromArgb(37, 37, 38)
            };
            pbChart.Paint += PbChart_Paint;

            // Add ke tab atau panel
            // tabPageStatistics.Controls.Add(pbChart); // Kalau ada tab statistics
            // Atau add ke panel lain yang tersedia
        }

        private void PbChart_Paint(object sender, PaintEventArgs e)
        {
            if (counter == null)
                return;

            var stats = counter.GetStatistics();
            Rectangle bounds = new Rectangle(10, 10, 180, 180);

            // Draw mini pie chart
            ChartManager.DrawPieChart(e.Graphics, bounds,
                stats.GoodCount, stats.NgCount, stats.WarningCount);
        }

        // Update chart saat inspection selesai
        private void UpdateMiniChart()
        {
            var pbChart = this.Controls.Find("pbMiniChart", true);
            if (pbChart.Length > 0 && pbChart[0] is PictureBox)
            {
                pbChart[0].Invalidate(); // Trigger repaint
            }
        }
    }
}