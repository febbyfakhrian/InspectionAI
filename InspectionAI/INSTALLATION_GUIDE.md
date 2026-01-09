# 🚀 INSPECTION AI PLATFORM - INSTALLATION GUIDE

## 📋 TAHAP 1: SETUP NUGET PACKAGES

### Step 1: Buka Visual Studio
1. Open project `InspectionAI.sln`
2. Replace file `InspectionAI.csproj` dengan `InspectionAI_Updated.csproj`
3. Rename `InspectionAI_Updated.csproj` → `InspectionAI.csproj`

### Step 2: Install NuGet Packages
Buka **Tools → NuGet Package Manager → Package Manager Console**

Jalankan command berikut satu per satu:

```powershell
# JSON Serialization
Install-Package Newtonsoft.Json -Version 13.0.3

# MySQL Database
Install-Package MySql.Data -Version 8.2.0

# ONNX Runtime (AI Inference)
Install-Package Microsoft.ML.OnnxRuntime -Version 1.16.3

# Image Processing (optional, untuk TAHAP 2)
Install-Package System.Drawing.Common -Version 8.0.0
```

### Step 3: Build Project
1. Klik **Build → Rebuild Solution**
2. Pastikan tidak ada error (ignore warning)
3. Check folder `bin/Debug` → ada `InspectionAI.exe`

---

## 🗄️ TAHAP 2: SETUP MYSQL DATABASE

### Step 1: Pastikan MySQL Running
- Buka **MySQL Workbench** atau **phpMyAdmin**
- Test connection ke `localhost:3306`

### Step 2: Create Database
```sql
CREATE DATABASE inspection_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE inspection_db;
```

### Step 3: Test dari Aplikasi
1. Run `InspectionAI.exe`
2. Akan auto-create file `config.json`
3. Buka `config.json`, edit database settings:

```json
{
  "Database": {
    "Server": "localhost",
    "Database": "inspection_db",
    "Username": "root",
    "Password": "your_password_here",
    "Port": 3306
  }
}
```

4. **IMPORTANT:** Simpan file `config.json`
5. Restart aplikasi
6. Database tables akan auto-create saat pertama kali run

---

## 🎨 TAHAP 3: VERIFY DARK THEME

### Check List:
- ✅ MenuStrip: Dark background, white text
- ✅ ToolStrip: Dark background
- ✅ StatusStrip: Blue (RUNNING), Gray (IDLE), Red (NG), Orange (WARNING)
- ✅ CheckedListBox: Dark background dengan white text
- ✅ DataGridView: Dark theme
- ✅ TabControl: Dark theme dengan custom tabs

### Jika ada yang putih:
- Check `MainForm_Load()` event
- Pastikan semua controls sudah di-set `BackColor` dan `ForeColor`

---

## 🧪 TAHAP 4: TEST FUNCTIONALITY

### Test 1: Config Manager
```csharp
// Add di MainForm constructor untuk testing
var config = ConfigManager.LoadConfig();
MessageBox.Show($"Database: {config.Database.Database}");
```

### Test 2: Database Connection
```csharp
// Add di MainForm_Load untuk testing
try
{
    var logger = new DataLogger(ConfigManager.GetConfig().Database.GetConnectionString());
    string error;
    
    if (logger.TestConnection(out error))
    {
        MessageBox.Show("Database connected!");
        logger.InitializeDatabase();
    }
    else
    {
        MessageBox.Show($"Database error: {error}");
    }
}
catch (Exception ex)
{
    MessageBox.Show($"Error: {ex.Message}");
}
```

### Test 3: Insert Dummy Data
```csharp
// Test insert inspection result
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
    X = 0.5f,
    Y = 0.5f,
    Width = 0.1f,
    Height = 0.1f,
    IsDefect = false
});

logger.InsertInspectionResult(result);
MessageBox.Show("Data saved!");
```

### Test 4: Sound Alert
```csharp
// Test sound alert
SoundAlertManager.PlayBeep(1000, 200); // NG alert
```

---

## ⚠️ COMMON ERRORS & SOLUTIONS

### Error: "Could not load file or assembly 'Newtonsoft.Json'"
**Solution:**
```powershell
Install-Package Newtonsoft.Json -Version 13.0.3
```

### Error: "Unable to connect to MySQL server"
**Solution:**
1. Check MySQL service running:
   - Windows: Services → MySQL80 → Start
2. Check `config.json` database credentials
3. Test connection dengan MySQL Workbench

### Error: "The type or namespace 'MySql' could not be found"
**Solution:**
```powershell
Install-Package MySql.Data -Version 8.2.0
```

### Error: "Access denied for user 'root'@'localhost'"
**Solution:**
1. Buka MySQL Workbench
2. Run: `ALTER USER 'root'@'localhost' IDENTIFIED BY 'your_new_password';`
3. Update `config.json` dengan password baru

---

## 📂 FILE STRUCTURE (After TAHAP 1)

```
InspectionAI/
├── Classes/
│   ├── Models/
│   │   ├── InspectionResult.cs      ✅ Done
│   │   ├── CameraInfo.cs            ✅ Done
│   │   └── AppConfig.cs             ✅ Done
│   ├── Managers/
│   │   ├── ConfigManager.cs         ✅ Done
│   │   ├── DataLogger.cs            ✅ Done
│   │   └── SoundAlertManager.cs     ✅ Done
│   ├── AI/                          ⏳ TAHAP 3
│   └── Hardware/                    ⏳ TAHAP 5
├── Forms/
│   └── Labeling/                    ⏳ TAHAP 4
├── Resources/
│   └── Sounds/                      (Add .wav files here)
├── MainForm.cs                      ✅ Done (existing)
├── config.json                      (Auto-generated)
└── InspectionAI.exe

```

---

## ✅ CHECKLIST BEFORE MOVING TO TAHAP 2

- [ ] Project builds without errors
- [ ] All NuGet packages installed
- [ ] `config.json` created dengan correct database settings
- [ ] MySQL connection successful
- [ ] Database tables auto-created
- [ ] Can insert dummy inspection result
- [ ] Sound alert works (atau at least beep)
- [ ] Dark theme applied correctly

---

## 🎯 NEXT STEPS

Setelah TAHAP 1 selesai dan semua test passed:

**TAHAP 2:** Camera Integration
- CameraManager.cs (HIKRobot SDK)
- ImageProcessor.cs
- Real-time camera feed di MainForm

**Pertanyaan untuk TAHAP 2:**
1. HIKRobot SDK sudah installed? Location: `C:\Program Files (x86)\MVS\`?
2. Ada camera HIKRobot connected untuk testing?
3. Camera IP address berapa?

---

## 📞 SUPPORT

Jika ada error atau pertanyaan, kirim:
1. Screenshot error message
2. File `config.json` (tanpa password)
3. Output dari **Build** window

---

**Author:** Claude AI
**Version:** 1.0 - TAHAP 1 FOUNDATION
**Date:** January 2026
