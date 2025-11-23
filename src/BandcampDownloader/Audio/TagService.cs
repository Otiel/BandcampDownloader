using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ATL;
using BandcampDownloader.Audio.AtlDotNet;
using BandcampDownloader.Model;
using BandcampDownloader.Settings;
using Track = BandcampDownloader.Model.Track;

namespace BandcampDownloader.Audio;

internal interface ITagService
{
    Task SaveTagsInTrackAsync(Track track, Album album, Stream artworkStream, CancellationToken cancellationToken);
}

internal sealed class TagService : ITagService
{
    private readonly IUserSettings _userSettings;

    public TagService(ISettingsService settingsService, IAtlLogInterceptor atlLogInterceptor)
    {
        _userSettings = settingsService.GetUserSettings();
        atlLogInterceptor.Initialize();
    }

    public async Task SaveTagsInTrackAsync(Track track, Album album, Stream artworkStream, CancellationToken cancellationToken)
    {
        var atlTrack = new ATL.Track(track.Path);

        if (_userSettings.ModifyTags)
        {
            atlTrack = UpdateStringTags(atlTrack, track, album);
        }

        if (_userSettings.SaveCoverArtInTags && artworkStream != null)
        {
            atlTrack = await UpdateCoverArtTagAsync(atlTrack, artworkStream, cancellationToken);
        }

        atlTrack.Save();
    }

    private ATL.Track UpdateStringTags(ATL.Track atlTrack, Track track, Album album)
    {
        atlTrack = UpdateArtist(atlTrack, album.Artist, _userSettings.TagArtist);
        atlTrack = UpdateAlbumArtist(atlTrack, album.Artist, _userSettings.TagAlbumArtist);
        atlTrack = UpdateAlbumTitle(atlTrack, album.Title, _userSettings.TagAlbumTitle);
        atlTrack = UpdateAlbumYear(atlTrack, album.ReleaseDate.Year, _userSettings.TagYear);
        atlTrack = UpdateTrackNumber(atlTrack, track.Number, _userSettings.TagTrackNumber);
        atlTrack = UpdateTrackTitle(atlTrack, track.Title, _userSettings.TagTrackTitle);
        atlTrack = UpdateTrackLyrics(atlTrack, track.Lyrics, _userSettings.TagLyrics);
        atlTrack = UpdateComments(atlTrack, _userSettings.TagComments);
        return atlTrack;
    }

    private static async Task<ATL.Track> UpdateCoverArtTagAsync(ATL.Track atlTrack, Stream artworkStream, CancellationToken cancellationToken)
    {
        // Copy the input stream to be thread-safe
        using var artworkStreamCopy = new MemoryStream();
        artworkStream.Position = 0;
        await artworkStream.CopyToAsync(artworkStreamCopy, cancellationToken);

        var pictureInfo = PictureInfo.fromBinaryData(artworkStreamCopy.ToArray());

        atlTrack.EmbeddedPictures.Clear();
        atlTrack.EmbeddedPictures.Add(pictureInfo);
        return atlTrack;
    }

    private static ATL.Track UpdateAlbumArtist(ATL.Track atlTrack, string albumArtist, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                atlTrack.AlbumArtist = "";
                break;
            case TagEditAction.Modify:
                atlTrack.AlbumArtist = ToEmptyIfNull(albumArtist);
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return atlTrack;
    }

    private static ATL.Track UpdateAlbumTitle(ATL.Track atlTrack, string albumTitle, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                atlTrack.Album = "";
                break;
            case TagEditAction.Modify:
                atlTrack.Album = ToEmptyIfNull(albumTitle);
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return atlTrack;
    }

    private static ATL.Track UpdateAlbumYear(ATL.Track atlTrack, int albumYear, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                atlTrack.Year = 0;
                break;
            case TagEditAction.Modify:
                atlTrack.Year = albumYear;
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return atlTrack;
    }

    private static ATL.Track UpdateArtist(ATL.Track atlTrack, string artist, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                atlTrack.Artist = "";
                break;
            case TagEditAction.Modify:
                atlTrack.Artist = ToEmptyIfNull(artist);
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return atlTrack;
    }

    private static ATL.Track UpdateComments(ATL.Track atlTrack, TagRemoveAction removeAction)
    {
        switch (removeAction)
        {
            case TagRemoveAction.Empty:
                atlTrack.Comment = "";
                break;
            case TagRemoveAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(removeAction), removeAction, null);
        }

        return atlTrack;
    }

    private static ATL.Track UpdateTrackLyrics(ATL.Track atlTrack, string trackLyrics, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
            case TagEditAction.Modify when string.IsNullOrWhiteSpace(trackLyrics):
                atlTrack.Lyrics = new List<LyricsInfo>();
                break;
            case TagEditAction.Modify when !string.IsNullOrWhiteSpace(trackLyrics):
                var lyricsInfo = new LyricsInfo
                {
                    UnsynchronizedLyrics = trackLyrics,
                };
                atlTrack.Lyrics = new List<LyricsInfo> { lyricsInfo };
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return atlTrack;
    }

    private static ATL.Track UpdateTrackNumber(ATL.Track atlTrack, int trackNumber, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                atlTrack.TrackNumber = 0;
                break;
            case TagEditAction.Modify:
                atlTrack.TrackNumber = trackNumber;
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return atlTrack;
    }

    private static ATL.Track UpdateTrackTitle(ATL.Track atlTrack, string trackTitle, TagEditAction editAction)
    {
        switch (editAction)
        {
            case TagEditAction.Empty:
                atlTrack.Title = "";
                break;
            case TagEditAction.Modify:
                atlTrack.Title = ToEmptyIfNull(trackTitle);
                break;
            case TagEditAction.DoNotModify:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editAction), editAction, null);
        }

        return atlTrack;
    }

    private static string ToEmptyIfNull(string s)
    {
        return s ?? "";
    }
}
