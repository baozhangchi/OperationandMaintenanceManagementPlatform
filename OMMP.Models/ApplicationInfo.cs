using System.ComponentModel.DataAnnotations;
using System.Data;
using Newtonsoft.Json;
using SqlSugar;
using SqlSugar.DbConvert;

namespace OMMP.Models;

/// <summary>
/// 应用信息
/// </summary>
public class ApplicationInfo : TableBase
{
    [SugarColumn(CreateTableFieldSort = 1)]
    [Display(Name = "应用名称", Order = 1), Required]
    public string Name { get; set; }

    [SugarColumn(CreateTableFieldSort = 2)]
    [Display(Name = "应用启动目录", Order = 2), Required]
    public string AppFolder { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    [Display(Name = "应用执行文件", Order = 3), Required]
    public string AppFileName { get; set; }

    [SugarColumn(CreateTableFieldSort = 4, IsNullable = true)]
    [Display(Name = "应用启动参数", Order = 4)]
    public string Argument { get; set; }

    [SugarColumn(CreateTableFieldSort = 5)]
    [Display(Name = "是否自动重启", Order = 5)]
    public bool AutoRestart { get; set; }

    [SugarColumn(IsIgnore = true)] public bool CanSetAutoRestartTime => !AutoRestart;

    [SugarColumn(CreateTableFieldSort = 6, IsNullable = true)]
    [JsonIgnore]
    public string AutoRestartTime
    {
        get => AutoRestartTimeValue.ToString();
        set => AutoRestartTimeValue = string.IsNullOrWhiteSpace(value) ? null : TimeOnly.Parse(value);
    }

    [SugarColumn(IsIgnore = true, IsNullable = true)]
    [Display(Name = "自动重启时间", Order = 6)]
    public TimeOnly? AutoRestartTimeValue { get; set; }
}