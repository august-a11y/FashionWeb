using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync(CancellationToken cancellationToken);
        void Dispose();
        Task ExecuteTransactionAsync(Func<Task> action, CancellationToken cancellation);
    }
}
