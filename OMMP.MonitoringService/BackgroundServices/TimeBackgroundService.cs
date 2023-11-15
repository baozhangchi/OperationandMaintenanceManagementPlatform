using System.Timers;
using Timer = System.Timers.Timer;

namespace OMMP.MonitoringService.BackgroundServices;

public abstract class TimeBackgroundService : BackgroundService
{
    private Timer _timer;
    private bool _isRunning;

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (_isRunning)
        {
            await Task.Delay(100, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _isRunning = false;
        if (_timer is { Enabled: true })
        {
            _timer.Stop();
            _timer.Dispose();
        }

        return base.StopAsync(cancellationToken);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _isRunning = true;
        _timer = new Timer();
        _timer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
        _timer.Elapsed += TimerElapsed;
        _timer.Start();
        return base.StartAsync(cancellationToken);
    }

    private async void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (sender is Timer timer)
        {
            timer.Stop();
            CurrentTime = DateTime.Now;
            await ExecuteAsync();
            timer.Start();
        }
    }

    protected DateTime CurrentTime { get; private set; }

    protected abstract Task ExecuteAsync();
}