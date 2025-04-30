using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using VEXA.Models;

namespace VEXA
{


    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? TransactionId { get; set; }

        public string? PaymentMethod { get; set; }
        public enum PaymentStatus
        {
            Pending,
            Completed,
            Failed,
            Refunded
        }
    }
}
