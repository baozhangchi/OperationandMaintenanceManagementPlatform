using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace OMMP.Models;

/// <summary>
/// 应用信息
/// </summary>
public class ApplicationInfo : TableBase
{
    [SugarColumn(CreateTableFieldSort = 1)]
    [Display(Name = "应用名称"), Required]
    public string Name { get; set; }

    [SugarColumn(CreateTableFieldSort = 2)]
    [Display(Name = "应用启动目录"), Required]
    public string AppFolder { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    [Display(Name = "应用执行文件"), Required]
    public string AppFileName { get; set; }

    [SugarColumn(CreateTableFieldSort = 4, IsNullable = true)]
    [Display(Name = "应用启动参数")]
    public string Argument { get; set; }

    [SugarColumn(CreateTableFieldSort = 5)]
    [Display(Name = "是否自动重启")]
    public bool AutoRestart { get; set; }
    
    [SugarColumn(CreateTableFieldSort = 6)]
    [Display(Name = "自动重启时间")]
    public TimeOnly? AutoRestartTime { get; set; }
}