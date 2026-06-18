using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Thực thể phiếu nhập hàng
/// </summary>
public class StockImport
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ImportCode { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? SupplierName { get; set; }

    public DateTime ImportDate { get; set; } = DateTime.Now;

    public decimal TotalAmount { get; set; } = 0;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public virtual ICollection<StockImportItem> ImportItems { get; set; } = new List<StockImportItem>();
}
