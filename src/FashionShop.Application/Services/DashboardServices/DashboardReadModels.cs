namespace FashionShop.Application.Services.DashboardServices
{
    public sealed class DashboardKpiRawData
    {
        public decimal TotalRevenue { get; init; }
        public int TotalOrders { get; init; }
        public int CompletedOrders { get; init; }
        public decimal PreviousRevenue { get; init; }
        public int PreviousOrders { get; init; }
        public int PreviousCompletedOrders { get; init; }
        public int NewCustomers { get; init; }
        public int PreviousNewCustomers { get; init; }
    }

    public sealed class DashboardTrendRawData
    {
        public int BucketCount { get; init; }
        public DateTime LabelStartUtc { get; init; }
        public IReadOnlyDictionary<int, decimal> RevenueByBucket { get; init; } = new Dictionary<int, decimal>();
        public IReadOnlyDictionary<int, int> OrderByBucket { get; init; } = new Dictionary<int, int>();
    }

    public sealed class DashboardLowStockProductRow
    {
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Stock { get; set; }
    }

    public sealed class DashboardTopProductRow
    {
        public Guid ProductId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int UnitsSold { get; init; }
        public decimal Revenue { get; init; }
        public int Stock { get; set; }
    }

    public sealed class DashboardTopCategoryRow
    {
        public Guid CategoryId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int UnitsSold { get; init; }
        public decimal Revenue { get; init; }
    }

    public sealed class DashboardTopCustomerRow
    {
        public Guid UserId { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Email { get; init; }
        public decimal TotalSpent { get; init; }
        public int OrderCount { get; init; }
    }

    public sealed class DashboardRecentOrderRow
    {
        public Guid OrderId { get; init; }
        public string Code { get; init; } = string.Empty;
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Email { get; init; }
        public decimal Total { get; init; }
        public string PaymentMethod { get; init; } = "Khac";
        public string Status { get; init; } = "pending";
        public DateTime CreatedAt { get; init; }
    }
}
