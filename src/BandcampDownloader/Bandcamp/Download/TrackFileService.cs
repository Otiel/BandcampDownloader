using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.Helpers;
using BandcampDownloader.Model;
using BandcampDownloader.Net;
using BandcampDownloader.Settings;

namespace BandcampDownloader.Bandcamp.Download;

internal interface ITrackFileService
{
    Task<IReadOnlyList<TrackFile>> GetFilesToDownloadAsync(IReadOnlyList<Album> albums, CancellationToken cancellationToken);
    event DownloadProgressChangedEventHandler DownloadProgressChanged;
}

internal sealed class TrackFileService : ITrackFileService
{
    private readonly IHttpService _httpService;
    private readonly IResilienceService _resilienceService;
    private readonly IUserSettings _userSettings;
    public event DownloadProgressChangedEventHandler DownloadProgressChanged;

    public TrackFileService(IHttpService httpService, IResilienceService resilienceService, ISettingsService settingsService)
    {
        _httpService = httpService;
        _resilienceService = resilienceService;
        _userSettings = settingsService.GetUserSettings();
    }

    public async Task<IReadOnlyList<TrackFile>> GetFilesToDownloadAsync(IReadOnlyList<Album> albums, CancellationToken cancellationToken)
    {
        var files = new List<TrackFile>();
        foreach (var album in albums)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Computing size for album \"{album.Title}\"...", DownloadProgressChangedLevel.Info));

            // Artwork
            if ((_userSettings.SaveCoverArtInTags || _userSettings.SaveCoverArtInFolder) && album.HasArtwork)
            {
                if (_userSettings.RetrieveFilesSize)
                {
                    var size = await GetFileSizeAsync(album.ArtworkUrl, album.Title, FileType.Artwork, cancellationToken);
                    files.Add(new TrackFile(album.ArtworkUrl, 0, size));
                }
                else
                {
                    files.Add(new TrackFile(album.ArtworkUrl, 0, 0));
                }
            }

            // Tracks
            if (_userSettings.RetrieveFilesSize)
            {
                var tracksIndexes = Enumerable.Range(0, album.Tracks.Count).ToArray();
                await Task.WhenAll(tracksIndexes.Select(async i =>
                {
                    var size = await GetFileSizeAsync(album.Tracks[i].Mp3Url, album.Tracks[i].Title, FileType.Track, cancellationToken);
                    files.Add(new TrackFile(album.Tracks[i].Mp3Url, 0, size));
                }));
            }
            else
            {
                foreach (var track in album.Tracks)
                {
                    files.Add(new TrackFile(track.Mp3Url, 0, 0));
                }
            }
        }

        return files;
    }

    private async Task<long> GetFileSizeAsync(string url, string titleForLog, FileType fileType, CancellationToken cancellationToken)
    {
        long size = 0;
        bool sizeRetrieved;
        var tries = 0;

        var fileTypeForLog = fileType switch
        {
            FileType.Artwork => "cover art file for album",
            FileType.Track => "MP3 file for the track",
            _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null),
        };

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                size = await _httpService.GetFileSizeAsync(url, cancellationToken);
                sizeRetrieved = true;
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Retrieved the size of the {fileTypeForLog} \"{titleForLog}\"", DownloadProgressChangedLevel.VerboseInfo));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                sizeRetrieved = false;
                if (tries + 1 < _userSettings.DownloadMaxTries)
                {
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Failed to retrieve the size of the {fileTypeForLog} \"{titleForLog}\". Try {tries + 1} of {_userSettings.DownloadMaxTries}", DownloadProgressChangedLevel.Warning));
                }
                else
                {
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs($"Failed to retrieve the size of the {fileTypeForLog} \"{titleForLog}\". Hit max retries of {_userSettings.DownloadMaxTries}. Progress update may be wrong.", DownloadProgressChangedLevel.Error));
                }
            }

            tries++;
            if (!sizeRetrieved && tries < _userSettings.DownloadMaxTries)
            {
                await _resilienceService.WaitForCooldownAsync(tries, cancellationToken);
            }
        }
        while (!sizeRetrieved && tries < _userSettings.DownloadMaxTries);

        return size;
    }
}
