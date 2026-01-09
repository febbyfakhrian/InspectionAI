# 🏗️ INSPECTION AI PLATFORM - ARCHITECTURE

## 📊 SYSTEM ARCHITECTURE DIAGRAM

```mermaid
graph TB
    subgraph "USER INTERFACE LAYER"
        A[MainForm.cs<br/>Dark Theme UI] 
        B[LabelingForm.cs<br/>Draw Bounding Boxes]
        C[SettingsForm.cs<br/>Configuration]
    end
    
    subgraph "BUSINESS LOGIC LAYER"
        D[ConfigManager<br/>Load/Save JSON Config]
        E[DataLogger<br/>MySQL CRUD]
        F[CameraManager<br/>HIKRobot SDK]
        G[InferenceEngine<br/>ONNX Runtime]
        H[PLCController<br/>Serial COM]
        I[SoundAlertManager<br/>Audio Alerts]
    end
    
    subgraph "DATA MODELS"
        J[InspectionResult<br/>Detections]
        K[CameraInfo<br/>Camera Config]
        L[AppConfig<br/>Settings]
    end
    
    subgraph "EXTERNAL SYSTEMS"
        M[(MySQL Database<br/>inspection_db)]
        N[HIKRobot Cameras<br/>10.0.0.101-103]
        O[PLC/Sensor<br/>COM3 Serial]
        P[ONNX Model<br/>AI Inference]
    end
    
    A --> D
    A --> E
    A --> F
    A --> G
    A --> H
    A --> I
    
    B --> D
    B --> F
    
    C --> D
    
    D --> L
    E --> M
    E --> J
    F --> N
    F --> K
    G --> P
    G --> J
    H --> O
    
    style A fill:#1e1e1e,stroke:#0078d4,color:#fff
    style B fill:#1e1e1e,stroke:#0078d4,color:#fff
    style C fill:#1e1e1e,stroke:#0078d4,color:#fff
    style D fill:#2d2d30,stroke:#569cd6,color:#fff
    style E fill:#2d2d30,stroke:#569cd6,color:#fff
    style F fill:#2d2d30,stroke:#569cd6,color:#fff
    style G fill:#2d2d30,stroke:#569cd6,color:#fff
    style H fill:#2d2d30,stroke:#569cd6,color:#fff
    style I fill:#2d2d30,stroke:#569cd6,color:#fff
    style M fill:#d73a4a,stroke:#fff,color:#fff
    style N fill:#28a745,stroke:#fff,color:#fff
    style O fill:#ffd700,stroke:#333,color:#333
    style P fill:#6f42c1,stroke:#fff,color:#fff
```

---

## 🔄 WORKFLOW DIAGRAM

### TRAINING PHASE (User → AI Team)

```mermaid
sequenceDiagram
    participant User
    participant LabelingForm
    participant CameraManager
    participant ConfigManager
    participant AI_Team
    
    User->>LabelingForm: Open Labeling Mode
    LabelingForm->>CameraManager: Capture Image / Import File
    CameraManager-->>LabelingForm: Return Image
    User->>LabelingForm: Draw Bounding Boxes
    User->>LabelingForm: Assign Classes (screw_good, screw_ng, etc)
    LabelingForm->>ConfigManager: Export YOLO JSON
    ConfigManager-->>User: Save annotations.json
    User->>AI_Team: Send JSON + Images
    AI_Team-->>User: Return ONNX Model
```

---

### INSPECTION PHASE (Production)

```mermaid
sequenceDiagram
    participant PLC
    participant PLCController
    participant MainForm
    participant CameraManager
    participant InferenceEngine
    participant DataLogger
    participant SoundAlert
    
    PLC->>PLCController: Trigger Signal (Serial)
    PLCController->>MainForm: OnSensorTriggered()
    MainForm->>MainForm: Delay 500ms (stabilization)
    MainForm->>CameraManager: Grab Frame (all cameras)
    CameraManager-->>MainForm: Return Images
    MainForm->>InferenceEngine: Run Inference (ONNX)
    InferenceEngine-->>MainForm: Return Detections
    
    alt All Detections GOOD
        MainForm->>MainForm: Set StatusBar = GREEN
        MainForm->>DataLogger: Save Result (GOOD)
    else Any Detection NG
        MainForm->>MainForm: Set StatusBar = RED
        MainForm->>DataLogger: Save Result (NG) + Image
        MainForm->>SoundAlert: Play NG Alert
    else Low Confidence
        MainForm->>MainForm: Set StatusBar = ORANGE
        MainForm->>DataLogger: Save Result (WARNING)
        MainForm->>SoundAlert: Play Warning Alert
    end
    
    DataLogger->>DataLogger: Insert to MySQL
    MainForm->>MainForm: Display Results in DataGridView
```

---

## 📁 CLASS DIAGRAM

