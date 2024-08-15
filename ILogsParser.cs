namespace ConsoleApp1;

public interface ILogsParser
{
    Task<StatusData> ParseAsync(DateTime from, DateTime to, string logsPath);
}