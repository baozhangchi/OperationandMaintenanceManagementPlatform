using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OMMP.Models;
using OMMP.WebClient.Shared;

namespace OMMP.WebClient.Pages;

public partial class ApplicationManagement
{
    [Inject] [NotNull] private DialogService? DialogService { get; set; }

    [Inject] private NavigationManager Navigation { get; set; }
    public Modal ApplicationDetailModal { get; set; }
    public string ApplicationDetailModalTitle { get; set; }
    public IEnumerable<ApplicationInfo> Applications { get; set; }
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
        if (CurrentApplicationValidateForm.Validate())
        {
            var result = await HttpHelper.PostAsync<bool>("/api/app", CurrentApplication);
            if (result)
            {
                await ApplicationsView.QueryAsync();
                return await Task.FromResult(true);
            }
        }

        return await Task.FromResult(false);
    }

    private async Task<bool> SaveApplicationAsync(ApplicationInfo application, ItemChangedType changedType)
    {
        var result = await HttpHelper.PostAsync<bool>("/api/app", application);
        if (result)
        {
            return true;
        }

        return false;
    }

    private async Task<bool> DeleteApplicationAsync(IEnumerable<ApplicationInfo> applications)
    {
        var result = await HttpHelper.DeleteAsync<bool>($@"/api/app/{applications.First().UUID}");
        if (result)
        {
            return true;
        }

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