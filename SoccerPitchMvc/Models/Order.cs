using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Thực thể đơn hàng lẻ
/// </summary>
public class Order
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderCode { get; set; } = string.Empty;

    public int? CustomerId { get; set; }

    [MaxLength(100)]
    public string? CustomerName { get; set; }

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public decimal TotalAmount { get; set; } = 0;

    public decimal DiscountAmount { get; set; } = 0;

    public decimal FinalAmount { get; set; } = 0;

    /// <summary>0 = Tiền mặt, 1 = Chuyển khoản, 2 = Ví điện tử</summary>
    public int PaymentMethod { get; set; } = 0;

    /// <summary>0 = Chưa thanh toán, 1 = Đã thanh toán</summary>
    public int PaymentStatus { get; set; } = 0;

    /// <summary>0 = Chờ xử lý, 1 = Hoàn thành, 2 = Đã hủy</summary>
    public int OrderStatus { get; set; } = 1;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
