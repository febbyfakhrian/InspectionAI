using System.Collections.Generic;

namespace InspectionAI.Classes.Models
{
    /// <summary>
    /// Model untuk application configuration
    /// Disimpan ke JSON file: config.json
    /// </summary>
    public class AppConfig
    {
        // Database Settings
        public DatabaseConfig Database { get; set; }

        // PLC/Serial Settings
        public SerialConfig Serial { get; set; }

        // AI Model Settings
        public ModelConfig Model { get; set; }

        // Inspection Settings
        public InspectionConfig Inspection { get; set; }

        // Camera Settings
        public List<CameraConfig> Cameras { get; set; }

        public AppConfig()
        {
            Database = new DatabaseConfig();
            Serial = new SerialConfig();
            Model = new ModelConfig();
            Inspection = new InspectionConfig();
            Cameras = new List<CameraConfig>();
        }
    }

    public class DatabaseConfig
    {
        public string Server { get; set; } = "localhost";
        public string Database { get; set; } = "inspection_db";
        public string Username { get; set; } = "root";
        public string Password { get; set; } = "";
        public int Port { get; set; } = 3306;

        public string GetConnectionString()
        {
            return $"Server={Server};Port={Port};Database={Database};Uid={Username};Pwd={Password};";
        }
    }

    public class SerialConfig
    {
        public string PortName { get; set; } = "COM3";
        public int BaudRate { get; set; } = 9600;
        public bool AutoRunOnTrigger { get; set; } = true;
        public int TriggerDelayMs { get; set; } = 500;
    }

    public class ModelConfig
    {
        public string ModelPath { get; set; } = "";
        public string ClassesPath { get; set; } = "";
        public float ConfidenceThreshold { get; set; } = 0.5f;
        public int InputWidth { get; set; } = 640;
        public int InputHeight { get; set; } = 640;
        public bool UseGPU { get; set; } = false;  // CPU only untuk sekarang
    }

    public class InspectionConfig
    {
        public bool SaveNGImages { get; set; } = true;
        public string NGImageFolder { get; set; } = "NG_Images";
        public bool PlaySoundOnNG { get; set; } = true;
        public bool AutoRunOnSensorTrigger { get; set; } = true;
        public int MinConfidenceWarning { get; set; } = 30;  // Below this = WARNING state
    }

    public class CameraConfig
    {
        public string CameraId { get; set; }
        public string IpAddress { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int ExposureTime { get; set; } = 10000;  // 10ms default
        public int Gain { get; set; } = 0;
    }
}
