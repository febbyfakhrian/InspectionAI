using System;
using System.Windows.Forms;
using InspectionAI.Classes.Managers;
using InspectionAI.Classes.Models;

namespace InspectionAI
{
    /// <summary>
    /// Test Helper Class - Add ini di MainForm untuk testing TAHAP 1
    /// Setelah TAHAP 1 verified, bisa di-comment atau dihapus
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Test semua functionality TAHAP 1
        /// Call ini dari MainForm_Load() atau button click
        /// </summary>
        public static void RunAllTests(Form owner)
        {
            string results = "=== TAHAP 1 TEST RESULTS ===\n\n";

            // Test 1: Config Manager
            results += TestConfigManager();
            results += "\n";

            // Test 2: Database Connection
            results += TestDatabaseConnection();
            results += "\n";

            // Test 3: Database Operations
            results += TestDatabaseOperations();
            results += "\n";

            // Test 4: Sound Alert
            results += TestSoundAlert();
            results += "\n";

            results += "\n=== ALL TESTS COMPLETED ===";

            MessageBox.Show(results, "Test Results", MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
        }

        private static string TestConfigManager()
        {
            try
            {
                // Test load config
                var config = ConfigManager.LoadConfig();
                
                // Test validation
                string error;
                bool valid = ConfigManager.ValidateConfig(config, out error);

                if (!valid)
                    return $"❌ ConfigManager: FAILED\n   {error}";

                return $"✅ ConfigManager: OK\n" +
                       $"   Database: {config.Database.Database}\n" +
                       $"   Serial Port: {config.Serial.PortName}\n" +
                       $"   Cameras: {config.Cameras.Count}";
            }
            catch (Exception ex)
            {
                return $"❌ ConfigManager: EXCEPTION\n   {ex.Message}";
            }
        }

        private static string TestDatabaseConnection()
        {
            try
            {
                var config = ConfigManager.GetConfig();
                var logger = new DataLogger(config.Database.GetConnectionString());

                string error;
                bool connected = logger.TestConnection(out error);

                if (!connected)
                    return $"❌ Database Connection: FAILED\n   {error}";

                // Try to initialize database
                logger.InitializeDatabase();

                return $"✅ Database Connection: OK\n" +
                       $"   Server: {config.Database.Server}\n" +
                       $"   Database: {config.Database.Database}\n" +
                       $"   Tables created successfully";
            }
            catch (Exception ex)
            {
                return $"❌ Database Connection: EXCEPTION\n   {ex.Message}";
            }
        }

        private static string TestDatabaseOperations()
        {
            try
            {
                var config = ConfigManager.GetConfig();
                var logger = new DataLogger(config.Database.GetConnectionString());

                // Create test inspection result
                var result = new InspectionResult
                {
                    SetNumber = "TEST_SET_001",
                    CameraId = "Camera_1",
                    Result = InspectionStatus.GOOD,
                    InspectionTimeMs = 45,
                    Timestamp = DateTime.Now
                };

                // Add sample detection
                result.Detections.Add(new DetectionResult
                {
                    ClassName = "screw_good",
                    Confidence = 0.95f,
                    X = 0.5f,
                    Y = 0.5f,
                    Width = 0.1f,
                    Height = 0.1f,
                    IsDefect = false
                });

                // Insert to database
                int id = logger.InsertInspectionResult(result);

                if (id <= 0)
                    return "❌ Database Insert: FAILED";

                // Get statistics
                var stats = logger.GetTodayStatistics();

                return $"✅ Database Operations: OK\n" +
                       $"   Inserted ID: {id}\n" +
                       $"   Today Total: {stats.TotalCount}\n" +
                       $"   Good: {stats.GoodCount} ({stats.GoodPercentage:F1}%)\n" +
                       $"   NG: {stats.NgCount} ({stats.NgPercentage:F1}%)";
            }
            catch (Exception ex)
            {
                return $"❌ Database Operations: EXCEPTION\n   {ex.Message}";
            }
        }

        private static string TestSoundAlert()
        {
            try
            {
                // Enable sound
                SoundAlertManager.SetEnabled(true);

                // Play test beep
                SoundAlertManager.PlayBeep(1000, 200);

                return $"✅ Sound Alert: OK\n" +
                       $"   Beep sound played (1000Hz, 200ms)";
            }
            catch (Exception ex)
            {
                return $"❌ Sound Alert: EXCEPTION\n   {ex.Message}";
            }
        }

        /// <summary>
        /// Test individual component
        /// </summary>
        public static void TestConfig()
        {
            var config = ConfigManager.LoadConfig();
            MessageBox.Show(
                $"Database: {config.Database.Database}\n" +
                $"Serial: {config.Serial.PortName}\n" +
                $"Model Path: {config.Model.ModelPath}\n" +
                $"Cameras: {config.Cameras.Count}",
                "Config Test"
            );
        }

        public static void TestDatabase()
        {
            var config = ConfigManager.GetConfig();
            var logger = new DataLogger(config.Database.GetConnectionString());

            string error;
            if (logger.TestConnection(out error))
            {
                logger.InitializeDatabase();
                MessageBox.Show("Database connected and initialized!", "Success");
            }
            else
            {
                MessageBox.Show($"Database error: {error}", "Error");
            }
        }

        public static void TestInsertDummyData()
        {
            var config = ConfigManager.GetConfig();
            var logger = new DataLogger(config.Database.GetConnectionString());

            // Insert 5 dummy results
            for (int i = 1; i <= 5; i++)
            {
                var result = new InspectionResult
                {
                    SetNumber = $"SET_{i:D3}",
                    CameraId = $"Camera_{(i % 3) + 1}",
                    Result = (i % 4 == 0) ? InspectionStatus.NG : InspectionStatus.GOOD,
                    InspectionTimeMs = 40 + i * 2,
                    Timestamp = DateTime.Now.AddSeconds(-i * 10)
                };

                result.Detections.Add(new DetectionResult
                {
                    ClassName = (i % 4 == 0) ? "screw_ng" : "screw_good",
                    Confidence = 0.85f + (i * 0.02f),
                    X = 0.5f,
                    Y = 0.5f,
                    Width = 0.1f,
                    Height = 0.1f,
                    IsDefect = (i % 4 == 0)
                });

                logger.InsertInspectionResult(result);
            }

            MessageBox.Show("5 dummy records inserted!", "Success");
        }
    }
}
