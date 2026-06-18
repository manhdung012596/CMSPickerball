using System;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Thực thể Khuyến mại & Mã giảm giá (Coupon).
/// </summary>
public class Promotion
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? Code { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Loại giảm giá: 0 = Tiền mặt (VND), 1 = Phần trăm (%)
    /// </summary>
    public int DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Trạng thái: 0 = Nháp, 1 = Đang hoạt động, 2 = Tạm dừng/Hết hạn
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// Phân loại: true = Mã giảm giá (Coupon), false = Chương trình ưu đãi chung (Offer)
    /// </summary>
    public bool IsCoupon { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
