using Sonara.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fakes
{
    public class InMemoryLibraryRepository : ILibraryRepository
    {
        private readonly Dictionary<string, TrackInfo> _tracks;
        private readonly List<string> _folders = new();

        public InMemoryLibraryRepository()
        {
            _tracks = GenerateSampleTracks().ToDictionary(t => t.FilePath);
        }

        public async Task<IReadOnlyList<TrackInfo>> GetAllTracksAsync()
        {
            await Task.Delay(150);   // имитация доступа к БД
            return _tracks.Values.OrderBy(t => t.Artist).ThenBy(t => t.Album).ToList();
        }

        public async Task<IReadOnlyList<TrackInfo>> SearchAsync(string query)
        {
            await Task.Delay(100);
            return _tracks.Values
                .Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || t.Artist.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || t.Album.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public Task UpsertTracksAsync(IEnumerable<TrackInfo> tracks)
        {
            foreach (var t in tracks) _tracks[t.FilePath] = t;   // ключ: путь к файлу
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> GetLibraryFoldersAsync()
            => Task.FromResult<IReadOnlyList<string>>(_folders.ToList());

        public Task AddLibraryFolderAsync(string path)
        {
            if (!_folders.Contains(path)) _folders.Add(path);
            return Task.CompletedTask;
        }

        private static IEnumerable<TrackInfo> GenerateSampleTracks()
        {
            var formats = new[] { "mp3", "flac", "m4a", "wav" };
            for (int i = 1; i <= 30; i++)
            {
                yield return new TrackInfo(
                    FilePath: $@"C:\Music\Artist {i % 5}\Album {i % 7}\Track {i}.{formats[i % 4]}",
                    Title: $"Track {i}",
                    Artist: $"Artist {i % 5}",
                    Album: $"Album {i % 7}",
                    AlbumArtists: "ddd",
                    Year: 2000 + i % 25,
                    Genre: i % 2 == 0 ? "Rock" : "Jazz",
                    TrackNumber: i,
                    Duration: TimeSpan.FromSeconds(120 + i * 7),
                    Format: formats[i % 4]);
            }
        }
    }
}
