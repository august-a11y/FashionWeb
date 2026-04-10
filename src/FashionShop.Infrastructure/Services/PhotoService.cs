using FashionShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FashionShop.Infrastructure.Services
{
    public class LocalPhotoService : IPhotoService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        public async Task<string> UploadPhotoAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only .jpg, .jpeg, .png, .webp files are allowed.");

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid content type. File must be an image.");

            var safeFolderName = string.IsNullOrWhiteSpace(folderName) ? "common" : folderName.Trim().ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "img", safeFolderName);

            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/img/{safeFolderName}/{fileName}";
        }

        public Task<bool> DeletePhotoAsync(string photoUrl)
        {
            if (string.IsNullOrWhiteSpace(photoUrl))
                return Task.FromResult(false);

            var relativePath = photoUrl;

            if (Uri.TryCreate(photoUrl, UriKind.Absolute, out var absoluteUri))
                relativePath = absoluteUri.AbsolutePath;

            relativePath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
            var imgRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "img"));

            if (!fullPath.StartsWith(imgRoot, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            if (!File.Exists(fullPath))
                return Task.FromResult(false);

            File.Delete(fullPath);
            return Task.FromResult(true);
        }
    }
}