namespace ConsoleApp1;

public class StatusData
{
    public string ServiceName { get; private set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int NumberOfErrors { get; set; }
    public string Status { get; set; }

    public StatusData SetServiceName(string serviceName)
    {
        ServiceName = serviceName;
        return this;
    }
}