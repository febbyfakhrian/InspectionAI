using System;
using System.Collections.Generic;
using System.Drawing;

namespace InspectionAI.Classes.Models
{
    /// <summary>
    /// Model untuk annotation data (labeling)
    /// </summary>
    public class ImageAnnotation
    {
        public string ImagePath { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public List<BoundingBox> Boxes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public ImageAnnotation()
        {
            Boxes = new List<BoundingBox>();
            CreatedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Bounding box untuk annotation
    /// </summary>
    public class BoundingBox
    {
        public string ClassName { get; set; }
        public int ClassId { get; set; }

        // Pixel coordinates (absolute)
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        // Normalized coordinates (0-1) untuk YOLO
        public float NormalizedX { get; set; }
        public float NormalizedY { get; set; }
        public float NormalizedWidth { get; set; }
        public float NormalizedHeight { get; set; }

        // ===== ADDED: IsDefect property =====
        public bool IsDefect { get; set; }

        public BoundingBox()
        {
        }

        /// <summary>
        /// Create dari pixel coordinates
        /// </summary>
        public BoundingBox(string className, int classId, Rectangle rect, int imageWidth, int imageHeight)
        {
            ClassName = className;
            ClassId = classId;
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;

            // Auto-detect if defect based on class name
            IsDefect = IsDefectClass(className);

            // Calculate normalized (YOLO format: center_x, center_y, width, height)
            float centerX = (X + Width / 2f) / imageWidth;
            float centerY = (Y + Height / 2f) / imageHeight;
            float normWidth = Width / (float)imageWidth;
            float normHeight = Height / (float)imageHeight;

            NormalizedX = centerX;
            NormalizedY = centerY;
            NormalizedWidth = normWidth;
            NormalizedHeight = normHeight;
        }

        /// <summary>
        /// Check if class is defect (NG)
        /// </summary>
        private bool IsDefectClass(string className)
        {
            if (string.IsNullOrEmpty(className))
                return false;

            string lower = className.ToLower();
            return lower.Contains("ng") ||
                   lower.Contains("defect") ||
                   lower.Contains("bad") ||
                   lower.Contains("fail") ||
                   lower.Contains("error");
        }

        /// <summary>
        /// Get as Rectangle
        /// </summary>
        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }
    }

    /// <summary>
    /// Project info untuk labeling
    /// </summary>
    public class LabelingProject
    {
        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public List<string> Classes { get; set; }
        public List<ImageAnnotation> Annotations { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }

        public LabelingProject()
        {
            Classes = new List<string>();
            Annotations = new List<ImageAnnotation>();
            CreatedAt = DateTime.Now;
            LastModified = DateTime.Now;
        }
    }
}