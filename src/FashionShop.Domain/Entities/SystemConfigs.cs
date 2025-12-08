using FashionShop.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Entities
{
    public class SystemConfigs : BaseEntity
    {
        public required string ConfigKey { get; set; } = string.Empty;
        public required string ConfigValue { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
