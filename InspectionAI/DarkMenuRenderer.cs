using System;
using System.Drawing;
using System.Windows.Forms;

namespace InspectionAI
{
    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkMenuColorTable())
        {
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            // Arrow untuk submenu (panah ►)
            e.ArrowColor = Color.White;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            // Text color putih
            e.TextColor = Color.White;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            // Background saat hover
            if (e.Item.Selected)
            {
                // Hover color (biru terang seperti VS2022)
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(51, 51, 55)),
                    new Rectangle(Point.Empty, e.Item.Size));
            }
            else
            {
                // Normal color (dark gray)
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(45, 45, 48)),
                    new Rectangle(Point.Empty, e.Item.Size));
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            // Separator line (garis pemisah)
            e.Graphics.DrawLine(
                new Pen(Color.FromArgb(62, 62, 66)),
                new Point(30, e.Item.Height / 2),
                new Point(e.Item.Width, e.Item.Height / 2));
        }
    }

    // Color table untuk dark theme
    public class DarkMenuColorTable : ProfessionalColorTable
    {
        public override Color MenuBorder => Color.FromArgb(51, 51, 55);
        public override Color MenuItemBorder => Color.FromArgb(51, 51, 55);
        public override Color MenuItemSelected => Color.FromArgb(51, 51, 55);
        public override Color MenuStripGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color MenuStripGradientEnd => Color.FromArgb(45, 45, 48);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(62, 62, 66);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(62, 62, 66);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(0, 122, 204);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(0, 122, 204);
        public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
        public override Color ToolStripBorder => Color.FromArgb(51, 51, 55);
    }
}
