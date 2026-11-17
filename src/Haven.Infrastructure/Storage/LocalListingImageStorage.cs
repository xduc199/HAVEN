using Haven.Application.Abstractions;

namespace Haven.Infrastructure.Storage;

public sealed class LocalListingImageStorage : IListingImageStorage, IListingVideoStorage
{
    private readonly string _root;
    public LocalListingImageStorage(string contentRootPath, string? configuredPath)
    {
        _root = Path.GetFullPath(string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(contentRootPath, "App_Data", "ListingImages")
            : Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(contentRootPath, configuredPath));
    }

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        if (extension is not (".jpg" or ".png" or ".webp")) throw new ArgumentException("Unsupported image extension.", nameof(extension));
        Directory.CreateDirectory(_root);
        var key = $"{Guid.NewGuid():N}{extension}";
        var target = Resolve(key);
        var temporary = target + ".tmp";
        try
        {
            await using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.WriteThrough))
                await content.CopyToAsync(file, cancellationToken);
            File.Move(temporary, target);
            return key;
        }
        catch
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); } catch { /* preserve the original write error */ }
            throw;
        }
    }

    public async Task<string> SaveAsync(Stream content, string extension, long maximumLength, long expectedLength, CancellationToken cancellationToken = default)
    {
        if (extension is not (".mp4" or ".webm")) throw new ArgumentException("Unsupported video extension.", nameof(extension));
        if (maximumLength <= 0 || expectedLength <= 0 || expectedLength > maximumLength) throw new ArgumentOutOfRangeException(nameof(expectedLength));
        Directory.CreateDirectory(_root);
        var key = $"{Guid.NewGuid():N}{extension}";
        var target = Resolve(key);
        var temporary = target + ".tmp";
        try
        {
            await using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                var buffer = new byte[81920];
                long copied = 0;
                int read;
                while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    copied += read;
                    if (copied > maximumLength || copied > expectedLength) throw new InvalidDataException("Video stream exceeds its declared size.");
                    await file.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }
                if (copied != expectedLength) throw new InvalidDataException("Video stream length does not match its declared size.");
            }
            File.Move(temporary, target);
            return key;
        }
        catch
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); } catch { /* preserve the original write error */ }
            throw;
        }
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(Resolve(storageKey), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Resolve(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string Resolve(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key != Path.GetFileName(key) || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(key), "N", out _) || Path.GetExtension(key) is not (".jpg" or ".png" or ".webp" or ".mp4" or ".webm"))
            throw new ArgumentException("Invalid media storage key.", nameof(key));
        return Path.Combine(_root, key);
    }
}
