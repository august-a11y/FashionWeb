using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Products.Commands
{
    public record DeleteProductCommand : IRequest<bool> 
    {
        public Guid Id { get; set; }
    }

    public class DeleteEntityCommandHandler : IRequestHandler<DeleteProductCommand, bool> 
    {
        private readonly Domain.Interfaces.IRepository<Product, Guid> _repo;
        private readonly IUnitOfWork _unitOfWork;
        public DeleteEntityCommandHandler(Domain.Interfaces.IRepository<Product, Guid> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with id {request.Id} not found.");
            }
            _repo.Remove(entity);
            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }

        
    }
    
}
