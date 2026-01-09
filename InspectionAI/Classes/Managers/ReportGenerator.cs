using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using InspectionAI.Classes.Models;

namespace InspectionAI.Classes.Managers
{
    /// <summary>
    /// Report Generator untuk export production reports
    /// </summary>
    public class ReportGenerator
    {
        private DataLogger dataLogger;

        public ReportGenerator(DataLogger logger)
        {
            dataLogger = logger;
        }

        /// <summary>
        /// Generate daily report
        /// </summary>
        public bool GenerateDailyReport(DateTime date, string outputPath)
        {
            try
            {
                var stats = dataLogger.GetTodayStatistics();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("========================================");
                sb.AppendLine($"DAILY INSPECTION REPORT - {date:yyyy-MM-dd}");
                sb.AppendLine("========================================");
                sb.AppendLine();
                sb.AppendLine("SUMMARY:");
                sb.AppendLine($"  Total Inspections: {stats.TotalCount}");
                sb.AppendLine($"  GOOD: {stats.GoodCount} ({stats.GoodPercentage:F2}%)");
                sb.AppendLine($"  NG: {stats.NgCount} ({stats.NgPercentage:F2}%)");
                sb.AppendLine($"  WARNING: {stats.WarningCount}");
                sb.AppendLine($"  Average Time: {stats.AvgInspectionTimeMs:F2} ms");
                sb.AppendLine();

                // Get detailed results
                var results = dataLogger.GetRecentResults(1000);

                sb.AppendLine("DETAILED RESULTS:");
                sb.AppendLine("Time\t\tSet Number\tCamera\tResult\tTime(ms)");
                sb.AppendLine("----------------------------------------------------------------");

                foreach (DataRow row in results.Rows)
                {
                    sb.AppendLine($"{row["timestamp"]}\t{row["set_number"]}\t{row["camera_id"]}\t{row["result"]}\t{row["inspection_time_ms"]}");
                }

                File.WriteAllText(outputPath, sb.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generate CSV report
        /// </summary>
        public bool GenerateCSVReport(DateTime startDate, DateTime endDate, string outputPath)
        {
            try
            {
                var results = dataLogger.GetRecentResults(10000);

                StringBuilder sb = new StringBuilder();
                // Header
                sb.AppendLine("Timestamp,Set Number,Camera ID,Result,Defect Summary,Inspection Time (ms),Image Path");

                // Data rows
                foreach (DataRow row in results.Rows)
                {
                    sb.AppendLine($"{row["timestamp"]},{row["set_number"]},{row["camera_id"]},{row["result"]},{row["defect_summary"]},{row["inspection_time_ms"]},{row["image_path"]}");
                }

                File.WriteAllText(outputPath, sb.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generate HTML report dengan styling
        /// </summary>
        public bool GenerateHTMLReport(DateTime date, string outputPath)
        {
            try
            {
                var stats = dataLogger.GetTodayStatistics();
                var results = dataLogger.GetRecentResults(100);

                StringBuilder html = new StringBuilder();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html>");
                html.AppendLine("<head>");
                html.AppendLine("<meta charset='UTF-8'>");
                html.AppendLine($"<title>Inspection Report - {date:yyyy-MM-dd}</title>");
                html.AppendLine("<style>");
                html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
                html.AppendLine("h1 { color: #333; }");
                html.AppendLine(".summary { background: white; padding: 20px; border-radius: 8px; margin-bottom: 20px; }");
                html.AppendLine(".stat { display: inline-block; margin-right: 30px; }");
                html.AppendLine(".stat-label { font-weight: bold; color: #666; }");
                html.AppendLine(".stat-value { font-size: 24px; color: #0078d4; }");
                html.AppendLine("table { width: 100%; border-collapse: collapse; background: white; }");
                html.AppendLine("th { background: #0078d4; color: white; padding: 12px; text-align: left; }");
                html.AppendLine("td { padding: 10px; border-bottom: 1px solid #ddd; }");
                html.AppendLine("tr:hover { background: #f0f0f0; }");
                html.AppendLine(".good { color: green; font-weight: bold; }");
                html.AppendLine(".ng { color: red; font-weight: bold; }");
                html.AppendLine("</style>");
                html.AppendLine("</head>");
                html.AppendLine("<body>");

                html.AppendLine($"<h1>Inspection Report - {date:yyyy-MM-dd}</h1>");

                html.AppendLine("<div class='summary'>");
                html.AppendLine("<h2>Summary</h2>");
                html.AppendLine($"<div class='stat'><div class='stat-label'>Total</div><div class='stat-value'>{stats.TotalCount}</div></div>");
                html.AppendLine($"<div class='stat'><div class='stat-label'>Good</div><div class='stat-value' style='color: green;'>{stats.GoodCount}</div></div>");
                html.AppendLine($"<div class='stat'><div class='stat-label'>NG</div><div class='stat-value' style='color: red;'>{stats.NgCount}</div></div>");
                html.AppendLine($"<div class='stat'><div class='stat-label'>Pass Rate</div><div class='stat-value'>{stats.GoodPercentage:F1}%</div></div>");
                html.AppendLine("</div>");

                html.AppendLine("<h2>Inspection Results</h2>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Time</th><th>Set Number</th><th>Camera</th><th>Result</th><th>Time (ms)</th></tr>");

                foreach (DataRow row in results.Rows)
                {
                    string resultClass = row["result"].ToString() == "GOOD" ? "good" : "ng";
                    html.AppendLine($"<tr><td>{row["timestamp"]}</td><td>{row["set_number"]}</td><td>{row["camera_id"]}</td><td class='{resultClass}'>{row["result"]}</td><td>{row["inspection_time_ms"]}</td></tr>");
                }

                html.AppendLine("</table>");
                html.AppendLine("</body>");
                html.AppendLine("</html>");

                File.WriteAllText(outputPath, html.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}