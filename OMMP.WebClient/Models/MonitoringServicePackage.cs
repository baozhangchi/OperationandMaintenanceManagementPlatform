using System.ComponentModel.DataAnnotations;
using OMMP.Models;
using SqlSugar;

namespace OMMP.WebClient.Models;

public class MonitoringServicePackage : TableBase
{
    [SugarColumn(CreateTableFieldSort = 1)]
    [Display(Name = "文件名", Order = 1), Required]
    public string FileName { get; set; }
    
    [SugarColumn(CreateTableFieldSort = 1)]
    [Display(Name = "原始文件名", Order = 2), Required]
    public string OriginFileName { get; set; }
    
    [SugarColumn(CreateTableFieldSort = 1)]
    [Display(Name = "版本", Order = 3), Required]
    public string Version { get; set; }
    
    [SugarColumn(CreateTableFieldSort = 1)]
    [Display(Name = "文件大小", Order = 4), Required]
    public long FileSize { get; set; }
    
    [SugarColumn(CreateTableFieldSort = 1)]
    [Display(Name = "上传时间", Order = 5), Required]
    public DateTime UploadTime { get; set; }
}