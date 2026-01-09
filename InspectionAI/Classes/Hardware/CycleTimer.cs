using System;
using System.Diagnostics;

namespace InspectionAI.Classes.Hardware
{
    /// <summary>
    /// Cycle Timer untuk measure inspection cycle time
    /// </summary>
    public class CycleTimer
    {
        private Stopwatch stopwatch;
        private DateTime startTime;
        private DateTime endTime;
        private bool isRunning;

        public bool IsRunning => isRunning;
        public long ElapsedMilliseconds => stopwatch?.ElapsedMilliseconds ?? 0;
        public TimeSpan ElapsedTime => stopwatch?.Elapsed ?? TimeSpan.Zero;

        public CycleTimer()
        {
            stopwatch = new Stopwatch();
            isRunning = false;
        }

        /// <summary>
        /// Start cycle timer
        /// </summary>
        public void Start()
        {
            startTime = DateTime.Now;
            stopwatch.Restart();
            isRunning = true;
        }

        /// <summary>
        /// Stop cycle timer
        /// </summary>
        public long Stop()
        {
            if (!isRunning)
                return 0;

            stopwatch.Stop();
            endTime = DateTime.Now;
            isRunning = false;

            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Reset timer
        /// </summary>
        public void Reset()
        {
            stopwatch.Reset();
            isRunning = false;
        }

        /// <summary>
        /// Get formatted elapsed time
        /// </summary>
        public string GetFormattedTime()
        {
            TimeSpan ts = stopwatch.Elapsed;
            return string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        /// <summary>
        /// Get cycle time info
        /// </summary>
        public CycleTimeInfo GetInfo()
        {
            return new CycleTimeInfo
            {
                StartTime = startTime,
                EndTime = endTime,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                IsRunning = isRunning
            };
        }
    }

    /// <summary>
    /// Cycle time information
    /// </summary>
    public class CycleTimeInfo
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long ElapsedMs { get; set; }
        public bool IsRunning { get; set; }

        public TimeSpan Duration => TimeSpan.FromMilliseconds(ElapsedMs);
    }
}