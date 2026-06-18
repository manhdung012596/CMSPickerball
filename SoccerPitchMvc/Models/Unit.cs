using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Đơn vị tính (cái, hộp, bộ, giờ...)
/// </summary>
public class Unit
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Symbol { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>1=Hoạt động, 0=Ẩn</summary>
    public int Status { get; set; } = 1;

    public int SortOrder { get; set; } = 0;
}
