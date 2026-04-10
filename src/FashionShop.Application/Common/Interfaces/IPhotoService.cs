using Microsoft.AspNetCore.Http;

namespace FashionShop.Application.Common.Interfaces
{
    public interface IPhotoService
    {
        Task<string> UploadPhotoAsync(IFormFile file, string folderName);

        Task<bool> DeletePhotoAsync(string photoUrl);
    }
}