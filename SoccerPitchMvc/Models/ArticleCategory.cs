using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Danh mục bài viết
/// </summary>
public class ArticleCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Slug { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Trạng thái: 1=Hoạt động, 0=Ẩn
    /// </summary>
    public int Status { get; set; } = 1;

    // Navigation properties
    public virtual ArticleCategory? Parent { get; set; }
    public virtual ICollection<ArticleCategory> Children { get; set; } = new List<ArticleCategory>();
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
