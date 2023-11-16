using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using OMMP.Models;
using OMMP.WebClient.Shared;

namespace OMMP.WebClient.Pages;

public partial class MemoryDataView
{
    private IMonitorComponent Monitor { get; set; }
    private DateTimeRangeValue MaxMinDateTimeRangeValue { get; set; }

    private async Task SearchButtonClick(MouseEventArgs arg)
    {
        await Monitor.Reload();
    }
}