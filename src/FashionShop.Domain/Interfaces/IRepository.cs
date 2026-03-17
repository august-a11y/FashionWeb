using FashionShop.Domain.Common;
using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Interfaces
{
    public interface IRepository<T, Key> where T : class
    {
      
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);

        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<T>> GetListByIdsAsync(List<Guid> ids, CancellationToken cancellationToken);
          
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        void Add(T Entity);
        
        void AddRange(IEnumerable<T> Entities);
        void RemoveRange(IEnumerable<T> Entities);
        void Remove(T Entity);


        public void Save(T Entity);


    }
}
