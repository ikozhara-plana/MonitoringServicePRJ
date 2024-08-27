using System.Text.RegularExpressions;

namespace ConsoleApp1;

public class SerilogLogsParser : ILogsParser
{
    private readonly IFilesProvider _filesProvider;

    public SerilogLogsParser(IFilesProvider filesProvider)
    {
        _filesProvider = filesProvider;
    }
    
    public async Task<StatusData> ParseAsync(DateTime from, DateTime to, string logsPath)
    {
        int errorCount = 0;
        
        bool allRecordsInRange = true;

        foreach (var file in _filesProvider.GetFiles(logsPath))
        {
            using (var streamReader = new StreamReader(new FileStream(file.FullName, FileMode.Open, FileAccess.Read)))
            {
                var lines = new List<string>();
                while (!streamReader.EndOfStream)
                {
                    lines.Add(await streamReader.ReadLineAsync());
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