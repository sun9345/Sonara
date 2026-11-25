using Sonara.Core;
using Sonara.Library;

// Валидный 1x1 PNG для проверки записи обложки
byte[] testPng = Convert.FromBase64String(
    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

var repoRoot = Directory.GetCurrentDirectory();
var inputDir = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(repoRoot, "tools", "TagSamples");

if (!Directory.Exists(inputDir))
{
    Directory.CreateDirectory(inputDir);
    Console.WriteLine($"Создана папка: {inputDir}");
    return;
}

string[] targetExts = [".mp3", ".flac", ".wav", ".m4a"];
var files = Directory.GetFiles(inputDir)
    .Where(f => targetExts.Contains(Path.GetExtension(f).ToLowerInvariant()))
    .OrderBy(f => f)
    .ToArray();

if (files.Length == 0)
{
    Console.WriteLine($"В папке '{inputDir}' пока нет файлов .mp3, .flac, .wav или .m4a.");
    Console.WriteLine("Положи туда тестовые файлы и запусти: dotnet run --project tools/Sonara.TagTester");
    return;
}

var tempDir = Path.Combine(Path.GetTempPath(), "SonaraTagTest_" + Guid.NewGuid().ToString("N")[..8]);
Directory.CreateDirectory(tempDir);
Console.WriteLine($"Временная папка для копий: {tempDir}\n");

ITagService service = new AtlTagService();

foreach (var srcFile in files)
{
    var ext = Path.GetExtension(srcFile).ToLowerInvariant();
    var dstFile = Path.Combine(tempDir, Path.GetFileName(srcFile));
    File.Copy(srcFile, dstFile, overwrite: true);

    var sizeBefore = new FileInfo(dstFile).Length;
    Console.WriteLine($"--- {Path.GetFileName(srcFile)} ({ext}) | размер до: {sizeBefore} Б ---");

    try
    {
        var before = await service.ReadAsync(dstFile);
        Console.WriteLine($"  До:    Title='{before.Title}', Artist='{before.Artist}', Album='{before.Album}', " +
                          $"Track#={before.TrackNumber}, AlbumArtist='{before.AlbumArtists}', Year={before.Year}, " +
                          $"Genre='{before.Genre}', Duration={before.Duration:mm\\:ss\\.fff}");

        var updated = new TrackInfo(
            FilePath: dstFile,
            Title: $"Sonara Test [{ext}]",
            Artist: $"Artist [{ext}]",
            Album: $"Album [{ext}]",
            TrackNumber: 5,
            AlbumArtists: $"AlbumArtist [{ext}]",
            Year: 2026,
            Genre: $"Test{ext.TrimStart('.')}",
            Duration: before.Duration,
            Format: before.Format
        );

        await service.WriteAsync(dstFile, updated);
        await service.WriteCoverAsync(dstFile, testPng, "image/png");

        var after = await service.ReadAsync(dstFile);
        var cover = await service.ReadCoverAsync(dstFile);
        var sizeAfter = new FileInfo(dstFile).Length;

        Console.WriteLine($"  После: Title='{after.Title}', Artist='{after.Artist}', Album='{after.Album}', " +
                          $"Track#={after.TrackNumber}, AlbumArtist='{after.AlbumArtists}', Year={after.Year}, " +
                          $"Genre='{after.Genre}', Duration={after.Duration:mm\\:ss\\.fff}");
        Console.WriteLine($"  Обложка: {(cover is { Length: > 0 } ? $"{cover.Length} Б" : "null")} | " +
                          $"Размер после: {sizeAfter} Б | Не пуст: {sizeAfter > 0}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ОШИБКА ({ex.GetType().Name}): {ex.Message}");
    }

    Console.WriteLine();
}
