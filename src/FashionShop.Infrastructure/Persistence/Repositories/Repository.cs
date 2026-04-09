using FashionShop.Application.Common.DTOs;
using FashionShop.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using FashionShop.Domain.Common;

namespace FashionShop.Infrastructure.Persistence.Repositories;

public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _dbContext;


    public Repository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;

    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }


    public async Task<List<T>> GetListByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>()
            .Where(e => ids.Contains(e.Id)) 
            .ToListAsync(cancellationToken);
    }


    public async Task<PageResponse<T>> PagedListAsync(
        Ardalis.Specification.ISpecification<T> specification,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var total = await CountAsync(specification, cancellationToken);
        if (total == 0) return new PageResponse<T>(new List<T>(), 0, pageSize, pageIndex);

        var items = await ListAsync(specification, cancellationToken);
       return new PageResponse<T>(items, total, pageSize, pageIndex);
    }

}
