using System.ComponentModel.DataAnnotations;
using FashionShop.Domain.Entities;

namespace FashionShop.Application.Services.OrderServices.DTO
{
    public class OrderStatusUpdateDTO
    {
        [Required(ErrorMessage = "Status is required")]
        public OrderStatus? Status { get; set; }
    }
}