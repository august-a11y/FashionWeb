using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Interfaces
{
    public interface ICacheCartRepository
    {
        Task<Cart?> GetCartWithItemsByUserId(Guid? userId, CancellationToken cancellationToken);
    }
}
