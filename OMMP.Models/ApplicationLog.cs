using SqlSugar;

namespace OMMP.Models;

public class ApplicationLog : LogTableBase
{
    [SugarColumn(CreateTableFieldSort = 2)]
    public long ApplicationId { get; set; }
    
    [SugarColumn(CreateTableFieldSort = 3)]
    public int ThreadCount { get; set; }

    [SugarColumn(CreateTableFieldSort = 4, IsNullable = true)]
    public int DatabaseConnectionPool { get; set; }

    [SugarColumn(CreateTableFieldSort = 5, IsNullable = true)]
    public double AverageResponseTime { get; set; }

    [SugarColumn(CreateTableFieldSort = 6)]
    public double CpuUsed { get; set; }

    [SugarColumn(CreateTableFieldSort = 7)]
    public double MemoryUsed { get; set; }

    [SugarColumn(CreateTableFieldSort = 8, IsNullable = true)]
    public int NetworkDownRate { get; set; }

    [SugarColumn(CreateTableFieldSort = 9, IsNullable = true)]
    public int NetworkUpRate { get; set; }

    [SugarColumn(CreateTableFieldSort = 10, ColumnName = "io_read_rate", IsNullable = true)]
    public int IOReadRate { get; set; }

    [SugarColumn(CreateTableFieldSort = 11, ColumnName = "io_write_rate", IsNullable = true)]
    public int IOWriteRate { get; set; }

    [SugarColumn(ColumnName = "jvm_memory_footprint", CreateTableFieldSort = 12, IsNullable = true)]
    public long JVMMemoryFootprint { get; set; }
}