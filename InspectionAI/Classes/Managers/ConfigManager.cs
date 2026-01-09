using System;
using System.IO;
using Newtonsoft.Json;
using InspectionAI.Classes.Models;

namespace InspectionAI.Classes.Managers
{
    /// <summary>
    /// Manager untuk load dan save application configuration
    /// File location: config.json di root folder aplikasi
    /// </summary>
    public class ConfigManager
    {
        private static readonly string CONFIG_FILE = "config.json";
        private static AppConfig _currentConfig;

        /// <summary>
        /// Load configuration dari file JSON
        /// Jika file tidak ada, create default config
        /// </summary>
        public static AppConfig LoadConfig()
        {
            try
            {
                string configPath = "config.json";

                if (!File.Exists(configPath))
                {
                    // Create default config
                    var defaultConfig = CreateDefaultConfig();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch (Exception ex)
            {
                // NEVER RETURN NULL!
                Logger.LogError("Config load failed, using defaults", ex);
                return CreateDefaultConfig();
            }
        }

        /// <summary>
        /// Save configuration ke file JSON
        /// </summary>
        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(CONFIG_FILE, json);
                _currentConfig = config;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save config: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current loaded config (singleton pattern)
        /// </summary>
        public static AppConfig GetConfig()
        {
            if (_currentConfig == null)
                _currentConfig = LoadConfig();
            
            return _currentConfig;
        }

        /// <summary>
        /// Create default configuration
        /// </summary>
        private static AppConfig CreateDefaultConfig()
        {
            var config = new AppConfig();

            // Default database settings
            config.Database.Server = "localhost";
            config.Database.Database = "inspection_db";
            config.Database.Username = "root";
            config.Database.Password = "";

            // Default serial/PLC settings
            config.Serial.PortName = "COM3";
            config.Serial.BaudRate = 9600;
            config.Serial.TriggerDelayMs = 500;

            // Default model settings
            config.Model.ConfidenceThreshold = 0.5f;
            config.Model.InputWidth = 640;
            config.Model.InputHeight = 640;

            // Default inspection settings
            config.Inspection.SaveNGImages = true;
            config.Inspection.NGImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NG_Images");
            config.Inspection.PlaySoundOnNG = true;

            // Default cameras (example)
            config.Cameras.Add(new CameraConfig 
            { 
                CameraId = "Camera_1", 
                IpAddress = "10.0.0.101",
                IsEnabled = true
            });
            config.Cameras.Add(new CameraConfig 
            { 
                CameraId = "Camera_2", 
                IpAddress = "10.0.0.102",
                IsEnabled = true
            });
            config.Cameras.Add(new CameraConfig 
            { 
                CameraId = "Camera_3", 
                IpAddress = "10.0.0.103",
                IsEnabled = false
            });

            return config;
        }

        /// <summary>
        /// Validate config dan ensure folders exist
        /// </summary>
        public static bool ValidateConfig(AppConfig config, out string errorMessage)
        {
            errorMessage = "";

            // Validate database
            if (string.IsNullOrEmpty(config.Database.Server))
            {
                errorMessage = "Database server tidak boleh kosong";
                return false;
            }

            // Validate serial port
            if (string.IsNullOrEmpty(config.Serial.PortName))
            {
                errorMessage = "Serial port name tidak boleh kosong";
                return false;
            }

            // Create NG image folder if not exists
            if (config.Inspection.SaveNGImages)
            {
                try
                {
                    if (!Directory.Exists(config.Inspection.NGImageFolder))
                        Directory.CreateDirectory(config.Inspection.NGImageFolder);
                }
                catch (Exception ex)
                {
                    errorMessage = $"Gagal create NG image folder: {ex.Message}";
                    return false;
                }
            }

            return true;
        }
    }
}
