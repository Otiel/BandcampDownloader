using System;
using System.IO;
using PlaylistsNET.Content;
using PlaylistsNET.Models;

namespace BandcampDownloader {

    internal static class PlaylistHelper {

        /// <summary>
        /// Saves the playlist file for the specified album in the specified folder.
        /// </summary>
        /// <param name="album">The album relative of the playlist.</param>
        /// <param name="folderPath">The folder where the playlist should be stored.</param>
        public static void SavePlaylistForAlbum(Album album, String folderPath) {
            String fileContent;

            switch (App.UserSettings.PlaylistFormat) {
                case PlaylistFormat.m3u:
                    fileContent = CreateM3uPlaylist(album);
                    break;
                case PlaylistFormat.pls:
                    fileContent = CreatePlsPlaylist(album);
                    break;
                case PlaylistFormat.wpl:
                    fileContent = CreateWplPlaylist(album);
                    break;
                case PlaylistFormat.zpl:
                    fileContent = CreateZplPlaylist(album);
                    break;
                default:
                    throw new NotImplementedException();
            }

            File.WriteAllText(ComputeFilePath(album, folderPath), fileContent);
        }

        /// <summary>
        /// Returns the full path where the playlist file should be saved.
        /// </summary>
        /// <param name="album">The album relative of the playlist.</param>
        /// <param name="folderPath">The folder where the playlist should be stored.</param>
        private static String ComputeFilePath(Album album, String folderPath) {
            String fileExt = GetFileExtension();

            // Compute paths where to save artwork
            String filePath = folderPath + "\\" + ParseFileName(album) + fileExt;

            if (filePath.Length >= 260) {
                // Windows doesn't do well with path + filename >= 260 characters (and path >= 248 characters)
                // Path has been shorten to 247 characters before, so we have 12 characters max left for filename.ext
                int fileNameMaxLength = 12 - fileExt.Length;
                filePath = folderPath + "\\" + ParseFileName(album).Substring(0, fileNameMaxLength) + fileExt;
            }

            return filePath;
        }

        /// <summary>
        /// Returns the playlist in m3u format for the specified album.
        /// </summary>
        /// <param name="album">The album relative of the playlist.</param>
        private static String CreateM3uPlaylist(Album album) {
            var playlist = new M3uPlaylist() {
                IsExtended = App.UserSettings.M3uExtended,
            };

            foreach (Track track in album.Tracks) {
                playlist.PlaylistEntries.Add(new M3uPlaylistEntry() {
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
        /// Returns the playlist in pls format for the specified album.
        /// </summary>
        /// <param name="album">The album relative of the playlist.</param>
        private static String CreatePlsPlaylist(Album album) {
            var playlist = new PlsPlaylist();

            foreach (Track track in album.Tracks) {
                playlist.PlaylistEntries.Add(new PlsPlaylistEntry() {
                    Length = TimeSpan.FromSeconds(track.Duration),
                    Path = Path.GetFileName(track.Path),
                    Title = track.Title,
                });
            }

            return new PlsContent().ToText(playlist);
        }

        /// <summary>
        /// Returns the playlist in wpl format for the specified album.
        /// </summary>
        /// <param name="album">The album relative of the playlist.</param>
        private static String CreateWplPlaylist(Album album) {
            var playlist = new WplPlaylist() {
                Title = album.Title,
            };

            foreach (Track track in album.Tracks) {
                playlist.PlaylistEntries.Add(new WplPlaylistEntry() {
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
        /// Returns the playlist in zpl format for the specified album.
        /// </summary>
        /// <param name="album">The album relative of the playlist.</param>
        private static String CreateZplPlaylist(Album album) {
            var playlist = new ZplPlaylist() {
                Title = album.Title,
            };

            foreach (Track track in album.Tracks) {
                playlist.PlaylistEntries.Add(new ZplPlaylistEntry() {
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

        /// <summary>
        /// Returns the file extension to be used for the playlist, depending of the type of playlist defined in UserSettings.
        /// </summary>
        private static String GetFileExtension() {
            switch (App.UserSettings.PlaylistFormat) {
                case PlaylistFormat.m3u:
                    return ".m3u";
                case PlaylistFormat.pls:
                    return ".pls";
                case PlaylistFormat.wpl:
                    return ".wpl";
                case PlaylistFormat.zpl:
                    return ".zpl";
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the file name to be used for the playlist file of the specified album from the file name format saved
        /// in the UserSettings, by replacing the placeholders strings with their corresponding values.
        /// </summary>
        /// <param name="album">The album relative of the playlist.</param>
        private static String ParseFileName(Album album) {
            String fileName = App.UserSettings.PlaylistFileNameFormat
                .Replace("{year}", album.ReleaseDate.Year.ToString())
                .Replace("{month}", album.ReleaseDate.Month.ToString("00"))
                .Replace("{day}", album.ReleaseDate.Day.ToString("00"))
                .Replace("{album}", album.Title)
                .Replace("{artist}", album.Artist);
            return fileName.ToAllowedFileName();
        }
    }
}