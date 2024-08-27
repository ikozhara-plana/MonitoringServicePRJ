namespace ConsoleApp1;

public class SensorManagerFilesProvider : IFilesProvider
{
    public IOrderedEnumerable<FileInfo> GetFiles(string path)
    {
        return new DirectoryInfo(path)
            .GetFiles("smgr-log-b*.txt")
            .OrderByDescending(f => f.CreationTimeUtc);
    }
}