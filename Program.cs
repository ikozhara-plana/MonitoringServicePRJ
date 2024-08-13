
MonitoringService.Instance.StartMonitoring(TimeSpan.FromMinutes(10));

// To get the status
var statusData = MonitoringService.Instance.CheckStatus();
Console.WriteLine($"Errors from {statusData.PeriodStart} to {statusData.PeriodEnd}: {statusData.NumberOfErrors}");

// To stop monitoring
MonitoringService.Instance.StopMonitoring();