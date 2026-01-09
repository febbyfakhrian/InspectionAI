using System;

namespace InspectionAI.Classes.Hardware
{
    /// <summary>
    /// Production Counter untuk track statistics
    /// </summary>
    public class ProductionCounter
    {
        private int totalCount;
        private int goodCount;
        private int ngCount;
        private int warningCount;
        private DateTime sessionStart;
        private DateTime lastUpdate;

        public int TotalCount => totalCount;
        public int GoodCount => goodCount;
        public int NgCount => ngCount;
        public int WarningCount => warningCount;
        public DateTime SessionStart => sessionStart;
        public TimeSpan SessionDuration => DateTime.Now - sessionStart;

        public double GoodRate => totalCount > 0 ? (goodCount * 100.0 / totalCount) : 0;
        public double NgRate => totalCount > 0 ? (ngCount * 100.0 / totalCount) : 0;

        public ProductionCounter()
        {
            Reset();
        }

        /// <summary>
        /// Record GOOD result
        /// </summary>
        public void RecordGood()
        {
            totalCount++;
            goodCount++;
            lastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Record NG result
        /// </summary>
        public void RecordNG()
        {
            totalCount++;
            ngCount++;
            lastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Record WARNING result
        /// </summary>
        public void RecordWarning()
        {
            totalCount++;
            warningCount++;
            lastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Reset counter
        /// </summary>
        public void Reset()
        {
            totalCount = 0;
            goodCount = 0;
            ngCount = 0;
            warningCount = 0;
            sessionStart = DateTime.Now;
            lastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Get statistics summary
        /// </summary>
        public ProductionStatistics GetStatistics()
        {
            return new ProductionStatistics
            {
                TotalCount = totalCount,
                GoodCount = goodCount,
                NgCount = ngCount,
                WarningCount = warningCount,
                GoodRate = GoodRate,
                NgRate = NgRate,
                SessionDuration = SessionDuration,
                LastUpdate = lastUpdate
            };
        }

        /// <summary>
        /// Get formatted summary text
        /// </summary>
        public string GetSummaryText()
        {
            return $"Total: {totalCount} | Good: {goodCount} ({GoodRate:F1}%) | NG: {ngCount} ({NgRate:F1}%) | Warning: {warningCount}";
        }
    }

    /// <summary>
    /// Production statistics
    /// </summary>
    public class ProductionStatistics
    {
        public int TotalCount { get; set; }
        public int GoodCount { get; set; }
        public int NgCount { get; set; }
        public int WarningCount { get; set; }
        public double GoodRate { get; set; }
        public double NgRate { get; set; }
        public TimeSpan SessionDuration { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}