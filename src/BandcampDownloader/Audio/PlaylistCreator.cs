using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.Model;
using BandcampDownloader.Settings;
using PlaylistsNET.Content;
using PlaylistsNET.Models;

namespace BandcampDownloader.Audio;

internal interface IPlaylistCreator
{
    /// <summary>
    /// Saves the playlist to a file.
    /// </summary>
    Task SavePlaylistToFileAsync(Album album, CancellationToken cancellationToken);
}

internal sealed class PlaylistCreator : IPlaylistCreator
{
    private readonly IUserSettings _userSettings;

    public PlaylistCreator(ISettingsService settingsService)
    {
        _userSettings = settingsService.GetUserSettings();
    }

    /// <summary>
    /// Saves the playlist to a file.
    /// </summary>
    public async Task SavePlaylistToFileAsync(Album album, CancellationToken cancellationToken)
    {
        var fileContent = _userSettings.PlaylistFormat switch
        {
            PlaylistFormat.m3u => CreateM3UPlaylist(album),
            PlaylistFormat.pls => CreatePlsPlaylist(album),
            PlaylistFormat.wpl => CreateWplPlaylist(album),
            PlaylistFormat.zpl => CreateZplPlaylist(album),
            _ => throw new ArgumentOutOfRangeException(),
        };

        await File.WriteAllTextAsync(album.PlaylistPath, fileContent, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the playlist in m3u format.
    /// </summary>
    private string CreateM3UPlaylist(Album album)
    {
        var playlist = new M3uPlaylist
        {
            IsExtended = _userSettings.M3uExtended,
        };

        foreach (var track in album.Tracks)
        {
            playlist.PlaylistEntries.Add(new M3uPlaylistEntry
            {
                Album = album.Title,
                AlbumArtist = album.Artist,
                Duration = TimeSpan.FromSeconds(track.Duration),
                Path = Path.GetFileName(track.Path),
                Title = track.Title,
            });
        }

        return new M3uContent().ToText(playlist);
    }

    /// <summary>
    /// Returns the playlist in pls format.
    /// </summary>
    private static string CreatePlsPlaylist(Album album)
    {
        var playlist = new PlsPlaylist();

        foreach (var track in album.Tracks)
        {
            playlist.PlaylistEntries.Add(new PlsPlaylistEntry
            {
                Length = TimeSpan.FromSeconds(track.Duration),
                Path = Path.GetFileName(track.Path),
                Title = track.Title,
            });
        }

        return new PlsContent().ToText(playlist);
    }

    /// <summary>
    /// Returns the playlist in wpl format.
    /// </summary>
    private static string CreateWplPlaylist(Album album)
    {
        var playlist = new WplPlaylist
        {
            Title = album.Title,
        };

        foreach (var track in album.Tracks)
        {
            playlist.PlaylistEntries.Add(new WplPlaylistEntry
            {
                AlbumArtist = album.Artist,
                AlbumTitle = album.Title,
                Duration = TimeSpan.FromSeconds(track.Duration),
                Path = Path.GetFileName(track.Path),
                TrackArtist = album.Artist,
                TrackTitle = track.Title,
            });
        }

        return new WplContent().ToText(playlist);
    }

    /// <summary>
    /// Returns the playlist in zpl format.
    /// </summary>
    private static string CreateZplPlaylist(Album album)
    {
        var playlist = new ZplPlaylist
        {
            Title = album.Title,
        };

        foreach (var track in album.Tracks)
        {
            playlist.PlaylistEntries.Add(new ZplPlaylistEntry
            {
                AlbumArtist = album.Artist,
                AlbumTitle = album.Title,
                Duration = TimeSpan.FromSeconds(track.Duration),
                Path = Path.GetFileName(track.Path),
                TrackArtist = album.Artist,
                TrackTitle = track.Title,
            });
        }

        return new ZplContent().ToText(playlist);
    }
}
