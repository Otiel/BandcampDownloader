using System;

namespace BandcampDownloader {

    internal static class TagHelper {

        public static TagLib.File UpdateAlbumArtist(TagLib.File file, String albumArtist, TagEditAction editAction) {
            switch (editAction) {
                case TagEditAction.Empty:
                    file.Tag.AlbumArtists = new String[1] { "" };
                    break;
                case TagEditAction.Modify:
                    file.Tag.AlbumArtists = new String[1] { albumArtist };
                    break;
                case TagEditAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }

        public static TagLib.File UpdateAlbumTitle(TagLib.File file, String albumTitle, TagEditAction editAction) {
            switch (editAction) {
                case TagEditAction.Empty:
                    file.Tag.Album = "";
                    break;
                case TagEditAction.Modify:
                    file.Tag.Album = albumTitle;
                    break;
                case TagEditAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }

        public static TagLib.File UpdateAlbumYear(TagLib.File file, uint albumYear, TagEditAction editAction) {
            switch (editAction) {
                case TagEditAction.Empty:
                    file.Tag.Year = 0;
                    break;
                case TagEditAction.Modify:
                    file.Tag.Year = albumYear;
                    break;
                case TagEditAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }

        public static TagLib.File UpdateArtist(TagLib.File file, String artist, TagEditAction editAction) {
            switch (editAction) {
                case TagEditAction.Empty:
                    file.Tag.Performers = new String[1] { "" };
                    break;
                case TagEditAction.Modify:
                    file.Tag.Performers = new String[1] { artist };
                    break;
                case TagEditAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }

        public static TagLib.File UpdateComments(TagLib.File file, TagRemoveAction removeAction) {
            switch (removeAction) {
                case TagRemoveAction.Empty:
                    file.Tag.Comment = "";
                    break;
                case TagRemoveAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }

        public static TagLib.File UpdateTrackLyrics(TagLib.File file, String trackLyrics, TagEditAction editAction) {
            switch (editAction) {
                case TagEditAction.Empty:
                    file.Tag.Lyrics = "";
                    break;
                case TagEditAction.Modify:
                    file.Tag.Lyrics = trackLyrics;
                    break;
                case TagEditAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }

        public static TagLib.File UpdateTrackNumber(TagLib.File file, uint trackNumber, TagEditAction editAction) {
            switch (editAction) {
                case TagEditAction.Empty:
                    file.Tag.Track = 0;
                    break;
                case TagEditAction.Modify:
                    file.Tag.Track = trackNumber;
                    break;
                case TagEditAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }

        public static TagLib.File UpdateTrackTitle(TagLib.File file, String trackTitle, TagEditAction editAction) {
            switch (editAction) {
                case TagEditAction.Empty:
                    file.Tag.Title = "";
                    break;
                case TagEditAction.Modify:
                    file.Tag.Title = trackTitle;
                    break;
                case TagEditAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }
    }
}