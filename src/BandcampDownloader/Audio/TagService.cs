using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using BandcampDownloader.IO;
using BandcampDownloader.Model;
using BandcampDownloader.Settings;
using TagLib;
using TagLib.Id3v2;
using File = TagLib.File;

namespace BandcampDownloader.Audio;

internal interface ITagService
{
    Task SaveTagsInTrackAsync(Track track, Album album, Stream artworkStream, CancellationToken cancellationToken);
}

internal sealed class TagService : ITagService
{
    private readonly IUserSettings _userSettings;
    private readonly IFileService _fileService;

    public TagService(ISettingsService settingsService, IFileService fileService)
    {
        _fileService = fileService;
        _userSettings = settingsService.GetUserSettings();
    }

    public async Task SaveTagsInTrackAsync(Track track, Album album, Stream artworkStream, CancellationToken cancellationToken)
    {
        var tagFile = File.Create(track.Path);

        if (_userSettings.ModifyTags)
        {
            tagFile = UpdateStringTags(tagFile, track, album);
        }

        if (_userSettings.SaveCoverArtInTags && artworkStream != null)
        {
            tagFile = await UpdateCoverArtTagAsync(tagFile, artworkStream, cancellationToken);
        }

        tagFile.Save();
    }

    private File UpdateStringTags(File tagFile, Track track, Album album)
    {
        tagFile = UpdateArtist(tagFile, album.Artist, _userSettings.TagArtist);
        tagFile = UpdateAlbumArtist(tagFile, album.Artist, _userSettings.TagAlbumArtist);
        tagFile = UpdateAlbumTitle(tagFile, album.Title, _userSettings.TagAlbumTitle);
        tagFile = UpdateAlbumYear(tagFile, (uint)album.ReleaseDate.Year, _userSettings.TagYear);
        tagFile = UpdateTrackNumber(tagFile, (uint)track.Number, _userSettings.TagTrackNumber);
        tagFile = UpdateTrackTitle(tagFile, track.Title, _userSettings.TagTrackTitle);
        tagFile = UpdateTrackLyrics(tagFile, track.Lyrics, _userSettings.TagLyrics);
        tagFile = UpdateComments(tagFile, _userSettings.TagComments);
        return tagFile;
    }

    private async Task<File> UpdateCoverArtTagAsync(File tagFile, Stream artworkStream, CancellationToken cancellationToken)
    {
        // Copy the input stream to be thread-safe
        using var tmpStream = new MemoryStream();
        await artworkStream.CopyToAsync(tmpStream, cancellationToken);
        tmpStream.Position = 0;

        var tempFileName = Path.GetTempFileName();
        await _fileService.SaveStreamToFileAsync(artworkStream, tempFileName, cancellationToken);

        var attachmentFrame = new AttachmentFrame
        {
            Type = PictureType.FrontCover,
            Description = "Cover",
            MimeType = MediaTypeNames.Image.Jpeg,
            Data = ByteVector.FromStream(tmpStream),
            TextEncoding = StringType.UTF16,
        };

        tagFile.Tag.Pictures = [attachmentFrame];
        return tagFile;
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
