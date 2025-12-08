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
    public record CreateSystemConfigCommand : IRequest<int>
    {
        public string ConfigKey { get; init; } = string.Empty;
        public string ConfigValue { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }

    public class CreateSystemConfigCommandHandler : IRequestHandler<CreateSystemConfigCommand, int>
    {
        private readonly ISystemConfigRepository _systemConfigRepository;
        public CreateSystemConfigCommandHandler(ISystemConfigRepository systemConfigRepository)
        {
            _systemConfigRepository = systemConfigRepository;
        }
        public async Task<int> Handle(CreateSystemConfigCommand request, CancellationToken cancellationToken)
        {
            var config = new SystemConfigs
            {
                ConfigKey = request.ConfigKey,
                ConfigValue = request.ConfigValue,
                Description = request.Description
            };
            await _systemConfigRepository.AddAsync(config);
            return Task.FromResult(config.Id).Result;
        }
    }
}
