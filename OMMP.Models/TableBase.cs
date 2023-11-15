using SqlSugar;

namespace OMMP.Models;

public abstract class TableBase
{
    [SugarColumn(ColumnName = "uuid", IsPrimaryKey = true, IsOnlyIgnoreUpdate = true, CreateTableFieldSort = 0)]
    public string UUID { get; set; } = Guid.NewGuid().ToString("N");
}

public abstract class LogTableBase : TableBase
{
    [SugarColumn(CreateTableFieldSort = 1)]
    public DateTime Time { get; set; }
}