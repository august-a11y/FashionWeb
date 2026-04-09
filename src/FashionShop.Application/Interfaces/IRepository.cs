using FashionShop.Application.Common.DTOs;
using System.Linq.Expressions;
using Ardalis.Specification;
using FashionShop.Domain.Common;

namespace FashionShop.Application.Interfaces
{
    public interface IRepository<T> : IRepositoryBase<T> where T : BaseEntity
    {
        Task<List<T>> GetListByIdsAsync(List<Guid> ids, CancellationToken cancellationToken);

        Task<PageResponse<T>> PagedListAsync(
            ISpecification<T> specification,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken);

  
    }
}
