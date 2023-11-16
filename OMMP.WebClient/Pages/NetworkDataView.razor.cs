using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components.Web;
using OMMP.WebClient.Shared;

namespace OMMP.WebClient.Pages;

public partial class NetworkDataView
{
    private IMonitorComponent Monitor { get; set; }
    private DateTimeRangeValue MaxMinDateTimeRangeValue { get; set; }

    private async Task SearchButtonClick(MouseEventArgs arg)
    {
        await Monitor.Reload();
    }
}