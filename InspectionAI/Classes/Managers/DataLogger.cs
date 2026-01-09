using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using InspectionAI.Classes.Models;

namespace InspectionAI.Classes.Managers
{
    /// <summary>
    /// Manager untuk handle semua database operations (MySQL)
    /// </summary>
    public class DataLogger
    {
        private string connectionString;

        public DataLogger(string connString)
        {
            connectionString = connString;
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        public bool TestConnection(out string errorMessage)
        {
            errorMessage = "";
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Initialize database tables jika belum ada
        /// </summary>
        public void InitializeDatabase()
        {
            string createTableSQL = @"
                CREATE TABLE IF NOT EXISTS inspection_results (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    timestamp DATETIME NOT NULL,
                    set_number VARCHAR(50),
                    camera_id VARCHAR(50),
                    result ENUM('GOOD', 'NG', 'WARNING') NOT NULL,
                    defect_summary TEXT,
                    inspection_time_ms INT,
                    image_path VARCHAR(255),
                    INDEX idx_timestamp (timestamp),
                    INDEX idx_camera (camera_id),
                    INDEX idx_result (result)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                CREATE TABLE IF NOT EXISTS detection_details (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    inspection_id INT,
                    class_name VARCHAR(100),
                    confidence FLOAT,
                    bbox_x FLOAT,
                    bbox_y FLOAT,
                    bbox_width FLOAT,
                    bbox_height FLOAT,
                    is_defect BOOLEAN,
                    FOREIGN KEY (inspection_id) REFERENCES inspection_results(id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                CREATE TABLE IF NOT EXISTS camera_config (
                    camera_id VARCHAR(50) PRIMARY KEY,
                    ip_address VARCHAR(15),
                    is_active BOOLEAN DEFAULT true,
                    target_fps INT DEFAULT 60,
                    last_update DATETIME
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            ";

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(createTableSQL, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Insert inspection result ke database
        /// </summary>
        public int InsertInspectionResult(InspectionResult result)
        {
            string sql = @"
                INSERT INTO inspection_results 
                (timestamp, set_number, camera_id, result, defect_summary, inspection_time_ms, image_path)
                VALUES (@timestamp, @setNumber, @cameraId, @result, @defect, @time, @image);
                SELECT LAST_INSERT_ID();
            ";

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@timestamp", result.Timestamp);
                    cmd.Parameters.AddWithValue("@setNumber", result.SetNumber ?? "");
                    cmd.Parameters.AddWithValue("@cameraId", result.CameraId ?? "");
                    cmd.Parameters.AddWithValue("@result", result.Result.ToString());
                    cmd.Parameters.AddWithValue("@defect", result.GetDefectSummary());
                    cmd.Parameters.AddWithValue("@time", result.InspectionTimeMs);
                    cmd.Parameters.AddWithValue("@image", result.ImagePath ?? "");

                    int inspectionId = Convert.ToInt32(cmd.ExecuteScalar());

                    // Insert detection details
                    if (result.Detections != null && result.Detections.Count > 0)
                    {
                        InsertDetectionDetails(conn, inspectionId, result.Detections);
                    }

                    return inspectionId;
                }
            }
        }

        /// <summary>
        /// Insert detection details (bounding boxes)
        /// </summary>
        private void InsertDetectionDetails(MySqlConnection conn, int inspectionId, List<DetectionResult> detections)
        {
            string sql = @"
                INSERT INTO detection_details 
                (inspection_id, class_name, confidence, bbox_x, bbox_y, bbox_width, bbox_height, is_defect)
                VALUES (@inspId, @class, @conf, @x, @y, @w, @h, @defect)
            ";

            foreach (var detection in detections)
            {
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@inspId", inspectionId);
                    cmd.Parameters.AddWithValue("@class", detection.ClassName);
                    cmd.Parameters.AddWithValue("@conf", detection.Confidence);
                    cmd.Parameters.AddWithValue("@x", detection.X);
                    cmd.Parameters.AddWithValue("@y", detection.Y);
                    cmd.Parameters.AddWithValue("@w", detection.Width);
                    cmd.Parameters.AddWithValue("@h", detection.Height);
                    cmd.Parameters.AddWithValue("@defect", detection.IsDefect);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Get recent inspection results untuk display di DataGridView
        /// </summary>
        public DataTable GetRecentResults(int limit = 100)
        {
            string sql = @"
                SELECT 
                    set_number AS 'Set',
                    camera_id AS 'Camera',
                    result AS 'Result',
                    defect_summary AS 'Defect',
                    inspection_time_ms AS 'Time (ms)',
                    DATE_FORMAT(timestamp, '%H:%i:%s') AS 'Timestamp'
                FROM inspection_results
                ORDER BY timestamp DESC
                LIMIT @limit
            ";

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@limit", limit);
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        /// <summary>
        /// Get statistics untuk hari ini
        /// </summary>
        public InspectionStatistics GetTodayStatistics()
        {
            string sql = @"
                SELECT 
                    COUNT(*) as total,
                    SUM(CASE WHEN result = 'GOOD' THEN 1 ELSE 0 END) as good_count,
                    SUM(CASE WHEN result = 'NG' THEN 1 ELSE 0 END) as ng_count,
                    SUM(CASE WHEN result = 'WARNING' THEN 1 ELSE 0 END) as warning_count,
                    AVG(inspection_time_ms) as avg_time
                FROM inspection_results
                WHERE DATE(timestamp) = CURDATE()
            ";

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new InspectionStatistics
                            {
                                TotalCount = reader.GetInt32("total"),
                                GoodCount = reader.GetInt32("good_count"),
                                NgCount = reader.GetInt32("ng_count"),
                                WarningCount = reader.GetInt32("warning_count"),
                                AvgInspectionTimeMs = reader.IsDBNull(reader.GetOrdinal("avg_time")) ? 0 : reader.GetDouble("avg_time")
                            };
                        }
                    }
                }
            }

            return new InspectionStatistics();
        }

        /// <summary>
        /// Update camera config
        /// </summary>
        public void UpdateCameraConfig(CameraInfo camera)
        {
            string sql = @"
                INSERT INTO camera_config (camera_id, ip_address, is_active, target_fps, last_update)
                VALUES (@id, @ip, @active, @fps, NOW())
                ON DUPLICATE KEY UPDATE
                    ip_address = @ip,
                    is_active = @active,
                    target_fps = @fps,
                    last_update = NOW()
            ";

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", camera.CameraId);
                    cmd.Parameters.AddWithValue("@ip", camera.IpAddress);
                    cmd.Parameters.AddWithValue("@active", camera.IsConnected);
                    cmd.Parameters.AddWithValue("@fps", camera.TargetFPS);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    /// <summary>
    /// Statistics model untuk display
    /// </summary>
    public class InspectionStatistics
    {
        public int TotalCount { get; set; }
        public int GoodCount { get; set; }
        public int NgCount { get; set; }
        public int WarningCount { get; set; }
        public double AvgInspectionTimeMs { get; set; }

        public double GoodPercentage => TotalCount > 0 ? (GoodCount * 100.0 / TotalCount) : 0;
        public double NgPercentage => TotalCount > 0 ? (NgCount * 100.0 / TotalCount) : 0;
    }
}
