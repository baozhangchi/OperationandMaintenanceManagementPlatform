using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using OMMP.Common;
using OMMP.Models;
using OMMP.WebClient.Hubs;
using OMMP.WebClient.Shared;

namespace OMMP.WebClient.Pages;

public partial class ApplicationManagement
{
    [CascadingParameter(Name = "ClientId")]
    private string ClientId { get; set; }

    [Inject] [NotNull] private DialogService? DialogService { get; set; }
    [Inject] [NotNull] private IHubContext<MonitoringHub> HubContext { get; set; }

    [Inject] private NavigationManager Navigation { get; set; }
    public Modal ApplicationDetailModal { get; set; }
    public string ApplicationDetailModalTitle { get; set; }
    public ApplicationInfo CurrentApplication { get; set; }
    public Table<ApplicationInfo> ApplicationsView { get; set; }
    public ValidateForm CurrentApplicationValidateForm { get; set; }

    private async Task AddApplication()
    {
        ApplicationDetailModalTitle = "添加应用";
        CurrentApplication = new ApplicationInfo();
        await ApplicationDetailModal.Toggle();
    }


    private async void EditApplication(ApplicationInfo application)
    {
        ApplicationDetailModalTitle = "修改应用";
        CurrentApplication = application;
        await ApplicationDetailModal.Toggle();
    }


    private async Task DeleteApplication(ApplicationInfo application)
    {
        var result = await HttpHelper.DeleteAsync<bool>($@"/api/app/{application.UUID}");
        if (result)
        {
            await ApplicationsView.QueryAsync();
        }
    }

    private async Task UploadApplication(UploadFile file)
    {
        var application = ApplicationsView.SelectedRows.SingleOrDefault();
        if (application == null)
        {
            return;
        }


        await Task.CompletedTask;
    }

    private async Task<QueryData<ApplicationInfo>> QueryAsync(QueryPageOptions arg)
    {
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            return new QueryData<ApplicationInfo>();
        }

        var data = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<List<ApplicationInfo>>(nameof(IMonitoringClientHub.GetApplications), CancellationToken.None);
        return new QueryData<ApplicationInfo>()
        {
            Items = data,
            TotalCount = data.Count
        };
    }

    private async Task<bool> SaveApplicationAsync(ApplicationInfo application, ItemChangedType changedType)
    {
        // return await State.SaveApplication(application);
        return await HubContext.Clients.Client(ClientId)
            .InvokeAsync<bool>(nameof(IMonitoringClientHub.SaveApplication), application, CancellationToken.None);
    }

    private async Task<bool> DeleteApplicationAsync(IEnumerable<ApplicationInfo> applications)
    {
        // var result = await HttpHelper.DeleteAsync<bool>($@"/api/app/{applications.First().UUID}");
        // if (result)
        // {
        //     return true;
        // }

        return false;
    }

    public List<ApplicationInfo> OpenedApplications { get; set; } = new List<ApplicationInfo>();
    public Tab RootTab { get; set; }

    private Task OpenApplicationPage(ApplicationInfo application)
    {
        // Navigation.NavigateTo($@"/applications/app/{application.UUID}");
        var tabItem = RootTab.Items.FirstOrDefault(x => x.Text == application.Name);
        if (tabItem != null)
        {
            RootTab.ActiveTab(tabItem);
        }
        else
        {
            var component = BootstrapDynamicComponent.CreateComponent<ApplicationMonitor>(
                new Dictionary<string, object>()
                {
                    [nameof(ApplicationMonitor.Application)] = application
                });
            RootTab.AddTab(new Dictionary<string, object>()
            {
                [nameof(TabItem.Text)] = application.Name,
                [nameof(TabItem.IsActive)] = true,
                [nameof(TabItem.ChildContent)] = component.Render()
            });
        }

        return Task.CompletedTask;
    }

    private async Task<bool> CloseTabItemAsync(TabItem arg)
    {
        return await Task.FromResult(true);
    }
}