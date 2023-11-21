using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace OMMP.WebClient.Pages;

public partial class ApplicationDataView
{
    [Parameter]
    public long UUID { get; set; }
}