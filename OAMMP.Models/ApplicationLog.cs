using SqlSugar;

namespace OAMMP.Models;

public class ApplicationLog : LogTableBase
{
    [SugarColumn(CreateTableFieldSort = 2)]
    public long ApplicationId { get; set; }

    /// <summary>
    /// 线程数
    /// </summary>
    [SugarColumn(CreateTableFieldSort = 3)]
    public int ThreadCount { get; set; }

    /// <summary>
    /// 数据库连接池
    /// </summary>
    [SugarColumn(CreateTableFieldSort = 4, IsNullable = true)]
    public int DatabaseConnectionPool { get; set; }

    /// <summary>
    /// 平均响应时间
    /// </summary>
    [SugarColumn(CreateTableFieldSort = 5, IsNullable = true)]
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Cpu使用记录
    /// </summary>
    [SugarColumn(CreateTableFieldSort = 6)]
    public double CpuUsage { get; set; }

    /// <summary>
    /// 内存使用记录
    /// </summary>
    [SugarColumn(CreateTableFieldSort = 7)]
    public double MemoryUsage { get; set; }

    /// <summary>
    /// 网络下载速度
    /// </summary>
    [SugarColumn(CreateTableFieldSort = 8, IsNullable = true)]
    public int? NetworkDownRate { get; set; }

    /// <summary>
    /// 网络上传速度
    /// </summary>
    [SugarColumn(CreateTableFieldSort = 9, IsNullable = true)]
    public int? NetworkUpRate { get; set; }

    /// <summary>
    /// 磁盘读取速度
    /// </summary>
    [SugarColumn(CreateTableFieldSort = 10, ColumnName = "io_read_rate", IsNullable = true)]
    public int IOReadRate { get; set; }

    /// <summary>
    /// 磁盘写入速度
    /// </summary>
    [SugarColumn(CreateTableFieldSort = 11, ColumnName = "io_write_rate", IsNullable = true)]
    public int IOWriteRate { get; set; }

    [SugarColumn(CreateTableFieldSort = 12)]
    public bool IsLive { get; set; }
}