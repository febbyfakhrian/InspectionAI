using System;

namespace InspectionAI.Classes.Models
{
    /// <summary>
    /// Model untuk menyimpan informasi camera HIKRobot
    /// </summary>
    public class CameraInfo
    {
        public string CameraId { get; set; }
        public string SerialNumber { get; set; }
        public string IpAddress { get; set; }
        public bool IsConnected { get; set; }
        public int CurrentFPS { get; set; }
        public int TargetFPS { get; set; }
        public DateTime LastHeartbeat { get; set; }
        
        // Camera settings
        public int ExposureTime { get; set; }  // microseconds
        public int Gain { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public CameraInfo()
        {
            TargetFPS = 60;
            IsConnected = false;
            LastHeartbeat = DateTime.MinValue;
            Width = 1920;
            Height = 1080;
        }

        /// <summary>
        /// Get display text untuk CheckedListBox
        /// </summary>
        public string GetDisplayText()
        {
            string status = IsConnected ? $"{CurrentFPS} FPS" : "Disconnected";
            return $"{CameraId} ({IpAddress}) - {status}";
        }

        /// <summary>
        /// Check apakah camera masih alive (heartbeat < 5 detik yang lalu)
        /// </summary>
        public bool IsAlive()
        {
            if (!IsConnected) return false;
            return (DateTime.Now - LastHeartbeat).TotalSeconds < 5;
        }
    }
}
