using System.Timers;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace OAMMP.Server.Shared.LogDataComponents;

public abstract class LogViewBase : ComponentBase, IDisposable
{
	protected Timer? _timer;

	protected DateTime? LastTime;

	[Parameter] public DateTime? EndTime { get; set; }

	[Parameter] public DateTime? StartTime { get; set; }

	public void Dispose()
	{
		_timer?.Dispose();
	}

	protected override Task OnInitializedAsync()
	{
		_timer = new Timer(5 * 1000);
		_timer.Elapsed += Timer_Elapsed;
		ReloadData();
		StartAutoRefreshData();
		return base.OnInitializedAsync();
	}

	public abstract Task RefreshData();

	public abstract Task ReloadData();

	public void StartAutoRefreshData()
	{
		_timer?.Stop();
		_timer?.Start();
	}

	public void StopAutoRefreshData()
	{
		_timer?.Stop();
	}

	private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
	{
		if (sender is Timer timer)
		{
			timer.Stop();
			RefreshData();
			timer.Start();
		}
	}
}