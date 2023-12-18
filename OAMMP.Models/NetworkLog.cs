using SqlSugar;

namespace OAMMP.Models;

public class NetworkLog : LogTableBase
{
    [SugarColumn(CreateTableFieldSort = 2)]
    public string? Mac { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    public string? IpAddress { get; set; }

    [SugarColumn(CreateTableFieldSort = 4)]
    public long Down { get; set; }

    [SugarColumn(CreateTableFieldSort = 5)]
    public long Up { get; set; }
}