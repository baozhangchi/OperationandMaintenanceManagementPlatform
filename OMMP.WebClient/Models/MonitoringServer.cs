using System.ComponentModel.DataAnnotations;
using OMMP.Models;
using SqlSugar;

namespace OMMP.WebClient.Models;

public class MonitoringServer : TableBase
{
    [SugarColumn(CreateTableFieldSort = 1)]
    [Display(Name = "服务器地址", Order = 1), Required]
    public string IpAddress { get; set; }

    [SugarColumn(CreateTableFieldSort = 2)]
    [Display(Name = "登录用户名", Order = 1), Required]
    public string Username { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    [Display(Name = "登录密码", Order = 1), Required]
    public string Password { get; set; }

    [SugarColumn(CreateTableFieldSort = 4)]
    [Display(Name = "监控服务目录", Order = 1), Required]
    public string AppFolder { get; set; }
}