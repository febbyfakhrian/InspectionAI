# 🚀 QUICK REFERENCE CARD

## 📦 TAHAP 1 - CORE CLASSES USAGE

### 1. Load Configuration
```csharp
using InspectionAI.Classes.Managers;
using InspectionAI.Classes.Models;

// Load config
var config = ConfigManager.LoadConfig();

// Access settings
string dbName = config.Database.Database;
string serialPort = config.Serial.PortName;
float threshold = config.Model.ConfidenceThreshold;

// Save config
config.Model.ConfidenceThreshold = 0.6f;
ConfigManager.SaveConfig(config);
```

### 2. Database Operations
```csharp
// Initialize
var logger = new DataLogger(config.Database.GetConnectionString());

// Test connection
string error;
if (!logger.TestConnection(out error))
{
    MessageBox.Show($"DB Error: {error}");
    return;
}

// Create tables (first time only)
logger.InitializeDatabase();

// Insert inspection result
var result = new InspectionResult
{
    SetNumber = "SET001",
    CameraId = "Camera_1",
    Result = InspectionStatus.GOOD,
    InspectionTimeMs = 45
};

result.Detections.Add(new DetectionResult
{
    ClassName = "screw_good",
    Confidence = 0.95f,
    X = 0.5f, Y = 0.5f,
    Width = 0.1f, Height = 0.1f,
    IsDefect = false
});

int id = logger.InsertInspectionResult(result);

// Query results
var dt = logger.GetRecentResults(100); // Last 100 results
dgvResults.DataSource = dt;

// Get statistics
var stats = logger.GetTodayStatistics();
lblTotal.Text = $"Total: {stats.TotalCount}";
lblGood.Text = $"Good: {stats.GoodCount} ({stats.GoodPercentage:F1}%)";
lblNG.Text = $"NG: {stats.NgCount} ({stats.NgPercentage:F1}%)";
```

### 3. Sound Alerts
```csharp
using InspectionAI.Classes.Managers;

// Enable/disable
SoundAlertManager.SetEnabled(true);

// Play NG alert
SoundAlertManager.PlayNGAlert();

// Play warning alert
SoundAlertManager.PlayWarningAlert();

// Play custom beep
SoundAlertManager.PlayBeep(1000, 200); // 1000Hz, 200ms
```

### 4. Status Bar Updates
```csharp
// Blue - Running
statusStrip1.BackColor = Color.FromArgb(0, 122, 204);
statusLabel.Text = "● RUNNING | Inspection Active";

// Green - Good
statusStrip1.BackColor = Color.FromArgb(0, 150, 0);
statusLabel.Text = "● GOOD | All Checks Passed";

// Red - NG
statusStrip1.BackColor = Color.FromArgb(200, 0, 0);
statusLabel.Text = "● NG | Defect Detected";
SoundAlertManager.PlayNGAlert();

// Orange - Warning
statusStrip1.BackColor = Color.FromArgb(255, 140, 0);
statusLabel.Text = "● WARNING | Low Confidence";
SoundAlertManager.PlayWarningAlert();

// Gray - Idle
statusStrip1.BackColor = Color.FromArgb(62, 62, 66);
statusLabel.Text = "Ready | Idle";
```

### 5. Data Models Usage
```csharp
// InspectionResult
var result = new InspectionResult();
result.SetNumber = "SET001";
result.CameraId = "Camera_1";
result.Result = InspectionStatus.GOOD; // or NG, WARNING
result.InspectionTimeMs = 45;

// DetectionResult (bounding box)
var detection = new DetectionResult
{
    ClassName = "screw_good",
    Confidence = 0.95f,
    X = 0.5f,        // Center X (0-1 normalized)
    Y = 0.5f,        // Center Y (0-1 normalized)
    Width = 0.1f,    // Width (0-1 normalized)
    Height = 0.1f,   // Height (0-1 normalized)
    IsDefect = false // false = GOOD, true = NG
};

// Convert to pixel coordinates
var rect = detection.ToPixelRect(1920, 1080); // imageWidth, imageHeight

// CameraInfo
var camera = new CameraInfo
{
    CameraId = "Camera_1",
    IpAddress = "10.0.0.101",
    IsConnected = true,
    CurrentFPS = 60,
    TargetFPS = 60
};

string displayText = camera.GetDisplayText(); // "Camera_1 (10.0.0.101) - 60 FPS"
bool alive = camera.IsAlive(); // Check if heartbeat recent
```

---

## 🎨 DARK THEME COLORS

```csharp
// Background Colors
Color.FromArgb(30, 30, 30)      // Main form background
Color.FromArgb(37, 37, 38)      // Panel/Control background
Color.FromArgb(45, 45, 48)      // ToolStrip background
Color.FromArgb(62, 62, 66)      // StatusStrip idle

// Status Colors
Color.FromArgb(0, 122, 204)     // Blue - Running
Color.FromArgb(0, 150, 0)       // Green - Good
Color.FromArgb(200, 0, 0)       // Red - NG
Color.FromArgb(255, 140, 0)     // Orange - Warning

// Text Colors
Color.White                      // Primary text
Color.FromArgb(200, 200, 200)   // Secondary text
Color.Lime                       // Success highlight
Color.Red                        // Error highlight
```

