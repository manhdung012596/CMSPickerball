using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Chi tiết đơn hàng lẻ
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; } = 1;

    [Required]
    public decimal Price { get; set; } = 0;

    [Required]
    public decimal TotalPrice { get; set; } = 0;

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
}
