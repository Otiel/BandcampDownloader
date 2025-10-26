using System;
using System.IO;
using System.Text;
using BandcampDownloader.Model;
using BandcampDownloader.Settings;
using PlaylistsNET.Content;
using PlaylistsNET.Models;

namespace BandcampDownloader.Core;

internal interface IPlaylistCreator
{
    void SavePlaylistToFile(Album album);
}

internal sealed class PlaylistCreator : IPlaylistCreator
{
    private readonly IUserSettings _userSettings;

    public PlaylistCreator(ISettingsService settingsService)
    {
        _userSettings = settingsService.GetUserSettings();
    }

    public void SavePlaylistToFile(Album album)
    {
        var fileContent = _userSettings.PlaylistFormat switch
        {
            PlaylistFormat.m3u => CreateM3UPlaylist(album),
            PlaylistFormat.pls => CreatePlsPlaylist(album),
            PlaylistFormat.wpl => CreateWplPlaylist(album),
            PlaylistFormat.zpl => CreateZplPlaylist(album),
            _ => throw new ArgumentOutOfRangeException(),
        };

        File.WriteAllText(album.PlaylistPath, fileContent, Encoding.UTF8);
    }

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
