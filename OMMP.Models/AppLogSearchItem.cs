using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OMMP.Models;

public class AppLogSearchItem
{
    [Display(Name = "开始时间", Order = 1), Required]
    public DateTime StartTime { get; set; }

    [Display(Name = "结束时间", Order = 2), Required]
    public DateTime EndTime { get; set; }
}