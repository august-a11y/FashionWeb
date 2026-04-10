using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Interfaces;
using FashionShop.Application.Services.DashboardServices;
using FashionShop.Application.Services.DashboardServices.DTO;
using FluentResults;

namespace FashionShop.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

        private readonly IDashboardReadRepository _dashboardReadRepository;
        private readonly IRedisCache _cache;

        public DashboardService(IDashboardReadRepository dashboardReadRepository, IRedisCache cache)
        {
            _dashboardReadRepository = dashboardReadRepository;
            _cache = cache;
        }

        public async Task<Result<DashboardOverviewDTO>> GetOverviewAsync(
            DashboardPeriod period,
            DateTime? from,
            DateTime? to,
            int topN,
            int recentLimit,
            CancellationToken cancellationToken)
        {
            topN = topN <= 0 ? 5 : Math.Min(topN, 20);
            recentLimit = recentLimit <= 0 ? 5 : Math.Min(recentLimit, 20);

            var (rangeStartUtc, rangeEndUtc, previousStartUtc, previousEndUtc) = BuildRanges(period, from, to);
            var cacheKey = BuildCacheKey(period, rangeStartUtc, rangeEndUtc, topN, recentLimit);

            try
            {
                var cached = await _cache.GetAsync<DashboardOverviewDTO>(cacheKey, cancellationToken);
                if (cached != null)
                {
                    return Result.Ok(cached);
                }
            }
            catch
            {
            }

            var result = new DashboardOverviewDTO
            {
                Kpis = await BuildKpisAsync(rangeStartUtc, rangeEndUtc, previousStartUtc, previousEndUtc, cancellationToken),
                Trend = await BuildTrendAsync(period, rangeStartUtc, rangeEndUtc, cancellationToken),
                Alerts = await BuildAlertsAsync(cancellationToken),
                TopProducts = await BuildTopProductsAsync(rangeStartUtc, rangeEndUtc, topN, cancellationToken),
                TopCategories = await BuildTopCategoriesAsync(rangeStartUtc, rangeEndUtc, topN, cancellationToken),
                TopCustomers = await BuildTopCustomersAsync(rangeStartUtc, rangeEndUtc, topN, cancellationToken),
                RecentOrders = await BuildRecentOrdersAsync(rangeStartUtc, rangeEndUtc, recentLimit, cancellationToken),
                Meta = new DashboardMetaDTO
                {
                    GeneratedAt = DateTime.UtcNow,
                    Currency = "VND",
                    Timezone = "Asia/Ho_Chi_Minh"
                }
            };

            try
            {
                await _cache.SetAsync(cacheKey, result, CacheTtl, cancellationToken);
            }
            catch
            {
            }

            return Result.Ok(result);
        }

        private async Task<DashboardKpisDTO> BuildKpisAsync(
            DateTime startUtc,
            DateTime endUtc,
            DateTime previousStartUtc,
            DateTime previousEndUtc,
            CancellationToken cancellationToken)
        {
            var raw = await _dashboardReadRepository.GetKpiRawDataAsync(
                startUtc,
                endUtc,
                previousStartUtc,
                previousEndUtc,
                cancellationToken);

            var conversionRate = DashboardOverviewCalculator.CalculateConversionRate(raw.CompletedOrders, raw.TotalOrders);
            var previousConversionRate = DashboardOverviewCalculator.CalculateConversionRate(raw.PreviousCompletedOrders, raw.PreviousOrders);

            return new DashboardKpisDTO
            {
                TotalRevenue = raw.TotalRevenue,
                TotalOrders = raw.TotalOrders,
                NewCustomers = raw.NewCustomers,
                ConversionRate = conversionRate,
                RevenueChangePct = DashboardOverviewCalculator.CalculateChangePct(raw.TotalRevenue, raw.PreviousRevenue),
                OrdersChangePct = DashboardOverviewCalculator.CalculateChangePct(raw.TotalOrders, raw.PreviousOrders),
                NewCustomersChangePct = DashboardOverviewCalculator.CalculateChangePct(raw.NewCustomers, raw.PreviousNewCustomers),
                ConversionChangePct = DashboardOverviewCalculator.CalculateChangePct(conversionRate, previousConversionRate)
            };
        }

        private async Task<DashboardTrendDTO> BuildTrendAsync(
            DashboardPeriod period,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken)
        {
            var rawTrend = await _dashboardReadRepository.GetTrendRawDataAsync(period, startUtc, endUtc, cancellationToken);
            var (revenueSeries, orderSeries) = DashboardOverviewCalculator.BuildSeries(
                rawTrend.BucketCount,
                rawTrend.RevenueByBucket,
                rawTrend.OrderByBucket);

            return new DashboardTrendDTO
            {
                Period = period.ToString(),
                Labels = DashboardOverviewCalculator.BuildLabels(period, rawTrend.LabelStartUtc),
                RevenueSeries = revenueSeries,
                OrderSeries = orderSeries
            };
        }

        private async Task<List<DashboardAlertDTO>> BuildAlertsAsync(CancellationToken cancellationToken)
        {
            var alerts = new List<DashboardAlertDTO>();

            var lowStockProducts = await _dashboardReadRepository.GetLowStockProductsAsync(5, cancellationToken);

            if (lowStockProducts.Count > 0)
            {
                foreach (var item in lowStockProducts)
                {
                    alerts.Add(new DashboardAlertDTO
                    {
                        Title = "Low stock",
                        Description = $"{item.ProductName} chỉ còn {item.Stock} sản phẩm trong kho.",
                        Severity = item.Stock <= 3 ? "high" : "medium"
                    });
                }
            }

            var delayedOrders = await _dashboardReadRepository.CountDelayedOrdersAsync(DateTime.UtcNow, cancellationToken);

            if (delayedOrders > 0)
            {
                alerts.Add(new DashboardAlertDTO
                {
                    Title = "Delayed orders",
                    Description = $"Có {delayedOrders} đơn hàng bị trễ giao.",
                    Severity = delayedOrders >= 10 ? "high" : "medium"
                });
            }

            var pendingRefund = await _dashboardReadRepository.CountPendingRefundOrdersAsync(cancellationToken);

            if (pendingRefund > 0)
            {
                alerts.Add(new DashboardAlertDTO
                {
                    Title = "Pending refund",
                    Description = $"Có {pendingRefund} đơn hàng đang chờ hoàn tiền.",
                    Severity = pendingRefund >= 5 ? "high" : "low"
                });
            }

            return alerts;
        }

        private async Task<List<DashboardTopProductDTO>> BuildTopProductsAsync(
            DateTime startUtc,
            DateTime endUtc,
            int topN,
            CancellationToken cancellationToken)
        {
            var rows = await _dashboardReadRepository.GetTopProductsAsync(startUtc, endUtc, topN, cancellationToken);

            return rows.Select(x => new DashboardTopProductDTO
            {
                Id = x.ProductId.ToString(),
                Name = x.Name,
                UnitsSold = x.UnitsSold,
                Revenue = x.Revenue,
                Stock = x.Stock
            }).ToList();
        }

        private async Task<List<DashboardTopCategoryDTO>> BuildTopCategoriesAsync(
            DateTime startUtc,
            DateTime endUtc,
            int topN,
            CancellationToken cancellationToken)
        {
            var rows = await _dashboardReadRepository.GetTopCategoriesAsync(startUtc, endUtc, topN, cancellationToken);

            return rows.Select(x => new DashboardTopCategoryDTO
            {
                Id = x.CategoryId.ToString(),
                Name = x.Name,
                UnitsSold = x.UnitsSold,
                Revenue = x.Revenue
            }).ToList();
        }

        private async Task<List<DashboardTopCustomerDTO>> BuildTopCustomersAsync(
            DateTime startUtc,
            DateTime endUtc,
            int topN,
            CancellationToken cancellationToken)
        {
            var rows = await _dashboardReadRepository.GetTopCustomersAsync(startUtc, endUtc, topN, cancellationToken);

            return rows.Select(x => new DashboardTopCustomerDTO
            {
                CustomerId = x.UserId.ToString(),
                Customer = BuildCustomerName(x.FirstName, x.LastName, x.Email),
                TotalSpent = x.TotalSpent,
                OrderCount = x.OrderCount
            }).ToList();
        }

        private async Task<List<DashboardRecentOrderDTO>> BuildRecentOrdersAsync(
            DateTime startUtc,
            DateTime endUtc,
            int recentLimit,
            CancellationToken cancellationToken)
        {
            var rows = await _dashboardReadRepository.GetRecentOrdersAsync(startUtc, endUtc, recentLimit, cancellationToken);

            return rows.Select(x => new DashboardRecentOrderDTO
            {
                Id = x.OrderId.ToString(),
                Code = x.Code,
                Customer = BuildCustomerName(x.FirstName, x.LastName, x.Email),
                Total = x.Total,
                PaymentMethod = x.PaymentMethod,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        private static string BuildCacheKey(DashboardPeriod period, DateTime fromUtc, DateTime toUtc, int topN, int recentLimit)
        {
            return $"dashboard:overview:{period}:{fromUtc:O}:{toUtc:O}:{topN}:{recentLimit}";
        }

        private static (DateTime CurrentStart, DateTime CurrentEnd, DateTime PreviousStart, DateTime PreviousEnd) BuildRanges(
            DashboardPeriod period,
            DateTime? from,
            DateTime? to)
        {
            if (from.HasValue && to.HasValue)
            {
                var f = EnsureUtc(from.Value);
                var t = EnsureUtc(to.Value);

                if (t <= f)
                {
                    t = f.AddDays(1);
                }

                var duration = t - f;
                return (f, t, f - duration, f);
            }

            var now = DateTime.UtcNow;

            return period switch
            {
                DashboardPeriod.Day => BuildDayRange(now),
                DashboardPeriod.Year => BuildYearRange(now),
                _ => BuildMonthRange(now)
            };
        }

        private static (DateTime CurrentStart, DateTime CurrentEnd, DateTime PreviousStart, DateTime PreviousEnd) BuildDayRange(DateTime now)
        {
            var start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddDays(1);
            var prevStart = start.AddDays(-1);
            return (start, end, prevStart, start);
        }

        private static (DateTime CurrentStart, DateTime CurrentEnd, DateTime PreviousStart, DateTime PreviousEnd) BuildMonthRange(DateTime now)
        {
            var end = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
            var start = end.AddDays(-30);
            var prevStart = start.AddDays(-30);
            return (start, end, prevStart, start);
        }

        private static (DateTime CurrentStart, DateTime CurrentEnd, DateTime PreviousStart, DateTime PreviousEnd) BuildYearRange(DateTime now)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var start = monthStart.AddMonths(-11);
            var end = monthStart.AddMonths(1);
            var prevStart = start.AddMonths(-12);
            return (start, end, prevStart, start);
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
            {
                return value;
            }

            return value.ToUniversalTime();
        }

        private static string BuildCustomerName(string? firstName, string? lastName, string? email)
        {
            var fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                return fullName;
            }

            return string.IsNullOrWhiteSpace(email) ? "Unknown customer" : email;
        }
    }
}
