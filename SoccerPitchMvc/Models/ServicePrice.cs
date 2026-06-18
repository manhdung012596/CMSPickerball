using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Giá dịch vụ (thuê vợt, thuê bóng, huấn luyện, tổ chức giải...)
/// </summary>
public class ServicePrice
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string ServiceName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Loại dịch vụ: 1=Thuê vợt, 2=Thuê bóng, 3=Huấn luyện, 4=Tổ chức giải, 5=Khác
    /// </summary>
    public int ServiceType { get; set; } = 5;

    public decimal Price { get; set; }

    [MaxLength(50)]
    public string? PriceUnit { get; set; } = "lần";

    [MaxLength(200)]
    public string? Note { get; set; }

    /// <summary>1=Hoạt động, 0=Ẩn</summary>
    public int Status { get; set; } = 1;

    public int SortOrder { get; set; } = 0;
}
