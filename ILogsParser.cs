namespace ConsoleApp1;

public interface ILogsParser
{
    StatusData Parse(DateTime from, DateTime to, string logsPath);
}