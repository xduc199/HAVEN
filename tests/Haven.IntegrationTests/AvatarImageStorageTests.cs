using Haven.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Haven.IntegrationTests;

public sealed class AvatarImageStorageTests
{
    [Fact]
    public async Task Saves_signature_checked_image_with_server_generated_public_key()
    {
        var root = Path.Combine(Path.GetTempPath(), $"haven-avatar-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var storage = new AvatarImageStorage(new TestEnvironment(root));
            var file = CreateFile(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0 }, "image/png", "my-photo.png");
            var path = await storage.SaveAsync(file, CancellationToken.None);
            Assert.Matches("^/uploads/avatars/[a-f0-9]{32}\\.png$", path);
            Assert.True(File.Exists(Path.Combine(root, "uploads", "avatars", Path.GetFileName(path))));
            await storage.DeleteAsync(path);
            Assert.False(File.Exists(Path.Combine(root, "uploads", "avatars", Path.GetFileName(path))));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Rejects_mismatched_mime_and_oversized_files()
    {
        var root = Path.Combine(Path.GetTempPath(), $"haven-avatar-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var storage = new AvatarImageStorage(new TestEnvironment(root));
            await Assert.ThrowsAsync<InvalidDataException>(() => storage.SaveAsync(
                CreateFile(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, "image/jpeg", "fake.jpg"),
                CancellationToken.None));
            await Assert.ThrowsAsync<InvalidDataException>(() => storage.SaveAsync(
                CreateFile(new byte[AvatarImageStorage.MaximumLength + 1], "image/png", "large.png"),
                CancellationToken.None));
        }
        finally { Directory.Delete(root, true); }
    }

    private static FormFile CreateFile(byte[] content, string contentType, string name)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "avatar", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "HAVEN.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = root;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
