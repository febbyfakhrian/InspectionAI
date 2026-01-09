using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using InspectionAI.Classes.Models;

namespace InspectionAI.Classes.Managers
{
    /// <summary>
    /// Export annotations ke berbagai format (YOLO, JSON, COCO)
    /// </summary>
    public class AnnotationExporter
    {
        /// <summary>
        /// Export ke YOLO format (.txt files)
        /// Format: class_id center_x center_y width height (normalized 0-1)
        /// </summary>
        public static void ExportToYOLO(LabelingProject project, string outputFolder)
        {
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // Create classes.txt
            string classesFile = Path.Combine(outputFolder, "classes.txt");
            File.WriteAllLines(classesFile, project.Classes);

            // Create labels folder
            string labelsFolder = Path.Combine(outputFolder, "labels");
            if (!Directory.Exists(labelsFolder))
                Directory.CreateDirectory(labelsFolder);

            // Export each annotation
            foreach (var annotation in project.Annotations)
            {
                string imageName = Path.GetFileNameWithoutExtension(annotation.ImagePath);
                string labelFile = Path.Combine(labelsFolder, imageName + ".txt");

                using (StreamWriter writer = new StreamWriter(labelFile))
                {
                    foreach (var box in annotation.Boxes)
                    {
                        // YOLO format: class_id center_x center_y width height
                        writer.WriteLine($"{box.ClassId} {box.NormalizedX:F6} {box.NormalizedY:F6} {box.NormalizedWidth:F6} {box.NormalizedHeight:F6}");
                    }
                }
            }
        }

        /// <summary>
        /// Export ke JSON format (custom)
        /// </summary>
        public static void ExportToJSON(LabelingProject project, string outputFile)
        {
            var export = new
            {
                project_name = project.ProjectName,
                classes = project.Classes,
                created_at = project.CreatedAt,
                last_modified = project.LastModified,
                images = new List<object>()
            };

            foreach (var annotation in project.Annotations)
            {
                var imageData = new
                {
                    file_path = annotation.ImagePath,
                    width = annotation.ImageWidth,
                    height = annotation.ImageHeight,
                    annotations = new List<object>()
                };

                foreach (var box in annotation.Boxes)
                {
                    var boxData = new
                    {
                        class_name = box.ClassName,
                        class_id = box.ClassId,
                        bbox = new
                        {
                            x = box.X,
                            y = box.Y,
                            width = box.Width,
                            height = box.Height
                        },
                        normalized = new
                        {
                            x = box.NormalizedX,
                            y = box.NormalizedY,
                            width = box.NormalizedWidth,
                            height = box.NormalizedHeight
                        }
                    };

                    ((List<object>)imageData.annotations).Add(boxData);
                }

                ((List<object>)export.images).Add(imageData);
            }

            string json = JsonConvert.SerializeObject(export, Formatting.Indented);
            File.WriteAllText(outputFile, json);
        }

        /// <summary>
        /// Export ke COCO format (untuk compatibility dengan tools lain)
        /// </summary>
        public static void ExportToCOCO(LabelingProject project, string outputFile)
        {
            var coco = new
            {
                info = new
                {
                    description = project.ProjectName,
                    date_created = project.CreatedAt.ToString("yyyy-MM-dd")
                },
                categories = new List<object>(),
                images = new List<object>(),
                annotations = new List<object>()
            };

            // Categories
            for (int i = 0; i < project.Classes.Count; i++)
            {
                ((List<object>)coco.categories).Add(new
                {
                    id = i,
                    name = project.Classes[i]
                });
            }

            // Images and annotations
            int imageId = 1;
            int annotationId = 1;

            foreach (var annotation in project.Annotations)
            {
                // Add image
                ((List<object>)coco.images).Add(new
                {
                    id = imageId,
                    file_name = Path.GetFileName(annotation.ImagePath),
                    width = annotation.ImageWidth,
                    height = annotation.ImageHeight
                });

                // Add annotations
                foreach (var box in annotation.Boxes)
                {
                    ((List<object>)coco.annotations).Add(new
                    {
                        id = annotationId++,
                        image_id = imageId,
                        category_id = box.ClassId,
                        bbox = new[] { box.X, box.Y, box.Width, box.Height },
                        area = box.Width * box.Height,
                        iscrowd = 0
                    });
                }

                imageId++;
            }

            string json = JsonConvert.SerializeObject(coco, Formatting.Indented);
            File.WriteAllText(outputFile, json);
        }

        /// <summary>
        /// Load project dari JSON
        /// </summary>
        public static LabelingProject LoadProject(string jsonFile)
        {
            if (!File.Exists(jsonFile))
                return null;

            string json = File.ReadAllText(jsonFile);
            // TODO: Implement proper JSON deserialization
            return JsonConvert.DeserializeObject<LabelingProject>(json);
        }

        /// <summary>
        /// Save project ke JSON
        /// </summary>
        public static void SaveProject(LabelingProject project, string jsonFile)
        {
            project.LastModified = DateTime.Now;
            string json = JsonConvert.SerializeObject(project, Formatting.Indented);
            File.WriteAllText(jsonFile, json);
        }
    }
}