using System.Text.RegularExpressions;

namespace ConsoleApp1;

public class SensorManagerLogsParser : ILogsParser
{
    public async Task<StatusData> ParseAsync(DateTime from, DateTime to, string logsPath)
    {
        int errorCount = 0;

        // Get all log files matching the pattern, ordered by their creation date descending
        var logFiles = new DirectoryInfo(logsPath)
            .GetFiles("smgr-log-b*.txt")
            .OrderByDescending(f => f.CreationTimeUtc)
            .ToList();

        bool allRecordsInRange = true;

        foreach (var file in logFiles)
        {
            using (var sr = new StreamReader(new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var lines = new List<string>();
                while (!sr.EndOfStream)
                {
                    lines.Add(await sr.ReadLineAsync());
                }

                var relevantLines = lines
                    .Reverse<string>()
                    .Select(line => new { line, timestamp = ExtractTimestamp(line) })
                    .TakeWhile(entry => entry.timestamp.HasValue && entry.timestamp.Value >= from && entry.timestamp.Value <= to)
                    .Where(entry => entry.line.Contains("[ERR]"));

                foreach (var entry in relevantLines)
                {
                    errorCount++;
                }

                if (!relevantLines.Any())
                {
                    allRecordsInRange = false;
                }
            }

            if (!allRecordsInRange)
            {
                break;
            }
        }

        return new StatusData
        {
            To = to,
            From = from,
            NumberOfErrors = errorCount,
            Status = "OK"
        };
    }

    private DateTime? ExtractTimestamp(string logLine)
    {
        var match = Regex.Match(logLine, @"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}Z)");
        if (match.Success && DateTime.TryParse(match.Groups["timestamp"].Value, out var timestamp))
        {
            return timestamp;
        }

        return null;
    }
}