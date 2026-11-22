using System.Collections.Concurrent;
using ATL;
using Sonara.Core;

namespace Sonara.Library;

public class AtlTagService : ITagService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks =
        new(StringComparer.OrdinalIgnoreCase);

    public async Task<TrackInfo> ReadAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
            throw new FileNotFoundException("Аудиофайл не найден.", path);

        var gate = GetLock(path);
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var track = new Track(path);
                var format = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(format) && track.AudioFormat is not null)
                    format = track.AudioFormat.Name.ToLowerInvariant();

                var title = !string.IsNullOrWhiteSpace(track.Title)
                    ? track.Title
                    : Path.GetFileNameWithoutExtension(path);

                return new TrackInfo(
                    FilePath: path,
                    Title: title,
                    Artist: track.Artist ?? string.Empty,
                    Album: track.Album ?? string.Empty,
                    TrackNumber: track.TrackNumber > 0 ? track.TrackNumber : null,
                    AlbumArtists: track.AlbumArtist ?? string.Empty,
                    Year: track.Year > 0 ? track.Year : null,
                    Genre: track.Genre ?? string.Empty,
                    Duration: track.Duration > 0 ? TimeSpan.FromSeconds(track.Duration) : TimeSpan.Zero,
                    Format: format
                );
            }, ct).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task WriteAsync(string path, TrackInfo tags, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(tags);

        if (!File.Exists(path))
            throw new FileNotFoundException("Аудиофайл не найден.", path);

        var gate = GetLock(path);
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var track = new Track(path);

                track.Title = tags.Title ?? string.Empty;
                track.Artist = tags.Artist ?? string.Empty;
                track.Album = tags.Album ?? string.Empty;
                track.AlbumArtist = tags.AlbumArtists ?? string.Empty;
                track.Genre = tags.Genre ?? string.Empty;
                track.TrackNumber = tags.TrackNumber is > 0 ? tags.TrackNumber.Value : 0;
                track.Year = tags.Year is > 0 ? tags.Year.Value : 0;

                if (!track.Save())
                    throw new InvalidOperationException($"Не удалось сохранить теги в файл: {path}");
            }, ct).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<byte[]?> ReadCoverAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
            throw new FileNotFoundException("Аудиофайл не найден.", path);

        var gate = GetLock(path);
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var track = new Track(path);
                var picture = track.EmbeddedPictures.FirstOrDefault(p => p.PictureData is { Length: > 0 })
                              ?? track.EmbeddedPictures.FirstOrDefault();

                return picture?.PictureData is { Length: > 0 } ? picture.PictureData : null;
            }, ct).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task WriteCoverAsync(string path, byte[] image, string mimeType, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
            throw new FileNotFoundException("Аудиофайл не найден.", path);

        var gate = GetLock(path);
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var track = new Track(path);
                track.EmbeddedPictures.Clear();

                if (image is { Length: > 0 })
                {
                    var pic = PictureInfo.fromBinaryData(image, PictureInfo.PIC_TYPE.Front);
                    track.EmbeddedPictures.Add(pic);
                }

                if (!track.Save())
                    throw new InvalidOperationException($"Не удалось записать обложку в файл: {path}");
            }, ct).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    private static SemaphoreSlim GetLock(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return FileLocks.GetOrAdd(fullPath, static _ => new SemaphoreSlim(1, 1));
    }
}