using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using InspectionAI.Classes.Models;

namespace InspectionAI.Classes.AI
{
    /// <summary>
    /// AI Client untuk communicate dengan AI Inference Server
    /// Mock version - simulate AI response
    /// TODO: Replace dengan real gRPC/REST client
    /// </summary>
    public class AIClient
    {
        private string serverUrl;
        private Random random;
        private bool isConnected;

        public bool IsConnected => isConnected;

        public AIClient(string url = "http://localhost:5000")
        {
            serverUrl = url;
            random = new Random();
            isConnected = false;
        }

        /// <summary>
        /// Connect to AI server
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                // TODO: Implement real connection check
                // For now, mock always success
                await Task.Delay(100); // Simulate network delay

                isConnected = true;
                return true;
            }
            catch
            {
                isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Process image - Send ke AI server untuk inference
        /// </summary>
        public async Task<AIInferenceResult> ProcessImageAsync(Bitmap image, string cameraId)
        {
            if (!isConnected)
            {
                return new AIInferenceResult
                {
                    Success = false,
                    ErrorMessage = "Not connected to AI server"
                };
            }

            try
            {
                // TODO: Implement real gRPC/REST call
                // For now, simulate AI processing

                await Task.Delay(50); // Simulate inference time (50ms)

                // Mock detection results
                var detections = GenerateMockDetections();

                // Determine overall result
                bool isPass = !detections.Exists(d => d.IsDefect);

                return new AIInferenceResult
                {
                    Success = true,
                    IsPass = isPass,
                    FinalLabel = isPass,
                    Detections = detections,
                    InferenceTimeMs = 50,
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                return new AIInferenceResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Generate mock detections (simulate AI output)
        /// TODO: Remove this when using real AI
        /// </summary>
        private List<DetectionResult> GenerateMockDetections()
        {
            var detections = new List<DetectionResult>();

            // Random: 80% chance GOOD, 20% chance NG
            int numDetections = random.Next(1, 4); // 1-3 objects

            for (int i = 0; i < numDetections; i++)
            {
                bool isDefect = random.Next(100) >= 80; // 20% NG

                detections.Add(new DetectionResult
                {
                    ClassName = isDefect ? "screw_ng" : "screw_good",
                    Confidence = 0.75f + (float)(random.NextDouble() * 0.2), // 0.75-0.95
                    X = 0.3f + (float)(random.NextDouble() * 0.4), // 0.3-0.7
                    Y = 0.3f + (float)(random.NextDouble() * 0.4),
                    Width = 0.05f + (float)(random.NextDouble() * 0.1), // 0.05-0.15
                    Height = 0.05f + (float)(random.NextDouble() * 0.1),
                    IsDefect = isDefect
                });
            }

            return detections;
        }

        /// <summary>
        /// Disconnect from AI server
        /// </summary>
        public void Disconnect()
        {
            isConnected = false;
        }
    }

    /// <summary>
    /// AI Inference Result
    /// This matches the response from AI server
    /// </summary>
    public class AIInferenceResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        // Result dari AI
        public bool IsPass { get; set; }
        public bool FinalLabel { get; set; } // Same as IsPass (dari workflow)
        public List<DetectionResult> Detections { get; set; }

        // Metadata
        public int InferenceTimeMs { get; set; }
        public DateTime Timestamp { get; set; }

        public AIInferenceResult()
        {
            Detections = new List<DetectionResult>();
        }
    }
}