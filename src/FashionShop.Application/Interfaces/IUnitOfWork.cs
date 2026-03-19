namespace FashionShop.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync(CancellationToken cancellationToken);
        void Dispose();
        Task ExecuteTransactionAsync(Func<Task> action, CancellationToken cancellation);
    }
}
