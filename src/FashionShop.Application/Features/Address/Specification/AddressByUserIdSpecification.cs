using Ardalis.Specification;
using FashionShop.Domain.Entities;

public class AddressesByUserIdSpecification : Specification<Address>
{
    public AddressesByUserIdSpecification(Guid userId)
    {
        Query.Where(x => x.UserId == userId);
    }
}