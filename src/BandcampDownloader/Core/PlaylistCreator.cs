using System;
using System.IO;
using System.Text;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Model;
using BandcampDownloader.Settings;
using PlaylistsNET.Content;
using PlaylistsNET.Models;

namespace BandcampDownloader.Core;

internal sealed class PlaylistCreator
{
    /// <summary>
    /// The album.
    /// </summary>
    private readonly Album _album;

    private readonly IUserSettings _userSettings;

    /// <summary>
    /// Initializes a new instance of PlaylistCreator.
    /// </summary>
    /// <param name="album"></param>
    public PlaylistCreator(Album album)
    {
        _userSettings = DependencyInjectionHelper.GetService<ISettingsService>().GetUserSettings();
        _album = album;
    }

    /// <summary>
    /// Saves the playlist to a file.
    /// </summary>
    public void SavePlaylistToFile()
    {
        var fileContent = _userSettings.PlaylistFormat switch
        {
            PlaylistFormat.m3u => CreateM3UPlaylist(),
            PlaylistFormat.pls => CreatePlsPlaylist(),
            PlaylistFormat.wpl => CreateWplPlaylist(),
            PlaylistFormat.zpl => CreateZplPlaylist(),
            _ => throw new ArgumentOutOfRangeException(),
        };

        File.WriteAllText(_album.PlaylistPath, fileContent, Encoding.UTF8);
    }

    /// <summary>
    /// Returns the playlist in m3u format.
    /// </summary>
    private string CreateM3UPlaylist()
    {
        var playlist = new M3uPlaylist
        {
            IsExtended = _userSettings.M3uExtended,
        };

        foreach (var track in _album.Tracks)
        {
            playlist.PlaylistEntries.Add(new M3uPlaylistEntry
            {
                Album = _album.Title,
                AlbumArtist = _album.Artist,
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
    private string CreatePlsPlaylist()
    {
        var playlist = new PlsPlaylist();

        foreach (var track in _album.Tracks)
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
    private string CreateWplPlaylist()
    {
        var playlist = new WplPlaylist
        {
            Title = _album.Title,
        };

        foreach (var track in _album.Tracks)
        {
            playlist.PlaylistEntries.Add(new WplPlaylistEntry
            {
                AlbumArtist = _album.Artist,
                AlbumTitle = _album.Title,
                Duration = TimeSpan.FromSeconds(track.Duration),
                Path = Path.GetFileName(track.Path),
                TrackArtist = _album.Artist,
                TrackTitle = track.Title,
            });
        }

        return new WplContent().ToText(playlist);
    }

    /// <summary>
    /// Returns the playlist in zpl format.
    /// </summary>
    private string CreateZplPlaylist()
    {
        var playlist = new ZplPlaylist
        {
            Title = _album.Title,
        };

        foreach (var track in _album.Tracks)
        {
            playlist.PlaylistEntries.Add(new ZplPlaylistEntry
            {
                AlbumArtist = _album.Artist,
                AlbumTitle = _album.Title,
                Duration = TimeSpan.FromSeconds(track.Duration),
                Path = Path.GetFileName(track.Path),
                TrackArtist = _album.Artist,
                TrackTitle = track.Title,
            });
        }

        return new ZplContent().ToText(playlist);
    }
}
