using FashionShop.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Entities
{
    public class Inventory : BaseEntity
    {

        public int WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set;} = null!;

        public int ProductVariantId { get; set; }
        public virtual ProductVariant ProductVariant { get; set; } = null!;

        public int StockQuantity { get; set; }       

    }
}
