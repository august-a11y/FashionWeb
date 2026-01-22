using FashionShop.Application.RequestDtos;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Products.Commands
{
    public record UpdateProductCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        [Required]
        public required ProductDto Data { get; set; }
    }
    

    public class UpdateEntityCommandHandler : IRequestHandler<UpdateProductCommand, bool> 
    {
        private readonly Domain.Interfaces.IRepository<Product, Guid> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateEntityCommandHandler(Domain.Interfaces.IRepository<Product, Guid> repo, AutoMapper.IMapper mapper, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }
        
        

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }
            entity.UpdateDetails(
                request.Data.Name,
                request.Data.Description,
                request.Data.ThumbnailUrl,
                request.Data.BasePrice
                
            );
            await _unitOfWork.CommitAsync(cancellationToken);
            return true;
        }
    }
}