---

## 🗄️ DATABASE SCHEMA

### inspection_results
```sql
id              INT PRIMARY KEY AUTO_INCREMENT
timestamp       DATETIME NOT NULL
set_number      VARCHAR(50)
camera_id       VARCHAR(50)
result          ENUM('GOOD','NG','WARNING')
defect_summary  TEXT
inspection_time_ms INT
image_path      VARCHAR(255)
```

### detection_details
```sql
id              INT PRIMARY KEY AUTO_INCREMENT
inspection_id   INT (FK → inspection_results.id)
class_name      VARCHAR(100)
confidence      FLOAT
bbox_x          FLOAT (normalized 0-1)
bbox_y          FLOAT (normalized 0-1)
bbox_width      FLOAT (normalized 0-1)
bbox_height     FLOAT (normalized 0-1)
is_defect       BOOLEAN
```

---

## 🧪 TESTING SNIPPETS

### Quick Test All Components
```csharp
TestHelper.RunAllTests(this);
```

### Test Individual
```csharp
TestHelper.TestConfig();
TestHelper.TestDatabase();
TestHelper.TestInsertDummyData();
```

### Manual Test
```csharp
// Test config
var config = ConfigManager.LoadConfig();
MessageBox.Show($"DB: {config.Database.Database}");

// Test database
var logger = new DataLogger(config.Database.GetConnectionString());
string error;
bool ok = logger.TestConnection(out error);
MessageBox.Show(ok ? "Connected!" : $"Error: {error}");

// Test sound
SoundAlertManager.PlayBeep(1000, 200);
```

---

## 📋 COMMON PATTERNS

### Pattern 1: Save Inspection with Error Handling
```csharp
try
{
    var result = new InspectionResult { /* ... */ };
    int id = dataLogger.InsertInspectionResult(result);
    
    UpdateStatusBar(result.Result);
    RefreshResultsTable();
}
catch (Exception ex)
{
    MessageBox.Show($"Error: {ex.Message}", "Error", 
        MessageBoxButtons.OK, MessageBoxIcon.Error);
}
```

### Pattern 2: Validate Config Before Use
```csharp
var config = ConfigManager.LoadConfig();
string error;

if (!ConfigManager.ValidateConfig(config, out error))
{
    MessageBox.Show($"Config error: {error}", "Error");
    return;
}

// Use config safely
var logger = new DataLogger(config.Database.GetConnectionString());
```

### Pattern 3: Graceful Database Failure
```csharp
try
{
    var logger = new DataLogger(connectionString);
    string error;
    
    if (logger.TestConnection(out error))
    {
        logger.InitializeDatabase();
        // Continue with operations
    }
    else
    {
        // Log to file or show warning, but don't crash
        Console.WriteLine($"DB unavailable: {error}");
    }
}
catch (Exception ex)
{
    // Always catch, never crash
    Console.WriteLine($"DB exception: {ex.Message}");
}
```

---

## 🔑 KEY METHODS

### ConfigManager
- `LoadConfig()` → AppConfig
- `SaveConfig(config)` → void
- `GetConfig()` → AppConfig (singleton)
- `ValidateConfig(config, out error)` → bool

### DataLogger
- `TestConnection(out error)` → bool
- `InitializeDatabase()` → void
- `InsertInspectionResult(result)` → int (id)
- `GetRecentResults(limit)` → DataTable
- `GetTodayStatistics()` → InspectionStatistics
- `UpdateCameraConfig(camera)` → void

### SoundAlertManager
- `PlayNGAlert()` → void
- `PlayWarningAlert()` → void
- `PlayBeep(frequency, duration)` → void
- `SetEnabled(enabled)` → void

---

## ⚡ PERFORMANCE TIPS

1. **Reuse DataLogger instance** - Don't create new one every call
2. **Batch inserts** - If inserting multiple results, consider transaction
3. **Use async/await** - For database operations in TAHAP 2+
4. **Cache config** - Use `ConfigManager.GetConfig()` instead of `LoadConfig()` repeatedly
5. **Limit query results** - Use `GetRecentResults(100)` not all data

---

## 🐛 DEBUG TIPS

### Enable Console Output
```csharp
// Program.cs - Add before Application.Run()
AllocConsole(); // Windows API call
Console.WriteLine("Debug mode enabled");
```

### Log to File
```csharp
File.AppendAllText("debug.log", 
    $"{DateTime.Now}: {message}\n");
```

### Breakpoint Locations
- `ConfigManager.LoadConfig()` - Check config values
- `DataLogger.InsertInspectionResult()` - Verify data
- `MainForm_Load()` - UI initialization

---

**Print this page and keep near your desk! 📌**

**Version:** 1.0 - TAHAP 1  
**Updated:** January 2026
