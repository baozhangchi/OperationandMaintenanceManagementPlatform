namespace OMMP.MonitoringService;

internal static class GlobalCache
{
    static GlobalCache()
    {
    }

    public static string DataFolder { get; set; }

    public static string DataSource => Path.Combine(DataFolder, $"latest.db");
}