namespace FashionShop.Application.Services.DashboardServices.DTO
{
    public enum DashboardPeriod
    {
        Day,
        Month,
        Year
    }

    public class DashboardOverviewDTO
    {
        public DashboardKpisDTO Kpis { get; set; } = new();
        public DashboardTrendDTO Trend { get; set; } = new();
        public List<DashboardAlertDTO> Alerts { get; set; } = new();
        public List<DashboardTopProductDTO> TopProducts { get; set; } = new();
        public List<DashboardTopCategoryDTO> TopCategories { get; set; } = new();
        public List<DashboardTopCustomerDTO> TopCustomers { get; set; } = new();
        public List<DashboardRecentOrderDTO> RecentOrders { get; set; } = new();
        public DashboardMetaDTO Meta { get; set; } = new();
    }

    public class DashboardKpisDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int NewCustomers { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal RevenueChangePct { get; set; }
        public decimal OrdersChangePct { get; set; }
        public decimal NewCustomersChangePct { get; set; }
        public decimal ConversionChangePct { get; set; }
    }

    public class DashboardTrendDTO
    {
        public string Period { get; set; } = DashboardPeriod.Month.ToString();
        public List<string> Labels { get; set; } = new();
        public List<decimal> RevenueSeries { get; set; } = new();
        public List<int> OrderSeries { get; set; } = new();
    }

    public class DashboardAlertDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "low";
    }

    public class DashboardTopProductDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public int Stock { get; set; }
    }

    public class DashboardTopCategoryDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DashboardTopCustomerDTO
    {
        public string? CustomerId { get; set; }
        public string Customer { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
    }

    public class DashboardRecentOrderDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = "Khac";
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardMetaDTO
    {
        public DateTime GeneratedAt { get; set; }
        public string Currency { get; set; } = "VND";
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    }
}
