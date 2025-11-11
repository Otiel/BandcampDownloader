using System;
using BandcampDownloader.Model;
using BandcampDownloader.Settings;
using TagLib;

namespace BandcampDownloader.Audio;

internal interface ITagService
{
    void SaveTagsInTrack(Track track, Album album);
    void SaveCoverInTrack(Track track, Picture artwork);
}

internal sealed class TagService : ITagService
{
    private readonly IUserSettings _userSettings;

    public TagService(ISettingsService settingsService)
    {
        _userSettings = settingsService.GetUserSettings();
    }

    public void SaveTagsInTrack(Track track, Album album)
    {
        var tagFile = File.Create(track.Path);
        tagFile = UpdateArtist(tagFile, album.Artist, _userSettings.TagArtist);
        tagFile = UpdateAlbumArtist(tagFile, album.Artist, _userSettings.TagAlbumArtist);
        tagFile = UpdateAlbumTitle(tagFile, album.Title, _userSettings.TagAlbumTitle);
        tagFile = UpdateAlbumYear(tagFile, (uint)album.ReleaseDate.Year, _userSettings.TagYear);
        tagFile = UpdateTrackNumber(tagFile, (uint)track.Number, _userSettings.TagTrackNumber);
        tagFile = UpdateTrackTitle(tagFile, track.Title, _userSettings.TagTrackTitle);
        tagFile = UpdateTrackLyrics(tagFile, track.Lyrics, _userSettings.TagLyrics);
        tagFile = UpdateComments(tagFile, _userSettings.TagComments);
        tagFile.Save();
    }

    public void SaveCoverInTrack(Track track, Picture artwork)
    {
        var tagFile = File.Create(track.Path);
        tagFile.Tag.Pictures = [artwork];
        tagFile.Save();
    }

    private static File UpdateAlbumArtist(File file, string albumArtist, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                file.Tag.AlbumArtists = [""];
                break;
            case TagEditAction.Modify:
                file.Tag.AlbumArtists = [albumArtist];
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return file;
    }

    private static File UpdateAlbumTitle(File file, string albumTitle, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                file.Tag.Album = "";
                break;
            case TagEditAction.Modify:
                file.Tag.Album = albumTitle;
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return file;
    }

    private static File UpdateAlbumYear(File file, uint albumYear, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                file.Tag.Year = 0;
                break;
            case TagEditAction.Modify:
                file.Tag.Year = albumYear;
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return file;
    }

    private static File UpdateArtist(File file, string artist, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                file.Tag.Performers = [""];
                break;
            case TagEditAction.Modify:
                file.Tag.Performers = [artist];
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return file;
    }

    private static File UpdateComments(File file, TagRemoveAction removeAction)
    {
        switch (removeAction)
        {
            case TagRemoveAction.Empty:
                file.Tag.Comment = "";
                break;
            case TagRemoveAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(removeAction), removeAction, null);
        }

        return file;
    }

    private static File UpdateTrackLyrics(File file, string trackLyrics, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                file.Tag.Lyrics = "";
                break;
            case TagEditAction.Modify:
                file.Tag.Lyrics = trackLyrics;
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return file;
    }

    private static File UpdateTrackNumber(File file, uint trackNumber, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                file.Tag.Track = 0;
                break;
            case TagEditAction.Modify:
                file.Tag.Track = trackNumber;
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return file;
    }

    private static File UpdateTrackTitle(File file, string trackTitle, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                file.Tag.Title = "";
                break;
            case TagEditAction.Modify:
                file.Tag.Title = trackTitle;
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return file;
    }
}