```mermaid
classDiagram
    class MainForm {
        +ConfigManager config
        +DataLogger logger
        +CameraManager camera
        +InferenceEngine inference
        +PLCController plc
        +OnRunClick()
        +OnStopClick()
        +OnSensorTriggered()
        +UpdateStatusBar(status)
        +DisplayResults(results)
    }
    
    class InspectionResult {
        +int Id
        +DateTime Timestamp
        +string SetNumber
        +string CameraId
        +InspectionStatus Result
        +List~DetectionResult~ Detections
        +int InspectionTimeMs
        +string ImagePath
        +GetDefectSummary()
    }
    
    class DetectionResult {
        +string ClassName
        +float Confidence
        +float X, Y, Width, Height
        +bool IsDefect
        +ToPixelRect()
    }
    
    class CameraInfo {
        +string CameraId
        +string IpAddress
        +bool IsConnected
        +int CurrentFPS
        +GetDisplayText()
        +IsAlive()
    }
    
    class AppConfig {
        +DatabaseConfig Database
        +SerialConfig Serial
        +ModelConfig Model
        +InspectionConfig Inspection
        +List~CameraConfig~ Cameras
    }
    
    class ConfigManager {
        +LoadConfig()
        +SaveConfig(config)
        +GetConfig()
        +ValidateConfig()
    }
    
    class DataLogger {
        +TestConnection()
        +InitializeDatabase()
        +InsertInspectionResult(result)
        +GetRecentResults()
        +GetTodayStatistics()
    }
    
    class CameraManager {
        +List~CameraInfo~ Cameras
        +ConnectAll()
        +GrabFrame(cameraId)
        +Disconnect()
    }
    
    class InferenceEngine {
        +LoadModel(path)
        +RunInference(image)
        +GetDetections()
    }
    
    class PLCController {
        +OpenSerialPort(portName)
        +OnTriggerReceived event
        +SendCommand(cmd)
    }
    
    MainForm --> ConfigManager
    MainForm --> DataLogger
    MainForm --> CameraManager
    MainForm --> InferenceEngine
    MainForm --> PLCController
    
    InspectionResult --> DetectionResult
    ConfigManager --> AppConfig
    DataLogger --> InspectionResult
    CameraManager --> CameraInfo
```

---

## 🎨 UI COMPONENT HIERARCHY

```mermaid
graph TD
    A[MainForm - ModernForm Base]
    A --> B[panelTopBar<br/>Custom Title Bar]
    A --> C[MenuStrip<br/>File, Edit, Inspection, View, Help]
    A --> D[ToolStrip<br/>Run, Stop, Import, Settings]
    A --> E[SplitContainer]
    A --> F[StatusStrip<br/>Status Label with Color]
    
    B --> B1[btnMinimize]
    B --> B2[btnMaximize]
    B --> B3[btnClose]
    
    E --> G[Panel Left<br/>Camera View]
    E --> H[TabControl Right]
    
    G --> G1[lblCameraPlaceholder<br/>Live Feed Display]
    
    H --> H1[TabPage: Cameras]
    H --> H2[TabPage: NG Gallery]
    H --> H3[TabPage: Statistics]
    
    H1 --> I1[CheckedListBox<br/>Camera List]
    H1 --> I2[btnRefreshCameras]
    
    H2 --> J1[FlowLayoutPanel<br/>NG Thumbnails]
    
    H3 --> K1[DataGridView<br/>Inspection Results]
    
    style A fill:#1e1e1e,stroke:#0078d4,color:#fff
    style E fill:#2d2d30,stroke:#569cd6,color:#fff
    style F fill:#3e3e42,stroke:#569cd6,color:#fff
```

---

## 🔌 EXTERNAL INTEGRATIONS

### 1. HIKRobot Camera SDK
```
MvCameraControl.dll (64-bit)
├── MV_CC_CreateHandle()
├── MV_CC_OpenDevice()
├── MV_CC_StartGrabbing()
├── MV_CC_GetOneFrameTimeout()
└── MV_CC_CloseDevice()
```

### 2. ONNX Runtime
```
Microsoft.ML.OnnxRuntime
├── InferenceSession.Run()
├── Input: float[1,3,640,640]
├── Output: float[1,25200,85]
└── Post-processing: NMS, Threshold
```

### 3. MySQL Database
```sql
inspection_results
├── id (PK)
├── timestamp
├── set_number
├── camera_id
├── result (GOOD/NG/WARNING)
├── defect_summary
└── inspection_time_ms

detection_details
├── id (PK)
├── inspection_id (FK)
├── class_name
├── confidence
└── bbox coordinates
```

### 4. Serial PLC Communication
```
System.IO.Ports.SerialPort
├── PortName: COM3
├── BaudRate: 9600
├── DataReceived Event
└── Trigger Command: 0x01
```

---

## 📊 DATA FLOW

```mermaid
flowchart LR
    A[PLC Sensor] -->|Serial| B[PLCController]
    B -->|Trigger| C[MainForm]
    C -->|Grab| D[CameraManager]
    D -->|Image| E[InferenceEngine]
    E -->|Detections| F{Check Result}
    F -->|GOOD| G[Green Status]
    F -->|NG| H[Red Status + Sound]
    F -->|WARNING| I[Orange Status]
    G --> J[DataLogger]
    H --> J
    I --> J
    J -->|Save| K[(MySQL)]
    
    style A fill:#ffd700,stroke:#333
    style D fill:#28a745,stroke:#fff,color:#fff
    style E fill:#6f42c1,stroke:#fff,color:#fff
    style H fill:#d73a4a,stroke:#fff,color:#fff
    style K fill:#d73a4a,stroke:#fff,color:#fff
```

---

## 🚀 DEPLOYMENT ARCHITECTURE

```
Factory Floor
├── Station 1
│   ├── PC (Windows 10)
│   ├── Camera 1-3 (HIKRobot)
│   ├── PLC (Serial COM3)
│   └── InspectionAI.exe
│
├── Station 2
│   ├── PC (Windows 10)
│   ├── Camera 1-2
│   └── InspectionAI.exe
│
└── Central MySQL Server
    ├── IP: 192.168.1.100
    ├── Port: 3306
    └── Database: inspection_db
```

---

**Author:** Claude AI  
**Version:** 1.0  
**Last Updated:** January 2026
