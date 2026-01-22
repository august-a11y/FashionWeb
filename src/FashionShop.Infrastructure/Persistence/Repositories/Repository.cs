using FashionShop.Domain.Common;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;

namespace FashionShop.Infrastructure.Persistence.Repositories
{
    public class Repository<T, Key> : IRepository<T, Key> where T : class
    {
        protected readonly ApplicationDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public void Add(T Entity)
        {
            _dbSet.Add(Entity);
        }

        public void AddRange(IEnumerable<T> Entities)
        {
            _dbSet.AddRange(Entities);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbSet.FindAsync(id, cancellationToken);
        }

        public async Task<List<T>> GetListByIdsAsync(List<Guid> ids, CancellationToken cancellationToken)
        {
            return await _dbSet.Where(e => ids.Contains(EF.Property<Guid>(e, "Id"))).ToListAsync(cancellationToken);
        }

        public void Remove(T Entity)
        {
            _dbSet.Remove(Entity);
        }

        public void RemoveRange(IEnumerable<T> Entities)
        {
            _dbSet.RemoveRange(Entities);
        }
    }
}
