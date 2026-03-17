using FashionShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FashionShop.Infrastructure.Persistence
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.SubTotal).HasPrecision(18, 2);
            builder.Property(x => x.ShippingFee).HasPrecision(18, 2);
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
            builder.Property(x => x.Note).IsRequired();
            builder.Property(x => x.PhoneNumber).IsRequired();
            builder.Property(x => x.RowVersion).IsRowVersion();

            builder.HasIndex(x => x.OrderCode).IsUnique();
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(x => x.ShippingAddress, sa =>
            {
                sa.ToJson();
            });

            builder.Navigation(x => x.ShippingAddress).IsRequired();

            builder.HasOne(x => x.Payment)
                .WithOne(x => x.Order)
                .HasForeignKey<Payment>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Shipment)
                .WithOne(x => x.Order)
                .HasForeignKey<Shipment>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
