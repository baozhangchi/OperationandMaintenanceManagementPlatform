using SqlSugar;

namespace OAMMP.Models;

public class MemoryLog : LogTableBase
{
    [SugarColumn(CreateTableFieldSort = 2)]
    public ulong Total { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    public ulong Used { get; set; }

    [SugarColumn(CreateTableFieldSort = 4)]
    public ulong Free { get; set; }
}