# 🤖 INSPECTION AI PLATFORM

> **Platform AI Inspection untuk Factory Automation dengan Dark Theme UI**

Platform ini memungkinkan **user** untuk melakukan labeling, training, dan inspection AI tanpa perlu coding. Dirancang khusus untuk factory environment dengan support multi-camera HIKRobot, PLC integration, dan real-time inspection.

---

## ✨ FEATURES

### ✅ ALREADY IMPLEMENTED (TAHAP 1)
- [x] **Modern Dark Theme UI** - Professional dark interface
- [x] **Configuration Management** - JSON-based config with auto-save
- [x] **MySQL Database Integration** - Store inspection results & statistics
- [x] **Sound Alert System** - Beep notification untuk NG detection
- [x] **Multi-Camera Support** - Manage multiple HIKRobot cameras
- [x] **Status Bar Indicators** - Blue (Running), Green (Good), Red (NG), Orange (Warning)
- [x] **Data Models** - InspectionResult, CameraInfo, AppConfig

### 🔨 IN PROGRESS (TAHAP 2-5)
- [ ] **Camera Live Feed** - Real-time streaming dari HIKRobot
- [ ] **AI Inference Engine** - ONNX model runner
- [ ] **Labeling Tool** - LabelStudio-like interface
- [ ] **PLC/Serial Integration** - Auto-trigger inspection
- [ ] **NG Image Auto-Save** - Traceability dengan timestamp

---

## 🏗️ ARCHITECTURE

```
┌─────────────────────────────────────────────────────────┐
│                   INSPECTION AI PLATFORM                 │
├─────────────────────────────────────────────────────────┤
│  UI Layer           │  MainForm, LabelingForm           │
│  Business Logic     │  Managers (Camera, Inference, DB) │
│  Data Models        │  Result, Config, CameraInfo       │
│  External Systems   │  MySQL, HIKRobot, PLC, ONNX      │
└─────────────────────────────────────────────────────────┘
```

Lihat [ARCHITECTURE.md](ARCHITECTURE.md) untuk diagram lengkap.

---

## 📦 REQUIREMENTS

### Software
- ✅ Windows 10/11 (64-bit)
- ✅ .NET Framework 4.8
- ✅ Visual Studio 2019/2022
- ✅ MySQL Server 8.0+
- ✅ HIKRobot MVS Software

### Hardware
- ✅ CPU: Intel i5 or higher (CPU-only inference)
- ✅ RAM: 8GB minimum
- ✅ HIKRobot Cameras (GigE Vision)
- ✅ PLC dengan Serial COM port (optional)

### NuGet Packages
```
Newtonsoft.Json         13.0.3
MySql.Data              8.2.0
Microsoft.ML.OnnxRuntime 1.16.3
System.Drawing.Common    8.0.0
```

---

## 🚀 QUICK START

### 1. Clone Repository
```bash
git clone https://github.com/yourcompany/InspectionAI.git
cd InspectionAI
```

### 2. Install Dependencies
Buka **Package Manager Console** di Visual Studio:
```powershell
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package MySql.Data -Version 8.2.0
Install-Package Microsoft.ML.OnnxRuntime -Version 1.16.3
```

### 3. Setup Database
```sql
CREATE DATABASE inspection_db;
```

### 4. Configure Application
Edit `config.json` (auto-generated saat pertama run):
```json
{
  "Database": {
    "Server": "localhost",
    "Database": "inspection_db",
    "Username": "root",
    "Password": "your_password"
  },
  "Serial": {
    "PortName": "COM3",
    "BaudRate": 9600
  }
}
```

### 5. Build & Run
```
Build → Rebuild Solution → Start (F5)
```

Lihat [INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md) untuk step-by-step lengkap.

---

## 📖 DOCUMENTATION

