namespace ConsoleApp1;

public class LogsMonitoring : IDisposable
{
    private readonly ServiceToMonitor[] _servicesToMonitor;

    private Timer? _timer;
    private bool _isMonitoring;
    private TimeSpan _period;
    private readonly object _lock = new();
    private Action<StatusData[]>? _callback;

    private LogsMonitoring(ServiceToMonitor[] servicesToMonitor, TimeSpan period)
    {
        _servicesToMonitor = servicesToMonitor;
        _period = period;
    }

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

    public async Task<StatusData[]> CheckStatus()
    {
        // Define time period for this monitoring session
        var now = DateTime.UtcNow;
        var from = now - _period;
        var to = now;

        // Re-read logs for the latest period
        /*var latestStatusData = _servicesToMonitor
            .Select(i => i.LogsParser.ParseAsync(from, to, i.LogsPath)
                .SetServiceName(i.ServiceName));*/
        var latestStatusData = await Task.WhenAll(_servicesToMonitor
            .Select(async i =>
            {
                var statusData = await i.LogsParser.ParseAsync(from, to, i.LogsPath);
                return statusData.SetServiceName(i.ServiceName);
            }));

        return latestStatusData.ToArray();
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

    private async void MonitorLogs(object? state)
    {
        if (!_isMonitoring) return;

        // Define time period for this monitoring session
        var now = DateTime.UtcNow;
        var from = now - _period;
        var to = now;

        // Scan log files
        /*var statusData = _servicesToMonitor
            .Select(i => i.LogsParser.ParseAsync(from, to, i.LogsPath)
                .SetServiceName(i.ServiceName));*/
        var statusData = await Task.WhenAll(_servicesToMonitor
            .Select(async i =>
            {
                var statusData = await i.LogsParser.ParseAsync(from, to, i.LogsPath);
                return statusData.SetServiceName(i.ServiceName);
            }));

        // Invoke the callback with the current period's status data for single service
        _callback?.Invoke(statusData.ToArray());
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}