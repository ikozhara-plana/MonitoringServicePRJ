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
        var relevantLines = new List<string>();
        bool allRecordsInRange = true;

        foreach (var file in _filesProvider.GetFiles(logsPath))
        {
            using (var streamReader = new StreamReader(new FileStream(file.FullName, FileMode.Open, FileAccess.Read)))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = await streamReader.ReadLineAsync();
                    var timestamp = ExtractTimestamp(line);

                    if (timestamp.HasValue)
                    {
                        if (timestamp.Value >= from && timestamp.Value <= to)
                        {
                            if (line.Contains("[ERR]"))
                            {
                                relevantLines.Add(line);
                            }
                        }
                        else if (timestamp.Value < from)
                        {
                            // If the timestamp is less than the 'from' time, we can stop reading further
                            break;
                        }
                    }
                    else
                    {
                        // If the line does not meet the timestamp criteria, set allRecordsInRange to false
                        allRecordsInRange = false;
                    }
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
            NumberOfErrors = relevantLines.Count,
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