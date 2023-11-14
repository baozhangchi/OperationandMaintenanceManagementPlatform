using SqlSugar;

namespace OMMP.Models;

/// <summary>
/// 网卡速度记录
/// </summary>
public class NetworkRateLog : LogTableBase
{
    [SugarColumn(CreateTableFieldSort = 2)]
    public string NetworkCard { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    public long Down { get; set; }

    [SugarColumn(CreateTableFieldSort = 4)]
    public long Up { get; set; }
}