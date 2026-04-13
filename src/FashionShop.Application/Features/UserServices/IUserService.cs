using FashionShop.Application.Services.UserServices.DTO;
using FluentResults;

namespace FashionShop.Application.Services.UserServices
{
    public interface IUserService
    {
        Task<Result<UserProfileDTO>> GetMyProfileAsync(CancellationToken cancellationToken);
        Task<Result<UserProfileDTO>> UpdateMyProfileAsync(UpdateUserProfileDTO updateUserProfileDTO, CancellationToken cancellationToken);
        Task<Result<bool>> ChangeMyPasswordAsync(ChangePasswordDTO changePasswordDTO, CancellationToken cancellationToken);
    }
}