using MorsadBackend.Infrastructure.Services;

public class MorsadWorkerHosted : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<MorsadWorkerHosted> _log;

    public MorsadWorkerHosted(IServiceProvider sp, ILogger<MorsadWorkerHosted> log)
    {
        _sp  = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("مرصاد Worker بدأ — دورة كل 6 ساعات");
        await RunCycleAsync();
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));
        while (await timer.WaitForNextTickAsync(ct))
            await RunCycleAsync();
    }

    private async Task RunCycleAsync()
    {
        using var scope = _sp.CreateScope();
        try
        {
            _log.LogInformation("═══ مرصاد: بدء دورة الرصد {T}", DateTime.Now.ToString("HH:mm"));
            await scope.ServiceProvider.GetRequiredService<RssFetcherService>().FetchAllAsync();
            await scope.ServiceProvider.GetRequiredService<ClaudeAnalyzerService>().AnalyzeUnprocessedAsync();
            await scope.ServiceProvider.GetRequiredService<ClaudeAnalyzerService>().AssessRisksAsync();
            _log.LogInformation("═══ مرصاد: اكتملت الدورة");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "خطأ في دورة مرصاد");
        }
    }
}
