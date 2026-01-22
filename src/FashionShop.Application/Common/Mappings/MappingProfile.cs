using AutoMapper;
using FashionShop.Application.RequestDtos;
using FashionShop.Application.ResponseDtos;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<AppUser, RegisterUserDto>().ReverseMap();
            CreateMap<AppUser, ProfileUserDto>().ReverseMap();
            CreateMap<AppUser, UserResponseDto>().ReverseMap();
        }
    }
}
