using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace OMMP.WebClient.Shared;

public interface IMonitorComponent
{
    bool AutoRefresh { get; set; }
    DateTimeRangeValue MaxMinDateTimeRangeValue { get; set; }

    Task Reload();
}