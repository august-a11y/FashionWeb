using FashionShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Persistence
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(x => x.Id);

            
            builder.OwnsOne(o => o.ShippingAddress, a =>
            {
                a.Property(p => p.FullNameAddress).HasColumnName("Ship_FullName").HasMaxLength(100).IsRequired();
                a.Property(p => p.StreetLine).HasColumnName("Ship_AddressLine").HasMaxLength(255).IsRequired();
                a.Property(p => p.Ward).HasColumnName("Ship_Ward").HasMaxLength(100);
                a.Property(p => p.District).HasColumnName("Ship_District").HasMaxLength(100);
                a.Property(p => p.City).HasColumnName("Ship_City").HasMaxLength(100);
            });

            
            builder.Property(x => x.SubTotal).HasPrecision(18, 2);
            builder.Property(x => x.ShippingFee).HasPrecision(18, 2);
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

            
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Status);

            
            builder.Property(x => x.RowVersion).IsRowVersion();
        }
    }
}
