using System;
using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public class AdminAuditLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // CreateUser, UpdateUser, DeleteUser, CreateProduct, UpdateProduct, DeleteProduct

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // User, Product, Variant, Order

        [Required]
        public int EntityId { get; set; }

        [StringLength(100)]
        public string? AdminUserId { get; set; } // Claims NameIdentifier

        [StringLength(150)]
        public string? AdminEmail { get; set; }

        [StringLength(100)]
        public string? AdminName { get; set; }

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        // Small snapshots for quick diffing; avoid PII/secrets
        public string? BeforeJson { get; set; }
        public string? AfterJson { get; set; }
    }
}


