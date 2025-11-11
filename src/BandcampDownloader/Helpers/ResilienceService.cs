using System;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.Settings;

namespace BandcampDownloader.Helpers;

internal interface IResilienceService
{
    /// <summary>
    /// Waits for a "cooldown" time, computed from the specified number of download tries.
    /// </summary>
    /// <param name="triesNumber">The times count we tried to download the same file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task WaitForCooldownAsync(int triesNumber, CancellationToken cancellationToken);
}

internal sealed class ResilienceService : IResilienceService
{
    private readonly IUserSettings _userSettings;

    public ResilienceService(ISettingsService settingsService)
    {
        _userSettings = settingsService.GetUserSettings();
    }

    public async Task WaitForCooldownAsync(int triesNumber, CancellationToken cancellationToken)
    {
        if (_userSettings.DownloadRetryCooldown != 0)
        {
            var cooldownDelay = (int)(Math.Pow(_userSettings.DownloadRetryExponent, triesNumber) * _userSettings.DownloadRetryCooldown * 1000);
            await Task.Delay(cooldownDelay, cancellationToken);
        }
    }
}
