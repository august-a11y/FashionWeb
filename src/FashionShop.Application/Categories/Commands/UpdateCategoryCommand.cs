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
    public class UpdateCategoryCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; } 
    }

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
    {
        private readonly IRepository<Category, Guid> _repository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCategoryCommandHandler(IRepository<Category, Guid> repository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                return await Task.FromResult(false);
            }
            category.UpdateDetails(request.Name, request.Description);
            await _unitOfWork.CommitAsync(cancellationToken);
            return true;
        }
    
    }
}
