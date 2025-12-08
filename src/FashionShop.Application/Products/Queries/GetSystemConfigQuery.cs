using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Products.Queries
{
    public record GetSystemConfigQuery : IRequest<List<SystemConfigs>>
    {
        public class GetSystemConfigQueryHandler : IRequestHandler<GetSystemConfigQuery, List<SystemConfigs>>
        {
            private readonly ISystemConfigRepository _systemConfigRepository;
            public GetSystemConfigQueryHandler(ISystemConfigRepository systemConfigRepository)
            {
                _systemConfigRepository = systemConfigRepository;
            }
            public async Task<List<SystemConfigs>> Handle(GetSystemConfigQuery request, CancellationToken cancellationToken)
            {
                var configs = await _systemConfigRepository.GetAllAsync();
                return Task.FromResult(configs.ToList()).Result;
            }
        }

    }
}
