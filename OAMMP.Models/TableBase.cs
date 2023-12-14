using SqlSugar;

namespace OAMMP.Models;

public abstract class TableBase
{
    [SugarColumn(ColumnName = "uuid", IsPrimaryKey = true, IsOnlyIgnoreUpdate = true, CreateTableFieldSort = 0)]
    public long UUID { get; set; }
}

public abstract class LogTableBase : TableBase
{
    [SugarColumn(CreateTableFieldSort = 1)]
    public DateTime Time { get; set; }
}