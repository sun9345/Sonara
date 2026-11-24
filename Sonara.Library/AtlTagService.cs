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
        var fullPath = ValidateExistingFile(path);
        ct.ThrowIfCancellationRequested();

        var gate = GetLock(fullPath);
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var track = new Track(fullPath);
                var format = ResolveFormat(fullPath, track);

                var title = !string.IsNullOrWhiteSpace(track.Title)
                    ? track.Title
                    : Path.GetFileNameWithoutExtension(fullPath);

                return new TrackInfo(
                    FilePath: fullPath,
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
        var fullPath = ValidateExistingFile(path);
        ArgumentNullException.ThrowIfNull(tags);
        ct.ThrowIfCancellationRequested();

        var gate = GetLock(fullPath);
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var track = new Track(fullPath)
                {
                    Title = tags.Title ?? string.Empty,
                    Artist = tags.Artist ?? string.Empty,
                    Album = tags.Album ?? string.Empty,
                    AlbumArtist = tags.AlbumArtists ?? string.Empty,
                    Genre = tags.Genre ?? string.Empty,
                    TrackNumber = tags.TrackNumber is > 0 ? tags.TrackNumber.Value : 0,
                    Year = tags.Year is > 0 ? tags.Year.Value : 0
                };

                ct.ThrowIfCancellationRequested();

                if (!track.Save())
                    throw new InvalidOperationException($"Не удалось сохранить теги в файл: {fullPath}");
            }, ct).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<byte[]?> ReadCoverAsync(string path, CancellationToken ct = default)
    {
        var fullPath = ValidateExistingFile(path);
        ct.ThrowIfCancellationRequested();

        var gate = GetLock(fullPath);
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var track = new Track(fullPath);
                var picture = track.EmbeddedPictures.FirstOrDefault(p => p.PictureData is { Length: > 0 })
                              ?? track.EmbeddedPictures.FirstOrDefault();

                if (picture?.PictureData is not { Length: > 0 })
                    return null;

                return picture.PictureData.ToArray();
            }, ct).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task WriteCoverAsync(string path, byte[] image, string mimeType, CancellationToken ct = default)
    {
        var fullPath = ValidateExistingFile(path);
        ct.ThrowIfCancellationRequested();

        var hasImage = image is { Length: > 0 };
        if (hasImage && (string.IsNullOrWhiteSpace(mimeType) ||
                         !mimeType.Trim().StartsWith("image/", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Укажите корректный MIME-тип изображения (например, image/jpeg или image/png).", nameof(mimeType));
        }

        var gate = GetLock(fullPath);
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var track = new Track(fullPath);
                track.EmbeddedPictures.Clear();

                if (hasImage)
                {
                    var pic = PictureInfo.fromBinaryData(image!, PictureInfo.PIC_TYPE.Front);
                    track.EmbeddedPictures.Add(pic);
                }

                ct.ThrowIfCancellationRequested();

                if (!track.Save())
                    throw new InvalidOperationException($"Не удалось обновить обложку в файле: {fullPath}");
            }, ct).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    private static string ValidateExistingFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Аудиофайл не найден.", fullPath);

        return fullPath;
    }

    private static string ResolveFormat(string fullPath, Track track)
    {
        var ext = Path.GetExtension(fullPath).TrimStart('.').ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(ext))
            return ext;

        return !string.IsNullOrWhiteSpace(track.AudioFormat?.Name)
            ? track.AudioFormat.Name.ToLowerInvariant()
            : string.Empty;
    }

    private static SemaphoreSlim GetLock(string fullPath) =>
        FileLocks.GetOrAdd(fullPath, static _ => new SemaphoreSlim(1, 1));
}