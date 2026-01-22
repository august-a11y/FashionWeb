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
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public Guid CategoryId { get; set; }
    }

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly IRepository<Category, Guid> _repository;
        private readonly IUnitOfWork _unitOfWork;
        public DeleteCategoryCommandHandler(IRepository<Category, Guid> categoryRepository, IUnitOfWork unitOfWork)
        {
            _repository = categoryRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                throw new Exception("Category not found.");
            }
            _repository.Remove(category);
            await _unitOfWork.CommitAsync(cancellationToken);
            return true;
        }
    }
}
