using System.IO.Compression;

namespace OAMMP.Monitor;

public class CompressionHelper
{
    public static void CreateFromDirectory(string startPath, string zipPath)
    {
        ZipFile.CreateFromDirectory(startPath, zipPath);
    }

    public static void ExtractToDirectory(string zipPath, string extractPath)
    {
        ZipFile.ExtractToDirectory(zipPath, extractPath);
    }
}