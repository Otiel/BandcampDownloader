using System;
using BandcampDownloader.Settings;
using TagLib;

namespace BandcampDownloader.Helpers;

internal interface ITagService
{
    /// <summary>
    /// Returns the file updated with the specified album artist based on the specified TagEditAction.
    /// </summary>
    /// <param name="file">The file to update.</param>
    /// <param name="albumArtist">The value used if TagEditAction = Modify.</param>
    /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
    File UpdateAlbumArtist(File file, string albumArtist, TagEditAction editAction);

    /// <summary>
    /// Returns the file updated with the specified album title based on the specified TagEditAction.
    /// </summary>
    /// <param name="file">The file to update.</param>
    /// <param name="albumTitle">The value used if TagEditAction = Modify.</param>
    /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
    File UpdateAlbumTitle(File file, string albumTitle, TagEditAction editAction);

    /// <summary>
    /// Returns the file updated with the specified album year based on the specified TagEditAction.
    /// </summary>
    /// <param name="file">The file to update.</param>
    /// <param name="albumYear">The value used if TagEditAction = Modify.</param>
    /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
    File UpdateAlbumYear(File file, uint albumYear, TagEditAction editAction);

    /// <summary>
    /// Returns the file updated with the specified artist based on the specified TagEditAction.
    /// </summary>
    /// <param name="file">The file to update.</param>
    /// <param name="artist">The value used if TagEditAction = Modify.</param>
    /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
    File UpdateArtist(File file, string artist, TagEditAction editAction);

    /// <summary>
    /// Returns the file updated by changing the comments based on the specified TagEditAction.
    /// </summary>
    /// <param name="file">The file to update.</param>
    /// <param name="removeAction">The TagRemoveAction specifying how the tag should be updated.</param>
    File UpdateComments(File file, TagRemoveAction removeAction);

    /// <summary>
    /// Returns the file updated with the specified lyrics based on the specified TagEditAction.
    /// </summary>
    /// <param name="file">The file to update.</param>
    /// <param name="trackLyrics">The value used if TagEditAction = Modify.</param>
    /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
    File UpdateTrackLyrics(File file, string trackLyrics, TagEditAction editAction);

    /// <summary>
    /// Returns the file updated with the specified track number based on the specified TagEditAction.
    /// </summary>
    /// <param name="file">The file to update.</param>
    /// <param name="trackNumber">The value used if TagEditAction = Modify.</param>
    /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
    File UpdateTrackNumber(File file, uint trackNumber, TagEditAction editAction);

    /// <summary>
    /// Returns the file updated with the specified track title based on the specified TagEditAction.
    /// </summary>
    /// <param name="file">The file to update.</param>
    /// <param name="trackTitle">The value used if TagEditAction = Modify.</param>
    /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
    File UpdateTrackTitle(File file, string trackTitle, TagEditAction editAction);
}

internal sealed class TagService : ITagService
{
    public File UpdateAlbumArtist(File file, string albumArtist, TagEditAction editAction)
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

    public File UpdateAlbumTitle(File file, string albumTitle, TagEditAction editAction)
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

    public File UpdateAlbumYear(File file, uint albumYear, TagEditAction editAction)
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

    public File UpdateArtist(File file, string artist, TagEditAction editAction)
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

    public File UpdateComments(File file, TagRemoveAction removeAction)
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

    public File UpdateTrackLyrics(File file, string trackLyrics, TagEditAction editAction)
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

    public File UpdateTrackNumber(File file, uint trackNumber, TagEditAction editAction)
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

    public File UpdateTrackTitle(File file, string trackTitle, TagEditAction editAction)
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
