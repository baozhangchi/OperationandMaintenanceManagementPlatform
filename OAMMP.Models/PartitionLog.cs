using SqlSugar;

namespace OAMMP.Models;

public class PartitionLog : LogTableBase
{
    [SugarColumn(CreateTableFieldSort = 2)]
    public string? Name { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    public long Total { get; set; }

    [SugarColumn(CreateTableFieldSort = 4)]
    public long Free { get; set; }

    [SugarColumn(CreateTableFieldSort = 5)]
    public long Used { get; set; }
}