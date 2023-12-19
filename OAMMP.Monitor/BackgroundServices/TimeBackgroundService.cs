using System.Timers;
using Timer = System.Timers.Timer;

namespace OAMMP.Monitor.BackgroundServices;

public abstract class TimeBackgroundService<T> : BackgroundService
    where T : TimeBackgroundService<T>
{
    private readonly ILogger<T> _logger;
    private bool _isRunning;
    private Timer? _timer;

    protected TimeBackgroundService(ILogger<T> logger)
    {
        _logger = logger;
    }

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (_isRunning) await Task.Delay(100, stoppingToken);
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

    private async void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (sender is Timer timer)
        {
            timer.Stop();
            await ExecuteAsync(DateTime.Now).ContinueWith(task =>
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    _logger.LogError(task.Exception, task.Exception.Message);
                }
            });
            timer.Start();
        }
    }

    protected abstract Task ExecuteAsync(DateTime currentTime);
}