using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Services.UserServices.DTO;
using FluentResults;

namespace FashionShop.Application.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IIdentityService _identityService;
        private readonly IRequestContext _requestContext;

        public UserService(IIdentityService identityService, IRequestContext requestContext)
        {
            _identityService = identityService;
            _requestContext = requestContext;
        }

        public async Task<Result<UserProfileDTO>> GetMyProfileAsync(CancellationToken cancellationToken)
        {
            if (!_requestContext.UserId.HasValue)
            {
                return Result.Fail<UserProfileDTO>("User is not authenticated.");
            }

            return await _identityService.GetProfileAsync(_requestContext.UserId.Value);
        }

        public async Task<Result<UserProfileDTO>> UpdateMyProfileAsync(
            UpdateUserProfileDTO updateUserProfileDTO,
            CancellationToken cancellationToken)
        {
            if (!_requestContext.UserId.HasValue)
            {
                return Result.Fail<UserProfileDTO>("User is not authenticated.");
            }

            return await _identityService.UpdateProfileAsync(_requestContext.UserId.Value, updateUserProfileDTO);
        }

        public async Task<Result<bool>> ChangeMyPasswordAsync(
            ChangePasswordDTO changePasswordDTO,
            CancellationToken cancellationToken)
        {
            if (!_requestContext.UserId.HasValue)
            {
                return Result.Fail<bool>("User is not authenticated.");
            }

            return await _identityService.ChangePasswordAsync(_requestContext.UserId.Value, changePasswordDTO);
        }
    }
}