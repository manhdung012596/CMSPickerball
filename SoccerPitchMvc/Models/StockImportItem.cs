using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Chi tiết phiếu nhập hàng
/// </summary>
public class StockImportItem
{
    public int Id { get; set; }

    [Required]
    public int StockImportId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; } = 1;

    [Required]
    public decimal PurchasePrice { get; set; } = 0;

    [Required]
    public decimal TotalPrice { get; set; } = 0;

    // Navigation properties
    public virtual StockImport? StockImport { get; set; }
    public virtual Product? Product { get; set; }
}
