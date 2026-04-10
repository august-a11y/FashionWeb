using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using FashionShop.Application.Services.DashboardServices;
using FashionShop.Application.Services.DashboardServices.DTO;
using FashionShop.Application.Services.DashboardServices.Specifications;
using FashionShop.Domain.Entities;
using FashionShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Infrastructure.Persistence.Repositories
{
    public class DashboardReadRepository : IDashboardReadRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DashboardReadRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardKpiRawData> GetKpiRawDataAsync(
    DateTime startUtc,
    DateTime endUtc,
    DateTime previousStartUtc,
    DateTime previousEndUtc,
    CancellationToken cancellationToken)
        {
            var currentOrders = ApplySpecification(_dbContext.Orders.AsNoTracking(), new DashboardOrdersInRangeSpec(startUtc, endUtc));
            var previousOrders = ApplySpecification(_dbContext.Orders.AsNoTracking(), new DashboardOrdersInRangeSpec(previousStartUtc, previousEndUtc));

            // Dùng 'await' trực tiếp để lấy kết quả tuần tự
            var totalRevenue = await currentOrders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount, cancellationToken);

            var totalOrders = await currentOrders.CountAsync(cancellationToken);

            var completedOrders = await currentOrders
                .CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);

            var previousRevenue = await previousOrders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount, cancellationToken);

            var previousOrdersCount = await previousOrders.CountAsync(cancellationToken);

            var previousCompletedOrders = await previousOrders
                .CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);

            var firstOrderDates = _dbContext.Orders.AsNoTracking()
                .GroupBy(o => o.UserId)
                .Select(g => g.Min(x => x.CreatedAt));

            var newCustomers = await firstOrderDates
                .CountAsync(firstDate => firstDate >= startUtc && firstDate < endUtc, cancellationToken);

            var previousNewCustomers = await firstOrderDates
                .CountAsync(firstDate => firstDate >= previousStartUtc && firstDate < previousEndUtc, cancellationToken);

            return new DashboardKpiRawData
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                PreviousRevenue = previousRevenue,
                PreviousOrders = previousOrdersCount,
                PreviousCompletedOrders = previousCompletedOrders,
                NewCustomers = newCustomers,
                PreviousNewCustomers = previousNewCustomers
            };
        }

        public async Task<DashboardTrendRawData> GetTrendRawDataAsync(
            DashboardPeriod period,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken)
        {
            var baseQuery = ApplySpecification(_dbContext.Orders.AsNoTracking(), new DashboardOrdersInRangeSpec(startUtc, endUtc));

            Dictionary<int, decimal> revenueByBucket;
            Dictionary<int, int> orderByBucket;
            int bucketCount;
            var labelStartUtc = startUtc;

            switch (period)
            {
                case DashboardPeriod.Day:
                {
                    bucketCount = 24;

                    var revenueRows = await baseQuery
                        .Where(o => o.Status != OrderStatus.Cancelled)
                        .GroupBy(o => EF.Functions.DateDiffHour(startUtc, o.CreatedAt))
                        .Select(g => new { Bucket = g.Key, Revenue = g.Sum(x => x.TotalAmount) })
                        .ToListAsync(cancellationToken);

                    var orderRows = await baseQuery
                        .GroupBy(o => EF.Functions.DateDiffHour(startUtc, o.CreatedAt))
                        .Select(g => new { Bucket = g.Key, Count = g.Count() })
                        .ToListAsync(cancellationToken);

                    revenueByBucket = revenueRows
                        .Where(x => x.Bucket >= 0 && x.Bucket < bucketCount)
                        .ToDictionary(x => x.Bucket, x => x.Revenue);

                    orderByBucket = orderRows
                        .Where(x => x.Bucket >= 0 && x.Bucket < bucketCount)
                        .ToDictionary(x => x.Bucket, x => x.Count);

                    break;
                }
                case DashboardPeriod.Year:
                {
                    bucketCount = 12;
                    var monthStart = new DateTime(startUtc.Year, startUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    labelStartUtc = monthStart;

                    var revenueRows = await baseQuery
                        .Where(o => o.Status != OrderStatus.Cancelled)
                        .GroupBy(o => EF.Functions.DateDiffMonth(monthStart, o.CreatedAt))
                        .Select(g => new { Bucket = g.Key, Revenue = g.Sum(x => x.TotalAmount) })
                        .ToListAsync(cancellationToken);

                    var orderRows = await baseQuery
                        .GroupBy(o => EF.Functions.DateDiffMonth(monthStart, o.CreatedAt))
                        .Select(g => new { Bucket = g.Key, Count = g.Count() })
                        .ToListAsync(cancellationToken);

                    revenueByBucket = revenueRows
                        .Where(x => x.Bucket >= 0 && x.Bucket < bucketCount)
                        .ToDictionary(x => x.Bucket, x => x.Revenue);

                    orderByBucket = orderRows
                        .Where(x => x.Bucket >= 0 && x.Bucket < bucketCount)
                        .ToDictionary(x => x.Bucket, x => x.Count);

                    break;
                }
                default:
                {
                    bucketCount = 30;

                    var revenueRows = await baseQuery
                        .Where(o => o.Status != OrderStatus.Cancelled)
                        .GroupBy(o => EF.Functions.DateDiffDay(startUtc, o.CreatedAt))
                        .Select(g => new { Bucket = g.Key, Revenue = g.Sum(x => x.TotalAmount) })
                        .ToListAsync(cancellationToken);

                    var orderRows = await baseQuery
                        .GroupBy(o => EF.Functions.DateDiffDay(startUtc, o.CreatedAt))
                        .Select(g => new { Bucket = g.Key, Count = g.Count() })
                        .ToListAsync(cancellationToken);

                    revenueByBucket = revenueRows
                        .Where(x => x.Bucket >= 0 && x.Bucket < bucketCount)
                        .ToDictionary(x => x.Bucket, x => x.Revenue);

                    orderByBucket = orderRows
                        .Where(x => x.Bucket >= 0 && x.Bucket < bucketCount)
                        .ToDictionary(x => x.Bucket, x => x.Count);

                    break;
                }
            }

            return new DashboardTrendRawData
            {
                BucketCount = bucketCount,
                LabelStartUtc = labelStartUtc,
                RevenueByBucket = revenueByBucket,
                OrderByBucket = orderByBucket
            };
        }

        public async Task<List<DashboardLowStockProductRow>> GetLowStockProductsAsync(int take, CancellationToken cancellationToken)
        {
            return await _dbContext.ProductVariants.AsNoTracking()
                .GroupBy(v => new { v.ProductId, v.Product.Name })
                .Select(g => new DashboardLowStockProductRow
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Stock = g.Sum(x => x.StockQuantity)
                })
                .Where(x => x.Stock <= 10)
                .OrderBy(x => x.Stock)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public Task<int> CountDelayedOrdersAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            var delayedQuery = ApplySpecification(_dbContext.Orders.AsNoTracking(), new DashboardDelayedOrdersSpec(nowUtc));
            return delayedQuery.CountAsync(cancellationToken);
        }

        public Task<int> CountPendingRefundOrdersAsync(CancellationToken cancellationToken)
        {
            var pendingRefundQuery = ApplySpecification(_dbContext.Orders.AsNoTracking(), new DashboardPendingRefundOrdersSpec());
            return pendingRefundQuery.CountAsync(cancellationToken);
        }

        public async Task<List<DashboardTopProductRow>> GetTopProductsAsync(
    DateTime startUtc,
    DateTime endUtc,
    int topN,
    CancellationToken cancellationToken)
        {
            // BƯỚC 1: Tìm danh sách OrderId hợp lệ
            var validOrderIds = ApplySpecification(_dbContext.Orders.AsNoTracking(),
                new DashboardOrdersInRangeSpec(startUtc, endUtc, excludeCancelled: true))
                .Select(o => o.Id);

            // BƯỚC 2: Truy vấn danh sách Top Sản Phẩm Bán Chạy (Chưa có Stock)
            var topProducts = await _dbContext.OrderItems.AsNoTracking()
                .Where(oi => validOrderIds.Contains(oi.OrderId))
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new DashboardTopProductRow
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.ProductName,
                    UnitsSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(topN)
                .ToListAsync(cancellationToken);

            // BƯỚC 3: Nếu không có data, thoát sớm để tối ưu
            if (!topProducts.Any())
            {
                return topProducts;
            }

            // BƯỚC 4: Chỉ lấy Tồn kho (Stock) cho đúng những sản phẩm nằm trong Top
            var productIds = topProducts.Select(p => p.ProductId).ToList();

            var stockDictionary = await _dbContext.ProductVariants.AsNoTracking()
                .Where(v => productIds.Contains(v.ProductId))
                .GroupBy(v => v.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalStock = g.Sum(x => x.StockQuantity)
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalStock, cancellationToken);

            // BƯỚC 5: Gép dữ liệu Tồn kho vào danh sách kết quả (Thực hiện trên RAM)
            foreach (var product in topProducts)
            {
                product.Stock = stockDictionary.GetValueOrDefault(product.ProductId, 0);
            }

            return topProducts;
        }

        public async Task<List<DashboardTopCategoryRow>> GetTopCategoriesAsync(
            DateTime startUtc,
            DateTime endUtc,
            int topN,
            CancellationToken cancellationToken)
        {
            var validOrderIds = ApplySpecification(_dbContext.Orders.AsNoTracking(), new DashboardOrdersInRangeSpec(startUtc, endUtc, excludeCancelled: true))
                .Select(o => o.Id);

            return await (from oi in _dbContext.OrderItems.AsNoTracking()
                          join p in _dbContext.Products.AsNoTracking() on oi.ProductId equals p.Id
                          join c in _dbContext.Categories.AsNoTracking() on p.CategoryId equals c.Id
                          where validOrderIds.Contains(oi.OrderId)
                          group oi by new { c.Id, c.Name }
                into g
                          orderby g.Sum(x => x.Quantity) descending
                          select new DashboardTopCategoryRow
                          {
                              CategoryId = g.Key.Id,
                              Name = g.Key.Name,
                              UnitsSold = g.Sum(x => x.Quantity),
                              Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
                          })
                .Take(topN)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<DashboardTopCustomerRow>> GetTopCustomersAsync(
            DateTime startUtc,
            DateTime endUtc,
            int topN,
            CancellationToken cancellationToken)
        {
            var validOrders = ApplySpecification(_dbContext.Orders.AsNoTracking(), new DashboardOrdersInRangeSpec(startUtc, endUtc, excludeCancelled: true));

            return await (from o in validOrders
                          join user in _dbContext.AppUsers.AsNoTracking() on o.UserId equals user.Id
                          group new { o, user } by new { o.UserId, user.FirstName, user.LastName, user.Email }
                into g
                          orderby g.Sum(x => x.o.TotalAmount) descending
                          select new DashboardTopCustomerRow
                          {
                              UserId = g.Key.UserId.HasValue ? g.Key.UserId.Value : Guid.Empty,
                              FirstName = g.Key.FirstName,
                              LastName = g.Key.LastName,
                              Email = g.Key.Email,
                              TotalSpent = g.Sum(x => x.o.TotalAmount),
                              OrderCount = g.Count()
                          })
                .Take(topN)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<DashboardRecentOrderRow>> GetRecentOrdersAsync(
            DateTime startUtc,
            DateTime endUtc,
            int recentLimit,
            CancellationToken cancellationToken)
        {
            var orders = await ApplySpecification(_dbContext.Orders.AsNoTracking(), new DashboardRecentOrdersSpec(startUtc, endUtc, recentLimit))
                .ToListAsync(cancellationToken);

            return orders.Select(o => new DashboardRecentOrderRow
            {
                OrderId = o.Id,
                Code = o.OrderCode,
                FirstName = o.User?.FirstName,
                LastName = o.User?.LastName,
                Email = o.User?.Email,
                Total = o.TotalAmount,
                PaymentMethod = o.Payment == null ? "Khac" : MapPaymentMethod(o.Payment.PaymentMethod),
                Status = MapOrderStatus(o.Status),
                CreatedAt = o.CreatedAt
            }).ToList();
        }

        private static IQueryable<T> ApplySpecification<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
        {
            return SpecificationEvaluator.Default.GetQuery(query, specification);
        }

        private static string MapOrderStatus(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "pending",
                OrderStatus.Cancelled => "cancelled",
                OrderStatus.Delivered => "completed",
                _ => "processing"
            };
        }

        private static string MapPaymentMethod(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.COD => "COD",
                PaymentMethod.CreditCard => "Card",
                PaymentMethod.Momo => "E-Wallet",
                PaymentMethod.PayPal => "Banking",
                _ => "Khac"
            };
        }
    }
}
