using AutoMapper;
using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Dtos;
using FashionShop.Application.ResponseDtos;
using FashionShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.User.Commands
{
    public class LoginUserCommand : IRequest<UserResponseDto>
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, UserResponseDto>
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        public LoginUserCommandHandler(IMapper mapper, IJwtService jwtService, UserManager<AppUser> userManager)
        {
            _mapper = mapper;
            _jwtService = jwtService;
            _userManager = userManager;
        }
        public async Task<UserResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is inactive.");
            }
            if(await _userManager.IsLockedOutAsync(user))
            {
                throw new UnauthorizedAccessException("User account is locked.");
            }
            var rolesOfUser = await _userManager.GetRolesAsync(user);
            var token = await _jwtService.GenerateTokenAsync(user, rolesOfUser);
            var result = _mapper.Map<UserResponseDto>(user);
            result.Token = token;
            return result;

        }
    }
}
