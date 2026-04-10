using Ardalis.Specification;
using FashionShop.Domain.Entities;

namespace FashionShop.Application.Services.DashboardServices.Specifications
{
    public sealed class DashboardOrdersInRangeSpec : Specification<Order>
    {
        public DashboardOrdersInRangeSpec(DateTime startUtc, DateTime endUtc, bool excludeCancelled = false)
        {
            Query.Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc);

            if (excludeCancelled)
            {
                Query.Where(o => o.Status != OrderStatus.Cancelled);
            }
        }
    }

    public sealed class DashboardDelayedOrdersSpec : Specification<Order>
    {
        public DashboardDelayedOrdersSpec(DateTime nowUtc)
        {
            Query.Where(o => o.ExpectedDeliveryDate.HasValue
                             && o.ExpectedDeliveryDate < nowUtc
                             && o.Status != OrderStatus.Delivered
                             && o.Status != OrderStatus.Cancelled);
        }
    }

    public sealed class DashboardPendingRefundOrdersSpec : Specification<Order>
    {
        public DashboardPendingRefundOrdersSpec()
        {
            Query.Where(o => !string.IsNullOrEmpty(o.Note)
                             && o.Note.Contains("refund")
                             && o.Status != OrderStatus.Cancelled);
        }
    }

    public sealed class DashboardRecentOrdersSpec : Specification<Order>
    {
        public DashboardRecentOrdersSpec(DateTime startUtc, DateTime endUtc, int take)
        {
            Query.Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc)
                 .Include(o => o.User)
                 .Include(o => o.Payment)
                 .OrderByDescending(o => o.CreatedAt)
                 .Take(take);
        }
    }
}
