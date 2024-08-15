namespace ConsoleApp1;

public record ServiceToMonitor(string LogsPath, string ServiceName, ILogsParser LogsParser);