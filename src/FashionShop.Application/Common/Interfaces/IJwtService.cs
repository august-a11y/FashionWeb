using FashionShop.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Common.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(AppUser  user , IList<string> roleOfUser);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
