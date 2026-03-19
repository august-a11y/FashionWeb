using FashionShop.Application.AddressServices.DTO;
using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Interfaces;
using FashionShop.Domain.Entities;
using FluentResults;

namespace FashionShop.Application.AddressServices
{
    public class AddressService : IAddressService
    {
        private readonly IRepository<Address, Guid> _addressRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestContext _requestContext;

        public AddressService(
            IRepository<Address, Guid> addressRepository,
            IUnitOfWork unitOfWork,
            IRequestContext requestContext)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
            _requestContext = requestContext;
        }

        public async Task<Result<List<AddressDTO>>> GetMyAddressesAsync(CancellationToken cancellationToken)
        {
            if (!_requestContext.UserId.HasValue)
                return Result.Fail<List<AddressDTO>>("User is not authenticated.");

            var addresses = _addressRepository
                .Find(x => x.UserId == _requestContext.UserId.Value)
                .Select(MapToDto)
                .ToList();

            return Result.Ok(addresses);
        }

        public async Task<Result<AddressDTO>> GetMyAddressByIdAsync(Guid addressId, CancellationToken cancellationToken)
        {
            if (!_requestContext.UserId.HasValue)
                return Result.Fail<AddressDTO>("User is not authenticated.");

            if (addressId == Guid.Empty)
                return Result.Fail<AddressDTO>("Invalid address id.");

            var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);
            if (address == null || address.UserId != _requestContext.UserId.Value)
                return Result.Fail<AddressDTO>("Address not found.");

            return Result.Ok(MapToDto(address));
        }

        public async Task<Result<AddressDTO>> CreateMyAddressAsync(CreateAddressDTO createAddressDTO, CancellationToken cancellationToken)
        {
            if (!_requestContext.UserId.HasValue)
                return Result.Fail<AddressDTO>("User is not authenticated.");

            var address = new Address
            {
                UserId = _requestContext.UserId.Value,
                FullName = createAddressDTO.FullName,
                PhoneNumber = createAddressDTO.PhoneNumber,
                StreetLine = createAddressDTO.StreetLine,
                Ward = createAddressDTO.Ward,
                District = createAddressDTO.District,
                City = createAddressDTO.City
            };

            _addressRepository.Add(address);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(MapToDto(address));
        }

        public async Task<Result<AddressDTO>> UpdateMyAddressAsync(Guid addressId, UpdateAddressDTO updateAddressDTO, CancellationToken cancellationToken)
        {
            if (!_requestContext.UserId.HasValue)
                return Result.Fail<AddressDTO>("User is not authenticated.");

            if (addressId == Guid.Empty)
                return Result.Fail<AddressDTO>("Invalid address id.");

            var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);
            if (address == null || address.UserId != _requestContext.UserId.Value)
                return Result.Fail<AddressDTO>("Address not found.");

            if (!string.IsNullOrWhiteSpace(updateAddressDTO.FullName))
                address.FullName = updateAddressDTO.FullName;

            if (!string.IsNullOrWhiteSpace(updateAddressDTO.PhoneNumber))
                address.PhoneNumber = updateAddressDTO.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(updateAddressDTO.StreetLine))
                address.StreetLine = updateAddressDTO.StreetLine;

            if (!string.IsNullOrWhiteSpace(updateAddressDTO.Ward))
                address.Ward = updateAddressDTO.Ward;

            if (!string.IsNullOrWhiteSpace(updateAddressDTO.District))
                address.District = updateAddressDTO.District;

            if (!string.IsNullOrWhiteSpace(updateAddressDTO.City))
                address.City = updateAddressDTO.City;

            _addressRepository.Save(address);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(MapToDto(address));
        }

        public async Task<Result<bool>> DeleteMyAddressAsync(Guid addressId, CancellationToken cancellationToken)
        {
            if (!_requestContext.UserId.HasValue)
                return Result.Fail<bool>("User is not authenticated.");

            if (addressId == Guid.Empty)
                return Result.Fail<bool>("Invalid address id.");

            var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);
            if (address == null || address.UserId != _requestContext.UserId.Value)
                return Result.Fail<bool>("Address not found.");

            _addressRepository.Remove(address);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }

        private static AddressDTO MapToDto(Address address)
        {
            return new AddressDTO
            {
                FullName = address.FullName,
                PhoneNumber = address.PhoneNumber,
                StreetLine = address.StreetLine,
                Ward = address.Ward,
                District = address.District,
                City = address.City
            };
        }
    }
}