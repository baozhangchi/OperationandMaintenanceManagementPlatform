using SqlSugar;

namespace OMMP.Models;

/// <summary>
/// 网卡信息
/// </summary>
public class NetworkLog : LogTableBase
{
    [SugarColumn(CreateTableFieldSort = 2)]
    public string NetworkCardName { get; set; }

    [SugarColumn(CreateTableFieldSort = 3)]
    public string IpAddress { get; set; }
}