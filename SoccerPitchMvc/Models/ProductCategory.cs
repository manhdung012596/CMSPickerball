using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Danh mục sản phẩm (vợt, bóng, phụ kiện...)
/// </summary>
public class ProductCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? IconClass { get; set; }

    public int? ParentId { get; set; }

    public int SortOrder { get; set; } = 0;

    /// <summary>1=Hoạt động, 0=Ẩn</summary>
    public int Status { get; set; } = 1;

    // Navigation
    public virtual ProductCategory? Parent { get; set; }
    public virtual ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();
}
