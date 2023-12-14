using SqlSugar;

namespace OAMMP.Models;

public class ServerResourceLog : LogTableBase
{
    [SugarColumn(CreateTableFieldSort = 2)]
    public int ProcessCount { get; set; }
}