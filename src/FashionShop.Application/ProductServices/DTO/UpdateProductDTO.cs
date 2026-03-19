using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.ProductServices.DTO
{
    public class UpdateDetailsProductDTO
    {
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;

        public decimal? Price { get; set; }
        public string? ThumbnailUrl { get; set; } = string.Empty;
    }
}
