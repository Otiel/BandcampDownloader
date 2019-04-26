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
        public static void SavePlaylistForAlbum(Album album) {
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

            File.WriteAllText(album.PlaylistPath, fileContent);
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
    }
}