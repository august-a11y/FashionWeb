using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
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

       

        public async Task<Cart?> GetCartWithItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Carts
        .Include(c => c.CartItems) 
        .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }
    }
}
