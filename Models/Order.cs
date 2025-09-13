using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        [Required]
        public decimal OrderTotal { get; set; }

        [Required]
        public string? ShippingAddress { get; set; }

        [Required]
        public string? BillingAddress { get; set; }

        [Required]
        [Phone]
        public string? ContactPhone { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Confirmed;

        public PaymentMethod Method { get; set; }


        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }

        public enum OrderStatus
        {
            Confirmed
        }

        public enum PaymentMethod
        {
            CashOnDelivery
        }
    }
}
