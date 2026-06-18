using System;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Thực thể sản phẩm (nước uống, vợt, bóng, phụ kiện...)
/// </summary>
public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [MaxLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Mã SKU tối đa 50 ký tự")]
    public string? SKU { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn đơn vị tính")]
    public int UnitId { get; set; }

    [Required(ErrorMessage = "Giá nhập không được để trống")]
    public decimal PurchasePrice { get; set; } = 0;

    [Required(ErrorMessage = "Giá bán không được để trống")]
    public decimal SellingPrice { get; set; } = 0;

    public int StockQuantity { get; set; } = 0;

    [MaxLength(500, ErrorMessage = "Đường dẫn ảnh tối đa 500 ký tự")]
    public string? ImageUrl { get; set; }

    [MaxLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
    public string? Description { get; set; }

    /// <summary>1=Đang bán, 0=Ngừng bán</summary>
    public int Status { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public virtual ProductCategory? Category { get; set; }
    public virtual Unit? Unit { get; set; }
}
