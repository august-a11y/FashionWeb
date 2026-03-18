using FashionShop.Application.AddressServices.DTO;
using FluentResults;

namespace FashionShop.Application.AddressServices
{
    public interface IAddressService
    {
        Task<Result<List<AddressDTO>>> GetMyAddressesAsync(CancellationToken cancellationToken);
        Task<Result<AddressDTO>> GetMyAddressByIdAsync(Guid addressId, CancellationToken cancellationToken);
        Task<Result<AddressDTO>> CreateMyAddressAsync(CreateAddressDTO createAddressDTO, CancellationToken cancellationToken);
        Task<Result<AddressDTO>> UpdateMyAddressAsync(Guid addressId, UpdateAddressDTO updateAddressDTO, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteMyAddressAsync(Guid addressId, CancellationToken cancellationToken);
    }
}