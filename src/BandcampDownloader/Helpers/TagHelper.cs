namespace BandcampDownloader {

    internal static class TagHelper {

        /// <summary>
        /// Returns the file updated with the specified album artist based on the specified TagEditAction.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="albumArtist">The value used if TagEditAction = Modify.</param>
        /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
        public static TagLib.File UpdateAlbumArtist(TagLib.File file, string albumArtist, TagEditAction editAction) {
            switch (editAction) {
                case TagEditAction.Empty:
                    file.Tag.AlbumArtists = new string[1] { "" };
                    break;
                case TagEditAction.Modify:
                    file.Tag.AlbumArtists = new string[1] { albumArtist };
                    break;
                case TagEditAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }

        /// <summary>
        /// Returns the file updated with the specified album title based on the specified TagEditAction.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="albumTitle">The value used if TagEditAction = Modify.</param>
        /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
        public static TagLib.File UpdateAlbumTitle(TagLib.File file, string albumTitle, TagEditAction editAction) {
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

        /// <summary>
        /// Returns the file updated with the specified album year based on the specified TagEditAction.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="albumYear">The value used if TagEditAction = Modify.</param>
        /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
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

        /// <summary>
        /// Returns the file updated with the specified artist based on the specified TagEditAction.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="artist">The value used if TagEditAction = Modify.</param>
        /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
        public static TagLib.File UpdateArtist(TagLib.File file, string artist, TagEditAction editAction) {
            switch (editAction) {
                case TagEditAction.Empty:
                    file.Tag.Performers = new string[1] { "" };
                    break;
                case TagEditAction.Modify:
                    file.Tag.Performers = new string[1] { artist };
                    break;
                case TagEditAction.DoNotModify:
                    break;
                default:
                    break;
            }
            return file;
        }

        /// <summary>
        /// Returns the file updated by changing the comments based on the specified TagEditAction.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="editAction">The TagRemoveAction specifying how the tag should be updated.</param>
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

        /// <summary>
        /// Returns the file updated with the specified lyrics based on the specified TagEditAction.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="trackLyrics">The value used if TagEditAction = Modify.</param>
        /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
        public static TagLib.File UpdateTrackLyrics(TagLib.File file, string trackLyrics, TagEditAction editAction) {
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

        /// <summary>
        /// Returns the file updated with the specified track number based on the specified TagEditAction.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="trackNumber">The value used if TagEditAction = Modify.</param>
        /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
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

        /// <summary>
        /// Returns the file updated with the specified track title based on the specified TagEditAction.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="trackTitle">The value used if TagEditAction = Modify.</param>
        /// <param name="editAction">The TagEditAction specifying how the tag should be updated.</param>
        public static TagLib.File UpdateTrackTitle(TagLib.File file, string trackTitle, TagEditAction editAction) {
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