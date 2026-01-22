using FashionShop.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Services
{
    public class UserContext : IUserContext
    {
        public Guid? UserId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; } = false;
    }
}
