using System;
using System.Collections.Generic;

namespace InspectionAI.Classes.Models
{
    /// <summary>
    /// Model untuk menyimpan hasil inspection dari satu set
    /// </summary>
    public class InspectionResult
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string SetNumber { get; set; }
        public string CameraId { get; set; }
        public InspectionStatus Result { get; set; }
        public List<DetectionResult> Detections { get; set; }
        public int InspectionTimeMs { get; set; }
        public string ImagePath { get; set; }

        public InspectionResult()
        {
            Timestamp = DateTime.Now;
            Detections = new List<DetectionResult>();
        }

        /// <summary>
        /// Get defect summary untuk display di table
        /// </summary>
        public string GetDefectSummary()
        {
            if (Result == InspectionStatus.GOOD)
                return "No Defect";

            var defects = new List<string>();
            foreach (var detection in Detections)
            {
                if (detection.IsDefect)
                    defects.Add($"{detection.ClassName} ({detection.Confidence:P0})");
            }

            return defects.Count > 0 ? string.Join(", ", defects) : "Unknown";
        }
    }

    /// <summary>
    /// Hasil detection untuk satu object (bounding box)
    /// </summary>
    public class DetectionResult
    {
        public string ClassName { get; set; }
        public float Confidence { get; set; }
        public float X { get; set; }  // Center X (normalized 0-1)
        public float Y { get; set; }  // Center Y (normalized 0-1)
        public float Width { get; set; }
        public float Height { get; set; }
        public bool IsDefect { get; set; }  // true = NG, false = GOOD

        /// <summary>
        /// Convert to pixel coordinates
        /// </summary>
        public System.Drawing.Rectangle ToPixelRect(int imageWidth, int imageHeight)
        {
            int pixelX = (int)((X - Width / 2) * imageWidth);
            int pixelY = (int)((Y - Height / 2) * imageHeight);
            int pixelW = (int)(Width * imageWidth);
            int pixelH = (int)(Height * imageHeight);

            return new System.Drawing.Rectangle(pixelX, pixelY, pixelW, pixelH);
        }
    }

    public enum InspectionStatus
    {
        GOOD,       // All checks passed
        NG,         // Defect detected
        WARNING     // Low confidence or camera issue
    }
}
