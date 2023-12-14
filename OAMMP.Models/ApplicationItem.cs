using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SqlSugar;

namespace OAMMP.Models;

public class ApplicationItem : TableBase
{
    private string? _autoRestartTime;
    private TimeOnly? _autoRestartTimeValue;

    [SugarColumn(CreateTableFieldSort = 4)]
    [Display(Name = "应用执行文件", Order = 4)]
    [Required(ErrorMessage = "应用执行文件不能为空")]
    
    public string? AppFileName { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    [Display(Name = "应用启动目录", Order = 3)]
    [Required(ErrorMessage = "应用启动目录不能为空")]
    public string? AppFolder { get; set; }

    [SugarColumn(CreateTableFieldSort = 5, IsNullable = true)]
    [Display(Name = "应用启动参数", Order = 5)]
    public string? Argument { get; set; }

    [SugarColumn(CreateTableFieldSort = 6)]
    [Display(Name = "是否自动重启", Order = 6)]
    public bool AutoRestart { get; set; }

    [SugarColumn(CreateTableFieldSort = 7, IsNullable = true)]
    [JsonIgnore]
    public string? AutoRestartTime
    {
        get => _autoRestartTime;
        set
        {
            _autoRestartTime = value;
            AutoRestartTimeValue = string.IsNullOrWhiteSpace(value) ? null : TimeOnly.Parse(value);
        }
    }
    //{
    //    get => AutoRestartTimeValue.ToString();
    //    set => AutoRestartTimeValue = string.IsNullOrWhiteSpace(value) ? null : TimeOnly.Parse(value);
    //}

    [SugarColumn(IsIgnore = true, IsNullable = true)]
    [Display(Name = "自动重启时间", Order = 7)]
    public TimeOnly? AutoRestartTimeValue
    {
        get => _autoRestartTimeValue;
        set
        {
            _autoRestartTimeValue = value;
            _autoRestartTime = value?.ToString();
        }
    }

    [SugarColumn(IsIgnore = true)] public bool CanSetAutoRestartTime => !AutoRestart;

    [SugarColumn(CreateTableFieldSort = 1)]
    [Display(Name = "应用名称", Order = 1)]
    [Required(ErrorMessage = "应用名称不能为空")]
    public string? Name { get; set; }
    
    [SugarColumn(CreateTableFieldSort = 2)]
    [Display(Name = "应用地址", Order = 2)]
    [Required(ErrorMessage = "应用地址不能为空")]
    public string? AppUrl { get; set; }
}