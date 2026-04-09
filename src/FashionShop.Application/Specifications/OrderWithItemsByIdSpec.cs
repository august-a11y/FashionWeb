using Ardalis.Specification;
using FashionShop.Domain.Entities;

namespace FashionShop.Application.Specifications;

public class OrderWithItemsByIdSpec : SingleResultSpecification<Order>
{
    public OrderWithItemsByIdSpec(Guid id)
    {
        Query.Where(o => o.Id == id)
             .Include(o => o.OrderItems)
             .Include(o => o.Payment)
             .Include(o => o.Shipment);
    }
}