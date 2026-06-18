using System;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Khung giờ mặc định áp dụng cho tất cả sân
/// </summary>
public class DefaultTimeSlot
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Giờ bắt đầu (0-23)</summary>
    public int StartHour { get; set; }

    /// <summary>Phút bắt đầu (0 hoặc 30)</summary>
    public int StartMinute { get; set; } = 0;

    /// <summary>Giờ kết thúc (0-23)</summary>
    public int EndHour { get; set; }

    /// <summary>Phút kết thúc (0 hoặc 30)</summary>
    public int EndMinute { get; set; } = 0;

    /// <summary>
    /// Loại ngày: 0=Tất cả, 1=Ngày thường, 2=Cuối tuần, 3=Ngày lễ
    /// </summary>
    public int DayType { get; set; } = 0;

    /// <summary>Giá tiền cho khung giờ này (override)</summary>
    public decimal? PriceOverride { get; set; }

    /// <summary>1=Hoạt động, 0=Ẩn</summary>
    public int Status { get; set; } = 1;

    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
