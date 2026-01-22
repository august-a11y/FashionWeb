using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.RequestDtos
{
    public record ProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? BasePrice { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public Guid CategoryID { get; set; }
        
    }
}
