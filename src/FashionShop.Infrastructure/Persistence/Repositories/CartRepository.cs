using FashionShop.Application.Interfaces;
using FashionShop.Domain.Entities;
using FashionShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Persistence.Repositories
{
    public class CartRepository : Repository<Cart, Guid>, ICartRepository
    {
        public  CartRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

       

        public async Task<Cart?> GetCartWithItemsByUserIdAsync(Guid? userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Carts
        .Include(c => c.Items) 
        .ThenInclude(i => i.Variant)
        .ThenInclude(v => v.Product)
        .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }
    }
}
