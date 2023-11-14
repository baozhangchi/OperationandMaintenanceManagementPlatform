using SqlSugar;

namespace OMMP.Models;

/// <summary>
/// 应用信息
/// </summary>
public class ApplicationInfo : TableBase
{
    [SugarColumn(CreateTableFieldSort = 1)]
    public string Name { get; set; }

    [SugarColumn(CreateTableFieldSort = 2)]
    public string AppFolder { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    public string AppFileName { get; set; }

    [SugarColumn(CreateTableFieldSort = 4)]
    public string Argument { get; set; }
    
    public bool AutoRestart { get; set; }
    
    
}