| Document | Description |
|----------|-------------|
| [INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md) | Step-by-step installation & setup |
| [ARCHITECTURE.md](ARCHITECTURE.md) | System architecture & diagrams |
| [API_REFERENCE.md](#) | Class & method documentation *(coming soon)* |
| [USER_MANUAL.md](#) | End-user guide *(coming soon)* |

---

## 📂 PROJECT STRUCTURE

```
InspectionAI/
├── Classes/
│   ├── Models/              # Data models
│   │   ├── InspectionResult.cs
│   │   ├── CameraInfo.cs
│   │   └── AppConfig.cs
│   ├── Managers/            # Business logic
│   │   ├── ConfigManager.cs
│   │   ├── DataLogger.cs
│   │   ├── CameraManager.cs      [TAHAP 2]
│   │   └── SoundAlertManager.cs
│   ├── AI/                  # AI inference [TAHAP 3]
│   │   ├── InferenceEngine.cs
│   │   └── ModelConfig.cs
│   └── Hardware/            # Hardware integration [TAHAP 5]
│       └── PLCController.cs
├── Forms/
│   ├── MainForm.cs          # Main UI
│   └── Labeling/            # Labeling tool [TAHAP 4]
│       └── LabelingForm.cs
├── Resources/
│   └── Sounds/              # Alert sounds
├── config.json              # Application config
└── InspectionAI.exe         # Main executable
```

---

## 🎯 USAGE

### For End Users (Operators)

1. **Start Inspection**
   - Click **Run** button atau wait untuk PLC trigger
   - Status bar akan berubah **BLUE** (Running)

2. **Monitor Results**
   - **GREEN** = All Good ✅
   - **RED** = NG Detected ❌ (dengan sound alert)
   - **ORANGE** = Warning ⚠️ (low confidence / camera issue)

3. **View History**
   - Check **DataGridView** untuk hasil inspection
   - NG images auto-saved di folder `NG_Images/`

### For Setup Users (Engineers)

1. **Labeling Mode** *(TAHAP 4)*
   - Open **Labeling → New Project**
   - Import images atau capture dari camera
   - Draw bounding boxes
   - Export YOLO JSON untuk AI team

2. **Import AI Model** *(TAHAP 3)*
   - File → Import Model (`.onnx`)
   - Load `classes.txt`
   - Set confidence threshold

3. **Camera Configuration**
   - View → Camera Panel
   - Select cameras to enable
   - Click **Refresh** untuk detect cameras

---

## 🔧 CONFIGURATION

### config.json Structure
```json
{
  "Database": {
    "Server": "localhost",
    "Port": 3306,
    "Database": "inspection_db",
    "Username": "root",
    "Password": ""
  },
  "Serial": {
    "PortName": "COM3",
    "BaudRate": 9600,
    "AutoRunOnTrigger": true,
    "TriggerDelayMs": 500
  },
  "Model": {
    "ModelPath": "models/inspection.onnx",
    "ClassesPath": "models/classes.txt",
    "ConfidenceThreshold": 0.5,
    "InputWidth": 640,
    "InputHeight": 640
  },
  "Inspection": {
    "SaveNGImages": true,
    "NGImageFolder": "NG_Images",
    "PlaySoundOnNG": true,
    "MinConfidenceWarning": 30
  },
  "Cameras": [
    {
      "CameraId": "Camera_1",
      "IpAddress": "10.0.0.101",
      "IsEnabled": true,
      "ExposureTime": 10000,
      "Gain": 0
    }
  ]
}
```

---

## 🐛 TROUBLESHOOTING

### Database Connection Failed
```
Error: Unable to connect to MySQL server

Solution:
1. Check MySQL service running
2. Verify credentials di config.json
3. Test connection dengan MySQL Workbench
```

### Camera Not Found
```
Error: No HIKRobot cameras detected

Solution:
1. Open MVS software → Check camera connected
2. Verify camera IP address
3. Check network connection
4. Restart HIKRobot SDK service
```

### ONNX Model Error
```
Error: Failed to load ONNX model

Solution:
1. Check model path di config.json
2. Verify ONNX Runtime installed
3. Ensure model compatible dengan OnnxRuntime 1.16.3
```

Lihat [INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md) untuk more solutions.

---

## 📊 DATABASE SCHEMA

### inspection_results
| Column | Type | Description |
|--------|------|-------------|
| id | INT | Primary key |
| timestamp | DATETIME | Inspection time |
| set_number | VARCHAR(50) | Product set number |
| camera_id | VARCHAR(50) | Camera identifier |
| result | ENUM | GOOD/NG/WARNING |
| defect_summary | TEXT | Defect description |
| inspection_time_ms | INT | Processing time |
| image_path | VARCHAR(255) | Path to saved image |

### detection_details
| Column | Type | Description |
|--------|------|-------------|
| id | INT | Primary key |
| inspection_id | INT | FK to inspection_results |
| class_name | VARCHAR(100) | Object class |
| confidence | FLOAT | Detection confidence |
| bbox_x, bbox_y | FLOAT | Bounding box position |
| bbox_width, bbox_height | FLOAT | Bounding box size |
| is_defect | BOOLEAN | Defect flag |

---

## 🤝 INTEGRATION WITH AI TEAM

### What User Provides (Export dari Platform)
```
annotations/
├── images/              # Captured images
│   ├── img001.jpg
│   ├── img002.jpg
│   └── ...
└── labels.json          # YOLO format
    {
      "images": [...],
      "annotations": [
        {
          "image_id": 1,
          "category_id": 0,
          "bbox": [x, y, w, h],
          "category_name": "screw_good"
        }
      ]
    }
```

### What AI Team Returns
```
models/
├── inspection.onnx      # Trained model
├── classes.txt          # Class names
└── config.json          # Model metadata
    {
      "input_size": [640, 640],
      "confidence_threshold": 0.5,
      "classes": ["screw_good", "screw_ng", "solder_ok", ...]
    }
```

---

## 📈 ROADMAP

### TAHAP 1: Foundation ✅ (DONE)
- [x] Project structure
- [x] Dark theme UI
- [x] Config management
- [x] Database integration

### TAHAP 2: Camera Integration (IN PROGRESS)
- [ ] HIKRobot SDK wrapper
- [ ] Live camera feed
- [ ] Multi-camera support
- [ ] FPS monitoring

### TAHAP 3: AI Inference Engine (NEXT)
- [ ] ONNX Runtime integration
- [ ] Model loader
- [ ] Bounding box overlay
- [ ] Real-time detection

### TAHAP 4: Labeling Tool
- [ ] Draw bounding boxes
- [ ] Class management
- [ ] Export YOLO/JSON
- [ ] Import dataset

### TAHAP 5: Automation
- [ ] PLC serial communication
- [ ] Auto-trigger inspection
- [ ] NG image auto-save
- [ ] Statistics dashboard

---

## 📝 LICENSE

Proprietary - Internal Use Only  
© 2026 Your Company Name

---

## 👥 CONTRIBUTORS

- **Lead Developer:** Your Name
- **AI Integration:** AI Team
- **Hardware Integration:** Automation Team
- **Architecture Design:** Claude AI

---

## 📞 SUPPORT

For technical support:
- 📧 Email: support@yourcompany.com
- 💬 Slack: #inspection-ai-support
- 📱 Phone: +62-xxx-xxxx-xxxx

---

## 🔄 CHANGELOG

### Version 1.0.0 - TAHAP 1 (January 2026)
- ✅ Initial release
- ✅ Core business logic classes
- ✅ MySQL integration
- ✅ Configuration management
- ✅ Dark theme UI
- ✅ Sound alert system

---

**Built with ❤️ for Factory Automation**
