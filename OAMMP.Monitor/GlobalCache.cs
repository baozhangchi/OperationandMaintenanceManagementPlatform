namespace OAMMP.Monitor;

internal class GlobalCache
{
    public static string DataFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    public static string DataSource => Path.Combine(DataFolder, "latest.db");
}