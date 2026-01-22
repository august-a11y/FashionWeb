using FashionShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public async Task ExecuteTransactionAsync(Func<Task> action, CancellationToken cancellation)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellation);
                try
                {
                    await action();
                    await transaction.CommitAsync(cancellation);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellation);
                    throw;
                }
            });
        }
    }
}
