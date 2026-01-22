using AutoMapper;
using FashionShop.Application.Dtos;
using FashionShop.Domain.Common;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using MediatR;


namespace FashionShop.Application.Products.Commands
{
    public record CreateProductCommand : IRequest<Guid> 
    {
        public required string Name { get; set; } 
        public required string Description { get; set; }
        public required decimal BasePrice { get; set; }
        public required string ThumbnailUrl { get; set; } 
        public required Guid CategoryID { get; set; }
    }
    

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IRepository<Product, Guid> _repo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public CreateProductCommandHandler(IRepository<Product, Guid> repo, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new ProductDto
            {
                Name = request.Name,
                Description = request.Description,
                BasePrice = request.BasePrice,
                ThumbnailUrl = request.ThumbnailUrl,
                CategoryID = request.CategoryID,
            };
            var entity = _mapper.Map<Product>(product);
            _repo.Add(entity);
            await _unitOfWork.CommitAsync(cancellationToken);
            return entity.Id;   
        }
    }
}
