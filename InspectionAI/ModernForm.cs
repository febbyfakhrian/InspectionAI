using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InspectionAI
{
    public class ModernForm : Form
    {
        private int borderRadius = 8;
        public bool EnableRoundedCorners { get; set; } = false;

        public ModernForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.DoubleBuffered = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyRoundedCorners();
        }

        private void ApplyRoundedCorners()
        {
            // Matikan radius saat maximize
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.Region = null;
                return;
            }

            if (this.Width <= 0 || this.Height <= 0) return;

            GraphicsPath path = new GraphicsPath();
            int radius = borderRadius * 2;

            // Top-left
            path.AddArc(0, 0, radius, radius, 180, 90);
            // Top-right
            path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
            // Bottom-right
            path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
            // Bottom-left
            path.AddArc(0, this.Height - radius, radius, radius, 90, 90);

            path.CloseFigure();
            this.Region = new Region(path);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ApplyRoundedCorners();
        }
    }
}
