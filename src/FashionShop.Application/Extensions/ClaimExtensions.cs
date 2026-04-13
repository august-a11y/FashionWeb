using FashionShop.Application.Services.AuthServices.Models;
using FashionShop.Domain.Identity;
using System.ComponentModel;
using System.Reflection;

namespace FashionShop.Application.Extensions
{
    public static class ClaimExtensions
    {
        public static void GetPermissions(this List<RoleClaimsDTO> allPermissions, Type policy)
        {
            FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
                string displayName = field.GetValue(null).ToString();
                var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if(attributes.Length > 0)
                {
                    var descriptionAttribute = (DescriptionAttribute)attributes[0];
                    displayName = descriptionAttribute.Description;
                }
                allPermissions.Add(new RoleClaimsDTO { Value = field.GetValue(null).ToString(), Type = "Permission", DisplayName = displayName });
            }
        }


        public static async Task AddPermissionClaim<TRole>(this Microsoft.AspNetCore.Identity.RoleManager<AppRole> roleManager, AppRole role, string permission)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
            {
                await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", permission));
            }
        }
    }
}
