using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Timer = System.Timers.Timer;

namespace OAMMP.Server.Shared.LogDataComponents;

public abstract class LogViewBase : ComponentBase, IDisposable
{
	protected Timer? _timer;

	protected DateTime? LastTime;

	[CascadingParameter(Name = "ConnectionId")]
	public string? ConnectionId { get; set; }

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
		if (string.IsNullOrEmpty(ConnectionId))
		{
			_timer.Start();
		}

		return base.OnInitializedAsync();
	}

	public abstract Task RefreshData();

	public abstract Task ReloadData();

	public override async Task SetParametersAsync(ParameterView parameters)
	{
		await base.SetParametersAsync(parameters);
		foreach (var parameter in parameters)
		{
			switch (parameter.Name)
			{
				case nameof(ConnectionId):
					{
						if (_timer != null)
						{
							StartTime = null;
							EndTime = null;
							LastTime = null;
							await ReloadData();
						}

						break;
					}
			}
		}
	}

	public void StartAutoRefreshData()
	{
		_timer?.Stop();
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