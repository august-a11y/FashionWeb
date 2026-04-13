using FashionShop.Application.Services.DashboardServices.DTO;
using FluentResults;

namespace FashionShop.Application.Services.DashboardServices
{
    public interface IDashboardService
    {
        Task<Result<DashboardOverviewDTO>> GetOverviewAsync(
            DashboardPeriod period,
            DateTime? from,
            DateTime? to,
            int topN,
            int recentLimit,
            CancellationToken cancellationToken);
    }
}
