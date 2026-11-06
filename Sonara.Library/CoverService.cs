using SkiaSharp; // для изменения размера изображений
using Sonara.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Sonara.Library
{
    public class CoverService : ICoverService
    {
        // для перебора имен для поиска обложки трека в папке с треком
        private static readonly string[] FolderCoverNames = { "cover", "folder", "front", "album" };
        private static readonly string[] FolderCoverExtensions = { ".jpg", ".jpeg", ".png" };

        private readonly ITagService _tags;
        private readonly string _cacheDir; // папка в которой хранятся уже загруженные когда либо обложки 
        public CoverService(ITagService tags, string? cacheDirectory = null)
        {
            _tags = tags;

            // если обложки еще никогда не кешировались, создает путь для этого и след. строчка создаст папку
            _cacheDir = cacheDirectory ?? Path.Combine( 
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sonara", "Covers"); 

            Directory.CreateDirectory(_cacheDir);
        }

        /// <summary>
        /// ищет обложку для трека в папке с этим треком
        /// </summary>
        private static async Task<byte[]?> ReadFolderCoverAsync(string trackPath, CancellationToken ct)
        {
            var dir = Path.GetDirectoryName(trackPath);
            if (dir is null || !Directory.Exists(dir)) return null;

            foreach (var name in FolderCoverNames)
                foreach (var ext in FolderCoverExtensions)
                {
                    var candidate = Path.Combine(dir, name + ext);
                    if (File.Exists(candidate))
                        return await File.ReadAllBytesAsync(candidate, ct);
                }
            return null;
        }

        public async Task<string?> GetCoverPathAsync(TrackInfo track, CoverSize size,
                                                 CancellationToken ct = default)
        {
            byte[]? original;

            try
            {
                original = await _tags.ReadCoverAsync(track.FilePath, ct)
                           ?? await ReadFolderCoverAsync(track.FilePath, ct);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return null;   // файл занят или недоступен: обложки просто нет
            }

            if (original is null || original.Length == 0) return null;

            // хеш
            var coverId = ComputeCoverId(original);

            // кэш
            var cachePath = Path.Combine(_cacheDir, $"{coverId}_{(int)size}.jpg");
            if (File.Exists(cachePath)) 
                return cachePath;

            // если нашлась обложка то она кэшируется чтобы потом не искать
            return await Task.Run(() => SaveResized(original, (int)size, cachePath), ct);
        }

        /// <summary>
        /// считает хеш обложки
        /// </summary>
        private static string ComputeCoverId(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes))[..16].ToLowerInvariant();

        /// <summary>
        /// ресайзит изображение в кэш
        /// </summary>
        private static string? SaveResized(byte[] original, int maxSide, string cachePath)
        {
            using var bitmap = SKBitmap.Decode(original);
            if (bitmap is null) return null;   // это не изображение

            var scale = Math.Min(1.0, (double)maxSide / Math.Max(bitmap.Width, bitmap.Height));
            var info = new SKImageInfo(
                Math.Max(1, (int)(bitmap.Width * scale)),
                Math.Max(1, (int)(bitmap.Height * scale)));

            using var resized = bitmap.Resize(info,
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            if (resized is null) return null;

            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

            // Запись через временный файл: два параллельных запроса не испортят друг друга
            var tmp = $"{cachePath}.{Guid.NewGuid():N}.tmp";
            File.WriteAllBytes(tmp, data.ToArray());
            File.Move(tmp, cachePath, overwrite: true);
            return cachePath;
        }

    }
}
