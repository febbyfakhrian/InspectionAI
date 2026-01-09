using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace InspectionAI.Classes.Managers
{
    /// <summary>
    /// Chart Manager - Simple chart drawing tanpa library eksternal
    /// </summary>
    public class ChartManager
    {
        /// <summary>
        /// Draw pie chart untuk Good/NG ratio
        /// </summary>
        public static void DrawPieChart(Graphics g, Rectangle bounds, int goodCount, int ngCount, int warningCount)
        {
            if (goodCount + ngCount + warningCount == 0)
            {
                // No data - draw empty circle
                using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 2))
                {
                    g.DrawEllipse(pen, bounds);
                }
                return;
            }

            int total = goodCount + ngCount + warningCount;
            float startAngle = 0;

            // Good (Green)
            if (goodCount > 0)
            {
                float sweepAngle = (goodCount / (float)total) * 360;
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 150, 0)))
                {
                    g.FillPie(brush, bounds, startAngle, sweepAngle);
                }
                startAngle += sweepAngle;
            }

            // NG (Red)
            if (ngCount > 0)
            {
                float sweepAngle = (ngCount / (float)total) * 360;
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(200, 0, 0)))
                {
                    g.FillPie(brush, bounds, startAngle, sweepAngle);
                }
                startAngle += sweepAngle;
            }

            // Warning (Orange)
            if (warningCount > 0)
            {
                float sweepAngle = (warningCount / (float)total) * 360;
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 140, 0)))
                {
                    g.FillPie(brush, bounds, startAngle, sweepAngle);
                }
            }

            // Draw border
            using (Pen pen = new Pen(Color.FromArgb(60, 60, 60), 2))
            {
                g.DrawEllipse(pen, bounds);
            }
        }

        /// <summary>
        /// Draw bar chart untuk production over time
        /// </summary>
        public static void DrawBarChart(Graphics g, Rectangle bounds, List<int> values, int maxValue, Color barColor)
        {
            if (values == null || values.Count == 0)
                return;

            int barWidth = bounds.Width / values.Count;
            int padding = 2;

            for (int i = 0; i < values.Count; i++)
            {
                if (maxValue == 0)
                    continue;

                int barHeight = (int)((values[i] / (float)maxValue) * bounds.Height);
                int x = bounds.X + (i * barWidth) + padding;
                int y = bounds.Y + bounds.Height - barHeight;
                int w = barWidth - (padding * 2);

                using (SolidBrush brush = new SolidBrush(barColor))
                {
                    g.FillRectangle(brush, x, y, w, barHeight);
                }
            }

            // Draw axes
            using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 1))
            {
                // X axis
                g.DrawLine(pen, bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom);
                // Y axis
                g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom);
            }
        }

        /// <summary>
        /// Draw line chart untuk trends
        /// </summary>
        public static void DrawLineChart(Graphics g, Rectangle bounds, List<float> values, float maxValue, Color lineColor)
        {
            if (values == null || values.Count < 2)
                return;

            List<PointF> points = new List<PointF>();

            for (int i = 0; i < values.Count; i++)
            {
                float x = bounds.X + (i / (float)(values.Count - 1)) * bounds.Width;
                float y = bounds.Y + bounds.Height - ((values[i] / maxValue) * bounds.Height);
                points.Add(new PointF(x, y));
            }

            // Draw line
            using (Pen pen = new Pen(lineColor, 2))
            {
                g.DrawLines(pen, points.ToArray());
            }

            // Draw points
            using (SolidBrush brush = new SolidBrush(lineColor))
            {
                foreach (PointF point in points)
                {
                    g.FillEllipse(brush, point.X - 3, point.Y - 3, 6, 6);
                }
            }

            // Draw grid
            using (Pen gridPen = new Pen(Color.FromArgb(50, 50, 50), 1))
            {
                gridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                // Horizontal lines
                for (int i = 0; i <= 4; i++)
                {
                    int y = bounds.Y + (i * bounds.Height / 4);
                    g.DrawLine(gridPen, bounds.Left, y, bounds.Right, y);
                }

                // Vertical lines
                for (int i = 0; i <= 4; i++)
                {
                    int x = bounds.X + (i * bounds.Width / 4);
                    g.DrawLine(gridPen, x, bounds.Top, x, bounds.Bottom);
                }
            }
        }

        /// <summary>
        /// Draw gauge chart untuk pass rate
        /// </summary>
        public static void DrawGauge(Graphics g, Rectangle bounds, float percentage)
        {
            // Clamp percentage
            percentage = Math.Max(0, Math.Min(100, percentage));

            // Draw background arc
            using (Pen bgPen = new Pen(Color.FromArgb(60, 60, 60), 10))
            {
                g.DrawArc(bgPen, bounds, 180, 180);
            }

            // Draw value arc
            Color gaugeColor;
            if (percentage >= 95)
                gaugeColor = Color.FromArgb(0, 200, 0); // Green
            else if (percentage >= 85)
                gaugeColor = Color.FromArgb(255, 200, 0); // Yellow
            else
                gaugeColor = Color.FromArgb(200, 0, 0); // Red

            using (Pen valuePen = new Pen(gaugeColor, 10))
            {
                float sweepAngle = (percentage / 100f) * 180;
                g.DrawArc(valuePen, bounds, 180, sweepAngle);
            }

            // Draw percentage text
            string text = $"{percentage:F1}%";
            using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                SizeF textSize = g.MeasureString(text, font);
                float x = bounds.X + (bounds.Width - textSize.Width) / 2;
                float y = bounds.Y + bounds.Height - 30;
                g.DrawString(text, font, brush, x, y);
            }
        }
    }
}