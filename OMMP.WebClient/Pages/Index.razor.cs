using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;

namespace OMMP.WebClient.Pages;

public partial class Index
{
    [CascadingParameter(Name = "ClientId")] private string ClientId { get; set; }
    private bool RefreshDataSignaler { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    private void OnTimerElapsed()
    {
        RefreshDataSignaler = !RefreshDataSignaler;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        return true;
    }
}