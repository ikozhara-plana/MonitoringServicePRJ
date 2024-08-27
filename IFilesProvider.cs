namespace ConsoleApp1;

public interface IFilesProvider
{
    IOrderedEnumerable<FileInfo> GetFiles(string path);
}