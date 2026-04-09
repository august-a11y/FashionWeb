using Ardalis.Specification;
using FashionShop.Domain.Entities;

namespace FashionShop.Application.Specifications;

public class OrdersWithItemsByUserIdSpec : Specification<Order>
{
    public OrdersWithItemsByUserIdSpec(Guid userId)
    {
        Query.Where(o => o.UserId == userId)
             .Include(o => o.OrderItems)
             .Include(o => o.Payment)
             .Include(o => o.Shipment);
    }
}