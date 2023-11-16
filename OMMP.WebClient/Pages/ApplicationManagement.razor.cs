using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components.Web;
using OMMP.Models;

namespace OMMP.WebClient.Pages;

public partial class ApplicationManagement
{
    public Modal ApplicationDetailModal { get; set; }
    public string ApplicationDetailModalTitle { get; set; }
    public IEnumerable<ApplicationInfo> Applications { get; set; }
    public ApplicationInfo CurrentApplication { get; set; }

    private async Task AddApplication()
    {
        ApplicationDetailModalTitle = "添加应用";
        CurrentApplication = new ApplicationInfo();
        await ApplicationDetailModal.Toggle();
    }

    private async Task<QueryData<ApplicationInfo>> QueryAsync(QueryPageOptions arg)
    {
        var url = @"/api/app";
        var data = await HttpHelper.GetAsync<List<ApplicationInfo>>(url);
        return new QueryData<ApplicationInfo>()
        {
            Items = data,
            TotalCount = data.Count
        };
    }

    private async Task<bool> SaveAsync()
    {
        return await Task.FromResult(true);
    }
}