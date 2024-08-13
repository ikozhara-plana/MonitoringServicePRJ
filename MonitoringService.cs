using System.Text.RegularExpressions;

public class MonitoringService : IDisposable
{
    private static readonly Lazy<MonitoringService> _instance = 
        new Lazy<MonitoringService>(() => new MonitoringService());
    
    public static MonitoringService Instance => _instance.Value;

    private Timer? _timer;
    private bool _isMonitoring;
    private readonly string _logFolderPath = @"E:\Projects\RiderProject\Logs";
    private TimeSpan _period;
    private readonly object _lock = new object();
    private Action<StatusData[]>? _callback;

    private MonitoringService() { }

    public MonitoringStatus StartMonitoring(TimeSpan period, Action<StatusData[]> callback)
    {
        lock (_lock)
        {
            if (_isMonitoring)
                throw new InvalidOperationException("Monitoring is already running.");

            _period = period;
            _callback = callback;
            _isMonitoring = true;
            _timer = new Timer(MonitorLogs, null, TimeSpan.Zero, _period);
        }

        return MonitoringStatus.Running;
    }

    public StatusData CheckStatus()
    {
        lock (_lock)
        {
            if (!_isMonitoring)
                throw new InvalidOperationException("Monitoring is not running.");

            // Calculate the latest period
            var periodEnd = DateTime.UtcNow;
            var periodStart = periodEnd - _period;

            // Re-read logs for the latest period
            var latestStatusData = ParseLogFiles(periodStart, periodEnd);

            return latestStatusData;
        }
    }

    public MonitoringStatus StopMonitoring()
    {
        lock (_lock)
        {
            if (!_isMonitoring)
                throw new InvalidOperationException("Monitoring is not running.");

            _timer?.Dispose();
            _isMonitoring = false;
        }

        return MonitoringStatus.Stopped;
    }

    private void MonitorLogs(object? state)
    {
        lock (_lock)
        {
            if (!_isMonitoring) return;

            // Define time period for this monitoring session
            var periodStart = DateTime.UtcNow - _period;
            var periodEnd = DateTime.UtcNow;

            // Scan log files
            var statusData = ParseLogFiles(periodStart, periodEnd);

            // Invoke the callback with the current period's status data for single service
            _callback?.Invoke(new []{statusData});
        }
    }

    private StatusData ParseLogFiles(DateTime periodStart, DateTime periodEnd)
    {
        int errorCount = 0;

        foreach (var file in Directory.GetFiles(_logFolderPath, "smgr-log-b*.txt"))
        {
            try
            {
                var lines = File.ReadAllLines(file);

                foreach (var line in lines)
                {
                    var logTimestamp = ExtractTimestamp(line);
                    if (logTimestamp.HasValue && logTimestamp.Value >= periodStart && logTimestamp.Value <= periodEnd)
                    {
                        if (line.Contains("[ERR]"))
                        {
                            errorCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle file read exceptions
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
            }
        }

        return new StatusData
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            NumberOfErrors = errorCount
        };
    }

    private DateTime? ExtractTimestamp(string logLine)
    {
        // Extract the timestamp from the log line using regex
        var match = Regex.Match(logLine, @"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}Z)");
        if (match.Success && DateTime.TryParse(match.Groups["timestamp"].Value, out var timestamp))
        {
            return timestamp;
        }

        return null;
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}
public class StatusData
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int NumberOfErrors { get; set; }
}
public enum MonitoringStatus
{
    Running,
    Stopped
}