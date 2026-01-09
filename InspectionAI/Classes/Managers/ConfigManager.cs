using System;
using System.IO;
using Newtonsoft.Json;
using InspectionAI.Classes.Models;

namespace InspectionAI.Classes.Managers
{
    public class ConfigManager
    {
        private static readonly string CONFIG_FILE = "config.json";
        private static AppConfig _currentConfig;

        public static AppConfig LoadConfig()
        {
            try
            {
                string configPath = "config.json";

                if (!File.Exists(configPath))
                {
                    var defaultConfig = CreateDefaultConfig();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<AppConfig>(json);

                // ===== FIX: Never return NULL =====
                if (config == null)
                {
                    Logger.LogWarning("Config deserialization failed, using defaults");
                    return CreateDefaultConfig();
                }

                _currentConfig = config;
                return _currentConfig;
            }
            catch (Exception ex)
            {
                Logger.LogError("Config load failed, using defaults", ex);
                return CreateDefaultConfig();
            }
        }

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
                throw new Exception("Failed to save config: " + ex.Message);
            }
        }

        public static AppConfig GetConfig()
        {
            if (_currentConfig == null)
                _currentConfig = LoadConfig();

            return _currentConfig;
        }

        // ===== FIX: Changed from private to public =====
        public static AppConfig CreateDefaultConfig()
        {
            var config = new AppConfig();

            config.Database.Server = "localhost";
            config.Database.Database = "inspection_db";
            config.Database.Username = "root";
            config.Database.Password = "";

            config.Serial.PortName = "COM3";
            config.Serial.BaudRate = 9600;
            config.Serial.TriggerDelayMs = 500;

            config.Model.ConfidenceThreshold = 0.5f;
            config.Model.InputWidth = 640;
            config.Model.InputHeight = 640;

            config.Inspection.SaveNGImages = true;
            config.Inspection.NGImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NG_Images");
            config.Inspection.PlaySoundOnNG = true;

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

        public static bool ValidateConfig(AppConfig config, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrEmpty(config.Database.Server))
            {
                errorMessage = "Database server tidak boleh kosong";
                return false;
            }

            if (string.IsNullOrEmpty(config.Serial.PortName))
            {
                errorMessage = "Serial port name tidak boleh kosong";
                return false;
            }

            if (config.Inspection.SaveNGImages)
            {
                try
                {
                    if (!Directory.Exists(config.Inspection.NGImageFolder))
                        Directory.CreateDirectory(config.Inspection.NGImageFolder);
                }
                catch (Exception ex)
                {
                    errorMessage = "Gagal create NG image folder: " + ex.Message;
                    return false;
                }
            }

            return true;
        }
    }
}