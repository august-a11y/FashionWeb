using FashionShop.Application.Services.DashboardServices.DTO;

namespace FashionShop.Application.Services.DashboardServices
{
    public static class DashboardOverviewCalculator
    {
        public static decimal CalculateChangePct(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current == 0 ? 0 : 100;
            }

            return Math.Round((current - previous) / previous * 100m, 2);
        }

        public static decimal CalculateConversionRate(int completedOrders, int totalOrders)
        {
            if (totalOrders <= 0)
            {
                return 0;
            }

            return Math.Round((decimal)completedOrders / totalOrders * 100m, 2);
        }

        public static (List<decimal> RevenueSeries, List<int> OrderSeries) BuildSeries(
            int bucketCount,
            IReadOnlyDictionary<int, decimal> revenueByBucket,
            IReadOnlyDictionary<int, int> orderByBucket)
        {
            var revenueSeries = new List<decimal>(bucketCount);
            var orderSeries = new List<int>(bucketCount);

            for (var i = 0; i < bucketCount; i++)
            {
                revenueSeries.Add(revenueByBucket.TryGetValue(i, out var revenue) ? revenue : 0m);
                orderSeries.Add(orderByBucket.TryGetValue(i, out var orderCount) ? orderCount : 0);
            }

            return (revenueSeries, orderSeries);
        }

        public static List<string> BuildLabels(DashboardPeriod period, DateTime startUtc)
        {
            return period switch
            {
                DashboardPeriod.Day => Enumerable.Range(0, 24)
                    .Select(hour => $"{hour:00}:00")
                    .ToList(),
                DashboardPeriod.Month => Enumerable.Range(0, 30)
                    .Select(day => startUtc.AddDays(day).ToString("dd/MM"))
                    .ToList(),
                DashboardPeriod.Year => Enumerable.Range(0, 12)
                    .Select(month => startUtc.AddMonths(month).ToString("MM/yyyy"))
                    .ToList(),
                _ => new List<string>()
            };
        }
    }
}
