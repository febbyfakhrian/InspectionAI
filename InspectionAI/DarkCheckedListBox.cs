using System;
using System.Drawing;
using System.Windows.Forms;

namespace InspectionAI
{
    public class DarkCheckedListBox : CheckedListBox
    {
        public DarkCheckedListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.ItemHeight = 20;
            this.BackColor = Color.FromArgb(37, 37, 38);
            this.ForeColor = Color.White;
            this.BorderStyle = BorderStyle.None;
            this.DrawItem += DarkCheckedListBox_DrawItem;
        }

        private void DarkCheckedListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Background
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(37, 37, 38)))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            // Checkbox
            int checkSize = 14;
            int checkX = e.Bounds.X + 4;
            int checkY = e.Bounds.Y + (e.Bounds.Height - checkSize) / 2;
            Rectangle checkBox = new Rectangle(checkX, checkY, checkSize, checkSize);

            using (SolidBrush checkBg = new SolidBrush(Color.FromArgb(62, 62, 66)))
            {
                e.Graphics.FillRectangle(checkBg, checkBox);
            }

            using (Pen borderPen = new Pen(Color.FromArgb(80, 80, 84), 1))
            {
                e.Graphics.DrawRectangle(borderPen, checkBox);
            }

            bool isChecked = this.GetItemChecked(e.Index);
            if (isChecked)
            {
                using (Pen checkPen = new Pen(Color.FromArgb(0, 122, 204), 2))
                {
                    e.Graphics.DrawLine(checkPen, checkX + 3, checkY + 7, checkX + 6, checkY + 10);
                    e.Graphics.DrawLine(checkPen, checkX + 6, checkY + 10, checkX + 11, checkY + 4);
                }
            }

            string text = this.Items[e.Index].ToString();
            Color textColor = text.Contains("Disconnected") ? Color.FromArgb(150, 150, 150) : Color.White;

            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(text, e.Font, textBrush, e.Bounds.X + 22, e.Bounds.Y + 2);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(37, 37, 38)))
            {
                e.Graphics.FillRectangle(bgBrush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }
    }
}