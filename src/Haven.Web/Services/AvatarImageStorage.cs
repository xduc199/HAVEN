using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Haven.Web.Services;

public sealed class AvatarImageStorage
{
    public const long MaximumLength = 5 * 1024 * 1024;
    private readonly string _root;

    public AvatarImageStorage(IWebHostEnvironment environment)
    {
        _root = Path.GetFullPath(Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "avatars"));
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Length is <= 0 or > MaximumLength) throw new InvalidDataException("Ảnh đại diện phải có dung lượng dưới 5 MB.");

        var buffer = new byte[checked((int)file.Length)];
        await using (var input = file.OpenReadStream())
        {
            var read = 0;
            while (read < buffer.Length)
            {
                var count = await input.ReadAsync(buffer.AsMemory(read), cancellationToken);
                if (count == 0) break;
                read += count;
            }
            if (read != buffer.Length) throw new InvalidDataException("Không thể đọc tệp ảnh.");
        }

        var extension = DetectImageExtension(buffer);
        if (extension is null || !MimeMatches(file.ContentType, extension))
            throw new InvalidDataException("Hãy chọn ảnh JPEG, PNG hoặc WebP hợp lệ.");

        Directory.CreateDirectory(_root);
        var key = $"{Guid.NewGuid():N}{extension}";
        var path = Resolve(key);
        await File.WriteAllBytesAsync(path, buffer, cancellationToken);
        return $"/uploads/avatars/{key}";
    }

    public Task DeleteAsync(string? avatarPath)
    {
        if (string.IsNullOrWhiteSpace(avatarPath)) return Task.CompletedTask;
        var match = Regex.Match(avatarPath, @"^/uploads/avatars/(?<key>[a-fA-F0-9]{32}\.(jpg|png|webp))$");
        if (match.Success)
        {
            var path = Resolve(match.Groups["key"].Value);
            if (File.Exists(path)) File.Delete(path);
        }
        return Task.CompletedTask;
    }

    private string Resolve(string key)
    {
        if (Path.GetFileName(key) != key || !Regex.IsMatch(key, @"^[a-fA-F0-9]{32}\.(jpg|png|webp)$"))
            throw new InvalidDataException("Invalid avatar key.");
        var path = Path.GetFullPath(Path.Combine(_root, key));
        if (!path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Invalid avatar path.");
        return path;
    }

    private static string? DetectImageExtension(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 8 && data[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return ".png";
        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF) return ".jpg";
        if (data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && data.Slice(8, 4).SequenceEqual("WEBP"u8)) return ".webp";
        return null;
    }

    private static bool MimeMatches(string? mime, string extension) =>
        extension switch
        {
            ".jpg" => mime is "image/jpeg" or "image/pjpeg",
            ".png" => mime == "image/png",
            ".webp" => mime == "image/webp",
            _ => false
        };
}
