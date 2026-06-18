using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Thực thể bài viết/tin tức
/// </summary>
public class Article
{
    public int Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Slug { get; set; }

    [MaxLength(500)]
    public string? Summary { get; set; }

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public int? CategoryId { get; set; }

    /// <summary>
    /// Trạng thái: 0=Nháp, 1=Đã xuất bản, 2=Ẩn
    /// </summary>
    public int Status { get; set; } = 0;

    public bool IsFeatured { get; set; } = false;

    public int ViewCount { get; set; } = 0;

    [MaxLength(450)]
    public string? AuthorId { get; set; }

    [MaxLength(100)]
    public string? AuthorName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    public virtual ArticleCategory? Category { get; set; }
    public virtual ICollection<ArticleComment> Comments { get; set; } = new List<ArticleComment>();
}
