using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using InspectionAI.Classes.Models;

namespace InspectionAI.Classes.Managers
{
    /// <summary>
    /// Image Processor untuk resize, convert, dan overlay
    /// </summary>
    public class ImageProcessor
    {
        /// <summary>
        /// Resize image dengan maintain aspect ratio
        /// </summary>
        public static Bitmap ResizeImage(Bitmap source, int targetWidth, int targetHeight, bool maintainAspectRatio = true)
        {
            if (source == null)
                return null;

            int newWidth, newHeight;

            if (maintainAspectRatio)
            {
                float aspectRatio = (float)source.Width / source.Height;
                float targetAspectRatio = (float)targetWidth / targetHeight;

                if (aspectRatio > targetAspectRatio)
                {
                    newWidth = targetWidth;
                    newHeight = (int)(targetWidth / aspectRatio);
                }
                else
                {
                    newHeight = targetHeight;
                    newWidth = (int)(targetHeight * aspectRatio);
                }
            }
            else
            {
                newWidth = targetWidth;
                newHeight = targetHeight;
            }

            Bitmap result = new Bitmap(newWidth, newHeight);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                g.DrawImage(source, 0, 0, newWidth, newHeight);
            }

            return result;
        }

        /// <summary>
        /// Draw bounding boxes pada image
        /// </summary>
        public static void DrawBoundingBoxes(Bitmap image, System.Collections.Generic.List<DetectionResult> detections)
        {
            if (image == null || detections == null || detections.Count == 0)
                return;

            using (Graphics g = Graphics.FromImage(image))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                foreach (var detection in detections)
                {
                    // Convert normalized coordinates to pixel
                    Rectangle rect = detection.ToPixelRect(image.Width, image.Height);

                    // Choose color based on defect status
                    Color boxColor = detection.IsDefect ? Color.Red : Color.Lime;
                    Color textBgColor = detection.IsDefect ? Color.FromArgb(200, 200, 0, 0) : Color.FromArgb(200, 0, 150, 0);

                    // Draw rectangle
                    using (Pen pen = new Pen(boxColor, 3))
                    {
                        g.DrawRectangle(pen, rect);
                    }

                    // Draw label background
                    string label = $"{detection.ClassName} {detection.Confidence:P0}";
                    using (Font font = new Font("Arial", 10, FontStyle.Bold))
                    {
                        SizeF labelSize = g.MeasureString(label, font);
                        Rectangle labelRect = new Rectangle(
                            rect.X,
                            rect.Y - (int)labelSize.Height - 2,
                            (int)labelSize.Width + 6,
                            (int)labelSize.Height + 2
                        );

                        // Ensure label is inside image
                        if (labelRect.Y < 0)
                        {
                            labelRect.Y = rect.Y + 2;
                        }

                        using (Brush bgBrush = new SolidBrush(textBgColor))
                        {
                            g.FillRectangle(bgBrush, labelRect);
                        }

                        // Draw label text
                        using (Brush textBrush = new SolidBrush(Color.White))
                        {
                            g.DrawString(label, font, textBrush, labelRect.X + 3, labelRect.Y + 1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw overlay info (FPS, timestamp, camera ID)
        /// </summary>
        public static void DrawOverlay(Bitmap image, string cameraId, int fps, DateTime timestamp)
        {
            if (image == null)
                return;

            using (Graphics g = Graphics.FromImage(image))
            {
                // Background untuk text
                using (Brush bgBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                {
                    g.FillRectangle(bgBrush, 0, 0, image.Width, 30);
                }

                // Draw info text
                using (Font font = new Font("Arial", 10, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    string info = $"{cameraId} | FPS: {fps} | {timestamp:HH:mm:ss}";
                    g.DrawString(info, font, textBrush, 5, 5);
                }
            }
        }

        /// <summary>
        /// Clone bitmap (deep copy)
        /// </summary>
        public static Bitmap CloneBitmap(Bitmap source)
        {
            if (source == null)
                return null;

            return new Bitmap(source);
        }
    }
}