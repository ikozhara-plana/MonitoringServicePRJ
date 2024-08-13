
var service = MonitoringService.Instance;
service.StartMonitoring(TimeSpan.FromMinutes(10), statusDataArray =>
{
    foreach (var statusData in statusDataArray)
    {
        Console.WriteLine($"Period: {statusData.PeriodStart} - {statusData.PeriodEnd}, Errors: {statusData.NumberOfErrors}");
    }
});