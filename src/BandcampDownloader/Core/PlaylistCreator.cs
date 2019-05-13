using System;
using System.IO;
using System.Text;
using PlaylistsNET.Content;
using PlaylistsNET.Models;

namespace BandcampDownloader {

    internal class PlaylistCreator {
        /// <summary>
        /// The album.
        /// </summary>
        private readonly Album _album;

        /// <summary>
        /// Initializes a new instance of PlaylistCreator.
        /// </summary>
        /// <param name="album"></param>
        public PlaylistCreator(Album album) {
            _album = album;
        }

        /// <summary>
        /// Saves the playlist to a file.
        /// </summary>
        public void SavePlaylistToFile() {
            string fileContent;

            switch (App.UserSettings.PlaylistFormat) {
                case PlaylistFormat.m3u:
                    fileContent = CreateM3uPlaylist();
                    break;
                case PlaylistFormat.pls:
                    fileContent = CreatePlsPlaylist();
                    break;
                case PlaylistFormat.wpl:
                    fileContent = CreateWplPlaylist();
                    break;
                case PlaylistFormat.zpl:
                    fileContent = CreateZplPlaylist();
                    break;
                default:
                    throw new NotImplementedException();
            }

            File.WriteAllText(_album.PlaylistPath, fileContent, Encoding.UTF8);
        }

        /// <summary>
        /// Returns the playlist in m3u format.
        /// </summary>
        private string CreateM3uPlaylist() {
            var playlist = new M3uPlaylist() {
                IsExtended = App.UserSettings.M3uExtended,
            };

            foreach (Track track in _album.Tracks) {
                playlist.PlaylistEntries.Add(new M3uPlaylistEntry() {
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
        private string CreatePlsPlaylist() {
            var playlist = new PlsPlaylist();

            foreach (Track track in _album.Tracks) {
                playlist.PlaylistEntries.Add(new PlsPlaylistEntry() {
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
        private string CreateWplPlaylist() {
            var playlist = new WplPlaylist() {
                Title = _album.Title,
            };

            foreach (Track track in _album.Tracks) {
                playlist.PlaylistEntries.Add(new WplPlaylistEntry() {
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
        private string CreateZplPlaylist() {
            var playlist = new ZplPlaylist() {
                Title = _album.Title,
            };

            foreach (Track track in _album.Tracks) {
                playlist.PlaylistEntries.Add(new ZplPlaylistEntry() {
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
}