using BootstrapBlazor.Components;

namespace OMMP.WebClient.Shared;

public partial class NavMenu
{
    private IEnumerable<MenuItem> MenuItems { get; set; }
    protected override void OnInitialized()
    {
        base.OnInitialized();
        var items = new List<MenuItem>();
        items.Add(new MenuItem("首页","/","fa-solid fa-house"));
        items.Add(new MenuItem("CPU","/cpu","fa-solid fa-microchip"));
        items.Add(new MenuItem("内存","/memory","fa-solid fa-memory"));
        items.Add(new MenuItem("网络","/networks","fa-solid fa-network-wired"));
        items.Add(new MenuItem("应用管理","/applications","fa-solid fa-grip"));
        MenuItems = items;
    }
}