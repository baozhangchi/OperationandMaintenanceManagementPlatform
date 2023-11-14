using SqlSugar;

namespace OMMP.Models;

public class CpuLog : LogTableBase
{
    [SugarColumn(CreateTableFieldSort = 2)]
    public double Used { get; set; }
}