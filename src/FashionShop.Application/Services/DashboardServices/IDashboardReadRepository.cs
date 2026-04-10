using FashionShop.Application.Services.DashboardServices.DTO;

namespace FashionShop.Application.Services.DashboardServices
{
    public interface IDashboardReadRepository
    {
        Task<DashboardKpiRawData> GetKpiRawDataAsync(
            DateTime startUtc,
            DateTime endUtc,
            DateTime previousStartUtc,
            DateTime previousEndUtc,
            CancellationToken cancellationToken);

        Task<DashboardTrendRawData> GetTrendRawDataAsync(
            DashboardPeriod period,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken);

        Task<List<DashboardLowStockProductRow>> GetLowStockProductsAsync(int take, CancellationToken cancellationToken);

        Task<int> CountDelayedOrdersAsync(DateTime nowUtc, CancellationToken cancellationToken);

        Task<int> CountPendingRefundOrdersAsync(CancellationToken cancellationToken);

        Task<List<DashboardTopProductRow>> GetTopProductsAsync(
            DateTime startUtc,
            DateTime endUtc,
            int topN,
            CancellationToken cancellationToken);

        Task<List<DashboardTopCategoryRow>> GetTopCategoriesAsync(
            DateTime startUtc,
            DateTime endUtc,
            int topN,
            CancellationToken cancellationToken);

        Task<List<DashboardTopCustomerRow>> GetTopCustomersAsync(
            DateTime startUtc,
            DateTime endUtc,
            int topN,
            CancellationToken cancellationToken);

        Task<List<DashboardRecentOrderRow>> GetRecentOrdersAsync(
            DateTime startUtc,
            DateTime endUtc,
            int recentLimit,
            CancellationToken cancellationToken);
    }
}
