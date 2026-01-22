using AutoMapper;
using FashionShop.Application.Dtos;
using FashionShop.Domain.Identity;
using FashionShop.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.User.Queries
{
    public class CreateUserCommand : IRequest<bool>
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, bool>
    {
       
        public readonly IMapper _mapper;
        public readonly UserManager<AppUser> _userManager;


        public CreateUserCommandHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
           
        }
        public async Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _userManager.FindByNameAsync(request.UserName!);

            if (userExists != null)
            {
                throw new Exception("User already exists!");
            }

            var emailExists = await _userManager.FindByEmailAsync(request.Email!);
            if (emailExists != null)
            {
                throw new Exception("Email already exists!");
            }

            
            var user = new AppUser
            {
                Email = request.Email,
                NormalizedEmail = request.Email!.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = request.UserName,
                NormalizedUserName = request.UserName!.ToUpper(),
                FirstName = request.FirstName,
                LastName = request.LastName,  
                PhoneNumber = request.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, request.Password!);
            
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Register Fail: {errors}");
            }
            await _userManager.AddToRoleAsync(user, "User");
            return true;
        }
    }
}
