using AutoMapper;
using FashionShop.Application.Dtos;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Categories.Commands
{
    public class CreateCategoryCommand : IRequest<bool>
    {
        public required string Name { get; set; }
        public required string Description { get; set; } 
    }

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, bool>
    {
        private readonly IRepository<Category, Guid> _repository;
        private readonly IUnitOfWork _unitOfWork;
        public CreateCategoryCommandHandler(IRepository<Category, Guid> repository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }
        
        public  async Task<bool> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            
            var categoryEntity = new Category(request.Name, request.Description);

            _repository.Add(categoryEntity);
            await _unitOfWork.CommitAsync(cancellationToken);
            return true;
            

        }
    }
}
